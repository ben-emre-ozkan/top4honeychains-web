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
| RC-6 | **Account pages lost or never showed some messages.** Login and ResetPassword already rendered Identity errors in their own error blocks. However, ForgotPassword had no error display, an SMTP failure was an unhandled 500, and the reset success message was set in `ViewBag` before a redirect and lost. A signed-in non-admin was stuck in an AccessDenied loop (Login → Dashboard → AccessDenied, whose only link is the Dashboard). | `Controllers/AccountController.cs`; `Views/Account/{Login,ForgotPassword}.cshtml` (reproduced 2026-09-29) | FB-01, FB-02 |
| RC-7 | **The language switcher drops the query string.** `returnUrl = Context.Request.Path` loses values like `?wholesalerId=`, so pages that take query parameters return 404. | `Views/Shared/_horizontal.cshtml` L85-125 | FB-34 |
| RC-8 | **Role model.** "Administrator" is the only role that is enforced. Every business controller is admin-only, and the menu has no role checks. | ARCHITECTURE §2.4 | FB-02, FB-33 |

## Findings

| ID | Title | Area | Type | Severity | RC | Code location / cause | Decision | Status |
|---|---|---|---|---|---|---|---|---|
| **FB-01** | Password reset does not work | login | broken flow | **critical** | RC-6 | See note 1 below the table. | [D-03](DECISIONS.md): reset must work for admins | fixed (`fix/account-flows`, merged) |
| **FB-02** | "Sign up now" does not work | login | broken flow | **critical** | RC-6, RC-8 | The link itself was fine; the Register flow behind it was the problem. See note 2 below the table. | [D-02](DECISIONS.md), D-01: sign-up removed | fixed: sign-up removed (`fix/account-flows`, merged) |
| FB-03 | Mark required beekeeper fields (First/Last name, Birth date) with * | beekeeper | UX | low | RC-4 | `Views/Beekeepers/Create.cshtml`, `Edit.cshtml` | | fixed (`fix/required-markers`, merged) |
| FB-04 | Explain what the "File upload" field is for | beekeeper | UX | low | | `Views/Beekeepers/Create.cshtml` (profile photo, stored as a blob) | | fixed (`fix/upload-limits`, merged): the field is labelled "Profile photo (optional)" with a help text (where it is shown, allowed types, size) |
| FB-05 | Cancel button does not work (beekeeper) | beekeeper | broken flow | medium | RC-5 | `Views/Beekeepers/Create.cshtml` L90 (confirmed) | | fixed (`fix/cancel-buttons`, merged): Cancel is a link back to the list |
| FB-06 | Purpose type, beekeeping type and education level have no practical function | beekeeper | UX | low | | `Beekeepers` FKs to the lookups. Nothing uses them beyond display. | [D-08](DECISIONS.md): keep, make optional | fixed (`fix/lookup-optional`, merged) |
| FB-07 | Explain why Activate/Deactivate matters (beekeeper) | beekeeper | UX | low | | `BeekeepersController.Activate/Deactivate`: sets `IsActive` and locks the Identity login out | | fixed (`fix/deactivate-ux`, merged) |
| FB-08 | No delete function for beekeepers | beekeeper | broken flow | medium | | No delete exists by design (soft deactivate only). A hard delete would hit FKs (Apiaries) and the linked Identity user. | [D-04](DECISIONS.md): deactivate only, explain in UI | fixed (`fix/deactivate-ux`, merged) |
| FB-09 | Pick the apiary location on a map instead of typing coordinates | apiary | UX (feature) | low | | `Views/Apiaries/Create.cshtml`, `Edit.cshtml`. A Maps key already exists (see GO-LIVE secrets). | [D-07](DECISIONS.md) | deferred |
| FB-10 | Cancel button does not work (apiary) | apiary | broken flow | medium | RC-5 | `Views/Apiaries/Create.cshtml` L53 (confirmed) | | fixed (`fix/cancel-buttons`, merged): Cancel is a link back to the list |
| FB-11 | No validation for hive count and coordinates | apiary | validation | medium | RC-2 | `Models/Entities/Apiary.cs`: decimal lat/long without a `[Range]`, no hive-count rule | | fixed (`fix/form-validation`, merged) |
| FB-12 | Empty apiary form can be submitted with no error message | apiary | validation | high | RC-2 | `ApiariesController.Create` L51 checks `ModelState`, but the entity lacks rules. Cause unverified. | | fixed (`fix/form-validation`, merged) |
| FB-13 | Explain the purpose of Activate/Deactivate (apiary) | apiary | UX | low | | `ApiariesController.Activate/Deactivate` | | fixed (`fix/deactivate-ux`, merged) |
| FB-14 | Apiaries cannot be deleted | apiary | broken flow | medium | | No delete by design. FKs from ApiaryPhotos and RawHoneyProductions (NO ACTION). | [D-04](DECISIONS.md): deactivate only, explain in UI | fixed (`fix/deactivate-ux`, merged) |
| FB-15 | Mark all raw honey production fields as required | raw honey production | validation | medium | RC-2, RC-4 | `Views/RawHoneyProductions/Create.cshtml`, `Models/Entities/RawHoneyProduction.cs` | | fixed (`fix/form-validation`, `fix/required-markers`, merged) |
| FB-16 | Same period + honey type cannot be entered twice. What if another wholesaler buys from the same beekeeper? | raw honey production | validation | medium | | Duplicate check in `RawHoneyProductionsController.Create` covers apiary + period + honey type. The partner is waiting for a decision. | [D-06](DECISIONS.md), [PQ-01](PARTNER-QUESTIONS.md) | deferred |
| FB-17 | Link production period and honey type (a season can currently take any honey type) | raw honey production | validation | medium | | There is no period↔honey-type relation in the schema. Would need a DB script. | [D-06](DECISIONS.md), [PQ-01](PARTNER-QUESTIONS.md) | deferred |
| FB-18 | Mark required wholesaler fields with * | wholesalers | UX | low | RC-4 | `Views/Wholesalers/Create.cshtml` (only Title is required) | | fixed (`fix/required-markers`, merged) |
| **FB-19** | A new wholesaler cannot be created | wholesalers | broken flow | **critical** | | See note 3 below the table. | | fixed (`fix/wholesaler-create`, merged) |
| **FB-20** | Raw batches do not appear in the final production form, so no record can be created | final honey production | broken flow | **critical** | | See note 4 below the table. No longer blocked by FB-19; lots only appear once all their CCPs are completed. | [D-05](DECISIONS.md): rule stays, explain in UI | fixed (`fix/final-production-visibility`, merged) |
| FB-21 | Mark required user fields with * | administrators | UX | low | RC-4 | `Views/UserManagement/Create.cshtml` | | fixed (`fix/required-markers`, merged) |
| FB-22 | Show that Role is a dropdown (arrow icon) | administrators | UX | low | | `Views/UserManagement/Create.cshtml` L86: `<select class="form-control">` should use `form-select` (likely) | | fixed (`fix/form-select`, merged): the select uses Bootstrap `form-select`, which shows the arrow |
| FB-23 | Creating a user without a role returns HTTP 500 | administrators | crash/500 | high | RC-3 | **Confirmed:** the POST returns `View(model)` without `ViewBag.Roles`, and the view's `foreach` hits null (`UserManagementController.cs` L45-48, `Create.cshtml` L90). | | fixed (`fix/form-validation`, merged) |
| FB-24 | Saving an empty "Create CCP Parameter" form returns HTTP 500 | CCPs | crash/500 | high | RC-1, RC-2 | `CriticalControlPointParametersController.Create` L74. **Confirmed (reproduced):** `CCPId` posts 0 and passes validation, then the INSERT violates the FK (SQL 547). **Fixed:** the global `DbUpdateExceptionFilter` redirects back to the form with "a required selection is missing or invalid". Follow-up in `fix/form-validation`: field-level validation so the entered values are not lost. | | fixed (`fix/db-error-handling`, merged) |
| FB-25 | An empty CCP parameter category can be saved | CCPs | validation | medium | RC-2 | `Models/Entities/CriticalControlPointParameterCategory.cs`: `CategoryName` is nullable and has no `[Required]` | | fixed (`fix/form-validation`, merged) |
| FB-26 | The content video is required in practice but not marked as required | CCPs | validation | medium | RC-4 | `CriticalControlPointParameterContentsController.Create`, `Views/CriticalControlPointParameterContents/Create.cshtml`. The upload also lacks type and size checks (ARCHITECTURE §7). | | fixed (`fix/form-validation`, `fix/required-markers`, merged) |
| FB-27 | Support other media types, e.g. images (suggestion) | CCPs | UX (feature) | low | | same as FB-26 | [D-07](DECISIONS.md) | deferred |
| FB-28 | Add a dropdown icon to Content Type | CCPs | UX | low | | `Views/CriticalControlPointParameterContents/Create.cshtml` (same `form-select` issue as FB-22, likely) | | fixed (`fix/form-select`, merged): the select uses Bootstrap `form-select`, which shows the arrow |
| FB-29 | Mark the title field as required in every lookup table | lookup tables | validation | medium | RC-2, RC-4 | `Models/Entities/{HoneyType,BeekeepingType,BeekeepingPurposeType,BeekeeperEducationLevel,RawHoneyProductionPeriod}.cs` (none has `[Required]`) | | server rule already present (verified); marker: fixed (`fix/required-markers`, merged) |
| FB-30 | Deleting a record that is in use returns HTTP 500 instead of "this record is in use" | lookup tables | crash/500 | high | RC-1 | **Confirmed (reproduced):** the lookup `DeleteConfirmed` actions have no `DbUpdateException` handling (SQL 547). The same issue exists in Wholesalers and RawHoneyProductions. **Fixed globally:** `Filters/DbUpdateExceptionFilter.cs` catches FK/CHECK/unique violations in any controller, logs a warning, and returns to the previous page with a localized "This record is in use and cannot be deleted." | | fixed (`fix/db-error-handling`, merged) |
| FB-31 | An empty production period can be saved; all fields should be required | lookup tables | validation | medium | RC-2 | `Models/Entities/RawHoneyProductionPeriod.cs` (title and dates are all nullable) | | fixed (`fix/form-validation`, merged) |
| FB-32 | Enforce End Date > Start Date | lookup tables | validation | medium | RC-2 | `RawHoneyProductionPeriodsController` Create/Edit | | fixed (`fix/form-validation`, merged) |
| FB-33 | There are several roles but they all have the same functionality; role-based authorization is needed | general | broken flow | high | RC-8 | Only `[Authorize(Roles="Administrator")]` exists. The Beekeeper, Wholesaler, Consumer and SampleAcceptanceOfficer roles do nothing. | [D-01](DECISIONS.md): admins only this release | deferred |
| FB-34 | Changing the language on the wholesaler page returns 404 (`FinalHoneyProductions/ByWholesaler`) | general | broken flow | medium | RC-7 | **Confirmed:** `returnUrl` drops `?wholesalerId=`. `ByWholesaler(0)` then returns NotFound. | | fixed (`fix/language-switch`, merged): the switcher returns to path + query string; `SetLanguage` sends an empty or external `returnUrl` to `/` (was a 500). Verified on `ByWholesaler?wholesalerId=` |

### Notes on the critical findings

1. **FB-01 (password reset).** Code location: `AccountController` ForgotPassword/ResetPassword, `Services/EmailService.cs`.
   - **Reproduced locally (2026-09-29, smtp4dev):**
     - With a working SMTP server the reset itself works end to end, and invalid-token errors are shown.
     - The defects were: an SMTP failure caused an unhandled `SmtpException` (HTTP 500); the success message was lost after the redirect; and the "sent" message differed for existing vs. unknown e-mails (account enumeration).
     - Beekeepers' placeholder e-mails can never receive mail; that is accepted by [D-03](DECISIONS.md).
   - **Fix (`fix/account-flows`):**
     - An SMTP failure is logged, and the form shows "could not be sent, try later or contact an administrator".
     - The same "if an account exists…" message is shown in both cases.
     - After a reset, the login page shows a confirmation (TempData).
     - Messages are localized in 5 languages.
   - **Hardening after review (`fix/account-flows`):**
     - The reset link is built from `App:PublicBaseUrl`, not from the request's `Host` header, so a forged `Host` cannot send a real token to another site. Without the setting, Development falls back to the request and other environments refuse to send (logged). Verified: a POST with `Host: evil.example` produced a link to the configured URL.
     - Login and the lock screen lock an account for 15 minutes after 5 failed attempts. A wrong password and a locked account get the same message, so user names are not revealed.
     - Login POST, `UpdatePhone` and `ChangePassword` validate the anti-forgery token (login POST without token → 400). Login GET redirects after signing out a non-admin session, so the form's token is issued anonymously.
   - The production failure itself (Gmail credentials or `From` address) is still unverified. It will be checked with the go-live smoke test after the SMTP secrets are rotated.
2. **FB-02 (sign-up).** Per [D-02](DECISIONS.md), public self-registration is closed.
   - **Removed:** the `Register` GET/POST, `Views/Account/Register.cshtml`, the "Sign up now" link and its resource strings. `/Account/Register` now returns 404. Admins create users in `UserManagement/Create`.
   - **Non-admins ([D-01](DECISIONS.md)):**
     - Valid credentials without the Administrator role now get "This account does not have access to the web panel…" on the login page, and are not left signed in.
     - A stale non-admin session is signed out by `Login` (GET) and `AccessDenied` instead of looping.
   - The role-escalation part was fixed earlier (`1b89b58`).
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
   - **Fix (`fix/final-production-visibility`, [D-05](DECISIONS.md)):**
     - ManageBatch states the rule above the lot table and shows a warning when no lot is eligible.
     - A new "Raw honey lots not yet eligible" card lists the wholesaler's other lots with each missing CCP check (code and status). Every check links to its CCP monitoring page; lots without any CCP check link to "Assign CCP checks".
     - Verified with wholesaler 3: 9 lots, 1 without checks, 32 pending checks, matching the DB.

### Note on FB-07, FB-08, FB-13, FB-14 (deactivate instead of delete)
Per [D-04](DECISIONS.md) there is no delete. On branch `fix/deactivate-ux`:
- The Beekeepers list, the Apiaries list and the apiaries-of-a-beekeeper page explain that records are never deleted and what Deactivate and Activate do. For beekeepers the records are kept, the row is greyed out and the login is locked. For apiaries the records are kept, the row is greyed out and the apiary no longer counts as active on the dashboard.
- The Deactivate and Activate buttons have tooltips and ask for confirmation. They are POST forms with an anti-forgery token (before, a plain GET link changed the status, so a crafted link could deactivate a record).
- The beekeeper Edit form no longer has its own "Active" checkbox. It changed the status without locking or unlocking the login, and every edit cleared `DeactivatedAt` (reproduced). Status now changes only through Deactivate/Activate.
- Verified with a test beekeeper: deactivate locked the login (login refused), activate unlocked it, and editing kept the status.

### Note on FB-06 (optional lookup fields)
Per [D-08](DECISIONS.md) the fields stay and are optional. The DB columns were already nullable, and Create already accepted empty values.
- **Reproduced defect:** the beekeeper **Edit** form had no empty option. Editing a beekeeper without these values silently saved the first entry of each list, and a value could never be cleared again. None of the 62 beekeepers in the local copy currently has an empty value.
- **Fix (`fix/lookup-optional`):**
  - Create and Edit label the three fields "(optional)" and offer "Not specified".
  - In Edit you can now clear a value, and an unchanged edit keeps it empty.
- Verified with a test beekeeper (values 6/5/5 → cleared → unchanged edit → still `NULL`).

### Note on server-side validation (`fix/form-validation`, merged)
Reproduced 2026-09-29 by posting every Create form empty (client-side checks bypassed):
- **Empty rows were saved** for production periods (FB-31), CCP parameter categories (FB-25), raw honey productions (FB-15) and final honey batches (`FinalHoneyProductions.Create` ignored `ModelState`).
- **Rejected without any message:** apiaries (FB-12: the view had no field messages) and CCP contents (the implicit "video required" error was hidden). The CCP content **Edit** had the same implicit rule, so saving an edit without a new video failed silently.
- **HTTP 500:** user creation without a role (FB-23, roles were not reloaded).
- Beekeepers, CCPs, CCP parameters, wholesalers and all lookup tables already rejected empty input.

Fix:
- Entity rules: apiary title, hive count 1-10000, latitude ±90, longitude ±180 (FB-11); raw honey period, honey type, wholesaler and amount > 0 (FB-15); period title and dates, with end date after start date (FB-31, FB-32); category name (FB-25); batch title and date; CCP id ≥ 1 (FB-24 follow-up).
- `FinalHoneyProductions.Create` checks `ModelState`. User creation and raw honey Create reload their dropdowns after a failed POST, and user creation checks that the role exists. A new CCP content needs a video ("A video file is required."); on edit the video is optional and the old one is kept.
- The forms show field messages and keep the entered values. An empty number or date field now says "This field is required." instead of "The value '' is invalid."
- Follow-up (`fix/form-validation-range`): the decimal limit of the amount rule was parsed with the UI culture, so under `tr` the raw honey form could return HTTP 500 depending on which culture first used the rule. The limits are now parsed invariantly; verified under `tr` and `en`.
- Follow-up (`fix/final-batch-create`): once `FinalHoneyProductions.Create` honoured `ModelState`, every valid batch was rejected without a message, because the non-nullable navigation `FinalHoneyProductionWholesaler` counts as implicitly required. The batch **Edit** had the same hidden failure before this work (metadata edits were never saved). The navigation is now `[ValidateNever]`; verified: create, edit and the empty-title rejection.
- Verified in the browser: every form rejects empty input with messages and nothing is saved; values and dropdown selections are kept; a valid apiary, period and CCP content still save; editing a CCP content without a new video keeps the video.

### Note on required-field markers (RC-4, `fix/required-markers`)
Every Create and Edit form marks the fields that the server requires with a red `*` (CSS class `required` on the label, `wwwroot/assets/css/forms.css`, linked from `_HeadCSS`). The marked set matches the server rules from `fix/form-validation`: beekeeper first/last name and birth date (FB-03), apiary title/hives/coordinates, all raw honey fields (FB-15), period title and dates, category name, CCP id, CCP processing step and code, content type and (on create) the video (FB-26), lookup titles (FB-29), wholesaler company name (FB-18), all user fields including the role (FB-21), batch title and date. Verified in the browser on all 15 Create forms and on the apiary, beekeeper and wholesaler Edit forms.

### Note on decimal input (ENG-07, `fix/decimal-input`)
- **Reproduced (UI language `tr`):** apiary latitude `1.5` was stored as `15`, raw honey amount `1.5` as `15.00`. Text inputs and the hand-written number inputs on ManageBatch were parsed with the UI culture. The CCP monitoring numbers were already safe (tag-helper `type="number"` inputs post an `__Invariant` marker). On ManageBatch the saved amounts were rendered as `12,50` into `type="number"` inputs, which browsers show as empty.
- **Fix:** a global model binder (`ModelBinding/FlexibleDecimalModelBinder.cs`) for `decimal`/`double`/`float`: the right-most `.` or `,` is the decimal separator, a repeated single separator is a group separator, spaces are ignored; parsing is invariant. ManageBatch renders amounts invariantly.
- **Verified in en, tr, es, pl, lv:** coordinates `1.5` / `2,5` → 1.5 / 2.5; amounts `1.5`, `1,5` → 1.50, `1.234,5`, `1,234.5` → 1234.50, `abc` rejected; ManageBatch amount saved and shown again (tr `1.5`, pl `2.25`); CCP value `4.25` under lv stored as 4.25.
- **Read-only data scan (2026-09-29, local prod copy, aggregates only, no data changed):**
  - Apiaries (62): no coordinate outside ±90/±180, none at 0/0, all within lat 30–72 and lng −10–50; no 10×/100× pattern.
  - Raw honey amounts (61): median 5000 kg, range 1540–15600 kg; none ≥ 9× or ≤ 1/9 of the median, none ≤ 0.
  - Batch details (22): none above its lot amount, no lot used beyond its amount.
  - CCP numeric values (36 parameters with values): no value ≥ 9× or ≤ 1/9 of its parameter's median. Not a decimal problem, but worth a look by the partners: most parameters hold the same value repeated 10 times (demo-like data), and parameters 38–42 hold exactly the values of parameters 22–26 shifted by one position (e.g. "Moisture" 5.90 %, "Proline" 3.75 mg/kg, both implausible for honey), which looks like values entered under the wrong parameter.

## Proposed order of work

Owner-requested order for the decided items (2026-09-29): 2 → 4 → 9 → 10. The remaining items follow afterwards.

Shared root causes and critical items come first. Each work item is one branch.

| # | Work item | Covers | Decision needed first? |
|---|---|---|---|
| 1 | `fix/wholesaler-create`: set `CreatedAt` server-side, never from a hidden field (**done, merged**) | FB-19 (critical); unblocks FB-20 | no |
| 2 | (**done, merged**) `fix/account-flows`: remove self-registration (D-02); show validation and Identity errors on Login and Forgot/Reset password; non-admins get a clear message instead of an AccessDenied loop (D-01); verify reset end-to-end with a local SMTP catcher (D-03) | FB-01, FB-02 (critical), RC-6 | decided |
| 3 | (**done, merged**) `fix/db-error-handling`: catch FK violations on delete with an "in use, cannot be deleted" message; guard invalid inserts; make the error page anonymous and fix its layout | FB-30, FB-24, RC-1 | no |
| 4 | (**done, merged**) `fix/final-production-visibility`: show which lots are not eligible and which CCP checks are missing (D-05) | FB-20 (critical) | decided |
| 5 | (**done** in `fix/form-validation`) `fix/dropdown-repopulation` | FB-23, RC-3 | no |
| 6 | (**done, merged**) `fix/form-validation`: server-side rules; the `*` markers follow in `fix/required-markers` | FB-03, 11, 12, 15, 18, 21, 25, 26, 29, 31, 32 (RC-2, RC-4) | no |
| 7 | (**done, merged**) `fix/language-switch` | FB-34 | no |
| 8 | `fix/form-ux`: Cancel buttons, select styling, help texts | FB-04, 05, 07, 10, 13, 22, 28 | no |
| 9 | (**done, merged**) `fix/deactivate-ux`: make clear that beekeepers and apiaries are deactivated, not deleted, and what deactivation does (D-04) | FB-08, FB-14 (also FB-07, FB-13) | decided |
| 10 | (**done, merged**) `fix/lookup-optional`: education level, purpose type and beekeeping type are optional (D-08) | FB-06 | decided |
| – | Deferred to a later release | FB-09, FB-27 (D-07), FB-16, FB-17 (D-06, PQ-01), FB-33 (D-01) | decided |

## Engineering follow-ups (from code review, 2026-09-29)

These are not partner findings. They are pre-existing patterns found while reviewing `fix/wholesaler-create` and `fix/db-error-handling`.

| ID | Issue | Where | Status |
|---|---|---|---|
| ENG-01 | **Multi-step saves are not atomic.** `BeekeepersController.Create` creates the Identity user before saving the Beekeepers row. If the second step fails, the login remains and retries fail with "username taken" (the business number is regenerated identically). CCPMonitoring saves in two phases in the same way. Needs a transaction around both steps. | `BeekeepersController.Create`, `CCPMonitoringController` | open |
| ENG-02 | **Flash messages are inconsistent.** Controllers set `TempData["Error"/"Success"/"error"/"success"]`, but most pages never render them (e.g. "Password changed." on Profile is dropped). One shared alert partial should render them all; `DbUpdateExceptionFilter` should then write to it. | `Views/Shared/*`, 7+ controllers | fixed (`fix/flash-messages`, merged): `Views/Shared/_FlashMessages.cshtml` (both layouts) shows `Success`/`Error` in either key case plus the DB-error message; ManageBatch keeps its SweetAlert (`ViewData["FlashHandled"]`). The role page's own success popup never worked (script ran before SweetAlert loaded) and the user page ignored errors; both now use the shared banner. Verified: profile phone update, rejected apiary photo, role created, user password error, in-use delete. |
| ENG-03 | **Edits call `_context.Update()` on partially bound entities**, so any column not in the form is overwritten with its default. Load the tracked entity and copy the posted fields (or use `TryUpdateModelAsync` with a whitelist). | scaffolded `Edit` POST actions | open |
| ENG-04 | **A failed Create loses the entered values.** When a constraint violation redirects back, the typed data is gone. Field-level validation of FK ids (RC-2) avoids reaching the DB. | covered by `fix/form-validation` | fixed (`fix/form-validation`, merged): required ids are validated before saving, so the form is shown again with the typed values |
| ENG-06 | **Beekeeper logins were guessable:** `BeekeepersController.Create` set the password to the business number, and Activate unlocked the login. Fixed per [D-09](DECISIONS.md): random password and locked account on create, Activate keeps the lock, and `db/migrations/001_lock_out_non_admin_accounts.sql` locks existing non-admin accounts (verified on LocalDB: 63 of 63 locked, admins untouched, re-run changes 0 rows). | `BeekeepersController`, `db/migrations/001_…` | fixed (`fix/beekeeper-accounts`, merged) |
| ENG-07 | **Decimal input depends on the UI culture.** Coordinates and amounts are parsed with the current culture (default `tr`, decimal comma). In `tr`, `40.5` is read as `405`. Before `fix/form-validation` such values were saved silently; now the coordinate range rule rejects most of them, but a small value such as `1.5` still becomes `15`. Needs one input convention (e.g. `type="number"` plus invariant parsing for these fields). | `Apiaries` Create/Edit, amount fields | fixed (`fix/decimal-input`, merged), see the note on decimal input |
| ENG-08 | **Secrets in tracked files:** Gmail SMTP account and password in `appsettings.json`, admin credentials in a commented-out seed in `Program.cs`, Google Maps key in `Views/HoneyStory/Index.cshtml`. Fixed: placeholders in `appsettings.json`, seed deleted, Maps key read from `GoogleMaps:ApiKey`, local values in user-secrets, `appsettings.Production.json` git-ignored; production options documented in GO-LIVE. History not rewritten, so the three values must be rotated (GO-LIVE). Verified: HoneyStory loads the map with the key from user-secrets and without a key omits the script; the reset e-mail is sent through the user-secrets SMTP settings. | `appsettings.json`, `Program.cs`, `Views/HoneyStory/Index.cshtml` | fixed (`fix/secrets-out-of-config`, merged) |
| ENG-09 | **Anti-forgery gaps and deletes via GET:** role/user management POSTs had no token check; user delete (AJAX `$.post` without token), role delete and CCP content delete ran on GET. Fixed: global `AutoValidateAntiforgeryToken` filter; the three deletes are POST-only and posted from hidden forms with a token. Verified: all 68 POST endpoints return 400 without a token, the three GET deletes return 405; every POST flow re-tested with test records (lookups, periods, categories, CCPs, parameters, contents, wholesalers, beekeepers, apiaries, raw lots incl. CCP assignment and monitoring, batches incl. composition, CCPs, finalize/unfinalize, roles, users incl. password reset, profile, lock screen, logout, forgot/reset password). | `Program.cs`, `RoleManagement`, `UserManagement`, `CriticalControlPointParameterContents` | fixed (`fix/antiforgery-global`, merged) |
| ENG-10 | **Uploads were not restricted server-side.** CCP content videos were stored under `wwwroot` with the client's file extension and no checks, so any file type was served by the site (reproduced: an `.html` file uploaded as "video" was served as `text/html`). Apiary photos trusted the posted Content-Type; profile photos had no check; a non-image caused HTTP 500 in both. Fixed with `Services/UploadValidator.cs` (extension allow-list, file signature, size, image dimensions; limits in [D-10](DECISIONS.md), provisional); stored videos get a fixed `.mp4`/`.webm` extension. Verified: `.html`, a fake PNG and a 26 MB MP4 are rejected as videos; fake images are rejected as apiary and profile photos (nothing stored); a valid PNG and MP4 are accepted. | `ApiaryPhotosController`, `BeekeepersController`, `CriticalControlPointParameterContentsController` | fixed (`fix/upload-limits`, merged) |
| ENG-05 | **JavaScript error on ManageBatch:** `updateTotals()` uses an undefined variable `bar` (`ReferenceError: bar is not defined`), so the batch-total progress bar does not update when amounts change. Pre-existing; found while verifying FB-20. | `Views/FinalHoneyProductions/ManageBatch.cshtml` (scripts) | fixed (`fix/managebatch-js`, merged): the page has no progress bar any more, so the leftover colouring code was removed; no console error, "Total Selected" updates (verified on a finalized batch without submitting). Observation: on a finalized batch "Total Selected" starts at 0.00 because the amount inputs start empty |
