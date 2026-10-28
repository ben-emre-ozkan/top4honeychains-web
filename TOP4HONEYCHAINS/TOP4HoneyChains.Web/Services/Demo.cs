using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services
{
    // D-86: the demo environment for the partner teams, section "Demo". Off on the live site. While on, the site shows a banner,
    // asks search engines not to index it, offers the demo SMS inbox, accepts the demo registry numbers of the partner countries,
    // and runs only with the fake SMS provider against a database that carries the demo marker.
    public class DemoOptions
    {
        public bool Enabled { get; set; }
    }

    public class DemoMode
    {
        private readonly DemoOptions _options;
        private volatile bool _databaseOk;

        public DemoMode(IOptions<DemoOptions> options) => _options = options.Value;

        public bool Enabled => _options.Enabled;

        // set by DemoGuard once the database carries the marker; until then a demo site answers 503
        public bool DatabaseOk => _databaseOk;

        internal void MarkDatabaseOk() => _databaseOk = true;
    }

    // The marker of a demo database: a database-level extended property, written by the demo database script. The live
    // database never has it, so a demo configuration that points at the live database refuses to start.
    public static class DemoDatabase
    {
        public const string MarkerName = "TOP4HoneyChains.Demo";

        public static async Task<bool> HasMarkerAsync(string? connectionString, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand("SELECT CAST(value AS nvarchar(20)) FROM sys.extended_properties WHERE class = 0 AND name = @name", conn);
            cmd.Parameters.AddWithValue("@name", MarkerName);
            return (await cmd.ExecuteScalarAsync(ct)) is string value && value == "1";
        }
    }

    // Refuses to start a demo site with a real SMS provider or without the demo marker in its database
    public class DemoGuard : IHostedService
    {
        private readonly DemoMode _demo;
        private readonly SmsOptions _sms;
        private readonly IConfiguration _config;

        public DemoGuard(DemoMode demo, IOptions<SmsOptions> sms, IConfiguration config)
        {
            _demo = demo;
            _sms = sms.Value;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_demo.Enabled)
                return;
            if (!_sms.IsFake)
                throw new InvalidOperationException("Demo:Enabled needs Sms:Provider=Fake: a demo site never sends real SMS.");
            if (!await DemoDatabase.HasMarkerAsync(_config.GetConnectionString("DefaultConnection"), cancellationToken))
                throw new InvalidOperationException($"Demo:Enabled, but the database has no '{DemoDatabase.MarkerName}' marker. "
                                                    + "A demo site runs only on a database created by the demo database script, never on the live database.");
            _demo.MarkDatabaseOk();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
