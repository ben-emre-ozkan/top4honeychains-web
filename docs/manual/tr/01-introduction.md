# 1. TOP4HoneyChains hakkında {#about}

TOP4HoneyChains, balı arıcının arılığından perakendecinin rafına kadar izleyen ve her adımın doğrulanabilir kaydını tutan bir web platformudur. TOP4HoneyChains projesi ("Akıllı Bal Değer Zincirleri için Güvenilir ve Sürdürülebilir Açık Platform") kapsamında geliştirilmiştir; proje, ICT-AGRI-FOOD 2022 çağrısı kapsamında ortak ülkelerin ulusal fonlama kuruluşları tarafından desteklenmektedir.

Bu kılavuz, platformu bugün çalıştığı haliyle, kurum modeli etkinken anlatır. Panele giriş yapan herkes ve platform yöneticileri için yazılmıştır. Hesap gerektirmeyen herkese açık sayfalar, ilgili oldukları yerlerde anlatılır.

## Bu kılavuz nasıl okunur {#how-to-read}

- **Bölüm 1–4** platformu, yetenekleri, kurumları ve rolleri, oturum açmayı ve bir bal partisinin baştan sona yolculuğunu anlatır. Önce bu bölümleri okuyun.
- **Bölüm 5–11** her rol için ayrı bir bölümdür. Her bölüm "Günlük işiniz" ile başlar, ardından her görevi adım adım açıklar. Kendi rolünüzün bölümünü okuyun.
- **Bölüm 12** platform yöneticileri içindir.
- **Bölüm 13** denetim kaydını ve herkesin okuyabildiği bilgileri açıklar.
- **Bölüm 14** bir sözlük ve sık karşılaşılan sorunlara yanıtlar içerir.

Düğme, menü ve alan adları ekranda göründüğü gibi **kalın** yazılmıştır. Sistemin gösterdiği mesajlar tırnak içinde verilmiştir. Şekiller ve tablolar numaralandırılmıştır; şekil ve tablo listeleri içindekiler tablosundan sonra gelir. Örneklerde adı **[DEMO]** ile başlayan test kayıtları kullanılır; bunlar gerçek kişi veya şirketler değildir.

> **Bir sonraki sürümde değişecek etiketler.** Ekrandaki bazı etiketler platformun bir sonraki sürümünde yeniden adlandırılacaktır. Bu kılavuz yeni adları şimdiden kullanır; örneğin **Kalite Güvence Sorumlusu** rolü (bugün: "Kalite onaylayıcı") ve "… yapabilir" biçiminde yazılmış yetenek açıklamaları. Ekran görüntüleri bugünkü etiketleri gösterir. Sözlükteki [Tablo](#tbl-renamed-labels), tüm eski ve yeni adları listeler.

> Bu kılavuz, ekranda olduğu gibi **KKN** (kritik kontrol noktası) kısaltmasını kullanır. İngilizce sürümde uluslararası CCP (critical control point) terimi geçer; diğer dil sürümleri de ekrandaki gibi kendi kısaltmalarını kullanır: PCC (İspanyolca), KPK (Lehçe), KKP (Letonca).

## Bal değer zinciri {#value-chain}

Bal beş istasyondan geçer. Platform her birini kayıt altına alır:

1. **Arılık**: arıcı balı üretir. Arıcılar sisteme giriş yapmaz ve hesapları yoktur; her arıcı platformda, platform yöneticilerinin ve arıcının birliğinin güncel tuttuğu bir **Arıcı kaydı** ile temsil edilir.
2. **Teslim**: arıcı balı bir işleme tesisine teslim eder. Tesis teslimi kaydeder; bu kayıt bir *ham bal lotu* oluşturur. Tesis balı kabulde kontrol eder (KKN 0).
3. **Laboratuvar**: tesis bir numuneyi mühürleyip bir gıda analiz laboratuvarına (genellikle akredite) gönderir. Laboratuvarda bir analist analiz sonuçlarını girer, ikinci bir kişi olan laboratuvar müdürü de raporu onaylar.
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
| Arıcılar | yok; bir **Arıcı kaydı** ile temsil edilir | Bal teslim eder; birlikleri, herkese açık arıcı sayfası için rızalarını kaydeder |
| Arıcı birlikleri | bir kurumun üyeleri | Üye arıcılarını bağlar, herkese açık arıcı sayfası için rızalarını kaydeder |
| İşleme ve paketleme tesisleri | bir kurumun üyeleri | Teslimleri kaydeder, numune alır, kontrol noktalarını kaydeder, kesinleştirme talep eder ve onaylar, sevk eder |
| Gıda analiz laboratuvarları | bir kurumun üyeleri | Numuneleri teslim alır, sonuçları girer, raporları onaylar |
| Toptancılar | bir kurumun üyeleri | Sevkiyatları onaylar, depolama koşullarını kaydeder, üreticinin kayıtlarını okur |
| Perakendeciler | bir kurumun üyeleri | Sevkiyatları onaylar, depolama koşullarını kaydeder, üreticinin kayıtlarını okur |
| Kamu kurumları | bir kurumun üyeleri | Kayıtları okur (salt okunur) |
| Platform yöneticileri | kullanıcı hesabında genel bir rol | Kurumları doğrular, üyelikleri yönetir, kontrol noktalarını ve limitleri tanımlar, parti oluşturur, denetim kaydını okur |
| Tüketiciler ve diğer ziyaretçiler | yok | Herkese açık sayfaları okur: kavanozdaki QR koddan açılan Bal Hikâyesi, arıcı sayfaları, rapor doğrulama |

## Herkese açık Bal Hikâyesi ve QR kodlar {#honey-story}

Bir parti kesinleştiğinde platform, partinin herkese açık **Bal Hikâyesi** sayfasını ve bu sayfayı açan bir QR kod oluşturur. QR kod etikete basılabilir. Her ziyaretçi kodu hesap olmadan tarayabilir.

Bal Hikâyesi, ziyaretçinin sayfada seçtiği dilde şunları gösterir:

- partinin adı, miktarı ve üretim tarihi;
- balın yolculuğu: hasat, teslim, laboratuvar, işleme ve raf;
- balın kökeni bir harita üzerinde: şu anda Türkiye'nin illeri; diğer ortak ülkelerin haritaları planlanmaktadır;
- katkıda bulunan arıcılar; arıcı herkese açık bir sayfaya onay verdiyse o sayfanın bağlantısıyla birlikte;
- laboratuvar sonuçları; limit içinde/limit dışında değerlendirmesi yalnızca doğrulanmış limitler için gösterilir;
- işleme ve paketlemenin kontrol noktaları;
- laboratuvar raporunu doğrulamak için bir bağlantı.

Bal Hikâyesi hiçbir zaman arıcının telefon numarasını, bir kişinin fotoğrafını, arılığın tam konumunu veya kişisel veri içerebilecek serbest metin notlarını göstermez. Bkz. [Herkesin okuyabildiği bilgiler](#trust-public).

<!-- SHOT: public-honey-story -->
Figure: Kesinleşmiş bir partinin herkese açık Bal Hikâyesi, telefonda.

Diğer herkese açık sayfalar:

- **Arıcı sayfası** (`/storefront/<code>`): bir arıcının herkese açık sayfası; yalnızca arıcının imzalı rızası ve doğrulanmış kayıt numarasıyla yayımlanır.
- **Laboratuvar raporu doğrula** (`/verify-report`): herkes bir rapor kodu girerek raporun var olup olmadığını ve onaylanıp onaylanmadığını okuyabilir, bir PDF'i parmak iziyle karşılaştırabilir.
- **Karşılama sayfası** (`/`): projenin ana sayfası; oturum açma düğmesi buradadır.
