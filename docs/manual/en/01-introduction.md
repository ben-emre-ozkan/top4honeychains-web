# 1. About TOP4HoneyChains {#about}

TOP4HoneyChains is a web platform that follows honey from the beekeeper's apiary to the shop shelf and keeps a verifiable record of every step. It was built in the TÜBİTAK TOP4HoneyChains project ("Trustable and Sustainable Open Platform for Smart Honey Value Chains").

This manual describes the platform as it runs today, with the organization model switched on. It is written for everyone who signs in to the panel and for the platform administrators. The public pages, which need no account, are described where they matter.

## How to read this manual {#how-to-read}

- **Chapters 1–4** explain the platform, signing in, organizations and roles, and the full journey of a batch of honey. Read them first.
- **Chapters 5–11** have one chapter per role. Each starts with "your daily work" and then explains every task step by step. Read the chapter of your own role.
- **Chapter 12** is for platform administrators.
- **Chapter 13** explains the audit log and what the public sees.
- **Chapter 14** has a glossary and answers to common problems.

Button, menu and field names are written in **bold**, exactly as they appear on screen. Messages that the system shows are written in quotation marks. The examples use test records whose names start with **[DEMO]**; they are not real people or companies.

> The platform uses the international term **CCP** (critical control point) in English. The other language versions use their own abbreviation, as on screen: KKN (Turkish), PCC (Spanish), KPK (Polish), KKP (Latvian).

## The honey value chain {#value-chain}

Honey passes through five stations. The platform records each one:

1. **Apiary**: the beekeeper produces the honey. Beekeepers do not sign in; they are records kept by the platform and their association.
2. **Delivery**: the beekeeper delivers the honey to a processing plant. The plant records the delivery, which creates a *raw honey lot*, and checks it at reception (CCP 0).
3. **Laboratory**: the plant seals a sample and sends it to a laboratory. The laboratory enters the results of its analysis, and a second person at the laboratory approves the report.
4. **Processing**: the plant processes the honey (CCP 2–3 on the lot), combines lots into a *batch* and records the processing, filling, labelling, packing and storage checks (CCP 4–10 on the batch). Two people then *finalize* (seal) the batch.
5. **Shelf**: the batch is shipped to a wholesaler or a shop. The receiver confirms the receipt and records its own storage conditions.

Every value is entered by one person and approved by another (the *four-eyes principle*). Every change is written to a tamper-evident audit log.

<!-- SHOT: intro-chain -->

## Traceability {#traceability}

Traceability means that for any jar of honey you can answer three questions:

- **Where does it come from?** The batch knows its raw honey lots, and each lot knows its apiary, beekeeper, delivery and laboratory report.
- **Was it checked?** Every critical control point of the lot and of the batch has recorded values, who entered them, who approved them and when.
- **Where did it go?** Shipments record which organization holds which quantity of the batch.

If there is a problem, the **Recall report** shows every lot upstream and every shipment downstream of a batch or a lot (see [Recall](#admin-recall)).

## Who the platform is for {#audience}

| Who | What they do in the platform |
|---|---|
| Beekeepers' associations | Link their member beekeepers, record their consent for a public beekeeper page |
| Processing and packing plants | Record deliveries, take samples, record the control points, request and confirm finalizing, ship |
| Laboratories | Receive samples, enter results, approve reports |
| Wholesalers and shops | Confirm shipments, record storage conditions, read the producer's records |
| Public authorities | Read records (read-only) |
| Platform administrators | Verify organizations, manage members, define control points and limits, create batches, keep the audit log |
| Consumers | Open the public Honey Story from the QR code on the jar |

## The public Honey Story and QR codes {#honey-story}

When a batch is finalized, the platform creates its public **Honey Story** page and a QR code that points to it. The QR code can be printed on the label. Anyone can scan it without an account.

The Honey Story shows, in the visitor's language:

- the batch name, amount and production date;
- the journey of the honey: harvest, delivery, laboratory, processing and shelf;
- the origin of the honey by province, on a map of Türkiye;
- the beekeepers who contributed, with a link to their public beekeeper page when they have agreed to one;
- the laboratory results, with a within/out-of-limit verdict only for limits that have been verified;
- the control points of processing and packing;
- a link to verify the laboratory report.

The Honey Story never shows a beekeeper's phone number, a photo of a person, the exact location of an apiary or any free-text note that could contain personal data. See [What the public sees](#trust-public).

<!-- SHOT: public-honey-story -->

Other public pages:

- **Beekeeper page** (`/storefront/<code>`): a beekeeper's public page, published only with the beekeeper's signed consent and a verified registry number.
- **Verify a lab report** (`/verify-report`): anyone can enter a report code and see whether the report exists and was approved, and compare a PDF with its fingerprint.
- **Landing page** (`/`): the project's home page, with the button to sign in.
