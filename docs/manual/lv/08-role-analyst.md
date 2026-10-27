# 8. Analītiķis {#role-analyst}

## Jūsu ikdienas darbs {#analyst-daily}

Jūs strādājat pārtikas analīžu laboratorijā un esat pirmā no tās divām personām: jūs ievadāt, laboratorijas vadītājs apstiprina. Jūs saņemat noplombētās paraugu burkas, ko jums nosūta pārstrādes uzņēmumi (no katras piegādes tvertnes pa vienai), pārbaudāt plombu, etiķeti un vāciņu, sagatavojat kopparaugu, analizējat medu, ievadāt rezultātus kopā ar pārskata PDF failu un iesniedzat pārskatu apstiprināšanai savam laboratorijas vadītājam.

Jūsu uzdevumi redzami sadaļā **Mans darbs**, grupās **Saņemamie paraugi** un **Ievadāmie laboratorijas rezultāti**. Izvēlnē redzams vienums **Paraugi un laboratorija**.

## Parauga saņemšana {#analyst-receive}

1. Sadaļā **Mans darbs** atveriet **Saņemamie paraugi** vai dodieties uz **Paraugi un laboratorija** → **Jūsu laboratorijai nosūtītie paraugi**. Katrai burkai redzams tās kods (`S-{lot}-C{n}`), tvertne un plombas numurs.
2. Atbildiet uz trim burkas pārbaudēm: **Plombas numurs atbilst ierakstam**, **Etiķete ir neskarta** un **Vāciņš neskarts (poga nospiesta)**.
3. Ja visas trīs atbildes ir **Jā**, noklikšķiniet uz **Reģistrēt pieņemšanu**: burka ir saņemta.
4. Ja kāda pārbaude ir **Nē**, aizpildiet **Iemesls, ja pārbaude neizdodas** un noklikšķiniet uz **Reģistrēt pieņemšanu**: burka tiek noraidīta. Uzņēmums paņem jaunu burku no tās pašas tvertnes un nosūta to jums.

<!-- SHOT: sample-receive -->
Figure: Paraugu burku saņemšana laboratorijā: plomba, etiķete un vāciņš.

Agrākas piegādes paraugam (viens paraugs partijai) ir viena plombas pārbaude: **Saņemts, plomba neskarta** vai **iemesls (ja plomba ir bojāta)** un **Plomba bojāta: noraidīt**.

## Kopparauga sagatavošana {#analyst-composite}

Kopparauga analīzē (noklusējums) laboratorija analizē vienu kopparaugu katrai partijai (skatīt [Kopparaugs vai katra tvertne atsevišķi](#flow-analysis)). Kad visas partijas tvertņu burkas ir pienākušas kārtībā, partija parādās sarakstā **Partijas, kas gatavas kopparaugam**: "Šo partiju katras tvertnes burka pienāca kārtībā. Sajauciet vienādas daļas no burkām vienā kopparaugā un analizējiet to; burkas glabā atsevišķām analīzēm."

1. Sajauciet vienādas daļas no burkām.
2. Noklikšķiniet uz **Izveidot kopparaugu**. Tiek izveidots un saņemts kopparaugs `S-{lot}-COMP`; tā pārskatu ievada tāpat kā jebkuru citu.

Ja kopparaugs neatbilst prasībām vai pēc pieprasījuma, varat ievadīt pārskatu arī atsevišķai burkai; tas paliek pie šīs burkas un nemaina partijas KKP 1 vērtības. Analizējot katru tvertni atsevišķi, kopparauga nav: katrai burkai ir savs pārskats, un partijas KKP 1 iegūst katra parametra sliktāko vērtību (skatīt [Sliktākās vērtības noteikums](#flow-worst)).

## Laboratorijas rezultātu ievadīšana {#analyst-results}

1. Sadaļā **Mans darbs** atveriet **Ievadāmie laboratorijas rezultāti**. Paraugam atveras lapa **Laboratorijas pārskats**.
2. Pārbaudiet, vai lapā nav redzams paziņojums "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; …". Kamēr partijas KKP 0 nav apstiprināts, veidlapa ir tikai lasāma. Gaidiet, līdz uzņēmuma kvalitātes nodrošināšanas speciālists apstiprina KKP 0.
3. Aizpildiet laukus **Pārskata numurs** un **Izdošanas datums** (ne nākotnē).
4. Laukā **Pārskata PDF** izvēlieties failu (PDF līdz 10 MB).
5. Ievadiet katra parametra vērtību. Obligātie parametri ir atzīmēti ar *. Skaitļos var lietot komatu vai punktu.
6. Noklikšķiniet uz **Saglabāt**: "Laboratorijas rezultāti saglabāti."

<!-- SHOT: lab-results-form -->
Figure: Laboratorijas rezultātu ievadīšana kopā ar pārskata PDF.

Pēc saglabāšanas platforma:

- saglabā PDF failu ar tā SHA-256 nospiedumu (tas redzams kopā ar kopēšanas pogu);
- izveido 12 rakstzīmju pārskata kodu (**Pārskata kods**), ko ikviens var pārbaudīt publiskajā pārbaudes lapā;
- salīdzina katru vērtību ar tās robežvērtību (**Robežās** / **Ārpus robežām**).

Kamēr pārskats ir melnraksts, to var saglabāt tik bieži, cik vēlaties.

## Pārskata iesniegšana {#analyst-submit}

1. Lapā redzams "Ievadītās obligātās vērtības: … no …"; skaits mainās, rakstot, un "Trūkst:" nosauc vēl tukšās vērtības. Poga **Iesniegt apstiprināšanai** ir aktīva tikai tad, ja visas obligātās vērtības, PDF fails, pārskata numurs un datums ir **saglabāti**: ja viss ir ievadīts, bet vēl nav saglabāts, lapā redzams "Pirms iesniegšanas saglabājiet vērtības."
2. Noklikšķiniet uz **Iesniegt apstiprināšanai**: "Pārskats iesniegts; tas jāapstiprina laboratorijas vadītājs (citai personai)."

<!-- SHOT: lab-submit -->
Figure: Ievadīti visi obligātie dati: pārskatu var iesniegt.

Pēc iesniegšanas pārskatu vairs nevar rediģēt, ja vien laboratorijas vadītājs to neatgriež. Atgrieztā pārskatā redzams iemesls; izlabojiet to un iesniedziet vēlreiz.

**Biežākās kļūdas**

Table: Biežākās kļūdas, ievadot laboratorijas rezultātus {#tbl-lab-errors}

| Paziņojums | Kas jādara |
|---|---|
| "Rezultātus ievada tikai saņemtam paraugam ar neskartu plombu." | Vispirms saņemiet burku; noraidītai burkai rezultātus neievada. |
| "Pirms iesniegšanas augšupielādējiet pārskata PDF." | Izvēlieties PDF failu un saglabājiet. |
| "Pirms iesniegšanas ievadiet visas obligātās vērtības: ievadītas … no …." | Aizpildiet trūkstošās vērtības, kas uzskaitītas zem "Trūkst:". |
| "Laboratorijas pārskatam jābūt PDF formātā." | Pārskatam tiek pieņemti tikai PDF faili. |
| "Pārskats ir iesniegts; laboratorijas vadītājs to apstiprina vai atgriež labošanai." | Gaidiet laboratorijas vadītāja lēmumu. |
| "Apstiprinātu laboratorijas pārskatu nevar mainīt; tā vietā pieprasiet labojumu." | Lūdziet platformas administratoram veikt labojumu. |
| "Partija ir bloķēta pabeigtā (fiksētā) gala partijā; laboratorijas vērtības var mainīt tikai ar labojumu." | Partija ir noslēgta gala partijā; melnrakstu vairs nevar saglabāt vai iesniegt. Vērsieties pie platformas administratora. |
