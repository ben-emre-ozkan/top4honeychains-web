# 10. Administrator organizacji {#role-org-admin}

## Twoja codzienna praca {#orgadmin-daily}

Administrator organizacji zarządza organizacją na platformie. Twoje zadania to:

- aktualizowanie dokumentów organizacji (licencja, akredytacja, zaświadczenie podatkowe, dowód wpisu do rejestru);
- w zakładzie: odczytywanie dostaw i próbek, składanie wniosków o ostateczne zatwierdzenie, wysyłanie towaru i potwierdzanie wysyłek;
- u hurtownika lub detalisty: potwierdzanie wysyłek;
- w stowarzyszeniu: rejestrowanie i wycofywanie zgód pszczelarzy na publiczną stronę pszczelarza.

Członkostwami i funkcjami Twojej organizacji zarządzają administratorzy platformy. Jeśli współpracownik potrzebuje dostępu lub innej roli, zwróć się do nich.

## Strona Twojej organizacji {#orgadmin-org-page}

1. Otwórz **Moja organizacja** i kliknij **Strona organizacji**.
2. Strona pokazuje status, funkcje, dokumenty, historię weryfikacji i członków Twojej organizacji.

<!-- SHOT: org-details -->
Figure: Dokumenty organizacji wraz z ich odciskami SHA-256.

### Przesyłanie dokumentu {#orgadmin-documents}

1. W sekcji dokumentów wybierz typ: **Licencja**, **Akredytacja**, **Zaświadczenie podatkowe**, **Dowód wpisu do rejestru** lub **Inne**.
2. Wybierz plik: PDF do 10 MB lub JPG, PNG albo WEBP do 8 MB.
3. Kliknij **Prześlij**: „Dokument został przesłany.”

Platforma przechowuje plik wraz z jego odciskiem SHA-256. Dokumentów nie można usuwać. Nowa organizacja musi przesłać co najmniej jeden dokument, zanim administrator platformy będzie mógł ją zweryfikować.

## Ostateczne zatwierdzenie i wysyłki {#orgadmin-plant}

W zakładzie możesz złożyć wniosek o ostateczne zatwierdzenie partii końcowej przyciskiem **Wnioskuj o ostateczne zatwierdzenie** i wysłać ją przyciskiem **Wyślij**, tak samo jak Personel: zob. [Wniosek o ostateczne zatwierdzenie](#staff-request-finalize) i [Wysyłka partii końcowej](#staff-ship). U odbiorcy możesz użyć przycisków **Potwierdź odbiór** lub **Odrzuć**: zob. [Potwierdzenie lub odrzucenie wysyłki](#staff-receive).

## Strony pszczelarzy (stowarzyszenia) {#orgadmin-storefronts}

Stowarzyszenie może opublikować publiczną **Stronę pszczelarza** dla każdego pszczelarza członkowskiego, który wyrazi na to pisemną zgodę. Strona pokazuje wyłącznie zweryfikowane dane: imię i nazwisko pszczelarza (tylko za zgodą; w przeciwnym razie neutralny kod, na przykład `BK-…`), prowincję i powiat, rodzaje miodu, zweryfikowane partie i zatwierdzone wyniki laboratoryjne oraz partie końcowe, do których trafił miód. Nigdy nie pokazuje numeru telefonu, zdjęcia osoby, daty urodzenia ani dokładnej lokalizacji pasieki. Odwiedzający kontaktują się z pszczelarzem za pośrednictwem stowarzyszenia.

Strona jest publiczna tylko wtedy, gdy spełnione są **wszystkie** poniższe warunki:

- pszczelarz jest aktywny;
- numer gospodarstwa pszczelarza w rejestrze został zweryfikowany przez administratora platformy;
- Twoje stowarzyszenie jest zweryfikowane;
- zgoda została zarejestrowana przez Twoje stowarzyszenie (obecne stowarzyszenie pszczelarza) i nie została wycofana.

### Rejestracja zgody {#orgadmin-consent}

1. Odczytaj pszczelarzowi treść zgody i poproś o podpisanie formularza.
2. Otwórz **Strony pszczelarzy** i kliknij **Zarejestruj zgodę** przy nazwisku pszczelarza.
3. Zaznacz **Treść zgody została odczytana pszczelarzowi i podpisana.**
4. Zaznacz **Pszczelarz zgadza się na wyświetlanie nazwiska (w przeciwnym razie wyświetlany jest neutralny kod)** tylko wtedy, gdy pszczelarz wyraził na to zgodę.
5. Wpisz **Data udzielenia zgody** (nie może być datą przyszłą) i załącz **Podpisany dokument zgody** (zdjęcie lub plik PDF).
6. Zapisz.

<!-- SHOT: consent-form -->
Figure: Rejestracja zgody pszczelarza na telefonie.

Wyświetla się komunikat „Zgoda została zarejestrowana; strona pszczelarza została opublikowana.” albo, jeśli numer w rejestrze nie został jeszcze zweryfikowany, „Zgoda została zarejestrowana. Strona pszczelarza zostanie opublikowana po weryfikacji numeru w rejestrze.”

**Karta do druku.** Przycisk **Karta do druku** drukuje kartę z kodem QR strony pszczelarza, przeznaczoną na stoisko pszczelarza.

### Wycofanie zgody {#orgadmin-withdraw}

1. Na stronie **Strony pszczelarzy** wpisz opcjonalnie powód w polu **powód (opcjonalnie)** i kliknij **Wycofaj zgodę**.
2. Strona zostaje natychmiast usunięta: „Zgoda została wycofana; strona pszczelarza nie jest już publiczna.”

Błędy: „Ten pszczelarz ma już aktywną zgodę; aby zarejestrować nową, najpierw wycofaj obecną.”, „Potwierdź, że tekst zgody został odczytany pszczelarzowi.”, „Załącz podpisany dokument zgody.”
