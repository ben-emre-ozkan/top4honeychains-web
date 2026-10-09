# 3. Kurumlar, roller ve yetkiler {#roles}

TOP4HoneyChains'te kimse tek başına işlem yapmaz: her işlem **bir kurum adına, bir rolde** yapılır. Neler yapabileceğinizi üç şey belirler:

1. kurumunuzun **yetenekleri** (ne tür işler yaptığı);
2. kurumdaki **rolünüz** (kurum içinde kim olduğunuz);
3. kurumunuzun **durumu**: yalnızca **Doğrulandı** durumundaki bir kurum veri girebilir.

Bunların yanında bazı kurallar kodda sabittir ve kapatılamaz (bkz. [Sabit kurallar](#fixed-rules)).

## Kurum türleri (yetenekler) {#capabilities}

Bir kurumun bir veya daha fazla yeteneği olur. Yetenekleri bir platform yöneticisi belirler.

| Yetenek | Tipik kurum | Neye izin verir |
|---|---|---|
| **Birlik** | Arıcı birliği | Üye arıcıları bağlar; herkese açık arıcı sayfası için arıcıların rızasını kaydeder |
| **Merkez Birlik** | Ulusal birlik | İsteğe bağlı: platform buna göre yapılandırılmışsa birlikleri doğrulayabilir |
| **İşleme** | Bal işleme tesisi | Teslimleri kaydeder, numune alır, lotta KKN 0–3'ü ve partide KKN 4–6'yı kaydeder |
| **Paketleme** | Dolum ve paketleme tesisi | Partide KKN 7–9'u (dolum, etiketleme, paketleme) ve KKN 10'u kaydeder |
| **Laboratuvar** | Analiz laboratuvarı (akredite veya kurum içi) | Numuneleri teslim alır, laboratuvar raporlarını girer ve onaylar |
| **Toptan Satış** | Toptancı | Parti bulundurur; KKN 10 depolamayı kaydeder |
| **Perakende** | Mağaza veya market | Parti bulundurur; KKN 10 depolamayı kaydeder |
| **Kamu Otoritesi** | Bakanlık, denetim kuruluşu | Kayıtları okur (salt okunur) |
| **Arıcılık** | Arıcılık işletmesi | Arıcılık işletmeleri için ayrılmıştır; arıcılar sisteme giriş yapmaz |
| **Platform Yöneticisi** | Platformu işleten kuruluş | Platform kurumu için ayrılmıştır |

Kendi balını dolduran ve depolayan bir işleme tesisinin **İşleme**, **Paketleme** ve **Toptan Satış** (veya **Perakende**) yeteneklerine ihtiyacı vardır; çünkü partideki KKN 7–9 için Paketleme, KKN 10 için Toptan Satış, Perakende veya Paketleme gerekir.

### Kurum durumu {#org-status}

| Durum | Anlamı |
|---|---|
| **Beklemede** | Yeni kurum, doğrulanmayı bekliyor. Üyeler okuyabilir, kurum yöneticisi belge yükleyebilir. Kimse veri giremez. |
| **Doğrulandı** | Bir platform yöneticisi tarafından kontrol edildi. Üyeler rollerine göre çalışabilir. |
| **Askıda** | Doğrulama (bir gerekçeyle) geri alındı. Yeniden etkinleştirilene kadar Beklemede gibi çalışır. |

## Roller {#role-list}

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
