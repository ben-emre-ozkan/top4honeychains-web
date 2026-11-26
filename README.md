# TOP4HoneyChains

A web platform for honey traceability from the apiary to the shelf. Organizations (district directorates, beekeepers'
associations, processing and packing plants, laboratories, wholesalers and retailers) record deliveries, sealed containers,
samples, laboratory results, critical control points (CCPs) and batches; every finalized batch gets a public Honey Story page
behind its QR code. The interface is available in Turkish, English, Spanish, Polish and Latvian.

## Folder map

| Folder | What it holds |
|---|---|
| `TOP4HONEYCHAINS/TOP4HoneyChains.Web` | The application (ASP.NET Core 8 MVC, EF Core, ASP.NET Identity). The only part that is deployed. |
| `TOP4HONEYCHAINS/TOP4HoneyChains.Web.Tests` | xUnit tests; `Category=E2E` tests host the app against throw-away LocalDB databases. |
| `TOP4HONEYCHAINS/TOP4HoneyChains.API`, `TOP4HoneyChainsWebApps` | An unused template and a legacy app (nested repository). Not deployed, not changed. |
| `db/migrations` | Numbered SQL scripts (`001_…`), idempotent and additive. They update an existing database. |
| `db/install` | A new, empty database: `01_schema.sql` (schema after the latest migration) and `02_reference_data.sql` (lookup and configuration rows only). |
| `db/demo` | Schema and scripts of the separate demo database for the partner teams. |
| `tools/DemoSchema` | Generates `db/demo/schema.sql` and `db/install` from a database (read-only). |
| `tools/live-like` | Builds a fictional cast on a developer's local copy (never on a server). |
| `docs` | `GO-LIVE.md` (deployment checklist), `KURULUM-TR.md` (step-by-step guide for the server operator, Turkish), `DECISIONS.md`, `ARCHITECTURE.md`, `manual/` (user manuals). |
| `scripts` | Small helper scripts for images and maps. |

## Prerequisites

- Windows (QR codes and images use System.Drawing and SkiaSharp on Windows).
- .NET 8 SDK to build; on the server the ASP.NET Core 8 Hosting Bundle.
- SQL Server 2019 or later (compatibility level 140 or 150) and `sqlcmd`.
- IIS on the server (TLS ends at IIS; the app redirects HTTP to HTTPS in Production).

## Three ways in

1. **Update the live server**: follow [docs/KURULUM-TR.md](docs/KURULUM-TR.md), part A (backup, maintenance mode, every new
   script of `db/migrations` with `sqlcmd -I -f 65001`, publish, server configuration, smoke test, rollback). The full checklist
   with the reasons is [docs/GO-LIVE.md](docs/GO-LIVE.md).
2. **New installation on an empty database**: [docs/KURULUM-TR.md](docs/KURULUM-TR.md), part B (empty database, `db/install`,
   `db/migrations`, the first administrator with `TOP4HoneyChains.Web.exe create-first-admin`, configuration, first organizations).
3. **Local development**: see [CLAUDE.md](CLAUDE.md) for the rules. In short:
   - `dotnet build TOP4HONEYCHAINS/TOP4HoneyChains.Web`
   - `dotnet test TOP4HONEYCHAINS/TOP4HoneyChains.Web.Tests`
   - `dotnet run --project TOP4HONEYCHAINS/TOP4HoneyChains.Web --launch-profile http` (http://localhost:5255, Development,
     database `(localdb)\MSSQLLocalDB` / `TOP4HoneyChainsDb` from `appsettings.Development.json`; secrets in `dotnet user-secrets`).
   - A local database can also be built from the repository alone with `db/install` and `db/migrations` (part B of KURULUM-TR.md).

Secrets never go into the repository: `appsettings.json` holds placeholders only; real values live in the server configuration
(environment variables or `appsettings.Production.json` on the server) or, locally, in `dotnet user-secrets`.

## User manuals

The user manuals (five languages) are written in `docs/manual` (Markdown, screenshots, build scripts); see
[docs/manual/README.md](docs/manual/README.md). The built Word and PDF files are not in git: build them with
`cd docs/manual/build`, `npm install`, `node build.mjs all` (output in `docs/manual/dist/`).

## Regenerating db/install

After a new migration, apply it to a database that has every migration (by default the developer's local copy) and run, read-only:

```
dotnet run --project tools/DemoSchema -- install "Server=(localdb)\MSSQLLocalDB;Database=TOP4HoneyChainsDb;Trusted_Connection=True;TrustServerCertificate=True" db/install
```

It exports only the lookup and configuration tables listed in `tools/DemoSchema/Program.cs` (`InstallData.Tables`) and stops when
a row looks like test data. The E2E test `InstallTests` builds a database from `db/install` and every migration and creates the
first administrator on it.
