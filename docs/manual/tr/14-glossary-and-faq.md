# 14. Sözlük ve SSS {#glossary-faq}

## Sözlük {#glossary}

[Tablo](#tbl-glossary), bu kılavuzda kullanılan terimleri tanımlar; [Tablo](#tbl-renamed-labels) ise bir sonraki sürümde değişecek etiketleri listeler.

Table: Sözlük {#tbl-glossary}

| Terim | Anlamı |
|---|---|
| **HACCP** | Tehlike Analizi ve Kritik Kontrol Noktaları (Hazard Analysis and Critical Control Points): kontrol noktalarının dayandığı gıda güvenliği yöntemi. |
| **KKN (kritik kontrol noktası)** | Bir tehlikenin ölçüm veya kontrolle denetim altında tutulduğu adım. İngilizcede CCP, İspanyolcada PCC, Lehçede KPK, Letoncada KKP. |
| **Veri** | Kişilerin platforma kaydettiği bilgiler: ölçülen değerler (örneğin % cinsinden nem) ve yanıtlar (örneğin Evet / Hayır). |
| **Parametre** | Bir KKN'de kaydedilen tek bir veri öğesi, örneğin nem veya sıcaklık. |
| **Kritik limit** | Bir parametre için izin verilen en düşük ve/veya en yüksek değer. |
| **Doğrulanmış limit** | Proje tarafından teyit edilmiş limit; yalnızca doğrulanmış limitler herkese açık bir limit içinde/limit dışında değerlendirmesi verir. |
| **Limit içinde / Limit dışında** | Bir değerin limitiyle karşılaştırılmasının sonucu. Limit dışındaki değerler onaydan sonra da işaretli kalır. |
| **Uygulanamaz** | Bu bal için geçerli olmayan ve gerekçesiyle kaydedilen KKN; yine de onay gerektirir. |
| **Ham bal lotu** | Tek bir arılıktan yapılan tek bir teslimin balı (bir dönem, bir bal türü). KKN 0–3'ü taşır. |
| **Parti** | Bir veya daha fazla lottan oluşan, birlikte işlenip paketlenen bal. KKN 4–10'u ve QR kodu taşır. |
| **Kesinleştirme / mühürleme** | Bir partiyi ve lotlarını kilitleyen ve Bal Hikâyesi'ni yayımlayan iki kişilik adım. |
| **Dört göz ilkesi** | Her kayıtta iki farklı kişi yer alır: biri veriyi girer, ikinci bir kişi kontrol edip onaylar. Kimse kendi girdiğini onaylayamaz. Bkz. [Temel kurallar](#fixed-rules). |
| **Dört göz muafiyeti** | Küçük kurumlar için, KKN kayıtlarının kişinin kendisi tarafından onaylanmasına izin veren ve bu onayları her zaman işaretleyen bir ayar. Laboratuvar sonuçları için hiçbir zaman geçerli değildir. |
| **Numune / şahit numune** | Laboratuvara gönderilen mühürlü numune ve tesiste saklanan eşi. |
| **Mühür numarası** | Numunenin mührü üzerindeki numara; laboratuvar numuneyi teslim alırken bu numarayı kontrol eder. |
| **Laboratuvar raporu / rapor kodu** | Laboratuvarın raporu (PDF ve değerler) ve raporun herkese açık 12 karakterlik kodu. |
| **Gıda analiz laboratuvarı** | Bal numunelerini analiz eden laboratuvar; genellikle akredite bir laboratuvardır. Sonuçları laboratuvarın analisti girer, laboratuvar müdürü onaylar. |
| **Akredite laboratuvar** | Geçerli akreditasyonu olan laboratuvar; akreditasyon durumu rapor onaylandığında kaydedilir. |
| **Kurum içi laboratuvar** | Lot sahibine ait laboratuvar. |
| **Emanetçi** | Onaylanmış bir sevkiyattan sonra bir partinin belirli bir miktarını elinde tutan kurum. |
| **Emanet** | Ürünün kimin elinde olduğu; alıcı bir sevkiyatı onayladığında el değiştirir. |
| **Geri çağırma raporu** | Bir partinin veya lotun kaynağındaki ve sonrasındaki her şeyin listesi. |
| **Bal Hikâyesi** | Kesinleşmiş bir partinin, QR kodundan açılan herkese açık sayfası. |
| **Arıcı sayfası** | Bir arıcının, rızasıyla yayımlanan herkese açık sayfası. |
| **Kayıt numarası (AKS)** | Arıcının işletme kayıt numarası: `TR` + il kodu + 10 rakam. |
| **Üyelik** | Bir kullanıcı hesabı × bir kurum × bir rol. Bir kişinin birden fazla üyeliği olabilir, aynı kurumda birkaç üyeliği de olabilir. |
| **Arıcı (kaydı)** | Arıcıların hesabı yoktur. Her arıcı platformda bir Arıcı kaydı ile temsil edilir. |
| **Okuma** | Kayıtları değiştirmeden açma ve okuma hakkı. |
| **Yetenek** | Bir kurumun sahip olduğu bir beceri: kurumun bal zincirinde neler yapabileceği (örneğin "Balı teslim alıp işleyebilir"). Bkz. [Tablo](#tbl-capabilities). |
| **Temel kurallar** | Platforma yerleşik olan ve hiçbir ayarın kapatamadığı kurallar. Bkz. [Temel kurallar](#fixed-rules). |
| **Denetim kaydı** | Her değişikliğin, hash zinciriyle korunan ve yalnızca eklenebilen kaydı. |
| **SHA-256 parmak izi** | Bir dosyanın içeriğinden hesaplanan kod; dosyadaki her değişiklik bu kodu da değiştirir. |

Table: Bir sonraki sürümde yeniden adlandırılacak etiketler (ekran görüntüleri eski etiketleri gösterir) {#tbl-renamed-labels}

| Bugün ekranda | Yeni ad (bu kılavuzda kullanılan) |
|---|---|
| Kalite onaylayıcı | Kalite Güvence Sorumlusu |
| Sabit kurallar (kapatılamaz) | Temel kurallar (kapatılamaz) |
| Yetenek açıklamaları (örneğin "Kabul ve işleme: teslim almalar, numuneler, lot KKN 0–3 ve parti KKN 4–6") | "… yapabilir" biçimindeki açıklamalar (örneğin "Balı teslim alıp işleyebilir: …"), bkz. [Tablo](#tbl-capabilities) |
| Arıcılık (yetenek) | Arıcılık İşletmesi |
| Rolü adıyla anan mesajlar ("bir kalite onaylayıcı (başka bir kişi) onaylamalı") | Yeni rol adı ("bir Kalite Güvence Sorumlusu (başka bir kişi) onaylamalı") |

## SSS ve sorun giderme {#faq}

**Oturum açamıyorum: "Kullanıcı adı veya şifre hatalı. …"**
Kullanıcı adını (e-posta adresini değil) ve şifreyi kontrol edin. 5 başarısız denemeden sonra hesap 15 dakika kilitlenir; bekleyin ya da bir platform yöneticisinden hesabı **Kilidi aç** ile açmasını isteyin, ardından gerekirse **Şifrenizi mi unuttunuz?** bağlantısını kullanın.

**"Bu hesabın henüz panele erişimi yok: hiçbir kurumun üyesi değil."**
Hesabınızın etkin bir üyeliği yok. Bir platform yöneticisinden sizi doğru rolle kurumunuza eklemesini isteyin.

**"Bu sayfayı açamazsınız" (403) sayfasını görüyorum.**
Rolünüz veya kurumunuz bu sayfayı ya da bu kaydı açma yetkisine sahip değil. Başka bir kuruma ait bir KKN için bu beklenen bir durumdur; kurumunuz o partiyi elinde tutuyorsa sayfa açılır. Erişime ihtiyacınız varsa kurum yöneticinize başvurun.

**"Bu sayfa bulunamadı" (404) sayfasını görüyorum.**
Adres yanlış, kayıt mevcut değil ya da (Bal Hikâyesi için) parti henüz kesinleşmemiş.

**Hiçbir şey giremiyorum; sayfada "Salt okunur" yazıyor.**
Altındaki gerekçeyi okuyun. Olağan nedenler: kurumunuz henüz doğrulanmamış; bu KKN'den başka bir kurum veya rol sorumlu; kayıt kesinleştirme nedeniyle kilitli.

**Kurumumuz doğrulanmadı.**
Kurum yöneticisi kurum sayfasında bir belge (ruhsat, akreditasyon, kayıt kanıtı) yükler; ardından bir platform yöneticisi kurumu doğrular.

**Laboratuvar sonuç giremiyor: "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; …"**
Önce tesisin KKN 0'ı girmesi ve tesisin Kalite Güvence Sorumlusunun onaylaması gerekir.

**Onaylayamıyorum: "Dört göz: bu KKN'nin değerlerini siz girdiniz, başka bir kişinin onaylaması gerekir."**
Kaydı başka bir onaylayıcı onaylamalıdır. Kurumunuzda tek bir kalite sorumlusu varsa bir platform yöneticisine dört göz muafiyetini sorun (laboratuvar sonuçları için kullanılamaz).

**"Kesinleştirme talep et" devre dışı.**
"Kesinleştirmeden önce eksik olanlar:" listesini okuyun. Tesisteki KKN 10 dahil her parti KKN'si ve her lot KKN'si onaylanmış olmalıdır.

**Bir değeri değiştirdim ve onay kayboldu.**
Bu bilinçli bir davranıştır: her değişiklik yeni bir onay gerektirir.

**Lot "Partiyi Yönet" sayfasında görünmüyor.**
Bir lot yalnızca tüm KKN'leri tamamlandığında veya uygulanamaz olarak işaretlendiğinde ve onaylandığında, ayrıca yalnızca partinin toptancısı için görünür. Bir işleme tesisi kendi toptancı kaydına bağlı olmalıdır (kurum → **Düzenle**); sayfa, bağlı olmayan tesislerin adını verir.

**Bir değer onaylandı ama yanlış.**
Bir platform yöneticisinden düzeltme isteyin; düzeltmeyi ikinci bir yönetici onaylar.

**Arıcı sayfası herkese açık değil.**
Rızanın kaydedildiğini, kayıt numarasının doğrulandığını, birliğin doğrulandığını ve arıcının etkin olduğunu kontrol edin.

**İki kurum için çalışıyorum.**
Üst çubuktaki kurum · rol etiketiyle üyelik değiştirin. Seçim tarayıcıyı kapatana kadar geçerlidir.

**Şifre sıfırlama e-postası gelmiyor.**
İstenmeyen (spam) klasörünü kontrol edin. Bir platform yöneticisi sizin için yeni bir şifre belirleyebilir.
