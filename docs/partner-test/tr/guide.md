# 1. Bu test hakkında {#about}

## TOP4HoneyChains nedir {#what}

TOP4HoneyChains, balı arıcının arılığından mağaza rafına kadar izleyen ve her adımın doğrulanabilir kaydını tutan bir web platformudur. Platform, ICT-AGRI-FOOD 2022 çağrısı kapsamında desteklenen TOP4HoneyChains projesinde ("Trustable and Sustainable Open Platform for Smart Honey Value Chains") geliştirilmektedir.

Neden önemli:

- **Kontrol edilebilir güven.** Her kaydı bir kişi girer, başka bir kişi onaylar (dört göz ilkesi). Her değişiklik, düzenlenemeyen bir denetim kaydına yazılır.
- **Zincirin tamamı tek yerde.** Arıcının kaydı, mühürlü kaplarla teslim, her kaptan alınan numune, laboratuvar analizi, tesisteki kontrol noktaları (KKN'ler), mühürlenen parti ve mağazaya sevkiyat.
- **Hile zorlaşır.** Mühürler, güvenlik kapakları ve SMS kodları her kaydı onu onaylayan kişilere bağlar. Kırık bir mühür veya açılmış bir kavanoz fark edilir.
- **Tüketici için bir hikâye.** Kavanozdaki QR kod Bal Hikâyesi'ni açar: balın nereden geldiği, limitleriyle birlikte onaylı laboratuvar sonuçları ve kontrol noktaları.
- **Arıcıya karşı adil.** Arıcı teslimlerini, sonuçlarını ve partilerini görür, istediği fiyatı girer ve kendisi hakkında neyin gösterileceğine karar verir.

Bu kılavuzla ekibiniz bunların hepsini bir demo ortamında deneyebilir: tek bir bal, arıcısının kaydından QR kodu tarayan tüketiciye kadar. Birkaç kişiyle yaklaşık iki üç saat sürer; tek bir kişi de bütün rolleri sırayla oynayabilir.

## Demo nasıl çalışır {#demo}

- **Demo sitesi:** {{demo.url}} (adresi koordinatörünüz teyit eder). Her sayfanın altındaki koyu şeritte **Demo ortamı – gerçek SMS gönderilmez** yazar. Gördüğünüz her şey **[DEMO]** işaretli test verisidir.
- **Hesaplar:** ülkenizin her rolü ve kurumu için bir hesap (bölüm 3). Şifreler ayrı bir sayfada, credentials.txt dosyasındadır; bu dosyayı kimseyle paylaşmayın.
- **Dil:** dili sağ üstteki bayrakla seçin (giriş sayfasında: TR EN ES PL LV).
- **SMS:** demo SMS göndermez. Kodlar ve giriş bilgileri **Demo SMS kutusu** sayfasında görünür (alt şeritteki bağlantı, yeni bir sekmede açılır). Etkin kurumunuzun mesajlarını ve sizin gönderdiğiniz mesajları görürsünüz.
- **İlk giriş:** platform aydınlatma metnini okuyup onaylamanızı ister. Kurumu adına imza atan roller (Kurum yöneticisi, Arıcı Kayıt Görevlisi, Kalite Güvence Sorumlusu, Laboratuvar müdürü, Uyum Sorumlusu) bir SMS koduyla onaylar: kodu gönderin, Demo SMS kutusunda okuyun ve girin. Ardından kısa bir tur gelir; **Turu atla** düğmesine tıklayın veya turu izleyin.
- **Ortak hesaplar:** ülkenizdeki diğer test kullanıcıları aynı hesapları aynı anda kullanır. Kayıtlarınıza tanınabilir bir ad verin (baş harfleriniz) ve başkalarının kayıtlarını değiştirmeyin.
- **Unutulan şifre:** demo SMS göndermediği için şifre sıfırlama çalışmaz. Koordinatörünüze başvurun.
- **Sıfırlama:** platform ekibi demoyu başlangıç durumuna döndürebilir. Bu durumda son sıfırlamadan sonra girilen her şey silinir.
- **Örnek bal:** ülkenizde tamamlanmış bir örnek zaten var: {{b1.name}} arıcısından {{example.batch}}. Bal Hikâyesi'ni şu adresten açın: {{example.story}}.

## Ortak ülkeler için demonun sınırları {#limits}

Platform önce Türkiye için geliştirildi. Arjantin, Polonya ve Letonya için demo en küçük güvenli geçici çözümleri kullanır; bir adımı değerlendirmeden önce lütfen bunları okuyun:

- **Kayıt numarası.** Platform Türkiye'nin işletme kayıt numarasını (TR, il kodu, 10 hane) kontrol eder. Demoda ortak ülkeler bir test numarası kullanır: **AR-DEMO-**, **PL-DEMO-** veya **LV-DEMO-** ve 6 hane. Bu numara ulusal bir sicille karşılaştırılmaz.
- **İller ve ilçeler.** Listelerde Türkiye'nin 81 ili vardır. Demo her ortak ülke için (AR), (PL) veya (LV) işaretli iki bölge ve birkaç ilçe ekler. Bunlar örnektir, resmî listeler değildir.
- **Köken haritası.** Bal Hikâyesi, arıcı sayfası ve panel kökenleri yalnızca Türkiye haritasında gösterir. Diğer ülkeler için sayfa ülkeyi ve bölgeyi metin olarak gösterir ("haritada yok"). Her ortak ülke için bir harita planlanan bir genişletmedir (A8).
- **İşletme numaraları.** Demoda kaydedilen arıcılar AR-BEE-DEMO-000001 gibi numaralar alır.
- **Metinler ve limitler.** Aydınlatma ve rıza metinleri Türkiye için yazılmış taslaklardır (**DEMO METİN – onaylı değil** işaretli). Laboratuvar parametreleri ve limitleri Türk bal mevzuatına dayanır; ülkeniz başka limitler kullanıyor olabilir. Bunu geri bildiriminizde belirtin.
- **Para birimi.** İstenen fiyat ARS, PLN, EUR, USD veya TRY olarak girilebilir.

# 2. Roller {#roles}

TOP4HoneyChains'te kimse tek başına işlem yapmaz: her işlem bir kurum adına, bir rolde yapılır. Aşağıda her rol için bir sayfa vardır: kim olduğu, ne yaptığı, hangi ekranları kullandığı ve hangi demo hesabının bu rolü oynadığı. Kullanım kılavuzu her rolü ayrıntılı olarak açıklar (bölüm numarası her kartta yazılıdır).

{{ROLECARDS}}

# 3. Demo hesaplarınız {#accounts}

{{COORDINATORNOTE}}

{{ACCOUNTS}}

# 4. Bir balı izleyin {#scenario}

Senaryo tek bir balı platform boyunca izler. Her adımda rol, demo hesabı, yapılacaklar ve görmeniz gerekenler yazılıdır. Bu sırayla çalışın: birçok adım bir önceki adımı bekler. Kontrol noktaları ve laboratuvar raporu için önerilen değerler [Tablo](#tbl-values) içindedir.

Her yerde karşılaşacağınız iki kural: **bir kaydı bir kişi girer, başka bir kişi onaylar** ve **bir lot veya parti ancak kendisinden önceki her şey onaylandığında ilerler**. Bir düğme devre dışıysa sayfa neyin eksik olduğunu söyler.

{{SCENARIO}}

# 5. Geri bildirim {#feedback}

Lütfen her adım için geri bildirim verin ve genel soruları yanıtlayın. Bu sayfaya yazabilir, etkileşimli kontrol listesini (bu paketteki checklist HTML dosyası; yanıtlarınızı dışa aktarabilir) kullanabilir veya geri bildirim şablonunu (CSV) doldurabilirsiniz. Sonucu koordinatörünüze gönderin.

{{FEEDBACK}}

# Ek: önerilen değerler {#values}

Bu değerler platformun limitleri içindedir. Platformun nasıl uyardığını ve nasıl teyit istediğini görmek için bir değeri limitinin dışında girmeyi deneyin.

{{VALUES}}
