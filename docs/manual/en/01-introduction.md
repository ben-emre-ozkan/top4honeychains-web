# 1. About TOP4HoneyChains {#about}

TOP4HoneyChains is a web platform that follows honey from the beekeeper's apiary to the retailer's shelf and keeps a verifiable record of every step. It was developed in the TOP4HoneyChains project ("Trustable and Sustainable Open Platform for Smart Honey Value Chains"), which is funded under the ICT-AGRI-FOOD 2022 call by the national funding agencies of the partner countries.

This manual describes the platform as it runs today, with the organization model switched on. It is written for everyone who signs in to the panel and for the platform administrators. The public pages, which need no account, are described where they matter.

## How to read this manual {#how-to-read}

- **Chapters 1–4** explain the platform, capabilities, organizations and roles, signing in, and the full journey of a batch of honey. Read them first.
- **Chapters 5–13** have one chapter per role. Each starts with "your daily work" and then explains every task step by step. Read the chapter of your own role.
- **Chapter 14** is for beekeepers: the beekeeper portal.
- **Chapter 15** is for platform administrators.
- **Chapter 16** explains the audit log, privacy notices and consents, and what the public can read.
- **Chapter 17** has a glossary and answers to common problems.

Button, menu and field names are written in **bold**, exactly as they appear on screen. Messages that the system shows are written in quotation marks. Figures and tables are numbered; the lists of figures and tables follow the contents. The examples use test records whose names start with **[DEMO]**; they are not real people or companies.

> **Labels renamed in this release.** A few labels on screen were renamed in this release of the platform, for example the role **Quality Assurance Officer** (before: "Quality approver"), **Ground rules** and capability descriptions that start with "Able to …". [Table](#tbl-renamed-labels) in the glossary lists every old and new name.

> The platform uses the international term **CCP** (critical control point) in English. The other language versions use their own abbreviation, as on screen: KKN (Turkish), PCC (Spanish), KPK (Polish), KKP (Latvian).

## The honey value chain {#value-chain}

Honey passes through five stations. The platform records each one:

1. **Apiary**: the beekeeper produces the honey. Each beekeeper is represented in the platform by a **Beekeeper** record, registered by the district directorate of agriculture and confirmed by the beekeeper with a code received by SMS. A beekeeper can also have a personal account for the **beekeeper portal**.
2. **Delivery**: the beekeeper delivers the honey to a processing plant in containers. The plant records every container with its weight and seals, the beekeeper confirms the delivery with an SMS code, and the delivery becomes a *raw honey lot*, checked at reception (CCP 0).
3. **Laboratory**: the plant fills a sealed sample jar from every container and sends the jars to a food-analysis laboratory (typically an accredited one). The laboratory checks every jar, analyses the honey, an analyst enters the results and a second person, the lab manager, approves the report.
4. **Processing**: the plant processes the honey (CCP 2–3 on the lot), combines lots into a *batch* and records the processing, filling, labelling, packing and storage checks (CCP 4–10 on the batch). Two people then *finalize* (seal) the batch.
5. **Shelf**: the batch is shipped to a wholesaler or a retailer. The receiver confirms the receipt and records its own storage conditions.

In this manual, **data** means what people record in the platform: measured values (such as moisture in %) and answers to questions (such as Yes / No).

**The four-eyes principle.** Every piece of data is entered by one person and approved by a second, different person; nobody can approve what they entered themselves. The two "pairs of eyes" make mistakes and manipulation much less likely. The exact rules are listed under [Ground rules](#fixed-rules). Every change is also written to a tamper-evident audit log.

<!-- SHOT: intro-chain -->
Figure: The five stations of the honey value chain, as shown on the public landing page.

## Traceability {#traceability}

Traceability means that for any jar of honey you can answer three questions:

- **Where does it come from?** The batch knows its raw honey lots, and each lot knows its apiary, beekeeper, delivery and laboratory report.
- **Was it checked?** Every critical control point of the lot and of the batch has recorded data, who entered it, who approved it and when.
- **Where did it go?** Shipments record which organization holds which quantity of the batch.

If there is a problem, the **Recall report** lists every lot upstream and every shipment downstream of a batch or a lot (see [Recall](#admin-recall)).

## Who the platform is for {#audience}

[Table](#tbl-audience) lists everyone who uses the platform. Organizations and their roles are defined in [chapter 2](#roles).

Table: Who the platform is for {#tbl-audience}

| Who | Account | What they do in the platform |
|---|---|---|
| Beekeepers | represented by a **Beekeeper** record; optionally a portal account | Deliver honey and confirm deliveries with an SMS code; in the portal: follow their deliveries, samples and batches, enter their asking price, give or withdraw consents |
| District directorates of agriculture | members of an organization | Register beekeepers and their apiaries, attest their identity and consents, send their portal sign-in |
| Beekeepers' associations | members of an organization | Link their member beekeepers, record their consent for a public beekeeper page |
| Processing and packing plants | members of an organization | Record deliveries, take samples, record the control points, request and confirm finalizing, ship |
| Food-analysis laboratories | members of an organization | Receive samples, enter results, approve reports |
| Wholesalers | members of an organization | Confirm shipments, record storage conditions, read the producer's records |
| Retailers | members of an organization | Confirm shipments, record storage conditions, read the producer's records |
| Public authorities | members of an organization | Read records (read-only) |
| Compliance Officers | a role in every organization | Review evidence, sampling quality and approvals; handle data-protection requests |
| Platform administrators | global role on a user account | Verify organizations, manage memberships, define control points and limits, publish notice texts, create batches, read the audit log |
| Consumers and other public visitors | none | Read the public pages: the Honey Story from the QR code on the jar, beekeeper pages, report verification |

## The public Honey Story and QR codes {#honey-story}

When a batch is finalized, the platform creates its public **Honey Story** page and a QR code that points to it. The QR code can be printed on the label. Any public visitor can scan it without an account.

The Honey Story shows, in the language the visitor chooses on the page:

- the batch name, amount and production date;
- the journey of the honey: harvest, delivery, laboratory, processing and shelf;
- the origin of the honey on a map: currently by province of Türkiye; maps of the other partner countries are planned;
- the beekeepers who contributed: by name only when the beekeeper agreed to it, otherwise by a neutral code, with a link to their public beekeeper page when they have one;
- the laboratory results, with a within/out-of-limit verdict only for limits that have been verified;
- the control points of processing and packing;
- a link to verify the laboratory report.

The Honey Story never shows a beekeeper's phone number, a photo of a person, the exact location of an apiary or any free-text note that could contain personal data. See [What the public can read](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: The public Honey Story of a finalized batch on a phone.

Other public pages:

- **Beekeeper page** (`/storefront/<code>`): a beekeeper's public page, published only with the beekeeper's signed consent and a verified registry number.
- **Verify a lab report** (`/verify-report`): anyone can enter a report code, read whether an approved report exists, and compare a PDF with its fingerprint.
- **Privacy and consent** (`/privacy`): the current privacy notices for beekeepers and for users of the platform.
- **Landing page** (`/`): the project's home page, with the button to sign in.
