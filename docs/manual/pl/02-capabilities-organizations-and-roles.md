# 2. Funkcje, organizacje i role {#roles}

W TOP4HoneyChains nikt nie działa samodzielnie: każda czynność jest wykonywana **w imieniu organizacji, w określonej roli**. O tym, co możesz robić, decydują trzy elementy:

1. **funkcje** Twojej organizacji: co organizacja może robić w łańcuchu miodu;
2. Twoja **rola** w organizacji: kim jesteś w jej ramach;
3. **status** Twojej organizacji: dane może wprowadzać tylko organizacja **Zweryfikowana**. Organizacje weryfikuje administrator platformy (zob. [Status organizacji](#org-status)).

Rolę pełni się w ramach **członkostwa**: jedno konto użytkownika × jedna organizacja × jedna rola (zob. [Kilka członkostw](#memberships)). Jedynym wyjątkiem są pszczelarze: pszczelarz może mieć osobiste konto w portalu pszczelarza, powiązane z jego rekordem pszczelarza i bez żadnego członkostwa (zob. [rozdział 14](#portal)). Ponadto niektóre zasady są wbudowane w platformę i nie można ich wyłączyć (zob. [Zasady podstawowe](#fixed-rules)).

Ten rozdział rozróżnia dwie rzeczy:

- **czynności w łańcuchu wartości**: pracę, którą ludzie wykonują z miodem poza komputerem, taką jak rejestrowanie, dostarczanie, pobieranie próbek, analizowanie, przetwarzanie, przechowywanie i wysyłanie ([Table](#tbl-activities));
- **prawa do danych**: co każda rola może robić z rekordami na platformie: tworzyć, odczytywać, aktualizować, usuwać i zatwierdzać ([Table](#tbl-crud)).

## Funkcje {#capabilities}

Funkcja to zdolność organizacji: określa, co organizacja **może robić** w łańcuchu miodu. To, który członek wykonuje daną pracę, określają role (zob. [Role](#role-list)). Organizacja ma jedną lub więcej funkcji; nadaje je i odbiera administrator platformy (zob. [Funkcje](#admin-org-capabilities)). Sama lista funkcji jest stała. [Table](#tbl-capabilities) wymienia funkcje wraz z ich nazwami na ekranie.

Table: Funkcje: co organizacja może robić {#tbl-capabilities}

| Funkcja (na ekranie) | Zdolność | KPK (domyślnie) |
|---|---|---|
| **Stowarzyszenie** | Zdolność do przypisywania pszczelarzy członkowskich i rejestrowania ich zgody na publiczną stronę pszczelarza. | brak; KPK 0 tylko wtedy, gdy administrator przypisze go stowarzyszeniu |
| **Przetwarzanie** | Zdolność do przyjmowania i przetwarzania miodu: dostawy, próbki, KPK 0–3 partii i KPK 4–6 partii końcowej. | KPK 0–3 partii, KPK 4–6 partii końcowej |
| **Konfekcjonowanie** | Zdolność do rozlewu, etykietowania i pakowania miodu: KPK 7–9 partii końcowej oraz przechowywanie w KPK 10 w zakładzie. | KPK 7–9 partii końcowej, KPK 10 |
| **Laboratorium** | Zdolność do analizowania próbek miodu i wydawania raportów laboratoryjnych, jako akredytowane lub zakładowe laboratorium analizy żywności. Numer akredytacji i data jej ważności są przechowywane przy tej funkcji. | brak; jego zatwierdzone wyniki wypełniają KPK 1 partii |
| **Handel hurtowy** | Zdolność do posiadania partii końcowych w handlu hurtowym, rejestrowania ich przechowywania (KPK 10) i wysyłania ich dalej. | KPK 10 |
| **Handel detaliczny** | Zdolność do posiadania partii końcowych przeznaczonych do sprzedaży konsumentom, rejestrowania ich przechowywania (KPK 10) i wysyłania ich dalej. | KPK 10 |
| **Rejestracja pszczelarzy** | Zdolność do rejestrowania pszczelarzy ze swojego obszaru i ich pasiek, poświadczania ich tożsamości i numeru w rejestrze, rejestrowania ich zgody i wydawania im danych logowania. | brak |
| **Związek centralny** | Zdolność do weryfikowania stowarzyszeń pszczelarzy, jeśli platforma jest tak skonfigurowana. | brak |
| **Organ publiczny** | Zdolność do odczytu wszystkich zapisów, jako ministerstwo lub organ kontrolny; niczego nie wprowadza. | brak (odczytuje wszystko) |
| **Pszczelarstwo** | Zdolność do wytwarzania miodu. Pszczelarze nie mają członkostw; ich gospodarstwo reprezentuje rekord pszczelarza. | brak |

Zakład przetwórczy, który sam rozlewa i przechowuje swój miód, potrzebuje funkcji **Przetwarzanie**, **Konfekcjonowanie** i **Handel hurtowy** (lub **Handel detaliczny**), ponieważ KPK 7–9 partii końcowej wymagają funkcji Konfekcjonowanie, a KPK 10 wymaga funkcji Handel hurtowy, Handel detaliczny lub Konfekcjonowanie. Funkcję sprawdzają też same czynności: pobranie próbki wymaga funkcji **Przetwarzanie**; wysyłka i odbiór wymagają funkcji **Przetwarzanie**, **Konfekcjonowanie**, **Handel hurtowy** lub **Handel detaliczny**; wniosek o ostateczne zatwierdzenie i jego potwierdzenie wymagają funkcji **Przetwarzanie** lub **Konfekcjonowanie**.

**Platforma.** *Administrator platformy* nie jest funkcją, którą można nadać organizacji: to globalna rola konta użytkownika, przeznaczona dla osób prowadzących platformę. Administratorzy platformy zarządzają organizacjami, członkostwami, danymi referencyjnymi i ustawieniami oraz odczytują każdy rekord, ale nie wprowadzają danych procesowych (zob. [rozdział 15](#admin)). Własna organizacja platformy ma zarezerwowaną funkcję **Administrator platformy**; nie można jej nadać żadnej innej organizacji ani jej odebrać.

## Czynności w łańcuchu wartości {#activities}

Czynności w łańcuchu wartości wykonują ludzie, z miodem, poza komputerem. Platforma rejestruje wynik każdej czynności jako dane. [Table](#tbl-activities) pokazuje dla każdej funkcji, jakie czynności wykonują jej ludzie i co jest rejestrowane na platformie.

Table: Czynności w łańcuchu wartości (ludzie) i to, co rejestruje platforma (komputer) {#tbl-activities}

| Funkcja | Czynności w łańcuchu wartości (ludzie, poza komputerem) | Rejestrowane na platformie (w komputerze) |
|---|---|---|
| Pszczelarstwo | Utrzymywanie pszczół, zbiór miodu, dostarczenie go do zakładu w pojemnikach, odczytanie zakładowi kodu z SMS-a, ustalenie ceny oczekiwanej | Rekordy pszczelarzy i pasiek; potwierdzenie dostawy (kod SMS); w portalu: cena oczekiwana i własne zgody pszczelarza |
| Rejestracja pszczelarzy | Spotkanie z pszczelarzem, sprawdzenie dokumentu tożsamości i wpisu w rejestrze, odczytanie mu informacji o danych | Rekordy pszczelarzy i pasiek, rejestracja (kod SMS lub podpisane zdjęcie), zgody udzielone przy rejestracji, dane logowania pszczelarza |
| Stowarzyszenie | Przypisywanie i informowanie pszczelarzy członkowskich, odczytywanie im treści zgody, zbieranie podpisów | Rekordy zgód; publiczne strony pszczelarzy |
| Przetwarzanie | Przyjmowanie dostaw w pojemnikach, sprawdzanie lub zakładanie plomb, napełnianie z każdego pojemnika słoika z próbką i słoika z próbką kontrolną, podgrzewanie, filtrowanie, mieszanie i odstawianie miodu | Dostawa z pojemnikami i historią plomb, słoiki z próbkami, dane KPK 0 i KPK 2–6 |
| Konfekcjonowanie | Rozlew, etykietowanie i pakowanie miodu, przechowywanie go w zakładzie | Dane KPK 7–10 |
| Laboratorium | Przyjmowanie zaplombowanych słoików i sprawdzanie plomby, etykiety i wieczka; utworzenie próbki zbiorczej; analiza miodu; podpisanie raportu | Kontrole przy przyjęciu, próbka zbiorcza, raport laboratoryjny (PDF) i wyniki (KPK 1) |
| Handel hurtowy, Handel detaliczny | Przyjmowanie towaru, przechowywanie go, sprzedaż lub wysyłanie dalej | Potwierdzenie wysyłki, rekord przechowywania (KPK 10), nowe wysyłki |
| Związek centralny | Sprawdzanie stowarzyszeń pszczelarzy | Weryfikacja stowarzyszeń (jeśli skonfigurowano) |
| Organ publiczny | Kontrola | Odczytuje zapisy |

Każda organizacja, niezależnie od swoich funkcji, ma również **Specjalistę ds. zgodności**: osobę, która nadzoruje zatwierdzenia, dowody i ochronę danych (zob. [rozdział 13](#role-co)).

## Typy organizacji {#org-types}

Platforma nie ma osobnej listy typów organizacji: typ organizacji wynika z posiadanych przez nią funkcji. Typowe organizacje wymienia [Table](#tbl-org-types).

Table: Typowe organizacje i ich funkcje {#tbl-org-types}

| Organizacja | Funkcje | Typowe role członków |
|---|---|---|
| Powiatowa dyrekcja rolnictwa | **Rejestracja pszczelarzy** | Administrator organizacji, Specjalista ds. rejestracji pszczelarzy, Specjalista ds. zgodności |
| Stowarzyszenie pszczelarzy | **Stowarzyszenie** | Administrator organizacji, Inspektor terenowy, Specjalista ds. zgodności |
| Zakład przetwórczy lub firma konfekcjonująca | **Przetwarzanie**, **Konfekcjonowanie** oraz **Handel hurtowy** lub **Handel detaliczny** do przechowywania w zakładzie | Administrator organizacji, Personel, Pracownik przyjmujący próbki, Specjalista ds. zapewnienia jakości, Specjalista ds. zgodności |
| Laboratorium analizy żywności (akredytowane lub zakładowe) | **Laboratorium** | Administrator organizacji, Analityk, Kierownik laboratorium, Specjalista ds. zgodności |
| Hurtownik | **Handel hurtowy** | Administrator organizacji, Personel, Specjalista ds. zapewnienia jakości, Specjalista ds. zgodności |
| Detalista | **Handel detaliczny** | Administrator organizacji, Personel, Specjalista ds. zapewnienia jakości, Specjalista ds. zgodności |
| Ministerstwo lub organ kontrolny | **Organ publiczny** | Administrator organizacji (tylko odczyt) |

Stowarzyszenie, które prowadzi własny zakład przetwórczy, ma również funkcję **Przetwarzanie** (i **Konfekcjonowanie**), a wtedy także role zakładu. W małej organizacji Specjalistą ds. zgodności może być administrator organizacji, posiadający drugie członkostwo; zasada czterech oczu nadal obowiązuje.

### Status organizacji {#org-status}

Administrator platformy weryfikuje, zawiesza i przywraca organizacje. Weryfikacja wymaga co najmniej jednego przesłanego dokumentu; zawieszenie wymaga podania powodu. Organizacja weryfikowana po raz pierwszy musi też mieć aktywnego **Specjalistę ds. zgodności**; organizacja zweryfikowana wcześniej bez takiej osoby wyświetla ostrzeżenie z datą, do której musi ją wyznaczyć, i nie jest blokowana. [Table](#tbl-org-status) wymienia statusy.

**Związek centralny.** Jeśli platforma jest tak skonfigurowana, administrator organizacji zweryfikowanego **Związku centralnego** weryfikuje, zawiesza i przywraca stowarzyszenia pszczelarzy i jest wtedy **jedyną** osobą, która to robi: administratorzy platformy nie weryfikują już stowarzyszeń, a stowarzyszenia znikają z ich listy oczekujących.

Table: Status organizacji {#tbl-org-status}

| Status | Znaczenie |
|---|---|
| **Oczekująca** | Nowa organizacja oczekująca na weryfikację. Członkowie mogą odczytywać dane, a administrator organizacji może przesyłać dokumenty. Nikt nie może wprowadzać danych. |
| **Zweryfikowana** | Sprawdzona przez administratora platformy (lub, w przypadku stowarzyszeń, przez związek centralny, jeśli tak skonfigurowano). Członkowie mogą pracować zgodnie ze swoimi rolami. |
| **Zawieszona** | Weryfikacja cofnięta (z podaniem powodu). Do czasu przywrócenia działa jak organizacja oczekująca. |

## Role {#role-list}

[Table](#tbl-roles) to wykaz referencyjny ról. Wszystkie pozostałe tabele w tym podręczniku używają tych samych ról w tej samej kolejności. Tabele praw do danych używają skrótów z drugiej kolumny.

Table: Role {#tbl-roles}

| Rola (na ekranie) | Skrót | Kod | Typowa osoba | Główne zadania |
|---|---|---|---|---|
| **Administrator organizacji** | AO | OrgAdmin | Kierownik organizacji | Przesyła dokumenty, składa wnioski o ostateczne zatwierdzenie, wysyła towar i potwierdza wysyłki, rejestruje zgody pszczelarzy (stowarzyszenia), odczytuje dostawy, próbki i cenę oczekiwaną partii należących do organizacji |
| **Personel** | Pe | Staff | Pracownik produkcji, rozlewni lub sklepu | Wprowadza dane KPK 1–10, składa wnioski o ostateczne zatwierdzenie, wysyła towar i potwierdza wysyłki, rejestruje przechowywanie |
| **Inspektor terenowy** | IT | FieldInspector | Technik terenowy stowarzyszenia | Odczytuje zapisy; wprowadza KPK 0 tylko wtedy, gdy administrator przypisze ten KPK stowarzyszeniu |
| **Pracownik przyjmujący próbki** | PPP | SampleAcceptanceOfficer | Pracownik przyjęcia w zakładzie | Rejestruje dostawy z zaplombowanymi pojemnikami, wysyła pszczelarzowi kod SMS i go wprowadza, pobiera próbkę z każdego pojemnika, wprowadza KPK 0 |
| **Specjalista ds. zapewnienia jakości** | SZJ | QualityApprover | Kierownik ds. jakości, technolog żywności | Zatwierdza rekordy KPK i rekordy przechowywania, potwierdza ostateczne zatwierdzenie |
| **Analityk** | An | Analyst | Analityk laboratoryjny | Przyjmuje próbki (kontrola plomby, etykiety i wieczka), wprowadza wyniki laboratoryjne, przesyła raporty (pierwsza osoba w laboratorium) |
| **Kierownik laboratorium** | KL | LabManager | Kierownik laboratorium | Przyjmuje próbki (kontrola plomby, etykiety i wieczka), zatwierdza lub zwraca raporty laboratoryjne (druga osoba w laboratorium) |
| **Specjalista ds. rejestracji pszczelarzy** | SR | RegistrationOfficer | Pracownik powiatowej dyrekcji rolnictwa | Rejestruje pszczelarzy i ich pasieki, sprawdza numer w rejestrze, wysyła pszczelarzowi kod SMS, poświadcza tożsamość, rejestruje informację o danych i zgody, wydaje pszczelarzowi dane logowania |
| **Specjalista ds. zgodności** | SZ | ComplianceOfficer | Osoba odpowiedzialna za jakość lub ochronę danych w dowolnej organizacji | Przegląda dowody w postaci podpisanych zdjęć, jakość pobierania próbek i zatwierdzenia, prowadzi dane administratora danych, obsługuje żądania osób, których dane dotyczą, i wycofania zgód, odczytuje wpisy audytu swojej organizacji |
| **Administrator platformy** | AP | (rola globalna) | Operator platformy | Administracja ([rozdział 15](#admin)); odczytuje wszystkie dane procesowe, ale żadnych nie wprowadza ani nie zatwierdza; może też rejestrować pszczelarzy |
| **Pszczelarz** (konto w portalu) | Psz | (bez członkostwa) | Pszczelarz | Odczytuje własne pasieki, dostawy, próbki, wyniki laboratoryjne, partie końcowe i strony Historii miodu; wprowadza cenę oczekiwaną swojego miodu; udziela i wycofuje własne zgody ([rozdział 14](#portal)) |

Administrator platformy nie uczestniczy w procesach organizacji biznesowych: może odczytać każdy rekord, ale **nie może wprowadzać ani zatwierdzać danych procesowych** (dostaw, danych KPK, wyników laboratoryjnych, wysyłek, zgód). Jeśli po zatwierdzeniu dane okażą się błędne, administrator składa wniosek o *korektę*, którą zatwierdza drugi administrator (zob. [Korekty](#admin-corrections)). Administratorzy prowadzą natomiast dane podstawowe: pszczelarzy, pasieki, partie, partie końcowe i ich skład (zob. [rozdział 15](#admin)).

## Prawa do danych (CRUD) {#crud}

[Table](#tbl-crud) pokazuje domyślne prawa każdej roli do poszczególnych rodzajów rekordów, w modelu CRUD: **C** tworzenie (create), **R** odczyt (read), **U** aktualizacja (update), **D** usuwanie (delete) oraz **A** zatwierdzanie (approve; osobne prawo). „–” oznacza brak prawa. Psz to konto pszczelarza w portalu, Publ to odwiedzający stronę publiczną bez konta. Prawa członka dotyczą wyłącznie rekordów pracy jego własnej organizacji (jej partii i partii końcowych, posiadanych przez nią partii końcowych, pszczelarzy jej stowarzyszenia, rejestracji jej dyrekcji) i, z wyjątkiem odczytu, tylko wtedy, gdy organizacja jest zweryfikowana. Prawa wynikają z kodu i ustawień domyślnych; [Table](#tbl-admin-changeable) wymienia, które z nich administrator może zmienić.

Table: Domyślne prawa do danych według ról (CRUD + zatwierdzanie) {#tbl-crud}

| Rekord | AO | Pe | IT | PPP | SZJ | An | KL | SR | SZ | AP | Psz | Publ |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Pszczelarz | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (siebie) | R ³ |
| Pasieka | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (własne) | R ³ |
| Rejestracja pszczelarza | – | – | – | – | – | – | – | C R ¹⁸ | A ¹⁹ | C R | – | – |
| Dane logowania | – | – | – | – | – | – | – | C ²⁰ | – | C ²⁰ | – | – |
| Partia surowego miodu | R | R | R | C R ⁴ | R | – | – | – | R | C R U D ⁵ | R (własne) | R ³ |
| Potwierdzenie dostawy (kod SMS lub podpisane zdjęcie) | R | – | – | C R | – | – | – | – | R A ¹⁹ | R | A ²¹ | – |
| Pojemnik i jego plomby | R | R ²⁹ | R ²⁹ | C R | R ²⁹ | R | R | – | R | R | R (własne) | – |
| Próbka | R | – | – | C R | – | R A ⁶ | R A ⁶ | – | R | R | R (własne) | – |
| Raport laboratoryjny i wyniki | R | R | R | R | R | C R U D ⁷ | R A ⁷ | – | R | R | R (zatwierdzone, własne) | R ⁸ |
| Rekord KPK (dane, „Nie dotyczy”) | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | – | R | R ¹⁰ | – | R ³ |
| Partia końcowa | R, wniosek o ostateczne zatwierdzenie | R, wniosek o ostateczne zatwierdzenie | R | R | R, potwierdzenie ostatecznego zatwierdzenia (A) | R | R | – | R | C R U D ¹¹ | R ²² | R ³ |
| Wysyłka | C R A ¹² | C R A ¹² | R ²⁹ | R ²⁹ | R | R ²⁹ | R ²⁹ | – | R | R | – | – |
| Rekord przechowywania u odbiorcy (KPK 10) | R | C R U D | R | R | R A | R | R | – | R | R | – | – |
| Cena oczekiwana partii | R ²³ | – | – | – | – | – | – | – | R ²³ | R | C R ²³ | – ²⁴ |
| Zgoda pszczelarza | C R U ¹³ | R | R | R | R | R | R | C R ²⁵ | R U ²⁵ | R | R U ²⁵ | R ¹⁴ |
| Informacja o danych (treść, wersja) | R | R | R | R | R | R | R | R | R | C R ²⁶ | R | R |
| Potwierdzenie zapoznania się z informacją | R (własne) | R (własne) | R (własne) | R (własne) | R (własne) | R (własne) | R (własne) | R (własne) | R (własne) | R | R (własne) | – |
| Dane administratora danych | R | R | R | R | R | R | R | R | C R U | C R U | R | R (w informacji) |
| Przegląd jakości pobierania próbek | – | – | – | – | – | – | – | – | C R ³⁰ | – | – | – |
| Żądanie osoby, której dane dotyczą | – | – | – | – | – | – | – | – | C R U ³⁰ | – | – | – |
| Wiadomość SMS | – | – | – | – | – | – | – | – | – | R ²⁷ | – | – |
| Organizacja (dane, dokumenty) | R; dokumenty C | R | R | R | R | R | R | R | R; dokumenty C | C R U, status A ¹⁵ | – | – |
| Członkostwo | R | R | R | R | R | R | R | R | R | C R U ¹⁶ | – | – |
| Wpis w dzienniku audytu | – | – | – | – | – | – | – | – | R ²⁸ | R ¹⁷ | – | – |

Uwagi:

1. Odczyt tylko poprzez rekordy, nad którymi pracuje organizacja (strony KPK, raport z wycofania). Późniejszy posiadacz partii końcowej odczytuje dane bez nazw pszczelarzy i pasiek. Członkowie stowarzyszenia odczytują listę swoich pszczelarzy członkowskich.
2. Pszczelarzy i pasiek nigdy się nie usuwa, tylko dezaktywuje. Zdjęcia pasiek można dodawać, zmieniać i usuwać.
3. Na stronach publicznych partii końcowych zatwierdzonych ostatecznie (Historia miodu) i na opublikowanych stronach pszczelarzy: imię i nazwisko tylko za zgodą (w przeciwnym razie neutralny kod), prowincja i zatwierdzone dane; nigdy współrzędne ani dane kontaktowe (zob. [Co może odczytać publiczność](#trust-public)).
4. Partia powstaje przez zarejestrowanie dostawy w zweryfikowanej organizacji z funkcją **Przetwarzanie**.
5. Administratorzy prowadzą dane podstawowe partii (na przykład organizację będącą właścicielem i przypisanie KPK). Faktów dostawy partii zarejestrowanej z pojemnikami (data, pojemniki, wagi, numery plomb) nie można edytować, również administratorom; zmienia je wyłącznie korekta dostawy przeprowadzana przez dwóch administratorów (zob. [Korekty](#admin-corrections)). Gdy partia trafi do partii końcowej zatwierdzonej ostatecznie, niczego nie można już zmienić; partii, której dostawy, próbki lub dane KPK są w użyciu, nie można usunąć.
6. **A** = przyjęcie próbki po sprawdzeniu plomby, etykiety i wieczka albo jej odrzucenie.
7. Analityk tworzy, zapisuje, zmienia i przesyła raport, dopóki jest on wersją roboczą; kierownik laboratorium go zatwierdza lub zwraca. Zatwierdzonego raportu nie można zmienić.
8. Strona **Zweryfikuj raport laboratoryjny**, według kodu raportu; tylko zatwierdzone raporty.
9. Wprowadzanie danych (C), ich zmiana lub usuwanie (U, D) oraz oznaczanie „Nie dotyczy”, dopóki rekord nie zostanie zatwierdzony lub zablokowany. Domyślne role wprowadzające: Pracownik przyjmujący próbki dla KPK 0–1, Personel dla KPK 1–10. Inspektor terenowy wprowadza KPK 0 tylko wtedy, gdy administrator przypisze KPK 0 stowarzyszeniu.
10. Administratorzy zmieniają zatwierdzone lub zablokowane dane wyłącznie w drodze korekty, o którą wnioskuje jeden administrator, a zatwierdza drugi.
11. Utworzenie partii końcowej, ustalenie jej składu z partii, przypisanie KPK i ustalenie właściciela. Partię końcową można usunąć tylko przed jej ostatecznym zatwierdzeniem. Administratorzy nie mogą zatwierdzać ostatecznie.
12. **C** = wysłanie partii końcowej zatwierdzonej ostatecznie, którą posiada Twoja organizacja; **A** = potwierdzenie lub odrzucenie wysyłki, którą otrzymuje Twoja organizacja.
13. Rejestracja (C) i wycofanie (U) zgody na publiczną stronę pszczelarza; tylko stowarzyszenia.
14. Opublikowana strona pszczelarza.
15. Weryfikacja, żądanie dokumentów, zawieszenie i przywrócenie (zob. [Status organizacji](#org-status)). Organizacji nigdy się nie usuwa.
16. Dodanie członkostwa i jego cofnięcie (U); członkostw nigdy się nie usuwa.
17. Do dziennika audytu można tylko dopisywać; nikt nie może zmienić ani usunąć wpisu.
18. Specjalista ds. rejestracji pszczelarzy rejestruje pszczelarzy i ich pasieki dla swojej dyrekcji i odczytuje tylko jej rejestracje. Poświadczone fakty (tożsamość, numer w rejestrze, telefon) zmieniają się wyłącznie przez nową rejestrację tego samego pszczelarza (ponowną weryfikację), ponownie potwierdzoną.
19. **A** = przyjęcie lub odrzucenie dowodu zarejestrowanego za pomocą podpisanego zdjęcia zamiast kodu SMS: zdjęcia z rejestracji lub zdjęcia z dostawy. Decyduje Specjalista ds. zgodności organizacji, która zarejestrowała dowód, nigdy osoba, która go zarejestrowała. W przypadku rejestracji samej platformy decyduje administrator platformy.
20. Dane logowania pszczelarza wysyła SMS-em specjalista ds. rejestracji pszczelarzy (gdy portal pszczelarza jest otwarty); dane logowania członków wysyłają administratorzy platformy. Po wysłaniu nikt nie widzi hasła.
21. **A** pszczelarza to kod SMS, który odczytuje pracownikowi przyjmującemu próbki; potwierdza on dostawę wraz z pojemnikami, wagami i plombami.
22. Partie końcowe zawierające miód danego pszczelarza: nazwa, udział pszczelarza, status i odnośnik do Historii miodu. Nigdy nazwiska ani ceny innych pszczelarzy.
23. Pszczelarz wprowadza cenę oczekiwaną dla każdej dostarczonej partii i może ją zmieniać (nowy wpis; historia zostaje), dopóki partia nie trafi do partii końcowej zatwierdzonej ostatecznie. Odczytują ją AO i SZ właściciela partii oraz administratorzy platformy; stowarzyszenie tylko wtedy, gdy pszczelarz się na to zgodził. Personel, laboratoria i późniejsi posiadacze nigdy.
24. Strony publiczne i raport z wycofania nie pokazują ceny.
25. Specjalista ds. rejestracji pszczelarzy rejestruje zgody udzielone przy rejestracji; pszczelarz udziela lub wycofuje zgodę dobrowolną w portalu; Specjalista ds. zgodności rejestruje wycofanie zgody otrzymane na piśmie lub telefonicznie. Wycofanie działa natychmiast.
26. Treści informacji mają wersje i nigdy się ich nie edytuje: opublikowanie nowej wersji wycofuje starą.
27. Tylko skrzynka nadawcza dostawcy testowego (rozwój i demonstracje); prawdziwy dostawca nie przechowuje kodów ani haseł.
28. Wpisy audytu własnej organizacji Specjalisty ds. zgodności.
29. Tylko w raporcie z wycofania: strona **Wysyłki i zapasy** wymaga prawa odczytu wysyłek.
30. Własna sekcja Specjalisty ds. zgodności, **Zgodność** (zob. [rozdział 13](#role-co)).

**Kto pobiera pliki dowodowe.** Podpisane formularze dostaw, zdjęcia z rejestracji, dokumenty zgód i dokumenty organizacji pobierają wyłącznie rola, która je rejestruje (pracownik przyjmujący próbki, specjalista ds. rejestracji pszczelarzy lub, w przypadku zgód, administrator organizacji stowarzyszenia), Specjalista ds. zgodności i administrator organizacji oraz administratorzy platformy. Inna rola w organizacji otrzymuje stronę „Nie możesz otworzyć tej strony” (403); inna organizacja otrzymuje stronę „Tej strony tu nie ma” (404). Każde pobranie jest zapisywane w dzienniku audytu.

**Co można usunąć.** Członkowie usuwają wyłącznie własne dane przed ich zatwierdzeniem (dane KPK, robocze wyniki laboratoryjne, dane przechowywania). Administratorzy mogą usuwać partie i partie końcowe przed ostatecznym zatwierdzeniem, zdjęcia pasiek, dane referencyjne i konta użytkowników (nigdy własnego). Pszczelarzy, pasiek, organizacji, dokumentów, członkostw, zgód, rejestracji, pojemników, zdarzeń plomb, próbek, raportów laboratoryjnych, wysyłek, wpisów cen, wiadomości SMS, informacji, potwierdzeń zapoznania się i wpisów w dzienniku audytu nigdy się nie usuwa.

## Macierz uprawnień {#permission-matrix}

[Table](#tbl-permissions) pokazuje domyślne uprawnienia według **czynności na platformie** (funkcji, takiej jak „rejestracja dostawy”) w obecnej konfiguracji. Uzupełnia ona prawa do danych z [Table](#tbl-crud) i używa tych samych ról w tej samej kolejności. Legenda: **R** odczyt, **C** tworzenie lub wprowadzanie, **A** zatwierdzanie (lub przyjęcie albo akceptacja dowodu), **F** ostateczne zatwierdzenie (wniosek lub potwierdzenie), **–** brak uprawnienia. Kolumna „Wymagana funkcja” wskazuje, jakiej funkcji potrzebuje organizacja.

Table: Domyślne uprawnienia według czynności na platformie i roli {#tbl-permissions}

| Czynność na platformie | Wymagana funkcja | AO | Pe | IT | PPP | SZJ | An | KL | SR | SZ | AP |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Rejestracja pszczelarza i pasiek | Rejestracja pszczelarzy | – | – | – | – | – | – | – | C R | – | C R |
| Wysłanie pszczelarzowi danych logowania SMS-em | Rejestracja pszczelarzy | – | – | – | – | – | – | – | C | – | C |
| Rejestracja dostawy z pojemnikami, wysłanie kodu SMS | Przetwarzanie | R | – | – | C R | – | – | – | – | R | R |
| Pobranie próbek (z każdego pojemnika) | Przetwarzanie (właściciel partii) | R | – | – | C R | – | R | R | – | R | R |
| Akceptacja lub odrzucenie podpisanego zdjęcia (rejestracja, dostawa) | własna organizacja | – | – | – | – | – | – | – | – | A | A ¹ |
| Przyjęcie próbki (plomba, etykieta, wieczko) | Laboratorium | – | – | – | – | – | A | A | – | R | R |
| Wprowadzenie wyników laboratoryjnych, przesłanie raportu | Laboratorium | R | R | R | R | R | C R | R | – | R | R |
| Zatwierdzenie lub zwrot raportu laboratoryjnego | Laboratorium | R | R | R | R | R | R | A R | – | R | R |
| KPK 0 – kontrola przy przyjęciu (partia) | Przetwarzanie (właściciel partii) | R | R | R | C R | A R | R | R | – | R | R |
| KPK 1 – dane laboratoryjne (partia) | przepisywane z zatwierdzonego raportu laboratoryjnego | R | R | R | R | R | R | R | – | R | R |
| KPK 2–3 (partia) | Przetwarzanie (właściciel partii) | R | C R | R | R | A R | R | R | – | R | R |
| Utworzenie partii końcowej, ustalenie jej składu, przypisanie KPK | – | – | – | – | – | – | – | – | – | – | C |
| KPK 4–6 (partia końcowa) | Przetwarzanie (właściciel partii końcowej) | R | C R | R | R | A R | R | R | – | R | R |
| KPK 7–9 (partia końcowa) | Konfekcjonowanie (właściciel partii końcowej) | R | C R | R | R | A R | R | R | – | R | R |
| KPK 10 – przechowywanie w zakładzie (partia końcowa) | Handel hurtowy, Handel detaliczny lub Konfekcjonowanie (właściciel partii końcowej) | R | C R | R | R | A R | R | R | – | R | R |
| Wniosek o ostateczne zatwierdzenie | Przetwarzanie lub Konfekcjonowanie (właściciel), zweryfikowany | F | F | – | – | – | – | – | – | – | – |
| Potwierdzenie ostatecznego zatwierdzenia (zamknięcie) | Przetwarzanie lub Konfekcjonowanie (właściciel), zweryfikowany | – | – | – | – | F | – | – | – | – | – |
| Utworzenie wysyłki | Przetwarzanie, Konfekcjonowanie, Handel hurtowy lub Handel detaliczny (posiadacz), zweryfikowany | C R | C R | – | – | R | – | – | – | R | R |
| Potwierdzenie lub odrzucenie otrzymanej wysyłki | Przetwarzanie, Konfekcjonowanie, Handel hurtowy lub Handel detaliczny (odbiorca), zweryfikowany | A R | A R | – | – | R | – | – | – | R | R |
| Rekord przechowywania KPK 10 u odbiorcy | Handel hurtowy, Handel detaliczny lub Konfekcjonowanie | R | C R | R | R | A R | R | R | – | R | R |
| Raport z wycofania (własny zakres) | – | R | R | R | R | R | R | R | – | R | R (wszystko) |
| Odczyt ceny oczekiwanej własnych partii | Przetwarzanie (właściciel partii) | R | – | – | – | – | – | – | – | R | R |
| Rejestracja lub wycofanie zgody pszczelarza (strona pszczelarza) | Stowarzyszenie | C R | – | – | – | – | – | – | – | – | R |
| Zgodność: przeglądy, żądania osób, których dane dotyczą, kontrole zatwierdzeń | własna organizacja | – | – | – | – | – | – | – | – | C R | – |
| Prowadzenie danych administratora danych | własna organizacja | R | – | – | – | – | – | – | – | C R | C R |
| Odczyt wpisów audytu organizacji | własna organizacja | – | – | – | – | – | – | – | – | R | R (wszystko) |
| Przesyłanie dokumentów organizacji | własna organizacja | C | – | – | – | – | – | – | – | C | C |
| Publikowanie treści informacji | – | – | – | – | – | – | – | – | – | – | C |
| Weryfikacja organizacji, zarządzanie członkostwami i funkcjami | – | – | – | – | – | – | – | – | – | – | C A ² |
| Weryfikacja numerów pszczelarzy w rejestrze | – | – | – | – | – | – | – | – | – | – | A |
| Zmiana uprawnień, KPK, limitów, tłumaczeń | – | – | – | – | – | – | – | – | – | – | C |
| Wniosek o korektę lub decyzja w sprawie korekty (dane KPK, fakty dostawy) | – | – | – | – | – | – | – | – | – | – | C A (dwóch różnych administratorów) |
| Dziennik audytu, raport przechowywania danych | – | – | – | – | – | – | – | – | – | – | R |

1. W przypadku rejestracji zarejestrowanych przez administratorów platformy.
2. Stowarzyszenia weryfikuje zamiast tego związek centralny, jeśli platforma jest tak skonfigurowana.

Uwagi:

- **Odczyt KPK (R)**: każda rola może otwierać KPK partii i partii końcowych, w których uczestniczy jej organizacja: jako właściciel lub organizacja odpowiedzialna, jako organizacja, która pobrała próbkę (w przypadku partii), jako późniejszy posiadacz partii końcowej lub jako zweryfikowany organ publiczny. Posiadacz odczytuje KPK partii końcowej i jej partii **tylko do odczytu**. Każda inna organizacja otrzymuje stronę „Nie możesz otworzyć tej strony” (403).
- **KPK 1** jest wypełniany na podstawie zatwierdzonego raportu laboratoryjnego. Skonfigurowane dla niego role wprowadzające (Pracownik przyjmujący próbki, Personel) są używane tylko dla tych parametrów KPK 1, których raport laboratoryjny nie obejmuje.
- **Strony sprawdzają prawo odczytu, nie tylko menu.** Rola bez prawa odczytu danej strony (na przykład **Wysyłki i zapasy** lub **Strony pszczelarzy**) zostaje odesłana na stronę **Moja praca** z komunikatem, który wymienia role uprawnione do jej otwarcia.
- Administrator może przypisać **KPK 0** stowarzyszeniu pszczelarza zamiast zakładowi (**Odpowiedzialności za KPK** → KPK 0 → **Stowarzyszenie pszczelarza danej partii**, funkcja Stowarzyszenie, rola Inspektor terenowy). Nie jest to ustawienie domyślne.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Role i uprawnienia**: domyślne uprawnienia roli Personel.

### Co może zmienić administrator {#admin-changeable}

[Table](#tbl-admin-changeable) wymienia wszystkie ustawienia, które administrator platformy może zmienić w panelu. Wszystko inne jest stałe: lista ról, lista funkcji i [Zasady podstawowe](#fixed-rules). Niektóre przełączniki ustawia się natomiast w konfiguracji serwera (zob. [Ustawienia w konfiguracji serwera](#admin-settings)).

Table: Co może zmienić administrator platformy {#tbl-admin-changeable}

| Ustawienie | Gdzie | Co dokładnie można zmienić |
|---|---|---|
| Uprawnienia ról | **Role i uprawnienia** → **Uprawnienia ról** | Dla każdej roli: **Wprowadzanie**, **Zatwierdzenie** i **Odczyt** dla każdego KPK; **Wprowadzanie** i **Odczyt** dla dostaw (przyjęcie); **Wprowadzanie**, **Zatwierdzenie** (przyjęcie) i **Odczyt** dla próbek; **Wprowadzanie**, **Zatwierdzenie** i **Odczyt** dla wyników laboratoryjnych; **Wprowadzanie** (wysłanie), **Zatwierdzenie** (potwierdzenie odbioru) i **Odczyt** dla wysyłek; **Wprowadzanie** i **Odczyt** dla stron pszczelarzy; **Odczyt** dla raportu z wycofania; **Wprowadzanie** (wniosek) i **Zatwierdzenie** (potwierdzenie) dla ostatecznego zatwierdzenia; **Wprowadzanie** i **Odczyt** dla rejestracji pszczelarzy; **Zatwierdzenie** dla dowodów; **Odczyt** dla cen i dla wpisów audytu organizacji. Każde uprawnienie zachowuje co najmniej jedną rolę. |
| Odpowiedzialności za KPK | **Odpowiedzialności za KPK** | Dla każdego KPK: organizacja odpowiedzialna (właściciel partii lub partii końcowej albo, w przypadku partii, stowarzyszenie pszczelarza), wymagane funkcje, role wprowadzające, role zatwierdzające oraz to, czy zatwierdzenie jest wymagane. |
| Funkcje | Strona organizacji; **Funkcje organizacji** | Dodawanie lub usuwanie funkcji organizacji (co najmniej jedna musi pozostać); numer akredytacji laboratorium i jej ważność. |
| Wyjątek od zasady czterech oczu | Strona organizacji | Dla każdej organizacji: pozwala jej członkom zatwierdzać własne rekordy KPK i rekordy przechowywania oraz potwierdzać własny wniosek o ostateczne zatwierdzenie; zawsze oznaczane w dzienniku audytu. Nigdy dla wyników laboratoryjnych ani dowodów. |
| Status organizacji | Strona organizacji | Weryfikacja, żądanie dokumentów, zawieszenie, przywrócenie. |
| Dane administratora danych | Strona organizacji | Nazwa prawna, dane kontaktowe i adres organizacji jako administratora danych (prowadzi je także jej Specjalista ds. zgodności). |
| Członkostwa i użytkownicy | Strona organizacji; **Członkowie**; **Zarządzanie Użytkownikami** | Dodawanie i cofanie członkostw; tworzenie użytkowników z danymi logowania wysyłanymi SMS-em lub z hasłem początkowym; wysyłanie nowych danych logowania; odblokowywanie i usuwanie kont. |
| Informacje i treści zgód | **Informacje i treści zgód** | Publikowanie nowej wersji informacji lub treści zgody, z oznaczeniem projektu do czasu zatwierdzenia przez zespół ochrony danych. |
| Punkty kontrolne, parametry, limity | **Krytyczne Punkty Kontrolne** | Parametry, typy danych, pytania wymagane, limity, „Limity zweryfikowane”. |
| Dane podstawowe i referencyjne | Menu administracyjne | Pszczelarze, pasieki, numery w rejestrze, partie, partie końcowe, rodzaje miodu, okresy, tłumaczenia. |

## Zasady podstawowe {#fixed-rules}

Poniższe zasady są wbudowane w platformę; żadne ustawienie ich nie wyłącza:

- **Dane wprowadzają tylko zweryfikowane organizacje.** Organizacja weryfikowana po raz pierwszy potrzebuje Specjalisty ds. zgodności.
- **Zasada czterech oczu dla rekordów KPK**: nikt nie zatwierdza rekordu KPK, w którym sam zarejestrował dane lub który sam oznaczył jako „Nie dotyczy”. Wyjątek: administrator platformy może przyznać organizacji **Wyjątek od zasady czterech oczu**; jej członkowie mogą wtedy zatwierdzać własne rekordy KPK, a każde takie zatwierdzenie jest oznaczane w dzienniku audytu.
- **Zasada czterech oczu dla wyników laboratoryjnych, zawsze**: osoba, która wprowadziła wyniki, nie może ich zatwierdzić. Nie ma od tego wyjątku.
- **Zasada czterech oczu dla dowodów, zawsze**: podpisane zdjęcie, które zastępuje kod SMS (przy rejestracji lub dostawie), akceptuje lub odrzuca Specjalista ds. zgodności organizacji, nigdy osoba, która je zarejestrowała.
- **Zasada czterech oczu przy ostatecznym zatwierdzeniu i rekordach przechowywania**: osoba, która złożyła wniosek o ostateczne zatwierdzenie, nie może go potwierdzić, a osoba, która zarejestrowała rekord przechowywania, nie może go zatwierdzić (chyba że organizacja ma wyjątek).
- **Kod potwierdza dokładnie to, do czego go wysłano**: kod SMS pszczelarza wiąże rejestrację lub dostawę (pojemniki, wagi, rodzaj miodu, próbki, każdy numer plomby); każda zmiana po wysłaniu unieważnia kod.
- **Fakty dostawy się nie zmieniają**: do pojemników i zdarzeń plomb można tylko dopisywać; fakty potwierdzonej dostawy zmienia wyłącznie korekta przeprowadzana przez dwóch administratorów, a pszczelarz jest o niej informowany SMS-em.
- **Próbka z każdego pojemnika**: każdy pojemnik otrzymuje własny zaplombowany słoik z próbką i próbkę kontrolną z wieczkiem gwarancyjnym; laboratorium sprawdza plombę, etykietę i wieczko i odrzuca słoik, jeśli którakolwiek kontrola nie wypadnie pomyślnie.
- **Każda zmiana usuwa zatwierdzenie**: zmiana danych kasuje istniejące zatwierdzenie; dane poza limitem trzeba przy zatwierdzaniu wyraźnie potwierdzić.
- **Wyniki laboratoryjne idą za próbką**: wyniki wprowadza się tylko dla przyjętej próbki z pozytywnymi wynikami kontroli i dopiero po zatwierdzeniu KPK 0 partii; zatwierdzonego raportu nie można zmienić.
- **Administratorzy platformy mają dostęp tylko do odczytu** do danych procesowych i korygują je wyłącznie w procesie korekty z udziałem dwóch administratorów.
- **Zatwierdzone ostatecznie oznacza zablokowane**: partia końcowa zostaje zatwierdzona ostatecznie dopiero wtedy, gdy każdy KPK partii końcowej i jej partii jest gotowy; partii końcowej zatwierdzonej ostatecznie ani żadnej z jej partii nie można już zmienić, chyba że w drodze korekty. Nie ma „cofnięcia ostatecznego zatwierdzenia”.
- **Wysyłki**: tylko partie końcowe zatwierdzone ostatecznie, tylko do innej zweryfikowanej organizacji, najwyżej posiadana ilość.
- **Organizacje konfigurują wyłącznie administratorzy**: funkcje, członkostwa i wyjątek.
- **Każde uprawnienie zachowuje co najmniej jedną rolę.**
- **Do dziennika audytu można tylko dopisywać**; niczego w nim nie można zmienić ani usunąć, a każda zmiana na stronach uprawnień również jest rejestrowana.
- **Ceny należą do pszczelarza**: cenę oczekiwaną wprowadza wyłącznie pszczelarz; nigdy nie jest ona pokazywana na stronach publicznych.
- **Konto pszczelarza widzi tylko dane własnego pszczelarza** i może otwierać wyłącznie portal pszczelarza.
- **Publiczne podanie nazwiska wymaga zgody**: strony publiczne podają imię i nazwisko pszczelarza tylko przy aktywnej zgodzie na podanie nazwiska; w przeciwnym razie pokazują neutralny kod.
- **Strony pszczelarzy** są publiczne tylko przy aktywnej zgodzie, zweryfikowanym numerze w rejestrze i zweryfikowanym stowarzyszeniu.

## Kilka członkostw {#memberships}

**Członkostwo** to jedno konto użytkownika × jedna organizacja × jedna rola. Jedna osoba może mieć kilka członkostw: w różnych organizacjach albo kilka ról w tej samej organizacji (na przykład **Specjalista ds. zapewnienia jakości** i **Administrator organizacji** w stowarzyszeniu albo **Administrator organizacji** i **Specjalista ds. zgodności** w małej organizacji). W danej chwili zawsze pracujesz w ramach jednego członkostwa, a uprawnienia się nie sumują: posiadanie dwóch ról nigdy nie pozwala zatwierdzić danych wprowadzonych przez Ciebie samego.

Aby je przełączyć:

1. Kliknij **wskaźnik organizacja · rola** na górnym pasku (zob. [Górny pasek i Twoja rola](#top-bar)). Lista z nagłówkiem **Twoje role** pokazuje wszystkie aktywne członkostwa; bieżące jest zaznaczone.
2. Kliknij inne członkostwo. Platforma otworzy stronę **Moja organizacja** w nowym członkostwie.

Menu, zadania na stronie **Moja praca**, rekordy, które możesz odczytać, oraz dozwolone czynności zależą od aktywnego członkostwa. Wybór obowiązuje do zamknięcia przeglądarki; potem zaczynasz ponownie w pierwszym członkostwie (alfabetycznie według nazwy organizacji). Jeśli masz co najmniej dwa członkostwa, strona **Moja organizacja** zawiera również listę **Działaj jako** z przyciskiem **Przełącz**.

Konto pszczelarza w portalu nie ma członkostwa ani wskaźnika roli; otwiera wyłącznie portal pszczelarza.
