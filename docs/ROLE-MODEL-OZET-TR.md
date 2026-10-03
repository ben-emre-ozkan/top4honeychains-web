# TOP4HoneyChains – Rol ve kurum modeli (özet)

*Toplantı notu, 3 Ekim 2026. Ayrıntılı tasarım: `ROLE-MODEL.md` (İngilizce).*

## Amaç
Bal zincirinde yalnızca **lisanslı veya platformca onaylı** kurumlar veri girer. Her kayıt kimin, hangi kurum adına, ne zaman girdiğiyle saklanır. Önemli adımlar ikinci bir kişinin onayından geçer. Tüketici yalnızca doğrulanmış bilgiyi görür.

## Kim ne yapar
| Kurum türü | Görevi | Kritik kontrol noktaları (KKN) |
|---|---|---|
| Arıcı | Sisteme giriş yapmaz, veri girmez. Kaydını birliği tutar. | — |
| Arıcılar birliği | Üye arıcıların kaydını ve tarla denetimini yapar. | KKN 0 (tarla denetimi) |
| İşleme yapan kurum (birlik veya paketleyici) | Balı teslim alır, numune alır, işler. | KKN 1–6 (kabul, ısıtma, süzme, karıştırma, pastörizasyon, dinlendirme) |
| Laboratuvar (akredite veya kurum içi) | Analiz yapar; sonucu laboratuvar sorumlusu onaylar. | KKN 1 laboratuvar sonuçları |
| Paketleyici | Dolum, etiket ve ambalaj işlerini yapar. | KKN 7–9 |
| Toptancı / perakendeci | Ürünü teslim alır ve depolar. | KKN 10 (depolama) |
| Merkez birlik | İleride birlikleri onaylayabilir. | — |
| Kamu otoritesi | Her kaydı okuyabilir, hiçbir şeyi değiştiremez. | — |
| Platform yönetimi | Kurumları onaylar ve sistemi ayarlar. Şimdilik proje ekibi. | — |

- Bir kurumun birden fazla rolü olabilir. Örneğin bir birlik balı kendisi paketleyebilir.
- Kurumların içinde kişisel roller vardır: yönetici, personel, tarla denetçisi, numune kabul sorumlusu, kalite onaycısı, analist ve laboratuvar sorumlusu.
- **Kuralları:**
  - Bir KKN'yi, ürün o anda kimin elindeyse o kurum girer.
  - Zincirde sonra gelen kurum, öncekilerin kayıtlarını okuyabilir.
  - Önemli adımlar ikinci bir kişinin onayını gerektirir: KKN değerleri, laboratuvar sonuçları ve partinin kesinleştirilmesi.
  - Tek kişilik kurumlar için platform yönetimi muafiyet verebilir. Muafiyet laboratuvar sonuçlarına uygulanmaz ve muafiyetle verilen her onay kayıt altına alınır.
  - Kesinleşen parti bir daha açılmaz; düzeltme yalnızca gerekçeli bir düzeltme kaydıyla yapılır.

## Arıcı ne kazanır
- **Kendi vitrin sayfası.** Yalnızca doğrulanmış verilerden oluşur ve arıcının kayıtlı onayından (KVKK) sonra yayınlanır.
- **Konum gizliliği.** İl ve ilçe gösterilir, tam konum gösterilmez.
- **Görünürlük.** Balının hangi partilere girdiğini ve analiz sonuçlarını tüketici de görür.
- **Bilgilendirme.** Teslimat sırasında isteğe bağlı SMS ile bilgi alabilir.

## Neler ayarlanabilir (programlama gerekmeden)
- Hangi KKN'yi hangi kurum türünün ve hangi rolün gireceği, kimin onaylayacağı.
- Analiz limitleri, bal türlerine göre de ayrı ayrı. Başlangıç değerleri Türk Gıda Kodeksi'nden alındı; doğrulanmayı bekliyor.
- Birlikleri platform yönetiminin mi yoksa merkez birliğin mi onaylayacağı.
- Tek kişilik kurumlar için dört göz muafiyeti.
- Arıcı kaydının doğrulanması: Şimdilik belge yükleme ve elle onayla yapılıyor. AKS erişimi olunca otomatik doğrulamaya geçilebilir.

## Aşamalı plan
| Aşama | İçerik | Durum |
|---|---|---|
| A1 | Kurumlar, roller ve üyelikler; kurum doğrulama; değiştirilemez işlem kaydı; erişim kuralları | Hazır ve test edildi. Sistemde **kapalı**: açılınca devreye girer. |
| A2 | KKN sorumluluk tablosu, onay akışı, kuruma özel ekranlar | Planlandı |
| A3 | Teslim alma, kurumlar arası sevkiyat, depolama (KKN 10), geri çağırma raporu | Planlandı |
| A4 | Numune, mühür, şahit numune, laboratuvar raporu ve laboratuvar rozetleri | Planlandı |
| A5 | Arıcı vitrin sayfası, AKS bağlantısı, harmanlarda menşe yüzdeleri (AB 2024/1438) | Planlandı |

## Açık sorular
1. "Toptancı" balı paketleyen ve satan kurumla aynı şey mi?
2. Platformu proje bitince kim yönetecek? Birlikleri kim onaylayacak?
3. Kim kimin kayıtlarını görebilir? Örneğin birlik, üyesinin balının nereye satıldığını görebilir mi?
4. AKS'ye erişim mümkün mü? İşletme numarası biçimi doğru mu?
5. KVKK açısından platformun rolü ne? Kayıtlar ne kadar süre saklanacak?
6. Menşe yüzdeleri Türkiye'de nasıl uygulanacak?
7. Tarla denetimi (KKN 0) ne sıklıkla yapılacak?
8. Analiz limitleri akredite laboratuvarca doğrulanacak (`LIMITS-TO-VERIFY-TR.md`).
9. Teslimatta arıcıya SMS gönderilecek mi? Gönderilecekse ücretini kim karşılayacak?
