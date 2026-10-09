# 3. Organizacje, role i uprawnienia {#roles}

W TOP4HoneyChains nikt nie działa samodzielnie: każda czynność jest wykonywana **w imieniu organizacji, w określonej roli**. O tym, co możesz robić, decydują trzy elementy:

1. **funkcje** Twojej organizacji (jakim rodzajem pracy się zajmuje);
2. Twoja **rola** w organizacji (kim jesteś w jej ramach);
3. **status** Twojej organizacji: dane może wprowadzać tylko organizacja **Zweryfikowana**.

Ponadto niektóre zasady są zapisane na stałe w kodzie i nie można ich wyłączyć (zob. [Stałe zasady](#fixed-rules)).

## Typy organizacji (funkcje) {#capabilities}

Organizacja ma jedną lub więcej funkcji. Ustala je administrator platformy.

| Funkcja | Typowa organizacja | Na co pozwala |
|---|---|---|
| **Stowarzyszenie** | Stowarzyszenie pszczelarzy | Przypisuje pszczelarzy członkowskich; rejestruje zgody pszczelarzy na publiczną stronę pszczelarza |
| **Związek centralny** | Związek krajowy | Opcjonalnie: może weryfikować stowarzyszenia, jeśli platforma jest tak skonfigurowana |
| **Przetwarzanie** | Zakład przetwórstwa miodu | Rejestruje dostawy, pobiera próbki, rejestruje KPK 0–3 partii oraz KPK 4–6 partii końcowej |
| **Konfekcjonowanie** | Zakład rozlewu i pakowania | Rejestruje KPK 7–9 partii końcowej (rozlew, etykietowanie, pakowanie) oraz KPK 10 |
| **Laboratorium** | Laboratorium analityczne (akredytowane lub własne) | Przyjmuje próbki, wprowadza i zatwierdza raporty laboratoryjne |
| **Handel hurtowy** | Hurtownik | Posiada partie końcowe; rejestruje przechowywanie w KPK 10 |
| **Handel detaliczny** | Sklep lub market | Posiada partie końcowe; rejestruje przechowywanie w KPK 10 |
| **Organ publiczny** | Ministerstwo, organ kontrolny | Przegląda zapisy (tylko do odczytu) |
| **Pszczelarstwo** | Gospodarstwo pszczelarskie | Zarezerwowana dla gospodarstw pszczelarskich; pszczelarze nie logują się do platformy |
| **Administrator platformy** | Operator platformy | Zarezerwowana dla organizacji prowadzącej platformę |

Zakład przetwórczy, który sam rozlewa i przechowuje swój miód, potrzebuje funkcji **Przetwarzanie**, **Konfekcjonowanie** i **Handel hurtowy** (lub **Handel detaliczny**), ponieważ KPK 7–9 partii końcowej wymagają funkcji Konfekcjonowanie, a KPK 10 wymaga funkcji Handel hurtowy, Handel detaliczny lub Konfekcjonowanie.

### Status organizacji {#org-status}

| Status | Znaczenie |
|---|---|
| **Oczekująca** | Nowa organizacja oczekująca na weryfikację. Członkowie mogą przeglądać dane, a administrator organizacji może przesyłać dokumenty. Nikt nie może wprowadzać danych. |
| **Zweryfikowana** | Sprawdzona przez administratora platformy. Członkowie mogą pracować zgodnie ze swoimi rolami. |
| **Zawieszona** | Weryfikacja cofnięta (z podaniem powodu). Do czasu przywrócenia działa jak organizacja oczekująca. |

## Role {#role-list}

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

- **Zasada czterech oczu dla rekordów KPK**: nikt nie zatwierdza rekordu KPK, w którym sam zarejestrował wartości. Wyjątek: administrator platformy może przyznać organizacji **Wyjątek od zasady czterech oczu**; jej członkowie mogą wtedy zatwierdzać własne rekordy KPK, a każde takie zatwierdzenie jest oznaczane w dzienniku audytu.
- **Zasada czterech oczu dla wyników laboratoryjnych, zawsze**: osoba, która wprowadziła wyniki, nie może ich zatwierdzić. Nie ma od tego wyjątku.
- **Zasada czterech oczu przy ostatecznym zatwierdzeniu**: osoba, która złożyła wniosek o ostateczne zatwierdzenie, nie może go potwierdzić (chyba że organizacja ma wyjątek).
- **Dane wprowadzają tylko zweryfikowane organizacje.**
- **Administratorzy platformy mają dostęp tylko do odczytu** do danych biznesowych i korygują je wyłącznie w procesie korekty z udziałem dwóch administratorów.
- **Zatwierdzone ostatecznie oznacza zablokowane**: partii końcowej zatwierdzonej ostatecznie ani żadnej z jej partii nie można już zmienić, chyba że w drodze korekty.
- **Do dziennika audytu można tylko dopisywać**; niczego w nim nie można zmienić ani usunąć, a każda zmiana na stronach uprawnień również jest rejestrowana.
