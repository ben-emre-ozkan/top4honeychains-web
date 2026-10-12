# 1. O platformie TOP4HoneyChains {#about}

> **Wersja robocza do przeglądu przez partnerów.**

TOP4HoneyChains to platforma internetowa, która śledzi miód od pasieki pszczelarza aż po półkę sklepową i prowadzi weryfikowalny zapis każdego etapu. Powstała w ramach projektu TOP4HoneyChains („Wiarygodna i zrównoważona otwarta platforma dla inteligentnych łańcuchów wartości miodu”), finansowanego w ramach konkursu ICT-AGRI-FOOD 2022 przez krajowe agencje finansujące krajów partnerskich.

Niniejszy podręcznik opisuje platformę w obecnej postaci, z włączonym modelem organizacji. Jest przeznaczony dla wszystkich osób, które logują się do panelu, oraz dla administratorów platformy. Strony publiczne, które nie wymagają konta, są opisane tam, gdzie mają znaczenie.

## Jak korzystać z podręcznika {#how-to-read}

- **Rozdziały 1–4** opisują platformę, funkcje, organizacje i role, logowanie oraz całą drogę partii miodu. Przeczytaj je w pierwszej kolejności.
- **Rozdziały 5–11** to osobne rozdziały dla poszczególnych ról. Każdy zaczyna się od części „Twoja codzienna praca”, a następnie opisuje krok po kroku wszystkie zadania. Przeczytaj rozdział dotyczący Twojej roli.
- **Rozdział 12** jest przeznaczony dla administratorów platformy.
- **Rozdział 13** opisuje dziennik audytu oraz to, co może odczytać publiczność.
- **Rozdział 14** zawiera słowniczek i odpowiedzi na częste problemy.

Nazwy przycisków, menu i pól są zapisane **pogrubioną czcionką**, dokładnie tak, jak wyświetlają się na ekranie. Komunikaty systemu są podane w cudzysłowie. Rysunki i tabele są numerowane; ich wykazy znajdują się po spisie treści. W przykładach użyto rekordów testowych, których nazwy zaczynają się od **[DEMO]**; nie są to prawdziwe osoby ani firmy.

> **Etykiety, które zmienią się w następnej wersji.** Kilka etykiet na ekranie otrzyma nowe nazwy w następnej wersji platformy. Ten podręcznik używa już nowych nazw, na przykład roli **Specjalista ds. zapewnienia jakości** (obecnie: „Zatwierdzający jakość”) oraz opisów funkcji zaczynających się od „Zdolność do …”. Zrzuty ekranu pokazują obecne etykiety. [Table](#tbl-renamed-labels) w słowniczku wymienia wszystkie stare i nowe nazwy.

> W wersji polskiej platforma używa skrótu **KPK** (krytyczny punkt kontroli). W wersji angielskiej stosowany jest międzynarodowy termin CCP (critical control point), a pozostałe wersje językowe mają własne skróty, zgodne z ekranem: KKN (turecka), PCC (hiszpańska), KKP (łotewska).

## Łańcuch wartości miodu {#value-chain}

Miód przechodzi przez pięć etapów. Platforma rejestruje każdy z nich:

1. **Pasieka**: pszczelarz wytwarza miód. Pszczelarze nie logują się do platformy i nie mają kont; każdego pszczelarza reprezentuje na platformie **rekord pszczelarza**, który aktualizują administratorzy platformy i stowarzyszenie pszczelarza.
2. **Dostawa**: pszczelarz dostarcza miód do zakładu przetwórczego. Zakład rejestruje dostawę, co tworzy *partię surowego miodu*, i sprawdza ją przy przyjęciu (KPK 0).
3. **Laboratorium**: zakład plombuje próbkę i wysyła ją do laboratorium analizy żywności (zwykle akredytowanego). W laboratorium analityk wprowadza wyniki analizy, a druga osoba, kierownik laboratorium, zatwierdza raport.
4. **Przetwarzanie**: zakład przetwarza miód (KPK 2–3 w partii), łączy partie w *partię końcową* i rejestruje kontrole przetwarzania, rozlewu, etykietowania, pakowania i przechowywania (KPK 4–10 w partii końcowej). Następnie dwie osoby *zatwierdzają ostatecznie* (zamykają) partię końcową.
5. **Półka**: partia końcowa zostaje wysłana do hurtownika lub detalisty. Odbiorca potwierdza odbiór i rejestruje własne warunki przechowywania.

W tym podręczniku **dane** oznaczają to, co użytkownicy rejestrują na platformie: zmierzone wartości (na przykład wilgotność w %) oraz odpowiedzi na pytania (na przykład Tak / Nie).

**Zasada czterech oczu.** Każde dane wprowadza jedna osoba, a zatwierdza druga, inna osoba; nikt nie może zatwierdzić tego, co sam wprowadził. Dzięki dwóm „parom oczu” pomyłki i manipulacje są znacznie mniej prawdopodobne. Dokładne reguły wymieniono w części [Zasady podstawowe](#fixed-rules). Każda zmiana jest ponadto zapisywana w dzienniku audytu zabezpieczonym przed manipulacją.

<!-- SHOT: intro-chain -->
Figure: Pięć etapów łańcucha wartości miodu na publicznej stronie startowej.

## Identyfikowalność {#traceability}

Identyfikowalność oznacza, że dla każdego słoika miodu można odpowiedzieć na trzy pytania:

- **Skąd pochodzi?** Partia końcowa zna swoje partie surowego miodu, a każda partia zna swoją pasiekę, pszczelarza, dostawę i raport laboratoryjny.
- **Czy została sprawdzona?** Dla każdego krytycznego punktu kontroli partii i partii końcowej zapisane są dane oraz informacje o tym, kto je wprowadził, kto je zatwierdził i kiedy.
- **Dokąd trafiła?** Wysyłki rejestrują, która organizacja posiada jaką ilość partii końcowej.

W razie problemu **Raport z wycofania** pokazuje wszystkie partie w górę łańcucha i wszystkie wysyłki w dół łańcucha dla danej partii końcowej lub partii (zob. [Wycofanie](#admin-recall)).

## Dla kogo jest platforma {#audience}

[Table](#tbl-audience) wymienia wszystkich użytkowników platformy. Organizacje i ich role opisuje [rozdział 2](#roles).

Table: Dla kogo jest platforma {#tbl-audience}

| Kto | Konto | Co robi na platformie |
|---|---|---|
| Pszczelarze | brak; reprezentowani przez **rekord pszczelarza** | Dostarczają miód; ich stowarzyszenie rejestruje ich zgodę na publiczną stronę pszczelarza |
| Stowarzyszenia pszczelarzy | członkowie organizacji | Przypisują pszczelarzy członkowskich, rejestrują ich zgodę na publiczną stronę pszczelarza |
| Zakłady przetwórcze i konfekcjonujące | członkowie organizacji | Rejestrują dostawy, pobierają próbki, rejestrują punkty kontrolne, składają i potwierdzają wnioski o ostateczne zatwierdzenie, wysyłają towar |
| Laboratoria analizy żywności | członkowie organizacji | Przyjmują próbki, wprowadzają wyniki, zatwierdzają raporty |
| Hurtownicy | członkowie organizacji | Potwierdzają wysyłki, rejestrują warunki przechowywania, odczytują zapisy producenta |
| Detaliści | członkowie organizacji | Potwierdzają wysyłki, rejestrują warunki przechowywania, odczytują zapisy producenta |
| Organy publiczne | członkowie organizacji | Odczytują zapisy (tylko do odczytu) |
| Administratorzy platformy | rola globalna konta użytkownika | Weryfikują organizacje, zarządzają członkostwami, definiują punkty kontrolne i limity, tworzą partie końcowe, odczytują dziennik audytu |
| Konsumenci i inni odwiedzający strony publiczne | brak | Odczytują strony publiczne: Historię miodu z kodu QR na słoiku, strony pszczelarzy, weryfikację raportów |

## Publiczna Historia miodu i kody QR {#honey-story}

Gdy partia końcowa zostaje zatwierdzona ostatecznie, platforma tworzy jej publiczną stronę **Historia miodu** oraz kod QR, który do niej prowadzi. Kod QR można wydrukować na etykiecie. Każdy odwiedzający może go zeskanować bez zakładania konta.

Historia miodu pokazuje, w języku wybranym przez odwiedzającego na stronie:

- nazwę partii końcowej, ilość i datę produkcji;
- drogę miodu: zbiór, dostawa, laboratorium, przetwarzanie i półka;
- pochodzenie miodu na mapie: obecnie według prowincji Turcji; mapy pozostałych krajów partnerskich są planowane;
- pszczelarzy, którzy dostarczyli miód, z odnośnikiem do ich publicznej strony pszczelarza, jeśli wyrazili na nią zgodę;
- wyniki laboratoryjne, z oceną „w granicach limitu / poza limitem” tylko dla limitów, które zostały zweryfikowane;
- punkty kontrolne przetwarzania i pakowania;
- odnośnik do weryfikacji raportu laboratoryjnego.

Historia miodu nigdy nie pokazuje numeru telefonu pszczelarza, zdjęcia osoby, dokładnej lokalizacji pasieki ani żadnej notatki tekstowej, która mogłaby zawierać dane osobowe. Zob. [Co może odczytać publiczność](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: Publiczna Historia miodu partii końcowej zatwierdzonej ostatecznie, na telefonie.

Inne strony publiczne:

- **Strona pszczelarza** (`/storefront/<code>`): publiczna strona pszczelarza, publikowana wyłącznie za podpisaną zgodą pszczelarza i przy zweryfikowanym numerze w rejestrze.
- **Zweryfikuj raport laboratoryjny** (`/verify-report`): każdy może wpisać kod raportu i sprawdzić, czy raport istnieje i został zatwierdzony, a także porównać plik PDF z jego odciskiem.
- **Strona startowa** (`/`): strona główna projektu z przyciskiem logowania.
