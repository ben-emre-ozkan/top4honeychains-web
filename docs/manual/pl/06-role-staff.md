# 6. Personel {#role-staff}

## Twoja codzienna praca {#staff-daily}

**W zakładzie przetwórczym lub konfekcjonującym** wprowadzasz dane produkcyjnych punktów kontrolnych: KPK 2–3 w partiach surowego miodu oraz KPK 4–10 w partiach końcowych. Gdy partia końcowa jest kompletna, składasz wniosek o jej ostateczne zatwierdzenie, a po jej zamknięciu wysyłasz ją.

**U hurtownika lub detalisty** potwierdzasz lub odrzucasz przychodzące wysyłki oraz rejestrujesz warunki przechowywania (KPK 10) posiadanych partii końcowych.

Twoje zadania pojawiają się na stronie **Moja praca** w grupach **Wartości do wprowadzenia** i **Wysyłki do potwierdzenia**. Menu zawiera pozycje **Moja organizacja** oraz **Wysyłki i zapasy**.

## Moja organizacja {#staff-my-organization}

Strona **Moja organizacja** zawiera listę partii surowego miodu i partii końcowych Twojej organizacji. Przy każdej partii i partii końcowej jej KPK są wyświetlane jako małe znaczniki. Kolor wskazuje stan:

- szary: otwarty (brak wartości lub wartości niekompletne);
- żółty: przesłany, oczekuje na zatwierdzenie;
- zielony obramowany: zakończony, ale jeszcze niezatwierdzony;
- zielony z ✓: zatwierdzony;
- ciemny obramowany: nie dotyczy.

Kliknij znacznik, aby otworzyć stronę KPK. Odnośnik **Przebieg procesu** pokazuje oś czasu partii lub partii końcowej.

<!-- SHOT: my-organization -->
Figure: **Moja organizacja**: partie i partie końcowe z ich znacznikami KPK.

## Wprowadzanie KPK 2–3 partii {#staff-lot-ccps}

1. Na stronie **Moja praca** otwórz „KPK 2 · #[numer partii]” (lub kliknij znacznik na stronie **Moja organizacja**).
2. Wprowadź wartości w sposób opisany w części [Praca na stronie KPK](#ccp-page) i kliknij **Zapisz zapisy monitorowania**.
3. Jeśli dany etap nie dotyczy tego miodu, oznacz go jako **Nie dotyczy**, podając powód.
4. Powtórz te czynności dla KPK 3.

Następnie specjalista ds. zapewnienia jakości je zatwierdza. Partię można dodać do partii końcowej dopiero wtedy, gdy wszystkie jej KPK są zakończone lub oznaczone jako nie dotyczy i zatwierdzone.

## Wprowadzanie KPK 4–10 partii końcowej {#staff-batch-ccps}

Administrator platformy tworzy partię końcową, dodaje do niej partie i przypisuje KPK 4–10. Partia końcowa pojawia się wtedy na stronie **Moja organizacja**, a jej KPK na Twojej stronie **Moja praca**.

1. Otwórz kolejno każdy KPK partii końcowej (od KPK 4 do KPK 10) i wprowadź jego wartości.
2. KPK 10 partii końcowej dotyczy **przechowywania w Twoim zakładzie**. Musi zostać wprowadzony i zatwierdzony, zanim partia końcowa zostanie zamknięta.
3. Za każdym razem kliknij **Zapisz zapisy monitorowania**.

<!-- SHOT: ccp-page-batch -->
Figure: Strona KPK partii końcowej: skład źródłowy i parametry KPK 10.

## Wniosek o ostateczne zatwierdzenie {#staff-request-finalize}

1. Na stronie **Moja organizacja** otwórz partię końcową.
2. Karta **Zatwierdź ostatecznie** wymienia wszystko, czego jeszcze brakuje, pod nagłówkiem „Brakuje przed ostatecznym zatwierdzeniem:”, na przykład „Partia końcowa: KPK 10 (Monitorowanie temperatury i wilgotności) ma status Przesłany, jeszcze niezatwierdzony.” Dopóki czegokolwiek brakuje, przycisk **Wnioskuj o ostateczne zatwierdzenie** jest nieaktywny.
3. Gdy lista jest pusta, kliknij **Wnioskuj o ostateczne zatwierdzenie**.

<!-- SHOT: finalize-request -->
Figure: Karta **Zatwierdź ostatecznie** z listą tego, czego jeszcze brakuje.

Komunikat „Złożono wniosek o ostateczne zatwierdzenie; musi go potwierdzić specjalista ds. zapewnienia jakości (inna osoba).” potwierdza złożenie wniosku. Specjalista ds. zapewnienia jakości Twojej organizacji znajduje go w grupie **Wnioski o ostateczne zatwierdzenie**. Nie możesz potwierdzić własnego wniosku.

## Wysyłka partii końcowej {#staff-ship}

Wysłać można tylko partię końcową zatwierdzoną ostatecznie.

1. Otwórz **Wysyłki i zapasy**. Tabela **Posiadane partie końcowe** pokazuje dla każdej partii końcowej posiadaną ilość, ilość zarezerwowaną przez oczekujące wysyłki oraz ilość dostępną.
2. Kliknij **Wyślij** przy partii końcowej.
3. W polu **Odbiorca** wybierz organizację odbierającą (dowolną inną zweryfikowaną organizację), wpisz **Ilość (kg)** i opcjonalnie **Uwaga**.
4. Potwierdź przyciskiem **Wyślij**. Ilość zostaje od razu zarezerwowana: „Utworzono wysyłkę; piecza przechodzi na odbiorcę po jej potwierdzeniu.”

<!-- SHOT: shipment-create -->
Figure: Wysyłanie partii końcowej do innej organizacji.

Błędy: „Wysłać można tylko partię końcową zatwierdzoną ostatecznie.”, „Z tej partii końcowej możesz wysłać maksymalnie … kg.”, „Wybierz inną zweryfikowaną organizację jako odbiorcę.”

## Potwierdzenie lub odrzucenie wysyłki (odbiorca) {#staff-receive}

1. Na stronie **Moja praca** otwórz grupę **Wysyłki do potwierdzenia** albo przejdź do **Wysyłki i zapasy** → **Wysyłki przychodzące**.
2. Sprawdź towar. Kliknij **Potwierdź odbiór**: „Potwierdzono odbiór; jesteś teraz posiadaczem tej ilości.”
3. Możesz też wpisać **powód (w przypadku odrzucenia)** i kliknąć **Odrzuć**: „Wysyłka została odrzucona; ilość pozostaje u nadawcy.” Podanie powodu jest wymagane.

<!-- SHOT: shipment-confirm -->
Figure: Przychodząca wysyłka na telefonie: **Potwierdź odbiór** lub **Odrzuć**.

## Rejestracja przechowywania (KPK 10 u odbiorcy) {#staff-storage}

Po przyjęciu partii końcowej:

1. Na stronie **Wysyłki i zapasy** w kolumnie **Przechowywanie** przy partii końcowej kliknij **KPK 10** (strona „KPK 10 – Przechowywanie”).
2. Wprowadź jednorazowo wszystkie wymagane wartości przechowywania (na przykład temperaturę i wilgotność).
3. Kliknij **Zapisz**: „Zapisano wartości przechowywania.” Rekord otrzymuje status **Przesłany**, a specjalista ds. zapewnienia jakości Twojej organizacji znajduje go w grupie **Wartości do zatwierdzenia**.

<!-- SHOT: storage-record -->
Figure: Rejestracja przechowywania KPK 10 u odbiorcy.

## Przeglądanie zapisów producenta {#staff-read-only}

Jeśli Twoja organizacja posiada partię końcową lub ją otrzymała, możesz otwierać punkty kontrolne producenta dotyczące tej partii końcowej i jej partii **tylko do odczytu**: możesz odczytać dane i to, kto je zatwierdził, ale nie ma przycisków zapisu ani zatwierdzenia. Imiona i nazwiska pszczelarzy oraz nazwy pasiek są wyświetlane jako „nie wyświetlono”, tak jak w raporcie z wycofania. Strona wyświetla oznaczenie **Tylko do odczytu** z komunikatem „Za ten punkt kontrolny odpowiada inna organizacja; możesz go tylko odczytywać.” W przypadku partii końcowej, której Twoja organizacja nigdy nie otrzymała, platforma wyświetla stronę „Nie możesz otworzyć tej strony”.

Aby je otworzyć, przejdź do strony **Wysyłki i zapasy**, kliknij **Przebieg procesu** przy posiadanej partii końcowej, a następnie kliknij etap przebiegu.

## Raport z wycofania {#staff-recall}

Z poziomu partii końcowej lub partii możesz otworzyć **Raport z wycofania**. Zawiera on partie w górę łańcucha i wysyłki w dół łańcucha, które Twoja organizacja może odczytać, oraz przyciski **Drukuj** i **CSV**. Zob. [Wycofanie](#admin-recall).
