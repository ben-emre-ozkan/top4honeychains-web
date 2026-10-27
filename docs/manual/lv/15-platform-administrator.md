# 12. Platformas administrators {#admin}

## Jūsu ikdienas darbs {#admin-daily}

Platformas administratori uztur platformas darbību. Viņi pārbauda organizācijas, pārvalda dalības un atļaujas, uztur atsauces datus un kontroles punktu definīcijas, reģistrē biškopjus, izveido gala partijas (pagaidām), izlemj labojumus un seko audita žurnālam.

Administratori var lasīt katru ierakstu, taču **neievada un neapstiprina uzņēmējdarbības datus**. Pēc pieteikšanās viņi nonāk lapā **Mans darbs**, kurā redzamas grupas **Pārbaudāmās organizācijas**, **Pārbaudāmie reģistrācijas numuri** un **Izlemjamie labojumi**. Lapa **Informācijas Panelis** sniedz pārskatu (skaitļi, ķēde, biškopju un dravu karte pa provincēm, nesenās darbības un gaidošie apstiprinājumi).

<!-- SHOT: admin-dashboard -->
Figure: Platformas administratora informācijas panelis.

Administrēšanas izvēlne ([Table](#tbl-admin-menu)):

Table: Administrēšanas izvēlne {#tbl-admin-menu}

| Izvēlne | Vienumi |
|---|---|
| **Informācijas Panelis** | Pārskats |
| **Biškopji** | Biškopju saraksts, dravas, izglītības līmeņi, biškopības mērķa veidi, biškopības veidi |
| **Neapstrādāta Medus Ražošana** | Neapstrādāta medus partijas, medus veidi, ražošanas periodi |
| **Vairumtirgotāji** | Vairumtirgotāju saraksts, vairumtirgotāja pievienošana |
| **Gala Medus Ražošanas** | Gala partijas |
| **Kritiskie Kontroles Punkti** | KKP, parametru kategorijas, parametri, atsauces datu tulkojumi |
| **Lietotāju Pārvaldība** | Lietotāji, lomas, **Audita žurnāls** |
| **Organizācijas** | Mans darbs, organizācijas, kompetences, reģistrs, lomas un atļaujas, KKP atbildība, labojumi, sūtījumi, paraugi, biškopju lapas |

## Organizācijas {#admin-organizations}

### Organizācijas izveide {#admin-org-create}

1. **Organizācijas** → **Jauna organizācija**.
2. Ievadiet nosaukumu, juridisko nosaukumu, nodokļu maksātāja numuru, provinci un rajonu.
3. Ja organizācija ir vairumtirgotājs, kas jau ir platformā, izvēlieties to esošā vairumtirgotāja laukā. **Pārstrādes uzņēmumam jābūt piesaistītam savam vairumtirgotāja ierakstam**: tā saņemtās partijas tiek reģistrētas šī vairumtirgotāja vārdā, un gala partijā var izmantot tikai sava vairumtirgotāja partijas. Saiti var iestatīt arī vēlāk organizācijas lapā **Rediģēt**. Partijas, ko uzņēmums saņēma pirms piesaistes, izmanto tā vairumtirgotāja gala partijas, kuram uzņēmums ir piesaistīts.
4. Atzīmējiet vismaz vienu kompetenci un noklikšķiniet uz **Izveidot**.

Organizācijai sākotnēji ir statuss **Gaida**: "Organizācija izveidota. Tās statuss paliek "Gaida", līdz tā tiek pārbaudīta."

<!-- SHOT: admin-org-create -->
Figure: Organizācijas izveide: saite ar vairumtirgotāju un kompetences.

### Pārbaude, dokumentu pieprasīšana, apturēšana {#admin-org-verify}

Organizācijas lapā (platformas administrators; biškopju biedrībām arī pārbaudītas centrālās biedrības organizācijas administrators, ja platforma ir tā konfigurēta):

- **Pārbaudīt** (no statusa "Gaida") – nepieciešams vismaz viens augšupielādēts dokuments.
- **Pieprasīt dokumentus** (kamēr statuss ir "Gaida") – nepieciešams iemesls, piemēram, kurš dokuments trūkst.
- **Apturēt** (kamēr statuss ir "Pārbaudīta") – nepieciešams iemesls. **Atjaunot** atkal padara apturētu organizāciju par pārbaudītu.

Katra izmaiņa parādās pārbaužu vēsturē un audita žurnālā.

### Kompetences {#admin-org-capabilities}

**Pievienot kompetenci** pievieno kompetenci. Kompetencei **Laboratorija** atzīmējiet akreditāciju un ievadiet akreditācijas numuru un tā derīguma termiņu; akreditētai laboratorijai numurs ir obligāts. **Noņemt** noņem kompetenci; organizācijai paliek vismaz viena kompetence. Lapā **Kompetences** katrai kompetencei redzams, kuriem KKP tā nepieciešama un kurām organizācijām tā ir. Cilnē **Organizācijas kompetences** lapā **Lomas un atļaujas** var rediģēt visas organizācijas vienlaikus.

### Dalībnieki {#admin-members}

1. Organizācijas lapā, sadaļā ar dalībniekiem, izvēlieties lomu.
2. Ievadiet esoša lietotāja lietotājvārdu vai arī jaunu lietotājvārdu, e-pasta adresi un paroli, lai vienlaikus izveidotu lietotāju.
3. Noklikšķiniet uz **Pievienot dalībnieku**: "… pievienots kā …".

Dalība ir viens lietotāja konts × viena organizācija × viena loma; lietotājam var būt vairākas dalības. **Atsaukt** izbeidz dalību un izraksta lietotāju visās ierīcēs. Cilnē **Dalībnieki** lapā **Lomas un atļaujas** uzskaitītas visu organizāciju visas dalības.

<!-- SHOT: admin-org-members -->
Figure: Organizācijas dalības un veidlapa **Pievienot dalībnieku**.

### Četru acu principa izņēmums {#admin-four-eyes}

Mazas organizācijas ar vienu kvalitātes speciālistu var saņemt izņēmumu (**Četru acu principa izņēmums**): tad dalībnieki drīkst apstiprināt savus KKP ierakstus un apstiprināt savus pabeigšanas (fiksēšanas) pieprasījumus. Katrs šāds apstiprinājums tiek atzīmēts audita žurnālā. Izņēmums nekad neattiecas uz laboratorijas rezultātiem.

## Lietotāji {#admin-users}

Sadaļā **Lietotāju Pārvaldība** → **Lietotāji** uzskaitīti konti.

- **Pievienot jaunu lietotāju** izveido kontu. Organizāciju modelī atstājiet lauku **Globālā loma** tukšu organizāciju dalībniekiem un piešķiriet viņiem dalību organizācijas lapā; vērtību **Platformas administrators** izvēlieties tikai administratoriem.
- **Atiestatīt paroli** iestata lietotājam jaunu paroli.
- **Atbloķēt** parādās blakus kontam ar statusu **Bloķēts** un uzreiz beidz bloķēšanu ("Konts atbloķēts."). Bez tā bloķēšana beidzas pati pēc 15 minūtēm.
- **Dzēst** noņem kontu (lietojiet piesardzīgi; darbību nevar atsaukt). Savu kontu dzēst nevar. Katra dzēšana un katra atbloķēšana tiek ierakstīta audita žurnālā.

Parolei jābūt vismaz 6 rakstzīmes garai. Pēc 5 neveiksmīgiem pieteikšanās mēģinājumiem konts tiek bloķēts uz 15 minūtēm.

Sadaļā **Lietotāju Pārvaldība** → **Lomas** uzskaitītas globālās lomas. Lomas, uz kurām balstās platforma (**Administrators** un **Biškopis**), ir atzīmētas kā **Aizsargāta**, un tās nevar pārdēvēt vai dzēst.

## Lomas un atļaujas {#admin-permissions}

Lapā **Lomas un atļaujas** ir trīs cilnes:

- **Dalībnieki**: visas aktīvās dalības; pievienošana un atsaukšana.
- **Lomu atļaujas**: izvēlieties lomu un katrai funkcijai atzīmējiet **Ievadīt**, **Apstiprināt** un **Lasīt**. Funkcijas ir KKP (katram sava rinda) un jomas: pieņemšana, paraugi, laboratorijas rezultāti, sūtījumi, biškopju lapas, atsaukšana un pabeigšana (fiksēšana). Katrām tiesībām jāpaliek vismaz vienai lomai. Izmaiņas stājas spēkā nekavējoties.
- **Organizācijas kompetences**: kompetences katrai organizācijai.

Lodziņā "Pamatnoteikumi (tos nevar izslēgt):" uzskaitīti noteikumi, kurus nemaina neviens iestatījums (skatīt [Pamatnoteikumi](#fixed-rules)). [Table](#tbl-admin-changeable) precīzi uzskaita, ko administrators var mainīt.

<!-- SHOT: admin-role-permissions -->
Figure: Kvalitātes nodrošināšanas speciālista lomas atļaujas.

## KKP atbildība {#admin-ccp-responsibilities}

Lapā **KKP atbildība** katram KKP nosaka, kura organizācija ir atbildīga (partijas vai gala partijas īpašniece vai – partijām – biškopja biedrība), kāda kompetence tai nepieciešama, kuras lomas ievada un kuras apstiprina vērtības un vai apstiprinājums ir obligāts. Piemērs: lai piešķirtu KKP 0 biedrībai, izvēlieties "Partijas biškopja biedrība", **Biedrība** un **Lauka inspektors**.

## Kontroles punkti, parametri un robežvērtības {#admin-ccps}

- **Kritiskie kontroles punkti**: kods, nosaukums, apstrādes solis, kritiskuma pamatojums un **Attiecas uz** (neapstrādāta medus partija vai gala partija). Iespējai rādīt brīva teksta vērtības publiskajā Medus stāstā jāpaliek izslēgtai, ja atbildēs var būt personas dati.
- **Parametri**: nosaukums, mērvienība, kategorija, datu tips (**Skaitlis**, **Jā / Nē**, **Teksts**, **Fails**), minimums un maksimums, obligāts vai neobligāts, kā arī nosacījums "obligāts tikai tad", ja uz citu jā/nē jautājumu sniegta noteikta atbilde.
- **Robežvērtības**: katram parametram noklusējuma robežvērtības visiem medus veidiem un īpašas robežvērtības pēc medus klases (ziedu, lapu, priežu) vai medus veida, pēc izvēles ar nosacījumu, summu vai attiecību. Mainot robežvērtības, saglabātie rezultāti netiek pārrēķināti.

**Pārbaudītas robežvērtības.** Katram parametram ir izvēles rūtiņa **Robežvērtības pārbaudītas (tiek rādītas publiski)**. Tikai pārbaudītas robežvērtības Medus stāstā un biškopja lapā dod publisku spriedumu "robežās / ārpus robežām". Nepārbaudītas robežvērtības panelī ir atzīmētas ar "jāpārbauda", un publiskajās lapās norādīts, ka robežvērtības vēl tiek pārbaudītas.

<!-- SHOT: admin-parameter-limits -->
Figure: Parametra robežvērtības (KKP 1, mitrums).

## Atsauces dati un tulkojumi {#admin-reference}

Medus veidi (ar to medus klasi robežvērtību noteikšanai), ražošanas periodi, izglītības līmeņi, biškopības veidi un mērķi, kā arī vairumtirgotāji tiek uzturēti attiecīgajās izvēlnēs. Izmantotu ierakstu nevar dzēst.

Lapā **Atsauces datu tulkojumi** glabājas nosaukumi, kas sarakstos, tabulās un publiskajās lapās redzami piecās valodās: medus veidi, periodi, statusi, kompetences, lomas, KKP, parametri, parametru kategorijas, piezīmes par robežvērtībām, parametru skaidrojumi un valstis. Ja lauks ir tukšs, tiek izmantots angļu teksts, bet, ja arī tā nav, – pamatteksts.

## Biškopji, dravas un reģistrs {#admin-registry}

- **Biškopji**: pievienojiet biškopi (vārds, dzimšanas datums, pieredze, tālrunis, izglītība, biškopības veids un mērķis, pēc izvēles fotoattēls). Saimniecības numurs tiek ģenerēts automātiski. Biškopji nepiesakās platformā. Viņi nekad netiek dzēsti, tikai deaktivizēti.
- **Dravas**: nosaukums, stropu skaits un atrašanās vieta (platums un garums). Atrašanās vieta nekad netiek rādīta publiski; publiskajās lapās redzama tikai province.
- **Biškopju reģistrs un biedrības**: katram biškopim piesaistiet biedrību un ievadiet saimniecības reģistrācijas numuru (formāts: `TR` + provinces kods + 10 cipari, piemēram, `TR061234567890`) ar apliecinošu dokumentu. Pēc tam izvēlieties **Pārbaudīt** vai **Noraidīt** (noraidīšanai jānorāda iemesls).

<!-- SHOT: admin-registry -->
Figure: Biškopju reģistrs un biedrība.

## Gala partijas (pagaidām tikai administratoriem) {#admin-batches}

1. **Vairumtirgotāji** → vairumtirgotājs → **Gala medus ražošana (gala partijas)** → **Jauna gala partija**.
2. Aizpildiet laukus **Gala partijas nosaukums** un **Ražošanas datums** un noklikšķiniet uz **Izveidot gala partiju**. Tiek ģenerēts QR kods.
3. Lapā **Pārvaldīt gala partiju** ievadiet daudzumu, kas jāizmanto no katras piemērotās partijas (šī vairumtirgotāja partijas, kuru visi KKP ir apstiprināti), un noklikšķiniet uz **Saglabāt gala partijas sastāvu**: "Gala partijas sastāvs saglabāts: kopā … kg." Ja daudzums ir pārāk liels vai partiju nevar izmantot, lapa uzskaita katru problēmu (piemēram, "Partija #…: pieejami ne vairāk kā … kg.") un neko nesaglabā. Ja uzņēmumam ir partijas, kuras nevar izmantot neviena gala partija, jo uzņēmums nav piesaistīts vairumtirgotāja ierakstam, lapa to nosauc un sniedz saiti uz tā lapu **Rediģēt**.
4. Ar **Rediģēt** gala partijai nosakiet īpašnieci organizāciju (uzņēmumu).
5. **Pārvaldīt KKP**: atzīmējiet no KKP 4 līdz KKP 10 un noklikšķiniet uz **Saglabāt piešķīrumus**: "KKP piešķīrumi saglabāti: pievienoti …, noņemti …."

Pēc tam uzņēmums ievada gala partijas KKP un pabeidz (fiksē) gala partiju. Administratori nevar pabeigt gala partiju: "Organizāciju modelī gala partiju pabeidz (fiksē) īpašniece organizācija (pieprasījums un apstiprinājums, ko veic divas personas)."

<!-- SHOT: admin-manage-batch -->
Figure: Gala partijas sastāva veidošana no piemērotām partijām.

## Labojumi {#admin-corrections}

Apstiprinātus vai bloķētus datus var mainīt tikai ar labojumu:

1. Atveriet ieraksta KKP lapu. Lodziņā **Labojumi** izvēlieties parametru, ievadiet jauno vērtību un iemeslu (**iemesls**) un noklikšķiniet uz **Pieprasīt**: "Labojums pieprasīts; tas jāapstiprina citam platformas administratoram."
2. **Otrs** administrators atver lapu **Labojumi**, izlasa veco un jauno vērtību un iemeslu un noklikšķina uz **Apstiprināt un piemērot** vai **Noraidīt** (noraidīšanai jānorāda piezīme).

Persona, kas pieprasīja labojumu, nevar to izlemt. Labojumi attiecas arī uz bloķētiem ierakstiem, un abi soļi tiek reģistrēti audita žurnālā.

## Biškopju lapas, sūtījumi un paraugi {#admin-overviews}

Administratori var atvērt lapas **Biškopju lapas**, **Sūtījumi un krājumi** un **Paraugi un laboratorija** tikai lasīšanas režīmā, lai sekotu visu organizāciju darbam.

## Atsaukšana {#admin-recall}

Gala partijas vai partijas **Atsaukšanas pārskats** uzskaita:

- **augšupējo ķēdi**: partijas, dravas, biškopjus un piegādes veidlapas;
- gala partijas;
- **lejupējo ķēdi**: sūtījumus un pašreizējos turētājus.

Dalībnieki lasa to daļu, ko drīkst lasīt viņu organizācija (īpašnieks, turētājs vai biedrība); paslēptās rindas tiek saskaitītas. **Drukāt** un **CSV** eksportē pārskatu.

<!-- SHOT: recall-report -->
Figure: Gala partijas atsaukšanas pārskats.

## Audita žurnāls {#admin-audit}

Skatīt [Audita žurnāls](#trust-audit).
