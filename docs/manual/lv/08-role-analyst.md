# 8. Analītiķis {#role-analyst}

## Jūsu ikdienas darbs {#analyst-daily}

Jūs strādājat laboratorijā. Jūs saņemat noplombētos paraugus, ko jums nosūta pārstrādes uzņēmumi, pārbaudāt plombu, analizējat medu, ievadāt rezultātus kopā ar pārskata PDF failu un iesniedzat pārskatu apstiprināšanai savam laboratorijas vadītājam.

Jūsu uzdevumi redzami sadaļā **Mans darbs**, grupās **Saņemamie paraugi** un **Ievadāmie laboratorijas rezultāti**. Izvēlnē redzams vienums **Paraugi un laboratorija**.

## Parauga saņemšana {#analyst-receive}

1. Sadaļā **Mans darbs** atveriet **Saņemamie paraugi** vai dodieties uz **Paraugi un laboratorija** → **Jūsu laboratorijai nosūtītie paraugi**.
2. Salīdziniet plombas numuru uz parauga ar parādīto numuru.
3. Ja plomba ir neskarta, noklikšķiniet uz **Saņemts, plomba neskarta**: "Paraugs saņemts; plomba ir neskarta."
4. Ja plomba ir bojāta, ierakstiet laukā **iemesls (ja plomba ir bojāta)** iemeslu un noklikšķiniet uz **Plomba bojāta: noraidīt**: "Paraugs noraidīts: plomba ir bojāta." Uzņēmumam jāņem jauns paraugs.

<!-- SHOT: sample-receive -->

## Laboratorijas rezultātu ievadīšana {#analyst-results}

1. Sadaļā **Mans darbs** atveriet **Ievadāmie laboratorijas rezultāti**. Paraugam atveras lapa **Laboratorijas pārskats**.
2. Pārbaudiet, vai lapā nav redzams paziņojums "Šīs partijas KKP 0 (sākotnējais audits) vēl nav pabeigts; …". Kamēr partijas KKP 0 nav apstiprināts, veidlapa ir tikai lasāma. Gaidiet, līdz uzņēmuma kvalitātes apstiprinātājs apstiprina KKP 0.
3. Aizpildiet laukus **Pārskata numurs** un **Izdošanas datums** (ne nākotnē).
4. Laukā **Pārskata PDF** izvēlieties failu (PDF līdz 10 MB).
5. Ievadiet katra parametra vērtību. Obligātie parametri ir atzīmēti ar *. Skaitļos var lietot komatu vai punktu.
6. Noklikšķiniet uz **Saglabāt**: "Laboratorijas rezultāti saglabāti."

<!-- SHOT: lab-results-form -->

Pēc saglabāšanas platforma:

- saglabā PDF failu ar tā SHA-256 nospiedumu (tas redzams kopā ar kopēšanas pogu);
- izveido 12 rakstzīmju pārskata kodu (**Pārskata kods**), ko ikviens var pārbaudīt publiskajā pārbaudes lapā;
- salīdzina katru vērtību ar tās robežvērtību (**Robežās** / **Ārpus robežām**).

Kamēr pārskats ir melnraksts, to var saglabāt tik bieži, cik vēlaties.

## Pārskata iesniegšana {#analyst-submit}

1. Lapā redzams "Ievadītās obligātās vērtības: … no …". Poga **Iesniegt apstiprināšanai** ir aktīva tikai tad, ja ir visas obligātās vērtības, PDF fails, pārskata numurs un datums.
2. Noklikšķiniet uz **Iesniegt apstiprināšanai**: "Pārskats iesniegts; tas jāapstiprina laboratorijas vadītājs (citai personai)."

<!-- SHOT: lab-submit -->

Pēc iesniegšanas pārskatu vairs nevar rediģēt, ja vien laboratorijas vadītājs to neatgriež. Atgrieztā pārskatā redzams iemesls; izlabojiet to un iesniedziet vēlreiz.

**Biežākās kļūdas**

| Paziņojums | Kas jādara |
|---|---|
| "Rezultātus ievada tikai saņemtam paraugam ar neskartu plombu." | Vispirms saņemiet paraugu. |
| "Pirms iesniegšanas augšupielādējiet pārskata PDF." | Izvēlieties PDF failu un saglabājiet. |
| "Pirms iesniegšanas ievadiet visas obligātās vērtības: ievadītas … no …." | Aizpildiet trūkstošās vērtības, kas uzskaitītas zem "Trūkst:". |
| "Laboratorijas pārskatam jābūt PDF formātā." | Pārskatam tiek pieņemti tikai PDF faili. |
| "Pārskats ir iesniegts; laboratorijas vadītājs to apstiprina vai atgriež labošanai." | Gaidiet laboratorijas vadītāja lēmumu. |
| "Apstiprinātu laboratorijas pārskatu nevar mainīt; tā vietā pieprasiet labojumu." | Lūdziet platformas administratoram veikt labojumu. |
| "Partija ir bloķēta pabeigtā (fiksētā) gala partijā; laboratorijas vērtības var mainīt tikai ar labojumu." | Partija ir noslēgta gala partijā; melnrakstu vairs nevar saglabāt vai iesniegt. Vērsieties pie platformas administratora. |
