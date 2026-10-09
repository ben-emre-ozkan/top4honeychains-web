# TOP4HoneyChains – proje özeti (Türkçe)

Güncelleme: 2026-10-09. Kısa ve güncel bir özettir; ayrıntılar kullanım kılavuzunda ve İngilizce dokümanlardadır (bkz. son bölüm). Burada anlatılan davranış, canlıda açık olan kurum ve rol modelini (`Features:OrgRoleModel = true`) esas alır.

## 1. Platform nedir?

TOP4HoneyChains, balın arıcıdan tüketiciye kadar izlenebilirliğini sağlayan bir web platformudur. Her kayıt, kimin girdiği, hangi kurum adına girildiği ve kimin onayladığı bilgisiyle saklanır. Tüketici kavanozdaki QR kodu okutunca herkese açık **Bal Hikâyesi** sayfasını görür: balın hangi illerden geldiğini, laboratuvar sonuçlarını ve kontrol noktalarını.

## 2. Beş istasyon

1. **Arılık**: arıcı balı üretir. Arıcılar sisteme giriş yapmaz; platformun ve birliklerinin tuttuğu kayıtlar olarak yer alırlar.
2. **Teslim**: arıcı balı bir işleme tesisine teslim eder. Tesis teslimi kaydeder; bu kayıt bir *ham bal lotu* oluşturur ve tesis balı kabulde kontrol eder (KKN 0).
3. **Laboratuvar**: tesis numuneyi mühürleyip laboratuvara gönderir; laboratuvar sonuçları girer, ikinci bir kişi raporu onaylar.
4. **İşleme**: lotta KKN 2–3, lotlar birleştirilerek oluşturulan *partide* KKN 4–10 kaydedilir; ardından parti iki kişiyle kesinleştirilir (mühürlenir).
5. **Raf**: parti bir toptancıya veya mağazaya sevk edilir; alıcı teslimi onaylar ve kendi depolama kaydını tutar.

## 3. Kurumlar ve roller

Her işlem **bir kurum adına, bir rolde** yapılır. Kurumun **yetenekleri** (İşleme, Paketleme, Laboratuvar, Toptan Satış, Perakende, Birlik, Kamu Otoritesi …) ne tür işler yapabileceğini, kişinin kurumdaki **rolü** ne yapabileceğini belirler. Yalnızca **Doğrulandı** durumundaki kurumlar veri girebilir. Kurumları, yetenekleri, üyeleri ve yetki ayarlarını platform yöneticisi yönetir.

| Rol | Ana işi (varsayılan) |
|---|---|
| **Numune kabul sorumlusu** | Teslimi kaydeder, numune alıp mühürler ve gönderir, KKN 0'ı girer |
| **Personel** | KKN 2–10 değerlerini girer, kesinleştirme talep eder, sevk eder, sevkiyatı onaylar, depolama kaydı girer |
| **Kalite onaylayıcı** | KKN kayıtlarını, depolama kayıtlarını ve kesinleştirmeyi onaylar |
| **Analist** | Numuneyi teslim alır, laboratuvar sonuçlarını girer, raporu onaya gönderir |
| **Laboratuvar müdürü** | Numuneyi teslim alır, laboratuvar raporunu onaylar veya geri gönderir |
| **Kurum yöneticisi** | Kurum belgelerini yükler, kesinleştirme talep eder, sevk eder, sevkiyatı onaylar, birliklerde arıcı rızasını kaydeder |
| **Saha denetçisi** | Kayıtları okur; KKN 0'ı yalnızca platform yöneticisi bu KKN'yi birliğe verirse girer |

**Platform yöneticisi** iş verisi girmez ve onaylamaz; her şeyi okuyabilir. Kurumları doğrular, üyeleri ve yetkileri yönetir, arıcı kaydını tutar ve şimdilik partileri oluşturur.

## 4. Akış (kısaca)

- Numune kabul sorumlusu teslimi kaydeder; lot oluşur ve KKN 0–3 lota otomatik atanır.
- **Önce KKN 0, sonra laboratuvar:** laboratuvar sonuçları ancak lotun KKN 0 kaydı onaylandıktan sonra kaydedilip onaylanabilir. Onaylanan laboratuvar raporu lotun KKN 1 kaydına yazılır. Ardından KKN 2–3 girilir ve onaylanır.
- **Parti şimdilik yalnızca platform yöneticisi tarafından oluşturulur:** lotları ve miktarları seçer, sahip kurumu belirler, KKN 4–10'u atar. Değerleri sahip kurumun personeli girer.
- **KKN 10 tesisteki depolamadır** ve partinin diğer KKN'leri gibi **mühürlemeden önce zorunludur**: partinin her KKN'si onaylanmadan kesinleştirme yapılamaz.
- Personel veya kurum yöneticisi kesinleştirme talep eder, kalite onaylayıcı (başka bir kişi) onaylar; parti ve lotları kilitlenir, Bal Hikâyesi yayına girer. Kesinleştirmeyi geri alma yoktur.
- Yalnızca kesinleşmiş partiler sevk edilir. Emanet, alıcı teslimi onaylayınca el değiştirir; her alıcı kendi KKN 10 depolama kaydını tutar ve üreticinin KKN kayıtlarını arıcı ve arılık adları olmadan salt okunur görür.

## 5. Dört göz ilkesi

KKN değerlerini, laboratuvar sonuçlarını ve kesinleştirmeyi giren veya talep eden kişi bunları onaylayamaz; bir KKN'yi "Uygulanamaz" olarak işaretleyen kişi de onu onaylayamaz. Tek kişilik kurumlara platform yöneticisi istisna tanıyabilir; istisnayla verilen onaylar işaretlenir ve denetim kaydına yazılır. Laboratuvar sonuçlarında istisna yoktur. "İşlerim" sayfası yalnızca kuralın izin verdiği onayları sunar. Onaydan sonra hatalı bulunan bir değer için düzeltmeyi bir platform yöneticisi talep eder, ikinci bir platform yöneticisi karara bağlar.

## 6. Bal Hikâyesi ve gizlilik

- Bal Hikâyesi yalnızca kesinleşmiş partiler için vardır; QR kodu `/honeystory/{kod}` adresini açar.
- Konum yalnızca **il düzeyindedir**: Türkiye haritası illeri renklendirir, hiçbir zaman nokta veya koordinat göstermez.
- **Kişisel veri yayımlanmaz:** telefon, doğum tarihi, kişi fotoğrafı ve kullanıcı adları gösterilmez; arıcının adı yalnızca kaydedilmiş yazılı rızayla görünür, aksi halde nötr bir kod gösterilir.
- Arıcı sayfası (`/storefront/{kod}`) yalnızca doğrulanmış verilerden ve birliğin kaydettiği rızadan sonra yayımlanır; rıza geri çekilince kalkar.
- Laboratuvar raporu PDF'leri SHA-256 parmak iziyle saklanır; herkes `/verify-report` sayfasında bir raporu doğrulayabilir.

## 7. Denetim kaydı

Her kayıt, değişiklik, onay, kesinleştirme, sevkiyat, oturum açma ve yetki değişikliği denetim kaydına yazılır (kim, hangi kurum adına, ne zaman, eski ve yeni değerler). Kayda yalnızca ekleme yapılabilir; veritabanı değiştirme ve silmeyi reddeder ve her satır bir öncekinin parmak izini taşır, böylece oynama fark edilir. Denetim kaydını platform yöneticileri okur.

## 8. Nereye bakmalı

- **Kullanım kılavuzu (Türkçe):** [`docs/manual/tr/README.md`](manual/tr/README.md) (Markdown) ve [`docs/manual/dist/TOP4HoneyChains-User-Manual-TR.pdf`](manual/dist/TOP4HoneyChains-User-Manual-TR.pdf). Roller, yetki matrisi ve adım adım akış buradadır.
- **Demo senaryosu:** [`docs/test/END-TO-END-SCENARIO.md`](test/END-TO-END-SCENARIO.md).
- **Canlıya geçiş:** [`docs/GO-LIVE.md`](GO-LIVE.md).
- **Teknik ayrıntılar:** [`ARCHITECTURE.md`](ARCHITECTURE.md), [`ROLE-MODEL.md`](ROLE-MODEL.md), [`DECISIONS.md`](DECISIONS.md).
