# 13. Uzticamība un privātums {#trust}

## Audita žurnāls {#trust-audit}

Katras izmaiņas platformā (katra saglabāšana, apstiprināšana, pabeigšana (fiksēšana), sūtījums, pieteikšanās, atļauju maiņa) tiek ierakstītas lapā **Audita žurnāls**. Platformas administratori to atver sadaļā **Lietotāju Pārvaldība** → **Audita žurnāls**.

Katrā rindā redzams:

- **Kad**: vietējais laiks; novietojiet kursoru virs tā, lai redzētu UTC laiku;
- **Kas**: lietotājs un organizācija;
- **Darbība**: piemēram, Create, Update, Approve, Finalize, Verify, Login (izveide, atjaunināšana, apstiprināšana, pabeigšana, pārbaude, pieteikšanās), ar karodziņiem, piemēram, "SelfApproved";
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
- Labojumiem nepieciešami divi administratori, un tie tiek reģistrēti žurnālā kopā ar iemeslu.

## Faili un nospiedumi {#trust-files}

Piegādes veidlapas, laboratorijas pārskatu PDF faili, organizāciju dokumenti, reģistrācijas apliecinājumi un piekrišanas dokumenti tiek glabāti ārpus publiskās tīmekļa mapes ar nejaušu nosaukumu un to **SHA-256 nospiedumu**. Augšupielādes tiek pārbaudītas pēc satura, ne tikai pēc faila nosaukuma: PDF līdz 10 MB vai JPG, PNG vai WEBP līdz 8 MB. Pirms apstiprināta laboratorijas pārskata izmantošanas platforma pārbauda, vai saglabātais PDF fails joprojām atbilst tā nospiedumam. Ikviens var salīdzināt pārskata PDF failu ar tā nospiedumu publiskajā pārbaudes lapā, to neaugšupielādējot.

## Ko var lasīt sabiedrība {#trust-public}

Publiskajās lapās (Medus stāsts, biškopja lapa, pārskata pārbaude) redzama tikai apstiprināta, pārbaudīta informācija:

- **Vieta**: tikai **province** (biškopja lapā norādīts arī rajons). Provinci aprēķina serverī; dravu koordinātas nekad nepamet serveri. Kartē tiek iekrāsotas provinces, nekad netiek rādīti punkti; pašlaik tā aptver Turciju, un citu partnervalstu kartes ir plānotas.
- **Cilvēki**: Medus stāstā ir nosaukti biškopji, kuru medus ir gala partijā; biškopja lapā vārds tiek rādīts tikai ar rakstisku piekrišanu (pretējā gadījumā neitrāls kods). Nekad netiek rādīts tālruņa numurs, dzimšanas datums vai personas fotoattēls, un netiek rādīti platformas lietotājvārdi.
- **Dati**: laboratorijas un kontroles punktu dati; spriedums "robežās / ārpus robežām" tikai **pārbaudītām robežvērtībām**. Brīva teksta atbildes un faili netiek publicēti, ja vien administrators to nav skaidri atļāvis KKP, kura atbildēs nav personas datu.
- **Pārskati**: pārskata kods, laboratorija, akreditācija, datums un nospiedums, nekad – partija vai biškopis.

- **Fotoattēli**: tikai to dravu fotoattēli, kas piegādāja medu gala partijai, un bez metadatiem (bez GPS atrašanās vietas vai kameras datiem).

Medus stāsts pastāv tikai pabeigtām (fiksētām) gala partijām; biškopja lapa – tikai tik ilgi, kamēr piekrišana ir spēkā.

<!-- SHOT: public-map -->
Figure: Izcelsme pa provincēm publiskajā Medus stāstā.
