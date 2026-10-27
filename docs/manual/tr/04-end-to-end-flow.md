# 4. Baştan sona akış {#flow}

Bu bölüm bir bal partisini platform boyunca izler: her adımı kimin yaptığını ve bir sonraki adım başlamadan önce neyin tamamlanması gerektiğini gösterir ([Şekil](#fig-flow-overview)). Rol bölümleri (5–14) her ekranı ayrıntılı olarak açıklar.

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
| 1 | Arıcı ve ilk arılığı kaydedilir: kayıt numarası, kimlik, aydınlatma metni ve rızalar, bir SMS koduyla (veya imzalı fotoğrafla) onaylanır | Arıcı Kayıt Görevlisi (ilçe müdürlüğü) veya platform yöneticisi | **Arıcı kaydı** | İmzalı fotoğrafla onaylanan bir arıcı, teslim almada ancak Uyum Sorumlusu fotoğrafı kabul ettikten sonra bulunur |
| 2 | Kayıt numarası doğrulanır, birlik bağlanır | Platform yöneticisi | **Arıcı kaydı ve birlikler** | (Teslimler için değil, herkese açık arıcı sayfası için gerekir) |
| 3 | Teslim kaydedilir: her kap ağırlığı ve mührüyle, her kaptan bir numune kavanozu ve bir şahit numune kavanozu; arıcı SMS koduyla onaylar; bir ham bal lotu oluşur | Numune kabul sorumlusu (tesis) | **Teslim almalar** → **Teslim al** | – (KKN 0–3 lota otomatik olarak atanır; kavanozlar seçilen laboratuvara gider) |
| 4 | Etiketler yazdırılır ve kaplara ve kavanozlara yapıştırılır | Numune kabul sorumlusu | Teslimat sayfası → **Etiketler** | – |
| 5 | KKN 0 girilir | Numune kabul sorumlusu | **İşlerim** → KKN 0 | Kayıt gönderilir |
| 6 | KKN 0 onaylanır | Kalite Güvence Sorumlusu (başka bir kişi) | **İşlerim** → KKN 0 | **Laboratuvar sonuçları ancak KKN 0 onaylandıktan sonra girilebilir** |
| 7 | Her kavanoz teslim alınır: mühür numarası, etiket ve kapak kontrol edilir | Analist veya Laboratuvar müdürü | **İşlerim** → **Teslim alınacak numuneler** | Başarısız bir kontrol kavanozu reddeder; tesis aynı kaptan yeni bir kavanoz alır |
| 8 | Teslim alınan kavanozlardan karışım numunesi oluşturulur (karışım analizi) | Analist veya Laboratuvar müdürü | **Numuneler ve laboratuvar** → **Karışım numunesini oluştur** | Her kabın kavanozu sağlam olarak teslim alınmış olmalı |
| 9 | Laboratuvar sonuçları girilir, PDF yüklenir, rapor onaya gönderilir | Analist | **İşlerim** → **Girilecek laboratuvar sonuçları** | Tüm zorunlu değerler, PDF, rapor numarası ve tarih girilmiş |
| 10 | Laboratuvar raporu onaylanır (veya geri gönderilir) | Laboratuvar müdürü (başka bir kişi) | **İşlerim** → **Onaylanacak laboratuvar raporları** | Onay, sonuçları lotun KKN 1 kaydına yazar |
| 11 | KKN 2–3 girilir ve onaylanır | Personel, ardından Kalite Güvence Sorumlusu | **İşlerim** | Bir lot ancak **tüm** KKN'leri tamamlandığında veya uygulanamaz olarak işaretlendiğinde **ve onaylandığında** partiye girebilir |
| 12 | Parti oluşturulur, lotlar eklenir, sahip belirlenir, KKN 4–10 atanır | **Platform yöneticisi** (şimdilik) | **Nihai Bal Üretimleri** | Partinin lotları ve KKN'leri var |
| 13 | Tesisteki KKN 10 depolama dahil KKN 4–10 girilir ve onaylanır | Personel, ardından Kalite Güvence Sorumlusu | **İşlerim** | Partinin her KKN'si onaylanmış olmalı |
| 14 | Kesinleştirme talep edilir | Sahip kurumun Personel veya Kurum yöneticisi | **Kurumum** → parti → **Kesinleştirme talep et** | Eksik bir şey olduğu sürece düğme devre dışıdır (Uyum Sorumlusunun henüz kabul etmediği bir teslim fotoğrafı da dahil) |
| 15 | Kesinleştirme onaylanır: parti mühürlenir | Kalite Güvence Sorumlusu (başka bir kişi) | **İşlerim** → **Kesinleştirme talepleri** | – Bal Hikâyesi ve QR kod yayına girer; parti ve lotları kilitlenir; arıcının fiyatı kilitlenir |
| 16 | Sevkiyat oluşturulur | Partiyi elinde tutan kurumun Personel veya Kurum yöneticisi | **Sevkiyatlar ve stok** → **Sevk et** | Alıcı onaylar |
| 17 | Teslim onaylanır (veya reddedilir) | Alıcı kurumun Personel veya Kurum yöneticisi | **İşlerim** → **Onaylanacak sevkiyatlar** | Emanet yalnızca onayla el değiştirir |
| 18 | Alıcıda depolama kaydı (KKN 10) girilir ve onaylanır | Alıcının Personeli, ardından Kalite Güvence Sorumlusu | **Sevkiyatlar ve stok** → **Depolama** | – |
| 19 | Alıcı üreticinin kayıtlarını okur | Alıcı (KKN okuma yetkisi olan her rol) | KKN sayfaları, salt okunur | – |
| 20 | Tüketici QR kodu tarar | Herkese açık sayfaların her ziyaretçisi (hesap gerekmez) | Bal Hikâyesi | – |

Akışla eş zamanlı olarak arıcı, teslimlerini, numunelerini ve partilerini arıcı portalında izler ve istediği fiyatı girer (bkz. [bölüm 14](#portal)); her kurumun Uyum Sorumlusu da imzalı fotoğrafları, reddedilen numuneleri ve kırık mühürleri inceler (bkz. [bölüm 13](#role-co)).

### SMS kodu veya imzalı fotoğrafla kayıt {#flow-registration}

Bir arıcı, ilçe tarım müdürlüğünün arıcı kayıt görevlisi (veya bir platform yöneticisi) tarafından bir kez kaydedilir. Görevli işletme kayıt numarasını (`TR`, il plaka kodu ve 10 hane) müdürlüğün iline göre kontrol eder, arıcıyı ve ilk arılığı kaydeder, arıcıya aydınlatma metnini okutur ve arıcının tercihlerini kaydeder: adının herkese açık sayfalarda gösterilmesine ilişkin isteğe bağlı rıza ve SMS bildirimlerine ilişkin isteğe bağlı rıza. Arıcı bunların hepsini SMS ile gelen ve kendisinin okuduğu **6 haneli bir kodla** onaylar; kod tam olarak bu içeriğe bağlıdır.

Cep telefonu olmayan bir arıcı bunun yerine basılı formu imzalar; görevli formun fotoğrafını gerekçesiyle birlikte yükler. Böyle bir kayıt müdürlüğün **Uyum Sorumlusunu** bekler; fotoğraf kabul edilene kadar arıcı teslim yapamaz (teslim almada "Bu numarayla etkin bir arıcı yok."), reddedilen bir fotoğraf da rızaları geri çeker. Cep telefonu olmayan bir arıcı portal hesabı alamaz. Bkz. [bölüm 12](#role-ro).

### Kaplar, mühürler ve numune kavanozları {#flow-containers}

Bal **kaplar** (tenekeler, bidonlar, variller) içinde gelir. Kabulde numune kabul sorumlusu her kap için bir satır kaydeder:

1. **Gelişteki mühür**: mühürlü bir kap kontrol edilir (mühür numarası ve **mühür sağlam**); mühürsüz bir kaba arıcının önünde bir işletme mührü takılır (**Şimdi takılan işletme mührü**).
2. **Ağırlık**: net kilogram (ve isteğe bağlı olarak brüt); lotun miktarı kapların toplamıdır.
3. **Numune kavanozları**: kap açılır ve ondan, her birinin kendi mühür numarası ve **güvenlik kapağı** olan bir **numune kavanozu** ve bir **şahit numune kavanozu** doldurulur. Kavanoz mühürlenirken kapak sağlam (düğme aşağıda) olmalıdır; fırlamış bir kapak kavanozun açıldığını gösterir; bu kavanoz atılır ve yeni bir kavanoz doldurulur.
4. Numune alındıktan sonra **yeni bir mühür** kabı kapatır.

Her adım, yalnızca eklenebilen, hiçbir zaman değiştirilemeyen bir **mühür olayıdır**. Numune kavanozları laboratuvara gider; şahit numuneler **Saklama bitişi** altında belirlenen süre boyunca tesiste kalır.

**Arıcının onayı.** Arıcıya kurumu, tarihi, kapları, kilogramları, bal türünü, numune sayısını ve bir kodu içeren bir SMS gider. Kod girildiğinde teslim onaylanır ve teslimin **içerik parmak izi** saklanır. Kod gönderildikten sonra herhangi bir şeyin değişmesi kodu geçersiz kılar ve yeni bir kod gerekir. Arıcı ayrıldıysa görevli onu arar, arıcı kodu telefonundan okur. SMS olmadan (telefon yok, çekmiyor) görevli gerekçesiyle birlikte **imzalı teslim formunu** yükler: lot üzerinde çalışılabilir, ancak lot yalnızca tesisin Uyum Sorumlusu fotoğrafı kabul ettikten sonra kesinleştirilebilir.

<!-- SHOT: intake-containers -->
Figure: Teslim kaydı: her kap için mühürleri ve numune kavanozlarıyla bir satır.

**Etiketler.** Teslimat sayfası QR kodlu etiketleri yazdırır: her kap için bir tane (`L{lot}-C{n}`), her numune kavanozu için bir tane (`S-{lot}-C{n}`) ve her şahit numune için bir tane (`…-K`); A4 bir sayfada (2 × 5) veya sayfa başına bir 100 × 50 mm etiket olarak.

### Laboratuvarda kabul {#flow-reception}

Laboratuvar her kavanoz için üç kontrol kaydeder: **Mühür numarası kayıtla aynı**, **Etiket sağlam** ve **Kapak sağlam (düğme aşağıda)**. Yanıtlardan biri **Hayır** ise kavanoz bir gerekçeyle reddedilir. Bunun üzerine tesis **aynı kaptan yeni bir kavanoz** alır (**Aç, numune al ve yeniden mühürle**); bu işlem kap mührünü yeniden kırar ve yeni bir mühür takar; ikisi de mühür geçmişinde görünür. Onaylanmış teslim içeriği ilk kavanozu korur.

### Karışım veya kap başına analiz {#flow-analysis}

Platform bir lotu analiz etmenin iki yolunu destekler; hangisinin kullanılacağına platformun ayarı karar verir:

- **Karışım (varsayılan).** Her kabın kavanozu sağlam olarak teslim alındığında laboratuvar, kavanozlardan eşit miktarlarla **tek bir karışım numunesi** oluşturur (**Karışım numunesini oluştur**, kod `S-{lot}-COMP`) ve onu analiz eder. Onaylanan raporu lotun KKN 1'ini doldurur. Tek tek kavanozlar saklanmaya devam eder; karışım başarısız olursa veya istenirse tek bir kavanoz ayrıca analiz edilir. Böyle bir tek kavanoz raporu kendi kavanozunda kalır ve lotun KKN 1 değerlerinin yerine geçmez.
- **Kap başına.** Her kavanoz ayrı ayrı analiz edilir ve her onaylı kavanoz raporu lot için geçerli olur.

### En kötü değer kuralı {#flow-worst}

Kavanozlar tek tek analiz edildiğinde, lotun her parametre için KKN 1 değeri kaplar arasındaki **en kötü** değerdir:

- önce limitleri dışındaki bir değer gelir; limitin en çok dışında olanı;
- aksi halde, önemli olan yönde limite en yakın değer (azami değer olan yerde en yükseği, asgari değer olan yerde en düşüğü);
- limit yoksa en yüksek değer;
- metin yanıtlarının hepsi, her biri kendi kabıyla birlikte saklanır.

Laboratuvar raporu ve geri çağırma raporu her kabın değerlerini ("Kap başına laboratuvar değerleri") gösterir; lot değeri kalın yazılır.

### Teslim bilgilerinin düzeltilmesi {#flow-corrections}

Bir teslim onaylandıktan sonra bilgileri (tarih, kapların türü ve ağırlıkları, mühür numaraları) yöneticiler için de kilitlenir. Bir hata **iki platform yöneticisi** tarafından düzeltilir: biri gerekçesiyle düzeltmeyi talep eder, ikincisi onaylar (talep eden hiçbir zaman karar vermez). Özgün ve düzeltilmiş içerik parmak izleriyle birlikte teslimat sayfasında okunabilir kalır ve arıcıya bilgilendirme SMS'i gider. Bir düzeltmeyle hiçbir zaman kap eklenmez veya çıkarılmaz.

### KKN 0 neden laboratuvar testinden önce gelir {#why-ccp0-first}

KKN 0 kabuldeki ön denetimdir: bu bal kabul edilebilir mi? Bu nedenle laboratuvar, lotun KKN 0 kaydı tamamlanıp **onaylanana** kadar sonuçları kaydedemez, onaya gönderemez ve onaylayamaz. O zamana kadar laboratuvar görevinde şu yazar: "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; laboratuvar sonuçları ancak bundan sonra girilip onaylanabilir." Bu bekleme sırasında bir rapor yine de geri gönderilebilir.

### Partideki KKN 10 neden mühürlemeden önce gerekir {#why-ccp10}

Kesinleştirme, partiyi ancak partiye atanmış her KKN onaylandığında mühürler. Partideki KKN 10 tesisin kendi depolama koşullarını kaydeder; bu nedenle tesisin kendi depolama kaydı mühürlemeden önce hazır olmalıdır. Partiyi sonradan elinde tutan her kurum, kendi stoku için kendi KKN 10 depolama kaydını tutar.

### Parti oluşturma şimdilik yalnızca yöneticidedir {#batch-admin-only}

Parti oluşturma, lotlarını ve miktarlarını seçme, sahip kurumu belirleme ve parti KKN'lerini atama işlerini bir platform yöneticisi yapar. Ardından tesis personeli parti KKN'lerini girer. Tesis personelinin partileri kendisinin oluşturup oluşturamayacağına proje karar verecektir.

### Kaplardan önce kaydedilen teslimler {#flow-legacy}

Kap prosedüründen önce (veya bir tesis henüz bu prosedür olmadan çalışırken) kaydedilen teslimler **eski kayıt (kaplar kaydedilmemiş)** olarak işaretlenir: tek bir imzalı teslim formu ve tek bir mühür kontrolüyle tek bir numuneleri vardır. Geçerliliklerini korurlar ve eskisi gibi kesinleştirilir.

## Neyin neyi engellediği: özet {#flow-blocks}

- **Teslim kaydı yapılamaz**: doğrulanmış bir İşleme kurumu, teslim alma yetkisine sahip bir rol ve kayıtlı bir arıcı olmadan (fotoğrafla yapılan bir kayıt ancak Uyum Sorumlusu kabul ettikten sonra).
- **Teslim onaylanamaz**: arıcının tam olarak bu içerik için gelen SMS kodu ya da gerekçeli imzalı bir fotoğraf olmadan.
- **Laboratuvar sonucu girilemez**: mühür, etiket ve kapak kontrollerini geçmiş, teslim alınmış bir kavanoz ve onaylanmış bir KKN 0 olmadan.
- **Raporu onaylayamaz**: sonuçları giren kişi.
- **Lot partiye giremez**: tüm KKN'leri tamamlanmadan veya uygulanamaz olarak işaretlenip onaylanmadan ve lot partinin toptancısına ait olmadan.
- **Kesinleştirme yapılamaz**: partide bal yokken, bir parti KKN'si eksik veya onaysızken, bir lot KKN'si açıkken ya da bir teslim fotoğrafı Uyum Sorumlusunu beklerken; "Kesinleştirmeden önce eksik olanlar:" listesi her sorunu tek tek belirtir.
- **Kesinleştirmeyi onaylayamaz**: kesinleştirmeyi talep eden kişi.
- **Sevkiyat yapılamaz**: kesinleşmemiş bir parti için; elinizdeki miktardan fazlası da sevk edilemez.
- **Değişiklik yapılamaz**: kesinleşmiş bir partide veya lotlarında ya da onaylanmış bir teslimin bilgilerinde; iki yöneticinin yaptığı bir düzeltme dışında.
