# 12. Specjalista ds. rejestracji pszczelarzy {#role-ro}

## Twoja codzienna praca {#ro-daily}

Pracujesz w powiatowej dyrekcji rolnictwa, czyli w organizacji z funkcją **Rejestracja pszczelarzy**. Rejestrujesz pszczelarzy ze swojego obszaru i ich pasieki, aby zakłady mogły rejestrować ich dostawy. Dla każdego pszczelarza:

1. sprawdzasz numer gospodarstwa w rejestrze;
2. rejestrujesz pszczelarza i jego pierwszą pasiekę;
3. dajesz pszczelarzowi do przeczytania informację o danych i rejestrujesz jego wybory dotyczące zgód;
4. potwierdzasz rejestrację kodem, który pszczelarz otrzymuje SMS-em, albo podpisanym i sfotografowanym formularzem;
5. wysyłasz pszczelarzowi dane logowania do portalu pszczelarza, gdy portal jest otwarty.

Menu zawiera pozycję **Rejestracja pszczelarzy**. Odczytujesz i wyszukujesz wyłącznie rejestracje własnej dyrekcji. Administrator platformy może rejestrować pszczelarzy w ten sam sposób (dla własnej organizacji platformy).

## Wyszukiwanie lub rejestracja pszczelarza {#ro-find}

1. Otwórz **Rejestracja pszczelarzy**. Lista pokazuje pszczelarzy zarejestrowanych przez Twoją organizację wraz z kolumnami **Data**, **Numer rejestrowy**, sposobem potwierdzenia rejestracji (**Potwierdzono przez**: kod SMS lub zdjęcie podpisu) oraz stanem w kolumnie **Dowód**.
2. W polu **Numer rejestrowy (AKS) lub numer gospodarstwa** wpisz numer i kliknij **Znajdź lub zarejestruj**. „Znany pszczelarz jest weryfikowany ponownie; nowy numer rejestrowy rozpoczyna nową rejestrację.”

Jeśli pszczelarz należy do innej dyrekcji: „Ten pszczelarz jest zarejestrowany przez inną dyrekcję.”

<!-- SHOT: registration-list -->
Figure: **Rejestracja pszczelarzy**: rejestracje dyrekcji.

## Rejestracja pszczelarza {#ro-register}

Formularz **Zarejestruj pszczelarza** (lub **Ponownie zweryfikuj pszczelarza** w przypadku znanego pszczelarza) składa się z czterech części ([Figure](#fig-registration-form)).

1. **Numer rejestrowy**: `TR`, kod tablicy rejestracyjnej prowincji i 10 cyfr, na przykład `TR281234500004` („TR, kod prowincji i 10 cyfr. Porównywany z prowincją dyrekcji.”). Jeśli numer należy do innej prowincji, wpisz powód w polu **Jeśli numer należy do innej prowincji: powód**.
2. **Pszczelarz**: **Imię**, **Nazwisko**, **Data urodzenia**, **Telefon komórkowy**, **Język SMS** oraz opcjonalnie poziom wykształcenia, cel pszczelarstwa i typ pszczelarstwa. Zaznacz **Pszczelarz nie ma telefonu komórkowego (zdjęcie podpisu, bez konta)**, jeśli pszczelarz nie ma telefonu.
3. **Pasieka**: miejsce pierwszej pasieki (prowincja, powiat i miejscowość, które tworzą też nazwę pasieki), jej położenie i liczba uli. Kolejne pasieki można dodać później na stronie rejestracji.
4. **Informacja i zgoda**: „Daj pszczelarzowi tę informację do przeczytania lub przeczytaj ją na głos”. Następnie zarejestruj wybory pszczelarza:
   - **Pszczelarz zgadza się na pokazywanie imienia i nazwiska na stronach publicznych (opcjonalnie; inaczej pokazywany jest neutralny kod)**;
   - **Pszczelarz zgadza się na powiadomienia SMS o dostawach i wynikach badań (opcjonalnie)**.
   **Pełne treści zgód** otwiera treści zgód.
5. **Twoje oświadczenie**: zaznacz **Okazano mi dokument tożsamości pszczelarza i wpis w rejestrze.** Zaznacz **Wyślij też pszczelarzowi dane logowania SMS-em**, jeśli pszczelarz ma już teraz otrzymać konto w portalu (opcja widoczna tylko wtedy, gdy portal jest otwarty).

<!-- SHOT: registration-form -->
Figure: Rejestracja pszczelarza: numer rejestrowy, tożsamość, pasieka, informacja i zgody.

### Potwierdzenie kodem SMS {#ro-sms}

1. W sekcji **Potwierdzenie** kliknij **Wyślij kod**. „Pszczelarz dostaje SMS z nazwą organizacji, linkiem do informacji, wyborem dotyczącym nazwiska i kodem. Zmiana pola potem wymaga nowego kodu.”
2. Wpisz **Kod z SMS (6 cyfr)**, który pszczelarz odczytuje na głos, i kliknij **Potwierdź rejestrację**.

„Pszczelarz jest zarejestrowany i zweryfikowany.” Rejestracja zostaje zapisana z potwierdzoną treścią, zgody zaczynają obowiązywać, a pszczelarz może od razu dostarczać miód. Zasady dotyczące kodu są takie same jak przy dostawie: ważność 5 minut, 3 próby, nowy kod po minucie.

### SMS niemożliwy: zdjęcie podpisu {#ro-photo}

Jeśli pszczelarz nie ma telefonu komórkowego albo SMS-a nie da się wysłać:

1. Otwórz **SMS niemożliwy: zdjęcie podpisu** i kliknij **Drukuj formularz**. Wydrukowana strona zawiera wybory pszczelarza i wiersz „Zapoznałem/zapoznałam się z informacją; powyższe wybory są prawidłowe. Podpis pszczelarza i data:”.
2. Daj ją pszczelarzowi do podpisania, zrób zdjęcie, wybierz je w polu **Zdjęcie podpisanego formularza** i wpisz **Dlaczego bez kodu SMS**.
3. Kliknij **Zapisz ze zdjęciem podpisu**: „Rejestrację zapisano ze zdjęciem podpisu. Czeka na sprawdzenie przez specjalistę ds. zgodności.”

Dopóki Specjalista ds. zgodności Twojej dyrekcji nie zaakceptuje zdjęcia, pszczelarz nie może dostarczać miodu; odrzucenie zdjęcia wycofuje też zgody z tej rejestracji. Nie możesz sprawdzać rejestracji, którą sam zarejestrowałeś. Pszczelarz bez telefonu komórkowego nie otrzymuje konta w portalu.

## Strona rejestracji {#ro-details}

Kliknij rejestrację na liście, aby ją otworzyć. Strona pokazuje rejestrację, **Numer rejestrowy** (z oznaczeniem „inna prowincja”, jeśli się różni), **Telefon komórkowy**, **Zgody** (z oznaczeniem „wycofana”, jeśli zgodę wycofano), **Dowód** i jego sprawdzenie oraz **Pasieki** z liczbą uli.

- **Dodaj pasiekę** rejestruje kolejną pasiekę pszczelarza.
- **Wyślij dane logowania SMS-em** wysyła pszczelarzowi nazwę użytkownika i hasło tymczasowe do portalu pszczelarza (tylko wtedy, gdy portal jest otwarty, i tylko w przypadku rejestracji potwierdzonej kodem SMS: „Logowanie wymaga rejestracji potwierdzonej kodem SMS (bez telefonu komórkowego brak konta).”). Przy pierwszym logowaniu pszczelarz ustawia własne hasło.

<!-- SHOT: registration-details -->
Figure: Potwierdzona rejestracja ze zgodami i pasiekami.

Poświadczone fakty (tożsamość, numer rejestrowy, telefon) są tylko do odczytu, również na stronie pszczelarza u administratorów; zmieniają się wyłącznie przez nową rejestrację tego samego pszczelarza (ponowną weryfikację), ponownie potwierdzoną.

**Częste błędy**

Table: Częste błędy przy rejestracji pszczelarza {#tbl-registration-errors}

| Komunikat | Co zrobić |
|---|---|
| „Wpisz numer rejestrowy (lub numer gospodarstwa zarejestrowanego pszczelarza).” | Wpisz numer przed kliknięciem **Znajdź lub zarejestruj**. |
| „Numer musi składać się z TR, kodu prowincji 01-81 i 10 cyfr (np. TR061234567890).” | Popraw numer rejestrowy. |
| „Ten pszczelarz jest zarejestrowany przez inną dyrekcję.” | Zwróć się do tej dyrekcji. |
| „Błędny kod. Pozostało prób: ….” / „Kod wygasł. Wyślij nowy kod.” | Poproś pszczelarza o ponowne odczytanie kodu albo wyślij nowy. |
| „Dane zmieniły się po wysłaniu kodu. Wyślij nowy kod.” | Po wysłaniu kodu zmieniono pole. |
| „Za dużo kodów na ten numer telefonu. Spróbuj później.” | Na jeden numer telefonu można wysłać najwyżej 5 kodów na godzinę i 10 na dzień. |
