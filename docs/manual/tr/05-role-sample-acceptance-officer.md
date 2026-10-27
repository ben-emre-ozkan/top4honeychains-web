# 5. Numune kabul sorumlusu {#role-sao}

## Günlük işiniz {#sao-daily}

Bir işleme tesisinin kabul biriminde çalışırsınız. Bir arıcı bal teslim ettiğinde:

1. teslimi mühürleriyle birlikte kap kap kaydeder, her kaptan bir numune kavanozu ve bir şahit numune kavanozu doldurursunuz;
2. arıcının teslimi SMS ile aldığı kodla onaylamasını sağlarsınız;
3. etiketleri yazdırır ve numune kavanozlarını laboratuvara gönderirsiniz;
4. lotun ön denetimi olan KKN 0'ı girersiniz; böylece bir Kalite Güvence Sorumlusu kaydı onaylayabilir.

Görevleriniz **İşlerim** sayfasında **Girilecek değerler** altında görünür. Menüde **Teslim almalar** ve **Numuneler ve laboratuvar** bulunur.

## Teslim kaydetme {#sao-intake}

"Teslimattaki her kabı mührüyle kaydedin, her kaptan numune alın; ardından arıcı hepsini SMS koduyla onaylar."

1. **Teslim almalar** sayfasını açın ve **Teslim al** düğmesine tıklayın.
2. **Arıcı işletme veya kayıt numarası** alanına arıcının işletme numarasını veya doğrulanmış işletme kayıt numarasını eksiksiz yazın ve **Bul** düğmesine tıklayın. Hiçbir şey bulunamazsa şu mesaj çıkar: "Bu numarayla etkin bir arıcı yok." Numarayı kontrol edin. Arıcıları arıcı kayıt görevlisi kaydeder; imzalı fotoğrafla kaydedilen bir arıcı ancak Uyum Sorumlusu fotoğrafı kabul ettikten sonra teslim yapabilir. "cep telefonu yok: yalnızca imzalı fotoğraf" ifadesi, arıcının SMS kodu alamayacağı anlamına gelir.
3. **Teslimat**: **Arılık**, **Bal türü** ve **Üretim dönemi** (geçerli dönem önceden seçilidir), **Menşe ülke** ve **Teslim tarihi** (varsayılan olarak bugün; ileri bir tarih olamaz) alanlarını seçin.
4. **Kaplar ve numuneler**: her kap için bir satır ([Şekil](#fig-intake-form)).
   - Kabın **Tür** bilgisi, **Net kg** ve isteğe bağlı olarak **Brüt kg** (ondalık ayırıcı olarak virgül veya nokta kullanılabilir).
   - **Gelişte**: **mühürlü geldi** kutusunu işaretleyip **Mühür numarası** alanını doldurun, mühür sağlamsa **mühür sağlam** kutusunu işaretleyin; mühürsüz bir kap için arıcının önünde takılan **Şimdi takılan işletme mührü** numarasını girin.
   - Kabı açın, numune kavanozunu ve şahit numune kavanozunu doldurun. **Kavanoz mührü** ve **Şahit numune mührü** alanlarını doldurun.
   - **Kapak sağlam** kutusunu yalnızca kapağın düğmesi aşağıdayken işaretleyin. "Güvenlik kapağı: yalnızca düğme aşağıdayken işaretleyin. Fırlamış kapak kavanozun açıldığını gösterir: kavanozu atın ve yenisini doldurun."
   - Kabı yeniden kapatan **Numuneden sonra yeni mühür** numarasını girin.
   - **Kap ekle** bir satır ekler; **Satırı kaldır** bir satırı kaldırır. **Toplam net** kapların toplamını gösterir.
5. **Laboratuvar ve şahit numuneler**: **Laboratuvar** (yalnızca doğrulanmış laboratuvarlar listelenir; akredite bir laboratuvar, akreditasyon numarasıyla birlikte gösterilir), **Şahit numunelerin saklandığı yer** ve **Saklama bitişi** (varsayılan olarak 12 ay sonrası) alanlarını doldurun.
6. **Arıcı onaylar**: **Kod gönder** düğmesine tıklayın. Arıcıya kurumunuzu, tarihi, kapları, kilogramları, bal türünü, numune sayısını ve bir kodu içeren bir SMS gider. Arıcının okuduğu kodu **SMS'teki kod (6 hane)** alanına girin ve **Teslimatı onayla** düğmesine tıklayın. "Arıcı burada değilse onu arayın: kodu telefonundan okur."

<!-- SHOT: intake-form -->
Figure: Telefonda teslim kaydetme: kaplar, mühürler ve numune kavanozları.

"Teslimat arıcı tarafından onaylandı ve #… numaralı lot olarak kaydedildi." mesajı yeni lotun oluştuğunu bildirir. Platform lota KKN 0–3'ü atar, her mührü bir mühür olayı olarak kaydeder ve her kap için bir numune kavanozunu seçilen laboratuvara gönderir.

**Kod.** Kod 5 dakika geçerlidir ve 3 kez denenebilir; **Yeni kod gönder** bir dakika sonra yeni bir kod gönderir. Kodu gönderdikten sonra herhangi bir şeyi değiştirirseniz kod geçersiz olur ("Kod gönderildikten sonra bilgiler değişti. Yeni bir kod gönderin."). Bir saatte 10 yanlış koddan sonra 15 dakika beklemeniz gerekir; bir görevli saatte en çok 30 kod gönderir.

**SMS mümkün değil: imzalı fotoğraf.** Arıcının cep telefonu yoksa veya SMS gönderilemiyorsa **SMS mümkün değil: imzalı fotoğraf** bölümünü açın, teslim formunu yazdırın, arıcıya imzalatın, **İmzalı teslim formunun fotoğrafı** alanında fotoğrafı seçin (kamera veya dosya: 10 MB'a kadar PDF ya da 8 MB'a kadar JPG, PNG veya WEBP), **SMS kodu neden kullanılamıyor** alanını doldurun ve **İmzalı fotoğrafla kaydet** düğmesine tıklayın: "Teslimat imzalı fotoğrafla #… numaralı lot olarak kaydedildi; Uyum Sorumlusunu bekliyor." Lot üzerinde çalışılabilir, ancak tesisin Uyum Sorumlusu fotoğrafı kabul edene kadar lot kesinleştirilemez.

**Sık karşılaşılan hatalar**

Table: Teslim kaydederken sık karşılaşılan hatalar {#tbl-intake-errors}

| Mesaj | Ne yapmalı |
|---|---|
| "En az bir kap kaydedin." | Her kap için bir satır ekleyin. |
| "Kap …: net ağırlığı girin (0 kg'dan fazla)." / "…: brüt ağırlık net ağırlıktan düşük olamaz." | O kabın ağırlıklarını düzeltin. |
| "Kap …: geldiği mührün numarasını girin." / "…: arıcının önünde işletme mührü takın ve numarasını girin." | Her kabın geliş mührü veya bir işletme mührü olmalıdır. |
| "Kap …: numune kavanozunun ve şahit numune kavanozunun mühür numaralarını girin." | Her kabın iki kavanozu da mühürlenir. |
| "Kap …: mühürlenirken kavanoz kapağı sağlam olmalı (düğme aşağıda). Fırlamış kapak: kavanozu atın ve yenisini doldurun." | Kapağı sağlam yeni bir kavanoz doldurun. |
| "Kap …: numuneden sonra takılan yeni işletme mührünü girin." | Kabı yeni bir mühürle kapatın. |
| "Teslimattaki her mühür numarası farklı olmalıdır." | Bir mühür numarası iki kez kullanılmış; numaraları kontrol edin. |
| "Arıcının arılıklarından birini seçin." / "Teslim tarihini girin (gelecekte olamaz)." | Teslimat alanlarını düzeltin. |
| "Bu arılığın seçilen dönem ve bal türü için zaten bir üretim kaydı var." | Bir arılık her dönem ve bal türü için yalnızca bir kez teslim yapar. Dönemi ve bal türünü kontrol edin. |
| "Yanlış kod. … deneme hakkı kaldı." / "Kodun süresi doldu. Yeni bir kod gönderin." | Arıcıdan kodu yeniden okumasını isteyin veya yeni bir kod gönderin. |
| "SMS kodunun neden kullanılamadığını yazın." / "İmzalı teslim formunun fotoğrafını ekleyin." | İmzalı fotoğraf yolu ikisini de gerektirir. |
| "Teslimleri yalnızca işleme yeteneği olan kurum kaydeder." / "Rolünüz (…) teslim kaydedemez; yetkili roller: …" | Kurumunuz veya rolünüz teslim kaydedemez. Kurum yöneticinize başvurun. |

**Teslim almalar** listesi her teslimi lot numarası, tarih, arılık, bal türü, miktar, köken ve nasıl onaylandığıyla birlikte gösterir. Her satırdan teslimat sayfasını, **Numune** ve **Geri çağırma** sayfalarını açabilirsiniz.

## Teslimat sayfası ve etiketler {#sao-labels}

Teslimat sayfası lotu, **Mühür geçmişi** ile birlikte kapları, **Numune kavanozları** ve bunların laboratuvardaki durumunu, teslimin hangi **Onay yolu** ile (SMS kodu veya imzalı fotoğraf) onaylandığını ve **Onaylanan içerik** kontrolünü gösterir: "onaydan beri değişmedi" ya da bir düzeltmeden sonra düzeltilmiş içerik.

**Etiketler (A4 sayfa)** teslimin tüm etiketlerini A4 sayfalara (2 × 5) yazdırır; **Etiketler (100 × 50 mm)** bir etiket yazıcısı için sayfa başına bir etiket yazdırır. Her kap (`L{lot}-C{n}`), her numune kavanozu (`S-{lot}-C{n}`) ve her şahit numune (`S-{lot}-C{n}-K`) için, her biri QR kodlu ve mühür numaralı bir etiket vardır; şahit numune etiketinde ayrıca **saklama** tarihi yer alır.

<!-- SHOT: intake-labels -->
Figure: Bir teslimin etiketleri: kap, numune kavanozu ve şahit numune.

## Laboratuvar bir kavanozu reddettiğinde yeni kavanoz alma {#sao-resample}

Laboratuvar bir kavanozu reddederse (mühür numarası, etiket veya kapak uygun değil) teslimat sayfası o kavanozda "Laboratuvar kavanozu reddetti: yeni numune alın" uyarısını gösterir. Kabı yeniden açın, yeni bir numune kavanozu ve şahit numune kavanozu doldurun, mühürlerini girin, **Kapak sağlam** kutusunu işaretleyip **Numuneden sonra yeni mühür** alanını doldurun ve **Aç, numune al ve yeniden mühürle** düğmesine tıklayın. Yeni kavanoz (`S-{lot}-C{n}-2`, …) aynı laboratuvara gider; mühür geçmişi kabın açılıp yeniden mühürlendiğini gösterir.

## Eski bir teslimden numune alma {#sao-sample}

Kap prosedüründen önce kaydedilen teslimlerin ("eski kayıt (kaplar kaydedilmemiş)") tek bir numunesi vardır; bu numune **Numune al** sayfasıyla alınır:

1. **Numune al** sayfasını **Numuneler ve laboratuvar** sayfasından veya teslim satırından açın.
2. **Laboratuvar** seçin: yalnızca doğrulanmış laboratuvarlar listelenir.
3. Numunenin **Mühür numarası** ile **Şahit numune mühür numarası** alanlarını doldurun. İki numara farklı olmalıdır.
4. **Şahit numunenin saklandığı yer** ve **Saklama süresi sonu** alanlarını doldurun (varsayılan olarak 12 ay sonrası).
5. **Laboratuvara gönder** düğmesine ya da numuneyi daha sonra göndermek için **Sonra** düğmesine tıklayın.

<!-- SHOT: sample-form -->
Figure: Eski bir teslimden numune alma: laboratuvar ve mühür numaraları.

Numune `S-20070-1` gibi bir kod ve **Gönderildi** durumunu alır. Laboratuvar mührü bozuk bulursa numune reddedilir; bu durumda aynı lot için yeni bir numune alın.

Hatalar: "Doğrulanmış bir laboratuvar seçin.", "Numune ve şahit numune farklı mühürler gerektirir.", "Şahit numunenin nerede saklandığını girin (en çok 200 karakter).", "Şahit numunenin ne zamana kadar saklanacağını girin (ileri bir tarih).", "Numuneyi yalnızca lotun bulunduğu kurum alır.", "Numuneyi yalnızca işleme yapan bir kuruluş alır."

## KKN sayfasında çalışma {#ccp-page}

KKN verilerini giren veya onaylayan tüm roller aynı sayfayı kullanır ([Şekil](#fig-ccp-page-lot)). Sayfayı **İşlerim** sayfasından, **Kurumum** sayfasındaki KKN etiketlerinden veya bir lotun ya da partinin **Süreç akışı** sayfasından açın.

<!-- SHOT: ccp-page-lot -->
Figure: Bir lot KKN sayfası: **Evet** yanıtı zorunlu bir ek soru açar.

Sayfa şunları gösterir:

- lot (arıcı, arılık, bal türü, üretim tarihi) veya parti (parti, toplam miktar, toptancı, kaynak bileşimi);
- **Durum** göstergesi ve kaydı kimin girip kimin onayladığı;
- limitler için kullanılan bal sınıfı ("Bal sınıfı için limitler: …");
- gruplanmış parametreler; her birinin giriş alanı, **Kritik limit:** bilgisi ve kayıttan sonra **Limit içinde** veya **Limit dışında** sonucu;
- veri giremediğiniz durumlarda, gerekçesiyle birlikte bir **Salt okunur** uyarısı.

Giriş türleri:

- **Sayı**: ondalık ayırıcı olarak virgül veya nokta.
- **Evet / Hayır**: listeden seçin.
- **Metin**: en çok 100 karakter.
- **Dosya**: bir belge yükleyin (10 MB'a kadar PDF veya 8 MB'a kadar görüntü). Silmek için **Kaldır** kutusunu işaretleyin.
- **Notlar**: isteğe bağlı bir gözlem, en çok 500 karakter.

Bazı parametreler yalnızca başka bir soru belirli bir şekilde yanıtlandığında zorunludur, örneğin "Yalnızca “…” sorusu Evet yanıtlanırsa zorunludur."

**Veri girmek için**

1. Parametreleri doldurun.
2. **İzleme Kayıtlarını Kaydet** düğmesine tıklayın. "KKN … değerleri kaydedildi." mesajı kaydın yapıldığını bildirir ve **Kurumum** sayfasına dönersiniz.

Ayrı bir gönderme düğmesi yoktur. Tüm zorunlu parametrelerin değeri girildiğinde kayıt **Gönderildi** durumuna geçer ve onaylayıcıların **İşlerim** sayfasında görünür. Yalnızca bazı değerler girildiyse kayıt **Devam ediyor** durumunda kalır.

**KKN uygulanmıyorsa** (örneğin bal ısıtılmıyorsa):

1. Kayıtta henüz değer yokken "Bu KKN burada uygulanmıyor ("Uygulanamaz" olarak işaretle)" bölümünü açın.
2. **Gerekçe** alanını doldurun, örneğin "Ham bal ısıtılmaz veya pastörize edilmez".
3. **"Uygulanamaz" olarak işaretle** düğmesine tıklayın. Gerekçe, kullanıcı adınız ve zaman kaydedilir; herkese açık Bal Hikâyesi'nde "Uygulanamaz" olarak gösterilir.

Geri almak için **"Uygulanamaz" işaretini kaldır** düğmesine tıklayın. Uygulanamaz olarak işaretlenen bir KKN'nin de onaylanması gerekir; onayı, işareti koyan kişiden başka biri verir.

**Sık karşılaşılan hatalar**

Table: KKN sayfasında sık karşılaşılan hatalar {#tbl-ccp-errors}

| Mesaj | Anlamı |
|---|---|
| "…: bir sayı girin." / "…: Evet veya Hayır seçin." | Bir değerin türü yanlış. |
| "Bu KKN "Uygulanamaz" olarak işaretli. Değer kaydetmek için önce bu işareti kaldırın." | Değer girmeden önce işareti kaldırın. |
| "Bu kontrol noktası başka bir kurumun sorumluluğunda; yalnızca okuyabilirsiniz." | Bu KKN'yi okuyabilirsiniz, ancak değiştiremezsiniz. |
| "Rolünüz (…) bu kontrol noktasına veri giremez; veri giren roller: …" | Bu KKN'ye başka bir rol veri girer. |
| "Kurumunuzun durumu: Beklemede; yalnızca doğrulanmış kurumlar veri girebilir." | Kurumun doğrulanmasını bekleyin. |
| "Bu ham bal lotu kesinleşmiş … partisinde kullanıldığı için kilitli. …" | Lot mühürlenmiştir; yalnızca bir düzeltmeyle değiştirilebilir. |

## KKN 0 girişi (ön denetim) {#sao-ccp0}

1. **İşlerim** sayfasında, **Girilecek değerler** altında "KKN 0 · #[lot]" öğesini açın.
2. Kabul kontrollerini (KKN 0 parametrelerini) girin. Bazı sorular **Evet** olarak yanıtlandığında başka zorunlu sorular açılır.
3. **İzleme Kayıtlarını Kaydet** düğmesine tıklayın.

Tüm zorunlu veriler girildiğinde KKN 0 **Gönderildi** durumuna geçer ([Şekil](#fig-ccp0-submitted)) ve kurumunuzun Kalite Güvence Sorumlusu kaydı **Onaylanacak değerler** altında bulur. Kendi girdiğiniz KKN 0 kaydını onaylayamazsınız ("Dört göz: bu KKN'nin değerlerini siz girdiniz, başka bir kişinin onaylaması gerekir."). Laboratuvar ancak KKN 0 onaylandıktan sonra sonuç girebilir.

<!-- SHOT: ccp0-submitted -->
Figure: Gönderilmiş ve başka bir kişinin onayını bekleyen KKN 0.
