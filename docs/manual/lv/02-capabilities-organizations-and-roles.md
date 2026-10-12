# 2. Kompetences, organizācijas un lomas {#roles}

TOP4HoneyChains platformā persona nekad nerīkojas viena: katra darbība tiek veikta **organizācijas vārdā, noteiktā lomā**. To, ko jūs drīkstat darīt, nosaka trīs lietas:

1. jūsu organizācijas **kompetences**: tas, ko tā spēj darīt medus ķēdē;
2. jūsu **loma** organizācijā: kas jūs esat tās iekšienē;
3. jūsu organizācijas **statuss**: datus var ievadīt tikai organizācija ar statusu **Pārbaudīta**. Organizācijas pārbauda platformas administrators (skatīt [Organizācijas statuss](#org-status)).

Lomu iegūst ar **dalību**: viens lietotāja konts × viena organizācija × viena loma (skatīt [Vairākas dalības](#memberships)). Papildus tam daži noteikumi ir iestrādāti platformā, un tos nevar izslēgt (skatīt [Pamatnoteikumi](#fixed-rules)).

Šajā nodaļā nošķirtas divas lietas:

- **vērtību ķēdes darbības**: darbs, ko cilvēki ar medu veic ārpus datora, piemēram, piegāde, paraugu ņemšana, analīze, apstrāde, uzglabāšana un nosūtīšana ([Table](#tbl-activities));
- **datu tiesības**: tas, ko katra loma drīkst darīt ar ierakstiem platformā: izveidot, lasīt, mainīt, dzēst un apstiprināt ([Table](#tbl-crud)).

## Kompetences {#capabilities}

Kompetence ir organizācijas spēja: tā nosaka, ko organizācija **spēj darīt** medus ķēdē. To, kurš dalībnieks darbu veic, nosaka lomas (skatīt [Lomas](#role-list)). Organizācijai ir viena vai vairākas kompetences; tās piešķir un noņem platformas administrators (skatīt [Kompetences](#admin-org-capabilities)). Pats kompetenču saraksts ir nemainīgs. [Table](#tbl-capabilities) uzskaita kompetences ar to nosaukumu ekrānā.

Table: Kompetences: ko organizācija spēj darīt {#tbl-capabilities}

| Kompetence (ekrānā) | Spēja | KKP (noklusējums) |
|---|---|---|
| **Biedrība** | Spēj sasaistīt savus biedrus biškopjus un reģistrēt viņu piekrišanu publiskai biškopja lapai. | nav; KKP 0 tikai tad, ja administrators to piešķir biedrībai |
| **Pārstrāde** | Spēj pieņemt un pārstrādāt medu: piegādes, paraugi, partijas KKP 0–3 un gala partijas KKP 4–6. | partijas KKP 0–3, gala partijas KKP 4–6 |
| **Iepakošana** | Spēj pildīt, marķēt un iepakot medu: gala partijas KKP 7–9 un KKP 10 uzglabāšana uzņēmumā. | gala partijas KKP 7–9, KKP 10 |
| **Laboratorija** | Spēj analizēt medus paraugus un izsniegt laboratorijas pārskatus kā akreditēta vai iekšējā pārtikas analīžu laboratorija. Akreditācijas numurs un tā derīguma termiņš tiek glabāts pie šīs kompetences. | nav; tās apstiprinātie rezultāti aizpilda partijas KKP 1 |
| **Vairumtirdzniecība** | Spēj turēt gala partijas vairumtirdzniecībā, reģistrēt to uzglabāšanu (KKP 10) un sūtīt tās tālāk. | KKP 10 |
| **Mazumtirdzniecība** | Spēj turēt gala partijas pārdošanai patērētājiem, reģistrēt to uzglabāšanu (KKP 10) un sūtīt tās tālāk. | KKP 10 |
| **Centrālā biedrība** | Spēj pārbaudīt biškopju biedrības, ja platforma ir tā konfigurēta. | nav |
| **Valsts iestāde** | Spēj lasīt visus ierakstus kā ministrija vai uzraudzības iestāde; neko neievada. | nav (lasa visu) |
| **Biškopības saimniecība** | Spēj iegūt medu. Biškopjiem nav kontu; viņu saimniecību pārstāv biškopja ieraksts. | nav |

Pārstrādes uzņēmumam, kas arī pilda un uzglabā savu medu, nepieciešamas kompetences **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** (vai **Mazumtirdzniecība**), jo gala partijas KKP 7–9 prasa kompetenci "Iepakošana", bet KKP 10 – "Vairumtirdzniecība", "Mazumtirdzniecība" vai "Iepakošana".

**Platforma.** *Platformas administrators* nav kompetence, ko var piešķirt organizācijai: tā ir lietotāja konta globāla loma, kas paredzēta personām, kuras uztur platformu. Platformas administratori pārvalda organizācijas, dalības, atsauces datus un iestatījumus un lasa katru ierakstu, taču neievada darba plūsmas datus (skatīt [12. nodaļu](#admin)). Platformas pašas organizācijai ir rezervētā kompetence **Platformas administrators**; to nevar piešķirt nevienai citai organizācijai un nevar noņemt.

## Vērtību ķēdes darbības {#activities}

Vērtību ķēdes darbības veic cilvēki ar medu ārpus datora. Platforma katras darbības rezultātu reģistrē kā datus. [Table](#tbl-activities) katrai kompetencei parāda, kādas darbības veic tās cilvēki un kas tiek reģistrēts platformā.

Table: Vērtību ķēdes darbības (cilvēki) un tas, ko reģistrē platforma (dators) {#tbl-activities}

| Kompetence | Vērtību ķēdes darbības (cilvēki, ārpus datora) | Reģistrē platformā (datorizēti) |
|---|---|---|
| Biškopības saimniecība | Tur bites, ievāc medu, piegādā to uzņēmumam | Biškopja un dravas ieraksti, ko uztur administratori un biedrība |
| Biedrība | Sasaista un informē biedrus biškopjus, nolasa viņiem piekrišanas tekstu, savāc parakstus | Piekrišanas ieraksti; publiskās biškopju lapas |
| Pārstrāde | Pieņem piegādes, pārbauda medu pieņemšanas brīdī, noplombē paraugus, karsē, filtrē, sajauc un nostādina medu | Piegāde (neapstrādāta medus partija), paraugs, KKP 0 un KKP 2–6 dati |
| Iepakošana | Pilda, marķē un iepako medu, uzglabā to uzņēmumā | KKP 7–10 dati |
| Laboratorija | Saņem un pārbauda noplombētus paraugus, analizē medu, paraksta pārskatu | Parauga saņemšana, laboratorijas pārskats (PDF) un rezultāti (KKP 1) |
| Vairumtirdzniecība, Mazumtirdzniecība | Saņem preces, uzglabā tās, pārdod vai sūta tālāk | Sūtījuma apstiprinājums, uzglabāšanas ieraksts (KKP 10), jauni sūtījumi |
| Centrālā biedrība | Pārbauda biškopju biedrības | Biedrību pārbaude (ja tā konfigurēts) |
| Valsts iestāde | Veic uzraudzību | Lasa ierakstus |

## Organizāciju veidi {#org-types}

Platformā nav atsevišķa organizāciju veidu saraksta: organizācijas veidu nosaka tās kompetences. [Table](#tbl-org-types) uzskaita tipiskās organizācijas.

Table: Tipiskās organizācijas un to kompetences {#tbl-org-types}

| Organizācija | Kompetences | Tipiskās dalībnieku lomas |
|---|---|---|
| Biškopju biedrība | **Biedrība** | Organizācijas administrators, Lauka inspektors |
| Pārstrādes uzņēmums vai iepakotājs | **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** vai **Mazumtirdzniecība** uzglabāšanai uzņēmumā | Organizācijas administrators, Darbinieks, Paraugu pieņemšanas speciālists, Kvalitātes nodrošināšanas speciālists |
| Pārtikas analīžu laboratorija (akreditēta vai iekšējā) | **Laboratorija** | Organizācijas administrators, Analītiķis, Laboratorijas vadītājs |
| Vairumtirgotājs | **Vairumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes nodrošināšanas speciālists |
| Mazumtirgotājs | **Mazumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes nodrošināšanas speciālists |
| Ministrija vai uzraudzības iestāde | **Valsts iestāde** | Organizācijas administrators (tikai lasīšana) |

Biedrībai, kas pati uztur pārstrādes uzņēmumu, ir arī kompetence **Pārstrāde** (un **Iepakošana**), un tad arī uzņēmuma lomas.

### Organizācijas statuss {#org-status}

Platformas administrators pārbauda, aptur un atjauno organizācijas. Ja platforma ir tā konfigurēta, pārbaudītas **Centrālās biedrības** organizācijas administrators var arī pārbaudīt, apturēt un atjaunot biškopju biedrības. Pārbaudei nepieciešams vismaz viens augšupielādēts dokuments; apturēšanai – iemesls. [Table](#tbl-org-status) uzskaita statusus.

Table: Organizācijas statuss {#tbl-org-status}

| Statuss | Nozīme |
|---|---|
| **Gaida** | Jauna organizācija, kas gaida pārbaudi. Dalībnieki var lasīt datus, un organizācijas administrators var augšupielādēt dokumentus. Neviens nevar ievadīt datus. |
| **Pārbaudīta** | Organizāciju ir pārbaudījis platformas administrators (vai – biedrības – centrālā biedrība, ja tā konfigurēts). Dalībnieki var strādāt atbilstoši savām lomām. |
| **Apturēta** | Pārbaude atsaukta (ar norādītu iemeslu). Līdz atjaunošanai darbojas tāpat kā statuss "Gaida". |

## Lomas {#role-list}

[Table](#tbl-roles) ir lomu atsauces saraksts. Visas pārējās tabulas šajā rokasgrāmatā lieto tos pašus lomu nosaukumus tādā pašā secībā.

Table: Lomas {#tbl-roles}

| Loma (šajā rokasgrāmatā) | Kods | Tipiska persona | Galvenie darbi |
|---|---|---|---|
| **Organizācijas administrators** | OrgAdmin | Organizācijas vadītājs | Augšupielādē dokumentus, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē biškopju piekrišanas (biedrībās), lasa piegādes un paraugus |
| **Darbinieks** | Staff | Ražošanas, pildīšanas vai mazumtirdzniecības darbinieks | Ievada KKP 1–10 datus, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē uzglabāšanu |
| **Lauka inspektors** | FieldInspector | Biedrības lauka tehniķis | Lasa ierakstus; ievada KKP 0 tikai tad, ja administrators šo KKP piešķir biedrībai |
| **Paraugu pieņemšanas speciālists** | SampleAcceptanceOfficer | Pieņemšanas speciālists uzņēmumā | Reģistrē piegādes, ievada KKP 0, ņem un nosūta paraugus |
| **Kvalitātes nodrošināšanas speciālists** | QualityApprover | Kvalitātes vadītājs, pārtikas inženieris | Apstiprina KKP ierakstus un uzglabāšanas ierakstus, apstiprina pabeigšanu (fiksēšanu) |
| **Analītiķis** | Analyst | Laboratorijas analītiķis | Saņem paraugus, ievada laboratorijas rezultātus, iesniedz pārskatus (pirmā persona laboratorijā) |
| **Laboratorijas vadītājs** | LabManager | Laboratorijas vadītājs | Saņem paraugus, apstiprina vai atgriež laboratorijas pārskatus (otrā persona laboratorijā) |
| **Platformas administrators** | (globāla loma) | Platformas uzturētājs | Administrēšana (12. nodaļa); lasa visus darba plūsmas datus, bet neko neievada un neapstiprina |

Platformas administrators nepiedalās uzņēmējdarbības organizācijas darba plūsmā: viņš var lasīt katru ierakstu, taču **nevar ievadīt vai apstiprināt darba plūsmas datus** (piegādes, KKP datus, laboratorijas rezultātus, sūtījumus, piekrišanas). Ja dati pēc apstiprināšanas izrādās nepareizi, administrators pieprasa *labojumu*, ko apstiprina otrs administrators (skatīt [Labojumi](#admin-corrections)). Administratori gan uztur pamatdatus: biškopjus, dravas, partijas, gala partijas un to sastāvu (skatīt [12. nodaļu](#admin)).

## Datu tiesības (CRUD) {#crud}

[Table](#tbl-crud) parāda katras lomas noklusējuma datu tiesības katram ierakstu veidam pēc CRUD modeļa: **C** izveidot (create), **R** lasīt (read), **U** mainīt (update), **D** dzēst (delete) un **A** apstiprināt (atsevišķas tiesības). "–" nozīmē, ka tiesību nav. Dalībnieka tiesības attiecas tikai uz viņa organizācijas darba ierakstiem (tās partijām un gala partijām, gala partijām tās turējumā, tās biedrības biškopjiem) un, izņemot lasīšanu, tikai tik ilgi, kamēr organizācija ir pārbaudīta. Tiesības izriet no programmas koda un noklusējuma iestatījumiem; [Table](#tbl-admin-changeable) uzskaita, kuras no tām administrators var mainīt.

Table: Noklusējuma datu tiesības pa lomām (CRUD + apstiprināšana) {#tbl-crud}

| Ieraksts | Organizācijas administrators | Darbinieks | Lauka inspektors | Paraugu pieņemšanas speciālists | Kvalitātes nodrošināšanas speciālists | Analītiķis | Laboratorijas vadītājs | Platformas administrators | Publisks apmeklētājs |
|---|---|---|---|---|---|---|---|---|---|
| Biškopis | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Drava | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Neapstrādāta medus partija | R | R | R | C R ⁴ | R | – | – | C R U D ⁵ | R ³ |
| Paraugs | R | – | – | C R | – | R A ⁶ | R A ⁶ | R | – |
| Laboratorijas pārskats un rezultāti | R | R | R | R | R | C R U D ⁷ | R A ⁷ | R | R ⁸ |
| KKP ieraksts (dati, "Nav piemērojams") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | R ¹⁰ | R ³ |
| Gala partija | R, pieprasa pabeigšanu | R, pieprasa pabeigšanu | R | R | R, apstiprina pabeigšanu (A) | R | R | C R U D ¹¹ | R ³ |
| Sūtījums | C R A ¹² | C R A ¹² | R | R | R | R | R | R | – |
| Uzglabāšanas ieraksts pie saņēmēja (KKP 10) | R | C R U D | R | R | R A | R | R | R | – |
| Biškopja piekrišana | C R U ¹³ | R | R | R | R | R | R | R | R ¹⁴ |
| Organizācija (dati, dokumenti) | R; dokumenti C | R | R | R | R | R | R | C R U, statuss A ¹⁵ | – |
| Dalība | R | R | R | R | R | R | R | C R U ¹⁶ | – |
| Audita ieraksts | – | – | – | – | – | – | – | R ¹⁷ | – |

Piezīmes:

1. Lasa tikai ar to ierakstu starpniecību, ar kuriem strādā organizācija (KKP lapas, atsaukšanas pārskats). Vēlāks gala partijas turētājs lasa datus bez biškopju vārdiem un dravu nosaukumiem. Biedrības dalībnieki lasa savu biedru biškopju sarakstu.
2. Biškopji un dravas nekad netiek dzēsti, tikai deaktivizēti. Dravu fotoattēlus var pievienot, mainīt un dzēst.
3. Pabeigtu (fiksētu) gala partiju publiskajās lapās (Medus stāsts) un publicētajās biškopju lapās: vārdi, province un apstiprināti dati; nekad koordinātas vai kontaktinformācija (skatīt [Ko var lasīt sabiedrība](#trust-public)).
4. Partija tiek izveidota, reģistrējot piegādi, pārbaudītā organizācijā ar kompetenci **Pārstrāde**.
5. Administratori uztur partijas pamatdatus (piemēram, īpašnieci organizāciju un KKP piešķīrumu). Kad partija ir pabeigtā (fiksētā) gala partijā, neko vairs nevar mainīt; partiju, kurai ir izmantotas piegādes, paraugi vai KKP dati, nevar dzēst.
6. **A** = saņemt paraugu ar neskartu plombu vai to noraidīt.
7. Analītiķis izveido, saglabā, maina un iesniedz pārskatu, kamēr tas ir melnraksts; laboratorijas vadītājs to apstiprina vai atgriež. Apstiprinātu pārskatu nevar mainīt.
8. Lapa **Pārbaudīt laboratorijas pārskatu**, pēc pārskata koda.
9. Ievadīt datus (C), mainīt vai notīrīt tos (U, D) un atzīmēt "Nav piemērojams", līdz ieraksts tiek apstiprināts vai bloķēts. Noklusējuma ievadītāju lomas: paraugu pieņemšanas speciālists KKP 0–1, darbinieks KKP 1–10. Lauka inspektors ievada KKP 0 tikai tad, ja administrators KKP 0 piešķir biedrībai.
10. Apstiprinātus vai bloķētus datus administratori maina tikai ar labojumu, ko pieprasa viens administrators un apstiprina otrs.
11. Izveidot gala partiju, noteikt tās sastāvu no partijām, piešķirt tās KKP un noteikt īpašnieku. Gala partiju var dzēst tikai pirms tās pabeigšanas (fiksēšanas). Administratori nevar pabeigt (fiksēt) gala partiju.
12. **C** = nosūtīt pabeigtu (fiksētu) gala partiju, kas ir jūsu organizācijas turējumā; **A** = apstiprināt vai noraidīt sūtījumu, ko saņem jūsu organizācija.
13. Reģistrēt (C) un atsaukt (U) piekrišanu; tikai biedrībām.
14. Publicētā biškopja lapa.
15. Pārbaudīt, pieprasīt dokumentus, apturēt un atjaunot (skatīt [Organizācijas statuss](#org-status)). Organizācijas nekad netiek dzēstas.
16. Pievienot dalību un to atsaukt (U); dalības nekad netiek dzēstas.
17. Audita žurnālu var tikai papildināt; neviens nevar mainīt vai dzēst tā ierakstus.

**Ko var dzēst.** Dalībnieki notīra tikai savus datus, pirms tie ir apstiprināti (KKP dati, laboratorijas rezultātu melnraksti, uzglabāšanas dati). Administratori var dzēst partijas un gala partijas pirms pabeigšanas (fiksēšanas), dravu fotoattēlus, atsauces datus un lietotāju kontus (nekad savu). Biškopji, dravas, organizācijas, dokumenti, dalības, piekrišanas, paraugi, laboratorijas pārskati, sūtījumi un audita ieraksti nekad netiek dzēsti.

## Atļauju matrica {#permission-matrix}

[Table](#tbl-permissions) parāda noklusējuma tiesības katrai **darbībai platformā** (funkcijai, piemēram, "reģistrēt piegādi"), kā tās ir konfigurētas pašlaik. Tā papildina datu tiesības ([Table](#tbl-crud)) un lieto tās pašas lomas tādā pašā secībā. Apzīmējumi: **R** lasīt, **C** izveidot vai ievadīt, **A** apstiprināt (vai saņemt), **F** pabeigt (fiksēt) (pieprasīt vai apstiprināt), **–** nav tiesību. Kolonnā "Nepieciešamā kompetence" norādīts, kāda kompetence nepieciešama organizācijai.

Table: Noklusējuma tiesības pa darbībām platformā un lomām {#tbl-permissions}

| Darbība platformā | Nepieciešamā kompetence | Organizācijas administrators | Darbinieks | Lauka inspektors | Paraugu pieņemšanas speciālists | Kvalitātes nodrošināšanas speciālists | Analītiķis | Laboratorijas vadītājs | Platformas administrators |
|---|---|---|---|---|---|---|---|---|---|
| Reģistrēt piegādi (pieņemšana) | Pārstrāde | R | – | – | C R | – | – | – | R |
| Ņemt un nosūtīt paraugu | Pārstrāde (partijas īpašnieks) | R | – | – | C R | – | R | R | R |
| Saņemt paraugu (plombas pārbaude) | Laboratorija | – | – | – | – | – | A | A | R |
| Ievadīt laboratorijas rezultātus, iesniegt pārskatu | Laboratorija | R | R | R | R | R | C R | R | R |
| Apstiprināt vai atgriezt laboratorijas pārskatu | Laboratorija | R | R | R | R | R | R | A R | R |
| KKP 0 pieņemšanas pārbaude (partija) | Pārstrāde (partijas īpašnieks) | R | R | R | C R | A R | R | R | R |
| KKP 1 laboratorijas dati (partija) | tiek ierakstīti no apstiprinātā laboratorijas pārskata | R | R | R | R | R | R | R | R |
| KKP 2–3 (partija) | Pārstrāde (partijas īpašnieks) | R | C R | R | R | A R | R | R | R |
| Izveidot gala partiju, noteikt tās sastāvu, piešķirt KKP | – | – | – | – | – | – | – | – | C |
| KKP 4–6 (gala partija) | Pārstrāde (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | R |
| KKP 7–9 (gala partija) | Iepakošana (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | R |
| KKP 10 uzglabāšana uzņēmumā (gala partija) | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | R |
| Pieprasīt pabeigšanu (fiksēšanu) | īpašnieks, pārbaudīts | F | F | – | – | – | – | – | – |
| Apstiprināt pabeigšanu (fiksēt) | īpašnieks, pārbaudīts | – | – | – | – | F | – | – | – |
| Izveidot sūtījumu | turētājs, pārbaudīts | C R | C R | – | – | R | – | – | R |
| Apstiprināt vai noraidīt saņemtu sūtījumu | saņēmējs, pārbaudīts | A R | A R | – | – | R | – | – | R |
| KKP 10 uzglabāšanas ieraksts pie saņēmēja | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana | R | C R | R | R | A R | R | R | R |
| Atsaukšanas pārskats (savā tvērumā) | – | R | R | R | R | R | R | R | R (visi) |
| Reģistrēt vai atsaukt biškopja piekrišanu | Biedrība | C R | – | – | – | – | – | – | R |
| Augšupielādēt organizācijas dokumentus | sava organizācija | C | – | – | – | – | – | – | C |
| Pārbaudīt organizācijas, pārvaldīt dalības un kompetences | – | – | – | – | – | – | – | – | C A |
| Pārbaudīt biškopju reģistrācijas numurus | – | – | – | – | – | – | – | – | A |
| Mainīt atļaujas, KKP, robežvērtības, tulkojumus | – | – | – | – | – | – | – | – | C |
| Pieprasīt vai izlemt labojumu | – | – | – | – | – | – | – | – | C A (divi dažādi administratori) |
| Audita žurnāls | – | – | – | – | – | – | – | – | R |

Piezīmes:

- **KKP lasīšana (R)**: katra loma var atvērt to partiju un gala partiju KKP, kuros piedalās tās organizācija: kā īpašnieks vai atbildīgā organizācija, kā organizācija, kas paņēma paraugu (partijām), kā vēlāks gala partijas turētājs vai kā pārbaudīta valsts iestāde. Turētājs gala partijas un tās partiju KKP lasa **tikai lasīšanas režīmā**. Jebkurai citai organizācijai tiek parādīta lapa "Jūs nevarat atvērt šo lapu" (403).
- **KKP 1** tiek aizpildīts no apstiprinātā laboratorijas pārskata. Tam konfigurētās ievadītāju lomas (paraugu pieņemšanas speciālists, darbinieks) tiek izmantotas tikai tiem KKP 1 parametriem, kurus laboratorijas pārskats neaptver.
- **Sūtījumiem un pabeigšanai (fiksēšanai)** tiek pārbaudīta organizācijas loma un statuss, nevis kompetence: jebkurš pārbaudīts pabeigtas (fiksētas) gala partijas īpašnieks var to nosūtīt.
- Administrators var piešķirt **KKP 0** biškopja biedrībai, nevis uzņēmumam (**KKP atbildība** → KKP 0 → **Partijas biškopja biedrība**, kompetence "Biedrība", loma "Lauka inspektors"). Tas nav noklusējuma iestatījums.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Lomas un atļaujas**: lomas Darbinieks noklusējuma tiesības.

### Ko administrators var mainīt {#admin-changeable}

[Table](#tbl-admin-changeable) uzskaita katru iestatījumu, ko platformas administrators var mainīt. Viss pārējais ir nemainīgs: lomu saraksts, kompetenču saraksts un [Pamatnoteikumi](#fixed-rules).

Table: Ko platformas administrators var mainīt {#tbl-admin-changeable}

| Iestatījums | Kur | Ko tieši var mainīt |
|---|---|---|
| Lomu atļaujas | **Lomas un atļaujas** → **Lomu atļaujas** | Katrai lomai: **Ievadīt**, **Apstiprināt** un **Lasīt** katram KKP; **Ievadīt** un **Lasīt** piegādēm (pieņemšanai); **Ievadīt**, **Apstiprināt** (saņemt) un **Lasīt** paraugiem; **Ievadīt**, **Apstiprināt** un **Lasīt** laboratorijas rezultātiem; **Ievadīt** (nosūtīt), **Apstiprināt** (apstiprināt saņemšanu) un **Lasīt** sūtījumiem; **Ievadīt** un **Lasīt** biškopju lapām; **Lasīt** atsaukšanas pārskatam; **Ievadīt** (pieprasīt) un **Apstiprināt** (apstiprināt) pabeigšanai (fiksēšanai). Katrām tiesībām paliek vismaz viena loma. |
| KKP atbildība | **KKP atbildība** | Katram KKP: atbildīgā organizācija (partijas vai gala partijas īpašniece vai – partijām – biškopja biedrība), tai nepieciešamās kompetences, ievadītāju lomas, apstiprinātāju lomas un tas, vai apstiprinājums ir obligāts. |
| Kompetences | Organizācijas lapa; **Organizācijas kompetences** | Pievienot vai noņemt organizācijas kompetences (vismaz viena paliek); laboratorijas akreditācijas numurs un tā derīguma termiņš. |
| Četru acu principa izņēmums | Organizācijas lapa | Katrai organizācijai: ļauj tās dalībniekiem apstiprināt savus KKP ierakstus un uzglabāšanas ierakstus un apstiprināt savu pabeigšanas (fiksēšanas) pieprasījumu; audita žurnālā tas vienmēr tiek atzīmēts. Nekad – laboratorijas rezultātiem. |
| Organizācijas statuss | Organizācijas lapa | Pārbaudīt, pieprasīt dokumentus, apturēt, atjaunot. |
| Dalības un lietotāji | Organizācijas lapa; **Dalībnieki**; **Lietotāju Pārvaldība** | Pievienot un atsaukt dalības; izveidot lietotājus, iestatīt paroles, atbloķēt, dzēst kontus. |
| Kontroles punkti, parametri, robežvērtības | **Kritiskie Kontroles Punkti** | Parametri, datu tipi, obligātie jautājumi, robežvērtības, "Robežvērtības pārbaudītas". |
| Pamatdati un atsauces dati | Administrēšanas izvēlnes | Biškopji, dravas, reģistrācijas numuri, partijas, gala partijas, medus veidi, periodi, tulkojumi. |

## Pamatnoteikumi {#fixed-rules}

Šie noteikumi ir iestrādāti platformā; neviens iestatījums tos neizslēdz:

- **Datus ievada tikai pārbaudītas organizācijas.**
- **Četru acu princips KKP ierakstiem**: neviens nevar apstiprināt KKP ierakstu, kurā pats ir reģistrējis datus vai kuru pats ir atzīmējis kā "Nav piemērojams". Izņēmums: platformas administrators var piešķirt organizācijai izņēmumu (**Četru acu principa izņēmums**); tad dalībnieki drīkst apstiprināt savus KKP ierakstus, un katrs šāds apstiprinājums tiek atzīmēts audita žurnālā.
- **Četru acu princips laboratorijas rezultātiem – vienmēr**: persona, kas ievadīja rezultātus, nevar tos apstiprināt. Izņēmumu nav.
- **Četru acu princips pabeigšanai (fiksēšanai) un uzglabāšanas ierakstiem**: persona, kas pieprasīja pabeigšanu (fiksēšanu), nevar to apstiprināt, un persona, kas reģistrēja uzglabāšanas ierakstu, nevar to apstiprināt (ja vien organizācijai nav piešķirts izņēmums).
- **Jebkuras izmaiņas noņem apstiprinājumu**: mainot datus, esošais apstiprinājums tiek noņemts; dati ārpus robežvērtībām apstiprināšanas brīdī ir skaidri jāapstiprina.
- **Laboratorijas rezultāti seko paraugam**: rezultātus ievada tikai saņemtam paraugam ar neskartu plombu un tikai pēc tam, kad ir apstiprināts partijas KKP 0; apstiprinātu pārskatu nevar mainīt.
- **Platformas administratori darba plūsmas datus tikai lasa** un labo tos tikai ar divu administratoru labojumu procesu.
- **Pabeigts (fiksēts) nozīmē bloķēts**: gala partiju var pabeigt (fiksēt) tikai tad, kad izpildīti visi gala partijas un tās partiju KKP; pabeigtu (fiksētu) gala partiju un visas tās partijas vairs nevar mainīt, izņemot ar labojumu. Pabeigšanu (fiksēšanu) atcelt nevar.
- **Sūtījumi**: tikai pabeigtas (fiksētas) gala partijas, tikai citai pārbaudītai organizācijai, ne vairāk par turējumā esošo daudzumu.
- **Organizācijas konfigurē tikai administratori**: kompetences, dalības un izņēmumu.
- **Katrām tiesībām paliek vismaz viena loma.**
- **Audita žurnālu var tikai papildināt**; tajā nekas netiek mainīts vai dzēsts, un arī katra izmaiņa atļauju lapās tiek reģistrēta žurnālā.
- **Biškopju lapas** ir publiskas tikai ar spēkā esošu piekrišanu, pārbaudītu reģistrācijas numuru un pārbaudītu biedrību.

## Vairākas dalības {#memberships}

**Dalība** ir viens lietotāja konts × viena organizācija × viena loma. Vienai personai var būt vairākas dalības: dažādās organizācijās vai vairākas lomas vienā organizācijā (piemēram, **Kvalitātes nodrošināšanas speciālists** un **Organizācijas administrators** biedrībā). Vienlaikus jūs vienmēr strādājat vienā dalībā, un tiesības netiek apvienotas: divas lomas nekad neļauj apstiprināt datus, ko esat ievadījis pats.

Lai pārslēgtos:

1. Noklikšķiniet uz **organizācijas · lomas žetona** augšējā joslā (skatīt [Augšējā josla un jūsu loma](#top-bar)). Sarakstā ar virsrakstu **Jūsu lomas** redzamas visas aktīvās dalības; pašreizējā ir atzīmēta.
2. Noklikšķiniet uz citas dalības. Platforma atver lapu **Mana organizācija** jaunajā dalībā.

Izvēlne, uzdevumi sadaļā **Mans darbs**, ieraksti, kurus varat lasīt, un atļautās darbības atbilst aktīvajai dalībai. Izvēle ir spēkā, līdz aizverat pārlūku; pēc tam jūs atkal sākat savā pirmajā dalībā (alfabētiskā secībā pēc organizācijas nosaukuma). Ja jums ir divas vai vairākas dalības, lapā **Mana organizācija** ir arī saraksts **Rīkoties kā** ar pogu **Pārslēgt**.
