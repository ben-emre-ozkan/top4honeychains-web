# Interface issues found while writing the English manual

Found on 2026-11-21 in the manual's own run of the platform (fictional database, all four feature switches on). Nothing was changed in the application; each item is a proposal for a later work item. The manual describes what the screen does today.

| # | Where | What happens | Effect on the manual | Proposal |
| --- | --- | --- | --- | --- |
| 1 | Record a delivery, table **Containers and samples** | The table needs about 1470 px. At 1280 px (and at 1366 px) it scrolls sideways; the columns **Type** or **New seal after sampling** are out of view. | The table is shown in two pictures (left and right part). | Wrap a row onto two lines, or let the seal columns stack under the weights. |
| 2 | Beekeeper portal header on a phone | The language switch added in D-92 shows five underlined links stacked on top of each other next to the logo. | Visible in every portal picture. | Use the same compact language menu as the panel, or a single select. |
| 3 | Consumer **Send feedback to the producer** on a phone | The radio buttons of **What is it about?** sit far from their labels, at the left edge. | Visible in the feedback picture. | Align the radio button with its label. |
| 4 | Analyst's **My work** and the administrator's **Dashboard** | **Lab results to enter** keeps the single jars of lots whose composite report is approved (4 items in the sample data; the dashboard shows "Lab: Waiting: 4"). Known since D-91. | The analyst's My work picture shows the extra group. The text does not mention it. | Leave out jars that are part of an approved composite. |
| 5 | Sample acceptance officer's **My work** after a delivery | **CCP 1** is listed under **Values to enter**, although the laboratory's approved report fills it in. | The manual says: CCP 1 is filled in by the laboratory, you do not enter it. | List CCP 1 only for the laboratory, or not at all until the report exists. |
| 6 | Lab report, limit of **Electrical Conductivity** | The limit reads "≤ 0.8 mS/cm; ≥ 0.8 mS/cm": the limits of blossom and honeydew/chestnut honey are shown together without saying which applies. | Visible in the lab report picture. | Show only the limit of the lot's honey type, or name the honey types. |
| 7 | Beekeeper portal **Privacy and consents** | **Privacy notices you confirmed** lists the same notice version twice. | Not visible in the pictures (below the fold). | Show each version once. |
| 8 | Phone numbers in **Account** (portal, consumer) and **Profile** | Numbers are shown as `+905550000802`, without spaces, while forms show `+90 555 000 08 02`. | Visible in the account pictures. | Format the number for display. |
| 9 | **Set-up > Organizations** | The association is tagged **Wholesaler** next to its name (it was created from an older wholesaler record). The word does not match any capability. | Visible in the organisations picture; not explained in the text. | Show the tag only to the platform administrator with a tooltip, or say "former wholesaler record". |
| 10 | English interface | English uses the US flag and US date format in long dates ("SATURDAY, NOVEMBER 21, 2026"), while short dates are day.month.year. The partners write British English. | Visible on the dashboard. | Use a neutral or UK flag and one date format. |
| 11 | Role chip in the top bar | Long organisation names are cut ("Şile Arıcılar Bi…"). | Visible in most pictures; the full name is in the role list. | Acceptable; or show the full name on hover. |
| 12 | Privacy notices and consent texts | All notices carry the badge **DRAFT** ("for review by the university's data-protection office"). | The badge is visible in the notice pictures. | Expected until the texts are approved; the pictures need retaking after publishing. |

Not issues, but noted:
- The browser tab title of the panel pages is "… | Management Information System", a name used nowhere else.
- With the test SMS provider and `Sms:DemoPanel` on (local demos), the registration and delivery forms show the SMS text with its code on the page. The manual's run switches it off, as on the live server.
