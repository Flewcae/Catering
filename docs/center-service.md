# CenterService

Taban URL (Docker): `http://localhost:5103` — Taban URL (lokal): `http://localhost:5050`

Şimdilik sadece `Name` + `Address` tutan basit bir CRUD servisi; ileride uygulamanın geri
kalanı (kullanıcılar, ileride diğer kaynaklar) Center'a göre filtrelenecek şekilde genişleyecek.
Tüm uçlar `Authorize` gerektirir — token'ı [UserService](user-service.md) → "Giriş yap" isteğiyle
alın.

Bir Center oluşturulduğunda `CenterCreatedIntegrationEvent` Kafka'ya (`catering.center-created-events`)
yayınlanır; [UserService](user-service.md) bunu dinleyip kendi veritabanında salt-okunur bir
önbellek tutar (`DeviceToken` önbelleğiyle aynı desen) — bu sayede bir kullanıcıya Center atarken
UserService, CenterService'e senkron bir çağrı yapmadan o Center'ın var olduğunu doğrulayabilir.

## Center oluştur

Flag: `manage_centers` (bkz. [UserService](user-service.md) → "Permission flag'leri"). `SuperAdmin`/`HRAdmin` rolündeki kullanıcılar flag'e bakılmaksızın erişebilir.

```bash
curl --location 'http://localhost:5103/api/centers' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "name": "Merkez Şube",
  "address": "Atatürk Cad. No:1, İstanbul"
}'
```

Yanıt: `200 OK` + oluşturulan Center'ın `Guid` id'si.

## Center'ları listele

```bash
curl --location 'http://localhost:5103/api/centers' \
--header 'Authorization: Bearer {{accessToken}}'
```

## Tek bir Center getir

```bash
curl --location 'http://localhost:5103/api/centers/{{centerId}}' \
--header 'Authorization: Bearer {{accessToken}}'
```

Center bulunamazsa `404 Not Found` döner.
