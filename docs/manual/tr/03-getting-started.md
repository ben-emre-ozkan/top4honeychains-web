# 3. Başlarken {#getting-started}

## Oturum açma {#sign-in}

1. Platform adresini açın ve karşılama sayfasında **Giriş yap** düğmesine tıklayın ya da doğrudan `/Account/Login` adresine gidin.
2. **Kullanıcı Adı** (e-posta adresiniz değil, kullanıcı adınız) ve **Şifre** alanlarını doldurun.
3. Tarayıcıyı kapattıktan sonra da oturumunuzun açık kalmasını istiyorsanız **Beni hatırla** kutusunu işaretleyin. Ortak kullanılan bir bilgisayarda bu kutuyu boş bırakın.
4. **Giriş Yap** düğmesine tıklayın.

<!-- SHOT: sign-in -->
Figure: Oturum açma sayfası.

Oturum açtıktan sonra sizi bekleyen görevlerin listesi olan **İşlerim** sayfası açılır. Arıcılar ise arıcı portalına yönlendirilir (bkz. [bölüm 14](#portal)).

**Kimler oturum açabilir?** Platform yöneticisi rolüne ya da en az bir etkin üyeliğe (bir kurum × bir rol) sahip olmanız gerekir. Üyeliği bir platform yöneticisi verir (bkz. [Üyeler](#admin-members)). Arıcılar, kayıt görevlilerinin kendilerine gönderdiği bir portal hesabıyla oturum açar.

**Sık görülen mesajlar**

- "Kullanıcı adı veya şifre hatalı. Çok sayıda başarısız denemeden sonra hesap bir süre kilitlenir." Kullanıcı adı veya şifre yanlıştır ya da hesap kilitlenmiştir. **5 başarısız denemeden sonra hesap 15 dakika kilitlenir**. Bekleyip yeniden deneyin ya da **Şifrenizi mi unuttunuz?** bağlantısını kullanın.
- "Bu hesabın henüz panele erişimi yok: hiçbir kurumun üyesi değil. …" Hesabınızın etkin bir üyeliği yoktur. Bir platform yöneticisinden sizi kurumunuza eklemesini isteyin.

Oturumu kapatmak için sağ üstteki avatarınızın menüsünü açın ve **Çıkış Yap** seçeneğini seçin.

### İlk oturum açışınız {#first-sign-in}

Yeni hesapların giriş bilgileri **SMS** ile gelir: kullanıcı adı ve **72 saat** geçerli, 10 karakterlik geçici bir şifre. Bu şifreyi sizden başka kimse görmez. (Bir platform yöneticisi bunun yerine bir ilk şifre de belirleyip size yüz yüze verebilir.)

1. Kullanıcı adı ve geçici şifreyle oturum açın.
2. **Şifrenizi belirleyin** sayfası açılır: "Geçici bir şifreyle giriş yaptınız. Devam etmeden önce kendi şifrenizi belirleyin."
3. **Yeni Parola** alanını iki kez doldurun ve **Parolayı Güncelle** düğmesine tıklayın.

Şifre **en az 10 karakter** olmalı, kullanıcı adınız olmamalı ve yaygın bir şifre olmamalıdır. Geçici şifreyle oturum açmak cep telefonu numaranızı da doğrular. 72 saat geçtiyse yeni giriş bilgileri isteyin: bunları platform yöneticisi (üyeler) veya kayıt görevlisi (arıcılar) yeniden gönderir.

## Şifrenizi unuttuysanız {#password-reset}

Şifre, **doğrulanmış** cep telefonunuza SMS ile gönderilen bir kodla sıfırlanır:

1. Oturum açma sayfasında **Şifrenizi mi unuttunuz?** bağlantısına tıklayın.
2. **Kullanıcı Adı** alanını doldurun ve **Kod gönder** düğmesine tıklayın. Sayfa her durumda şu yanıtı verir: "Hesap varsa ve doğrulanmış bir cep telefonu varsa SMS ile bir kod gönderildi." (Hangi hesapların var olduğunu açığa vurmaz.)
3. **SMS'teki kod (6 hane)** alanını, ardından **Yeni Parola** alanını iki kez doldurun ve onaylayın. Kod **5 dakika** geçerlidir; **Yeni kod gönder** bir dakika sonra yeni bir kod gönderir.

"Doğrulanmış cep telefonunuz yok mu? Giriş bilgilerinizi veren kişiden yenilerini göndermesini isteyin." Bir kod 3 kez denenebilir; bir hesaba saatte en çok 3 sıfırlama kodu gönderilir.

<!-- SHOT: forgot-password -->
Figure: Şifre sıfırlama: SMS'teki kod ve yeni şifre.

(Platform şifrelerin e-postayla sıfırlanacağı şekilde yapılandırılmışsa sayfa bunun yerine **E-posta** adresini ister ve bir gün geçerli bir bağlantı gönderir.)

## Profiliniz {#profile}

Avatar menüsünü açın ve **Profil** seçeneğini seçin.

- **Kullanıcı Adı** ve **E-posta** gösterilir, ancak burada değiştirilemez. Bir platform yöneticisine başvurun.
- **Telefon Numarası** numaranın **doğrulandı** olup olmadığını gösterir. Değiştirmek için yeni numarayı girip **Kod gönder** düğmesine tıklayın, ardından **SMS'teki kod (6 hane)** alanını doldurup **Onayla** düğmesine tıklayın. "Yeni numara, ona gönderdiğimiz kodu girdikten sonra kullanılır. Doğrulanmış numara şifrenizi SMS ile sıfırlamanızı sağlar."
- **Parolayı Değiştir** bir pencere açar: **Mevcut Parola**, **Yeni Parola** ve **Parolayı onaylayın** alanlarını doldurun, ardından **Parolayı Güncelle** düğmesine tıklayın.

Ortak kullanılan bir bilgisayardan ayrılırken **Çıkış Yap** seçeneğini kullanın; ekran kilitleme özelliği yoktur.

<!-- SHOT: profile -->
Figure: Telefon doğrulaması ve **Parolayı Değiştir** penceresiyle profil sayfası.

## Aydınlatma metni {#notice-accept}

Platformda çalışmaya başlamadan önce **aydınlatma metnini** okursunuz: kişisel verilerinizin platform ve çalıştığınız her kurum tarafından nasıl işlendiği ve **veri sorumlusunun** kim olduğu (bkz. [Aydınlatma metinleri, rızalar ve veri sorumlusu](#trust-notices)).

1. Sizi bekleyen bir aydınlatma metni varsa bir şerit şunu yazar: "Lütfen aydınlatma metnini okuyun ve okuduğunuzu onaylayın." Şeride tıklayın. (Platform bunu zorunlu kılacak şekilde yapılandırılmışsa aydınlatma metni sayfası oturum açtıktan sonra kendiliğinden açılır ve siz onaylayana kadar panel kapalı kalır.)
2. Metni okuyun. Her kurumun aydınlatma metni, veri sorumlusunu ve **Başvuru için iletişim** bilgisini belirtir.
3. **Yukarıdaki aydınlatma metnini okudum.** kutusunu işaretleyin ve **Onayla** düğmesine tıklayın.
4. Kurumu adına imza yetkisi taşıyan roller (Kurum yöneticisi, Uyum Sorumlusu, Arıcı Kayıt Görevlisi, Kalite Güvence Sorumlusu, Laboratuvar müdürü) bir kodla onaylar: "Rolünüz kuruluşunuz adına imza yetkisi taşıdığı için cep telefonunuza gönderilen bir kodla onaylarsınız." **Kod gönder** düğmesine tıklayın ve **SMS'teki kod (6 hane)** alanını doldurun.

Bir aydınlatma metninin yeni bir sürümü yayımlandığında sizden yeniden onay istenir. "Güncel aydınlatma metinlerini kabul ettiniz. Başka bir işlem gerekmiyor." yazıyorsa her şey günceldir.

<!-- SHOT: notice-accept -->
Figure: Aydınlatma metnini SMS koduyla onaylama.

## Dil seçimi {#language}

Platform Türkçe, İngilizce, İspanyolca, Lehçe ve Letonca kullanılabilir. Varsayılan dil Türkçedir.

- Panelde üst çubuktaki bayrak düğmesini kullanın. (Dar bir telefon ekranında bayrak düğmesi gizlenir; bu durumda dili oturum açma sayfasında değiştirin.)
- Oturum açma sayfalarında sağ üstteki **TR EN ES PL LV** bağlantılarını kullanın.
- Herkese açık sayfalarda üst çubuktaki dil bağlantılarını kullanın.

Seçiminiz tarayıcınızda bir yıl boyunca hatırlanır. Referans verilerinden gelen adlar (bal türleri, kontrol noktaları, roller) da aynı dili izler; çevirisi olmayan yerlerde İngilizce metin gösterilir.

## Üst çubuk ve rolünüz {#top-bar}

Üst çubukta soldan sağa şunlar yer alır:

- menü düğmesi (telefonlarda) ve herkese açık karşılama sayfasını açan TOP4HoneyChains logosu;
- **kurum · rol etiketi**: şu anda hangi kurumda ve hangi rolde çalıştığınız;
- dil düğmesi;
- tam ekran düğmesi;
- **Profil**, **Turu göster**, **Animasyonları göster** anahtarı ve **Çıkış Yap** seçenekleriyle avatarınız.

<!-- SHOT: top-bar-role-chip -->
Figure: Kurum · rol etiketiyle üst çubuk.

Kurumunuz doğrulanmışsa etikette bir onay işareti, doğrulama beklerken bir ünlem işareti, askıya alınmışsa kırmızı zemin üzerinde bir dur işareti görünür. Kurum doğrulanmadığı sürece üyeleri verilerini okuyabilir, kurum yöneticisi de belge yükleyebilir; ancak **kimse veri giremez**. Bir uyarı şeridinde şu yazar: "Kurumunuz henüz doğrulanmadı. …"; askıya alınmış bir kurum için ise "Kurumunuz askıya alındı. Verilerini okuyabilirsiniz, ancak bir platform yöneticisi yeniden etkinleştirene kadar kimse veri giremez."

Birden fazla üyeliğiniz varsa aralarında geçiş yapmak için etikete tıklayın; bkz. [Birden fazla üyelik](#memberships).

<!-- SHOT: role-switch -->
Figure: Üyelik değiştirme: etiketin **Rolleriniz** listesi.

## Tur ve animasyonlar {#animations}

**İlk kullanım turu.** Bir rolde ilk kez çalıştığınızda kısa bir tur (**Kısa bir tur**, 3 ila 5 adım), o rolün menü öğelerini birer satırla gösterir. **İleri** ve **Geri** ile turda ilerlersiniz; **Turu atla** veya Esc turu kapatır. Platform, turu gördüğünüzü kullanıcı ve rol başına, başka bir bilgisayarda veya telefonda da hatırlar. Avatar menüsündeki **Turu göster** turu yeniden açar.

**Açıklayıcı animasyonlar.** Birkaç yerde en fazla bir buçuk saniyelik kısa bir sahne, bir düğmenin ne yaptığını veya az önce ne olduğunu tek satırlık bir açıklamayla gösterir: örneğin numune aldığınızda bir numune kavanozunun kapağının bastırılması, bir teslim kaydedildiğinde bir kamyon, kapanan bir mühür, laboratuvara giden kavanozlar, kabuldeki kontrol ve yola çıkan bir sevkiyat. Bir sahne sayfayı hiçbir zaman engellemez: bir tuşa basmak, tıklamak veya Esc onu hemen bitirir ve son karesi durağan bir resim olarak kalır. ⓘ **Ne olduğunu göster** düğmesi sahneyi yeniden oynatır.

<!-- SHOT: scene-static -->
Figure: Açıklayıcı bir sahnenin (numune kavanozu) açıklamasıyla birlikte durağan resmi.

Avatar menüsündeki **Animasyonları göster**, hesabınız için sahneleri kapatır veya açar ("Bir düğmenin ne yaptığını ve az önce ne olduğunu gösteren kısa sahneler. Kapalıyken durağan bir resim gösterilir."). Cihazınız azaltılmış hareket istiyorsa yalnızca durağan resimler gösterilir.

## İşlerim {#my-work}

**İşlerim** ana sayfanızdır ([Şekil](#fig-my-work)). Etkin üyelikte sizi bekleyen her şeyi türüne göre gruplar ve en eskisinden başlayarak listeler ([Tablo](#tbl-mywork-groups)). Her grubun üst kısımda bir kutucuğu vardır; kutucuğa tıklayınca o gruba gidersiniz.

<!-- SHOT: my-work -->
Figure: Bir Kalite Güvence Sorumlusunun **İşlerim** sayfası, grup kutucuklarıyla.

Table: **İşlerim** sayfasındaki gruplar {#tbl-mywork-groups}

| Grup | Anlamı | Kime düşer |
|---|---|---|
| **Girilecek değerler** | Bir lotun veya partinin bir kontrol noktası (KKN) için veri girilmesi gerekiyor | O KKN'ye veri giren roller |
| **Onaylanacak değerler** | Bir KKN kaydı veya bir KKN 10 depolama kaydı gönderildi ya da onay almadan tamamlandı veya "Uygulanamaz" olarak işaretlendi ve ikinci bir kişinin onayını bekliyor. **İşlerim**, kendi girdiğiniz veya işaretlediğiniz kayıtları hiçbir zaman listelemez. | O KKN'yi onaylayan roller |
| **Kesinleştirme talepleri** | Biri bir partinin kesinleştirilmesini talep etti | Parti sahibi kurumun Kalite Güvence Sorumlusu |
| **Teslim alınacak numuneler** | Laboratuvarınıza bir numune kavanozu gönderildi | Analist, Laboratuvar müdürü |
| **Girilecek laboratuvar sonuçları** | Teslim alınan bir numunenin henüz raporu yok ya da rapor geri gönderildi | Analist |
| **Onaylanacak laboratuvar raporları** | Bir rapor onaya gönderildi | Laboratuvar müdürü |
| **Onaylanacak sevkiyatlar** | Kurumunuza gelen bir sevkiyat bekliyor | Alıcı kurumun Personel ve Kurum yöneticisi rolleri |
| **İmzalı fotoğrafları incele** | Bir kayıt veya teslim, SMS kodu yerine imzalı bir fotoğrafla onaylandı | Kaydı yapan kurumun Uyum Sorumlusu (kaydı yapan kişi hiçbir zaman) |
| **İncelemeler** | Fotoğrafla onaylanan bir teslim, reddedilen bir numune veya kırık bir mühür, numune alma kalitesi incelemesini bekliyor | Uyum Sorumlusu |
| **İlgili kişi başvuruları** | Bir kişinin kendi verileriyle ilgili bir başvurusu açık (30 gün içinde yanıtlanmalı) | Uyum Sorumlusu |
| **Doğrulanacak kurumlar** | Bir kurum belgelerini yükledi ve doğrulanmayı bekliyor | Platform yöneticisi (birlikler için, yapılandırılmışsa merkez birlik) |
| **Doğrulanacak kayıt numaraları** | Bir arıcının işletme kayıt numarası doğrulanmayı bekliyor | Platform yöneticisi |
| **Karar bekleyen düzeltmeler** | Başka bir yönetici KKN verilerinin veya teslim bilgilerinin düzeltilmesini talep etti | Platform yöneticisi |

Her öğe bir başlık (örneğin "KKN 0 · #20070" ve **lot**, **parti** ya da **depolama** sözcüğü), ne kadar süredir beklediği (üç günden eski öğeler turuncu bir işaret alır) ve daha önce görmediyseniz bir **Yeni** etiketi gösterir. Bir laboratuvar öğesi neden beklemesi gerektiğini de gösterebilir, örneğin: "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; laboratuvar sonuçları ancak bundan sonra girilip onaylanabilir."

Menüdeki **İşlerim** bağlantısında bekleyen öğe sayısını gösteren bir rozet bulunur. Sayfa açıkken her dakika yeni iş olup olmadığını kontrol eder; yeni bir şey geldiğinde bağlantı bir kez parlar ve küçük bir bildirim, büyüyen grupları listeler. Bekleyen bir şey yoksa sayfada "Sizi bekleyen bir iş yok." yazar ve açabileceğiniz sayfaların bağlantıları sunulur.

## Menü {#menu}

Kurum üyelerinin kısa bir menüsü vardır ([Tablo](#tbl-menu)). Her öğe yalnızca kurumunuz ilgili yeteneğe ve rolünüz ilgili yetkiye sahipse görünür; sayfaları adresleriyle açtığınızda da aynı yetkiler kontrol edilir:

Table: Kurum üyelerinin menüsü {#tbl-menu}

| Menü öğesi | Açtığı sayfa | Ne zaman görünür |
|---|---|---|
| **İşlerim** | Görevleriniz | Her zaman |
| **Kurumum** | Kurumunuzun lotları ve partileri | Her zaman |
| **Teslim almalar** | Kaydedilmiş teslimler | İşleme kurumu; teslim alma okuma yetkisi olan roller |
| **Numuneler ve laboratuvar** | Gönderilen ve teslim alınan numuneler, laboratuvar raporları | İşleme veya laboratuvar kurumu; numune okuma yetkisi olan roller |
| **Sevkiyatlar ve stok** | Elinizdeki partiler, gelen ve giden sevkiyatlar, depolama kayıtları | İşleme, paketleme, toptan satış veya perakende kurumu; sevkiyat okuma yetkisi olan roller |
| **Arıcı sayfaları** | Rızalar ve herkese açık arıcı sayfaları | Birlik; arıcı sayfası okuma yetkisi olan roller |
| **Arıcı kaydı** | Müdürlüğünüzün kayıtları | Arıcı kaydı kurumu; Arıcı Kayıt Görevlisi |
| **Uyum** | İncelemeler, imzalı fotoğraflar, başvurular, onaylar, sözleşmeler, denetim kaydı girişleri | Uyum Sorumlusu |

Platform yöneticileri yönetim menüsünün tamamını kullanır (bkz. [bölüm 15](#admin)).

## Mesajlar, yükleme göstergesi ve hata sayfaları {#messages}

- **Mesajlar** üst köşede görünür. Başarı ve bilgi mesajları birkaç saniye sonra kapanır; hata mesajları siz kapatana kadar kalır. Bir rolden söz eden mesaj, rolü kodu ile değil adıyla anar.
- **Yükleme göstergesi** (hareketli bir bal arısı), bir sayfanın açılması veya bir kaydın tamamlanması biraz uzun sürdüğünde görünür.
- **Hata sayfaları**: "Bu sayfayı açamazsınız" (403), rolünüzün bu sayfaya izin vermediği anlamına gelir; kurum yöneticinize başvurun. "Bu sayfa bulunamadı" (404), adresin mevcut olmadığı ya da kaydın size görünür olmadığı anlamına gelir. 500 sayfası kısa bir başvuru kodu gösterir; sorunu bildirirken bu kodu belirtin. Her hata sayfasında **İşlerim** sayfasına dönen bir düğme ve bir **Geri dön** düğmesi bulunur.
