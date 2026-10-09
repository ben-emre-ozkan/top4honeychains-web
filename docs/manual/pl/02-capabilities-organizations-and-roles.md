# 2. Funkcje, organizacje i role {#roles}

W TOP4HoneyChains nikt nie działa samodzielnie: każda czynność jest wykonywana **w imieniu organizacji, w określonej roli**. O tym, co możesz robić, decydują trzy elementy:

1. **funkcje** Twojej organizacji (jakim rodzajem pracy się zajmuje);
2. Twoja **rola** w organizacji (kim jesteś w jej ramach);
3. **status** Twojej organizacji: dane może wprowadzać tylko organizacja **Zweryfikowana**.

Ponadto niektóre zasady są zapisane na stałe w kodzie i nie można ich wyłączyć (zob. [Stałe zasady](#fixed-rules)).

## Funkcje {#capabilities}

Funkcja określa, jaką pracę organizacja może wykonywać w łańcuchu miodu. To, który członek ją wykonuje, określają role (zob. [Role](#role-list)). Organizacja ma jedną lub więcej funkcji; nadaje je i odbiera administrator platformy (zob. [Funkcje](#admin-org-capabilities)). Sama lista funkcji jest stała.

| Funkcja (na ekranie) | Znaczenie (opis na ekranie) | Co może robić organizacja | KPK (domyślnie) |
|---|---|---|---|
| **Stowarzyszenie** | Stowarzyszenie pszczelarzy; łączy pszczelarzy członkowskich i rejestruje ich zgodę na publiczną stronę pszczelarza | Przypisuje pszczelarzy członkowskich, rejestruje i wycofuje ich zgody oraz publikuje ich strony pszczelarzy (**Strony pszczelarzy**); widzi partie swoich pszczelarzy członkowskich, na przykład w raportach wycofania | brak; KPK 0 tylko wtedy, gdy administrator przypisze go stowarzyszeniu |
| **Przetwarzanie** | Przyjęcie i przetwarzanie: dostawy, próbki, KPK 0–3 partii i KPK 4–6 partii końcowej | Rejestruje dostawy, co tworzy należące do niej partie miodu surowego; kontroluje je przy przyjęciu; pobiera zaplombowane próbki i wysyła je do laboratorium; rejestruje KPK przetwarzania (**Dostawy (przyjęcie)**, **Próbki i laboratorium**) | KPK 0–3 partii, KPK 4–6 partii końcowej |
| **Konfekcjonowanie** | Rozlew, etykietowanie i pakowanie, KPK 7-9 | Rejestruje rozlew, etykietowanie i pakowanie swoich partii końcowych oraz przechowywanie w KPK 10 w zakładzie | KPK 7–9 partii końcowej, KPK 10 |
| **Laboratorium** | Laboratorium (akredytowane lub zakładowe) | Przyjmuje zaplombowane próbki, wprowadza wyniki laboratoryjne i zatwierdza raport; raport zawsze zatwierdza druga osoba. Numer akredytacji i data jej ważności są przechowywane przy tej funkcji (**Próbki i laboratorium**) | brak; jego zatwierdzone wyniki wypełniają KPK 1 partii |
| **Handel hurtowy** | Handel hurtowy; posiadacz w KPK 10, dopóki ma towar | Przechowuje otrzymane partie końcowe, rejestruje przechowywanie w KPK 10 dla posiadanej ilości i może wysyłać dalej (**Wysyłki i zapasy**) | KPK 10 |
| **Handel detaliczny** | Handel detaliczny; posiadacz w KPK 10, dopóki ma towar | Przyjmuje przesyłki do sprzedaży, rejestruje przechowywanie w KPK 10 dla posiadanej ilości i może wysyłać dalej (**Wysyłki i zapasy**) | KPK 10 |

Zakład przetwórczy, który sam rozlewa i przechowuje swój miód, potrzebuje funkcji **Przetwarzanie**, **Konfekcjonowanie** i **Handel hurtowy** (lub **Handel detaliczny**), ponieważ KPK 7–9 partii końcowej wymagają funkcji Konfekcjonowanie, a KPK 10 wymaga funkcji Handel hurtowy, Handel detaliczny lub Konfekcjonowanie.

Lista zawiera też trzy funkcje dla przypadków szczególnych:

- **Związek centralny**: krajowy związek pszczelarzy. Jeśli platforma jest skonfigurowana tak, aby stowarzyszenia weryfikował związek centralny, administrator organizacji zweryfikowanego związku centralnego weryfikuje stowarzyszenia i widzi stowarzyszenia, które weryfikuje.
- **Organ publiczny**: ministerstwo lub organ kontrolny. Po weryfikacji jego członkowie przeglądają wszystkie zapisy; niczego nie wprowadza.
- **Pszczelarstwo**: gospodarstwo pszczelarza. Pszczelarze nie logują się do platformy; są zapisami prowadzonymi przez platformę i ich stowarzyszenie.

**Platforma.** *Administrator platformy* nie jest funkcją, którą można nadać organizacji: to globalna rola konta użytkownika, przeznaczona dla osób prowadzących platformę. Administratorzy platformy zarządzają organizacjami, członkami, danymi referencyjnymi i ustawieniami oraz mogą przeglądać każdy rekord, ale nie wprowadzają danych biznesowych (zob. [rozdział 12](#admin)). Własna organizacja platformy ma zarezerwowaną funkcję **Administrator platformy**; nie można jej nadać żadnej innej organizacji ani jej odebrać.

## Typy organizacji {#org-types}

Platforma nie ma osobnej listy typów organizacji: typ organizacji wynika z posiadanych przez nią funkcji. Typowe organizacje:

| Organizacja | Funkcje | Typowe role członków |
|---|---|---|
| Stowarzyszenie pszczelarzy | **Stowarzyszenie** | Administrator organizacji; Inspektor terenowy |
| Zakład przetwórczy lub firma konfekcjonująca | **Przetwarzanie**, **Konfekcjonowanie** oraz **Handel hurtowy** lub **Handel detaliczny** do przechowywania w zakładzie | Administrator organizacji, Pracownik przyjmujący próbki, Personel, Zatwierdzający jakość |
| Laboratorium (akredytowane lub zakładowe) | **Laboratorium** | Administrator organizacji, Analityk, Kierownik laboratorium |
| Hurtownik | **Handel hurtowy** | Administrator organizacji, Personel, Zatwierdzający jakość |
| Sklep lub market | **Handel detaliczny** | Administrator organizacji, Personel, Zatwierdzający jakość |
| Ministerstwo lub organ kontrolny | **Organ publiczny** | Administrator organizacji (tylko odczyt) |

Stowarzyszenie, które prowadzi własny zakład przetwórczy, ma również funkcję **Przetwarzanie** (i **Konfekcjonowanie**), a wtedy także role zakładu.

### Status organizacji {#org-status}

| Status | Znaczenie |
|---|---|
| **Oczekująca** | Nowa organizacja oczekująca na weryfikację. Członkowie mogą przeglądać dane, a administrator organizacji może przesyłać dokumenty. Nikt nie może wprowadzać danych. |
| **Zweryfikowana** | Sprawdzona przez administratora platformy. Członkowie mogą pracować zgodnie ze swoimi rolami. |
| **Zawieszona** | Weryfikacja cofnięta (z podaniem powodu). Do czasu przywrócenia działa jak organizacja oczekująca. |

## Role {#role-list}

Rolę pełni się w ramach **członkostwa**: jedna osoba, jedna organizacja, jedna rola. Członkostwa tworzą administratorzy platformy (zob. [Członkowie](#admin-members)); jedna osoba może mieć kilka członkostw (zob. [Kilka członkostw](#memberships)).

| Rola (na ekranie) | Kod | Typowa osoba | Główne zadania |
|---|---|---|---|
| **Administrator organizacji** | OrgAdmin | Kierownik organizacji | Przesyła dokumenty, składa wnioski o ostateczne zatwierdzenie, wysyła towar i potwierdza wysyłki, rejestruje zgody pszczelarzy (stowarzyszenia), przegląda przyjęcia i próbki |
| **Personel** | Staff | Pracownik produkcji, rozlewni lub sklepu | Wprowadza wartości KPK 2–10, składa wnioski o ostateczne zatwierdzenie, wysyła towar i potwierdza wysyłki, rejestruje przechowywanie |
| **Inspektor terenowy** | FieldInspector | Technik terenowy stowarzyszenia | Przegląda zapisy; wprowadza KPK 0 tylko wtedy, gdy administrator przypisze ten KPK stowarzyszeniu |
| **Pracownik przyjmujący próbki** | SampleAcceptanceOfficer | Pracownik przyjęcia w zakładzie | Rejestruje dostawy, wprowadza KPK 0, pobiera i wysyła próbki |
| **Zatwierdzający jakość** | QualityApprover | Kierownik ds. jakości, technolog żywności | Zatwierdza rekordy KPK, potwierdza ostateczne zatwierdzenie, zatwierdza rekordy przechowywania |
| **Analityk** | Analyst | Analityk laboratoryjny | Przyjmuje próbki, wprowadza wyniki laboratoryjne, przesyła raporty |
| **Kierownik laboratorium** | LabManager | Kierownik laboratorium | Przyjmuje próbki, zatwierdza lub zwraca raporty laboratoryjne |
| **Administrator platformy** | (rola globalna) | Operator platformy | Administracja (rozdział 12); dane biznesowe tylko do odczytu |

Administrator platformy nie uczestniczy w procesach organizacji biznesowych: może przeglądać każdy rekord, ale **nie może wprowadzać ani zatwierdzać danych biznesowych** (dostaw, wartości KPK, wyników laboratoryjnych, wysyłek, zgód). Jeśli po zatwierdzeniu dane okażą się błędne, administrator składa wniosek o *korektę*, którą zatwierdza drugi administrator (zob. [Korekty](#admin-corrections)).

## Macierz uprawnień {#permission-matrix}

Poniższa macierz przedstawia **domyślne** uprawnienia w obecnej konfiguracji. Większość z nich administrator platformy może zmienić na stronach **Role i uprawnienia** oraz **Odpowiedzialności za KPK**; opisanych niżej stałych zasad nie można zmienić.

Legenda: **O** odczyt, **W** tworzenie lub wprowadzanie, **Z** zatwierdzanie (lub przyjęcie), **OZ** ostateczne zatwierdzenie (wniosek lub potwierdzenie), **–** brak uprawnienia. Kolumna organizacji wskazuje, jakiej funkcji potrzebuje organizacja.

| Czynność | Wymagana funkcja | Administrator organizacji | Personel | Inspektor terenowy | Pracownik przyjmujący próbki | Zatwierdzający jakość | Analityk | Kierownik laboratorium | Administrator platformy |
|---|---|---|---|---|---|---|---|---|---|
| Rejestracja dostawy (przyjęcie) | Przetwarzanie | O | – | – | W O | – | – | – | O |
| Pobranie i wysłanie próbki | Przetwarzanie (właściciel partii) | O | – | – | W O | – | O | O | O |
| Przyjęcie próbki (kontrola plomby) | Laboratorium | – | – | – | – | – | Z | Z | O |
| Wprowadzenie wyników laboratoryjnych, przesłanie raportu | Laboratorium | O | O | O | O | O | W O | O | O |
| Zatwierdzenie lub zwrot raportu laboratoryjnego | Laboratorium | O | O | O | O | O | O | Z O | O |
| KPK 0 – kontrola przy przyjęciu (partia) | Przetwarzanie (właściciel partii) | O | O | O | W O | Z O | O | O | O |
| KPK 1 – wartości laboratoryjne (partia) | przepisywane z zatwierdzonego raportu laboratoryjnego | O | O | O | O | O | O | O | O |
| KPK 2–3 (partia) | Przetwarzanie (właściciel partii) | O | W O | O | O | Z O | O | O | O |
| Utworzenie partii końcowej, ustalenie jej składu, przypisanie KPK | – | – | – | – | – | – | – | – | W |
| KPK 4–6 (partia końcowa) | Przetwarzanie (właściciel partii końcowej) | O | W O | O | O | Z O | O | O | O |
| KPK 7–9 (partia końcowa) | Konfekcjonowanie (właściciel partii końcowej) | O | W O | O | O | Z O | O | O | O |
| KPK 10 – przechowywanie w zakładzie (partia końcowa) | Handel hurtowy, Handel detaliczny lub Konfekcjonowanie (właściciel partii końcowej) | O | W O | O | O | Z O | O | O | O |
| Wniosek o ostateczne zatwierdzenie | właściciel, zweryfikowany | OZ | OZ | – | – | – | – | – | – |
| Potwierdzenie ostatecznego zatwierdzenia (zamknięcie) | właściciel, zweryfikowany | – | – | – | – | OZ | – | – | – |
| Utworzenie wysyłki | posiadacz, zweryfikowany | W O | W O | – | – | O | – | – | O |
| Potwierdzenie lub odrzucenie otrzymanej wysyłki | odbiorca, zweryfikowany | Z O | Z O | – | – | O | – | – | O |
| Rekord przechowywania KPK 10 u odbiorcy | Handel hurtowy, Handel detaliczny lub Konfekcjonowanie | O | W O | O | O | Z O | O | O | O |
| Raport z wycofania (własny zakres) | – | O | O | O | O | O | O | O | O (wszystko) |
| Rejestracja lub wycofanie zgody pszczelarza | Stowarzyszenie | W O | – | – | – | – | – | – | O |
| Przesyłanie dokumentów organizacji | własna organizacja | W | – | – | – | – | – | – | W |
| Weryfikacja organizacji, zarządzanie członkami i funkcjami | – | – | – | – | – | – | – | – | W Z |
| Weryfikacja numerów pszczelarzy w rejestrze | – | – | – | – | – | – | – | – | Z |
| Zmiana uprawnień, KPK, limitów, tłumaczeń | – | – | – | – | – | – | – | – | W |
| Wniosek o korektę lub decyzja w sprawie korekty | – | – | – | – | – | – | – | – | W Z (dwóch różnych administratorów) |
| Dziennik audytu | – | – | – | – | – | – | – | – | O |

Uwagi:

- **Odczyt KPK (O)**: każda rola może otwierać KPK partii i partii końcowych, w których uczestniczy jej organizacja: jako właściciel lub organizacja odpowiedzialna, jako organizacja, która pobrała próbkę (w przypadku partii), jako późniejszy posiadacz partii końcowej lub jako zweryfikowany organ publiczny. Posiadacz widzi KPK partii końcowej i jej partii **tylko do odczytu**. Każda inna organizacja otrzymuje stronę „Nie możesz otworzyć tej strony” (403).
- **KPK 1** jest wypełniany na podstawie zatwierdzonego raportu laboratoryjnego. Skonfigurowane dla niego role wprowadzające (Pracownik przyjmujący próbki, Personel) są używane tylko dla tych parametrów KPK 1, których raport laboratoryjny nie obejmuje.
- **Wysyłki i ostateczne zatwierdzenie** sprawdzają rolę i status organizacji, a nie jej funkcję: każdy zweryfikowany właściciel partii końcowej zatwierdzonej ostatecznie może ją wysłać.
- Administrator może przypisać **KPK 0** stowarzyszeniu pszczelarza zamiast zakładowi (**Odpowiedzialności za KPK** → KPK 0 → „Stowarzyszenie pszczelarza danej partii”, funkcja Stowarzyszenie, rola Inspektor terenowy). Nie jest to ustawienie domyślne.

<!-- SHOT: admin-permissions-matrix -->

## Stałe zasady {#fixed-rules}

Poniższe zasady są wbudowane w platformę:

- **Zasada czterech oczu dla rekordów KPK**: nikt nie zatwierdza rekordu KPK, w którym sam zarejestrował wartości lub który sam oznaczył jako „Nie dotyczy”. Wyjątek: administrator platformy może przyznać organizacji **Wyjątek od zasady czterech oczu**; jej członkowie mogą wtedy zatwierdzać własne rekordy KPK, a każde takie zatwierdzenie jest oznaczane w dzienniku audytu.
- **Zasada czterech oczu dla wyników laboratoryjnych, zawsze**: osoba, która wprowadziła wyniki, nie może ich zatwierdzić. Nie ma od tego wyjątku.
- **Zasada czterech oczu przy ostatecznym zatwierdzeniu**: osoba, która złożyła wniosek o ostateczne zatwierdzenie, nie może go potwierdzić (chyba że organizacja ma wyjątek).
- **Dane wprowadzają tylko zweryfikowane organizacje.**
- **Administratorzy platformy mają dostęp tylko do odczytu** do danych biznesowych i korygują je wyłącznie w procesie korekty z udziałem dwóch administratorów.
- **Zatwierdzone ostatecznie oznacza zablokowane**: partii końcowej zatwierdzonej ostatecznie ani żadnej z jej partii nie można już zmienić, chyba że w drodze korekty.
- **Do dziennika audytu można tylko dopisywać**; niczego w nim nie można zmienić ani usunąć, a każda zmiana na stronach uprawnień również jest rejestrowana.

## Kilka członkostw {#memberships}

Jedna osoba może mieć kilka członkostw: w różnych organizacjach albo kilka ról w tej samej organizacji (na przykład **Zatwierdzający jakość** i **Administrator organizacji** w stowarzyszeniu). W danej chwili zawsze pracujesz w ramach jednego członkostwa.

Aby je przełączyć:

1. Kliknij **wskaźnik organizacja · rola** na górnym pasku (zob. [Górny pasek i Twoja rola](#top-bar)). Lista z nagłówkiem **Twoje role** pokazuje wszystkie aktywne członkostwa; bieżące jest zaznaczone.
2. Kliknij inne członkostwo. Platforma otworzy stronę **Moja organizacja** w nowym członkostwie.

Menu, zadania na stronie **Moja praca**, widoczne rekordy i dozwolone czynności zależą od aktywnego członkostwa. Wybór obowiązuje do zamknięcia przeglądarki; potem zaczynasz ponownie w pierwszym członkostwie (alfabetycznie według nazwy organizacji). Jeśli masz co najmniej dwa członkostwa, strona **Moja organizacja** zawiera również listę **Działaj jako** z przyciskiem **Przełącz**.
