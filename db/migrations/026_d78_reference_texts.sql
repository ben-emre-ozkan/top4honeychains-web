-- 026_d78_reference_texts.sql
-- D-78 P5 (D-84; docs/manual/PLANNED-APP-CHANGES.md A1, A5, A6, B1): reference texts as the user manual describes them.
--   A5  capability descriptions written as abilities ("Able to ...") in five languages (manual table "Capabilities");
--   A6  names for the capabilities that showed their codes (CentralAssociation, PublicAuthority, Beekeeping, PlatformAdmin) and
--       the manual's names for Beekeeping ("Beekeeping enterprise");
--   A1  the role QualityApprover is named "Quality Assurance Officer" (the English name lives in RefText.RoleEnglish; the role
--       code is unchanged).
-- Base text (English) in Capabilities.Description; names and translations in ReferenceTranslations. Reference texts only; no
-- business data. Idempotent: a row is written only when its text differs, a missing row is added. SQL Server 2019 (level 150).
-- Run with sqlcmd -I -f 65001 (or in SSMS).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Capabilities', N'U') IS NULL OR OBJECT_ID(N'dbo.ReferenceTranslations', N'U') IS NULL OR OBJECT_ID(N'dbo.OrgRoles', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 004_org_model.sql and 009 (reference translations) first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

BEGIN TRANSACTION;

DECLARE @texts TABLE (Entity varchar(40) NOT NULL, Code varchar(40) NOT NULL, Field varchar(40) NOT NULL, Culture varchar(5) NOT NULL, Value nvarchar(400) NOT NULL);
INSERT @texts (Entity, Code, Field, Culture, Value) VALUES
    -- A5: descriptions
    ('Capability', 'Association', 'Description', 'en', N'Able to link its member beekeepers and record their consent for a public beekeeper page.'),
    ('Capability', 'Association', 'Description', 'tr', N'Üye arıcılarını bağlayabilir ve herkese açık arıcı sayfası için rızalarını kaydedebilir.'),
    ('Capability', 'Association', 'Description', 'es', N'Capaz de vincular a sus apicultores miembros y registrar su consentimiento para una página pública de apicultor.'),
    ('Capability', 'Association', 'Description', 'pl', N'Zdolność do przypisywania pszczelarzy członkowskich i rejestrowania ich zgody na publiczną stronę pszczelarza.'),
    ('Capability', 'Association', 'Description', 'lv', N'Spēj sasaistīt savus biedrus biškopjus un reģistrēt viņu piekrišanu publiskai biškopja lapai.'),
    ('Capability', 'Processing', 'Description', 'en', N'Able to receive and process honey: deliveries, samples, lot CCP 0–3 and batch CCP 4–6.'),
    ('Capability', 'Processing', 'Description', 'tr', N'Balı teslim alıp işleyebilir: teslimler, numuneler, lot KKN 0–3 ve parti KKN 4–6.'),
    ('Capability', 'Processing', 'Description', 'es', N'Capaz de recibir y procesar miel: entregas, muestras, PCC 0–3 del lote y PCC 4–6 de la partida.'),
    ('Capability', 'Processing', 'Description', 'pl', N'Zdolność do przyjmowania i przetwarzania miodu: dostawy, próbki, KPK 0–3 partii i KPK 4–6 partii końcowej.'),
    ('Capability', 'Processing', 'Description', 'lv', N'Spēj pieņemt un pārstrādāt medu: piegādes, paraugi, partijas KKP 0–3 un gala partijas KKP 4–6.'),
    ('Capability', 'Packing', 'Description', 'en', N'Able to fill, label and pack honey: batch CCP 7–9 and the CCP 10 storage at the plant.'),
    ('Capability', 'Packing', 'Description', 'tr', N'Balı doldurabilir, etiketleyebilir ve paketleyebilir: parti KKN 7–9 ve tesisteki KKN 10 depolama.'),
    ('Capability', 'Packing', 'Description', 'es', N'Capaz de llenar, etiquetar y envasar miel: PCC 7–9 de la partida y el almacenamiento del PCC 10 en la planta.'),
    ('Capability', 'Packing', 'Description', 'pl', N'Zdolność do rozlewu, etykietowania i pakowania miodu: KPK 7–9 partii końcowej oraz przechowywanie w KPK 10 w zakładzie.'),
    ('Capability', 'Packing', 'Description', 'lv', N'Spēj pildīt, marķēt un iepakot medu: gala partijas KKP 7–9 un KKP 10 uzglabāšana uzņēmumā.'),
    ('Capability', 'Laboratory', 'Description', 'en', N'Able to analyse honey samples and issue lab reports, as an accredited or an in-house food-analysis laboratory. The accreditation number and its validity date are kept on this capability.'),
    ('Capability', 'Laboratory', 'Description', 'tr', N'Akredite veya kurum içi bir gıda analiz laboratuvarı olarak bal numunelerini analiz edebilir ve laboratuvar raporu düzenleyebilir. Akreditasyon numarası ve geçerlilik tarihi bu yetenekte tutulur.'),
    ('Capability', 'Laboratory', 'Description', 'es', N'Capaz de analizar muestras de miel y emitir informes de laboratorio, como laboratorio de análisis de alimentos acreditado o propio. El número de acreditación y su fecha de validez se guardan en esta capacidad.'),
    ('Capability', 'Laboratory', 'Description', 'pl', N'Zdolność do analizowania próbek miodu i wydawania raportów laboratoryjnych, jako akredytowane lub zakładowe laboratorium analizy żywności. Numer akredytacji i data jej ważności są przechowywane przy tej funkcji.'),
    ('Capability', 'Laboratory', 'Description', 'lv', N'Spēj analizēt medus paraugus un izsniegt laboratorijas pārskatus kā akreditēta vai iekšējā pārtikas analīžu laboratorija. Akreditācijas numurs un tā derīguma termiņš tiek glabāts pie šīs kompetences.'),
    ('Capability', 'Wholesale', 'Description', 'en', N'Able to hold batches in wholesale, record their storage (CCP 10) and ship them on.'),
    ('Capability', 'Wholesale', 'Description', 'tr', N'Partileri toptan satış için elinde tutabilir, depolamalarını (KKN 10) kaydedebilir ve sevk edebilir.'),
    ('Capability', 'Wholesale', 'Description', 'es', N'Capaz de tener partidas en el comercio mayorista, registrar su almacenamiento (PCC 10) y reenviarlas.'),
    ('Capability', 'Wholesale', 'Description', 'pl', N'Zdolność do posiadania partii końcowych w handlu hurtowym, rejestrowania ich przechowywania (KPK 10) i wysyłania ich dalej.'),
    ('Capability', 'Wholesale', 'Description', 'lv', N'Spēj turēt gala partijas vairumtirdzniecībā, reģistrēt to uzglabāšanu (KKP 10) un sūtīt tās tālāk.'),
    ('Capability', 'Retail', 'Description', 'en', N'Able to hold batches for sale to consumers, record their storage (CCP 10) and ship them on.'),
    ('Capability', 'Retail', 'Description', 'tr', N'Partileri tüketiciye satış için elinde tutabilir, depolamalarını (KKN 10) kaydedebilir ve sevk edebilir.'),
    ('Capability', 'Retail', 'Description', 'es', N'Capaz de tener partidas para la venta a los consumidores, registrar su almacenamiento (PCC 10) y reenviarlas.'),
    ('Capability', 'Retail', 'Description', 'pl', N'Zdolność do posiadania partii końcowych przeznaczonych do sprzedaży konsumentom, rejestrowania ich przechowywania (KPK 10) i wysyłania ich dalej.'),
    ('Capability', 'Retail', 'Description', 'lv', N'Spēj turēt gala partijas pārdošanai patērētājiem, reģistrēt to uzglabāšanu (KKP 10) un sūtīt tās tālāk.'),
    ('Capability', 'CentralAssociation', 'Description', 'en', N'Able to verify beekeepers'' associations, when the platform is configured for it.'),
    ('Capability', 'CentralAssociation', 'Description', 'tr', N'Platform bu şekilde yapılandırılmışsa arıcı birliklerini doğrulayabilir.'),
    ('Capability', 'CentralAssociation', 'Description', 'es', N'Capaz de verificar asociaciones de apicultores, cuando la plataforma está configurada para ello.'),
    ('Capability', 'CentralAssociation', 'Description', 'pl', N'Zdolność do weryfikowania stowarzyszeń pszczelarzy, jeśli platforma jest tak skonfigurowana.'),
    ('Capability', 'CentralAssociation', 'Description', 'lv', N'Spēj pārbaudīt biškopju biedrības, ja platforma ir tā konfigurēta.'),
    ('Capability', 'PublicAuthority', 'Description', 'en', N'Able to read all records, as a ministry or inspection body; enters nothing.'),
    ('Capability', 'PublicAuthority', 'Description', 'tr', N'Bakanlık veya denetim kuruluşu olarak tüm kayıtları okuyabilir; hiçbir şey girmez.'),
    ('Capability', 'PublicAuthority', 'Description', 'es', N'Capaz de leer todos los registros, como ministerio u organismo de inspección; no ingresa nada.'),
    ('Capability', 'PublicAuthority', 'Description', 'pl', N'Zdolność do odczytu wszystkich zapisów, jako ministerstwo lub organ kontrolny; niczego nie wprowadza.'),
    ('Capability', 'PublicAuthority', 'Description', 'lv', N'Spēj lasīt visus ierakstus kā ministrija vai uzraudzības iestāde; neko neievada.'),
    ('Capability', 'Beekeeping', 'Description', 'en', N'Able to produce honey. Beekeepers have no accounts; their enterprise is represented by a Beekeeper record.'),
    ('Capability', 'Beekeeping', 'Description', 'tr', N'Bal üretebilir. Arıcıların hesabı yoktur; işletmeleri bir Arıcı kaydı ile temsil edilir.'),
    ('Capability', 'Beekeeping', 'Description', 'es', N'Capaz de producir miel. Los apicultores no tienen cuenta; su explotación está representada por un registro de apicultor.'),
    ('Capability', 'Beekeeping', 'Description', 'pl', N'Zdolność do wytwarzania miodu. Pszczelarze nie mają kont; ich gospodarstwo reprezentuje rekord pszczelarza.'),
    ('Capability', 'Beekeeping', 'Description', 'lv', N'Spēj iegūt medu. Biškopjiem nav kontu; viņu saimniecību pārstāv biškopja ieraksts.'),
    ('Capability', 'BeekeeperRegistration', 'Description', 'en', N'Able to register beekeepers and their apiaries in its area, attest their identity and registry number, record their consent and issue their sign-in.'),
    ('Capability', 'BeekeeperRegistration', 'Description', 'tr', N'Bölgesindeki arıcıları ve arılıklarını kaydedebilir, kimliklerini ve kayıt numaralarını doğrulayabilir, rızalarını kaydedebilir ve giriş bilgilerini verebilir.'),
    ('Capability', 'BeekeeperRegistration', 'Description', 'es', N'Capaz de registrar a los apicultores de su zona y sus colmenares, certificar su identidad y número de registro, registrar su consentimiento y emitir su acceso.'),
    ('Capability', 'BeekeeperRegistration', 'Description', 'pl', N'Zdolność do rejestrowania pszczelarzy ze swojego obszaru i ich pasiek, poświadczania ich tożsamości i numeru rejestrowego, rejestrowania ich zgody i wydawania danych logowania.'),
    ('Capability', 'BeekeeperRegistration', 'Description', 'lv', N'Spēj reģistrēt savas teritorijas biškopjus un viņu dravas, apliecināt viņu identitāti un reģistra numuru, reģistrēt viņu piekrišanu un izsniegt pieteikšanās datus.'),
    ('Capability', 'PlatformAdmin', 'Description', 'en', N'Able to run the platform.'),
    ('Capability', 'PlatformAdmin', 'Description', 'tr', N'Platformu işletebilir.'),
    ('Capability', 'PlatformAdmin', 'Description', 'es', N'Capaz de operar la plataforma.'),
    ('Capability', 'PlatformAdmin', 'Description', 'pl', N'Zdolność do prowadzenia platformy.'),
    ('Capability', 'PlatformAdmin', 'Description', 'lv', N'Spēj uzturēt platformu.'),
    -- A6 / B1: names (English rows only where the code is not a readable name)
    ('Capability', 'CentralAssociation', 'Name', 'en', N'Central association'),
    ('Capability', 'PublicAuthority', 'Name', 'en', N'Public authority'),
    ('Capability', 'Beekeeping', 'Name', 'en', N'Beekeeping enterprise'),
    ('Capability', 'Beekeeping', 'Name', 'tr', N'Arıcılık İşletmesi'),
    ('Capability', 'Beekeeping', 'Name', 'es', N'Explotación apícola'),
    ('Capability', 'Beekeeping', 'Name', 'pl', N'Pszczelarstwo'),
    ('Capability', 'Beekeeping', 'Name', 'lv', N'Biškopības saimniecība'),
    ('Capability', 'PlatformAdmin', 'Name', 'en', N'Platform administration'),
    ('Capability', 'BeekeeperRegistration', 'Name', 'en', N'Beekeeper registration'),
    -- A1: the Quality Assurance Officer
    ('OrgRole', 'QualityApprover', 'Name', 'tr', N'Kalite Güvence Sorumlusu'),
    ('OrgRole', 'QualityApprover', 'Name', 'es', N'Responsable de aseguramiento de la calidad'),
    ('OrgRole', 'QualityApprover', 'Name', 'pl', N'Specjalista ds. zapewnienia jakości'),
    ('OrgRole', 'QualityApprover', 'Name', 'lv', N'Kvalitātes nodrošināšanas speciālists');

-- base text (English description)
UPDATE c SET c.Description = t.Value
FROM dbo.Capabilities c JOIN @texts t ON t.Entity = 'Capability' AND t.Code = c.Code AND t.Field = 'Description' AND t.Culture = 'en'
WHERE ISNULL(c.Description, N'') <> t.Value;

-- translations (the English description is the base text, not a translation row)
UPDATE r SET r.Value = t.Value, r.UpdatedAt = SYSUTCDATETIME(), r.UpdatedByUserId = NULL
FROM dbo.ReferenceTranslations r
JOIN @texts t ON r.Entity = t.Entity AND r.EntityKey = t.Code AND r.Field = t.Field AND r.Culture = t.Culture
WHERE NOT (t.Field = 'Description' AND t.Culture = 'en') AND r.Value <> t.Value;

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value, UpdatedAt)
SELECT t.Entity, t.Code, t.Field, t.Culture, t.Value, SYSUTCDATETIME()
FROM @texts t
WHERE NOT (t.Field = 'Description' AND t.Culture = 'en')
  AND (   (t.Entity = 'Capability' AND EXISTS (SELECT 1 FROM dbo.Capabilities c WHERE c.Code = t.Code))
       OR (t.Entity = 'OrgRole' AND EXISTS (SELECT 1 FROM dbo.OrgRoles o WHERE o.Code = t.Code)))
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations r
                  WHERE r.Entity = t.Entity AND r.EntityKey = t.Code AND r.Field = t.Field AND r.Culture = t.Culture);

COMMIT TRANSACTION;
PRINT N'026: reference texts written.';
GO

SET NOEXEC OFF;
GO
