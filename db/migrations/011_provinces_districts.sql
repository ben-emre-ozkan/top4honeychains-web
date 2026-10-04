-- 011_provinces_districts.sql
-- Province and district lists for the province / district dropdowns (D-52).
--   * Provinces: the 81 provinces with their plate code (the 2 digits after TR in a beekeeper registry number)
--   * Districts: the 973 districts by province
-- Seed source: turkey-neighbourhoods 4.0.3 (MIT), built from the PTT postal code list; to verify against the Ministry of Interior list. Marked "to verify" (Source column); district names follow the dataset ("Merkez" for a
-- central district). Stored addresses keep their text: the forms store the names, not ids.
--
-- Additive and idempotent; a second run inserts nothing. SQL Server 2019 (level 150). Run with sqlcmd -I -f 65001.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Provinces', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Provinces
    (
        Code   char(2)       NOT NULL CONSTRAINT PK_Provinces PRIMARY KEY,
        Name   nvarchar(50)  NOT NULL,
        Source nvarchar(200) NULL,
        CONSTRAINT UQ_Provinces_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.Districts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Districts
    (
        DistrictId   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Districts PRIMARY KEY,
        ProvinceCode char(2)      NOT NULL CONSTRAINT FK_Districts_Provinces REFERENCES dbo.Provinces (Code),
        Name         nvarchar(60) NOT NULL,
        CONSTRAINT UQ_Districts_Province_Name UNIQUE (ProvinceCode, Name)
    );
END;
GO

BEGIN TRANSACTION;

INSERT dbo.Provinces (Code, Name, Source)
SELECT v.Code, v.Name, N'turkey-neighbourhoods 4.0.3 (MIT), built from the PTT postal code list; to verify against the Ministry of Interior list'
FROM (VALUES
        ('01', N'Adana'),
        ('02', N'Adıyaman'),
        ('03', N'Afyonkarahisar'),
        ('04', N'Ağrı'),
        ('05', N'Amasya'),
        ('06', N'Ankara'),
        ('07', N'Antalya'),
        ('08', N'Artvin'),
        ('09', N'Aydın'),
        ('10', N'Balıkesir'),
        ('11', N'Bilecik'),
        ('12', N'Bingöl'),
        ('13', N'Bitlis'),
        ('14', N'Bolu'),
        ('15', N'Burdur'),
        ('16', N'Bursa'),
        ('17', N'Çanakkale'),
        ('18', N'Çankırı'),
        ('19', N'Çorum'),
        ('20', N'Denizli'),
        ('21', N'Diyarbakır'),
        ('22', N'Edirne'),
        ('23', N'Elazığ'),
        ('24', N'Erzincan'),
        ('25', N'Erzurum'),
        ('26', N'Eskişehir'),
        ('27', N'Gaziantep'),
        ('28', N'Giresun'),
        ('29', N'Gümüşhane'),
        ('30', N'Hakkari'),
        ('31', N'Hatay'),
        ('32', N'Isparta'),
        ('33', N'Mersin'),
        ('34', N'İstanbul'),
        ('35', N'İzmir'),
        ('36', N'Kars'),
        ('37', N'Kastamonu'),
        ('38', N'Kayseri'),
        ('39', N'Kırklareli'),
        ('40', N'Kırşehir'),
        ('41', N'Kocaeli'),
        ('42', N'Konya'),
        ('43', N'Kütahya'),
        ('44', N'Malatya'),
        ('45', N'Manisa'),
        ('46', N'Kahramanmaraş'),
        ('47', N'Mardin'),
        ('48', N'Muğla'),
        ('49', N'Muş'),
        ('50', N'Nevşehir'),
        ('51', N'Niğde'),
        ('52', N'Ordu'),
        ('53', N'Rize'),
        ('54', N'Sakarya'),
        ('55', N'Samsun'),
        ('56', N'Siirt'),
        ('57', N'Sinop'),
        ('58', N'Sivas'),
        ('59', N'Tekirdağ'),
        ('60', N'Tokat'),
        ('61', N'Trabzon'),
        ('62', N'Tunceli'),
        ('63', N'Şanlıurfa'),
        ('64', N'Uşak'),
        ('65', N'Van'),
        ('66', N'Yozgat'),
        ('67', N'Zonguldak'),
        ('68', N'Aksaray'),
        ('69', N'Bayburt'),
        ('70', N'Karaman'),
        ('71', N'Kırıkkale'),
        ('72', N'Batman'),
        ('73', N'Şırnak'),
        ('74', N'Bartın'),
        ('75', N'Ardahan'),
        ('76', N'Iğdır'),
        ('77', N'Yalova'),
        ('78', N'Karabük'),
        ('79', N'Kilis'),
        ('80', N'Osmaniye'),
        ('81', N'Düzce')) v(Code, Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Provinces p WHERE p.Code = v.Code);

INSERT dbo.Districts (ProvinceCode, Name)
SELECT '01', v.Name FROM (VALUES (N'Aladağ'), (N'Ceyhan'), (N'Feke'), (N'Karaisalı'), (N'Karataş'), (N'Kozan'), (N'Pozantı'), (N'Saimbeyli'), (N'Sarıçam'), (N'Seyhan'), (N'Tufanbeyli'), (N'Yumurtalık'), (N'Yüreğir'), (N'Çukurova'), (N'İmamoğlu')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '01' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '02', v.Name FROM (VALUES (N'Besni'), (N'Gerger'), (N'Gölbaşı'), (N'Kahta'), (N'Merkez'), (N'Samsat'), (N'Sincik'), (N'Tut'), (N'Çelikhan')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '02' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '03', v.Name FROM (VALUES (N'Bayat'), (N'Başmakçı'), (N'Bolvadin'), (N'Dazkırı'), (N'Dinar'), (N'Emirdağ'), (N'Evciler'), (N'Hocalar'), (N'Kızılören'), (N'Merkez'), (N'Sandıklı'), (N'Sinanpaşa'), (N'Sultandağı'), (N'Çay'), (N'Çobanlar'), (N'İhsaniye'), (N'İscehisar'), (N'Şuhut')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '03' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '04', v.Name FROM (VALUES (N'Diyadin'), (N'Doğubayazıt'), (N'Eleşkirt'), (N'Hamur'), (N'Merkez'), (N'Patnos'), (N'Taşlıçay'), (N'Tutak')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '04' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '05', v.Name FROM (VALUES (N'Göynücek'), (N'Gümüşhacıköy'), (N'Hamamözü'), (N'Merkez'), (N'Merzifon'), (N'Suluova'), (N'Taşova')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '05' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '06', v.Name FROM (VALUES (N'Akyurt'), (N'Altındağ'), (N'Ayaş'), (N'Bala'), (N'Beypazarı'), (N'Elmadağ'), (N'Etimesgut'), (N'Evren'), (N'Gölbaşı'), (N'Güdül'), (N'Haymana'), (N'Kahramankazan'), (N'Kalecik'), (N'Keçiören'), (N'Kızılcahamam'), (N'Mamak'), (N'Nallıhan'), (N'Polatlı'), (N'Pursaklar'), (N'Sincan'), (N'Yenimahalle'), (N'Çamlıdere'), (N'Çankaya'), (N'Çubuk'), (N'Şereflikoçhisar')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '06' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '07', v.Name FROM (VALUES (N'Akseki'), (N'Aksu'), (N'Alanya'), (N'Demre'), (N'Döşemealtı'), (N'Elmalı'), (N'Finike'), (N'Gazipaşa'), (N'Gündoğmuş'), (N'Kaş'), (N'Kemer'), (N'Kepez'), (N'Konyaaltı'), (N'Korkuteli'), (N'Kumluca'), (N'Manavgat'), (N'Muratpaşa'), (N'Serik'), (N'İbradı')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '07' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '08', v.Name FROM (VALUES (N'Ardanuç'), (N'Arhavi'), (N'Borçka'), (N'Hopa'), (N'Kemalpaşa'), (N'Merkez'), (N'Murgul'), (N'Yusufeli'), (N'Şavşat')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '08' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '09', v.Name FROM (VALUES (N'Bozdoğan'), (N'Buharkent'), (N'Didim'), (N'Efeler'), (N'Germencik'), (N'Karacasu'), (N'Karpuzlu'), (N'Koçarlı'), (N'Kuyucak'), (N'Kuşadası'), (N'Köşk'), (N'Nazilli'), (N'Sultanhisar'), (N'Söke'), (N'Yenipazar'), (N'Çine'), (N'İncirliova')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '09' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '10', v.Name FROM (VALUES (N'Altıeylül'), (N'Ayvalık'), (N'Balya'), (N'Bandırma'), (N'Bigadiç'), (N'Burhaniye'), (N'Dursunbey'), (N'Edremit'), (N'Erdek'), (N'Gömeç'), (N'Gönen'), (N'Havran'), (N'Karesi'), (N'Kepsut'), (N'Manyas'), (N'Marmara'), (N'Savaştepe'), (N'Susurluk'), (N'Sındırgı'), (N'İvrindi')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '10' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '11', v.Name FROM (VALUES (N'Bozüyük'), (N'Gölpazarı'), (N'Merkez'), (N'Osmaneli'), (N'Pazaryeri'), (N'Söğüt'), (N'Yenipazar'), (N'İnhisar')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '11' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '12', v.Name FROM (VALUES (N'Adaklı'), (N'Genç'), (N'Karlıova'), (N'Kiğı'), (N'Merkez'), (N'Solhan'), (N'Yayladere'), (N'Yedisu')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '12' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '13', v.Name FROM (VALUES (N'Adilcevaz'), (N'Ahlat'), (N'Güroymak'), (N'Hizan'), (N'Merkez'), (N'Mutki'), (N'Tatvan')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '13' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '14', v.Name FROM (VALUES (N'Dörtdivan'), (N'Gerede'), (N'Göynük'), (N'Kıbrıscık'), (N'Mengen'), (N'Merkez'), (N'Mudurnu'), (N'Seben'), (N'Yeniçağa')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '14' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '15', v.Name FROM (VALUES (N'Altınyayla'), (N'Ağlasun'), (N'Bucak'), (N'Gölhisar'), (N'Karamanlı'), (N'Kemer'), (N'Merkez'), (N'Tefenni'), (N'Yeşilova'), (N'Çavdır'), (N'Çeltikçi')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '15' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '16', v.Name FROM (VALUES (N'Büyükorhan'), (N'Gemlik'), (N'Gürsu'), (N'Harmancık'), (N'Karacabey'), (N'Keles'), (N'Kestel'), (N'Mudanya'), (N'Mustafakemalpaşa'), (N'Nilüfer'), (N'Orhaneli'), (N'Orhangazi'), (N'Osmangazi'), (N'Yenişehir'), (N'Yıldırım'), (N'İnegöl'), (N'İznik')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '16' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '17', v.Name FROM (VALUES (N'Ayvacık'), (N'Bayramiç'), (N'Biga'), (N'Bozcaada'), (N'Eceabat'), (N'Ezine'), (N'Gelibolu'), (N'Gökçeada'), (N'Lapseki'), (N'Merkez'), (N'Yenice'), (N'Çan')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '17' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '18', v.Name FROM (VALUES (N'Atkaracalar'), (N'Bayramören'), (N'Eldivan'), (N'Ilgaz'), (N'Korgun'), (N'Kurşunlu'), (N'Kızılırmak'), (N'Merkez'), (N'Orta'), (N'Yapraklı'), (N'Çerkeş'), (N'Şabanözü')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '18' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '19', v.Name FROM (VALUES (N'Alaca'), (N'Bayat'), (N'Boğazkale'), (N'Dodurga'), (N'Kargı'), (N'Laçin'), (N'Mecitözü'), (N'Merkez'), (N'Ortaköy'), (N'Osmancık'), (N'Oğuzlar'), (N'Sungurlu'), (N'Uğurludağ'), (N'İskilip')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '19' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '20', v.Name FROM (VALUES (N'Acıpayam'), (N'Babadağ'), (N'Baklan'), (N'Bekilli'), (N'Beyağaç'), (N'Bozkurt'), (N'Buldan'), (N'Güney'), (N'Honaz'), (N'Kale'), (N'Merkezefendi'), (N'Pamukkale'), (N'Sarayköy'), (N'Serinhisar'), (N'Tavas'), (N'Çal'), (N'Çameli'), (N'Çardak'), (N'Çivril')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '20' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '21', v.Name FROM (VALUES (N'Bağlar'), (N'Bismil'), (N'Dicle'), (N'Ergani'), (N'Eğil'), (N'Hani'), (N'Hazro'), (N'Kayapınar'), (N'Kocaköy'), (N'Kulp'), (N'Lice'), (N'Silvan'), (N'Sur'), (N'Yenişehir'), (N'Çermik'), (N'Çüngüş'), (N'Çınar')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '21' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '22', v.Name FROM (VALUES (N'Enez'), (N'Havsa'), (N'Keşan'), (N'Lalapaşa'), (N'Meriç'), (N'Merkez'), (N'Süloğlu'), (N'Uzunköprü'), (N'İpsala')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '22' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '23', v.Name FROM (VALUES (N'Alacakaya'), (N'Arıcak'), (N'Ağın'), (N'Baskil'), (N'Karakoçan'), (N'Keban'), (N'Kovancılar'), (N'Maden'), (N'Merkez'), (N'Palu'), (N'Sivrice')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '23' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '24', v.Name FROM (VALUES (N'Kemah'), (N'Kemaliye'), (N'Merkez'), (N'Otlukbeli'), (N'Refahiye'), (N'Tercan'), (N'Çayırlı'), (N'Üzümlü'), (N'İliç')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '24' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '25', v.Name FROM (VALUES (N'Aziziye'), (N'Aşkale'), (N'Horasan'), (N'Hınıs'), (N'Karayazı'), (N'Karaçoban'), (N'Köprüköy'), (N'Narman'), (N'Oltu'), (N'Olur'), (N'Palandöken'), (N'Pasinler'), (N'Pazaryolu'), (N'Tekman'), (N'Tortum'), (N'Uzundere'), (N'Yakutiye'), (N'Çat'), (N'İspir'), (N'Şenkaya')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '25' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '26', v.Name FROM (VALUES (N'Alpu'), (N'Beylikova'), (N'Günyüzü'), (N'Han'), (N'Mahmudiye'), (N'Mihalgazi'), (N'Mihalıççık'), (N'Odunpazarı'), (N'Sarıcakaya'), (N'Seyitgazi'), (N'Sivrihisar'), (N'Tepebaşı'), (N'Çifteler'), (N'İnönü')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '26' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '27', v.Name FROM (VALUES (N'Araban'), (N'Karkamış'), (N'Nizip'), (N'Nurdağı'), (N'Oğuzeli'), (N'Yavuzeli'), (N'İslahiye'), (N'Şahinbey'), (N'Şehitkamil')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '27' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '28', v.Name FROM (VALUES (N'Alucra'), (N'Bulancak'), (N'Dereli'), (N'Doğankent'), (N'Espiye'), (N'Eynesil'), (N'Görele'), (N'Güce'), (N'Keşap'), (N'Merkez'), (N'Piraziz'), (N'Tirebolu'), (N'Yağlıdere'), (N'Çamoluk'), (N'Çanakçı'), (N'Şebinkarahisar')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '28' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '29', v.Name FROM (VALUES (N'Kelkit'), (N'Köse'), (N'Kürtün'), (N'Merkez'), (N'Torul'), (N'Şiran')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '29' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '30', v.Name FROM (VALUES (N'Derecik'), (N'Merkez'), (N'Yüksekova'), (N'Çukurca'), (N'Şemdinli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '30' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '31', v.Name FROM (VALUES (N'Altınözü'), (N'Antakya'), (N'Arsuz'), (N'Belen'), (N'Defne'), (N'Dörtyol'), (N'Erzin'), (N'Hassa'), (N'Kumlu'), (N'Kırıkhan'), (N'Payas'), (N'Reyhanlı'), (N'Samandağ'), (N'Yayladağı'), (N'İskenderun')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '31' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '32', v.Name FROM (VALUES (N'Aksu'), (N'Atabey'), (N'Eğirdir'), (N'Gelendost'), (N'Gönen'), (N'Keçiborlu'), (N'Merkez'), (N'Senirkent'), (N'Sütçüler'), (N'Uluborlu'), (N'Yalvaç'), (N'Yenişarbademli'), (N'Şarkikaraağaç')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '32' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '33', v.Name FROM (VALUES (N'Akdeniz'), (N'Anamur'), (N'Aydıncık'), (N'Bozyazı'), (N'Erdemli'), (N'Gülnar'), (N'Mezitli'), (N'Mut'), (N'Silifke'), (N'Tarsus'), (N'Toroslar'), (N'Yenişehir'), (N'Çamlıyayla')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '33' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '34', v.Name FROM (VALUES (N'Adalar'), (N'Arnavutköy'), (N'Ataşehir'), (N'Avcılar'), (N'Bahçelievler'), (N'Bakırköy'), (N'Bayrampaşa'), (N'Bağcılar'), (N'Başakşehir'), (N'Beykoz'), (N'Beylikdüzü'), (N'Beyoğlu'), (N'Beşiktaş'), (N'Büyükçekmece'), (N'Esenler'), (N'Esenyurt'), (N'Eyüpsultan'), (N'Fatih'), (N'Gaziosmanpaşa'), (N'Güngören'), (N'Kadıköy'), (N'Kartal'), (N'Kağıthane'), (N'Küçükçekmece'), (N'Maltepe'), (N'Pendik'), (N'Sancaktepe'), (N'Sarıyer'), (N'Silivri'), (N'Sultanbeyli'), (N'Sultangazi'), (N'Tuzla'), (N'Zeytinburnu'), (N'Çatalca'), (N'Çekmeköy'), (N'Ümraniye'), (N'Üsküdar'), (N'Şile'), (N'Şişli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '34' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '35', v.Name FROM (VALUES (N'Aliağa'), (N'Balçova'), (N'Bayraklı'), (N'Bayındır'), (N'Bergama'), (N'Beydağ'), (N'Bornova'), (N'Buca'), (N'Dikili'), (N'Foça'), (N'Gaziemir'), (N'Güzelbahçe'), (N'Karabağlar'), (N'Karaburun'), (N'Karşıyaka'), (N'Kemalpaşa'), (N'Kiraz'), (N'Konak'), (N'Kınık'), (N'Menderes'), (N'Menemen'), (N'Narlıdere'), (N'Seferihisar'), (N'Selçuk'), (N'Tire'), (N'Torbalı'), (N'Urla'), (N'Çeşme'), (N'Çiğli'), (N'Ödemiş')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '35' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '36', v.Name FROM (VALUES (N'Akyaka'), (N'Arpaçay'), (N'Digor'), (N'Kağızman'), (N'Merkez'), (N'Sarıkamış'), (N'Selim'), (N'Susuz')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '36' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '37', v.Name FROM (VALUES (N'Abana'), (N'Araç'), (N'Azdavay'), (N'Ağlı'), (N'Bozkurt'), (N'Cide'), (N'Daday'), (N'Devrekani'), (N'Doğanyurt'), (N'Hanönü'), (N'Küre'), (N'Merkez'), (N'Pınarbaşı'), (N'Seydiler'), (N'Taşköprü'), (N'Tosya'), (N'Çatalzeytin'), (N'İhsangazi'), (N'İnebolu'), (N'Şenpazar')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '37' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '38', v.Name FROM (VALUES (N'Akkışla'), (N'Bünyan'), (N'Develi'), (N'Felahiye'), (N'Hacılar'), (N'Kocasinan'), (N'Melikgazi'), (N'Pınarbaşı'), (N'Sarıoğlan'), (N'Sarız'), (N'Talas'), (N'Tomarza'), (N'Yahyalı'), (N'Yeşilhisar'), (N'Özvatan'), (N'İncesu')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '38' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '39', v.Name FROM (VALUES (N'Babaeski'), (N'Demirköy'), (N'Kofçaz'), (N'Lüleburgaz'), (N'Merkez'), (N'Pehlivanköy'), (N'Pınarhisar'), (N'Vize')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '39' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '40', v.Name FROM (VALUES (N'Akpınar'), (N'Akçakent'), (N'Boztepe'), (N'Kaman'), (N'Merkez'), (N'Mucur'), (N'Çiçekdağı')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '40' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '41', v.Name FROM (VALUES (N'Başiskele'), (N'Darıca'), (N'Derince'), (N'Dilovası'), (N'Gebze'), (N'Gölcük'), (N'Kandıra'), (N'Karamürsel'), (N'Kartepe'), (N'Körfez'), (N'Çayırova'), (N'İzmit')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '41' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '42', v.Name FROM (VALUES (N'Ahırlı'), (N'Akören'), (N'Akşehir'), (N'Altınekin'), (N'Beyşehir'), (N'Bozkır'), (N'Cihanbeyli'), (N'Derbent'), (N'Derebucak'), (N'Doğanhisar'), (N'Emirgazi'), (N'Ereğli'), (N'Güneysınır'), (N'Hadim'), (N'Halkapınar'), (N'Hüyük'), (N'Ilgın'), (N'Kadınhanı'), (N'Karapınar'), (N'Karatay'), (N'Kulu'), (N'Meram'), (N'Sarayönü'), (N'Selçuklu'), (N'Seydişehir'), (N'Taşkent'), (N'Tuzlukçu'), (N'Yalıhüyük'), (N'Yunak'), (N'Çeltik'), (N'Çumra')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '42' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '43', v.Name FROM (VALUES (N'Altıntaş'), (N'Aslanapa'), (N'Domaniç'), (N'Dumlupınar'), (N'Emet'), (N'Gediz'), (N'Hisarcık'), (N'Merkez'), (N'Pazarlar'), (N'Simav'), (N'Tavşanlı'), (N'Çavdarhisar'), (N'Şaphane')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '43' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '44', v.Name FROM (VALUES (N'Akçadağ'), (N'Arapgir'), (N'Arguvan'), (N'Battalgazi'), (N'Darende'), (N'Doğanyol'), (N'Doğanşehir'), (N'Hekimhan'), (N'Kale'), (N'Kuluncak'), (N'Pütürge'), (N'Yazıhan'), (N'Yeşilyurt')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '44' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '45', v.Name FROM (VALUES (N'Ahmetli'), (N'Akhisar'), (N'Alaşehir'), (N'Demirci'), (N'Gölmarmara'), (N'Gördes'), (N'Kula'), (N'Köprübaşı'), (N'Kırkağaç'), (N'Salihli'), (N'Saruhanlı'), (N'Sarıgöl'), (N'Selendi'), (N'Soma'), (N'Turgutlu'), (N'Yunusemre'), (N'Şehzadeler')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '45' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '46', v.Name FROM (VALUES (N'Afşin'), (N'Andırın'), (N'Dulkadiroğlu'), (N'Ekinözü'), (N'Elbistan'), (N'Göksun'), (N'Nurhak'), (N'Onikişubat'), (N'Pazarcık'), (N'Türkoğlu'), (N'Çağlayancerit')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '46' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '47', v.Name FROM (VALUES (N'Artuklu'), (N'Dargeçit'), (N'Derik'), (N'Kızıltepe'), (N'Mazıdağı'), (N'Midyat'), (N'Nusaybin'), (N'Savur'), (N'Yeşilli'), (N'Ömerli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '47' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '48', v.Name FROM (VALUES (N'Bodrum'), (N'Dalaman'), (N'Datça'), (N'Fethiye'), (N'Kavaklıdere'), (N'Köyceğiz'), (N'Marmaris'), (N'Menteşe'), (N'Milas'), (N'Ortaca'), (N'Seydikemer'), (N'Ula'), (N'Yatağan')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '48' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '49', v.Name FROM (VALUES (N'Bulanık'), (N'Hasköy'), (N'Korkut'), (N'Malazgirt'), (N'Merkez'), (N'Varto')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '49' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '50', v.Name FROM (VALUES (N'Acıgöl'), (N'Avanos'), (N'Derinkuyu'), (N'Gülşehir'), (N'Hacıbektaş'), (N'Kozaklı'), (N'Merkez'), (N'Ürgüp')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '50' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '51', v.Name FROM (VALUES (N'Altunhisar'), (N'Bor'), (N'Merkez'), (N'Ulukışla'), (N'Çamardı'), (N'Çiftlik')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '51' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '52', v.Name FROM (VALUES (N'Akkuş'), (N'Altınordu'), (N'Aybastı'), (N'Fatsa'), (N'Gölköy'), (N'Gülyalı'), (N'Gürgentepe'), (N'Kabadüz'), (N'Kabataş'), (N'Korgan'), (N'Kumru'), (N'Mesudiye'), (N'Perşembe'), (N'Ulubey'), (N'Çamaş'), (N'Çatalpınar'), (N'Çaybaşı'), (N'Ünye'), (N'İkizce')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '52' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '53', v.Name FROM (VALUES (N'Ardeşen'), (N'Derepazarı'), (N'Fındıklı'), (N'Güneysu'), (N'Hemşin'), (N'Kalkandere'), (N'Merkez'), (N'Pazar'), (N'Çamlıhemşin'), (N'Çayeli'), (N'İkizdere'), (N'İyidere')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '53' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '54', v.Name FROM (VALUES (N'Adapazarı'), (N'Akyazı'), (N'Arifiye'), (N'Erenler'), (N'Ferizli'), (N'Geyve'), (N'Hendek'), (N'Karapürçek'), (N'Karasu'), (N'Kaynarca'), (N'Kocaali'), (N'Pamukova'), (N'Sapanca'), (N'Serdivan'), (N'Söğütlü'), (N'Taraklı')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '54' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '55', v.Name FROM (VALUES (N'19 Mayıs'), (N'Alaçam'), (N'Asarcık'), (N'Atakum'), (N'Ayvacık'), (N'Bafra'), (N'Canik'), (N'Havza'), (N'Kavak'), (N'Ladik'), (N'Salıpazarı'), (N'Tekkeköy'), (N'Terme'), (N'Vezirköprü'), (N'Yakakent'), (N'Çarşamba'), (N'İlkadım')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '55' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '56', v.Name FROM (VALUES (N'Baykan'), (N'Eruh'), (N'Kurtalan'), (N'Merkez'), (N'Pervari'), (N'Tillo'), (N'Şirvan')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '56' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '57', v.Name FROM (VALUES (N'Ayancık'), (N'Boyabat'), (N'Dikmen'), (N'Durağan'), (N'Erfelek'), (N'Gerze'), (N'Merkez'), (N'Saraydüzü'), (N'Türkeli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '57' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '58', v.Name FROM (VALUES (N'Akıncılar'), (N'Altınyayla'), (N'Divriği'), (N'Doğanşar'), (N'Gemerek'), (N'Gölova'), (N'Gürün'), (N'Hafik'), (N'Kangal'), (N'Koyulhisar'), (N'Merkez'), (N'Suşehri'), (N'Ulaş'), (N'Yıldızeli'), (N'Zara'), (N'İmranlı'), (N'Şarkışla')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '58' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '59', v.Name FROM (VALUES (N'Ergene'), (N'Hayrabolu'), (N'Kapaklı'), (N'Malkara'), (N'Marmaraereğlisi'), (N'Muratlı'), (N'Saray'), (N'Süleymanpaşa'), (N'Çerkezköy'), (N'Çorlu'), (N'Şarköy')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '59' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '60', v.Name FROM (VALUES (N'Almus'), (N'Artova'), (N'Başçiftlik'), (N'Erbaa'), (N'Merkez'), (N'Niksar'), (N'Pazar'), (N'Reşadiye'), (N'Sulusaray'), (N'Turhal'), (N'Yeşilyurt'), (N'Zile')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '60' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '61', v.Name FROM (VALUES (N'Akçaabat'), (N'Araklı'), (N'Arsin'), (N'Beşikdüzü'), (N'Dernekpazarı'), (N'Düzköy'), (N'Hayrat'), (N'Köprübaşı'), (N'Maçka'), (N'Of'), (N'Ortahisar'), (N'Sürmene'), (N'Tonya'), (N'Vakfıkebir'), (N'Yomra'), (N'Çarşıbaşı'), (N'Çaykara'), (N'Şalpazarı')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '61' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '62', v.Name FROM (VALUES (N'Hozat'), (N'Mazgirt'), (N'Merkez'), (N'Nazımiye'), (N'Ovacık'), (N'Pertek'), (N'Pülümür'), (N'Çemişgezek')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '62' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '63', v.Name FROM (VALUES (N'Akçakale'), (N'Birecik'), (N'Bozova'), (N'Ceylanpınar'), (N'Eyyübiye'), (N'Halfeti'), (N'Haliliye'), (N'Harran'), (N'Hilvan'), (N'Karaköprü'), (N'Siverek'), (N'Suruç'), (N'Viranşehir')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '63' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '64', v.Name FROM (VALUES (N'Banaz'), (N'Eşme'), (N'Karahallı'), (N'Merkez'), (N'Sivaslı'), (N'Ulubey')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '64' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '65', v.Name FROM (VALUES (N'Bahçesaray'), (N'Başkale'), (N'Edremit'), (N'Erciş'), (N'Gevaş'), (N'Gürpınar'), (N'Muradiye'), (N'Saray'), (N'Tuşba'), (N'Çaldıran'), (N'Çatak'), (N'Özalp'), (N'İpekyolu')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '65' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '66', v.Name FROM (VALUES (N'Akdağmadeni'), (N'Aydıncık'), (N'Boğazlıyan'), (N'Kadışehri'), (N'Merkez'), (N'Saraykent'), (N'Sarıkaya'), (N'Sorgun'), (N'Yenifakılı'), (N'Yerköy'), (N'Çandır'), (N'Çayıralan'), (N'Çekerek'), (N'Şefaatli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '66' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '67', v.Name FROM (VALUES (N'Alaplı'), (N'Devrek'), (N'Ereğli'), (N'Gökçebey'), (N'Kilimli'), (N'Kozlu'), (N'Merkez'), (N'Çaycuma')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '67' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '68', v.Name FROM (VALUES (N'Ağaçören'), (N'Eskil'), (N'Gülağaç'), (N'Güzelyurt'), (N'Merkez'), (N'Ortaköy'), (N'Sarıyahşi'), (N'Sultanhanı')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '68' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '69', v.Name FROM (VALUES (N'Aydıntepe'), (N'Demirözü'), (N'Merkez')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '69' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '70', v.Name FROM (VALUES (N'Ayrancı'), (N'Başyayla'), (N'Ermenek'), (N'Kazımkarabekir'), (N'Merkez'), (N'Sarıveliler')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '70' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '71', v.Name FROM (VALUES (N'Bahşılı'), (N'Balışeyh'), (N'Delice'), (N'Karakeçili'), (N'Keskin'), (N'Merkez'), (N'Sulakyurt'), (N'Yahşihan'), (N'Çelebi')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '71' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '72', v.Name FROM (VALUES (N'Beşiri'), (N'Gercüş'), (N'Hasankeyf'), (N'Kozluk'), (N'Merkez'), (N'Sason')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '72' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '73', v.Name FROM (VALUES (N'Beytüşşebap'), (N'Cizre'), (N'Güçlükonak'), (N'Merkez'), (N'Silopi'), (N'Uludere'), (N'İdil')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '73' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '74', v.Name FROM (VALUES (N'Amasra'), (N'Kurucaşile'), (N'Merkez'), (N'Ulus')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '74' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '75', v.Name FROM (VALUES (N'Damal'), (N'Göle'), (N'Hanak'), (N'Merkez'), (N'Posof'), (N'Çıldır')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '75' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '76', v.Name FROM (VALUES (N'Aralık'), (N'Karakoyunlu'), (N'Merkez'), (N'Tuzluca')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '76' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '77', v.Name FROM (VALUES (N'Altınova'), (N'Armutlu'), (N'Merkez'), (N'Termal'), (N'Çiftlikköy'), (N'Çınarcık')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '77' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '78', v.Name FROM (VALUES (N'Eflani'), (N'Eskipazar'), (N'Merkez'), (N'Ovacık'), (N'Safranbolu'), (N'Yenice')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '78' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '79', v.Name FROM (VALUES (N'Elbeyli'), (N'Merkez'), (N'Musabeyli'), (N'Polateli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '79' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '80', v.Name FROM (VALUES (N'Bahçe'), (N'Düziçi'), (N'Hasanbeyli'), (N'Kadirli'), (N'Merkez'), (N'Sumbas'), (N'Toprakkale')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '80' AND x.Name = v.Name);
INSERT dbo.Districts (ProvinceCode, Name)
SELECT '81', v.Name FROM (VALUES (N'Akçakoca'), (N'Cumayeri'), (N'Gölyaka'), (N'Gümüşova'), (N'Kaynaşlı'), (N'Merkez'), (N'Yığılca'), (N'Çilimli')) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Districts x WHERE x.ProvinceCode = '81' AND x.Name = v.Name);

COMMIT TRANSACTION;
GO
