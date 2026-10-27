# 15. Platformas administrators {#admin}

## Jūsu ikdienas darbs {#admin-daily}

Platformas administratori uztur platformas darbību. Viņi pārbauda organizācijas, pārvalda dalības un atļaujas, nosūta pieteikšanās datus, publicē paziņojumu tekstus, uztur atsauces datus un kontroles punktu definīcijas, reģistrē biškopjus, izveido gala partijas (pagaidām), izlemj labojumus un seko audita žurnālam.

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
| **Lietotāju Pārvaldība** | Lietotāji, lomas, **Audita žurnāls**, **Datu glabāšana**, **Paziņojumi un piekrišanas teksti**, **SMS izsūtne (testa pakalpojums)** (tikai ar testa SMS pakalpojumu) |
| **Organizācijas** | Mans darbs, organizācijas, kompetences, reģistrs, biškopju reģistrācija, lomas un atļaujas, KKP atbildība, labojumi, sūtījumi, paraugi, biškopju lapas |

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

Organizācijas lapā (platformas administrators; biškopju biedrībām tā vietā pārbaudītas centrālās biedrības organizācijas administrators, ja platforma ir tā konfigurēta: tad administratori biedrībām saņem paziņojumu "Piekļuve liegta"):

- **Pārbaudīt** (no statusa "Gaida") – nepieciešams vismaz viens augšupielādēts dokuments un, ja organizācija tiek pārbaudīta pirmo reizi, aktīvs atbilstības speciālists.
- **Pieprasīt dokumentus** (kamēr statuss ir "Gaida") – nepieciešams iemesls, piemēram, kurš dokuments trūkst.
- **Apturēt** (kamēr statuss ir "Pārbaudīta") – nepieciešams iemesls. **Atjaunot** atkal padara apturētu organizāciju par pārbaudītu.

Katra izmaiņa parādās pārbaužu vēsturē un audita žurnālā.

### Kompetences {#admin-org-capabilities}

**Pievienot kompetenci** pievieno kompetenci. Kompetencei **Laboratorija** atzīmējiet akreditāciju un ievadiet akreditācijas numuru un tā derīguma termiņu; akreditētai laboratorijai numurs ir obligāts. **Noņemt** noņem kompetenci; organizācijai paliek vismaz viena kompetence. Lapā **Kompetences** katrai kompetencei redzams, kuriem KKP tā nepieciešama un kurām organizācijām tā ir. Cilnē **Organizācijas kompetences** lapā **Lomas un atļaujas** var rediģēt visas organizācijas vienlaikus.

### Dalībnieki {#admin-members}

1. Organizācijas lapā, sadaļā ar dalībniekiem, izvēlieties lomu. Loma **Biškopju reģistrācijas speciālists** ir pieejama tikai organizācijā ar kompetenci "Biškopju reģistrācija".
2. Ievadiet esoša lietotāja lietotājvārdu vai arī jaunu lietotājvārdu, e-pasta adresi un **Mobilais tālrunis**, lai vienlaikus izveidotu lietotāju. Izvēlieties, kā lietotājs saņems pieteikšanās datus: **Nosūtīt lietotājvārdu un pagaidu paroli pa SMS (ieteicams)** vai **Iestatīt sākotnējo paroli šeit (lietotājs to nomaina pirmajā pieteikšanās reizē)**.
3. Noklikšķiniet uz **Pievienot dalībnieku**: "… pievienots kā …".

Dalība ir viens lietotāja konts × viena organizācija × viena loma; lietotājam var būt vairākas dalības. **Atsaukt** izbeidz dalību un izraksta lietotāju visās ierīcēs. Cilnē **Dalībnieki** lapā **Lomas un atļaujas** uzskaitītas visu organizāciju visas dalības.

<!-- SHOT: admin-org-members -->
Figure: Organizācijas dalības un veidlapa **Pievienot dalībnieku**.

### Četru acu principa izņēmums {#admin-four-eyes}

Mazas organizācijas ar vienu kvalitātes speciālistu var saņemt izņēmumu (**Četru acu principa izņēmums**): tad dalībnieki drīkst apstiprināt savus KKP ierakstus un apstiprināt savus pabeigšanas (fiksēšanas) pieprasījumus. Katrs šāds apstiprinājums tiek atzīmēts audita žurnālā, un organizācijas atbilstības speciālists to redz sadaļā **Apstiprināšanas mehānisms**. Izņēmums nekad neattiecas uz laboratorijas rezultātiem un uz parakstītu foto pārskatīšanu.

### Datu pārzinis {#admin-data-controller}

Katra organizācija ir pārzinis personas datiem, ko tā reģistrē. Organizācijas lapā sadaļā **Datu pārzinis** glabājas tās **Juridiskais nosaukums**, **Kontakts (e-pasts vai tālrunis)** un **Adrese**; tie ir norādīti organizācijas privātuma paziņojumā. Tos aktualizē organizācijas atbilstības speciālists vai platformas administrators.

## Lietotāji {#admin-users}

Sadaļā **Lietotāju Pārvaldība** → **Lietotāji** uzskaitīti konti.

- **Pievienot jaunu lietotāju** izveido kontu. Organizāciju modelī atstājiet lauku **Globālā loma** tukšu organizāciju dalībniekiem un piešķiriet viņiem dalību organizācijas lapā; vērtību **Platformas administrators** izvēlieties tikai administratoriem. Ievadiet **Mobilais tālrunis** un izvēlieties **Nosūtīt lietotājvārdu un pagaidu paroli pa SMS (ieteicams)** vai **Iestatīt sākotnējo paroli šeit (lietotājs to nomaina pirmajā pieteikšanās reizē)**.
- **Sūtīt pieteikšanos pa SMS** nosūta jaunus pieteikšanās datus (jaunu pagaidu paroli, derīgu 72 stundas) lietotājam, kurš tos pazaudējis. Pagaidu paroli neredz neviens, arī ne administrators.
- **Atiestatīt paroli** iestata lietotājam jaunu paroli; tā tiek uzskatīta arī par sākotnējo paroli, kas lietotājam jānomaina nākamajā pieteikšanās reizē.
- **Atbloķēt** parādās blakus kontam ar statusu **Bloķēts** un uzreiz beidz bloķēšanu ("Konts atbloķēts."). Bez tā bloķēšana beidzas pati pēc 15 minūtēm.
- **Dzēst** noņem kontu (lietojiet piesardzīgi; darbību nevar atsaukt). Savu kontu dzēst nevar. Katra dzēšana un katra atbloķēšana tiek ierakstīta audita žurnālā.

Jaunām parolēm jābūt vismaz 10 rakstzīmes garām, tās nedrīkst sakrist ar lietotājvārdu un nedrīkst būt plaši izplatītas paroles; esošās paroles paliek derīgas. Pēc 5 neveiksmīgiem pieteikšanās mēģinājumiem konts tiek bloķēts uz 15 minūtēm.

Sadaļā **Lietotāju Pārvaldība** → **Lomas** uzskaitītas globālās lomas. Lomas, uz kurām balstās platforma (**Administrators** un **Biškopis**), ir atzīmētas kā **Aizsargāta**, un tās nevar pārdēvēt vai dzēst.

## Lomas un atļaujas {#admin-permissions}

Lapā **Lomas un atļaujas** ir trīs cilnes:

- **Dalībnieki**: visas aktīvās dalības; pievienošana un atsaukšana.
- **Lomu atļaujas**: izvēlieties lomu un katrai funkcijai atzīmējiet **Ievadīt**, **Apstiprināt** un **Lasīt**. Funkcijas ir KKP (katram sava rinda, ar KKP saīsinājumu lapas valodā) un jomas: pieņemšana, paraugi, laboratorijas rezultāti, sūtījumi, biškopju lapas, atsaukšana, pabeigšana (fiksēšana), biškopju reģistrācija, pierādījumi, cena un organizācijas audita ieraksti. Katrām tiesībām jāpaliek vismaz vienai lomai. Izmaiņas stājas spēkā nekavējoties.
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

- **Biškopju reģistrācija**: biškopjus reģistrē novadu lauksaimniecības pārvalžu biškopju reģistrācijas speciālisti ar SMS kodu vai parakstītu foto (skatīt [12. nodaļu](#role-ro)). Platformas administrators var reģistrēt biškopjus tādā pašā veidā; šādas reģistrācijas tiek reģistrētas platformas paša organizācijai, un to parakstītos foto pārskata cits platformas administrators.
- **Biškopji**: biškopju saraksts un biškopja lapa (vārds, dzimšanas datums, pieredze, tālrunis, izglītība, biškopības veids un mērķis, pēc izvēles fotoattēls). Saimniecības numurs tiek ģenerēts automātiski. Reģistrācijā apliecinātos faktus (identitāti, reģistrācijas numuru, tālruni) šeit var tikai lasīt; tie mainās tikai ar jaunu reģistrāciju. Biškopji nekad netiek dzēsti, tikai deaktivizēti. Biškopjus, kas reģistrēti pirms reģistrācijas procedūras ieviešanas, atkārtoti pārbauda ar to pašu veidlapu; viņu vecie pieteikšanās konti paliek bloķēti.
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

**Piegādes fakti.** Apstiprinātas piegādes faktus (datumu, tvertņu veidu un svaru, plombu numurus) labo piegādes lapā, sadaļā **Šīs piegādes labojumi** ("Tikai kļūdai ierakstā. Labojumu jāapstiprina citam platformas administratoram; sākotnējais ieraksts paliek redzams, un biškopis saņem SMS."):

1. Noklikšķiniet uz **Pieprasīt labojumu**, mainiet laukus, ierakstiet **Iemesls** un noklikšķiniet uz **Nosūtīt pieprasījumu**: "Labojums pieprasīts; tas jāapstiprina citam platformas administratoram."
2. Otrs administrators salīdzina **Sākotnēji** un **Izlabots** un noklikšķina uz **Apstiprināt un piemērot** vai ieraksta **Piezīme (vajadzīga noraidīšanai)** un noklikšķina uz **Noraidīt**. "Labojums piemērots; biškopis informēts ar SMS."

Ar labojumu tvertnes nekad netiek pievienotas vai noņemtas. Piegāde saglabā abus saturus ar to nospiedumiem; audita žurnālā tiek reģistrēta katra mainītā rinda.

<!-- SHOT: admin-delivery-correction -->
Figure: Piegādes labojums, kas gaida otro administratoru.

## Biškopju lapas, sūtījumi un paraugi {#admin-overviews}

Administratori var atvērt lapas **Biškopju lapas**, **Sūtījumi un krājumi** un **Paraugi un laboratorija** tikai lasīšanas režīmā, lai sekotu visu organizāciju darbam.

## Atsaukšana {#admin-recall}

Gala partijas vai partijas **Atsaukšanas pārskats** uzskaita:

- **augšupējo ķēdi**: partijas, dravas, biškopjus un piegādes veidlapas, bet piegādēm ar tvertnēm – katru tvertni ar tās plombu vēsturi, paraugu burkām, laboratorijas vērtībām katrai tvertnei un norādi, vai apstiprinātais saturs nav mainīts;
- gala partijas;
- **lejupējo ķēdi**: sūtījumus un pašreizējos turētājus.

Dalībnieki lasa to daļu, ko drīkst lasīt viņu organizācija (īpašnieks, turētājs vai biedrība); paslēptās rindas tiek saskaitītas. Dalībnieks redz tikai savas organizācijas krājumu sadaļā **Jūsu pašreizējais krājums**; administratori redz visus turētājus sadaļā **Pašreizējie turētāji**. Cenas atsaukšanas pārskatā nekad netiek rādītas. **Drukāt** un **CSV** eksportē pārskatu.

<!-- SHOT: recall-report -->
Figure: Gala partijas atsaukšanas pārskats.

## Paziņojumi un piekrišanas teksti {#admin-notices}

Lapā **Paziņojumi un piekrišanas teksti** uzskaitīti privātuma paziņojumi (lietotājiem un biškopjiem, platformas un katras organizācijas), piekrišanas teksti un reģistrācijas apliecinājums katrā valodā, ar to **Versija**, publicēšanas datumu (**Publicēts**) un to, cik personu tos ir pieņēmušas (**Pieņemts**). "Tekstiem ir versijas, un pēc publicēšanas tos nemaina: publicējiet jaunu versiju."

1. Noklikšķiniet uz **Publicēt jaunu versiju** (vai uz **Jauna versija** blakus tekstam).
2. Izvēlieties **Veids**, **Kam** tas paredzēts, **Organizācija** (tukšs – platformai), piekrišanas tekstam **Piekrišanas mērķis**, kā arī **Versija** un **Valoda**, un uzrakstiet **Teksts**.
3. Atzīmējiet **Projekts (datu aizsardzības nodaļa vēl nav apstiprinājusi)**, kamēr teksts nav apstiprināts; tad tas tiek rādīts ar atzīmi **PROJEKTS**.
4. Noklikšķiniet uz **Publicēt**. "Publicēšana atsauc pašreizējo tā paša veida, auditorijas, organizācijas, mērķa un valodas tekstu. Lietotājiem, kas pieņēma iepriekšējo versiju, jautās vēlreiz."

Publiskajā lapā **Privātums un piekrišana** (`/privacy`) redzami pašreizējie paziņojumi biškopjiem un platformas lietotājiem.

## SMS {#admin-sms}

Platforma sūta SMS ar reģistrācijas kodiem, pieteikšanās datiem, piegāžu apstiprinājumiem, paziņojumu apstiprinājumiem, paroles atiestatīšanai, tālruņa pārbaudei un informatīvām ziņām. Kurš pakalpojuma sniedzējs tās sūta, nosaka servera iestatījums (skatīt [Iestatījumi servera konfigurācijā](#admin-settings)). Platforma reģistrē katru ziņu ar maskētu numuru; kodi un paroles nekad netiek glabāti atklātā veidā. Dienas limits sargā SMS budžetu; paroles atiestatīšanas un piegāžu apstiprinājumu ziņas tiek sūtītas arī pēc tā sasniegšanas.

Ar **testa pakalpojumu** (tikai izstrādei un demonstrācijām) nekas netiek sūtīts: lapā **SMS izsūtne (testa pakalpojums)** tā vietā redzama katra ziņa ar tās tekstu un kodu, kā arī "Daļas šodien / dienas limits".

<!-- SHOT: admin-sms-outbox -->
Figure: Testa pakalpojuma SMS izsūtne.

## Iestatījumi servera konfigurācijā {#admin-settings}

Daži slēdži tiek iestatīti servera konfigurācijā, nevis panelī, un to maiņai vajadzīgs servera operators. Katru no tiem ieslēdz atsevišķi, kad ir gatavs viss nepieciešamais:

Table: Slēdži servera konfigurācijā {#tbl-settings}

| Iestatījums | Izslēgts (šodien) | Ieslēgts | Ieslēgt, kad |
|---|---|---|---|
| SMS pakalpojuma sniedzējs | Ziņas tiek tikai ierakstītas žurnālā | Ziņas sūta līgumā noteiktais pakalpojuma sniedzējs | SMS pakete ir iegādāta un sūtītāja vārds reģistrēts |
| Piegādes ar tvertnēm | Uzņēmumi reģistrē piegādi ar vienu parakstītu veidlapu un vienu paraugu (vecā kārtība) | Tvertņu procedūra no [5. nodaļas](#sao-intake) | Uzņēmumiem ir paraugu burkas ar drošības vāciņiem, plombas un etiķetes |
| Obligāta paziņojuma apstiprināšana | Josla aicina lietotājus apstiprināt paziņojumu | Panelis paliek slēgts, līdz paziņojums ir apstiprināts | Paziņojumu tekstus ir apstiprinājusi datu aizsardzības nodaļa |
| Biškopju portāls | Biškopji nesaņem pieteikšanās datus | Reģistrācijas speciālisti nosūta pieteikšanās datus; atveras [14. nodaļā](#portal) aprakstītais portāls | Projekts dod atļauju portālam |
| Agrāk reģistrētu biškopju publiskie vārdi | – | Medus stāstā redzams neitrālais kods, līdz ir piekrišana vārda rādīšanai (noklusējums) | – (alternatīva saglabā vārdus, līdz biškopis tiek reģistrēts no jauna) |
| Datu glabāšanas palaišana | Lapa **Datu glabāšana** tikai saskaita | Palaišana ir iespējama (skatīt tālāk) | Īpašnieks apstiprina pirmo palaišanu un pseidonīmu formātu |

## Datu glabāšana {#admin-retention}

Lapā **Datu glabāšana** redzams, cik ilgi platforma glabā katru datu veidu un kas notiek pēc tam ([Table](#tbl-retention)). Lapa ir **izmēģinājuma palaišana**: "tā saskaita rindas, kuru termiņš ir beidzies, un neko nemaina". **Izmēģinājuma pārskats (CSV)** lejupielādē skaitļus.

Table: Glabāšanas termiņi {#tbl-retention}

| Dati | Glabā | Pēc tam |
|---|---|---|
| Vienreizējie kodi | 30 dienas | dzēsti (glabāti tikai jaucējkodi); kods, kas pierāda ierakstu, paliek kopā ar to |
| SMS ziņas | 1 gadu | numurs un teksts noņemti, skaiti saglabāti |
| Cenas | 5 gadus | dzēstas |
| Partijas, pieņemšanas, tvertnes, paraugi, laboratorijas pārskati, gala partijas | 5 gadus pēc gala partijas vēlākā derīguma termiņa (partijas datums + 2 gadi) vai pēc piegādes | saite ar biškopi pseidonimizēta; produkta dati saglabāti |
| Biškopja identitāte (neaktīvi biškopji) | kamēr aktīvs, pēc tam – viņa partiju termiņš | pseidonimizēta |
| Pierādījumu fotoattēli (parakstītas piegādes veidlapas) | tik ilgi, cik ieraksts, ko tie pierāda | dzēsti, jaucējkods saglabāts |
| Piekrišanas un paziņojumu pieņemšanas | tik ilgi, cik saistītie dati, + 3 gadi (pierādījums) | dzēstas |
| Bijušo dalībnieku konti | 2 gadus pēc pēdējās dalības beigām | anonimizēti; audita ierakstos paliek tikai lietotāja identifikators |
| Audita žurnāls | 10 gadus | netiek mainīts (tikai papildināšana) |

Glabāšanas palaišana ir **izslēgta** ("Izslēgta: par pirmo palaišanu lemj īpašnieks"). Kad īpašnieks to ir apstiprinājis un platformas operators to ir ieslēdzis, parādās **Palaist glabāšanu tagad**; tam vajadzīgs apstiprinājums "Īpašnieks apstiprināja šo palaišanu (tā dzēš un notīra datus, un to nevar atsaukt)." Palaišana dzēš vecos vienreizējos kodus, notīra veco SMS ziņu numuru un tekstu un dzēš vecās cenas; tā tiek ierakstīta audita žurnālā. Pārējie noteikumi tiek tikai saskaitīti ("tikai saskaita (lēmums O-14)"), līdz tiek izlemts pseidonīmu formāts. Ikmēneša palaišanu var ieslēgt tikai pēc pirmās palaišanas ar roku.

**Derīguma termiņš.** Gala partijām nav derīguma termiņa; to aizstāj partijas datums plus 2 gadi (iestatījums). Ražotājs apstiprina sava medus faktisko glabāšanas laiku; tad termiņu iestata atbilstoši.

## Audita žurnāls {#admin-audit}

Skatīt [Audita žurnāls](#trust-audit).
