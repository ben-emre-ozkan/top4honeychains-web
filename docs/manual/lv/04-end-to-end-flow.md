# 4. Pilnā plūsma no sākuma līdz beigām {#flow}

Šajā nodaļā aprakstīts vienas medus gala partijas ceļš platformā: kas veic katru soli un kam jābūt pabeigtam, pirms var sākt nākamo soli. Lomu nodaļās (5.–11.) katrs ekrāns aprakstīts sīkāk.

<!-- SHOT: flow-overview -->

## Kontroles punkti (KKP) {#ccp-list}

**Kritiskais kontroles punkts (KKP)** ir solis, kurā kaut kas tiek mērīts vai pārbaudīts. Katram KKP ir parametri (piemēram, temperatūra vai mitrums), robežvērtības un statuss. KKP 0–3 attiecas uz **neapstrādāta medus partiju**, KKP 4–10 – uz **gala partiju**.

| KKP | Attiecas uz | Kas tiek pārbaudīts | Ievada (pēc noklusējuma) | Apstiprina (pēc noklusējuma) |
|---|---|---|---|---|
| KKP 0 | Partija | Sākotnējais audits pieņemšanas brīdī, **pirms** laboratorijas testa | Uzņēmuma paraugu pieņemšanas speciālists | Uzņēmuma kvalitātes apstiprinātājs |
| KKP 1 | Partija | Paraugu ņemšana un laboratorijas rezultāti | Tiek ierakstīts no apstiprinātā laboratorijas pārskata | Laboratorijas vadītājs (vai uzņēmuma kvalitātes apstiprinātājs, skatīt tālāk) |
| KKP 2 | Partija | Temperatūras uzraudzība (karsēšana) | Uzņēmuma darbinieks | Kvalitātes apstiprinātājs |
| KKP 3 | Partija | Filtra stāvoklis | Darbinieks | Kvalitātes apstiprinātājs |
| KKP 4 | Gala partija | Viendabīgums (sajaukšana) | Gala partijas īpašnieka darbinieks | Kvalitātes apstiprinātājs |
| KKP 5 | Gala partija | Laiks un temperatūra (pasterizācija) | Darbinieks | Kvalitātes apstiprinātājs |
| KKP 6 | Gala partija | Putu un nogulšņu noņemšana | Darbinieks | Kvalitātes apstiprinātājs |
| KKP 7 | Gala partija | Daudzums (pildīšana) | Darbinieks (Iepakošana) | Kvalitātes apstiprinātājs |
| KKP 8 | Gala partija | Marķējuma precizitāte | Darbinieks (Iepakošana) | Kvalitātes apstiprinātājs |
| KKP 9 | Gala partija | Iepakojuma integritāte | Darbinieks (Iepakošana) | Kvalitātes apstiprinātājs |
| KKP 10 | Gala partija | Temperatūra un mitrums uzglabāšanas laikā | Darbinieks (Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana) | Kvalitātes apstiprinātājs |

Gala partijas dzīves ciklā KKP 10 parādās divreiz: vispirms **gala partijā, uzņēmumā** (tam jābūt apstiprinātam, pirms gala partiju var fiksēt), un vēlāk kā **uzglabāšanas ieraksts pie katra saņēmēja** pēc sūtījuma.

KKP ieraksts iziet cauri šādiem statusiem: **Piešķirts** (vērtību vēl nav) → **Procesā** (daļa vērtību) → **Iesniegts** (ievadītas visas obligātās vērtības; gaida apstiprinājumu) → **Pabeigts** ar apstiprinājumu. KKP, kas nav piemērojams, var atzīmēt kā **Nav piemērojams**, norādot iemeslu; arī tas ir jāapstiprina. Mainot vērtību, esošais apstiprinājums tiek noņemts.

## Soli pa solim {#flow-steps}

| # | Solis | Kas | Kur | Bloķē nākamo soli, līdz… |
|---|---|---|---|---|
| 1 | Tiek reģistrēts biškopis un drava | Platformas administrators | **Biškopji**, **Dravas** | Biškopis ir aktīvs, un viņam ir drava |
| 2 | Tiek pārbaudīts reģistrācijas numurs, piesaistīta biedrība | Platformas administrators | **Biškopju reģistrs un biedrības** | (Vajadzīgs publiskajai biškopja lapai, nevis piegādēm) |
| 3 | Tiek reģistrēta piegāde: izveidota neapstrādāta medus partija | Paraugu pieņemšanas speciālists (uzņēmums) | **Piegādes (pieņemšana)** → **Reģistrēt piegādi** | – (KKP 0–3 partijai tiek piešķirti automātiski) |
| 4 | Paraugs tiek noplombēts un nosūtīts uz laboratoriju | Paraugu pieņemšanas speciālists | **Paraugi un laboratorija** → **Ņemt paraugu** | Laboratorija to saņem |
| 5 | Tiek ievadīts KKP 0 | Paraugu pieņemšanas speciālists | **Mans darbs** → KKP 0 | Tas ir iesniegts |
| 6 | Tiek apstiprināts KKP 0 | Kvalitātes apstiprinātājs (cita persona) | **Mans darbs** → KKP 0 | **Laboratorijas rezultātus var ievadīt tikai pēc KKP 0 apstiprināšanas** |
| 7 | Paraugs tiek saņemts, plomba pārbaudīta | Analītiķis vai laboratorijas vadītājs | **Mans darbs** → **Saņemamie paraugi** | Ja plomba ir bojāta, paraugs tiek noraidīts; jāņem jauns paraugs |
| 8 | Tiek ievadīti laboratorijas rezultāti, augšupielādēts PDF, iesniegts pārskats | Analītiķis | **Mans darbs** → **Ievadāmie laboratorijas rezultāti** | Ir visas obligātās vērtības, PDF, pārskata numurs un datums |
| 9 | Laboratorijas pārskats tiek apstiprināts (vai atgriezts) | Laboratorijas vadītājs (cita persona) | **Mans darbs** → **Apstiprināmie laboratorijas pārskati** | Apstiprināšana ieraksta rezultātus partijas KKP 1 |
| 10 | Tiek ievadīti un apstiprināti KKP 2–3 | Darbinieks, pēc tam kvalitātes apstiprinātājs | **Mans darbs** | Partiju var iekļaut gala partijā tikai tad, ja **visi** tās KKP ir pabeigti vai nav piemērojami **un ir apstiprināti** |
| 11 | Tiek izveidota gala partija, pievienotas partijas, noteikts īpašnieks, piešķirti KKP 4–10 | **Platformas administrators** (pagaidām) | **Gala Medus Ražošanas** | Gala partijai ir partijas un tās KKP |
| 12 | Tiek ievadīti un apstiprināti KKP 4–10, tostarp KKP 10 uzglabāšana uzņēmumā | Darbinieks, pēc tam kvalitātes apstiprinātājs | **Mans darbs** | Katram gala partijas KKP jābūt apstiprinātam |
| 13 | Tiek pieprasīta pabeigšana (fiksēšana) | Īpašnieka darbinieks vai organizācijas administrators | **Mana organizācija** → gala partija → **Pieprasīt pabeigšanu (fiksēšanu)** | Kamēr kaut kā trūkst, poga ir atspējota |
| 14 | Tiek apstiprināta pabeigšana: gala partija tiek fiksēta | Kvalitātes apstiprinātājs (cita persona) | **Mans darbs** → **Pabeigšanas (fiksēšanas) pieprasījumi** | – Medus stāsts un QR kods kļūst publiski pieejami; gala partija un tās partijas tiek bloķētas |
| 15 | Tiek izveidots sūtījums | Turētāja darbinieks vai organizācijas administrators | **Sūtījumi un krājumi** → **Nosūtīt** | Saņēmējs apstiprina |
| 16 | Tiek apstiprināta (vai noraidīta) saņemšana | Saņēmēja darbinieks vai organizācijas administrators | **Mans darbs** → **Apstiprināmie sūtījumi** | Turējums pāriet tikai pēc apstiprināšanas |
| 17 | Uzglabāšanas ieraksts (KKP 10) pie saņēmēja, apstiprināts | Saņēmēja darbinieks, pēc tam tā kvalitātes apstiprinātājs | **Sūtījumi un krājumi** → **Uzglabāšana** | – |
| 18 | Saņēmējs skata ražotāja ierakstus | Saņēmējs (jebkura loma ar KKP lasīšanas tiesībām) | KKP lapas, tikai lasīšanas režīmā | – |
| 19 | Patērētājs noskenē QR kodu | Ikviens | Medus stāsts | – |

### Kāpēc KKP 0 ir pirms laboratorijas testa {#why-ccp0-first}

KKP 0 ir sākotnējais audits pieņemšanas brīdī: vai šis medus vispār ir pieņemams? Tāpēc laboratorija nevar saglabāt, iesniegt vai apstiprināt rezultātus, kamēr partijas KKP 0 nav pabeigts **un apstiprināts**. Līdz tam laboratorijas uzdevumā redzams paziņojums "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; laboratorijas rezultātus ievada un apstiprina tikai pēc tā." Gaidīšanas laikā pārskatu tomēr var atgriezt.

### Kāpēc gala partijas KKP 10 ir vajadzīgs pirms fiksēšanas {#why-ccp10}

Pabeigšana (fiksēšana) fiksē gala partiju tikai tad, ja katrs tai piešķirtais KKP ir apstiprināts. Gala partijas KKP 10 reģistrē uzglabāšanas apstākļus pašā uzņēmumā, tāpēc uzņēmuma uzglabāšanas ierakstam jābūt pirms fiksēšanas. Katrs vēlākais turētājs savam krājumam uztur savu KKP 10 uzglabāšanas ierakstu.

### Gala partijas pagaidām izveido tikai administrators {#batch-admin-only}

Gala partijas izveidi, tās partiju un daudzumu izvēli, īpašnieces organizācijas noteikšanu un gala partijas KKP piešķiršanu veic platformas administrators. Pēc tam uzņēmuma darbinieki ievada gala partijas KKP. To, vai uzņēmuma darbinieki drīkstēs paši izveidot gala partijas, izlems projekts.

## Kas ko bloķē: kopsavilkums {#flow-blocks}

- **Nav piegādes bez** pārbaudītas pārstrādes organizācijas un lomas ar pieņemšanas tiesībām.
- **Nav laboratorijas rezultātu bez** saņemta parauga ar neskartu plombu un apstiprināta KKP 0.
- **Pārskatu nevar apstiprināt** persona, kas ievadīja rezultātus.
- **Partiju nevar iekļaut gala partijā**, kamēr visi tās KKP nav pabeigti vai atzīmēti kā nepiemērojami un apstiprināti, un partijai jāpieder gala partijas vairumtirgotājam.
- **Pabeigšana (fiksēšana) nav iespējama**, ja gala partijā nav medus, kāds gala partijas KKP trūkst vai nav apstiprināts vai kāds partijas KKP ir atvērts; sarakstā "Trūkst pirms pabeigšanas (fiksēšanas):" nosaukta katra problēma.
- **Pabeigšanu nevar apstiprināt** persona, kas to pieprasīja.
- **Nav sūtījuma**, ja gala partija nav pabeigta (fiksēta), un nevar nosūtīt vairāk par jūsu turējumā esošo daudzumu.
- **Nav izmaiņu** pabeigtā (fiksētā) gala partijā vai tās partijās, izņemot divu administratoru veiktu labojumu.
