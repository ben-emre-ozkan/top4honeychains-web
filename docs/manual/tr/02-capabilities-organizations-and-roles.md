# 2. Yetenekler, kurumlar ve roller {#roles}

TOP4HoneyChains'te kimse tek başına işlem yapmaz: her işlem **bir kurum adına, bir rolde** yapılır. Neler yapabileceğinizi üç şey belirler:

1. kurumunuzun **yetenekleri**: kurumun bal zincirinde neler yapabildiği;
2. kurumdaki **rolünüz**: kurum içinde kim olduğunuz;
3. kurumunuzun **durumu**: yalnızca **Doğrulandı** durumundaki bir kurum veri girebilir. Kurumları bir platform yöneticisi doğrular (bkz. [Kurum durumu](#org-status)).

Rol bir **üyelik** ile alınır: bir kullanıcı hesabı × bir kurum × bir rol (bkz. [Birden fazla üyelik](#memberships)). Tek istisna arıcılardır: bir arıcının arıcı portalı için kişisel bir hesabı olabilir; bu hesap arıcının Arıcı kaydına bağlıdır ve hiçbir üyelik taşımaz (bkz. [bölüm 14](#portal)). Bunların yanında bazı kurallar platforma yerleşiktir ve kapatılamaz (bkz. [Temel kurallar](#fixed-rules)).

Bu bölüm iki şeyi birbirinden ayırır:

- **değer zinciri faaliyetleri**: kişilerin bilgisayar dışında balla yaptığı işler; örneğin kayıt, teslim etme, numune alma, analiz etme, işleme, depolama ve sevk etme ([Tablo](#tbl-activities));
- **veri hakları**: her rolün platformdaki kayıtlarla neler yapabileceği: oluşturma, okuma, güncelleme, silme ve onaylama ([Tablo](#tbl-crud)).

## Yetenekler {#capabilities}

Yetenek, bir kurumun sahip olduğu bir beceridir: kurumun bal zincirinde **neler yapabileceğini** belirtir. İşi kurumda hangi üyenin yapacağını roller belirler (bkz. [Roller](#role-list)). Bir kurumun bir veya daha fazla yeteneği olur; yetenekleri bir platform yöneticisi verir ve kaldırır (bkz. [Yetenekler](#admin-org-capabilities)). Yetenek listesinin kendisi sabittir. [Tablo](#tbl-capabilities), yetenekleri ekrandaki adlarıyla listeler.

Table: Yetenekler: bir kurumun neler yapabileceği {#tbl-capabilities}

| Yetenek (ekranda) | Yapabildikleri | KKN'ler (varsayılan) |
|---|---|---|
| **Birlik** | Üye arıcılarını bağlayabilir ve herkese açık arıcı sayfası için rızalarını kaydedebilir. | yok; KKN 0 yalnızca bir yönetici bu KKN'yi birliğe verirse |
| **İşleme** | Balı teslim alıp işleyebilir: teslimler, numuneler, lot KKN 0–3 ve parti KKN 4–6. | lot KKN 0–3, parti KKN 4–6 |
| **Paketleme** | Balı doldurabilir, etiketleyebilir ve paketleyebilir: parti KKN 7–9 ve tesisteki KKN 10 depolama. | parti KKN 7–9, KKN 10 |
| **Laboratuvar** | Akredite veya kurum içi bir gıda analiz laboratuvarı olarak bal numunelerini analiz edebilir ve laboratuvar raporu düzenleyebilir. Akreditasyon numarası ve geçerlilik tarihi bu yetenekte tutulur. | yok; onaylanan sonuçları lot KKN 1'i doldurur |
| **Toptan Satış** | Partileri toptan satış için elinde tutabilir, depolamalarını (KKN 10) kaydedebilir ve sevk edebilir. | KKN 10 |
| **Perakende** | Partileri tüketiciye satış için elinde tutabilir, depolamalarını (KKN 10) kaydedebilir ve sevk edebilir. | KKN 10 |
| **Arıcı kaydı** | Bölgesindeki arıcıları ve arılıklarını kaydedebilir, kimliklerini ve kayıt numaralarını doğrulayabilir, rızalarını kaydedebilir ve giriş bilgilerini verebilir. | yok |
| **Merkez Birlik** | Platform bu şekilde yapılandırılmışsa arıcı birliklerini doğrulayabilir. | yok |
| **Kamu Otoritesi** | Bakanlık veya denetim kuruluşu olarak tüm kayıtları okuyabilir; hiçbir şey girmez. | yok (tümünü okur) |
| **Arıcılık İşletmesi** | Bal üretebilir. Arıcıların üyeliği yoktur; işletmeleri bir Arıcı kaydı ile temsil edilir. | yok |

Kendi balını dolduran ve depolayan bir işleme tesisinin **İşleme**, **Paketleme** ve **Toptan Satış** (veya **Perakende**) yeteneklerine ihtiyacı vardır; çünkü partideki KKN 7–9 için Paketleme, KKN 10 için Toptan Satış, Perakende veya Paketleme gerekir. Yeteneği işlemlerin kendisi de kontrol eder: numune almak için **İşleme**; sevk etmek ve teslim almak için **İşleme**, **Paketleme**, **Toptan Satış** veya **Perakende**; kesinleştirme talep etmek ve onaylamak için **İşleme** veya **Paketleme** gerekir.

**Platform.** *Platform yöneticisi* bir kuruma verilebilen bir yetenek değildir: platformu işleten kişilere verilen, kullanıcı hesabına ait genel bir roldür. Platform yöneticileri kurumları, üyelikleri, referans verilerini ve ayarları yönetir ve her kaydı okur, ancak iş akışı verisi girmez (bkz. [bölüm 15](#admin)). Platformun kendi kurumu ayrılmış **Platform Yöneticisi** yeteneğini taşır; bu yetenek başka bir kuruma verilemez ve kaldırılamaz.

## Değer zinciri faaliyetleri {#activities}

Değer zinciri faaliyetlerini kişiler, balla ve bilgisayar dışında yapar. Platform her faaliyetin sonucunu veri olarak kaydeder. [Tablo](#tbl-activities), her yetenek için kurum çalışanlarının hangi faaliyetleri yaptığını ve platformda neyin kaydedildiğini gösterir.

Table: Değer zinciri faaliyetleri (kişiler) ve platformun kaydettikleri (bilgisayar) {#tbl-activities}

| Yetenek | Değer zinciri faaliyetleri (kişiler, bilgisayar dışında) | Platformda kaydedilenler (bilgisayar ortamında) |
|---|---|---|
| Arıcılık İşletmesi | Arıcılık yapar, bal hasat eder, balı kaplar içinde bir tesise teslim eder, SMS kodunu tesise okur, istediği fiyatı belirler | Arıcı ve arılık kayıtları; teslim onayı (SMS kodu); portalda: istenen fiyat ve arıcının kendi rızaları |
| Arıcı kaydı | Arıcıyla görüşür, kimlik belgesini ve kayıt bilgisini kontrol eder, aydınlatma metnini arıcıya okur | Arıcı ve arılık kayıtları, kayıt (SMS kodu veya imzalı fotoğraf), kayıt sırasında verilen rızalar, arıcının giriş bilgileri |
| Birlik | Üye arıcılarını bağlar ve bilgilendirir, rıza metnini onlara okur, imzalarını toplar | Rıza kayıtları; herkese açık arıcı sayfaları |
| İşleme | Teslimleri kaplar halinde teslim alır, mühürleri kontrol eder veya mühür takar, her kaptan bir numune kavanozu ve bir şahit numune kavanozu doldurur, balı ısıtır, süzer, karıştırır ve dinlendirir | Kapları ve mühür geçmişiyle teslim, numune kavanozları, KKN 0 ve KKN 2–6 verileri |
| Paketleme | Balı doldurur, etiketler ve paketler, tesiste depolar | KKN 7–10 verileri |
| Laboratuvar | Mühürlü kavanozları teslim alır ve mührü, etiketi ve kapağı kontrol eder; karışım numunesini hazırlar; balı analiz eder; raporu imzalar | Kabul kontrolleri, karışım numunesi, laboratuvar raporu (PDF) ve sonuçlar (KKN 1) |
| Toptan Satış, Perakende | Malları teslim alır, depolar, satar veya sevk eder | Sevkiyat onayı, depolama kaydı (KKN 10), yeni sevkiyatlar |
| Merkez Birlik | Arıcı birliklerini kontrol eder | Birliklerin doğrulanması (yapılandırılmışsa) |
| Kamu Otoritesi | Denetler | Kayıtları okur |

Yetenekleri ne olursa olsun her kurumun bir de **Uyum Sorumlusu** vardır: onayları, kanıtları ve kişisel verilerin korunmasını gözeten kişi (bkz. [bölüm 13](#role-co)).

## Kurum türleri {#org-types}

Platformda ayrı bir kurum türü listesi yoktur: bir kurumun türü, sahip olduğu yeteneklerin toplamıdır. [Tablo](#tbl-org-types), tipik kurumları listeler.

Table: Tipik kurumlar ve yetenekleri {#tbl-org-types}

| Kurum | Yetenekler | Üyelerin tipik rolleri |
|---|---|---|
| İlçe tarım müdürlüğü | **Arıcı kaydı** | Kurum yöneticisi, Arıcı Kayıt Görevlisi, Uyum Sorumlusu |
| Arıcı birliği | **Birlik** | Kurum yöneticisi, Saha denetçisi, Uyum Sorumlusu |
| İşleme tesisi veya paketleyici | **İşleme**, **Paketleme** ve tesiste depolama için **Toptan Satış** veya **Perakende** | Kurum yöneticisi, Personel, Numune kabul sorumlusu, Kalite Güvence Sorumlusu, Uyum Sorumlusu |
| Gıda analiz laboratuvarı (akredite veya kurum içi) | **Laboratuvar** | Kurum yöneticisi, Analist, Laboratuvar müdürü, Uyum Sorumlusu |
| Toptancı | **Toptan Satış** | Kurum yöneticisi, Personel, Kalite Güvence Sorumlusu, Uyum Sorumlusu |
| Perakendeci | **Perakende** | Kurum yöneticisi, Personel, Kalite Güvence Sorumlusu, Uyum Sorumlusu |
| Bakanlık veya denetim kuruluşu | **Kamu Otoritesi** | Kurum yöneticisi (salt okunur) |

Kendi işleme tesisini işleten bir birliğin **İşleme** (ve **Paketleme**) yeteneği de olur; bu durumda tesisin rolleri de birlikte bulunur. Küçük bir kurumda Uyum Sorumlusu, ikinci bir üyelikle kurum yöneticisi olabilir; dört göz kuralları yine geçerlidir.

### Kurum durumu {#org-status}

Kurumları bir platform yöneticisi doğrular, askıya alır ve yeniden etkinleştirir. Doğrulama için en az bir yüklenmiş belge, askıya alma için bir gerekçe gerekir. İlk kez doğrulanan bir kurumun ayrıca etkin bir **Uyum Sorumlusu** olmalıdır; daha önce Uyum Sorumlusu olmadan doğrulanmış bir kurum, bir Uyum Sorumlusu ataması gereken tarihi belirten bir uyarı görür ve engellenmez. [Tablo](#tbl-org-status), durumları listeler.

**Merkez Birlik.** Platform bu şekilde yapılandırılmışsa arıcı birliklerini doğrulanmış bir **Merkez Birlik** kurumunun kurum yöneticisi doğrular, askıya alır ve yeniden etkinleştirir; bu durumda bunu **yalnızca** o yapar: platform yöneticileri artık birlikleri doğrulamaz ve birlikler onların bekleme listesinden çıkar.

Table: Kurum durumu {#tbl-org-status}

| Durum | Anlamı |
|---|---|
| **Beklemede** | Yeni kurum, doğrulanmayı bekliyor. Üyeler okuyabilir, kurum yöneticisi belge yükleyebilir. Kimse veri giremez. |
| **Doğrulandı** | Bir platform yöneticisi (veya yapılandırılmışsa, birlikler için merkez birlik) tarafından kontrol edildi. Üyeler rollerine göre çalışabilir. |
| **Askıda** | Doğrulama (bir gerekçeyle) geri alındı. Yeniden etkinleştirilene kadar Beklemede gibi çalışır. |

## Roller {#role-list}

[Tablo](#tbl-roles), rollerin başvuru listesidir. Bu kılavuzdaki diğer tüm tablolar aynı rolleri aynı sırayla kullanır. Veri hakları tabloları ikinci sütundaki kısaltmaları kullanır.

Table: Roller {#tbl-roles}

| Rol (ekranda) | Kısa | Kod | Tipik kişi | Ana işi |
|---|---|---|---|---|
| **Kurum yöneticisi** | KY | OrgAdmin | Kurumun yöneticisi | Belge yükler, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, arıcı rızalarını kaydeder (birlikler), teslimleri, numuneleri ve kurumun sahip olduğu lotların istenen fiyatını okur |
| **Personel** | Pe | Staff | Üretim, dolum veya perakende personeli | KKN 1–10 verilerini girer, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, depolamayı kaydeder |
| **Saha denetçisi** | SD | FieldInspector | Birliğin saha teknisyeni | Kayıtları okur; KKN 0'ı yalnızca bir yönetici bu KKN'yi birliğe verirse girer |
| **Numune kabul sorumlusu** | NKS | SampleAcceptanceOfficer | Tesisteki kabul görevlisi | Teslimleri mühürlü kaplarıyla kaydeder, arıcının SMS kodunu gönderir ve girer, her kaptan numune alır, KKN 0'ı girer |
| **Kalite Güvence Sorumlusu** | KGS | QualityApprover | Kalite yöneticisi, gıda mühendisi | KKN kayıtlarını ve depolama kayıtlarını onaylar, kesinleştirmeyi onaylar |
| **Analist** | An | Analyst | Laboratuvar analisti | Numuneleri teslim alır (mühür, etiket ve kapak kontrolü), laboratuvar sonuçlarını girer, raporları onaya gönderir (laboratuvardaki birinci kişi) |
| **Laboratuvar müdürü** | LM | LabManager | Laboratuvar müdürü | Numuneleri teslim alır (mühür, etiket ve kapak kontrolü), laboratuvar raporlarını onaylar veya geri gönderir (laboratuvardaki ikinci kişi) |
| **Arıcı Kayıt Görevlisi** | AKG | RegistrationOfficer | İlçe tarım müdürlüğü görevlisi | Arıcıları ve arılıklarını kaydeder, kayıt numarasını kontrol eder, arıcının SMS kodunu gönderir, kimliği tasdik eder, aydınlatma metnini ve rızaları kaydeder, arıcının giriş bilgilerini verir |
| **Uyum Sorumlusu** | US | ComplianceOfficer | Herhangi bir kurumun kalite veya kişisel verilerin korunması sorumlusu | İmzalı fotoğraf kanıtlarını, numune alma kalitesini ve onayları inceler, veri sorumlusu bilgilerini günceller, ilgili kişi başvurularını ve rıza geri çekmelerini ele alır, kurumun denetim kaydı girişlerini okur |
| **Platform yöneticisi** | PY | (genel rol) | Platformu işleten kişi | Yönetim ([bölüm 15](#admin)); tüm iş akışı verilerini okur, ancak hiçbirini girmez veya onaylamaz; arıcı da kaydedebilir |
| **Arıcı** (portal hesabı) | Ar | (üyelik yok) | Arıcının kendisi | Kendi arılıklarını, teslimlerini, numunelerini, laboratuvar sonuçlarını, partilerini ve Bal Hikâyesi sayfalarını okur; balının istenen fiyatını girer; kendi rızalarını verir ve geri çeker ([bölüm 14](#portal)) |

Platform yöneticisi, iş kurumlarının iş akışında yer almaz: her kaydı okuyabilir, ancak **iş akışı verisi giremez ve onaylayamaz** (teslimler, KKN verileri, laboratuvar sonuçları, sevkiyatlar, rızalar). Onaydan sonra bir verinin hatalı olduğu anlaşılırsa yönetici bir *düzeltme* talep eder; düzeltmeyi ikinci bir yönetici onaylar (bkz. [Düzeltmeler](#admin-corrections)). Ana verileri ise yöneticiler tutar: arıcılar, arılıklar, lotlar, partiler ve partilerin bileşimi (bkz. [bölüm 15](#admin)).

## Veri hakları (CRUD) {#crud}

[Tablo](#tbl-crud), her rolün her kayıt türü üzerindeki varsayılan veri haklarını CRUD modeliyle gösterir: **C** oluşturma (create), **R** okuma (read), **U** güncelleme (update), **D** silme (delete) ve ayrı bir hak olarak **A** onay (approve). "–" hak olmadığını gösterir. Ar arıcının portal hesabı, Ziy hesabı olmayan herkese açık sayfa ziyaretçisidir. Bir üyenin hakları yalnızca kendi kurumunun işine ait kayıtlar için geçerlidir (kurumun kendi lotları ve partileri, elinde tuttuğu partiler, birliğinin arıcıları, müdürlüğünün kayıtları) ve okuma dışında yalnızca kurum doğrulanmışken geçerlidir. Haklar koddan ve varsayılan ayarlardan gelir; bunlardan hangilerini bir yöneticinin değiştirebileceğini [Tablo](#tbl-admin-changeable) listeler.

Table: Rollere göre varsayılan veri hakları (CRUD + Onay) {#tbl-crud}

| Kayıt | KY | Pe | SD | NKS | KGS | An | LM | AKG | US | PY | Ar | Ziy |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Arıcı | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (kendisi) | R ³ |
| Arılık | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ¹⁸ | R ¹ | C R U ² | R (kendi) | R ³ |
| Arıcı kaydı | – | – | – | – | – | – | – | C R ¹⁸ | A ¹⁹ | C R | – | – |
| Giriş bilgileri | – | – | – | – | – | – | – | C ²⁰ | – | C ²⁰ | – | – |
| Ham bal lotu | R | R | R | C R ⁴ | R | – | – | – | R | C R U D ⁵ | R (kendi) | R ³ |
| Teslim onayı (SMS kodu veya imzalı fotoğraf) | R | – | – | C R | – | – | – | – | R A ¹⁹ | R | A ²¹ | – |
| Kap ve mühürleri | R | R ²⁹ | R ²⁹ | C R | R ²⁹ | R | R | – | R | R | R (kendi) | – |
| Numune | R | – | – | C R | – | R A ⁶ | R A ⁶ | – | R | R | R (kendi) | – |
| Laboratuvar raporu ve sonuçları | R | R | R | R | R | C R U D ⁷ | R A ⁷ | – | R | R | R (onaylı, kendi) | R ⁸ |
| KKN kaydı (veriler, "Uygulanamaz") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | – | R | R ¹⁰ | – | R ³ |
| Parti | R, kesinleştirme talebi | R, kesinleştirme talebi | R | R | R, kesinleştirmeyi onaylama (A) | R | R | – | R | C R U D ¹¹ | R ²² | R ³ |
| Sevkiyat | C R A ¹² | C R A ¹² | R ²⁹ | R ²⁹ | R | R ²⁹ | R ²⁹ | – | R | R | – | – |
| Alıcıdaki depolama kaydı (KKN 10) | R | C R U D | R | R | R A | R | R | – | R | R | – | – |
| Bir lotun istenen fiyatı | R ²³ | – | – | – | – | – | – | – | R ²³ | R | C R ²³ | – ²⁴ |
| Arıcı rızası | C R U ¹³ | R | R | R | R | R | R | C R ²⁵ | R U ²⁵ | R | R U ²⁵ | R ¹⁴ |
| Aydınlatma metni (metin, sürüm) | R | R | R | R | R | R | R | R | R | C R ²⁶ | R | R |
| Aydınlatma metninin onayı | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R (kendi) | R | R (kendi) | – |
| Veri sorumlusu bilgileri | R | R | R | R | R | R | R | R | C R U | C R U | R | R (metinde) |
| Numune alma kalitesi incelemesi | – | – | – | – | – | – | – | – | C R ³⁰ | – | – | – |
| İlgili kişi başvurusu | – | – | – | – | – | – | – | – | C R U ³⁰ | – | – | – |
| SMS mesajı | – | – | – | – | – | – | – | – | – | R ²⁷ | – | – |
| Kurum (bilgiler, belgeler) | R; belgeler C | R | R | R | R | R | R | R | R; belgeler C | C R U, durum A ¹⁵ | – | – |
| Üyelik | R | R | R | R | R | R | R | R | R | C R U ¹⁶ | – | – |
| Denetim kaydı girişi | – | – | – | – | – | – | – | – | R ²⁸ | R ¹⁷ | – | – |

Notlar:

1. Yalnızca kurumun üzerinde çalıştığı kayıtlar aracılığıyla okunur (KKN sayfaları, geri çağırma raporu). Partiyi sonradan elinde tutan kurum verileri arıcı ve arılık adları olmadan okur. Birlik üyeleri, birliğin üye arıcılarının listesini okur.
2. Arıcılar ve arılıklar hiçbir zaman silinmez, yalnızca pasifleştirilir. Arılık fotoğrafları eklenebilir, değiştirilebilir ve silinebilir.
3. Kesinleşmiş partilerin herkese açık sayfalarında (Bal Hikâyesi) ve yayımlanmış arıcı sayfalarında: ad yalnızca rızayla (aksi halde nötr bir kod), il ve onaylı veriler; hiçbir zaman koordinatlar veya iletişim bilgileri değil (bkz. [Herkesin okuyabildiği bilgiler](#trust-public)).
4. Lot, **İşleme** yeteneğine sahip doğrulanmış bir kurumda bir teslim kaydedilerek oluşturulur.
5. Yöneticiler lotun ana verilerini tutar (örneğin sahip kurum ve KKN ataması). Kaplarla kaydedilmiş bir lotun teslim bilgileri (tarih, kaplar, ağırlıklar, mühür numaraları) yöneticiler dahil kimse tarafından düzenlenemez; yalnızca iki yöneticinin yaptığı bir teslim düzeltmesiyle değiştirilir (bkz. [Düzeltmeler](#admin-corrections)). Lot kesinleşmiş bir partide yer alıyorsa hiçbir şey değiştirilemez; teslimleri, numuneleri veya kullanımda KKN verileri olan bir lot silinemez.
6. **A** = numuneyi mührü, etiketi ve kapağı kontrol ettikten sonra teslim alma veya reddetme.
7. Analist, rapor taslak durumundayken raporu oluşturur, kaydeder, değiştirir ve onaya gönderir; laboratuvar müdürü onaylar veya geri gönderir. Onaylanmış bir rapor değiştirilemez.
8. Rapor koduyla **Laboratuvar raporu doğrula** sayfası; yalnızca onaylanmış raporlar.
9. Kayıt onaylanana veya kilitlenene kadar veri girme (C), veriyi değiştirme veya temizleme (U, D) ve "Uygulanamaz" olarak işaretleme. Varsayılan veri giren roller: KKN 0–1 için Numune kabul sorumlusu, KKN 1–10 için Personel. Saha denetçisi KKN 0'ı yalnızca bir yönetici KKN 0'ı birliğe verirse girer.
10. Yöneticiler onaylanmış veya kilitli verileri yalnızca bir yöneticinin talep ettiği ve ikinci bir yöneticinin onayladığı bir düzeltmeyle değiştirir.
11. Parti oluşturma, bileşimini lotlardan belirleme, KKN'lerini atama ve sahibini belirleme. Bir parti yalnızca kesinleşmeden önce silinebilir. Yöneticiler kesinleştirme yapamaz.
12. **C** = kurumunuzun elinde tuttuğu kesinleşmiş bir partiyi sevk etme; **A** = kurumunuza gelen bir sevkiyatı onaylama veya reddetme.
13. Herkese açık arıcı sayfası için rızayı kaydetme (C) ve geri çekme (U); yalnızca birlikler.
14. Yayımlanmış arıcı sayfası.
15. Doğrulama, belge isteme, askıya alma ve yeniden etkinleştirme (bkz. [Kurum durumu](#org-status)). Kurumlar hiçbir zaman silinmez.
16. Üyelik ekleme ve iptal etme (U); üyelikler hiçbir zaman silinmez.
17. Denetim kaydına yalnızca ekleme yapılabilir; hiç kimse bir girişi değiştiremez veya silemez.
18. Bir kayıt görevlisi arıcıları ve arılıklarını kendi müdürlüğü adına kaydeder ve yalnızca o müdürlüğün kayıtlarını okur. Tasdik edilmiş bilgiler (kimlik, kayıt numarası, telefon) yalnızca aynı arıcının yeniden onaylanan yeni bir kaydıyla (yeniden doğrulama) değişir.
19. **A** = SMS kodu yerine imzalı fotoğrafla kaydedilmiş bir kanıtı (kayıt fotoğrafı veya teslim fotoğrafı) kabul etme veya reddetme. Kararı, kaydı yapan kişi değil, kaydı yapan kurumun Uyum Sorumlusu verir. Platformun kendi kayıtları için bir platform yöneticisi karar verir.
20. Arıcının giriş bilgilerini (arıcı portalı açıksa) kayıt görevlisi SMS ile gönderir; üyelerin giriş bilgilerini platform yöneticileri gönderir. Gönderildikten sonra bir şifreyi kimse göremez.
21. Arıcının **A**'sı, numune kabul sorumlusuna okuduğu SMS kodudur; teslimi kapları, ağırlıkları ve mühürleriyle birlikte onaylar.
22. Arıcının kendi balını içeren partiler: ad, arıcının payı, durum ve Bal Hikâyesi bağlantısı. Hiçbir zaman diğer arıcıların adları veya fiyatları değil.
23. Arıcı teslim ettiği her lot için bir istenen fiyat girer ve lot kesinleşmiş bir partiye girene kadar bunu değiştirebilir (yeni bir giriş olarak; geçmiş kalır). Lotun sahibi kurumun KY'si ve US'si ile platform yöneticileri fiyatı okur; birlik ise yalnızca arıcı buna rıza verdiyse. Personel, laboratuvarlar ve partiyi sonradan elinde tutanlar hiçbir zaman okumaz.
24. Herkese açık sayfalar ve geri çağırma raporu fiyat göstermez.
25. Kayıt görevlisi kayıt sırasında verilen rızaları kaydeder; arıcı isteğe bağlı bir rızayı portalda verir veya geri çeker; Uyum Sorumlusu kâğıtla veya telefonla gelen bir geri çekmeyi kaydeder. Geri çekme hemen geçerli olur.
26. Aydınlatma metinleri sürümlenir ve hiçbir zaman düzenlenmez: yeni bir sürüm yayımlamak eskisini yürürlükten kaldırır.
27. Yalnızca test sağlayıcısının giden kutusu (geliştirme ve tanıtımlar); gerçek bir sağlayıcı kod veya şifre saklamaz.
28. Uyum Sorumlusunun kendi kurumunun denetim kaydı girişleri.
29. Yalnızca geri çağırma raporunda: **Sevkiyatlar ve stok** sayfası sevkiyat okuma hakkı gerektirir.
30. Uyum Sorumlusunun kendi bölümü, **Uyum** (bkz. [bölüm 13](#role-co)).

**Kanıt dosyalarını kimler indirir?** İmzalı teslim formları, kayıt fotoğrafları, rıza belgeleri ve kurum belgeleri yalnızca bunları kaydeden rol (numune kabul sorumlusu, kayıt görevlisi veya rızalar için birliğin kurum yöneticisi), kurumun Uyum Sorumlusu ve kurum yöneticisi ve platform yöneticileri tarafından indirilir. Kurumun başka bir rolü "Bu sayfayı açamazsınız" (403), başka bir kurum "Bu sayfa bulunamadı" (404) sayfasını alır. Her indirme denetim kaydına yazılır.

**Neler silinebilir?** Üyeler yalnızca kendi verilerini ve yalnızca onaylanmadan önce temizler (KKN verileri, taslak laboratuvar sonuçları, depolama verileri). Yöneticiler kesinleşmeden önce lotları ve partileri, arılık fotoğraflarını, referans verilerini ve kullanıcı hesaplarını (kendi hesapları hariç) silebilir. Arıcılar, arılıklar, kurumlar, belgeler, üyelikler, rızalar, kayıtlar, kaplar, mühür olayları, numuneler, laboratuvar raporları, sevkiyatlar, fiyat girişleri, SMS mesajları, aydınlatma metinleri, onaylar ve denetim kaydı girişleri hiçbir zaman silinmez.

## Yetki matrisi {#permission-matrix}

[Tablo](#tbl-permissions), bugün yapılandırıldığı haliyle varsayılan yetkileri **platformdaki işlemlere** (örneğin "teslim kaydetme" gibi bir işlev) göre gösterir. Veri hakları tablosunu ([Tablo](#tbl-crud)) tamamlar ve aynı rolleri aynı sırayla kullanır. Açıklama: **R** okuma, **C** oluşturma veya veri girişi, **A** onay (veya teslim alma ya da kanıtı kabul etme), **F** kesinleştirme (talep veya onay), **–** yetki yok. "Gereken yetenek" sütunu, kurumun hangi yeteneğe sahip olması gerektiğini belirtir.

Table: Platformdaki işlemlere ve rollere göre varsayılan yetkiler {#tbl-permissions}

| Platformdaki işlem | Gereken yetenek | KY | Pe | SD | NKS | KGS | An | LM | AKG | US | PY |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Arıcı ve arılık kaydetme | Arıcı kaydı | – | – | – | – | – | – | – | C R | – | C R |
| Arıcının giriş bilgilerini SMS ile gönderme | Arıcı kaydı | – | – | – | – | – | – | – | C | – | C |
| Teslimi kaplarıyla kaydetme, SMS kodunu gönderme | İşleme | R | – | – | C R | – | – | – | – | R | R |
| Numune alma (her kaptan bir tane) | İşleme (lot sahibi) | R | – | – | C R | – | R | R | – | R | R |
| İmzalı fotoğrafı kabul etme veya reddetme (kayıt, teslim) | kendi kurumu | – | – | – | – | – | – | – | – | A | A ¹ |
| Numune teslim alma (mühür, etiket, kapak) | Laboratuvar | – | – | – | – | – | A | A | – | R | R |
| Laboratuvar sonuçlarını girme, raporu onaya gönderme | Laboratuvar | R | R | R | R | R | C R | R | – | R | R |
| Laboratuvar raporunu onaylama veya geri gönderme | Laboratuvar | R | R | R | R | R | R | A R | – | R | R |
| KKN 0 kabul kontrolü (lot) | İşleme (lot sahibi) | R | R | R | C R | A R | R | R | – | R | R |
| KKN 1 laboratuvar verileri (lot) | onaylı laboratuvar raporundan yazılır | R | R | R | R | R | R | R | – | R | R |
| KKN 2–3 (lot) | İşleme (lot sahibi) | R | C R | R | R | A R | R | R | – | R | R |
| Parti oluşturma, bileşimini belirleme, KKN atama | – | – | – | – | – | – | – | – | – | – | C |
| KKN 4–6 (parti) | İşleme (parti sahibi) | R | C R | R | R | A R | R | R | – | R | R |
| KKN 7–9 (parti) | Paketleme (parti sahibi) | R | C R | R | R | A R | R | R | – | R | R |
| Tesiste KKN 10 depolama (parti) | Toptan Satış, Perakende veya Paketleme (parti sahibi) | R | C R | R | R | A R | R | R | – | R | R |
| Kesinleştirme talep etme | İşleme veya Paketleme (sahip), doğrulanmış | F | F | – | – | – | – | – | – | – | – |
| Kesinleştirmeyi onaylama (mühürleme) | İşleme veya Paketleme (sahip), doğrulanmış | – | – | – | – | F | – | – | – | – | – |
| Sevkiyat oluşturma | İşleme, Paketleme, Toptan Satış veya Perakende (partiyi elinde tutan), doğrulanmış | C R | C R | – | – | R | – | – | – | R | R |
| Gelen sevkiyatı onaylama veya reddetme | İşleme, Paketleme, Toptan Satış veya Perakende (alıcı), doğrulanmış | A R | A R | – | – | R | – | – | – | R | R |
| Alıcıda KKN 10 depolama kaydı | Toptan Satış, Perakende veya Paketleme | R | C R | R | R | A R | R | R | – | R | R |
| Geri çağırma raporu (kendi kapsamı) | – | R | R | R | R | R | R | R | – | R | R (tümü) |
| Sahip olunan lotların istenen fiyatını okuma | İşleme (lot sahibi) | R | – | – | – | – | – | – | – | R | R |
| Arıcı rızasını kaydetme veya geri çekme (arıcı sayfası) | Birlik | C R | – | – | – | – | – | – | – | – | R |
| Uyum: incelemeler, ilgili kişi başvuruları, onay kontrolleri | kendi kurumu | – | – | – | – | – | – | – | – | C R | – |
| Veri sorumlusu bilgilerini güncel tutma | kendi kurumu | R | – | – | – | – | – | – | – | C R | C R |
| Kurumun denetim kaydı girişlerini okuma | kendi kurumu | – | – | – | – | – | – | – | – | R | R (tümü) |
| Kurum belgelerini yükleme | kendi kurumu | C | – | – | – | – | – | – | – | C | C |
| Aydınlatma metinlerini yayımlama | – | – | – | – | – | – | – | – | – | – | C |
| Kurumları doğrulama, üyelikleri ve yetenekleri yönetme | – | – | – | – | – | – | – | – | – | – | C A ² |
| Arıcı kayıt numaralarını doğrulama | – | – | – | – | – | – | – | – | – | – | A |
| Yetkileri, KKN'leri, limitleri, çevirileri değiştirme | – | – | – | – | – | – | – | – | – | – | C |
| Düzeltme talep etme veya karara bağlama (KKN verileri, teslim bilgileri) | – | – | – | – | – | – | – | – | – | – | C A (iki farklı yönetici) |
| Denetim kaydı, veri saklama raporu | – | – | – | – | – | – | – | – | – | – | R |

1. Platform yöneticilerinin yaptığı kayıtlar için.
2. Platform bu şekilde yapılandırılmışsa birlikleri bunun yerine merkez birlik doğrular.

Notlar:

- **KKN'leri okuma (R)**: her rol, kurumunun yer aldığı lot ve partilerin KKN'lerini açabilir: sahip veya sorumlu kurum olarak, numuneyi alan kurum olarak (lotlar için), partiyi sonradan elinde tutan kurum olarak ya da doğrulanmış bir kamu otoritesi olarak. Partiyi elinde tutan kurum, partinin ve lotlarının KKN'lerini **salt okunur** olarak okur. Diğer kurumlar "Bu sayfayı açamazsınız" (403) sayfasını alır.
- **KKN 1** onaylı laboratuvar raporundan doldurulur. KKN 1 için yapılandırılmış veri giren roller (Numune kabul sorumlusu, Personel) yalnızca laboratuvar raporunun kapsamadığı KKN 1 parametreleri için kullanılır.
- **Sayfalar yalnızca menüyü değil, okuma hakkını da kontrol eder.** Bir sayfanın okuma hakkı olmayan bir rol (örneğin **Sevkiyatlar ve stok** veya **Arıcı sayfaları**), sayfayı açabilecek rolleri adlandıran bir mesajla **İşlerim** sayfasına geri gönderilir.
- Yönetici **KKN 0**'ı tesis yerine arıcının birliğine verebilir (**KKN sorumlulukları** → KKN 0 → **Lotun arıcısının birliği**, yetenek Birlik, rol Saha denetçisi). Bu varsayılan ayar değildir.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Roller ve yetkiler**: Personel rolünün varsayılan yetkileri.

### Bir yöneticinin değiştirebilecekleri {#admin-changeable}

[Tablo](#tbl-admin-changeable), bir platform yöneticisinin panelde değiştirebileceği tüm ayarları listeler. Geri kalan her şey sabittir: roller listesi, yetenekler listesi ve [Temel kurallar](#fixed-rules). Bazı anahtarlar ise sunucu yapılandırmasında ayarlanır (bkz. [Sunucu yapılandırmasındaki ayarlar](#admin-settings)).

Table: Bir platform yöneticisinin değiştirebilecekleri {#tbl-admin-changeable}

| Ayar | Nerede | Tam olarak neler değiştirilebilir |
|---|---|---|
| Rol yetkileri | **Roller ve yetkiler** → **Rol yetkileri** | Rol başına: her KKN için **Giriş**, **Onayla** ve **Okuma**; teslimler (teslim alma) için **Giriş** ve **Okuma**; numuneler için **Giriş**, **Onayla** (teslim alma) ve **Okuma**; laboratuvar sonuçları için **Giriş**, **Onayla** ve **Okuma**; sevkiyatlar için **Giriş** (sevk etme), **Onayla** (teslimi onaylama) ve **Okuma**; arıcı sayfaları için **Giriş** ve **Okuma**; geri çağırma raporu için **Okuma**; kesinleştirme için **Giriş** (talep) ve **Onayla** (onaylama); arıcı kaydı için **Giriş** ve **Okuma**; kanıtlar için **Onayla**; fiyatlar ve kurumun denetim kaydı girişleri için **Okuma**. Her yetkide en az bir rol kalır. |
| KKN sorumlulukları | **KKN sorumlulukları** | KKN başına: sorumlu kurum (lotun veya partinin sahibi ya da lotlar için arıcının birliği), gereken yetenekler, veri giren roller, onaylayan roller ve onayın gerekip gerekmediği. |
| Yetenekler | Kurum sayfası; **Kurum yetkinlikleri** | Bir kurumun yeteneklerini ekleme veya kaldırma (en az biri kalır); laboratuvar akreditasyon numarası ve geçerlilik tarihi. |
| Dört göz muafiyeti | Kurum sayfası | Kurum başına: üyelerin kendi KKN kayıtlarını ve depolama kayıtlarını onaylamasına ve kendi kesinleştirme taleplerini onaylamasına izin verir; denetim kaydında her zaman işaretlenir. Laboratuvar sonuçları ve kanıtlar için hiçbir zaman geçerli değildir. |
| Kurum durumu | Kurum sayfası | Doğrulama, belge isteme, askıya alma, yeniden etkinleştirme. |
| Veri sorumlusu bilgileri | Kurum sayfası | Kurumun veri sorumlusu olarak tüzel kişi adı, iletişim bilgisi ve adresi (kurumun Uyum Sorumlusu da günceller). |
| Üyelikler ve kullanıcılar | Kurum sayfası; **Üyeler**; **Kullanıcı Yönetimi** | Üyelik ekleme ve iptal etme; giriş bilgileri SMS ile veya bir ilk şifreyle gönderilen kullanıcılar oluşturma; yeni giriş bilgileri gönderme; kilit açma ve hesap silme. |
| Aydınlatma ve rıza metinleri | **Aydınlatma ve rıza metinleri** | Bir aydınlatma veya rıza metninin yeni bir sürümünü yayımlama; kişisel verilerin korunması birimi onaylayana kadar taslak işaretiyle. |
| Kontrol noktaları, parametreler, limitler | **Kritik Kontrol Noktaları** | Parametreler, veri türleri, zorunlu sorular, limitler, "Limitler doğrulandı". |
| Ana veriler ve referans verileri | Yönetim menüleri | Arıcılar, arılıklar, kayıt numaraları, lotlar, partiler, bal türleri, dönemler, çeviriler. |

## Temel kurallar {#fixed-rules}

Bu kurallar platforma yerleşiktir; hiçbir ayar bunları kapatamaz:

- **Yalnızca doğrulanmış kurumlar veri girer.** İlk kez doğrulanan bir kurumun bir Uyum Sorumlusu olmalıdır.
- **KKN kayıtlarında dört göz**: kimse veri kaydettiği veya "Uygulanamaz" olarak işaretlediği bir KKN kaydını onaylayamaz. İstisna: bir platform yöneticisi bir kuruma **Dört göz muafiyeti** tanıyabilir; bu durumda üyeler kendi KKN kayıtlarını onaylayabilir ve bu tür her onay denetim kaydında işaretlenir.
- **Laboratuvar sonuçlarında dört göz, her zaman**: sonuçları giren kişi bu sonuçları onaylayamaz. Muafiyet yoktur.
- **Kanıtlarda dört göz, her zaman**: SMS kodunun yerine geçen imzalı bir fotoğrafı (kayıt veya teslim) kaydeden kişi değil, kurumun Uyum Sorumlusu kabul eder veya reddeder.
- **Kesinleştirmede ve depolama kayıtlarında dört göz**: kesinleştirmeyi talep eden kişi talebi onaylayamaz, bir depolama kaydını giren kişi de o kaydı onaylayamaz (kurumun muafiyeti yoksa).
- **Bir kod tam olarak gönderildiği içeriği onaylar**: arıcının SMS kodu kaydı veya teslimi (kaplar, ağırlıklar, bal türü, numuneler, her mühür numarası) bağlar; kod gönderildikten sonraki her değişiklik kodu geçersiz kılar.
- **Teslim bilgileri değişmez**: kaplara ve mühür olaylarına yalnızca ekleme yapılabilir; onaylanmış bir teslimin bilgileri yalnızca iki yöneticinin yaptığı bir düzeltmeyle değişir ve arıcıya SMS ile bilgi verilir.
- **Her kaptan bir numune**: her kap, kendi mühürlü numune kavanozunu ve güvenlik kapaklı şahit numunesini alır; laboratuvar mührü, etiketi ve kapağı kontrol eder ve bir kontrol başarısız olursa kavanozu reddeder.
- **Her değişiklik onayı kaldırır**: verinin değiştirilmesi mevcut bir onayı siler; limit dışındaki veriler onaylanırken açıkça teyit edilmelidir.
- **Laboratuvar sonuçları numuneye bağlıdır**: sonuçlar yalnızca kontrolleri sorunsuz geçmiş, teslim alınmış bir numune için ve yalnızca lotun KKN 0'ı onaylandıktan sonra girilir; onaylanmış bir rapor değiştirilemez.
- **Platform yöneticileri iş akışı verilerinde salt okunurdur** ve bu verileri yalnızca iki yöneticili düzeltme süreciyle düzeltir.
- **Kesinleşen kilitlenir**: bir parti ancak partinin ve lotlarının her KKN'si tamamlandığında kesinleştirilir; kesinleşmiş bir parti ve tüm lotları, düzeltme dışında artık değiştirilemez. Kesinleştirmeyi geri almak mümkün değildir.
- **Sevkiyatlar**: yalnızca kesinleşmiş partiler, yalnızca doğrulanmış başka bir kuruma, en çok eldeki miktar kadar.
- **Kurumları yalnızca yöneticiler yapılandırır**: yetenekler, üyelikler ve muafiyet.
- **Her yetkide en az bir rol kalır.**
- **Denetim kaydına yalnızca ekleme yapılabilir**; kayıttaki hiçbir şey değiştirilmez veya silinmez, yetki sayfalarındaki her değişiklik de ayrıca kaydedilir.
- **Fiyatlar arıcınındır**: istenen fiyatı yalnızca arıcı girer; fiyat hiçbir zaman herkese açık sayfalarda gösterilmez.
- **Bir arıcı hesabı yalnızca kendi arıcısının verilerini görür** ve yalnızca arıcı portalını açabilir.
- **Herkese açık adlar rıza gerektirir**: herkese açık sayfalar bir arıcıyı yalnızca etkin bir ad rızası varsa adıyla anar; aksi halde nötr bir kod gösterir.
- **Arıcı sayfaları** yalnızca etkin bir rıza, doğrulanmış bir kayıt numarası ve doğrulanmış bir birlikle herkese açık olur.

## Birden fazla üyelik {#memberships}

**Üyelik**, bir kullanıcı hesabı × bir kurum × bir rol demektir. Bir kişinin birden fazla üyeliği olabilir: farklı kurumlarda ya da aynı kurumda birkaç rol (örneğin bir birlikte **Kalite Güvence Sorumlusu** ve **Kurum yöneticisi** ya da küçük bir kurumda **Kurum yöneticisi** ve **Uyum Sorumlusu**). Her zaman tek bir üyelikle çalışırsınız ve haklar birleştirilmez: iki role sahip olmak, kendi girdiğiniz veriyi onaylamanıza hiçbir zaman izin vermez.

Üyelik değiştirmek için:

1. Üst çubuktaki **kurum · rol etiketine** tıklayın (bkz. [Üst çubuk ve rolünüz](#top-bar)). **Rolleriniz** başlıklı bir liste tüm etkin üyeliklerinizi gösterir; geçerli üyelik işaretlidir.
2. Başka bir üyeliğe tıklayın. Platform, yeni üyelikte **Kurumum** sayfasını açar.

Menü, **İşlerim** sayfasındaki görevler, okuyabildiğiniz kayıtlar ve yapabileceğiniz işlemler etkin üyeliğe göre belirlenir. Seçim tarayıcıyı kapatana kadar geçerlidir; sonra ilk üyeliğinizle (kurum adına göre alfabetik sırada) yeniden başlarsınız. İki veya daha fazla üyeliğiniz varsa **Kurumum** sayfasında **Geçiş yap** düğmesiyle birlikte bir **Şu rolle çalış** listesi de bulunur.

Bir arıcının portal hesabının üyeliği ve etiketi yoktur; yalnızca arıcı portalını açar.
