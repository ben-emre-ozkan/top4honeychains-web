# TOP4HoneyChains – Architecture (discovery notes)

Status: discovery written 2026-09-29 against the baseline commit `36b835f`. Fixes made since then are noted inline ("**Fixed on main**"). Nothing is deployed until go-live (see `docs/GO-LIVE.md`).
All paths are relative to `TOP4HONEYCHAINS/` unless noted. Secrets are masked as `***`.
Anything marked **unverified** could not be confirmed from the repo or the local DB.

---

## 0. Answers from the project owner (2026-09-29)

| Question | Answer |
|---|---|
| Is the local `TOP4HoneyChainsDb` a copy of production? | **Yes.** So the schema in §4, including the EF↔DB drift, reflects production. |
| Production SMTP credentials | Not a concern for now. Development continues with a local/dev SMTP. |
| How was the first Administrator created? | Unknown. Probably added manually (the seed in `Program.cs` is commented out). |
| QR content | No printed labels exist yet, so we are free to change the QR to encode the full public URL. |
| Are API / WebApps live? | The owner believes only the web side is live, not the API. Low priority. |
| Deployment | This codebase will go live after the current work, hosted on the team's university servers (INTALA Lab; Işık University inferred from contact links, unverified). |

---

## 1. The three apps

| App | Stack | Role | Evidence |
|---|---|---|---|
| `TOP4HoneyChains.Web` | .NET 8 MVC, EF Core 8, ASP.NET Identity | **Live web platform** | Only project with a publish profile (`Properties/PublishProfiles/FolderProfile.pubxml`, FileSystem → `C:\Users\sahin\Desktop\TOP4HONEYCHAINS\Publish`). `FolderProfile.pubxml.user` records the last publish on 2026-04-07, the same date as the newest source file (`Data/TOP4HoneyChainsContext.cs`). Only app with the full feature set in the test report (wholesalers, final honey, CCPs, QR page). |
| `TOP4HoneyChains.API` | .NET 8 Web API | **Unused template** | Contains only `WeatherForecastController`. The only package is Swashbuckle. No DbContext, auth, CORS or connection string. 3 source files, all dated 2026-02-03. No consumer found: Web makes no `HttpClient` calls, and there is no mobile/CORS/JWT hint. |
| `TOP4HoneyChainsWebApps` | .NET Framework 4.7.2 MVC5, EF6 (edmx, db-first) | **Legacy predecessor** | Same DB name (`TOP4HoneyChainsDb`). Shares its core tables (Apiaries, Beekeepers, lookups) but has its own disease/treatment/honey-test tables and no CCP/wholesaler/final-honey features. Last commit 2025-09-23. Not part of `TOP4HoneyChains.sln` (that solution holds only Web and API). |

- The domain `top4honeychains.intalalab.net` appears nowhere in the repo, so the mapping domain → Web is **unverified** (inferred from the evidence above). The only intalalab references are contact links in `Web/Views/Home/Index.cshtml`.
- `TOP4HoneyChainsWebApps/` is a **nested git repo** (own `.git`, origin `github.com/sahinaydin/TOP4HoneyChainsWebApps`). The outer repo stores it as a gitlink (mode 160000, commit `5f70cad`) with **no `.gitmodules`**, so its files are *not* in the outer repo.
- `SQLQuery1.sql` (repo root) is a DDL draft of an alternative traceability schema (PackingCompanies, RawHoneyLots, …). No code uses it.

---

## 2. Live app (`TOP4HoneyChains.Web`)

### 2.1 Entry point and pipeline – `Program.cs`

| Step | Line | Notes |
|---|---|---|
| `AddDbContext<ApplicationDbContext>` | L13 | Identity store, `DefaultConnection` |
| `AddDbContext<TOP4HoneyChainsContext>` | L14 | Business data, same `DefaultConnection` |
| `AddIdentity<ApplicationUser, IdentityRole>` | L16-25 | Password: length ≥ 6, no complexity rules. No `RequireConfirmedEmail`. Default token providers. |
| `ConfigureApplicationCookie` | L27-30 | Only sets `AccessDeniedPath = /Account/AccessDenied` |
| Localization | L32-55 | Cultures en, tr, es, pl, lv. Resources in `Resources/SharedResource.*.resx` |
| `Configure<SmtpSettings>("SmtpSettings")` + `IEmailService` | L58-60 | |
| Middleware | L66-80 | `UseExceptionHandler("/Dashboard/Error")` (non-Development only) → StaticFiles → Routing → RequestLocalization → AuthN → AuthZ → `{controller=Home}/{action=Index}/{id?}` |
| Seed (roles/admin) | – | Was fully commented out with hard-coded admin credentials; **deleted** in `fix/secrets-out-of-config` (still in git history, see GO-LIVE rotation). |

Missing from the pipeline: `UseHttpsRedirection`, `UseHsts`, `UseStatusCodePages`.

### 2.2 Folder structure / layers

| Folder | Content |
|---|---|
| `Controllers/` | 22 MVC controllers. They inject `TOP4HoneyChainsContext` directly. **No service or repository layer.** |
| `Data/` | `ApplicationDbContext.cs` (Identity), `TOP4HoneyChainsContext.cs` (scaffolded db-first, then hand-edited) |
| `Models/Entities/` | 20 scaffold-style entity classes |
| `Models/ViewModels/`, `Models/UsersManagement/` | View models |
| `Models/Identity/ApplicationUser.cs` | Empty `IdentityUser` subclass |
| `Models/Email/SmtpSettings.cs` | SMTP options POCO |
| `Services/EmailService.cs` | The only service |
| `Migrations/` | Identity migration only |
| `Views/` | One folder per controller. Shared layouts: `HorizontalLayout` (admin), `_BlankLayout` (auth pages). HoneyStory and Home use `Layout = null`. |
| `wwwroot/` | `assets/` (admin theme + vendored libs), `landingassets/` (landing page) |

### 2.3 Data access
- Two DbContexts on one connection string. Identity tables are managed by EF migrations. Business tables are db-first (see §4).
- `Beekeepers.BeekeeperId` (nvarchar) is set to `AspNetUsers.Id` in `BeekeepersController.Create`. This is an implicit link with no FK in the DB or in EF.

### 2.4 Authentication and authorization
- Cookie-based ASP.NET Identity. Login uses `lockoutOnFailure: false` (`AccountController.cs:52`).
- **Every business controller is `[Authorize(Roles = "Administrator")]`.** Public controllers: `HomeController` and `HoneyStoryController`. `AccountController` actions are `[AllowAnonymous]` where needed.
- Roles used in code:
  - `Administrator`: the only role that is enforced.
  - `Beekeeper`: assigned, but no feature uses it.
  - There is no Wholesaler role.
- No per-user ownership filtering exists. This is an admin-only back office.
- The menu (`Views/Shared/_horizontal.cshtml`) has no role checks.

### 2.5 Email and password reset
- `Services/EmailService.cs` uses `System.Net.Mail.SmtpClient` + `NetworkCredential`, with options from the `SmtpSettings` section (`Host, Port, EnableSsl, UserName, Password, From, SenderName`). It has no try/catch, no logging and no dispose.
- Password reset flow in `Controllers/AccountController.cs`:
  1. `ForgotPassword` POST (L106-143) calls `FindByEmailAsync`, then `GeneratePasswordResetTokenAsync`.
  2. The link is built from `App:PublicBaseUrl` (not the request's Host header; Development falls back to the request) and sent by email.
  3. `ResetPassword` GET/POST (L145-192) calls `ResetPasswordAsync`.
  - Views: `Views/Account/ForgotPassword.cshtml` and `Views/Account/ResetPassword.cshtml`.
  - ~~An SMTP failure is unhandled (500). The success message is lost because `ViewBag` is set before a redirect.~~ Fixed in `fix/account-flows`: the failure is logged and shown on the form, and the success message goes through TempData to the login page.
- Self-registration was removed in `fix/account-flows` ([D-02](DECISIONS.md)); users are created by admins in `UserManagement/Create`. Non-admins cannot use the panel ([D-01](DECISIONS.md)): login signs them out with a message.
- Local e-mail: SMTP settings are overridden with `dotnet user-secrets` (`UserSecretsId` in the csproj) to point at smtp4dev (`localhost:2525`, UI `http://localhost:5100`). See `CLAUDE.md`.

### 2.6 Error handling and logging
**Baseline** (confirmed at runtime 2026-09-29):
- `UseExceptionHandler("/Dashboard/Error")` pointed at an Administrator-only action, so anonymous users were redirected to login.
- `Error.cshtml` used `_Layout.cshtml`, which references the missing `_Header.cshtml`, so even admins got an empty 500.
- FK violations were unhandled 500s.
- There was no `ILogger` usage.

**Since `fix/db-error-handling` (merged to main):**
- `DashboardController.Error` is `[AllowAnonymous]`. It logs the exception with the reference ID shown to the user and renders `Error.cshtml` on `_BlankLayout` with localized text.
- A global MVC filter, `Filters/DbUpdateExceptionFilter.cs`, handles `DbUpdateException` with SQL errors 547 (FK/CHECK), 2601 and 2627 (unique):
  - It logs a warning and stores a localized message in TempData.
  - It redirects back to the posting page, or to the controller's Index.
  - `Views/Shared/_FlashMessages.cshtml`, rendered from `HorizontalLayout` and `_BlankLayout`, shows the message together with `TempData["Success"/"Error"]` (ENG-02).
- Not covered:
  - Custom 404 page.
  - Logging only goes to the default providers (console, Windows Event Log).
  - `_Layout.cshtml` is still broken, but nothing uses it after this change except `Shared/View.cshtml`.

---

## 3. Feature → code map (Web)

All controllers are in `Controllers/`, views in `Views/<Controller>/`, and entities in `Models/Entities/`. Table names match the DbSet names.

| Area (test report) | Controller(s) | Key actions | Views | Tables |
|---|---|---|---|---|
| Login / sign-up / reset | `AccountController` | Login, ~~Register~~ (removed, D-02), ForgotPassword, ResetPassword, Logout, Profile, ChangePassword, UpdatePhone, LockScreen/Unlock, AccessDenied | `Account/*` | AspNetUsers, AspNetRoles, AspNetUserRoles |
| Beekeeper profile | `BeekeepersController` | Index, Create, Edit, Activate, Deactivate (no delete; Details has no view) | `Beekeepers/Index, Create, Edit` | Beekeepers (+ lookups) |
| Apiary | `ApiariesController`, `ApiaryPhotosController` | Index, ByBeekeeper, Create, Edit, (De)Activate; photos: UploadSingle, UpdateDesc, Delete, Image | `Apiaries/*`, `ApiaryPhotos/Index` | Apiaries, ApiaryPhotos (blob) |
| Raw honey production | `RawHoneyProductionsController`, `RawHoneyProductionPeriodsController` | CRUD, RawHoneyProductionByApiary, ManageCCP, CCPOverview | `RawHoneyProductions/*`, `RawHoneyProductionPeriods/*` | RawHoneyProductions, RawHoneyProductionPeriods, RawHoneyCCPAssignments |
| Wholesalers | `WholesalersController` | CRUD, RawHoneyProductions(id) | `Wholesalers/*` | Wholesalers |
| Final honey production | `FinalHoneyProductionsController` | CRUD, ByWholesaler, ManageBatch(Post), FinalizeBatch, UnfinalizeBatch, ManageCCP, CCPOverview | `FinalHoneyProductions/*` | FinalHoneyProductions, FinalHoneyProductionDetails, FinalHoneyCCPAssignments |
| Administrators | `UserManagementController`, `RoleManagementController` | Users: Index, Create, Delete, ChangePassword. Roles: Index, Create, Edit, Delete | `UserManagement/*`, `RoleManagement/Index` | AspNet* |
| CCPs | `CriticalControlPointsController`, `CriticalControlPointParametersController`, `CriticalControlPointParameterCategoriesController`, `CriticalControlPointParameterContentsController`, `CCPMonitoringController` | CRUD; monitoring: Raw/FinalHoneyProductionManageCCP | `CriticalControlPoint*/*`, `CCPMonitoring/*` | CriticalControlPoints, …Parameters, …ParameterCategories, …ParameterContents (video uploads to `wwwroot/assets/videos/parameters/`), …MonitoringLogs |
| Lookup tables | `HoneyTypesController`, `BeekeepingTypesController`, `BeekeepingPurposeTypesController`, `BeekeeperEducationLevelsController` | Scaffolded CRUD | matching folders | HoneyTypes, BeekeepingTypes, BeekeepingPurposeTypes, BeekeeperEducationLevels. `AssignmentStatuses` has no UI; ids 1/2/3 are hard-coded. |
| Dashboard | `DashboardController` | Index, Privacy, Error | `Dashboard/*` | aggregates |
| Landing | `HomeController` (public) | Index, SetLanguage | `Home/Index` | – |
| **Consumer page (QR)** | `HoneyStoryController` (public, `[Route("honeystory")]`) | `GET /honeystory/{qr:guid}` → Index; `/honeystory/details/{qr}`; `/honeystory/qrcode?code=` (PNG); `/honeystory?id=` (photo); `/honeystory/parameter/{id}` | `HoneyStory/Index, Details, ParameterDetails` | FinalHoneyProductions (lookup by `FinalHoneyProductionQR` GUID) and everything joined under it |

QR notes:
- QR codes are generated by QRCoder in `HoneyStoryController.BatchQRCode` (L187). It uses System.Drawing, which is Windows-only.
- The views pass `code = Model.BatchQR`, so **the QR encodes a bare GUID, not a URL**. No printed labels exist yet, so the QR can be changed to encode the full URL.
- The page shows CCP logs by hard-coded CCP ids (2–11).
- `Apiaries.ApiaryQRCode` (GUID) is generated but not used for routing.

---

## 4. Database

### 4.1 Schema management
- **EF migrations cover Identity only.**
  - `Migrations/20260210180600_IdentityInit.cs` belongs to `ApplicationDbContext`.
  - `__EFMigrationsHistory` in the local DB holds exactly `20260210180600_IdentityInit` (EF 8.0.23), which **matches** the Migrations folder.
- **The business tables are managed manually (db-first).**
  - `TOP4HoneyChainsContext` has no migrations.
  - The DB also contains the trigger `TR_Check_WholesalerConsistency` on FinalHoneyProductionDetails and a `sysdiagrams` table.
  - No schema script for the current business tables is in the repo.
- The local DB is a copy of production (confirmed by the owner).

Local DB tables (27 + history):
- **Identity:** AspNet* (7)
- **Core:** Beekeepers, Apiaries, ApiaryPhotos
- **Production:** RawHoneyProductions, RawHoneyProductionPeriods, FinalHoneyProductions, FinalHoneyProductionDetails
- **Wholesalers:** Wholesalers
- **CCP:** CriticalControlPoints, CriticalControlPointParameters, …ParameterCategories, …ParameterContents, …MonitoringLogs, RawHoneyCCPAssignments, FinalHoneyCCPAssignments, AssignmentStatuses
- **Lookups:** HoneyTypes, BeekeepingTypes, BeekeepingPurposeTypes, BeekeeperEducationLevels
- **Other:** sysdiagrams

### 4.2 Foreign keys (local DB, `sys.foreign_keys`)

| Child → Parent | ON DELETE (DB) | EF model says |
|---|---|---|
| Apiaries → Beekeepers | NO ACTION | default |
| ApiaryPhotos → Apiaries | NO ACTION | default |
| ApiaryPhotos → RawHoneyProductionPeriods | NO ACTION | default |
| Beekeepers → BeekeepingTypes / BeekeepingPurposeTypes / BeekeeperEducationLevels | NO ACTION | default |
| RawHoneyProductions → Apiaries / HoneyTypes / RawHoneyProductionPeriods | NO ACTION | default |
| RawHoneyProductions → Wholesalers | NO ACTION | Restrict |
| RawHoneyCCPAssignments → RawHoneyProductions | NO ACTION | **Cascade** ⚠ |
| RawHoneyCCPAssignments → CriticalControlPoints / AssignmentStatuses | NO ACTION | default |
| FinalHoneyProductions → Wholesalers | NO ACTION | Restrict |
| FinalHoneyProductionDetails → FinalHoneyProductions | **CASCADE** | Cascade |
| FinalHoneyProductionDetails → RawHoneyProductions | NO ACTION | Restrict |
| FinalHoneyCCPAssignments → FinalHoneyProductions | NO ACTION | **Cascade** ⚠ |
| FinalHoneyCCPAssignments → CriticalControlPoints / AssignmentStatuses | NO ACTION | default |
| CriticalControlPointParameters → CriticalControlPoints / ParameterCategories | NO ACTION | default |
| CriticalControlPointParameterContents → Parameters | NO ACTION | **Cascade** ⚠ |
| MonitoringLogs → RawHoneyProductions / CriticalControlPoints / Parameters | NO ACTION | Restrict/default |
| MonitoringLogs → FinalHoneyProductions | **no FK in DB** ⚠ | Restrict (mapped in EF) |
| AspNet* children → AspNetUsers / AspNetRoles | CASCADE | Identity default |

⚠ = drift between the EF model (`Data/TOP4HoneyChainsContext.cs`) and the local DB.
- Where EF says Cascade but the DB says NO ACTION, EF deletes tracked dependents client-side only if they are loaded. Otherwise the delete fails with an FK error.
- Beekeepers ↔ AspNetUsers has **no FK**. Deleting a user in UserManagement leaves an orphan Beekeepers row.

---

## 5. Running locally (report only, nothing changed)

| Setting | Where | Current value | Local change needed |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `TOP4HoneyChains.Web/appsettings.json` L9 | `Server=SAHINAYDIN-PC\MSSQLSERVER2019; Database=TOP4HoneyChainsDb; Integrated Security; Encrypt=False; TrustServerCertificate=True; Pooling=False` | **Done:** `appsettings.Development.json` overrides it with `(localdb)\MSSQLLocalDB` (commit `1b89b58`). `appsettings.json` is unchanged. |
| `SmtpSettings` (Host `smtp.gmail.com`, 587, SSL, UserName, Password (placeholder in `appsettings.json` since `fix/secrets-out-of-config`; locally in user-secrets, in production server-side config), From, SenderName) | `appsettings.json` L11-19, bound in `Program.cs` L58 | Real Gmail account | Use a dev SMTP catcher (smtp4dev/Papercut) or a pickup directory. Do not send from the real account. |
| Launch URLs | `Properties/launchSettings.json` | `http://localhost:5255` (Kestrel), `http://localhost:35398` (IIS Express), HTTP only | none |
| Environment | launchSettings | `Development` → no exception handler, developer exception page | none |

Other prerequisites:
- .NET 8 SDK.
- Windows, because of System.Drawing in QR generation.
- A local DB that already has the business tables. They cannot be created from migrations.
- An Administrator user. The seed is commented out; the owner believes the existing admin was added manually. Since the local DB is a prod copy, an admin row probably exists there (unverified; user tables were not read).
- The Google Maps key is read from configuration `GoogleMaps:ApiKey` in `Views/HoneyStory/Index.cshtml` (was hard-coded until `fix/secrets-out-of-config`). Without a key the map script is not loaded.

---

## 6. Git state

- The outer repo has 1 commit: `36b835f Baseline: live version as received (TOP4HONEYCHAINS (4).rar)`. It tracks 1400 files.
- `git status --short` shows only ` m TOP4HONEYCHAINS/TOP4HoneyChainsWebApps`, which means the nested repo has uncommitted edits:
  - the csproj has an import reordered;
  - `Web.config` has its server changed from `HASAN\MSSQLSERVER2017` to `SAHINAYDIN-PC\MSSQLSERVER2019`.
- `git status --ignored --short -- TOP4HONEYCHAINS/TOP4HoneyChains.Web/wwwroot` lists these ignored files:

| Ignored | Needed at runtime? |
|---|---|
| `assets/libs/datatables.net*/node_modules/` (12 dirs), `assets/libs/gulp-rtlcss/node_modules/` | No. These are the libs' own dev dependencies. No view references `node_modules`. |
| `assets/libs/d3/bin/` (meteor, start, uglify) | No. These are build scripts. The page uses `assets/libs/d3/d3.min.js`, which **is tracked**. |
| `assets/libs/gmaps/yarn-error.log`, `landingassets/css/style.css.bak`, `landingassets/**/desktop.ini` | No |

→ No runtime file was lost to `.gitignore`. Outside wwwroot, only bin/obj/.vs and `*.user` files are ignored.
- User-uploaded videos go to `wwwroot/assets/videos/parameters/`. Uploads that exist only on the server are **not** in the repo (**unverified** whether any exist).

---

## 7. Risks and technical debt

**Security**
- ~~**Privilege escalation:** anonymous `Register` POST assigns `model.Role` from the form and creates the role if missing. Anyone can register as `Administrator`.~~ **Fixed on main** (`1b89b58`): self-registration always gets `Beekeeper`, no roles are created, and antiforgery is validated. Production stays vulnerable until go-live.
- **Guessable beekeeper logins:** `BeekeepersController.Create` sets both username and password to the sequential business number (`TR-BEE-{year}-{nnnnnn}`). There is no lockout.
- **Secrets in the repo:**
  - Gmail SMTP password in `appsettings.json`
  - Google Maps API key in `Views/HoneyStory/Index.cshtml`
  - admin credentials in the commented-out seed in `Program.cs`
- **CSRF / unsafe verbs:**
  - No antiforgery on Register, Login, UserManagement and RoleManagement.
  - Delete, Activate and Deactivate are reachable via GET (RoleManagement.Delete, UserManagement.Delete, ParameterContents.Delete, Beekeepers/Apiaries Activate/Deactivate).
- **Unrestricted upload:** ParameterContents video upload has no type or size check, keeps the client's extension, and writes under `wwwroot` (served statically).
- **Public data exposure:**
  - ~~`/honeystory/{guid}` shows beekeeper names, photos and apiary GPS, even for non-finalized batches~~: since D-12 only finalized batches are public (others 404); beekeeper name only; coordinates are rounded server-side to 0.1° and drawn as an area; `/honeystory?id=` serves apiary photos only for apiaries of finalized batches.
  - `/honeystory?id=N` enumerates every apiary photo by sequential id.
  - ~~`/honeystory/qrcode` renders arbitrary text anonymously~~: since D-11 it only renders the full public URL (`App:PublicBaseUrl` + `/honeystory/{guid}`) of an existing batch; anything else is 404.
- No HTTPS redirect or HSTS in the app. This may be handled by IIS or a reverse proxy (**unverified**).
- Overposting: RawHoneyProductions.Edit and CCP Parameters.Create bind the full entity.

**Reliability**
- ~~The error page is admin-only, and its layout references missing partials.~~ Fixed on main (`fix/db-error-handling`).
- ~~Deletes on FK-restricted rows throw unhandled 500s (Wholesalers, RawHoneyProductions, lookups).~~ Fixed on main (`fix/db-error-handling`) (global filter).
- An SMTP failure during password reset produces a 500.
- No logging anywhere.
- CCPMonitoring has null-reference paths.
- Hard-coded CCP ids and AssignmentStatus ids.

**Maintainability**
- There is no schema source of truth for the business tables (no migrations, no script), and the EF model has drifted from the DB (§4.2).
- There is no service layer and no tests.
- Dead weight:
  - the API template;
  - `SQLQuery1.sql`;
  - the legacy WebApps nested repo (a gitlink without `.gitmodules`);
  - about 200 csproj entries for vendored node_modules.
- The QR image encodes only the GUID, not a URL (§3).
- `Views/Shared/_Layout.cshtml` and `View.cshtml` look like unused template leftovers.
