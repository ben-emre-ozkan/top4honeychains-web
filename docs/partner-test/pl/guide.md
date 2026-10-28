# 1. O tym teście {#about}

## Czym jest TOP4HoneyChains {#what}

TOP4HoneyChains to platforma internetowa, która śledzi miód od pasieki pszczelarza aż po półkę sklepową i prowadzi weryfikowalny zapis każdego etapu. Powstała w ramach projektu TOP4HoneyChains („Wiarygodna i zrównoważona otwarta platforma dla inteligentnych łańcuchów wartości miodu”), finansowanego w ramach konkursu ICT-AGRI-FOOD 2022.

Dlaczego to ważne:

- **Zaufanie, które można sprawdzić.** Każdy rekord wprowadza jedna osoba, a zatwierdza inna (zasada czterech oczu). Każda zmiana trafia do dziennika audytu, którego nie można edytować.
- **Cały łańcuch w jednym miejscu.** Rejestracja pszczelarza, dostawa w zaplombowanych pojemnikach, próbka z każdego pojemnika, analiza laboratoryjna, punkty kontrolne (KPK) w zakładzie, zamknięta partia końcowa, wysyłka do sklepu.
- **Trudniej o oszustwo.** Plomby, wieczka gwarancyjne słoików z próbkami i kody SMS wiążą każdy rekord z osobami, które go potwierdziły. Naruszona plomba lub ponownie otwarty słoik są od razu widoczne.
- **Historia dla konsumenta.** Kod QR na słoiku otwiera Historię miodu: skąd pochodzi miód, zatwierdzone wyniki laboratoryjne z ich limitami oraz punkty kontrolne.
- **Uczciwie wobec pszczelarza.** Pszczelarz widzi swoje dostawy, wyniki i partie końcowe, podaje cenę oczekiwaną i decyduje, co jest o nim pokazywane.

Ten przewodnik pozwala Twojemu zespołowi wypróbować to wszystko w środowisku demonstracyjnym: jeden miód, od rejestracji jego pszczelarza aż po konsumenta, który skanuje kod QR. Test trwa około dwóch–trzech godzin, jeśli bierze w nim udział kilka osób; jedna osoba może też odegrać wszystkie role, jedną po drugiej.

## Jak działa wersja demonstracyjna {#demo}

- **Strona demonstracyjna:** {{demo.url}} (adres potwierdza Twój koordynator). Ciemny pasek na dole każdej strony informuje: **Środowisko demonstracyjne – nie są wysyłane prawdziwe SMS-y**. Wszystko, co widzisz, to dane testowe oznaczone **[DEMO]**.
- **Konta:** jedno konto na każdą rolę i organizację Twojego kraju (rozdział 3). Hasła znajdują się w osobnym arkuszu credentials.txt; nie udostępniaj go innym.
- **Język:** wybierz język flagą w prawym górnym rogu (na stronie logowania: TR EN ES PL LV).
- **SMS:** wersja demonstracyjna nie wysyła SMS-ów. Kody i dane logowania pojawiają się w skrzynce **Skrzynka SMS (demo)** (odnośnik na dolnym pasku, otwiera się w nowej karcie). Widzisz wiadomości swojej aktywnej organizacji oraz te, które sam wysłałeś.
- **Pierwsze logowanie:** platforma prosi o przeczytanie i potwierdzenie informacji o danych. Role, które podpisują w imieniu swojej organizacji (administrator organizacji, specjalista ds. rejestracji pszczelarzy, Specjalista ds. zapewnienia jakości, kierownik laboratorium, Specjalista ds. zgodności), potwierdzają ją kodem SMS: wyślij kod, odczytaj go w skrzynce SMS (demo) i wpisz. Potem następuje krótkie oprowadzenie; kliknij **Pomiń** albo przejdź je do końca.
- **Wspólne konta:** inni testerzy z Twojego kraju używają tych samych kont w tym samym czasie. Nadawaj swoim rekordom rozpoznawalne nazwy (Twoje inicjały) i nie zmieniaj rekordów innych testerów.
- **Zapomniane hasło:** wersja demonstracyjna nie wysyła SMS-ów, więc resetowanie hasła nie działa. Zwróć się do koordynatora.
- **Reset:** zespół platformy może przywrócić wersję demonstracyjną do stanu początkowego. Wszystko, co wprowadzono po ostatnim resecie, zostaje wtedy usunięte.
- **Przykładowy miód:** Twój kraj ma już jeden gotowy przykład: {{example.batch}} od pszczelarza {{b1.name}}. Jego Historia miodu jest dostępna pod adresem {{example.story}}.

## Ograniczenia wersji demonstracyjnej dla krajów partnerskich {#limits}

Platforma została zbudowana najpierw dla Turcji. Dla Argentyny, Polski i Łotwy wersja demonstracyjna stosuje najmniejsze bezpieczne obejścia; przeczytaj je, zanim ocenisz dany krok:

- **Numer rejestrowy.** Platforma sprawdza turecki numer gospodarstwa w rejestrze (TR, kod prowincji, 10 cyfr). W wersji demonstracyjnej kraj partnerski używa numeru testowego **AR-DEMO-**, **PL-DEMO-** lub **LV-DEMO-** i 6 cyfr. Nie jest on sprawdzany w krajowym rejestrze.
- **Prowincje i powiaty.** Listy zawierają 81 prowincji Turcji. Wersja demonstracyjna dodaje po dwa regiony na kraj partnerski, oznaczone (AR), (PL) lub (LV), z kilkoma powiatami. Są to przykłady, a nie oficjalne wykazy.
- **Mapa pochodzenia.** Historia miodu, strona pszczelarza i panel pokazują pochodzenie wyłącznie na mapie Turcji. W przypadku innych krajów strona podaje kraj i region jako tekst („poza mapą”). Mapa dla każdego kraju partnerskiego jest planowanym rozszerzeniem (A8).
- **Numery działalności.** Pszczelarze zarejestrowani w wersji demonstracyjnej otrzymują numery takie jak AR-BEE-DEMO-000001.
- **Teksty i limity.** Informacja o danych i treści zgód to projekty przygotowane dla Turcji (oznaczone **TEKST DEMONSTRACYJNY – niezatwierdzony**). Parametry laboratoryjne i ich limity są zgodne z tureckimi przepisami dotyczącymi miodu; w Twoim kraju mogą obowiązywać inne limity. Napisz nam o tym w opinii.
- **Waluta.** Cenę oczekiwaną można podać w ARS, PLN, EUR, USD lub TRY.

# 2. Role {#roles}

W TOP4HoneyChains nikt nie działa samodzielnie: każda czynność jest wykonywana w imieniu organizacji, w określonej roli. Poniżej każda rola ma swoją stronę: kim jest, co robi, z jakich ekranów korzysta i które konto demonstracyjne ją odgrywa. Podręcznik użytkownika szczegółowo opisuje każdą rolę (numer rozdziału podano na każdej karcie).

{{ROLECARDS}}

# 3. Twoje konta demonstracyjne {#accounts}

{{COORDINATORNOTE}}

{{ACCOUNTS}}

# 4. Śladem jednego miodu {#scenario}

Scenariusz prowadzi jeden miód przez platformę. Każdy krok podaje rolę, konto demonstracyjne, co należy zrobić i co powinno się pojawić. Wykonuj kroki w podanej kolejności: wiele kroków czeka na poprzedni. Wartości sugerowane dla punktów kontrolnych i raportu laboratoryjnego podaje [Tabela](#tbl-values).

Dwie zasady, które spotkasz wszędzie: **rekord wprowadza jedna osoba, a zatwierdza inna** oraz **partia lub partia końcowa przechodzi dalej dopiero wtedy, gdy wszystko wcześniej jest zatwierdzone**. Jeśli przycisk jest nieaktywny, strona informuje, czego brakuje.

{{SCENARIO}}

# 5. Opinie {#feedback}

Przekaż opinię o każdym kroku i odpowiedz na pytania ogólne. Możesz pisać na tej stronie, skorzystać z interaktywnej listy kontrolnej (plik HTML z listą kontrolną w tym pakiecie; pozwala wyeksportować odpowiedzi) albo wypełnić szablon opinii (CSV). Wynik prześlij koordynatorowi.

{{FEEDBACK}}

# Załącznik: wartości sugerowane {#values}

Te wartości mieszczą się w limitach platformy. Wpisz jedną wartość spoza limitu, aby zobaczyć, jak platforma ostrzega i prosi o potwierdzenie.

{{VALUES}}
