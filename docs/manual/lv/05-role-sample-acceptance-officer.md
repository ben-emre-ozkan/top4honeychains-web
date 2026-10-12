# 5. Paraugu pieņemšanas speciālists {#role-sao}

## Jūsu ikdienas darbs {#sao-daily}

Jūs strādājat pārstrādes uzņēmuma pieņemšanas punktā. Kad biškopis piegādā medu, jūs:

1. reģistrējat piegādi (tā izveido neapstrādāta medus partiju);
2. noplombējat paraugu un kontrolparaugu un nosūtāt paraugu uz laboratoriju;
3. ievadāt KKP 0 – partijas sākotnējo auditu –, lai kvalitātes nodrošināšanas speciālists to varētu apstiprināt.

Jūsu uzdevumi redzami sadaļā **Mans darbs**, grupā **Ievadāmās vērtības**. Izvēlnē redzami vienumi **Piegādes (pieņemšana)** un **Paraugi un laboratorija**.

## Piegādes reģistrēšana {#sao-intake}

1. Atveriet **Piegādes (pieņemšana)** un noklikšķiniet uz **Reģistrēt piegādi**.
2. Laukā **Biškopja saimniecības vai reģistrācijas numurs** precīzi ierakstiet biškopja saimniecības numuru vai pārbaudīto saimniecības reģistrācijas numuru un noklikšķiniet uz **Atrast**. Ja nekas netiek atrasts, tiek parādīts paziņojums "Nav aktīva biškopja ar šo numuru." Pārbaudiet numuru; biškopjus reģistrē platformas administrators.
3. Aizpildiet veidlapu:
   - **Drava**: viena no biškopja dravām.
   - **Medus veids**.
   - **Ražošanas periods**: pašreizējais periods ir atlasīts iepriekš.
   - **Izcelsmes valsts**.
   - **Piegādes datums**: pēc noklusējuma šodiena; tas nevar būt nākotnē.
   - **Daudzums (kg)**: kā decimāldaļu atdalītāju var lietot komatu vai punktu.
   - Izcelsmes **Province** un **Rajons**: ja iespējams, province tiek aizpildīta automātiski pēc reģistrācijas numura.
   - **Parakstītās piegādes veidlapas fotoattēls**: nofotografējiet ar kameras pogu vai izvēlieties failu (PDF līdz 10 MB vai JPG, PNG vai WEBP līdz 8 MB).
   - **Nosūtīt biškopim īsziņu** (neobligāti; pašreizējā versijā ziņojums tiek reģistrēts žurnālā, bet netiek nosūtīts).
4. Noklikšķiniet uz **Reģistrēt piegādi**.

<!-- SHOT: intake-form -->
Figure: Piegādes reģistrēšana tālrunī.

Paziņojums "Piegāde reģistrēta kā partija Nr. … ." apstiprina jauno partiju. Platforma piešķir partijai KKP 0–3 un atver tai lapu **Ņemt paraugu**.

**Biežākās kļūdas**

Table: Biežākās kļūdas, reģistrējot piegādi {#tbl-intake-errors}

| Paziņojums | Kas jādara |
|---|---|
| "Izvēlieties vienu no biškopja dravām." | Izvēlieties dravu no saraksta. |
| "Ievadiet piegādes datumu (ne nākotnē)." | Izlabojiet datumu. |
| "Daudzumam jābūt lielākam par 0." | Ievadiet daudzumu kilogramos. |
| "Izvēlieties izvēlētās provinces rajonu." | Vispirms izvēlieties provinci, pēc tam rajonu no tās saraksta. |
| "Pievienojiet parakstītās piegādes veidlapas fotoattēlu." | Parakstītā veidlapa ir obligāta. |
| "Šai dravai jau ir ražošanas ieraksts izvēlētajam periodam un medus veidam." | Viena drava piegādā vienu reizi katrā periodā katram medus veidam. Pārbaudiet periodu un medus veidu. |
| "Piegādes reģistrē tikai pārstrādes organizācija." / "Jūsu loma (…) nevar reģistrēt piegādes; lomas: …" | Jūsu organizācija vai loma nevar reģistrēt piegādes. Vērsieties pie savas organizācijas administratora. |

Sarakstā **Piegādes (pieņemšana)** redzama katra piegāde ar partijas numuru, datumu, dravu, medus veidu, daudzumu, izcelsmi un parakstīto veidlapu (ar tās SHA-256 nospieduma pirmajām rakstzīmēm). No katras rindas varat atvērt saites **Paraugs** un **Atsaukšana**.

## Parauga ņemšana un nosūtīšana {#sao-sample}

1. Pēc piegādes reģistrēšanas atveras lapa **Ņemt paraugu**. To var atvērt arī no **Paraugi un laboratorija** vai no piegādes rindas.
2. Laukā **Laboratorija** izvēlieties laboratoriju: sarakstā ir tikai pārbaudītas laboratorijas; akreditētai laboratorijai redzams tās akreditācijas numurs.
3. Aizpildiet laukus **Plombas numurs** (paraugam) un **Kontrolparauga plombas numurs**. Tiem jābūt atšķirīgiem.
4. Aizpildiet laukus **Kontrolparauga glabāšanas vieta** un **Glabāt līdz** (pēc noklusējuma 12 mēnešus uz priekšu).
5. Noklikšķiniet uz **Nosūtīt uz laboratoriju** vai uz **Vēlāk**, lai to nosūtītu citā reizē.

<!-- SHOT: sample-form -->
Figure: Parauga ņemšana: laboratorija un plombu numuri.

Paraugs saņem kodu, piemēram, `S-20070-1`, un statusu **Nosūtīts**. Laboratorija to atrod grupā **Saņemamie paraugi**. Ja laboratorija konstatē, ka plomba ir bojāta, paraugs tiek noraidīts; ņemiet jaunu paraugu no tās pašas partijas.

Kļūdas: "Izvēlieties pārbaudītu laboratoriju.", "Paraugam un kontrolparaugam nepieciešamas dažādas plombas.", "Ievadiet, kur tiek glabāts kontrolparaugs (ne vairāk kā 200 rakstzīmes).", "Ievadiet, līdz kuram datumam tiek glabāts kontrolparaugs (datums nākotnē).", "Paraugus ņem tikai organizācija, kuras turējumā ir partija."

## Darbs KKP lapā {#ccp-page}

Visas lomas, kas ievada vai apstiprina KKP datus, izmanto vienu un to pašu lapu ([Figure](#fig-ccp-page-lot)). Atveriet to no **Mans darbs**, no KKP žetoniem lapā **Mana organizācija** vai no partijas vai gala partijas lapas **Procesa plūsma**.

<!-- SHOT: ccp-page-lot -->
Figure: Partijas KKP lapa: atbilde **Jā** atver obligātu papildjautājumu.

Lapā redzams:

- partija (biškopis, drava, medus veids, ražošanas datums) vai gala partija (gala partija, kopējais daudzums, vairumtirgotājs, izcelsmes sastāvs);
- statusa žetons **Statuss** un tas, kas ierakstu ievadīja un apstiprināja;
- robežvērtībām izmantotā medus klase ("Robežvērtības medus klasei: …");
- parametri, sagrupēti, katram ar ievades lauku, norādi **Kritiskā robežvērtība:** un pēc saglabāšanas – **Robežās** vai **Ārpus robežām**;
- reklāmjosla **Tikai lasāms** ar iemeslu, ja jūs nedrīkstat ievadīt datus.

Ievades veidi:

- **Skaitlis**: kā decimāldaļu atdalītāju var lietot komatu vai punktu.
- **Jā / Nē**: izvēlieties no saraksta.
- **Teksts**: ne vairāk kā 100 rakstzīmes.
- **Fails**: augšupielādējiet dokumentu (PDF līdz 10 MB vai attēlu līdz 8 MB). Lai to dzēstu, atzīmējiet **Noņemt**.
- **Piezīmes**: neobligāts novērojums, ne vairāk kā 500 rakstzīmes.

Daži parametri ir obligāti tikai tad, ja uz citu jautājumu atbildēts noteiktā veidā, piemēram: "Obligāts tikai tad, ja uz “…” atbildēts Jā."

**Datu ievadīšana**

1. Aizpildiet parametrus.
2. Noklikšķiniet uz **Saglabāt uzraudzības ierakstus**. Paziņojums "Saglabātas KKP … vērtības." apstiprina saglabāšanu, un jūs atgriežaties lapā **Mana organizācija**.

Atsevišķas iesniegšanas pogas nav. Kad katram obligātajam parametram ir vērtība, ieraksts iegūst statusu **Iesniegts** un parādās apstiprinātāju sadaļā **Mans darbs**. Ja ievadīta tikai daļa vērtību, tas paliek statusā **Procesā**.

**Ja KKP nav piemērojams** (piemēram, medus netiek karsēts):

1. Kamēr ierakstam nav vērtību, atveriet sadaļu "Šis KKP šeit nav piemērojams (atzīmēt kā "Nav piemērojams")".
2. Laukā **Iemesls** ierakstiet iemeslu, piemēram, "neapstrādāts medus netiek karsēts vai pasterizēts".
3. Noklikšķiniet uz **Atzīmēt kā "Nav piemērojams"**. Iemesls, jūsu lietotājvārds un laiks tiek reģistrēti, un publiskajā Medus stāstā tiek parādīts "Nav piemērojams".

Lai to atceltu, noklikšķiniet uz **Noņemt atzīmi "Nav piemērojams"**. Arī nepiemērojams KKP ir jāapstiprina, turklāt citai personai, nevis tai, kas to atzīmēja.

**Biežākās kļūdas**

Table: Biežākās kļūdas KKP lapā {#tbl-ccp-errors}

| Paziņojums | Nozīme |
|---|---|
| "…: ievadiet skaitli." / "…: izvēlieties Jā vai Nē." | Vērtībai ir nepareizs veids. |
| "Šis KKP ir atzīmēts kā "Nav piemērojams". Lai reģistrētu vērtības, vispirms noņemiet šo atzīmi." | Pirms vērtību ievadīšanas noņemiet atzīmi. |
| "Par šo kontroles punktu atbild cita organizācija; jūs to varat tikai skatīt." | Jūs varat šo KKP lasīt, bet ne mainīt. |
| "Jūsu loma (…) nevar ievadīt šo kontroles punktu; ievadīt drīkst lomas: …" | Šo KKP ievada cita loma. |
| "Jūsu organizācijas statuss ir Gaida; datus var ievadīt tikai pārbaudītas organizācijas." | Gaidiet pārbaudi. |
| "Šī neapstrādāta medus partija ir izmantota pabeigtajā (fiksētajā) gala partijā … un ir bloķēta. …" | Partija ir fiksēta; to var mainīt tikai ar labojumu. |

## KKP 0 (sākotnējā audita) ievadīšana {#sao-ccp0}

1. Sadaļā **Mans darbs**, grupā **Ievadāmās vērtības**, atveriet vienumu "KKP 0 · #[partija]".
2. Ievadiet pieņemšanas pārbaužu rezultātus (KKP 0 parametrus). Daži jautājumi, ja uz tiem atbildat **Jā**, atver papildu obligātos jautājumus.
3. Noklikšķiniet uz **Saglabāt uzraudzības ierakstus**.

Kad ir visi obligātie dati, KKP 0 iegūst statusu **Iesniegts** ([Figure](#fig-ccp0-submitted)), un jūsu organizācijas kvalitātes nodrošināšanas speciālists to atrod grupā **Apstiprināmās vērtības**. Jūs nevarat apstiprināt savu KKP 0 ("Četru acu princips: jūs reģistrējāt šī KKP vērtības, tāpēc tas jāapstiprina citai personai."). Laboratorija var ievadīt rezultātus tikai pēc tam, kad KKP 0 ir apstiprināts.

<!-- SHOT: ccp0-submitted -->
Figure: KKP 0 ir iesniegts un gaida, kad to apstiprinās cita persona.
