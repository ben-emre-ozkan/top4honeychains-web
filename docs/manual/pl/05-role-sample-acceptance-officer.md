# 5. Pracownik przyjmujący próbki {#role-sao}

## Twoja codzienna praca {#sao-daily}

Pracujesz w punkcie przyjęć zakładu przetwórczego. Gdy pszczelarz dostarcza miód:

1. rejestrujesz dostawę (co tworzy partię surowego miodu);
2. plombujesz próbkę i przeciwpróbkę, a następnie wysyłasz próbkę do laboratorium;
3. wprowadzasz KPK 0, czyli audyt wstępny partii, aby zatwierdzający jakość mógł go zatwierdzić.

Twoje zadania pojawiają się na stronie **Moja praca** w grupie **Wartości do wprowadzenia**. Menu zawiera pozycje **Dostawy (przyjęcie)** oraz **Próbki i laboratorium**.

## Rejestracja dostawy {#sao-intake}

1. Otwórz **Dostawy (przyjęcie)** i kliknij **Zarejestruj dostawę**.
2. W polu **Numer działalności lub numer w rejestrze pszczelarza** wpisz dokładnie numer działalności pszczelarza lub jego zweryfikowany numer gospodarstwa w rejestrze i kliknij **Znajdź**. Jeśli nic nie zostanie znalezione, wyświetli się komunikat „Brak aktywnego pszczelarza o tym numerze.” Sprawdź numer; pszczelarzy rejestruje administrator platformy.
3. Wypełnij formularz:
   - **Pasieka**: jedna z pasiek pszczelarza.
   - **Rodzaj miodu**.
   - **Okres produkcji**: bieżący okres jest wybrany domyślnie.
   - **Kraj pochodzenia**.
   - **Data dostawy**: domyślnie dzisiejsza; nie może być datą przyszłą.
   - **Ilość (kg)**: jako separator dziesiętny można użyć przecinka lub kropki.
   - **Prowincja** i **Powiat** pochodzenia: prowincja jest w miarę możliwości uzupełniana automatycznie na podstawie numeru w rejestrze.
   - **Zdjęcie podpisanego formularza dostawy**: zrób zdjęcie przyciskiem aparatu albo wybierz plik (PDF do 10 MB lub JPG, PNG albo WEBP do 8 MB).
   - **Wyślij SMS do pszczelarza** (opcjonalnie; w obecnej wersji wiadomość jest tylko rejestrowana, a nie wysyłana).
4. Kliknij **Zarejestruj dostawę**.

<!-- SHOT: intake-form -->

Komunikat „Dostawę zarejestrowano jako partię nr ….” potwierdza utworzenie nowej partii. Platforma przypisuje do partii KPK 0–3 i otwiera dla niej stronę **Pobierz próbkę**.

**Częste błędy**

| Komunikat | Co zrobić |
|---|---|
| „Wybierz jedną z pasiek pszczelarza.” | Wybierz pasiekę z listy. |
| „Wprowadź datę dostawy (nie z przyszłości).” | Popraw datę. |
| „Ilość musi być większa od 0.” | Wpisz ilość w kilogramach. |
| „Wybierz powiat (dystrykt) wybranej prowincji.” | Najpierw wybierz prowincję, a następnie powiat z jej listy. |
| „Załącz zdjęcie podpisanego formularza dostawy.” | Podpisany formularz jest wymagany. |
| „Ta pasieka ma już rekord produkcji dla wybranego okresu i rodzaju miodu.” | Jedna pasieka dostarcza miód raz na okres i rodzaj miodu. Sprawdź okres i rodzaj miodu. |
| „Dostawy rejestruje tylko organizacja z funkcją przetwarzanie.” / „Twoja rola (…) nie może rejestrować dostaw; role: ….” | Twoja organizacja lub rola nie może rejestrować dostaw. Zwróć się do administratora organizacji. |

Lista **Dostawy (przyjęcie)** pokazuje wszystkie dostawy wraz z numerem partii, datą, pasieką, rodzajem miodu, ilością, pochodzeniem i podpisanym formularzem (z pierwszymi znakami jego odcisku SHA-256). Z każdego wiersza można otworzyć **Próbka** i **Wycofanie**.

## Pobranie i wysłanie próbki {#sao-sample}

1. Po zarejestrowaniu dostawy otwiera się strona **Pobierz próbkę**. Możesz ją też otworzyć ze strony **Próbki i laboratorium** lub z wiersza dostawy.
2. Wybierz **Laboratorium**: na liście są tylko zweryfikowane laboratoria; przy laboratorium akredytowanym wyświetla się numer akredytacji.
3. Wpisz **Numer plomby** próbki oraz **Numer plomby przeciwpróbki**. Numery muszą się różnić.
4. Wpisz **Miejsce przechowywania przeciwpróbki** oraz **Przechowywać do** (domyślnie 12 miesięcy naprzód).
5. Kliknij **Wyślij do laboratorium** albo **Później**, aby wysłać próbkę w innym terminie.

<!-- SHOT: sample-form -->

Próbka otrzymuje kod, na przykład `S-20070-1`, oraz status **Wysłana**. Laboratorium widzi ją w grupie **Próbki do przyjęcia**. Jeśli laboratorium stwierdzi naruszenie plomby, próbka zostaje odrzucona; pobierz nową próbkę z tej samej partii.

Błędy: „Wybierz zweryfikowane laboratorium.”, „Próbka i przeciwpróbka wymagają różnych plomb.”, „Wprowadź, gdzie przechowywana jest przeciwpróbka (maksymalnie 200 znaków).”, „Wprowadź, do kiedy przechowywana jest przeciwpróbka (data w przyszłości).”, „Próbki pobiera tylko organizacja, która posiada partię.”

## Praca na stronie KPK {#ccp-page}

Wszystkie role, które wprowadzają lub zatwierdzają wartości KPK, korzystają z tej samej strony. Otwórz ją ze strony **Moja praca**, ze znaczników KPK na stronie **Moja organizacja** lub ze strony **Przebieg procesu** partii albo partii końcowej.

<!-- SHOT: ccp-page-lot -->

Strona pokazuje:

- partię (pszczelarz, pasieka, rodzaj miodu, data produkcji) lub partię końcową (partia końcowa, łączna ilość, hurtownik, skład źródłowy);
- etykietę **Status** oraz informację, kto wprowadził i kto zatwierdził rekord;
- klasę miodu, według której stosowane są limity („Limity dla klasy miodu: …”);
- parametry w grupach, każdy z polem do wprowadzania, wartością **Limit krytyczny:** oraz, po zapisaniu, oceną **W granicach limitu** lub **Poza limitem**;
- baner **Tylko do odczytu** z podaniem przyczyny, jeśli nie możesz wprowadzać wartości.

Typy pól:

- **Liczba**: jako separator dziesiętny można użyć przecinka lub kropki.
- **Tak / Nie**: wybierz z listy.
- **Tekst**: maksymalnie 100 znaków.
- **Plik**: prześlij dokument (PDF do 10 MB lub obraz do 8 MB). Zaznacz **Usuń**, aby go skasować.
- **Uwagi**: opcjonalna obserwacja, maksymalnie 500 znaków.

Niektóre parametry są wymagane tylko wtedy, gdy na inne pytanie udzielono określonej odpowiedzi, na przykład „Wymagane tylko, gdy na »…« odpowiedziano Tak.”

**Aby wprowadzić wartości**

1. Wypełnij parametry.
2. Kliknij **Zapisz zapisy monitorowania**. Komunikat „Zapisano wartości: KPK ….” potwierdza zapis, po czym nastąpi powrót do strony **Moja organizacja**.

Nie ma osobnego przycisku przesyłania. Gdy każdy wymagany parametr ma wartość, rekord otrzymuje status **Przesłany** i pojawia się na stronie **Moja praca** osób zatwierdzających. Jeśli wprowadzono tylko część wartości, rekord pozostaje w statusie **W toku**.

**Jeśli KPK nie ma zastosowania** (na przykład miód nie jest podgrzewany):

1. Dopóki rekord nie ma wartości, rozwiń sekcję „Ten KPK nie ma tu zastosowania (oznacz jako »Nie dotyczy«)”.
2. Wpisz **Powód**, na przykład „Surowy miód nie jest podgrzewany ani pasteryzowany”.
3. Kliknij **Oznacz jako „Nie dotyczy”**. Powód, Twoja nazwa użytkownika i czas zostają zapisane, a w publicznej Historii miodu KPK jest wyświetlany jako „Nie dotyczy”.

Aby cofnąć to oznaczenie, kliknij **Usuń „Nie dotyczy”**. KPK oznaczony jako nie dotyczy nadal wymaga zatwierdzenia, i to przez inną osobę niż ta, która go oznaczyła.

**Częste błędy**

| Komunikat | Znaczenie |
|---|---|
| „…: wprowadź liczbę.” / „…: wybierz Tak lub Nie.” | Wartość ma nieprawidłowy typ. |
| „Ten KPK jest oznaczony jako „Nie dotyczy”. Aby rejestrować wartości, najpierw usuń to oznaczenie.” | Przed wprowadzeniem wartości usuń oznaczenie. |
| „Za ten punkt kontrolny odpowiada inna organizacja; możesz go tylko odczytywać.” | Ten KPK jest dla Ciebie dostępny tylko do odczytu. |
| „Twoja rola (…) nie może wprowadzać danych w tym punkcie kontrolnym; role wprowadzające: ….” | Dane tego KPK wprowadza inna rola. |
| „Twoja organizacja ma status Oczekująca; dane mogą wprowadzać tylko zweryfikowane organizacje.” | Poczekaj na weryfikację. |
| „Ta partia surowca miodu jest wykorzystana w zatwierdzonej ostatecznie partii końcowej … i jest zablokowana. …” | Partia jest zamknięta; zmienić ją można tylko w drodze korekty. |

## Wprowadzanie KPK 0 (audyt wstępny) {#sao-ccp0}

1. Na stronie **Moja praca**, w grupie **Wartości do wprowadzenia**, otwórz pozycję „KPK 0 · #[numer partii]”.
2. Wprowadź wyniki kontroli przy przyjęciu (parametry KPK 0). Niektóre pytania po udzieleniu odpowiedzi **Tak** odsłaniają kolejne pytania wymagane.
3. Kliknij **Zapisz zapisy monitorowania**.

Gdy wszystkie wymagane wartości są wprowadzone, KPK 0 otrzymuje status **Przesłany**, a zatwierdzający jakość Twojej organizacji widzi go w grupie **Wartości do zatwierdzenia**. Nie możesz zatwierdzić własnego KPK 0 („Zasada czterech oczu: wartości tego KPK zostały zarejestrowane przez Ciebie, więc musi je zatwierdzić inna osoba.”). Laboratorium może wprowadzić wyniki dopiero po zatwierdzeniu KPK 0.

<!-- SHOT: ccp0-submitted -->
