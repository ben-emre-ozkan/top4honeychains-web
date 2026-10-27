# 8. Analityk {#role-analyst}

## Twoja codzienna praca {#analyst-daily}

Pracujesz w laboratorium analizy żywności i jesteś pierwszą z jego dwóch osób: Ty wprowadzasz dane, a kierownik laboratorium je zatwierdza. Przyjmujesz zaplombowane słoiki z próbkami przesłane przez zakłady przetwórcze (po jednym z każdego pojemnika dostawy), sprawdzasz plombę, etykietę i wieczko, tworzysz próbkę zbiorczą, analizujesz miód, wprowadzasz wyniki wraz z plikiem PDF raportu i przesyłasz raport do zatwierdzenia kierownikowi laboratorium.

Twoje zadania pojawiają się na stronie **Moja praca** w grupach **Próbki do przyjęcia** i **Wyniki laboratoryjne do wprowadzenia**. Menu zawiera pozycję **Próbki i laboratorium**.

## Przyjęcie próbki {#analyst-receive}

1. Na stronie **Moja praca** otwórz grupę **Próbki do przyjęcia** albo przejdź do **Próbki i laboratorium** → **Próbki wysłane do Twojego laboratorium**. Każdy słoik pokazuje swój kod (`S-{lot}-C{n}`), swój pojemnik i numer plomby.
2. Odpowiedz na trzy kontrole słoika: **Numer plomby zgadza się z zapisem**, **Etykieta jest nienaruszona** i **Wieczko nienaruszone (przycisk wciśnięty)**.
3. Jeśli wszystkie trzy odpowiedzi brzmią **Tak**, kliknij **Zapisz przyjęcie**: słoik zostaje przyjęty.
4. Jeśli którakolwiek kontrola ma odpowiedź **Nie**, wpisz **Powód, jeśli kontrola się nie powiedzie** i kliknij **Zapisz przyjęcie**: słoik zostaje odrzucony. Zakład pobiera nowy słoik z tego samego pojemnika i wysyła go do Ciebie.

<!-- SHOT: sample-receive -->
Figure: Przyjmowanie słoików z próbkami w laboratorium: plomba, etykieta i wieczko.

Próbka z wcześniejszej dostawy (jedna próbka na partię) ma jedną kontrolę plomby: **Odebrano, plomba nienaruszona** albo **powód (jeśli plomba jest naruszona)** i **Plomba naruszona: odrzuć**.

## Utworzenie próbki zbiorczej {#analyst-composite}

Przy analizie próbki zbiorczej (ustawienie domyślne) laboratorium bada jedną próbkę zbiorczą na partię (zob. [Próbka zbiorcza lub osobno dla każdego pojemnika](#flow-analysis)). Gdy słoiki ze wszystkich pojemników partii dotrą w dobrym stanie, partia pojawia się w sekcji **Partie gotowe do próbki zbiorczej**: „Słoiki ze wszystkich pojemników tych partii dotarły w dobrym stanie. Połącz równe części słoików w jedną próbkę zbiorczą i ją zbadaj; słoiki są przechowywane do badań pojedynczych.”

1. Połącz równe części słoików.
2. Kliknij **Utwórz próbkę zbiorczą**. Zostaje utworzona i przyjęta próbka zbiorcza `S-{lot}-COMP`; jej raport wprowadza się tak jak każdy inny.

Gdy próbka zbiorcza nie spełnia wymagań lub na żądanie możesz też wprowadzić raport dla pojedynczego słoika; pozostaje on przy tym słoiku i nie zmienia wartości KPK 1 partii. Przy analizie osobno dla każdego pojemnika próbki zbiorczej nie ma: każdy słoik otrzymuje własny raport, a KPK 1 partii przyjmuje najgorszą wartość każdego parametru (zob. [Zasada najgorszej wartości](#flow-worst)).

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

1. Strona wyświetla informację „Wprowadzone wartości wymagane: … z …”; licznik zmienia się podczas wpisywania, a „Brakuje:” wymienia wartości, które są jeszcze puste. Przycisk **Prześlij do zatwierdzenia** jest aktywny tylko wtedy, gdy wszystkie wymagane wartości, plik PDF, numer raportu i data są **zapisane**: gdy wszystko jest wpisane, ale jeszcze niezapisane, strona wyświetla komunikat „Zapisz wartości przed przesłaniem.”
2. Kliknij **Prześlij do zatwierdzenia**: „Raport został przesłany; musi go zatwierdzić kierownik laboratorium (inna osoba).”

<!-- SHOT: lab-submit -->
Figure: Wprowadzono wszystkie wymagane dane: raport można przesłać.

Po przesłaniu raportu nie można już edytować, chyba że kierownik laboratorium go zwróci. Zwrócony raport pokazuje powód zwrotu; popraw raport i prześlij go ponownie.

**Częste błędy**

Table: Częste błędy przy wprowadzaniu wyników laboratoryjnych {#tbl-lab-errors}

| Komunikat | Co zrobić |
|---|---|
| „Wyniki wprowadza się tylko dla przyjętej próbki z nienaruszoną plombą.” | Najpierw przyjmij słoik; odrzucony słoik nie otrzymuje wyników. |
| „Przed przesłaniem prześlij plik PDF raportu.” | Wybierz plik PDF i zapisz. |
| „Przed wysłaniem wprowadź wszystkie wymagane wartości: wprowadzono … z ….” | Uzupełnij brakujące wartości wymienione pod nagłówkiem „Brakuje:”. |
| „Raport laboratoryjny musi być plikiem PDF.” | Jako raport akceptowane są wyłącznie pliki PDF. |
| „Raport został przesłany; kierownik laboratorium go zatwierdzi lub zwróci do poprawy.” | Poczekaj na decyzję kierownika laboratorium. |
| „Zatwierdzonego raportu laboratoryjnego nie można zmienić; zamiast tego zgłoś korektę.” | Poproś administratora platformy o korektę. |
| „Partia jest zablokowana w zatwierdzonej ostatecznie partii końcowej; wartości laboratoryjne można zmienić tylko poprzez korektę.” | Partia została zamknięta w partii końcowej; wersji roboczej nie można już zapisać ani przesłać. Zwróć się do administratora platformy. |
