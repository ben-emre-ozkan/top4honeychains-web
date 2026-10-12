# 4. Przebieg procesu od początku do końca {#flow}

Ten rozdział śledzi jedną partię miodu na platformie: kto wykonuje każdy krok i co musi zostać zakończone, zanim będzie można rozpocząć kolejny ([Figure](#fig-flow-overview)). Rozdziały poświęcone rolom (5–11) szczegółowo opisują poszczególne ekrany.

<!-- SHOT: flow-overview -->
Figure: **Przebieg procesu** partii końcowej zatwierdzonej ostatecznie: każdy etap z jego statusem i organizacją odpowiedzialną.

## Punkty kontrolne (KPK) {#ccp-list}

**Krytyczny punkt kontroli (KPK)** to etap, na którym coś jest mierzone lub sprawdzane. Każdy KPK ma parametry (na przykład temperaturę lub wilgotność), limity i status. KPK 0–3 należą do **partii surowego miodu**, a KPK 4–10 do **partii końcowej** ([Table](#tbl-ccps)).

Table: Krytyczne punkty kontroli (KPK) i ich domyślne role {#tbl-ccps}

| KPK | Należy do | Co jest sprawdzane | Wprowadza (domyślnie) | Zatwierdza (domyślnie) |
|---|---|---|---|---|
| KPK 0 | Partia | Audyt wstępny przy przyjęciu, **przed** badaniem laboratoryjnym | Pracownik przyjmujący próbki w zakładzie | Specjalista ds. zapewnienia jakości w zakładzie |
| KPK 1 | Partia | Pobieranie próbek i wyniki laboratoryjne | Przepisywane z zatwierdzonego raportu laboratoryjnego | Kierownik laboratorium (lub specjalista ds. zapewnienia jakości w zakładzie, zob. niżej) |
| KPK 2 | Partia | Monitorowanie temperatury (podgrzewanie) | Personel zakładu | Specjalista ds. zapewnienia jakości |
| KPK 3 | Partia | Kontrola stanu filtra | Personel | Specjalista ds. zapewnienia jakości |
| KPK 4 | Partia końcowa | Kontrola jednorodności (mieszanie) | Personel właściciela partii końcowej | Specjalista ds. zapewnienia jakości |
| KPK 5 | Partia końcowa | Kontrola czasu i temperatury (pasteryzacja) | Personel | Specjalista ds. zapewnienia jakości |
| KPK 6 | Partia końcowa | Usuwanie piany i osadu | Personel | Specjalista ds. zapewnienia jakości |
| KPK 7 | Partia końcowa | Kontrola ilości (rozlew) | Personel (Konfekcjonowanie) | Specjalista ds. zapewnienia jakości |
| KPK 8 | Partia końcowa | Weryfikacja poprawności etykiety | Personel (Konfekcjonowanie) | Specjalista ds. zapewnienia jakości |
| KPK 9 | Partia końcowa | Kontrola integralności opakowania | Personel (Konfekcjonowanie) | Specjalista ds. zapewnienia jakości |
| KPK 10 | Partia końcowa | Temperatura i wilgotność podczas przechowywania | Personel (Handel hurtowy, Handel detaliczny lub Konfekcjonowanie) | Specjalista ds. zapewnienia jakości |

KPK 10 występuje w życiu partii końcowej dwukrotnie: najpierw **w partii końcowej, w zakładzie** (musi zostać zatwierdzony, zanim partia końcowa zostanie zamknięta), a później jako **rekord przechowywania u każdego odbiorcy** po wysyłce.

Rekord KPK przechodzi przez następujące statusy: **Przypisany** (brak danych) → **W toku** (część danych) → **Przesłany** (wprowadzono wszystkie wymagane dane; oczekuje na zatwierdzenie) → **Zakończony** wraz z zatwierdzeniem. KPK, który nie ma zastosowania, można oznaczyć jako **Nie dotyczy**, podając powód; nadal wymaga on zatwierdzenia. Każda zmiana danych usuwa istniejące zatwierdzenie.

## Krok po kroku {#flow-steps}

[Table](#tbl-flow-steps) wymienia każdy krok, osobę, która go wykonuje, oraz to, co blokuje.

Table: Przebieg procesu krok po kroku {#tbl-flow-steps}

| Nr | Krok | Kto | Gdzie | Kolejny krok jest możliwy dopiero, gdy… |
|---|---|---|---|---|
| 1 | Zarejestrowanie pszczelarza i pasieki | Administrator platformy | **Pszczelarze**, **Pasieki** | Pszczelarz jest aktywny i ma pasiekę |
| 2 | Weryfikacja numeru w rejestrze, przypisanie stowarzyszenia | Administrator platformy | **Rejestr pszczelarzy i stowarzyszenia** | (Wymagane do publicznej strony pszczelarza, nie do dostaw) |
| 3 | Rejestracja dostawy: powstaje partia surowego miodu | Pracownik przyjmujący próbki (zakład) | **Dostawy (przyjęcie)** → **Zarejestruj dostawę** | – (KPK 0–3 są przypisywane do partii automatycznie) |
| 4 | Zaplombowanie próbki i wysłanie jej do laboratorium | Pracownik przyjmujący próbki | **Próbki i laboratorium** → **Pobierz próbkę** | Laboratorium ją przyjmie |
| 5 | Wprowadzenie KPK 0 | Pracownik przyjmujący próbki | **Moja praca** → KPK 0 | KPK 0 zostanie przesłany |
| 6 | Zatwierdzenie KPK 0 | Specjalista ds. zapewnienia jakości (inna osoba) | **Moja praca** → KPK 0 | **Wyniki laboratoryjne można wprowadzić dopiero po zatwierdzeniu KPK 0** |
| 7 | Przyjęcie próbki, kontrola plomby | Analityk lub Kierownik laboratorium | **Moja praca** → **Próbki do przyjęcia** | Naruszona plomba powoduje odrzucenie próbki; należy pobrać nową |
| 8 | Wprowadzenie wyników laboratoryjnych, przesłanie pliku PDF, przesłanie raportu | Analityk | **Moja praca** → **Wyniki laboratoryjne do wprowadzenia** | Dostępne są wszystkie wymagane wartości, plik PDF, numer raportu i data |
| 9 | Zatwierdzenie (lub zwrot) raportu laboratoryjnego | Kierownik laboratorium (inna osoba) | **Moja praca** → **Raporty laboratoryjne do zatwierdzenia** | Zatwierdzenie przepisuje wyniki do KPK 1 partii |
| 10 | Wprowadzenie i zatwierdzenie KPK 2–3 | Personel, a następnie Specjalista ds. zapewnienia jakości | **Moja praca** | Partię można dodać do partii końcowej dopiero wtedy, gdy **wszystkie** jej KPK są zakończone lub oznaczone jako nie dotyczy **i zatwierdzone** |
| 11 | Utworzenie partii końcowej, dodanie partii, ustalenie właściciela, przypisanie KPK 4–10 | **Administrator platformy** (na razie) | **Końcowe Produkcje Miodu** | Partia końcowa ma partie i swoje KPK |
| 12 | Wprowadzenie i zatwierdzenie KPK 4–10, w tym KPK 10 – przechowywanie w zakładzie | Personel, a następnie Specjalista ds. zapewnienia jakości | **Moja praca** | Każdy KPK partii końcowej musi zostać zatwierdzony |
| 13 | Wniosek o ostateczne zatwierdzenie | Personel lub Administrator organizacji właściciela | **Moja organizacja** → partia końcowa → **Wnioskuj o ostateczne zatwierdzenie** | Przycisk jest nieaktywny, dopóki czegokolwiek brakuje |
| 14 | Potwierdzenie ostatecznego zatwierdzenia: partia końcowa zostaje zamknięta | Specjalista ds. zapewnienia jakości (inna osoba) | **Moja praca** → **Wnioski o ostateczne zatwierdzenie** | – Historia miodu i kod QR stają się dostępne publicznie; partia końcowa i jej partie zostają zablokowane |
| 15 | Utworzenie wysyłki | Personel lub Administrator organizacji posiadacza | **Wysyłki i zapasy** → **Wyślij** | Odbiorca potwierdzi wysyłkę |
| 16 | Potwierdzenie (lub odrzucenie) odbioru | Personel lub Administrator organizacji odbiorcy | **Moja praca** → **Wysyłki do potwierdzenia** | Piecza przechodzi dopiero po potwierdzeniu |
| 17 | Rekord przechowywania (KPK 10) u odbiorcy, zatwierdzony | Personel odbiorcy, a następnie jego Specjalista ds. zapewnienia jakości | **Wysyłki i zapasy** → **Przechowywanie** | – |
| 18 | Odbiorca odczytuje zapisy producenta | Odbiorca (każda rola z prawem odczytu KPK) | Strony KPK, tylko do odczytu | – |
| 19 | Konsument skanuje kod QR | Każdy odwiedzający stronę publiczną (bez konta) | Historia miodu | – |

### Dlaczego KPK 0 poprzedza badanie laboratoryjne {#why-ccp0-first}

KPK 0 to audyt wstępny przy przyjęciu: czy ten miód w ogóle nadaje się do przyjęcia? Dlatego laboratorium nie może zapisać, przesłać ani zatwierdzić wyników, dopóki KPK 0 partii nie zostanie zakończony **i zatwierdzony**. Do tego czasu zadanie laboratoryjne wyświetla komunikat „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; wyniki badań wprowadza się i zatwierdza dopiero po nim.” W czasie oczekiwania raport nadal można zwrócić.

### Dlaczego KPK 10 partii końcowej jest potrzebny przed zamknięciem {#why-ccp10}

Ostateczne zatwierdzenie zamyka partię końcową tylko wtedy, gdy każdy przypisany do niej KPK jest zatwierdzony. KPK 10 partii końcowej rejestruje warunki przechowywania w samym zakładzie, dlatego własny rekord przechowywania zakładu musi istnieć przed zamknięciem. Każdy kolejny posiadacz prowadzi własny rekord przechowywania KPK 10 dla swoich zapasów.

### Tworzenie partii końcowych jest na razie zastrzeżone dla administratora {#batch-admin-only}

Utworzenie partii końcowej, wybór jej partii i ilości, ustalenie organizacji będącej właścicielem oraz przypisanie KPK partii końcowej wykonuje administrator platformy. Następnie personel zakładu wprowadza KPK partii końcowej. O tym, czy personel zakładu będzie mógł samodzielnie tworzyć partie końcowe, zdecyduje projekt.

## Co blokuje co: podsumowanie {#flow-blocks}

- **Nie ma dostawy bez** zweryfikowanej organizacji z funkcją Przetwarzanie i roli z prawem do przyjęć.
- **Nie ma wyników laboratoryjnych bez** przyjętej próbki z nienaruszoną plombą i zatwierdzonego KPK 0.
- **Raportu nie może zatwierdzić** osoba, która wprowadziła wyniki.
- **Partii nie można dodać do partii końcowej**, dopóki wszystkie jej KPK nie są zakończone lub oznaczone jako nie dotyczy i zatwierdzone, a partia nie należy do hurtownika partii końcowej.
- **Ostateczne zatwierdzenie jest niemożliwe**, gdy partia końcowa nie zawiera miodu, brakuje KPK partii końcowej lub nie jest on zatwierdzony albo KPK partii jest otwarty; lista „Brakuje przed ostatecznym zatwierdzeniem:” wymienia każdy problem.
- **Ostatecznego zatwierdzenia nie może potwierdzić** osoba, która złożyła wniosek.
- **Nie można wysłać** partii końcowej, która nie została zatwierdzona ostatecznie, ani ilości większej niż posiadana.
- **Nie można zmienić** partii końcowej zatwierdzonej ostatecznie ani jej partii, chyba że w drodze korekty przeprowadzonej przez dwóch administratorów.
