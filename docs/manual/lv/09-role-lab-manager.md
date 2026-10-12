# 9. Laboratorijas vadītājs {#role-lab-manager}

## Jūsu ikdienas darbs {#labmanager-daily}

Jūs esat otrā no laboratorijas divām personām: jūs apstiprināt vai atgriežat laboratorijas pārskatus, ko iesnieguši jūsu analītiķi. Jūs varat arī saņemt paraugus. Jūs nekad neapstiprināt pārskatu, kura rezultātus esat ievadījis pats; laboratorijas rezultātiem izņēmumu no šī noteikuma nav.

Jūsu uzdevumi redzami sadaļā **Mans darbs**, grupā **Apstiprināmie laboratorijas pārskati** (un **Saņemamie paraugi**).

## Pārskata apstiprināšana vai atgriešana {#labmanager-decide}

1. Sadaļā **Mans darbs** atveriet **Apstiprināmie laboratorijas pārskati**.
2. Pārbaudiet pārskatu: PDF failu, pārskata numuru, izdošanas datumu un katru vērtību. Vērtības ārpus robežām ir atzīmētas.
3. Lai apstiprinātu, noklikšķiniet uz **Apstiprināt**. Ja kāda vērtība ir ārpus robežām, dialoglodziņā **Vērtības ārpus robežām** jums tiek lūgts to apstiprināt; noklikšķiniet uz **Tomēr apstiprināt**.
4. Lai atgrieztu pārskatu, ierakstiet laukā **iemesls (lai atgrieztu)** iemeslu un noklikšķiniet uz **Atgriezt**: "Pārskats atgriezts analītiķim."

<!-- SHOT: lab-approve -->
Figure: Laboratorijas vadītājs apstiprina vai atgriež pārskatu.

Apstiprinot jūs apliecināt, ka pārskats ir reģistrēts pareizi, nevis to, ka medus atbilst prasībām; vērtības ārpus robežām partijā paliek atzīmētas.

**Ko dara apstiprināšana** ("Laboratorijas pārskats apstiprināts; rezultāti tagad ir partijas KKP 1 vērtības."):

- Pārskats iegūst statusu **Apstiprināts** un kļūst redzams publiskajā pārbaudes lapā.
- Platforma reģistrē, vai laboratorija tajā brīdī bija **akreditēta** (ar tās akreditācijas numuru) un vai tā ir partijas īpašnieka **iekšējā** laboratorija.
- Rezultāti kļūst par partijas **KKP 1** vērtībām. Tos vairs nevar rediģēt, tikai administratori var tos labot. Ja laboratorija ir aptvērusi visus KKP 1 parametrus, KKP 1 uzreiz tiek pabeigts un apstiprināts; pretējā gadījumā tas tiek nodots uzņēmuma kvalitātes nodrošināšanas speciālistam.

**Biežākās kļūdas**

Table: Biežākās kļūdas, apstiprinot laboratorijas pārskatu {#tbl-labmgr-errors}

| Paziņojums | Nozīme |
|---|---|
| "Četru acu princips: persona, kas ievadīja rezultātus, nevar tos apstiprināt (laboratorijas rezultātiem izņēmums netiek piemērots)." | Pārskats jāapstiprina citam laboratorijas vadītājam. |
| "Lai atgrieztu pārskatu, jānorāda iemesls." | Ierakstiet iemeslu. |
| "Saglabātais pārskata fails neatbilst tā SHA-256 jaucējvērtībai; augšupielādējiet to vēlreiz." | Saglabātais PDF fails ir mainījies; analītiķim tas jāaugšupielādē vēlreiz. |
| "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; …" | Apstiprināšana gaida KKP 0. Pārskatu tomēr varat atgriezt. |

## Pārskata publiska pārbaude {#labmanager-verify}

Ikviens var pārbaudīt pārskatu lapā **Pārbaudīt laboratorijas pārskatu** (`/verify-report`): laukā **Pārskata kods** ievadiet kodu un noklikšķiniet uz **Pārbaudīt**. Lapā redzams, vai pārskats ir apstiprināts, laboratorija, tās akreditācija, pārskata numurs, datums un SHA-256 nospiedums. Funkcija **Salīdzināt PDF ar šo nospiedumu (fails paliek jūsu ierīcē)** pārbauda PDF failu apmeklētāja ierīcē (fails netiek augšupielādēts). Netiek rādīti nekādi partijas, biškopja vai lietotāja dati.

<!-- SHOT: verify-report -->
Figure: Laboratorijas pārskata publiska pārbaude tālrunī.
