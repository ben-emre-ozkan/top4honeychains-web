# 2. Yetenekler, kurumlar ve roller {#roles}

TOP4HoneyChains'te kimse tek başına işlem yapmaz: her işlem **bir kurum adına, bir rolde** yapılır. Neler yapabileceğinizi üç şey belirler:

1. kurumunuzun **yetenekleri** (ne tür işler yaptığı);
2. kurumdaki **rolünüz** (kurum içinde kim olduğunuz);
3. kurumunuzun **durumu**: yalnızca **Doğrulandı** durumundaki bir kurum veri girebilir.

Bunların yanında bazı kurallar kodda sabittir ve kapatılamaz (bkz. [Sabit kurallar](#fixed-rules)).

## Yetenekler {#capabilities}

Yetenek, bir kurumun bal zincirinde hangi işi yapabileceğini belirler. İşi kurumda hangi üyenin yapacağını roller belirler (bkz. [Roller](#role-list)). Bir kurumun bir veya daha fazla yeteneği olur; yetenekleri bir platform yöneticisi verir ve kaldırır (bkz. [Yetenekler](#admin-org-capabilities)). Yetenek listesinin kendisi sabittir.

| Yetenek (ekranda) | Anlamı (ekrandaki açıklama) | Kurumun yapabildikleri | KKN'ler (varsayılan) |
|---|---|---|---|
| **Birlik** | Arıcı birliği; üye arıcılarını bağlar ve herkese açık arıcı sayfası için rızalarını kaydeder | Üye arıcılarını bağlar, rızalarını kaydeder veya geri çeker ve arıcı sayfalarını yayımlar (**Arıcı sayfaları**); üye arıcılarının lotlarını görür, örneğin geri çağırma raporlarında | yok; KKN 0 yalnızca bir yönetici bu KKN'yi birliğe verirse |
| **İşleme** | Kabul ve işleme: teslim almalar, numuneler, lot KKN 0–3 ve parti KKN 4–6 | Teslimleri kaydeder, bu kayıt kuruma ait ham bal lotlarını oluşturur; lotları kabulde kontrol eder; mühürlü numune alıp laboratuvara gönderir; işleme KKN'lerini kaydeder (**Teslim almalar**, **Numuneler ve laboratuvar**) | lot KKN 0–3, parti KKN 4–6 |
| **Paketleme** | Dolum, etiketleme ve paketleme, KKN 7-9 | Partilerinin dolum, etiketleme ve paketlemesini ve tesisteki KKN 10 depolamayı kaydeder | parti KKN 7–9, KKN 10 |
| **Laboratuvar** | Laboratuvar (akredite veya kurum içi) | Mühürlü numuneleri teslim alır, laboratuvar sonuçlarını girer ve raporu onaylar; raporu her zaman ikinci bir kişi onaylar. Akreditasyon numarası ve geçerlilik tarihi bu yetenekte tutulur (**Numuneler ve laboratuvar**) | yok; onaylanan sonuçları lot KKN 1'i doldurur |
| **Toptan Satış** | Toptan satış; ürün elindeyken KKN 10'da emanetçi | Teslim aldığı partileri elinde tutar, elindeki miktar için KKN 10 depolama kaydını girer ve sevk edebilir (**Sevkiyatlar ve stok**) | KKN 10 |
| **Perakende** | Perakende; ürün elindeyken KKN 10'da emanetçi | Satış için sevkiyat alır, elindeki miktar için KKN 10 depolama kaydını girer ve sevk edebilir (**Sevkiyatlar ve stok**) | KKN 10 |

Kendi balını dolduran ve depolayan bir işleme tesisinin **İşleme**, **Paketleme** ve **Toptan Satış** (veya **Perakende**) yeteneklerine ihtiyacı vardır; çünkü partideki KKN 7–9 için Paketleme, KKN 10 için Toptan Satış, Perakende veya Paketleme gerekir.

Listede özel durumlar için üç yetenek daha vardır:

- **Merkez Birlik**: ulusal arıcı birliği. Platform, birlikleri merkez birliğin doğrulayacağı şekilde yapılandırılmışsa, doğrulanmış bir merkez birliğin kurum yöneticisi birlikleri doğrular ve doğruladığı birlikleri görür.
- **Kamu Otoritesi**: bakanlık veya denetim kuruluşu. Doğrulandıktan sonra üyeleri tüm kayıtları okur; hiçbir şey girmez.
- **Arıcılık**: arıcının işletmesi. Arıcılar sisteme giriş yapmaz; platformun ve birliklerinin tuttuğu kayıtlardır.

**Platform.** *Platform yöneticisi* bir kuruma verilebilen bir yetenek değildir: platformu işleten kişilere verilen, kullanıcı hesabına ait genel bir roldür. Platform yöneticileri kurumları, üyeleri, referans verilerini ve ayarları yönetir ve her kaydı okur, ancak iş verisi girmez (bkz. [bölüm 12](#admin)). Platformun kendi kurumu ayrılmış **Platform Yöneticisi** yeteneğini taşır; bu yetenek başka bir kuruma verilemez ve kaldırılamaz.

## Kurum türleri {#org-types}

Platformda ayrı bir kurum türü listesi yoktur: bir kurumun türü, sahip olduğu yeteneklerin toplamıdır. Tipik kurumlar:

| Kurum | Yetenekler | Üyelerin tipik rolleri |
|---|---|---|
| Arıcı birliği | **Birlik** | Kurum yöneticisi; Saha denetçisi |
| İşleme tesisi veya paketleyici | **İşleme**, **Paketleme** ve tesiste depolama için **Toptan Satış** veya **Perakende** | Kurum yöneticisi, Numune kabul sorumlusu, Personel, Kalite onaylayıcı |
| Laboratuvar (akredite veya kurum içi) | **Laboratuvar** | Kurum yöneticisi, Analist, Laboratuvar müdürü |
| Toptancı | **Toptan Satış** | Kurum yöneticisi, Personel, Kalite onaylayıcı |
| Market veya mağaza | **Perakende** | Kurum yöneticisi, Personel, Kalite onaylayıcı |
| Bakanlık veya denetim kuruluşu | **Kamu Otoritesi** | Kurum yöneticisi (salt okunur) |

Kendi işleme tesisini işleten bir birliğin **İşleme** (ve **Paketleme**) yeteneği de olur; bu durumda tesisin rolleri de birlikte bulunur.

### Kurum durumu {#org-status}

| Durum | Anlamı |
|---|---|
| **Beklemede** | Yeni kurum, doğrulanmayı bekliyor. Üyeler okuyabilir, kurum yöneticisi belge yükleyebilir. Kimse veri giremez. |
| **Doğrulandı** | Bir platform yöneticisi tarafından kontrol edildi. Üyeler rollerine göre çalışabilir. |
| **Askıda** | Doğrulama (bir gerekçeyle) geri alındı. Yeniden etkinleştirilene kadar Beklemede gibi çalışır. |

## Roller {#role-list}

Rol bir **üyelik** ile alınır: bir kişi, bir kurum, bir rol. Üyelikleri platform yöneticileri oluşturur (bkz. [Üyeler](#admin-members)); bir kişinin birden fazla üyeliği olabilir (bkz. [Birden fazla üyelik](#memberships)).

| Rol (ekranda) | Kod | Tipik kişi | Ana işi |
|---|---|---|---|
| **Kurum yöneticisi** | OrgAdmin | Kurumun yöneticisi | Belge yükler, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, arıcı rızalarını kaydeder (birlikler), teslimleri ve numuneleri okur |
| **Personel** | Staff | Üretim, dolum veya mağaza personeli | KKN 2–10 değerlerini girer, kesinleştirme talep eder, sevk eder ve sevkiyatları onaylar, depolamayı kaydeder |
| **Saha denetçisi** | FieldInspector | Birliğin saha teknisyeni | Kayıtları okur; KKN 0'ı yalnızca bir yönetici bu KKN'yi birliğe verirse girer |
| **Numune kabul sorumlusu** | SampleAcceptanceOfficer | Tesisteki kabul görevlisi | Teslimleri kaydeder, KKN 0'ı girer, numune alır ve gönderir |
| **Kalite onaylayıcı** | QualityApprover | Kalite yöneticisi, gıda mühendisi | KKN kayıtlarını onaylar, kesinleştirmeyi onaylar, depolama kayıtlarını onaylar |
| **Analist** | Analyst | Laboratuvar analisti | Numuneleri teslim alır, laboratuvar sonuçlarını girer, raporları onaya gönderir |
| **Laboratuvar müdürü** | LabManager | Laboratuvar müdürü | Numuneleri teslim alır, laboratuvar raporlarını onaylar veya geri gönderir |
| **Platform yöneticisi** | (genel rol) | Platformu işleten kişi | Yönetim (bölüm 12); iş verilerinde salt okunur |

Platform yöneticisi, iş kurumlarının iş akışında yer almaz: her kaydı okuyabilir, ancak **iş verisi giremez ve onaylayamaz** (teslimler, KKN değerleri, laboratuvar sonuçları, sevkiyatlar, rızalar). Onaydan sonra bir verinin hatalı olduğu anlaşılırsa yönetici bir *düzeltme* talep eder; düzeltmeyi ikinci bir yönetici onaylar (bkz. [Düzeltmeler](#admin-corrections)).

## Yetki matrisi {#permission-matrix}

Aşağıdaki matris, bugün yapılandırıldığı haliyle **varsayılan** yetkileri gösterir. Bir platform yöneticisi bunların çoğunu **Roller ve yetkiler** ile **KKN sorumlulukları** sayfalarında değiştirebilir; aşağıdaki sabit kurallar ise değiştirilemez.

Açıklama: **G** görüntüleme, **E** ekleme veya veri girişi, **O** onay (veya teslim alma), **K** kesinleştirme (talep veya onay), **–** yetki yok. "Gereken yetenek" sütunu, kurumun hangi yeteneğe sahip olması gerektiğini belirtir.

| İşlem | Gereken yetenek | Kurum yöneticisi | Personel | Saha denetçisi | Numune kabul sorumlusu | Kalite onaylayıcı | Analist | Laboratuvar müdürü | Platform yöneticisi |
|---|---|---|---|---|---|---|---|---|---|
| Teslim kaydetme (teslim alma) | İşleme | G | – | – | E G | – | – | – | G |
| Numune alma ve gönderme | İşleme (lot sahibi) | G | – | – | E G | – | G | G | G |
| Numune teslim alma (mühür kontrolü) | Laboratuvar | – | – | – | – | – | O | O | G |
| Laboratuvar sonuçlarını girme, raporu onaya gönderme | Laboratuvar | G | G | G | G | G | E G | G | G |
| Laboratuvar raporunu onaylama veya geri gönderme | Laboratuvar | G | G | G | G | G | G | O G | G |
| KKN 0 kabul kontrolü (lot) | İşleme (lot sahibi) | G | G | G | E G | O G | G | G | G |
| KKN 1 laboratuvar değerleri (lot) | onaylı laboratuvar raporundan yazılır | G | G | G | G | G | G | G | G |
| KKN 2–3 (lot) | İşleme (lot sahibi) | G | E G | G | G | O G | G | G | G |
| Parti oluşturma, bileşimini belirleme, KKN atama | – | – | – | – | – | – | – | – | E |
| KKN 4–6 (parti) | İşleme (parti sahibi) | G | E G | G | G | O G | G | G | G |
| KKN 7–9 (parti) | Paketleme (parti sahibi) | G | E G | G | G | O G | G | G | G |
| Tesiste KKN 10 depolama (parti) | Toptan Satış, Perakende veya Paketleme (parti sahibi) | G | E G | G | G | O G | G | G | G |
| Kesinleştirme talep etme | sahip, doğrulanmış | K | K | – | – | – | – | – | – |
| Kesinleştirmeyi onaylama (mühürleme) | sahip, doğrulanmış | – | – | – | – | K | – | – | – |
| Sevkiyat oluşturma | partiyi elinde tutan, doğrulanmış | E G | E G | – | – | G | – | – | G |
| Gelen sevkiyatı onaylama veya reddetme | alıcı, doğrulanmış | O G | O G | – | – | G | – | – | G |
| Alıcıda KKN 10 depolama kaydı | Toptan Satış, Perakende veya Paketleme | G | E G | G | G | O G | G | G | G |
| Geri çağırma raporu (kendi kapsamı) | – | G | G | G | G | G | G | G | G (tümü) |
| Arıcı rızasını kaydetme veya geri çekme | Birlik | E G | – | – | – | – | – | – | G |
| Kurum belgelerini yükleme | kendi kurumu | E | – | – | – | – | – | – | E |
| Kurumları doğrulama, üyeleri ve yetenekleri yönetme | – | – | – | – | – | – | – | – | E O |
| Arıcı kayıt numaralarını doğrulama | – | – | – | – | – | – | – | – | O |
| Yetkileri, KKN'leri, limitleri, çevirileri değiştirme | – | – | – | – | – | – | – | – | E |
| Düzeltme talep etme veya karara bağlama | – | – | – | – | – | – | – | – | E O (iki farklı yönetici) |
| Denetim kaydı | – | – | – | – | – | – | – | – | G |

Notlar:

- **KKN'leri görüntüleme (G)**: her rol, kurumunun yer aldığı lot ve partilerin KKN'lerini açabilir: sahip veya sorumlu kurum olarak, numuneyi alan kurum olarak (lotlar için), partiyi sonradan elinde tutan kurum olarak ya da doğrulanmış bir kamu otoritesi olarak. Partiyi elinde tutan kurum, partinin ve lotlarının KKN'lerini **salt okunur** görür. Diğer kurumlar "Bu sayfayı açamazsınız" (403) sayfasını görür.
- **KKN 1** onaylı laboratuvar raporundan doldurulur. KKN 1 için yapılandırılmış veri giren roller (Numune kabul sorumlusu, Personel) yalnızca laboratuvar raporunun kapsamadığı KKN 1 parametreleri için kullanılır.
- **Sevkiyatlar ve kesinleştirme** bir yeteneği değil, kurumun parti üzerindeki rolünü ve kurumun durumunu kontrol eder: kesinleşmiş bir partinin doğrulanmış her sahibi partiyi sevk edebilir.
- Yönetici **KKN 0**'ı tesis yerine arıcının birliğine verebilir (**KKN sorumlulukları** → KKN 0 → "Lotun arıcısının birliği", yetenek Birlik, rol Saha denetçisi). Bu varsayılan ayar değildir.

<!-- SHOT: admin-permissions-matrix -->

## Sabit kurallar {#fixed-rules}

Bu kurallar platforma yerleşiktir:

- **KKN kayıtlarında dört göz**: kimse değer girdiği veya "Uygulanamaz" olarak işaretlediği bir KKN kaydını onaylayamaz. İstisna: bir platform yöneticisi bir kuruma **Dört göz muafiyeti** tanıyabilir; bu durumda üyeler kendi KKN kayıtlarını onaylayabilir ve bu tür her onay denetim kaydında işaretlenir.
- **Laboratuvar sonuçlarında dört göz, her zaman**: sonuçları giren kişi bu sonuçları onaylayamaz. Muafiyet yoktur.
- **Kesinleştirmede dört göz**: kesinleştirmeyi talep eden kişi talebi onaylayamaz (kurumun muafiyeti yoksa).
- **Yalnızca doğrulanmış kurumlar veri girer.**
- **Platform yöneticileri iş verilerinde salt okunurdur** ve bu verileri yalnızca iki yöneticili düzeltme süreciyle düzeltir.
- **Kesinleşen kilitlenir**: kesinleşmiş bir parti ve tüm lotları, düzeltme dışında artık değiştirilemez.
- **Denetim kaydına yalnızca ekleme yapılabilir**; kayıttaki hiçbir şey değiştirilmez veya silinmez, yetki sayfalarındaki her değişiklik de ayrıca kaydedilir.

## Birden fazla üyelik {#memberships}

Bir kişinin birden fazla üyeliği olabilir: farklı kurumlarda ya da aynı kurumda birkaç rol (örneğin bir birlikte **Kalite onaylayıcı** ve **Kurum yöneticisi**). Her zaman tek bir üyelikle çalışırsınız.

Üyelik değiştirmek için:

1. Üst çubuktaki **kurum · rol etiketine** tıklayın (bkz. [Üst çubuk ve rolünüz](#top-bar)). **Rolleriniz** başlıklı bir liste tüm etkin üyeliklerinizi gösterir; geçerli üyelik işaretlidir.
2. Başka bir üyeliğe tıklayın. Platform, yeni üyelikte **Kurumum** sayfasını açar.

Menü, **İşlerim** sayfasındaki görevler, görebildiğiniz kayıtlar ve yapabileceğiniz işlemler etkin üyeliğe göre belirlenir. Seçim tarayıcıyı kapatana kadar geçerlidir; sonra ilk üyeliğinizle (kurum adına göre alfabetik sırada) yeniden başlarsınız. İki veya daha fazla üyeliğiniz varsa **Kurumum** sayfasında **Geçiş yap** düğmesiyle birlikte bir **Şu rolle çalış** listesi de bulunur.
