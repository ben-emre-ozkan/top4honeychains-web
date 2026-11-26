# TOP4HoneyChains – Sunucu kurulum ve güncelleme rehberi

Bu rehber sunucuyu yöneten kişi içindir. İki bölümü vardır:

- **A) Canlı sunucuyu güncelleme**: çalışan sitenin yeni sürüme geçirilmesi.
- **B) Yeni kurulum**: boş bir veritabanı üzerinde yepyeni bir sistem kurulması.

Her şey bu depodan ya da sürüm paketinden yapılır; başka bir dosya veya yedek gerekmez. Ayrıntılı kontrol listesi ve
gerekçeler depodaki `docs/GO-LIVE.md` dosyasındadır (İngilizce; pakette yoktur, bu rehber tek başına yeterlidir).

**Sürüm paketiyle çalışıyorsanız** (ör. `TOP4-surum-2026-11-26`): Derleme gerekmez; `<YAYIN>` paketteki `Publish` klasörü,
`<DEPO>` paketin klasörüdür, `db\migrations` yerine `SQL\migrations`, `db\install` yerine `SQL\install` kullanılır. 0. bölümün
derleme adımlarını (Git, SDK, `dotnet publish`) atlayın; önce paketteki `ONCE-OKU.pdf` dosyasını okuyun.

Komutlardaki yer tutucular:

| Yer tutucu | Anlamı | Örnek |
|---|---|---|
| `<SUNUCU>` | SQL Server adı | `VTSUNUCU\MSSQLSERVER2019` |
| `<VERITABANI>` | Veritabanı adı | `TOP4HoneyChainsDb` |
| `<SITE>` | IIS sitesinin klasörü | `C:\inetpub\top4honeychains` |
| `<DEPO>` | Deponun indirildiği klasör | `C:\TOP4\depo` |
| `<YAYIN>` | Derlenmiş uygulamanın klasörü | `C:\TOP4\yayin` |

Komutlar PowerShell içindir. `sqlcmd` komutlarındaki seçenekler: `-E` Windows oturumuyla bağlanır, `-I` tırnaklı adları
açar (bu olmadan bazı betikler hata verir), `-f 65001` Türkçe harfleri doğru okur (UTF-8), `-b` hata olursa durur.
SQL kullanıcısıyla bağlanmanız gerekirse `-E` yerine `-U <kullanıcı>` yazın; parolayı komuta yazmayın, `sqlcmd` sorar.

## 0. Hazırlık (her iki bölüm için)

1. Derleme yapılacak bilgisayarda Git ve **.NET 8 SDK** kurulu olsun. Sunucuda **ASP.NET Core 8 Hosting Bundle**,
   **SQL Server 2019 veya üstü** ve `sqlcmd` kurulu olsun. Sunucu Windows olmalıdır.
2. Depoyu indirin ve yayınlanacak sürüme geçin (sürüm etiketi sahibi tarafından verilir):

   ```powershell
   git clone <depo adresi> <DEPO>
   cd <DEPO>
   git checkout <sürüm etiketi>
   ```

3. Uygulamayı derleyin ve geliştirici ayar dosyasını yayından çıkarın:

   ```powershell
   dotnet publish TOP4HONEYCHAINS\TOP4HoneyChains.Web -c Release -o <YAYIN>
   Remove-Item <YAYIN>\appsettings.Development.json
   ```

   Sonuç 0 hata ile bitmelidir. `<YAYIN>` klasöründe `TOP4HoneyChains.Web.exe` ve `web.config` bulunur.

## A) Canlı sunucuyu güncelleme

### A1. Yedek

1. Veritabanının tam yedeğini alın ve yedeğin okunabildiğini kontrol edin. Yedek sunucuda kalır; depoya veya pakete konmaz.

   ```powershell
   sqlcmd -S <SUNUCU> -E -b -Q "BACKUP DATABASE [<VERITABANI>] TO DISK = N'D:\Yedek\TOP4-oncesi.bak' WITH COPY_ONLY, CHECKSUM, INIT"
   sqlcmd -S <SUNUCU> -E -b -Q "RESTORE VERIFYONLY FROM DISK = N'D:\Yedek\TOP4-oncesi.bak' WITH CHECKSUM"
   ```

2. Site klasörünün tamamını yedekleyin. Yüklenen dosyalar yalnızca sunucuda vardır: `App_Data` (KKN dosyaları, kurum
   belgeleri, teslim formları, laboratuvar raporları, onaylar, kayıt fotoğrafları, hata kayıtları) ve
   `wwwroot\assets\videos\parameters`.

   ```powershell
   robocopy <SITE> D:\Yedek\site-oncesi /E
   ```

### A2. Bakım modu

Site klasörüne `app_offline.htm` adlı bir dosya koyun (içine "Bakım çalışması yapılıyor" gibi kısa bir metin yazın).
Dosya durdukça site ziyaretçilere bu sayfayı gösterir ve uygulama dosyaları serbest kalır.

### A3. Veritabanı betikleri

`db\migrations` klasöründeki betikler numara sırasıyla çalıştırılır. Her betik tekrar çalıştırılabilir (ikinci kez
çalıştırıldığında bir şey değiştirmez), ama **hangi betiklerin çalıştırıldığını bir deftere yazın**.

- **Nereden başlanır?** Canlı veritabanında şu sorguyu çalıştırın ve ilk boş (NULL) satırın numarasından başlayın:

  ```sql
  SELECT '004' AS Betik, OBJECT_ID('dbo.Capabilities') AS Var UNION ALL SELECT '018', OBJECT_ID('dbo.SmsMessages')
  UNION ALL SELECT '029', OBJECT_ID('dbo.ConsumerAccounts') UNION ALL SELECT '033', OBJECT_ID('dbo.SealCodes')
  UNION ALL SELECT '036', OBJECT_ID('dbo.ContainerEmptyings');
  ```

  9 Ekim paketiyle güncellenmiş bir site `018`'den, 28 Ekim paketiyle güncellenmiş bir site `029`'dan başlar. Bütün
  betikler tekrar çalıştırılabilir; emin değilseniz `002`'den başlamak da güvenlidir.
- **`001_lock_out_non_admin_accounts.sql` eski sisteme ilk geçişe aittir.** Kurum modeli (`004`) kurulmuş bir veritabanında
  artık kendiliğinden atlanır ("Skipped: … 001 runs only once" yazar) ve hiçbir hesabı kilitlemez; yine de çalıştırmanız gerekmez.

Bir betiği tek başına çalıştırmak için (her betik için aynı komut, yalnızca dosya adı değişir):

```powershell
cd <DEPO>
sqlcmd -S <SUNUCU> -d <VERITABANI> -E -I -f 65001 -b -i db\migrations\002_ccp_integrity.sql
```

Belli bir numaradan başlayarak sonuna kadar hepsini sırayla çalıştırmak için (`$ilk` değerini ilk çalıştırılacak betiğin
numarasına ayarlayın: canlı sitede yukarıdaki sorgunun sonucu, yeni kurulumda `1`). Bir betik hata verirse döngü durur; hatayı not edip sahibine bildirin, sonrakilere geçmeyin.

```powershell
cd <DEPO>
$ilk = 18   # nereden başlanacağı yukarıdaki sorguyla bulunur
Get-ChildItem db\migrations\*.sql | Sort-Object Name | Where-Object { [int]$_.Name.Substring(0,3) -ge $ilk } | ForEach-Object {
    Write-Host "== $($_.Name)"
    sqlcmd -S <SUNUCU> -d <VERITABANI> -E -I -f 65001 -b -i $_.FullName
    if ($LASTEXITCODE -ne 0) { Write-Host "HATA: $($_.Name)"; break }
}
```

Betikler ve beklenen çıktılar:

| Betik | Ne yapar | Not |
|---|---|---|
| `001_lock_out_non_admin_accounts.sql` | Yönetici olmayan eski hesapları kilitler | Yalnızca eski sisteme ilk geçişte; `004` kurulduysa atlar. |
| `002` – `008` | KKN düzeni, kurum modeli, sorumluluklar, teslim, laboratuvar, köken | `004`, `005`, `006`, `007`, `008` bir öncekine bağlıdır; sıra bozulursa durur. |
| `009` – `011` | Çeviriler, iller ve ilçeler | Türkçe harfler için `-f 65001` şarttır. |
| `012` – `017` | Türkçe adlar, yetkiler, koşullu alanlar, dizinler, metinler | |
| `018` – `028` | D-78 sürümü: roller, SMS kodları, bildirimler, arıcı hesapları, kaplar, fiyatlar, uyum, saklama | |
| `029` | Tüketici hesapları | "maximum key length" uyarısı beklenir, hata değildir. |
| `030` – `032` | Parti yetkisi, metinler | |
| `033_d97_seal_pool_containers.sql` | Mühür havuzu ve kurum kapları | Sonrasında her teslim alan kurumun mühür etiketi olmalı (A6). |
| `034_d98_ccp_paths.sql` ve sonrakiler | D-98: KKN yolları, mühürlerin boşaltılması | Yeni betikler bu sırayla eklenir. |

Betiklerin her birinin ayrıntılı açıklaması GO-LIVE.md "Deployment day", adım 4'tedir.

### A4. Uygulama dosyalarını kopyalama

Sunucuya özel dosyalar korunarak yeni sürüm kopyalanır: `web.config` (sunucu ayarlarını taşır),
`appsettings.Production.json` (kullanılıyorsa), `App_Data` ve yüklenen videolar üzerine yazılmaz.

```powershell
robocopy <YAYIN> <SITE> /E /XF web.config appsettings.Production.json appsettings.Development.json /XD App_Data parameters
```

`<SITE>` içinde `appsettings.Development.json` kalmışsa silin.

### A5. Sunucu ayarları

Ayarlar depodan gelmez; sunucuda `web.config` içindeki `<environmentVariables>` bölümünde (veya site klasöründeki
`appsettings.Production.json` dosyasında) tutulur. Bu dosyalar hiçbir zaman depoya kopyalanmaz. Paketteki `Publish` klasöründe
`appsettings*.json` yoktur: Sunucudakiler aynen kalır. Sunucuda hiç `appsettings.json` yoksa günlük ayarını da verin:
`Logging__LogLevel__Default=Warning` (yoksa günlük her veritabanı komutunu yazar).

**Dört özellik anahtarı** (canlıda dördü de açık):

| Ortam değişkeni | Değer |
|---|---|
| `Features__OrgRoleModel` | `true` |
| `Features__CustodyIntake` | `true` |
| `Features__BeekeeperAccounts` | `true` |
| `Features__ConsumerAccounts` | `true` |

**Zorunlu ayarlar**:

| Ortam değişkeni | Açıklama |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Veritabanı bağlantısı, ör. `Server=<SUNUCU>;Database=<VERITABANI>;Trusted_Connection=True;TrustServerCertificate=True` |
| `App__PublicBaseUrl` | Sitenin genel adresi, ör. `https://<alan adı>` (SMS bağlantıları, QR kodlar, e-posta) |
| `AllowedHosts` | Sitenin alan adları, ör. `<alan adı>;www.<alan adı>` |
| `Sms__Provider` | `Netgsm` veya `IletiMerkezi` (paket alınana kadar `Logging`: SMS gönderilmez, yalnızca kayda yazılır) |
| `Sms__CodeKey` | 32 rastgele baytın base64 hali; **bu olmadan uygulama açılmaz**. Bir kez üretilir ve değiştirilmez. Üretmek için: `$b = New-Object byte[] 32; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)` |
| `Sms__Sender` | Sağlayıcıda kayıtlı gönderici adı (en çok 11 karakter) |
| `Sms__DailyLimit`, `Sms__AllowedCountries` | ör. `100` ve `TR` |
| `Sms__Netgsm__UserCode`, `Sms__Netgsm__Password` veya `Sms__IletiMerkezi__Key`, `Sms__IletiMerkezi__Hash` | Sağlayıcının bilgileri |
| `SmtpSettings__Host`, `__Port`, `__EnableSsl`, `__UserName`, `__Password`, `__From`, `__SenderName` | E-posta sunucusu |
| `Compliance__GraceStart` | Canlıya geçiş tarihi, ör. `2026-12-01` |
| `PublicNames__LegacyMode` | Sahibin kararı: `HideUntilConsent` (varsayılan) veya `ShowUntilReverified` |
| `OrgModel__AssociationVerifier` | `PlatformAdmin` (varsayılan) veya `CentralAssociation` |

İsteğe bağlı: `GoogleMaps__ApiKey`, `Branding__IntalaLabUrl`, `Landing__DemoHoneyStoryQr`. Kapalı kalması gerekenler:
`Notices__Enforce`, `Retention__ApplyEnabled`, `Retention__Schedule` (yazmayın veya `false`), `Demo__Enabled` (yazmayın).

`web.config` örneği (yalnızca sunucuda; değerleri kendiniz doldurun):

```xml
<aspNetCore processPath=".\TOP4HoneyChains.Web.exe" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
    <environmentVariable name="ConnectionStrings__DefaultConnection" value="Server=...;Database=...;Trusted_Connection=True;TrustServerCertificate=True" />
    <environmentVariable name="App__PublicBaseUrl" value="https://..." />
    <environmentVariable name="AllowedHosts" value="..." />
    <environmentVariable name="Features__OrgRoleModel" value="true" />
    <environmentVariable name="Features__CustodyIntake" value="true" />
    <environmentVariable name="Features__BeekeeperAccounts" value="true" />
    <environmentVariable name="Features__ConsumerAccounts" value="true" />
    <environmentVariable name="Sms__Provider" value="..." />
    <environmentVariable name="Sms__CodeKey" value="..." />
    <!-- diğer ayarlar yukarıdaki tablodaki gibi -->
  </environmentVariables>
</aspNetCore>
```

IIS uygulama havuzu: ".NET CLR sürümü" = **Yönetilen Kod Yok**, "Kullanıcı Profilini Yükle" = **True** (oturum anahtarları
yeniden başlatmadan sonra da geçerli kalır). Havuz hesabı `App_Data` klasörüne yazabilmelidir.

### A6. Malzeme ve kişiler

Canlıya geçmeden önce (ayrıntı: GO-LIVE.md "People and material"):

- Her rol için kendi dilinde kullanıcı kılavuzu (kılavuzlar `docs\manual` içinden üretilir).
- Teslim alan her kurum için: **VOID (sökülünce iz bırakan) etiket** (100 × 50 mm, A4'te 2 × 5 veya tek etiket) ve etiket
  yazıcısı veya A4 etiket yaprakları; numaralı **plastik mühürler**; **güvenlik düğmeli (vakumlu) veya halkalı kapaklı
  numune kavanozları**; düğmeli kapaklar için **el vakum pompası** (bal vakum için asla ısıtılmaz). Resepsiyon için USB QR
  okuyucu isteğe bağlıdır.
- Her teslim alan kurumun yöneticisi ilk teslimden önce **Teslim almalar > Mühür havuzu ve kaplar** sayfasında mühür etiketi
  üretip basar, plastik mühürleri numara aralığı olarak ekler ve derneğin kalıcı kaplarını kaydeder.

### A7. Bakım modunu kaldırma ve duman testi

1. `<SITE>\app_offline.htm` dosyasını silin.
2. Kontrol edin (ayrıntılı liste: GO-LIVE.md bölüm 3):
   - Yönetici girişi çalışıyor; ana sayfa ve her menü sayfası açılıyor.
   - `http://<alan adı>/` adresi `https://` adresine yönleniyor (Cloudflare kullanılıyorsa B5a: "Full (strict)", asla "Flexible").
   - Bir onaylı partinin Bal Hikâyesi (`/honeystory/...`) açılıyor; QR kodu bu adrese gidiyor.
   - **Veri saklama** sayfası (/Retention) "Kapalı" diyor.
   - Denetim kaydında (`/Audit`) "Hash zincirini doğrula" bütün satırların doğrulandığını söylüyor.
   - Veritabanında `SELECT is_disabled FROM sys.triggers WHERE name = 'TR_AuditEntries_AppendOnly'` sonucu `0`.
   - `/phone/deneme/905550000101` adresi 404 veriyor.
   - Veritabanı kullanıcıları gözden geçirildi: Uygulamanın hesabı `db_owner` değil; eski `DefaultAppPool` ve `apiaryphotoapi`
     kullanıcıları (8 Ekim teslimindeki veritabanında `db_owner`) artık gerekmiyorsa kaldırıldı.

### A8. Geri dönüş

1. Site klasörünü A1'deki yedekten geri yükleyin (`robocopy D:\Yedek\site-oncesi <SITE> /MIR`).
2. Veritabanı yedeğini yalnızca bir betik veriye zarar verdiyse geri yükleyin. Betikler yalnızca ekleme yaptığı için eski
   uygulama yeni şemada çalışır.
3. Değiştirilen parolalar ve anahtarlar eski haline döndürülmez.

### A9. Parametre videoları (Bal Hikâyesi)

Bal Hikâyesi'nde bir laboratuvar değerine dokunulunca o parametrenin kısa videosu oynar. Videolar paketteki
`Videos\` klasöründedir (Türkçe ve İngilizce, 30 dosya); uygulamanın içinde değildir, ayrıca kopyalanır.

1. Paketteki `Videos` klasöründe yönetici olarak PowerShell açın: `cd <DEPO>\Videos`.
2. Şu komutu çalıştırın (uygulama klasörünü kendi yolunuzla değiştirin):

   ```
   powershell -ExecutionPolicy Bypass -File .\ImportParameterVideos.ps1 -Source . -Target "C:\inetpub\TOP4HoneyChains\App_Data\parameter-videos"
   ```

3. Komut "Copied 30 video(s)" yazmalıdır. `es`, `pl` ve `lv` için "no videos" satırı normaldir: Bu dillerde İngilizce video oynar.
4. Uygulama havuzunun kullanıcısı `App_Data\parameter-videos` klasörünü okuyabilmelidir (App_Data'nın diğer klasörleri gibi).
5. Kontrol: Bir Bal Hikâyesi açın, bir laboratuvar değerine dokunun; açılan pencerede video oynamalıdır.

Videoları başka bir klasörde tutmak isterseniz `appsettings.Production.json` dosyasına `"Videos": { "Root": "D:\\Videolar\\parameter-videos" }` yazın ve komuttaki `-Target` yolunu buna göre verin. Video yoksa sayfa hata vermez; pencerede yalnızca açıklama görünür.

## B) Yeni kurulum (boş veritabanı)

Bu bölüm, içinde hiç veri olmayan yeni bir sistem içindir. **Canlı veritabanında çalıştırmayın**: `db\install` betikleri
içinde hesap bulunan bir veritabanını reddeder.

### B1. Boş veritabanı

```powershell
sqlcmd -S <SUNUCU> -E -b -Q "CREATE DATABASE [<VERITABANI>]"
sqlcmd -S <SUNUCU> -E -b -Q "ALTER DATABASE [<VERITABANI>] SET COMPATIBILITY_LEVEL = 150"
```

### B2. Şema ve başlangıç verileri (`db\install`)

```powershell
cd <DEPO>
sqlcmd -S <SUNUCU> -d <VERITABANI> -E -I -f 65001 -b -i db\install\01_schema.sql
sqlcmd -S <SUNUCU> -d <VERITABANI> -E -I -f 65001 -b -i db\install\02_reference_data.sql
```

- `01_schema.sql` bütün tabloları, anahtarları, dizinleri ve tetikleyicileri kurar. "The maximum key length for a clustered
  index is 900 bytes" uyarıları beklenir, hata değildir.
- `02_reference_data.sql` yalnızca başvuru verilerini yükler: KKN'ler ve parametreleri, sınırlar, bal türleri, durumlar,
  yetenekler ve roller, yetkiler, iller ve ilçeler, çeviriler ve taslak bildirim metinleri. Kişi, hesap, arıcı, kurum, parti
  veya dosya **içermez**. Her tablo için kaç satır eklendiğini yazar.
- İkisi de tekrar çalıştırılabilir; ikinci kez çalıştırıldığında bir şey eklemez.

### B3. Bütün veritabanı betikleri

`db\install` en son betikten sonraki şemayı içerir; yine de **bütün** `db\migrations` betiklerini sırayla çalıştırın (A3'teki
döngü, `$ilk = 1`). Böylece `db\install` dosyalarından sonra eklenmiş betikler de uygulanır ve platform kurumu ("Platform
administration") oluşturulur. Beklenen çıktılar:

- `001`: "No user accounts yet (new installation); nothing to lock." (henüz hesap yok; normaldir).
- `004`: "Platform organization created" (yalnızca ilk çalıştırmada).
- `029`: Yeni kurulumda uyarı vermez (aynı uyarıyı B2'de `01_schema.sql` zaten verir).

**Bundan sonra `001` bu veritabanında bir daha çalıştırılmaz** (A3'teki uyarı). Sonraki güncellemeler A bölümüne göre yapılır.

### B4. Uygulamanın veritabanı hesabı

Uygulama veritabanına IIS uygulama havuzunun hesabıyla bağlanır. Bu hesaba yalnızca okuma ve yazma yetkisi verin; denetim
kaydının değiştirilmesini ve silinmesini yasaklayın (`<HESAP>`: aynı sunucuda `IIS APPPOOL\<havuz adı>`, SQL Server başka
bir sunucudaysa `<ETKİ ALANI>\<web sunucusu adı>$`):

```powershell
sqlcmd -S <SUNUCU> -d <VERITABANI> -E -I -b -Q "CREATE LOGIN [<HESAP>] FROM WINDOWS; CREATE USER [<HESAP>] FOR LOGIN [<HESAP>]; ALTER ROLE db_datareader ADD MEMBER [<HESAP>]; ALTER ROLE db_datawriter ADD MEMBER [<HESAP>]; DENY UPDATE, DELETE ON dbo.AuditEntries TO [<HESAP>];"
```

(Giriş sunucuda zaten varsa `CREATE LOGIN` bölümünü çıkarın.) Bu hesap `db_owner` olmamalıdır.

### B5. Site

1. IIS'te yeni bir site ve uygulama havuzu oluşturun (havuz ayarları A5'in sonunda). HTTPS bağlantısı ve sertifika üniversite
   bilgi işlemi tarafından yapılır (GO-LIVE.md "Request to the university IT").
2. `<YAYIN>` klasörünün içeriğini `<SITE>` klasörüne kopyalayın: `robocopy <YAYIN> <SITE> /E`
3. `<SITE>\App_Data` klasörünü oluşturun ve uygulama havuzu hesabına yazma izni verin.
4. `<SITE>\web.config` dosyasına A5'teki ayarları yazın (dört anahtar açık, `Sms__CodeKey` dahil bütün zorunlu ayarlar).

### B5a. Cloudflare (alan adı Cloudflare üzerindeyse)

Site Cloudflare'in arkasındaysa HTTPS iki parçadır: Ziyaretçi ile Cloudflare arası ve Cloudflare ile sunucu arası.

1. Cloudflare panelinde **SSL/TLS** modu **"Full (strict)"** olmalıdır. **"Flexible" asla seçilmez:** Uygulama HTTP isteğini
   HTTPS'e yönlendirir; Flexible modda Cloudflare sunucuya HTTP ile gittiği için sayfa sonsuz bir yönlendirme döngüsüne girer.
2. Sunucuda IIS sitesinin **443** bağlantısına geçerli bir sertifika bağlanır. Cloudflare'in ücretsiz **Origin CA** sertifikası
   yeterlidir (Cloudflare > SSL/TLS > Origin Server > Create Certificate; sertifikayı ve anahtarı PFX olarak sunucuya alıp
   IIS > Server Certificates > Import ile yükleyin, sonra site > Bindings > https 443'e seçin).
3. **80** bağlantısı da kalır (uygulama oradan gelen isteği HTTPS'e yönlendirir).
4. `App:PublicBaseUrl` sitenin `https://` adresidir; `AllowedHosts` alan adını içerir.
5. Kontrol: `http://<alan adı>/` tarayıcıda tek bir yönlendirmeyle `https://` adresine gider; "too many redirects" hatası
   görülürse SSL/TLS modu yanlıştır.

### B6. İlk platform yöneticisi

İlk yönetici, uygulamanın kendi komutuyla bir kez oluşturulur. Komut yalnızca **hiç yönetici olmayan** bir veritabanında
çalışır; parolayı iki kez sorar ve ekranda göstermez (parola hiçbir dosyaya, komut satırına veya depoya yazılmaz); uygulamanın
parola kurallarını uygular (en az 10 karakter, kullanıcı adını içermez, yaygın bir parola değildir); hesabı **Administrator**
rolüne ve platform kurumuna **Kurum yöneticisi** (OrgAdmin) olarak ekler; denetim kaydına yazar ve web sunucusunu başlatmadan
kapanır. SMS ayarlarına ihtiyaç duymaz; yalnızca veritabanı bağlantısını kullanır.

Sunucuda PowerShell açın (veritabanında yazma yetkisi olan kendi Windows hesabınızla) ve:

```powershell
cd <SITE>
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__DefaultConnection = "Server=<SUNUCU>;Database=<VERITABANI>;Trusted_Connection=True;TrustServerCertificate=True"
.\TOP4HoneyChains.Web.exe create-first-admin --user <kullanıcı adı> --phone +905XXXXXXXXX --email <e-posta adresi>
```

- `--phone` zorunludur (uluslararası biçimde; `0532…` yazılırsa Türkiye numarası sayılır), `--email` isteğe bağlıdır.
- `Password:` ve `Password again:` sorularına parolayı yazın; yazdıklarınız görünmez.
- Başarılı olunca "The administrator … was created with an Organization admin membership in "Platform administration"." yazar.
- Bağlantı ayarı **mutlaka** bu pencerede verilmelidir: `web.config` içindeki ayarlar bu komuta ulaşmaz. Verilmezse komut
  uygulama klasöründe bir `appsettings.json` varsa oradaki bağlantıyı, yoksa hiçbir bağlantıyı kullanmaz ve hata verir.
- Komutu sunucunun kendi penceresinde (masaüstü veya uzak masaüstü) çalıştırın; parola klavyeden okunduğu için uzaktan komut
  çalıştıran araçlarla (ör. girdi yönlendirilen bir oturumda) çalışmaz.
- `cmd` kullanıyorsanız da adı `.\TOP4HoneyChains.Web.exe` diye yazın: bazı sunucularda bulunduğunuz klasördeki program yalnızca
  adıyla bulunmaz.
- İşiniz bitince PowerShell penceresini kapatın.

Komutun reddettiği durumlar (hiçbir şey oluşturulmaz): zaten bir yönetici var; iki parola farklı; parola kurallara uymuyor;
telefon numarası geçersiz; komut satırında `--password` verilmiş; parola bir dosyadan veya borudan okunmaya çalışılmış.
Sonraki yöneticiler ve bütün kullanıcılar uygulama içinden eklenir (**Kullanıcı Yönetimi**, kurum sayfasında **Üye ekle**).

### B7. İlk giriş ve ilk kurumlar

1. Siteyi başlatın ve ilk yöneticiyle giriş yapın. **Profil** sayfasında telefon numarasını doğrulayın (parola sıfırlama SMS ile
   bu numaraya gelir; gerçek bir SMS sağlayıcısı gerekir).
2. **Kurulum > Kurumlar**: ilk günden çalışacak kurumları oluşturun (ilçe tarım müdürlükleri **Arıcı kaydı** ile, dernekler ve
   tesisler **İşleme** ve **Paketleme** ile, laboratuvarlar **Laboratuvar** ve akreditasyonlarıyla, toptancılar ve
   perakendeciler). Her kurum için en az bir belge yükleyin, **Uyum Sorumlusu**nu atayın ve **Doğrula**'yı seçin. Yalnızca
   doğrulanmış bir kurum veri girer.
3. Platform kurumunun ve her kurumun veri sorumlusu bilgilerini kurum sayfasında doldurun.
4. **Kurulum > Referans veriler > Üretim Dönemleri**: en az bir hasat dönemi oluşturun (yeni kurulumda dönem yoktur; teslimlerde dönem seçilir).
5. **Kurulum > Aydınlatma ve rıza metinleri**: kişisel verilerin korunması biriminin onayladığı metinleri "Taslak" işareti kaldırılarak
   yeni sürüm olarak yayımlayın (yeni kurulumda yalnızca taslaklar vardır).
6. Kurum yöneticilerini **Üye ekle** ile ekleyin (giriş bilgileri SMS ile gider). Her teslim alan kurumun yöneticisi mühür
   havuzunu hazırlar (A6).
7. Parametre videolarını A9'daki gibi yükleyin.
8. A7'deki kontrolleri yapın.

### B8. Bu rehberin denenmesi

Bu bölüm (B), deponun temiz bir kopyası ve boş bir yerel veritabanı (LocalDB) ile adım adım denendi: yayın `-c Release`,
`db\install`, bütün `db\migrations`, ilk yönetici komutu, uygulama `Production` ortamında ortam değişkenleriyle (dört anahtar
açık, `Sms__Provider=Logging`, deneme amaçlı bir `Sms__CodeKey`), giriş, bir kurumun oluşturulup doğrulanması ve bütün menü
sayfaları. IIS ve HTTPS adımları (B4 hesap, B5) yerel denemede yapılamaz; uygulama doğrudan `TOP4HoneyChains.Web.exe --urls`
ile çalıştırıldı.

Denemede bulunan ve düzeltilen eksikler:

- Deponun temiz kopyası derlenmiyordu: proje dosyası depoda olmayan `node_modules` dosyalarını sayıyordu (kaldırıldı).
- `023` betiği ikinci kez çalıştırıldığında (veya `db\install` sonrasında) `034`'ün kaldırdığı dar numune türü kuralını geri
  getiriyordu; parti numuneleri reddedilirdi (düzeltildi).
- `001` betiği hesabı olmayan yeni bir veritabanında durup hata veriyordu (artık bir mesaj yazıp geçiyor).
- `db\install` ikinci kez çalıştırıldığında adı sistem tarafından verilen bazı kuralları çift ekliyordu (düzeltildi).
- Yeni kurulumda hasat dönemi yoktur: B7 adım 4 eklendi.
