# 16. Uzticamība un privātums {#trust}

## Audita žurnāls {#trust-audit}

Katras izmaiņas platformā (katra saglabāšana, apstiprināšana, pabeigšana (fiksēšana), sūtījums, pieteikšanās, atļauju maiņa, pierādījumu lejupielāde un datu glabāšanas palaišana) tiek ierakstītas lapā **Audita žurnāls**. Platformas administratori to atver sadaļā **Lietotāju Pārvaldība** → **Audita žurnāls**; atbilstības speciālists savas organizācijas ierakstus lasa lapā **Atbilstība**.

Katrā rindā redzams:

- **Kad**: vietējais laiks; novietojiet kursoru virs tā, lai redzētu UTC laiku;
- **Kas**: lietotājs un organizācija;
- **Darbība**: piemēram, Create, Update, Approve, Finalize, Verify, Login, Download (izveide, atjaunināšana, apstiprināšana, pabeigšana, pārbaude, pieteikšanās, lejupielāde), ar karodziņiem, piemēram, "SelfApproved";
- **Ieraksts**: kurš ieraksts, ar saiti, ja ir attiecīgā administrēšanas lapa;
- **Izmaiņas**: katrs mainītais lauks formā **vecā → jaunā**, ar lauka nosaukumu jūsu valodā; lietotāju un organizāciju identifikatoru vietā tiek rādīti nosaukumi;
- **Jaucējvērtība**: rindas nospieduma sākums.

**Rādīt neapstrādātus datus** atver saglabātos datus tieši tādus, kādi tie tika ierakstīti (vecās un jaunās vērtības).

Filtrējiet pēc ieraksta veida, darbības, lietotājvārda vai karodziņa (piemēram, `SelfApproved`) un noklikšķiniet uz **Filtrēt**.

<!-- SHOT: audit-log -->
Figure: Audita žurnāls ar izmaiņām vecā → jaunā un neapstrādātajiem datiem.

### Tikai papildināms žurnāls un jaucējvērtību ķēde {#trust-hash-chain}

Audita žurnālu var tikai papildināt. Datubāze noraida jebkuru mēģinājumu mainīt vai dzēst rindu. Katrā rindā ir SHA-256 nospiedums no tās satura un iepriekšējās rindas, tādējādi veidojot ķēdi.

**Pārbaudīt jaucējvērtību ķēdi** pārrēķina visu ķēdi. Paziņojums "Jaucējvērtību ķēde: Visas … rindas ir pārbaudītas." nozīmē, ka nekas nav mainīts. Pretējā gadījumā paziņojumā norādīta pirmā rinda, kas ir mainīta vai kas nenorāda uz iepriekšējo rindu.

## Ieraksti, kas nemainās {#trust-locked}

- Apstiprināts KKP ieraksts saglabā apstiprinājumu tikai tik ilgi, kamēr tā vērtības nemainās.
- Pabeigta (fiksēta) gala partija un tās partijas ir bloķētas.
- Apstiprināti laboratorijas rezultāti kļūst par tikai lasāmām KKP 1 vērtībām.
- Apstiprinātas piegādes fakti ir bloķēti; tvertnes, plombu notikumus, reģistrācijas, cenu ierakstus, SMS ziņas, paziņojumus un to pieņemšanas var tikai papildināt.
- Labojumiem nepieciešami divi administratori, un tie tiek reģistrēti žurnālā kopā ar iemeslu.

## Kodi, kas apstiprina saturu {#trust-codes}

Biškopis apstiprina reģistrāciju un piegādi ar 6 ciparu kodu, kas saņemts SMS ziņā, un parakstītājas lomas tādā pašā veidā apstiprina privātuma paziņojumu. Katrs kods ir piesaistīts nospiedumam no tieši tā satura, kuram tas tika nosūtīts: jebkuras izmaiņas pēc tam kodu anulē. Kodi ir derīgi 5 minūtes, un tos var mēģināt ievadīt 3 reizes; tiek glabāta tikai koda jaucējvērtība. Ja SMS nav iespējama, koda vietā izmanto parakstītu foto, un otra persona (atbilstības speciālists) to pieņem vai noraida.

## Faili un nospiedumi {#trust-files}

Piegādes veidlapas, reģistrācijas fotoattēli, laboratorijas pārskatu PDF faili, organizāciju dokumenti, reģistrācijas apliecinājumi un piekrišanas dokumenti tiek glabāti ārpus publiskās tīmekļa mapes ar nejaušu nosaukumu un to **SHA-256 nospiedumu**. Augšupielādes tiek pārbaudītas pēc satura, ne tikai pēc faila nosaukuma: PDF līdz 10 MB vai JPG, PNG vai WEBP līdz 8 MB. Pirms apstiprināta laboratorijas pārskata izmantošanas platforma pārbauda, vai saglabātais PDF fails joprojām atbilst tā nospiedumam. Ikviens var salīdzināt pārskata PDF failu ar tā nospiedumu publiskajā pārbaudes lapā, to neaugšupielādējot.

**Kas lejupielādē pierādījumus.** Pierādījumu failus lejupielādē tikai loma, kas tos reģistrē, organizācijas atbilstības speciālists un organizācijas administrators, kā arī platformas administratori; cita tās pašas organizācijas loma saņem 403, cita organizācija – 404, un katra lejupielāde tiek ierakstīta audita žurnālā (skatīt [Datu tiesības](#crud)).

## Paziņojumi, piekrišanas un datu pārzinis {#trust-notices}

**Privātuma paziņojumi.** Platforma un katra organizācija publicē privātuma paziņojumu: kā tiek apstrādāti personas dati, kādam nolūkam, cik ilgi tos glabā un kas ir **datu pārzinis**. Lietotāji apstiprina platformas un katras organizācijas, kurā viņi strādā, paziņojumus (skatīt [Privātuma paziņojums](#notice-accept)); biškopji izlasa savu paziņojumu reģistrācijas laikā un apstiprina to ar SMS kodu. Paziņojumiem ir versijas: jauna versija atkal jautā visiem, un vecās pieņemšanas paliek kā pierādījums. Teksti, kurus datu aizsardzības nodaļa vēl nav apstiprinājusi, ir ar atzīmi **PROJEKTS**. Publiskajā lapā **Privātums un piekrišana** (`/privacy`) redzami pašreizējie paziņojumi.

**Datu pārzinis.** Katra organizācija ir pārzinis datiem, ko tā reģistrē; tās juridiskais nosaukums, kontakts un adrese ir norādīti tās paziņojumā, un tos organizācijas lapā uztur tās atbilstības speciālists vai platformas administrators. Biškopis portālā redz, kurām organizācijām ir viņa dati (**Kam ir jūsu dati**).

**Piekrišanas.** Viss, kas pārsniedz darbam nepieciešamo, ir neobligāts un tiek jautāts atsevišķi:

- biškopja vārds publiskajās lapās;
- informatīvas SMS par piegādēm un rezultātiem;
- biedrības piekļuve biškopja prasītajām cenām;
- publiska biškopja lapa, ko reģistrē biedrība.

Piekrišanu sniedz reģistrācijas laikā (apstiprinot ar SMS kodu), portālā vai – biškopja lapai – biedrība ar parakstītu dokumentu. Atsaukums stājas spēkā nekavējoties: portālā vai to reģistrē atbilstības speciālists, ja tas saņemts vēstulē vai pa tālruni.

**Pieprasījumi par personas datiem.** Uz piekļuves, labošanas, dzēšanas un iebildumu pieprasījumiem 30 dienu laikā atbild organizācijas atbilstības speciālists (skatīt [Datu subjektu pieprasījumi](#co-requests)).

## Ko var lasīt sabiedrība {#trust-public}

Publiskajās lapās (Medus stāsts, biškopja lapa, pārskata pārbaude) redzama tikai apstiprināta, pārbaudīta informācija:

- **Vieta**: tikai **province** (biškopja lapā norādīts arī rajons). Provinci aprēķina serverī; dravu koordinātas nekad nepamet serveri. Kartē tiek iekrāsotas provinces, nekad netiek rādīti punkti; pašlaik tā aptver Turciju, un citu partnervalstu kartes ir plānotas.
- **Cilvēki**: Medus stāstā biškopis ir nosaukts vārdā **tikai ar spēkā esošu piekrišanu vārda rādīšanai**; pretējā gadījumā redzams biškopja lapas neitrālais kods. Biškopji, kas reģistrēti pirms piekrišanu procedūras ieviešanas, tiek rādīti ar kodu, līdz viņi sniedz piekrišanu (vai, ja platforma ir tā konfigurēta, saglabā vārdu, līdz tiek reģistrēti no jauna). Nekad netiek rādīts tālruņa numurs, dzimšanas datums vai personas fotoattēls, un netiek rādīti platformas lietotājvārdi. Dravu nosaukumi, kas ierakstīti pirms šīs versijas, bieži satur biškopja vārdu, tāpēc tie tiek rādīti tikai tur, kur drīkst rādīt viņa vārdu; pretējā gadījumā drava tiek apzīmēta ar provinci ("Drava: …"). Tas pats attiecas uz fotoattēlu aprakstiem.
- **Dati**: laboratorijas un kontroles punktu dati; spriedums "robežās / ārpus robežām" tikai **pārbaudītām robežvērtībām**. Brīva teksta atbildes un faili netiek publicēti, ja vien administrators to nav skaidri atļāvis KKP, kura atbildēs nav personas datu.
- **Pārskati**: pārbaudīt var tikai **apstiprinātus** pārskatus; lapā redzams pārskata kods, laboratorija, akreditācija, datums un nospiedums, nekad – partija vai biškopis.
- **Cenas**: nekad.
- **Fotoattēli**: tikai to dravu fotoattēli, kas piegādāja medu gala partijai, un bez metadatiem (bez GPS atrašanās vietas vai kameras datiem).

Medus stāsts pastāv tikai pabeigtām (fiksētām) gala partijām; biškopja lapa – tikai tik ilgi, kamēr piekrišana ir spēkā.

<!-- SHOT: public-map -->
Figure: Izcelsme pa provincēm publiskajā Medus stāstā.

## Cik ilgi dati tiek glabāti {#trust-retention}

Glabāšanas termiņi ir uzskaitīti tabulā ([Table](#tbl-retention)): piemēram, vienreizējie kodi 30 dienas, SMS ziņas 1 gadu, cenas 5 gadus, partijas un to ieraksti 5 gadus pēc gala partijas derīguma termiņa, audita žurnāls 10 gadus. Glabāšanas palaišana ir **izslēgta**: platformas administratori redz izmēģinājuma palaišanu, kas tikai saskaita, un pirmajai īstajai palaišanai vajadzīgs īpašnieka apstiprinājums un lēmums par pseidonīmu formātu (skatīt [Datu glabāšana](#admin-retention)).
