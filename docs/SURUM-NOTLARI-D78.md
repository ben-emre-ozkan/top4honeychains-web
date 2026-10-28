# TOP4HoneyChains – sürüm notları, D-78 sürümü

**Tarih:** 28.10.2026 · **Kararlar:** D-78 – D-85 ([DECISIONS.md](DECISIONS.md)) · **Kılavuz:** beş dilde, 28 Ekim 2026 baskısı · **Kurulum:** [GO-LIVE.md](GO-LIVE.md), paket `TOP4-guncelleme-2026-10-28`

Bu sürüm arıcı kaydını, arıcının onayladığı mühürlü kaplarla teslimi, her kaptan numuneyi, Uyum Sorumlusunu, aydınlatma metinleri ve rızaları ve arıcı portalını getirir. Dışarıda hazırlık gerektiren bölümler, her biri hazır olana kadar **kapalı** kalır ("Anahtarlar" bölümüne bakın).

## Herkes için

- **SMS ile giriş bilgileri.** Yeni hesaplara kullanıcı adı ve geçici şifre SMS ile gider (72 saat geçerli); ilk girişte kişi kendi şifresini seçer. Yeni şifreler en az 10 karakter olmalıdır. Unutulan şifre, doğrulanmış cep telefonuna gelen kodla yenilenir.
- **Aydınlatma metni.** Kullanıcılar platformun ve çalıştıkları her kurumun aydınlatma metnini okuyup onaylar; kurum adına imza atan roller SMS koduyla onaylar. Metinler onaylanana kadar yalnızca bir bant hatırlatır, hiçbir şey engellenmez.
- **İlk kullanım turu ve açıklayıcı animasyonlar.** Her rol için kısa bir tur ve bir düğmenin ne yaptığını gösteren kısa sahneler; ikisi de profil menüsünden (**Animasyonları göster**) kapatılabilir.
- **Daha açık metinler.** "Kalite Güvence Sorumlusu" rolü, "Temel kurallar" kutusu, yetenek olarak yazılmış kurum yetenekleri, mesajlarda rol kodu yerine rol adı.

## Yeni roller ve sayfalar

- **Arıcı Kayıt Görevlisi** (ilçe tarım müdürlüğü): arıcıları ve arılıklarını kaydeder, kayıt numarasını kontrol eder, aydınlatma metnini ve rızaları kaydeder; arıcı bunu SMS koduyla ya da imzalı fotoğrafla onaylar.
- **Uyum Sorumlusu** (her kurumda): imzalı fotoğrafları, reddedilen numuneleri ve kırık mühürleri inceler, bekleyen ve kendi kendine yapılan onayları izler, ilgili kişi başvurularını kaydeder (30 gün süre), veri sorumlusu bilgilerini ve sözleşmeleri tutar, kurumun denetim kayıtlarını okur.
- **Arıcı portalı** (açıldığında): arıcının kaplar ve mühürlerle teslimleri, numune kavanozları ve onaylı laboratuvar sonuçları, payıyla birlikte partileri, Bal Hikâyesi sayfaları, her teslim için istediği fiyat, rızaları ve hesabı.

## Gözetim zinciri

- **Kaplarla teslim** (açıldığında): her kap ağırlığı ve mühürleriyle, her kaptan kurcalamaya dayanıklı kapaklı, mühürlü bir numune ve şahit numune kavanozu, QR kodlu etiketler ve tam bu içeriği bağlayan arıcı SMS kodu. SMS yoksa Uyum Sorumlusunun kabul ettiği imzalı fotoğraf.
- **Laboratuvar kabulü** her kavanozun mühür numarasını, etiketini ve kapağını kontrol eder; bir kontrol tutmazsa kavanoz reddedilir ve tesis aynı kaptan yeni kavanoz alır. Laboratuvar kavanozlardan bir karışım numunesini (varsayılan) ya da her kavanozu ayrı analiz eder; ayrı analizde lot her parametrenin en kötü değerini alır.
- **Teslim düzeltmeleri** iki platform yöneticisiyle yapılır; arıcıya SMS ile bilgi gider.
- **Geri çağırma raporu** kapları, mühürleri ve kavanozları gösterir; üye kurum yalnızca kendi elindeki miktarı görür.

## Gizlilik ve güvenlik

- Herkese açık sayfalar arıcıyı **yalnızca ad rızasıyla** adıyla gösterir; yoksa nötr kod görünür. Bu sürümden önce elle yazılmış arılık adları (çoğu arıcının adını içerir) ve fotoğraf açıklamaları da aynı kurala uyar.
- Kanıt dosyalarını (imzalı formlar, kayıt fotoğrafları, rıza ve kurum belgeleri) yalnızca ihtiyacı olan roller indirir; her indirme denetim kaydına yazılır.
- Herkese açık **Laboratuvar raporu doğrula** sayfası yalnızca onaylı raporları bulur.
- Sayfalar yalnızca menüde değil, açıldıklarında da rolün okuma hakkını kontrol eder; numune alma, sevk ve kesinleştirme kurumun yeteneğini de kontrol eder.
- Merkez birlik ayarı açıkken birlikleri yalnızca merkez birlik doğrular.
- **Veri saklama**: süreler tanımlıdır; deneme raporu süresi dolanları sayar. Sahip ilk çalıştırmayı onaylayana kadar hiçbir şey silinmez.
- Görüntü kütüphanesi artık SkiaSharp (öncekisi sürüm derlemeleri için ticari lisans anahtarı istiyordu).

## Anahtarlar (kurulumda hepsi kapalı)

| Anahtar | Ne zaman açılır |
|---|---|
| SMS sağlayıcı (`Sms:Provider`) | SMS paketi alındığında ve gönderici adı onaylandığında |
| Kaplarla teslim (`Features:CustodyIntake`) | tesislerde kurcalamaya dayanıklı kavanozlar, mühürler ve etiketler hazır olduğunda; SMS açık |
| Aydınlatma onayı zorunlu (`Notices:Enforce`) | metinler KVKK birimince onaylandığında |
| Arıcı portalı (`Features:BeekeeperAccounts`) | proje kararı verildiğinde; SMS açık |
| Veri saklama çalıştırması (`Retention:ApplyEnabled`) | sahip ilk çalıştırmayı ve takma ad biçimini onayladığında |

## Veritabanı

`016`–`028` betikleri; yalnızca ekleme yapar ve tekrar çalıştırılabilir (ikinci çalıştırma hiçbir şeyi değiştirmez). 8 Ekim 2026 tarihli canlı veritabanı kopyası üzerinde denendi.
