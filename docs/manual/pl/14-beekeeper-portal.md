# 14. Portal pszczelarza {#portal}

## Czym jest portal {#portal-about}

Portal pszczelarza, **Mój obszar pszczelarza**, to własna część platformy przeznaczona dla pszczelarza. Konto w portalu może otrzymać pszczelarz, który ma telefon komórkowy i został zarejestrowany kodem SMS. W portalu pszczelarz odczytuje własne pasieki, dostawy, słoiki z próbkami, wyniki laboratoryjne, partie końcowe i strony Historii miodu, wprowadza **cenę oczekiwaną** za swój miód oraz udziela lub wycofuje własne zgody.

Portal pokazuje wyłącznie dane zalogowanego pszczelarza. Konto pszczelarza nie ma członkostwa w żadnej organizacji i nie może otworzyć żadnej innej strony panelu. Portal otwiera platforma, gdy będzie gotowy; do tego czasu pszczelarze nie otrzymują danych logowania.

## Logowanie {#portal-sign-in}

1. Specjalista ds. rejestracji pszczelarzy wysyła dane logowania SMS-em: nazwę użytkownika i hasło tymczasowe, ważne przez 72 godziny.
2. Pszczelarz loguje się na zwykłej stronie logowania i ustawia własne hasło (co najmniej 10 znaków, zob. [Pierwsze logowanie](#first-sign-in)).
3. Jeśli czeka informacja o danych, portal prosi pszczelarza, aby najpierw ją przeczytał i potwierdził.

Zapomniane hasło resetuje się kodem wysłanym SMS-em na zweryfikowany telefon pszczelarza (zob. [Nie pamiętasz hasła](#password-reset)).

## Strona startowa {#portal-home}

Strona **Start** wita pszczelarza po imieniu i pokazuje, co jest **Do zrobienia** („Dostawy bez Twojej ceny: …”, „Informacja o danych do przeczytania i potwierdzenia”), liczbę dostaw i partii końcowych z jego miodem, **Ostatnia dostawa** i **Ostatni wynik laboratoryjny**. Menu zawiera pozycje **Moje dostawy**, **Próbki i laboratorium**, **Partie z moim miodem**, **Moje strony Honey Story**, **Moje pasieki**, **Prywatność i zgody** oraz **Konto**.

<!-- SHOT: portal-home -->
Figure: Strona startowa portalu pszczelarza na telefonie.

## Twoje dostawy i Twoja cena {#portal-price}

**Moje dostawy** wymienia dostawy pszczelarza „tak, jak zapisały je zakłady, z pojemnikami i plombami”: datę, rodzaj miodu, ilość, sposób potwierdzenia dostawy, pojemniki z wagą netto i **Historia plomb** oraz oznaczenie „poprawiona przez platformę”, gdy fakt dostawy został skorygowany przez dwóch administratorów (pszczelarz otrzymuje też SMS).

**Cena oczekiwana.** Dla każdej potwierdzonej dostawy pszczelarz wprowadza cenę oczekiwaną za kg:

1. Wpisz **Cena za kg** (więcej niż 0, najwyżej dwa miejsca po przecinku), wybierz **Waluta** (TRY, EUR, USD, ARS lub PLN) i opcjonalnie wpisz **Uwaga (opcjonalnie)**.
2. Kliknij **Podaj cenę** (lub **Zmień cenę**). Strona pyta: „Zapisać tę cenę? Razem za dostawę: …”.

Cenę można zmieniać, dopóki partia nie trafi do partii końcowej zatwierdzonej ostatecznie; potem jest zablokowana („Partia jest w sfinalizowanej partii gotowej; jej cena jest zablokowana.”). Każda zmiana jest nowym wpisem; **Historia cen** pokazuje je wszystkie. Cenę odczytują wyłącznie organizacja będąca właścicielem partii (jej administrator organizacji i Specjalista ds. zgodności) oraz administratorzy platformy; stowarzyszenie pszczelarza tylko za zgodą **Moje stowarzyszenie może widzieć moje ceny**. Cena nigdy nie pojawia się na stronach publicznych.

<!-- SHOT: portal-deliveries -->
Figure: **Moje dostawy**: pojemniki, plomby i cena oczekiwana.

## Próbki, partie końcowe i strony Historii miodu {#portal-samples}

- **Próbki i laboratorium** pokazuje każdy słoik z próbką z dostaw pszczelarza: pojemnik, plombę, laboratorium, status i kontrolę wieczka przy przyjęciu. „Wyniki pojawiają się po zatwierdzeniu raportu przez laboratorium; ocena tylko tam, gdzie limit jest zweryfikowany.”
- **Partie z moim miodem** pokazuje partie końcowe zawierające miód pszczelarza oraz **Mój udział** w nich, z odnośnikiem do strony publicznej. Inni pszczelarze nie są wymieniani.
- **Moje strony Honey Story** wymienia publiczne strony partii końcowych zatwierdzonych ostatecznie i informuje, jak pszczelarz jest na nich pokazywany: „Twoje imię i nazwisko, za Twoją zgodą” albo „neutralny kod, Twoje nazwisko nie jest pokazywane”.
- **Moje pasieki** wymienia pasieki z prowincją, powiatem, liczbą uli i położeniem. „Tylko Ty widzisz dokładne położenie swoich pasiek.”

## Prywatność i zgody {#portal-privacy}

Strona **Prywatność i zgody** składa się z trzech części:

1. **Twoje zgody**: każda zgoda jest dobrowolna, a „Wycofanie działa natychmiast.” Przyciski **Udziel** i **Wycofaj**:
   - **Moje imię i nazwisko na stronach publicznych**: bez tej zgody Historia miodu pokazuje neutralny kod zamiast imienia i nazwiska;
   - **Wiadomości o moich dostawach i wynikach**: wiadomości informacyjne poza kodami potwierdzającymi;
   - **Moje stowarzyszenie może widzieć moje ceny** (tylko dla członka stowarzyszenia);
   - **Moja publiczna strona pszczelarza**: udzielana za pośrednictwem stowarzyszenia; można ją tutaj wycofać.
2. **Potwierdzone informacje o danych**: informacje o danych wraz z datą potwierdzenia.
3. **Kto przechowuje Twoje dane**: administratorzy danych każdej organizacji, która przechowuje dane pszczelarza, wraz z kontaktem w sprawie żądań. Żądania dotyczące danych osobowych można też kierować do Specjalisty ds. zgodności organizacji (zob. [Żądania osób, których dane dotyczą](#co-requests)).

<!-- SHOT: portal-privacy -->
Figure: **Prywatność i zgody** w portalu pszczelarza.

## Konto {#portal-account}

Strona **Konto** pozwala zmienić **Telefon komórkowy** (na nowy numer wysyłany jest kod; numer jest używany po kliknięciu **Zweryfikuj**), **Język** i **Hasło**. Zawiera też przełącznik **Pokazuj animacje** i opcję **Pokaż oprowadzenie** (zob. [Oprowadzenie i animacje](#animations)).
