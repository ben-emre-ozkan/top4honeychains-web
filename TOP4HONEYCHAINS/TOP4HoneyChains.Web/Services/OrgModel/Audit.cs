using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    public record AuditRecord(
        DateTime At, string? UserId, string? UserName, int? OrgId, string Action, string Entity,
        string? EntityKey, string? OldValues, string? NewValues, string? Flags, string? ClientIp);

    public record AuditChainResult(bool Ok, long Rows, long? BrokenAtAuditId, string Message);

    // Append-only, hash-chained audit log (D-28).
    // Hash = SHA-256 over the previous row's hash and this row's fields. Rows are appended one at a time
    // under an update lock on the newest row, so AuditId order is chain order; a unique index on PrevHash
    // makes a fork impossible, and triggers reject UPDATE and DELETE.
    public static class AuditChain
    {
        public const string Genesis = "0000000000000000000000000000000000000000000000000000000000000000";

        public static string ComputeHash(string prevHash, AuditRecord r)
        {
            var canonical = string.Join("\u001f",
                prevHash,
                r.At.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                r.UserId ?? "", r.UserName ?? "", r.OrgId?.ToString(CultureInfo.InvariantCulture) ?? "",
                r.Action, r.Entity, r.EntityKey ?? "", r.OldValues ?? "", r.NewValues ?? "", r.Flags ?? "", r.ClientIp ?? "");
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        }

        // Appends inside the context's current transaction (the caller's business change), or its own one
        public static async Task AppendAsync(DbContext ctx, IReadOnlyList<AuditRecord> records, CancellationToken ct = default)
        {
            if (records.Count == 0)
                return;

            var own = ctx.Database.CurrentTransaction == null ? await ctx.Database.BeginTransactionAsync(ct) : null;
            try
            {
                var prev = (await ctx.Database
                    .SqlQueryRaw<string>("SELECT TOP 1 Hash AS Value FROM dbo.AuditEntries WITH (UPDLOCK, HOLDLOCK) ORDER BY AuditId DESC")
                    .ToListAsync(ct)).FirstOrDefault() ?? Genesis;

                foreach (var r in records)
                {
                    // datetime2(3) keeps milliseconds; round now so the stored value hashes the same later
                    var rec = r with { At = new DateTime(r.At.Ticks - r.At.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc) };
                    var hash = ComputeHash(prev, rec);
                    await ctx.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO dbo.AuditEntries (At, UserId, UserName, OrgId, Action, Entity, EntityKey, OldValues, NewValues, Flags, ClientIp, PrevHash, Hash)
VALUES ({rec.At}, {rec.UserId}, {rec.UserName}, {rec.OrgId}, {rec.Action}, {rec.Entity}, {rec.EntityKey}, {rec.OldValues}, {rec.NewValues}, {rec.Flags}, {rec.ClientIp}, {prev}, {hash})", ct);
                    prev = hash;
                }

                if (own != null)
                    await own.CommitAsync(ct);
            }
            finally
            {
                if (own != null)
                    await own.DisposeAsync();
            }
        }

        // Recomputes every hash from the first row; reports the first row that does not match
        public static async Task<AuditChainResult> VerifyAsync(TOP4HoneyChainsContext ctx, CancellationToken ct = default)
        {
            var prev = Genesis;
            long rows = 0;
            await foreach (var e in ctx.AuditEntries.AsNoTracking().OrderBy(a => a.AuditId).AsAsyncEnumerable().WithCancellation(ct))
            {
                rows++;
                var rec = new AuditRecord(DateTime.SpecifyKind(e.At, DateTimeKind.Utc), e.UserId, e.UserName, e.OrgId, e.Action, e.Entity,
                    e.EntityKey, e.OldValues, e.NewValues, e.Flags, e.ClientIp);
                if (e.PrevHash != prev)
                    return new AuditChainResult(false, rows, e.AuditId, $"Row {e.AuditId} does not point to the previous row.");
                if (ComputeHash(prev, rec) != e.Hash)
                    return new AuditChainResult(false, rows, e.AuditId, $"Row {e.AuditId} was changed after it was written.");
                prev = e.Hash;
            }
            return new AuditChainResult(true, rows, null, rows == 0 ? "The audit log is empty." : $"All {rows} rows verify.");
        }
    }

    // Explicit audit rows for actions the change tracker does not see (Identity users, approvals, logins)
    public class AuditWriter
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly IHttpContextAccessor _http;

        public AuditWriter(TOP4HoneyChainsContext context, ICurrentOrg current, IHttpContextAccessor http)
        {
            _context = context;
            _current = current;
            _http = http;
        }

        // userId/userName: for actions before the sign-in cookie exists (login)
        public async Task WriteAsync(string action, string entity, string? key, object? oldValues = null, object? newValues = null,
            string? flags = null, string? userId = null, string? userName = null)
        {
            // The audit log is on from go-live, with or without the organization model (D-34)
            var active = userId == null ? await _current.GetAsync() : null;
            await AuditChain.AppendAsync(_context, new[]
            {
                new AuditRecord(DateTime.UtcNow, userId ?? _current.UserId, userName ?? _current.UserName, active?.OrgId, action, entity, key,
                    oldValues == null ? null : JsonSerializer.Serialize(oldValues), newValues == null ? null : JsonSerializer.Serialize(newValues),
                    flags, _http.HttpContext?.Connection.RemoteIpAddress?.ToString())
            });
        }
    }

    // Writes an audit row for every insert, update and delete of the business context, in the same
    // transaction as the change (D-28). Always active, independent of Features:OrgRoleModel (D-34).
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _services;
        private List<(EntityEntry Entry, string Action, string Entity, string? Key, string? Old, string? New)>? _pending;
        private IDbContextTransaction? _ownTransaction;

        public AuditInterceptor(IServiceProvider services)
        {
            _services = services;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
        {
            var ctx = eventData.Context;
            if (ctx == null)
                return result;

            _pending = ctx.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && e.Entity is not AuditEntry)
                .Select(Describe)
                .Where(p => p.Action != null)
                .Select(p => (p.Entry, p.Action!, p.Entity, p.Key, p.Old, p.New))
                .ToList();

            if (_pending.Count > 0 && ctx.Database.CurrentTransaction == null)
                _ownTransaction = await ctx.Database.BeginTransactionAsync(ct);
            return result;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
        {
            var ctx = eventData.Context;
            var pending = _pending;
            _pending = null;
            if (ctx == null || pending == null || pending.Count == 0)
                return result;

            var current = _services.GetRequiredService<ICurrentOrg>();
            var http = _services.GetRequiredService<IHttpContextAccessor>();
            var active = await current.GetAsync();
            var now = DateTime.UtcNow;
            var ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var records = pending.Select(p => new AuditRecord(now, current.UserId, current.UserName, active?.OrgId,
                p.Action, p.Entity, p.Key ?? KeyOf(p.Entry), p.Old, p.New, null, ip)).ToList();
            try
            {
                await AuditChain.AppendAsync(ctx, records, ct);
                if (_ownTransaction != null)
                    await _ownTransaction.CommitAsync(ct);
            }
            finally
            {
                await DisposeOwnAsync();
            }
            return result;
        }

        public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken ct = default)
        {
            _pending = null;
            if (_ownTransaction != null)
                await _ownTransaction.RollbackAsync(ct);
            await DisposeOwnAsync();
        }

        // The app only uses async saves; a synchronous save would bypass the audit, so refuse it
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context?.ChangeTracker.HasChanges() == true)
                throw new InvalidOperationException("Use SaveChangesAsync: synchronous saves are not audited.");
            return result;
        }

        private async Task DisposeOwnAsync()
        {
            if (_ownTransaction != null)
            {
                await _ownTransaction.DisposeAsync();
                _ownTransaction = null;
            }
        }

        private static readonly HashSet<string> NotLogged = new(StringComparer.OrdinalIgnoreCase) { "ConsumerAccounts", "ConsumerSavedBatches", "ConsumerFeedback" };

        private static (EntityEntry Entry, string? Action, string Entity, string? Key, string? Old, string? New) Describe(EntityEntry e)
        {
            var entity = e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name;
            // D-87: consumer tables never go into the log with their values (pseudonymous messages, saved honeys); the feedback
            // workflow writes explicit entries with ids only
            if (NotLogged.Contains(entity))
                return (e, null, entity, null, null, null);
            switch (e.State)
            {
                case EntityState.Added:
                    return (e, "Create", entity, null, null, Serialize(e.Properties.Where(p => !p.Metadata.IsPrimaryKey() || !p.IsTemporary), p => p.CurrentValue));
                case EntityState.Deleted:
                    return (e, "Delete", entity, KeyOf(e), Serialize(e.Properties, p => p.OriginalValue), null);
                case EntityState.Modified:
                    var changed = e.Properties.Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue)).ToList();
                    if (changed.Count == 0)
                        return (e, null, entity, null, null, null);
                    return (e, "Update", entity, KeyOf(e), Serialize(changed, p => p.OriginalValue), Serialize(changed, p => p.CurrentValue));
                default:
                    return (e, null, entity, null, null, null);
            }
        }

        private static string KeyOf(EntityEntry e) =>
            string.Join(",", e.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => Convert.ToString(p.CurrentValue ?? p.OriginalValue, CultureInfo.InvariantCulture)));

        private static string Serialize(IEnumerable<PropertyEntry> props, Func<PropertyEntry, object?> value)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var p in props)
            {
                var v = value(p);
                // Binary columns (photos) are not copied into the log; their size is enough to see a change
                dict[p.Metadata.Name] = v is byte[] bytes ? $"[binary {bytes.Length} bytes]" : v;
            }
            return JsonSerializer.Serialize(dict);
        }
    }
}
