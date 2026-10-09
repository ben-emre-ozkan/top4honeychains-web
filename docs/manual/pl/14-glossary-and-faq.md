# 14. Słowniczek i najczęściej zadawane pytania {#glossary-faq}

## Słowniczek {#glossary}

| Termin | Znaczenie |
|---|---|
| **HACCP** | Analiza zagrożeń i krytyczne punkty kontroli (Hazard Analysis and Critical Control Points): metoda zapewnienia bezpieczeństwa żywności, z której wywodzą się punkty kontrolne. |
| **KPK (krytyczny punkt kontroli)** | Etap, na którym zagrożenie jest kontrolowane przez pomiar lub sprawdzenie. W wersji angielskiej CCP, w tureckiej KKN, w hiszpańskiej PCC, w łotewskiej KKP. |
| **Parametr** | Jedna wartość rejestrowana w KPK, na przykład wilgotność lub temperatura. |
| **Limit krytyczny** | Minimalna i/lub maksymalna dopuszczalna wartość parametru. |
| **Zweryfikowany limit** | Limit potwierdzony w ramach projektu; tylko zweryfikowane limity dają publiczną ocenę „w granicach limitu / poza limitem”. |
| **W granicach limitu / Poza limitem** | Wynik porównania wartości z jej limitem. Wartości poza limitem pozostają oznaczone także po zatwierdzeniu. |
| **Nie dotyczy** | KPK, który nie ma zastosowania do danego miodu, zarejestrowany wraz z powodem; nadal wymaga zatwierdzenia. |
| **Partia surowego miodu** | Miód z jednej dostawy z jednej pasieki (jeden okres, jeden rodzaj miodu). Obejmuje KPK 0–3. |
| **Partia końcowa** | Miód z jednej lub kilku partii, przetworzony i zapakowany razem. Obejmuje KPK 4–10 i ma kod QR. |
| **Ostateczne zatwierdzenie / zamknięcie** | Dwuosobowy krok, który blokuje partię końcową i jej partie oraz publikuje Historię miodu. |
| **Zasada czterech oczu** | Każdy rekord zatwierdza inna osoba niż ta, która go wprowadziła. |
| **Wyjątek od zasady czterech oczu** | Ustawienie dla małych organizacji, które pozwala samodzielnie zatwierdzać rekordy KPK; takie zatwierdzenie jest zawsze oznaczane. Nigdy nie dotyczy wyników laboratoryjnych. |
| **Próbka / przeciwpróbka** | Zaplombowana próbka wysyłana do laboratorium oraz jej bliźniacza część przechowywana w zakładzie. |
| **Numer plomby** | Numer na plombie próbki; laboratorium sprawdza go przy przyjęciu. |
| **Raport laboratoryjny / kod raportu** | Raport laboratorium (plik PDF i wartości) oraz jego publiczny 12-znakowy kod. |
| **Akredytowane laboratorium** | Laboratorium z ważną akredytacją, odnotowaną w chwili zatwierdzenia raportu. |
| **Laboratorium własne** | Laboratorium należące do właściciela partii. |
| **Posiadacz** | Organizacja, która posiada ilość partii końcowej po potwierdzonej wysyłce. |
| **Piecza** | Informacja o tym, kto posiada towar; przechodzi na odbiorcę, gdy potwierdzi on wysyłkę. |
| **Raport z wycofania** | Zestawienie wszystkiego, co znajduje się w górę i w dół łańcucha od danej partii końcowej lub partii. |
| **Historia miodu** | Publiczna strona partii końcowej zatwierdzonej ostatecznie, otwierana z jej kodu QR. |
| **Strona pszczelarza** | Publiczna strona pszczelarza, publikowana za jego zgodą. |
| **Numer w rejestrze (AKS)** | Numer gospodarstwa pszczelarza w rejestrze: `TR` + kod prowincji + 10 cyfr. |
| **Członkostwo** | Rola danej osoby w organizacji. Jedna osoba może mieć kilka członkostw. |
| **Funkcja** | Rodzaj pracy wykonywanej przez organizację (Przetwarzanie, Laboratorium, Handel detaliczny, …). |
| **Dziennik audytu** | Rejestr wszystkich zmian, do którego można tylko dopisywać, chroniony łańcuchem skrótów. |
| **Odcisk SHA-256** | Kod obliczany na podstawie zawartości pliku; każda zmiana pliku zmienia ten kod. |

## Najczęściej zadawane pytania i rozwiązywanie problemów {#faq}

**Nie mogę się zalogować: „Nieprawidłowa nazwa użytkownika lub hasło. …”**
Sprawdź nazwę użytkownika (a nie adres e-mail) i hasło. Po 5 nieudanych próbach konto zostaje zablokowane na 15 minut; odczekaj lub poproś administratora platformy o odblokowanie konta (**Odblokuj**), a następnie w razie potrzeby użyj opcji **Zapomniałeś hasła?**.

**„To konto nie ma jeszcze dostępu do panelu: nie należy do żadnej organizacji.”**
Twoje konto nie ma aktywnego członkostwa. Poproś administratora platformy o dodanie Cię do Twojej organizacji z odpowiednią rolą.

**Widzę stronę „Nie możesz otworzyć tej strony” (403).**
Twoja rola lub organizacja nie ma dostępu do tej strony lub tego rekordu. W przypadku KPK innej organizacji jest to oczekiwane zachowanie, chyba że Twoja organizacja posiada daną partię końcową. Jeśli potrzebujesz dostępu, zwróć się do administratora organizacji.

**Widzę stronę „Tej strony tu nie ma” (404).**
Adres jest błędny, rekord nie istnieje lub (w przypadku Historii miodu) partia końcowa nie została jeszcze zatwierdzona ostatecznie.

**Nie mogę nic wprowadzić; strona wyświetla „Tylko do odczytu”.**
Przeczytaj podany pod spodem powód. Typowe powody: Twoja organizacja nie jest jeszcze zweryfikowana; za ten KPK odpowiada inna organizacja lub rola; rekord został zablokowany przez ostateczne zatwierdzenie.

**Moja organizacja nie jest zweryfikowana.**
Administrator organizacji przesyła dokument (licencję, akredytację, dowód wpisu do rejestru) na stronie organizacji; następnie administrator platformy weryfikuje organizację.

**Laboratorium nie może wprowadzić wyników: „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; …”**
Zakład musi najpierw wprowadzić KPK 0, a jego zatwierdzający jakość musi go zatwierdzić.

**Nie mogę zatwierdzić: „Zasada czterech oczu: wartości tego KPK zostały zarejestrowane przez Ciebie, więc musi je zatwierdzić inna osoba.”**
Musi to zrobić inny zatwierdzający. Jeśli w Twojej organizacji za jakość odpowiada tylko jedna osoba, zapytaj administratora platformy o wyjątek od zasady czterech oczu (niedostępny dla wyników laboratoryjnych).

**Przycisk „Wnioskuj o ostateczne zatwierdzenie” jest nieaktywny.**
Przeczytaj listę „Brakuje przed ostatecznym zatwierdzeniem:”. Każdy KPK partii końcowej, w tym KPK 10 w zakładzie, oraz każdy KPK partii musi być zatwierdzony.

**Zmieniłem wartość i zatwierdzenie zniknęło.**
Tak ma być: każda zmiana wymaga ponownego zatwierdzenia.

**Partia nie pojawia się na stronie „Zarządzaj partią”.**
Partia pojawia się tylko wtedy, gdy wszystkie jej KPK są zakończone lub oznaczone jako nie dotyczy i zatwierdzone, i tylko u hurtownika danej partii końcowej. Zakład przetwórczy musi być powiązany ze swoim rekordem hurtownika (organizacja → **Edytuj**); strona podaje nazwy zakładów, które nie są powiązane.

**Wartość została zatwierdzona, ale jest błędna.**
Poproś administratora platformy o korektę; zatwierdza ją drugi administrator.

**Strona pszczelarza nie jest publiczna.**
Sprawdź, czy zgoda jest zarejestrowana, numer w rejestrze jest zweryfikowany, stowarzyszenie jest zweryfikowane, a pszczelarz jest aktywny.

**Pracuję dla dwóch organizacji.**
Przełącz się za pomocą wskaźnika organizacja · rola na górnym pasku. Wybór obowiązuje do zamknięcia przeglądarki.

**Nie otrzymuję wiadomości e-mail umożliwiającej zresetowanie hasła.**
Sprawdź folder spamu. Administrator platformy może ustawić dla Ciebie nowe hasło.
