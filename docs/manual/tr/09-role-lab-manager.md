# 9. Laboratuvar müdürü {#role-lab-manager}

## Günlük işiniz {#labmanager-daily}

Laboratuvarın iki kişisinden ikincisisiniz: analistlerinizin onaya gönderdiği laboratuvar raporlarını onaylar veya geri gönderirsiniz. Numune kavanozlarını da teslim alabilir (mühür, etiket ve kapak kontrolleri, bkz. [Numune teslim alma](#analyst-receive)) ve karışım numunesini oluşturabilirsiniz. Sonuçlarını kendiniz girdiğiniz bir raporu hiçbir zaman onaylayamazsınız; laboratuvar sonuçlarında bu kuralın muafiyeti yoktur.

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
- Sonuçlar lotun **KKN 1** değerleri olur: karışım numunesinin değerleri ya da her kavanoz ayrı analiz edildiğinde her parametre için kaplar arasındaki en kötü değer (bkz. [En kötü değer kuralı](#flow-worst)). Karışım analizinde tek bir kavanozun raporu o kavanozda kalır. Sonuçlar artık düzenlenemez, yalnızca yöneticiler tarafından düzeltilebilir. Laboratuvar tüm KKN 1 parametrelerini kapsadıysa KKN 1 hemen tamamlanır ve onaylanır; aksi halde kayıt tesisin Kalite Güvence Sorumlusuna gider.

**Sık karşılaşılan hatalar**

Table: Bir laboratuvar raporunu onaylarken sık karşılaşılan hatalar {#tbl-labmgr-errors}

| Mesaj | Anlamı |
|---|---|
| "Dört göz: sonuçları giren kişi onaylayamaz (laboratuvar sonuçlarında muafiyet yok)." | Başka bir laboratuvar müdürü onaylamalıdır. |
| "Raporu geri göndermek için gerekçe zorunludur." | Gerekçeyi yazın. |
| "Saklanan rapor dosyası SHA-256 özetiyle uyuşmuyor; yeniden yükleyin." | Saklanan PDF değişmiş; analistin dosyayı yeniden yüklemesi gerekir. |
| "Bu lotun KKN 0 (ön denetim) aşaması henüz tamamlanmadı; …" | Onay için KKN 0 beklenir. Raporu yine de geri gönderebilirsiniz. |

## Raporu herkese açık olarak doğrulama {#labmanager-verify}

Herkes bir raporu **Laboratuvar raporu doğrula** (`/verify-report`) sayfasında kontrol edebilir: **Rapor kodu** girilir ve **Sorgula** düğmesine tıklanır. Yalnızca **onaylanmış** raporlar bulunur; taslak veya onaya gönderilmiş bir rapor, bilinmeyen bir kod gibi yanıtlanır. Sayfa onay tarihini, laboratuvarı, akreditasyonunu, rapor numarasını, düzenleme tarihini ve SHA-256 parmak izini gösterir. **Bir PDF dosyasını bu parmak izle karşılaştırın (dosya cihazınızdan çıkmaz)** altında **PDF seç** bir PDF'i ziyaretçinin kendi cihazında kontrol eder (dosya yüklenmez) ve uyuşup uyuşmadığını bildirir. Hiçbir lot, arıcı veya kullanıcı verisi gösterilmez.

<!-- SHOT: verify-report -->
Figure: Telefonda bir laboratuvar raporunun herkese açık doğrulanması.
