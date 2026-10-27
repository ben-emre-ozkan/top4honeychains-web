# 10. Kurum yöneticisi {#role-org-admin}

## Günlük işiniz {#orgadmin-daily}

Kurum yöneticisi, platformda bir kurumun yöneticisidir. Göreviniz:

- kurumun belgelerini güncel tutmak (ruhsat, akreditasyon, vergi levhası, kayıt kanıtı);
- bir tesiste: teslimleri ve numuneleri okumak, kurumunuzun sahip olduğu lotlar için arıcıların istediği fiyatı okumak, kesinleştirme talep etmek, sevk etmek ve sevkiyatları onaylamak;
- bir toptancıda veya perakendecide: sevkiyatları onaylamak;
- bir birlikte: herkese açık arıcı sayfası için arıcıların rızasını kaydetmek ve geri çekmek.

Kurumunuzun üyeliklerini ve yeteneklerini platform yöneticileri yönetir. Bir iş arkadaşınızın erişime veya farklı bir role ihtiyacı varsa onlara başvurun. Her kurumun bir **Uyum Sorumlusu** olmalıdır; küçük bir kurumda bu kişi, ikinci bir üyelikle siz olabilirsiniz (bkz. [bölüm 13](#role-co)).

## Kurum sayfanız {#orgadmin-org-page}

1. **Kurumum** sayfasını açın ve **Kurum sayfası** bağlantısına tıklayın.
2. Sayfa kurumunuzun durumunu, yeteneklerini, belgelerini, **Veri sorumlusu** bilgilerini, doğrulama geçmişini ve üyelerini gösterir.

Kurumun henüz bir Uyum Sorumlusu yoksa sayfada, bir Uyum Sorumlusunun atanması gereken tarihle birlikte şu yazar: "Bu kuruluşun henüz Uyum Sorumlusu yok. Her kuruluş en az bir kişi belirler (ikinci bir üyelikle kuruluş yöneticisi de olabilir)."

**Veri sorumlusu.** Kurumun aydınlatma metni, kurumu veri sorumlusu olarak adlandırır: **Tüzel kişi adı**, **İletişim (e-posta veya telefon)** ve **Adres**. "eksik", bilgilerin henüz girilmediği anlamına gelir. Bilgileri Uyum Sorumlusu veya bir platform yöneticisi güncel tutar.

<!-- SHOT: org-details -->
Figure: SHA-256 parmak izleriyle kurum belgeleri.

### Belge yükleme {#orgadmin-documents}

1. Belgeler bölümünde belge türünü seçin: **Ruhsat**, **Akreditasyon**, **Vergi levhası**, **Kayıt kanıtı** veya **Diğer**.
2. Dosyayı seçin: 10 MB'a kadar PDF ya da 8 MB'a kadar JPG, PNG veya WEBP.
3. **Yükle** düğmesine tıklayın: "Belge yüklendi."

Platform dosyayı SHA-256 parmak iziyle birlikte saklar. Belgeler silinemez. Yeni bir kurumun bir platform yöneticisi tarafından doğrulanabilmesi için en az bir belgesi olmalıdır.

## Kesinleştirme ve sevkiyatlar {#orgadmin-plant}

Bir tesiste, tıpkı Personel gibi bir parti için **Kesinleştirme talep et** işlemini yapabilir ve partiyi **Sevk et** ile sevk edebilirsiniz: bkz. [Kesinleştirme talebi](#staff-request-finalize) ve [Parti sevk etme](#staff-ship). Alıcı kurumda **Teslimi onayla** veya **Reddet** işlemlerini yapabilirsiniz: bkz. [Sevkiyatı onaylama veya reddetme](#staff-receive).

## Lotlarınızın istenen fiyatı {#orgadmin-price}

Arıcılar portallarında her teslim için kilogram başına bir istenen fiyat girer (bkz. [Teslimleriniz ve fiyatınız](#portal-price)). Kurumunuzun sahip olduğu bir lotun teslimat sayfasında **İstenen fiyat** kutusu güncel fiyatı ve geçmişini gösterir: "Arıcının kilogram başına istediği fiyat ve geçmişi. Diğer rollere, sonraki sahiplere veya herkese açık sayfalara gösterilmez." Fiyatı kurum yöneticisi, Uyum Sorumlusu ve platform yöneticileri okur; başka kimse okuyamaz. Bir birlik üyelerinin fiyatlarını yalnızca arıcı buna rıza verdiyse okur.

## Arıcı sayfaları (birlikler) {#orgadmin-storefronts}

Bir birlik, yazılı onay veren her üye arıcı için herkese açık bir **Arıcı sayfası** yayımlayabilir. Sayfa yalnızca doğrulanmış verileri gösterir: arıcının adı (yalnızca rızayla; aksi halde `BK-…` gibi nötr bir kod), il ve ilçe, bal türleri, doğrulanmış lotlar ve onaylı laboratuvar sonuçları ve balın kullanıldığı partiler. Sayfa hiçbir zaman telefon numarasını, kişinin fotoğrafını, doğum tarihini veya arılığın tam konumunu göstermez. Ziyaretçiler arıcıya birlik aracılığıyla ulaşır.

Sayfa yalnızca şu koşulların **tümü** sağlandığında herkese açıktır:

- arıcı etkindir;
- arıcının işletme kayıt numarası bir platform yöneticisi tarafından doğrulanmıştır;
- birliğiniz doğrulanmıştır;
- rıza, birliğiniz (arıcının şu anki birliği) tarafından kaydedilmiş ve geri çekilmemiştir.

### Rıza kaydetme {#orgadmin-consent}

1. Rıza metnini arıcıya okuyun ve formu imzalatın.
2. **Arıcı sayfaları** sayfasını açın ve arıcının yanındaki **Rıza kaydet** düğmesine tıklayın.
3. **Rıza metni arıcıya okundu ve imzalandı.** kutusunu işaretleyin.
4. **Arıcı adının gösterilmesine onay veriyor (aksi halde nötr bir kod gösterilir)** kutusunu yalnızca arıcı onay verdiyse işaretleyin.
5. **Rıza tarihi** alanını doldurun (ileri bir tarih olamaz) ve **İmzalı rıza belgesi** ekleyin (fotoğraf veya PDF).
6. Kaydedin.

<!-- SHOT: consent-form -->
Figure: Telefonda bir arıcının rızasını kaydetme.

Mesaj "Rıza kaydedildi; arıcı sayfası yayında." olur; kayıt numarası henüz doğrulanmadıysa "Rıza kaydedildi. Arıcı sayfası kayıt numarası doğrulanınca yayımlanır." olur.

**Yazdırılabilir kart.** **Yazdırılabilir kart** seçeneği, arıcının satış tezgâhı için arıcı sayfasının QR kodunu içeren bir kart yazdırır.

### Rızayı geri çekme {#orgadmin-withdraw}

1. **Arıcı sayfaları** sayfasında isterseniz **gerekçe (isteğe bağlı)** alanını doldurun ve **Rızayı geri çek** düğmesine tıklayın.
2. Sayfa hemen yayından kaldırılır: "Rıza geri çekildi; arıcı sayfası artık yayında değil."

Hatalar: "Bu arıcının etkin bir rızası var; yenisini kaydetmek için önce geri çekin.", "Rıza metninin arıcıya okunduğunu onaylayın.", "İmzalı rıza belgesini ekleyin."
