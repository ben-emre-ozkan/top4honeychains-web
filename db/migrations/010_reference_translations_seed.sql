-- 010_reference_translations_seed.sql
-- Full i18n (D-51): stored translations (tr, es, pl, lv; en where the base text is not English) of the
-- reference data shown in dropdowns and tables. Rows are matched by their current base text (by code for
-- capabilities and roles), so a translation is only added for the text it was made for; ids may differ
-- between databases. Insert-only: existing translations, also ones edited on the translation screen, are kept.
-- Translations are machine-assisted drafts; review them on Reference data -> Translations.
--
-- Idempotent; a second run inserts nothing. SQL Server 2019 (level 150). Run with sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ReferenceTranslations', N'U') IS NULL
BEGIN
    RAISERROR(N'Run 009_reference_translations.sql first; aborting.', 16, 1);
    SET NOEXEC ON;
END;
GO

BEGIN TRANSACTION;

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Çam Balı'),
        ('es', N'Miel de pino'),
        ('pl', N'Miód sosnowy (spadziowy)'),
        ('lv', N'Priežu medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Pine Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Çam ağaçlarındaki salgılardan (basra) üretilen; koyu rengi, güçlü aroması ve yüksek mineral içeriğiyle bilinen bal.'),
        ('es', N'Miel producida a partir de las secreciones de mielada de los pinos, caracterizada por su color oscuro, aroma intenso y alto contenido mineral.'),
        ('pl', N'Miód wytwarzany ze spadzi na sosnach, o ciemnej barwie, intensywnym aromacie i wysokiej zawartości składników mineralnych.'),
        ('lv', N'Medus, kas iegūts no lapu medus izdalījumiem uz priedēm; tam raksturīga tumša krāsa, spēcīgs aromāts un augsts minerālvielu saturs.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey produced from honeydew secretions on pine trees, characterized by its dark color, strong aroma, and high mineral content.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Çiçek Balı'),
        ('es', N'Miel de flores'),
        ('pl', N'Miód kwiatowy'),
        ('lv', N'Ziedu medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Flower Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Çeşitli çiçekli bitkilerin nektarından elde edilen; çiçek çeşitliliğine bağlı olarak geniş bir renk, tat ve fizikokimyasal özellik yelpazesi gösteren bal.'),
        ('es', N'Miel obtenida del néctar de diversas plantas con flores, que presenta una amplia gama de colores, sabores y propiedades fisicoquímicas según la diversidad floral.'),
        ('pl', N'Miód pozyskiwany z nektaru różnych roślin kwitnących, o szerokiej gamie barw, smaków i właściwości fizykochemicznych zależnych od różnorodności kwiatowej.'),
        ('lv', N'Medus, kas iegūts no dažādu ziedaugu nektāra, ar plašu krāsu, garšu un fizikāli ķīmisko īpašību daudzveidību atkarībā no ziedu dažādības.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey obtained from the nectar of various flowering plants, exhibiting a wide range of colors, flavors, and physicochemical properties depending on floral diversity.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Kekik Balı'),
        ('es', N'Miel de tomillo'),
        ('pl', N'Miód tymiankowy'),
        ('lv', N'Timiāna medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Thyme Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Ağırlıklı olarak kekik çiçeklerinden elde edilen; güçlü aroması, yüksek antioksidan etkinliği ve kendine özgü tadıyla bilinen bal.'),
        ('es', N'Miel procedente principalmente de flores de tomillo, conocida por su aroma intenso, su alta actividad antioxidante y su perfil de sabor característico.'),
        ('pl', N'Miód pochodzący głównie z kwiatów tymianku, znany z intensywnego aromatu, wysokiej aktywności przeciwutleniającej i charakterystycznego smaku.'),
        ('lv', N'Medus, kas iegūts galvenokārt no timiāna ziediem, pazīstams ar spēcīgu aromātu, augstu antioksidantu aktivitāti un raksturīgu garšas profilu.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey derived predominantly from thyme flowers, known for its strong aroma, high antioxidant activity, and distinctive flavor profile.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Ayçiçeği Balı'),
        ('es', N'Miel de girasol'),
        ('pl', N'Miód słonecznikowy'),
        ('lv', N'Saulespuķu medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Sunflower Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Çoğunlukla ayçiçeği nektarından üretilen; genellikle açık renkli, hafif tatlı ve orta düzeyde kristalleşme eğilimli bal.'),
        ('es', N'Miel producida principalmente a partir de néctar de girasol, por lo general de color claro, sabor suave y tendencia moderada a la cristalización.'),
        ('pl', N'Miód wytwarzany głównie z nektaru słonecznika, zwykle o jasnej barwie, łagodnym smaku i umiarkowanej skłonności do krystalizacji.'),
        ('lv', N'Medus, kas iegūts galvenokārt no saulespuķu nektāra, parasti gaišā krāsā, ar maigu garšu un mērenu tieksmi kristalizēties.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey produced mainly from sunflower nectar, typically light-colored with a mild taste and moderate crystallization tendency.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Narenciye Balı'),
        ('es', N'Miel de cítricos'),
        ('pl', N'Miód cytrusowy'),
        ('lv', N'Citrusaugu medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Citrus Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Narenciye çiçeklerinden elde edilen; açık rengi, taze aroması ve görece düşük antioksidan kapasitesiyle öne çıkan bal.'),
        ('es', N'Miel obtenida de flores de cítricos, caracterizada por su color claro, su aroma fresco y una capacidad antioxidante relativamente baja.'),
        ('pl', N'Miód pozyskiwany z kwiatów cytrusów, charakteryzujący się jasną barwą, świeżym aromatem i stosunkowo niską zdolnością przeciwutleniającą.'),
        ('lv', N'Medus, kas iegūts no citrusaugu ziediem, raksturīgs ar gaišu krāsu, svaigu aromātu un salīdzinoši zemu antioksidantu kapacitāti.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey obtained from citrus blossoms, characterized by its light color, fresh aroma, and relatively low antioxidant capacity.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Kestane Balı'),
        ('es', N'Miel de castaño'),
        ('pl', N'Miód kasztanowy'),
        ('lv', N'Kastaņu medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Chestnut Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Kestane çiçeklerinden üretilen koyu renkli bal; buruk-tatlı tadı, yüksek mineral içeriği ve güçlü biyoaktif özellikleriyle dikkat çeker.'),
        ('es', N'Miel de color oscuro producida a partir de flores de castaño, destacada por su sabor agridulce, su alto contenido mineral y sus marcadas propiedades bioactivas.'),
        ('pl', N'Ciemny miód z kwiatów kasztana, wyróżniający się gorzkawo-słodkim smakiem, wysoką zawartością składników mineralnych i silnymi właściwościami bioaktywnymi.'),
        ('lv', N'Tumšas krāsas medus, kas iegūts no kastaņu ziediem, izceļas ar rūgteni saldenu garšu, augstu minerālvielu saturu un izteiktām bioaktīvām īpašībām.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Dark-colored honey produced from chestnut flowers, notable for its bitter-sweet taste, high mineral content, and strong bioactive properties.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Title', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Yayla Balı'),
        ('es', N'Miel de montaña'),
        ('pl', N'Miód górski'),
        ('lv', N'Augstienes medus')) v(Culture, Value)
WHERE r.HoneyTypeTitle = N'Highland Honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'HoneyType', CONVERT(nvarchar(100), r.HoneyTypeId), 'Description', v.Culture, v.Value
FROM dbo.HoneyTypes r
CROSS JOIN (VALUES
        ('tr', N'Yüksek rakımlı, çeşitli floradan üretilen bal; farklı bitkisel kaynaklar sayesinde genellikle aroma ve biyoaktif bileşenler bakımından zengindir.'),
        ('es', N'Miel producida a partir de una flora diversa de gran altitud, generalmente rica en aroma y compuestos bioactivos gracias a la variedad de sus fuentes botánicas.'),
        ('pl', N'Miód z różnorodnej flory wysokogórskiej, zazwyczaj bogaty w aromat i związki bioaktywne dzięki zróżnicowanym źródłom botanicznym.'),
        ('lv', N'Medus, kas iegūts no daudzveidīgas augstkalnu floras, parasti bagāts ar aromātu un bioaktīviem savienojumiem, pateicoties dažādiem botāniskajiem avotiem.')) v(Culture, Value)
WHERE r.HoneyTypeDescription = N'Honey produced from diverse high-altitude flora, generally rich in aroma and bioactive compounds due to varied botanical sources.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'HoneyType' AND t.EntityKey = CONVERT(nvarchar(100), r.HoneyTypeId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ProductionPeriod', CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId), 'Title', v.Culture, v.Value
FROM dbo.RawHoneyProductionPeriods r
CROSS JOIN (VALUES
        ('tr', N'2025–2026 Kekik Balı Hasat Sezonu'),
        ('es', N'Temporada de cosecha de miel de tomillo 2025–2026'),
        ('pl', N'Sezon zbioru miodu tymiankowego 2025–2026'),
        ('lv', N'2025.–2026. gada timiāna medus ievākšanas sezona')) v(Culture, Value)
WHERE r.RawHoneyProductionPeriodTitle = N'2025–2026 Thyme Honey Harvest Season'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ProductionPeriod' AND t.EntityKey = CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ProductionPeriod', CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId), 'Title', v.Culture, v.Value
FROM dbo.RawHoneyProductionPeriods r
CROSS JOIN (VALUES
        ('tr', N'2025–2026 Ayçiçeği Balı Hasat Sezonu'),
        ('es', N'Temporada de cosecha de miel de girasol 2025–2026'),
        ('pl', N'Sezon zbioru miodu słonecznikowego 2025–2026'),
        ('lv', N'2025.–2026. gada saulespuķu medus ievākšanas sezona')) v(Culture, Value)
WHERE r.RawHoneyProductionPeriodTitle = N'2025–2026 Sunflower Honey Harvest Season'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ProductionPeriod' AND t.EntityKey = CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ProductionPeriod', CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId), 'Title', v.Culture, v.Value
FROM dbo.RawHoneyProductionPeriods r
CROSS JOIN (VALUES
        ('tr', N'2025–2026 Narenciye Balı Hasat Sezonu'),
        ('es', N'Temporada de cosecha de miel de cítricos 2025–2026'),
        ('pl', N'Sezon zbioru miodu cytrusowego 2025–2026'),
        ('lv', N'2025.–2026. gada citrusaugu medus ievākšanas sezona')) v(Culture, Value)
WHERE r.RawHoneyProductionPeriodTitle = N'2025–2026 Citrus Honey Harvest Season'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ProductionPeriod' AND t.EntityKey = CONVERT(nvarchar(100), r.RawHoneyProductionPeriodId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Ön Lisans'),
        ('es', N'Título técnico (ciclo corto)'),
        ('pl', N'Wykształcenie pomaturalne (dyplom zawodowy)'),
        ('lv', N'Īsā cikla augstākā izglītība')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Associate Degree'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Lisans'),
        ('es', N'Licenciatura / grado'),
        ('pl', N'Licencjat'),
        ('lv', N'Bakalaura grāds')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Bachelor''s Degree'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Doktora'),
        ('es', N'Doctorado (PhD)'),
        ('pl', N'Doktorat (PhD)'),
        ('lv', N'Doktora grāds (PhD)')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Doctorate (PhD)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Lise'),
        ('es', N'Educación secundaria superior'),
        ('pl', N'Szkoła średnia'),
        ('lv', N'Vidusskola')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'High School'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Okuryazar değil'),
        ('es', N'Analfabeto'),
        ('pl', N'Niepiśmienny'),
        ('lv', N'Analfabēts')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Illiterate'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Okuryazar'),
        ('es', N'Alfabetizado'),
        ('pl', N'Piśmienny'),
        ('lv', N'Lasītprasmīgs')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Literate'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Yüksek Lisans'),
        ('es', N'Maestría / máster'),
        ('pl', N'Magisterium'),
        ('lv', N'Maģistra grāds')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Master''s Degree'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'İlkokul'),
        ('es', N'Educación primaria'),
        ('pl', N'Szkoła podstawowa'),
        ('lv', N'Sākumskola')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Primary School'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'EducationLevel', CONVERT(nvarchar(100), r.BeekeeperEducationLevelId), 'Title', v.Culture, v.Value
FROM dbo.BeekeeperEducationLevels r
CROSS JOIN (VALUES
        ('tr', N'Ortaokul'),
        ('es', N'Educación secundaria'),
        ('pl', N'Gimnazjum'),
        ('lv', N'Pamatskola')) v(Culture, Value)
WHERE r.BeekeeperEducationLevelTitle = N'Secondary School'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'EducationLevel' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeeperEducationLevelId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Balmumu Üretimi'),
        ('es', N'Producción de cera de abejas'),
        ('pl', N'Produkcja wosku pszczelego'),
        ('lv', N'Vaska ražošana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Beeswax Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Bal Üretimi'),
        ('es', N'Producción de miel'),
        ('pl', N'Produkcja miodu'),
        ('lv', N'Medus ražošana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Honey Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Polen Üretimi'),
        ('es', N'Producción de polen'),
        ('pl', N'Produkcja pyłku'),
        ('lv', N'Ziedputekšņu ražošana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Pollen Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Tozlaşma Hizmetleri'),
        ('es', N'Servicios de polinización'),
        ('pl', N'Usługi zapylania'),
        ('lv', N'Apputeksnēšanas pakalpojumi')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Pollination Services'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Propolis Üretimi'),
        ('es', N'Producción de propóleos'),
        ('pl', N'Produkcja propolisu'),
        ('lv', N'Propolisa ražošana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Propolis Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Ana Arı Yetiştiriciliği'),
        ('es', N'Cría de abejas reinas'),
        ('pl', N'Hodowla matek pszczelich'),
        ('lv', N'Bišu māšu audzēšana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Queen Bee Rearing'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingPurposeType', CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingPurposeTypes r
CROSS JOIN (VALUES
        ('tr', N'Arı Sütü Üretimi'),
        ('es', N'Producción de jalea real'),
        ('pl', N'Produkcja mleczka pszczelego'),
        ('lv', N'Bišu peru piena ražošana')) v(Culture, Value)
WHERE r.BeekeepingPurposeTypeTitle = N'Royal Jelly Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingPurposeType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingPurposeTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Ticari Arıcılık'),
        ('es', N'Apicultura comercial'),
        ('pl', N'Pszczelarstwo towarowe'),
        ('lv', N'Komercbiškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Commercial Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Geleneksel Arıcılık'),
        ('es', N'Apicultura convencional'),
        ('pl', N'Pszczelarstwo konwencjonalne'),
        ('lv', N'Konvencionālā biškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Conventional Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Hobi Arıcılığı'),
        ('es', N'Apicultura recreativa'),
        ('pl', N'Pszczelarstwo hobbystyczne'),
        ('lv', N'Hobija biškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Hobby Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Gezginci Arıcılık'),
        ('es', N'Apicultura trashumante'),
        ('pl', N'Pszczelarstwo wędrowne'),
        ('lv', N'Klejojošā biškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Migratory Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Organik Arıcılık'),
        ('es', N'Apicultura orgánica'),
        ('pl', N'Pszczelarstwo ekologiczne'),
        ('lv', N'Bioloģiskā biškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Organic Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'BeekeepingType', CONVERT(nvarchar(100), r.BeekeepingTypeId), 'Title', v.Culture, v.Value
FROM dbo.BeekeepingTypes r
CROSS JOIN (VALUES
        ('tr', N'Sabit Arıcılık'),
        ('es', N'Apicultura estacionaria'),
        ('pl', N'Pszczelarstwo stacjonarne'),
        ('lv', N'Stacionārā biškopība')) v(Culture, Value)
WHERE r.BeekeepingTypeTitle = N'Stationary Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'BeekeepingType' AND t.EntityKey = CONVERT(nvarchar(100), r.BeekeepingTypeId) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Atandı'),
        ('es', N'Asignado'),
        ('pl', N'Przypisany'),
        ('lv', N'Piešķirts')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'Assigned'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Tamamlandı'),
        ('es', N'Completado'),
        ('pl', N'Zakończony'),
        ('lv', N'Pabeigts')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'Completed'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Devam ediyor'),
        ('es', N'En curso'),
        ('pl', N'W toku'),
        ('lv', N'Procesā')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'In Progress'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Uygulanamaz'),
        ('es', N'No aplicable'),
        ('pl', N'Nie dotyczy'),
        ('lv', N'Nav piemērojams')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'Not applicable'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Reddedildi'),
        ('es', N'Rechazado'),
        ('pl', N'Odrzucony'),
        ('lv', N'Noraidīts')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'Rejected'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'AssignmentStatus', CONVERT(nvarchar(100), r.AssignmentStatusId), 'Name', v.Culture, v.Value
FROM dbo.AssignmentStatuses r
CROSS JOIN (VALUES
        ('tr', N'Gönderildi'),
        ('es', N'Enviado'),
        ('pl', N'Przesłany'),
        ('lv', N'Iesniegts')) v(Culture, Value)
WHERE r.AssignmentStatusName = N'Submitted'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'AssignmentStatus' AND t.EntityKey = CONVERT(nvarchar(100), r.AssignmentStatusId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Birlik'),
        ('es', N'Asociación'),
        ('pl', N'Stowarzyszenie'),
        ('lv', N'Biedrība')) v(Culture, Value)
WHERE r.Code = N'Association'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Arıcı birliği; üye arıcıları adına veri girer (KKN 0)'),
        ('es', N'Asociación de apicultores; registra datos en nombre de sus apicultores miembros (PCC 0)'),
        ('pl', N'Stowarzyszenie pszczelarzy; rejestruje dane w imieniu pszczelarzy członkowskich (KPK 0)'),
        ('lv', N'Biškopju biedrība; reģistrē datus savu biedru biškopju vārdā (KKP 0)')) v(Culture, Value)
WHERE r.Description = N'Beekeeper association; records data for its member beekeepers (CCP 0)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Arıcılık'),
        ('es', N'Apicultura'),
        ('pl', N'Pszczelarstwo'),
        ('lv', N'Biškopība')) v(Culture, Value)
WHERE r.Code = N'Beekeeping'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Arıcılık işletmesi (giriş yok; kayıt numarası zorunlu)'),
        ('es', N'Explotación apícola (sin acceso; se requiere número de registro)'),
        ('pl', N'Gospodarstwo pszczelarskie (bez logowania; wymagany numer w rejestrze)'),
        ('lv', N'Biškopības saimniecība (bez pieteikšanās; nepieciešams reģistrācijas numurs)')) v(Culture, Value)
WHERE r.Description = N'Beekeeping enterprise (no login; registry number required)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Merkez Birlik'),
        ('es', N'Asociación central'),
        ('pl', N'Związek centralny'),
        ('lv', N'Centrālā biedrība')) v(Culture, Value)
WHERE r.Code = N'CentralAssociation'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Merkez arıcılar birliği'),
        ('es', N'Asociación central de apicultores'),
        ('pl', N'Centralny związek pszczelarzy'),
        ('lv', N'Centrālā biškopju biedrība')) v(Culture, Value)
WHERE r.Description = N'Central beekeepers'' association'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Laboratuvar'),
        ('es', N'Laboratorio'),
        ('pl', N'Laboratorium'),
        ('lv', N'Laboratorija')) v(Culture, Value)
WHERE r.Code = N'Laboratory'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Laboratuvar (akredite veya kurum içi)'),
        ('es', N'Laboratorio (acreditado o propio)'),
        ('pl', N'Laboratorium (akredytowane lub zakładowe)'),
        ('lv', N'Laboratorija (akreditēta vai iekšējā)')) v(Culture, Value)
WHERE r.Description = N'Laboratory (accredited or in-house)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Paketleme'),
        ('es', N'Envasado'),
        ('pl', N'Konfekcjonowanie'),
        ('lv', N'Iepakošana')) v(Culture, Value)
WHERE r.Code = N'Packing'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Dolum, etiketleme ve paketleme, KKN 7-9'),
        ('es', N'Llenado, etiquetado y envasado, PCC 7-9'),
        ('pl', N'Rozlew, etykietowanie i pakowanie, KPK 7-9'),
        ('lv', N'Pildīšana, marķēšana un iepakošana, KKP 7–9')) v(Culture, Value)
WHERE r.Description = N'Filling, labelling and packaging, CCP 7-9'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Platform Yöneticisi'),
        ('es', N'Administrador de la plataforma'),
        ('pl', N'Administrator platformy'),
        ('lv', N'Platformas administrators')) v(Culture, Value)
WHERE r.Code = N'PlatformAdmin'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Platformu işletir'),
        ('es', N'Opera la plataforma'),
        ('pl', N'Prowadzi platformę'),
        ('lv', N'Uztur platformu')) v(Culture, Value)
WHERE r.Description = N'Runs the platform'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'İşleme'),
        ('es', N'Procesamiento'),
        ('pl', N'Przetwarzanie'),
        ('lv', N'Pārstrāde')) v(Culture, Value)
WHERE r.Code = N'Processing'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Kabul ve işleme, KKN 1-6 (bir birlik veya bir paketleyici)'),
        ('es', N'Recepción y procesamiento, PCC 1-6 (una asociación o un envasador)'),
        ('pl', N'Przyjęcie i przetwarzanie, KPK 1-6 (stowarzyszenie lub firma konfekcjonująca)'),
        ('lv', N'Pieņemšana un pārstrāde, KKP 1–6 (biedrība vai iepakotājs)')) v(Culture, Value)
WHERE r.Description = N'Reception and processing, CCP 1-6 (an association or a packer)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Kamu Otoritesi'),
        ('es', N'Autoridad pública'),
        ('pl', N'Organ publiczny'),
        ('lv', N'Valsts iestāde')) v(Culture, Value)
WHERE r.Code = N'PublicAuthority'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Kamu otoritesi, salt okunur'),
        ('es', N'Autoridad pública, solo lectura'),
        ('pl', N'Organ publiczny, tylko odczyt'),
        ('lv', N'Valsts iestāde, tikai lasīšanas piekļuve')) v(Culture, Value)
WHERE r.Description = N'Public authority, read-only'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Perakende'),
        ('es', N'Comercio minorista'),
        ('pl', N'Handel detaliczny'),
        ('lv', N'Mazumtirdzniecība')) v(Culture, Value)
WHERE r.Code = N'Retail'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Perakende; ürün elindeyken KKN 10''da emanetçi'),
        ('es', N'Comercio minorista; poseedor en el PCC 10 mientras tiene la mercancía'),
        ('pl', N'Handel detaliczny; posiadacz w KPK 10, dopóki ma towar'),
        ('lv', N'Mazumtirdzniecība; turētājs KKP 10, kamēr preces ir tās rīcībā')) v(Culture, Value)
WHERE r.Description = N'Retail; holder at CCP 10 while it has the goods'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Toptan Satış'),
        ('es', N'Comercio mayorista'),
        ('pl', N'Handel hurtowy'),
        ('lv', N'Vairumtirdzniecība')) v(Culture, Value)
WHERE r.Code = N'Wholesale'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Capability', CONVERT(nvarchar(100), r.Code), 'Description', v.Culture, v.Value
FROM dbo.Capabilities r
CROSS JOIN (VALUES
        ('tr', N'Toptan satış; ürün elindeyken KKN 10''da emanetçi'),
        ('es', N'Comercio mayorista; poseedor en el PCC 10 mientras tiene la mercancía'),
        ('pl', N'Handel hurtowy; posiadacz w KPK 10, dopóki ma towar'),
        ('lv', N'Vairumtirdzniecība; turētājs KKP 10, kamēr preces ir tās rīcībā')) v(Culture, Value)
WHERE r.Description = N'Wholesale; holder at CCP 10 while it has the goods'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Capability' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Analist'),
        ('es', N'Analista'),
        ('pl', N'Analityk'),
        ('lv', N'Analītiķis')) v(Culture, Value)
WHERE r.Code = N'Analyst'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Saha denetçisi'),
        ('es', N'Inspector de campo'),
        ('pl', N'Inspektor terenowy'),
        ('lv', N'Lauka inspektors')) v(Culture, Value)
WHERE r.Code = N'FieldInspector'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Laboratuvar müdürü'),
        ('es', N'Responsable de laboratorio'),
        ('pl', N'Kierownik laboratorium'),
        ('lv', N'Laboratorijas vadītājs')) v(Culture, Value)
WHERE r.Code = N'LabManager'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Kurum yöneticisi'),
        ('es', N'Administrador de la organización'),
        ('pl', N'Administrator organizacji'),
        ('lv', N'Organizācijas administrators')) v(Culture, Value)
WHERE r.Code = N'OrgAdmin'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Kalite onaylayıcı'),
        ('es', N'Aprobador de calidad'),
        ('pl', N'Zatwierdzający jakość'),
        ('lv', N'Kvalitātes apstiprinātājs')) v(Culture, Value)
WHERE r.Code = N'QualityApprover'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Numune kabul sorumlusu'),
        ('es', N'Responsable de recepción de muestras'),
        ('pl', N'Pracownik przyjmujący próbki'),
        ('lv', N'Paraugu pieņemšanas speciālists')) v(Culture, Value)
WHERE r.Code = N'SampleAcceptanceOfficer'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'OrgRole', CONVERT(nvarchar(100), r.Code), 'Name', v.Culture, v.Value
FROM dbo.OrgRoles r
CROSS JOIN (VALUES
        ('tr', N'Personel'),
        ('es', N'Personal'),
        ('pl', N'Personel'),
        ('lv', N'Darbinieks')) v(Culture, Value)
WHERE r.Code = N'Staff'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'OrgRole' AND t.EntityKey = CONVERT(nvarchar(100), r.Code) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ön Denetim'),
        ('es', N'Auditoría primaria'),
        ('pl', N'Audyt wstępny'),
        ('lv', N'Sākotnējais audits')) v(Culture, Value)
WHERE r.Title = N'Primary Audit'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Üretim Öncesi'),
        ('es', N'Preproducción'),
        ('pl', N'Etap przedprodukcyjny'),
        ('lv', N'Pirmsražošana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Pre-Production'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Kaynakta İHU''yu (İyi Hijyen Uygulamaları) sağlar'),
        ('es', N'Garantiza las BPH (Buenas Prácticas de Higiene) en el origen'),
        ('pl', N'Zapewnia stosowanie GHP (Dobrej Praktyki Higienicznej) u źródła'),
        ('lv', N'Nodrošina LHP (labas higiēnas prakses) ievērošanu izcelsmes vietā')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Ensures GHP (Good Hygiene Practices) at the source'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Numune Alma ve Ön Kabul Testleri'),
        ('es', N'Muestreo y pruebas preliminares de aceptación'),
        ('pl', N'Pobieranie próbek i wstępne badania odbiorcze'),
        ('lv', N'Paraugu ņemšana un sākotnējie pieņemšanas testi')) v(Culture, Value)
WHERE r.Title = N'Sampling and Preliminary Acceptance Tests'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ham Bal Kabulü'),
        ('es', N'Recepción de miel cruda'),
        ('pl', N'Przyjęcie surowego miodu'),
        ('lv', N'Neapstrādāta medus pieņemšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Raw Honey Reception'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Gelen balın, ileri işlemeden önce güvenlik ve kalite standartlarına uygunluğunu sağlamak'),
        ('es', N'Garantizar que la miel entrante cumpla las normas de inocuidad y calidad antes de su procesamiento posterior'),
        ('pl', N'Zapewnienie, że przyjmowany miód spełnia normy bezpieczeństwa i jakości przed dalszym przetwarzaniem'),
        ('lv', N'Lai nodrošinātu, ka ienākošais medus pirms turpmākas apstrādes atbilst nekaitīguma un kvalitātes standartiem')) v(Culture, Value)
WHERE r.CriticalityJustification = N'To ensure incoming honey complies with safety and quality standards before further processing'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Sıcaklık İzleme'),
        ('es', N'Monitoreo de la temperatura'),
        ('pl', N'Monitorowanie temperatury'),
        ('lv', N'Temperatūras uzraudzība')) v(Culture, Value)
WHERE r.Title = N'Temperature Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Isıtma'),
        ('es', N'Calentamiento'),
        ('pl', N'Podgrzewanie'),
        ('lv', N'Karsēšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Heating'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Kaliteyi bozabilecek veya yasal limitleri aşabilecek aşırı ısıtmayı önler'),
        ('es', N'Evita un calentamiento excesivo que pueda degradar la calidad o infringir los límites reglamentarios'),
        ('pl', N'Zapobiega nadmiernemu podgrzewaniu, które może obniżyć jakość lub naruszyć limity prawne'),
        ('lv', N'Novērš pārmērīgu karsēšanu, kas var pasliktināt kvalitāti vai pārkāpt normatīvās robežvērtības')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Prevents excessive heating which may degrade quality or violate regulatory limits'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Filtre Durumu Kontrolü'),
        ('es', N'Control del estado del filtro'),
        ('pl', N'Kontrola stanu filtra'),
        ('lv', N'Filtra stāvokļa kontrole')) v(Culture, Value)
WHERE r.Title = N'Filter Condition Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Süzme'),
        ('es', N'Filtración'),
        ('pl', N'Filtracja'),
        ('lv', N'Filtrēšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Filtration'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Safsızlıkların ve yabancı maddelerin etkin biçimde uzaklaştırılmasını sağlar'),
        ('es', N'Garantiza la eliminación eficaz de impurezas y materias extrañas'),
        ('pl', N'Zapewnia skuteczne usuwanie zanieczyszczeń i ciał obcych'),
        ('lv', N'Nodrošina efektīvu piemaisījumu un svešķermeņu atdalīšanu')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Ensures effective removal of impurities and foreign matter'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Homojenlik Kontrolü'),
        ('es', N'Control de la homogeneidad'),
        ('pl', N'Kontrola jednorodności'),
        ('lv', N'Viendabīguma kontrole')) v(Culture, Value)
WHERE r.Title = N'Homogeneity Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Homojenizasyon ve Karıştırma'),
        ('es', N'Homogeneización y mezcla'),
        ('pl', N'Homogenizacja i mieszanie'),
        ('lv', N'Homogenizēšana un sajaukšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Homogenization & Mixing'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ürün tekdüzeliğini ve tüketici kabul edilebilirliğini korur'),
        ('es', N'Mantiene la uniformidad del producto y su aceptación por el consumidor'),
        ('pl', N'Utrzymuje jednorodność produktu i jego akceptację przez konsumentów'),
        ('lv', N'Uztur produkta viendabīgumu un patērētāju akceptējamību')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Maintains product uniformity and consumer acceptability'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Süre ve Sıcaklık Kontrolü'),
        ('es', N'Control de tiempo y temperatura'),
        ('pl', N'Kontrola czasu i temperatury'),
        ('lv', N'Laika un temperatūras kontrole')) v(Culture, Value)
WHERE r.Title = N'Time and Temperature Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Pastörizasyon'),
        ('es', N'Pasteurización'),
        ('pl', N'Pasteryzacja'),
        ('lv', N'Pasterizācija')) v(Culture, Value)
WHERE r.ProcessingStep = N'Pasteurization'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Güvenliği ve kaliteyi etkileyebilecek yetersiz veya aşırı işlemeyi önler'),
        ('es', N'Evita un procesamiento insuficiente o excesivo que pueda afectar a la inocuidad y la calidad'),
        ('pl', N'Zapobiega niedostatecznemu lub nadmiernemu przetworzeniu, które może wpłynąć na bezpieczeństwo i jakość'),
        ('lv', N'Novērš nepietiekamu vai pārmērīgu apstrādi, kas var ietekmēt nekaitīgumu un kvalitāti')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Prevents under/over-processing which can affect safety and quality'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Köpük ve Tortu Giderme'),
        ('es', N'Eliminación de espuma y sedimentos'),
        ('pl', N'Usuwanie piany i osadu'),
        ('lv', N'Putu un nogulšņu noņemšana')) v(Culture, Value)
WHERE r.Title = N'Foam and Sediment Removal'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Dinlendirme (Durultma)'),
        ('es', N'Decantación (clarificación)'),
        ('pl', N'Odstawanie (klarowanie)'),
        ('lv', N'Nostādināšana (dzidrināšana)')) v(Culture, Value)
WHERE r.ProcessingStep = N'Settling (Clarification)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ürünün berraklığını ve görünümünü iyileştirir'),
        ('es', N'Mejora la limpidez y el aspecto del producto'),
        ('pl', N'Poprawia klarowność i wygląd produktu'),
        ('lv', N'Uzlabo produkta dzidrumu un izskatu')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Enhances product clarity and appearance'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Miktar Kontrolü'),
        ('es', N'Control de la cantidad'),
        ('pl', N'Kontrola ilości'),
        ('lv', N'Daudzuma kontrole')) v(Culture, Value)
WHERE r.Title = N'Quantity Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Dolum'),
        ('es', N'Llenado'),
        ('pl', N'Rozlew'),
        ('lv', N'Pildīšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Filling'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Doğru etiketlemeyi ve mevzuata uygunluğu sağlar'),
        ('es', N'Garantiza un etiquetado preciso y el cumplimiento normativo'),
        ('pl', N'Zapewnia prawidłowe etykietowanie i zgodność z przepisami'),
        ('lv', N'Nodrošina precīzu marķējumu un atbilstību normatīvajām prasībām')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Ensures accurate labeling and regulatory compliance'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Etiket Doğruluğu Kontrolü'),
        ('es', N'Verificación de la exactitud de la etiqueta'),
        ('pl', N'Weryfikacja poprawności etykiety'),
        ('lv', N'Marķējuma precizitātes pārbaude')) v(Culture, Value)
WHERE r.Title = N'Label Accuracy Verification'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Etiketleme'),
        ('es', N'Etiquetado'),
        ('pl', N'Etykietowanie'),
        ('lv', N'Marķēšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Labeling'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Yanlış etiketlemeyi ve tüketicinin yanlış bilgilendirilmesini önler'),
        ('es', N'Evita errores de etiquetado y la desinformación del consumidor'),
        ('pl', N'Zapobiega błędnemu etykietowaniu i wprowadzaniu konsumentów w błąd'),
        ('lv', N'Novērš nepareizu marķējumu un patērētāju maldināšanu')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Prevents mislabeling and consumer misinformation'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ambalaj Bütünlüğü Kontrolü'),
        ('es', N'Control de la integridad del envase'),
        ('pl', N'Kontrola integralności opakowania'),
        ('lv', N'Iepakojuma integritātes pārbaude')) v(Culture, Value)
WHERE r.Title = N'Packaging Integrity Check'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ambalajlama'),
        ('es', N'Envasado'),
        ('pl', N'Pakowanie'),
        ('lv', N'Iepakošana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Packaging'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ürünü kontaminasyondan korur ve raf ömrünü destekler'),
        ('es', N'Protege el producto de la contaminación y favorece su vida útil'),
        ('pl', N'Chroni produkt przed zanieczyszczeniem i wspiera trwałość'),
        ('lv', N'Aizsargā produktu no piesārņojuma un veicina tā glabāšanas laiku')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Protects product from contamination and supports shelf life'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'Title', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Sıcaklık ve Nem İzleme'),
        ('es', N'Monitoreo de temperatura y humedad'),
        ('pl', N'Monitorowanie temperatury i wilgotności'),
        ('lv', N'Temperatūras un mitruma uzraudzība')) v(Culture, Value)
WHERE r.Title = N'Temperature and Humidity Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'Title' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'ProcessingStep', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Depolama'),
        ('es', N'Almacenamiento'),
        ('pl', N'Przechowywanie'),
        ('lv', N'Uzglabāšana')) v(Culture, Value)
WHERE r.ProcessingStep = N'Storage'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'ProcessingStep' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Ccp', CONVERT(nvarchar(100), r.Id), 'CriticalityJustification', v.Culture, v.Value
FROM dbo.CriticalControlPoints r
CROSS JOIN (VALUES
        ('tr', N'Ürün stabilitesini korur ve mikrobiyal etkinliği engeller'),
        ('es', N'Preserva la estabilidad del producto e inhibe la actividad microbiana'),
        ('pl', N'Zachowuje stabilność produktu i hamuje aktywność mikrobiologiczną'),
        ('lv', N'Saglabā produkta stabilitāti un kavē mikrobiālo aktivitāti')) v(Culture, Value)
WHERE r.CriticalityJustification = N'Preserves product stability and inhibits microbial activity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Ccp' AND t.EntityKey = CONVERT(nvarchar(100), r.Id) AND t.Field = 'CriticalityJustification' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 0 Kovan Hijyeni ve Bakımı'),
        ('es', N'PCC 0 Higiene y mantenimiento de la colmena'),
        ('pl', N'KPK 0 Higiena i utrzymanie uli'),
        ('lv', N'KKP 0 Stropu higiēna un uzturēšana')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 0 Hive Hygiene and Maintenance'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Kovan Bileşenlerini İnceleme, Kovan Sağlığını Değerlendirme, Kovan Hijyeni Uygulamalarını Hayata Geçirme'),
        ('es', N'Inspeccionar los componentes de la colmena, evaluar la salud de la colmena, aplicar prácticas de higiene de la colmena'),
        ('pl', N'Kontrola elementów ula, ocena stanu zdrowia ula, stosowanie praktyk higieny ula'),
        ('lv', N'Stropu komponentu pārbaude, stropu veselības novērtēšana, stropu higiēnas prakses ieviešana')) v(Culture, Value)
WHERE r.CategoryDesc = N'Inspect Hive Components, Assess Hive Health, Implement Hive Hygiene Practices'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 0 Pestisit Kullanımı ve Kalıntı İzleme'),
        ('es', N'PCC 0 Uso de plaguicidas y monitoreo de residuos'),
        ('pl', N'KPK 0 Stosowanie pestycydów i monitorowanie pozostałości'),
        ('lv', N'KKP 0 Pesticīdu lietošana un atlieku uzraudzība')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 0 Pesticide Usage and Residue Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Pestisit Kullanımı ve Kalıntı İzleme'),
        ('es', N'Uso de plaguicidas y monitoreo de residuos'),
        ('pl', N'Stosowanie pestycydów i monitorowanie pozostałości'),
        ('lv', N'Pesticīdu lietošana un atlieku uzraudzība')) v(Culture, Value)
WHERE r.CategoryDesc = N'Pesticide Usage and Residue Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 0 Su Kalitesi ve Sanitasyon'),
        ('es', N'PCC 0 Calidad del agua y saneamiento'),
        ('pl', N'KPK 0 Jakość wody i warunki sanitarne'),
        ('lv', N'KKP 0 Ūdens kvalitāte un sanitārija')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 0 Water Quality and Sanitation'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Su Kalitesi ve Sanitasyon'),
        ('es', N'Calidad del agua y saneamiento'),
        ('pl', N'Jakość wody i warunki sanitarne'),
        ('lv', N'Ūdens kvalitāte un sanitārija')) v(Culture, Value)
WHERE r.CategoryDesc = N'Water Quality and Sanitation'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 0 Bal Hasadı ve Süzme Uygulamaları'),
        ('es', N'PCC 0 Prácticas de cosecha y extracción de miel'),
        ('pl', N'KPK 0 Praktyki miodobrania i wirowania miodu'),
        ('lv', N'KKP 0 Medus ievākšanas un iegūšanas prakse')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 0 Honey Harvesting & Extraction Practices'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Bal Hasadı ve Süzme Uygulamaları'),
        ('es', N'Prácticas de cosecha y extracción de miel'),
        ('pl', N'Praktyki miodobrania i wirowania miodu'),
        ('lv', N'Medus ievākšanas un iegūšanas prakse')) v(Culture, Value)
WHERE r.CategoryDesc = N'Honey Harvesting & Extraction Practices'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 0 Taşıma ve Depolama'),
        ('es', N'PCC 0 Transporte y almacenamiento'),
        ('pl', N'KPK 0 Transport i przechowywanie'),
        ('lv', N'KKP 0 Transportēšana un uzglabāšana')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 0 Transportation and Storage'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Taşıma ve Depolama'),
        ('es', N'Transporte y almacenamiento'),
        ('pl', N'Transport i przechowywanie'),
        ('lv', N'Transportēšana un uzglabāšana')) v(Culture, Value)
WHERE r.CategoryDesc = N'Transportation and Storage'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 1 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 1'),
        ('pl', N'KPK 1 Parametry monitorowania'),
        ('lv', N'KKP 1 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 1 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Kritik Kontrol Noktası 1 İzleme Parametreleri, Numune Alma ve Ön Kabul Testleri'),
        ('es', N'Parámetros de monitoreo del punto crítico de control 1, muestreo y pruebas preliminares de aceptación'),
        ('pl', N'Parametry monitorowania Krytycznego Punktu Kontroli 1, pobieranie próbek i wstępne badania odbiorcze'),
        ('lv', N'1. kritiskā kontroles punkta uzraudzības parametri, paraugu ņemšana un sākotnējie pieņemšanas testi')) v(Culture, Value)
WHERE r.CategoryDesc = N'Critical Control Point 1 Monitoring Parameters, Sampling and Preliminary Acceptance Tests'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 2 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 2'),
        ('pl', N'KPK 2 Parametry monitorowania'),
        ('lv', N'KKP 2 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 2 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Sıcaklık İzleme'),
        ('es', N'Monitoreo de la temperatura'),
        ('pl', N'Monitorowanie temperatury'),
        ('lv', N'Temperatūras uzraudzība')) v(Culture, Value)
WHERE r.CategoryDesc = N'Temperature Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 3 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 3'),
        ('pl', N'KPK 3 Parametry monitorowania'),
        ('lv', N'KKP 3 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 3 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Filtre Durumu Kontrolü'),
        ('es', N'Control del estado del filtro'),
        ('pl', N'Kontrola stanu filtra'),
        ('lv', N'Filtra stāvokļa kontrole')) v(Culture, Value)
WHERE r.CategoryDesc = N'Filter Condition Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 4 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 4'),
        ('pl', N'KPK 4 Parametry monitorowania'),
        ('lv', N'KKP 4 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 4 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Homojenlik Kontrolü'),
        ('es', N'Control de la homogeneidad'),
        ('pl', N'Kontrola jednorodności'),
        ('lv', N'Viendabīguma kontrole')) v(Culture, Value)
WHERE r.CategoryDesc = N'Homogeneity Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 5 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 5'),
        ('pl', N'KPK 5 Parametry monitorowania'),
        ('lv', N'KKP 5 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 5 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Süre ve Sıcaklık Kontrolü'),
        ('es', N'Control de tiempo y temperatura'),
        ('pl', N'Kontrola czasu i temperatury'),
        ('lv', N'Laika un temperatūras kontrole')) v(Culture, Value)
WHERE r.CategoryDesc = N'Time and Temperature Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 6 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 6'),
        ('pl', N'KPK 6 Parametry monitorowania'),
        ('lv', N'KKP 6 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 6 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Köpük ve Tortu Giderme'),
        ('es', N'Eliminación de espuma y sedimentos'),
        ('pl', N'Usuwanie piany i osadu'),
        ('lv', N'Putu un nogulšņu noņemšana')) v(Culture, Value)
WHERE r.CategoryDesc = N'Foam and Sediment Removal'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 7 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 7'),
        ('pl', N'KPK 7 Parametry monitorowania'),
        ('lv', N'KKP 7 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 7 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Miktar Kontrolü'),
        ('es', N'Control de la cantidad'),
        ('pl', N'Kontrola ilości'),
        ('lv', N'Daudzuma kontrole')) v(Culture, Value)
WHERE r.CategoryDesc = N'Quantity Control'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 8 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 8'),
        ('pl', N'KPK 8 Parametry monitorowania'),
        ('lv', N'KKP 8 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 8 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Etiket Doğruluğu Kontrolü'),
        ('es', N'Verificación de la exactitud de la etiqueta'),
        ('pl', N'Weryfikacja poprawności etykiety'),
        ('lv', N'Marķējuma precizitātes pārbaude')) v(Culture, Value)
WHERE r.CategoryDesc = N'Label Accuracy Verification'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 9 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 9'),
        ('pl', N'KPK 9 Parametry monitorowania'),
        ('lv', N'KKP 9 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 9 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Ambalaj Bütünlüğü Kontrolü'),
        ('es', N'Control de la integridad del envase'),
        ('pl', N'Kontrola integralności opakowania'),
        ('lv', N'Iepakojuma integritātes pārbaude')) v(Culture, Value)
WHERE r.CategoryDesc = N'Packaging Integrity Check'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'KKN 10 İzleme Parametreleri'),
        ('es', N'Parámetros de monitoreo del PCC 10'),
        ('pl', N'KPK 10 Parametry monitorowania'),
        ('lv', N'KKP 10 uzraudzības parametri')) v(Culture, Value)
WHERE r.CategoryName = N'CCP 10 Monitoring Parameters'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterCategory', CONVERT(nvarchar(100), r.CategoryId), 'Description', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterCategories r
CROSS JOIN (VALUES
        ('tr', N'Sıcaklık ve Nem İzleme'),
        ('es', N'Monitoreo de temperatura y humedad'),
        ('pl', N'Monitorowanie temperatury i wilgotności'),
        ('lv', N'Temperatūras un mitruma uzraudzība')) v(Culture, Value)
WHERE r.CategoryDesc = N'Temperature and Humidity Monitoring'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterCategory' AND t.EntityKey = CONVERT(nvarchar(100), r.CategoryId) AND t.Field = 'Description' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Kullanılmayan arıcılık alet ve ekipmanları iyice temizlendikten sonra mı depolanıyor?'),
        ('es', N'¿Se almacenan las herramientas y los equipos apícolas no utilizados después de una limpieza a fondo?'),
        ('pl', N'Czy nieużywane narzędzia i sprzęt pszczelarski są przechowywane po dokładnym oczyszczeniu?'),
        ('lv', N'Vai neizmantotie biškopības darbarīki un aprīkojums tiek uzglabāti pēc rūpīgas tīrīšanas?')) v(Culture, Value)
WHERE r.ParameterName = N'Are unused beekeeping tools and equipment stored after thorough cleaning?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Kovan kutuları, çerçeveler ve temel petekler kullanım sırasında yönetiliyor ve kontrol ediliyor mu?'),
        ('es', N'¿Se gestionan y controlan durante su uso las cajas de colmena, los cuadros y las láminas de cera?'),
        ('pl', N'Czy korpusy uli, ramki i węza są zarządzane i kontrolowane podczas użytkowania?'),
        ('lv', N'Vai stropu korpusi, rāmji un vaska pamatnes tiek pārvaldīti un kontrolēti lietošanas laikā?')) v(Culture, Value)
WHERE r.ParameterName = N'Are hive boxes, frames, and foundation materials managed and controlled during use?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Stressiz kontroller için arı körüğü gibi özel ekipmanlar kullanılıyor mu?'),
        ('es', N'¿Se utiliza equipo especializado, como ahumadores, para garantizar inspecciones sin estrés?'),
        ('pl', N'Czy stosuje się specjalistyczny sprzęt, np. podkurzacze, aby przeglądy przebiegały bez stresu dla pszczół?'),
        ('lv', N'Vai tiek izmantots specializēts aprīkojums, piemēram, dūmotāji, lai nodrošinātu bezstresa pārbaudes?')) v(Culture, Value)
WHERE r.ParameterName = N'Is specialized equipment, such as hive smokers, utilized to ensure stress-free inspections?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Koloni nüfusu, yavru düzeni ve zararlı varlığı sistematik olarak değerlendiriliyor mu?'),
        ('es', N'¿Se evalúan de forma sistemática la población de la colonia, los patrones de cría y la presencia de plagas?'),
        ('pl', N'Czy liczebność rodziny, układ czerwiu i obecność szkodników są systematycznie oceniane?'),
        ('lv', N'Vai saimes lielums, perus struktūra un kaitēkļu klātbūtne tiek sistemātiski novērtēta?')) v(Culture, Value)
WHERE r.ParameterName = N'Is the colony population, brood patterns, and pest presence assessed systematically?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Laboratuvar analizi için kovan döküntüsü, bal veya yavru numuneleri alınıyor mu?'),
        ('es', N'¿Se recogen muestras de residuos de la colmena, miel o cría para su análisis en laboratorio?'),
        ('pl', N'Czy pobiera się próbki osypu, miodu lub czerwiu do analizy laboratoryjnej?'),
        ('lv', N'Vai stropu gružu, medus vai peru paraugi tiek ņemti laboratoriskai analīzei?')) v(Culture, Value)
WHERE r.ParameterName = N'Are samples of hive debris, honey or brood collected for laboratory analysis?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('en', N'Are environmental factors (temperature and humidity) that influence hive health regularly monitored? How are they measured (traditionally with a simple thermometer/hygrometer, or with real-time data collection sensors)?'),
        ('tr', N'Kovan sağlığını etkileyen çevresel faktörler (sıcaklık ve nem) düzenli olarak izleniyor mu? Nasıl ölçülüyor (geleneksel olarak basit bir termometre/higrometre ile mi, yoksa gerçek zamanlı veri toplayan sensörlerle mi)?'),
        ('es', N'¿Se monitorean regularmente los factores ambientales (temperatura y humedad) que influyen en la salud de la colmena? ¿Cómo se miden (de forma tradicional con un simple termómetro/higrómetro, o con sensores de recopilación de datos en tiempo real)?'),
        ('pl', N'Czy czynniki środowiskowe (temperatura i wilgotność) wpływające na zdrowie ula są regularnie monitorowane? W jaki sposób są mierzone (tradycyjnie prostym termometrem/higrometrem czy czujnikami zbierającymi dane w czasie rzeczywistym)?'),
        ('lv', N'Vai vides faktori (temperatūra un mitrums), kas ietekmē stropu veselību, tiek regulāri uzraudzīti? Kā tie tiek mērīti (tradicionāli ar vienkāršu termometru/higrometru vai ar reāllaika datu vākšanas sensoriem)?')) v(Culture, Value)
WHERE r.ParameterName = N'Are environmental factors (temperature and humidity) that influence hive health regularly monitored? How is it measured (tradional with a simple thermometer/ hygrometer or real time data collection sensors?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Kovan bileşenleri düzenli olarak temizleniyor ve dezenfekte ediliyor mu?'),
        ('es', N'¿Se limpian y desinfectan regularmente los componentes de la colmena?'),
        ('pl', N'Czy elementy ula są regularnie czyszczone i dezynfekowane?'),
        ('lv', N'Vai stropu komponenti tiek regulāri tīrīti un dezinficēti?')) v(Culture, Value)
WHERE r.ParameterName = N'Are hive components regularly cleaned and disinfected?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Evet ise, kovan alet ve ekipmanlarını temizlemek ve dezenfekte etmek için hangi yöntemler kullanılıyor?'),
        ('es', N'En caso afirmativo, ¿qué métodos se emplean para limpiar y desinfectar las herramientas y los equipos de la colmena?'),
        ('pl', N'Jeśli tak, jakie metody stosuje się do czyszczenia i dezynfekcji narzędzi i sprzętu pszczelarskiego?'),
        ('lv', N'Ja jā, kādas metodes tiek izmantotas stropu darbarīku un aprīkojuma tīrīšanai un dezinfekcijai?')) v(Culture, Value)
WHERE r.ParameterName = N'If yes, which methods are employed to clean and disinfect hive tools and equipment?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Kovan hijyeni, bakımı ve kovan sağlığına ilişkin kayıtlar özenle tutuluyor mu?'),
        ('es', N'¿Se llevan con rigor registros de higiene, mantenimiento y salud de las colmenas?'),
        ('pl', N'Czy dokumentacja higieny, utrzymania i zdrowia uli jest starannie prowadzona?'),
        ('lv', N'Vai tiek rūpīgi uzskaitīti ieraksti par stropu higiēnu, uzturēšanu un stropu veselību?')) v(Culture, Value)
WHERE r.ParameterName = N'Are records of hive hygiene, maintenance and hive health diligently kept?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Pestisit kullanımına ilişkin kapsamlı risk değerlendirmeleri yapılıyor mu?'),
        ('es', N'¿Se realizan evaluaciones de riesgos exhaustivas sobre el uso de plaguicidas?'),
        ('pl', N'Czy przeprowadza się kompleksowe oceny ryzyka dotyczące stosowania pestycydów?'),
        ('lv', N'Vai tiek veikti visaptveroši riska novērtējumi attiecībā uz pesticīdu lietošanu?')) v(Culture, Value)
WHERE r.ParameterName = N'Are comprehensive risk assessments conducted regarding pesticide usage?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Yasal olarak izin verilen pestisitlerin, özellikle varroa mücadelesinde, uygulama zamanına, dozuna ve sıklığına dikkat ediliyor mu?'),
        ('es', N'¿Se presta especial atención al momento, la dosis y la frecuencia de aplicación de los plaguicidas legalmente permitidos, en particular para el control de la varroa?'),
        ('pl', N'Czy zwraca się szczególną uwagę na termin, dawkowanie i częstotliwość stosowania dopuszczonych prawem pestycydów, zwłaszcza w zwalczaniu warrozy?'),
        ('lv', N'Vai tiek pievērsta rūpīga uzmanība likumīgi atļauto pesticīdu lietošanas laikam, devai un biežumam, īpaši varrozes kontrolei?')) v(Culture, Value)
WHERE r.ParameterName = N'Is careful attention given to the timing, dosage and frequency of legally permitted pesticides, particularly for varroa control?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Zararlı yönetiminde mümkün olduğunca kimyasal olmayan mücadele yöntemleri kullanılıyor mu?'),
        ('es', N'¿Se utilizan métodos de control no químicos para el manejo de plagas siempre que sea posible?'),
        ('pl', N'Czy w miarę możliwości stosuje się niechemiczne metody zwalczania szkodników?'),
        ('lv', N'Vai kaitēkļu apkarošanai, kad vien iespējams, tiek izmantotas neķīmiskas kontroles metodes?')) v(Culture, Value)
WHERE r.ParameterName = N'Are non-chemical control methods utilized for pest management whenever possible?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bal üretiminde kullanılan suyun mikrobiyolojik, kimyasal ve fiziksel özellikler bakımından uygunluğu akredite laboratuvar testleriyle ne ölçüde doğrulanıyor ve bu testler ne sıklıkla yapılıyor?'),
        ('es', N'¿En qué medida se verifica mediante análisis de laboratorios acreditados la idoneidad del agua utilizada en la producción de miel en cuanto a sus propiedades microbiológicas, químicas y físicas, y con qué frecuencia se realizan estos análisis?'),
        ('pl', N'W jakim zakresie przydatność wody używanej w produkcji miodu pod względem właściwości mikrobiologicznych, chemicznych i fizycznych jest weryfikowana badaniami w akredytowanym laboratorium i jak często przeprowadza się te badania?'),
        ('lv', N'Cik lielā mērā medus ražošanā izmantotā ūdens piemērotība attiecībā uz tā mikrobioloģiskajām, ķīmiskajām un fizikālajām īpašībām tiek pārbaudīta akreditētās laboratorijas testos, un cik bieži šie testi tiek veikti?')) v(Culture, Value)
WHERE r.ParameterName = N'To what extent is the suitability of water used in honey production, in terms of its microbiological, chemical, and physical properties, verified through accredited laboratory tests, and how frequently are these tests conducted?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Ekipman temizliğinde kullanılan suyun uygunluğu değerlendirilip teyit ediliyor mu?'),
        ('es', N'¿Se evalúa y se confirma la idoneidad del agua utilizada para limpiar los equipos?'),
        ('pl', N'Czy woda używana do czyszczenia sprzętu jest oceniana i potwierdzana jako odpowiednia?'),
        ('lv', N'Vai aprīkojuma tīrīšanai izmantotais ūdens tiek novērtēts un apstiprināts kā piemērots?')) v(Culture, Value)
WHERE r.ParameterName = N'Is the water used for cleaning equipment assessed and confirmed to be suitable?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Arı beslemesi için şekerli su çözeltilerinin hazırlanmasında hammadde olarak kullanılan suyun kalitesi uygun mu?'),
        ('es', N'¿Es adecuada la calidad del agua utilizada como materia prima para preparar las soluciones de agua azucarada para la alimentación de las abejas?'),
        ('pl', N'Czy jakość wody używanej jako surowiec do przygotowania syropu cukrowego do karmienia pszczół jest odpowiednia?'),
        ('lv', N'Vai bišu barošanai paredzēto cukura šķīdumu pagatavošanā kā izejviela izmantotā ūdens kvalitāte ir atbilstoša?')) v(Culture, Value)
WHERE r.ParameterName = N'Is the water quality used as raw material in the production of sugar water solutions for bee feeding appropriate?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bal hasadının zamanlaması, balın olgunluk göstergelerine ve çiçek nektarı kaynaklarına göre en uygun şekilde belirleniyor mu?'),
        ('es', N'¿Se optimiza el momento de la cosecha de miel en función de los indicadores de madurez de la miel y de las fuentes de néctar floral?'),
        ('pl', N'Czy termin miodobrania jest optymalizowany na podstawie wskaźników dojrzałości miodu i źródeł nektaru?'),
        ('lv', N'Vai medus ievākšanas laiks tiek optimizēts, pamatojoties uz medus gatavības rādītājiem un ziedu nektāra avotiem?')) v(Culture, Value)
WHERE r.ParameterName = N'Is the timing of honey harvesting optimized based on honey ripeness indicators and the sources of floral nectar?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bal kalitesini etkileyen fiziksel tehlike kaynakları (metal parçalar veya metal olmayan; tahta, taş, cam vb.) izleniyor mu?'),
        ('es', N'¿Se monitorean las fuentes de peligros físicos (fragmentos metálicos o no metálicos; madera, piedra, vidrio, etc.) que afectan a la calidad de la miel?'),
        ('pl', N'Czy monitoruje się źródła zagrożeń fizycznych (fragmenty metalowe lub niemetalowe; drewno, kamień, szkło itp.) wpływających na jakość miodu?'),
        ('lv', N'Vai tiek uzraudzīti fizikālo apdraudējumu avoti (metāla vai nemetāla daļiņas; koks, akmens, stikls u. c.), kas ietekmē medus kvalitāti?')) v(Culture, Value)
WHERE r.ParameterName = N'Are physical hazards sources (metal fragments or non-metal; wood, stone, glass, etc) which affect honey quality monitored?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bal kalitesini etkileyen biyolojik tehlike kaynakları (çevresel faktörlerden gelen bakteri, maya ve küf bulaşma etkenleri) izleniyor mu?'),
        ('es', N'¿Se monitorean las fuentes de peligros biológicos (agentes de contaminación bacteriana, por levaduras y mohos procedentes de factores ambientales) que afectan a la calidad de la miel?'),
        ('pl', N'Czy monitoruje się źródła zagrożeń biologicznych (czynniki skażenia bakteriami, drożdżami i pleśniami pochodzące ze środowiska) wpływających na jakość miodu?'),
        ('lv', N'Vai tiek uzraudzīti bioloģisko apdraudējumu avoti (baktēriju, raugu un pelējuma piesārņojuma izraisītāji no vides faktoriem), kas ietekmē medus kvalitāti?')) v(Culture, Value)
WHERE r.ParameterName = N'Are biological hazards sources (bacterial, yeast and mold contamination agent from environmental factors) which affect honey quality monitored?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('en', N'Are potential sources of chemical hazards (heavy metals, cleaning chemicals, chemicals from cleaning water, medicines and antibiotics, bee repellents or other contaminants of environmental origin) that affect honey quality tracked?'),
        ('tr', N'Bal kalitesini etkileyen olası kimyasal tehlike kaynakları (ağır metaller, temizlik kimyasalları, temizlik suyundan gelen kimyasallar, ilaçlar ve antibiyotikler, arı kaçırıcılar veya çevresel kaynaklı diğer kirleticiler) takip ediliyor mu?'),
        ('es', N'¿Se realiza un seguimiento de las posibles fuentes de peligros químicos (metales pesados, productos químicos de limpieza, sustancias químicas del agua de limpieza, medicamentos y antibióticos, repelentes de abejas u otros contaminantes de origen ambiental) que afectan a la calidad de la miel?'),
        ('pl', N'Czy śledzi się potencjalne źródła zagrożeń chemicznych (metale ciężkie, chemiczne środki czyszczące, chemikalia z wody do mycia, leki i antybiotyki, repelenty dla pszczół lub inne zanieczyszczenia pochodzenia środowiskowego) wpływających na jakość miodu?'),
        ('lv', N'Vai tiek izsekoti iespējamie ķīmisko apdraudējumu avoti (smagie metāli, tīrīšanas ķimikālijas, ķimikālijas no tīrīšanas ūdens, zāles un antibiotikas, bišu atbaidīšanas līdzekļi vai citi vides izcelsmes piesārņotāji), kas ietekmē medus kvalitāti?')) v(Culture, Value)
WHERE r.ParameterName = N'Are potantial chemical hazards sources (heavy metals, cleaning chemicals, chemicals from cleaning water, medicines and antibiotics, bee repellents or other environmental origin contaminants) that affect honey quality tracked?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Taşıma ve depolama sistemleri, olumsuz çevresel etkenlere maruziyeti en aza indirecek şekilde tasarlanmış mı?'),
        ('es', N'¿Están diseñados los sistemas de transporte y almacenamiento para minimizar la exposición a factores ambientales adversos?'),
        ('pl', N'Czy systemy transportu i przechowywania są zaprojektowane tak, aby ograniczać narażenie na niekorzystne czynniki środowiskowe?'),
        ('lv', N'Vai transportēšanas un uzglabāšanas sistēmas ir izveidotas tā, lai samazinātu nelabvēlīgu vides faktoru ietekmi?')) v(Culture, Value)
WHERE r.ParameterName = N'Are transportation and storage systems designed to minimize exposure to adverse environmental factors?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Taşıma ve depolama sırasında sıcaklık ve nem gibi koşulları izleyen sistemler var mı?'),
        ('es', N'¿Existen sistemas de monitoreo para registrar condiciones como la temperatura y la humedad durante el transporte y el almacenamiento?'),
        ('pl', N'Czy istnieją systemy monitorowania warunków, takich jak temperatura i wilgotność, podczas transportu i przechowywania?'),
        ('lv', N'Vai ir ieviestas uzraudzības sistēmas, lai transportēšanas un uzglabāšanas laikā izsekotu tādus apstākļus kā temperatūra un mitrums?')) v(Culture, Value)
WHERE r.ParameterName = N'Are monitoring systems in place to track conditions such as temperature and humidity during transportation and storage?'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Nem'),
        ('es', N'Humedad'),
        ('pl', N'Wilgotność'),
        ('lv', N'Mitrums')) v(Culture, Value)
WHERE r.ParameterName = N'Moisture'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'% (Yüzde)'),
        ('es', N'% (porcentaje)'),
        ('pl', N'% (procent)'),
        ('lv', N'% (procenti)')) v(Culture, Value)
WHERE r.ParameterUnit = N'% (Percentage)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Çam balı: en çok 23 (şüpheli: AB 2001/110''da %23 istisnası funda / Calluna balı içindir; doğrulanacak)'),
        ('es', N'Miel de pino: máx. 23 (dudoso: en la UE 2001/110 la excepción del 23 % corresponde a la miel de brezo / Calluna; por verificar)'),
        ('pl', N'Miód sosnowy: maks. 23 (wątpliwe: w EU 2001/110 wyjątek 23 % dotyczy miodu wrzosowego / Calluna; do weryfikacji)'),
        ('lv', N'Priežu medus: maks. 23 (apšaubāmi: ES 2001/110 23 % izņēmums attiecas uz virsāju / Calluna medu; jāpārbauda)')) v(Culture, Value)
WHERE r.LimitNote = N'Pine honey: max 23 (doubtful: in EU 2001/110 the 23 % exception is for heather / Calluna honey; to verify)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitSource', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Türk Gıda Kodeksi Bal Tebliği (başlangıç verisi, doğrulanacak)'),
        ('es', N'Código Alimentario Turco, comunicado sobre la miel (valor inicial, por verificar)'),
        ('pl', N'Turecki Kodeks Żywnościowy, komunikat w sprawie miodu (dane początkowe, do weryfikacji)'),
        ('lv', N'Turcijas Pārtikas kodekss, medus paziņojums (sākotnējie dati, jāpārbauda)')) v(Culture, Value)
WHERE r.LimitSource = N'Turkish Food Codex, honey communique (seed, to verify)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitSource' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'HMF'),
        ('es', N'HMF'),
        ('pl', N'HMF'),
        ('lv', N'HMF')) v(Culture, Value)
WHERE r.ParameterName = N'HMF'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'mg/kg'),
        ('es', N'mg/kg'),
        ('pl', N'mg/kg'),
        ('lv', N'mg/kg')) v(Culture, Value)
WHERE r.ParameterUnit = N'mg/kg'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Diastaz'),
        ('es', N'Diastasa'),
        ('pl', N'Diastaza'),
        ('lv', N'Diastāze')) v(Culture, Value)
WHERE r.ParameterName = N'Diastase'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'DN (Schade)'),
        ('es', N'DN (Schade)'),
        ('pl', N'DN (Schade)'),
        ('lv', N'DN (Schade)')) v(Culture, Value)
WHERE r.ParameterUnit = N'DN (Schade)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'HMF <= 15 mg/kg ise en az 3'),
        ('es', N'Mín. 3 si HMF <= 15 mg/kg'),
        ('pl', N'Min. 3, jeśli HMF <= 15 mg/kg'),
        ('lv', N'Min. 3, ja HMF <= 15 mg/kg')) v(Culture, Value)
WHERE r.LimitNote = N'Min 3 if HMF <= 15 mg/kg'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'C4 Şekeri'),
        ('es', N'Azúcar C4'),
        ('pl', N'Cukier C4'),
        ('lv', N'C4 cukurs')) v(Culture, Value)
WHERE r.ParameterName = N'C4 Sugar'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'%'),
        ('es', N'%'),
        ('pl', N'%'),
        ('lv', N'%')) v(Culture, Value)
WHERE r.ParameterUnit = N'%'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Sakkaroz'),
        ('es', N'Sacarosa'),
        ('pl', N'Sacharoza'),
        ('lv', N'Saharoze')) v(Culture, Value)
WHERE r.ParameterName = N'Sucrose'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'g/100 g; belirli ballar için en çok 10 (ör. akasya/Robinia, narenciye)'),
        ('es', N'g/100 g; máx. 10 para mieles específicas (p. ej. Robinia, cítricos)'),
        ('pl', N'g/100 g; maks. 10 dla określonych miodów (np. Robinia, cytrusowy)'),
        ('lv', N'g/100 g; maks. 10 konkrētiem medus veidiem (piem., robīnijas, citrusaugu)')) v(Culture, Value)
WHERE r.LimitNote = N'g/100 g; max 10 for specific honeys (e.g. Robinia, citrus)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('en', N'Glucose'),
        ('tr', N'Glikoz'),
        ('es', N'Glucosa'),
        ('pl', N'Glukoza'),
        ('lv', N'Glikoze')) v(Culture, Value)
WHERE r.ParameterName = N'Glikoz'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz + glikoz olarak kontrol edilir (bkz. Fruktoz)'),
        ('es', N'Se comprueba como fructosa + glucosa (véase Fructosa)'),
        ('pl', N'Sprawdzana jako fruktoza + glukoza (patrz Fruktoza)'),
        ('lv', N'Pārbauda kā fruktozi + glikozi (skatīt "Fruktoze")')) v(Culture, Value)
WHERE r.LimitNote = N'Checked as fructose + glucose (see Fruktoz)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('en', N'Fructose'),
        ('tr', N'Fruktoz'),
        ('es', N'Fructosa'),
        ('pl', N'Fruktoza'),
        ('lv', N'Fruktoze')) v(Culture, Value)
WHERE r.ParameterName = N'Fruktoz'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz + glikoz: çiçek balı en az 60, salgı/çam balı en az 45 g/100 g; çiçek balı için fruktoz/glikoz oranı 0,9-1,4'),
        ('es', N'Fructosa + glucosa: floral mín. 60, mielada/pino mín. 45 g/100 g; relación fructosa/glucosa 0,9-1,4 para miel floral'),
        ('pl', N'Fruktoza + glukoza: kwiatowy min. 60, spadziowy/sosnowy min. 45 g/100 g; stosunek fruktozy do glukozy 0,9-1,4 dla miodu kwiatowego'),
        ('lv', N'Fruktoze + glikoze: ziedu medum min. 60, lapu/priežu medum min. 45 g/100 g; fruktozes/glikozes attiecība ziedu medum 0,9–1,4')) v(Culture, Value)
WHERE r.LimitNote = N'Fructose + glucose: floral min 60, honeydew/pine min 45 g/100 g; fructose/glucose ratio 0.9-1.4 for floral honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('en', N'Maltose'),
        ('tr', N'Maltoz'),
        ('es', N'Maltosa'),
        ('pl', N'Maltoza'),
        ('lv', N'Maltoze')) v(Culture, Value)
WHERE r.ParameterName = N'Maltoz'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Pestisitler'),
        ('es', N'Plaguicidas'),
        ('pl', N'Pestycydy'),
        ('lv', N'Pesticīdi')) v(Culture, Value)
WHERE r.ParameterName = N'Pesticides'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'ppb'),
        ('es', N'ppb'),
        ('pl', N'ppb'),
        ('lv', N'ppb')) v(Culture, Value)
WHERE r.ParameterUnit = N'ppb'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Antibiyotikler'),
        ('es', N'Antibióticos'),
        ('pl', N'Antybiotyki'),
        ('lv', N'Antibiotikas')) v(Culture, Value)
WHERE r.ParameterName = N'Antibiotics'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Veteriner ilaç kalıntıları: sıfır tolerans'),
        ('es', N'Residuos de medicamentos veterinarios: tolerancia cero'),
        ('pl', N'Pozostałości leków weterynaryjnych: zerowa tolerancja'),
        ('lv', N'Veterināro zāļu atliekas: nulles tolerance')) v(Culture, Value)
WHERE r.LimitNote = N'Veterinary drug residues: zero tolerance'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Elektriksel İletkenlik'),
        ('es', N'Conductividad eléctrica'),
        ('pl', N'Przewodność elektryczna'),
        ('lv', N'Elektrovadītspēja')) v(Culture, Value)
WHERE r.ParameterName = N'Electrical Conductivity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'mS/cm'),
        ('es', N'mS/cm'),
        ('pl', N'mS/cm'),
        ('lv', N'mS/cm')) v(Culture, Value)
WHERE r.ParameterUnit = N'mS/cm'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Çiçek balı en çok 0,8; salgı/çam ve kestane balı en az 0,8 mS/cm'),
        ('es', N'Floral máx. 0,8; mielada/pino y castaño mín. 0,8 mS/cm'),
        ('pl', N'Kwiatowy maks. 0,8, spadziowy/sosnowy i kasztanowy min. 0,8 mS/cm'),
        ('lv', N'Ziedu medum maks. 0,8, lapu/priežu un kastaņu medum min. 0,8 mS/cm')) v(Culture, Value)
WHERE r.LimitNote = N'Floral max 0.8, honeydew/pine and chestnut min 0.8 mS/cm'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Prolin'),
        ('es', N'Prolina'),
        ('pl', N'Prolina'),
        ('lv', N'Prolīns')) v(Culture, Value)
WHERE r.ParameterName = N'Proline'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'pH'),
        ('es', N'pH'),
        ('pl', N'pH'),
        ('lv', N'pH')) v(Culture, Value)
WHERE r.ParameterName = N'pH'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'pH'),
        ('es', N'pH'),
        ('pl', N'pH'),
        ('lv', N'pH')) v(Culture, Value)
WHERE r.ParameterUnit = N'pH'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Serbest Asitlik'),
        ('es', N'Acidez libre'),
        ('pl', N'Kwasowość wolna'),
        ('lv', N'Brīvais skābums')) v(Culture, Value)
WHERE r.ParameterName = N'Free Acidity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'meq/kg'),
        ('es', N'meq/kg'),
        ('pl', N'meq/kg'),
        ('lv', N'meq/kg')) v(Culture, Value)
WHERE r.ParameterUnit = N'meq/kg'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Suda Çözünmeyen Madde'),
        ('es', N'Materia insoluble'),
        ('pl', N'Substancje nierozpuszczalne'),
        ('lv', N'Nešķīstošās vielas')) v(Culture, Value)
WHERE r.ParameterName = N'Insoluble Matter'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Duyusal Analiz'),
        ('es', N'Análisis sensorial'),
        ('pl', N'Analiza sensoryczna'),
        ('lv', N'Sensorā analīze')) v(Culture, Value)
WHERE r.ParameterName = N'Sensory Analysis'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Sıcaklık'),
        ('es', N'Temperatura'),
        ('pl', N'Temperatura'),
        ('lv', N'Temperatūra')) v(Culture, Value)
WHERE r.ParameterName = N'Temperature'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'°C (Santigrat)'),
        ('es', N'°C (Celsius)'),
        ('pl', N'°C (Celsjusz)'),
        ('lv', N'°C (Celsija grādi)')) v(Culture, Value)
WHERE r.ParameterUnit = N'°C (Celsius)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Filtre bütünlüğü'),
        ('es', N'Integridad del filtro'),
        ('pl', N'Integralność filtra'),
        ('lv', N'Filtra integritāte')) v(Culture, Value)
WHERE r.ParameterName = N'Filter integrity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Mikron (µm)'),
        ('es', N'Micrón (µm)'),
        ('pl', N'Mikron (µm)'),
        ('lv', N'Mikrons (µm)')) v(Culture, Value)
WHERE r.ParameterUnit = N'Micron (µm)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Tortu varlığı'),
        ('es', N'Presencia de sedimentos'),
        ('pl', N'Obecność osadu'),
        ('lv', N'Nogulšņu klātbūtne')) v(Culture, Value)
WHERE r.ParameterName = N'Sediment presence'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Gözenek boyutu'),
        ('es', N'Tamaño de poro'),
        ('pl', N'Wielkość porów'),
        ('lv', N'Poru izmērs')) v(Culture, Value)
WHERE r.ParameterName = N'Pore size'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Görsel görünüm'),
        ('es', N'Aspecto visual'),
        ('pl', N'Wygląd'),
        ('lv', N'Vizuālais izskats')) v(Culture, Value)
WHERE r.ParameterName = N'Visual appearance'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Viskozite'),
        ('es', N'Viscosidad'),
        ('pl', N'Lepkość'),
        ('lv', N'Viskozitāte')) v(Culture, Value)
WHERE r.ParameterName = N'Viscosity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Renk tutarlılığı'),
        ('es', N'Uniformidad del color'),
        ('pl', N'Jednolitość barwy'),
        ('lv', N'Krāsas vienmērīgums')) v(Culture, Value)
WHERE r.ParameterName = N'Color consistency'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bekletme Süresi'),
        ('es', N'Tiempo de mantenimiento'),
        ('pl', N'Czas przetrzymania'),
        ('lv', N'Noturēšanas laiks')) v(Culture, Value)
WHERE r.ParameterName = N'Holding Time'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Dakika/Saniye'),
        ('es', N'Minutos/segundos'),
        ('pl', N'Minuty/sekundy'),
        ('lv', N'Minūtes/sekundes')) v(Culture, Value)
WHERE r.ParameterUnit = N'Minutes/Seconds'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Görsel Kontrol'),
        ('es', N'Inspección visual'),
        ('pl', N'Kontrola wizualna'),
        ('lv', N'Vizuālā pārbaude')) v(Culture, Value)
WHERE r.ParameterName = N'Visual Inspection'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Net ağırlık'),
        ('es', N'Peso neto'),
        ('pl', N'Masa netto'),
        ('lv', N'Neto svars')) v(Culture, Value)
WHERE r.ParameterName = N'Net weight'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'g (Gram)'),
        ('es', N'g (gramos)'),
        ('pl', N'g (gramy)'),
        ('lv', N'g (grami)')) v(Culture, Value)
WHERE r.ParameterUnit = N'g (Grams)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Menşe'),
        ('es', N'Origen'),
        ('pl', N'Pochodzenie'),
        ('lv', N'Izcelsme')) v(Culture, Value)
WHERE r.ParameterName = N'Origin'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bitkisel kaynak'),
        ('es', N'Origen botánico'),
        ('pl', N'Źródło botaniczne'),
        ('lv', N'Botāniskā izcelsme')) v(Culture, Value)
WHERE r.ParameterName = N'Botanical source'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Analitik tutarlılık'),
        ('es', N'Coherencia analítica'),
        ('pl', N'Zgodność analityczna'),
        ('lv', N'Analītiskā atbilstība')) v(Culture, Value)
WHERE r.ParameterName = N'Analytical consistency'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Malzeme türü'),
        ('es', N'Tipo de material'),
        ('pl', N'Rodzaj materiału'),
        ('lv', N'Materiāla veids')) v(Culture, Value)
WHERE r.ParameterName = N'Material type'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Temizlik'),
        ('es', N'Limpieza'),
        ('pl', N'Czystość'),
        ('lv', N'Tīrība')) v(Culture, Value)
WHERE r.ParameterName = N'Cleanliness'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Fiziksel dayanıklılık'),
        ('es', N'Durabilidad física'),
        ('pl', N'Wytrzymałość fizyczna'),
        ('lv', N'Fiziskā izturība')) v(Culture, Value)
WHERE r.ParameterName = N'Physical durability'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'LimitNote', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Depolama sıcaklığı'),
        ('es', N'Temperatura de almacenamiento'),
        ('pl', N'Temperatura przechowywania'),
        ('lv', N'Uzglabāšanas temperatūra')) v(Culture, Value)
WHERE r.LimitNote = N'Storage temperature'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'LimitNote' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Bağıl Nem'),
        ('es', N'Humedad relativa'),
        ('pl', N'Wilgotność względna'),
        ('lv', N'Relatīvais mitrums')) v(Culture, Value)
WHERE r.ParameterName = N'Relative Humidity'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Unit', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Yüzde (%)'),
        ('es', N'Porcentaje (%)'),
        ('pl', N'Procent (%)'),
        ('lv', N'Procenti (%)')) v(Culture, Value)
WHERE r.ParameterUnit = N'Percentage (%)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Unit' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Parameter', CONVERT(nvarchar(100), r.ParameterId), 'Name', v.Culture, v.Value
FROM dbo.CriticalControlPointParameters r
CROSS JOIN (VALUES
        ('tr', N'Naftalin'),
        ('es', N'Naftaleno'),
        ('pl', N'Naftalen'),
        ('lv', N'Naftalīns')) v(Culture, Value)
WHERE r.ParameterName = N'Naphthalene'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Parameter' AND t.EntityKey = CONVERT(nvarchar(100), r.ParameterId) AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Çam balı (şüpheli: AB''deki %23 istisnası funda / Calluna balı içindir; doğrulanacak)'),
        ('es', N'Miel de pino (dudoso: la excepción del 23 % de la UE corresponde a la miel de brezo / Calluna; por verificar)'),
        ('pl', N'Miód sosnowy (wątpliwe: wyjątek UE 23 % dotyczy miodu wrzosowego / Calluna; do weryfikacji)'),
        ('lv', N'Priežu medus (apšaubāmi: ES 23 % izņēmums attiecas uz virsāju / Calluna medu; jāpārbauda)')) v(Culture, Value)
WHERE r.Note = N'Pine honey (doubtful: the EU 23 % exception is for heather / Calluna honey; to verify)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Doğal enzim içeriği düşük bal (HMF <= 15 mg/kg)'),
        ('es', N'Miel con bajo contenido natural de enzimas (HMF <= 15 mg/kg)'),
        ('pl', N'Miód o niskiej naturalnej zawartości enzymów (HMF <= 15 mg/kg)'),
        ('lv', N'Medus ar zemu dabisko enzīmu saturu (HMF <= 15 mg/kg)')) v(Culture, Value)
WHERE r.Note = N'Honey with low natural enzyme content (HMF <= 15 mg/kg)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz + glikoz, çiçek balı (g/100 g)'),
        ('es', N'Fructosa + glucosa, miel floral (g/100 g)'),
        ('pl', N'Fruktoza + glukoza, miód kwiatowy (g/100 g)'),
        ('lv', N'Fruktoze + glikoze, ziedu medus (g/100 g)')) v(Culture, Value)
WHERE r.Note = N'Fructose + glucose, floral honey (g/100 g)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz + glikoz, salgı balı (g/100 g)'),
        ('es', N'Fructosa + glucosa, miel de mielada (g/100 g)'),
        ('pl', N'Fruktoza + glukoza, miód spadziowy (g/100 g)'),
        ('lv', N'Fruktoze + glikoze, lapu medus (g/100 g)')) v(Culture, Value)
WHERE r.Note = N'Fructose + glucose, honeydew honey (g/100 g)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz + glikoz, çam (salgı) balı (g/100 g)'),
        ('es', N'Fructosa + glucosa, miel de pino (mielada) (g/100 g)'),
        ('pl', N'Fruktoza + glukoza, miód sosnowy (spadziowy) (g/100 g)'),
        ('lv', N'Fruktoze + glikoze, priežu (lapu) medus (g/100 g)')) v(Culture, Value)
WHERE r.Note = N'Fructose + glucose, pine (honeydew) honey (g/100 g)'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Çiçek balı'),
        ('es', N'Miel floral (de flores)'),
        ('pl', N'Miód kwiatowy (nektarowy)'),
        ('lv', N'Ziedu medus')) v(Culture, Value)
WHERE r.Note = N'Floral (blossom) honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Salgı balı'),
        ('es', N'Miel de mielada'),
        ('pl', N'Miód spadziowy'),
        ('lv', N'Lapu medus')) v(Culture, Value)
WHERE r.Note = N'Honeydew honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Çam (salgı) balı'),
        ('es', N'Miel de pino (mielada)'),
        ('pl', N'Miód sosnowy (spadziowy)'),
        ('lv', N'Priežu (lapu) medus')) v(Culture, Value)
WHERE r.Note = N'Pine (honeydew) honey'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Kestane balı salgı balı kuralına tabidir (Codex / AB 2001/110); doğrulanacak'),
        ('es', N'La miel de castaño sigue la regla de la miel de mielada (Codex / UE 2001/110); por verificar'),
        ('pl', N'Miód kasztanowy podlega zasadzie dla miodu spadziowego (Codex / EU 2001/110); do weryfikacji'),
        ('lv', N'Uz kastaņu medu attiecas lapu medus noteikums (Codex / ES 2001/110); jāpārbauda')) v(Culture, Value)
WHERE r.Note = N'Chestnut honey follows the honeydew rule (Codex / EU 2001/110); to verify'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterLimit', CONVERT(nvarchar(100), r.LimitId), 'Note', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterLimits r
CROSS JOIN (VALUES
        ('tr', N'Fruktoz / glikoz oranı, çiçek balı; doğrulanacak'),
        ('es', N'Relación fructosa / glucosa, miel floral; por verificar'),
        ('pl', N'Stosunek fruktozy do glukozy, miód kwiatowy; do weryfikacji'),
        ('lv', N'Fruktozes / glikozes attiecība, ziedu medus; jāpārbauda')) v(Culture, Value)
WHERE r.Note = N'Fructose / glucose ratio, floral honey; to verify'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterLimit' AND t.EntityKey = CONVERT(nvarchar(100), r.LimitId) AND t.Field = 'Note' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'TR', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Türkiye'),
        ('es', N'Türkiye'),
        ('pl', N'Turcja'),
        ('lv', N'Turcija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'TR' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'AR', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Arjantin'),
        ('es', N'Argentina'),
        ('pl', N'Argentyna'),
        ('lv', N'Argentīna')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'AR' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'BG', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Bulgaristan'),
        ('es', N'Bulgaria'),
        ('pl', N'Bułgaria'),
        ('lv', N'Bulgārija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'BG' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'CN', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Çin'),
        ('es', N'China'),
        ('pl', N'Chiny'),
        ('lv', N'Ķīna')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'CN' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'DE', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Almanya'),
        ('es', N'Alemania'),
        ('pl', N'Niemcy'),
        ('lv', N'Vācija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'DE' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'ES', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'İspanya'),
        ('es', N'España'),
        ('pl', N'Hiszpania'),
        ('lv', N'Spānija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'ES' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'GE', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Gürcistan'),
        ('es', N'Georgia'),
        ('pl', N'Gruzja'),
        ('lv', N'Gruzija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'GE' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'GR', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Yunanistan'),
        ('es', N'Grecia'),
        ('pl', N'Grecja'),
        ('lv', N'Grieķija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'GR' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'HU', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Macaristan'),
        ('es', N'Hungría'),
        ('pl', N'Węgry'),
        ('lv', N'Ungārija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'HU' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'IT', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'İtalya'),
        ('es', N'Italia'),
        ('pl', N'Włochy'),
        ('lv', N'Itālija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'IT' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'LV', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Letonya'),
        ('es', N'Letonia'),
        ('pl', N'Łotwa'),
        ('lv', N'Latvija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'LV' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'MX', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Meksika'),
        ('es', N'México'),
        ('pl', N'Meksyk'),
        ('lv', N'Meksika')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'MX' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'NZ', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Yeni Zelanda'),
        ('es', N'Nueva Zelanda'),
        ('pl', N'Nowa Zelandia'),
        ('lv', N'Jaunzēlande')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'NZ' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'PL', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Polonya'),
        ('es', N'Polonia'),
        ('pl', N'Polska'),
        ('lv', N'Polija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'PL' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'RO', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Romanya'),
        ('es', N'Rumania'),
        ('pl', N'Rumunia'),
        ('lv', N'Rumānija')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'RO' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'UA', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Ukrayna'),
        ('es', N'Ucrania'),
        ('pl', N'Ukraina'),
        ('lv', N'Ukraina')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'UA' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'UY', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Uruguay'),
        ('es', N'Uruguay'),
        ('pl', N'Urugwaj'),
        ('lv', N'Urugvaja')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'UY' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'Country', N'VN', 'Name', v.Culture, v.Value
FROM (VALUES
        ('tr', N'Vietnam'),
        ('es', N'Vietnam'),
        ('pl', N'Wietnam'),
        ('lv', N'Vjetnama')) v(Culture, Value)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'Country' AND t.EntityKey = N'VN' AND t.Field = 'Name' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterContent', CONVERT(nvarchar(100), r.ContentId), 'WhatIsIt', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterContents r
CROSS JOIN (VALUES
        ('tr', N'Diastaz, balda bulunan ve nişastayı şekerlere parçalayan doğal bir enzimdir.'),
        ('es', N'La diastasa es una enzima natural de la miel que descompone el almidón en azúcares.'),
        ('pl', N'Diastaza to naturalny enzym w miodzie, który rozkłada skrobię na cukry.'),
        ('lv', N'Diastāze ir dabisks medus enzīms, kas sašķeļ cieti cukuros.')) v(Culture, Value)
WHERE r.WhatIsIt = N'Diastase is a natural enzyme in honey that breaks down starch into sugars.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterContent' AND t.EntityKey = CONVERT(nvarchar(100), r.ContentId) AND t.Field = 'WhatIsIt' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterContent', CONVERT(nvarchar(100), r.ContentId), 'WhyImportant', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterContents r
CROSS JOIN (VALUES
        ('tr', N'Balın tazeliğini, ısıya maruz kalıp kalmadığını ve genel kalitesini gösterir (DN ≥ 8).'),
        ('es', N'Indica la frescura de la miel, su exposición al calor y su calidad general (DN ≥ 8).'),
        ('pl', N'Wskazuje świeżość miodu, ekspozycję na ciepło i ogólną jakość (DN ≥ 8).'),
        ('lv', N'Tā norāda medus svaigumu, karsēšanas ietekmi un kopējo kvalitāti (DN ≥ 8).')) v(Culture, Value)
WHERE r.WhyImportant = N'It indicates honey freshness, heat exposure, and overall quality (DN ≥ 8).'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterContent' AND t.EntityKey = CONVERT(nvarchar(100), r.ContentId) AND t.Field = 'WhyImportant' AND t.Culture = v.Culture);

INSERT dbo.ReferenceTranslations (Entity, EntityKey, Field, Culture, Value)
SELECT 'ParameterContent', CONVERT(nvarchar(100), r.ContentId), 'ExtraInfo', v.Culture, v.Value
FROM dbo.CriticalControlPointParameterContents r
CROSS JOIN (VALUES
        ('tr', N'Düzeyi bal türüne göre değişir ve ısıya maruz kaldığında azalır.'),
        ('es', N'Su nivel varía según el tipo de miel y disminuye con la exposición al calor.'),
        ('pl', N'Jej poziom zależy od rodzaju miodu i spada pod wpływem ciepła.'),
        ('lv', N'Tās līmenis atšķiras atkarībā no medus veida un samazinās karsējot.')) v(Culture, Value)
WHERE r.ExtraInfo = N'Its level varies by honey type and decreases when exposed to heat.'
  AND NOT EXISTS (SELECT 1 FROM dbo.ReferenceTranslations t WHERE t.Entity = 'ParameterContent' AND t.EntityKey = CONVERT(nvarchar(100), r.ContentId) AND t.Field = 'ExtraInfo' AND t.Culture = v.Culture);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO
