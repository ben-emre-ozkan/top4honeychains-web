# 3. Darba sākšana {#getting-started}

## Pieteikšanās {#sign-in}

1. Atveriet platformas adresi un sākumlapā noklikšķiniet uz **Pieteikties** vai dodieties tieši uz `/Account/Login`.
2. Laukā **Lietotājvārds** ievadiet savu lietotājvārdu (nevis e-pasta adresi), bet laukā **Parole** – savu paroli.
3. Atzīmējiet **Atcerēties mani**, ja vēlaties palikt pieteicies arī pēc pārlūka aizvēršanas. Koplietošanas datorā šo lauku neatzīmējiet.
4. Noklikšķiniet uz **Pieteikties**.

<!-- SHOT: sign-in -->
Figure: Pieteikšanās lapa.

Pēc pieteikšanās tiek atvērta lapa **Mans darbs** – jūs gaidošo uzdevumu saraksts. Biškopim tā vietā tiek atvērts biškopju portāls (skatīt [14. nodaļu](#portal)).

**Kas var pieteikties.** Jums jābūt platformas administratora lomai vai vismaz vienai aktīvai dalībai (viena organizācija × viena loma). Dalību piešķir platformas administrators (skatīt [Dalībnieki](#admin-members)). Biškopji piesakās ar portāla kontu, kura datus viņiem nosūta reģistrācijas speciālists.

**Biežākie paziņojumi**

- "Nepareizs lietotājvārds vai parole. Pēc pārāk daudziem neveiksmīgiem mēģinājumiem konts uz laiku tiek bloķēts." Lietotājvārds vai parole ir nepareiza, vai arī konts ir bloķēts. Pēc **5 neveiksmīgiem mēģinājumiem konts tiek bloķēts uz 15 minūtēm**. Uzgaidiet un mēģiniet vēlreiz vai izmantojiet **Aizmirsāt paroli?**.
- "Šim kontam vēl nav piekļuves panelim: tas nav nevienas organizācijas dalībnieks. …" Jūsu kontam nav aktīvas dalības. Lūdziet platformas administratoru pievienot jūs jūsu organizācijai.

Lai izrakstītos, atveriet izvēlni zem sava avatāra (augšējā labajā stūrī) un izvēlieties **Izrakstīties**.

### Pirmā pieteikšanās {#first-sign-in}

Jauni konti saņem pieteikšanās datus **SMS** veidā: lietotājvārdu un 10 rakstzīmju pagaidu paroli, kas derīga **72 stundas**. Neviens cits šo paroli nekad neredz. (Platformas administrators var arī iestatīt sākotnējo paroli un nodot to jums personīgi.)

1. Piesakieties ar lietotājvārdu un pagaidu paroli.
2. Tiek atvērta lapa **Izvēlieties savu paroli**: "Jūs pieslēdzāties ar pagaidu paroli. Pirms turpināt, izvēlieties savu paroli."
3. Divreiz ievadiet **Jaunā parole** un noklikšķiniet uz **Atjaunināt paroli**.

Parolei jābūt **vismaz 10 rakstzīmes** garai, tā nedrīkst sakrist ar jūsu lietotājvārdu un nedrīkst būt bieži lietota parole. Pieteikšanās ar pagaidu paroli apstiprina arī jūsu mobilā tālruņa numuru. Ja 72 stundas ir pagājušas, pieprasiet jaunus pieteikšanās datus: tos atkārtoti nosūta platformas administrators (dalībniekiem) vai reģistrācijas speciālists (biškopjiem).

## Aizmirsta parole {#password-reset}

Paroli atiestata ar kodu, kas pa SMS tiek nosūtīts uz jūsu **pārbaudīto** mobilo tālruni:

1. Pieteikšanās lapā noklikšķiniet uz **Aizmirsāt paroli?**.
2. Ievadiet savu **Lietotājvārds** un noklikšķiniet uz **Sūtīt kodu**. Lapa vienmēr atbild: "Ja konts pastāv un tam ir pārbaudīts mobilais tālrunis, kods tika nosūtīts pa SMS." (Tā neatklāj, kuri konti pastāv.)
3. Ievadiet **Kods no SMS (6 cipari)**, pēc tam divreiz **Jaunā parole**, un apstipriniet. Kods ir derīgs **5 minūtes**; **Sūtīt jaunu kodu** pēc minūtes nosūta citu kodu.

"Nav pārbaudīta mobilā tālruņa? Palūdziet personai, kas jums deva pieteikšanās datus, nosūtīt jaunus." Kodu var mēģināt ievadīt 3 reizes; vienam kontam stundā tiek nosūtīti ne vairāk kā 3 atiestatīšanas kodi.

<!-- SHOT: forgot-password -->
Figure: Paroles atiestatīšana: kods no SMS un jaunā parole.

(Ja platforma ir konfigurēta paroles atiestatīšanai pa e-pastu, lapa tā vietā prasa **E-pasts** adresi un nosūta saiti, kas derīga vienu dienu.)

## Jūsu profils {#profile}

Atveriet avatāra izvēlni un izvēlieties **Profils**.

- **Lietotājvārds** un **E-pasts** ir redzami, bet šeit tos mainīt nevar. Vērsieties pie platformas administratora.
- Laukā **Tālruņa numurs** redzams, vai numurs ir **pārbaudīts**. Lai to mainītu, ievadiet jauno numuru un noklikšķiniet uz **Sūtīt kodu**, pēc tam ievadiet **Kods no SMS (6 cipari)** un noklikšķiniet uz **Apstiprināt**. "Jauns numurs tiek izmantots tikai pēc tam, kad ievadāt uz to nosūtīto kodu. Pārbaudīts numurs ļauj atjaunot paroli pa SMS."
- **Mainīt paroli** atver dialoglodziņu: aizpildiet laukus **Pašreizējā parole**, **Jaunā parole** un **Apstipriniet paroli**, pēc tam noklikšķiniet uz **Atjaunināt paroli**.

Atstājot koplietošanas datoru, izmantojiet **Izrakstīties**; ekrāna bloķēšanas nav.

<!-- SHOT: profile -->
Figure: Profila lapa ar tālruņa pārbaudi un dialoglodziņu **Mainīt paroli**.

## Privātuma paziņojums {#notice-accept}

Pirms darba platformā jūs izlasāt **privātuma paziņojumu**: kā tiek apstrādāti jūsu personas dati – gan platformā, gan katrā organizācijā, kurā strādājat –, un kas ir **datu pārzinis** (skatīt [Paziņojumi, piekrišanas un datu pārzinis](#trust-notices)).

1. Kamēr jūs gaida paziņojums, reklāmjoslā redzams "Lūdzu, izlasiet privātuma paziņojumu un apstipriniet, ka esat to izlasījis." Noklikšķiniet uz tā. (Ja platforma ir konfigurēta to pieprasīt, paziņojuma lapa pēc pieteikšanās atveras pati, un panelis paliek slēgts, līdz apstiprināt.)
2. Izlasiet paziņojumu. Katras organizācijas paziņojumā norādīts tās datu pārzinis un **Kontakts pieprasījumiem**.
3. Atzīmējiet **Esmu izlasījis iepriekš minēto paziņojumu.** un noklikšķiniet uz **Apstiprināt**.
4. Lomas, kas paraksta organizācijas vārdā (organizācijas administrators, atbilstības speciālists, biškopju reģistrācijas speciālists, kvalitātes nodrošināšanas speciālists, laboratorijas vadītājs), apstiprina ar kodu: "Jūsu loma paraksta organizācijas vārdā, tāpēc jūs apstiprināt ar kodu, kas nosūtīts uz jūsu mobilo tālruni." Noklikšķiniet uz **Sūtīt kodu** un ievadiet **Kods no SMS (6 cipari)**.

Kad tiek publicēta jauna paziņojuma versija, jums to jautā vēlreiz. "Jūs esat pieņēmis pašreizējos paziņojumus. Nekas cits nav vajadzīgs." nozīmē, ka viss ir kārtībā.

<!-- SHOT: notice-accept -->
Figure: Privātuma paziņojuma apstiprināšana ar SMS kodu.

## Valodas izvēle {#language}

Platforma pieejama turku, angļu, spāņu, poļu un latviešu valodā. Noklusējuma valoda ir turku.

- Panelī izmantojiet karoga pogu augšējā joslā. (Šaurā tālruņa ekrānā karoga poga ir paslēpta; tādā gadījumā mainiet valodu pieteikšanās lapā.)
- Pieteikšanās lapās izmantojiet saites **TR EN ES PL LV** augšējā labajā stūrī.
- Publiskajās lapās izmantojiet valodu saites augšējā joslā.

Izvēle jūsu pārlūkā tiek saglabāta uz vienu gadu. Nosaukumi no atsauces datiem (medus veidi, kontroles punkti, lomas) tiek rādīti tajā pašā valodā; ja tulkojuma nav, tiek rādīts teksts angļu valodā.

## Augšējā josla un jūsu loma {#top-bar}

Augšējā joslā no kreisās uz labo pusi redzams:

- izvēlnes poga (tālruņos) un TOP4HoneyChains logotips, kas atver publisko sākumlapu;
- **organizācijas · lomas žetons**: organizācija un loma, kurā pašlaik strādājat;
- valodas poga;
- pilnekrāna režīma poga;
- jūsu avatārs ar vienumiem **Profils**, **Rādīt ekskursiju**, slēdzi **Rādīt animācijas** un **Izrakstīties**.

<!-- SHOT: top-bar-role-chip -->
Figure: Augšējā josla ar organizācijas · lomas žetonu.

Žetonā redzama ķeksīša zīme, ja jūsu organizācija ir pārbaudīta, izsaukuma zīme, kamēr tā gaida pārbaudi, un apturēšanas zīme uz sarkana fona, ja tā ir apturēta. Kamēr organizācija nav pārbaudīta, tās dalībnieki var lasīt tās datus un organizācijas administrators var augšupielādēt dokumentus, taču **neviens nevar ievadīt datus**. Reklāmjoslā redzams paziņojums: "Jūsu organizācija vēl nav pārbaudīta. …", bet apturētai organizācijai – "Jūsu organizācija ir apturēta. Jūs varat skatīt tās datus, taču neviens nevar ievadīt datus, kamēr platformas administrators to neatjauno."

Ja jums ir vairākas dalības, noklikšķiniet uz žetona, lai pārslēgtos starp tām; skatīt [Vairākas dalības](#memberships).

<!-- SHOT: role-switch -->
Figure: Dalības pārslēgšana: žetona saraksts **Jūsu lomas**.

## Ekskursija un animācijas {#animations}

**Ievada ekskursija.** Pirmo reizi strādājot kādā lomā, īsa ekskursija (**Īsa ekskursija**, 3 līdz 5 soļi) norāda uz šīs lomas izvēlnes vienumiem, katram pa vienai rindai. Ar **Tālāk** un **Atpakaļ** pārvietojaties pa to; **Izlaist** vai Esc to aizver. Platforma katram lietotājam un lomai atceras, ka esat to redzējis, arī citā datorā vai tālrunī. **Rādīt ekskursiju** avatāra izvēlnē to atver vēlreiz.

**Skaidrojošās animācijas.** Dažās vietās īsa aina, ne garāka par pusotru sekundi, parāda, ko dara poga vai kas tikko notika, ar vienas rindas parakstu: piemēram, paraugu burkas vāciņš nospiežas, kad ņemat paraugu, kravas auto, kad tiek reģistrēta piegāde, plomba noklikšķ ciet, burkas ceļo uz laboratoriju, pārbaude pieņemšanā un sūtījums aizbrauc. Aina nekad nebloķē lapu: taustiņa nospiešana, klikšķis vai Esc to uzreiz beidz, un tās pēdējais kadrs paliek kā nekustīgs attēls. Poga ⓘ **Parādīt, kas notiek** to atskaņo vēlreiz.

<!-- SHOT: scene-static -->
Figure: Skaidrojošas ainas (paraugu burka) nekustīgais attēls ar parakstu.

**Rādīt animācijas** avatāra izvēlnē izslēdz vai ieslēdz ainas jūsu kontam ("Īsas ainas, kas rāda, ko dara poga un kas tikko notika. Ja izslēgts, tiek rādīts nekustīgs attēls."). Ja jūsu ierīce pieprasa samazinātu kustību, tiek rādīti tikai nekustīgie attēli.

## Mans darbs {#my-work}

**Mans darbs** ir jūsu sākumlapa ([Figure](#fig-my-work)). Tajā uzskaitīts viss, kas jūs gaida aktīvajā dalībā, sagrupēts pēc veida, sākot ar vecākajiem ([Table](#tbl-mywork-groups)). Katrai grupai lapas augšdaļā ir sava flīze; noklikšķiniet uz tās, lai pārietu uz grupu.

<!-- SHOT: my-work -->
Figure: Kvalitātes nodrošināšanas speciālista lapa **Mans darbs** ar grupu flīzēm.

Table: Grupas lapā **Mans darbs** {#tbl-mywork-groups}

| Grupa | Ko tā nozīmē | Kas to saņem |
|---|---|---|
| **Ievadāmās vērtības** | Partijas vai gala partijas kontroles punktam (KKP) jāievada dati | Lomas, kas ievada šo KKP |
| **Apstiprināmās vērtības** | KKP ieraksts vai KKP 10 uzglabāšanas ieraksts ir iesniegts vai bez apstiprinājuma pabeigts vai atzīmēts kā "Nav piemērojams", un to jāapstiprina otrai personai. Lapā **Mans darbs** nekad netiek uzskaitīti ieraksti, kurus pats ievadījāt vai atzīmējāt. | Lomas, kas apstiprina šo KKP |
| **Pabeigšanas (fiksēšanas) pieprasījumi** | Kāds ir pieprasījis gala partijas pabeigšanu (fiksēšanu) | Gala partijas īpašnieka kvalitātes nodrošināšanas speciālists |
| **Saņemamie paraugi** | Uz jūsu laboratoriju ir nosūtīta paraugu burka | Analītiķis, laboratorijas vadītājs |
| **Ievadāmie laboratorijas rezultāti** | Saņemtam paraugam vēl nav pārskata, vai pārskats ir atgriezts | Analītiķis |
| **Apstiprināmie laboratorijas pārskati** | Pārskats ir iesniegts | Laboratorijas vadītājs |
| **Apstiprināmie sūtījumi** | Gaida sūtījums jūsu organizācijai | Saņēmēja darbinieks, organizācijas administrators |
| **Pārbaudīt parakstītos foto** | Reģistrācija vai piegāde ir apstiprināta ar parakstītu foto SMS koda vietā | Reģistrētājas organizācijas atbilstības speciālists (nekad ne persona, kas to reģistrēja) |
| **Pārskati** | Piegāde ar foto, noraidīts paraugs vai bojāta plomba gaida paraugu ņemšanas kvalitātes pārskatu | Atbilstības speciālists |
| **Datu subjektu pieprasījumi** | Ir atvērts personas pieprasījums par saviem datiem (jāatbild 30 dienu laikā) | Atbilstības speciālists |
| **Pārbaudāmās organizācijas** | Organizācija ir augšupielādējusi dokumentus un gaida pārbaudi | Platformas administrators (biedrības: centrālā biedrība, ja tā konfigurēts) |
| **Pārbaudāmie reģistrācijas numuri** | Biškopja saimniecības reģistrācijas numurs gaida pārbaudi | Platformas administrators |
| **Izlemjamie labojumi** | Cits administrators ir pieprasījis KKP datu vai piegādes faktu labojumu | Platformas administrators |

Katram vienumam redzams nosaukums (piemēram, "KKP 0 · #20070" un vārds **partija**, **gala partija** vai **uzglabāšana**), cik ilgi tas gaida (vienumi, kas vecāki par trim dienām, tiek atzīmēti dzintara krāsā), un žetons **Jauns**, ja to vēl neesat redzējis. Laboratorijas vienumā var būt redzams arī iemesls, kāpēc tam jāgaida, piemēram: "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; laboratorijas rezultātus ievada un apstiprina tikai pēc tā."

Izvēlnes saitei **Mans darbs** ir emblēma ar gaidošo vienumu skaitu. Kamēr lapa ir atvērta, tā katru minūti pārbauda, vai nav jauna darba; kad pienāk kas jauns, saite vienreiz iemirdzas un neliels paziņojums uzskaita grupas, kas ir papildinājušās. Ja nekas negaida, lapā redzams "Nekas jūs negaida." un saites uz lapām, kuras varat atvērt.

## Izvēlne {#menu}

Organizāciju dalībniekiem ir īsa izvēlne ([Table](#tbl-menu)). Katrs vienums tiek rādīts tikai tad, ja jūsu organizācijai ir attiecīgā kompetence un jūsu lomai ir attiecīgās tiesības; tās pašas tiesības lapas pārbauda arī tad, ja tās atverat pēc adreses:

Table: Organizāciju dalībnieku izvēlne {#tbl-menu}

| Izvēlnes vienums | Ko atver | Kad redzams |
|---|---|---|
| **Mans darbs** | Jūsu uzdevumus | Vienmēr |
| **Mana organizācija** | Jūsu organizācijas partijas un gala partijas | Vienmēr |
| **Piegādes (pieņemšana)** | Reģistrētās piegādes | Pārstrādes organizācija; lomas ar pieņemšanas lasīšanas tiesībām |
| **Paraugi un laboratorija** | Nosūtītie un saņemtie paraugi, laboratorijas pārskati | Pārstrādes vai laboratorijas organizācija; lomas ar paraugu lasīšanas tiesībām |
| **Sūtījumi un krājumi** | Jūsu turējumā esošās gala partijas, ienākošie un izejošie sūtījumi, uzglabāšanas ieraksti | Pārstrādes, iepakošanas, vairumtirdzniecības vai mazumtirdzniecības organizācija; lomas ar sūtījumu lasīšanas tiesībām |
| **Biškopju lapas** | Piekrišanas un publiskās biškopju lapas | Biedrība; lomas ar biškopju lapu lasīšanas tiesībām |
| **Biškopju reģistrācija** | Jūsu pārvaldes reģistrācijas | Biškopju reģistrācijas organizācija; biškopju reģistrācijas speciālists |
| **Atbilstība** | Pārskati, parakstītie foto, pieprasījumi, apstiprinājumi, līgumi, audita ieraksti | Atbilstības speciālists |

Platformas administratoriem ir pilna administrēšanas izvēlne (skatīt [15. nodaļu](#admin)).

## Paziņojumi, ielāde un kļūdu lapas {#messages}

- **Paziņojumi** parādās augšējā stūrī. Paziņojumi par veiksmīgu darbību un informatīvi paziņojumi aizveras pēc dažām sekundēm; kļūdu paziņojumi paliek, līdz tos aizverat. Paziņojumā, kas attiecas uz lomu, tiek nosaukta loma, nekad tās kods.
- **Ielādes indikators** (animēta medus bite) parādās, ja lapas atvēršana vai saglabāšana ilgst nedaudz ilgāk.
- **Kļūdu lapas**: "Jūs nevarat atvērt šo lapu" (403) nozīmē, ka jūsu loma neļauj atvērt šo lapu; vērsieties pie savas organizācijas administratora. "Šīs lapas šeit nav" (404) nozīmē, ka adrese neeksistē vai ieraksts jums nav redzams. Lapa 500 parāda īsu atsauci; norādiet to, ziņojot par problēmu. Katrā kļūdas lapā ir poga atpakaļ uz **Mans darbs** un poga **Atpakaļ**.
