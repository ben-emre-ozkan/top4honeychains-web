# 1. About this test {#about}

## What TOP4HoneyChains is {#what}

TOP4HoneyChains is a web platform that follows honey from the beekeeper's apiary to the shop shelf and keeps a verifiable record of every step. It is built in the TOP4HoneyChains project ("Trustable and Sustainable Open Platform for Smart Honey Value Chains"), funded under the ICT-AGRI-FOOD 2022 call.

Why it matters:

- **Trust you can check.** Every record is entered by one person and approved by another (four-eyes principle). Every change goes into an audit log that cannot be edited.
- **The whole chain in one place.** The beekeeper's registration, the delivery in sealed containers, a sample from every container, the laboratory analysis, the control points (CCPs) at the plant, the sealed batch, the shipment to the shop.
- **Fraud is harder.** Seals, tamper-evident sample caps and SMS codes bind each record to the people who confirmed it. A broken seal or a reopened jar shows.
- **A story for the consumer.** The QR code on the jar opens the Honey Story: where the honey comes from, the approved lab results with their limits, and the control points.
- **Fair to the beekeeper.** The beekeeper sees their deliveries, results and batches, sets an asking price and decides what is shown about them.

This guide lets your team try all of it in a demo environment: one honey, from the registration of its beekeeper to the consumer who scans the QR code. It takes about two to three hours with several people; one person can also play every role, one after the other.

## How the demo works {#demo}

- **Demo site:** {{demo.url}} (your coordinator confirms the address). A dark strip at the bottom of every page says **Demo environment – no real SMS**. Everything you see is test data marked **[DEMO]**.
- **Accounts:** one account per role and organization of your country (chapter 3). The passwords are in the separate sheet credentials.txt; keep it private.
- **Language:** choose the language with the flag at the top right (on the sign-in page: TR EN ES PL LV).
- **SMS:** the demo sends no SMS. Codes and sign-in details appear in **Demo SMS inbox** (link in the bottom strip, it opens in a new tab). You see the messages of your active organization and the ones you sent.
- **First sign-in:** the platform asks you to read and confirm the privacy notice. Roles that sign for their organization (organization admin, registration officer, Quality Assurance Officer, lab manager, Compliance Officer) confirm with an SMS code: send it, read it in the demo SMS inbox, enter it. A short tour follows; **Skip the tour** or follow it.
- **Shared accounts:** other testers of your country use the same accounts at the same time. Give your records a recognizable name (your initials), and do not change other testers' records.
- **Forgotten password:** the demo sends no SMS, so the password reset does not work. Ask your coordinator.
- **Reset:** the platform team can put the demo back to its starting state. Everything entered after the last reset is then deleted.
- **Example honey:** your country already has one finished example: {{example.batch}} from {{b1.name}}. Open its Honey Story at {{example.story}}.

## Limits of the demo for partner countries {#limits}

The platform is built for Türkiye first. For Argentina, Poland and Latvia the demo uses the smallest safe workarounds; please read them before you judge a step:

- **Registry number.** The platform checks Türkiye's enterprise registry number (TR, province code, 10 digits). In the demo a partner country uses a test number **AR-DEMO-**, **PL-DEMO-** or **LV-DEMO-** and 6 digits. It is not checked against a national register.
- **Provinces and districts.** The lists hold Türkiye's 81 provinces. The demo adds two regions per partner country, marked (AR), (PL) or (LV), with a few districts. They are examples, not official lists.
- **Origin map.** The Honey Story, the beekeeper page and the dashboard draw origins on a map of Türkiye only. For other countries the page shows the country and the region as text ("not on the map"). A map per partner country is a planned extension (A8).
- **Business numbers.** Beekeepers registered in the demo get numbers like AR-BEE-DEMO-000001.
- **Texts and limits.** The privacy notice and consent texts are drafts written for Türkiye (marked **DEMO TEXT – not approved**). The lab parameters and their limits follow the Turkish honey rules; your country may use other limits. Tell us in the feedback.
- **Currency.** The asking price can be entered in ARS, PLN, EUR, USD or TRY.

# 2. The roles {#roles}

In TOP4HoneyChains a person never acts alone: every action is done for an organization, in a role. One page per role follows: who it is, what they do, which screens they use and which demo account plays it. The user manual explains each role in detail (chapter given on each card).

{{ROLECARDS}}

# 3. Your demo accounts {#accounts}

{{COORDINATORNOTE}}

{{ACCOUNTS}}

# 4. Follow one honey {#scenario}

The scenario follows one honey through the platform. Each step names the role, the demo account, what to do and what you should see. Work in this order: many steps wait for the previous one. Suggested values for the control points and the lab report are in [Table](#tbl-values).

Two rules you will meet everywhere: **a record is entered by one person and approved by another**, and **a lot or batch moves on only when everything before it is approved**. If a button is disabled, the page tells you what is missing.

{{SCENARIO}}

# 5. Feedback {#feedback}

Please give feedback per step and answer the general questions. You can write on this page, use the interactive checklist (checklist HTML file in this pack, it can export your answers) or fill the feedback template (CSV). Send the result to your coordinator.

{{FEEDBACK}}

# Appendix: suggested values {#values}

These values are within the platform's limits. Try one value outside its limit to see how the platform warns and asks for a confirmation.

{{VALUES}}
