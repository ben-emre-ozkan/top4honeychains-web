# 2. Kompetences, organizācijas un lomas {#roles}

TOP4HoneyChains platformā persona nekad nerīkojas viena: katra darbība tiek veikta **organizācijas vārdā, noteiktā lomā**. To, ko jūs drīkstat darīt, nosaka trīs lietas:

1. jūsu organizācijas **kompetences** (kāda veida darbu tā veic);
2. jūsu **loma** organizācijā (kas jūs esat tās iekšienē);
3. jūsu organizācijas **statuss**: datus var ievadīt tikai organizācija ar statusu **Pārbaudīta**.

Papildus tam daži noteikumi ir iestrādāti programmas kodā, un tos nevar izslēgt (skatīt [Fiksēti noteikumi](#fixed-rules)).

## Kompetences {#capabilities}

Kompetence nosaka, kādu darbu organizācija drīkst veikt medus ķēdē. To, kurš dalībnieks darbu veic, nosaka lomas (skatīt [Lomas](#role-list)). Organizācijai ir viena vai vairākas kompetences; tās piešķir un noņem platformas administrators (skatīt [Kompetences](#admin-org-capabilities)). Pats kompetenču saraksts ir nemainīgs.

| Kompetence (ekrānā) | Nozīme (apraksts ekrānā) | Ko organizācija var darīt | KKP (noklusējums) |
|---|---|---|---|
| **Biedrība** | Biškopju biedrība; sasaista savus biedrus biškopjus un reģistrē viņu piekrišanu publiskai biškopja lapai | Sasaista savus biedrus biškopjus, reģistrē un atsauc viņu piekrišanu un publicē viņu biškopju lapas (**Biškopju lapas**); redz savu biedru biškopju partijas, piemēram, atsaukšanas pārskatos | nav; KKP 0 tikai tad, ja administrators to piešķir biedrībai |
| **Pārstrāde** | Pieņemšana un pārstrāde: piegādes, paraugi, partijas KKP 0–3 un gala partijas KKP 4–6 | Reģistrē piegādes, tādējādi izveidojot savas neapstrādāta medus partijas; pārbauda tās pieņemšanas brīdī; ņem aizzīmogotus paraugus un nosūta tos laboratorijai; reģistrē pārstrādes KKP (**Piegādes (pieņemšana)**, **Paraugi un laboratorija**) | partijas KKP 0–3, gala partijas KKP 4–6 |
| **Iepakošana** | Pildīšana, marķēšana un iepakošana, KKP 7–9 | Reģistrē savu gala partiju pildīšanu, marķēšanu un iepakošanu, kā arī KKP 10 uzglabāšanu uzņēmumā | gala partijas KKP 7–9, KKP 10 |
| **Laboratorija** | Laboratorija (akreditēta vai iekšējā) | Saņem aizzīmogotus paraugus, ievada laboratorijas rezultātus un apstiprina pārskatu; pārskatu vienmēr apstiprina otra persona. Akreditācijas numurs un tā derīguma termiņš tiek glabāts pie šīs kompetences (**Paraugi un laboratorija**) | nav; tās apstiprinātie rezultāti aizpilda partijas KKP 1 |
| **Vairumtirdzniecība** | Vairumtirdzniecība; turētājs KKP 10, kamēr preces ir tās rīcībā | Tur saņemtās gala partijas, reģistrē KKP 10 uzglabāšanu savam daudzumam un var sūtīt tālāk (**Sūtījumi un krājumi**) | KKP 10 |
| **Mazumtirdzniecība** | Mazumtirdzniecība; turētājs KKP 10, kamēr preces ir tās rīcībā | Saņem sūtījumus pārdošanai, reģistrē KKP 10 uzglabāšanu savam daudzumam un var sūtīt tālāk (**Sūtījumi un krājumi**) | KKP 10 |

Pārstrādes uzņēmumam, kas arī pilda un uzglabā savu medu, nepieciešamas kompetences **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** (vai **Mazumtirdzniecība**), jo gala partijas KKP 7–9 prasa kompetenci "Iepakošana", bet KKP 10 – "Vairumtirdzniecība", "Mazumtirdzniecība" vai "Iepakošana".

Sarakstā ir arī trīs kompetences īpašiem gadījumiem:

- **Centrālā biedrība**: nacionālā biškopju biedrība. Ja platforma ir konfigurēta tā, ka biedrības pārbauda centrālā biedrība, pārbaudītas centrālās biedrības organizācijas administrators pārbauda biedrības un redz biedrības, kuras pārbauda.
- **Valsts iestāde**: ministrija vai uzraudzības iestāde. Pēc pārbaudes tās dalībnieki skata visus ierakstus; tā neko neievada.
- **Biškopība**: biškopja saimniecība. Biškopji nepiesakās; tie ir ieraksti, ko uztur platforma un viņu biedrība.

**Platforma.** *Platformas administrators* nav kompetence, ko var piešķirt organizācijai: tā ir lietotāja konta globāla loma, kas paredzēta personām, kuras uztur platformu. Platformas administratori pārvalda organizācijas, dalībniekus, atsauces datus un iestatījumus un skata katru ierakstu, taču neievada uzņēmējdarbības datus (skatīt [12. nodaļu](#admin)). Platformas pašas organizācijai ir rezervētā kompetence **Platformas administrators**; to nevar piešķirt nevienai citai organizācijai un nevar noņemt.

## Organizāciju veidi {#org-types}

Platformā nav atsevišķa organizāciju veidu saraksta: organizācijas veidu nosaka tās kompetences. Tipiskas organizācijas:

| Organizācija | Kompetences | Tipiskās dalībnieku lomas |
|---|---|---|
| Biškopju biedrība | **Biedrība** | Organizācijas administrators; Lauka inspektors |
| Pārstrādes uzņēmums vai iepakotājs | **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** vai **Mazumtirdzniecība** uzglabāšanai uzņēmumā | Organizācijas administrators, Paraugu pieņemšanas speciālists, Darbinieks, Kvalitātes apstiprinātājs |
| Laboratorija (akreditēta vai iekšējā) | **Laboratorija** | Organizācijas administrators, Analītiķis, Laboratorijas vadītājs |
| Vairumtirgotājs | **Vairumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes apstiprinātājs |
| Veikals vai tirgus | **Mazumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes apstiprinātājs |
| Ministrija vai uzraudzības iestāde | **Valsts iestāde** | Organizācijas administrators (tikai lasīšana) |

Biedrībai, kas pati uztur pārstrādes uzņēmumu, ir arī kompetence **Pārstrāde** (un **Iepakošana**), un tad arī uzņēmuma lomas.

### Organizācijas statuss {#org-status}

| Statuss | Nozīme |
|---|---|
| **Gaida** | Jauna organizācija, kas gaida pārbaudi. Dalībnieki var skatīt datus, un organizācijas administrators var augšupielādēt dokumentus. Neviens nevar ievadīt datus. |
| **Pārbaudīta** | Organizāciju ir pārbaudījis platformas administrators. Dalībnieki var strādāt atbilstoši savām lomām. |
| **Apturēta** | Pārbaude atsaukta (ar norādītu iemeslu). Līdz atjaunošanai darbojas tāpat kā statuss "Gaida". |

## Lomas {#role-list}

Loma tiek piešķirta ar **dalību**: viena persona, viena organizācija, viena loma. Dalības izveido platformas administratori (skatīt [Dalībnieki](#admin-members)); vienai personai var būt vairākas dalības (skatīt [Vairākas dalības](#memberships)).

| Loma (ekrānā) | Kods | Tipiska persona | Galvenie darbi |
|---|---|---|---|
| **Organizācijas administrators** | OrgAdmin | Organizācijas vadītājs | Augšupielādē dokumentus, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē biškopju piekrišanas (biedrībās), skata piegādes un paraugus |
| **Darbinieks** | Staff | Ražošanas, pildīšanas vai veikala darbinieks | Ievada KKP 2–10 vērtības, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē uzglabāšanu |
| **Lauka inspektors** | FieldInspector | Biedrības lauka tehniķis | Skata ierakstus; ievada KKP 0 tikai tad, ja administrators šo KKP piešķir biedrībai |
| **Paraugu pieņemšanas speciālists** | SampleAcceptanceOfficer | Pieņemšanas speciālists uzņēmumā | Reģistrē piegādes, ievada KKP 0, ņem un nosūta paraugus |
| **Kvalitātes apstiprinātājs** | QualityApprover | Kvalitātes vadītājs, pārtikas inženieris | Apstiprina KKP ierakstus, apstiprina pabeigšanu (fiksēšanu), apstiprina uzglabāšanas ierakstus |
| **Analītiķis** | Analyst | Laboratorijas analītiķis | Saņem paraugus, ievada laboratorijas rezultātus, iesniedz pārskatus |
| **Laboratorijas vadītājs** | LabManager | Laboratorijas vadītājs | Saņem paraugus, apstiprina vai atgriež laboratorijas pārskatus |
| **Platformas administrators** | (globāla loma) | Platformas uzturētājs | Administrēšana (12. nodaļa); uzņēmējdarbības datus tikai skata |

Platformas administrators nepiedalās uzņēmējdarbības organizācijas darba plūsmā: viņš var skatīt katru ierakstu, taču **nevar ievadīt vai apstiprināt uzņēmējdarbības datus** (piegādes, KKP vērtības, laboratorijas rezultātus, sūtījumus, piekrišanas). Ja dati pēc apstiprināšanas izrādās nepareizi, administrators pieprasa *labojumu*, ko apstiprina otrs administrators (skatīt [Labojumi](#admin-corrections)).

## Atļauju matrica {#permission-matrix}

Tālāk redzamajā matricā parādītas **noklusējuma** tiesības, kā tās ir konfigurētas pašlaik. Platformas administrators var mainīt lielāko daļu no tām lapās **Lomas un atļaujas** un **KKP atbildība**; tālāk minētos fiksētos noteikumus mainīt nevar.

Apzīmējumi: **V** skatīt, **C** izveidot vai ievadīt, **A** apstiprināt (vai saņemt), **F** pabeigt (fiksēt) (pieprasīt vai apstiprināt), **–** nav tiesību. Kolonnā par organizāciju norādīts, kāda kompetence organizācijai nepieciešama.

| Darbība | Nepieciešamā kompetence | Organizācijas administrators | Darbinieks | Lauka inspektors | Paraugu pieņemšanas speciālists | Kvalitātes apstiprinātājs | Analītiķis | Laboratorijas vadītājs | Platformas administrators |
|---|---|---|---|---|---|---|---|---|---|
| Reģistrēt piegādi (pieņemšana) | Pārstrāde | V | – | – | C V | – | – | – | V |
| Ņemt un nosūtīt paraugu | Pārstrāde (partijas īpašnieks) | V | – | – | C V | – | V | V | V |
| Saņemt paraugu (plombas pārbaude) | Laboratorija | – | – | – | – | – | A | A | V |
| Ievadīt laboratorijas rezultātus, iesniegt pārskatu | Laboratorija | V | V | V | V | V | C V | V | V |
| Apstiprināt vai atgriezt laboratorijas pārskatu | Laboratorija | V | V | V | V | V | V | A V | V |
| KKP 0 pieņemšanas pārbaude (partija) | Pārstrāde (partijas īpašnieks) | V | V | V | C V | A V | V | V | V |
| KKP 1 laboratorijas vērtības (partija) | tiek ierakstītas no apstiprinātā laboratorijas pārskata | V | V | V | V | V | V | V | V |
| KKP 2–3 (partija) | Pārstrāde (partijas īpašnieks) | V | C V | V | V | A V | V | V | V |
| Izveidot gala partiju, noteikt tās sastāvu, piešķirt KKP | – | – | – | – | – | – | – | – | C |
| KKP 4–6 (gala partija) | Pārstrāde (gala partijas īpašnieks) | V | C V | V | V | A V | V | V | V |
| KKP 7–9 (gala partija) | Iepakošana (gala partijas īpašnieks) | V | C V | V | V | A V | V | V | V |
| KKP 10 uzglabāšana uzņēmumā (gala partija) | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana (gala partijas īpašnieks) | V | C V | V | V | A V | V | V | V |
| Pieprasīt pabeigšanu (fiksēšanu) | īpašnieks, pārbaudīts | F | F | – | – | – | – | – | – |
| Apstiprināt pabeigšanu (fiksēt) | īpašnieks, pārbaudīts | – | – | – | – | F | – | – | – |
| Izveidot sūtījumu | turētājs, pārbaudīts | C V | C V | – | – | V | – | – | V |
| Apstiprināt vai noraidīt saņemtu sūtījumu | saņēmējs, pārbaudīts | A V | A V | – | – | V | – | – | V |
| KKP 10 uzglabāšanas ieraksts pie saņēmēja | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana | V | C V | V | V | A V | V | V | V |
| Atsaukšanas pārskats (savā tvērumā) | – | V | V | V | V | V | V | V | V (visi) |
| Reģistrēt vai atsaukt biškopja piekrišanu | Biedrība | C V | – | – | – | – | – | – | V |
| Augšupielādēt organizācijas dokumentus | sava organizācija | C | – | – | – | – | – | – | C |
| Pārbaudīt organizācijas, pārvaldīt dalībniekus un kompetences | – | – | – | – | – | – | – | – | C A |
| Pārbaudīt biškopju reģistrācijas numurus | – | – | – | – | – | – | – | – | A |
| Mainīt atļaujas, KKP, robežvērtības, tulkojumus | – | – | – | – | – | – | – | – | C |
| Pieprasīt vai izlemt labojumu | – | – | – | – | – | – | – | – | C A (divi dažādi administratori) |
| Audita žurnāls | – | – | – | – | – | – | – | – | V |

Piezīmes:

- **KKP skatīšana (V)**: katra loma var atvērt to partiju un gala partiju KKP, kuros piedalās tās organizācija: kā īpašnieks vai atbildīgā organizācija, kā organizācija, kas paņēma paraugu (partijām), kā vēlāks gala partijas turētājs vai kā pārbaudīta valsts iestāde. Turētājs gala partijas un tās partiju KKP redz **tikai lasīšanas režīmā**. Jebkurai citai organizācijai tiek parādīta lapa "Jūs nevarat atvērt šo lapu" (403).
- **KKP 1** tiek aizpildīts no apstiprinātā laboratorijas pārskata. Tam konfigurētās ievadītāju lomas (paraugu pieņemšanas speciālists, darbinieks) tiek izmantotas tikai tiem KKP 1 parametriem, kurus laboratorijas pārskats neaptver.
- **Sūtījumiem un pabeigšanai (fiksēšanai)** tiek pārbaudīta organizācijas loma un statuss, nevis kompetence: jebkurš pārbaudīts pabeigtas (fiksētas) gala partijas īpašnieks var to nosūtīt.
- Administrators var piešķirt **KKP 0** biškopja biedrībai, nevis uzņēmumam (**KKP atbildība** → KKP 0 → "Partijas biškopja biedrība", kompetence "Biedrība", loma "Lauka inspektors"). Tas nav noklusējuma iestatījums.

<!-- SHOT: admin-permissions-matrix -->

## Fiksēti noteikumi {#fixed-rules}

Šie noteikumi ir iestrādāti platformā:

- **Četru acu princips KKP ierakstiem**: neviens nevar apstiprināt KKP ierakstu, kurā pats ir reģistrējis vērtības vai kuru pats ir atzīmējis kā "Nav piemērojams". Izņēmums: platformas administrators var piešķirt organizācijai izņēmumu (**Četru acu principa izņēmums**); tad dalībnieki drīkst apstiprināt savus KKP ierakstus, un katrs šāds apstiprinājums tiek atzīmēts audita žurnālā.
- **Četru acu princips laboratorijas rezultātiem – vienmēr**: persona, kas ievadīja rezultātus, nevar tos apstiprināt. Izņēmumu nav.
- **Četru acu princips pabeigšanai (fiksēšanai)**: persona, kas pieprasīja pabeigšanu (fiksēšanu), nevar to apstiprināt (ja vien organizācijai nav piešķirts izņēmums).
- **Datus ievada tikai pārbaudītas organizācijas.**
- **Platformas administratori uzņēmējdarbības datus tikai skata** un labo tos tikai ar divu administratoru labojumu procesu.
- **Pabeigts (fiksēts) nozīmē bloķēts**: pabeigtu (fiksētu) gala partiju un visas tās partijas vairs nevar mainīt, izņemot ar labojumu.
- **Audita žurnālu var tikai papildināt**; tajā nekas netiek mainīts vai dzēsts, un arī katra izmaiņa atļauju lapās tiek reģistrēta žurnālā.

## Vairākas dalības {#memberships}

Vienai personai var būt vairākas dalības: dažādās organizācijās vai vairākas lomas vienā organizācijā (piemēram, **Kvalitātes apstiprinātājs** un **Organizācijas administrators** biedrībā). Vienlaikus jūs vienmēr strādājat vienā dalībā.

Lai pārslēgtos:

1. Noklikšķiniet uz **organizācijas · lomas žetona** augšējā joslā (skatīt [Augšējā josla un jūsu loma](#top-bar)). Sarakstā ar virsrakstu **Jūsu lomas** redzamas visas aktīvās dalības; pašreizējā ir atzīmēta.
2. Noklikšķiniet uz citas dalības. Platforma atver lapu **Mana organizācija** jaunajā dalībā.

Izvēlne, uzdevumi sadaļā **Mans darbs**, redzamie ieraksti un atļautās darbības atbilst aktīvajai dalībai. Izvēle ir spēkā, līdz aizverat pārlūku; pēc tam jūs atkal sākat savā pirmajā dalībā (alfabētiskā secībā pēc organizācijas nosaukuma). Ja jums ir divas vai vairākas dalības, lapā **Mana organizācija** ir arī saraksts **Rīkoties kā** ar pogu **Pārslēgt**.
