# 14. Sözlük ve SSS {#glossary-faq}

## Sözlük {#glossary}

| Terim | Anlamı |
|---|---|
| **HACCP** | Tehlike Analizi ve Kritik Kontrol Noktaları (Hazard Analysis and Critical Control Points): kontrol noktalarının dayandığı gıda güvenliği yöntemi. |
| **KKN (kritik kontrol noktası)** | Bir tehlikenin ölçüm veya kontrolle denetim altında tutulduğu adım. İngilizcede CCP, İspanyolcada PCC, Lehçede KPK, Letoncada KKP. |
| **Parametre** | Bir KKN'de kaydedilen tek bir değer, örneğin nem veya sıcaklık. |
| **Kritik limit** | Bir parametre için izin verilen en düşük ve/veya en yüksek değer. |
| **Doğrulanmış limit** | Proje tarafından teyit edilmiş limit; yalnızca doğrulanmış limitler herkese açık bir limit içinde/limit dışında değerlendirmesi verir. |
| **Limit içinde / Limit dışında** | Bir değerin limitiyle karşılaştırılmasının sonucu. Limit dışındaki değerler onaydan sonra da işaretli kalır. |
| **Uygulanamaz** | Bu bal için geçerli olmayan ve gerekçesiyle kaydedilen KKN; yine de onay gerektirir. |
| **Ham bal lotu** | Tek bir arılıktan yapılan tek bir teslimin balı (bir dönem, bir bal türü). KKN 0–3'ü taşır. |
| **Parti** | Bir veya daha fazla lottan oluşan, birlikte işlenip paketlenen bal. KKN 4–10'u ve QR kodu taşır. |
| **Kesinleştirme / mühürleme** | Bir partiyi ve lotlarını kilitleyen ve Bal Hikâyesi'ni yayımlayan iki kişilik adım. |
| **Dört göz ilkesi** | Her kaydı, onu girenden farklı bir kişi onaylar. |
| **Dört göz muafiyeti** | Küçük kurumlar için, KKN kayıtlarının kişinin kendisi tarafından onaylanmasına izin veren ve bu onayları her zaman işaretleyen bir ayar. Laboratuvar sonuçları için hiçbir zaman geçerli değildir. |
| **Numune / şahit numune** | Laboratuvara gönderilen mühürlü numune ve tesiste saklanan eşi. |
| **Mühür numarası** | Numunenin mührü üzerindeki numara; laboratuvar numuneyi teslim alırken bu numarayı kontrol eder. |
| **Laboratuvar raporu / rapor kodu** | Laboratuvarın raporu (PDF ve değerler) ve raporun herkese açık 12 karakterlik kodu. |
| **Akredite laboratuvar** | Geçerli akreditasyonu olan laboratuvar; akreditasyon durumu rapor onaylandığında kaydedilir. |
| **Kurum içi laboratuvar** | Lot sahibine ait laboratuvar. |
| **Emanetçi** | Onaylanmış bir sevkiyattan sonra bir partinin belirli bir miktarını elinde tutan kurum. |
| **Emanet** | Ürünün kimin elinde olduğu; alıcı bir sevkiyatı onayladığında el değiştirir. |
| **Geri çağırma raporu** | Bir partinin veya lotun kaynağındaki ve sonrasındaki her şeyin listesi. |
| **Bal Hikâyesi** | Kesinleşmiş bir partinin, QR kodundan açılan herkese açık sayfası. |
| **Arıcı sayfası** | Bir arıcının, rızasıyla yayımlanan herkese açık sayfası. |
| **Kayıt numarası (AKS)** | Arıcının işletme kayıt numarası: `TR` + il kodu + 10 rakam. |
| **Üyelik** | Bir kişinin bir kurumdaki rolü. Bir kişinin birden fazla üyeliği olabilir. |
| **Yetenek** | Bir kurumun yaptığı iş türü (İşleme, Laboratuvar, Perakende, …). |
| **Denetim kaydı** | Her değişikliğin, hash zinciriyle korunan ve yalnızca eklenebilen kaydı. |
| **SHA-256 parmak izi** | Bir dosyanın içeriğinden hesaplanan kod; dosyadaki her değişiklik bu kodu da değiştirir. |

## SSS ve sorun giderme {#faq}

**Oturum açamıyorum: "Kullanıcı adı veya şifre hatalı. …"**
Kullanıcı adını (e-posta adresini değil) ve şifreyi kontrol edin. 5 başarısız denemeden sonra hesap 15 dakika kilitlenir; bekleyin, gerekirse **Şifrenizi mi unuttunuz?** bağlantısını kullanın.

**"Bu hesabın web paneline erişimi yok."**
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
Önce tesisin KKN 0'ı girmesi ve tesisin kalite onaylayıcısının onaylaması gerekir.

**Onaylayamıyorum: "Dört göz: bu KKN'nin değerlerini siz girdiniz, başka bir kişinin onaylaması gerekir."**
Kaydı başka bir onaylayıcı onaylamalıdır. Kurumunuzda tek bir kalite sorumlusu varsa bir platform yöneticisine dört göz muafiyetini sorun (laboratuvar sonuçları için kullanılamaz).

**"Kesinleştirme talep et" devre dışı.**
"Kesinleştirmeden önce eksik olanlar:" listesini okuyun. Tesisteki KKN 10 dahil her parti KKN'si ve her lot KKN'si onaylanmış olmalıdır.

**Bir değeri değiştirdim ve onay kayboldu.**
Bu bilinçli bir davranıştır: her değişiklik yeni bir onay gerektirir.

**Lot "Partiyi Yönet" sayfasında görünmüyor.**
Bir lot yalnızca tüm KKN'leri tamamlandığında veya uygulanamaz olarak işaretlendiğinde ve onaylandığında, ayrıca yalnızca partinin toptancısı için görünür. Bir işleme tesisi kendi toptancı kaydına bağlı olmalıdır.

**Bir değer onaylandı ama yanlış.**
Bir platform yöneticisinden düzeltme isteyin; düzeltmeyi ikinci bir yönetici onaylar.

**Arıcı sayfası herkese açık değil.**
Rızanın kaydedildiğini, kayıt numarasının doğrulandığını, birliğin doğrulandığını ve arıcının etkin olduğunu kontrol edin.

**İki kurum için çalışıyorum.**
Üst çubuktaki kurum · rol etiketiyle üyelik değiştirin. Seçim tarayıcıyı kapatana kadar geçerlidir.

**Şifre sıfırlama e-postası gelmiyor.**
İstenmeyen (spam) klasörünü kontrol edin. Bir platform yöneticisi sizin için yeni bir şifre belirleyebilir.
