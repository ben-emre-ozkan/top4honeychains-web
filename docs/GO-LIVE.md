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
- [ ] Rotate the Google Maps API key (`GoogleMaps:ApiKey`). The old one is in git history. Since D-59 no page loads Google Maps, so the old key can simply be revoked and the setting left empty.
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

### CCP limits and honey classes (D-18) – verify before go-live
The seeds in `db/migrations/002_ccp_integrity.sql` come from the Turkish Food Codex honey communique as given by the owner and are marked **to verify** (`LimitVerified = 0`). Until a parameter is marked verified (CCP Parameters → Edit → "Limits verified"), the admin pages show its in-range result with a "to verify" tag and Honey Story shows **no** compliance for it.
- [ ] Send [`LIMITS-TO-VERIFY-TR.md`](LIMITS-TO-VERIFY-TR.md) to the accredited partner lab (PQ-03). Then verify each seed and mark it verified, or correct it first (Parameters → Edit for min/max, Parameters → Limits for honey-class, honey-type, conditional and ratio rows):
  - Moisture max 20 %, Pine max 23 % (doubtful: the EU 23 % exception is for heather) (CCP 1, 2, 4, 5)
  - Free acidity max 50 meq/kg (CCP 1)
  - Electrical conductivity: Floral max 0.8, Honeydew, Pine and chestnut honey min 0.8 mS/cm (CCP 1)
  - Fructose + glucose: Floral min 60, Honeydew and Pine min 45 g/100 g; fructose/glucose ratio 0.9–1.4 for Floral (CCP 1, checked on "Fruktoz" with "Glikoz")
  - Sucrose max 5 g/100 g; the "max 10 for specific honeys" case is only a note, no honey class exists for it (CCP 1)
  - HMF max 40 mg/kg (CCP 1, 2, 5)
  - Diastase min 8, min 3 when HMF ≤ 15 mg/kg (CCP 1, 2, 5)
  - Proline min 300 mg/kg (CCP 1, 2, 5)
  - pH 3.2–4.5 (CCP 1)
  - C4 sugar max 7 % (CCP 1)
  - Antibiotics max 0 ppb, zero tolerance for veterinary drug residues (CCP 1). Pesticides and naphthalene have no limit.
  - Storage temperature max 25 °C (CCP 10); no relative humidity limit
- [ ] Verify the seeded data types (Number / Yes-No / Text) and that every parameter should stay required.
- [ ] Verify the honey classes: Pine Honey = Pine; Flower, Thyme, Sunflower, Citrus, Chestnut and Highland = Floral. Chestnut honey's conductivity exception is a honey-type row (D-21); it depends on the honey type being named "Chestnut Honey" when 003 runs.

### Organization model and audit log (D-22 to D-39)
- [x] `Features:OrgRoleModel` stays **off** at go-live ([D-33](DECISIONS.md)); it is switched on only after P2 and the partners' confirmation. Nothing to set: the default in `appsettings.json` is `false`.
- [x] The audit log is **on** from go-live, independent of the flag ([D-34](DECISIONS.md)); migration 004 creates the table and its append-only trigger.
- [ ] The app's DB login is **not** `db_owner` (a `db_owner` can disable the trigger). On the LocalDB copy the IIS identity `DefaultAppPool` exists as a DB user; confirm the real one. Give it only the rights the app needs (`db_datareader`, `db_datawriter`, `EXECUTE` if needed) ([D-39](DECISIONS.md)).
- [ ] `DENY UPDATE, DELETE ON dbo.AuditEntries TO [<app user>];` for that login.
- [ ] The IIS app pool can write to the `App_Data` upload folders: `App_Data/ccp-files/`, `App_Data/org-documents/`, `App_Data/registry-proofs/`, `App_Data/intake-forms/`, `App_Data/lab-reports/`, `App_Data/consents/` (created on first upload).
- [ ] `App_Data` is included in the site backups (deployment step 2 and the regular backups).
- [ ] The audit trigger `TR_AuditEntries_AppendOnly` is **never disabled** in production; check after deployment: `SELECT is_disabled FROM sys.triggers WHERE name = 'TR_AuditEntries_AppendOnly'` returns 0.
- [ ] Optional setting `OrgModel:AssociationVerifier` = `PlatformAdmin` (default) or `CentralAssociation` (only relevant once the flag is on).

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

### Release candidate check (1.0.0-rc3, 2026-10-03)
- Includes D-14 to D-21 (CCP integrity and follow-up, migrations 002 and 003), migration 004 and the organization model code switched off (D-22 to D-32), and the removal of project dates from the landing page. `dotnet build`: 0 errors.
- Development run against LocalDB, flag off: admin login, batch list, batches by wholesaler, ManageBatch and a published Honey Story page return 200; the landing page shows no project dates. Not deployed anywhere.

## 2. Deployment day (in this order)
1. [ ] **Back up the production database** (full backup) and confirm it restores. The backup stays on the DB server and never goes into the repo or the deploy package.
2. [ ] Back up the current live site folder, including server-only uploads (`wwwroot/assets/videos/parameters/`, and `App_Data/ccp-files/`, `App_Data/org-documents/`, `App_Data/registry-proofs/`, `App_Data/intake-forms/`, `App_Data/lab-reports/`, `App_Data/consents/` once they exist, D-19, D-22, D-41, D-42, D-43).
3. [ ] Put the site into maintenance mode (e.g. `app_offline.htm`).
4. [ ] Apply every SQL script in `db/migrations/` **in numeric order** (`001_…`, `002_…`). Scripts are idempotent; record which ones were applied. The DB compatibility level must be 150 (SQL Server 2019).
   - `001_lock_out_non_admin_accounts.sql` ([D-09](DECISIONS.md)) locks out every account that is not an Administrator and prints how many it changed. It aborts if no Administrator exists. Run it again after steps 5-7 (it is idempotent): an account removed from the Administrator role there becomes a non-admin that must be locked.
   - `002_ccp_integrity.sql` ([D-14 to D-19](DECISIONS.md)) adds CCP applicability, the "Not applicable" status (id 5), parameter metadata and limits, honey classes and the in-range columns, and seeds them. It prints what it seeded; on LocalDB: 4 RawLot + 7 Batch CCPs, 21 + 48 data types, 22 default limits, 3 units, 13 limit rows. It aborts if `AssignmentStatuses` id 5 is already used by another status. Existing assignments, values and finalized batches are not changed. A second run changes 0 rows.
   - `004_org_model.sql` ([D-22 to D-32](DECISIONS.md), [ROLE-MODEL.md](ROLE-MODEL.md) P1) adds organizations, capabilities, memberships, documents, verification events, beekeeper association and registry tables, the append-only audit log, and approval / recorded-by columns. It inserts (never updates): the platform organization, an OrgAdmin membership for every Administrator, and one Pending organization per wholesaler. A second run inserts nothing. Nothing reads these tables while `Features:OrgRoleModel` is `false`.
   - `005_org_model_p2.sql` ([D-40](DECISIONS.md), [ROLE-MODEL.md](ROLE-MODEL.md) P2) runs after 004 (it aborts without it). It adds the CCP responsibility tables and seeds them by CCP code, the status "Submitted" (id 6; it aborts if id 6 is used by another status), the owning-org and finalize-request columns on lots and batches, and the correction table. Existing rows are not changed. A second run inserts nothing. Nothing reads these tables while `Features:OrgRoleModel` is `false`.
   - `006_custody_p3.sql` ([D-41](DECISIONS.md), [ROLE-MODEL.md](ROLE-MODEL.md) P3) runs after 005 (it aborts without it). It adds `LotIntakes`, `Shipments`, `HolderStorageRecords` and `HolderStorageValues`. Existing rows are not changed. A second run creates nothing. Nothing reads these tables while `Features:OrgRoleModel` is `false`.
   - `007_lab_p4.sql` ([D-42](DECISIONS.md), [ROLE-MODEL.md](ROLE-MODEL.md) P4) runs after 006 (it aborts without it). It adds `Samples`, `SampleCustodyEvents`, `LabReports`, `LabResults` and the nullable `CriticalControlPointMonitoringLogs.LabReportId`. Existing rows are not changed. A second run creates nothing. Nothing reads these tables while `Features:OrgRoleModel` is `false`.
   - `008_storefront_origin_p5.sql` ([D-43](DECISIONS.md), [ROLE-MODEL.md](ROLE-MODEL.md) P5) runs after 007 (it aborts without it). It adds `Consents`, `BeekeeperStorefronts`, `BatchOriginShares` and the nullable `RawHoneyProductions.OriginCountry` (default TR for new rows; existing rows stay NULL). Existing rows are not changed. A second run creates nothing. With `Features:OrgRoleModel` `false` only finalize writes `BatchOriginShares`; nothing shows it.
   - `003_ccp_followup.sql` ([D-20, D-21](DECISIONS.md)) runs after 002 (it aborts without it). It adds `PublicTextValues` (on for CCP 1–10, off for CCP 0), honey-type and ratio limit columns, the chestnut conductivity and fructose/glucose ratio rows, and the pine moisture notes. A second run changes 0 rows.
   - `009_reference_translations.sql` ([D-51](DECISIONS.md)) adds the table `ReferenceTranslations` (translations of reference data). A second run creates nothing.
   - `010_reference_translations_seed.sql` ([D-51](DECISIONS.md)) runs after 009 (it aborts without it). It inserts the tr, es, pl and lv translations (and English where the base text is not English) of the reference data, matched by the current base text, so a renamed row gets no translation and keeps its base text. Insert-only; a second run inserts nothing. Run it with `sqlcmd -I -f 65001` (UTF-8).
   - `011_provinces_districts.sql` ([D-52](DECISIONS.md)) adds `Provinces` (81, with plate codes) and `Districts` (973) and seeds them. Insert-only; a second run inserts nothing. Run it with `sqlcmd -I -f 65001` (UTF-8). The lists are marked to verify against the Ministry of Interior list.
   - `012_unicode_names.sql` ([D-55](DECISIONS.md)) changes beekeeper first and last names, apiary titles, apiary photo descriptions and production period titles from `varchar` to `nvarchar` (same length and nullability) so Turkish letters are stored. Existing values are kept; letters already lost are not restored. A second run changes nothing.
   - `013_role_permissions.sql` ([D-56](DECISIONS.md)) runs after 005 (it aborts without it). It adds `RolePermissions` and seeds it with the rights the code applied before (CCP read for every role; intake, samples, lab results, shipments, beekeeper pages, recall, finalize). Insert-only; a second run inserts nothing. Nothing reads it while `Features:OrgRoleModel` is `false`.
5. [ ] Review users in the **Administrator** role on production and remove unknown accounts. The register role-escalation bug was live until this deployment.
6. [ ] Also review who created the roles `Consumer`, `Wholesaler` and `SampleAcceptanceOfficer`, and any users in them.
7. [ ] Make sure at least one known admin account exists with the new, strong password.
8. [ ] Deploy the published Web build.
   - Keep the server-only config and the uploaded files (including `App_Data/ccp-files/`).
   - The app pool identity needs write access to `App_Data/ccp-files/` (created on the first CCP file upload; D-19). It is outside `wwwroot`, so files are only served through the admin-only download.
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
- [ ] A finalized batch shows "Finalized (locked)" and has no Edit/Delete/Unfinalize; its CCP pages are read-only (D-14). A draft batch lists what is missing before it can be finalized (D-16).
- [ ] Honey Story of a finalized batch lists every CCP; batches finalized before this release (ids 2–6 on the copy) show their missing CCPs as "Not recorded" (D-16).
- [ ] Audit log: after a few admin actions, `/Audit` lists them (login, creates, updates) and "Verify hash chain" reports that all rows verify; an `UPDATE` on `AuditEntries` with the app's DB login is refused (D-34, D-39).
- [ ] Error page: as an anonymous user, open `/Dashboard/Error`. Expect the "Something went wrong" page with a reference, not a login redirect. (Unhandled errors are logged with the same reference; `/honeystory/qrcode` no longer fails, it returns 404 for unknown codes.)
- [ ] Deleting a lookup value that is in use shows "This record is in use and cannot be deleted." (no 500).

## 4. Rollback
- Restore the previous site folder.
- Restore the DB backup only if a migration script caused damage. Scripts are additive, so the old app should run on the new schema.
- Rotated secrets stay rotated.
