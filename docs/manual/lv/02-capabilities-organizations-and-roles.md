# 2. Kompetences, organizācijas un lomas {#roles}

TOP4HoneyChains platformā persona nekad nerīkojas viena: katra darbība tiek veikta **organizācijas vārdā, noteiktā lomā**. To, ko jūs drīkstat darīt, nosaka trīs lietas:

1. jūsu organizācijas **kompetences**: tas, ko tā spēj darīt medus ķēdē;
2. jūsu **loma** organizācijā: kas jūs esat tās iekšienē;
3. jūsu organizācijas **statuss**: datus var ievadīt tikai organizācija ar statusu **Pārbaudīta**. Organizācijas pārbauda platformas administrators (skatīt [Organizācijas statuss](#org-status)).

Lomu iegūst ar **dalību**: viens lietotāja konts × viena organizācija × viena loma (skatīt [Vairākas dalības](#memberships)). Vienīgais izņēmums ir biškopji: biškopim var būt personīgs konts biškopju portālā, kas ir sasaistīts ar viņa biškopja ierakstu un kam nav dalības (skatīt [14. nodaļu](#portal)). Papildus tam daži noteikumi ir iestrādāti platformā, un tos nevar izslēgt (skatīt [Pamatnoteikumi](#fixed-rules)).

Šajā nodaļā nošķirtas divas lietas:

- **vērtību ķēdes darbības**: darbs, ko cilvēki ar medu veic ārpus datora, piemēram, reģistrācija, piegāde, paraugu ņemšana, analīze, apstrāde, uzglabāšana un nosūtīšana ([Table](#tbl-activities));
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
| **Biškopju reģistrācija** | Spēj reģistrēt savas teritorijas biškopjus un viņu dravas, apliecināt viņu identitāti un reģistrācijas numuru, reģistrēt viņu piekrišanu un izsniegt viņu pieteikšanās datus. | nav |
| **Centrālā biedrība** | Spēj pārbaudīt biškopju biedrības, ja platforma ir tā konfigurēta. | nav |
| **Valsts iestāde** | Spēj lasīt visus ierakstus kā ministrija vai uzraudzības iestāde; neko neievada. | nav (lasa visu) |
| **Biškopības saimniecība** | Spēj iegūt medu. Biškopjiem nav dalību; viņu saimniecību pārstāv biškopja ieraksts. | nav |

Pārstrādes uzņēmumam, kas arī pilda un uzglabā savu medu, nepieciešamas kompetences **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** (vai **Mazumtirdzniecība**), jo gala partijas KKP 7–9 prasa kompetenci "Iepakošana", bet KKP 10 – "Vairumtirdzniecība", "Mazumtirdzniecība" vai "Iepakošana". Kompetenci pārbauda arī pašas darbības: paraugu ņemšanai vajadzīga kompetence **Pārstrāde**; nosūtīšanai un saņemšanai – **Pārstrāde**, **Iepakošana**, **Vairumtirdzniecība** vai **Mazumtirdzniecība**; pabeigšanas (fiksēšanas) pieprasīšanai un apstiprināšanai – **Pārstrāde** vai **Iepakošana**.

**Platforma.** *Platformas administrators* nav kompetence, ko var piešķirt organizācijai: tā ir lietotāja konta globāla loma, kas paredzēta personām, kuras uztur platformu. Platformas administratori pārvalda organizācijas, dalības, atsauces datus un iestatījumus un lasa katru ierakstu, taču neievada darba plūsmas datus (skatīt [15. nodaļu](#admin)). Platformas pašas organizācijai ir rezervētā kompetence **Platformas administrators**; to nevar piešķirt nevienai citai organizācijai un nevar noņemt.

## Vērtību ķēdes darbības {#activities}

Vērtību ķēdes darbības veic cilvēki ar medu ārpus datora. Platforma katras darbības rezultātu reģistrē kā datus. [Table](#tbl-activities) katrai kompetencei parāda, kādas darbības veic tās cilvēki un kas tiek reģistrēts platformā.

Table: Vērtību ķēdes darbības (cilvēki) un tas, ko reģistrē platforma (dators) {#tbl-activities}

| Kompetence | Vērtību ķēdes darbības (cilvēki, ārpus datora) | Reģistrē platformā (datorizēti) |
|---|---|---|
| Biškopības saimniecība | Tur bites, ievāc medu, piegādā to uzņēmumam tvertnēs, nolasa uzņēmumam SMS kodu, nosaka prasīto cenu | Biškopja un dravas ieraksti; piegādes apstiprinājums (SMS kods); portālā: prasītā cena un paša biškopja piekrišanas |
| Biškopju reģistrācija | Tiekas ar biškopi, pārbauda personu apliecinošo dokumentu un reģistra ierakstu, nolasa viņam informatīvo paziņojumu | Biškopja un dravas ieraksti, reģistrācija (SMS kods vai parakstīts foto), reģistrācijā dotās piekrišanas, biškopja pieteikšanās dati |
| Biedrība | Sasaista un informē biedrus biškopjus, nolasa viņiem piekrišanas tekstu, savāc parakstus | Piekrišanas ieraksti; publiskās biškopju lapas |
| Pārstrāde | Pieņem piegādes tvertnēs, pārbauda vai uzliek plombas, no katras tvertnes piepilda paraugu burku un kontrolparauga burku, karsē, filtrē, sajauc un nostādina medu | Piegāde ar tās tvertnēm un plombu vēsturi, paraugu burkas, KKP 0 un KKP 2–6 dati |
| Iepakošana | Pilda, marķē un iepako medu, uzglabā to uzņēmumā | KKP 7–10 dati |
| Laboratorija | Saņem noplombētās burkas un pārbauda plombu, etiķeti un vāciņu; sagatavo kopparaugu; analizē medu; paraksta pārskatu | Pieņemšanas pārbaudes, kopparaugs, laboratorijas pārskats (PDF) un rezultāti (KKP 1) |
| Vairumtirdzniecība, Mazumtirdzniecība | Saņem preces, uzglabā tās, pārdod vai sūta tālāk | Sūtījuma apstiprinājums, uzglabāšanas ieraksts (KKP 10), jauni sūtījumi |
| Centrālā biedrība | Pārbauda biškopju biedrības | Biedrību pārbaude (ja tā konfigurēts) |
| Valsts iestāde | Veic uzraudzību | Lasa ierakstus |

Katrai organizācijai neatkarīgi no tās kompetencēm ir arī **atbilstības speciālists**: persona, kas uzrauga apstiprinājumus, pierādījumus un datu aizsardzību (skatīt [13. nodaļu](#role-co)).

## Organizāciju veidi {#org-types}

Platformā nav atsevišķa organizāciju veidu saraksta: organizācijas veidu nosaka tās kompetences. [Table](#tbl-org-types) uzskaita tipiskās organizācijas.

Table: Tipiskās organizācijas un to kompetences {#tbl-org-types}

| Organizācija | Kompetences | Tipiskās dalībnieku lomas |
|---|---|---|
| Novada lauksaimniecības pārvalde | **Biškopju reģistrācija** | Organizācijas administrators, Biškopju reģistrācijas speciālists, Atbilstības speciālists |
| Biškopju biedrība | **Biedrība** | Organizācijas administrators, Lauka inspektors, Atbilstības speciālists |
| Pārstrādes uzņēmums vai iepakotājs | **Pārstrāde**, **Iepakošana** un **Vairumtirdzniecība** vai **Mazumtirdzniecība** uzglabāšanai uzņēmumā | Organizācijas administrators, Darbinieks, Paraugu pieņemšanas speciālists, Kvalitātes nodrošināšanas speciālists, Atbilstības speciālists |
| Pārtikas analīžu laboratorija (akreditēta vai iekšējā) | **Laboratorija** | Organizācijas administrators, Analītiķis, Laboratorijas vadītājs, Atbilstības speciālists |
| Vairumtirgotājs | **Vairumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes nodrošināšanas speciālists, Atbilstības speciālists |
| Mazumtirgotājs | **Mazumtirdzniecība** | Organizācijas administrators, Darbinieks, Kvalitātes nodrošināšanas speciālists, Atbilstības speciālists |
| Ministrija vai uzraudzības iestāde | **Valsts iestāde** | Organizācijas administrators (tikai lasīšana) |

Biedrībai, kas pati uztur pārstrādes uzņēmumu, ir arī kompetence **Pārstrāde** (un **Iepakošana**), un tad arī uzņēmuma lomas. Mazā organizācijā atbilstības speciālists var būt organizācijas administrators ar otru dalību; četru acu noteikumi joprojām ir spēkā.

### Organizācijas statuss {#org-status}

Platformas administrators pārbauda, aptur un atjauno organizācijas. Pārbaudei nepieciešams vismaz viens augšupielādēts dokuments; apturēšanai – iemesls. Organizācijai, ko pārbauda pirmo reizi, vajadzīgs arī aktīvs **atbilstības speciālists**; organizācijai, kas pārbaudīta agrāk bez tāda, tiek rādīts brīdinājums ar datumu, līdz kuram tā jānosaka, un organizācija netiek bloķēta. [Table](#tbl-org-status) uzskaita statusus.

**Centrālā biedrība.** Ja platforma ir tā konfigurēta, pārbaudītas **Centrālās biedrības** organizācijas administrators pārbauda, aptur un atjauno biškopju biedrības un tad ir **vienīgais**, kas to dara: platformas administratori vairs nepārbauda biedrības, un biedrības pazūd no viņu gaidīšanas saraksta.

Table: Organizācijas statuss {#tbl-org-status}

| Statuss | Nozīme |
|---|---|
| **Gaida** | Jauna organizācija, kas gaida pārbaudi. Dalībnieki var lasīt datus, un organizācijas administrators var augšupielādēt dokumentus. Neviens nevar ievadīt datus. |
| **Pārbaudīta** | Organizāciju ir pārbaudījis platformas administrators (vai – biedrības – centrālā biedrība, ja tā konfigurēts). Dalībnieki var strādāt atbilstoši savām lomām. |
| **Apturēta** | Pārbaude atsaukta (ar norādītu iemeslu). Līdz atjaunošanai darbojas tāpat kā statuss "Gaida". |

## Lomas {#role-list}

[Table](#tbl-roles) ir lomu atsauces saraksts. Visas pārējās tabulas šajā rokasgrāmatā lieto tās pašas lomas tādā pašā secībā. Datu tiesību tabulās lietoti otrās kolonnas saīsinājumi.

Table: Lomas {#tbl-roles}

| Loma (ekrānā) | Saīs. | Kods | Tipiska persona | Galvenie darbi |
|---|---|---|---|---|
| **Organizācijas administrators** | OA | OrgAdmin | Organizācijas vadītājs | Augšupielādē dokumentus, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē biškopju piekrišanas (biedrībās), lasa piegādes, paraugus un organizācijai piederošo partiju prasīto cenu |
| **Darbinieks** | Da | Staff | Ražošanas, pildīšanas vai mazumtirdzniecības darbinieks | Ievada KKP 1–10 datus, pieprasa pabeigšanu (fiksēšanu), nosūta un apstiprina sūtījumus, reģistrē uzglabāšanu |
| **Lauka inspektors** | LI | FieldInspector | Biedrības lauka tehniķis | Lasa ierakstus; ievada KKP 0 tikai tad, ja administrators šo KKP piešķir biedrībai |
| **Paraugu pieņemšanas speciālists** | PPS | SampleAcceptanceOfficer | Pieņemšanas speciālists uzņēmumā | Reģistrē piegādes ar to noplombētajām tvertnēm, nosūta biškopim SMS kodu un to ievada, ņem paraugu no katras tvertnes, ievada KKP 0 |
| **Kvalitātes nodrošināšanas speciālists** | KNS | QualityApprover | Kvalitātes vadītājs, pārtikas inženieris | Apstiprina KKP ierakstus un uzglabāšanas ierakstus, apstiprina pabeigšanu (fiksēšanu) |
| **Analītiķis** | An | Analyst | Laboratorijas analītiķis | Saņem paraugus (plombas, etiķetes un vāciņa pārbaude), ievada laboratorijas rezultātus, iesniedz pārskatus (pirmā persona laboratorijā) |
| **Laboratorijas vadītājs** | LabV | LabManager | Laboratorijas vadītājs | Saņem paraugus (plombas, etiķetes un vāciņa pārbaude), apstiprina vai atgriež laboratorijas pārskatus (otrā persona laboratorijā) |
| **Biškopju reģistrācijas speciālists** | BRS | RegistrationOfficer | Novada lauksaimniecības pārvaldes darbinieks | Reģistrē biškopjus un viņu dravas, pārbauda reģistrācijas numuru, nosūta biškopim SMS kodu, apliecina identitāti, reģistrē paziņojumu un piekrišanas, izsniedz biškopja pieteikšanās datus |
| **Atbilstības speciālists** | AS | ComplianceOfficer | Jebkuras organizācijas kvalitātes vai datu aizsardzības atbildīgais | Pārskata parakstīto foto pierādījumus, paraugu ņemšanas kvalitāti un apstiprinājumus, uztur datu pārziņa datus, apstrādā datu subjektu pieprasījumus un piekrišanu atsaukumus, lasa organizācijas audita ierakstus |
| **Platformas administrators** | PA | (globāla loma) | Platformas uzturētājs | Administrēšana ([15. nodaļa](#admin)); lasa visus darba plūsmas datus, bet neko neievada un neapstiprina; var arī reģistrēt biškopjus |
| **Biškopis** (portāla konts) | Bi | (nav dalības) | Biškopis | Lasa savas dravas, piegādes, paraugus, laboratorijas rezultātus, gala partijas un Medus stāsta lapas; ievada sava medus prasīto cenu; dod un atsauc savas piekrišanas ([14. nodaļa](#portal)) |

Platformas administrators nepiedalās uzņēmējdarbības organizācijas darba plūsmā: viņš var lasīt katru ierakstu, taču **nevar ievadīt vai apstiprināt darba plūsmas datus** (piegādes, KKP datus, laboratorijas rezultātus, sūtījumus, piekrišanas). Ja dati pēc apstiprināšanas izrādās nepareizi, administrators pieprasa *labojumu*, ko apstiprina otrs administrators (skatīt [Labojumi](#admin-corrections)). Administratori gan uztur pamatdatus: biškopjus, dravas, partijas, gala partijas un to sastāvu (skatīt [15. nodaļu](#admin)).

## Datu tiesības (CRUD) {#crud}

[Table](#tbl-crud) parāda katras lomas noklusējuma datu tiesības katram ierakstu veidam pēc CRUD modeļa: **C** izveidot (create), **R** lasīt (read), **U** mainīt (update), **D** dzēst (delete) un **A** apstiprināt (atsevišķas tiesības). "–" nozīmē, ka tiesību nav. Bi ir biškopja portāla konts, Pub – publisks apmeklētājs bez konta. Dalībnieka tiesības attiecas tikai uz viņa organizācijas darba ierakstiem (tās partijām un gala partijām, gala partijām tās turējumā, tās biedrības biškopjiem, tās pārvaldes reģistrācijām) un, izņemot lasīšanu, tikai tik ilgi, kamēr organizācija ir pārbaudīta. Tiesības izriet no programmas koda un noklusējuma iestatījumiem; [Table](#tbl-admin-changeable) uzskaita, kuras no tām administrators var mainīt.

Table: Noklusējuma datu tiesības pa lomām (CRUD + apstiprināšana) {#tbl-crud}

| Ieraksts | OA | Da | LI | PPS | KNS | An | LabV | BRS | AS | PA | Bi | Pub |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Biškopis | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (sevi) | R ³ |
| Drava | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (savas) | R ³ |
| Biškopja reģistrācija | – | – | – | – | – | – | – | C R ¹⁸ | A ¹⁹ | C R | – | – |
| Pieteikšanās dati (akreditācijas dati) | – | – | – | – | – | – | – | C ²⁰ | – | C ²⁰ | – | – |
| Neapstrādāta medus partija | R | R | R | C R ⁴ | R | – | – | – | R | C R U D ⁵ | R (savas) | R ³ |
| Piegādes apstiprinājums (SMS kods vai parakstīts foto) | R | – | – | C R | – | – | – | – | R A ¹⁹ | R | A ²¹ | – |
| Tvertne un tās plombas | R | R ²⁹ | R ²⁹ | C R | R ²⁹ | R | R | – | R | R | R (savas) | – |
| Paraugs | R | – | – | C R | – | R A ⁶ | R A ⁶ | – | R | R | R (savi) | – |
| Laboratorijas pārskats un rezultāti | R | R | R | R | R | C R U D ⁷ | R A ⁷ | – | R | R | R (apstiprināti, savi) | R ⁸ |
| KKP ieraksts (dati, "Nav piemērojams") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | – | R | R ¹⁰ | – | R ³ |
| Gala partija | R, pieprasa pabeigšanu | R, pieprasa pabeigšanu | R | R | R, apstiprina pabeigšanu (A) | R | R | – | R | C R U D ¹¹ | R ²² | R ³ |
| Sūtījums | C R A ¹² | C R A ¹² | R ²⁹ | R ²⁹ | R | R ²⁹ | R ²⁹ | – | R | R | – | – |
| Uzglabāšanas ieraksts pie saņēmēja (KKP 10) | R | C R U D | R | R | R A | R | R | – | R | R | – | – |
| Partijas prasītā cena | R ²³ | – | – | – | – | – | – | – | R ²³ | R | C R ²³ | – ²⁴ |
| Biškopja piekrišana | C R U ¹³ | R | R | R | R | R | R | C R ²⁵ | R U ²⁵ | R | R U ²⁵ | R ¹⁴ |
| Informatīvais paziņojums (teksts, versija) | R | R | R | R | R | R | R | R | R | C R ²⁶ | R | R |
| Paziņojuma apstiprināšana | R (savu) | R (savu) | R (savu) | R (savu) | R (savu) | R (savu) | R (savu) | R (savu) | R (savu) | R | R (savu) | – |
| Datu pārziņa dati | R | R | R | R | R | R | R | R | C R U | C R U | R | R (paziņojumā) |
| Paraugu ņemšanas kvalitātes pārskats | – | – | – | – | – | – | – | – | C R ³⁰ | – | – | – |
| Datu subjekta pieprasījums | – | – | – | – | – | – | – | – | C R U ³⁰ | – | – | – |
| SMS ziņa | – | – | – | – | – | – | – | – | – | R ²⁷ | – | – |
| Organizācija (dati, dokumenti) | R; dokumenti C | R | R | R | R | R | R | R | R; dokumenti C | C R U, statuss A ¹⁵ | – | – |
| Dalība | R | R | R | R | R | R | R | R | R | C R U ¹⁶ | – | – |
| Audita ieraksts | – | – | – | – | – | – | – | – | R ²⁸ | R ¹⁷ | – | – |

Piezīmes:

1. Lasa tikai ar to ierakstu starpniecību, ar kuriem strādā organizācija (KKP lapas, atsaukšanas pārskats). Vēlāks gala partijas turētājs lasa datus bez biškopju vārdiem un dravu nosaukumiem. Biedrības dalībnieki lasa savu biedru biškopju sarakstu.
2. Biškopji un dravas nekad netiek dzēsti, tikai deaktivizēti. Dravu fotoattēlus var pievienot, mainīt un dzēst.
3. Pabeigtu (fiksētu) gala partiju publiskajās lapās (Medus stāsts) un publicētajās biškopju lapās: vārds tikai ar piekrišanu (citādi neitrāls kods), province un apstiprināti dati; nekad koordinātas vai kontaktinformācija (skatīt [Ko var lasīt sabiedrība](#trust-public)).
4. Partija tiek izveidota, reģistrējot piegādi, pārbaudītā organizācijā ar kompetenci **Pārstrāde**.
5. Administratori uztur partijas pamatdatus (piemēram, īpašnieci organizāciju un KKP piešķīrumu). Ar tvertnēm reģistrētas partijas piegādes faktus (datums, tvertnes, svari, plombu numuri) nevar rediģēt, arī administratori ne; tos maina tikai ar divu administratoru piegādes labojumu (skatīt [Labojumi](#admin-corrections)). Kad partija ir pabeigtā (fiksētā) gala partijā, neko vairs nevar mainīt; partiju, kurai ir izmantotas piegādes, paraugi vai KKP dati, nevar dzēst.
6. **A** = saņemt paraugu pēc plombas, etiķetes un vāciņa pārbaudes vai to noraidīt.
7. Analītiķis izveido, saglabā, maina un iesniedz pārskatu, kamēr tas ir melnraksts; laboratorijas vadītājs to apstiprina vai atgriež. Apstiprinātu pārskatu nevar mainīt.
8. Lapa **Pārbaudīt laboratorijas pārskatu**, pēc pārskata koda; tikai apstiprināti pārskati.
9. Ievadīt datus (C), mainīt vai notīrīt tos (U, D) un atzīmēt "Nav piemērojams", līdz ieraksts tiek apstiprināts vai bloķēts. Noklusējuma ievadītāju lomas: paraugu pieņemšanas speciālists KKP 0–1, darbinieks KKP 1–10. Lauka inspektors ievada KKP 0 tikai tad, ja administrators KKP 0 piešķir biedrībai.
10. Apstiprinātus vai bloķētus datus administratori maina tikai ar labojumu, ko pieprasa viens administrators un apstiprina otrs.
11. Izveidot gala partiju, noteikt tās sastāvu no partijām, piešķirt tās KKP un noteikt īpašnieku. Gala partiju var dzēst tikai pirms tās pabeigšanas (fiksēšanas). Administratori nevar pabeigt (fiksēt) gala partiju.
12. **C** = nosūtīt pabeigtu (fiksētu) gala partiju, kas ir jūsu organizācijas turējumā; **A** = apstiprināt vai noraidīt sūtījumu, ko saņem jūsu organizācija.
13. Reģistrēt (C) un atsaukt (U) piekrišanu publiskai biškopja lapai; tikai biedrībām.
14. Publicētā biškopja lapa.
15. Pārbaudīt, pieprasīt dokumentus, apturēt un atjaunot (skatīt [Organizācijas statuss](#org-status)). Organizācijas nekad netiek dzēstas.
16. Pievienot dalību un to atsaukt (U); dalības nekad netiek dzēstas.
17. Audita žurnālu var tikai papildināt; neviens nevar mainīt vai dzēst tā ierakstus.
18. Reģistrācijas speciālists reģistrē biškopjus un viņu dravas savai pārvaldei un lasa tikai tās reģistrācijas. Apliecinātie fakti (identitāte, reģistrācijas numurs, tālrunis) mainās tikai ar jaunu tā paša biškopja reģistrāciju (atkārtotu pārbaudi), kas atkal tiek apstiprināta.
19. **A** = pieņemt vai noraidīt pierādījumu, kas reģistrēts ar parakstītu foto SMS koda vietā: reģistrācijas foto vai piegādes foto. Lemj reģistrētājas organizācijas atbilstības speciālists, nekad ne persona, kas to reģistrēja. Platformas pašas reģistrācijām lemj platformas administrators.
20. Biškopja pieteikšanās datus SMS veidā nosūta reģistrācijas speciālists (kad biškopju portāls ir atvērts); dalībnieku pieteikšanās datus nosūta platformas administratori. Pēc nosūtīšanas paroli neviens neredz.
21. Biškopja **A** ir SMS kods, ko viņš nolasa paraugu pieņemšanas speciālistam; tas apstiprina piegādi ar tās tvertnēm, svariem un plombām.
22. Gala partijas, kurās ir paša biškopja medus: nosaukums, biškopja daļa, statuss un saite uz Medus stāstu. Nekad citu biškopju vārdi vai cenas.
23. Biškopis ievada prasīto cenu katrai piegādātajai partijai un var to mainīt (jauns ieraksts; vēsture saglabājas), līdz partija ir pabeigtā (fiksētā) gala partijā. To lasa partijas īpašnieces OA un AS un platformas administratori; biedrība – tikai tad, ja biškopis tam piekrita. Darbinieki, laboratorijas un vēlāki turētāji – nekad.
24. Publiskajās lapās un atsaukšanas pārskatā cena netiek rādīta.
25. Reģistrācijas speciālists reģistrē reģistrācijā dotās piekrišanas; biškopis portālā dod vai atsauc neobligātu piekrišanu; atbilstības speciālists reģistrē atsaukumu, kas saņemts papīra formā vai pa tālruni. Atsaukums stājas spēkā nekavējoties.
26. Paziņojumu tekstiem ir versijas, un tos nekad nerediģē: publicējot jaunu versiju, vecā tiek izbeigta.
27. Tikai testa nodrošinātāja izsūtne (izstrāde un demonstrācijas); īsts nodrošinātājs kodus vai paroles neglabā.
28. Atbilstības speciālista paša organizācijas audita ieraksti.
29. Tikai atsaukšanas pārskatā: lapai **Sūtījumi un krājumi** vajadzīgas sūtījumu lasīšanas tiesības.
30. Atbilstības speciālista paša sadaļa **Atbilstība** (skatīt [13. nodaļu](#role-co)).

**Kas lejupielādē pierādījumu failus.** Parakstītās piegādes veidlapas, reģistrācijas foto, piekrišanas dokumentus un organizācijas dokumentus lejupielādē tikai loma, kas tos reģistrē (paraugu pieņemšanas speciālists, reģistrācijas speciālists vai – piekrišanām – biedrības organizācijas administrators), organizācijas atbilstības speciālists un organizācijas administrators, kā arī platformas administratori. Cita organizācijas loma saņem "Jūs nevarat atvērt šo lapu" (403); cita organizācija – "Šīs lapas šeit nav" (404). Katra lejupielāde tiek ierakstīta audita žurnālā.

**Ko var dzēst.** Dalībnieki notīra tikai savus datus, pirms tie ir apstiprināti (KKP dati, laboratorijas rezultātu melnraksti, uzglabāšanas dati). Administratori var dzēst partijas un gala partijas pirms pabeigšanas (fiksēšanas), dravu fotoattēlus, atsauces datus un lietotāju kontus (nekad savu). Biškopji, dravas, organizācijas, dokumenti, dalības, piekrišanas, reģistrācijas, tvertnes, plombu notikumi, paraugi, laboratorijas pārskati, sūtījumi, cenu ieraksti, SMS ziņas, paziņojumi, apstiprinājumi un audita ieraksti nekad netiek dzēsti.

## Atļauju matrica {#permission-matrix}

[Table](#tbl-permissions) parāda noklusējuma tiesības katrai **darbībai platformā** (funkcijai, piemēram, "reģistrēt piegādi"), kā tās ir konfigurētas pašlaik. Tā papildina datu tiesības ([Table](#tbl-crud)) un lieto tās pašas lomas tādā pašā secībā. Apzīmējumi: **R** lasīt, **C** izveidot vai ievadīt, **A** apstiprināt (vai saņemt, vai pieņemt pierādījumu), **F** pabeigt (fiksēt) (pieprasīt vai apstiprināt), **–** nav tiesību. Kolonnā "Nepieciešamā kompetence" norādīts, kāda kompetence nepieciešama organizācijai.

Table: Noklusējuma tiesības pa darbībām platformā un lomām {#tbl-permissions}

| Darbība platformā | Nepieciešamā kompetence | OA | Da | LI | PPS | KNS | An | LabV | BRS | AS | PA |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Reģistrēt biškopi un dravas | Biškopju reģistrācija | – | – | – | – | – | – | – | C R | – | C R |
| Nosūtīt biškopja pieteikšanās datus SMS veidā | Biškopju reģistrācija | – | – | – | – | – | – | – | C | – | C |
| Reģistrēt piegādi ar tvertnēm, nosūtīt SMS kodu | Pārstrāde | R | – | – | C R | – | – | – | – | R | R |
| Ņemt paraugus (no katras tvertnes) | Pārstrāde (partijas īpašnieks) | R | – | – | C R | – | R | R | – | R | R |
| Pieņemt vai noraidīt parakstītu foto (reģistrācija, piegāde) | sava organizācija | – | – | – | – | – | – | – | – | A | A ¹ |
| Saņemt paraugu (plomba, etiķete, vāciņš) | Laboratorija | – | – | – | – | – | A | A | – | R | R |
| Ievadīt laboratorijas rezultātus, iesniegt pārskatu | Laboratorija | R | R | R | R | R | C R | R | – | R | R |
| Apstiprināt vai atgriezt laboratorijas pārskatu | Laboratorija | R | R | R | R | R | R | A R | – | R | R |
| KKP 0 pieņemšanas pārbaude (partija) | Pārstrāde (partijas īpašnieks) | R | R | R | C R | A R | R | R | – | R | R |
| KKP 1 laboratorijas dati (partija) | tiek ierakstīti no apstiprinātā laboratorijas pārskata | R | R | R | R | R | R | R | – | R | R |
| KKP 2–3 (partija) | Pārstrāde (partijas īpašnieks) | R | C R | R | R | A R | R | R | – | R | R |
| Izveidot gala partiju, noteikt tās sastāvu, piešķirt KKP | – | – | – | – | – | – | – | – | – | – | C |
| KKP 4–6 (gala partija) | Pārstrāde (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | – | R | R |
| KKP 7–9 (gala partija) | Iepakošana (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | – | R | R |
| KKP 10 uzglabāšana uzņēmumā (gala partija) | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana (gala partijas īpašnieks) | R | C R | R | R | A R | R | R | – | R | R |
| Pieprasīt pabeigšanu (fiksēšanu) | Pārstrāde vai Iepakošana (īpašnieks), pārbaudīts | F | F | – | – | – | – | – | – | – | – |
| Apstiprināt pabeigšanu (fiksēt) | Pārstrāde vai Iepakošana (īpašnieks), pārbaudīts | – | – | – | – | F | – | – | – | – | – |
| Izveidot sūtījumu | Pārstrāde, Iepakošana, Vairumtirdzniecība vai Mazumtirdzniecība (turētājs), pārbaudīts | C R | C R | – | – | R | – | – | – | R | R |
| Apstiprināt vai noraidīt saņemtu sūtījumu | Pārstrāde, Iepakošana, Vairumtirdzniecība vai Mazumtirdzniecība (saņēmējs), pārbaudīts | A R | A R | – | – | R | – | – | – | R | R |
| KKP 10 uzglabāšanas ieraksts pie saņēmēja | Vairumtirdzniecība, Mazumtirdzniecība vai Iepakošana | R | C R | R | R | A R | R | R | – | R | R |
| Atsaukšanas pārskats (savā tvērumā) | – | R | R | R | R | R | R | R | – | R | R (visi) |
| Lasīt piederošo partiju prasīto cenu | Pārstrāde (partijas īpašnieks) | R | – | – | – | – | – | – | – | R | R |
| Reģistrēt vai atsaukt biškopja piekrišanu (biškopja lapa) | Biedrība | C R | – | – | – | – | – | – | – | – | R |
| Atbilstība: pārskati, datu subjektu pieprasījumi, apstiprinājumu pārbaudes | sava organizācija | – | – | – | – | – | – | – | – | C R | – |
| Uzturēt datu pārziņa datus | sava organizācija | R | – | – | – | – | – | – | – | C R | C R |
| Lasīt organizācijas audita ierakstus | sava organizācija | – | – | – | – | – | – | – | – | R | R (visi) |
| Augšupielādēt organizācijas dokumentus | sava organizācija | C | – | – | – | – | – | – | – | C | C |
| Publicēt paziņojumu tekstus | – | – | – | – | – | – | – | – | – | – | C |
| Pārbaudīt organizācijas, pārvaldīt dalības un kompetences | – | – | – | – | – | – | – | – | – | – | C A ² |
| Pārbaudīt biškopju reģistrācijas numurus | – | – | – | – | – | – | – | – | – | – | A |
| Mainīt atļaujas, KKP, robežvērtības, tulkojumus | – | – | – | – | – | – | – | – | – | – | C |
| Pieprasīt vai izlemt labojumu (KKP dati, piegādes fakti) | – | – | – | – | – | – | – | – | – | – | C A (divi dažādi administratori) |
| Audita žurnāls, datu glabāšanas pārskats | – | – | – | – | – | – | – | – | – | – | R |

1. Platformas administratoru reģistrētajām reģistrācijām.
2. Biedrības tā vietā pārbauda centrālā biedrība, ja platforma ir tā konfigurēta.

Piezīmes:

- **KKP lasīšana (R)**: katra loma var atvērt to partiju un gala partiju KKP, kuros piedalās tās organizācija: kā īpašnieks vai atbildīgā organizācija, kā organizācija, kas paņēma paraugu (partijām), kā vēlāks gala partijas turētājs vai kā pārbaudīta valsts iestāde. Turētājs gala partijas un tās partiju KKP lasa **tikai lasīšanas režīmā**. Jebkurai citai organizācijai tiek parādīta lapa "Jūs nevarat atvērt šo lapu" (403).
- **KKP 1** tiek aizpildīts no apstiprinātā laboratorijas pārskata. Tam konfigurētās ievadītāju lomas (paraugu pieņemšanas speciālists, darbinieks) tiek izmantotas tikai tiem KKP 1 parametriem, kurus laboratorijas pārskats neaptver.
- **Lapas pārbauda lasīšanas tiesības, ne tikai izvēlni.** Loma bez lapas lasīšanas tiesībām (piemēram, **Sūtījumi un krājumi** vai **Biškopju lapas**) tiek novirzīta atpakaļ uz **Mans darbs** ar paziņojumu, kurā nosauktas lomas, kas to drīkst atvērt.
- Administrators var piešķirt **KKP 0** biškopja biedrībai, nevis uzņēmumam (**KKP atbildība** → KKP 0 → **Partijas biškopja biedrība**, kompetence "Biedrība", loma "Lauka inspektors"). Tas nav noklusējuma iestatījums.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Lomas un atļaujas**: lomas Darbinieks noklusējuma tiesības.

### Ko administrators var mainīt {#admin-changeable}

[Table](#tbl-admin-changeable) uzskaita katru iestatījumu, ko platformas administrators var mainīt panelī. Viss pārējais ir nemainīgs: lomu saraksts, kompetenču saraksts un [Pamatnoteikumi](#fixed-rules). Dažus slēdžus tā vietā iestata servera konfigurācijā (skatīt [Iestatījumi servera konfigurācijā](#admin-settings)).

Table: Ko platformas administrators var mainīt {#tbl-admin-changeable}

| Iestatījums | Kur | Ko tieši var mainīt |
|---|---|---|
| Lomu atļaujas | **Lomas un atļaujas** → **Lomu atļaujas** | Katrai lomai: **Ievadīt**, **Apstiprināt** un **Lasīt** katram KKP; **Ievadīt** un **Lasīt** piegādēm (pieņemšanai); **Ievadīt**, **Apstiprināt** (saņemt) un **Lasīt** paraugiem; **Ievadīt**, **Apstiprināt** un **Lasīt** laboratorijas rezultātiem; **Ievadīt** (nosūtīt), **Apstiprināt** (apstiprināt saņemšanu) un **Lasīt** sūtījumiem; **Ievadīt** un **Lasīt** biškopju lapām; **Lasīt** atsaukšanas pārskatam; **Ievadīt** (pieprasīt) un **Apstiprināt** (apstiprināt) pabeigšanai (fiksēšanai); **Ievadīt** un **Lasīt** biškopju reģistrācijai; **Apstiprināt** pierādījumiem; **Lasīt** cenām un organizācijas audita ierakstiem. Katrām tiesībām paliek vismaz viena loma. |
| KKP atbildība | **KKP atbildība** | Katram KKP: atbildīgā organizācija (partijas vai gala partijas īpašniece vai – partijām – biškopja biedrība), tai nepieciešamās kompetences, ievadītāju lomas, apstiprinātāju lomas un tas, vai apstiprinājums ir obligāts. |
| Kompetences | Organizācijas lapa; **Organizācijas kompetences** | Pievienot vai noņemt organizācijas kompetences (vismaz viena paliek); laboratorijas akreditācijas numurs un tā derīguma termiņš. |
| Četru acu principa izņēmums | Organizācijas lapa | Katrai organizācijai: ļauj tās dalībniekiem apstiprināt savus KKP ierakstus un uzglabāšanas ierakstus un apstiprināt savu pabeigšanas (fiksēšanas) pieprasījumu; audita žurnālā tas vienmēr tiek atzīmēts. Nekad – laboratorijas rezultātiem vai pierādījumiem. |
| Organizācijas statuss | Organizācijas lapa | Pārbaudīt, pieprasīt dokumentus, apturēt, atjaunot. |
| Datu pārziņa dati | Organizācijas lapa | Organizācijas kā datu pārziņa juridiskais nosaukums, kontakts un adrese (tos uztur arī tās atbilstības speciālists). |
| Dalības un lietotāji | Organizācijas lapa; **Dalībnieki**; **Lietotāju Pārvaldība** | Pievienot un atsaukt dalības; izveidot lietotājus ar pieteikšanās datiem SMS veidā vai ar sākotnējo paroli; nosūtīt jaunus pieteikšanās datus; atbloķēt un dzēst kontus. |
| Paziņojumi un piekrišanas teksti | **Paziņojumi un piekrišanas teksti** | Publicēt jaunu paziņojuma vai piekrišanas teksta versiju ar melnraksta atzīmi, līdz to apstiprina datu aizsardzības birojs. |
| Kontroles punkti, parametri, robežvērtības | **Kritiskie Kontroles Punkti** | Parametri, datu tipi, obligātie jautājumi, robežvērtības, "Robežvērtības pārbaudītas". |
| Pamatdati un atsauces dati | Administrēšanas izvēlnes | Biškopji, dravas, reģistrācijas numuri, partijas, gala partijas, medus veidi, periodi, tulkojumi. |

## Pamatnoteikumi {#fixed-rules}

Šie noteikumi ir iestrādāti platformā; neviens iestatījums tos neizslēdz:

- **Datus ievada tikai pārbaudītas organizācijas.** Organizācijai, ko pārbauda pirmo reizi, vajadzīgs atbilstības speciālists.
- **Četru acu princips KKP ierakstiem**: neviens nevar apstiprināt KKP ierakstu, kurā pats ir reģistrējis datus vai kuru pats ir atzīmējis kā "Nav piemērojams". Izņēmums: platformas administrators var piešķirt organizācijai izņēmumu (**Četru acu principa izņēmums**); tad dalībnieki drīkst apstiprināt savus KKP ierakstus, un katrs šāds apstiprinājums tiek atzīmēts audita žurnālā.
- **Četru acu princips laboratorijas rezultātiem – vienmēr**: persona, kas ievadīja rezultātus, nevar tos apstiprināt. Izņēmumu nav.
- **Četru acu princips pierādījumiem – vienmēr**: parakstītu foto, kas aizstāj SMS kodu (reģistrācijā vai piegādē), pieņem vai noraida organizācijas atbilstības speciālists, nekad ne persona, kas to reģistrēja.
- **Četru acu princips pabeigšanai (fiksēšanai) un uzglabāšanas ierakstiem**: persona, kas pieprasīja pabeigšanu (fiksēšanu), nevar to apstiprināt, un persona, kas reģistrēja uzglabāšanas ierakstu, nevar to apstiprināt (ja vien organizācijai nav piešķirts izņēmums).
- **Kods apstiprina tieši to, kam tas nosūtīts**: biškopja SMS kods ir piesaistīts reģistrācijai vai piegādei (tvertnes, svari, medus veids, paraugi, katrs plombas numurs); jebkuras izmaiņas pēc nosūtīšanas kodu padara nederīgu.
- **Piegādes fakti nemainās**: tvertnes un plombu notikumus var tikai papildināt; apstiprinātas piegādes faktus maina tikai ar divu administratoru labojumu, un biškopis par to tiek informēts ar SMS.
- **Paraugs no katras tvertnes**: katrai tvertnei ir sava noplombēta paraugu burka un kontrolparaugs ar drošības vāciņu; laboratorija pārbauda plombu, etiķeti un vāciņu un noraida burku, ja kāda pārbaude neizdodas.
- **Jebkuras izmaiņas noņem apstiprinājumu**: mainot datus, esošais apstiprinājums tiek noņemts; dati ārpus robežvērtībām apstiprināšanas brīdī ir skaidri jāapstiprina.
- **Laboratorijas rezultāti seko paraugam**: rezultātus ievada tikai saņemtam paraugam, kura pārbaudes ir izturētas, un tikai pēc tam, kad ir apstiprināts partijas KKP 0; apstiprinātu pārskatu nevar mainīt.
- **Platformas administratori darba plūsmas datus tikai lasa** un labo tos tikai ar divu administratoru labojumu procesu.
- **Pabeigts (fiksēts) nozīmē bloķēts**: gala partiju var pabeigt (fiksēt) tikai tad, kad izpildīti visi gala partijas un tās partiju KKP; pabeigtu (fiksētu) gala partiju un visas tās partijas vairs nevar mainīt, izņemot ar labojumu. Pabeigšanu (fiksēšanu) atcelt nevar.
- **Sūtījumi**: tikai pabeigtas (fiksētas) gala partijas, tikai citai pārbaudītai organizācijai, ne vairāk par turējumā esošo daudzumu.
- **Organizācijas konfigurē tikai administratori**: kompetences, dalības un izņēmumu.
- **Katrām tiesībām paliek vismaz viena loma.**
- **Audita žurnālu var tikai papildināt**; tajā nekas netiek mainīts vai dzēsts, un arī katra izmaiņa atļauju lapās tiek reģistrēta žurnālā.
- **Cenas pieder biškopim**: prasīto cenu ievada tikai biškopis; publiskajās lapās tā nekad netiek rādīta.
- **Biškopja konts redz tikai sava biškopja datus** un var atvērt tikai biškopju portālu.
- **Publiskiem vārdiem vajadzīga piekrišana**: publiskajās lapās biškopis tiek nosaukts vārdā tikai ar spēkā esošu piekrišanu vārda rādīšanai; citādi tiek rādīts neitrāls kods.
- **Biškopju lapas** ir publiskas tikai ar spēkā esošu piekrišanu, pārbaudītu reģistrācijas numuru un pārbaudītu biedrību.

## Vairākas dalības {#memberships}

**Dalība** ir viens lietotāja konts × viena organizācija × viena loma. Vienai personai var būt vairākas dalības: dažādās organizācijās vai vairākas lomas vienā organizācijā (piemēram, **Kvalitātes nodrošināšanas speciālists** un **Organizācijas administrators** biedrībā vai **Organizācijas administrators** un **Atbilstības speciālists** mazā organizācijā). Vienlaikus jūs vienmēr strādājat vienā dalībā, un tiesības netiek apvienotas: divas lomas nekad neļauj apstiprināt datus, ko esat ievadījis pats.

Lai pārslēgtos:

1. Noklikšķiniet uz **organizācijas · lomas žetona** augšējā joslā (skatīt [Augšējā josla un jūsu loma](#top-bar)). Sarakstā ar virsrakstu **Jūsu lomas** redzamas visas aktīvās dalības; pašreizējā ir atzīmēta.
2. Noklikšķiniet uz citas dalības. Platforma atver lapu **Mana organizācija** jaunajā dalībā.

Izvēlne, uzdevumi sadaļā **Mans darbs**, ieraksti, kurus varat lasīt, un atļautās darbības atbilst aktīvajai dalībai. Izvēle ir spēkā, līdz aizverat pārlūku; pēc tam jūs atkal sākat savā pirmajā dalībā (alfabētiskā secībā pēc organizācijas nosaukuma). Ja jums ir divas vai vairākas dalības, lapā **Mana organizācija** ir arī saraksts **Rīkoties kā** ar pogu **Pārslēgt**.

Biškopja portāla kontam nav dalības un nav žetona; tas atver tikai biškopju portālu.
