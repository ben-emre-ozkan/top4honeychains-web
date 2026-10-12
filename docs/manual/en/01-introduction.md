# 1. About TOP4HoneyChains {#about}

TOP4HoneyChains is a web platform that follows honey from the beekeeper's apiary to the retailer's shelf and keeps a verifiable record of every step. It was developed in the TOP4HoneyChains project ("Trustable and Sustainable Open Platform for Smart Honey Value Chains"), which is funded under the ICT-AGRI-FOOD 2022 call by the national funding agencies of the partner countries.

This manual describes the platform as it runs today, with the organization model switched on. It is written for everyone who signs in to the panel and for the platform administrators. The public pages, which need no account, are described where they matter.

## How to read this manual {#how-to-read}

- **Chapters 1–4** explain the platform, capabilities, organizations and roles, signing in, and the full journey of a batch of honey. Read them first.
- **Chapters 5–11** have one chapter per role. Each starts with "your daily work" and then explains every task step by step. Read the chapter of your own role.
- **Chapter 12** is for platform administrators.
- **Chapter 13** explains the audit log and what the public can read.
- **Chapter 14** has a glossary and answers to common problems.

Button, menu and field names are written in **bold**, exactly as they appear on screen. Messages that the system shows are written in quotation marks. Figures and tables are numbered; the lists of figures and tables follow the contents. The examples use test records whose names start with **[DEMO]**; they are not real people or companies.

> **Labels that change in the next release.** A few labels on screen will be renamed in the next release of the platform. This manual already uses the new names, for example the role **Quality Assurance Officer** (today: "Quality approver") and capability descriptions that start with "Able to …". The screenshots show today's labels. [Table](#tbl-renamed-labels) in the glossary lists every old and new name.

> The platform uses the international term **CCP** (critical control point) in English. The other language versions use their own abbreviation, as on screen: KKN (Turkish), PCC (Spanish), KPK (Polish), KKP (Latvian).

## The honey value chain {#value-chain}

Honey passes through five stations. The platform records each one:

1. **Apiary**: the beekeeper produces the honey. Beekeepers do not sign in and have no accounts; each beekeeper is represented in the platform by a **Beekeeper** record, which the platform administrators and the beekeeper's association keep up to date.
2. **Delivery**: the beekeeper delivers the honey to a processing plant. The plant records the delivery, which creates a *raw honey lot*, and checks it at reception (CCP 0).
3. **Laboratory**: the plant seals a sample and sends it to a food-analysis laboratory (typically an accredited one). At the laboratory, an analyst enters the results of the analysis and a second person, the lab manager, approves the report.
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
| Beekeepers | none; represented by a **Beekeeper** record | Deliver honey; their association records their consent for a public beekeeper page |
| Beekeepers' associations | members of an organization | Link their member beekeepers, record their consent for a public beekeeper page |
| Processing and packing plants | members of an organization | Record deliveries, take samples, record the control points, request and confirm finalizing, ship |
| Food-analysis laboratories | members of an organization | Receive samples, enter results, approve reports |
| Wholesalers | members of an organization | Confirm shipments, record storage conditions, read the producer's records |
| Retailers | members of an organization | Confirm shipments, record storage conditions, read the producer's records |
| Public authorities | members of an organization | Read records (read-only) |
| Platform administrators | global role on a user account | Verify organizations, manage memberships, define control points and limits, create batches, read the audit log |
| Consumers and other public visitors | none | Read the public pages: the Honey Story from the QR code on the jar, beekeeper pages, report verification |

## The public Honey Story and QR codes {#honey-story}

When a batch is finalized, the platform creates its public **Honey Story** page and a QR code that points to it. The QR code can be printed on the label. Any public visitor can scan it without an account.

The Honey Story shows, in the language the visitor chooses on the page:

- the batch name, amount and production date;
- the journey of the honey: harvest, delivery, laboratory, processing and shelf;
- the origin of the honey on a map: currently by province of Türkiye; maps of the other partner countries are planned;
- the beekeepers who contributed, with a link to their public beekeeper page when they have agreed to one;
- the laboratory results, with a within/out-of-limit verdict only for limits that have been verified;
- the control points of processing and packing;
- a link to verify the laboratory report.

The Honey Story never shows a beekeeper's phone number, a photo of a person, the exact location of an apiary or any free-text note that could contain personal data. See [What the public can read](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: The public Honey Story of a finalized batch on a phone.

Other public pages:

- **Beekeeper page** (`/storefront/<code>`): a beekeeper's public page, published only with the beekeeper's signed consent and a verified registry number.
- **Verify a lab report** (`/verify-report`): anyone can enter a report code and read whether the report exists and was approved, and compare a PDF with its fingerprint.
- **Landing page** (`/`): the project's home page, with the button to sign in.
