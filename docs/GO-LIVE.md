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
- [ ] Rotate the Gmail app password (`SmtpSettings:Password`). The old one is in git history.
- [ ] Rotate the Google Maps API key (`GoogleMaps:ApiKey`). The old one is in git history.
- [ ] Rotate the password of the admin account whose credentials were in the commented-out seed in `Program.cs` (in git history).
- [x] Code: `appsettings.json` holds placeholders only (`SmtpSettings:UserName/Password`, `GoogleMaps:ApiKey`, `App:PublicBaseUrl` are empty). The Maps key is read from configuration (`Views/HoneyStory/Index.cshtml`; without a key the map script is not loaded). The seed with credentials is deleted from `Program.cs` (`fix/secrets-out-of-config`). Git history was not rewritten, so rotation above is required.
- [ ] Server-side config holds the real values. Never in the repo. Use **one** of the two options:
  - **Environment variables** on the app pool / site (double underscore = section separator):

    | Setting | Environment variable |
    |---|---|
    | SMTP host / port / SSL | `SmtpSettings__Host`, `SmtpSettings__Port`, `SmtpSettings__EnableSsl` |
    | SMTP account | `SmtpSettings__UserName`, `SmtpSettings__Password` |
    | Sender | `SmtpSettings__From`, `SmtpSettings__SenderName` |
    | Maps key | `GoogleMaps__ApiKey` |
    | Public URL for e-mail links | `App__PublicBaseUrl` (e.g. `https://<domain>`) |
    | Allowed host names | `AllowedHosts` (e.g. `<domain>;www.<domain>`) |
    | Database | `ConnectionStrings__DefaultConnection` |
    | Environment | `ASPNETCORE_ENVIRONMENT=Production` |

    Under IIS, set them in `web.config` (`<aspNetCore><environmentVariables>`) **on the server only**, or in the app pool environment. A `web.config` with secrets must not be copied back into the repo.
  - **`appsettings.Production.json`** placed next to the app on the server, with the same keys in JSON form (`{"SmtpSettings": {"Password": "..."}, "GoogleMaps": {"ApiKey": "..."}, ...}`). It is git-ignored and must not be part of the publish package from the repo; keep it when redeploying. Restrict its file permissions to the app pool identity and administrators.
- [ ] Locally, secrets live in `dotnet user-secrets` (`dotnet user-secrets list --project TOP4HONEYCHAINS/TOP4HoneyChains.Web`); SMTP points to a local catcher, never Gmail.
- [ ] Restrict the Maps key to our domain (HTTP referrer) and to the Maps JavaScript API only.
- [ ] Set `App:PublicBaseUrl` to the public site URL (e.g. `https://<domain>`) in server-side config. Password-reset links are built from it; without it, production refuses to send reset e-mails.
- [ ] Set `AllowedHosts` to the production host name(s) instead of `*`.

### Accounts and access
- [x] Public self-registration: **closed** ([D-02](DECISIONS.md)). Removed in `fix/account-flows`.
- [x] Beekeeper accounts whose password equals their business number: decided in [D-09](DECISIONS.md). New accounts get a random password (`fix/beekeeper-accounts`); existing non-admin accounts are locked by `db/migrations/001_lock_out_non_admin_accounts.sql` (deployment step 4).
- [ ] Review users who self-registered on production while sign-up was open (role `Beekeeper` without a Beekeepers profile) and remove them.

### Open product decisions (resolve or explicitly accept)
- [x] QR codes encode the full public URL (`https://<domain>/honeystory/{guid}`), with the base URL taken from `App:PublicBaseUrl` ([D-11](DECISIONS.md), `feat/qr-full-url`). Without the setting, production cannot render QR codes (logged error).
- [x] ~~HoneyStory shows non-finalized (draft) batches, beekeeper names/photos and apiary GPS publicly~~: [D-12](DECISIONS.md), `feat/honeystory-privacy`. Drafts are 404; the page shows the beekeeper's name only; apiary locations are rounded to 0.1° (about 10 km) and drawn as an area; apiary photos are served only for apiaries of finalized batches.
- [x] ~~The error page is admin-only and its layout is broken~~: fixed in `fix/db-error-handling`.
- [x] ~~Deleting records referenced by FKs gives unhandled 500s~~: fixed in `fix/db-error-handling`.
- [ ] The localized messages added for es/pl/lv (`Error.*` keys in `Resources/SharedResource.*.resx`) were translated by us. Ask the partners to review them.
- [x] ~~Missing antiforgery / state changes via GET in User/Role management~~: global `AutoValidateAntiforgeryToken` filter; user, role and CCP-content deletes are POST-only (`fix/antiforgery-global`).
- [x] ~~Unrestricted video upload in CCP ParameterContents~~: uploads are checked server-side (type, signature, size; [D-10](DECISIONS.md), `fix/upload-limits`). Check the production folder `wwwroot/assets/videos/parameters/` for files that are not `.mp4`/`.webm` and remove them.
- [x] ~~HTTPS: is it enforced by IIS/proxy?~~ [D-13](DECISIONS.md): TLS ends at IIS; the app redirects HTTP → HTTPS and sends HSTS (30 days, no subdomains, no preload) in Production only (`feat/https-production`). Needs the IT request below.

### Request to the university IT (D-13)
Please set up for the TOP4HoneyChains site on IIS:
1. A trusted TLS certificate for the public host name (e.g. `<domain>`), with automatic renewal.
2. An **HTTPS binding on port 443** for that host name, and keep the **HTTP binding on port 80** (the app redirects HTTP to HTTPS itself).
3. If HTTPS runs on a port other than 443, or the app does not redirect after go-live: set the environment variable `ASPNETCORE_HTTPS_PORT` to the public HTTPS port on the site.
4. Do not add an HSTS header in IIS (the app sends it); if IIS also redirects to HTTPS, that is fine.
5. Confirm the host name(s), so we can set `AllowedHosts` and `App:PublicBaseUrl` (`https://<domain>`).

### Release candidate check (1.0.0-rc1, 2026-09-29)
- `dotnet publish -c Release` to a local folder: 0 errors.
- Started with `ASPNETCORE_ENVIRONMENT=Production` against LocalDB, all configuration from environment variables (connection string, SMTP, Maps key, `App__PublicBaseUrl`, `AllowedHosts`): login and all main pages return 200, anonymous access redirects to login, a foreign `Host` header is rejected (400), and an error shows the friendly error page (no developer page). Not deployed anywhere.

### Release candidate check (1.0.0-rc2, 2026-09-29)
- Includes D-11 (QR full URL), D-12 (HoneyStory privacy) and D-13 (HTTPS). `dotnet publish -c Release`: 0 errors.
- Production run against LocalDB with configuration from environment variables and a local test certificate: HTTP → 307 to HTTPS, HSTS header on HTTPS for a real host name, login over HTTPS (auth cookie `Secure`, `HttpOnly`), main pages and a published Honey Story page return 200. The draft-batch 404, the QR content and the approximate map were verified in the Development run. Not deployed anywhere.

## 2. Deployment day (in this order)
1. [ ] **Back up the production database** (full backup) and confirm it restores. The backup stays on the DB server and never goes into the repo or the deploy package.
2. [ ] Back up the current live site folder, including server-only uploads (`wwwroot/assets/videos/parameters/`).
3. [ ] Put the site into maintenance mode (e.g. `app_offline.htm`).
4. [ ] Apply every SQL script in `db/migrations/` **in numeric order** (`001_…`, `002_…`). Scripts are idempotent; record which ones were applied. The DB compatibility level must be 150 (SQL Server 2019).
   - `001_lock_out_non_admin_accounts.sql` ([D-09](DECISIONS.md)) locks out every account that is not an Administrator and prints how many it changed. It aborts if no Administrator exists. Run it again after steps 5-7 (it is idempotent): an account removed from the Administrator role there becomes a non-admin that must be locked.
5. [ ] Review users in the **Administrator** role on production and remove unknown accounts. The register role-escalation bug was live until this deployment.
6. [ ] Also review who created the roles `Consumer`, `Wholesaler` and `SampleAcceptanceOfficer`, and any users in them.
7. [ ] Make sure at least one known admin account exists with the new, strong password.
8. [ ] Deploy the published Web build.
   - Keep the server-only config and the uploaded files.
   - Set `ASPNETCORE_ENVIRONMENT=Production`.
   - Point the `DefaultConnection` connection string to the production DB.
   - The publish output contains the repo's `appsettings.json`, whose `DefaultConnection` points to a developer machine, and `appsettings.Development.json` (LocalDB, not loaded in Production). The production connection string **must** be supplied by server config (`ConnectionStrings__DefaultConnection` or `appsettings.Production.json`); delete `appsettings.Development.json` from the server folder.
9. [ ] Make sure ASP.NET Core Data Protection keys persist across app-pool restarts, otherwise logins and antiforgery tokens break after a restart.
   - Also decide where logs go.
   - Unhandled errors are logged with the reference ID shown on the error page, and constraint violations as warnings.
   - The Windows Event Log receives warnings and errors by default. Console output is lost under IIS unless stdout logging is enabled.
10. [ ] Remove maintenance mode.

## 3. Smoke test after deployment
- [ ] Admin login works. The dashboard and every menu page open.
- [ ] `http://<domain>/` redirects to `https://<domain>/`, and HTTPS responses carry `Strict-Transport-Security` (D-13).
- [ ] `/Account/Register` returns 404 (GET and POST) and the login page has no sign-up link (D-02).
- [ ] A non-admin account cannot sign in: beekeeper accounts are locked (D-09) and get the generic "invalid user name or password" message; any other non-admin sees "no access to the web panel" and is not left signed in (D-01).
- [ ] Forgot password for a real admin e-mail sends a mail (new SMTP password), the reset link works and points to `App:PublicBaseUrl`, and the login page confirms the reset.
- [ ] Five wrong passwords for a test admin lock the login for 15 minutes (then wait, or clear `LockoutEnd` for that account).
- [ ] If the SMTP server is unreachable, forgot password shows "could not be sent" (no 500) and the error is logged.
- [ ] Public `/honeystory/{guid}` loads, the map shows (new Maps key and domain restriction), and the QR code scans and opens `https://<domain>/honeystory/{guid}`.
- [ ] Raw → final honey flow and CCP monitoring pages save correctly.
- [ ] Error page: as an anonymous user, open `/Dashboard/Error`. Expect the "Something went wrong" page with a reference, not a login redirect. (Unhandled errors are logged with the same reference; `/honeystory/qrcode` no longer fails, it returns 404 for unknown codes.)
- [ ] Deleting a lookup value that is in use shows "This record is in use and cannot be deleted." (no 500).

## 4. Rollback
- Restore the previous site folder.
- Restore the DB backup only if a migration script caused damage. Scripts are additive, so the old app should run on the new schema.
- Rotated secrets stay rotated.
