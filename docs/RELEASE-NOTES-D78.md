# TOP4HoneyChains – release notes, D-78 release

**Date:** 2026-10-28 · **Decisions:** D-78 to D-85 ([DECISIONS.md](DECISIONS.md)) · **Manual:** five languages, edition of 28 October 2026 · **Deployment:** [GO-LIVE.md](GO-LIVE.md), package `TOP4-guncelleme-2026-10-28`

This release adds beekeeper registration, deliveries in sealed containers confirmed by the beekeeper, a sample from every container, the Compliance Officer, privacy notices and consents, and the beekeeper portal. The new parts that depend on outside preparations stay **off** until each one is ready (see "Switches").

## For every user

- **Sign-in details by SMS.** New accounts receive a user name and a temporary password by SMS (valid 72 hours) and choose their own password at the first sign-in. New passwords need at least 10 characters. A forgotten password is reset with a code sent to the verified mobile phone.
- **Privacy notice.** Users read and confirm the privacy notice of the platform and of each organization they work for; roles that sign for their organization confirm with an SMS code. Until the texts are approved, a banner asks for it and nothing is blocked.
- **First-use tour and explanatory animations.** A short tour per role, and short scenes that show what a button does; both can be switched off in the avatar menu (**Show animations**).
- **Clearer texts.** The role "Quality Assurance Officer", the "Ground rules" box, capability descriptions written as abilities, role names instead of codes in messages, American spelling in English.

## New roles and pages

- **Beekeeper registration officer** (district directorate of agriculture): registers beekeepers and their apiaries, checks the registry number, records the notice and the consents, confirmed by the beekeeper's SMS code or a signed photo.
- **Compliance Officer** (every organization): reviews signed photos, rejected samples and broken seals, watches waiting and self-made approvals, records data-subject requests (30-day deadline), keeps the data controller details and agreements, reads the organization's audit entries.
- **Beekeeper portal** (when switched on): the beekeeper's deliveries with containers and seals, sample jars and approved lab results, batches with their share, Honey Story pages, the asking price per delivery, consents and the account.

## The chain of custody

- **Deliveries in containers** (when switched on): every container with its weight and seals, a sealed sample jar and counter-sample with a tamper-evident cap from every container, labels with QR codes, and the beekeeper's SMS code that binds exactly this content. Without SMS, a signed photo that the Compliance Officer accepts.
- **Laboratory reception** checks seal number, label and cap of every jar; a failed check rejects the jar and the plant takes a new one from the same container. The laboratory analyses a composite of the jars (default) or each jar; with single jars the lot takes the worst value per parameter.
- **Delivery corrections** by two platform administrators; the beekeeper is informed by SMS.
- **Recall report** with containers, seals and jars; a member organization sees only its own holding.

## Privacy and security

- Public pages name a beekeeper **only with a name consent**; otherwise a neutral code. Apiary titles typed before this release (they often contain the beekeeper's name) and photo descriptions follow the same rule.
- Evidence files (signed forms, registration photos, consent and organization documents) are downloaded only by the roles that need them, and every download is in the audit log.
- The public **Verify a lab report** page answers only for approved reports.
- Pages check the read rights of the role, not only the menu; sampling, shipping and finalizing check the organization's capability.
- With the central-association setting, only the central association verifies associations.
- **Data retention**: the periods are defined and a dry-run report counts what is past its period; nothing is deleted until the owner approves the first run.
- The image library is now SkiaSharp (the previous one needed a commercial licence key for release builds).

## Switches (all off at installation)

| Switch | Turn on when |
|---|---|
| SMS provider (`Sms:Provider`) | the SMS package is bought and the sender name approved |
| Deliveries in containers (`Features:CustodyIntake`) | the plants have tamper-evident jars, seals and labels; SMS is on |
| Notice confirmation required (`Notices:Enforce`) | the notice texts are approved by the data-protection office |
| Beekeeper portal (`Features:BeekeeperAccounts`) | the project gives the go; SMS is on |
| Retention run (`Retention:ApplyEnabled`) | the owner approves the first run and the pseudonym format |

## Database

Scripts `016`–`028`, additive and repeatable (a second run changes nothing), checked on a copy of the live database of 8 October 2026.
