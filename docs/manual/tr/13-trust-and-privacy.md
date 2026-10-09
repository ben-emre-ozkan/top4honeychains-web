# 13. Güven ve gizlilik {#trust}

## Denetim kaydı {#trust-audit}

Platformdaki her değişiklik (her kayıt, onay, kesinleştirme, sevkiyat, oturum açma, yetki değişikliği) denetim kaydına yazılır. Platform yöneticileri bu kaydı **Kullanıcı Yönetimi** → **Denetim kaydı** menüsünden açar.

Her satır şunları gösterir:

- **Ne zaman**: yerel saat; UTC saatini görmek için fareyle üzerine gelin;
- **Kim**: kullanıcı ve kurum;
- **İşlem**: örneğin Oluştur, Güncelle, Onayla, Kesinleştir, Doğrula, Giriş; "Kendi onayladı" gibi işaretlerle birlikte;
- **Kayıt**: hangi kayıt olduğu; bir yönetim sayfası varsa bağlantısıyla birlikte;
- **Değişiklikler**: değişen her alan **eski → yeni** biçiminde, alan adı sizin dilinizde; kullanıcı ve kurum kimlikleri adlarıyla gösterilir;
- **Hash**: satırın parmak izinin başlangıcı.

**Ham veriyi göster**, saklanan veriyi yazıldığı haliyle açar (eski ve yeni değerler).

Kayıt türüne, işleme, kullanıcı adına veya işarete (örneğin `SelfApproved`) göre süzün ve **Filtrele** düğmesine tıklayın.

<!-- SHOT: audit-log -->

### Yalnızca ekleme ve hash zinciri {#trust-hash-chain}

Denetim kaydına yalnızca ekleme yapılabilir. Veritabanı, bir satırı değiştirme veya silme girişimlerini reddeder. Her satır, kendi içeriğinin ve bir önceki satırın SHA-256 parmak izini içerir; satırlar böylece bir zincir oluşturur.

**Hash zincirini doğrula** tüm zinciri yeniden hesaplar. "Hash zinciri: … satırın tümü doğrulandı." mesajı hiçbir şeyin değiştirilmediği anlamına gelir. Aksi halde mesaj, değiştirilmiş veya bir önceki satıra işaret etmeyen ilk satırı belirtir.

## Değişmeyen kayıtlar {#trust-locked}

- Onaylanmış bir KKN kaydı, onayını yalnızca değerleri değişmediği sürece korur.
- Kesinleşmiş bir parti ve lotları kilitlidir.
- Onaylanmış laboratuvar sonuçları salt okunur KKN 1 değerleri olur.
- Düzeltmeler iki yönetici gerektirir ve gerekçesiyle birlikte kaydedilir.

## Dosyalar ve parmak izleri {#trust-files}

Teslim formları, laboratuvar raporu PDF'leri, kurum belgeleri, kayıt kanıtları ve rıza belgeleri, herkese açık web klasörünün dışında, rastgele bir adla ve **SHA-256 parmak izleriyle** birlikte saklanır. Yüklenen dosyalar yalnızca dosya adına göre değil, içeriklerine göre de kontrol edilir: 10 MB'a kadar PDF ya da 8 MB'a kadar JPG, PNG veya WEBP. Platform, onaylanmış bir laboratuvar raporunu kullanmadan önce saklanan PDF'in parmak iziyle hâlâ eşleştiğini kontrol eder. Herkes, bir rapor PDF'ini yüklemeden, herkese açık doğrulama sayfasında parmak iziyle karşılaştırabilir.

## Kamuya açık bilgiler {#trust-public}

Herkese açık sayfalar (Bal Hikâyesi, arıcı sayfası, rapor doğrulama) yalnızca onaylanmış ve doğrulanmış bilgileri gösterir:

- **Yer**: yalnızca **il** (arıcı sayfası ilçeyi de belirtir). İl sunucuda hesaplanır; arılık koordinatları sunucudan hiç çıkmaz. Türkiye haritası hiçbir zaman nokta göstermez, yalnızca illeri renklendirir.
- **Kişiler**: arıcının adı yalnızca yazılı rızayla gösterilir (aksi halde nötr bir kod); telefon numarası, doğum tarihi veya kişi fotoğrafı hiçbir zaman gösterilmez. Platformun kullanıcı adları da gösterilmez.
- **Değerler**: laboratuvar ve kontrol noktası değerleri; limit içinde/limit dışında değerlendirmesi yalnızca **doğrulanmış limitler** için. Serbest metin yanıtlar ve dosyalar, bir yönetici yanıtları kişisel veri içermeyen bir KKN için buna açıkça izin vermedikçe yayımlanmaz.
- **Raporlar**: rapor kodu, laboratuvar, akreditasyon, tarih ve parmak izi; lot veya arıcı hiçbir zaman gösterilmez.

Bal Hikâyesi yalnızca kesinleşmiş partiler için vardır; arıcı sayfası ise yalnızca rıza geçerli olduğu sürece yayındadır.

<!-- SHOT: public-map -->
