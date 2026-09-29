# Product decisions

Decisions by the project owner, recorded 2026-09-29. They apply to the current release (the go-live described in `docs/GO-LIVE.md`) unless stated otherwise.
Findings in `docs/FEEDBACK.md` refer to these by ID.

| ID | Decision | Consequence for this release | Findings |
|---|---|---|---|
| D-01 | **Roles:** in this release only administrators use the web panel. Beekeeper and wholesaler logins are deferred to a later release. | No role-based feature work. The existing `[Authorize(Roles="Administrator")]` model stays. A non-admin who logs in gets a clear message instead of an AccessDenied loop. | FB-33 (deferred), FB-02 |
| D-02 | **Public self-registration is closed.** Only admins create users; `UserManagement/Create` stays. | Remove the Register page, the POST action and the "Sign up" links. | FB-02 |
| D-03 | **Beekeeper e-mails:** beekeepers do not log in, so placeholder e-mails (`{businessNo}@top4honeychains.com`) are acceptable for now. Password reset must work for admin accounts with real e-mails. | Fix and verify the forgot/reset flow for real addresses; no change to beekeeper e-mails. | FB-01 |
| D-04 | **Beekeepers and apiaries are deactivated, never deleted** (traceability). | No delete function. The UI must say clearly that deactivation replaces deletion and what it does. | FB-08, FB-14, FB-07, FB-13 |
| D-05 | **Final-production rule stays:** a raw lot is eligible only when all its CCP checks are completed. | The UI must show which lots are not eligible and which CCP checks are missing. | FB-20 |
| D-06 | **Raw honey rules unchanged:** no change and no DB change in this release. | The partner's question goes to `docs/PARTNER-QUESTIONS.md` (PQ-01). | FB-16, FB-17 (deferred) |
| D-07 | **Deferred features:** map-based location picking and CCP images are postponed to a later release. | Not implemented now. | FB-09, FB-27 (deferred) |
| D-08 | **Lookup fields stay but are optional:** education level, beekeeping purpose type and beekeeping type. | Make them clearly optional in the beekeeper forms. No DB change: the FK columns are already nullable. | FB-06 |
| D-09 | **Non-admin accounts are locked out** (follows from D-01). Beekeeper logins used the business number as the password, which is guessable. | (a) New beekeeper accounts get a random, never-shown password. (b) `db/migrations/001_lock_out_non_admin_accounts.sql` locks out every account outside the Administrator role at go-live (Identity lockout, security stamp rotated). *Provisional (conservative choice, owner to confirm):* new beekeeper accounts are also locked at creation, and **Activate** no longer unlocks a beekeeper's login; Deactivate still locks it. | ENG-06 |
| D-10 | *Provisional (conservative choice, owner to confirm):* **upload limits.** Images (beekeeper profile photo, apiary photos): JPG, PNG or WEBP, at most 8 MB and 10000 px per side. CCP content videos: MP4 or WEBM, at most 25 MB. | Enforced server-side (extension plus file signature). 25 MB stays below the default request limits of Kestrel (about 28.6 MB) and IIS (30,000,000 bytes), so no server change is needed. Larger videos would need a higher limit here and in IIS (`requestLimits maxAllowedContentLength`). | FB-26, ENG-10 |

Decisions marked *provisional* were taken during implementation as the most conservative option and still need the owner's confirmation.
