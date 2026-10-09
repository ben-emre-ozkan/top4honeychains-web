# 6. Personel {#role-staff}

## Günlük işiniz {#staff-daily}

**Bir işleme veya paketleme tesisinde** üretim kontrol noktalarının değerlerini girersiniz: ham bal lotlarında KKN 2–3, partilerde KKN 4–10. Bir parti tamamlandığında kesinleştirme talep eder, parti mühürlendikten sonra da partiyi sevk edersiniz.

**Bir toptancıda veya mağazada** gelen sevkiyatları onaylar veya reddeder, elinizdeki partilerin depolama koşullarını (KKN 10) kaydedersiniz.

Görevleriniz **İşlerim** sayfasında **Girilecek değerler** ve **Onaylanacak sevkiyatlar** altında görünür. Menüde **Kurumum** ve **Sevkiyatlar ve stok** bulunur.

## Kurumum {#staff-my-organization}

**Kurumum**, kurumunuzun ham bal lotlarını ve partilerini listeler. Her lot ve parti, KKN'lerini küçük etiketler halinde gösterir. Etiketin rengi durumu belirtir:

- gri: açık (değer yok veya eksik);
- sarı: gönderildi, onay bekliyor;
- yeşil çerçeveli: tamamlandı ancak henüz onaylanmadı;
- ✓ işaretli yeşil: onaylandı;
- koyu çerçeveli: uygulanamaz.

KKN sayfasını açmak için bir etikete tıklayın. **Süreç akışı** bağlantısı bir lotun veya partinin zaman çizelgesini gösterir.

<!-- SHOT: my-organization -->

## Lotta KKN 2–3 girişi {#staff-lot-ccps}

1. **İşlerim** sayfasında "KKN 2 · #[lot]" öğesini açın (veya **Kurumum** sayfasındaki etikete tıklayın).
2. Değerleri [KKN sayfasında çalışma](#ccp-page) bölümünde anlatıldığı gibi girin ve **İzleme Kayıtlarını Kaydet** düğmesine tıklayın.
3. Bu adım bu bal için geçerli değilse, bir gerekçeyle **Uygulanamaz** olarak işaretleyin.
4. Aynı işlemi KKN 3 için tekrarlayın.

Ardından kalite onaylayıcı bu kayıtları onaylar. Bir lot ancak tüm KKN'leri tamamlandığında veya uygulanamaz olarak işaretlendiğinde ve onaylandığında partiye eklenebilir.

## Partide KKN 4–10 girişi {#staff-batch-ccps}

Partiyi bir platform yöneticisi oluşturur, lotları ekler ve KKN 4–10'u atar. Ardından parti **Kurumum** sayfasında, KKN'leri de **İşlerim** sayfanızda görünür.

1. Her parti KKN'sini (KKN 4'ten KKN 10'a kadar) açın ve değerlerini girin.
2. Partideki KKN 10, **tesisinizdeki depolamadır**. Parti mühürlenmeden önce girilmiş ve onaylanmış olmalıdır.
3. Her seferinde **İzleme Kayıtlarını Kaydet** düğmesine tıklayın.

<!-- SHOT: ccp-page-batch -->

## Kesinleştirme talebi {#staff-request-finalize}

1. **Kurumum** sayfasında partiyi açın.
2. **Kesinleştirme** kartı, hâlâ eksik olan her şeyi "Kesinleştirmeden önce eksik olanlar:" başlığı altında listeler, örneğin "Parti: KKN 10 (Sıcaklık ve Nem İzleme) durumu: Gönderildi, henüz onaylanmadı". Eksik bir şey olduğu sürece **Kesinleştirme talep et** düğmesi devre dışıdır.
3. Liste boşaldığında **Kesinleştirme talep et** düğmesine tıklayın.

<!-- SHOT: finalize-request -->

"Kesinleştirme talep edildi; bir kalite onaylayıcı (başka bir kişi) onaylamalı." mesajı talebin alındığını bildirir. Kurumunuzun bir kalite onaylayıcısı talebi **Kesinleştirme talepleri** altında görür. Kendi talebinizi onaylayamazsınız.

## Parti sevk etme {#staff-ship}

Yalnızca kesinleşmiş bir parti sevk edilebilir.

1. **Sevkiyatlar ve stok** sayfasını açın. **Elinizdeki partiler** her parti için elinizdeki miktarı, bekleyen sevkiyatlar için ayrılan miktarı ve kullanılabilir miktarı gösterir.
2. Partinin yanındaki **Sevk et** düğmesine tıklayın.
3. **Alıcı** alanında alıcı kurumu (doğrulanmış başka herhangi bir kurum) seçin, **Miktar (kg)** alanını doldurun ve isteğe bağlı bir **Not** ekleyin.
4. Onaylayın. Miktar hemen ayrılır: "Sevkiyat oluşturuldu; emanet alıcı onaylayınca geçer."

<!-- SHOT: shipment-create -->

Hatalar: "Yalnızca kesinleşmiş bir parti sevk edilebilir.", "Bu partiden en çok … kg sevk edebilirsiniz.", "Alıcı olarak başka bir doğrulanmış kurum seçin."

## Sevkiyatı onaylama veya reddetme (alıcı) {#staff-receive}

1. **İşlerim** sayfasında **Onaylanacak sevkiyatlar** grubunu açın veya **Sevkiyatlar ve stok** → **Gelen sevkiyatlar** bölümüne gidin.
2. Ürünleri kontrol edin. **Teslimi onayla** düğmesine tıklayın: "Teslim onaylandı; bu miktar artık sizde."
3. Ya da **gerekçe (reddetmek için)** alanını doldurup **Reddet** düğmesine tıklayın: "Sevkiyat reddedildi; miktar gönderende kalır." Gerekçe zorunludur.

<!-- SHOT: shipment-confirm -->

## Depolama kaydı (alıcıda KKN 10) {#staff-storage}

Bir partiyi teslim aldıktan sonra:

1. **Sevkiyatlar ve stok** sayfasında partinin **Depolama** sütunundaki **KKN 10** bağlantısına tıklayın ("KKN 10 – Depolama" sayfası).
2. Tüm zorunlu depolama değerlerini tek seferde girin (örneğin sıcaklık ve nem).
3. **Kaydet** düğmesine tıklayın: "Depolama değerleri kaydedildi." Kayıt **Gönderildi** durumuna geçer ve kalite onaylayıcınız kaydı **Onaylanacak değerler** altında görür.

<!-- SHOT: storage-record -->

## Üreticinin kayıtlarını okuma {#staff-read-only}

Kurumunuz bir partiyi elinde tutuyor veya teslim almışsa, o partinin ve lotlarının üreticiye ait kontrol noktalarını **salt okunur** olarak açabilirsiniz: değerleri ve onaylayanları görürsünüz, ancak kaydetme veya onaylama düğmeleri yoktur. Arıcı ve arılık adları, geri çağırma raporunda olduğu gibi "gösterilmiyor" olarak görünür. Sayfada **Salt okunur** ve "Bu kontrol noktası başka bir kurumun sorumluluğunda; yalnızca okuyabilirsiniz." yazar. Kurumunuzun hiç teslim almadığı bir parti için platform "Bu sayfayı açamazsınız" sayfasını gösterir.

Bu sayfaları açmak için **Sevkiyatlar ve stok** sayfasında elinizdeki partinin yanındaki **Süreç akışı** düğmesine, ardından akıştaki bir aşamaya tıklayın.

## Geri çağırma raporu {#staff-recall}

Bir partiden veya lottan **Geri çağırma raporu** açabilirsiniz. Rapor, kurumunuzun görebildiği kaynak lotları ve sonraki sevkiyatları **Yazdır** ve **CSV** düğmeleriyle birlikte listeler. Bkz. [Geri çağırma](#admin-recall).
