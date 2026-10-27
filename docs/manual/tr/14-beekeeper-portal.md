# 14. Arıcı portalı {#portal}

## Portal nedir {#portal-about}

Arıcı portalı (**Arıcı alanım**), platformun arıcıya ait bölümüdür. Cep telefonu olan ve SMS koduyla kaydedilmiş bir arıcı portal hesabı alabilir. Arıcı portalda kendi arılıklarını, teslimlerini, numune kavanozlarını, laboratuvar sonuçlarını, partilerini ve Bal Hikâyesi sayfalarını okur, balının **istenen fiyatını** girer ve kendi rızalarını verir veya geri çeker.

Portal yalnızca oturum açmış arıcının kendi verilerini gösterir. Bir arıcı hesabının hiçbir kurumda üyeliği yoktur ve panelin başka hiçbir sayfasını açamaz. Portalı platform hazır olduğunda açar; o zamana kadar arıcılara giriş bilgisi verilmez.

## Oturum açma {#portal-sign-in}

1. Arıcı kayıt görevlisi giriş bilgilerini SMS ile gönderir: bir kullanıcı adı ve 72 saat geçerli geçici bir şifre.
2. Arıcı normal oturum açma sayfasında oturum açar ve kendi şifresini belirler (en az 10 karakter, bkz. [İlk oturum açışınız](#first-sign-in)).
3. Bekleyen bir aydınlatma metni varsa portal, arıcıdan önce bu metni okuyup onaylamasını ister.

Unutulan bir şifre, arıcının doğrulanmış telefonuna SMS ile gönderilen bir kodla sıfırlanır (bkz. [Şifrenizi unuttuysanız](#password-reset)).

## Ana sayfa {#portal-home}

**Ana sayfa** arıcıyı adıyla karşılar ve **Yapılacaklar** listesini ("Fiyat girilmemiş … teslimat", "Okunup onaylanacak bir aydınlatma metni"), teslim sayısını ve balının bulunduğu parti sayısını, **Son teslimat** ve **Son laboratuvar sonucu** bilgilerini gösterir. Menüde **Teslimatlarım**, **Numuneler ve laboratuvar**, **Balımın olduğu partiler**, **Bal Hikâyesi sayfalarım**, **Arılıklarım**, **Gizlilik ve onaylar** ve **Hesap** bulunur.

<!-- SHOT: portal-home -->
Figure: Telefonda arıcı portalının ana sayfası.

## Teslimleriniz ve fiyatınız {#portal-price}

**Teslimatlarım**, arıcının teslimlerini "işletmelerin kaydettiği şekilde kapları ve mühürleriyle" listeler: tarih, bal türü, miktar, teslimin nasıl onaylandığı, net ağırlıkları ve **Mühür geçmişi** ile kaplar ve bir teslim bilgisi iki yönetici tarafından düzeltildiyse "platform tarafından düzeltildi" ibaresi (arıcıya ayrıca bir SMS gider).

**İstenen fiyat.** Arıcı onaylanmış her teslim için kilogram başına bir istenen fiyat girer:

1. **Kilogram fiyatı** alanını doldurun (0'dan büyük, en çok iki ondalık hane), **Para birimi** seçin (TRY, EUR, USD, ARS veya PLN) ve dilerseniz **Not (isteğe bağlı)** alanını doldurun.
2. **Fiyat gir** (veya **Fiyatı değiştir**) düğmesine tıklayın. Sayfa şunu sorar: "Bu fiyat kaydedilsin mi? Teslimatın toplamı: …".

Fiyat, lot kesinleşmiş bir partiye girene kadar değiştirilebilir; ardından kilitlenir ("Lot sonlandırılmış bir partide; fiyatı kilitli."). Her değişiklik yeni bir giriştir; **Fiyat geçmişi** hepsini gösterir. Fiyatı yalnızca lotun sahibi kurum (kurum yöneticisi ve Uyum Sorumlusu) ve platform yöneticileri okur; arıcının birliği ise yalnızca **Birliğim fiyatlarımı görebilir** rızasıyla. Fiyat hiçbir zaman herkese açık sayfalarda görünmez.

<!-- SHOT: portal-deliveries -->
Figure: **Teslimatlarım**: kaplar, mühürler ve istenen fiyat.

## Numuneler, partiler ve Bal Hikâyesi sayfaları {#portal-samples}

- **Numuneler ve laboratuvar**, arıcının teslimlerine ait her numune kavanozunu gösterir: kap, mühür, laboratuvar, durum ve kabuldeki kapak kontrolü. "Laboratuvar değerleri, laboratuvar raporunu onayladığında görünür; yalnızca limiti doğrulanmış değerlerde sonuç gösterilir."
- **Balımın olduğu partiler**, arıcının balını içeren partileri ve bunlardaki **Payım** bilgisini, herkese açık sayfanın bağlantısıyla birlikte gösterir. Diğer arıcıların adı verilmez.
- **Bal Hikâyesi sayfalarım**, kesinleşmiş partilerin herkese açık sayfalarını listeler ve arıcının bu sayfalarda nasıl göründüğünü belirtir: "onayınızla adınız" veya "nötr bir kod, adınız gösterilmez".
- **Arılıklarım**, arılıkları il, ilçe, kovan sayısı ve konumla birlikte listeler. "Arılıklarınızın tam konumunu yalnızca siz görürsünüz."

## Gizlilik ve onaylar {#portal-privacy}

**Gizlilik ve onaylar** üç bölümden oluşur:

1. **Onaylarınız**: her rıza isteğe bağlıdır ve "Geri çekme hemen geçerli olur." **Onay ver** veya **Geri çek**:
   - **Herkese açık sayfalarda adım**: bu rıza olmadan Bal Hikâyesi ad yerine nötr bir kod gösterir;
   - **Teslimatlarım ve sonuçlarım hakkında mesajlar**: onay kodlarının yanı sıra bilgilendirme mesajları;
   - **Birliğim fiyatlarımı görebilir** (yalnızca bir birliğin üyesi için);
   - **Herkese açık arıcı sayfam**: birlik aracılığıyla verilir; burada geri çekilebilir.
2. **Onayladığınız aydınlatma metinleri**: onay tarihleriyle birlikte aydınlatma metinleri.
3. **Verilerinizi kimler tutuyor**: arıcının verilerini tutan her kurumun veri sorumlusu ve başvuru için iletişim bilgisi. Kişisel verilere ilişkin başvurular kurumun Uyum Sorumlusuna da yapılabilir (bkz. [İlgili kişi başvuruları](#co-requests)).

<!-- SHOT: portal-privacy -->
Figure: Arıcı portalında **Gizlilik ve onaylar**.

## Hesap {#portal-account}

**Hesap** sayfasında **Cep telefonu** (yeni numaraya bir kod gönderilir; numara **Doğrula** sonrasında kullanılır), **Dil** ve **Şifre** değiştirilir. Sayfada ayrıca **Animasyonları göster** anahtarı ve **Turu göster** bulunur (bkz. [Tur ve animasyonlar](#animations)).
