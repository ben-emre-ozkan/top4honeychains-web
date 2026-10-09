# 8. Analist {#role-analyst}

## Günlük işiniz {#analyst-daily}

Bir laboratuvarda çalışırsınız. İşleme tesislerinin gönderdiği mühürlü numuneleri teslim alır, mührü kontrol eder, balı analiz eder, sonuçları rapor PDF'iyle birlikte girer ve raporu onay için laboratuvar müdürünüze gönderirsiniz.

Görevleriniz **İşlerim** sayfasında **Teslim alınacak numuneler** ve **Girilecek laboratuvar sonuçları** altında görünür. Menüde **Numuneler ve laboratuvar** bulunur.

## Numune teslim alma {#analyst-receive}

1. **İşlerim** sayfasında **Teslim alınacak numuneler** grubunu açın veya **Numuneler ve laboratuvar** → **Laboratuvarınıza gelen numuneler** bölümüne gidin.
2. Numunenin üzerindeki mühür numarasını ekranda gösterilen numarayla karşılaştırın.
3. Mühür sağlamsa **Teslim alındı, mühür sağlam** düğmesine tıklayın: "Numune teslim alındı; mühür sağlam."
4. Mühür bozuksa **gerekçe (mühür bozuksa)** alanını doldurun ve **Mühür bozuk: reddet** düğmesine tıklayın: "Numune reddedildi: mühür bozuk." Bu durumda tesisin yeni bir numune alması gerekir.

<!-- SHOT: sample-receive -->

## Laboratuvar sonuçlarını girme {#analyst-results}

1. **İşlerim** sayfasında **Girilecek laboratuvar sonuçları** grubunu açın. Numune için **Laboratuvar raporu** sayfası açılır.
2. Sayfada "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; …" uyarısının olmadığını kontrol edin. Lotun KKN 0 kaydı onaylanmadığı sürece form salt okunurdur. Tesisin kalite onaylayıcısı KKN 0'ı onaylayana kadar bekleyin.
3. **Rapor numarası** ve **Düzenleme tarihi** (ileri bir tarih olamaz) alanlarını doldurun.
4. **Rapor PDF** dosyasını seçin (10 MB'a kadar PDF).
5. Her parametrenin değerini girin. Zorunlu parametreler * ile işaretlidir. Sayılarda virgül veya nokta kullanılabilir.
6. **Kaydet** düğmesine tıklayın: "Laboratuvar sonuçları kaydedildi."

<!-- SHOT: lab-results-form -->

Kayıttan sonra platform:

- PDF'i SHA-256 parmak iziyle birlikte saklar (parmak izi bir kopyalama düğmesiyle gösterilir);
- 12 karakterlik **Rapor kodu** üretir; bu kodu herkes, herkese açık doğrulama sayfasında kontrol edebilir;
- her değeri limitiyle karşılaştırır (**Limit içinde** / **Limit dışında**).

Rapor taslak olduğu sürece istediğiniz kadar kaydedebilirsiniz.

## Raporu onaya gönderme {#analyst-submit}

1. Sayfada "Girilen zorunlu değer: … / …" yazar. **Onaya gönder** düğmesi yalnızca tüm zorunlu değerler, PDF, rapor numarası ve tarih girildiğinde etkin olur.
2. **Onaya gönder** düğmesine tıklayın: "Rapor onaya gönderildi; bir laboratuvar müdürü (başka bir kişi) onaylamalı."

<!-- SHOT: lab-submit -->

Onaya gönderdikten sonra rapor, laboratuvar müdürü geri göndermedikçe artık düzenlenemez. Geri gönderilen bir raporda gerekçe gösterilir; raporu düzeltip yeniden gönderin.

**Sık karşılaşılan hatalar**

| Mesaj | Ne yapmalı |
|---|---|
| "Sonuçlar yalnızca mührü sağlam teslim alınmış numune için girilir." | Önce numuneyi teslim alın. |
| "Göndermeden önce rapor PDF dosyasını yükleyin." | PDF'i seçin ve kaydedin. |
| "Onaya göndermeden önce tüm zorunlu değerleri girin: … / … girildi." | "Eksik:" altında listelenen değerleri tamamlayın. |
| "Laboratuvar raporu PDF olmalıdır." | Rapor için yalnızca PDF dosyaları kabul edilir. |
| "Rapor onaya gönderildi; laboratuvar müdürü onaylar ya da değişiklik için geri gönderir." | Laboratuvar müdürünü bekleyin. |
| "Onaylı laboratuvar raporu değiştirilemez; bunun yerine düzeltme talep edin." | Bir platform yöneticisinden düzeltme isteyin. |
