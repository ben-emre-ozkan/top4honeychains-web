# 12. Administrator platformy {#admin}

## Twoja codzienna praca {#admin-daily}

Administratorzy platformy prowadzą platformę. Weryfikują organizacje, zarządzają członkami i uprawnieniami, utrzymują dane referencyjne i definicje punktów kontrolnych, rejestrują pszczelarzy, tworzą partie końcowe (na razie), rozstrzygają korekty i monitorują dziennik audytu.

Administratorzy mogą przeglądać każdy rekord, ale **nie wprowadzają ani nie zatwierdzają danych biznesowych**. Po zalogowaniu trafiają na stronę **Moja praca**, która pokazuje grupy **Organizacje do weryfikacji**, **Numery w rejestrze do weryfikacji** i **Korekty do rozstrzygnięcia**. Strona **Panel** zawiera przegląd (liczby, łańcuch, mapę pszczelarzy i pasiek według prowincji, ostatnią aktywność i oczekujące zatwierdzenia).

<!-- SHOT: admin-dashboard -->

Menu administracyjne:

| Menu | Pozycje |
|---|---|
| **Panel** | Przegląd |
| **Pszczelarze** | Lista pszczelarzy, pasieki, poziomy edukacji, cele pszczelarstwa, typy pszczelarstwa |
| **Produkcja Surowego Miodu** | Partie surowego miodu, rodzaje miodu, okresy produkcji |
| **Hurtownicy** | Lista hurtowników, dodawanie hurtownika |
| **Końcowe Produkcje Miodu** | Partie końcowe |
| **Krytyczne Punkty Kontrolne** | KPK, kategorie parametrów, parametry, tłumaczenia danych referencyjnych |
| **Zarządzanie Użytkownikami** | Użytkownicy, role, **Dziennik audytu** |
| **Organizacje** | Moja praca, organizacje, funkcje, rejestr, role i uprawnienia, odpowiedzialności za KPK, korekty, wysyłki, próbki, strony pszczelarzy |

## Organizacje {#admin-organizations}

### Tworzenie organizacji {#admin-org-create}

1. **Organizacje** → **Nowa organizacja**.
2. Wpisz nazwę, nazwę prawną, numer podatkowy, prowincję i powiat.
3. Jeśli organizacja jest hurtownikiem, który już istnieje na platformie, wybierz go w polu istniejącego hurtownika. **Zakład przetwórczy musi być powiązany ze swoim rekordem hurtownika**: przyjmowane przez niego partie są przypisywane do tego hurtownika, a partia końcowa może korzystać wyłącznie z partii własnego hurtownika.
4. Zaznacz co najmniej jedną funkcję i kliknij **Utwórz**.

Organizacja otrzymuje początkowo status **Oczekująca**: „Organizacja została utworzona. Pozostaje w stanie Oczekująca do czasu weryfikacji.”

<!-- SHOT: admin-org-create -->

### Weryfikacja, żądanie dokumentów, zawieszenie {#admin-org-verify}

Na stronie organizacji:

- **Zweryfikuj** (ze statusu Oczekująca) wymaga co najmniej jednego przesłanego dokumentu.
- **Zażądaj dokumentów** (w statusie Oczekująca) wymaga podania powodu, na przykład którego dokumentu brakuje.
- **Zawieś** (w statusie Zweryfikowana) wymaga podania powodu. **Przywróć** ponownie weryfikuje zawieszoną organizację.

Każda zmiana pojawia się w historii weryfikacji i w dzienniku audytu.

### Funkcje {#admin-org-capabilities}

**Dodaj funkcję** dodaje funkcję. W przypadku funkcji **Laboratorium** zaznacz pole akredytacji i wpisz numer akredytacji oraz datę jej ważności; laboratorium akredytowane musi mieć numer. **Usuń** usuwa funkcję; organizacja musi zachować co najmniej jedną funkcję. Strona **Funkcje** pokazuje dla każdej funkcji, które KPK jej wymagają i które organizacje ją mają. Zakładka **Funkcje organizacji** na stronie **Role i uprawnienia** pozwala edytować wszystkie organizacje jednocześnie.

### Członkowie {#admin-members}

1. Na stronie organizacji, w sekcji członków, wybierz rolę.
2. Wpisz nazwę istniejącego użytkownika albo nową nazwę użytkownika, adres e-mail i hasło, aby jednocześnie utworzyć użytkownika.
3. Kliknij **Dodaj członka**: „Dodano … jako ….”

Użytkownik może mieć kilka członkostw. **Cofnij** kończy członkostwo i wylogowuje użytkownika na wszystkich urządzeniach. Zakładka **Członkowie** na stronie **Role i uprawnienia** zawiera listę wszystkich członkostw we wszystkich organizacjach.

<!-- SHOT: admin-org-members -->

### Wyjątek od zasady czterech oczu {#admin-four-eyes}

Małe organizacje, w których za jakość odpowiada jedna osoba, mogą otrzymać **Wyjątek od zasady czterech oczu**: ich członkowie mogą wtedy zatwierdzać własne rekordy KPK i potwierdzać własne wnioski o ostateczne zatwierdzenie. Każde takie zatwierdzenie jest oznaczane w dzienniku audytu. Wyjątek nigdy nie dotyczy wyników laboratoryjnych.

## Użytkownicy {#admin-users}

**Zarządzanie Użytkownikami** → **Użytkownicy** zawiera listę kont.

- **Dodaj nowego użytkownika** tworzy konto. W modelu organizacji pozostaw pole **Rola globalna** puste dla członków organizacji i nadaj im członkostwo na stronie organizacji; opcję **Administrator platformy** wybieraj tylko dla administratorów.
- **Zresetuj hasło** ustawia nowe hasło dla użytkownika.
- **Usuń** usuwa konto (używaj ostrożnie; tej operacji nie można cofnąć).

Hasło musi mieć co najmniej 6 znaków. Po 5 nieudanych próbach logowania konto zostaje zablokowane na 15 minut. Nie ma przycisku odblokowania; blokada wygasa samoczynnie.

## Role i uprawnienia {#admin-permissions}

Strona **Role i uprawnienia** ma trzy zakładki:

- **Członkowie**: wszystkie aktywne członkostwa; dodawanie i cofanie.
- **Uprawnienia ról**: wybierz rolę i dla każdej funkcji zaznacz **Wprowadzanie**, **Zatwierdzenie** i **Odczyt**. Funkcjami są poszczególne KPK (każdy w osobnym wierszu) oraz obszary: przyjęcie, próbki, wyniki laboratoryjne, wysyłki, strony pszczelarzy, wycofanie i ostateczne zatwierdzenie. Każde uprawnienie musi zachować co najmniej jedną rolę. Zmiany obowiązują natychmiast.
- **Funkcje organizacji**: funkcje poszczególnych organizacji.

Ramka „Stałe zasady (nie można ich wyłączyć):” wymienia zasady, których nie zmienia żadne ustawienie (zob. [Stałe zasady](#fixed-rules)).

<!-- SHOT: admin-role-permissions -->

## Odpowiedzialności za KPK {#admin-ccp-responsibilities}

Strona **Odpowiedzialności za KPK** określa dla każdego KPK, która organizacja jest odpowiedzialna (właściciel partii lub partii końcowej albo, w przypadku partii, stowarzyszenie pszczelarza), jakiej funkcji potrzebuje, które role wprowadzają wartości, które role je zatwierdzają oraz czy zatwierdzenie jest wymagane. Przykład: aby przypisać KPK 0 stowarzyszeniu, wybierz „Stowarzyszenie pszczelarza danej partii”, **Stowarzyszenie** i **Inspektor terenowy**.

## Punkty kontrolne, parametry i limity {#admin-ccps}

- **Krytyczne Punkty Kontrolne**: kod, tytuł, etap przetwarzania, uzasadnienie krytyczności oraz **Dotyczy** (partia surowego miodu lub partia końcowa). Opcja wyświetlania wartości tekstowych w publicznej Historii miodu powinna pozostać wyłączona, jeśli odpowiedzi mogą zawierać dane osobowe.
- **Parametry**: nazwa, jednostka, kategoria, typ danych (**Liczba**, **Tak / Nie**, **Tekst**, **Plik**), wartość minimalna i maksymalna, parametr wymagany lub opcjonalny oraz warunek „wymagany tylko wtedy, gdy” inne pytanie typu tak/nie ma określoną odpowiedź.
- **Limity**: dla każdego parametru limity domyślne dla wszystkich rodzajów miodu oraz limity szczegółowe według klasy miodu (nektarowy, spadziowy, sosnowy) lub rodzaju miodu, opcjonalnie z warunkiem, sumą lub proporcją. Zapisane wyniki nie są przeliczane po zmianie limitów.

**Zweryfikowane limity.** Każdy parametr ma pole wyboru **Limity zweryfikowane (wyświetlane publicznie)**. Tylko zweryfikowane limity dają publiczną ocenę „w granicach limitu / poza limitem” w Historii miodu i na stronie pszczelarza. Niezweryfikowane limity są oznaczone w panelu jako „do weryfikacji”, a strony publiczne informują, że limity są wciąż sprawdzane.

<!-- SHOT: admin-parameter-limits -->

## Dane referencyjne i tłumaczenia {#admin-reference}

Rodzaje miodu (wraz z klasą miodu stosowaną dla limitów), okresy produkcji, poziomy edukacji, typy i cele pszczelarstwa oraz hurtownicy są prowadzeni w odpowiednich pozycjach menu. Rekordu, który jest w użyciu, nie można usunąć.

Strona **Tłumaczenia danych referencyjnych** zawiera nazwy wyświetlane na listach, w tabelach i na stronach publicznych w pięciu językach: rodzaje miodu, okresy, statusy, funkcje, role, KPK, parametry, kategorie parametrów, uwagi do limitów, objaśnienia parametrów i kraje. Puste pole jest zastępowane tekstem angielskim, a w dalszej kolejności tekstem bazowym.

## Pszczelarze, pasieki i rejestr {#admin-registry}

- **Pszczelarze**: dodawanie pszczelarza (imię i nazwisko, data urodzenia, doświadczenie, telefon, wykształcenie, typ i cel pszczelarstwa, opcjonalne zdjęcie). Numer działalności jest generowany automatycznie. Pszczelarze nie logują się do platformy. Nigdy nie są usuwani, tylko dezaktywowani.
- **Pasieki**: nazwa, liczba uli i lokalizacja (szerokość i długość geograficzna). Lokalizacja nigdy nie jest wyświetlana publicznie; strony publiczne pokazują tylko prowincję.
- **Rejestr pszczelarzy i stowarzyszenia**: dla każdego pszczelarza przypisz stowarzyszenie i wpisz numer gospodarstwa w rejestrze (format: `TR` + kod prowincji + 10 cyfr, na przykład `TR061234567890`) wraz z dokumentem potwierdzającym. Następnie kliknij **Zweryfikuj** lub **Odrzuć** (odrzucenie wymaga podania powodu).

<!-- SHOT: admin-registry -->

## Partie końcowe (na razie tylko administrator) {#admin-batches}

1. **Hurtownicy** → hurtownik → **Końcowe produkcje miodu (partie końcowe)** → **Nowa partia końcowa**.
2. Wpisz **Tytuł partii** i **Data produkcji**, a następnie kliknij **Utwórz partię**. Kod QR zostaje wygenerowany.
3. Na stronie **Zarządzaj partią** wpisz ilość, która ma zostać użyta z każdej kwalifikującej się partii (partie tego hurtownika, których wszystkie KPK są zatwierdzone), i kliknij **Zapisz skład partii**. Ilości przekraczające dostępną ilość są pomijane.
4. Użyj opcji **Edytuj**, aby ustawić organizację będącą właścicielem partii końcowej (zakład).
5. **Zarządzaj KPK**: zaznacz od KPK 4 do KPK 10 i kliknij **Zapisz przypisania**.

Następnie zakład wprowadza KPK partii końcowej i zatwierdza ją ostatecznie. Administratorzy nie mogą zatwierdzać ostatecznie: „W modelu organizacji partię końcową zatwierdza ostatecznie organizacja będąca jej właścicielem (wniosek i zatwierdzenie przez dwie osoby).”

<!-- SHOT: admin-manage-batch -->

## Korekty {#admin-corrections}

Zatwierdzone lub zablokowane dane można zmienić wyłącznie w drodze korekty:

1. Otwórz stronę KPK danego rekordu. W ramce **Korekty** wybierz parametr, wpisz nową wartość i **Powód**, a następnie kliknij **Złóż wniosek**: „Zgłoszono korektę; musi ją zatwierdzić inny administrator platformy.”
2. **Drugi** administrator otwiera stronę **Korekty**, zapoznaje się ze starą i nową wartością oraz powodem i klika **Zatwierdź i zastosuj** lub **Odrzuć** (odrzucenie wymaga wpisania uwagi).

Osoba, która złożyła wniosek o korektę, nie może jej rozstrzygnąć. Korekty dotyczą również rekordów zablokowanych, a oba kroki są zapisywane w dzienniku audytu.

## Strony pszczelarzy, wysyłki i próbki {#admin-overviews}

Administratorzy mogą otwierać strony **Strony pszczelarzy**, **Wysyłki i zapasy** oraz **Próbki i laboratorium** w trybie tylko do odczytu, aby śledzić pracę wszystkich organizacji.

## Wycofanie {#admin-recall}

**Raport z wycofania** partii końcowej lub partii zawiera:

- **w górę łańcucha**: partie, pasieki, pszczelarzy i formularze dostawy;
- partie końcowe;
- **w dół łańcucha**: wysyłki i obecnych posiadaczy.

Członkowie widzą tę część, którą może widzieć ich organizacja (jako właściciel, posiadacz lub stowarzyszenie); ukryte wiersze są zliczane. Przyciski **Drukuj** i **CSV** eksportują raport.

<!-- SHOT: recall-report -->

## Dziennik audytu {#admin-audit}

Zob. [Dziennik audytu](#trust-audit).
