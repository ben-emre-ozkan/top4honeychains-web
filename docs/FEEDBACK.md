# Feedback backlog – LBTU partner test report

Source: `docs/feedback/LBTU_Bulgu_Listesi_.docx`, last modified 2026-09-08, written in Turkish, 34 findings.
The report marks items 1, 2, 19 and 20 as critical (shown in bold in the report).

- **IDs:** `FB-NN` equals the item number in the report and never changes.
- **Paths:** relative to `TOP4HONEYCHAINS/TOP4HoneyChains.Web/`.
- **Cause labels:**
  - **confirmed** means it was read in the code.
  - **likely** means it is inferred from the code but not reproduced in the running app.
  - **unverified** means it has not been investigated yet.
- **Decision column:** links to the owner's decisions in [`DECISIONS.md`](DECISIONS.md) (D-xx) and to partner questions in [`PARTNER-QUESTIONS.md`](PARTNER-QUESTIONS.md) (PQ-xx). Status `deferred` means not in this release.

## Shared root causes

| RC | Root cause | Evidence | Findings |
|---|---|---|---|
| RC-1 | **Database exceptions are unhandled and return HTTP 500.** Nothing catches `DbUpdateException` (FK violations on delete, invalid inserts). The error page `/Dashboard/Error` is Administrator-only and its layout references missing partials (see ARCHITECTURE §2.6). | `*Controller.DeleteConfirmed` actions; `Program.cs` L66-69; `DashboardController.cs` L11/L214 | FB-24, FB-30 |
| RC-2 | **Server-side validation is missing.** Entities have no `[Required]`/`[Range]`/date rules. FK ints default to 0 and pass validation. Some actions skip `ModelState`, e.g. `FinalHoneyProductions.Create` and the commented-out block in `CriticalControlPointParameters.Create`. | `Models/Entities/*.cs` (the lookup entities have 0 `[Required]`) | FB-11, FB-12, FB-15, FB-24, FB-25, FB-29, FB-31, FB-32 |
| RC-3 | **Dropdown data is not reloaded when a POST fails validation.** The view then iterates a null `ViewBag`, which causes a NullReferenceException and a 500. | `UserManagementController.Create` POST (`ViewBag.Roles` is only set in GET) → `Views/UserManagement/Create.cshtml` L90. The same pattern exists in `RawHoneyProductions.Create` (wrong ViewData key). | FB-23 |
| RC-4 | **There is no required-field marker convention.** No view marks required fields with `*`. | all `Views/*/Create.cshtml`, `Edit.cshtml` | FB-03, FB-15, FB-18, FB-21, FB-26, FB-29 |
| RC-5 | **"Cancel" is `<button type="reset">`.** It clears the form instead of navigating back. | `Views/Beekeepers/Create.cshtml` L90, `Views/Apiaries/Create.cshtml` L53 | FB-05, FB-10 |
| RC-6 | **Account pages hide errors.** Identity errors are added under the empty key, but Login, Register and ResetPassword have no validation summary. `ViewBag.Message` is set before a redirect and is lost. The Login page shows no messages. | `Controllers/AccountController.cs` L93-94, L175-191; `Views/Account/{Login,Register,ResetPassword}.cshtml` | FB-01, FB-02 |
| RC-7 | **The language switcher drops the query string.** `returnUrl = Context.Request.Path` loses values like `?wholesalerId=`, so pages that take query parameters return 404. | `Views/Shared/_horizontal.cshtml` L85-125 | FB-34 |
| RC-8 | **Role model.** "Administrator" is the only role that is enforced. Every business controller is admin-only, and the menu has no role checks. | ARCHITECTURE §2.4 | FB-02, FB-33 |

## Findings

| ID | Title | Area | Type | Severity | RC | Code location / cause | Decision | Status |
|---|---|---|---|---|---|---|---|---|
| **FB-01** | Password reset does not work | login | broken flow | **critical** | RC-6 | See note 1 below the table. | [D-03](DECISIONS.md): reset must work for admins | open |
| **FB-02** | "Sign up now" does not work | login | broken flow | **critical** | RC-6, RC-8 | The link itself is fine (`Views/Account/Login.cshtml` L226). See note 2 below the table. | [D-02](DECISIONS.md), D-01: sign-up removed | open |
| FB-03 | Mark required beekeeper fields (First/Last name, Birth date) with * | beekeeper | UX | low | RC-4 | `Views/Beekeepers/Create.cshtml`, `Edit.cshtml` | | open |
| FB-04 | Explain what the "File upload" field is for | beekeeper | UX | low | | `Views/Beekeepers/Create.cshtml` (profile photo, stored as a blob) | | open |
| FB-05 | Cancel button does not work (beekeeper) | beekeeper | broken flow | medium | RC-5 | `Views/Beekeepers/Create.cshtml` L90 (confirmed) | | open |
| FB-06 | Purpose type, beekeeping type and education level have no practical function | beekeeper | UX | low | | `Beekeepers` FKs to the lookups. Nothing uses them beyond display. | [D-08](DECISIONS.md): keep, make optional | open |
| FB-07 | Explain why Activate/Deactivate matters (beekeeper) | beekeeper | UX | low | | `BeekeepersController.Activate/Deactivate`: sets `IsActive` and locks the Identity login out | fixed (branch `fix/deactivate-ux`, not merged) |
| FB-08 | No delete function for beekeepers | beekeeper | broken flow | medium | | No delete exists by design (soft deactivate only). A hard delete would hit FKs (Apiaries) and the linked Identity user. | [D-04](DECISIONS.md): deactivate only, explain in UI | fixed (branch `fix/deactivate-ux`, not merged) |
| FB-09 | Pick the apiary location on a map instead of typing coordinates | apiary | UX (feature) | low | | `Views/Apiaries/Create.cshtml`, `Edit.cshtml`. A Maps key already exists (see GO-LIVE secrets). | [D-07](DECISIONS.md) | deferred |
| FB-10 | Cancel button does not work (apiary) | apiary | broken flow | medium | RC-5 | `Views/Apiaries/Create.cshtml` L53 (confirmed) | | open |
| FB-11 | No validation for hive count and coordinates | apiary | validation | medium | RC-2 | `Models/Entities/Apiary.cs`: decimal lat/long without a `[Range]`, no hive-count rule | | open |
| FB-12 | Empty apiary form can be submitted with no error message | apiary | validation | high | RC-2 | `ApiariesController.Create` L51 checks `ModelState`, but the entity lacks rules. Cause unverified. | | open |
| FB-13 | Explain the purpose of Activate/Deactivate (apiary) | apiary | UX | low | | `ApiariesController.Activate/Deactivate` | fixed (branch `fix/deactivate-ux`, not merged) |
| FB-14 | Apiaries cannot be deleted | apiary | broken flow | medium | | No delete by design. FKs from ApiaryPhotos and RawHoneyProductions (NO ACTION). | [D-04](DECISIONS.md): deactivate only, explain in UI | fixed (branch `fix/deactivate-ux`, not merged) |
| FB-15 | Mark all raw honey production fields as required | raw honey production | validation | medium | RC-2, RC-4 | `Views/RawHoneyProductions/Create.cshtml`, `Models/Entities/RawHoneyProduction.cs` | | open |
| FB-16 | Same period + honey type cannot be entered twice. What if another wholesaler buys from the same beekeeper? | raw honey production | validation | medium | | Duplicate check in `RawHoneyProductionsController.Create` covers apiary + period + honey type. The partner is waiting for a decision. | [D-06](DECISIONS.md), [PQ-01](PARTNER-QUESTIONS.md) | deferred |
| FB-17 | Link production period and honey type (a season can currently take any honey type) | raw honey production | validation | medium | | There is no period↔honey-type relation in the schema. Would need a DB script. | [D-06](DECISIONS.md), [PQ-01](PARTNER-QUESTIONS.md) | deferred |
| FB-18 | Mark required wholesaler fields with * | wholesalers | UX | low | RC-4 | `Views/Wholesalers/Create.cshtml` (only Title is required) | | open |
| **FB-19** | A new wholesaler cannot be created | wholesalers | broken flow | **critical** | | See note 3 below the table. | | fixed (`fix/wholesaler-create`, merged) |
| **FB-20** | Raw batches do not appear in the final production form, so no record can be created | final honey production | broken flow | **critical** | | See note 4 below the table. No longer blocked by FB-19; lots only appear once all their CCPs are completed. | [D-05](DECISIONS.md): rule stays, explain in UI | open |
| FB-21 | Mark required user fields with * | administrators | UX | low | RC-4 | `Views/UserManagement/Create.cshtml` | | open |
| FB-22 | Show that Role is a dropdown (arrow icon) | administrators | UX | low | | `Views/UserManagement/Create.cshtml` L86: `<select class="form-control">` should use `form-select` (likely) | | open |
| FB-23 | Creating a user without a role returns HTTP 500 | administrators | crash/500 | high | RC-3 | **Confirmed:** the POST returns `View(model)` without `ViewBag.Roles`, and the view's `foreach` hits null (`UserManagementController.cs` L45-48, `Create.cshtml` L90). | | open |
| FB-24 | Saving an empty "Create CCP Parameter" form returns HTTP 500 | CCPs | crash/500 | high | RC-1, RC-2 | `CriticalControlPointParametersController.Create` L74. **Confirmed (reproduced):** `CCPId` posts 0 and passes validation, then the INSERT violates the FK (SQL 547). **Fixed:** the global `DbUpdateExceptionFilter` redirects back to the form with "a required selection is missing or invalid". Follow-up in `fix/form-validation`: field-level validation so the entered values are not lost. | | fixed (`fix/db-error-handling`, merged) |
| FB-25 | An empty CCP parameter category can be saved | CCPs | validation | medium | RC-2 | `Models/Entities/CriticalControlPointParameterCategory.cs`: `CategoryName` is nullable and has no `[Required]` | | open |
| FB-26 | The content video is required in practice but not marked as required | CCPs | validation | medium | RC-4 | `CriticalControlPointParameterContentsController.Create`, `Views/CriticalControlPointParameterContents/Create.cshtml`. The upload also lacks type and size checks (ARCHITECTURE §7). | | open |
| FB-27 | Support other media types, e.g. images (suggestion) | CCPs | UX (feature) | low | | same as FB-26 | [D-07](DECISIONS.md) | deferred |
| FB-28 | Add a dropdown icon to Content Type | CCPs | UX | low | | `Views/CriticalControlPointParameterContents/Create.cshtml` (same `form-select` issue as FB-22, likely) | | open |
| FB-29 | Mark the title field as required in every lookup table | lookup tables | validation | medium | RC-2, RC-4 | `Models/Entities/{HoneyType,BeekeepingType,BeekeepingPurposeType,BeekeeperEducationLevel,RawHoneyProductionPeriod}.cs` (none has `[Required]`) | | open |
| FB-30 | Deleting a record that is in use returns HTTP 500 instead of "this record is in use" | lookup tables | crash/500 | high | RC-1 | **Confirmed (reproduced):** the lookup `DeleteConfirmed` actions have no `DbUpdateException` handling (SQL 547). The same issue exists in Wholesalers and RawHoneyProductions. **Fixed globally:** `Filters/DbUpdateExceptionFilter.cs` catches FK/CHECK/unique violations in any controller, logs a warning, and returns to the previous page with a localized "This record is in use and cannot be deleted." | | fixed (`fix/db-error-handling`, merged) |
| FB-31 | An empty production period can be saved; all fields should be required | lookup tables | validation | medium | RC-2 | `Models/Entities/RawHoneyProductionPeriod.cs` (title and dates are all nullable) | | open |
| FB-32 | Enforce End Date > Start Date | lookup tables | validation | medium | RC-2 | `RawHoneyProductionPeriodsController` Create/Edit | | open |
| FB-33 | There are several roles but they all have the same functionality; role-based authorization is needed | general | broken flow | high | RC-8 | Only `[Authorize(Roles="Administrator")]` exists. The Beekeeper, Wholesaler, Consumer and SampleAcceptanceOfficer roles do nothing. | [D-01](DECISIONS.md): admins only this release | deferred |
| FB-34 | Changing the language on the wholesaler page returns 404 (`FinalHoneyProductions/ByWholesaler`) | general | broken flow | medium | RC-7 | **Confirmed:** `returnUrl` drops `?wholesalerId=`. `ByWholesaler(0)` then returns NotFound. | | open |

### Notes on the critical findings

1. **FB-01 (password reset).** Code location: `AccountController` ForgotPassword/ResetPassword (L106-194), `Services/EmailService.cs`. Likely causes:
   - (a) Beekeeper accounts created by admins get the fake address `{businessNo}@top4honeychains.com`, so reset mail can never reach them.
   - (b) Reset errors are invisible (RC-6) and the success message is lost.
   - (c) An SMTP failure is unhandled.
   - The actual failure on production is unverified.
2. **FB-02 (sign-up).** Code location: `AccountController.Register`. Likely causes:
   - (a) Identity errors such as a duplicate username are not shown (RC-6), so the form just reloads.
   - (b) After a successful sign-up the user gets `Beekeeper`, and Login redirects to the admin-only Dashboard, which ends in AccessDenied.
   - The role-escalation part is already fixed (`1b89b58`).
3. **FB-19 (wholesaler creation).** Code location: `WholesalersController.Create`/`Edit`, `Views/Wholesalers/Create.cshtml`, `Edit.cshtml`.
   - **Confirmed cause (reproduced 2026-09-29):** the hidden `CreatedAt` field posted an empty value. Binding it to a non-nullable `DateTime` failed, so `ModelState` was invalid. The `ModelOnly` validation summary hid that property error, and the form silently re-rendered without saving. The earlier "0001-01-01 → 500" guess was wrong: the request never reached the DB.
   - **Fix:** `CreatedAt` is no longer bound from the form. Create sets it on the server; Edit keeps the stored value (`IsModified = false`).
   - Verified with Playwright: create saves with a server timestamp; edit keeps `CreatedAt`.
   - No other form posts timestamps through hidden fields. All other timestamps (`MeasurementDate`, `*StartDate`, `FinalizedDate`, `DeactivatedAt`, `ApiaryPhotoCreatedDate`) are already set on the server.
4. **FB-20 (raw batches missing).** Code location: `FinalHoneyProductionsController.ManageBatch` L69-80.
   - **Confirmed filter:** only raw lots from the **same wholesaler** with **at least one CCP assignment, all in status 3 (Completed)** are listed.
   - **Checked after the FB-19 fix (2026-09-29, local prod copy):**
     - New wholesalers can now get batches, but the ManageBatch list is empty ("No data available in table") with no explanation.
     - For wholesaler 2, the list shows exactly its 10 eligible lots.
     - Data-wide, only 10 of 61 raw lots are eligible: 49 have incomplete CCPs and 2 have none.
   - **So FB-20 is mainly the CCP-completion gate** plus a missing empty-state message, not a crash. Decision [D-05](DECISIONS.md): the rule stays and the UI must explain it.
   - `Create` POST also skips `ModelState`.

### Note on FB-07, FB-08, FB-13, FB-14 (deactivate instead of delete)
Per [D-04](DECISIONS.md) there is no delete. On branch `fix/deactivate-ux`:
- The Beekeepers list, the Apiaries list and the apiaries-of-a-beekeeper page explain that records are never deleted and what Deactivate and Activate do. For beekeepers the records are kept, the row is greyed out and the login is locked. For apiaries the records are kept, the row is greyed out and the apiary no longer counts as active on the dashboard.
- The Deactivate and Activate buttons have tooltips.
- The beekeeper Edit form no longer has its own "Active" checkbox. It changed the status without locking or unlocking the login, and every edit cleared `DeactivatedAt` (reproduced). Status now changes only through Deactivate/Activate.
- Verified with a test beekeeper: deactivate locked the login (login refused), activate unlocked it, and editing kept the status.

## Proposed order of work

Owner-requested order for the decided items (2026-09-29): 2 → 4 → 9 → 10. The remaining items follow afterwards.

Shared root causes and critical items come first. Each work item is one branch.

| # | Work item | Covers | Decision needed first? |
|---|---|---|---|
| 1 | `fix/wholesaler-create`: set `CreatedAt` server-side, never from a hidden field (**done, merged**) | FB-19 (critical); unblocks FB-20 | no |
| 2 | `fix/account-flows`: remove self-registration (D-02); show validation and Identity errors on Login and Forgot/Reset password; non-admins get a clear message instead of an AccessDenied loop (D-01); verify reset end-to-end with a local SMTP catcher (D-03) | FB-01, FB-02 (critical), RC-6 | decided |
| 3 | (**done, merged**) `fix/db-error-handling`: catch FK violations on delete with an "in use, cannot be deleted" message; guard invalid inserts; make the error page anonymous and fix its layout | FB-30, FB-24, RC-1 | no |
| 4 | `fix/final-production-visibility`: show which lots are not eligible and which CCP checks are missing (D-05) | FB-20 (critical) | decided |
| 5 | `fix/dropdown-repopulation` | FB-23, RC-3 | no |
| 6 | `fix/form-validation`: entity rules + required `*` markers across all forms | FB-03, 11, 12, 15, 18, 21, 25, 26, 29, 31, 32 (RC-2, RC-4) | no |
| 7 | `fix/language-switch-query` | FB-34 | no |
| 8 | `fix/form-ux`: Cancel buttons, select styling, help texts | FB-04, 05, 07, 10, 13, 22, 28 | no |
| 9 | `fix/deactivate-ux`: make clear that beekeepers and apiaries are deactivated, not deleted, and what deactivation does (D-04) | FB-08, FB-14 (also FB-07, FB-13) | decided |
| 10 | `fix/lookup-optional`: education level, purpose type and beekeeping type are optional (D-08) | FB-06 | decided |
| – | Deferred to a later release | FB-09, FB-27 (D-07), FB-16, FB-17 (D-06, PQ-01), FB-33 (D-01) | decided |

## Engineering follow-ups (from code review, 2026-09-29)

These are not partner findings. They are pre-existing patterns found while reviewing `fix/wholesaler-create` and `fix/db-error-handling`.

| ID | Issue | Where | Status |
|---|---|---|---|
| ENG-01 | **Multi-step saves are not atomic.** `BeekeepersController.Create` creates the Identity user before saving the Beekeepers row. If the second step fails, the login remains and retries fail with "username taken" (the business number is regenerated identically). CCPMonitoring saves in two phases in the same way. Needs a transaction around both steps. | `BeekeepersController.Create`, `CCPMonitoringController` | open |
| ENG-02 | **Flash messages are inconsistent.** Controllers set `TempData["Error"/"Success"/"error"/"success"]`, but most pages never render them (e.g. "Password changed." on Profile is dropped). One shared alert partial should render them all; `DbUpdateExceptionFilter` should then write to it. | `Views/Shared/*`, 7+ controllers | open |
| ENG-03 | **Edits call `_context.Update()` on partially bound entities**, so any column not in the form is overwritten with its default. Load the tracked entity and copy the posted fields (or use `TryUpdateModelAsync` with a whitelist). | scaffolded `Edit` POST actions | open |
| ENG-04 | **A failed Create loses the entered values.** When a constraint violation redirects back, the typed data is gone. Field-level validation of FK ids (RC-2) avoids reaching the DB. | covered by `fix/form-validation` | open |
