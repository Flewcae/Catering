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
| `userId` | (boş — "Kullanıcı hesabı oluştur" isteğiyle otomatik dolar) |

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

Self-service kayıt **yok** — hiç kimse kendi hesabını oluşturamaz. Hesaplar yalnızca
`create_account` flag'ine sahip bir kullanıcı tarafından, aşağıdaki "Kullanıcılar" →
"Kullanıcı hesabı oluştur" ucuyla açılır (bkz. orada şifrenin nasıl üretilip iletildiği).

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

## Permission flag'leri

`Authorize` gerektiren her uç, aşağıdaki tabloda görülen flag'lerden biriyle korunur
(`SuperAdmin`/`HRAdmin` rolündeki kullanıcılar flag'e bakılmaksızın her zaman geçer — bkz.
[Mimari](mimari.md) → "Yetkilendirme: rol + permission flag"). Bir flag'i bir Position'a atamak
için aşağıdaki "Pozisyonlar" → "Permission flag'lerini güncelle" ucunu kullanın.

| Flag | Korunan uç |
|---|---|
| `create_account` | `POST /api/users` |
| `delete_user` | `DELETE /api/users/{id}` |
| `view_users` | `GET /api/users`, `GET /api/users/{id}` |
| `update_self_profile` | `PUT /api/users/me` |
| `update_user_profile` | `PUT /api/users/{id}/profile` |
| `update_employment_details` | `PUT /api/users/{id}/employment-details` |
| `update_user_status` | `PUT /api/users/{id}/status` |
| `assign_user_center` | `PUT /api/users/{id}/center` |
| `manage_departments` | `POST /api/departments` |
| `manage_positions` | `POST /api/positions` |
| `manage_centers` | [CenterService](center-service.md) → `POST /api/centers` |
| `send_custom_email` / `send_custom_sms` / `send_custom_push_notification` / `view_user_notification` | bkz. [NotificationService](notification-service.md) |

`PUT /api/positions/{id}/permissions` (flag atama ucunun kendisi) **kasıtlı olarak flag'siz** —
sadece `HRAdmin`/`SuperAdmin` rolüyle korunur. Bunun bir flag'i olsaydı, o flag'e sahip biri
kendi Position'ına istediği herhangi bir flag'i (örn. `delete_user`) ekleyip yetkisini
yükseltebilirdi; bu yüzden yetki *veren* uç, yetkilerin kendisinden bağımsız tutulur.

Gerçek kullanımda, hesabı olmayan bir kullanıcı için hiç flag yoktur — Position'lara hangi
flag'lerin atanacağına siz karar verirsiniz (örn. bir "İK Uzmanı" pozisyonuna `create_account` +
`view_users`, bir "Saha Yöneticisi" pozisyonuna sadece `view_users` + `update_employment_details`
gibi).

## Kullanıcılar (`/api/users`) — hepsi `Authorize` gerektirir

### Kendi profilim

```bash
curl --location 'http://localhost:5101/api/users/me' \
--header 'Authorization: Bearer {{accessToken}}'
```

Yanıttaki `passwordRegistered: false` ise kullanıcı hâlâ admin tarafından oluşturulan geçici
şifreyi kullanıyor demektir (bkz. "Kullanıcı hesabı oluştur" altında) — ilerideki frontend bu
durumda kullanıcıyı doğrudan bir "şifre belirleme" ekranına yönlendirecek. `POST
/me/change-password` ile şifresini değiştirdiğinde bu alan otomatik `true` olur.

### Profilimi güncelle

Flag: `update_self_profile`.

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

Flag yok — her giriş yapmış kullanıcı kendi şifresini her zaman değiştirebilmeli (özellikle admin
tarafından verilen geçici şifreyi ilk girişte değiştirmek için bu uç gerekli, bu yüzden bilerek
flag'siz bırakıldı).

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
cihazlardan oturum kapanır), `passwordRegistered` `true`'ya çekilir ve
`PasswordChangedIntegrationEvent` yayınlanır.

### Kullanıcı hesabı oluştur (admin)

Flag: `create_account`.

```bash
curl --location 'http://localhost:5101/api/users' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "email": "ayse.yilmaz@catering.local",
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
`tcIdentityNumber` alanı gerçek TC kimlik no checksum algoritmasıyla doğrulanır.

Body'de **şifre yok** — sunucu rastgele, güçlü bir geçici şifre üretir, kullanıcıyı
`passwordRegistered: false` olarak oluşturur ve `UserCreatedIntegrationEvent`
(`catering.user-events`) ile bu geçici şifreyi [NotificationService](notification-service.md)'e
iletir; o da kullanıcıya hem email hem SMS olarak gönderir (bkz. NotificationService →
"UserCreated tüketimi"). Kullanıcı bu şifreyle giriş yapıp `POST /me/change-password` ile kendi
şifresini belirlediğinde `passwordRegistered` `true` olur.

### Kullanıcıyı sil

Flag: `delete_user`.

```bash
curl --location --request DELETE 'http://localhost:5101/api/users/{{userId}}' \
--header 'Authorization: Bearer {{adminToken}}'
```

Yanıt: `204 No Content`. Kalıcı (hard) silme — kullanıcı bulunamazsa `404`. Kullanıcıya bağlı
refresh token'lar, şifre sıfırlama istekleri ve cihaz token'ları cascade ile birlikte silinir.

### Kullanıcı listesi

Flag: `view_users`.

```bash
curl --location 'http://localhost:5101/api/users' \
--header 'Authorization: Bearer {{adminToken}}'
```

### Id ile kullanıcı

Flag: `view_users`.

```bash
curl --location 'http://localhost:5101/api/users/{{userId}}' \
--header 'Authorization: Bearer {{adminToken}}'
```

### Başka bir kullanıcının profilini güncelle (admin)

Flag: `update_user_profile`.

```bash
curl --location --request PUT 'http://localhost:5101/api/users/{{userId}}/profile' \
--header 'Authorization: Bearer {{adminToken}}' \
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

"Profilimi güncelle" ile aynı alanlar/aynı komut — tek fark, başkasının `id`'sini hedefleyip
`update_self_profile` yerine `update_user_profile` flag'i gerektirmesi.

### İstihdam bilgilerini güncelle (admin)

Flag: `update_employment_details`.

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

### Kullanıcı durumunu güncelle (admin)

Flag: `update_user_status`.

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

### Kullanıcıyı bir Center'a ata (admin)

Flag: `assign_user_center`.

```bash
curl --location --request PUT 'http://localhost:5101/api/users/{{userId}}/center' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "centerId": "{{centerId}}"
}'
```

`centerId` [CenterService](center-service.md)'te oluşturulmuş bir Center'ın id'si olmalı —
UserService bunu kendi yerel önbelleğine (`CenterCreatedIntegrationEvent` ile doldurulan
`centers_cache` tablosu) bakarak doğrular; önbellekte yoksa `404` döner. `null` göndererek
kullanıcının Center atamasını kaldırabilirsiniz. Yanıt: `204 No Content`. Atanan Center, bir
sonraki login/refresh'te JWT'de `centerId` claim'i olarak taşınır.

### Cihaz token'ı kaydet (Firebase push)

```bash
curl --location 'http://localhost:5101/api/users/me/device-tokens' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "token": "device-token-123",
  "platform": "android"
}'
```

Yanıt: `204 No Content`. Aynı token başka bir kullanıcıya kayıtlıysa (örn. paylaşılan bir cihaz)
sahibi otomatik olarak çağıran kullanıcıya güncellenir. Başarılı kayıt
`DeviceTokenRegisteredIntegrationEvent` (`catering.device-token-registered-events`) yayınlar —
[NotificationService](notification-service.md) bunu dinleyip kendi önbelleğine yazar, böylece
`userId` ile push gönderimi (bkz. NotificationService → "Push bildirim gönder (UserId ile)")
çalışabilir.

### Cihaz token'ını iptal et

```bash
curl --location 'http://localhost:5101/api/users/me/device-tokens/revoke' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "token": "device-token-123"
}'
```

Yanıt: `204 No Content` (token kayıtlı değilse de aynı yanıt döner). Uygulamadan çıkış yapan
cihazlarda çağrılması önerilir; `DeviceTokenRevokedIntegrationEvent` yayınlanır.

## Departmanlar (`/api/departments`) — `Authorize` gerektirir

### Listele — herhangi bir giriş yapmış kullanıcı

```bash
curl --location 'http://localhost:5101/api/departments' \
--header 'Authorization: Bearer {{accessToken}}'
```

### Oluştur

Flag: `manage_departments`.

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

### Oluştur

Flag: `manage_positions`.

```bash
curl --location 'http://localhost:5101/api/positions' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "name": "Yazılım Mühendisi",
  "description": null
}'
```

### Permission flag'lerini güncelle

Flag yok — kasıtlı olarak sadece `HRAdmin`/`SuperAdmin` rolüyle korunur (bkz. yukarıdaki
"Permission flag'leri" bölümündeki gerekçe).

```bash
curl --location --request PUT 'http://localhost:5101/api/positions/{{positionId}}/permissions' \
--header 'Authorization: Bearer {{adminToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "permissions": ["send_custom_email", "view_user_notification"]
}'
```

Yanıt: `204 No Content`. Flag'ler serbest biçimlidir (kanonik bir listeye karşı doğrulanmaz) —
sadece `^[a-z0-9_]+$` formatına uymalı, geçersiz/boş olanlar sessizce elenir, tekrarlar
tekilleştirilir. Bu Position'a sahip bir kullanıcı login olduğunda (veya token'ını yenilediğinde)
JWT'sinde her flag için bir `permission` claim'i taşır — bkz. [NotificationService](notification-service.md)
örneğin `send_custom_email` flag'ini `POST /api/notifications/email` ucunu korumak için kullanır.
`SuperAdmin`/`HRAdmin` rolündeki kullanıcılar Position'larında flag olmasa da tüm flag-korumalı
uçlara erişebilir.

## Hata formatı

Hatalar [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110) `ProblemDetails` formatında döner:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "A user with this TC Identity Number already exists.",
  "instance": "/api/users"
}
```

| Durum kodu | Sebep |
|---|---|
| `400` | Doğrulama hatası (örn. geçersiz TC kimlik no) |
| `401` | Kimlik doğrulama hatası (yanlış şifre, geçersiz/süresi dolmuş token, kilitli/aktif olmayan hesap) |
| `403` | Yetkisiz erişim (rol uygun değil veya gerekli permission flag'i yok) |
| `404` | Kayıt bulunamadı (kullanıcı, departman, pozisyon) |
| `409` | Çakışma (email veya TC kimlik no zaten kayıtlı) |
| `500` | Beklenmeyen hata — detay döndürülmez |
