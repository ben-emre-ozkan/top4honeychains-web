# 1. TOP4HoneyChains hakkında {#about}

TOP4HoneyChains, balı arıcının arılığından market rafına kadar izleyen ve her adımın doğrulanabilir kaydını tutan bir web platformudur. TÜBİTAK TOP4HoneyChains projesi ("Akıllı Bal Değer Zincirleri için Güvenilir ve Sürdürülebilir Açık Platform") kapsamında geliştirilmiştir.

Bu kılavuz, platformu bugün çalıştığı haliyle, kurum modeli etkinken anlatır. Panele giriş yapan herkes ve platform yöneticileri için yazılmıştır. Hesap gerektirmeyen herkese açık sayfalar, ilgili oldukları yerlerde anlatılır.

## Bu kılavuz nasıl okunur {#how-to-read}

- **Bölüm 1–4** platformu, yetenekleri, kurumları ve rolleri, oturum açmayı ve bir bal partisinin baştan sona yolculuğunu anlatır. Önce bu bölümleri okuyun.
- **Bölüm 5–11** her rol için ayrı bir bölümdür. Her bölüm "Günlük işiniz" ile başlar, ardından her görevi adım adım açıklar. Kendi rolünüzün bölümünü okuyun.
- **Bölüm 12** platform yöneticileri içindir.
- **Bölüm 13** denetim kaydını ve kamuya açık bilgileri açıklar.
- **Bölüm 14** bir sözlük ve sık karşılaşılan sorunlara yanıtlar içerir.

Düğme, menü ve alan adları ekranda göründüğü gibi **kalın** yazılmıştır. Sistemin gösterdiği mesajlar tırnak içinde verilmiştir. Örneklerde adı **[DEMO]** ile başlayan test kayıtları kullanılır; bunlar gerçek kişi veya şirketler değildir.

> Bu kılavuz, ekranda olduğu gibi **KKN** (kritik kontrol noktası) kısaltmasını kullanır. İngilizce sürümde uluslararası CCP (critical control point) terimi geçer; diğer dil sürümleri de ekrandaki gibi kendi kısaltmalarını kullanır: PCC (İspanyolca), KPK (Lehçe), KKP (Letonca).

## Bal değer zinciri {#value-chain}

Bal beş istasyondan geçer. Platform her birini kayıt altına alır:

1. **Arılık**: arıcı balı üretir. Arıcılar sisteme giriş yapmaz; platformun ve birliklerinin tuttuğu kayıtlar olarak yer alırlar.
2. **Teslim**: arıcı balı bir işleme tesisine teslim eder. Tesis teslimi kaydeder; bu kayıt bir *ham bal lotu* oluşturur. Tesis balı kabulde kontrol eder (KKN 0).
3. **Laboratuvar**: tesis bir numuneyi mühürleyip laboratuvara gönderir. Laboratuvar analiz sonuçlarını girer, laboratuvardaki ikinci bir kişi de raporu onaylar.
4. **İşleme**: tesis balı işler (lotta KKN 2–3), lotları bir *parti* halinde birleştirir ve işleme, dolum, etiketleme, paketleme ve depolama kontrollerini kaydeder (partide KKN 4–10). Ardından iki kişi partiyi *kesinleştirir* (mühürler).
5. **Raf**: parti bir toptancıya veya mağazaya sevk edilir. Alıcı teslimi onaylar ve kendi depolama koşullarını kaydeder.

Her değeri bir kişi girer, başka bir kişi onaylar (*dört göz ilkesi*). Her değişiklik, üzerinde yapılan oynamaların fark edilebildiği bir denetim kaydına yazılır.

<!-- SHOT: intro-chain -->

## İzlenebilirlik {#traceability}

İzlenebilirlik, herhangi bir bal kavanozu için şu üç soruyu yanıtlayabilmek demektir:

- **Nereden geliyor?** Parti, ham bal lotlarını bilir; her lot da arılığını, arıcısını, teslimini ve laboratuvar raporunu bilir.
- **Kontrol edildi mi?** Lotun ve partinin her kritik kontrol noktası için değerler kaydedilmiştir; bunları kimin girdiği, kimin onayladığı ve ne zaman onaylandığı bellidir.
- **Nereye gitti?** Sevkiyatlar, partinin hangi miktarının hangi kurumda olduğunu kaydeder.

Bir sorun çıktığında **Geri çağırma raporu**, bir partinin veya lotun kaynağındaki tüm lotları ve sonrasındaki tüm sevkiyatları gösterir (bkz. [Geri çağırma](#admin-recall)).

## Platform kimler için {#audience}

| Kim | Platformda ne yapar |
|---|---|
| Arıcı birlikleri | Üye arıcılarını bağlar, herkese açık arıcı sayfası için arıcıların rızasını kaydeder |
| İşleme ve paketleme tesisleri | Teslimleri kaydeder, numune alır, kontrol noktalarını kaydeder, kesinleştirme talep eder ve onaylar, sevk eder |
| Laboratuvarlar | Numuneleri teslim alır, sonuçları girer, raporları onaylar |
| Toptancılar ve mağazalar | Sevkiyatları onaylar, depolama koşullarını kaydeder, üreticinin kayıtlarını okur |
| Kamu kurumları | Kayıtları okur (salt okunur) |
| Platform yöneticileri | Kurumları doğrular, üyeleri yönetir, kontrol noktalarını ve limitleri tanımlar, parti oluşturur, denetim kaydını tutar |
| Tüketiciler | Kavanozdaki QR koddan herkese açık Bal Hikâyesi sayfasını açar |

## Herkese açık Bal Hikâyesi ve QR kodlar {#honey-story}

Bir parti kesinleştiğinde platform, partinin herkese açık **Bal Hikâyesi** sayfasını ve bu sayfayı açan bir QR kod oluşturur. QR kod etikete basılabilir. Herkes kodu hesap olmadan tarayabilir.

Bal Hikâyesi, ziyaretçinin dilinde şunları gösterir:

- partinin adı, miktarı ve üretim tarihi;
- balın yolculuğu: hasat, teslim, laboratuvar, işleme ve raf;
- balın il bazında kökeni, Türkiye haritası üzerinde;
- katkıda bulunan arıcılar; arıcı herkese açık bir sayfaya onay verdiyse o sayfanın bağlantısıyla birlikte;
- laboratuvar sonuçları; limit içinde/limit dışında değerlendirmesi yalnızca doğrulanmış limitler için gösterilir;
- işleme ve paketlemenin kontrol noktaları;
- laboratuvar raporunu doğrulamak için bir bağlantı.

Bal Hikâyesi hiçbir zaman arıcının telefon numarasını, bir kişinin fotoğrafını, arılığın tam konumunu veya kişisel veri içerebilecek serbest metin notlarını göstermez. Bkz. [Kamuya açık bilgiler](#trust-public).

<!-- SHOT: public-honey-story -->

Diğer herkese açık sayfalar:

- **Arıcı sayfası** (`/storefront/<code>`): bir arıcının herkese açık sayfası; yalnızca arıcının imzalı rızası ve doğrulanmış kayıt numarasıyla yayımlanır.
- **Laboratuvar raporu doğrula** (`/verify-report`): herkes bir rapor kodu girerek raporun var olup olmadığını ve onaylanıp onaylanmadığını görebilir, bir PDF'i parmak iziyle karşılaştırabilir.
- **Karşılama sayfası** (`/`): projenin ana sayfası; oturum açma düğmesi buradadır.
