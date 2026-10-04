# TOP4HoneyChains web panel – release notes

**Version:** 1.0.0-rc4 (release candidate, not yet deployed)
**Date:** 2026-10-04
**Audience:** project partners (LBTU test report follow-up)

This release answers the LBTU partner test report. Each finding keeps its number (FB-01 … FB-34).
In this release only administrators use the web panel; beekeeper and wholesaler logins are planned for a later release.

Status legend: **fixed** = changed in this release · **deferred** = agreed to move to a later release · **needs decision** = waiting for an answer.

**rc4 (2026-10-04):** the audit log is on from go-live for all users, independent of the organization model; administrators find it under User Management → Audit log.

## What is new in rc3 (2026-10-03)

### CCP integrity
- **Finalized batches are locked.** After a batch is finalized, its data, its composition, its CCP assignments and CCP values can no longer be changed or deleted; the same applies to every raw honey lot used in it. The pages become read-only. The "Unfinalize" button was removed: a correction needs a new batch.
- **Recorded values are never lost.** A CCP that already has values (or a "Not applicable" record) cannot be removed from a lot or batch.
- **"Not applicable".** A CCP can be marked as not applicable (for example no heating or pasteurization of raw honey) with a required reason; the user and time are recorded. The public Honey Story page shows it as "Not applicable" with the reason.
- **Finalize rule.** A batch can be finalized only when all its CCPs (and those of its lots) are completed or not applicable. The batch page lists what is still missing.
- **CCPs by type of record.** Each CCP is marked for raw honey lots (CCP 0–3) or for final batches (CCP 4–10); the assignment pages only offer the matching ones.
- **Parameter details and limits.** Parameters have a data type (number, yes/no, text, file), unit, minimum/maximum and a required flag, with limits per honey type where needed (for example floral and honeydew/pine honey). Values are checked when saved and shown green or red. Lab reports can be attached as files (PDF or image, admin download only). All limits are starting values from the Turkish Food Codex and are marked **to verify**; the public page shows compliance only for verified limits.
- **Honey Story shows gaps.** Every CCP of the batch and its lots is listed. Missing ones appear as "Not recorded" instead of being hidden. The five batches finalized before this release are unchanged; their missing CCPs now show as "Not recorded".

### CCP follow-up
- CCP 0 (field audit at the beekeeper) shows only its status and yes/no answers publicly; free-text answers and files are never published (a per-CCP setting).
- Chestnut honey uses the honeydew rule for electrical conductivity; a fructose/glucose ratio check (0.9–1.4) was added for floral honey; the pine 23 % moisture value is marked as doubtful. All to verify.
- A one-page Turkish table of all limits was prepared for the accredited partner lab (`docs/LIMITS-TO-VERIFY-TR.md`, question PQ-03).

### Other
- The public landing page no longer shows project dates.
- The code for the organization and role model (phase 1, `docs/ROLE-MODEL.md`) is included but **switched off** (`Features:OrgRoleModel = false`). The panel works exactly as before: administrators only.

### Database scripts for this release
Run in numeric order at deployment (see `docs/GO-LIVE.md`); all are additive and can be re-run safely:
- `002_ccp_integrity.sql` – CCP types, "Not applicable", parameter details and limits.
- `003_ccp_followup.sql` – public text setting, chestnut and ratio limits, notes.
- `004_org_model.sql` – tables for the organization model (not used while it is switched off).

## Findings from the test report

### Login and accounts
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-01 | Password reset does not work | fixed | Reset e-mails are sent reliably, errors are shown instead of a crash, and the login page confirms a successful reset. |
| FB-02 | "Sign up now" does not work | fixed | Public sign-up is closed; administrators create users. Non-admin accounts get a clear "no access to the web panel" message. |
| FB-21 | Mark required user fields with * | fixed | All required fields of the user form are marked with a red *. |
| FB-22 | Show that Role is a dropdown | fixed | The role field now shows the dropdown arrow. |
| FB-23 | Creating a user without a role crashes | fixed | The form shows "The Role field is required." and keeps the entered values. |

### Beekeepers
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-03 | Mark required beekeeper fields | fixed | First name, last name and birth date are marked with *. |
| FB-04 | Explain the "File upload" field | fixed | Labelled "Profile photo (optional)" with a short explanation, allowed types and size. |
| FB-05 | Cancel button does not work | fixed | Cancel returns to the beekeeper list instead of clearing the form. |
| FB-06 | Purpose type, beekeeping type and education level have no function | fixed | The three fields stay but are clearly optional ("Not specified"). |
| FB-07 | Explain Activate/Deactivate | fixed | The list explains what deactivation does; both buttons ask for confirmation. |
| FB-08 | No delete function for beekeepers | fixed | By design beekeepers are deactivated, never deleted (traceability); the page now says so. |

### Apiaries
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-09 | Pick the location on a map | deferred | Planned for the next release. |
| FB-10 | Cancel button does not work | fixed | Cancel returns to the beekeeper's apiaries. |
| FB-11 | No validation for hive count and coordinates | fixed | Hive count 1–10000, latitude ±90, longitude ±180. Decimal numbers are read correctly in every UI language ("1.5" and "1,5"). |
| FB-12 | Empty apiary form can be submitted without a message | fixed | Every field shows its own error message. |
| FB-13 | Explain Activate/Deactivate | fixed | The page explains what deactivation does; both buttons ask for confirmation. |
| FB-14 | Apiaries cannot be deleted | fixed | By design apiaries are deactivated, never deleted (traceability); the page now says so. |

### Raw honey production
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-15 | Mark all fields as required | fixed | Period, honey type, wholesaler and amount are required, marked with * and checked on the server; the amount must be above 0. |
| FB-16 | Same period + honey type cannot be entered twice (several wholesalers) | needs decision | Rule unchanged in this release; question PQ-01 below. |
| FB-17 | Link production period and honey type | needs decision | Unchanged in this release; question PQ-01 below. |

### Wholesalers
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-18 | Mark required fields with * | fixed | The company name is marked as required. |
| FB-19 | A new wholesaler cannot be created | fixed | Wholesalers can be created again. |

### Final honey production
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-20 | Raw batches do not appear in the final production form | fixed | The rule is unchanged (a raw lot is usable once all its CCP checks are completed), but the page now lists the lots that are not yet usable and which CCP checks are missing. Saved batch amounts are shown again, and batch creation and editing save correctly. |

### CCPs and content
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-24 | Saving an empty "Create CCP Parameter" form crashes | fixed | A clear message is shown instead of an error page. |
| FB-25 | An empty category can be saved | fixed | The category name is required. |
| FB-26 | The content video is required but not marked | fixed | Marked with *, with a clear message; allowed types and size are checked (MP4 or WEBM, up to 25 MB). |
| FB-27 | Support other media types (e.g. images) | deferred | Planned for the next release. |
| FB-28 | Dropdown icon for Content Type | fixed | The field now shows the dropdown arrow. |

### Lookup tables
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-29 | Mark the title field as required | fixed | Titles were already required; they are now marked with *. |
| FB-30 | Deleting a record in use crashes | fixed | The message "This record is in use and cannot be deleted." is shown. |
| FB-31 | An empty production period can be saved | fixed | Title, start date and end date are required. |
| FB-32 | End date must be after start date | fixed | Enforced with a message on the end date. |

### General
| ID | Finding | Status | Change |
|---|---|---|---|
| FB-33 | Role-based authorization | deferred | This release is for administrators only; roles for beekeepers and wholesalers are planned for the next release. |
| FB-34 | Changing the language on the wholesaler page gives "not found" | fixed | Switching the language stays on the same page, including its parameters. |

## Public Honey Story page (QR code)
- The QR code on a batch now contains the full web address of its Honey Story page, so scanning it with a phone opens the page directly (before, it contained only an internal code).
- Only published (finalized) batches have a public page; unfinished batches are not visible.
- Privacy of the beekeepers: the page shows the beekeeper's name, but no photo or phone number. Apiary locations are shown only approximately (an area of about 10 km, no exact position), and the exact coordinates are not sent to the browser.
- Apiary photos are only available for apiaries that supplied a published batch.

## Other improvements found during the work
- Numbers with decimals are read the same way in every UI language (English, Turkish, Spanish, Polish, Latvian). Before, "1.5" could be stored as 15 in some languages. A check of the existing data found no values affected in this way.
- Forms keep the entered values when a check fails, and every form shows its messages next to the fields.
- Success and error messages are shown consistently across the panel.
- Several pages that could end in an error page now show a message instead (e.g. saving a batch without usable lots).
- Saving CCP monitoring values and creating beekeepers are now all-or-nothing, so a failure cannot leave half-saved records.

## Security
The release includes a security review of the panel. In general terms:
- Account security: public sign-up closed; stronger protection of the login (temporary lock after repeated failed attempts); password-reset links can no longer be redirected to other sites; beekeeper logins, which are not used in this release, are locked.
- Protection against forged requests on every form and action that changes data; deleting and (de)activating records requires a confirmed form submission.
- Uploaded files are checked on the server (type, content and size).
- Passwords and keys were removed from the source code and are supplied by the server configuration; the affected credentials will be renewed at go-live.
- In production the site is served over HTTPS only: plain HTTP is redirected and browsers are told to use HTTPS (HSTS). The certificate is managed by the university IT.

## Questions for the partners
- **PQ-01 (FB-16, FB-17):** A raw honey production is unique per apiary + production period + honey type. If two wholesalers buy honey from the same beekeeper for the same apiary, period and honey type, how should this be recorded? Should a production period be restricted to certain honey types?
- **PQ-02, CCP measurement data:** in the current data, most CCP parameters hold the same value ten times, and the values of five parameters look shifted by one position (for example a honey moisture of 5.9 % and a proline value of 3.75 mg/kg). Please check whether these records are test data or were entered under the wrong parameter, and whether they should be corrected before go-live.
- **Translations:** the new messages in Spanish, Polish and Latvian were translated by us; please review them.
