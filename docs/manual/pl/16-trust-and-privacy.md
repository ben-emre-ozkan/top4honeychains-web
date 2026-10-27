# 16. Zaufanie i prywatność {#trust}

## Dziennik audytu {#trust-audit}

Każda zmiana na platformie (każdy zapis, zatwierdzenie, ostateczne zatwierdzenie, wysyłka, logowanie, zmiana uprawnień, pobranie dowodu i uruchomienie przechowywania danych) jest zapisywana w dzienniku audytu (**Dziennik audytu**). Administratorzy platformy otwierają go w menu **Zarządzanie Użytkownikami** → **Dziennik audytu**; Specjalista ds. zgodności odczytuje wpisy własnej organizacji na stronie **Zgodność**.

Każdy wiersz pokazuje:

- **Kiedy**: czas lokalny; po najechaniu kursorem widoczny jest czas UTC;
- **Kto**: użytkownika i organizację;
- **Akcja**: na przykład Utwórz, Zaktualizuj, Zatwierdzenie, Ostateczne zatwierdzenie, Zweryfikuj, Logowanie, Pobranie, z oznaczeniami takimi jak „Zatwierdzone samodzielnie”;
- **Rekord**: którego rekordu dotyczy wpis, z odnośnikiem, jeśli istnieje odpowiednia strona administracyjna;
- **Zmiany**: każde zmienione pole w postaci **stara → nowa**, z nazwą pola w Twoim języku; identyfikatory użytkowników i organizacji są wyświetlane jako nazwy;
- **Skrót**: początek odcisku wiersza.

**Pokaż surowe dane** wyświetla zapisane dane dokładnie w takiej postaci, w jakiej zostały zapisane (stare i nowe wartości).

Filtruj według typu rekordu, akcji, nazwy użytkownika lub oznaczenia (na przykład `SelfApproved`) i kliknij **Filtruj**.

<!-- SHOT: audit-log -->
Figure: Dziennik audytu ze zmianami stara → nowa wartość i surowymi danymi.

### Tylko dopisywanie i łańcuch skrótów {#trust-hash-chain}

Do dziennika audytu można tylko dopisywać. Baza danych odrzuca każdą próbę zmiany lub usunięcia wiersza. Każdy wiersz zawiera odcisk SHA-256 własnej treści oraz poprzedniego wiersza, dzięki czemu wiersze tworzą łańcuch.

**Zweryfikuj łańcuch skrótów** przelicza cały łańcuch. Komunikat „Łańcuch skrótów: Wszystkie wiersze (…) zostały poprawnie zweryfikowane.” oznacza, że nic nie zostało zmienione. W przeciwnym razie komunikat wskazuje pierwszy wiersz, który został zmieniony lub który nie wskazuje na poprzedni wiersz.

## Rekordy, które się nie zmieniają {#trust-locked}

- Zatwierdzony rekord KPK zachowuje zatwierdzenie tylko dopóty, dopóki jego wartości się nie zmienią.
- Partia końcowa zatwierdzona ostatecznie i jej partie są zablokowane.
- Zatwierdzone wyniki laboratoryjne stają się wartościami KPK 1 dostępnymi tylko do odczytu.
- Fakty potwierdzonej dostawy są zablokowane; do pojemników, zdarzeń plomb, rejestracji, wpisów cen, wiadomości SMS, informacji o danych i ich akceptacji można tylko dopisywać.
- Korekty wymagają udziału dwóch administratorów i są rejestrowane wraz z powodem.

## Kody potwierdzające treść {#trust-codes}

Pszczelarz potwierdza rejestrację i dostawę 6-cyfrowym kodem otrzymanym SMS-em; w ten sam sposób role podpisujące potwierdzają informację o danych. Każdy kod jest powiązany z odciskiem dokładnie tej treści, dla której został wysłany: każda późniejsza zmiana go unieważnia. Kody są ważne przez 5 minut i można je wpisać 3 razy; przechowywany jest tylko skrót kodu. Gdy SMS jest niemożliwy, kod zastępuje podpisane zdjęcie, które akceptuje lub odrzuca druga osoba (Specjalista ds. zgodności).

## Pliki i odciski {#trust-files}

Formularze dostawy, zdjęcia z rejestracji, pliki PDF raportów laboratoryjnych, dokumenty organizacji, dokumenty potwierdzające wpis do rejestru oraz dokumenty zgody są przechowywane poza publicznym folderem WWW, pod losową nazwą, wraz z ich **odciskiem SHA-256**. Przesyłane pliki są sprawdzane na podstawie zawartości, a nie tylko nazwy: PDF do 10 MB lub JPG, PNG albo WEBP do 8 MB. Zanim zatwierdzony raport laboratoryjny zostanie wykorzystany, platforma sprawdza, czy zapisany plik PDF nadal odpowiada swojemu odciskowi. Każdy może porównać plik PDF raportu z jego odciskiem na publicznej stronie weryfikacji bez przesyłania pliku.

**Kto pobiera dowody.** Pliki dowodowe pobierają wyłącznie rola, która je rejestruje, Specjalista ds. zgodności i administrator organizacji oraz administratorzy platformy; inna rola w organizacji otrzymuje błąd 403, inna organizacja błąd 404, a każde pobranie jest zapisywane w dzienniku audytu (zob. [Prawa do danych](#crud)).

## Informacje, zgody i administrator danych {#trust-notices}

**Informacje o danych.** Platforma i każda organizacja publikują informację o danych: jak przetwarzane są dane osobowe, w jakim celu, jak długo są przechowywane i kto jest **administratorem danych**. Użytkownicy potwierdzają informacje platformy i każdej organizacji, dla której pracują (zob. [Informacja o danych](#notice-accept)); pszczelarze czytają swoją informację przy rejestracji i potwierdzają ją kodem SMS. Informacje mają wersje: nowa wersja ponownie prosi wszystkich o potwierdzenie, a dawne akceptacje pozostają jako dowód. Teksty jeszcze niezatwierdzone przez zespół ochrony danych mają oznaczenie **PROJEKT**. Publiczna strona **Prywatność i zgoda** (`/privacy`) pokazuje aktualne informacje.

**Administrator danych.** Każda organizacja jest administratorem danych, które rejestruje; jej nazwa prawna, dane kontaktowe i adres są podane w jej informacji i prowadzone na stronie organizacji przez jej Specjalistę ds. zgodności lub administratora platformy. Pszczelarz widzi w portalu, które organizacje przechowują jego dane (**Kto przechowuje Twoje dane**).

**Zgody.** Wszystko, co wykracza poza potrzeby pracy, jest dobrowolne i jest przedmiotem osobnego pytania:

- imię i nazwisko pszczelarza na stronach publicznych;
- informacyjne SMS-y o dostawach i wynikach;
- dostęp stowarzyszenia do cen oczekiwanych przez pszczelarza;
- publiczna strona pszczelarza, rejestrowana przez stowarzyszenie.

Zgody udziela się przy rejestracji (potwierdzonej kodem SMS), w portalu albo, w przypadku strony pszczelarza, za pośrednictwem stowarzyszenia z podpisanym dokumentem. Wycofanie działa natychmiast: w portalu albo zarejestrowane przez Specjalistę ds. zgodności, gdy dotrze listownie lub telefonicznie.

**Żądania dotyczące danych osobowych.** Na żądania dostępu, sprostowania, usunięcia i sprzeciwu odpowiada Specjalista ds. zgodności organizacji w ciągu 30 dni (zob. [Żądania osób, których dane dotyczą](#co-requests)).

## Co może odczytać publiczność {#trust-public}

Strony publiczne (Historia miodu, strona pszczelarza, weryfikacja raportu) pokazują wyłącznie zatwierdzone, zweryfikowane informacje:

- **Miejsce**: tylko **prowincja** (strona pszczelarza podaje także powiat). Prowincja jest ustalana na serwerze; współrzędne pasiek nigdy nie opuszczają serwera. Mapa koloruje prowincje, nigdy nie pokazuje punktów; obecnie obejmuje Turcję, a mapy pozostałych krajów partnerskich są planowane.
- **Osoby**: Historia miodu podaje imię i nazwisko pszczelarza **tylko przy aktywnej zgodzie na podanie nazwiska**; w przeciwnym razie pokazuje neutralny kod strony pszczelarza. Pszczelarze zarejestrowani przed wprowadzeniem procedury zgód są pokazywani pod swoim kodem do czasu udzielenia zgody (lub, jeśli platforma jest tak skonfigurowana, zachowują imię i nazwisko do czasu ponownej rejestracji). Nigdy numer telefonu, data urodzenia ani zdjęcie osoby i żadnych nazw użytkowników platformy. Nazwy pasiek wpisane przed tą wersją często zawierają nazwisko pszczelarza, dlatego są pokazywane tylko tam, gdzie wolno pokazać jego nazwisko; w przeciwnym razie pasieka jest określana prowincją ("Pasieka w: …"). To samo dotyczy opisów zdjęć.
- **Dane**: dane laboratoryjne i dane punktów kontrolnych; ocena „w granicach limitu / poza limitem” tylko dla **zweryfikowanych limitów**. Odpowiedzi tekstowe i pliki nie są publikowane, chyba że administrator wyraźnie na to zezwolił dla KPK, którego odpowiedzi nie zawierają danych osobowych.
- **Raporty**: weryfikować można tylko raporty **zatwierdzone**; strona pokazuje kod raportu, laboratorium, akredytację, datę i odcisk, nigdy partii ani pszczelarza.
- **Ceny**: nigdy.
- **Zdjęcia**: tylko zdjęcia pasiek, które dostarczyły miód do partii końcowej, udostępniane bez metadanych (bez pozycji GPS i danych aparatu).

Historia miodu istnieje tylko dla partii końcowych zatwierdzonych ostatecznie, a strona pszczelarza tylko dopóty, dopóki zgoda jest aktywna.

<!-- SHOT: public-map -->
Figure: Pochodzenie według prowincji w publicznej Historii miodu.

## Jak długo dane są przechowywane {#trust-retention}

Okresy przechowywania wymienia [Table](#tbl-retention): na przykład kody jednorazowe 30 dni, wiadomości SMS 1 rok, ceny 5 lat, partie i ich zapisy 5 lat po dacie minimalnej trwałości partii końcowej, dziennik audytu 10 lat. Uruchamianie przechowywania jest **wyłączone**: administratorzy platformy widzą przebieg próbny, który tylko liczy, a pierwsze rzeczywiste uruchomienie wymaga zatwierdzenia właściciela i decyzji o formacie pseudonimów (zob. [Przechowywanie danych](#admin-retention)).
