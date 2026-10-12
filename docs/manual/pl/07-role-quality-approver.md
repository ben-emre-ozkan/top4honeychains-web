# 7. Specjalista ds. zapewnienia jakości {#role-qa}

## Twoja codzienna praca {#qa-daily}

Jesteś drugą parą oczu w swojej organizacji. Twoje zadania to:

- zatwierdzanie rekordów KPK przesłanych przez współpracowników (KPK 0, KPK 2–10 i rekordy przechowywania);
- potwierdzanie wniosków o ostateczne zatwierdzenie, co zamyka partię końcową;
- u hurtownika lub detalisty: zatwierdzanie rekordów przechowywania KPK 10.

Twoje zadania pojawiają się na stronie **Moja praca** w grupach **Wartości do zatwierdzenia** i **Wnioski o ostateczne zatwierdzenie**. Nigdy nie zatwierdzasz rekordu, w którym sam zarejestrowałeś dane.

## Zatwierdzanie rekordu KPK {#qa-approve}

1. Na stronie **Moja praca** otwórz pozycję w grupie **Wartości do zatwierdzenia**, na przykład „KPK 0 · #20070”.
2. Sprawdź wartości. Przy każdej wartości widać ocenę **W granicach limitu** lub **Poza limitem** względem jej limitu.
3. Na karcie **Zatwierdzenie (zasada czterech oczu)** przeczytaj uwagę: zatwierdzenie potwierdza, że wartości są poprawnie zarejestrowane, a nie że mieszczą się w limitach.
4. Kliknij **Zatwierdź**.

<!-- SHOT: ccp-approve -->
Figure: Zatwierdzanie rekordu KPK (zasada czterech oczu).

Komunikat „Rekord KPK został zatwierdzony.” potwierdza zatwierdzenie; etykieta statusu pokazuje **Zatwierdzono** wraz z Twoim nazwiskiem i czasem.

**Wartości poza limitem.** Jeśli którakolwiek wartość jest poza limitem, okno dialogowe **Wartości poza limitem** wymienia takie wartości, każdą z jej wartością **Limit:** (oraz adnotacją „limit jeszcze niezweryfikowany” w przypadku limitów, które wciąż czekają na weryfikację). Kliknij **Zatwierdź mimo to**, aby zatwierdzić, lub **Anuluj**. Wartości pozostają oznaczone jako poza limitem; zatwierdzenie potwierdza jedynie, że zostały poprawnie zarejestrowane.

<!-- SHOT: out-of-limit-dialog -->
Figure: Okno dialogowe dla danych poza limitem.

**Rekordy oznaczone jako nie dotyczy.** KPK oznaczony jako „Nie dotyczy” również wymaga zatwierdzenia i pojawia się w sekcji **Wartości do zatwierdzenia** u zatwierdzających, którzy go nie oznaczyli. Przed zatwierdzeniem przeczytaj podany powód.

**Częste błędy**

Table: Częste błędy przy zatwierdzaniu rekordu KPK {#tbl-qa-errors}

| Komunikat | Znaczenie |
|---|---|
| „Zasada czterech oczu: wartości tego KPK zostały zarejestrowane przez Ciebie, więc musi je zatwierdzić inna osoba.” | Wprowadziłeś wartości w tym rekordzie. Musi go zatwierdzić inny zatwierdzający. |
| „Zasada czterech oczu: to Ty oznaczyłeś ten KPK jako „Nie dotyczy”, więc musi go zatwierdzić inna osoba.” | Oznaczyłeś ten rekord jako „Nie dotyczy”. Musi go zatwierdzić inny zatwierdzający. |
| „Zatwierdzić można tylko przesłany (lub oznaczony jako "Nie dotyczy") rekord KPK.” | Nadal brakuje wymaganych wartości. |
| „Ten rekord KPK jest już zatwierdzony.” | Nie trzeba nic robić. |
| „Twoja rola (…) nie może zatwierdzać tego punktu kontrolnego; role zatwierdzające: ….” | Ten KPK zatwierdza inna rola. |

Jeśli Twoja organizacja ma **Wyjątek od zasady czterech oczu**, możesz zatwierdzać własne rekordy; komunikat brzmi wtedy „Zatwierdzono w ramach wyjątku od zasady czterech oczu (oznaczono w dzienniku audytu).” Wyjątek nigdy nie dotyczy wyników laboratoryjnych.

## Potwierdzenie ostatecznego zatwierdzenia (zamknięcie partii końcowej) {#qa-finalize}

1. Na stronie **Moja praca** otwórz pozycję w grupie **Wnioski o ostateczne zatwierdzenie** albo otwórz partię końcową na stronie **Moja organizacja**.
2. Sprawdź partię końcową: wszystkie **Punkty kontrolne partii końcowej** i **Partie źródłowe** są zatwierdzone, a lista „Brakuje przed ostatecznym zatwierdzeniem:” jest pusta.
3. Kliknij **Potwierdź i zatwierdź ostatecznie**.

<!-- SHOT: finalize-confirm -->
Figure: Potwierdzanie wniosku o ostateczne zatwierdzenie.

Partia końcowa zostaje zamknięta: „Partia końcowa została zatwierdzona ostatecznie.” Jednorazowo pojawia się pieczęć z odnośnikami **Otwórz Historię miodu** i **Pobierz kod QR**. Od tej chwili:

- partia końcowa i wszystkie jej partie są zablokowane;
- publiczna Historia miodu jest dostępna pod jej kodem QR;
- partię końcową można wysłać.

Nie ma możliwości „cofnięcia ostatecznego zatwierdzenia”. Błędy wykryte później są korygowane w procesie korekty prowadzonym przez administratorów.

Błędy: „Zasada czterech oczu: osoba, która złożyła wniosek o ostateczne zatwierdzenie, nie może go potwierdzić.”, „Najpierw należy złożyć wniosek o ostateczne zatwierdzenie.”, „Partii końcowej nie można jeszcze zatwierdzić ostatecznie: …”.

<!-- SHOT: batch-sealed -->
Figure: Zamknięta partia końcowa z odnośnikiem do Historii miodu i kodem QR.

## Zatwierdzanie rekordów przechowywania (hurtownik lub detalista) {#qa-storage}

1. Na stronie **Moja praca** otwórz pozycję oznaczoną słowem **przechowywanie** („KPK 10 · [partia końcowa]”) albo przejdź do **Wysyłki i zapasy** → **Przechowywanie**.
2. Sprawdź wartości i kliknij **Zatwierdź** (jeśli pojawi się takie pytanie, potwierdź w oknie dialogowym wartości poza limitem).

Obowiązuje ta sama zasada czterech oczu: współpracownik, który zarejestrował wartości, nie może ich zatwierdzić.
