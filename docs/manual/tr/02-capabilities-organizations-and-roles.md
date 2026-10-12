# 2. Yetenekler, kurumlar ve roller {#roles}

TOP4HoneyChains'te kimse tek başına işlem yapmaz: her işlem **bir kurum adına, bir rolde** yapılır. Neler yapabileceğinizi üç şey belirler:

1. kurumunuzun **yetenekleri**: kurumun bal zincirinde neler yapabildiği;
2. kurumdaki **rolünüz**: kurum içinde kim olduğunuz;
3. kurumunuzun **durumu**: yalnızca **Doğrulandı** durumundaki bir kurum veri girebilir. Kurumları bir platform yöneticisi doğrular (bkz. [Kurum durumu](#org-status)).

Rol bir **üyelik** ile alınır: bir kullanıcı hesabı × bir kurum × bir rol (bkz. [Birden fazla üyelik](#memberships)). Bunların yanında bazı kurallar platforma yerleşiktir ve kapatılamaz (bkz. [Temel kurallar](#fixed-rules)).

Bu bölüm iki şeyi birbirinden ayırır:

- **değer zinciri faaliyetleri**: kişilerin bilgisayar dışında balla yaptığı işler; örneğin teslim etme, numune alma, analiz etme, işleme, depolama ve sevk etme ([Tablo](#tbl-activities));
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
| **Merkez Birlik** | Platform bu şekilde yapılandırılmışsa arıcı birliklerini doğrulayabilir. | yok |
| **Kamu Otoritesi** | Bakanlık veya denetim kuruluşu olarak tüm kayıtları okuyabilir; hiçbir şey girmez. | yok (tümünü okur) |
| **Arıcılık İşletmesi** | Bal üretebilir. Arıcıların hesabı yoktur; işletmeleri bir Arıcı kaydı ile temsil edilir. | yok |

Kendi balını dolduran ve depolayan bir işleme tesisinin **İşleme**, **Paketleme** ve **Toptan Satış** (veya **Perakende**) yeteneklerine ihtiyacı vardır; çünkü partideki KKN 7–9 için Paketleme, KKN 10 için Toptan Satış, Perakende veya Paketleme gerekir.

**Platform.** *Platform yöneticisi* bir kuruma verilebilen bir yetenek değildir: platformu işleten kişilere verilen, kullanıcı hesabına ait genel bir roldür. Platform yöneticileri kurumları, üyelikleri, referans verilerini ve ayarları yönetir ve her kaydı okur, ancak iş akışı verisi girmez (bkz. [bölüm 12](#admin)). Platformun kendi kurumu ayrılmış **Platform Yöneticisi** yeteneğini taşır; bu yetenek başka bir kuruma verilemez ve kaldırılamaz.

## Değer zinciri faaliyetleri {#activities}

Değer zinciri faaliyetlerini kişiler, balla ve bilgisayar dışında yapar. Platform her faaliyetin sonucunu veri olarak kaydeder. [Tablo](#tbl-activities), her yetenek için kurum çalışanlarının hangi faaliyetleri yaptığını ve platformda neyin kaydedildiğini gösterir.

Table: Değer zinciri faaliyetleri (kişiler) ve platformun kaydettikleri (bilgisayar) {#tbl-activities}

| Yetenek | Değer zinciri faaliyetleri (kişiler, bilgisayar dışında) | Platformda kaydedilenler (bilgisayar ortamında) |
|---|---|---|
| Arıcılık İşletmesi | Arıcılık yapar, bal hasat eder, balı bir tesise teslim eder | Yöneticilerin ve birliğin güncel tuttuğu arıcı ve arılık kayıtları |
| Birlik | Üye arıcılarını bağlar ve bilgilendirir, rıza metnini onlara okur, imzalarını toplar | Rıza kayıtları; herkese açık arıcı sayfaları |
| İşleme | Teslimleri teslim alır, balı kabulde kontrol eder, numuneleri mühürler, balı ısıtır, süzer, karıştırır ve dinlendirir | Teslim (ham bal lotu), numune, KKN 0 ve KKN 2–6 verileri |
| Paketleme | Balı doldurur, etiketler ve paketler, tesiste depolar | KKN 7–10 verileri |
| Laboratuvar | Mühürlü numuneleri teslim alıp kontrol eder, balı analiz eder, raporu imzalar | Numune teslimi, laboratuvar raporu (PDF) ve sonuçlar (KKN 1) |
| Toptan Satış, Perakende | Malları teslim alır, depolar, satar veya sevk eder | Sevkiyat onayı, depolama kaydı (KKN 10), yeni sevkiyatlar |
| Merkez Birlik | Arıcı birliklerini kontrol eder | Birliklerin doğrulanması (yapılandırılmışsa) |
| Kamu Otoritesi | Denetler | Kayıtları okur |

## Kurum türleri {#org-types}

Platformda ayrı bir kurum türü listesi yoktur: bir kurumun türü, sahip olduğu yeteneklerin toplamıdır. [Tablo](#tbl-org-types), tipik kurumları listeler.

Table: Tipik kurumlar ve yetenekleri {#tbl-org-types}

| Kurum | Yetenekler | Üyelerin tipik rolleri |
|---|---|---|
| Arıcı birliği | **Birlik** | Kurum yöneticisi, Saha denetçisi |
| İşleme tesisi veya paketleyici | **İşleme**, **Paketleme** ve tesiste depolama için **Toptan Satış** veya **Perakende** | Kurum yöneticisi, Personel, Numune kabul sorumlusu, Kalite Güvence Sorumlusu |
| Gıda analiz laboratuvarı (akredite veya kurum içi) | **Laboratuvar** | Kurum yöneticisi, Analist, Laboratuvar müdürü |
| Toptancı | **Toptan Satış** | Kurum yöneticisi, Personel, Kalite Güvence Sorumlusu |
| Perakendeci | **Perakende** | Kurum yöneticisi, Personel, Kalite Güvence Sorumlusu |
| Bakanlık veya denetim kuruluşu | **Kamu Otoritesi** | Kurum yöneticisi (salt okunur) |

Kendi işleme tesisini işleten bir birliğin **İşleme** (ve **Paketleme**) yeteneği de olur; bu durumda tesisin rolleri de birlikte bulunur.

### Kurum durumu {#org-status}

Kurumları bir platform yöneticisi doğrular, askıya alır ve yeniden etkinleştirir. Platform bu şekilde yapılandırılmışsa, doğrulanmış bir **Merkez Birlik** kurumunun kurum yöneticisi de arıcı birliklerini doğrulayabilir, askıya alabilir ve yeniden etkinleştirebilir. Doğrulama için en az bir yüklenmiş belge, askıya alma için bir gerekçe gerekir. [Tablo](#tbl-org-status), durumları listeler.

Table: Kurum durumu {#tbl-org-status}

| Durum | Anlamı |
|---|---|
| **Beklemede** | Yeni kurum, doğrulanmayı bekliyor. Üyeler okuyabilir, kurum yöneticisi belge yükleyebilir. Kimse veri giremez. |
| **Doğrulandı** | Bir platform yöneticisi (veya yapılandırılmışsa, birlikler için merkez birlik) tarafından kontrol edildi. Üyeler rollerine göre çalışabilir. |
| **Askıda** | Doğrulama (bir gerekçeyle) geri alındı. Yeniden etkinleştirilene kadar Beklemede gibi çalışır. |

## Roller {#role-list}

[Tablo](#tbl-roles), rollerin başvuru listesidir. Bu kılavuzdaki diğer tüm tablolar aynı rol adlarını aynı sırayla kullanır.

Table: Roller {#tbl-roles}

| Rol (bu kılavuzda) | Kod | Tipik kişi | Ana işi |
|---|---|---|---|
| **Kurum yöneticisi** | OrgAdmin | Kurumun yöneticisi | Belge yükler, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, arıcı rızalarını kaydeder (birlikler), teslimleri ve numuneleri okur |
| **Personel** | Staff | Üretim, dolum veya perakende personeli | KKN 1–10 verilerini girer, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, depolamayı kaydeder |
| **Saha denetçisi** | FieldInspector | Birliğin saha teknisyeni | Kayıtları okur; KKN 0'ı yalnızca bir yönetici bu KKN'yi birliğe verirse girer |
| **Numune kabul sorumlusu** | SampleAcceptanceOfficer | Tesisteki kabul görevlisi | Teslimleri kaydeder, KKN 0'ı girer, numune alır ve gönderir |
| **Kalite Güvence Sorumlusu** | QualityApprover | Kalite yöneticisi, gıda mühendisi | KKN kayıtlarını ve depolama kayıtlarını onaylar, kesinleştirmeyi onaylar |
| **Analist** | Analyst | Laboratuvar analisti | Numuneleri teslim alır, laboratuvar sonuçlarını girer, raporları onaya gönderir (laboratuvardaki birinci kişi) |
| **Laboratuvar müdürü** | LabManager | Laboratuvar müdürü | Numuneleri teslim alır, laboratuvar raporlarını onaylar veya geri gönderir (laboratuvardaki ikinci kişi) |
| **Platform yöneticisi** | (genel rol) | Platformu işleten kişi | Yönetim (bölüm 12); tüm iş akışı verilerini okur, ancak hiçbirini girmez veya onaylamaz |

Platform yöneticisi, iş kurumlarının iş akışında yer almaz: her kaydı okuyabilir, ancak **iş akışı verisi giremez ve onaylayamaz** (teslimler, KKN verileri, laboratuvar sonuçları, sevkiyatlar, rızalar). Onaydan sonra bir verinin hatalı olduğu anlaşılırsa yönetici bir *düzeltme* talep eder; düzeltmeyi ikinci bir yönetici onaylar (bkz. [Düzeltmeler](#admin-corrections)). Ana verileri ise yöneticiler tutar: arıcılar, arılıklar, lotlar, partiler ve partilerin bileşimi (bkz. [bölüm 12](#admin)).

## Veri hakları (CRUD) {#crud}

[Tablo](#tbl-crud), her rolün her kayıt türü üzerindeki varsayılan veri haklarını CRUD modeliyle gösterir: **C** oluşturma (create), **R** okuma (read), **U** güncelleme (update), **D** silme (delete) ve ayrı bir hak olarak **A** onay (approve). "–" hak olmadığını gösterir. Bir üyenin hakları yalnızca kendi kurumunun işine ait kayıtlar için geçerlidir (kurumun kendi lotları ve partileri, elinde tuttuğu partiler, birliğinin arıcıları) ve okuma dışında yalnızca kurum doğrulanmışken geçerlidir. Haklar koddan ve varsayılan ayarlardan gelir; bunlardan hangilerini bir yöneticinin değiştirebileceğini [Tablo](#tbl-admin-changeable) listeler.

Table: Rollere göre varsayılan veri hakları (CRUD + Onay) {#tbl-crud}

| Kayıt | Kurum yöneticisi | Personel | Saha denetçisi | Numune kabul sorumlusu | Kalite Güvence Sorumlusu | Analist | Laboratuvar müdürü | Platform yöneticisi | Herkese açık sayfaların ziyaretçisi |
|---|---|---|---|---|---|---|---|---|---|
| Arıcı | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Arılık | R ¹ | R ¹ | R ¹ | R ¹ | R ¹ | – | – | C R U ² | R ³ |
| Ham bal lotu | R | R | R | C R ⁴ | R | – | – | C R U D ⁵ | R ³ |
| Numune | R | – | – | C R | – | R A ⁶ | R A ⁶ | R | – |
| Laboratuvar raporu ve sonuçları | R | R | R | R | R | C R U D ⁷ | R A ⁷ | R | R ⁸ |
| KKN kaydı (veriler, "Uygulanamaz") | R | C R U D ⁹ | R ⁹ | C R U D ⁹ | R A | R | R | R ¹⁰ | R ³ |
| Parti | R, kesinleştirme talebi | R, kesinleştirme talebi | R | R | R, kesinleştirmeyi onaylama (A) | R | R | C R U D ¹¹ | R ³ |
| Sevkiyat | C R A ¹² | C R A ¹² | R | R | R | R | R | R | – |
| Alıcıdaki depolama kaydı (KKN 10) | R | C R U D | R | R | R A | R | R | R | – |
| Arıcı rızası | C R U ¹³ | R | R | R | R | R | R | R | R ¹⁴ |
| Kurum (bilgiler, belgeler) | R; belgeler C | R | R | R | R | R | R | C R U, durum A ¹⁵ | – |
| Üyelik | R | R | R | R | R | R | R | C R U ¹⁶ | – |
| Denetim kaydı girişi | – | – | – | – | – | – | – | R ¹⁷ | – |

Notlar:

1. Yalnızca kurumun üzerinde çalıştığı kayıtlar aracılığıyla okunur (KKN sayfaları, geri çağırma raporu). Partiyi sonradan elinde tutan kurum verileri arıcı ve arılık adları olmadan okur. Birlik üyeleri, birliğin üye arıcılarının listesini okur.
2. Arıcılar ve arılıklar hiçbir zaman silinmez, yalnızca pasifleştirilir. Arılık fotoğrafları eklenebilir, değiştirilebilir ve silinebilir.
3. Kesinleşmiş partilerin herkese açık sayfalarında (Bal Hikâyesi) ve yayımlanmış arıcı sayfalarında: adlar, il ve onaylı veriler; hiçbir zaman koordinatlar veya iletişim bilgileri değil (bkz. [Herkesin okuyabildiği bilgiler](#trust-public)).
4. Lot, **İşleme** yeteneğine sahip doğrulanmış bir kurumda bir teslim kaydedilerek oluşturulur.
5. Yöneticiler lotun ana verilerini tutar (örneğin sahip kurum ve KKN ataması). Lot kesinleşmiş bir partide yer alıyorsa hiçbir şey değiştirilemez; teslimleri, numuneleri veya kullanımda KKN verileri olan bir lot silinemez.
6. **A** = numuneyi mührü sağlam olarak teslim alma veya reddetme.
7. Analist, rapor taslak durumundayken raporu oluşturur, kaydeder, değiştirir ve onaya gönderir; laboratuvar müdürü onaylar veya geri gönderir. Onaylanmış bir rapor değiştirilemez.
8. Rapor koduyla **Laboratuvar raporu doğrula** sayfası.
9. Kayıt onaylanana veya kilitlenene kadar veri girme (C), veriyi değiştirme veya temizleme (U, D) ve "Uygulanamaz" olarak işaretleme. Varsayılan veri giren roller: KKN 0–1 için Numune kabul sorumlusu, KKN 1–10 için Personel. Saha denetçisi KKN 0'ı yalnızca bir yönetici KKN 0'ı birliğe verirse girer.
10. Yöneticiler onaylanmış veya kilitli verileri yalnızca bir yöneticinin talep ettiği ve ikinci bir yöneticinin onayladığı bir düzeltmeyle değiştirir.
11. Parti oluşturma, bileşimini lotlardan belirleme, KKN'lerini atama ve sahibini belirleme. Bir parti yalnızca kesinleşmeden önce silinebilir. Yöneticiler kesinleştirme yapamaz.
12. **C** = kurumunuzun elinde tuttuğu kesinleşmiş bir partiyi sevk etme; **A** = kurumunuza gelen bir sevkiyatı onaylama veya reddetme.
13. Rızayı kaydetme (C) ve geri çekme (U); yalnızca birlikler.
14. Yayımlanmış arıcı sayfası.
15. Doğrulama, belge isteme, askıya alma ve yeniden etkinleştirme (bkz. [Kurum durumu](#org-status)). Kurumlar hiçbir zaman silinmez.
16. Üyelik ekleme ve iptal etme (U); üyelikler hiçbir zaman silinmez.
17. Denetim kaydına yalnızca ekleme yapılabilir; hiç kimse bir girişi değiştiremez veya silemez.

**Neler silinebilir?** Üyeler yalnızca kendi verilerini ve yalnızca onaylanmadan önce temizler (KKN verileri, taslak laboratuvar sonuçları, depolama verileri). Yöneticiler kesinleşmeden önce lotları ve partileri, arılık fotoğraflarını, referans verilerini ve kullanıcı hesaplarını (kendi hesapları hariç) silebilir. Arıcılar, arılıklar, kurumlar, belgeler, üyelikler, rızalar, numuneler, laboratuvar raporları, sevkiyatlar ve denetim kaydı girişleri hiçbir zaman silinmez.

## Yetki matrisi {#permission-matrix}

[Tablo](#tbl-permissions), bugün yapılandırıldığı haliyle varsayılan yetkileri **platformdaki işlemlere** (örneğin "teslim kaydetme" gibi bir işlev) göre gösterir. Veri hakları tablosunu ([Tablo](#tbl-crud)) tamamlar ve aynı rolleri aynı sırayla kullanır. Açıklama: **R** okuma, **C** oluşturma veya veri girişi, **A** onay (veya teslim alma), **F** kesinleştirme (talep veya onay), **–** yetki yok. "Gereken yetenek" sütunu, kurumun hangi yeteneğe sahip olması gerektiğini belirtir.

Table: Platformdaki işlemlere ve rollere göre varsayılan yetkiler {#tbl-permissions}

| Platformdaki işlem | Gereken yetenek | Kurum yöneticisi | Personel | Saha denetçisi | Numune kabul sorumlusu | Kalite Güvence Sorumlusu | Analist | Laboratuvar müdürü | Platform yöneticisi |
|---|---|---|---|---|---|---|---|---|---|
| Teslim kaydetme (teslim alma) | İşleme | R | – | – | C R | – | – | – | R |
| Numune alma ve gönderme | İşleme (lot sahibi) | R | – | – | C R | – | R | R | R |
| Numune teslim alma (mühür kontrolü) | Laboratuvar | – | – | – | – | – | A | A | R |
| Laboratuvar sonuçlarını girme, raporu onaya gönderme | Laboratuvar | R | R | R | R | R | C R | R | R |
| Laboratuvar raporunu onaylama veya geri gönderme | Laboratuvar | R | R | R | R | R | R | A R | R |
| KKN 0 kabul kontrolü (lot) | İşleme (lot sahibi) | R | R | R | C R | A R | R | R | R |
| KKN 1 laboratuvar verileri (lot) | onaylı laboratuvar raporundan yazılır | R | R | R | R | R | R | R | R |
| KKN 2–3 (lot) | İşleme (lot sahibi) | R | C R | R | R | A R | R | R | R |
| Parti oluşturma, bileşimini belirleme, KKN atama | – | – | – | – | – | – | – | – | C |
| KKN 4–6 (parti) | İşleme (parti sahibi) | R | C R | R | R | A R | R | R | R |
| KKN 7–9 (parti) | Paketleme (parti sahibi) | R | C R | R | R | A R | R | R | R |
| Tesiste KKN 10 depolama (parti) | Toptan Satış, Perakende veya Paketleme (parti sahibi) | R | C R | R | R | A R | R | R | R |
| Kesinleştirme talep etme | sahip, doğrulanmış | F | F | – | – | – | – | – | – |
| Kesinleştirmeyi onaylama (mühürleme) | sahip, doğrulanmış | – | – | – | – | F | – | – | – |
| Sevkiyat oluşturma | partiyi elinde tutan, doğrulanmış | C R | C R | – | – | R | – | – | R |
| Gelen sevkiyatı onaylama veya reddetme | alıcı, doğrulanmış | A R | A R | – | – | R | – | – | R |
| Alıcıda KKN 10 depolama kaydı | Toptan Satış, Perakende veya Paketleme | R | C R | R | R | A R | R | R | R |
| Geri çağırma raporu (kendi kapsamı) | – | R | R | R | R | R | R | R | R (tümü) |
| Arıcı rızasını kaydetme veya geri çekme | Birlik | C R | – | – | – | – | – | – | R |
| Kurum belgelerini yükleme | kendi kurumu | C | – | – | – | – | – | – | C |
| Kurumları doğrulama, üyelikleri ve yetenekleri yönetme | – | – | – | – | – | – | – | – | C A |
| Arıcı kayıt numaralarını doğrulama | – | – | – | – | – | – | – | – | A |
| Yetkileri, KKN'leri, limitleri, çevirileri değiştirme | – | – | – | – | – | – | – | – | C |
| Düzeltme talep etme veya karara bağlama | – | – | – | – | – | – | – | – | C A (iki farklı yönetici) |
| Denetim kaydı | – | – | – | – | – | – | – | – | R |

Notlar:

- **KKN'leri okuma (R)**: her rol, kurumunun yer aldığı lot ve partilerin KKN'lerini açabilir: sahip veya sorumlu kurum olarak, numuneyi alan kurum olarak (lotlar için), partiyi sonradan elinde tutan kurum olarak ya da doğrulanmış bir kamu otoritesi olarak. Partiyi elinde tutan kurum, partinin ve lotlarının KKN'lerini **salt okunur** olarak okur. Diğer kurumlar "Bu sayfayı açamazsınız" (403) sayfasını alır.
- **KKN 1** onaylı laboratuvar raporundan doldurulur. KKN 1 için yapılandırılmış veri giren roller (Numune kabul sorumlusu, Personel) yalnızca laboratuvar raporunun kapsamadığı KKN 1 parametreleri için kullanılır.
- **Sevkiyatlar ve kesinleştirme** bir yeteneği değil, kurumun rolünü ve durumunu kontrol eder: kesinleşmiş bir partinin doğrulanmış her sahibi partiyi sevk edebilir.
- Yönetici **KKN 0**'ı tesis yerine arıcının birliğine verebilir (**KKN sorumlulukları** → KKN 0 → **Lotun arıcısının birliği**, yetenek Birlik, rol Saha denetçisi). Bu varsayılan ayar değildir.

<!-- SHOT: admin-permissions-matrix -->
Figure: **Roller ve yetkiler**: Personel rolünün varsayılan yetkileri.

### Bir yöneticinin değiştirebilecekleri {#admin-changeable}

[Tablo](#tbl-admin-changeable), bir platform yöneticisinin değiştirebileceği tüm ayarları listeler. Geri kalan her şey sabittir: roller listesi, yetenekler listesi ve [Temel kurallar](#fixed-rules).

Table: Bir platform yöneticisinin değiştirebilecekleri {#tbl-admin-changeable}

| Ayar | Nerede | Tam olarak neler değiştirilebilir |
|---|---|---|
| Rol yetkileri | **Roller ve yetkiler** → **Rol yetkileri** | Rol başına: her KKN için **Giriş**, **Onayla** ve **Okuma**; teslimler (teslim alma) için **Giriş** ve **Okuma**; numuneler için **Giriş**, **Onayla** (teslim alma) ve **Okuma**; laboratuvar sonuçları için **Giriş**, **Onayla** ve **Okuma**; sevkiyatlar için **Giriş** (sevk etme), **Onayla** (teslimi onaylama) ve **Okuma**; arıcı sayfaları için **Giriş** ve **Okuma**; geri çağırma raporu için **Okuma**; kesinleştirme için **Giriş** (talep) ve **Onayla** (onaylama). Her yetkide en az bir rol kalır. |
| KKN sorumlulukları | **KKN sorumlulukları** | KKN başına: sorumlu kurum (lotun veya partinin sahibi ya da lotlar için arıcının birliği), gereken yetenekler, veri giren roller, onaylayan roller ve onayın gerekip gerekmediği. |
| Yetenekler | Kurum sayfası; **Kurum yetkinlikleri** | Bir kurumun yeteneklerini ekleme veya kaldırma (en az biri kalır); laboratuvar akreditasyon numarası ve geçerlilik tarihi. |
| Dört göz muafiyeti | Kurum sayfası | Kurum başına: üyelerin kendi KKN kayıtlarını ve depolama kayıtlarını onaylamasına ve kendi kesinleştirme taleplerini onaylamasına izin verir; denetim kaydında her zaman işaretlenir. Laboratuvar sonuçları için hiçbir zaman geçerli değildir. |
| Kurum durumu | Kurum sayfası | Doğrulama, belge isteme, askıya alma, yeniden etkinleştirme. |
| Üyelikler ve kullanıcılar | Kurum sayfası; **Üyeler**; **Kullanıcı Yönetimi** | Üyelik ekleme ve iptal etme; kullanıcı oluşturma, şifre belirleme, kilit açma, hesap silme. |
| Kontrol noktaları, parametreler, limitler | **Kritik Kontrol Noktaları** | Parametreler, veri türleri, zorunlu sorular, limitler, "Limitler doğrulandı". |
| Ana veriler ve referans verileri | Yönetim menüleri | Arıcılar, arılıklar, kayıt numaraları, lotlar, partiler, bal türleri, dönemler, çeviriler. |

## Temel kurallar {#fixed-rules}

Bu kurallar platforma yerleşiktir; hiçbir ayar bunları kapatamaz:

- **Yalnızca doğrulanmış kurumlar veri girer.**
- **KKN kayıtlarında dört göz**: kimse veri kaydettiği veya "Uygulanamaz" olarak işaretlediği bir KKN kaydını onaylayamaz. İstisna: bir platform yöneticisi bir kuruma **Dört göz muafiyeti** tanıyabilir; bu durumda üyeler kendi KKN kayıtlarını onaylayabilir ve bu tür her onay denetim kaydında işaretlenir.
- **Laboratuvar sonuçlarında dört göz, her zaman**: sonuçları giren kişi bu sonuçları onaylayamaz. Muafiyet yoktur.
- **Kesinleştirmede ve depolama kayıtlarında dört göz**: kesinleştirmeyi talep eden kişi talebi onaylayamaz, bir depolama kaydını giren kişi de o kaydı onaylayamaz (kurumun muafiyeti yoksa).
- **Her değişiklik onayı kaldırır**: verinin değiştirilmesi mevcut bir onayı siler; limit dışındaki veriler onaylanırken açıkça teyit edilmelidir.
- **Laboratuvar sonuçları numuneye bağlıdır**: sonuçlar yalnızca mührü sağlam olarak teslim alınmış bir numune için ve yalnızca lotun KKN 0'ı onaylandıktan sonra girilir; onaylanmış bir rapor değiştirilemez.
- **Platform yöneticileri iş akışı verilerinde salt okunurdur** ve bu verileri yalnızca iki yöneticili düzeltme süreciyle düzeltir.
- **Kesinleşen kilitlenir**: bir parti ancak partinin ve lotlarının her KKN'si tamamlandığında kesinleştirilir; kesinleşmiş bir parti ve tüm lotları, düzeltme dışında artık değiştirilemez. Kesinleştirmeyi geri almak mümkün değildir.
- **Sevkiyatlar**: yalnızca kesinleşmiş partiler, yalnızca doğrulanmış başka bir kuruma, en çok eldeki miktar kadar.
- **Kurumları yalnızca yöneticiler yapılandırır**: yetenekler, üyelikler ve muafiyet.
- **Her yetkide en az bir rol kalır.**
- **Denetim kaydına yalnızca ekleme yapılabilir**; kayıttaki hiçbir şey değiştirilmez veya silinmez, yetki sayfalarındaki her değişiklik de ayrıca kaydedilir.
- **Arıcı sayfaları** yalnızca etkin bir rıza, doğrulanmış bir kayıt numarası ve doğrulanmış bir birlikle herkese açık olur.

## Birden fazla üyelik {#memberships}

**Üyelik**, bir kullanıcı hesabı × bir kurum × bir rol demektir. Bir kişinin birden fazla üyeliği olabilir: farklı kurumlarda ya da aynı kurumda birkaç rol (örneğin bir birlikte **Kalite Güvence Sorumlusu** ve **Kurum yöneticisi**). Her zaman tek bir üyelikle çalışırsınız ve haklar birleştirilmez: iki role sahip olmak, kendi girdiğiniz veriyi onaylamanıza hiçbir zaman izin vermez.

Üyelik değiştirmek için:

1. Üst çubuktaki **kurum · rol etiketine** tıklayın (bkz. [Üst çubuk ve rolünüz](#top-bar)). **Rolleriniz** başlıklı bir liste tüm etkin üyeliklerinizi gösterir; geçerli üyelik işaretlidir.
2. Başka bir üyeliğe tıklayın. Platform, yeni üyelikte **Kurumum** sayfasını açar.

Menü, **İşlerim** sayfasındaki görevler, okuyabildiğiniz kayıtlar ve yapabileceğiniz işlemler etkin üyeliğe göre belirlenir. Seçim tarayıcıyı kapatana kadar geçerlidir; sonra ilk üyeliğinizle (kurum adına göre alfabetik sırada) yeniden başlarsınız. İki veya daha fazla üyeliğiniz varsa **Kurumum** sayfasında **Geçiş yap** düğmesiyle birlikte bir **Şu rolle çalış** listesi de bulunur.
