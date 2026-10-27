# 4. Pilnā plūsma no sākuma līdz beigām {#flow}

Šajā nodaļā aprakstīts vienas medus gala partijas ceļš platformā: kas veic katru soli un kam jābūt pabeigtam, pirms var sākt nākamo soli ([Figure](#fig-flow-overview)). Lomu nodaļās (5.–14.) katrs ekrāns aprakstīts sīkāk.

<!-- SHOT: flow-overview -->
Figure: Pabeigtas (fiksētas) gala partijas lapa **Procesa plūsma**: katrs posms ar tā statusu un atbildīgo organizāciju.

## Kontroles punkti (KKP) {#ccp-list}

**Kritiskais kontroles punkts (KKP)** ir solis, kurā kaut kas tiek mērīts vai pārbaudīts. Katram KKP ir parametri (piemēram, temperatūra vai mitrums), robežvērtības un statuss. KKP 0–3 attiecas uz **neapstrādāta medus partiju**, KKP 4–10 – uz **gala partiju** ([Table](#tbl-ccps)).

Table: Kritiskie kontroles punkti (KKP) un to noklusējuma lomas {#tbl-ccps}

| KKP | Attiecas uz | Kas tiek pārbaudīts | Ievada (pēc noklusējuma) | Apstiprina (pēc noklusējuma) |
|---|---|---|---|---|
| KKP 0 | Partija | Sākotnējais audits pieņemšanas brīdī, **pirms** laboratorijas testa | Uzņēmuma paraugu pieņemšanas speciālists | Uzņēmuma kvalitātes nodrošināšanas speciālists |
| KKP 1 | Partija | Paraugu ņemšana un laboratorijas rezultāti | Tiek ierakstīts no apstiprinātā laboratorijas pārskata | Laboratorijas vadītājs (vai uzņēmuma kvalitātes nodrošināšanas speciālists, skatīt tālāk) |
| KKP 2 | Partija | Temperatūras uzraudzība (karsēšana) | Uzņēmuma darbinieks | Kvalitātes nodrošināšanas speciālists |
| KKP 3 | Partija | Filtra stāvoklis | Darbinieks | Kvalitātes nodrošināšanas speciālists |
| KKP 4 | Gala partija | Viendabīgums (sajaukšana) | Gala partijas īpašnieka darbinieks | Kvalitātes nodrošināšanas speciālists |
| KKP 5 | Gala partija | Laiks un temperatūra (pasterizācija) | Darbinieks | Kvalitātes nodrošināšanas speciālists |
| KKP 6 | Gala partija | Putu un nogulšņu noņemšana | Darbinieks | Kvalitātes nodrošināšanas speciālists |
| KKP 7 | Gala partija | Daudzums (pildīšana) | Darbinieks (Iepakošana) | Kvalitātes nodrošināšanas speciālists |
| KKP 8 | Gala partija | Marķējuma precizitāte | Darbinieks (Iepakošana) | Kvalitātes nodrošināšanas speciālists |
| KKP 9 | Gala partija | Iepakojuma integritāte | Darbinieks (Iepakošana) | Kvalitātes nodrošināšanas speciālists |
| KKP 10 | Gala partija | Temperatūra un mitrums uzglabāšanas laikā | Darbinieks (Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana) | Kvalitātes nodrošināšanas speciālists |

Gala partijas dzīves ciklā KKP 10 parādās divreiz: vispirms **gala partijā, uzņēmumā** (tam jābūt apstiprinātam, pirms gala partiju var fiksēt), un vēlāk kā **uzglabāšanas ieraksts pie katra saņēmēja** pēc sūtījuma.

KKP ieraksts iziet cauri šādiem statusiem: **Piešķirts** (datu vēl nav) → **Procesā** (daļa datu) → **Iesniegts** (ievadīti visi obligātie dati; gaida apstiprinājumu) → **Pabeigts** ar apstiprinājumu. KKP, kas nav piemērojams, var atzīmēt kā **Nav piemērojams**, norādot iemeslu; arī tas ir jāapstiprina. Mainot jebkurus datus, esošais apstiprinājums tiek noņemts.

## Soli pa solim {#flow-steps}

[Table](#tbl-flow-steps) uzskaita katru soli, kas to veic un ko tas bloķē.

Table: Pilnā plūsma soli pa solim {#tbl-flow-steps}

| # | Solis | Kas | Kur | Bloķē nākamo soli, līdz… |
|---|---|---|---|---|
| 1 | Tiek reģistrēts biškopis un pirmā drava: reģistrācijas numurs, identitāte, paziņojums un piekrišanas, ko apstiprina ar SMS kodu (vai parakstītu foto) | Biškopju reģistrācijas speciālists (novada pārvalde) vai platformas administrators | **Biškopju reģistrācija** | Ar parakstītu foto apstiprinātu biškopi pieņemšanā var atrast tikai pēc tam, kad atbilstības speciālists foto pieņēmis |
| 2 | Tiek pārbaudīts reģistrācijas numurs, piesaistīta biedrība | Platformas administrators | **Biškopju reģistrs un biedrības** | (Vajadzīgs publiskajai biškopja lapai, nevis piegādēm) |
| 3 | Tiek reģistrēta piegāde: katra tvertne ar tās svaru un plombu, no katras tvertnes paraugu burka un kontrolparauga burka; biškopis apstiprina ar SMS kodu; tiek izveidota neapstrādāta medus partija | Paraugu pieņemšanas speciālists (uzņēmums) | **Piegādes (pieņemšana)** → **Reģistrēt piegādi** | – (KKP 0–3 partijai tiek piešķirti automātiski; burkas tiek nosūtītas izvēlētajai laboratorijai) |
| 4 | Etiķetes tiek izdrukātas un uzlīmētas uz tvertnēm un burkām | Paraugu pieņemšanas speciālists | Piegādes lapa → **Etiķetes** | – |
| 5 | Tiek ievadīts KKP 0 | Paraugu pieņemšanas speciālists | **Mans darbs** → KKP 0 | Tas ir iesniegts |
| 6 | Tiek apstiprināts KKP 0 | Kvalitātes nodrošināšanas speciālists (cita persona) | **Mans darbs** → KKP 0 | **Laboratorijas rezultātus var ievadīt tikai pēc KKP 0 apstiprināšanas** |
| 7 | Tiek saņemta katra burka: pārbaudīts plombas numurs, etiķete un vāciņš | Analītiķis vai laboratorijas vadītājs | **Mans darbs** → **Saņemamie paraugi** | Ja kāda pārbaude neizdodas, burka tiek noraidīta; uzņēmums paņem jaunu burku no tās pašas tvertnes |
| 8 | No saņemtajām burkām tiek sagatavots kopparaugs (kopparauga analīze) | Analītiķis vai laboratorijas vadītājs | **Paraugi un laboratorija** → **Izveidot kopparaugu** | Katras tvertnes burkai jābūt saņemtai kārtībā |
| 9 | Tiek ievadīti laboratorijas rezultāti, augšupielādēts PDF, iesniegts pārskats | Analītiķis | **Mans darbs** → **Ievadāmie laboratorijas rezultāti** | Ir visas obligātās vērtības, PDF, pārskata numurs un datums |
| 10 | Laboratorijas pārskats tiek apstiprināts (vai atgriezts) | Laboratorijas vadītājs (cita persona) | **Mans darbs** → **Apstiprināmie laboratorijas pārskati** | Apstiprināšana ieraksta rezultātus partijas KKP 1 |
| 11 | Tiek ievadīti un apstiprināti KKP 2–3 | Darbinieks, pēc tam kvalitātes nodrošināšanas speciālists | **Mans darbs** | Partiju var iekļaut gala partijā tikai tad, ja **visi** tās KKP ir pabeigti vai nav piemērojami **un ir apstiprināti** |
| 12 | Tiek izveidota gala partija, pievienotas partijas, noteikts īpašnieks, piešķirti KKP 4–10 | **Platformas administrators** (pagaidām) | **Gala Medus Ražošanas** | Gala partijai ir partijas un tās KKP |
| 13 | Tiek ievadīti un apstiprināti KKP 4–10, tostarp KKP 10 uzglabāšana uzņēmumā | Darbinieks, pēc tam kvalitātes nodrošināšanas speciālists | **Mans darbs** | Katram gala partijas KKP jābūt apstiprinātam |
| 14 | Tiek pieprasīta pabeigšana (fiksēšana) | Īpašnieka darbinieks vai organizācijas administrators | **Mana organizācija** → gala partija → **Pieprasīt pabeigšanu (fiksēšanu)** | Kamēr kaut kā trūkst, poga ir atspējota (arī tad, ja piegādes foto vēl nav pieņēmis atbilstības speciālists) |
| 15 | Tiek apstiprināta pabeigšana: gala partija tiek fiksēta | Kvalitātes nodrošināšanas speciālists (cita persona) | **Mans darbs** → **Pabeigšanas (fiksēšanas) pieprasījumi** | – Medus stāsts un QR kods kļūst publiski pieejami; gala partija un tās partijas tiek bloķētas; biškopja cena tiek bloķēta |
| 16 | Tiek izveidots sūtījums | Turētāja darbinieks vai organizācijas administrators | **Sūtījumi un krājumi** → **Nosūtīt** | Saņēmējs apstiprina |
| 17 | Tiek apstiprināta (vai noraidīta) saņemšana | Saņēmēja darbinieks vai organizācijas administrators | **Mans darbs** → **Apstiprināmie sūtījumi** | Turējums pāriet tikai pēc apstiprināšanas |
| 18 | Uzglabāšanas ieraksts (KKP 10) pie saņēmēja, apstiprināts | Saņēmēja darbinieks, pēc tam tā kvalitātes nodrošināšanas speciālists | **Sūtījumi un krājumi** → **Uzglabāšana** | – |
| 19 | Saņēmējs lasa ražotāja ierakstus | Saņēmējs (jebkura loma ar KKP lasīšanas tiesībām) | KKP lapas, tikai lasīšanas režīmā | – |
| 20 | Patērētājs noskenē QR kodu | Jebkurš publisks apmeklētājs (bez konta) | Medus stāsts | – |

Paralēli plūsmai biškopis biškopju portālā seko savām piegādēm, paraugiem un gala partijām un ievada savu prasīto cenu (skatīt [14. nodaļu](#portal)), bet katras organizācijas atbilstības speciālists pārskata parakstītos foto, noraidītos paraugus un bojātās plombas (skatīt [13. nodaļu](#role-co)).

### Reģistrācija ar SMS kodu vai parakstītu foto {#flow-registration}

Biškopi reģistrē vienreiz – to dara novada lauksaimniecības pārvaldes biškopju reģistrācijas speciālists (vai platformas administrators). Speciālists pārbauda saimniecības reģistrācijas numuru (`TR`, provinces numurzīmes kods un 10 cipari) atbilstoši pārvaldes provincei, reģistrē biškopi un pirmo dravu, ļauj biškopim izlasīt informatīvo paziņojumu un reģistrē biškopja izvēles: neobligāto piekrišanu vārda rādīšanai publiskajās lapās un neobligāto piekrišanu SMS paziņojumiem. Biškopis to visu apstiprina ar **6 ciparu kodu**, kas pienāk SMS un ko viņš nolasa; kods ir piesaistīts tieši šim saturam.

Biškopis bez mobilā tālruņa tā vietā paraksta izdrukāto veidlapu; speciālists augšupielādē tās foto ar iemeslu. Šāda reģistrācija gaida pārvaldes **atbilstības speciālistu**; kamēr foto nav pieņemts, biškopis nevar piegādāt ("Nav aktīva biškopja ar šo numuru." pieņemšanā), un, ja foto tiek noraidīts, piekrišanas tiek atsauktas. Biškopis bez mobilā tālruņa nesaņem portāla kontu. Skatīt [12. nodaļu](#role-ro).

### Tvertnes, plombas un paraugu burkas {#flow-containers}

Medus pienāk **tvertnēs** (kannās, mucās, bučās). Pieņemšanā paraugu pieņemšanas speciālists katrai tvertnei reģistrē vienu rindu:

1. **Plomba pienākot**: noplombētai tvertnei pārbauda plombu (plombas numuru un **plomba neskarta**); tvertnei bez plombas biškopja klātbūtnē uzliek uzņēmuma plombu (**Tagad uzliktā uzņēmuma plomba**).
2. **Svars**: neto kilogrami (un pēc izvēles bruto); partijas daudzums ir tvertņu summa.
3. **Paraugu burkas**: tvertni atver un no tās piepilda **paraugu burku** un **kontrolparauga burku**, katru ar savu plombas numuru un **drošības vāciņu**. Noplombējot vāciņam jābūt neskartam (poga nospiesta); izlēkusi poga nozīmē, ka burka ir atvērta, tāpēc to izmet un piepilda jaunu burku.
4. **Jauna plomba** pēc paraugu ņemšanas aizver tvertni.

Katrs solis ir **plombas notikums**, ko var tikai papildināt, nekad mainīt. Laboratorija saņem paraugu burkas; kontrolparaugi paliek uzņēmumā laikā, kas norādīts laukā **Glabāt līdz**.

**Biškopja apstiprinājums.** Biškopis saņem SMS ar organizāciju, datumu, tvertnēm, kilogramiem, medus veidu, paraugu skaitu un kodu. Kad kods ir ievadīts, piegāde ir apstiprināta un tiek saglabāts tās **satura nospiedums**. Jebkuras izmaiņas pēc koda nosūtīšanas to padara nederīgu, un vajadzīgs jauns kods. Ja biškopis jau aizgājis, speciālists viņam piezvana, un biškopis nolasa kodu no sava tālruņa. Bez SMS (nav tālruņa, nav signāla) speciālists augšupielādē **parakstīto piegādes veidlapu** ar iemeslu: ar partiju var strādāt, bet to var pabeigt (fiksēt) tikai pēc tam, kad uzņēmuma atbilstības speciālists foto pieņēmis.

<!-- SHOT: intake-containers -->
Figure: Piegādes reģistrēšana: viena rinda katrai tvertnei ar tās plombām un paraugu burkām.

**Etiķetes.** Piegādes lapā drukā etiķetes ar QR kodiem: vienu katrai tvertnei (`L{lot}-C{n}`), vienu katrai paraugu burkai (`S-{lot}-C{n}`) un vienu katram kontrolparaugam (`…-K`), A4 lapā (2 × 5) vai pa vienai 100 × 50 mm etiķetei lapā.

### Pieņemšana laboratorijā {#flow-reception}

Katrai burkai laboratorija reģistrē trīs pārbaudes: **Plombas numurs atbilst ierakstam**, **Etiķete ir neskarta** un **Vāciņš neskarts (poga nospiesta)**. Ja kāda atbilde ir **Nē**, burka tiek noraidīta ar iemeslu. Tad uzņēmums paņem **jaunu burku no tās pašas tvertnes** (**Atvērt, paņemt paraugu un noplombēt no jauna**), atkal noņemot tvertnes plombu un uzliekot jaunu; abi notikumi redzami plombu vēsturē. Apstiprinātajā piegādes saturā paliek pirmā burka.

### Kopparaugs vai katra tvertne atsevišķi {#flow-analysis}

Platforma atbalsta divus partijas analīzes veidus; kuru lieto, nosaka platformas iestatījums:

- **Kopparaugs (noklusējums).** Kad katras tvertnes burka ir saņemta kārtībā, laboratorija no vienādām burku daļām sagatavo **vienu kopparaugu** (**Izveidot kopparaugu**, kods `S-{lot}-COMP`) un to analizē. Tā apstiprinātais pārskats aizpilda partijas KKP 1. Atsevišķās burkas tiek glabātas; ja kopparaugs neatbilst prasībām vai pēc pieprasījuma, atsevišķu burku analizē atsevišķi. Šāds vienas burkas pārskats paliek pie savas burkas un neaizstāj partijas KKP 1 vērtības.
- **Katra tvertne atsevišķi.** Katru burku analizē atsevišķi, un katrs apstiprinātais burkas pārskats attiecas uz partiju.

### Sliktākās vērtības noteikums {#flow-worst}

Ja burkas analizē pa vienai, katra parametra partijas KKP 1 vērtība ir **sliktākā** tvertnes vērtība:

- vispirms vērtība ārpus robežvērtībām – tā, kas ir vistālāk ārpus tām;
- citādi vērtība, kas vistuvāk robežvērtībai būtiskajā virzienā (augstākā, ja ir maksimums, zemākā, ja ir minimums);
- ja robežvērtības nav – augstākā vērtība;
- teksta atbildes tiek saglabātas visas, katra ar savu tvertni.

Laboratorijas pārskatā un atsaukšanas pārskatā redzamas katras tvertnes vērtības ("Laboratorijas vērtības pa tvertnēm"), partijas vērtība – treknrakstā.

### Piegādes faktu labojumi {#flow-corrections}

Kad piegāde ir apstiprināta, tās fakti (datums, tvertņu veids un svari, plombu numuri) ir bloķēti, arī administratoriem. Kļūdu labo **divi platformas administratori**: viens pieprasa labojumu ar iemeslu, otrs to apstiprina (pieprasītājs nekad nelemj). Sākotnējais un labotais saturs paliek lasāms piegādes lapā ar to nospiedumiem, un biškopis saņem informatīvu SMS. Ar labojumu tvertnes nekad netiek pievienotas vai noņemtas.

### Kāpēc KKP 0 ir pirms laboratorijas testa {#why-ccp0-first}

KKP 0 ir sākotnējais audits pieņemšanas brīdī: vai šis medus vispār ir pieņemams? Tāpēc laboratorija nevar saglabāt, iesniegt vai apstiprināt rezultātus, kamēr partijas KKP 0 nav pabeigts **un apstiprināts**. Līdz tam laboratorijas uzdevumā redzams paziņojums "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; laboratorijas rezultātus ievada un apstiprina tikai pēc tā." Gaidīšanas laikā pārskatu tomēr var atgriezt.

### Kāpēc gala partijas KKP 10 ir vajadzīgs pirms fiksēšanas {#why-ccp10}

Pabeigšana (fiksēšana) fiksē gala partiju tikai tad, ja katrs tai piešķirtais KKP ir apstiprināts. Gala partijas KKP 10 reģistrē uzglabāšanas apstākļus pašā uzņēmumā, tāpēc uzņēmuma uzglabāšanas ierakstam jābūt pirms fiksēšanas. Katrs vēlākais turētājs savam krājumam uztur savu KKP 10 uzglabāšanas ierakstu.

### Gala partijas pagaidām izveido tikai administrators {#batch-admin-only}

Gala partijas izveidi, tās partiju un daudzumu izvēli, īpašnieces organizācijas noteikšanu un gala partijas KKP piešķiršanu veic platformas administrators. Pēc tam uzņēmuma darbinieki ievada gala partijas KKP. To, vai uzņēmuma darbinieki drīkstēs paši izveidot gala partijas, izlems projekts.

### Piegādes, kas reģistrētas pirms tvertnēm {#flow-legacy}

Piegādes, kas reģistrētas pirms tvertņu procedūras ieviešanas (vai kamēr uzņēmums vēl strādā bez tās), ir atzīmētas kā **agrāka (tvertnes nav reģistrētas)**: tām ir viena parakstīta piegādes veidlapa un viens paraugs ar vienu plombas pārbaudi. Tās paliek derīgas un tiek pabeigtas (fiksētas) tāpat kā līdz šim.

## Kas ko bloķē: kopsavilkums {#flow-blocks}

- **Nav piegādes bez** pārbaudītas pārstrādes organizācijas, lomas ar pieņemšanas tiesībām un reģistrēta biškopja (reģistrācija ar foto – tikai pēc tam, kad atbilstības speciālists to pieņēmis).
- **Nav apstiprinātas piegādes bez** biškopja SMS koda tieši šim saturam vai parakstīta foto ar iemeslu.
- **Nav laboratorijas rezultātu bez** saņemtas burkas, kuras plombas, etiķetes un vāciņa pārbaudes ir izturētas, un apstiprināta KKP 0.
- **Pārskatu nevar apstiprināt** persona, kas ievadīja rezultātus.
- **Partiju nevar iekļaut gala partijā**, kamēr visi tās KKP nav pabeigti vai atzīmēti kā nepiemērojami un apstiprināti, un partijai jāpieder gala partijas vairumtirgotājam.
- **Pabeigšana (fiksēšana) nav iespējama**, ja gala partijā nav medus, kāds gala partijas KKP trūkst vai nav apstiprināts kāds partijas KKP ir atvērts vai piegādes foto gaida atbilstības speciālistu; sarakstā "Trūkst pirms pabeigšanas (fiksēšanas):" nosaukta katra problēma.
- **Pabeigšanu nevar apstiprināt** persona, kas to pieprasīja.
- **Nav sūtījuma**, ja gala partija nav pabeigta (fiksēta), un nevar nosūtīt vairāk par jūsu turējumā esošo daudzumu.
- **Nav izmaiņu** pabeigtā (fiksētā) gala partijā vai tās partijās vai apstiprinātas piegādes faktos, izņemot divu administratoru veiktu labojumu.
