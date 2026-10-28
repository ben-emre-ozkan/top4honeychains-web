# 1. Par TOP4HoneyChains {#about}

> **Melnraksts partneru pārskatīšanai.**

TOP4HoneyChains ir tīmekļa platforma, kas izseko medu no biškopja dravas līdz mazumtirgotāja plauktam un saglabā pārbaudāmu ierakstu par katru posmu. Tā ir izstrādāta projektā TOP4HoneyChains ("Uzticama un ilgtspējīga atvērtā platforma viedām medus vērtību ķēdēm"), ko ICT-AGRI-FOOD 2022. gada uzsaukuma ietvaros finansē partnervalstu nacionālās finansēšanas aģentūras.

Šajā rokasgrāmatā aprakstīta platforma tās pašreizējā darbības veidā, ar ieslēgtu organizāciju modeli. Tā paredzēta visiem, kas piesakās panelī, kā arī platformas administratoriem. Publiskās lapas, kurām konts nav vajadzīgs, aprakstītas tur, kur tas ir svarīgi.

## Kā lietot šo rokasgrāmatu {#how-to-read}

- **1.–4. nodaļā** aprakstīta platforma, kompetences, organizācijas un lomas, pieteikšanās, kā arī viss medus gala partijas ceļš. Izlasiet tās vispirms.
- **5.–13. nodaļā** katrai lomai ir sava nodaļa. Katra sākas ar sadaļu "Jūsu ikdienas darbs", pēc tam soli pa solim aprakstīts katrs uzdevums. Izlasiet savas lomas nodaļu.
- **14. nodaļa** paredzēta biškopjiem: biškopju portāls.
- **15. nodaļa** paredzēta platformas administratoriem.
- **16. nodaļā** aprakstīts audita žurnāls, privātuma paziņojumi un piekrišanas, kā arī tas, ko var lasīt sabiedrība.
- **17. nodaļā** ir terminu skaidrojumi un atbildes uz biežāk sastopamajām problēmām.

Pogu, izvēlņu un lauku nosaukumi rakstīti **treknrakstā** tieši tā, kā tie redzami ekrānā. Sistēmas paziņojumi rakstīti pēdiņās. Attēli un tabulas ir numurēti; attēlu un tabulu saraksti ir pēc satura rādītāja. Piemēros izmantoti testa ieraksti, kuru nosaukumi sākas ar **[DEMO]**; tie nav reālas personas vai uzņēmumi.

> **Šajā versijā mainītie nosaukumi.** Daži ekrānā redzamie nosaukumi šajā platformas versijā ir mainīti, piemēram, loma **Kvalitātes nodrošināšanas speciālists** (agrāk: "Kvalitātes apstiprinātājs"), **Pamatnoteikumi** un kompetenču apraksti, kas sākas ar "Spēj …". [Table](#tbl-renamed-labels) terminu skaidrojumos uzskaita visus vecos un jaunos nosaukumus.

> Latviešu valodas versijā platforma, tāpat kā ekrānā, lieto saīsinājumu **KKP** (kritiskais kontroles punkts). Angļu valodā lieto starptautisko terminu CCP (critical control point), citās valodu versijās – savu saīsinājumu: KKN (turku), PCC (spāņu), KPK (poļu). Pagaidām izņēmums ir lapa **Lomas un atļaujas**, kurā vēl redzams "CCP"; šī rokasgrāmata visur lieto KKP.

## Medus vērtību ķēde {#value-chain}

Medus iziet cauri piecām stacijām. Platforma reģistrē katru no tām:

1. **Drava**: biškopis iegūst medu. Katru biškopi platformā pārstāv **biškopja ieraksts**, ko reģistrē novada lauksaimniecības pārvalde un ko biškopis apstiprina ar kodu, kas saņemts SMS. Biškopim var būt arī personīgs konts **biškopju portālā**.
2. **Piegāde**: biškopis piegādā medu pārstrādes uzņēmumam tvertnēs. Uzņēmums reģistrē katru tvertni ar tās svaru un plombām, biškopis apstiprina piegādi ar SMS kodu, un piegāde kļūst par *neapstrādāta medus partiju*, ko pārbauda pieņemšanas brīdī (KKP 0).
3. **Laboratorija**: uzņēmums no katras tvertnes piepilda noplombētu paraugu burku un nosūta burkas uz pārtikas analīžu laboratoriju (parasti akreditētu). Laboratorija pārbauda katru burku un analizē medu, analītiķis ievada rezultātus, un otra persona – laboratorijas vadītājs – apstiprina pārskatu.
4. **Apstrāde**: uzņēmums apstrādā medu (partijas KKP 2–3), apvieno partijas *gala partijā* un reģistrē apstrādes, pildīšanas, marķēšanas, iepakošanas un uzglabāšanas pārbaudes (gala partijas KKP 4–10). Pēc tam divas personas gala partiju *pabeidz (fiksē)*.
5. **Plaukts**: gala partija tiek nosūtīta vairumtirgotājam vai mazumtirgotājam. Saņēmējs apstiprina saņemšanu un reģistrē savus uzglabāšanas apstākļus.

Šajā rokasgrāmatā **dati** ir tas, ko cilvēki reģistrē platformā: izmērītās vērtības (piemēram, mitrums %) un atbildes uz jautājumiem (piemēram, Jā / Nē).

**Četru acu princips.** Katru datu vienību ievada viena persona, un to apstiprina otra, cita persona; neviens nevar apstiprināt to, ko pats ievadījis. Divi "acu pāri" ievērojami samazina kļūdu un manipulāciju iespēju. Precīzi noteikumi uzskaitīti sadaļā [Pamatnoteikumi](#fixed-rules). Katras izmaiņas turklāt tiek ierakstītas pret viltošanu aizsargātā audita žurnālā.

<!-- SHOT: intro-chain -->
Figure: Medus vērtību ķēdes piecas stacijas, kā tās parādītas publiskajā sākumlapā.

## Izsekojamība {#traceability}

Izsekojamība nozīmē, ka par jebkuru medus burku var atbildēt uz trim jautājumiem:

- **No kurienes tas nāk?** Gala partija zina savas neapstrādātā medus partijas, un katra partija zina savu dravu, biškopi, piegādi un laboratorijas pārskatu.
- **Vai tas tika pārbaudīts?** Katram partijas un gala partijas kritiskajam kontroles punktam ir reģistrēti dati, kā arī norādīts, kas tos ievadīja, kas apstiprināja un kad.
- **Kur tas nonāca?** Sūtījumi parāda, kuras organizācijas turējumā ir kāds gala partijas daudzums.

Ja rodas problēma, **Atsaukšanas pārskats** parāda visas partijas augšupējā ķēdē un visus sūtījumus lejupējā ķēdē attiecībā uz gala partiju vai partiju (skatīt [Atsaukšana](#admin-recall)).

## Kam platforma paredzēta {#audience}

[Table](#tbl-audience) uzskaita visus, kas lieto platformu. Organizācijas un to lomas aprakstītas [2. nodaļā](#roles).

Table: Kam platforma paredzēta {#tbl-audience}

| Kas | Konts | Ko viņi dara platformā |
|---|---|---|
| Biškopji | viņus pārstāv **biškopja ieraksts**; pēc izvēles portāla konts | Piegādā medu un apstiprina piegādes ar SMS kodu; portālā: seko savām piegādēm, paraugiem un gala partijām, ievada savu prasīto cenu, dod vai atsauc piekrišanas |
| Novadu lauksaimniecības pārvaldes | organizācijas dalībnieki | Reģistrē biškopjus un viņu dravas, apliecina viņu identitāti un piekrišanas, nosūta viņu pieteikšanās datus portālam |
| Biškopju biedrības | organizācijas dalībnieki | Sasaista savus biedrus biškopjus, reģistrē viņu piekrišanu publiskai biškopja lapai |
| Pārstrādes un iepakošanas uzņēmumi | organizācijas dalībnieki | Reģistrē piegādes, ņem paraugus, reģistrē kontroles punktus, pieprasa un apstiprina pabeigšanu (fiksēšanu), nosūta preces |
| Pārtikas analīžu laboratorijas | organizācijas dalībnieki | Saņem paraugus, ievada rezultātus, apstiprina pārskatus |
| Vairumtirgotāji | organizācijas dalībnieki | Apstiprina sūtījumus, reģistrē uzglabāšanas apstākļus, lasa ražotāja ierakstus |
| Mazumtirgotāji | organizācijas dalībnieki | Apstiprina sūtījumus, reģistrē uzglabāšanas apstākļus, lasa ražotāja ierakstus |
| Valsts iestādes | organizācijas dalībnieki | Lasa ierakstus (tikai lasīšanas piekļuve) |
| Atbilstības speciālisti | loma katrā organizācijā | Pārskata pierādījumus, paraugu ņemšanas kvalitāti un apstiprinājumus; apstrādā datu aizsardzības pieprasījumus |
| Platformas administratori | globāla loma lietotāja kontā | Pārbauda organizācijas, pārvalda dalības, nosaka kontroles punktus un robežvērtības, publicē paziņojumu tekstus, izveido gala partijas, lasa audita žurnālu |
| Patērētāji un citi publiski apmeklētāji | nav | Lasa publiskās lapas: Medus stāstu, ko atver ar QR kodu uz burkas, biškopju lapas, laboratorijas pārskatu pārbaudi |

## Publiskais Medus stāsts un QR kodi {#honey-story}

Kad gala partija ir pabeigta (fiksēta), platforma izveido tās publisko lapu **Medus stāsts** un QR kodu, kas uz to norāda. QR kodu var uzdrukāt uz etiķetes. Jebkurš publisks apmeklētājs to var noskenēt bez konta.

Medus stāsts parāda valodā, ko apmeklētājs izvēlas lapā:

- gala partijas nosaukumu, daudzumu un ražošanas datumu;
- medus ceļu: ievākšanu, piegādi, laboratoriju, apstrādi un plauktu;
- medus izcelsmi kartē: pašlaik pa Turcijas provincēm; citu partnervalstu kartes ir plānotas;
- biškopjus, kuri piedalījās: ar vārdu tikai tad, ja biškopis tam piekritis, citādi ar neitrālu kodu, ar saiti uz viņu publisko biškopja lapu, ja tāda ir;
- laboratorijas rezultātus, norādot, vai vērtība ir robežās vai ārpus tām, tikai tām robežvērtībām, kas ir pārbaudītas;
- apstrādes un iepakošanas kontroles punktus;
- saiti laboratorijas pārskata pārbaudei.

Medus stāstā nekad netiek rādīts biškopja tālruņa numurs, personas fotoattēls, precīza dravas atrašanās vieta vai brīva teksta piezīmes, kurās varētu būt personas dati. Skatīt [Ko var lasīt sabiedrība](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: Pabeigtas (fiksētas) gala partijas publiskais Medus stāsts tālrunī.

Citas publiskās lapas:

- **Biškopja lapa** (`/storefront/<code>`): biškopja publiskā lapa, kas tiek publicēta tikai ar biškopja parakstītu piekrišanu un pārbaudītu reģistrācijas numuru.
- **Pārbaudīt laboratorijas pārskatu** (`/verify-report`): ikviens var ievadīt pārskata kodu, izlasīt, vai pastāv apstiprināts pārskats, un salīdzināt PDF failu ar tā nospiedumu.
- **Privātums un piekrišana** (`/privacy`): spēkā esošie privātuma paziņojumi biškopjiem un platformas lietotājiem.
- **Sākumlapa** (`/`): projekta mājaslapa ar pieteikšanās pogu.

<!-- SHOT: landing-logos -->
Figure: Sākumlapas kājene: projekta partneri un finansētājorganizācijas.
