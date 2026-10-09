# 12. Platform yöneticisi {#admin}

## Günlük işiniz {#admin-daily}

Platform yöneticileri platformu işletir. Kurumları doğrular, üyeleri ve yetkileri yönetir, referans verilerini ve kontrol noktası tanımlarını güncel tutar, arıcıları kaydeder, (şimdilik) parti oluşturur, düzeltmelere karar verir ve denetim kaydını izler.

Yöneticiler her kaydı okuyabilir, ancak **iş verisi girmez ve onaylamaz**. Oturum açtıklarında **İşlerim** sayfası açılır; bu sayfada **Doğrulanacak kurumlar**, **Doğrulanacak kayıt numaraları** ve **Karar bekleyen düzeltmeler** grupları yer alır. **Panel** genel bir bakış sunar (rakamlar, zincir, il bazında arıcı ve arılık haritası, son etkinlikler ve bekleyen onaylar).

<!-- SHOT: admin-dashboard -->

Yönetim menüsü:

| Menü | Öğeler |
|---|---|
| **Panel** | Genel bakış |
| **Arıcılar** | Arıcı listesi, arılıklar, eğitim seviyeleri, arıcılık amaçları, arıcılık türleri |
| **Ham Bal Üretimi** | Ham bal lotları, bal türleri, üretim dönemleri |
| **Toptancılar** | Toptancı listesi, toptancı ekleme |
| **Nihai Bal Üretimleri** | Partiler |
| **Kritik Kontrol Noktaları** | KKN'ler, parametre kategorileri, parametreler, referans verisi çevirileri |
| **Kullanıcı Yönetimi** | Kullanıcılar, roller, **Denetim kaydı** |
| **Kurumlar** | İşlerim, kurumlar, yetenekler, arıcı kaydı, roller ve yetkiler, KKN sorumlulukları, düzeltmeler, sevkiyatlar, numuneler, arıcı sayfaları |

## Kurumlar {#admin-organizations}

### Kurum oluşturma {#admin-org-create}

1. **Kurumlar** → **Yeni kurum**.
2. Adı, ticari unvanı, vergi numarasını, ili ve ilçeyi girin.
3. Kurum platformda zaten kayıtlı bir toptancıysa, mevcut toptancı alanında onu seçin. **İşleme tesisi kendi toptancı kaydına bağlanmalıdır**: tesisin teslim aldığı lotlar o toptancının altında tutulur ve bir parti yalnızca kendi toptancısının lotlarını kullanabilir.
4. En az bir yeteneği işaretleyin ve **Oluştur** düğmesine tıklayın.

Kurum **Beklemede** durumunda başlar: "Kurum oluşturuldu. Doğrulanana kadar Beklemede kalır."

<!-- SHOT: admin-org-create -->

### Doğrulama, belge isteme, askıya alma {#admin-org-verify}

Kurum sayfasında:

- **Doğrula** (Beklemede durumundan) için en az bir yüklenmiş belge gerekir.
- **Belge iste** (Beklemede iken) için bir gerekçe gerekir, örneğin hangi belgenin eksik olduğu.
- **Askıya al** (Doğrulandı iken) için bir gerekçe gerekir. **Yeniden etkinleştir**, askıya alınmış bir kurumu yeniden doğrular.

Her değişiklik doğrulama geçmişinde ve denetim kaydında görünür.

### Yetenekler {#admin-org-capabilities}

**Yetenek ekle** bir yetenek ekler. **Laboratuvar** için akredite kutusunu işaretleyin, akreditasyon numarasını ve geçerlilik tarihini girin; akredite bir laboratuvarın numarası olmalıdır. **Kaldır** bir yeteneği kaldırır; her kurumun en az bir yeteneği kalır. **Yetenekler** sayfası her yetenek için hangi KKN'lerin o yeteneği gerektirdiğini ve hangi kurumların o yeteneğe sahip olduğunu gösterir. **Roller ve yetkiler** altındaki **Kurum yetkinlikleri** sekmesi, tüm kurumları tek seferde düzenlemenizi sağlar.

### Üyeler {#admin-members}

1. Kurum sayfasında, üyeler bölümünde rolü seçin.
2. Mevcut bir kullanıcı adı girin ya da kullanıcıyı aynı anda oluşturmak için yeni bir kullanıcı adı, e-posta ve şifre girin.
3. **Üye ekle** düğmesine tıklayın: "…, … olarak eklendi."

Bir kullanıcının birden fazla üyeliği olabilir. **İptal et** bir üyeliği sona erdirir ve kullanıcının tüm oturumlarını kapatır. **Roller ve yetkiler** altındaki **Üyeler** sekmesi, tüm kurumların tüm üyeliklerini listeler.

<!-- SHOT: admin-org-members -->

### Dört göz muafiyeti {#admin-four-eyes}

Tek bir kalite sorumlusu olan küçük kurumlara **Dört göz muafiyeti** tanınabilir: bu durumda üyeler kendi KKN kayıtlarını ve kendi kesinleştirme taleplerini onaylayabilir. Bu tür her onay denetim kaydında işaretlenir. Muafiyet laboratuvar sonuçlarına hiçbir zaman uygulanmaz.

## Kullanıcılar {#admin-users}

**Kullanıcı Yönetimi** → **Kullanıcılar** hesapları listeler.

- **Yeni Kullanıcı Ekle** bir hesap oluşturur. Kurum modelinde kurum üyeleri için **Genel rol** alanını boş bırakın ve onlara kurum sayfasında bir üyelik verin; **Platform yöneticisi** seçeneğini yalnızca yöneticiler için seçin.
- **Parolayı Sıfırla** bir kullanıcı için yeni şifre belirler.
- **Sil** bir hesabı kaldırır (dikkatli kullanın; geri alınamaz).

Şifreler en az 6 karakter olmalıdır. 5 başarısız oturum açma denemesinden sonra hesap 15 dakika kilitlenir. Kilit açma düğmesi yoktur; kilit kendiliğinden kalkar.

## Roller ve yetkiler {#admin-permissions}

**Roller ve yetkiler** sayfasında üç sekme vardır:

- **Üyeler**: tüm etkin üyelikler; üye ekleme ve üyelik iptali.
- **Rol yetkileri**: bir rol seçin ve her işlev için **Giriş**, **Onayla** ve **Okuma** kutularını işaretleyin. İşlevler, KKN'ler (her biri ayrı bir satır) ile teslim alma, numuneler, laboratuvar sonuçları, sevkiyatlar, arıcı sayfaları, geri çağırma ve kesinleştirme alanlarıdır. Her yetki en az bir rolde kalmalıdır. Değişiklikler hemen uygulanır.
- **Kurum yetkinlikleri**: kurum başına yetenekler.

"Sabit kurallar (kapatılamaz):" kutusu, hiçbir ayarın değiştiremediği kuralları listeler (bkz. [Sabit kurallar](#fixed-rules)).

<!-- SHOT: admin-role-permissions -->

## KKN sorumlulukları {#admin-ccp-responsibilities}

**KKN sorumlulukları** sayfası her KKN için hangi kurumun sorumlu olduğunu (lotun veya partinin sahibi ya da lotlar için arıcının birliği), hangi yeteneğin gerektiğini, hangi rollerin veri girdiğini ve hangi rollerin onayladığını ve onayın gerekip gerekmediğini belirler. Örnek: "Lotun arıcısının birliği", **Birlik** ve **Saha denetçisi** seçilerek KKN 0 birliğe verilir.

## Kontrol noktaları, parametreler ve limitler {#admin-ccps}

- **Kritik Kontrol Noktaları**: kod, başlık, işleme adımı, kritiklik gerekçesi ve **Uygulandığı yer** (ham bal lotu veya nihai parti). Yanıtlar kişisel veri içerebiliyorsa, serbest metin değerlerini herkese açık Bal Hikâyesi'nde gösterme seçeneği kapalı kalmalıdır.
- **Parametreler**: ad, birim, kategori, veri türü (**Sayı**, **Evet / Hayır**, **Metin**, **Dosya**), en küçük ve en büyük değer, zorunlu veya isteğe bağlı oluşu ve "Yalnızca şu soru…" koşulu (parametre, başka bir evet/hayır sorusu belirli bir yanıt aldığında zorunlu olur).
- **Limitler**: her parametre için tüm bal türlerine uygulanan varsayılan limitler ile bal sınıfına (çiçek, salgı, çam) veya bal türüne göre özel limitler; isteğe bağlı olarak bir koşul, bir toplam veya bir oranla. Limitler değiştiğinde kayıtlı sonuçlar yeniden hesaplanmaz.

**Doğrulanmış limitler.** Her parametrede **Limitler doğrulandı (herkese açık gösterilir)** kutusu bulunur. Bal Hikâyesi'nde ve arıcı sayfasında yalnızca doğrulanmış limitler herkese açık bir limit içinde/limit dışında değerlendirmesi üretir. Doğrulanmamış limitler panelde "doğrulanacak" olarak işaretlenir; herkese açık sayfalar ise limitlerin hâlâ kontrol edildiğini belirtir.

<!-- SHOT: admin-parameter-limits -->

## Referans verileri ve çeviriler {#admin-reference}

Bal türleri (limitler için kullanılan bal sınıflarıyla birlikte), üretim dönemleri, eğitim seviyeleri, arıcılık türleri ve amaçları ve toptancılar kendi menülerinde tutulur. Kullanımda olan bir kayıt silinemez.

**Referans verisi çevirileri**, listelerde, tablolarda ve herkese açık sayfalarda gösterilen adları beş dilde tutar: bal türleri, dönemler, durumlar, yetenekler, roller, KKN'ler, parametreler, parametre kategorileri, limit notları, parametre açıklamaları ve ülkeler. Boş bırakılan bir alan için önce İngilizce metin, o da yoksa temel metin gösterilir.

## Arıcılar, arılıklar ve kayıt {#admin-registry}

- **Arıcılar**: arıcı ekleyin (ad, doğum tarihi, deneyim, telefon, eğitim, arıcılık türü ve amacı, isteğe bağlı fotoğraf). İşletme numarası otomatik üretilir. Arıcılar sisteme giriş yapmaz. Arıcılar hiçbir zaman silinmez, yalnızca pasifleştirilir.
- **Arılıklar**: başlık, kovan sayısı ve konum (enlem ve boylam). Konum hiçbir zaman herkese açık gösterilmez; herkese açık sayfalar yalnızca ili gösterir.
- **Arıcı kaydı ve birlikler**: her arıcı için birliği bağlayın ve işletme kayıt numarasını (biçim: `TR` + il kodu + 10 rakam, örneğin `TR061234567890`) bir kanıt belgesiyle birlikte girin. Ardından **Doğrula** veya **Reddet** düğmesine tıklayın (reddetmek için gerekçe zorunludur).

<!-- SHOT: admin-registry -->

## Partiler (şimdilik yalnızca yönetici) {#admin-batches}

1. **Toptancılar** → toptancı → **Nihai Bal Üretimleri (Partiler)** → **Yeni Parti**.
2. **Parti Başlığı** ve **Üretim Tarihi** alanlarını doldurun ve **Parti Oluştur** düğmesine tıklayın. QR kod üretilir.
3. **Partiyi Yönet** sayfasında uygun her lottan (bu toptancının, tüm KKN'leri onaylanmış lotları) kullanılacak miktarı girin ve **Parti Bileşimini Kaydet** düğmesine tıklayın. Kullanılabilir miktarı aşan miktarlar dikkate alınmaz.
4. Sahip kurumu (tesisi) belirlemek için partiyi **Düzenle** ile açın.
5. **KKN Yönet**: KKN 4'ten KKN 10'a kadar işaretleyin ve **Atamaları Kaydet** düğmesine tıklayın.

Ardından tesis parti KKN'lerini girer ve partiyi kesinleştirir. Yöneticiler kesinleştirme yapamaz: "Kurum modelinde partiyi sahibi olan kurum kesinleştirir (iki kişi: talep ve onay)."

<!-- SHOT: admin-manage-batch -->

## Düzeltmeler {#admin-corrections}

Onaylanmış veya kilitli veriler yalnızca bir düzeltmeyle değiştirilir:

1. Kaydın KKN sayfasını açın. **Düzeltmeler** kutusunda parametreyi seçin, yeni değeri ve bir **gerekçe** girin ve **Talep et** düğmesine tıklayın: "Düzeltme talep edildi; başka bir platform yöneticisinin onaylaması gerekir."
2. **İkinci** bir yönetici **Düzeltmeler** sayfasını açar, eski ve yeni değeri ve gerekçeyi okur, ardından **Onayla ve uygula** veya **Reddet** düğmesine tıklar (reddetmek için not zorunludur).

Düzeltmeyi talep eden kişi o düzeltmeye karar veremez. Düzeltmeler kilitli kayıtlara da uygulanır ve her iki adım da denetim kaydında yer alır.

## Arıcı sayfaları, sevkiyatlar ve numuneler {#admin-overviews}

Yöneticiler, tüm kurumların çalışmasını izlemek için **Arıcı sayfaları**, **Sevkiyatlar ve stok** ve **Numuneler ve laboratuvar** sayfalarını salt okunur olarak açabilir.

## Geri çağırma {#admin-recall}

Bir partinin veya lotun **Geri çağırma raporu** şunları listeler:

- **kaynak**: lotlar, arılıklar, arıcılar ve teslim formları;
- partiler;
- **sonrası**: sevkiyatlar ve partiyi şu anda elinde tutan kurumlar.

Üyeler, kurumlarının görebildiği kısmı görür (sahip, partiyi elinde tutan kurum veya birlik); gizlenen satırların yalnızca sayısı gösterilir. **Yazdır** ve **CSV** raporu dışa aktarır.

<!-- SHOT: recall-report -->

## Denetim kaydı {#admin-audit}

Bkz. [Denetim kaydı](#trust-audit).
