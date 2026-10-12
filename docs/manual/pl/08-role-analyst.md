# 8. Analityk {#role-analyst}

## Twoja codzienna praca {#analyst-daily}

Pracujesz w laboratorium analizy żywności i jesteś pierwszą z jego dwóch osób: Ty wprowadzasz dane, a kierownik laboratorium je zatwierdza. Przyjmujesz zaplombowane próbki przesłane przez zakłady przetwórcze, sprawdzasz plombę, analizujesz miód, wprowadzasz wyniki wraz z plikiem PDF raportu i przesyłasz raport do zatwierdzenia kierownikowi laboratorium.

Twoje zadania pojawiają się na stronie **Moja praca** w grupach **Próbki do przyjęcia** i **Wyniki laboratoryjne do wprowadzenia**. Menu zawiera pozycję **Próbki i laboratorium**.

## Przyjęcie próbki {#analyst-receive}

1. Na stronie **Moja praca** otwórz grupę **Próbki do przyjęcia** albo przejdź do **Próbki i laboratorium** → **Próbki wysłane do Twojego laboratorium**.
2. Porównaj numer plomby na próbce z numerem wyświetlanym na ekranie.
3. Jeśli plomba jest nienaruszona, kliknij **Odebrano, plomba nienaruszona**: „Próbka została przyjęta; plomba jest nienaruszona.”
4. Jeśli plomba jest naruszona, wpisz **powód (jeśli plomba jest naruszona)** i kliknij **Plomba naruszona: odrzuć**: „Próbka została odrzucona: plomba jest naruszona.” Zakład musi pobrać nową próbkę.

<!-- SHOT: sample-receive -->
Figure: Przyjmowanie próbek w laboratorium: kontrola plomby.

## Wprowadzanie wyników laboratoryjnych {#analyst-results}

1. Na stronie **Moja praca** otwórz grupę **Wyniki laboratoryjne do wprowadzenia**. Dla próbki otwiera się strona **Raport laboratoryjny**.
2. Sprawdź, czy strona nie wyświetla komunikatu „KPK 0 (audyt wstępny) tej partii nie jest jeszcze zakończony; …”. Dopóki KPK 0 partii nie jest zatwierdzony, formularz jest dostępny tylko do odczytu. Poczekaj, aż specjalista ds. zapewnienia jakości w zakładzie zatwierdzi KPK 0.
3. Wpisz **Numer raportu** i **Data wydania** (nie może być datą przyszłą).
4. Wybierz **PDF raportu** (plik PDF do 10 MB).
5. Wpisz wartość każdego parametru. Parametry wymagane są oznaczone gwiazdką (*). W polach liczbowych można użyć przecinka lub kropki.
6. Kliknij **Zapisz**: „Wyniki laboratoryjne zostały zapisane.”

<!-- SHOT: lab-results-form -->
Figure: Wprowadzanie wyników laboratoryjnych wraz z plikiem PDF raportu.

Po zapisaniu platforma:

- przechowuje plik PDF wraz z jego odciskiem SHA-256 (wyświetlanym z przyciskiem kopiowania);
- generuje 12-znakowy **Kod raportu**, który każdy może sprawdzić na publicznej stronie weryfikacji;
- porównuje każdą wartość z jej limitem (**W granicach limitu** / **Poza limitem**).

Dopóki raport jest wersją roboczą, możesz go zapisywać dowolnie wiele razy.

## Przesłanie raportu {#analyst-submit}

1. Strona wyświetla informację „Wprowadzone wartości wymagane: … z …”. Przycisk **Prześlij do zatwierdzenia** jest aktywny tylko wtedy, gdy wprowadzono wszystkie wymagane wartości, plik PDF, numer raportu i datę.
2. Kliknij **Prześlij do zatwierdzenia**: „Raport został przesłany; musi go zatwierdzić kierownik laboratorium (inna osoba).”

<!-- SHOT: lab-submit -->
Figure: Wprowadzono wszystkie wymagane dane: raport można przesłać.

Po przesłaniu raportu nie można już edytować, chyba że kierownik laboratorium go zwróci. Zwrócony raport pokazuje powód zwrotu; popraw raport i prześlij go ponownie.

**Częste błędy**

Table: Częste błędy przy wprowadzaniu wyników laboratoryjnych {#tbl-lab-errors}

| Komunikat | Co zrobić |
|---|---|
| „Wyniki wprowadza się tylko dla przyjętej próbki z nienaruszoną plombą.” | Najpierw przyjmij próbkę. |
| „Przed przesłaniem prześlij plik PDF raportu.” | Wybierz plik PDF i zapisz. |
| „Przed wysłaniem wprowadź wszystkie wymagane wartości: wprowadzono … z ….” | Uzupełnij brakujące wartości wymienione pod nagłówkiem „Brakuje:”. |
| „Raport laboratoryjny musi być plikiem PDF.” | Jako raport akceptowane są wyłącznie pliki PDF. |
| „Raport został przesłany; kierownik laboratorium go zatwierdzi lub zwróci do poprawy.” | Poczekaj na decyzję kierownika laboratorium. |
| „Zatwierdzonego raportu laboratoryjnego nie można zmienić; zamiast tego zgłoś korektę.” | Poproś administratora platformy o korektę. |
| „Partia jest zablokowana w zatwierdzonej ostatecznie partii końcowej; wartości laboratoryjne można zmienić tylko poprzez korektę.” | Partia została zamknięta w partii końcowej; wersji roboczej nie można już zapisać ani przesłać. Zwróć się do administratora platformy. |
