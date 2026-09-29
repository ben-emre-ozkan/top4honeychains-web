# TOP4HoneyChains – rules for this repo

## Scope
- The live app is `TOP4HONEYCHAINS/TOP4HoneyChains.Web` (.NET 8 MVC, EF Core, ASP.NET Identity).
- Do **not** modify `TOP4HoneyChains.API` (unused template) or `TOP4HoneyChainsWebApps` (legacy, nested git repo).
- Background: `docs/ARCHITECTURE.md`. Deploy steps: `docs/GO-LIVE.md`. Keep both up to date when a change affects them.

## Build and run locally
- Build: `dotnet build TOP4HONEYCHAINS/TOP4HoneyChains.Web` (must be 0 errors).
- Run: `dotnet run --project TOP4HONEYCHAINS/TOP4HoneyChains.Web --launch-profile http` → http://localhost:5255 (the profile sets `ASPNETCORE_ENVIRONMENT=Development`).
- DB: `(localdb)\MSSQLLocalDB`, database `TOP4HoneyChainsDb`. The connection string lives in `appsettings.Development.json`; never edit the connection string in `appsettings.json`.
- QR generation uses System.Drawing, so the app runs on Windows only.
- E-mail: local SMTP settings live in `dotnet user-secrets` and point to smtp4dev (`localhost:2525`, web UI http://localhost:5100). Install with `dotnet tool install -g Rnwood.Smtp4dev` and run `smtp4dev --urls=http://localhost:5100 --smtpport=2525 --db=""`. Never test mail with the Gmail settings from `appsettings.json`.
- Local login: a local-only test admin `zz_local_admin` (email `zz.local.admin@example.invalid`) exists in the LocalDB copy only, in the Administrator role. The password is kept by the user, not in the repo. Never use real users' accounts.
- sqlcmd: pass `-I` (QUOTED_IDENTIFIER ON) for any DML, otherwise deletes fail on indexed tables.

## Data safety
- The local DB is a **copy of production with real personal data**.
  - Never print rows from tables that hold personal data (AspNetUsers, Beekeepers, Apiaries, ApiaryPhotos, Wholesalers, …). Counts and schema only.
  - Create test data through the app, use obviously fake values (e.g. `zz_test_*`, `@example.invalid`), and delete it afterwards.
- Never connect to production or any other remote server (e.g. `SAHINAYDIN-PC`). The only allowed remote is `git push` to `origin`.

## Database changes
- Business tables are database-first. EF migrations cover Identity tables only.
- Every schema change is a new numbered SQL script in `db/migrations/` (`001_description.sql`, `002_…`). Scripts must be:
  - idempotent (guard with `IF NOT EXISTS` / `COL_LENGTH` / `OBJECT_ID`);
  - additive and backward-compatible only: no drops, renames or type narrowing;
  - compatible with SQL Server 2019 (compatibility level 150).
- Apply the script to the local DB, then keep the EF model (`Data/TOP4HoneyChainsContext.cs`, `Models/Entities/`) in sync.
- Add a line to `docs/GO-LIVE.md` if a script needs a manual step.
- Never produce `.bak` files or database dumps for deployment.

## Secrets
- Secrets never go into tracked files.
- Locally, use `dotnet user-secrets`. The Web csproj has no `UserSecretsId` yet; run `dotnet user-secrets init` on first use.
- `appsettings.json` holds placeholders only; real values live in server-side config.

## Workflow
- Create one branch per work item: `fix/…` or `feat/…`, branched from `main`. `main` is the development line; releases are tagged at deployment.
- Keep changes scoped to the work item. Don't refactor unrelated code.
- Before committing:
  - build with 0 errors;
  - run the app and verify the change in the browser (Playwright MCP is available), including the security-relevant negative cases.
- **Ask the user before merging into `main`.**
- Nothing is deployed piecemeal. Everything goes live together via `docs/GO-LIVE.md`.

## Language
- Code, comments, commits and docs: English.
- Reports to the user: Turkish, short.
