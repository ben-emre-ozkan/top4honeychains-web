# 1. TOP4HoneyChains hakkında {#about}

TOP4HoneyChains, balı arıcının arılığından perakendecinin rafına kadar izleyen ve her adımın doğrulanabilir kaydını tutan bir web platformudur. TOP4HoneyChains projesi ("Akıllı Bal Değer Zincirleri için Güvenilir ve Sürdürülebilir Açık Platform") kapsamında geliştirilmiştir; proje, ICT-AGRI-FOOD 2022 çağrısı kapsamında ortak ülkelerin ulusal fonlama kuruluşları tarafından desteklenmektedir.

Bu kılavuz, platformu bugün çalıştığı haliyle, kurum modeli etkinken anlatır. Panele giriş yapan herkes ve platform yöneticileri için yazılmıştır. Hesap gerektirmeyen herkese açık sayfalar, ilgili oldukları yerlerde anlatılır.

## Bu kılavuz nasıl okunur {#how-to-read}

- **Bölüm 1–4** platformu, yetenekleri, kurumları ve rolleri, oturum açmayı ve bir bal partisinin baştan sona yolculuğunu anlatır. Önce bu bölümleri okuyun.
- **Bölüm 5–13** her rol için ayrı bir bölümdür. Her bölüm "Günlük işiniz" ile başlar, ardından her görevi adım adım açıklar. Kendi rolünüzün bölümünü okuyun.
- **Bölüm 14** arıcılar içindir: arıcı portalı.
- **Bölüm 15** platform yöneticileri içindir.
- **Bölüm 16** denetim kaydını, aydınlatma metinlerini ve rızaları, ayrıca herkesin okuyabildiği bilgileri açıklar.
- **Bölüm 17** bir sözlük ve sık karşılaşılan sorunlara yanıtlar içerir.

Düğme, menü ve alan adları ekranda göründüğü gibi **kalın** yazılmıştır. Sistemin gösterdiği mesajlar tırnak içinde verilmiştir. Şekiller ve tablolar numaralandırılmıştır; şekil ve tablo listeleri içindekiler tablosundan sonra gelir. Örneklerde adı **[DEMO]** ile başlayan test kayıtları kullanılır; bunlar gerçek kişi veya şirketler değildir.

> **Bu sürümde yeniden adlandırılan etiketler.** Ekrandaki bazı etiketler platformun bu sürümünde yeniden adlandırıldı; örneğin **Kalite Güvence Sorumlusu** rolü (önceden: "Kalite onaylayıcı"), **Temel kurallar** ve "… yapabilir" biçiminde yazılmış yetenek açıklamaları. Sözlükteki [Tablo](#tbl-renamed-labels), tüm eski ve yeni adları listeler.

> Bu kılavuz, ekranda olduğu gibi **KKN** (kritik kontrol noktası) kısaltmasını kullanır. İngilizce sürümde uluslararası CCP (critical control point) terimi geçer; diğer dil sürümleri de ekrandaki gibi kendi kısaltmalarını kullanır: PCC (İspanyolca), KPK (Lehçe), KKP (Letonca).

## Bal değer zinciri {#value-chain}

Bal beş istasyondan geçer. Platform her birini kayıt altına alır:

1. **Arılık**: arıcı balı üretir. Her arıcı platformda bir **Arıcı kaydı** ile temsil edilir; bu kaydı ilçe tarım müdürlüğü oluşturur, arıcı da SMS ile aldığı bir kodla onaylar. Arıcının **arıcı portalı** için kişisel bir hesabı da olabilir.
2. **Teslim**: arıcı balı kaplar içinde bir işleme tesisine teslim eder. Tesis her kabı ağırlığı ve mühürleriyle kaydeder, arıcı teslimi bir SMS koduyla onaylar ve teslim, kabulde kontrol edilen (KKN 0) bir *ham bal lotu* olur.
3. **Laboratuvar**: tesis her kaptan mühürlü bir numune kavanozu doldurur ve kavanozları bir gıda analiz laboratuvarına (genellikle akredite) gönderir. Laboratuvar her kavanozu kontrol eder, balı analiz eder; bir analist sonuçları girer, ikinci bir kişi olan laboratuvar müdürü de raporu onaylar.
4. **İşleme**: tesis balı işler (lotta KKN 2–3), lotları bir *parti* halinde birleştirir ve işleme, dolum, etiketleme, paketleme ve depolama kontrollerini kaydeder (partide KKN 4–10). Ardından iki kişi partiyi *kesinleştirir* (mühürler).
5. **Raf**: parti bir toptancıya veya perakendeciye sevk edilir. Alıcı teslimi onaylar ve kendi depolama koşullarını kaydeder.

Bu kılavuzda **veri**, kişilerin platforma kaydettiği bilgiler demektir: ölçülen değerler (örneğin % cinsinden nem) ve sorulara verilen yanıtlar (örneğin Evet / Hayır).

**Dört göz ilkesi.** Her veriyi bir kişi girer, farklı ikinci bir kişi onaylar; kimse kendi girdiğini onaylayamaz. İki "göz çifti", hata ve manipülasyon olasılığını büyük ölçüde azaltır. Kuralların tam listesi [Temel kurallar](#fixed-rules) bölümündedir. Her değişiklik ayrıca, üzerinde yapılan oynamaların fark edilebildiği bir denetim kaydına yazılır.

<!-- SHOT: intro-chain -->
Figure: Bal değer zincirinin beş istasyonu, herkese açık karşılama sayfasında gösterildiği haliyle.

## İzlenebilirlik {#traceability}

İzlenebilirlik, herhangi bir bal kavanozu için şu üç soruyu yanıtlayabilmek demektir:

- **Nereden geliyor?** Parti, ham bal lotlarını bilir; her lot da arılığını, arıcısını, teslimini ve laboratuvar raporunu bilir.
- **Kontrol edildi mi?** Lotun ve partinin her kritik kontrol noktası için veriler kaydedilmiştir; bunları kimin girdiği, kimin onayladığı ve ne zaman onaylandığı bellidir.
- **Nereye gitti?** Sevkiyatlar, partinin hangi miktarının hangi kurumda olduğunu kaydeder.

Bir sorun çıktığında **Geri çağırma raporu**, bir partinin veya lotun kaynağındaki tüm lotları ve sonrasındaki tüm sevkiyatları listeler (bkz. [Geri çağırma](#admin-recall)).

## Platform kimler için {#audience}

[Tablo](#tbl-audience), platformu kullanan herkesi listeler. Kurumlar ve rolleri [bölüm 2](#roles) içinde tanımlanır.

Table: Platform kimler için {#tbl-audience}

| Kim | Hesap | Platformda ne yapar |
|---|---|---|
| Arıcılar | bir **Arıcı kaydı** ile temsil edilir; isteğe bağlı olarak bir portal hesabı | Bal teslim eder ve teslimleri bir SMS koduyla onaylar; portalda: teslimlerini, numunelerini ve partilerini izler, istediği fiyatı girer, rızalarını verir veya geri çeker |
| İlçe tarım müdürlükleri | bir kurumun üyeleri | Arıcıları ve arılıklarını kaydeder, kimliklerini ve rızalarını tasdik eder, portal giriş bilgilerini gönderir |
| Arıcı birlikleri | bir kurumun üyeleri | Üye arıcılarını bağlar, herkese açık arıcı sayfası için rızalarını kaydeder |
| İşleme ve paketleme tesisleri | bir kurumun üyeleri | Teslimleri kaydeder, numune alır, kontrol noktalarını kaydeder, kesinleştirme talep eder ve onaylar, sevk eder |
| Gıda analiz laboratuvarları | bir kurumun üyeleri | Numuneleri teslim alır, sonuçları girer, raporları onaylar |
| Toptancılar | bir kurumun üyeleri | Sevkiyatları onaylar, depolama koşullarını kaydeder, üreticinin kayıtlarını okur |
| Perakendeciler | bir kurumun üyeleri | Sevkiyatları onaylar, depolama koşullarını kaydeder, üreticinin kayıtlarını okur |
| Kamu kurumları | bir kurumun üyeleri | Kayıtları okur (salt okunur) |
| Uyum Sorumluları | her kurumda bir rol | Kanıtları, numune alma kalitesini ve onayları inceler; kişisel verilerin korunmasına ilişkin talepleri ele alır |
| Platform yöneticileri | kullanıcı hesabında genel bir rol | Kurumları doğrular, üyelikleri yönetir, kontrol noktalarını ve limitleri tanımlar, aydınlatma metinlerini yayımlar, parti oluşturur, denetim kaydını okur |
| Tüketiciler ve diğer ziyaretçiler | yok | Herkese açık sayfaları okur: kavanozdaki QR koddan açılan Bal Hikâyesi, arıcı sayfaları, rapor doğrulama |

## Herkese açık Bal Hikâyesi ve QR kodlar {#honey-story}

Bir parti kesinleştiğinde platform, partinin herkese açık **Bal Hikâyesi** sayfasını ve bu sayfayı açan bir QR kod oluşturur. QR kod etikete basılabilir. Her ziyaretçi kodu hesap olmadan tarayabilir.

Bal Hikâyesi, ziyaretçinin sayfada seçtiği dilde şunları gösterir:

- partinin adı, miktarı ve üretim tarihi;
- balın yolculuğu: hasat, teslim, laboratuvar, işleme ve raf;
- balın kökeni bir harita üzerinde: şu anda Türkiye'nin illeri; diğer ortak ülkelerin haritaları planlanmaktadır;
- katkıda bulunan arıcılar: adıyla yalnızca arıcı buna rıza verdiyse, aksi halde nötr bir kodla; herkese açık bir arıcı sayfası varsa o sayfanın bağlantısıyla birlikte;
- laboratuvar sonuçları; limit içinde/limit dışında değerlendirmesi yalnızca doğrulanmış limitler için gösterilir;
- işleme ve paketlemenin kontrol noktaları;
- laboratuvar raporunu doğrulamak için bir bağlantı.

Bal Hikâyesi hiçbir zaman arıcının telefon numarasını, bir kişinin fotoğrafını, arılığın tam konumunu veya kişisel veri içerebilecek serbest metin notlarını göstermez. Bkz. [Herkesin okuyabildiği bilgiler](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: Kesinleşmiş bir partinin herkese açık Bal Hikâyesi, telefonda.

Diğer herkese açık sayfalar:

- **Arıcı sayfası** (`/storefront/<code>`): bir arıcının herkese açık sayfası; yalnızca arıcının imzalı rızası ve doğrulanmış kayıt numarasıyla yayımlanır.
- **Laboratuvar raporu doğrula** (`/verify-report`): herkes bir rapor kodu girerek onaylanmış bir raporun var olup olmadığını okuyabilir ve bir PDF'i parmak iziyle karşılaştırabilir.
- **Gizlilik ve rıza** (`/privacy`): arıcılar ve platform kullanıcıları için geçerli aydınlatma metinleri.
- **Karşılama sayfası** (`/`): projenin ana sayfası; oturum açma düğmesi buradadır.
