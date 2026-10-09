# 7. Kalite onaylayıcı {#role-qa}

## Günlük işiniz {#qa-daily}

Kurumunuzun ikinci göz çiftisiniz. Göreviniz:

- iş arkadaşlarınızın gönderdiği KKN kayıtlarını (KKN 0, KKN 2–10 ve depolama kayıtları) onaylamak;
- bir partiyi mühürleyen kesinleştirme taleplerini onaylamak;
- bir toptancıda veya mağazada KKN 10 depolama kayıtlarını onaylamak.

Görevleriniz **İşlerim** sayfasında **Onaylanacak değerler** ve **Kesinleştirme talepleri** altında görünür. Değerlerini kendiniz girdiğiniz bir kaydı hiçbir zaman onaylayamazsınız.

## KKN kaydını onaylama {#qa-approve}

1. **İşlerim** sayfasında **Onaylanacak değerler** altındaki bir öğeyi açın, örneğin "KKN 0 · #20070".
2. Değerleri kontrol edin. Her değer, limitine göre **Limit içinde** veya **Limit dışında** gösterir.
3. **Onay (dört göz)** kartındaki notu okuyun: onay, değerlerin limit içinde olduğunu değil, doğru kaydedildiğini teyit eder.
4. **Onayla** düğmesine tıklayın.

<!-- SHOT: ccp-approve -->

"KKN kaydı onaylandı." mesajı onayı bildirir; durum göstergesinde adınız ve zamanla birlikte **Onaylandı** yazar.

**Limit dışındaki değerler.** Herhangi bir değer limit dışındaysa **Sınır dışındaki değerler** penceresi bu değerleri, her birinin **Limit:** bilgisiyle listeler (henüz doğrulanmamış limitler için "sınır henüz doğrulanmadı" yazar). Onaylamak için **Yine de onayla**, vazgeçmek için **Vazgeç** düğmesine tıklayın. Değerler limit dışı olarak işaretli kalır; onay yalnızca değerlerin doğru kaydedildiğini teyit eder.

<!-- SHOT: out-of-limit-dialog -->

**Uygulanamaz kayıtlar.** "Uygulanamaz" olarak işaretlenmiş bir KKN'nin de onaylanması gerekir. Onaylamadan önce gerekçeyi okuyun.

**Sık karşılaşılan hatalar**

| Mesaj | Anlamı |
|---|---|
| "Dört göz: bu KKN'nin değerlerini siz girdiniz, başka bir kişinin onaylaması gerekir." | Bu kayda siz değer girdiniz. Kaydı başka bir onaylayıcı onaylamalıdır. |
| "Yalnızca gönderilmiş (veya "Uygulanamaz") bir KKN kaydı onaylanabilir." | Zorunlu değerler hâlâ eksik. |
| "Bu KKN kaydı zaten onaylandı." | Yapılacak bir şey yok. |
| "Rolünüz (…) bu kontrol noktasını onaylayamaz; onaylayan roller: …" | Bu KKN'yi başka bir rol onaylar. |

Kurumunuzun **Dört göz muafiyeti** varsa kendi kayıtlarınızı onaylayabilirsiniz; bu durumda mesaj "Dört göz muafiyetiyle onaylandı (denetim kaydında işaretlendi)." olur. Muafiyet laboratuvar sonuçlarına hiçbir zaman uygulanmaz.

## Kesinleştirmeyi onaylama (partiyi mühürleme) {#qa-finalize}

1. **İşlerim** sayfasında **Kesinleştirme talepleri** altındaki öğeyi açın veya partiyi **Kurumum** sayfasında açın.
2. Partiyi kontrol edin: **Partinin kontrol noktaları** ve **Kaynak lotlar** onaylanmış olmalı, "Kesinleştirmeden önce eksik olanlar:" listesi de boş olmalıdır.
3. **Onayla ve kesinleştir** düğmesine tıklayın.

<!-- SHOT: finalize-confirm -->

Parti mühürlenir: "Parti kesinleşti." Bir kez, **Bal Hikâyesini aç** ve **QR'ı indir** bağlantılarıyla birlikte bir mühür görünür. Bundan sonra:

- parti ve tüm lotları kilitlenir;
- herkese açık Bal Hikâyesi, QR kodu üzerinden yayına girer;
- parti sevk edilebilir.

Kesinleştirmeyi geri almak mümkün değildir. Sonradan fark edilen hatalar, yöneticilerin düzeltme süreciyle düzeltilir.

Hatalar: "Dört göz: kesinleştirmeyi talep eden kişi onaylayamaz.", "Önce kesinleştirme talep edilmelidir.", "Parti henüz kesinleştirilemez: …".

<!-- SHOT: batch-sealed -->

## Depolama kayıtlarını onaylama (toptancı veya mağaza) {#qa-storage}

1. **İşlerim** sayfasında **depolama** olarak işaretlenmiş öğeyi ("KKN 10 · [parti]") açın veya **Sevkiyatlar ve stok** → **Depolama** bölümüne gidin.
2. Değerleri kontrol edin ve **Onayla** düğmesine tıklayın (istenirse limit dışındaki değerleri pencerede onaylayın).

Aynı dört göz kuralı geçerlidir: değerleri kaydeden iş arkadaşınız bu değerleri onaylayamaz.
