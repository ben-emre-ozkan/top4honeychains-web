# 8. Analist {#role-analyst}

## Günlük işiniz {#analyst-daily}

Bir gıda analiz laboratuvarında çalışırsınız ve laboratuvarın iki kişisinden birincisisiniz: siz girersiniz, laboratuvar müdürü onaylar. İşleme tesislerinin gönderdiği mühürlü numune kavanozlarını (bir teslimin her kabından bir tane) teslim alır, mührü, etiketi ve kapağı kontrol eder, karışım numunesini oluşturur, balı analiz eder, sonuçları rapor PDF'iyle birlikte girer ve raporu onay için laboratuvar müdürünüze gönderirsiniz.

Görevleriniz **İşlerim** sayfasında **Teslim alınacak numuneler** ve **Girilecek laboratuvar sonuçları** altında görünür. Menüde **Numuneler ve laboratuvar** bulunur.

## Numune teslim alma {#analyst-receive}

1. **İşlerim** sayfasında **Teslim alınacak numuneler** grubunu açın veya **Numuneler ve laboratuvar** → **Laboratuvarınıza gelen numuneler** bölümüne gidin. Her kavanoz kodunu (`S-{lot}-C{n}`), kabını ve mühür numarasını gösterir.
2. Kavanoz için üç kontrolü yanıtlayın: **Mühür numarası kayıtla aynı**, **Etiket sağlam** ve **Kapak sağlam (düğme aşağıda)**.
3. Üçü de **Evet** ise **Kabulü kaydet** düğmesine tıklayın: kavanoz teslim alınır.
4. Kontrollerden biri **Hayır** ise **Bir kontrol başarısızsa gerekçe** alanını doldurun ve **Kabulü kaydet** düğmesine tıklayın: kavanoz reddedilir. Tesis aynı kaptan yeni bir kavanoz alır ve size gönderir.

<!-- SHOT: sample-receive -->
Figure: Laboratuvarda numune kavanozlarını teslim alma: mühür, etiket ve kapak.

Eski bir teslimin numunesinde (lot başına bir numune) tek bir mühür kontrolü vardır: **Teslim alındı, mühür sağlam** ya da **gerekçe (mühür bozuksa)** ve **Mühür bozuk: reddet**.

## Karışım numunesini oluşturma {#analyst-composite}

Karışım analizinde (varsayılan) laboratuvar lot başına bir karışım numunesi analiz eder (bkz. [Karışım veya kap başına analiz](#flow-analysis)). Bir lotun her kabının kavanozu sağlam ulaştığında lot **Karışım numunesine hazır lotlar** altında görünür: "Bu lotların her kabının kavanozu sağlam ulaştı. Kavanozlardan eşit miktarları tek bir karışım numunesinde birleştirip analiz edin; kavanozlar tek tek analiz için saklanır."

1. Kavanozlardan eşit miktarları karıştırın.
2. **Karışım numunesini oluştur** düğmesine tıklayın. `S-{lot}-COMP` karışım numunesi oluşturulur ve teslim alınmış olarak kaydedilir; raporu diğer raporlar gibi girilir.

Karışım başarısız olursa veya istenirse tek bir kavanoz için de rapor girebilirsiniz; bu rapor o kavanozda kalır ve lotun KKN 1 değerlerini değiştirmez. Kap başına analizde karışım numunesi yoktur: her kavanozun kendi raporu olur ve lotun KKN 1'i her parametrenin en kötü değerini alır (bkz. [En kötü değer kuralı](#flow-worst)).

## Laboratuvar sonuçlarını girme {#analyst-results}

1. **İşlerim** sayfasında **Girilecek laboratuvar sonuçları** grubunu açın. Numune için **Laboratuvar raporu** sayfası açılır.
2. Sayfada "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; …" uyarısının olmadığını kontrol edin. Lotun KKN 0 kaydı onaylanmadığı sürece form salt okunurdur. Tesisin Kalite Güvence Sorumlusu KKN 0'ı onaylayana kadar bekleyin.
3. **Rapor numarası** ve **Düzenleme tarihi** (ileri bir tarih olamaz) alanlarını doldurun.
4. **Rapor PDF** dosyasını seçin (10 MB'a kadar PDF).
5. Her parametrenin değerini girin. Zorunlu parametreler * ile işaretlidir. Sayılarda virgül veya nokta kullanılabilir.
6. **Kaydet** düğmesine tıklayın: "Laboratuvar sonuçları kaydedildi."

<!-- SHOT: lab-results-form -->
Figure: Rapor PDF'iyle birlikte laboratuvar sonuçlarını girme.

Kayıttan sonra platform:

- PDF'i SHA-256 parmak iziyle birlikte saklar (parmak izi bir kopyalama düğmesiyle gösterilir);
- 12 karakterlik **Rapor kodu** üretir; bu kodu herkes, herkese açık doğrulama sayfasında kontrol edebilir;
- her değeri limitiyle karşılaştırır (**Limit içinde** / **Limit dışında**).

Rapor taslak olduğu sürece istediğiniz kadar kaydedebilirsiniz.

## Raporu onaya gönderme {#analyst-submit}

1. Sayfada "Girilen zorunlu değer: … / …" yazar; sayı siz yazdıkça güncellenir ve "Eksik:" henüz boş olan değerleri belirtir. **Onaya gönder** düğmesi yalnızca tüm zorunlu değerler, PDF, rapor numarası ve tarih **kaydedildiğinde** etkin olur: her şey yazılmış ancak henüz kaydedilmemişse sayfada "Göndermeden önce değerleri kaydedin." yazar.
2. **Onaya gönder** düğmesine tıklayın: "Rapor onaya gönderildi; bir laboratuvar müdürü (başka bir kişi) onaylamalı."

<!-- SHOT: lab-submit -->
Figure: Tüm zorunlu veriler girildi: rapor onaya gönderilebilir.

Onaya gönderdikten sonra rapor, laboratuvar müdürü geri göndermedikçe artık düzenlenemez. Geri gönderilen bir raporda gerekçe gösterilir; raporu düzeltip yeniden gönderin.

**Sık karşılaşılan hatalar**

Table: Laboratuvar sonuçlarını girerken sık karşılaşılan hatalar {#tbl-lab-errors}

| Mesaj | Ne yapmalı |
|---|---|
| "Sonuçlar yalnızca mührü sağlam teslim alınmış numune için girilir." | Önce kavanozu teslim alın; reddedilen bir kavanoza sonuç girilmez. |
| "Göndermeden önce rapor PDF dosyasını yükleyin." | PDF'i seçin ve kaydedin. |
| "Onaya göndermeden önce tüm zorunlu değerleri girin: … / … girildi." | "Eksik:" altında listelenen değerleri tamamlayın. |
| "Laboratuvar raporu PDF olmalıdır." | Rapor için yalnızca PDF dosyaları kabul edilir. |
| "Rapor onaya gönderildi; laboratuvar müdürü onaylar ya da değişiklik için geri gönderir." | Laboratuvar müdürünü bekleyin. |
| "Onaylı laboratuvar raporu değiştirilemez; bunun yerine düzeltme talep edin." | Bir platform yöneticisinden düzeltme isteyin. |
| "Lot kesinleşmiş bir partide kilitli; laboratuvar değerleri yalnızca düzeltmeyle değişebilir." | Lot bir partide mühürlendi; taslak artık kaydedilemez veya gönderilemez. Bir platform yöneticisine başvurun. |
