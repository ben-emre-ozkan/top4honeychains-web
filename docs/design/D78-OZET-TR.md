# D-78 özeti: 24 Ekim 2026 toplantısından gelen değişiklikler

Tarih: 27 Ekim 2026. Bu bir **tasarım** özetidir; uygulama, veritabanı ve canlı veriler değiştirilmedi. Ayrıntılar: [D78-SPEC.md](D78-SPEC.md). Kesinleşen kararlar `docs/DECISIONS.md` içinde **D-79** olarak kayıtlı (D-78 numarası 19 Ekim'deki yerel test ortamı kaydında kullanılmıştı).

## Kesinleşen kararlar

- **SMS gerçek olacak.** Türk bir sağlayıcıdan küçük bir paket alınacak; sağlayıcı kodda sabit değil, sunucu ayarından seçiliyor. Geliştirme ve demo için sahte sağlayıcı ve "giden kutusu" ekranı var. E-posta kapalı kalıyor; bu yüzden şifre sıfırlama da SMS ile yapılacak.
- **Arıcı kullanıcı olacak**: kendi verisini okuyacak, yazabildiği tek iş verisi balının fiyatı (rıza tercihleri ve şifresi hesap işlemi sayılır).
- **Yeni rolün adı "Compliance Officer"**; önerilen karşılıklar: TR Uyum Sorumlusu, ES Responsable de cumplimiento, PL Specjalista ds. zgodności, LV Atbilstības speciālists.

## Ne değişiyor

1. **İlçe Tarım ve Orman Müdürlüğü** yeni bir yetenekle ("Arıcı kaydı") sisteme giriyor. Oradaki **Kayıt Görevlisi** (toplantıdaki adıyla "TOP officer") arıcıyı ve arılıklarını kaydediyor.
2. **Arıcının doğrulanması:** AKS numarası biçim kontrolü (TR + il kodu + 10 hane, il kodu müdürlüğün iliyle karşılaştırılıyor), arıcının telefonuna giden **SMS kodu** ve görevlinin beyanı. Telefon yoksa imzalı form fotoğrafı yükleniyor; bunu ikinci bir kişi (Uyum Sorumlusu) onaylıyor.
3. **Aydınlatma metni ve açık rıza kayıt sırasında** alınıyor ve aynı SMS koduyla onaylanıyor. "Adım herkese açık sayfalarda görünsün" gibi isteğe bağlı rızalar ayrı ayrı evet/hayır olarak soruluyor.
4. **Arılık başlığını sistem üretiyor**: "İl – İlçe – Mevki" (ör. "Giresun – Dereli – Kümbet Yaylası"). Mevcut 71 arılığın başlığına dokunulmuyor.
5. **Kullanıcı adı ve geçici şifre SMS ile** gönderiliyor (72 saat geçerli); ilk girişte şifre değiştirmek zorunlu. Herkes ilk girişte aydınlatma metnini kabul ediyor; imza yetkisi olan roller bunu SMS koduyla onaylıyor. Her kuruluş **veri sorumlusunu** kaydediyor.
6. **Teslim alma (Intake):** bal **mühürlü kaplarla** (teneke) geliyor; her teneke ve mühür numarası kaydediliyor. Arıcıya kısa bir özetle SMS kodu gidiyor (kaç teneke, kaç kg, kaç numune); görevli kodu girince teslimat **kilitleniyor**, sonradan değiştirilemiyor. SMS birincil yol, imzalı form fotoğrafı ikincil. Teslim tarihini görevli giriyor. WhatsApp daha sonra ikinci kanal olarak eklenecek.
7. **Numune her tenekeden** alınıyor; kavanozlarda açılınca "pıt" eden **güvenlik kapağı** var. Kapağın sağlam olduğu mühürlemede ve laboratuvar kabulünde kaydediliyor. Numune → teneke → teslimat zinciri (ebeveyn–çocuk ilişkisi) arıcının SMS koduyla onayladığı içeriğin parmak iziyle (SHA-256) kanıtlanıyor.
8. **Arıcı portalı:** arılıklarım, teslimatlarım, numune ve laboratuvar durumu, balımın bulunduğu partiler, Bal Hikâyesi sayfalarım, rızalarım ve **fiyat girişi** (lot başına, kg fiyatı ve para birimi; geçmişi saklanıyor; yalnızca lotun sahibi kuruluş ve platform yöneticisi görüyor; halka açık sayfada gösterilmiyor).
9. **Uyum Sorumlusu** her kuruluşta: onay mekanizmasını ve numune kalitesini izliyor, yedek kanıtları (fotoğrafları) onaylıyor, sözleşmeleri, aydınlatma metnini ve veri sorumlusu bilgilerini yönetiyor, ilgili kişi başvurularını (KVKK 30 gün) takip ediyor.
10. **Ana sayfa logoları** iki grup: "Proje paydaşları" (9 ortak) ve "Fonlayan kuruluşlar" (şimdilik ICT-AGRI-FOOD, ajans logoları gelince).
11. **Açıklayıcı animasyonlar:** "Numune al" düğmesi kavanoz şeklinde; basınca 1,2 sn'lik bir sahne oynuyor (teneke açılıyor, kavanoz doluyor, kapak kapanıyor). Kurallar: en fazla 1,5 sn, atlanabilir, yazmayı hiç engellemez, "hareketi azalt" ayarında sabit resim gösterir.
12. **İki tanıtım filmi** (tüketici için ~75 sn, arıcı için ~85 sn): sahneler ve anlatım metni spec'te.
13. **Veri yerleşimi (KVKK/GDPR):** bu aşamada yalnızca tasarım notu; karar aşağıda.

Canlı veriler: tüm betikler ekleme yapıyor (018'den 026'ya), hiçbir satır değiştirilmiyor. Eski lotlar, teslimatlar ve numuneler "D-78 öncesi" olarak geçerli kalıyor; mevcut kullanıcılar bir sonraki girişte aydınlatma metnini kabul ediyor; eski arıcı hesapları, arıcı yeniden doğrulanana kadar kilitli kalıyor.

## Aşamalar

| Aşama | Kapsam | Betikler | Canlı veriye risk | Tahmini süre |
|---|---|---|---|---|
| P1 | Roller, arıcı kaydı (SMS / fotoğraf), aydınlatma ve rıza, kullanıcı adı-şifre SMS'i, SMS altyapısı, arılık başlığı | 018–022 | düşük | 14–18 gün |
| P2 | Teslimatta SMS kodu, mühürlü kaplar, tenekeden numune, kapak kontrolü, laboratuvar kabul kontrolleri | 023 | orta-düşük | 12–15 gün |
| P3 | Arıcı portalı ve fiyat girişi, Uyum Sorumlusu ekranları | 024–025 | düşük | 10–13 gün |
| P4 | Ana sayfa logoları, uygulama içi animasyonlar | – | yok | 6–8 gün |
| P5 | Gizlilik düzeltmeleri (C1–C6) ve PLANNED-APP-CHANGES'ten kalanlar (A1–A7, B2–B9) | 026 | düşük | 6–8 gün |
| P6 | Beş dilde kılavuz, ekran görüntüleri, canlı güncelleme paketi | – | dağıtımın kendisi | 8–10 gün |

Toplam yaklaşık 56–72 geliştirici günü. Hiçbir şey parça parça canlıya çıkmıyor; dağıtım öncesi onayınız alınacak.

**SMS paketi:** ilk yıl için yaklaşık 2.100 SMS (bilgilendirme mesajlarıyla 2.700). Öneri: **5.000'lik paket**. Gönderici adı (ör. TOP4HONEY) kaydı için sağlayıcının istediği belgeler ve mesajların "bilgilendirme" (İYS gerektirmeyen) olarak gönderilmesi satın almadan önce teyit edilmeli.

## Sizden / koordinatörden beklenen kararlar

| # | Karar | Önerimiz |
|---|---|---|
| O-1 | Kayıt rolünün adı | "Arıcı Kayıt Görevlisi" (EN "Beekeeper registration officer"); kod `RegistrationOfficer` |
| O-2 | Veri yerleşimi | Şimdilik **yalnızca Türkiye**; kayıtlara bölge etiketi; AB ortaklarıyla standart sözleşme. AB'li arıcılar gelince iki bölgeli yapı. "AB veri merkezine Türkiye şubesi" fikri hukuken yine yurt dışına aktarım sayılır. |
| O-3 | SMS sağlayıcısı | Netgsm veya İleti Merkezi; 5.000 SMS fiyatı, gönderici adı işlemleri, OTP hattı ve iletim raporuna göre seçin |
| O-4 | Her tenekenin ayrı analizi mi, karışım mı | Laboratuvar kavanozları karıştırıp lot başına analiz etsin, kavanozları saklasın; sorun çıkarsa tek teneke analizi |
| O-5/6/7 | Fiyat: lot başına mı, kim görür, halka açık mı | Lot başına "istenen fiyat"; yalnızca lot sahibi ve platform yöneticisi; Bal Hikâyesi'nde **gösterilmez** |
| O-8 | Rızası olmayan eski arıcıların adı Bal Hikâyesi'nde | Yayında nötr kod göster, rıza geldikçe ad geri gelsin; birlikler canlıya çıkmadan rıza toplasın |
| O-9 | Uyum Sorumlusu zorunlu mu | Yeni kuruluşlarda zorunlu; mevcutlara 60 gün (uyarı, engel yok) |
| O-14 | Aydınlatma metinleri ve saklama süreleri | Biz taslak yazarız, üniversitenin KVKK birimi P1'den önce onaylar |
| O-15 | Kaç sürüm | İki sürüm: önce doğrulama ve emanet zinciri (P1+P2+P5), sonra portal (P3+P4) |
| O-17 | Cep telefonu olmayan arıcı | İmzalı fotoğrafla kayıt, portal yok |

Diğer açık noktalar (O-10 üye hesaplarını kim açar, O-11 parti başlıkları, O-12 SMS onayı kimler için, O-13 yurt dışı numaralar, O-16 merkez birlik) ve gerekçeleri spec'in 10. bölümünde.
