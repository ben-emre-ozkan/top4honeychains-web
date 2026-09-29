# TOP4HoneyChains – proje özeti (Türkçe, iç kullanım)

Proje sahibinin kendi kullanımı ve danışman sunumu için hazırlandı; ortaklarla paylaşılmaz. Ayrıntılar İngilizce dokümanlarda:
`ARCHITECTURE.md`, `FEEDBACK.md`, `DECISIONS.md`, `GO-LIVE.md`, `RELEASE-NOTES.md`. Durum: **1.0.0-rc2** (henüz canlıya alınmadı).

## 1. Sistem ne yapıyor?

Balın arıcıdan tüketiciye kadar izlenebilirliğini sağlayan bir web paneli. Tüketici, kavanozdaki QR kodu okutunca balın hangi arılıklardan geldiğini ve hangi kalite kontrollerinden (CCP) geçtiğini görür.

**Ana akış**
1. **Arıcı** (Beekeepers): kişi kaydı. Arıcının kendisi sisteme giriş yapmaz; kaydı admin tutar.
2. **Arılık** (Apiaries): arıcıya bağlı kovan yeri. Kovan sayısı ve konum (enlem/boylam) tutulur, fotoğraf eklenebilir.
3. **Ham bal üretimi** (Raw Honey Production): bir arılığın belirli bir dönem ve bal türündeki üretim lotu. Miktarı (kg) ve lotu satın alan **toptancı** kaydedilir.
4. **CCP kontrolleri** (Critical Control Points, HACCP): ham bal lotuna CCP'ler atanır ve her CCP'nin parametreleri (nem, HMF, sıcaklık vb.) ölçülüp girilir. Bir CCP'nin tüm parametreleri girilince o CCP "tamamlandı" sayılır.
5. **Toptancı** (Wholesalers): ham lotları alıp karıştıran firma.
6. **Nihai üretim / batch** (Final Honey Production): toptancı, tüm CCP'leri tamamlanmış kendi ham lotlarından hangisinden ne kadar kullanıldığını girerek bir batch oluşturur. Batch'in de kendi CCP'leri vardır.
7. **Kesinleştirme** (Finalize): batch kilitlenir ve artık değiştirilemez. Yalnızca kesinleşmiş batch'ler herkese açılır.
8. **QR / HoneyStory**: her batch'in bir QR kodu vardır. QR, `https://<alan-adı>/honeystory/{kod}` adresindeki herkese açık sayfayı açar. Bu sayfada kaynak arılıklar, arıcı adları, katkı oranları, yaklaşık konum haritası ve CCP sonuçları görünür.

**Kim neyi kullanıyor?**
- **Admin:** paneli bu sürümde yalnızca adminler kullanır (D-01). Tüm kayıtları admin girer.
- **Arıcı, toptancı:** bu sürümde giriş yapamaz. Arıcı hesapları kilitlidir (D-09); rol bazlı giriş bir sonraki sürümde gelecek (FB-33).
- **Tüketici:** giriş yapmadan sadece QR ile HoneyStory sayfasını görür.

## 2. Bu çalışmada bulunanlar ve düzeltilenler

**Güvenlik**
- Kayıt formundan kendini admin yapma açığı kapatıldı; herkese açık kayıt (sign-up) tamamen kaldırıldı.
- Admin girişine deneme sınırı eklendi: 5 hatalı denemeden sonra hesap 15 dakika kilitlenir.
- Şifre sıfırlama linki sahte `Host` başlığıyla başka bir siteye yönlendirilebiliyordu; artık yapılandırmadaki adresten üretiliyor.
- Arıcı hesaplarının şifresi işletme numarasıydı (tahmin edilebilir). Yeni hesaplar rastgele şifre alıyor, mevcut hesaplar migration script'iyle kilitleniyor.
- Tüm formlara CSRF koruması (anti-forgery) eklendi; silme ve aktif/pasif yapma işlemleri artık GET ile değil, onaylı POST ile çalışıyor.
- Dosya yükleme sunucuda kontrol ediliyor. Önceden `.html` dosyası "video" olarak yüklenip siteden sunulabiliyordu.
- Gmail şifresi, Maps key ve admin bilgileri koddan çıkarıldı. Git geçmişinde kaldıkları için canlıya geçerken değiştirilmeleri gerekiyor.
- HoneyStory sayfasındaki gizlilik sorunları giderildi: taslak batch'ler görünmüyor, arıcı fotoğrafı ve telefonu yok, konum yaklaşık (~10 km), apiary fotoğrafları ID ile toplu indirilemiyor.
- Production ortamında HTTPS yönlendirmesi ve HSTS eklendi.

**Veri doğruluğu**
- Ondalık sayılar dile bağlıydı: Türkçe arayüzde "1.5" 15 olarak kaydediliyordu. Artık her dilde "1.5" ve "1,5" 1,5 demek. Mevcut verilerde bu hatadan etkilenmiş bir değer bulunmadı.
- Boş formlar kayıt oluşturuyordu (dönem, kategori, ham bal, batch). Artık tüm formlar sunucuda doğrulanıyor ve hata olunca girilen değerler korunuyor.
- Arıcı oluşturma ve CCP ölçüm kayıtları artık ya tamamen kaydediliyor ya hiç; yarım kayıt kalmıyor.
- Batch oluşturma ve düzenleme görünmez bir doğrulama hatası yüzünden kaydedilmiyordu; düzeltildi.
- Kayıtlı batch miktarları formda boş görünüyordu (kaydedilince silinme riski vardı); düzeltildi.
- Uygun lotu olmayan bir batch kaydedilince 500 hatası çıkıyordu; düzeltildi.
- Silinen veya değiştirilen videolar diskte kalıyordu; artık temizleniyor.

**LBTU test raporu (34 bulgu: 29 düzeltildi, 5 ertelendi)**
- FB-01 şifre sıfırlama çalışmıyordu → çalışıyor, hatalar gösteriliyor.
- FB-02 "Sign up" çalışmıyordu → kayıt kapatıldı, adminler kullanıcı oluşturuyor.
- FB-19 yeni toptancı oluşturulamıyordu → düzeltildi.
- FB-20 ham lotlar batch formunda görünmüyordu → kural aynı kaldı, ama hangi lotun neden uygun olmadığı artık gösteriliyor.
- FB-23, FB-24, FB-30: 500 hataları → anlaşılır mesajlar.
- FB-11, 12, 15, 25, 31, 32: eksik doğrulamalar → eklendi.
- FB-03, 15, 18, 21, 26, 29: zorunlu alanlar → `*` ile işaretlendi.
- FB-05, 10: İptal butonu formu temizliyordu → artık listeye dönüyor.
- FB-06: lookup alanları → opsiyonel yapıldı.
- FB-07, 08, 13, 14: silme yerine pasif yapma → ekranda açıklandı.
- FB-04, 22, 28: küçük arayüz düzeltmeleri.
- FB-34: dil değiştirince 404 → düzeltildi.

## 3. Henüz yapılmayanlar ve nedenleri

- **FB-09 (haritadan konum seçme), FB-27 (CCP'ye resim ekleme), FB-33 (arıcı/toptancı rolleri):** kararla bir sonraki sürüme ertelendi (D-07, D-01).
- **FB-16, FB-17 (aynı dönem ve bal türü için birden fazla toptancı; dönem–bal türü ilişkisi):** ortaklardan cevap bekleniyor (PQ-01).
- **CCP test verisindeki tutarsızlıklar** (aynı değerin tekrarı, kaymış parametreler): kod sorunu değil, ortaklara soruldu (PQ-02).
- **İlçe/il adıyla konum gösterimi:** Google'ın reverse geocoding API'si gerektirir; şimdilik yuvarlanmış alan gösteriliyor.
- **Yönetim panelinde QR yazdırma ve public sayfayı açma bağlantısı yok:** QR sadece HoneyStory sayfasında görünüyor. Etiket basımı için eklenmesi faydalı olur.
- **Arayüz dili:** Menü ve hesap sayfaları 5 dilde; iş sayfalarının çoğu İngilizce. İspanyolca, Lehçe ve Letonca metinleri bizim çevirimiz, ortakların kontrol etmesi gerekiyor.
- **Canlıya geçiş işleri:** secret'ların değiştirilmesi, IT'den sertifika ve binding talebi, sunucu yapılandırması (5. bölüm).

## 4. 15 dakikalık demo senaryosu (yerel uygulama)

**Hazırlık (demodan önce)**
- Uygulamayı çalıştır: `dotnet run --project TOP4HONEYCHAINS/TOP4HoneyChains.Web --launch-profile http` → http://localhost:5255
- Şifre sıfırlamayı göstereceksen smtp4dev'i çalıştır (CLAUDE.md). Web arayüzü: http://localhost:5100
- Demo kayıtlarında `zz_demo` önekli sahte isimler kullan; gerçek arıcı kayıtlarını ekranda gösterme. Demodan sonra bu kayıtların silinmesini isteyebilirsin (arıcılar uygulamadan silinemez).

| # | Sayfa / işlem | Ne görmelisin |
|---|---|---|
| 1 | `/Account/Register` adresini aç | 404. Herkese açık kayıt kapalı (FB-02). |
| 2 | Login sayfasında 1–2 kez yanlış şifre gir | Genel "kullanıcı adı veya şifre hatalı" mesajı; 5 denemede hesap kilitlenir. |
| 3 | `zz_local_admin` ile giriş yap | Dashboard açılır. |
| 4 | Toptancılar → Yeni: sadece "Company Name" = `zz_demo_toptanci`, "Active" işaretli, Kaydet | Kayıt listede görünür (FB-19 eskiden bozuktu). |
| 5 | Arıcılar → Yeni: formu boş gönder, sonra "İptal"e bas | Alan bazlı hata mesajları; ad, soyad ve doğum tarihi `*` ile işaretli (FB-03). İptal formu temizlemek yerine listeye döner (FB-05). |
| 6 | Arıcılar → Yeni: `zz_demo`, `arici`, doğum tarihi, Kaydet | Liste açılır. Arama kutusuna `zz_demo` yaz; Deactivate/Activate butonlarının açıklamasını ve onay kutusunu göster (FB-07/08). |
| 7 | Arıcı satırında "Apiaries" → Yeni arılık: kovan sayısını boş bırak, enlem `95` | Boş kovan sayısı için "This field is required.", enlem için "The latitude must be between -90 and 90." (FB-11/12). Sonra enlem `41,05`, boylam `29.02`, kovan `10`, Kaydet: virgül ve nokta ikisi de doğru kaydedilir. |
| 8 | Arıcılar → Arılıklar listesinde demo arılığın üretimleri → Yeni üretim: dönem, bal türü, toptancı `zz_demo_toptanci`, miktar `120,5` | Kaydedilir. Boş gönderilirse dört alan için hata çıkar (FB-15). |
| 9 | Üretim satırında "Manage CCP" → sadece **CCP 7 Quantity Control**'ü seç, Kaydet | CCP lota atanır. |
| 10 | Üretim satırındaki CCP bağlantısından CCP 7'yi aç, 2 parametrenin ikisine de değer gir (ör. `100.5`), "Save Monitoring Logs" | CCP tamamlandı; lot artık batch için uygun. |
| 11 | Toptancılar → `zz_demo_toptanci` → Batch'ler → Yeni batch: başlık `zz_demo_batch`, Kaydet | Batch yönetim sayfası açılır. Uygun lot listede; CCP'si eksik lotlar alttaki "not yet eligible" bölümünde ve eksik CCP adıyla görünür (FB-20). |
| 12 | Lot için miktar `50` gir → "Save Batch Composition" → onayla | "Total Selected" 50.00 görünür ve miktar formda kalır. |
| 13 | "Finalize Batch" → onayla | Batch kilitlenir, uygun olmayan lotlar bölümü kaybolur. |
| 14 | QR kodunu al: `sqlcmd -S "(localdb)\MSSQLLocalDB" -d TOP4HoneyChainsDb -Q "SELECT FinalHoneyProductionQR FROM FinalHoneyProductions WHERE FinalHoneyProductionTitle='zz_demo_batch'"`, sonra `/honeystory/<kod>` adresini aç | Public sayfa: arıcı adı var, fotoğraf yok; harita pin yerine ~10 km'lik yaklaşık alan gösterir; sayfadaki QR okutulunca aynı sayfanın tam adresi çıkar. Kesinleştirme öncesi bu adres 404 verir (D-12). |
| 15 | Toptancının batch listesine dön ve dili değiştir (sağ üst) | Aynı sayfada kalır, 404 yok (FB-34). |
| 16 | Kullanıcılar → Yeni: rol seçmeden gönder | 500 yerine "Role field is required" mesajı, girilen değerler korunur (FB-23). |
| 17 | Bal Türleri → kullanımda olan bir türü silmeyi dene | "Bu kayıt kullanımda olduğu için silinemez." (FB-30) |
| 18 | Üretim Dönemleri → Yeni: bitiş tarihi başlangıçtan önce | "End date must be after the start date." (FB-32) |
| 19 | (Opsiyonel) Çıkış → "Şifremi unuttum" → `zz.local.admin@example.invalid` | smtp4dev'de reset maili görünür; link çalışır ve login sayfası onay mesajı gösterir (FB-01). |

## 5. Canlıya geçiş adımları (sırayla)

**Önceden**
1. Üniversite IT'sine talebi gönder (`GO-LIVE.md` → "Request to the university IT"): sertifika, 443 ve 80 binding'leri, alan adı.
2. Secret'ları değiştir: Gmail uygulama şifresi, Google Maps key (alan adına kısıtla), seed'de görünen admin hesabının şifresi.
3. Sunucu yapılandırmasını hazırla: ortam değişkenleri veya sadece sunucuda duran `appsettings.Production.json`. Gerekenler: veritabanı bağlantısı, SMTP, Maps key, `App__PublicBaseUrl=https://<alan-adı>`, `AllowedHosts`, `ASPNETCORE_ENVIRONMENT=Production`.
4. Main'den Release build al (0 hata) ve canlıya çıkacak commit'e `v1.0.0` tag'i koy.

**Geçiş günü**
5. Canlı veritabanının tam yedeğini al ve geri yüklenebildiğini doğrula.
6. Mevcut site klasörünü yedekle; yüklenmiş videolar da dahil.
7. Siteyi bakım moduna al (`app_offline.htm`).
8. `db/migrations/` script'lerini sırayla çalıştır. `001` script'i admin olmayan hesapları kilitler.
9. Admin rolündeki kullanıcıları kontrol et, tanımadıklarını kaldır. Sonra `001`'i tekrar çalıştır; tekrar çalıştırmak güvenli.
10. Şifresi bilinen ve yenilenmiş en az bir admin olduğundan emin ol.
11. Yeni build'i kopyala. Sunucudaki yapılandırmayı ve yüklenmiş dosyaları koru; `appsettings.Development.json` dosyasını sunucudan sil; video klasöründe `.mp4`/`.webm` dışı dosya kalmasın.
12. Data Protection anahtarlarının app pool yeniden başlatılınca kaybolmadığını kontrol et; yoksa girişler ve formlar bozulur.
13. Bakım modunu kaldır.

**Sonra (smoke test, `GO-LIVE.md` bölüm 3)**
14. HTTP adresi HTTPS'e yönleniyor mu; admin girişi ve tüm menü sayfaları açılıyor mu; `/Account/Register` 404 mü?
15. Şifre sıfırlama maili geliyor mu; linkin adresi doğru mu?
16. Kesinleşmiş bir batch'in QR'ı okutulunca HoneyStory açılıyor mu; harita görünüyor mu?
17. Sorun olursa geri dönüş: eski site klasörünü geri koy. Veritabanı yedeğini sadece script'ler zarar verdiyse geri yükle.
