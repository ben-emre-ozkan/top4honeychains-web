# 10. Organizācijas administrators {#role-org-admin}

## Jūsu ikdienas darbs {#orgadmin-daily}

Organizācijas administrators ir organizācijas vadītājs platformā. Jūs:

- uzturat aktuālus organizācijas dokumentus (licenci, akreditāciju, nodokļu apliecību, reģistrācijas apliecinājumu);
- uzņēmumā: lasāt piegādes un paraugus, pieprasāt pabeigšanu (fiksēšanu), nosūtāt un apstiprināt sūtījumus;
- pie vairumtirgotāja vai mazumtirgotāja: apstiprināt sūtījumus;
- biedrībā: reģistrējat un atsaucat biškopju piekrišanu publiskai biškopja lapai.

Jūsu organizācijas dalības un kompetences pārvalda platformas administratori. Ja kolēģim nepieciešama piekļuve vai cita loma, vērsieties pie viņiem.

## Jūsu organizācijas lapa {#orgadmin-org-page}

1. Atveriet **Mana organizācija** un noklikšķiniet uz **Organizācijas lapa**.
2. Lapā redzams jūsu organizācijas statuss, kompetences, dokumenti, pārbaužu vēsture un dalībnieki.

<!-- SHOT: org-details -->
Figure: Organizācijas dokumenti ar to SHA-256 nospiedumiem.

### Dokumenta augšupielāde {#orgadmin-documents}

1. Sadaļā ar dokumentiem izvēlieties veidu: **Licence**, **Akreditācija**, **Nodokļu apliecība**, **Reģistrācijas apliecinājums** vai **Cits**.
2. Izvēlieties failu: PDF līdz 10 MB vai JPG, PNG vai WEBP līdz 8 MB.
3. Noklikšķiniet uz **Augšupielādēt**: "Dokuments augšupielādēts."

Platforma saglabā failu ar tā SHA-256 nospiedumu. Dokumentus nevar dzēst. Jaunai organizācijai jābūt vismaz vienam dokumentam, pirms platformas administrators to var pārbaudīt.

## Pabeigšana (fiksēšana) un sūtījumi {#orgadmin-plant}

Uzņēmumā jūs varat izmantot **Pieprasīt pabeigšanu (fiksēšanu)** gala partijai un **Nosūtīt** to tieši tāpat kā darbinieks: skatīt [Pabeigšanas (fiksēšanas) pieprasīšana](#staff-request-finalize) un [Gala partijas nosūtīšana](#staff-ship). Saņēmēja organizācijā jūs varat izmantot **Apstiprināt saņemšanu** vai **Noraidīt**: skatīt [Sūtījuma apstiprināšana vai noraidīšana](#staff-receive).

## Biškopju lapas (biedrības) {#orgadmin-storefronts}

Biedrība var publicēt publisku biškopja lapu (**Biškopja lapa**) katram biedram biškopim, kurš tam rakstiski piekrīt. Lapā redzami tikai pārbaudīti dati: biškopja vārds (tikai ar piekrišanu; pretējā gadījumā neitrāls kods, piemēram, `BK-…`), province un rajons, medus veidi, pārbaudītas partijas un apstiprināti laboratorijas rezultāti, kā arī gala partijas, kurās medus nonāca. Tajā nekad netiek rādīts tālruņa numurs, personas fotoattēls, dzimšanas datums vai precīza dravas atrašanās vieta. Apmeklētāji sazinās ar biškopi ar biedrības starpniecību.

Lapa ir publiska tikai tad, ja izpildīti **visi** šie nosacījumi:

- biškopis ir aktīvs;
- biškopja saimniecības reģistrācijas numuru ir pārbaudījis platformas administrators;
- jūsu biedrība ir pārbaudīta;
- piekrišanu ir reģistrējusi jūsu biedrība (biškopja pašreizējā biedrība), un tā nav atsaukta.

### Piekrišanas reģistrēšana {#orgadmin-consent}

1. Nolasiet biškopim piekrišanas tekstu un lūdziet parakstīt veidlapu.
2. Atveriet **Biškopju lapas** un noklikšķiniet uz **Reģistrēt piekrišanu** blakus biškopim.
3. Atzīmējiet **Piekrišanas teksts tika nolasīts biškopim un parakstīts.**
4. Atzīmējiet **Biškopis piekrīt vārda rādīšanai (pretējā gadījumā tiek rādīts neitrāls kods)** tikai tad, ja biškopis tam piekrita.
5. Aizpildiet lauku **Piekrišanas datums** (ne nākotnē) un laukā **Parakstīts piekrišanas dokuments** pievienojiet parakstīto dokumentu (fotoattēlu vai PDF).
6. Saglabājiet.

<!-- SHOT: consent-form -->
Figure: Biškopja piekrišanas reģistrēšana tālrunī.

Paziņojums ir "Piekrišana reģistrēta; publiskā lapa ir publicēta." vai, ja reģistrācijas numurs vēl nav pārbaudīts, "Piekrišana reģistrēta. Publiskā lapa tiks publicēta, kad reģistrācijas numurs būs pārbaudīts."

**Drukājama kartīte.** Poga **Drukājama kartīte** izdrukā kartīti ar biškopja lapas QR kodu, ko biškopis var izmantot savā tirdzniecības vietā.

### Piekrišanas atsaukšana {#orgadmin-withdraw}

1. Lapā **Biškopju lapas** pēc vēlēšanās aizpildiet lauku **iemesls (neobligāts)** un noklikšķiniet uz **Atsaukt piekrišanu**.
2. Lapa tiek noņemta uzreiz: "Piekrišana atsaukta; publiskā lapa vairs nav publiska."

Kļūdas: "Šim biškopim jau ir aktīva piekrišana; lai reģistrētu jaunu, vispirms atsauciet esošo.", "Apstipriniet, ka piekrišanas teksts tika nolasīts biškopim.", "Pievienojiet parakstīto piekrišanas dokumentu."
