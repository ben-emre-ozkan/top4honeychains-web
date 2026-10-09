> **Melnraksts partneru pārskatīšanai.**

# 1. Par TOP4HoneyChains {#about}

TOP4HoneyChains ir tīmekļa platforma, kas izseko medu no biškopja dravas līdz veikala plauktam un saglabā pārbaudāmu ierakstu par katru posmu. Tā ir izstrādāta TÜBİTAK projektā TOP4HoneyChains ("Uzticama un ilgtspējīga atvērtā platforma viedām medus vērtību ķēdēm").

Šajā rokasgrāmatā aprakstīta platforma tās pašreizējā darbības veidā, ar ieslēgtu organizāciju modeli. Tā paredzēta visiem, kas piesakās panelī, kā arī platformas administratoriem. Publiskās lapas, kurām konts nav vajadzīgs, aprakstītas tur, kur tas ir svarīgi.

## Kā lietot šo rokasgrāmatu {#how-to-read}

- **1.–4. nodaļā** aprakstīta platforma, pieteikšanās, organizācijas un lomas, kā arī viss medus gala partijas ceļš. Izlasiet tās vispirms.
- **5.–11. nodaļā** katrai lomai ir sava nodaļa. Katra sākas ar sadaļu "Jūsu ikdienas darbs", pēc tam soli pa solim aprakstīts katrs uzdevums. Izlasiet savas lomas nodaļu.
- **12. nodaļa** paredzēta platformas administratoriem.
- **13. nodaļā** aprakstīts audita žurnāls un tas, ko redz sabiedrība.
- **14. nodaļā** ir terminu skaidrojumi un atbildes uz biežāk sastopamajām problēmām.

Pogu, izvēlņu un lauku nosaukumi rakstīti **treknrakstā** tieši tā, kā tie redzami ekrānā. Sistēmas paziņojumi rakstīti pēdiņās. Piemēros izmantoti testa ieraksti, kuru nosaukumi sākas ar **[DEMO]**; tie nav reālas personas vai uzņēmumi.

> Latviešu valodas versijā platforma, tāpat kā ekrānā, lieto saīsinājumu **KKP** (kritiskais kontroles punkts). Angļu valodā lieto starptautisko terminu CCP (critical control point), citās valodu versijās – savu saīsinājumu: KKN (turku), PCC (spāņu), KPK (poļu).

## Medus vērtību ķēde {#value-chain}

Medus iziet cauri piecām stacijām. Platforma reģistrē katru no tām:

1. **Drava**: biškopis iegūst medu. Biškopji nepiesakās platformā; viņi ir ieraksti, kurus uztur platforma un viņu biedrība.
2. **Piegāde**: biškopis piegādā medu pārstrādes uzņēmumam. Uzņēmums reģistrē piegādi, tādējādi izveidojot *neapstrādāta medus partiju*, un pārbauda to pieņemšanas brīdī (KKP 0).
3. **Laboratorija**: uzņēmums noplombē paraugu un nosūta to uz laboratoriju. Laboratorija ievada analīzes rezultātus, un otra laboratorijas persona apstiprina pārskatu.
4. **Apstrāde**: uzņēmums apstrādā medu (partijas KKP 2–3), apvieno partijas *gala partijā* un reģistrē apstrādes, pildīšanas, marķēšanas, iepakošanas un uzglabāšanas pārbaudes (gala partijas KKP 4–10). Pēc tam divas personas gala partiju *pabeidz (fiksē)*.
5. **Plaukts**: gala partija tiek nosūtīta vairumtirgotājam vai veikalam. Saņēmējs apstiprina saņemšanu un reģistrē savus uzglabāšanas apstākļus.

Katru vērtību ievada viena persona, un to apstiprina cita (*četru acu princips*). Katras izmaiņas tiek ierakstītas pret viltošanu aizsargātā audita žurnālā.

<!-- SHOT: intro-chain -->

## Izsekojamība {#traceability}

Izsekojamība nozīmē, ka par jebkuru medus burku var atbildēt uz trim jautājumiem:

- **No kurienes tas nāk?** Gala partija zina savas neapstrādātā medus partijas, un katra partija zina savu dravu, biškopi, piegādi un laboratorijas pārskatu.
- **Vai tas tika pārbaudīts?** Katram partijas un gala partijas kritiskajam kontroles punktam ir reģistrētas vērtības, kā arī norādīts, kas tās ievadīja, kas apstiprināja un kad.
- **Kur tas nonāca?** Sūtījumi parāda, kuras organizācijas turējumā ir kāds gala partijas daudzums.

Ja rodas problēma, **Atsaukšanas pārskats** parāda visas partijas augšupējā ķēdē un visus sūtījumus lejupējā ķēdē attiecībā uz gala partiju vai partiju (skatīt [Atsaukšana](#admin-recall)).

## Kam platforma paredzēta {#audience}

| Kas | Ko viņi dara platformā |
|---|---|
| Biškopju biedrības | Sasaista savus biedrus biškopjus, reģistrē viņu piekrišanu publiskai biškopja lapai |
| Pārstrādes un iepakošanas uzņēmumi | Reģistrē piegādes, ņem paraugus, reģistrē kontroles punktus, pieprasa un apstiprina pabeigšanu (fiksēšanu), nosūta preces |
| Laboratorijas | Saņem paraugus, ievada rezultātus, apstiprina pārskatus |
| Vairumtirgotāji un veikali | Apstiprina sūtījumus, reģistrē uzglabāšanas apstākļus, skata ražotāja ierakstus |
| Valsts iestādes | Skata ierakstus (tikai lasīšanas piekļuve) |
| Platformas administratori | Pārbauda organizācijas, pārvalda dalībniekus, nosaka kontroles punktus un robežvērtības, izveido gala partijas, uztur audita žurnālu |
| Patērētāji | Atver publisko Medus stāstu, noskenējot QR kodu uz burkas |

## Publiskais Medus stāsts un QR kodi {#honey-story}

Kad gala partija ir pabeigta (fiksēta), platforma izveido tās publisko lapu **Medus stāsts** un QR kodu, kas uz to norāda. QR kodu var uzdrukāt uz etiķetes. Ikviens to var noskenēt bez konta.

Medus stāsts apmeklētāja valodā parāda:

- gala partijas nosaukumu, daudzumu un ražošanas datumu;
- medus ceļu: ievākšanu, piegādi, laboratoriju, apstrādi un plauktu;
- medus izcelsmi pēc provinces Turcijas kartē;
- biškopjus, kuri piedalījās, ar saiti uz viņu publisko biškopja lapu, ja viņi tai ir piekrituši;
- laboratorijas rezultātus, norādot, vai vērtība ir robežās vai ārpus tām, tikai tām robežvērtībām, kas ir pārbaudītas;
- apstrādes un iepakošanas kontroles punktus;
- saiti laboratorijas pārskata pārbaudei.

Medus stāstā nekad netiek rādīts biškopja tālruņa numurs, personas fotoattēls, precīza dravas atrašanās vieta vai brīva teksta piezīmes, kurās varētu būt personas dati. Skatīt [Ko redz sabiedrība](#trust-public).

<!-- SHOT: public-honey-story -->

Citas publiskās lapas:

- **Biškopja lapa** (`/storefront/<code>`): biškopja publiskā lapa, kas tiek publicēta tikai ar biškopja parakstītu piekrišanu un pārbaudītu reģistrācijas numuru.
- **Pārbaudīt laboratorijas pārskatu** (`/verify-report`): ikviens var ievadīt pārskata kodu un redzēt, vai pārskats pastāv un ir apstiprināts, kā arī salīdzināt PDF failu ar tā nospiedumu.
- **Sākumlapa** (`/`): projekta mājaslapa ar pieteikšanās pogu.
