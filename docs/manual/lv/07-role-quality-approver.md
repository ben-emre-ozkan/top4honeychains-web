# 7. Kvalitātes nodrošināšanas speciālists {#role-qa}

## Jūsu ikdienas darbs {#qa-daily}

Jūs esat savas organizācijas "otrais acu pāris". Jūs:

- apstiprināt KKP ierakstus, ko iesnieguši kolēģi (KKP 0, KKP 2–10 un uzglabāšanas ierakstus);
- apstiprināt pabeigšanas (fiksēšanas) pieprasījumus, ar ko gala partija tiek fiksēta;
- pie vairumtirgotāja vai mazumtirgotāja apstiprināt KKP 10 uzglabāšanas ierakstus.

Jūsu uzdevumi redzami sadaļā **Mans darbs**, grupās **Apstiprināmās vērtības** un **Pabeigšanas (fiksēšanas) pieprasījumi**. Jūs nekad neapstiprināt ierakstu, kurā pats esat reģistrējis datus.

## KKP ieraksta apstiprināšana {#qa-approve}

1. Sadaļā **Mans darbs** atveriet vienumu grupā **Apstiprināmās vērtības**, piemēram, "KKP 0 · #20070".
2. Pārbaudiet vērtības. Pie katras vērtības redzams **Robežās** vai **Ārpus robežām** attiecībā pret tās robežvērtību.
3. Kartītē **Apstiprinājums (četru acu princips)** izlasiet piezīmi: apstiprinot jūs apliecināt, ka vērtības ir reģistrētas pareizi, nevis to, ka tās ir robežās.
4. Noklikšķiniet uz **Apstiprināt**.

<!-- SHOT: ccp-approve -->
Figure: KKP ieraksta apstiprināšana (četru acu princips).

Paziņojums "KKP ieraksts apstiprināts." to apstiprina; statusa žetonā redzams **Apstiprināts** ar jūsu vārdu un laiku.

**Vērtības ārpus robežām.** Ja kāda vērtība ir ārpus robežām, dialoglodziņā **Vērtības ārpus robežām** tās tiek uzskaitītas, katrai norādot **Robežvērtība:** (un "robeža vēl nav pārbaudīta" tām robežvērtībām, kas vēl jāpārbauda). Noklikšķiniet uz **Tomēr apstiprināt**, lai apstiprinātu, vai uz **Atcelt**. Vērtības paliek atzīmētas kā ārpus robežām; apstiprinājums tikai apliecina, ka tās ir reģistrētas pareizi.

<!-- SHOT: out-of-limit-dialog -->
Figure: Dialoglodziņš datiem ārpus to robežvērtībām.

**Nepiemērojami ieraksti.** Arī KKP, kas atzīmēts kā "Nav piemērojams", ir jāapstiprina; tas parādās sadaļā **Apstiprināmās vērtības** tiem apstiprinātājiem, kuri to nav atzīmējuši. Pirms apstiprināšanas izlasiet iemeslu.

**Biežākās kļūdas**

Table: Biežākās kļūdas, apstiprinot KKP ierakstu {#tbl-qa-errors}

| Paziņojums | Nozīme |
|---|---|
| "Četru acu princips: jūs reģistrējāt šī KKP vērtības, tāpēc tas jāapstiprina citai personai." | Jūs esat ievadījis vērtības šajā ierakstā. Tas jāapstiprina citam apstiprinātājam. |
| "Četru acu princips: jūs atzīmējāt šo KKP kā "Nav piemērojams", tāpēc tas jāapstiprina citai personai." | Jūs atzīmējāt šo ierakstu kā "Nav piemērojams". Tas jāapstiprina citam apstiprinātājam. |
| "Apstiprināt var tikai iesniegtu (vai kā "Nav piemērojams" atzīmētu) KKP ierakstu." | Vēl trūkst obligāto vērtību. |
| "Šis KKP ieraksts jau ir apstiprināts." | Nekas nav jādara. |
| "Jūsu loma (…) nevar apstiprināt šo kontroles punktu; apstiprināt drīkst lomas: …" | Šo KKP apstiprina cita loma. |

Ja jūsu organizācijai ir piešķirts **Četru acu principa izņēmums**, jūs drīkstat apstiprināt savus ierakstus; tad paziņojumā ir teikts "Apstiprināts, izmantojot četru acu principa izņēmumu (atzīmēts audita žurnālā)." Izņēmums nekad neattiecas uz laboratorijas rezultātiem.

## Pabeigšanas apstiprināšana (gala partijas fiksēšana) {#qa-finalize}

1. Sadaļā **Mans darbs** atveriet vienumu grupā **Pabeigšanas (fiksēšanas) pieprasījumi** vai atveriet gala partiju lapā **Mana organizācija**.
2. Pārbaudiet gala partiju: visiem ierakstiem sadaļās **Gala partijas kontroles punkti** un **Izcelsmes partijas** jābūt apstiprinātiem, un sarakstam "Trūkst pirms pabeigšanas (fiksēšanas):" jābūt tukšam.
3. Noklikšķiniet uz **Apstiprināt un pabeigt (fiksēt)**.

<!-- SHOT: finalize-confirm -->
Figure: Pabeigšanas (fiksēšanas) pieprasījuma apstiprināšana.

Gala partija ir fiksēta: "Gala partija pabeigta (fiksēta)." Vienreiz parādās zīmogs ar saitēm **Atvērt medus stāstu** un **Lejupielādēt QR**. No šī brīža:

- gala partija un visas tās partijas ir bloķētas;
- publiskais Medus stāsts ir pieejams, izmantojot tā QR kodu;
- gala partiju var nosūtīt.

Pabeigšanu (fiksēšanu) nevar atcelt. Vēlāk atrastas kļūdas tiek labotas ar administratoru labojumu procesu.

Kļūdas: "Četru acu princips: persona, kas pieprasīja pabeigšanu (fiksēšanu), nevar to apstiprināt.", "Vispirms jāpieprasa pabeigšana (fiksēšana).", "Gala partiju vēl nevar pabeigt (fiksēt): …".

<!-- SHOT: batch-sealed -->
Figure: Fiksēta gala partija ar saiti uz tās Medus stāstu un QR kodu.

## Uzglabāšanas ierakstu apstiprināšana (vairumtirgotājs vai mazumtirgotājs) {#qa-storage}

1. Sadaļā **Mans darbs** atveriet vienumu ar atzīmi **uzglabāšana** ("KKP 10 · [gala partija]") vai dodieties uz **Sūtījumi un krājumi** → **Uzglabāšana**.
2. Pārbaudiet vērtības un noklikšķiniet uz **Apstiprināt** (ja tiek prasīts, apstipriniet ārpus robežām esošās vērtības dialoglodziņā).

Spēkā ir tas pats četru acu princips: kolēģis, kas reģistrēja vērtības, tās apstiprināt nevar.
