# 15. Administrator platformy {#admin}

## Twoja codzienna praca {#admin-daily}

Administratorzy platformy prowadzą platformę. Weryfikują organizacje, zarządzają członkostwami i uprawnieniami, wysyłają dane logowania, publikują treści informacji, utrzymują dane referencyjne i definicje punktów kontrolnych, rejestrują pszczelarzy, tworzą partie końcowe (na razie), rozstrzygają korekty i monitorują dziennik audytu.

Administratorzy mogą odczytać każdy rekord, ale **nie wprowadzają ani nie zatwierdzają danych biznesowych**. Po zalogowaniu trafiają na stronę **Moja praca**, która pokazuje grupy **Organizacje do weryfikacji**, **Numery w rejestrze do weryfikacji** i **Korekty do rozstrzygnięcia**. Strona **Panel** zawiera przegląd (liczby, łańcuch, mapę pszczelarzy i pasiek według prowincji, ostatnią aktywność i oczekujące zatwierdzenia).

<!-- SHOT: admin-dashboard -->
Figure: Panel administratora platformy.

Menu administracyjne ([Table](#tbl-admin-menu)):

Table: Menu administracyjne {#tbl-admin-menu}

| Menu | Pozycje |
|---|---|
| **Panel** | Przegląd |
| **Pszczelarze** | Lista pszczelarzy, pasieki, poziomy edukacji, cele pszczelarstwa, typy pszczelarstwa |
| **Produkcja Surowego Miodu** | Partie surowego miodu, rodzaje miodu, okresy produkcji |
| **Hurtownicy** | Lista hurtowników, dodawanie hurtownika |
| **Końcowe Produkcje Miodu** | Partie końcowe |
| **Krytyczne Punkty Kontrolne** | KPK, kategorie parametrów, parametry, tłumaczenia danych referencyjnych |
| **Zarządzanie Użytkownikami** | Użytkownicy, role, **Dziennik audytu**, **Przechowywanie danych**, **Informacje i treści zgód**, **Skrzynka nadawcza SMS (dostawca testowy)** (tylko przy testowym dostawcy SMS) |
| **Organizacje** | Moja praca, organizacje, funkcje, rejestr, rejestracja pszczelarzy, role i uprawnienia, odpowiedzialności za KPK, korekty, wysyłki, próbki, strony pszczelarzy |

## Organizacje {#admin-organizations}

### Tworzenie organizacji {#admin-org-create}

1. **Organizacje** → **Nowa organizacja**.
2. Wpisz nazwę, nazwę prawną, numer podatkowy, prowincję i powiat.
3. Jeśli organizacja jest hurtownikiem, który już istnieje na platformie, wybierz go w polu istniejącego hurtownika. **Zakład przetwórczy musi być powiązany ze swoim rekordem hurtownika**: przyjmowane przez niego partie są przypisywane do tego hurtownika, a partia końcowa może korzystać wyłącznie z partii własnego hurtownika. Powiązanie można też ustawić później na stronie **Edytuj** organizacji. Partie, które zakład otrzymał przed powiązaniem, mogą być użyte w partiach końcowych hurtownika, z którym zakład został powiązany.
4. Zaznacz co najmniej jedną funkcję i kliknij **Utwórz**.

Organizacja otrzymuje początkowo status **Oczekująca**: „Organizacja została utworzona. Pozostaje w stanie Oczekująca do czasu weryfikacji.”

<!-- SHOT: admin-org-create -->
Figure: Tworzenie organizacji: powiązanie z hurtownikiem i funkcje.

### Weryfikacja, żądanie dokumentów, zawieszenie {#admin-org-verify}

Na stronie organizacji (administrator platformy; w przypadku stowarzyszeń pszczelarzy zamiast niego administrator organizacji zweryfikowanego związku centralnego, jeśli platforma jest tak skonfigurowana: administratorzy otrzymują wtedy w przypadku stowarzyszeń odmowę dostępu):

- **Zweryfikuj** (ze statusu Oczekująca) wymaga co najmniej jednego przesłanego dokumentu, a w przypadku organizacji weryfikowanej po raz pierwszy także aktywnego Specjalisty ds. zgodności.
- **Zażądaj dokumentów** (w statusie Oczekująca) wymaga podania powodu, na przykład którego dokumentu brakuje.
- **Zawieś** (w statusie Zweryfikowana) wymaga podania powodu. **Przywróć** ponownie weryfikuje zawieszoną organizację.

Każda zmiana pojawia się w historii weryfikacji i w dzienniku audytu.

### Funkcje {#admin-org-capabilities}

**Dodaj funkcję** dodaje funkcję. W przypadku funkcji **Laboratorium** zaznacz pole akredytacji i wpisz numer akredytacji oraz datę jej ważności; laboratorium akredytowane musi mieć numer. **Usuń** usuwa funkcję; organizacja musi zachować co najmniej jedną funkcję. Strona **Funkcje** pokazuje dla każdej funkcji, które KPK jej wymagają i które organizacje ją mają. Zakładka **Funkcje organizacji** na stronie **Role i uprawnienia** pozwala edytować wszystkie organizacje jednocześnie.

### Członkowie {#admin-members}

1. Na stronie organizacji, w sekcji członków, wybierz rolę. Rola **Specjalista ds. rejestracji pszczelarzy** jest dostępna tylko w organizacji z funkcją Rejestracja pszczelarzy.
2. Wpisz nazwę istniejącego użytkownika albo nową nazwę użytkownika, adres e-mail i **Telefon komórkowy**, aby jednocześnie utworzyć użytkownika. Wybierz, jak użytkownik otrzyma dane logowania: **Wyślij nazwę użytkownika i hasło tymczasowe SMS-em (zalecane)** albo **Ustaw tu hasło początkowe (użytkownik zmieni je przy pierwszym logowaniu)**.
3. Kliknij **Dodaj członka**: „Dodano … jako ….”

Członkostwo to jedno konto użytkownika × jedna organizacja × jedna rola; użytkownik może mieć ich kilka. **Cofnij** kończy członkostwo i wylogowuje użytkownika na wszystkich urządzeniach. Zakładka **Członkowie** na stronie **Role i uprawnienia** zawiera listę wszystkich członkostw we wszystkich organizacjach.

<!-- SHOT: admin-org-members -->
Figure: Członkostwa organizacji i formularz **Dodaj członka**.

### Wyjątek od zasady czterech oczu {#admin-four-eyes}

Małe organizacje, w których za jakość odpowiada jedna osoba, mogą otrzymać **Wyjątek od zasady czterech oczu**: ich członkowie mogą wtedy zatwierdzać własne rekordy KPK i potwierdzać własne wnioski o ostateczne zatwierdzenie. Każde takie zatwierdzenie jest oznaczane w dzienniku audytu, a Specjalista ds. zgodności organizacji widzi je w sekcji **Mechanizm zatwierdzania**. Wyjątek nigdy nie dotyczy wyników laboratoryjnych ani sprawdzania podpisanych zdjęć.

### Administrator danych {#admin-data-controller}

Każda organizacja jest administratorem danych osobowych, które rejestruje. Na stronie organizacji sekcja **Administrator danych** zawiera jej **Nazwa prawna**, **Kontakt (e-mail lub telefon)** i **Adres**; są one podane w informacji o danych organizacji. Aktualizuje je Specjalista ds. zgodności organizacji lub administrator platformy.

## Użytkownicy {#admin-users}

**Zarządzanie Użytkownikami** → **Użytkownicy** zawiera listę kont.

- **Dodaj nowego użytkownika** tworzy konto. W modelu organizacji pozostaw pole **Rola globalna** puste dla członków organizacji i nadaj im członkostwo na stronie organizacji; opcję **Administrator platformy** wybieraj tylko dla administratorów. Wpisz **Telefon komórkowy** i wybierz **Wyślij nazwę użytkownika i hasło tymczasowe SMS-em (zalecane)** albo **Ustaw tu hasło początkowe (użytkownik zmieni je przy pierwszym logowaniu)**.
- **Wyślij logowanie SMS** wysyła nowe dane logowania (nowe hasło tymczasowe, ważne 72 godziny) użytkownikowi, który je zgubił. Nikt, również administrator, nie widzi hasła tymczasowego.
- **Zresetuj hasło** ustawia nowe hasło dla użytkownika; jest ono również traktowane jako hasło początkowe, które użytkownik zmienia przy następnym logowaniu.
- **Odblokuj** pojawia się przy koncie ze statusem **Zablokowana** i natychmiast kończy blokadę („Konto zostało odblokowane.”). Bez tego blokada wygasa samoczynnie po 15 minutach.
- **Usuń** usuwa konto (używaj ostrożnie; tej operacji nie można cofnąć). Nie możesz usunąć własnego konta. Każde usunięcie i każde odblokowanie jest zapisywane w dzienniku audytu.

Nowe hasła muszą mieć co najmniej 10 znaków i nie mogą być nazwą użytkownika ani popularnym hasłem; dotychczasowe hasła pozostają ważne. Po 5 nieudanych próbach logowania konto zostaje zablokowane na 15 minut.

**Zarządzanie Użytkownikami** → **Role** zawiera listę ról globalnych. Role, na których opiera się platforma (**Administrator** i **Pszczelarz**), są oznaczone jako **Chroniona** i nie można zmieniać ich nazw ani ich usuwać.

## Role i uprawnienia {#admin-permissions}

Strona **Role i uprawnienia** ma trzy zakładki:

- **Członkowie**: wszystkie aktywne członkostwa; dodawanie i cofanie.
- **Uprawnienia ról**: wybierz rolę i dla każdej funkcji zaznacz **Wprowadzanie**, **Zatwierdzenie** i **Odczyt**. Funkcjami są poszczególne KPK (każdy w osobnym wierszu, ze skrótem KPK w języku strony) oraz obszary: przyjęcie, próbki, wyniki laboratoryjne, wysyłki, strony pszczelarzy, wycofanie, ostateczne zatwierdzenie, rejestracja pszczelarzy, dowody, cena i wpisy audytu organizacji. Każde uprawnienie musi zachować co najmniej jedną rolę. Zmiany obowiązują natychmiast.
- **Funkcje organizacji**: funkcje poszczególnych organizacji.

Ramka „Zasady podstawowe (nie można ich wyłączyć):” wymienia zasady, których nie zmienia żadne ustawienie (zob. [Zasady podstawowe](#fixed-rules)). [Table](#tbl-admin-changeable) wymienia dokładnie, co może zmienić administrator.

<!-- SHOT: admin-role-permissions -->
Figure: Uprawnienia roli Specjalista ds. zapewnienia jakości.

## Odpowiedzialności za KPK {#admin-ccp-responsibilities}

Strona **Odpowiedzialności za KPK** określa dla każdego KPK, która organizacja jest odpowiedzialna (właściciel partii lub partii końcowej albo, w przypadku partii, stowarzyszenie pszczelarza), jakiej funkcji potrzebuje, które role wprowadzają wartości, które role je zatwierdzają oraz czy zatwierdzenie jest wymagane. Przykład: aby przypisać KPK 0 stowarzyszeniu, wybierz „Stowarzyszenie pszczelarza danej partii”, **Stowarzyszenie** i **Inspektor terenowy**.

## Punkty kontrolne, parametry i limity {#admin-ccps}

- **Krytyczne Punkty Kontrolne**: kod, tytuł, etap przetwarzania, uzasadnienie krytyczności oraz **Dotyczy** (partia surowego miodu lub partia końcowa). Opcja wyświetlania wartości tekstowych w publicznej Historii miodu powinna pozostać wyłączona, jeśli odpowiedzi mogą zawierać dane osobowe.
- **Parametry**: nazwa, jednostka, kategoria, typ danych (**Liczba**, **Tak / Nie**, **Tekst**, **Plik**), wartość minimalna i maksymalna, parametr wymagany lub opcjonalny oraz warunek „wymagany tylko wtedy, gdy” inne pytanie typu tak/nie ma określoną odpowiedź.
- **Limity**: dla każdego parametru limity domyślne dla wszystkich rodzajów miodu oraz limity szczegółowe według klasy miodu (nektarowy, spadziowy, sosnowy) lub rodzaju miodu, opcjonalnie z warunkiem, sumą lub proporcją. Zapisane wyniki nie są przeliczane po zmianie limitów.

**Zweryfikowane limity.** Każdy parametr ma pole wyboru **Limity zweryfikowane (wyświetlane publicznie)**. Tylko zweryfikowane limity dają publiczną ocenę „w granicach limitu / poza limitem” w Historii miodu i na stronie pszczelarza. Niezweryfikowane limity są oznaczone w panelu jako „do weryfikacji”, a strony publiczne informują, że limity są wciąż sprawdzane.

<!-- SHOT: admin-parameter-limits -->
Figure: Limity parametru (KPK 1, wilgotność).

## Dane referencyjne i tłumaczenia {#admin-reference}

Rodzaje miodu (wraz z klasą miodu stosowaną dla limitów), okresy produkcji, poziomy edukacji, typy i cele pszczelarstwa oraz hurtownicy są prowadzeni w odpowiednich pozycjach menu. Rekordu, który jest w użyciu, nie można usunąć.

Strona **Tłumaczenia danych referencyjnych** zawiera nazwy wyświetlane na listach, w tabelach i na stronach publicznych w pięciu językach: rodzaje miodu, okresy, statusy, funkcje, role, KPK, parametry, kategorie parametrów, uwagi do limitów, objaśnienia parametrów i kraje. Puste pole jest zastępowane tekstem angielskim, a w dalszej kolejności tekstem bazowym.

## Pszczelarze, pasieki i rejestr {#admin-registry}

- **Rejestracja pszczelarzy**: pszczelarzy rejestrują specjaliści ds. rejestracji pszczelarzy powiatowych dyrekcji, kodem SMS lub podpisanym zdjęciem (zob. [rozdział 12](#role-ro)). Administrator platformy może rejestrować pszczelarzy w ten sam sposób; takie rejestracje są zapisywane dla własnej organizacji platformy, a ich podpisane zdjęcia sprawdza inny administrator platformy.
- **Pszczelarze**: lista i strona pszczelarza (imię i nazwisko, data urodzenia, doświadczenie, telefon, wykształcenie, typ i cel pszczelarstwa, opcjonalne zdjęcie). Numer działalności jest generowany automatycznie. Fakty poświadczone przy rejestracji (tożsamość, numer w rejestrze, telefon) są tutaj tylko do odczytu; zmieniają się wyłącznie przez nową rejestrację. Pszczelarzy nigdy się nie usuwa, tylko dezaktywuje. Starsi pszczelarze, zarejestrowani przed wprowadzeniem procedury rejestracji, są ponownie weryfikowani za pomocą tego samego formularza; ich stare loginy pozostają zablokowane.
- **Pasieki**: nazwa, liczba uli i lokalizacja (szerokość i długość geograficzna). Lokalizacja nigdy nie jest wyświetlana publicznie; strony publiczne pokazują tylko prowincję.
- **Rejestr pszczelarzy i stowarzyszenia**: dla każdego pszczelarza przypisz stowarzyszenie i wpisz numer gospodarstwa w rejestrze (format: `TR` + kod prowincji + 10 cyfr, na przykład `TR061234567890`) wraz z dokumentem potwierdzającym. Następnie kliknij **Zweryfikuj** lub **Odrzuć** (odrzucenie wymaga podania powodu).

<!-- SHOT: admin-registry -->
Figure: Rejestr pszczelarzy i stowarzyszenie.

## Partie końcowe (na razie tylko administrator) {#admin-batches}

1. **Hurtownicy** → hurtownik → **Końcowe produkcje miodu (partie końcowe)** → **Nowa partia końcowa**.
2. Wpisz **Tytuł partii** i **Data produkcji**, a następnie kliknij **Utwórz partię**. Kod QR zostaje wygenerowany.
3. Na stronie **Zarządzaj partią** wpisz ilość, która ma zostać użyta z każdej kwalifikującej się partii (partie tego hurtownika, których wszystkie KPK są zatwierdzone), i kliknij **Zapisz skład partii**: „Zapisano skład partii końcowej: łącznie … kg.” Jeśli ilość jest zbyt duża lub partii nie można użyć, strona wymienia każdy problem (na przykład „Partia #…: dostępne jest najwyżej … kg.”) i niczego nie zapisuje. Jeśli zakład ma partie, których żadna partia końcowa nie może użyć, ponieważ nie jest on powiązany z rekordem hurtownika, strona podaje jego nazwę wraz z odnośnikiem do jego strony **Edytuj**.
4. Użyj opcji **Edytuj**, aby ustawić organizację będącą właścicielem partii końcowej (zakład).
5. **Zarządzaj KPK**: zaznacz od KPK 4 do KPK 10 i kliknij **Zapisz przypisania**: „Zapisano przypisania KPK: dodano …, usunięto ….”

Następnie zakład wprowadza KPK partii końcowej i zatwierdza ją ostatecznie. Administratorzy nie mogą zatwierdzać ostatecznie: „W modelu organizacji partię końcową zatwierdza ostatecznie organizacja będąca jej właścicielem (wniosek i zatwierdzenie przez dwie osoby).”

<!-- SHOT: admin-manage-batch -->
Figure: Ustalanie składu partii końcowej z kwalifikujących się partii.

## Korekty {#admin-corrections}

Zatwierdzone lub zablokowane dane można zmienić wyłącznie w drodze korekty:

1. Otwórz stronę KPK danego rekordu. W ramce **Korekty** wybierz parametr, wpisz nową wartość i **Powód**, a następnie kliknij **Złóż wniosek**: „Zgłoszono korektę; musi ją zatwierdzić inny administrator platformy.”
2. **Drugi** administrator otwiera stronę **Korekty**, zapoznaje się ze starą i nową wartością oraz powodem i klika **Zatwierdź i zastosuj** lub **Odrzuć** (odrzucenie wymaga wpisania uwagi).

Osoba, która złożyła wniosek o korektę, nie może jej rozstrzygnąć. Korekty dotyczą również rekordów zablokowanych, a oba kroki są zapisywane w dzienniku audytu.

**Fakty dostawy.** Fakty potwierdzonej dostawy (data, rodzaj i wagi pojemników, numery plomb) koryguje się na stronie dostawy, w sekcji **Korekty tej dostawy** („Tylko w razie błędu w zapisie. Korektę musi potwierdzić inny administrator platformy; oryginał pozostaje tu czytelny, a pszczelarz dostaje SMS.”):

1. Kliknij **Zgłoś korektę**, zmień pola, wpisz **Powód** i kliknij **Wyślij zgłoszenie**: „Korekta zgłoszona; musi ją potwierdzić inny administrator platformy.”
2. Drugi administrator porównuje kolumny **Pierwotnie** i **Po korekcie** i klika **Potwierdź i zastosuj** albo wpisuje **Uwaga (wymagana przy odrzuceniu)** i klika **Odrzuć**. „Korekta zastosowana; pszczelarz dostał SMS.”

Korekta nigdy nie dodaje ani nie usuwa pojemników. Dostawa zachowuje obie treści wraz z ich odciskami; dziennik audytu zapisuje każdy zmieniony wiersz.

<!-- SHOT: admin-delivery-correction -->
Figure: Korekta dostawy oczekująca na drugiego administratora.

## Strony pszczelarzy, wysyłki i próbki {#admin-overviews}

Administratorzy mogą otwierać strony **Strony pszczelarzy**, **Wysyłki i zapasy** oraz **Próbki i laboratorium** w trybie tylko do odczytu, aby śledzić pracę wszystkich organizacji.

## Wycofanie {#admin-recall}

**Raport z wycofania** partii końcowej lub partii zawiera:

- **w górę łańcucha**: partie, pasieki, pszczelarzy i formularze dostawy, a w przypadku dostaw z pojemnikami każdy pojemnik z historią plomb, jego słoiki z próbkami, wyniki laboratoryjne dla każdego pojemnika i informację, czy potwierdzona treść pozostała bez zmian;
- partie końcowe;
- **w dół łańcucha**: wysyłki i obecnych posiadaczy.

Członkowie odczytują tę część, którą może odczytać ich organizacja (jako właściciel, posiadacz lub stowarzyszenie); ukryte wiersze są zliczane. Członek widzi tylko stan własnej organizacji, w sekcji **Twój obecny stan**; administratorzy widzą wszystkich posiadaczy w sekcji **Obecni posiadacze**. Raport z wycofania nigdy nie zawiera cen. Przyciski **Drukuj** i **CSV** eksportują raport.

<!-- SHOT: recall-report -->
Figure: Raport z wycofania partii końcowej.

## Informacje i treści zgód {#admin-notices}

Strona **Informacje i treści zgód** wymienia informacje o danych (dla użytkowników i dla pszczelarzy, platformy i każdej organizacji), treści zgód i oświadczenie przy rejestracji, dla każdego języka, wraz z kolumnami **Wersja**, **Opublikowano** (data) i **Akceptacje** (liczba osób, które je zaakceptowały). „Teksty mają wersje i nie zmienia się ich po publikacji: opublikuj nową wersję.”

1. Kliknij **Opublikuj nową wersję** (lub **Nowa wersja** przy danym tekście).
2. Wybierz **Rodzaj**, odbiorców w polu **Dla**, **Organizacja** (puste dla platformy), w przypadku treści zgody **Cel zgody**, a także **Wersja** i **Język**, i wpisz **Treść**.
3. Zaznacz **Projekt (jeszcze niezatwierdzony przez zespół ochrony danych)**, dopóki tekst nie jest zatwierdzony; jest on wtedy wyświetlany z oznaczeniem **PROJEKT**.
4. Kliknij **Opublikuj**. „Publikacja wycofuje aktualny tekst tego samego rodzaju, odbiorców, organizacji, celu i języka. Użytkownicy, którzy zaakceptowali wcześniejszą wersję, zostaną poproszeni ponownie.”

Publiczna strona **Prywatność i zgoda** (`/privacy`) pokazuje aktualne informacje o danych dla pszczelarzy i dla użytkowników platformy.

## SMS {#admin-sms}

Platforma wysyła SMS-y z kodami rejestracji, danymi logowania, potwierdzeniami dostaw, potwierdzeniami informacji o danych, resetowaniem haseł, weryfikacją telefonu i wiadomościami informacyjnymi. To, który dostawca je wysyła, jest ustawieniem serwera (zob. [Ustawienia w konfiguracji serwera](#admin-settings)). Platforma prowadzi dziennik każdej wiadomości z zamaskowanym numerem; kody i hasła nigdy nie są przechowywane jawnym tekstem. Dzienny limit chroni budżet SMS; resetowanie haseł i potwierdzenia dostaw są wysyłane także po jego osiągnięciu.

Przy **dostawcy testowym** (wyłącznie rozwój i demonstracje) nic nie jest wysyłane: strona **Skrzynka nadawcza SMS (dostawca testowy)** pokazuje zamiast tego każdą wiadomość z jej treścią i kodem, wraz z licznikiem „Części dzisiaj / limit dzienny”.

<!-- SHOT: admin-sms-outbox -->
Figure: Skrzynka nadawcza SMS dostawcy testowego.

## Ustawienia w konfiguracji serwera {#admin-settings}

Niektóre przełączniki ustawia się w konfiguracji serwera, a nie w panelu; ich zmiana wymaga operatora serwera. Każdy z nich włącza się osobno, gdy gotowe jest to, czego wymaga:

Table: Przełączniki w konfiguracji serwera {#tbl-settings}

| Ustawienie | Wyłączone (obecnie) | Włączone | Włączyć, gdy |
|---|---|---|---|
| Dostawca SMS | Wiadomości są tylko zapisywane w dzienniku | Wiadomości wysyła dostawca, z którym zawarto umowę | Pakiet SMS jest zakupiony, a nazwa nadawcy zarejestrowana |
| Dostawy z pojemnikami | Zakłady rejestrują dostawę z jednym podpisanym formularzem i jedną próbką (procedura wcześniejsza) | Procedura pojemników z [rozdziału 5](#sao-intake) | Zakłady mają słoiki na próbki z wieczkiem gwarancyjnym, plomby i etykiety |
| Wymagane potwierdzenie informacji o danych | Baner prosi użytkowników o potwierdzenie informacji | Panel pozostaje zamknięty do czasu potwierdzenia informacji | Treści informacji są zatwierdzone przez zespół ochrony danych |
| Portal pszczelarza | Pszczelarze nie otrzymują danych logowania | Specjaliści ds. rejestracji pszczelarzy wysyłają dane logowania; otwiera się portal z [rozdziału 14](#portal) | Projekt daje zgodę na uruchomienie portalu |
| Publiczne nazwiska starszych pszczelarzy | – | Historia miodu pokazuje neutralny kod, dopóki nie istnieje zgoda na podanie nazwiska (domyślnie) | – (alternatywa zachowuje nazwiska do czasu ponownej rejestracji pszczelarza) |
| Uruchamianie przechowywania danych | Strona **Przechowywanie danych** tylko liczy | Uruchomienie jest możliwe (zob. niżej) | Właściciel zatwierdza pierwsze uruchomienie i format pseudonimów |

## Przechowywanie danych {#admin-retention}

Strona **Przechowywanie danych** pokazuje, jak długo platforma przechowuje każdy rodzaj danych i co dzieje się potem ([Table](#tbl-retention)). Strona jest **przebiegiem próbnym**: „liczy wiersze po upływie okresu i niczego nie zmienia”. **Raport przebiegu próbnego (CSV)** pobiera liczby.

Table: Okresy przechowywania {#tbl-retention}

| Dane | Przechowywane | Następnie |
|---|---|---|
| Kody jednorazowe | 30 dni | usuwane (przechowywano tylko skróty); kod potwierdzający rekord pozostaje z nim |
| Wiadomości SMS | 1 rok | numer i treść usuwane, liczniki zachowane |
| Ceny | 5 lat | usuwane |
| Partie, przyjęcia, pojemniki, próbki, raporty laboratoryjne, partie końcowe | 5 lat po najpóźniejszej dacie minimalnej trwałości partii końcowej (data partii + 2 lata) lub po dostawie | powiązanie z pszczelarzem pseudonimizowane; dane produktu zachowane |
| Tożsamość pszczelarza (nieaktywni pszczelarze) | póki aktywny, potem okres jego partii | pseudonimizowana |
| Zdjęcia dowodowe (podpisane formularze dostaw) | tak długo jak rekord, który potwierdzają | usuwane, skrót zachowany |
| Zgody i akceptacje informacji | tak długo jak powiązane dane + 3 lata (dowód) | usuwane |
| Konta byłych członków | 2 lata po zakończeniu ostatniego członkostwa | anonimizowane; wpisy audytu zachowują tylko identyfikator użytkownika |
| Dziennik audytu | 10 lat | niezmieniany (tylko dopisywanie) |

Uruchamianie przechowywania jest **wyłączone** („Wyłączone: o pierwszym uruchomieniu decyduje właściciel”). Gdy właściciel je zatwierdzi, a operator platformy je włączy, pojawi się przycisk **Uruchom przechowywanie teraz**; wymaga on potwierdzenia „Właściciel zatwierdził to uruchomienie (usuwa i czyści dane, nie można go cofnąć).” Uruchomienie usuwa stare kody jednorazowe, czyści numer i treść starych wiadomości SMS i usuwa stare ceny; jest zapisywane w dzienniku audytu. Pozostałe reguły są tylko liczone („tylko liczy (decyzja O-14)”), dopóki nie zostanie ustalony format pseudonimów. Comiesięczne uruchamianie można włączyć dopiero po pierwszym uruchomieniu ręcznym.

**Data minimalnej trwałości.** Partie końcowe nie mają daty minimalnej trwałości; zastępuje ją data partii plus 2 lata (ustawienie). Producent potwierdza rzeczywisty okres przydatności swojego miodu; okres zostaje wtedy odpowiednio ustawiony.

## Dziennik audytu {#admin-audit}

Zob. [Dziennik audytu](#trust-audit).
