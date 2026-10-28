# 5. Pracownik przyjmujący próbki {#role-sao}

## Twoja codzienna praca {#sao-daily}

Pracujesz w punkcie przyjęć zakładu przetwórczego. Gdy pszczelarz dostarcza miód:

1. rejestrujesz dostawę pojemnik po pojemniku, wraz z plombami, i napełniasz z każdego pojemnika słoik z próbką i słoik z próbką kontrolną;
2. pozwalasz pszczelarzowi potwierdzić dostawę kodem, który otrzymuje SMS-em;
3. drukujesz etykiety i wysyłasz słoiki z próbkami do laboratorium;
4. wprowadzasz KPK 0, czyli audyt wstępny partii, aby specjalista ds. zapewnienia jakości mógł go zatwierdzić.

Twoje zadania pojawiają się na stronie **Moja praca** w grupie **Wartości do wprowadzenia**. Menu zawiera pozycje **Dostawy (przyjęcie)** oraz **Próbki i laboratorium**.

## Rejestracja dostawy {#sao-intake}

„Zapisz każdy pojemnik dostawy z jego plombą, pobierz próbkę z każdego pojemnika, a potem pszczelarz potwierdza całość kodem SMS.”

1. Otwórz **Dostawy (przyjęcie)** i kliknij **Zarejestruj dostawę**.
2. W polu **Numer działalności lub numer w rejestrze pszczelarza** wpisz dokładnie numer działalności pszczelarza lub jego zweryfikowany numer gospodarstwa w rejestrze i kliknij **Znajdź**. Jeśli nic nie zostanie znalezione, wyświetli się komunikat „Brak aktywnego pszczelarza o tym numerze.” Sprawdź numer. Pszczelarzy rejestruje specjalista ds. rejestracji pszczelarzy; pszczelarz zarejestrowany za pomocą podpisanego zdjęcia może dostarczać miód dopiero wtedy, gdy Specjalista ds. zgodności zaakceptuje zdjęcie. Oznaczenie „brak telefonu: tylko zdjęcie podpisu” oznacza, że pszczelarz nie może otrzymać kodu SMS.
3. **Dostawa**: wybierz **Pasieka**, **Rodzaj miodu** i **Okres produkcji** (bieżący jest wybrany domyślnie), **Kraj pochodzenia** oraz **Data dostawy** (domyślnie dzisiejsza; nie może być datą przyszłą).
4. **Pojemniki i próbki**: jeden wiersz na pojemnik ([Figure](#fig-intake-form)).
   - **Rodzaj** pojemnika, **Kg netto** i opcjonalnie **Kg brutto** (jako separator dziesiętny można użyć przecinka lub kropki).
   - **Przy przyjęciu**: zaznacz **przyjechał zaplombowany** i wpisz **Numer plomby**, zaznaczając **plomba nienaruszona**, jeśli tak jest; w przypadku pojemnika bez plomby wpisz numer w polu **Plomba zakładu założona teraz**, w obecności pszczelarza.
   - Otwórz pojemnik i napełnij słoik z próbką oraz słoik z próbką kontrolną. Wpisz **Plomba słoika** i **Plomba próbki kontrolnej**.
   - Zaznacz **Wieczko nienaruszone** tylko wtedy, gdy przycisk wieczka jest wciśnięty. „Wieczko z zabezpieczeniem: zaznacz tylko, gdy przycisk jest wciśnięty. Wyskoczony przycisk oznacza otwarcie słoika: wyrzuć go i napełnij nowy.”
   - Wpisz **Nowa plomba po pobraniu**, która ponownie zamyka pojemnik.
   - **Dodaj pojemnik** dodaje wiersz; **Usuń wiersz** usuwa wiersz. **Razem netto** sumuje pojemniki.
5. **Laboratorium i próbki kontrolne**: wybierz **Laboratorium** (na liście są tylko zweryfikowane laboratoria; przy laboratorium akredytowanym wyświetla się numer akredytacji), **Gdzie przechowywane są próbki kontrolne** oraz **Przechowywać do** (domyślnie 12 miesięcy naprzód).
6. **Pszczelarz potwierdza**: kliknij **Wyślij kod**. Pszczelarz otrzymuje SMS z nazwą Twojej organizacji, datą, pojemnikami, kilogramami, rodzajem miodu, liczbą próbek i kodem. Wpisz **Kod z SMS (6 cyfr)**, który pszczelarz odczytuje na głos, i kliknij **Potwierdź dostawę**. „Jeśli pszczelarza nie ma na miejscu, zadzwoń: odczyta kod z telefonu.”

<!-- SHOT: intake-form -->
Figure: Rejestracja dostawy: pojemniki, plomby i słoiki z próbkami.

Komunikat „Dostawa potwierdzona przez pszczelarza i zapisana jako partia #….” potwierdza utworzenie nowej partii. Platforma przypisuje do partii KPK 0–3, zapisuje każdą plombę jako zdarzenie plomby i wysyła po jednym słoiku z próbką z każdego pojemnika do wybranego laboratorium.

**Kod.** Kod jest ważny przez 5 minut i można go wpisać 3 razy; **Wyślij nowy kod** wysyła kolejny kod po minucie. Jeśli po wysłaniu kodu cokolwiek zmienisz, kod traci ważność („Dane zmieniły się po wysłaniu kodu. Wyślij nowy kod.”). Po 10 błędnych kodach w ciągu godziny musisz odczekać 15 minut, a jeden pracownik wysyła najwyżej 30 kodów na godzinę.

**SMS niemożliwy: zdjęcie podpisu.** Jeśli pszczelarz nie ma telefonu komórkowego albo SMS-a nie da się wysłać, otwórz **SMS niemożliwy: zdjęcie podpisu**, wydrukuj formularz dostawy, daj go pszczelarzowi do podpisania, wybierz **Zdjęcie podpisanego formularza dostawy** (aparat lub plik: PDF do 10 MB lub JPG, PNG albo WEBP do 8 MB), wpisz **Dlaczego bez kodu SMS** i kliknij **Zapisz ze zdjęciem podpisu**: „Dostawę zapisano jako partię #… ze zdjęciem podpisu; czeka na specjalistę ds. zgodności.” Na partii można pracować, ale nie można jej zatwierdzić ostatecznie, dopóki Specjalista ds. zgodności zakładu nie zaakceptuje zdjęcia.

**Częste błędy**

Table: Częste błędy przy rejestracji dostawy {#tbl-intake-errors}

| Komunikat | Co zrobić |
|---|---|
| „Zapisz co najmniej jeden pojemnik.” | Dodaj wiersz dla każdego pojemnika. |
| „Pojemnik …: wpisz wagę netto (więcej niż 0 kg).” / „…: waga brutto nie może być mniejsza niż netto.” | Popraw wagi tego pojemnika. |
| „Pojemnik …: wpisz numer plomby, z którą przyjechał.” / „…: załóż plombę zakładu przy pszczelarzu i wpisz jej numer.” | Każdy pojemnik potrzebuje plomby, z którą przyjechał, lub plomby zakładu. |
| „Pojemnik …: wpisz numery plomb słoika z próbką i słoika z próbką kontrolną.” | Oba słoiki każdego pojemnika są zaplombowane. |
| „Pojemnik …: przy plombowaniu wieczko słoika musi być nienaruszone (przycisk wciśnięty). Wyskoczony przycisk: wyrzuć słoik i napełnij nowy.” | Napełnij nowy słoik z nienaruszonym wieczkiem. |
| „Pojemnik …: wpisz nową plombę zakładu założoną po pobraniu próbki.” | Zamknij pojemnik nową plombą. |
| „Każdy numer plomby w dostawie musi być inny.” | Numer plomby został użyty dwukrotnie; sprawdź numery. |
| „Wybierz jedną z pasiek pszczelarza.” / „Wprowadź datę dostawy (nie z przyszłości).” | Popraw pola dostawy. |
| „Ta pasieka ma już rekord produkcji dla wybranego okresu i rodzaju miodu.” | Jedna pasieka dostarcza miód raz na okres i rodzaj miodu. Sprawdź okres i rodzaj miodu. |
| „Błędny kod. Pozostało prób: ….” / „Kod wygasł. Wyślij nowy kod.” | Poproś pszczelarza o ponowne odczytanie kodu albo wyślij nowy. |
| „Podaj powód, dla którego nie można użyć kodu SMS.” / „Załącz zdjęcie podpisanego formularza dostawy.” | Ścieżka ze zdjęciem podpisu wymaga obu tych elementów. |
| „Dostawy rejestruje tylko organizacja z funkcją przetwarzanie.” / „Twoja rola (…) nie może rejestrować dostaw; role: ….” | Twoja organizacja lub rola nie może rejestrować dostaw. Zwróć się do administratora organizacji. |

Lista **Dostawy (przyjęcie)** pokazuje wszystkie dostawy wraz z numerem partii, datą, pasieką, rodzajem miodu, ilością, pochodzeniem i sposobem potwierdzenia. Z każdego wiersza można otworzyć stronę dostawy, **Próbka** i **Wycofanie**.

## Strona dostawy i etykiety {#sao-labels}

Strona dostawy pokazuje partię, pojemniki wraz z ich **Historia plomb**, **Słoiki z próbkami** i ich status w laboratorium, sposób potwierdzenia dostawy (**Potwierdzono przez**: kod SMS lub zdjęcie podpisu) oraz kontrolę **Potwierdzona treść**: „bez zmian od potwierdzenia” albo, po korekcie, poprawioną treść.

**Etykiety (arkusz A4)** drukuje wszystkie etykiety dostawy na arkuszach A4 (2 × 5); **Etykiety (100 × 50 mm)** drukuje jedną etykietę na stronę, dla drukarki etykiet. Jest jedna etykieta na pojemnik (`L{lot}-C{n}`), na słoik z próbką (`S-{lot}-C{n}`) i na próbkę kontrolną (`S-{lot}-C{n}-K`), każda z kodem QR i numerem plomby; etykieta próbki kontrolnej pokazuje też **przechowywać do**.

<!-- SHOT: intake-labels -->
Figure: Etykiety dostawy: pojemnik, słoik z próbką i próbka kontrolna.

## Pobranie nowego słoika, gdy laboratorium odrzuci słoik {#sao-resample}

Jeśli laboratorium odrzuci słoik (numer plomby, etykieta lub wieczko nie są w porządku), strona dostawy pokazuje przy tym słoiku komunikat „Laboratorium odrzuciło słoik: pobierz nowy”. Ponownie otwórz pojemnik, napełnij nowy słoik z próbką i słoik z próbką kontrolną, wpisz ich plomby, zaznacz **Wieczko nienaruszone**, wpisz **Nowa plomba po pobraniu** i kliknij **Otwórz, pobierz i zaplombuj ponownie**. Nowy słoik (`S-{lot}-C{n}-2`, …) trafia do tego samego laboratorium; historia plomb pokazuje, że pojemnik został otwarty i ponownie zaplombowany.

## Pobranie próbki z wcześniejszej dostawy {#sao-sample}

Dostawy zarejestrowane przed wprowadzeniem procedury pojemników („wcześniejsza (pojemniki niezapisane)”) mają jedną próbkę, pobieraną na stronie **Pobierz próbkę**:

1. Otwórz stronę **Pobierz próbkę** ze strony **Próbki i laboratorium** lub z wiersza dostawy.
2. Wybierz **Laboratorium**: na liście są tylko zweryfikowane laboratoria.
3. Wpisz **Numer plomby** próbki oraz **Numer plomby przeciwpróbki**. Numery muszą się różnić.
4. Wpisz **Miejsce przechowywania przeciwpróbki** oraz **Przechowywać do** (domyślnie 12 miesięcy naprzód).
5. Kliknij **Wyślij do laboratorium** albo **Później**, aby wysłać próbkę w innym terminie.

<!-- SHOT: sample-form -->
Figure: Pobieranie próbki z wcześniejszej dostawy: laboratorium i numery plomb.

Próbka otrzymuje kod, na przykład `S-20070-1`, oraz status **Wysłana**. Jeśli laboratorium stwierdzi naruszenie plomby, próbka zostaje odrzucona; pobierz nową próbkę z tej samej partii.

Błędy: „Wybierz zweryfikowane laboratorium.”, „Próbka i przeciwpróbka wymagają różnych plomb.”, „Wprowadź, gdzie przechowywana jest przeciwpróbka (maksymalnie 200 znaków).”, „Wprowadź, do kiedy przechowywana jest przeciwpróbka (data w przyszłości).”, „Próbki pobiera tylko organizacja, która posiada partię.”, „Próbki pobiera tylko organizacja przetwarzająca.”

## Praca na stronie KPK {#ccp-page}

Wszystkie role, które wprowadzają lub zatwierdzają dane KPK, korzystają z tej samej strony ([Figure](#fig-ccp-page-lot)). Otwórz ją ze strony **Moja praca**, ze znaczników KPK na stronie **Moja organizacja** lub ze strony **Przebieg procesu** partii albo partii końcowej.

<!-- SHOT: ccp-page-lot -->
Figure: Strona KPK partii: odpowiedź **Tak** otwiera wymagane pytanie uzupełniające.

Strona pokazuje:

- partię (pszczelarz, pasieka, rodzaj miodu, data produkcji) lub partię końcową (partia końcowa, łączna ilość, hurtownik, skład źródłowy);
- etykietę **Status** oraz informację, kto wprowadził i kto zatwierdził rekord;
- klasę miodu, według której stosowane są limity („Limity dla klasy miodu: …”);
- parametry w grupach, każdy z polem do wprowadzania, wartością **Limit krytyczny:** oraz, po zapisaniu, oceną **W granicach limitu** lub **Poza limitem**;
- baner **Tylko do odczytu** z podaniem przyczyny, jeśli nie możesz wprowadzać danych.

Typy pól:

- **Liczba**: jako separator dziesiętny można użyć przecinka lub kropki.
- **Tak / Nie**: wybierz z listy.
- **Tekst**: maksymalnie 100 znaków.
- **Plik**: prześlij dokument (PDF do 10 MB lub obraz do 8 MB). Zaznacz **Usuń**, aby go skasować.
- **Uwagi**: opcjonalna obserwacja, maksymalnie 500 znaków.

Niektóre parametry są wymagane tylko wtedy, gdy na inne pytanie udzielono określonej odpowiedzi, na przykład „Wymagane tylko, gdy na »…« odpowiedziano Tak.”

**Aby wprowadzić dane**

1. Wypełnij parametry.
2. Kliknij **Zapisz zapisy monitorowania**. Komunikat „Zapisano wartości: KPK ….” potwierdza zapis, po czym nastąpi powrót do strony **Moja organizacja**.

Nie ma osobnego przycisku przesyłania. Gdy każdy wymagany parametr ma wartość, rekord otrzymuje status **Przesłany** i pojawia się na stronie **Moja praca** osób zatwierdzających. Jeśli wprowadzono tylko część wartości, rekord pozostaje w statusie **W toku**.

**Jeśli KPK nie ma zastosowania** (na przykład miód nie jest podgrzewany):

1. Dopóki rekord nie ma wartości, rozwiń sekcję „Ten KPK nie ma tu zastosowania (oznacz jako »Nie dotyczy«)”.
2. Wpisz **Powód**, na przykład „Surowy miód nie jest podgrzewany ani pasteryzowany”.
3. Kliknij **Oznacz jako „Nie dotyczy”**. Powód, Twoja nazwa użytkownika i czas zostają zapisane, a w publicznej Historii miodu KPK jest wyświetlany jako „Nie dotyczy”.

Aby cofnąć to oznaczenie, kliknij **Usuń „Nie dotyczy”**. KPK oznaczony jako nie dotyczy nadal wymaga zatwierdzenia, i to przez inną osobę niż ta, która go oznaczyła.

**Częste błędy**

Table: Częste błędy na stronie KPK {#tbl-ccp-errors}

| Komunikat | Znaczenie |
|---|---|
| „…: wprowadź liczbę.” / „…: wybierz Tak lub Nie.” | Wartość ma nieprawidłowy typ. |
| „Ten KPK jest oznaczony jako „Nie dotyczy”. Aby rejestrować wartości, najpierw usuń to oznaczenie.” | Przed wprowadzeniem wartości usuń oznaczenie. |
| „Za ten punkt kontrolny odpowiada inna organizacja; możesz go tylko odczytywać.” | Możesz odczytać ten KPK, ale nie możesz go zmienić. |
| „Twoja rola (…) nie może wprowadzać danych w tym punkcie kontrolnym; role wprowadzające: ….” | Dane tego KPK wprowadza inna rola. |
| „Twoja organizacja ma status Oczekująca; dane mogą wprowadzać tylko zweryfikowane organizacje.” | Poczekaj na weryfikację. |
| „Ta partia surowca miodu jest wykorzystana w zatwierdzonej ostatecznie partii końcowej … i jest zablokowana. …” | Partia jest zamknięta; zmienić ją można tylko w drodze korekty. |

## Wprowadzanie KPK 0 (audyt wstępny) {#sao-ccp0}

1. Na stronie **Moja praca**, w grupie **Wartości do wprowadzenia**, otwórz pozycję „KPK 0 · #[numer partii]”.
2. Wprowadź wyniki kontroli przy przyjęciu (parametry KPK 0). Niektóre pytania po udzieleniu odpowiedzi **Tak** odsłaniają kolejne pytania wymagane.
3. Kliknij **Zapisz zapisy monitorowania**.

Gdy wszystkie wymagane dane są wprowadzone, KPK 0 otrzymuje status **Przesłany** ([Figure](#fig-ccp0-submitted)), a specjalista ds. zapewnienia jakości Twojej organizacji znajduje go w grupie **Wartości do zatwierdzenia**. Nie możesz zatwierdzić własnego KPK 0 („Zasada czterech oczu: wartości tego KPK zostały zarejestrowane przez Ciebie, więc musi je zatwierdzić inna osoba.”). Laboratorium może wprowadzić wyniki dopiero po zatwierdzeniu KPK 0.

<!-- SHOT: ccp0-submitted -->
Figure: KPK 0 przesłany i oczekujący na zatwierdzenie przez inną osobę.
