# 12. Arıcı Kayıt Görevlisi {#role-ro}

## Günlük işiniz {#ro-daily}

**Arıcı kaydı** yeteneğine sahip bir kurum olan ilçe tarım müdürlüğünde çalışırsınız. Bölgenizdeki arıcıları ve arılıklarını kaydedersiniz; böylece tesisler onların teslimlerini kaydedebilir. Her arıcı için:

1. işletme kayıt numarasını kontrol edersiniz;
2. arıcıyı ve ilk arılığını kaydedersiniz;
3. arıcıya aydınlatma metnini okutur ve rıza tercihlerini kaydedersiniz;
4. kaydı, arıcının SMS ile aldığı kodla ya da imzalanıp fotoğrafı çekilmiş bir formla onaylarsınız;
5. portal açık olduğunda arıcıya arıcı portalının giriş bilgilerini gönderirsiniz.

Menüde **Arıcı kaydı** bulunur. Yalnızca kendi müdürlüğünüzün kayıtlarını okur ve bulursunuz. Bir platform yöneticisi de arıcıları aynı şekilde (platformun kendi kurumu adına) kaydedebilir.

## Arıcı bulma veya kaydetme {#ro-find}

1. **Arıcı kaydı** sayfasını açın. Liste, kurumunuzun kaydettiği arıcıları **Tarih**, **Kayıt numarası**, kaydın hangi **Onay yolu** ile (SMS kodu veya imzalı fotoğraf) onaylandığı ve **Kanıt** durumuyla birlikte gösterir.
2. **Kayıt numarası (AKS) veya işletme numarası** alanına numarayı girin ve **Bul veya kaydet** düğmesine tıklayın. "Kayıtlı bir arıcı yeniden doğrulanır; yeni bir kayıt numarası yeni bir kayıt başlatır."

Arıcı başka bir müdürlüğe aitse: "Bu arıcı başka bir müdürlük tarafından kaydedilmiş."

<!-- SHOT: registration-list -->
Figure: **Arıcı kaydı**: bir müdürlüğün kayıtları.

## Arıcı kaydetme {#ro-register}

**Arıcı kaydet** formu (kayıtlı bir arıcı için **Arıcıyı yeniden doğrula**) dört bölümden oluşur ([Şekil](#fig-registration-form)).

1. **Kayıt numarası**: `TR`, il plaka kodu ve 10 hane, örneğin `TR281234500004` ("TR, il plaka kodu ve 10 hane. Müdürlüğün iliyle karşılaştırılır."). Numara başka bir ile aitse gerekçeyi **Numara başka bir ile aitse: gerekçe** alanına yazın.
2. **Arıcı**: **Ad**, **Soyad**, **Doğum Tarihi**, **Cep telefonu**, **SMS dili** ve isteğe bağlı olarak eğitim seviyesi, arıcılık amacı ve arıcılık türü. Arıcının cep telefonu yoksa **Arıcının cep telefonu yok (imzalı fotoğraf, hesap yok)** kutusunu işaretleyin.
3. **Arılık**: ilk arılığın yeri (arılığa adını da veren il, ilçe ve mahalle/köy), konumu ve kovan sayısı. Diğer arılıklar daha sonra kayıt sayfasında eklenebilir.
4. **Aydınlatma metni ve rıza**: "Bu metni arıcıya okutun veya yüksek sesle okuyun". Ardından arıcının tercihlerini kaydedin:
   - **Arıcı, adının herkese açık sayfalarda gösterilmesini kabul ediyor (isteğe bağlı; aksi halde nötr kod gösterilir)**;
   - **Arıcı, teslimatlar ve analiz sonuçları hakkında SMS bildirimlerini kabul ediyor (isteğe bağlı)**.
   **Rıza metinlerinin tamamı** metinleri açar.
5. **Beyanınız**: **Arıcının kimlik belgesini ve kayıt bilgisini gördüm.** kutusunu işaretleyin. Arıcının şimdi bir portal hesabı alması gerekiyorsa **Arıcıya giriş bilgilerini de SMS ile gönder** kutusunu işaretleyin (yalnızca portal açıkken gösterilir).

<!-- SHOT: registration-form -->
Figure: Arıcı kaydetme: kayıt numarası, kimlik, arılık, aydınlatma metni ve rızalar.

### SMS koduyla onaylama {#ro-sms}

1. **Onay** altında **Kod gönder** düğmesine tıklayın. "Arıcıya kuruluşunuzun adı, metin bağlantısı, ad tercihi ve bir kod içeren SMS gider. Sonradan bir alan değişirse yeni kod gerekir."
2. Arıcının okuduğu kodu **SMS'teki kod (6 hane)** alanına girin ve **Kaydı onayla** düğmesine tıklayın.

"Arıcı kaydedildi ve doğrulandı." Kayıt onaylanan içerikle birlikte saklanır, rızalar geçerli olur ve arıcı hemen teslim yapabilir. Kod kuralları teslimdeki gibidir: 5 dakika geçerli, 3 deneme, bir dakika sonra yeni kod.

### SMS mümkün değil: imzalı fotoğraf {#ro-photo}

Arıcının cep telefonu yoksa veya SMS gönderilemiyorsa:

1. **SMS mümkün değil: imzalı fotoğraf** bölümünü açın ve **Formu yazdır** düğmesine tıklayın. Basılı sayfada arıcının tercihleri ve şu satır bulunur: "Aydınlatma metnini okudum; yukarıdaki tercihlerim doğrudur. Arıcının imzası ve tarih:".
2. Formu arıcıya imzalatın, fotoğrafını çekin, **İmzalı formun fotoğrafı** altında seçin ve **SMS kodu neden kullanılamıyor** alanını doldurun.
3. **İmzalı fotoğrafla kaydet** düğmesine tıklayın: "Kayıt imzalı fotoğrafla kaydedildi. Uyum Sorumlusunun incelemesini bekliyor."

Müdürlüğünüzün Uyum Sorumlusu fotoğrafı kabul edene kadar arıcı teslim yapamaz; reddedilen bir fotoğraf kayıttaki rızaları da geri çeker. Kendi girdiğiniz bir kaydı inceleyemezsiniz. Cep telefonu olmayan bir arıcı portal hesabı alamaz.

## Kayıt sayfası {#ro-details}

Açmak için listede bir kayda tıklayın. Sayfa kaydı, **Kayıt numarası** (farklıysa "başka il" ibaresiyle), **Cep telefonu**, **Rızalar** (geri alınan bir rıza için "geri alındı" ibaresiyle), **Kanıt** ve incelemesi ile kovanlarıyla birlikte **Arılıklar** bilgilerini gösterir.

- **Arılık ekle** arıcının başka bir arılığını kaydeder.
- **Giriş bilgilerini SMS ile gönder** arıcıya arıcı portalı için bir kullanıcı adı ve geçici bir şifre gönderir (yalnızca portal açıkken ve yalnızca SMS koduyla onaylanmış bir kayıt için: "Giriş için SMS koduyla onaylanmış bir kayıt gerekir (cep telefonu olmadan hesap yok)."). Arıcı kendi şifresini ilk oturum açışında belirler.

<!-- SHOT: registration-details -->
Figure: Rızaları ve arılıklarıyla onaylanmış bir kayıt.

Tasdik edilen bilgiler (kimlik, kayıt numarası, telefon), yöneticilerin arıcı sayfasında da salt okunurdur; yalnızca aynı arıcının yeniden onaylanan yeni bir kaydıyla (yeniden doğrulama) değişir.

**Sık karşılaşılan hatalar**

Table: Arıcı kaydederken sık karşılaşılan hatalar {#tbl-registration-errors}

| Mesaj | Ne yapmalı |
|---|---|
| "Kayıt numarasını (veya kayıtlı bir arıcının işletme numarasını) girin." | **Bul veya kaydet** düğmesine tıklamadan önce bir numara girin. |
| "Numara TR, 01-81 arası il kodu ve 10 haneden oluşmalı (ör. TR061234567890)." | Kayıt numarasını düzeltin. |
| "Bu arıcı başka bir müdürlük tarafından kaydedilmiş." | O müdürlüğe başvurun. |
| "Yanlış kod. … deneme hakkı kaldı." / "Kodun süresi doldu. Yeni bir kod gönderin." | Arıcıdan kodu yeniden okumasını isteyin veya yeni bir kod gönderin. |
| "Kod gönderildikten sonra bilgiler değişti. Yeni bir kod gönderin." | Kodu gönderdikten sonra bir alanı değiştirdiniz. |
| "Bu telefon numarasına çok fazla kod gönderildi. Daha sonra tekrar deneyin." | Bir telefon numarasına saatte en çok 5, günde en çok 10 kod gider. |
