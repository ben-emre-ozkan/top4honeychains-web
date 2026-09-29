# Go-live checklist

Everything ships together in one deployment. Nothing from `main` is live until this checklist is done.
Tick items as they are completed; add new items whenever a change needs a deploy-time step.
See `docs/ARCHITECTURE.md` for background on each point.

## 1. Before deployment day

### Release build
- [ ] All work items merged into `main`; `dotnet build -c Release` of `TOP4HONEYCHAINS/TOP4HoneyChains.Web` passes with 0 errors.
- [ ] Release tagged in git (e.g. `v1.0.0`) on the exact commit being deployed.
- [ ] Only `TOP4HoneyChains.Web` is published. `TOP4HoneyChains.API` (template) and `TOP4HoneyChainsWebApps` (legacy) are not deployed.
- [ ] Target server runs Windows, because QR generation uses System.Drawing.

### Secrets
The values below are in git history, so rotating them is the actual fix. Removing them from files is not enough.
- [ ] Rotate the Gmail app password (`SmtpSettings:Password`).
- [ ] Rotate the Google Maps API key (hard-coded in `Views/HoneyStory/Index.cshtml`).
- [ ] Rotate the admin password. The credentials in the commented-out seed in `Program.cs` are exposed.
- [ ] Code: `appsettings.json` holds placeholders only. The Maps key is read from configuration, not from the view. The seed credentials are deleted from `Program.cs`.
- [ ] Server-side config holds the real values: environment variables, or an `appsettings.Production.json` that exists only on the server. Never in the repo.
- [ ] Restrict the Maps key to our domain (HTTP referrer) and to the Maps JavaScript API only.

### Accounts and access
- [x] Public self-registration: **closed** ([D-02](DECISIONS.md)). Removed in `fix/account-flows`.
- [ ] Beekeeper accounts whose password equals their business number: beekeepers do not log in in this release ([D-01](DECISIONS.md), [D-03](DECISIONS.md)), but the logins still exist and are guessable. Decide whether to lock them or set random passwords at go-live (open).
- [ ] Review users who self-registered on production while sign-up was open (role `Beekeeper` without a Beekeepers profile) and remove them.

### Open product decisions (resolve or explicitly accept)
- [ ] QR codes encode the full public URL (`https://<domain>/honeystory/{guid}`), with the base URL taken from config.
- [ ] HoneyStory shows non-finalized (draft) batches, beekeeper names/photos and apiary GPS publicly.
- [x] ~~The error page is admin-only and its layout is broken~~: fixed in `fix/db-error-handling`.
- [x] ~~Deleting records referenced by FKs gives unhandled 500s~~: fixed in `fix/db-error-handling`.
- [ ] The localized messages added for es/pl/lv (`Error.*` keys in `Resources/SharedResource.*.resx`) were translated by us. Ask the partners to review them.
- [ ] Missing antiforgery / state changes via GET in User/Role management.
- [ ] Unrestricted video upload in CCP ParameterContents.
- [ ] HTTPS: is it enforced by IIS/proxy? If not, add HTTPS redirect + HSTS.

## 2. Deployment day (in this order)
1. [ ] **Back up the production database** (full backup) and confirm it restores. The backup stays on the DB server and never goes into the repo or the deploy package.
2. [ ] Back up the current live site folder, including server-only uploads (`wwwroot/assets/videos/parameters/`).
3. [ ] Put the site into maintenance mode (e.g. `app_offline.htm`).
4. [ ] Apply every SQL script in `db/migrations/` **in numeric order** (`001_…`, `002_…`). Scripts are idempotent; record which ones were applied. The DB compatibility level must be 150 (SQL Server 2019).
5. [ ] Review users in the **Administrator** role on production and remove unknown accounts. The register role-escalation bug was live until this deployment.
6. [ ] Also review who created the roles `Consumer`, `Wholesaler` and `SampleAcceptanceOfficer`, and any users in them.
7. [ ] Make sure at least one known admin account exists with the new, strong password.
8. [ ] Deploy the published Web build.
   - Keep the server-only config and the uploaded files.
   - Set `ASPNETCORE_ENVIRONMENT=Production`.
   - Point the `DefaultConnection` connection string to the production DB.
9. [ ] Make sure ASP.NET Core Data Protection keys persist across app-pool restarts, otherwise logins and antiforgery tokens break after a restart.
   - Also decide where logs go.
   - Unhandled errors are logged with the reference ID shown on the error page, and constraint violations as warnings.
   - The Windows Event Log receives warnings and errors by default. Console output is lost under IIS unless stdout logging is enabled.
10. [ ] Remove maintenance mode.

## 3. Smoke test after deployment
- [ ] Admin login works. The dashboard and every menu page open.
- [ ] Register with a tampered `Role=Administrator` → the user gets `Beekeeper`. Then delete the test user.
- [ ] A Register POST without an antiforgery token is rejected (400).
- [ ] Forgot password sends an email (new SMTP password), and the reset link works.
- [ ] Public `/honeystory/{guid}` loads, the map shows (new Maps key and domain restriction), and the QR code scans.
- [ ] Raw → final honey flow and CCP monitoring pages save correctly.
- [ ] Error page: as an anonymous user, trigger an error (e.g. `/honeystory/qrcode` without `code`). Expect HTTP 500 with the "Something went wrong" page and a reference, not a login redirect. The reference must appear in the log.
- [ ] Deleting a lookup value that is in use shows "This record is in use and cannot be deleted." (no 500).

## 4. Rollback
- Restore the previous site folder.
- Restore the DB backup only if a migration script caused damage. Scripts are additive, so the old app should run on the new schema.
- Rotated secrets stay rotated.
