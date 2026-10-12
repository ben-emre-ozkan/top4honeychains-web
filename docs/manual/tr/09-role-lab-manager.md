# 9. Laboratuvar müdürü {#role-lab-manager}

## Günlük işiniz {#labmanager-daily}

Laboratuvarın iki kişisinden ikincisisiniz: analistlerinizin onaya gönderdiği laboratuvar raporlarını onaylar veya geri gönderirsiniz. Numune de teslim alabilirsiniz. Sonuçlarını kendiniz girdiğiniz bir raporu hiçbir zaman onaylayamazsınız; laboratuvar sonuçlarında bu kuralın muafiyeti yoktur.

Görevleriniz **İşlerim** sayfasında **Onaylanacak laboratuvar raporları** (ve **Teslim alınacak numuneler**) altında görünür.

## Raporu onaylama veya geri gönderme {#labmanager-decide}

1. **İşlerim** sayfasında **Onaylanacak laboratuvar raporları** grubunu açın.
2. Raporu kontrol edin: PDF, rapor numarası, düzenleme tarihi ve her değer. Limit dışındaki değerler işaretlidir.
3. Onaylamak için **Onayla** düğmesine tıklayın. Limit dışında değerler varsa **Sınır dışındaki değerler** penceresi onayınızı ister; **Yine de onayla** düğmesine tıklayın.
4. Raporu geri göndermek için **gerekçe (geri göndermek için)** alanını doldurun ve **Geri gönder** düğmesine tıklayın: "Rapor analiste geri gönderildi."

<!-- SHOT: lab-approve -->
Figure: Laboratuvar müdürü bir raporu onaylar veya geri gönderir.

Onay, balın uygun olduğunu değil, raporun doğru kaydedildiğini teyit eder; limit dışındaki değerler lotta işaretli kalır.

**Onay ne yapar** ("Laboratuvar raporu onaylandı; sonuçlar artık lotun KKN 1 değerleri."):

- Rapor **Onaylandı** durumuna geçer ve herkese açık doğrulama sayfasında görünür.
- Platform, laboratuvarın o anda **akredite** olup olmadığını (akreditasyon numarasıyla birlikte) ve lot sahibinin **şirket içi** laboratuvarı olup olmadığını kaydeder.
- Sonuçlar lotun **KKN 1** değerleri olur. Artık düzenlenemez, yalnızca yöneticiler tarafından düzeltilebilir. Laboratuvar tüm KKN 1 parametrelerini kapsadıysa KKN 1 hemen tamamlanır ve onaylanır; aksi halde kayıt tesisin Kalite Güvence Sorumlusuna gider.

**Sık karşılaşılan hatalar**

Table: Bir laboratuvar raporunu onaylarken sık karşılaşılan hatalar {#tbl-labmgr-errors}

| Mesaj | Anlamı |
|---|---|
| "Dört göz: sonuçları giren kişi onaylayamaz (laboratuvar sonuçlarında muafiyet yok)." | Başka bir laboratuvar müdürü onaylamalıdır. |
| "Raporu geri göndermek için gerekçe zorunludur." | Gerekçeyi yazın. |
| "Saklanan rapor dosyası SHA-256 özetiyle uyuşmuyor; yeniden yükleyin." | Saklanan PDF değişmiş; analistin dosyayı yeniden yüklemesi gerekir. |
| "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; …" | Onay için KKN 0 beklenir. Raporu yine de geri gönderebilirsiniz. |

## Raporu herkese açık olarak doğrulama {#labmanager-verify}

Herkes bir raporu **Laboratuvar raporu doğrula** (`/verify-report`) sayfasında kontrol edebilir: **Rapor kodu** girilir ve **Sorgula** düğmesine tıklanır. Sayfa raporun onaylanıp onaylanmadığını, laboratuvarı, akreditasyonunu, rapor numarasını, tarihi ve SHA-256 parmak izini gösterir. **Bir PDF dosyasını bu parmak izle karşılaştırın (dosya cihazınızdan çıkmaz)** seçeneği bir PDF'i ziyaretçinin kendi cihazında kontrol eder (dosya yüklenmez). Hiçbir lot, arıcı veya kullanıcı verisi gösterilmez.

<!-- SHOT: verify-report -->
Figure: Telefonda bir laboratuvar raporunun herkese açık doğrulanması.
