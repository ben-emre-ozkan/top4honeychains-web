# 5. Numune kabul sorumlusu {#role-sao}

## Günlük işiniz {#sao-daily}

Bir işleme tesisinin kabul biriminde çalışırsınız. Bir arıcı bal teslim ettiğinde:

1. teslimi kaydedersiniz (bu kayıt ham bal lotunu oluşturur);
2. bir numune ve bir şahit numune mühürler, numuneyi laboratuvara gönderirsiniz;
3. lotun ön denetimi olan KKN 0'ı girersiniz; böylece bir kalite onaylayıcı kaydı onaylayabilir.

Görevleriniz **İşlerim** sayfasında **Girilecek değerler** altında görünür. Menüde **Teslim almalar** ve **Numuneler ve laboratuvar** bulunur.

## Teslim kaydetme {#sao-intake}

1. **Teslim almalar** sayfasını açın ve **Teslim al** düğmesine tıklayın.
2. **Arıcı işletme veya kayıt numarası** alanına arıcının işletme numarasını veya doğrulanmış işletme kayıt numarasını eksiksiz yazın ve **Bul** düğmesine tıklayın. Hiçbir şey bulunamazsa şu mesaj çıkar: "Bu numarayla etkin bir arıcı yok." Numarayı kontrol edin; arıcıları bir platform yöneticisi kaydeder.
3. Formu doldurun:
   - **Arılık**: arıcının arılıklarından biri.
   - **Bal türü**.
   - **Üretim dönemi**: geçerli dönem önceden seçilidir.
   - **Menşe ülke**.
   - **Teslim tarihi**: varsayılan olarak bugündür; ileri bir tarih olamaz.
   - **Miktar (kg)**: ondalık ayırıcı olarak virgül veya nokta kullanılabilir.
   - Kökenin **İl** ve **İlçe** bilgisi: il, mümkün olduğunda kayıt numarasından önceden doldurulur.
   - **İmzalı teslim formunun fotoğrafı**: kamera düğmesiyle fotoğraf çekin veya bir dosya seçin (10 MB'a kadar PDF ya da 8 MB'a kadar JPG, PNG veya WEBP).
   - **Arıcıya SMS gönder** (isteğe bağlı; mevcut sürümde mesaj gönderilmez, yalnızca kayda geçer).
4. **Teslimi kaydet** düğmesine tıklayın.

<!-- SHOT: intake-form -->

"Teslim #… numaralı lot olarak kaydedildi." mesajı yeni lotun oluştuğunu bildirir. Platform lota KKN 0–3'ü atar ve lot için **Numune al** sayfasını açar.

**Sık karşılaşılan hatalar**

| Mesaj | Ne yapmalı |
|---|---|
| "Arıcının arılıklarından birini seçin." | Listeden bir arılık seçin. |
| "Teslim tarihini girin (gelecekte olamaz)." | Tarihi düzeltin. |
| "Miktar 0'dan büyük olmalıdır." | Miktarı kilogram olarak girin. |
| "Seçilen ilin bir ilçesini seçin." | Önce ili, ardından o ilin listesinden bir ilçe seçin. |
| "İmzalı teslim formunun fotoğrafını ekleyin." | İmzalı form zorunludur. |
| "Bu arılığın seçilen dönem ve bal türü için zaten bir üretim kaydı var." | Bir arılık her dönem ve bal türü için yalnızca bir kez teslim yapar. Dönemi ve bal türünü kontrol edin. |
| "Teslimleri yalnızca işleme yeteneği olan kurum kaydeder." / "Rolünüz (…) teslim kaydedemez; yetkili roller: …" | Kurumunuz veya rolünüz teslim kaydedemez. Kurum yöneticinize başvurun. |

**Teslim almalar** listesi her teslimi lot numarası, tarih, arılık, bal türü, miktar, köken ve imzalı formla (SHA-256 parmak izinin ilk karakterleriyle birlikte) gösterir. Her satırdan **Numune** ve **Geri çağırma** sayfalarını açabilirsiniz.

## Numune alma ve gönderme {#sao-sample}

1. Teslimi kaydettikten sonra **Numune al** sayfası açılır. Bu sayfayı **Numuneler ve laboratuvar** sayfasından veya teslim satırından da açabilirsiniz.
2. **Laboratuvar** seçin: yalnızca doğrulanmış laboratuvarlar listelenir; akredite bir laboratuvar, akreditasyon numarasıyla birlikte gösterilir.
3. Numunenin **Mühür numarası** ile **Şahit numune mühür numarası** alanlarını doldurun. İki numara farklı olmalıdır.
4. **Şahit numunenin saklandığı yer** ve **Saklama süresi sonu** alanlarını doldurun (varsayılan olarak 12 ay sonrası).
5. **Laboratuvara gönder** düğmesine ya da numuneyi daha sonra göndermek için **Sonra** düğmesine tıklayın.

<!-- SHOT: sample-form -->

Numune `S-20070-1` gibi bir kod ve **Gönderildi** durumunu alır. Laboratuvar numuneyi **Teslim alınacak numuneler** altında görür. Laboratuvar mührü bozuk bulursa numune reddedilir; bu durumda aynı lot için yeni bir numune alın.

Hatalar: "Doğrulanmış bir laboratuvar seçin.", "Numune ve şahit numune farklı mühürler gerektirir.", "Şahit numunenin nerede saklandığını girin (en çok 200 karakter).", "Şahit numunenin ne zamana kadar saklanacağını girin (ileri bir tarih).", "Numuneyi yalnızca lotun bulunduğu kurum alır."

## KKN sayfasında çalışma {#ccp-page}

KKN değerlerini giren veya onaylayan tüm roller aynı sayfayı kullanır. Sayfayı **İşlerim** sayfasından, **Kurumum** sayfasındaki KKN etiketlerinden veya bir lotun ya da partinin **Süreç akışı** sayfasından açın.

<!-- SHOT: ccp-page-lot -->

Sayfa şunları gösterir:

- lot (arıcı, arılık, bal türü, üretim tarihi) veya parti (parti, toplam miktar, toptancı, kaynak bileşimi);
- **Durum** göstergesi ve kaydı kimin girip kimin onayladığı;
- limitler için kullanılan bal sınıfı ("Bal sınıfı için limitler: …");
- gruplanmış parametreler; her birinin giriş alanı, **Kritik limit:** bilgisi ve kayıttan sonra **Limit içinde** veya **Limit dışında** sonucu;
- değer giremediğiniz durumlarda, gerekçesiyle birlikte bir **Salt okunur** uyarısı.

Giriş türleri:

- **Sayı**: ondalık ayırıcı olarak virgül veya nokta.
- **Evet / Hayır**: listeden seçin.
- **Metin**: en çok 100 karakter.
- **Dosya**: bir belge yükleyin (10 MB'a kadar PDF veya 8 MB'a kadar görüntü). Silmek için **Kaldır** kutusunu işaretleyin.
- **Notlar**: isteğe bağlı bir gözlem, en çok 500 karakter.

Bazı parametreler yalnızca başka bir soru belirli bir şekilde yanıtlandığında zorunludur, örneğin "Yalnızca “…” sorusu Evet yanıtlanırsa zorunludur."

**Değer girmek için**

1. Parametreleri doldurun.
2. **İzleme Kayıtlarını Kaydet** düğmesine tıklayın. "KKN … değerleri kaydedildi." mesajı kaydın yapıldığını bildirir ve **Kurumum** sayfasına dönersiniz.

Ayrı bir gönderme düğmesi yoktur. Tüm zorunlu parametrelerin değeri girildiğinde kayıt **Gönderildi** durumuna geçer ve onaylayıcıların **İşlerim** sayfasında görünür. Yalnızca bazı değerler girildiyse kayıt **Devam ediyor** durumunda kalır.

**KKN uygulanmıyorsa** (örneğin bal ısıtılmıyorsa):

1. Kayıtta henüz değer yokken "Bu KKN burada uygulanmıyor ("Uygulanamaz" olarak işaretle)" bölümünü açın.
2. **Gerekçe** alanını doldurun, örneğin "Ham bal ısıtılmaz veya pastörize edilmez".
3. **"Uygulanamaz" olarak işaretle** düğmesine tıklayın. Gerekçe, kullanıcı adınız ve zaman kaydedilir; herkese açık Bal Hikâyesi'nde "Uygulanamaz" olarak gösterilir.

Geri almak için **"Uygulanamaz" işaretini kaldır** düğmesine tıklayın. Uygulanamaz olarak işaretlenen bir KKN'nin de onaylanması gerekir; onayı, işareti koyan kişiden başka biri verir.

**Sık karşılaşılan hatalar**

| Mesaj | Anlamı |
|---|---|
| "…: bir sayı girin." / "…: Evet veya Hayır seçin." | Bir değerin türü yanlış. |
| "Bu KKN "Uygulanamaz" olarak işaretli. Değer kaydetmek için önce bu işareti kaldırın." | Değer girmeden önce işareti kaldırın. |
| "Bu kontrol noktası başka bir kurumun sorumluluğunda; yalnızca okuyabilirsiniz." | Bu KKN'yi salt okunur görürsünüz. |
| "Rolünüz (…) bu kontrol noktasına veri giremez; veri giren roller: …" | Bu KKN'ye başka bir rol veri girer. |
| "Kurumunuzun durumu: Beklemede; yalnızca doğrulanmış kurumlar veri girebilir." | Kurumun doğrulanmasını bekleyin. |
| "Bu ham bal lotu kesinleşmiş … partisinde kullanıldığı için kilitli. …" | Lot mühürlenmiştir; yalnızca bir düzeltmeyle değiştirilebilir. |

## KKN 0 girişi (ön denetim) {#sao-ccp0}

1. **İşlerim** sayfasında, **Girilecek değerler** altında "KKN 0 · #[lot]" öğesini açın.
2. Kabul kontrollerini (KKN 0 parametrelerini) girin. Bazı sorular **Evet** olarak yanıtlandığında başka zorunlu sorular açılır.
3. **İzleme Kayıtlarını Kaydet** düğmesine tıklayın.

Tüm zorunlu değerler girildiğinde KKN 0 **Gönderildi** durumuna geçer ve kurumunuzun kalite onaylayıcısı kaydı **Onaylanacak değerler** altında görür. Kendi girdiğiniz KKN 0 kaydını onaylayamazsınız ("Dört göz: bu KKN'nin değerlerini siz girdiniz, başka bir kişinin onaylaması gerekir."). Laboratuvar ancak KKN 0 onaylandıktan sonra sonuç girebilir.

<!-- SHOT: ccp0-submitted -->
