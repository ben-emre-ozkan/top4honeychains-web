# 16. Güven ve gizlilik {#trust}

## Denetim kaydı {#trust-audit}

Platformdaki her değişiklik (her kayıt, onay, kesinleştirme, sevkiyat, oturum açma, yetki değişikliği, kanıt indirme ve veri saklama çalıştırması) denetim kaydına yazılır. Platform yöneticileri bu kaydı **Kullanıcı Yönetimi** → **Denetim kaydı** menüsünden açar; bir Uyum Sorumlusu kendi kurumunun girişlerini **Uyum** sayfasında okur.

Her satır şunları gösterir:

- **Ne zaman**: yerel saat; UTC saatini görmek için fareyle üzerine gelin;
- **Kim**: kullanıcı ve kurum;
- **İşlem**: örneğin Oluştur, Güncelle, Onayla, Kesinleştir, Doğrula, Giriş, İndir; "Kendi onayladı" gibi işaretlerle birlikte;
- **Kayıt**: hangi kayıt olduğu; bir yönetim sayfası varsa bağlantısıyla birlikte;
- **Değişiklikler**: değişen her alan **eski → yeni** biçiminde, alan adı sizin dilinizde; kullanıcı ve kurum kimlikleri adlarıyla gösterilir;
- **Hash**: satırın parmak izinin başlangıcı.

**Ham veriyi göster**, saklanan veriyi yazıldığı haliyle açar (eski ve yeni değerler).

Kayıt türüne, işleme, kullanıcı adına veya işarete (örneğin `SelfApproved`) göre süzün ve **Filtrele** düğmesine tıklayın.

<!-- SHOT: audit-log -->
Figure: Eski → yeni değişiklikleri ve ham verisiyle denetim kaydı.

### Yalnızca ekleme ve hash zinciri {#trust-hash-chain}

Denetim kaydına yalnızca ekleme yapılabilir. Veritabanı, bir satırı değiştirme veya silme girişimlerini reddeder. Her satır, kendi içeriğinin ve bir önceki satırın SHA-256 parmak izini içerir; satırlar böylece bir zincir oluşturur.

**Hash zincirini doğrula** tüm zinciri yeniden hesaplar. "Hash zinciri: … satırın tümü doğrulandı." mesajı hiçbir şeyin değiştirilmediği anlamına gelir. Aksi halde mesaj, değiştirilmiş veya bir önceki satıra işaret etmeyen ilk satırı belirtir.

## Değişmeyen kayıtlar {#trust-locked}

- Onaylanmış bir KKN kaydı, onayını yalnızca değerleri değişmediği sürece korur.
- Kesinleşmiş bir parti ve lotları kilitlidir.
- Onaylanmış laboratuvar sonuçları salt okunur KKN 1 değerleri olur.
- Onaylanmış bir teslimin bilgileri kilitlidir; kaplara, mühür olaylarına, kayıtlara, fiyat girişlerine, SMS mesajlarına, aydınlatma metinlerine ve bunların onaylarına yalnızca ekleme yapılabilir.
- Düzeltmeler iki yönetici gerektirir ve gerekçesiyle birlikte kaydedilir.

## İçeriği onaylayan kodlar {#trust-codes}

Arıcı bir kaydı ve bir teslimi SMS ile aldığı 6 haneli bir kodla onaylar; imza yetkisi taşıyan roller de aydınlatma metnini aynı şekilde onaylar. Her kod, tam olarak gönderildiği içeriğin parmak izine bağlıdır: sonradan herhangi bir şeyin değişmesi kodu geçersiz kılar. Kodlar 5 dakika geçerlidir ve 3 kez denenebilir; kodun yalnızca özeti saklanır. SMS mümkün olmayan durumlarda kodun yerini imzalı bir fotoğraf alır ve ikinci bir kişi (Uyum Sorumlusu) bu fotoğrafı kabul eder veya reddeder.

## Dosyalar ve parmak izleri {#trust-files}

Teslim formları, kayıt fotoğrafları, laboratuvar raporu PDF'leri, kurum belgeleri, kayıt kanıtları ve rıza belgeleri, herkese açık web klasörünün dışında, rastgele bir adla ve **SHA-256 parmak izleriyle** birlikte saklanır. Yüklenen dosyalar yalnızca dosya adına göre değil, içeriklerine göre de kontrol edilir: 10 MB'a kadar PDF ya da 8 MB'a kadar JPG, PNG veya WEBP. Platform, onaylanmış bir laboratuvar raporunu kullanmadan önce saklanan PDF'in parmak iziyle hâlâ eşleştiğini kontrol eder. Herkes, bir rapor PDF'ini yüklemeden, herkese açık doğrulama sayfasında parmak iziyle karşılaştırabilir.

**Kanıtları kimler indirir?** Kanıt dosyalarını yalnızca bunları kaydeden rol, kurumun Uyum Sorumlusu ve kurum yöneticisi ve platform yöneticileri indirir; kurumun başka bir rolü 403, başka bir kurum 404 alır ve her indirme denetim kaydına yazılır (bkz. [Veri hakları](#crud)).

## Aydınlatma metinleri, rızalar ve veri sorumlusu {#trust-notices}

**Aydınlatma metinleri.** Platform ve her kurum bir aydınlatma metni yayımlar: kişisel verilerin nasıl, hangi amaçla işlendiği, ne kadar saklandığı ve **veri sorumlusunun** kim olduğu. Kullanıcılar platformun ve çalıştıkları her kurumun aydınlatma metinlerini onaylar (bkz. [Aydınlatma metni](#notice-accept)); arıcılar kendi metinlerini kayıt sırasında okur ve SMS kodlarıyla onaylar. Aydınlatma metinleri sürümlüdür: yeni bir sürüm herkesten yeniden onay ister, eski onaylar kanıt olarak kalır. Henüz KVKK birimince onaylanmamış metinler **TASLAK** işaretini taşır. Herkese açık **Gizlilik ve rıza** sayfası (`/privacy`) geçerli metinleri gösterir.

**Veri sorumlusu.** Her kurum, kaydettiği verilerin veri sorumlusudur; tüzel kişi adı, iletişim bilgisi ve adresi aydınlatma metninde belirtilir ve kurum sayfasında kurumun Uyum Sorumlusu veya bir platform yöneticisi tarafından güncel tutulur. Arıcı, verilerini hangi kurumların tuttuğunu portalda görür (**Verilerinizi kimler tutuyor**).

**Rızalar.** İşin gerektirdiğinin ötesindeki her şey isteğe bağlıdır ve ayrı ayrı sorulur:

- arıcının adının herkese açık sayfalarda gösterilmesi;
- teslimler ve sonuçlar hakkında bilgilendirme SMS'leri;
- birliğin arıcının istediği fiyatlara erişmesi;
- birliğin kaydettiği, herkese açık bir arıcı sayfası.

Bir rıza kayıt sırasında (SMS koduyla onaylanarak), portalda ya da arıcı sayfası için birlik tarafından imzalı bir belgeyle verilir. Geri çekme hemen geçerli olur: portalda ya da mektupla veya telefonla geldiğinde Uyum Sorumlusu tarafından kaydedilerek.

**Kişisel verilere ilişkin başvurular.** Erişim, düzeltme, silme ve itiraz başvurularını kurumun Uyum Sorumlusu 30 gün içinde yanıtlar (bkz. [İlgili kişi başvuruları](#co-requests)).

## Herkesin okuyabildiği bilgiler {#trust-public}

Herkese açık sayfalar (Bal Hikâyesi, arıcı sayfası, rapor doğrulama) yalnızca onaylanmış ve doğrulanmış bilgileri gösterir:

- **Yer**: yalnızca **il** (arıcı sayfası ilçeyi de belirtir). İl sunucuda hesaplanır; arılık koordinatları sunucudan hiç çıkmaz. Harita hiçbir zaman nokta göstermez, yalnızca illeri renklendirir; şu anda Türkiye'yi kapsar, diğer ortak ülkelerin haritaları planlanmaktadır.
- **Kişiler**: Bal Hikâyesi bir arıcıyı **yalnızca etkin bir ad rızası varsa** adıyla anar; aksi halde arıcı sayfasının nötr kodunu gösterir. Rıza prosedüründen önce kaydedilmiş arıcılar, rızayı verene kadar kodlarıyla gösterilir (ya da platform bu şekilde yapılandırılmışsa yeniden kaydedilene kadar adlarını korur). Telefon numarası, doğum tarihi veya kişi fotoğrafı hiçbir zaman gösterilmez; platformun kullanıcı adları da gösterilmez. Bu sürümden önce elle yazılan arılık adları çoğu zaman arıcının adını içerir; bu yüzden yalnızca arıcının adının gösterilebildiği yerde gösterilir, aksi hâlde arılık iliyle anılır ("… ilindeki arılık"). Fotoğraf açıklamaları için de aynısı geçerlidir.
- **Veriler**: laboratuvar ve kontrol noktası verileri; limit içinde/limit dışında değerlendirmesi yalnızca **doğrulanmış limitler** için. Serbest metin yanıtlar ve dosyalar, bir yönetici yanıtları kişisel veri içermeyen bir KKN için buna açıkça izin vermedikçe yayımlanmaz.
- **Raporlar**: yalnızca **onaylanmış** raporlar doğrulanabilir; sayfa rapor kodunu, laboratuvarı, akreditasyonu, tarihi ve parmak izini gösterir, lotu veya arıcıyı hiçbir zaman göstermez.
- **Fiyatlar**: hiçbir zaman.
- **Fotoğraflar**: yalnızca partiye bal sağlayan arılıkların fotoğrafları, meta verileri olmadan (GPS konumu veya kamera bilgisi olmadan) sunulur.

Bal Hikâyesi yalnızca kesinleşmiş partiler için vardır; arıcı sayfası ise yalnızca rıza geçerli olduğu sürece yayındadır.

<!-- SHOT: public-map -->
Figure: Herkese açık Bal Hikâyesi'nde il bazında kökenler.

## Verilerin saklanma süresi {#trust-retention}

Saklama süreleri [Tablo](#tbl-retention) içinde listelenmiştir: örneğin tek kullanımlık kodlar 30 gün, SMS mesajları 1 yıl, fiyatlar 5 yıl, lotlar ve kayıtları partinin son tüketim tarihinden 5 yıl sonra, denetim kaydı 10 yıl. Saklama çalıştırması **kapalıdır**: platform yöneticileri yalnızca sayan bir deneme çalıştırmasını görür; ilk gerçek çalıştırma sahibin onayını ve takma adların biçimine ilişkin kararı gerektirir (bkz. [Veri saklama](#admin-retention)).
