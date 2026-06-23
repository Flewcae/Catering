# Sorun Giderme

## "The JSON value could not be converted... Path: $.departmentId"

**Sebep:** İstek body'sindeki `departmentId`/`positionId` alanına gerçek bir GUID yerine
örnek metindeki placeholder (`<departman-id>` gibi) ya da geçersiz bir değer yapıştırılmış.
System.Text.Json bunu `Guid`'e çeviremediği için tüm body geçersiz sayılır (`command` alanı da
"required" görünür çünkü deserialization tamamen başarısız olur).

**Çözüm:** Önce gerçek bir departman/pozisyon id'si alın:

```bash
curl --location '{{baseUrl}}/api/departments' \
--header 'Authorization: Bearer {{adminToken}}'
```

```bash
curl --location '{{baseUrl}}/api/positions' \
--header 'Authorization: Bearer {{adminToken}}'
```

ve bu id'leri kullanın. [UserService](user-service.md) sayfasındaki örnekler, bu id'leri
Postman "Tests" scriptiyle otomatik olarak `departmentId`/`positionId` collection variable'larına
yazan bir zincir içerir.

## "TC Identity Number is not valid."

**Sebep:** `tcIdentityNumber` rastgele 11 haneli bir sayı olamaz — gerçek TC kimlik no checksum
algoritmasını geçmesi gerekir.

**Çözüm:** Test için gerçek bir checksum'ı geçen bir numara üretin veya bilinen geçerli bir test
numarası kullanın (örnek: `98765432150`).

## Postman: "Error: Invalid character in header content [\"Authorization\"]"

**Sebep:** Login yanıtındaki JSON'dan `accessToken` değerini kopyalarken, değerin etrafındaki
tırnak işareti, virgül veya bir sonraki satıra taşan içerik (`"accessTokenExpiresAt": "..."`)
de yanlışlıkla header değerine dahil edilmiş. Header değerinde gizli bir satır sonu (`\n`)
karakteri olduğunda Postman bu hatayı verir.

**Çözüm — kalıcı:** Token'ı elle kopyalamayın. Login isteğine bir "Tests" / "Post-response"
scripti ekleyip otomatik bir collection variable'a yazdırın:

```javascript
const body = pm.response.json();
pm.collectionVariables.set("accessToken", body.accessToken);
pm.collectionVariables.set("refreshToken", body.refreshToken);
```

Diğer isteklerde Authorization sekmesinden `Type: Bearer Token` seçip değer kutusuna
`{{accessToken}}` yazın.

**Çözüm — geçici:** Header değer kutusunu tamamen silip (Ctrl+A, Delete), sadece üç nokta ile
ayrılmış JWT'yi (`eyJ...` ile başlayan) yeniden yapıştırın — kaynağında satır kaydırma veya
gizli karakter olmadığından emin olun.

## Kafka "Unknown topic or partition" / "Subscribed topic not available"

**Sebep:** Consumer, henüz hiçbir producer tarafından oluşturulmamış bir topic'e abone olmaya
çalışıyor.

**Çözüm:** `KafkaConsumerBackgroundService`, `Subscribe` çağrısından önce `AdminClient` ile
topic'i otomatik oluşturur — bu davranış zaten kodda mevcuttur. Hâlâ bu hatayı görüyorsanız
Kafka container'ının tamamen ayağa kalkmış olduğundan emin olun (`docker compose logs kafka`).

## Npgsql: "Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'"

**Sebep:** JSON'dan deserialize edilen `DateTime` değerlerinin `Kind`'ı `Unspecified` olur;
PostgreSQL'in `timestamptz` kolonu sadece UTC kabul eder.

**Çözüm:** Takvim-only tarih alanları (`BirthDate`, `HireDate`, `TerminationDate`) zaten
`DateOnly`/`DateOnly?` tipindedir — bu sorunu kökten ortadan kaldırır, ek bir işlem
gerekmez.

## Veritabanı şeması eski/uyumsuz görünüyor

**Sebep:** `EnsureCreatedAsync` var olan bir veritabanını migrate etmez. Domain modelinde
yapısal değişiklik yapıldıktan sonra eski volume'deki veritabanı şemasıyla uyumsuzluk oluşur.

**Çözüm:** `docker compose down -v` ile volume'leri silip yeniden `docker compose up --build`
çalıştırın (tüm veriler silinir).
