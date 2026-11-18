-- 029_d87_consumer_accounts.sql
-- D-87 (docs/DECISIONS.md): consumer accounts on the web platform.
--   * ConsumerAccounts: a consumer sign-in account (no role, no membership): pseudonym code, language, age confirmation, the
--     accepted consumer notice, created and last sign-in times, status. Deleting an account removes this row and the saved list;
--     the Identity user is anonymised by the application.
--   * ConsumerSavedBatches: the honeys a consumer saved explicitly (opened stories are never recorded).
--   * ConsumerFeedback: feedback to the organization that owns the batch, with its answer; it keeps the pseudonym only, so it
--     stays with the organization after the account is deleted (ConsumerUserId becomes NULL).
--   * Notices: audience 'Consumer' (the old check is disabled and replaced by a wider one, as 025 did for Consents).
--   * RolePermissions: area ConsumerFeedback, Read and Enter (answer), for OrgAdmin, QualityApprover and ComplianceOfficer.
--   * Seeds the DRAFT consumer privacy notice (version consumer-2026-11-draft1) in five languages (O-14). Insert-only.
-- Additive and idempotent; a second run changes nothing. SQL Server 2019 (the live database runs at level 140). sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Notices', N'U') IS NULL OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL OR OBJECT_ID(N'dbo.FinalHoneyProductions', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 013 and 020 first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

IF OBJECT_ID(N'dbo.ConsumerAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerAccounts
    (
        UserId          nvarchar(450) NOT NULL CONSTRAINT PK_ConsumerAccounts PRIMARY KEY,
        Pseudonym       varchar(12)   NOT NULL,
        Language        varchar(5)    NOT NULL,
        AgeConfirmedAt  datetime2     NOT NULL,
        NoticeId        int           NULL CONSTRAINT FK_ConsumerAccounts_Notices REFERENCES dbo.Notices (NoticeId),
        NoticeVersion   varchar(40)   NULL,
        CreatedAt       datetime2     NOT NULL CONSTRAINT DF_ConsumerAccounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        LastSignInAt    datetime2     NULL,
        Status          varchar(20)   NOT NULL CONSTRAINT DF_ConsumerAccounts_Status DEFAULT ('Active'),
        CONSTRAINT CK_ConsumerAccounts_Status CHECK (Status IN ('Active', 'Locked'))
    );
    CREATE UNIQUE INDEX UX_ConsumerAccounts_Pseudonym ON dbo.ConsumerAccounts (Pseudonym);
END;
GO

IF OBJECT_ID(N'dbo.ConsumerSavedBatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerSavedBatches
    (
        ConsumerUserId          nvarchar(450) NOT NULL CONSTRAINT FK_ConsumerSavedBatches_ConsumerAccounts REFERENCES dbo.ConsumerAccounts (UserId),
        FinalHoneyProductionId  int           NOT NULL CONSTRAINT FK_ConsumerSavedBatches_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        SavedAt                 datetime2     NOT NULL CONSTRAINT DF_ConsumerSavedBatches_SavedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_ConsumerSavedBatches PRIMARY KEY (ConsumerUserId, FinalHoneyProductionId)
    );
END;
GO

IF OBJECT_ID(N'dbo.ConsumerFeedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerFeedback
    (
        FeedbackId              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerFeedback PRIMARY KEY,
        ConsumerUserId          nvarchar(450)  NULL CONSTRAINT FK_ConsumerFeedback_ConsumerAccounts REFERENCES dbo.ConsumerAccounts (UserId),
        Pseudonym               varchar(12)    NOT NULL,
        FinalHoneyProductionId  int            NOT NULL CONSTRAINT FK_ConsumerFeedback_FinalHoneyProductions REFERENCES dbo.FinalHoneyProductions (FinalHoneyProductionId),
        OrgId                   int            NOT NULL CONSTRAINT FK_ConsumerFeedback_Organizations REFERENCES dbo.Organizations (OrgId),
        Category                varchar(30)    NOT NULL,
        Message                 nvarchar(1000) NOT NULL,
        Language                varchar(5)     NULL,
        CreatedAt               datetime2      NOT NULL CONSTRAINT DF_ConsumerFeedback_CreatedAt DEFAULT (SYSUTCDATETIME()),
        Status                  varchar(20)    NOT NULL CONSTRAINT DF_ConsumerFeedback_Status DEFAULT ('New'),
        ReadAt                  datetime2      NULL,
        ReadByUserId            nvarchar(450)  NULL,
        Answer                  nvarchar(1000) NULL,
        AnsweredAt              datetime2      NULL,
        AnsweredByUserId        nvarchar(450)  NULL,
        ClosedAt                datetime2      NULL,
        ClosedByUserId          nvarchar(450)  NULL,
        CONSTRAINT CK_ConsumerFeedback_Category CHECK (Category IN ('TasteSmell', 'TextureCrystallization', 'PackagingLabel', 'SuspectedFake', 'Other')),
        CONSTRAINT CK_ConsumerFeedback_Status CHECK (Status IN ('New', 'Read', 'Answered', 'Closed'))
    );
    CREATE INDEX IX_ConsumerFeedback_Org ON dbo.ConsumerFeedback (OrgId, Status);
    CREATE INDEX IX_ConsumerFeedback_Consumer ON dbo.ConsumerFeedback (ConsumerUserId, CreatedAt);
END;
GO

-- Notices for consumers: widen the audience check (the old one stays, disabled)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notices_Audience' AND is_disabled = 0)
    ALTER TABLE dbo.Notices NOCHECK CONSTRAINT CK_Notices_Audience;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notices_Audience2')
    ALTER TABLE dbo.Notices ADD CONSTRAINT CK_Notices_Audience2 CHECK (Audience IN ('Beekeeper', 'Member', 'All', 'Consumer'));
GO

INSERT dbo.RolePermissions (Area, AreaKey, Action, OrgRole)
SELECT v.Area, '', v.Action, v.OrgRole
FROM (VALUES ('ConsumerFeedback', 'Read', N'OrgAdmin'), ('ConsumerFeedback', 'Read', N'QualityApprover'), ('ConsumerFeedback', 'Read', N'ComplianceOfficer'),
             ('ConsumerFeedback', 'Enter', N'OrgAdmin'), ('ConsumerFeedback', 'Enter', N'QualityApprover'), ('ConsumerFeedback', 'Enter', N'ComplianceOfficer')) AS v (Area, Action, OrgRole)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions p WHERE p.Area = v.Area AND p.AreaKey = '' AND p.Action = v.Action AND p.OrgRole = v.OrgRole);
PRINT N'029: ' + CONVERT(nvarchar(10), @@ROWCOUNT) + N' role permission(s) added.';
GO

BEGIN TRANSACTION;

DECLARE @n TABLE (Kind varchar(30) NOT NULL, Audience varchar(20) NOT NULL, Purpose varchar(30) NULL, Version varchar(40) NOT NULL,
                  Language varchar(5) NOT NULL, Body nvarchar(max) NOT NULL);
INSERT @n (Kind, Audience, Purpose, Version, Language, Body) VALUES
    ('PrivacyNotice', 'Consumer', NULL, 'consumer-2026-11-draft1', 'en', REPLACE(N'DRAFT for review by the university''s data-protection office. This text is not yet approved.|NL||NL|Privacy notice for consumer accounts of TOP4HoneyChains|NL||NL|Who is responsible. The platform operator (the TOP4HoneyChains project, INTALA Lab, Işık University) is the data controller of your consumer account. When you send feedback about a honey, the organization that produced the honey receives it and is responsible for its answer; it sees only your code (for example C-7F3K2Q), never your user name or phone number.|NL||NL|What is processed. Your user name, the code given to your account, your language, your confirmation that you are 18 or older, the version of this notice you accepted, the time of sign-up and of your last sign-in, the network address at sign-up, the honeys you save and the feedback you send with its answers. Your mobile phone number only if you add and verify it. No name, no e-mail address, and the platform does not record which Honey Stories you open.|NL||NL|Why. To give you the account and its functions (your request); to send one-time codes by SMS if you add a phone (necessary for the service); to protect the platform against misuse (legitimate interest).|NL||NL|Who receives it. The organization that owns a honey receives your feedback about it, with your code only. Reports of a suspected fake or opened jar are also seen by the platform administrators. The SMS provider receives your phone number and the text of each message. Feedback is never public. Nothing is sold or used for advertising.|NL||NL|Where. The data is stored on servers in Türkiye.|NL||NL|How long (proposal). Your account and saved honeys: until you delete them, or two years after your last sign-in. Feedback: three years after it was closed; after your account is deleted it stays with the organization under your code only.|NL||NL|Your rights (KVKK Article 11, GDPR Articles 15 to 22). You can download your data and delete your account on your account page at any time. You can also ask the platform operator what is held about you, have it corrected and object to processing.', N'|NL|', NCHAR(10))),
    ('PrivacyNotice', 'Consumer', NULL, 'consumer-2026-11-draft1', 'tr', REPLACE(N'TASLAK: Üniversitenin kişisel verilerin korunması birimi tarafından incelenecektir. Bu metin henüz onaylanmamıştır.|NL||NL|TOP4HoneyChains tüketici hesapları için aydınlatma metni|NL||NL|Veri sorumlusu. Tüketici hesabınızın veri sorumlusu platform işletmecisidir (TOP4HoneyChains projesi, INTALA Lab, Işık Üniversitesi). Bir bal hakkında geri bildirim gönderdiğinizde, balı üreten kurum bu bildirimi alır ve yanıtından sorumludur; kurum yalnızca kodunuzu (örneğin C-7F3K2Q) görür, kullanıcı adınızı veya telefon numaranızı asla görmez.|NL||NL|İşlenen veriler. Kullanıcı adınız, hesabınıza verilen kod, diliniz, 18 yaşında veya daha büyük olduğunuza ilişkin beyanınız, kabul ettiğiniz bu metnin sürümü, kayıt ve son giriş zamanınız, kayıt sırasındaki ağ adresi, kaydettiğiniz ballar ve gönderdiğiniz geri bildirimler ile yanıtları. Cep telefonu numaranız yalnızca eklediğiniz ve doğruladığınız takdirde. Ad, e-posta adresi işlenmez; platform hangi Bal Hikâyelerini açtığınızı kaydetmez.|NL||NL|Amaç ve hukuki sebep. Hesabı ve işlevlerini size sunmak (talebiniz); telefon eklerseniz tek kullanımlık kodları SMS ile göndermek (hizmet için zorunlu); platformu kötüye kullanıma karşı korumak (meşru menfaat).|NL||NL|Aktarılan taraflar. Bir balın sahibi olan kurum, o bal hakkındaki geri bildiriminizi yalnızca kodunuzla alır. Sahte olabilir veya kavanoz açılmış bildirimlerini platform yöneticileri de görür. SMS sağlayıcısı telefon numaranızı ve her mesajın metnini alır. Geri bildirimler hiçbir zaman herkese açık değildir. Hiçbir veri satılmaz veya reklam için kullanılmaz.|NL||NL|Saklama yeri. Veriler Türkiye''deki sunucularda saklanır.|NL||NL|Saklama süresi (öneri). Hesabınız ve kayıtlı ballarınız: siz silene kadar veya son girişinizden iki yıl sonrasına kadar. Geri bildirimler: kapatılmalarından üç yıl sonrasına kadar; hesabınız silindikten sonra yalnızca kodunuzla kurumda kalır.|NL||NL|Haklarınız (KVKK madde 11, GDPR madde 15-22). Verilerinizi hesap sayfanızdan istediğiniz zaman indirebilir ve hesabınızı silebilirsiniz. Ayrıca platform işletmecisinden hakkınızda tutulan verileri öğrenmeyi, düzeltilmesini istemeyi ve işlemeye itiraz etmeyi talep edebilirsiniz.', N'|NL|', NCHAR(10))),
    ('PrivacyNotice', 'Consumer', NULL, 'consumer-2026-11-draft1', 'es', REPLACE(N'BORRADOR para revisión por la oficina de protección de datos de la universidad. Este texto aún no está aprobado.|NL||NL|Aviso de privacidad para las cuentas de consumidor de TOP4HoneyChains|NL||NL|Responsable. El operador de la plataforma (el proyecto TOP4HoneyChains, INTALA Lab, Universidad Işık) es el responsable del tratamiento de su cuenta de consumidor. Cuando envía un comentario sobre una miel, la organización que la produjo lo recibe y es responsable de su respuesta; solo ve su código (por ejemplo C-7F3K2Q), nunca su nombre de usuario ni su teléfono.|NL||NL|Qué se trata. Su nombre de usuario, el código de su cuenta, su idioma, su confirmación de que tiene 18 años o más, la versión de este aviso que aceptó, la hora del registro y de su último acceso, la dirección de red al registrarse, las mieles que guarda y los comentarios que envía con sus respuestas. Su teléfono móvil solo si lo añade y lo verifica. No se tratan nombre ni correo electrónico, y la plataforma no registra qué Historias de la Miel abre.|NL||NL|Para qué. Para darle la cuenta y sus funciones (su solicitud); para enviar códigos de un solo uso por SMS si añade un teléfono (necesario para el servicio); para proteger la plataforma contra el abuso (interés legítimo).|NL||NL|Quién lo recibe. La organización propietaria de una miel recibe sus comentarios sobre ella, solo con su código. Los avisos de posible falsificación o frasco abierto también los ven los administradores de la plataforma. El proveedor de SMS recibe su número y el texto de cada mensaje. Los comentarios nunca son públicos. Nada se vende ni se usa para publicidad.|NL||NL|Dónde. Los datos se guardan en servidores en Türkiye.|NL||NL|Cuánto tiempo (propuesta). Su cuenta y sus mieles guardadas: hasta que las elimine, o dos años después de su último acceso. Comentarios: tres años después de cerrarse; tras eliminar su cuenta quedan en la organización solo con su código.|NL||NL|Sus derechos (KVKK artículo 11, RGPD artículos 15 a 22). Puede descargar sus datos y eliminar su cuenta en cualquier momento desde la página de su cuenta. También puede pedir al operador de la plataforma qué datos tiene sobre usted, su rectificación y oponerse al tratamiento.', N'|NL|', NCHAR(10))),
    ('PrivacyNotice', 'Consumer', NULL, 'consumer-2026-11-draft1', 'pl', REPLACE(N'PROJEKT do weryfikacji przez uczelniany zespół ochrony danych. Ten tekst nie jest jeszcze zatwierdzony.|NL||NL|Informacja o przetwarzaniu danych dla kont konsumentów TOP4HoneyChains|NL||NL|Administrator. Administratorem danych Twojego konta konsumenta jest operator platformy (projekt TOP4HoneyChains, INTALA Lab, Uniwersytet Işık). Gdy wysyłasz opinię o miodzie, otrzymuje ją organizacja, która go wyprodukowała, i odpowiada ona za odpowiedź; widzi tylko Twój kod (np. C-7F3K2Q), nigdy nazwę użytkownika ani numer telefonu.|NL||NL|Jakie dane. Nazwa użytkownika, kod nadany kontu, język, potwierdzenie ukończenia 18 lat, zaakceptowana wersja tej informacji, czas rejestracji i ostatniego logowania, adres sieciowy przy rejestracji, zapisane miody oraz wysłane opinie z odpowiedziami. Numer telefonu tylko wtedy, gdy go dodasz i zweryfikujesz. Nie przetwarzamy imienia ani adresu e-mail, a platforma nie zapisuje, które Historie Miodu otwierasz.|NL||NL|W jakim celu. Aby udostępnić konto i jego funkcje (Twoje żądanie); aby wysyłać jednorazowe kody SMS, jeśli dodasz telefon (niezbędne dla usługi); aby chronić platformę przed nadużyciami (prawnie uzasadniony interes).|NL||NL|Odbiorcy. Organizacja będąca właścicielem miodu otrzymuje Twoją opinię o nim wyłącznie z Twoim kodem. Zgłoszenia podejrzenia podróbki lub otwartego słoika widzą także administratorzy platformy. Dostawca SMS otrzymuje numer telefonu i treść każdej wiadomości. Opinie nigdy nie są publiczne. Nic nie jest sprzedawane ani wykorzystywane do reklamy.|NL||NL|Gdzie. Dane są przechowywane na serwerach w Türkiye.|NL||NL|Jak długo (propozycja). Konto i zapisane miody: do czasu ich usunięcia lub dwa lata po ostatnim logowaniu. Opinie: trzy lata po zamknięciu; po usunięciu konta pozostają w organizacji tylko z Twoim kodem.|NL||NL|Twoje prawa (KVKK art. 11, RODO art. 15-22). W każdej chwili możesz pobrać swoje dane i usunąć konto na stronie konta. Możesz też zapytać operatora platformy, jakie dane o Tobie przechowuje, żądać ich sprostowania i wnieść sprzeciw wobec przetwarzania.', N'|NL|', NCHAR(10))),
    ('PrivacyNotice', 'Consumer', NULL, 'consumer-2026-11-draft1', 'lv', REPLACE(N'PROJEKTS, ko pārskatīs universitātes datu aizsardzības nodaļa. Šis teksts vēl nav apstiprināts.|NL||NL|Privātuma paziņojums TOP4HoneyChains patērētāju kontiem|NL||NL|Pārzinis. Jūsu patērētāja konta datu pārzinis ir platformas operators (projekts TOP4HoneyChains, INTALA Lab, Işık Universitāte). Kad nosūtāt atsauksmi par medu, to saņem organizācija, kas medu ražoja, un tā atbild par atbildi; tā redz tikai jūsu kodu (piemēram, C-7F3K2Q), nekad jūsu lietotājvārdu vai tālruņa numuru.|NL||NL|Kādi dati. Jūsu lietotājvārds, kontam piešķirtais kods, valoda, apstiprinājums, ka jums ir 18 gadi vai vairāk, pieņemtā šī paziņojuma versija, reģistrācijas un pēdējās pieteikšanās laiks, tīkla adrese reģistrācijas brīdī, saglabātais medus un nosūtītās atsauksmes ar atbildēm. Mobilā tālruņa numurs tikai tad, ja to pievienojat un apstiprināt. Vārds un e-pasta adrese netiek apstrādāti, un platforma nereģistrē, kurus Medus stāstus jūs atverat.|NL||NL|Kāpēc. Lai nodrošinātu kontu un tā funkcijas (jūsu pieprasījums); lai nosūtītu vienreizējos kodus pa SMS, ja pievienojat tālruni (nepieciešams pakalpojumam); lai aizsargātu platformu pret ļaunprātīgu izmantošanu (leģitīmās intereses).|NL||NL|Saņēmēji. Medus īpašniece organizācija saņem jūsu atsauksmi par to tikai ar jūsu kodu. Ziņojumus par aizdomām par viltojumu vai atvērtu burku redz arī platformas administratori. SMS pakalpojumu sniedzējs saņem jūsu tālruņa numuru un katras ziņas tekstu. Atsauksmes nekad nav publiskas. Nekas netiek pārdots vai izmantots reklāmai.|NL||NL|Kur. Dati tiek glabāti serveros Türkiye.|NL||NL|Cik ilgi (priekšlikums). Konts un saglabātais medus: līdz jūs tos izdzēšat vai divus gadus pēc pēdējās pieteikšanās. Atsauksmes: trīs gadus pēc slēgšanas; pēc konta dzēšanas tās paliek organizācijā tikai ar jūsu kodu.|NL||NL|Jūsu tiesības (KVKK 11. pants, VDAR 15.-22. pants). Jebkurā laikā varat lejupielādēt savus datus un dzēst kontu sava konta lapā. Varat arī jautāt platformas operatoram, kādi dati par jums tiek glabāti, pieprasīt to labošanu un iebilst pret apstrādi.', N'|NL|', NCHAR(10)));

INSERT dbo.Notices (Kind, Audience, OrgId, Purpose, Version, Language, Body, BodySha256, IsDraft)
SELECT n.Kind, n.Audience, NULL, n.Purpose, n.Version, n.Language, n.Body,
       LOWER(CONVERT(char(64), HASHBYTES('SHA2_256', n.Body), 2)), 1
FROM @n n
WHERE NOT EXISTS (SELECT 1 FROM dbo.Notices x
                  WHERE x.Kind = n.Kind AND x.Audience = n.Audience AND x.OrgId IS NULL AND ISNULL(x.Purpose, '') = ISNULL(n.Purpose, '')
                    AND x.Version = n.Version AND x.Language = n.Language);
PRINT N'029: ' + CONVERT(nvarchar(10), @@ROWCOUNT) + N' consumer notice text(s) seeded (draft).';

COMMIT TRANSACTION;
GO

PRINT N'029: ConsumerAccounts, ConsumerSavedBatches, ConsumerFeedback, the consumer notice audience and permissions are in place.';
