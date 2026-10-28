# 15. Platform yöneticisi {#admin}

## Günlük işiniz {#admin-daily}

Platform yöneticileri platformu işletir. Kurumları doğrular, üyelikleri ve yetkileri yönetir, giriş bilgilerini gönderir, aydınlatma metinlerini yayımlar, referans verilerini ve kontrol noktası tanımlarını güncel tutar, arıcıları kaydeder, (şimdilik) parti oluşturur, düzeltmelere karar verir ve denetim kaydını izler.

Yöneticiler her kaydı okuyabilir, ancak **iş verisi girmez ve onaylamaz**. Oturum açtıklarında **İşlerim** sayfası açılır; bu sayfada **Doğrulanacak kurumlar**, **Doğrulanacak kayıt numaraları** ve **Karar bekleyen düzeltmeler** grupları yer alır. **Panel** genel bir bakış sunar (rakamlar, zincir, il bazında arıcı ve arılık haritası, son etkinlikler ve bekleyen onaylar).

<!-- SHOT: admin-dashboard -->
Figure: Platform yöneticisinin paneli.

Yönetim menüsü ([Tablo](#tbl-admin-menu)):

Table: Yönetim menüsü {#tbl-admin-menu}

| Menü | Öğeler |
|---|---|
| **Panel** | Genel bakış |
| **Arıcılar** | Arıcı listesi, arılıklar, eğitim seviyeleri, arıcılık amaçları, arıcılık türleri |
| **Ham Bal Üretimi** | Ham bal lotları, bal türleri, üretim dönemleri |
| **Toptancılar** | Toptancı listesi, toptancı ekleme |
| **Nihai Bal Üretimleri** | Partiler |
| **Kritik Kontrol Noktaları** | KKN'ler, parametre kategorileri, parametreler, referans verisi çevirileri |
| **Kullanıcı Yönetimi** | Kullanıcılar, roller, **Denetim kaydı**, **Veri saklama**, **Aydınlatma ve rıza metinleri**, **SMS giden kutusu (test sağlayıcısı)** (yalnızca test SMS sağlayıcısıyla) |
| **Kurumlar** | İşlerim, kurumlar, yetenekler, arıcı kaydı ve birlikler, arıcı kaydı, roller ve yetkiler, KKN sorumlulukları, düzeltmeler, sevkiyatlar, numuneler, arıcı sayfaları |

## Kurumlar {#admin-organizations}

### Kurum oluşturma {#admin-org-create}

1. **Kurumlar** → **Yeni kurum**.
2. Adı, ticari unvanı, vergi numarasını, ili ve ilçeyi girin.
3. Kurum platformda zaten kayıtlı bir toptancıysa, mevcut toptancı alanında onu seçin. **İşleme tesisi kendi toptancı kaydına bağlanmalıdır**: tesisin teslim aldığı lotlar o toptancının altında tutulur ve bir parti yalnızca kendi toptancısının lotlarını kullanabilir. Bağlantı daha sonra kurumun **Düzenle** sayfasında da kurulabilir. Bir tesisin bağlanmadan önce teslim aldığı lotlar, bağlandığı toptancının partilerinde kullanılır.
4. En az bir yeteneği işaretleyin ve **Oluştur** düğmesine tıklayın.

Kurum **Beklemede** durumunda başlar: "Kurum oluşturuldu. Doğrulanana kadar Beklemede kalır."

<!-- SHOT: admin-org-create -->
Figure: Kurum oluşturma: toptancı bağlantısı ve yetenekler.

### Doğrulama, belge isteme, askıya alma {#admin-org-verify}

Kurum sayfasında (bir platform yöneticisi; platform bu şekilde yapılandırılmışsa arıcı birlikleri için bunun yerine doğrulanmış bir merkez birliğin kurum yöneticisi: bu durumda yöneticiler birliklerde erişim reddiyle karşılaşır):

- **Doğrula** (Beklemede durumundan) için en az bir yüklenmiş belge ve ilk kez doğrulanan bir kurum için etkin bir Uyum Sorumlusu gerekir.
- **Belge iste** (Beklemede iken) için bir gerekçe gerekir, örneğin hangi belgenin eksik olduğu.
- **Askıya al** (Doğrulandı iken) için bir gerekçe gerekir. **Yeniden etkinleştir**, askıya alınmış bir kurumu yeniden doğrular.

Her değişiklik doğrulama geçmişinde ve denetim kaydında görünür.

### Yetenekler {#admin-org-capabilities}

**Yetenek ekle** bir yetenek ekler. **Laboratuvar** için akredite kutusunu işaretleyin, akreditasyon numarasını ve geçerlilik tarihini girin; akredite bir laboratuvarın numarası olmalıdır. **Kaldır** bir yeteneği kaldırır; her kurumun en az bir yeteneği kalır. **Yetenekler** sayfası her yetenek için hangi KKN'lerin o yeteneği gerektirdiğini ve hangi kurumların o yeteneğe sahip olduğunu gösterir. **Roller ve yetkiler** altındaki **Kurum yetkinlikleri** sekmesi, tüm kurumları tek seferde düzenlemenizi sağlar.

### Üyeler {#admin-members}

1. Kurum sayfasında, üyeler bölümünde rolü seçin. **Arıcı Kayıt Görevlisi** rolü yalnızca Arıcı kaydı yeteneğine sahip bir kurumda kullanılabilir.
2. Mevcut bir kullanıcı adı girin ya da kullanıcıyı aynı anda oluşturmak için yeni bir kullanıcı adı, e-posta ve **Cep telefonu** girin. Kullanıcının giriş bilgilerini nasıl alacağını seçin: **Kullanıcı adını ve geçici şifreyi SMS ile gönder (önerilir)** veya **İlk şifreyi burada belirle (kullanıcı ilk girişte değiştirir)**.
3. **Üye ekle** düğmesine tıklayın: "…, … olarak eklendi."

Üyelik, bir kullanıcı hesabı × bir kurum × bir rol demektir; bir kullanıcının birden fazla üyeliği olabilir. **İptal et** bir üyeliği sona erdirir ve kullanıcının tüm oturumlarını kapatır. **Roller ve yetkiler** altındaki **Üyeler** sekmesi, tüm kurumların tüm üyeliklerini listeler.

<!-- SHOT: admin-org-members -->
Figure: Bir kurumun üyelikleri ve **Üye ekle** formu.

### Dört göz muafiyeti {#admin-four-eyes}

Tek bir kalite sorumlusu olan küçük kurumlara **Dört göz muafiyeti** tanınabilir: bu durumda üyeler kendi KKN kayıtlarını ve kendi kesinleştirme taleplerini onaylayabilir. Bu tür her onay denetim kaydında işaretlenir ve kurumun Uyum Sorumlusu bunu **Onay mekanizması** altında görür. Muafiyet laboratuvar sonuçlarına veya imzalı fotoğrafların incelenmesine hiçbir zaman uygulanmaz.

### Veri sorumlusu {#admin-data-controller}

Her kurum, kaydettiği kişisel verilerin veri sorumlusudur. Kurum sayfasındaki **Veri sorumlusu** bölümü kurumun **Tüzel kişi adı**, **İletişim (e-posta veya telefon)** ve **Adres** bilgilerini tutar; bu bilgiler kurumun aydınlatma metninde belirtilir. Bunları kurumun Uyum Sorumlusu veya bir platform yöneticisi güncel tutar.

## Kullanıcılar {#admin-users}

**Kullanıcı Yönetimi** → **Kullanıcılar** hesapları listeler.

- **Yeni Kullanıcı Ekle** bir hesap oluşturur. Kurum modelinde kurum üyeleri için **Genel rol** alanını boş bırakın ve onlara kurum sayfasında bir üyelik verin; **Platform yöneticisi** seçeneğini yalnızca yöneticiler için seçin. **Cep telefonu** alanını doldurun ve **Kullanıcı adını ve geçici şifreyi SMS ile gönder (önerilir)** ya da **İlk şifreyi burada belirle (kullanıcı ilk girişte değiştirir)** seçeneğini seçin.
- **Girişi SMS ile gönder**, giriş bilgilerini kaybetmiş bir kullanıcıya yeni giriş bilgileri (72 saat geçerli yeni bir geçici şifre) gönderir. Geçici şifreyi yönetici dahil hiç kimse görmez.
- **Parolayı Sıfırla** bir kullanıcı için yeni şifre belirler; bu şifre de kullanıcının bir sonraki oturum açışında değiştireceği bir ilk şifre sayılır.
- **Kilidi aç**, **Kilitli** bir hesabın yanında görünür ve kilidi hemen kaldırır ("Hesabın kilidi açıldı."). Bu düğme kullanılmazsa kilit 15 dakika sonra kendiliğinden kalkar.
- **Sil** bir hesabı kaldırır (dikkatli kullanın; geri alınamaz). Kendi hesabınızı silemezsiniz. Her silme ve her kilit açma işlemi denetim kaydına yazılır.

Yeni şifreler en az 10 karakter olmalı, kullanıcı adı ve yaygın bir şifre olmamalıdır; mevcut şifreler geçerliliğini korur. 5 başarısız oturum açma denemesinden sonra hesap 15 dakika kilitlenir.

**Kullanıcı Yönetimi** → **Roller** genel rolleri listeler. Platformun dayandığı roller (**Yönetici** ve **Arıcı**) **Korumalı** olarak işaretlidir; yeniden adlandırılamaz ve silinemez.

## Roller ve yetkiler {#admin-permissions}

**Roller ve yetkiler** sayfasında üç sekme vardır:

- **Üyeler**: tüm etkin üyelikler; üye ekleme ve üyelik iptali.
- **Rol yetkileri**: bir rol seçin ve her işlev için **Giriş**, **Onayla** ve **Okuma** kutularını işaretleyin. İşlevler, KKN'ler (her biri ayrı bir satır, sayfanın dilindeki KKN kısaltmasıyla) ile teslim alma, numuneler, laboratuvar sonuçları, sevkiyatlar, arıcı sayfaları, geri çağırma, kesinleştirme, arıcı kaydı, kanıtlar, fiyat ve kurumun denetim kaydı girişleri alanlarıdır. Her yetki en az bir rolde kalmalıdır. Değişiklikler hemen uygulanır.
- **Kurum yetkinlikleri**: kurum başına yetenekler.

"Temel kurallar (kapatılamaz):" kutusu, hiçbir ayarın değiştiremediği kuralları listeler (bkz. [Temel kurallar](#fixed-rules)). Bir yöneticinin tam olarak neleri değiştirebileceğini [Tablo](#tbl-admin-changeable) listeler.

<!-- SHOT: admin-role-permissions -->
Figure: Kalite Güvence Sorumlusunun rol yetkileri.

## KKN sorumlulukları {#admin-ccp-responsibilities}

**KKN sorumlulukları** sayfası her KKN için hangi kurumun sorumlu olduğunu (lotun veya partinin sahibi ya da lotlar için arıcının birliği), hangi yeteneğin gerektiğini, hangi rollerin veri girdiğini ve hangi rollerin onayladığını ve onayın gerekip gerekmediğini belirler. Örnek: "Lotun arıcısının birliği", **Birlik** ve **Saha denetçisi** seçilerek KKN 0 birliğe verilir.

## Kontrol noktaları, parametreler ve limitler {#admin-ccps}

- **Kritik Kontrol Noktaları**: kod, başlık, işleme adımı, kritiklik gerekçesi ve **Uygulandığı yer** (ham bal lotu veya nihai parti). Yanıtlar kişisel veri içerebiliyorsa, serbest metin değerlerini herkese açık Bal Hikâyesi'nde gösterme seçeneği kapalı kalmalıdır.
- **Parametreler**: ad, birim, kategori, veri türü (**Sayı**, **Evet / Hayır**, **Metin**, **Dosya**), en küçük ve en büyük değer, zorunlu veya isteğe bağlı oluşu ve "Yalnızca şu soru…" koşulu (parametre, başka bir evet/hayır sorusu belirli bir yanıt aldığında zorunlu olur).
- **Limitler**: her parametre için tüm bal türlerine uygulanan varsayılan limitler ile bal sınıfına (çiçek, salgı, çam) veya bal türüne göre özel limitler; isteğe bağlı olarak bir koşul, bir toplam veya bir oranla. Limitler değiştiğinde kayıtlı sonuçlar yeniden hesaplanmaz.

**Doğrulanmış limitler.** Her parametrede **Limitler doğrulandı (herkese açık gösterilir)** kutusu bulunur. Bal Hikâyesi'nde ve arıcı sayfasında yalnızca doğrulanmış limitler herkese açık bir limit içinde/limit dışında değerlendirmesi üretir. Doğrulanmamış limitler panelde "doğrulanacak" olarak işaretlenir; herkese açık sayfalar ise limitlerin hâlâ kontrol edildiğini belirtir.

<!-- SHOT: admin-parameter-limits -->
Figure: Bir parametrenin limitleri (KKN 1, nem).

## Referans verileri ve çeviriler {#admin-reference}

Bal türleri (limitler için kullanılan bal sınıflarıyla birlikte), üretim dönemleri, eğitim seviyeleri, arıcılık türleri ve amaçları ve toptancılar kendi menülerinde tutulur. Kullanımda olan bir kayıt silinemez.

**Referans verisi çevirileri**, listelerde, tablolarda ve herkese açık sayfalarda gösterilen adları beş dilde tutar: bal türleri, dönemler, durumlar, yetenekler, roller, KKN'ler, parametreler, parametre kategorileri, limit notları, parametre açıklamaları ve ülkeler. Boş bırakılan bir alan için önce İngilizce metin, o da yoksa temel metin gösterilir.

## Arıcılar, arılıklar ve kayıt {#admin-registry}

- **Arıcı kaydı**: arıcıları ilçe müdürlüklerinin arıcı kayıt görevlileri bir SMS kodu veya imzalı fotoğrafla kaydeder (bkz. [bölüm 12](#role-ro)). Bir platform yöneticisi de arıcıları aynı şekilde kaydedebilir; bu kayıtlar platformun kendi kurumu adına tutulur ve imzalı fotoğraflarını başka bir platform yöneticisi inceler.
- **Arıcılar**: arıcı listesi ve arıcı sayfası (ad, doğum tarihi, deneyim, telefon, eğitim, arıcılık türü ve amacı, isteğe bağlı fotoğraf). İşletme numarası otomatik üretilir. Kayıt sırasında tasdik edilen bilgiler (kimlik, kayıt numarası, telefon) burada salt okunurdur; yalnızca yeni bir kayıtla değişir. Arıcılar hiçbir zaman silinmez, yalnızca pasifleştirilir. Kayıt prosedüründen önce kaydedilmiş eski arıcılar aynı formla yeniden doğrulanır; eski girişleri kilitli kalır.
- **Arılıklar**: başlık, kovan sayısı ve konum (enlem ve boylam). Konum hiçbir zaman herkese açık gösterilmez; herkese açık sayfalar yalnızca ili gösterir.
- **Arıcı kaydı ve birlikler**: her arıcı için birliği bağlayın ve işletme kayıt numarasını (biçim: `TR` + il kodu + 10 rakam, örneğin `TR061234567890`) bir kanıt belgesiyle birlikte girin. Ardından **Doğrula** veya **Reddet** düğmesine tıklayın (reddetmek için gerekçe zorunludur).

<!-- SHOT: admin-registry -->
Figure: Arıcı kaydı ve birlik.

## Partiler (şimdilik yalnızca yönetici) {#admin-batches}

1. **Toptancılar** → toptancı → **Nihai Bal Üretimleri (Partiler)** → **Yeni Parti**.
2. **Parti Başlığı** ve **Üretim Tarihi** alanlarını doldurun ve **Parti Oluştur** düğmesine tıklayın. QR kod üretilir.
3. **Partiyi Yönet** sayfasında uygun her lottan (bu toptancının, tüm KKN'leri onaylanmış lotları) kullanılacak miktarı girin ve **Parti Bileşimini Kaydet** düğmesine tıklayın: "Parti bileşimi kaydedildi: toplam … kg." Bir miktar çok büyükse veya bir lot kullanılamıyorsa sayfa her sorunu listeler (örneğin "Lot #…: en çok … kg kullanılabilir.") ve hiçbir şey kaydetmez. Bir tesisin, bir toptancı kaydına bağlı olmadığı için hiçbir partinin alamadığı lotları varsa sayfa o tesisin adını, **Düzenle** sayfasına bir bağlantıyla birlikte gösterir.
4. Sahip kurumu (tesisi) belirlemek için partiyi **Düzenle** ile açın.
5. **KKN Yönet**: KKN 4'ten KKN 10'a kadar işaretleyin ve **Atamaları Kaydet** düğmesine tıklayın: "KKN atamaları kaydedildi: … eklendi, … kaldırıldı."

Ardından tesis parti KKN'lerini girer ve partiyi kesinleştirir. Yöneticiler kesinleştirme yapamaz: "Kurum modelinde partiyi sahibi olan kurum kesinleştirir (iki kişi: talep ve onay)."

<!-- SHOT: admin-manage-batch -->
Figure: Uygun lotlardan parti bileşimini belirleme.

## Düzeltmeler {#admin-corrections}

Onaylanmış veya kilitli veriler yalnızca bir düzeltmeyle değiştirilir:

1. Kaydın KKN sayfasını açın. **Düzeltmeler** kutusunda parametreyi seçin, yeni değeri ve bir **gerekçe** girin ve **Talep et** düğmesine tıklayın: "Düzeltme talep edildi; başka bir platform yöneticisinin onaylaması gerekir."
2. **İkinci** bir yönetici **Düzeltmeler** sayfasını açar, eski ve yeni değeri ve gerekçeyi okur, ardından **Onayla ve uygula** veya **Reddet** düğmesine tıklar (reddetmek için not zorunludur).

Düzeltmeyi talep eden kişi o düzeltmeye karar veremez. Düzeltmeler kilitli kayıtlara da uygulanır ve her iki adım da denetim kaydında yer alır.

**Teslim bilgileri.** Onaylanmış bir teslimin bilgileri (tarih, kapların türü ve ağırlıkları, mühür numaraları) teslimat sayfasında, **Bu teslimatın düzeltmeleri** altında düzeltilir ("Yalnızca kayıttaki bir hata için. Düzeltmeyi başka bir platform yöneticisi onaylamalıdır; özgün kayıt burada okunabilir kalır ve arıcıya SMS ile bilgi verilir."):

1. **Düzeltme iste** düğmesine tıklayın, alanları değiştirin, **Gerekçe** alanını doldurun ve **İsteği gönder** düğmesine tıklayın: "Düzeltme istendi; başka bir platform yöneticisi onaylamalıdır."
2. İkinci bir yönetici **Özgün** ve **Düzeltilmiş** içeriği karşılaştırır ve **Onayla ve uygula** düğmesine tıklar ya da **Not (reddetmek için gerekli)** alanını doldurup **Reddet** düğmesine tıklar. "Düzeltme uygulandı; arıcıya SMS ile bilgi verildi."

Bir düzeltmeyle hiçbir zaman kap eklenmez veya çıkarılmaz. Teslim, parmak izleriyle birlikte iki içeriği de saklar; denetim kaydı değişen her satırı kaydeder.

<!-- SHOT: admin-delivery-correction -->
Figure: İkinci yöneticiyi bekleyen bir teslim düzeltmesi.

## Arıcı sayfaları, sevkiyatlar ve numuneler {#admin-overviews}

Yöneticiler, tüm kurumların çalışmasını izlemek için **Arıcı sayfaları**, **Sevkiyatlar ve stok** ve **Numuneler ve laboratuvar** sayfalarını salt okunur olarak açabilir.

## Geri çağırma {#admin-recall}

Bir partinin veya lotun **Geri çağırma raporu** şunları listeler:

- **kaynak**: lotlar, arılıklar, arıcılar ve teslim formları; kaplarla kaydedilmiş teslimler için ayrıca mühür geçmişiyle her kap, numune kavanozları, kap başına laboratuvar değerleri ve onaylanan içeriğin değişmeden kalıp kalmadığı;
- partiler;
- **sonrası**: sevkiyatlar ve partiyi şu anda elinde tutan kurumlar.

Üyeler, kurumlarının okuyabildiği kısmı okur (sahip, partiyi elinde tutan kurum veya birlik); gizlenen satırların yalnızca sayısı gösterilir. Bir üye yalnızca kendi elindeki miktarı **Mevcut elinizdeki miktar** altında görür; yöneticiler tüm sahipleri **Şu anki emanetçiler** altında görür. Geri çağırma raporunda hiçbir zaman fiyat yer almaz. **Yazdır** ve **CSV** raporu dışa aktarır.

<!-- SHOT: recall-report -->
Figure: Bir partinin geri çağırma raporu.

## Aydınlatma ve rıza metinleri {#admin-notices}

**Aydınlatma ve rıza metinleri**, aydınlatma metinlerini (kullanıcılar ve arıcılar için; platformun ve her kurumun), rıza metinlerini ve kayıt beyanını dil başına, **Sürüm**, **Yayımlanma** tarihi ve kaç kişinin metni kabul ettiğini (**Kabul**) gösterecek şekilde listeler. "Metinler sürümlüdür ve yayımlandıktan sonra değiştirilmez; bunun yerine yeni bir sürüm yayımlayın."

1. **Yeni sürüm yayımla** (veya bir metnin yanındaki **Yeni sürüm**) düğmesine tıklayın.
2. **Tür**, metnin **Kimin için** olduğu, **Kurum** (platform için boş), bir rıza metni için **Rıza amacı**, **Sürüm** ve **Dil** alanlarını seçin ve **Metin** alanını yazın.
3. Metin henüz onaylanmadıysa **Taslak (henüz KVKK birimince onaylanmadı)** kutusunu işaretleyin; metin bu durumda **TASLAK** işaretiyle gösterilir.
4. **Yayımla** düğmesine tıklayın. "Yayımlamak, aynı tür, hedef kitle, kuruluş, amaç ve dildeki güncel metni yürürlükten kaldırır. Önceki sürümü kabul etmiş kullanıcılardan yeniden onay istenir."

Herkese açık **Gizlilik ve rıza** sayfası (`/privacy`), arıcılar ve platform kullanıcıları için geçerli aydınlatma metinlerini gösterir.

<!-- SHOT: admin-notices -->
Figure: **Aydınlatma ve rıza metinleri**: sürümler, diller ve onaylar.

## SMS {#admin-sms}

Platform kayıt kodları, giriş bilgileri, teslim onayları, aydınlatma metni onayları, şifre sıfırlamaları, telefon doğrulamaları ve bilgilendirme mesajları için SMS gönderir. Mesajları hangi sağlayıcının göndereceği bir sunucu ayarıdır (bkz. [Sunucu yapılandırmasındaki ayarlar](#admin-settings)). Platform her mesajın kaydını maskelenmiş numarayla tutar; kodlar ve şifreler hiçbir zaman açık olarak saklanmaz. Günlük bir sınır SMS bütçesini korur; sınıra ulaşıldıktan sonra da şifre sıfırlamaları ve teslim onayları gönderilmeye devam eder.

**Test sağlayıcısı** ile (yalnızca geliştirme ve tanıtımlar için) hiçbir şey gönderilmez: bunun yerine **SMS giden kutusu (test sağlayıcısı)** her mesajı metni ve koduyla birlikte, "Bugünkü bölüm / günlük sınır" bilgisiyle gösterir.

<!-- SHOT: admin-sms-outbox -->
Figure: Test sağlayıcısının SMS giden kutusu.

## Sunucu yapılandırmasındaki ayarlar {#admin-settings}

Bazı anahtarlar panelde değil, sunucu yapılandırmasında ayarlanır ve bunları değiştirmek sunucunun işletmecisini gerektirir. Her biri, gerektirdiği şey hazır olduğunda ayrı ayrı açılır:

Table: Sunucu yapılandırmasındaki anahtarlar {#tbl-settings}

| Ayar | Kapalı (bugün) | Açık | Ne zaman açılır |
|---|---|---|---|
| SMS sağlayıcısı | Mesajlar yalnızca kayda yazılır | Mesajları sözleşmeli sağlayıcı gönderir | SMS paketi satın alındığında ve gönderici adı kaydedildiğinde |
| Kaplarla teslim | Tesisler bir teslimi tek bir imzalı form ve tek bir numuneyle kaydeder (eski kayıt) | [Bölüm 5](#sao-intake)'teki kap prosedürü | Tesislerde güvenlik kapaklı numune kavanozları, mühürler ve etiketler hazır olduğunda |
| Aydınlatma metni onayı zorunlu | Bir şerit kullanıcılardan metni onaylamalarını ister | Metin onaylanana kadar panel kapalı kalır | Aydınlatma metinleri KVKK birimince onaylandığında |
| Arıcı portalı | Arıcılara giriş bilgisi verilmez | Kayıt görevlileri giriş bilgilerini gönderir; [bölüm 14](#portal)'teki portal açılır | Proje portal için onay verdiğinde |
| Eski arıcıların herkese açık adları | – | Bal Hikâyesi, bir ad rızası olana kadar nötr kodu gösterir (varsayılan) | – (alternatif, bir arıcı yeniden kaydedilene kadar adları korur) |
| Veri saklama çalıştırması | **Veri saklama** sayfası yalnızca sayar | Çalıştırma yapılabilir (aşağıya bakın) | Sahip ilk çalıştırmayı ve takma ad biçimini onayladığında |

## Veri saklama {#admin-retention}

**Veri saklama**, platformun her veri türünü ne kadar sakladığını ve sonrasında ne olduğunu gösterir ([Tablo](#tbl-retention)). Sayfa bir **deneme çalıştırmasıdır**: "süresi dolan satırları sayar, hiçbir şeyi değiştirmez". **Deneme raporu (CSV)** sayıları indirir.

Table: Saklama süreleri {#tbl-retention}

| Veri | Saklama süresi | Sonra |
|---|---|---|
| Tek kullanımlık kodlar | 30 gün | silinir (yalnızca özetleri saklanmıştı); bir kaydı kanıtlayan kod o kayıtla birlikte kalır |
| SMS mesajları | 1 yıl | numara ve metin kaldırılır, sayılar korunur |
| Fiyatlar | 5 yıl | silinir |
| Lotlar, teslim almalar, kaplar, numuneler, laboratuvar raporları, partiler | bir partinin en geç son tüketim tarihinden (parti tarihi + 2 yıl) ya da teslimden 5 yıl sonra | arıcı bağlantısı takma adlandırılır; ürün verileri korunur |
| Arıcı kimliği (pasif arıcılar) | etkin olduğu sürece, ardından lotlarının süresi | takma adlandırılır |
| Kanıt fotoğrafları (imzalı teslim formları) | kanıtladıkları kayıt kadar | silinir, özet korunur |
| Rızalar ve aydınlatma metni onayları | ilgili veriler kadar + 3 yıl (kanıt) | silinir |
| Eski üyelerin kullanıcı hesapları | son üyeliğin bitiminden 2 yıl sonra | anonimleştirilir; denetim kaydı girişlerinde yalnızca kullanıcı kimliği kalır |
| Denetim kaydı | 10 yıl | değiştirilmez (yalnızca ekleme yapılabilir) |

Saklama çalıştırması **kapalıdır** ("Kapalı: ilk çalıştırma sahibin kararıdır"). Sahip onayladığında ve platformun işletmecisi açtığında **Saklamayı şimdi çalıştır** düğmesi görünür; bu düğme "Bu çalıştırmayı sahip onayladı (verileri siler ve temizler, geri alınamaz)." onayını gerektirir. Çalıştırma eski tek kullanımlık kodları siler, eski SMS mesajlarının numarasını ve metnini temizler ve eski fiyatları siler; çalıştırma denetim kaydına yazılır. Diğer kurallar, takma adların biçimine karar verilene kadar yalnızca sayılır ("yalnızca sayar (karar O-14)"). Aylık bir çalıştırma ancak elle yapılan ilk çalıştırmadan sonra açılabilir.

**Son tüketim tarihi.** Partilerde son tüketim tarihi tutulmaz; yerine parti tarihi artı 2 yıl kullanılır (bir ayar). Üretici, balının gerçek raf ömrünü teyit eder; süre buna göre ayarlanır.

## Denetim kaydı {#admin-audit}

Bkz. [Denetim kaydı](#trust-audit).
