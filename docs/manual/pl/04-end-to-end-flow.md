# 4. Przebieg procesu od początku do końca {#flow}

Ten rozdział śledzi jedną partię miodu na platformie: kto wykonuje każdy krok i co musi zostać zakończone, zanim będzie można rozpocząć kolejny ([Figure](#fig-flow-overview)). Rozdziały poświęcone rolom (5–14) szczegółowo opisują poszczególne ekrany.

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
| 1 | Zarejestrowanie pszczelarza i pierwszej pasieki: numer w rejestrze, tożsamość, informacja o danych i zgody, potwierdzone kodem SMS (lub podpisanym zdjęciem) | Specjalista ds. rejestracji pszczelarzy (powiatowa dyrekcja) lub administrator platformy | **Rejestracja pszczelarzy** | Pszczelarz potwierdzony podpisanym zdjęciem jest wyszukiwany przy przyjęciu dopiero wtedy, gdy Specjalista ds. zgodności zaakceptuje zdjęcie |
| 2 | Weryfikacja numeru w rejestrze, przypisanie stowarzyszenia | Administrator platformy | **Rejestr pszczelarzy i stowarzyszenia** | (Wymagane do publicznej strony pszczelarza, nie do dostaw) |
| 3 | Rejestracja dostawy: każdy pojemnik z wagą i plombą, słoik z próbką i słoik z próbką kontrolną z każdego pojemnika; pszczelarz potwierdza kodem SMS; powstaje partia surowego miodu | Pracownik przyjmujący próbki (zakład) | **Dostawy (przyjęcie)** → **Zarejestruj dostawę** | – (KPK 0–3 są przypisywane do partii automatycznie; słoiki trafiają do wybranego laboratorium) |
| 4 | Wydrukowanie etykiet i naklejenie ich na pojemniki i słoiki | Pracownik przyjmujący próbki | Strona dostawy → **Etykiety** | – |
| 5 | Wprowadzenie KPK 0 | Pracownik przyjmujący próbki | **Moja praca** → KPK 0 | KPK 0 zostanie przesłany |
| 6 | Zatwierdzenie KPK 0 | Specjalista ds. zapewnienia jakości (inna osoba) | **Moja praca** → KPK 0 | **Wyniki laboratoryjne można wprowadzić dopiero po zatwierdzeniu KPK 0** |
| 7 | Przyjęcie każdego słoika: kontrola numeru plomby, etykiety i wieczka | Analityk lub Kierownik laboratorium | **Moja praca** → **Próbki do przyjęcia** | Negatywny wynik kontroli powoduje odrzucenie słoika; zakład pobiera nowy słoik z tego samego pojemnika |
| 8 | Utworzenie próbki zbiorczej z przyjętych słoików (analiza próbki zbiorczej) | Analityk lub Kierownik laboratorium | **Próbki i laboratorium** → **Utwórz próbkę zbiorczą** | Słoik każdego pojemnika musi być przyjęty w dobrym stanie |
| 9 | Wprowadzenie wyników laboratoryjnych, przesłanie pliku PDF, przesłanie raportu | Analityk | **Moja praca** → **Wyniki laboratoryjne do wprowadzenia** | Dostępne są wszystkie wymagane wartości, plik PDF, numer raportu i data |
| 10 | Zatwierdzenie (lub zwrot) raportu laboratoryjnego | Kierownik laboratorium (inna osoba) | **Moja praca** → **Raporty laboratoryjne do zatwierdzenia** | Zatwierdzenie przepisuje wyniki do KPK 1 partii |
| 11 | Wprowadzenie i zatwierdzenie KPK 2–3 | Personel, a następnie Specjalista ds. zapewnienia jakości | **Moja praca** | Partię można dodać do partii końcowej dopiero wtedy, gdy **wszystkie** jej KPK są zakończone lub oznaczone jako nie dotyczy **i zatwierdzone** |
| 12 | Utworzenie partii końcowej, dodanie partii, ustalenie właściciela, przypisanie KPK 4–10 | **Administrator platformy** (na razie) | **Końcowe Produkcje Miodu** | Partia końcowa ma partie i swoje KPK |
| 13 | Wprowadzenie i zatwierdzenie KPK 4–10, w tym KPK 10 – przechowywanie w zakładzie | Personel, a następnie Specjalista ds. zapewnienia jakości | **Moja praca** | Każdy KPK partii końcowej musi zostać zatwierdzony |
| 14 | Wniosek o ostateczne zatwierdzenie | Personel lub Administrator organizacji właściciela | **Moja organizacja** → partia końcowa → **Wnioskuj o ostateczne zatwierdzenie** | Przycisk jest nieaktywny, dopóki czegokolwiek brakuje (również gdy zdjęcie dostawy nie zostało zaakceptowane przez Specjalistę ds. zgodności) |
| 15 | Potwierdzenie ostatecznego zatwierdzenia: partia końcowa zostaje zamknięta | Specjalista ds. zapewnienia jakości (inna osoba) | **Moja praca** → **Wnioski o ostateczne zatwierdzenie** | – Historia miodu i kod QR stają się dostępne publicznie; partia końcowa i jej partie zostają zablokowane; cena pszczelarza zostaje zablokowana |
| 16 | Utworzenie wysyłki | Personel lub Administrator organizacji posiadacza | **Wysyłki i zapasy** → **Wyślij** | Odbiorca potwierdzi wysyłkę |
| 17 | Potwierdzenie (lub odrzucenie) odbioru | Personel lub Administrator organizacji odbiorcy | **Moja praca** → **Wysyłki do potwierdzenia** | Piecza przechodzi dopiero po potwierdzeniu |
| 18 | Rekord przechowywania (KPK 10) u odbiorcy, zatwierdzony | Personel odbiorcy, a następnie jego Specjalista ds. zapewnienia jakości | **Wysyłki i zapasy** → **Przechowywanie** | – |
| 19 | Odbiorca odczytuje zapisy producenta | Odbiorca (każda rola z prawem odczytu KPK) | Strony KPK, tylko do odczytu | – |
| 20 | Konsument skanuje kod QR | Każdy odwiedzający stronę publiczną (bez konta) | Historia miodu | – |

Równolegle do tego przebiegu pszczelarz śledzi w portalu pszczelarza swoje dostawy, próbki i partie końcowe oraz wprowadza swoją cenę oczekiwaną (zob. [rozdział 14](#portal)), a Specjalista ds. zgodności każdej organizacji przegląda podpisane zdjęcia, odrzucone próbki i naruszone plomby (zob. [rozdział 13](#role-co)).

### Rejestracja kodem SMS lub podpisanym zdjęciem {#flow-registration}

Pszczelarza rejestruje się jeden raz; robi to specjalista ds. rejestracji pszczelarzy powiatowej dyrekcji rolnictwa (lub administrator platformy). Specjalista sprawdza numer gospodarstwa w rejestrze (`TR`, kod tablicy rejestracyjnej prowincji i 10 cyfr) pod kątem prowincji dyrekcji, rejestruje pszczelarza i pierwszą pasiekę, daje pszczelarzowi do przeczytania informację o danych i rejestruje jego wybory: dobrowolną zgodę na pokazywanie imienia i nazwiska na stronach publicznych oraz dobrowolną zgodę na powiadomienia SMS. Pszczelarz potwierdza to wszystko **6-cyfrowym kodem**, który przychodzi SMS-em i który odczytuje na głos; kod jest powiązany dokładnie z tą treścią.

Pszczelarz bez telefonu komórkowego podpisuje zamiast tego wydrukowany formularz; specjalista przesyła jego zdjęcie wraz z powodem. Taka rejestracja czeka na **Specjalistę ds. zgodności** dyrekcji; dopóki zdjęcie nie zostanie zaakceptowane, pszczelarz nie może dostarczać miodu („Brak aktywnego pszczelarza o tym numerze.” przy przyjęciu), a odrzucenie zdjęcia wycofuje zgody. Pszczelarz bez telefonu komórkowego nie otrzymuje konta w portalu. Zob. [rozdział 12](#role-ro).

### Pojemniki, plomby i słoiki z próbkami {#flow-containers}

Miód przyjeżdża w **pojemnikach** (puszkach, bębnach, beczkach). Przy przyjęciu pracownik przyjmujący próbki rejestruje jeden wiersz na pojemnik:

1. **Plomba przy przyjęciu**: zaplombowany pojemnik jest sprawdzany (numer plomby i **plomba nienaruszona**); pojemnik bez plomby otrzymuje plombę zakładu w obecności pszczelarza (**Plomba zakładu założona teraz**).
2. **Waga**: kilogramy netto (i opcjonalnie brutto); ilość partii to suma pojemników.
3. **Słoiki z próbkami**: pojemnik zostaje otwarty i napełnia się z niego **słoik z próbką** oraz **słoik z próbką kontrolną**, każdy z własnym numerem plomby i **wieczkiem gwarancyjnym**. Przy plombowaniu słoika wieczko musi być nienaruszone (przycisk wciśnięty); wyskoczony przycisk oznacza, że słoik był otwarty, więc należy go wyrzucić i napełnić nowy.
4. **Nowa plomba** zamyka pojemnik po pobraniu próbki.

Każdy krok jest **zdarzeniem plomby**, do którego można tylko dopisywać, nigdy go zmieniać. Laboratorium otrzymuje słoiki z próbkami; próbki kontrolne pozostają w zakładzie przez czas ustawiony w polu **Przechowywać do**.

**Potwierdzenie pszczelarza.** Pszczelarz otrzymuje SMS z nazwą organizacji, datą, pojemnikami, kilogramami, rodzajem miodu, liczbą próbek i kodem. Po wpisaniu kodu dostawa jest potwierdzona, a jej **odcisk treści** zostaje zapisany. Każda zmiana po wysłaniu kodu go unieważnia i potrzebny jest nowy kod. Jeśli pszczelarz już odjechał, pracownik do niego dzwoni, a pszczelarz odczytuje kod z telefonu. Bez SMS-a (brak telefonu, brak zasięgu) pracownik przesyła **podpisany formularz dostawy** wraz z powodem: na partii można pracować, ale można ją zatwierdzić ostatecznie dopiero wtedy, gdy Specjalista ds. zgodności zakładu zaakceptuje zdjęcie.

<!-- SHOT: intake-containers -->
Figure: Rejestracja dostawy: jeden wiersz na pojemnik z plombami i słoikami z próbkami.

**Etykiety.** Strona dostawy drukuje etykiety z kodami QR: po jednej na każdy pojemnik (`L{lot}-C{n}`), na każdy słoik z próbką (`S-{lot}-C{n}`) i na każdą próbkę kontrolną (`…-K`), na arkuszu A4 (2 × 5) lub po jednej etykiecie 100 × 50 mm na stronę.

### Przyjęcie w laboratorium {#flow-reception}

Dla każdego słoika laboratorium rejestruje trzy kontrole: **Numer plomby zgadza się z zapisem**, **Etykieta jest nienaruszona** i **Wieczko nienaruszone (przycisk wciśnięty)**. Jeśli którakolwiek odpowiedź brzmi **Nie**, słoik zostaje odrzucony z podaniem powodu. Zakład pobiera wtedy **nowy słoik z tego samego pojemnika** (**Otwórz, pobierz i zaplombuj ponownie**), co ponownie narusza plombę pojemnika i zakłada nową; oba zdarzenia są widoczne w historii plomb. Potwierdzona treść dostawy zachowuje pierwszy słoik.

### Próbka zbiorcza lub osobno dla każdego pojemnika {#flow-analysis}

Platforma obsługuje dwa sposoby analizy partii; o tym, który z nich jest stosowany, decyduje ustawienie platformy:

- **Próbka zbiorcza (domyślnie).** Gdy słoik każdego pojemnika został przyjęty w dobrym stanie, laboratorium tworzy **jedną próbkę zbiorczą** z równych części słoików (**Utwórz próbkę zbiorczą**, kod `S-{lot}-COMP`) i ją analizuje. Jej zatwierdzony raport wypełnia KPK 1 partii. Pojedyncze słoiki są przechowywane; gdy próbka zbiorcza nie spełnia wymagań lub na żądanie, bada się pojedynczy słoik. Raport z takiego pojedynczego słoika pozostaje przy słoiku i nie zastępuje wartości KPK 1 partii.
- **Osobno dla każdego pojemnika.** Każdy słoik jest analizowany osobno, a każdy zatwierdzony raport słoika liczy się dla partii.

### Zasada najgorszej wartości {#flow-worst}

Gdy słoiki są analizowane pojedynczo, wartością KPK 1 partii dla każdego parametru jest **najgorsza** wartość spośród pojemników:

- najpierw wartość poza limitami, ta najdalej poza nimi;
- w przeciwnym razie wartość najbliższa limitu, w kierunku, który ma znaczenie (najwyższa, gdy jest maksimum, najniższa, gdy jest minimum);
- bez limitu: wartość najwyższa;
- odpowiedzi tekstowe są zachowywane wszystkie, każda z jej pojemnikiem.

Raport laboratoryjny i raport z wycofania pokazują wartości każdego pojemnika („Wyniki laboratoryjne dla każdego pojemnika”), z wartością partii wytłuszczoną.

### Korekty faktów dostawy {#flow-corrections}

Po potwierdzeniu dostawy jej fakty (data, rodzaj i wagi pojemników, numery plomb) są zablokowane, również dla administratorów. Błąd koryguje **dwóch administratorów platformy**: jeden składa wniosek o korektę z podaniem powodu, drugi ją potwierdza (wnioskodawca nigdy nie decyduje). Pierwotna i poprawiona treść pozostają widoczne na stronie dostawy wraz z odciskami, a pszczelarz otrzymuje informacyjny SMS. Korekta nigdy nie dodaje ani nie usuwa pojemników.

### Dlaczego KPK 0 poprzedza badanie laboratoryjne {#why-ccp0-first}

KPK 0 to audyt wstępny przy przyjęciu: czy ten miód w ogóle nadaje się do przyjęcia? Dlatego laboratorium nie może zapisać, przesłać ani zatwierdzić wyników, dopóki KPK 0 partii nie zostanie zakończony **i zatwierdzony**. Do tego czasu zadanie laboratoryjne wyświetla komunikat „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; wyniki badań wprowadza się i zatwierdza dopiero po nim.” W czasie oczekiwania raport nadal można zwrócić.

### Dlaczego KPK 10 partii końcowej jest potrzebny przed zamknięciem {#why-ccp10}

Ostateczne zatwierdzenie zamyka partię końcową tylko wtedy, gdy każdy przypisany do niej KPK jest zatwierdzony. KPK 10 partii końcowej rejestruje warunki przechowywania w samym zakładzie, dlatego własny rekord przechowywania zakładu musi istnieć przed zamknięciem. Każdy kolejny posiadacz prowadzi własny rekord przechowywania KPK 10 dla swoich zapasów.

### Tworzenie partii końcowych jest na razie zastrzeżone dla administratora {#batch-admin-only}

Utworzenie partii końcowej, wybór jej partii i ilości, ustalenie organizacji będącej właścicielem oraz przypisanie KPK partii końcowej wykonuje administrator platformy. Następnie personel zakładu wprowadza KPK partii końcowej. O tym, czy personel zakładu będzie mógł samodzielnie tworzyć partie końcowe, zdecyduje projekt.

### Dostawy zarejestrowane przed wprowadzeniem pojemników {#flow-legacy}

Dostawy zarejestrowane przed wprowadzeniem procedury pojemników (lub gdy zakład nadal pracuje bez niej) są oznaczone jako **wcześniejsza (pojemniki niezapisane)**: mają jeden podpisany formularz dostawy i jedną próbkę z jedną kontrolą plomby. Pozostają ważne i są zatwierdzane ostatecznie tak jak dotąd.

## Co blokuje co: podsumowanie {#flow-blocks}

- **Nie ma dostawy bez** zweryfikowanej organizacji z funkcją Przetwarzanie, roli z prawem do przyjęć i zarejestrowanego pszczelarza (rejestracja zdjęciem dopiero po jej akceptacji przez Specjalistę ds. zgodności).
- **Nie ma potwierdzonej dostawy bez** kodu SMS pszczelarza dla dokładnie tej treści albo podpisanego zdjęcia wraz z powodem.
- **Nie ma wyników laboratoryjnych bez** przyjętego słoika, którego kontrole plomby, etykiety i wieczka wypadły pomyślnie, oraz zatwierdzonego KPK 0.
- **Raportu nie może zatwierdzić** osoba, która wprowadziła wyniki.
- **Partii nie można dodać do partii końcowej**, dopóki wszystkie jej KPK nie są zakończone lub oznaczone jako nie dotyczy i zatwierdzone, a partia nie należy do hurtownika partii końcowej.
- **Ostateczne zatwierdzenie jest niemożliwe**, gdy partia końcowa nie zawiera miodu, brakuje KPK partii końcowej lub nie jest on zatwierdzony, KPK partii jest otwarty albo zdjęcie dostawy czeka na Specjalistę ds. zgodności; lista „Brakuje przed ostatecznym zatwierdzeniem:” wymienia każdy problem.
- **Ostatecznego zatwierdzenia nie może potwierdzić** osoba, która złożyła wniosek.
- **Nie można wysłać** partii końcowej, która nie została zatwierdzona ostatecznie, ani ilości większej niż posiadana.
- **Nie można zmienić** partii końcowej zatwierdzonej ostatecznie ani jej partii, ani faktów potwierdzonej dostawy, chyba że w drodze korekty przeprowadzonej przez dwóch administratorów.
