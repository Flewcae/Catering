# UserService

Taban URL (Docker): `http://localhost:5101` — Taban URL (lokal): `http://localhost:5049`

Aşağıdaki tüm istekler Postman'da kullanılmak üzere hazırlanmıştır: her biri tek başına
çalışan, eksiksiz bir `curl` komutudur — doğrudan kopyalayıp Postman'da **Import → Raw text**
ile yapıştırabilir veya bir isteği düzenlerken **Code** panelinden "cURL" formatını seçip
üzerine yapıştırabilirsiniz.

URL `http://localhost:5101` olarak gömülüdür — Docker dışında lokal çalıştırıyorsanız
`http://localhost:5049` ile değiştirin.

> ⚠️ Komutlardaki `{{adminToken}}`, `{{accessToken}}`, `{{refreshToken}}`, `{{departmentId}}`,
> `{{positionId}}`, `{{userId}}` Postman **collection variable**'larıdır (token/id'ler her
> çalıştırmada değiştiği için bunlar sabit yazılamaz). Postman bunları yalnızca istek Postman
> üzerinden gönderildiğinde çözer (bir terminalde çalıştırırsanız literal metin olarak
> gönderilir). Kurulum için aşağıdaki "Postman değişkenleri" bölümüne ve
> [Sorun Giderme](sorun-giderme.md) sayfasına bakın.

## Postman değişkenleri

Collection'ınızda (Collection → ... → Edit → Variables) şu değişkenleri tanımlayın:

| Değişken | Örnek değer |
|---|---|
| `adminToken` | (boş — aşağıdaki admin girişiyle otomatik dolar) |
| `accessToken` | (boş — kullanıcı girişiyle otomatik dolar) |
| `refreshToken` | (boş — kullanıcı girişiyle otomatik dolar) |
| `departmentId` | (boş — departman listesiyle otomatik dolar) |
| `positionId` | (boş — pozisyon listesiyle otomatik dolar) |
| `userId` | (boş — kayıt isteğiyle otomatik dolar) |

Her isteğin altındaki **Tests** kodunu o isteğin Postman "Tests" (veya "Post-response") sekmesine
ekleyin — yanıt geldiğinde ilgili değişkeni otomatik doldurur, böylece bir sonraki isteği elle
düzenlemeniz gerekmez.

### Admin girişi (id'leri toplamak için)

```bash
curl --location 'http://localhost:5101/api/auth/login' \
--header 'Content-Type: application/json' \
--data '{
  "email": "admin@catering.local",
  "password": "Admin123!"
}'
```

Tests:

```javascript
const body = pm.response.json();
pm.collectionVariables.set("adminToken", body.accessToken);
```

### Departman id'si al

```bash
curl --location 'http://localhost:5101/api/departments' \
--header 'Authorization: Bearer {{adminToken}}'
```

Tests:

```javascript
const body = pm.response.json();
if (body.length > 0) pm.collectionVariables.set("departmentId", body[0].id);
```

Liste boş gelirse önce aşağıdaki "Departmanlar" bölümündeki `POST` isteğiyle bir departman
oluşturun.

### Pozisyon id'si al

```bash
curl --location 'http://localhost:5101/api/positions' \
--header 'Authorization: Bearer {{adminToken}}'
```

Tests:

```javascript
const body = pm.response.json();
if (body.length > 0) pm.collectionVariables.set("positionId", body[0].id);
```

Liste boş gelirse önce aşağıdaki "Pozisyonlar" bölümündeki `POST` isteğiyle bir pozisyon
oluşturun.

## Kimlik doğrulama (`/api/auth`) — hepsi `AllowAnonymous`

### Kayıt

```bash
curl --location 'http://localhost:5101/api/auth/register' \
--header 'Content-Type: application/json' \
--data '{
  "email": "ayse.yilmaz@catering.local",
  "password": "Passw0rd!",
  "firstName": "Ayse",
  "lastName": "Yilmaz",
  "tcIdentityNumber": "12345678950",
  "phoneNumber": "+905551234567",
  "birthDate": "1990-05-01",
  "address": "Istanbul",
  "departmentId": "{{departmentId}}",
  "positionId": "{{positionId}}",
  "hireDate": "2026-01-15",
  "hasDisability": false,
  "disabilityDescription": null,
  "salaryCeiling": null,
  "notes": null
}'
```

Tests:

```javascript
pm.collectionVariables.set("userId", pm.response.text().replace(/"/g, ""));
```

Yanıt: `200 OK` → yeni kullanıcının `Guid` id'si. Olası hatalar: TC kimlik no algoritması
geçersizse `400`, email veya TC kimlik no zaten kayıtlıysa `409`, departman/pozisyon yoksa `404`.

`tcIdentityNumber` alanı gerçek TC kimlik no checksum algoritmasıyla doğrulanır — rastgele 11
haneli bir sayı kabul edilmez.

### Giriş

```bash
curl --location 'http://localhost:5101/api/auth/login' \
--header 'Content-Type: application/json' \
--data '{
  "email": "ayse.yilmaz@catering.local",
  "password": "Passw0rd!"
}'
```

Tests:

```javascript
const body = pm.response.json();
pm.collectionVariables.set("accessToken", body.accessToken);
pm.collectionVariables.set("refreshToken", body.refreshToken);
```

Yanıt:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "accessTokenExpiresAt": "2026-06-22T13:00:45.48Z",
  "refreshToken": "eorPg4OJ3D77mdORmG60PR7W6Tv8z79f...",
  "user": { "id": "ad163248-...", "email": "ayse.yilmaz@catering.local", "role": "Employee" }
}
```

`accessToken` 60 dakika geçerlidir, JWT içinde `role`, `departmentId`, `positionId` claim'leri
bulunur. `refreshToken` 7 gün geçerlidir ve veritabanında hash'lenerek saklanır.

5 başarısız giriş denemesi sonrası hesap 15 dakika kilitlenir (`401`). Hesap `Active` dışında bir
durumdaysa (`Suspended`, `Terminated` vb.) giriş `401` döner.

### Access token yenileme (refresh)

```bash
curl --location 'http://localhost:5101/api/auth/refresh-token' \
--header 'Content-Type: application/json' \
--data '{
  "refreshToken": "{{refreshToken}}"
}'
```

Kullanılan refresh token rotation ile geçersiz hâle gelir — aynı token ile ikinci kez çağrı
yapılırsa `401` döner.

### Çıkış (logout)

```bash
curl --location 'http://localhost:5101/api/auth/logout' \
--header 'Content-Type: application/json' \
--data '{
  "refreshToken": "{{refreshToken}}"
}'
```

Yanıt: `204 No Content`. Verilen refresh token'ı iptal eder.

### Şifremi unuttum (şifre sıfırlama isteği)

```bash
curl --location 'http://localhost:5101/api/auth/forgot-password' \
--header 'Content-Type: application/json' \
--data '{
  "email": "ayse.yilmaz@catering.local",
  "channel": "Email"
}'
```

`channel`: `Email` veya `Sms`. Yanıt her zaman `204 No Content` (email enumeration'ı önlemek
için). Email varsa, 6 haneli kod üretilir, hash'lenip 15 dakika geçerlilikle saklanır ve
`PasswordResetRequestedIntegrationEvent` (`catering.password-reset-requested-events`)
yayınlanır. NotificationService bunu henüz dinlemiyor (bkz. [Mimari](mimari.md)) — kodu görmek
için Kafka topic'ini doğrudan tüketin (bu bir HTTP isteği değildir, terminalde çalıştırılır):

```bash
docker exec catering-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic catering.password-reset-requested-events --from-beginning
```

### Şifre sıfırlama (kod ile)

```bash
curl --location 'http://localhost:5101/api/auth/reset-password' \
--header 'Content-Type: application/json' \
--data '{
  "email": "ayse.yilmaz@catering.local",
  "code": "967951",
  "newPassword": "ResetPassw0rd!"
}'
```

Kod yanlış/süresi dolmuş/zaten kullanılmışsa `401`. Başarılı sıfırlamada tüm refresh token'lar
iptal edilir ve `PasswordChangedIntegrationEvent` yayınlanır.

## Kullanıcılar (`/api/users`) — hepsi `Authorize` gerektirir

### Kendi profilim

```bash
curl --location 'http://localhost:5101/api/users/me' \
--header 'Authorization: Bearer {{accessToken}}'
```

### Profilimi güncelle

```bash
curl --location --request PUT 'http://localhost:5101/api/users/me' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "firstName": "Ayse",
  "lastName": "Yilmaz-Kaya",
  "phoneNumber": "+905559999999",
  "address": "Ankara",
  "birthDate": "1990-05-01",
  "profilePictureUrl": null
}'
```

Sadece kendi adı/soyadı/telefon/adres/doğum tarihi/profil resmi güncellenebilir — departman,
pozisyon, maaş, durum admin uçlarındandır.

### Şifremi değiştir

```bash
curl --location 'http://localhost:5101/api/users/me/change-password' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "currentPassword": "Passw0rd!",
  "newPassword": "NewPassw0rd!"
}'
```

Mevcut şifre yanlışsa `401`. Başarılı değişiklikte tüm refresh token'lar iptal edilir (diğer
cihazlardan oturum kapanır) ve `PasswordChangedIntegrationEvent` yayınlanır.

### Kullanıcı listesi — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl --location 'http://localhost:5101/api/users' \
--header 'Authorization: Bearer {{adminToken}}'
```

### Id ile kullanıcı — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl --location 'http://localhost:5101/api/users/{{userId}}' \
--header 'Authorization: Bearer {{adminToken}}'
```

### İstihdam bilgilerini güncelle (admin) — `HRAdmin`, `SuperAdmin`

```bash
curl --location --request PUT 'http://localhost:5101/api/users/{{userId}}/employment-details' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "departmentId": "{{departmentId}}",
  "positionId": "{{positionId}}",
  "salaryCeiling": 50000,
  "hasDisability": false,
  "disabilityDescription": null,
  "notes": "Terfi"
}'
```

### Kullanıcı durumunu güncelle (admin) — `HRAdmin`, `SuperAdmin`

```bash
curl --location --request PUT 'http://localhost:5101/api/users/{{userId}}/status' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "newStatus": "Suspended",
  "terminationDate": null
}'
```

`newStatus`: `Active`, `Inactive`, `OnLeave`, `Suspended`, `Terminated`. `Active` dışına geçişte
kullanıcının tüm refresh token'ları otomatik iptal edilir (zorunlu çıkış). `Terminated`
durumunda `terminationDate` verilmezse otomatik olarak bugünün tarihi atanır.

## Departmanlar (`/api/departments`) — `Authorize` gerektirir

### Listele — herhangi bir giriş yapmış kullanıcı

```bash
curl --location 'http://localhost:5101/api/departments' \
--header 'Authorization: Bearer {{accessToken}}'
```

### Oluştur — `HRAdmin`, `SuperAdmin`

```bash
curl --location 'http://localhost:5101/api/departments' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "name": "Bilgi Teknolojileri",
  "description": "Yazılım ve altyapı ekibi"
}'
```

## Pozisyonlar (`/api/positions`) — `Authorize` gerektirir

### Listele — herhangi bir giriş yapmış kullanıcı

```bash
curl --location 'http://localhost:5101/api/positions' \
--header 'Authorization: Bearer {{accessToken}}'
```

### Oluştur — `HRAdmin`, `SuperAdmin`

```bash
curl --location 'http://localhost:5101/api/positions' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "name": "Yazılım Mühendisi",
  "description": null
}'
```

## Hata formatı

Hatalar [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110) `ProblemDetails` formatında döner:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "A user with this TC Identity Number already exists.",
  "instance": "/api/auth/register"
}
```

| Durum kodu | Sebep |
|---|---|
| `400` | Doğrulama hatası (örn. geçersiz TC kimlik no) |
| `401` | Kimlik doğrulama hatası (yanlış şifre, geçersiz/süresi dolmuş token, kilitli/aktif olmayan hesap) |
| `403` | Yetkisiz erişim (rol uygun değil) |
| `404` | Kayıt bulunamadı (kullanıcı, departman, pozisyon) |
| `409` | Çakışma (email veya TC kimlik no zaten kayıtlı) |
| `500` | Beklenmeyen hata — detay döndürülmez |
