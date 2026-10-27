# 9. Kierownik laboratorium {#role-lab-manager}

## Twoja codzienna praca {#labmanager-daily}

Jesteś drugą z dwóch osób w laboratorium: zatwierdzasz lub zwracasz raporty laboratoryjne przesłane przez analityków. Możesz też przyjmować słoiki z próbkami (kontrole plomby, etykiety i wieczka, zob. [Przyjęcie próbki](#analyst-receive)) i tworzyć próbkę zbiorczą. Nigdy nie zatwierdzasz raportu, którego wyniki sam wprowadziłeś; w przypadku wyników laboratoryjnych od tej zasady nie ma wyjątku.

Twoje zadania pojawiają się na stronie **Moja praca** w grupie **Raporty laboratoryjne do zatwierdzenia** (oraz **Próbki do przyjęcia**).

## Zatwierdzenie lub zwrot raportu {#labmanager-decide}

1. Na stronie **Moja praca** otwórz grupę **Raporty laboratoryjne do zatwierdzenia**.
2. Sprawdź raport: plik PDF, numer raportu, datę wydania i każdą wartość. Wartości poza limitem są oznaczone.
3. Aby zatwierdzić raport, kliknij **Zatwierdź**. Jeśli wartości są poza limitem, okno dialogowe **Wartości poza limitem** prosi o potwierdzenie; kliknij **Zatwierdź mimo to**.
4. Aby zwrócić raport, wpisz **powód (w przypadku zwrotu)** i kliknij **Zwróć**: „Raport został zwrócony do analityka.”

<!-- SHOT: lab-approve -->
Figure: Kierownik laboratorium zatwierdza lub zwraca raport.

Zatwierdzenie potwierdza, że raport jest poprawnie zarejestrowany, a nie że miód spełnia wymagania; wartości poza limitem pozostają oznaczone w partii.

**Skutki zatwierdzenia** („Raport laboratoryjny został zatwierdzony; wyniki są teraz wartościami KPK 1 partii.”):

- Raport otrzymuje status **Zatwierdzono** i staje się widoczny na publicznej stronie weryfikacji.
- Platforma zapisuje, czy laboratorium było w tym momencie **akredytowane** (wraz z numerem akredytacji) oraz czy jest to laboratorium **własne** właściciela partii.
- Wyniki stają się wartościami **KPK 1** partii: wartościami próbki zbiorczej albo, gdy każdy słoik jest analizowany osobno, najgorszą wartością każdego parametru spośród pojemników (zob. [Zasada najgorszej wartości](#flow-worst)). Raport pojedynczego słoika przy analizie próbki zbiorczej pozostaje przy tym słoiku. Nie można ich już edytować; mogą je jedynie skorygować administratorzy. Jeśli laboratorium objęło badaniami każdy parametr KPK 1, KPK 1 zostaje od razu zakończony i zatwierdzony; w przeciwnym razie trafia do specjalisty ds. zapewnienia jakości w zakładzie.

**Częste błędy**

Table: Częste błędy przy zatwierdzaniu raportu laboratoryjnego {#tbl-labmgr-errors}

| Komunikat | Znaczenie |
|---|---|
| „Zasada czterech oczu: osoba, która wprowadziła wyniki, nie może ich zatwierdzić (brak wyjątku dla wyników laboratoryjnych).” | Musi go zatwierdzić inny kierownik laboratorium. |
| „Zwrot raportu wymaga podania powodu.” | Wpisz powód. |
| „Zapisany plik raportu nie odpowiada jego skrótowi SHA-256; prześlij go ponownie.” | Zapisany plik PDF uległ zmianie; analityk musi przesłać go ponownie. |
| „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; …” | Zatwierdzenie musi poczekać na KPK 0. Raport nadal można zwrócić. |

## Publiczna weryfikacja raportu {#labmanager-verify}

Każdy może sprawdzić raport na stronie **Zweryfikuj raport laboratoryjny** (`/verify-report`): wpisz **Kod raportu** i kliknij **Sprawdź**. Wyszukiwane są tylko raporty **zatwierdzone**; dla raportu roboczego lub przesłanego strona odpowiada tak jak dla nieznanego kodu. Strona pokazuje datę zatwierdzenia, laboratorium, jego akredytację, numer raportu, datę wydania i odcisk SHA-256. W sekcji **Porównaj plik PDF z tym odciskiem (plik pozostaje na Twoim urządzeniu)** przycisk **Wybierz PDF** sprawdza plik PDF na urządzeniu odwiedzającego (plik nie jest przesyłany) i informuje, czy jest zgodny. Strona nie pokazuje żadnych danych partii, pszczelarza ani użytkownika.

<!-- SHOT: verify-report -->
Figure: Publiczna weryfikacja raportu laboratoryjnego na telefonie.
