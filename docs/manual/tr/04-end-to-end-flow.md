# 4. Baştan sona akış {#flow}

Bu bölüm bir bal partisini platform boyunca izler: her adımı kimin yaptığını ve bir sonraki adım başlamadan önce neyin tamamlanması gerektiğini gösterir ([Şekil](#fig-flow-overview)). Rol bölümleri (5–11) her ekranı ayrıntılı olarak açıklar.

<!-- SHOT: flow-overview -->
Figure: Kesinleşmiş bir partinin **Süreç akışı**: her aşama, durumu ve sorumlu kurumuyla.

## Kontrol noktaları (KKN'ler) {#ccp-list}

**Kritik kontrol noktası (KKN)**, bir şeyin ölçüldüğü veya kontrol edildiği adımdır. Her KKN'nin parametreleri (örneğin sıcaklık veya nem), limitleri ve bir durumu vardır. KKN 0–3 **ham bal lotuna**, KKN 4–10 **partiye** aittir ([Tablo](#tbl-ccps)).

Table: Kritik kontrol noktaları (KKN'ler) ve varsayılan rolleri {#tbl-ccps}

| KKN | Ait olduğu | Neyin kontrol edildiği | Giren (varsayılan) | Onaylayan (varsayılan) |
|---|---|---|---|---|
| KKN 0 | Lot | Kabulde ön denetim, laboratuvar testinden **önce** | Tesisteki Numune kabul sorumlusu | Tesisteki Kalite Güvence Sorumlusu |
| KKN 1 | Lot | Numune alma ve laboratuvar sonuçları | Onaylı laboratuvar raporundan yazılır | Laboratuvar müdürü (veya tesisin Kalite Güvence Sorumlusu, aşağıya bakın) |
| KKN 2 | Lot | Sıcaklık izleme (ısıtma) | Tesisteki Personel | Kalite Güvence Sorumlusu |
| KKN 3 | Lot | Filtre durumu | Personel | Kalite Güvence Sorumlusu |
| KKN 4 | Parti | Homojenlik (karıştırma) | Parti sahibi kurumdaki Personel | Kalite Güvence Sorumlusu |
| KKN 5 | Parti | Süre ve sıcaklık (pastörizasyon) | Personel | Kalite Güvence Sorumlusu |
| KKN 6 | Parti | Köpük ve tortu giderme | Personel | Kalite Güvence Sorumlusu |
| KKN 7 | Parti | Miktar (dolum) | Personel (Paketleme) | Kalite Güvence Sorumlusu |
| KKN 8 | Parti | Etiket doğruluğu | Personel (Paketleme) | Kalite Güvence Sorumlusu |
| KKN 9 | Parti | Ambalaj bütünlüğü | Personel (Paketleme) | Kalite Güvence Sorumlusu |
| KKN 10 | Parti | Depolamada sıcaklık ve nem | Personel (Toptan Satış, Perakende veya Paketleme) | Kalite Güvence Sorumlusu |

KKN 10 bir partinin yaşamında iki kez yer alır: önce **partide, tesiste** (parti mühürlenmeden önce onaylanmış olmalıdır), daha sonra da bir sevkiyattan sonra **her alıcıda bir depolama kaydı** olarak.

Bir KKN kaydı şu durumlardan geçer: **Atandı** (henüz veri yok) → **Devam ediyor** (bazı veriler girildi) → **Gönderildi** (tüm zorunlu veriler girildi; onay bekliyor) → onayla birlikte **Tamamlandı**. Uygulanmayan bir KKN, bir gerekçeyle **Uygulanamaz** olarak işaretlenebilir; bu KKN'nin de onaylanması gerekir. Herhangi bir verinin değiştirilmesi mevcut onayı kaldırır.

## Adım adım {#flow-steps}

[Tablo](#tbl-flow-steps), her adımı, adımı kimin yaptığını ve neyi engellediğini listeler.

Table: Baştan sona akış, adım adım {#tbl-flow-steps}

| # | Adım | Kim | Nerede | Sonraki adım şu olana kadar bekler… |
|---|---|---|---|---|
| 1 | Arıcı ve arılık kaydedilir | Platform yöneticisi | **Arıcılar**, **Arılıklar** | Arıcı etkin ve bir arılığı var |
| 2 | Kayıt numarası doğrulanır, birlik bağlanır | Platform yöneticisi | **Arıcı kaydı ve birlikler** | (Teslimler için değil, herkese açık arıcı sayfası için gerekir) |
| 3 | Teslim kaydedilir: bir ham bal lotu oluşur | Numune kabul sorumlusu (tesis) | **Teslim almalar** → **Teslim al** | – (KKN 0–3 lota otomatik olarak atanır) |
| 4 | Numune mühürlenir ve laboratuvara gönderilir | Numune kabul sorumlusu | **Numuneler ve laboratuvar** → **Numune al** | Laboratuvar numuneyi teslim alır |
| 5 | KKN 0 girilir | Numune kabul sorumlusu | **İşlerim** → KKN 0 | Kayıt gönderilir |
| 6 | KKN 0 onaylanır | Kalite Güvence Sorumlusu (başka bir kişi) | **İşlerim** → KKN 0 | **Laboratuvar sonuçları ancak KKN 0 onaylandıktan sonra girilebilir** |
| 7 | Numune teslim alınır, mühür kontrol edilir | Analist veya Laboratuvar müdürü | **İşlerim** → **Teslim alınacak numuneler** | Mühür bozuksa numune reddedilir; yeni numune alınır |
| 8 | Laboratuvar sonuçları girilir, PDF yüklenir, rapor onaya gönderilir | Analist | **İşlerim** → **Girilecek laboratuvar sonuçları** | Tüm zorunlu değerler, PDF, rapor numarası ve tarih girilmiş |
| 9 | Laboratuvar raporu onaylanır (veya geri gönderilir) | Laboratuvar müdürü (başka bir kişi) | **İşlerim** → **Onaylanacak laboratuvar raporları** | Onay, sonuçları lotun KKN 1 kaydına yazar |
| 10 | KKN 2–3 girilir ve onaylanır | Personel, ardından Kalite Güvence Sorumlusu | **İşlerim** | Bir lot ancak **tüm** KKN'leri tamamlandığında veya uygulanamaz olarak işaretlendiğinde **ve onaylandığında** partiye girebilir |
| 11 | Parti oluşturulur, lotlar eklenir, sahip belirlenir, KKN 4–10 atanır | **Platform yöneticisi** (şimdilik) | **Nihai Bal Üretimleri** | Partinin lotları ve KKN'leri var |
| 12 | Tesisteki KKN 10 depolama dahil KKN 4–10 girilir ve onaylanır | Personel, ardından Kalite Güvence Sorumlusu | **İşlerim** | Partinin her KKN'si onaylanmış olmalı |
| 13 | Kesinleştirme talep edilir | Sahip kurumun Personel veya Kurum yöneticisi | **Kurumum** → parti → **Kesinleştirme talep et** | Eksik bir şey olduğu sürece düğme devre dışıdır |
| 14 | Kesinleştirme onaylanır: parti mühürlenir | Kalite Güvence Sorumlusu (başka bir kişi) | **İşlerim** → **Kesinleştirme talepleri** | – Bal Hikâyesi ve QR kod yayına girer; parti ve lotları kilitlenir |
| 15 | Sevkiyat oluşturulur | Partiyi elinde tutan kurumun Personel veya Kurum yöneticisi | **Sevkiyatlar ve stok** → **Sevk et** | Alıcı onaylar |
| 16 | Teslim onaylanır (veya reddedilir) | Alıcı kurumun Personel veya Kurum yöneticisi | **İşlerim** → **Onaylanacak sevkiyatlar** | Emanet yalnızca onayla el değiştirir |
| 17 | Alıcıda depolama kaydı (KKN 10) girilir ve onaylanır | Alıcının Personeli, ardından Kalite Güvence Sorumlusu | **Sevkiyatlar ve stok** → **Depolama** | – |
| 18 | Alıcı üreticinin kayıtlarını okur | Alıcı (KKN okuma yetkisi olan her rol) | KKN sayfaları, salt okunur | – |
| 19 | Tüketici QR kodu tarar | Herkese açık sayfaların her ziyaretçisi (hesap gerekmez) | Bal Hikâyesi | – |

### KKN 0 neden laboratuvar testinden önce gelir {#why-ccp0-first}

KKN 0 kabuldeki ön denetimdir: bu bal kabul edilebilir mi? Bu nedenle laboratuvar, lotun KKN 0 kaydı tamamlanıp **onaylanana** kadar sonuçları kaydedemez, onaya gönderemez ve onaylayamaz. O zamana kadar laboratuvar görevinde şu yazar: "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; laboratuvar sonuçları ancak bundan sonra girilip onaylanabilir." Bu bekleme sırasında bir rapor yine de geri gönderilebilir.

### Partideki KKN 10 neden mühürlemeden önce gerekir {#why-ccp10}

Kesinleştirme, partiyi ancak partiye atanmış her KKN onaylandığında mühürler. Partideki KKN 10 tesisin kendi depolama koşullarını kaydeder; bu nedenle tesisin kendi depolama kaydı mühürlemeden önce hazır olmalıdır. Partiyi sonradan elinde tutan her kurum, kendi stoku için kendi KKN 10 depolama kaydını tutar.

### Parti oluşturma şimdilik yalnızca yöneticidedir {#batch-admin-only}

Parti oluşturma, lotlarını ve miktarlarını seçme, sahip kurumu belirleme ve parti KKN'lerini atama işlerini bir platform yöneticisi yapar. Ardından tesis personeli parti KKN'lerini girer. Tesis personelinin partileri kendisinin oluşturup oluşturamayacağına proje karar verecektir.

## Neyin neyi engellediği: özet {#flow-blocks}

- **Teslim kaydı yapılamaz**: doğrulanmış bir İşleme kurumu ve teslim alma yetkisine sahip bir rol olmadan.
- **Laboratuvar sonucu girilemez**: mührü sağlam olarak teslim alınmış bir numune ve onaylanmış bir KKN 0 olmadan.
- **Raporu onaylayamaz**: sonuçları giren kişi.
- **Lot partiye giremez**: tüm KKN'leri tamamlanmadan veya uygulanamaz olarak işaretlenip onaylanmadan ve lot partinin toptancısına ait olmadan.
- **Kesinleştirme yapılamaz**: partide bal yokken, bir parti KKN'si eksik veya onaysızken ya da bir lot KKN'si açıkken; "Kesinleştirmeden önce eksik olanlar:" listesi her sorunu tek tek belirtir.
- **Kesinleştirmeyi onaylayamaz**: kesinleştirmeyi talep eden kişi.
- **Sevkiyat yapılamaz**: kesinleşmemiş bir parti için; elinizdeki miktardan fazlası da sevk edilemez.
- **Değişiklik yapılamaz**: kesinleşmiş bir partide veya lotlarında; iki yöneticinin yaptığı bir düzeltme dışında.
