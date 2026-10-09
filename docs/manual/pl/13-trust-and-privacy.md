# 13. Zaufanie i prywatność {#trust}

## Dziennik audytu {#trust-audit}

Każda zmiana na platformie (każdy zapis, zatwierdzenie, ostateczne zatwierdzenie, wysyłka, logowanie, zmiana uprawnień) jest zapisywana w dzienniku audytu (**Dziennik audytu**). Administratorzy platformy otwierają go w menu **Zarządzanie Użytkownikami** → **Dziennik audytu**.

Każdy wiersz pokazuje:

- **Kiedy**: czas lokalny; po najechaniu kursorem widoczny jest czas UTC;
- **Kto**: użytkownika i organizację;
- **Akcja**: na przykład Utwórz, Zaktualizuj, Zatwierdzenie, Ostateczne zatwierdzenie, Zweryfikuj, Logowanie, z oznaczeniami takimi jak „Zatwierdzone samodzielnie”;
- **Rekord**: którego rekordu dotyczy wpis, z odnośnikiem, jeśli istnieje odpowiednia strona administracyjna;
- **Zmiany**: każde zmienione pole w postaci **stara → nowa**, z nazwą pola w Twoim języku; identyfikatory użytkowników i organizacji są wyświetlane jako nazwy;
- **Skrót**: początek odcisku wiersza.

**Pokaż surowe dane** wyświetla zapisane dane dokładnie w takiej postaci, w jakiej zostały zapisane (stare i nowe wartości).

Filtruj według typu rekordu, akcji, nazwy użytkownika lub oznaczenia (na przykład `SelfApproved`) i kliknij **Filtruj**.

<!-- SHOT: audit-log -->

### Tylko dopisywanie i łańcuch skrótów {#trust-hash-chain}

Do dziennika audytu można tylko dopisywać. Baza danych odrzuca każdą próbę zmiany lub usunięcia wiersza. Każdy wiersz zawiera odcisk SHA-256 własnej treści oraz poprzedniego wiersza, dzięki czemu wiersze tworzą łańcuch.

**Zweryfikuj łańcuch skrótów** przelicza cały łańcuch. Komunikat „Łańcuch skrótów: Wszystkie wiersze (…) zostały poprawnie zweryfikowane.” oznacza, że nic nie zostało zmienione. W przeciwnym razie komunikat wskazuje pierwszy wiersz, który został zmieniony lub który nie wskazuje na poprzedni wiersz.

## Rekordy, które się nie zmieniają {#trust-locked}

- Zatwierdzony rekord KPK zachowuje zatwierdzenie tylko dopóty, dopóki jego wartości się nie zmienią.
- Partia końcowa zatwierdzona ostatecznie i jej partie są zablokowane.
- Zatwierdzone wyniki laboratoryjne stają się wartościami KPK 1 dostępnymi tylko do odczytu.
- Korekty wymagają udziału dwóch administratorów i są rejestrowane wraz z powodem.

## Pliki i odciski {#trust-files}

Formularze dostawy, pliki PDF raportów laboratoryjnych, dokumenty organizacji, dokumenty potwierdzające wpis do rejestru oraz dokumenty zgody są przechowywane poza publicznym folderem WWW, pod losową nazwą, wraz z ich **odciskiem SHA-256**. Przesyłane pliki są sprawdzane na podstawie zawartości, a nie tylko nazwy: PDF do 10 MB lub JPG, PNG albo WEBP do 8 MB. Zanim zatwierdzony raport laboratoryjny zostanie wykorzystany, platforma sprawdza, czy zapisany plik PDF nadal odpowiada swojemu odciskowi. Każdy może porównać plik PDF raportu z jego odciskiem na publicznej stronie weryfikacji bez przesyłania pliku.

## Co widzi publiczność {#trust-public}

Strony publiczne (Historia miodu, strona pszczelarza, weryfikacja raportu) pokazują wyłącznie zatwierdzone, zweryfikowane informacje:

- **Miejsce**: tylko **prowincja** (strona pszczelarza podaje także powiat). Prowincja jest ustalana na serwerze; współrzędne pasiek nigdy nie opuszczają serwera. Mapa Turcji koloruje prowincje, nigdy nie pokazuje punktów.
- **Osoby**: imię i nazwisko pszczelarza tylko za pisemną zgodą (w przeciwnym razie neutralny kod); nigdy numer telefonu, data urodzenia ani zdjęcie osoby. Żadnych nazw użytkowników platformy.
- **Wartości**: wartości laboratoryjne i wartości punktów kontrolnych; ocena „w granicach limitu / poza limitem” tylko dla **zweryfikowanych limitów**. Odpowiedzi tekstowe i pliki nie są publikowane, chyba że administrator wyraźnie na to zezwolił dla KPK, którego odpowiedzi nie zawierają danych osobowych.
- **Raporty**: kod raportu, laboratorium, akredytacja, data i odcisk; nigdy partia ani pszczelarz.

Historia miodu istnieje tylko dla partii końcowych zatwierdzonych ostatecznie, a strona pszczelarza tylko dopóty, dopóki zgoda jest aktywna.

<!-- SHOT: public-map -->
