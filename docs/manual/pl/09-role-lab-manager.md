# 9. Kierownik laboratorium {#role-lab-manager}

## Twoja codzienna praca {#labmanager-daily}

Zatwierdzasz lub zwracasz raporty laboratoryjne przesłane przez analityków. Możesz też przyjmować próbki. Nigdy nie zatwierdzasz raportu, którego wyniki sam wprowadziłeś; w przypadku wyników laboratoryjnych od tej zasady nie ma wyjątku.

Twoje zadania pojawiają się na stronie **Moja praca** w grupie **Raporty laboratoryjne do zatwierdzenia** (oraz **Próbki do przyjęcia**).

## Zatwierdzenie lub zwrot raportu {#labmanager-decide}

1. Na stronie **Moja praca** otwórz grupę **Raporty laboratoryjne do zatwierdzenia**.
2. Sprawdź raport: plik PDF, numer raportu, datę wydania i każdą wartość. Wartości poza limitem są oznaczone.
3. Aby zatwierdzić raport, kliknij **Zatwierdź**. Jeśli wartości są poza limitem, okno dialogowe **Wartości poza limitem** prosi o potwierdzenie; kliknij **Zatwierdź mimo to**.
4. Aby zwrócić raport, wpisz **powód (w przypadku zwrotu)** i kliknij **Zwróć**: „Raport został zwrócony do analityka.”

<!-- SHOT: lab-approve -->

Zatwierdzenie potwierdza, że raport jest poprawnie zarejestrowany, a nie że miód spełnia wymagania; wartości poza limitem pozostają oznaczone w partii.

**Skutki zatwierdzenia** („Raport laboratoryjny został zatwierdzony; wyniki są teraz wartościami KPK 1 partii.”):

- Raport otrzymuje status **Zatwierdzono** i staje się widoczny na publicznej stronie weryfikacji.
- Platforma zapisuje, czy laboratorium było w tym momencie **akredytowane** (wraz z numerem akredytacji) oraz czy jest to laboratorium **własne** właściciela partii.
- Wyniki stają się wartościami **KPK 1** partii. Nie można ich już edytować; mogą je jedynie skorygować administratorzy. Jeśli laboratorium objęło badaniami każdy parametr KPK 1, KPK 1 zostaje od razu zakończony i zatwierdzony; w przeciwnym razie trafia do zatwierdzającego jakość w zakładzie.

**Częste błędy**

| Komunikat | Znaczenie |
|---|---|
| „Zasada czterech oczu: osoba, która wprowadziła wyniki, nie może ich zatwierdzić (brak wyjątku dla wyników laboratoryjnych).” | Musi go zatwierdzić inny kierownik laboratorium. |
| „Zwrot raportu wymaga podania powodu.” | Wpisz powód. |
| „Zapisany plik raportu nie odpowiada jego skrótowi SHA-256; prześlij go ponownie.” | Zapisany plik PDF uległ zmianie; analityk musi przesłać go ponownie. |
| „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; …” | Zatwierdzenie musi poczekać na KPK 0. Raport nadal można zwrócić. |

## Publiczna weryfikacja raportu {#labmanager-verify}

Każdy może sprawdzić raport na stronie **Zweryfikuj raport laboratoryjny** (`/verify-report`): wpisz **Kod raportu** i kliknij **Sprawdź**. Strona pokazuje, czy raport jest zatwierdzony, a także laboratorium, jego akredytację, numer raportu, datę i odcisk SHA-256. Funkcja **Porównaj plik PDF z tym odciskiem (plik pozostaje na Twoim urządzeniu)** sprawdza plik PDF na urządzeniu odwiedzającego (plik nie jest przesyłany). Strona nie pokazuje żadnych danych partii, pszczelarza ani użytkownika.

<!-- SHOT: verify-report -->
