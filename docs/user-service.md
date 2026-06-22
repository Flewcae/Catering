# UserService

Taban URL (Docker): `http://localhost:5101` — Taban URL (lokal): `http://localhost:5049`

Aşağıdaki örneklerde `http://localhost:5101` kullanılmıştır, kendi ortamınıza göre değiştirin.

> ⚠️ Örneklerdeki `$ADMIN_TOKEN`, `$DEPARTMENT_ID`, `$POSITION_ID`, `$ACCESS_TOKEN`, `$USER_ID`
> kabuk (shell) değişkenidir. JSON body içine literal metin olarak yapıştırmayın — aşağıdaki
> "Ortam Hazırlığı" adımındaki komutları aynı terminalde çalıştırıp değişkenleri doldurun.
> Postman kullanıyorsanız bkz. [Sorun Giderme](sorun-giderme.md).

## Ortam hazırlığı (token ve id'leri almak)

```bash
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@catering.local", "password": "Admin123!"}' \
  | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

DEPARTMENT_ID=$(curl -s http://localhost:5101/api/departments \
  -H "Authorization: Bearer $ADMIN_TOKEN" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

POSITION_ID=$(curl -s http://localhost:5101/api/positions \
  -H "Authorization: Bearer $ADMIN_TOKEN" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
```

`DEPARTMENT_ID`/`POSITION_ID` boş gelirse henüz hiç departman/pozisyon oluşturulmamış demektir —
önce aşağıdaki "Departmanlar" / "Pozisyonlar" bölümlerindeki `POST` istekleriyle oluşturun.

## Kimlik doğrulama (`/api/auth`) — hepsi `AllowAnonymous`

### Kayıt

```bash
cat > register.json << EOF
{
  "email": "ayse.yilmaz@catering.local",
  "password": "Passw0rd!",
  "firstName": "Ayse",
  "lastName": "Yilmaz",
  "tcIdentityNumber": "12345678950",
  "phoneNumber": "+905551234567",
  "birthDate": "1990-05-01",
  "address": "Istanbul",
  "departmentId": "$DEPARTMENT_ID",
  "positionId": "$POSITION_ID",
  "hireDate": "2026-01-15",
  "hasDisability": false,
  "disabilityDescription": null,
  "salaryCeiling": null,
  "notes": null
}
EOF

USER_ID=$(curl -s -X POST http://localhost:5101/api/auth/register \
  -H "Content-Type: application/json" --data-binary @register.json | tr -d '"')
```

Yanıt: `200 OK` → yeni kullanıcının `Guid` id'si. Olası hatalar: TC kimlik no algoritması
geçersizse `400`, email veya TC kimlik no zaten kayıtlıysa `409`, departman/pozisyon yoksa `404`.

`tcIdentityNumber` alanı gerçek TC kimlik no checksum algoritmasıyla doğrulanır — rastgele 11
haneli bir sayı kabul edilmez.

### Giriş

```bash
LOGIN=$(curl -s -X POST http://localhost:5101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "ayse.yilmaz@catering.local", "password": "Passw0rd!"}')

ACCESS_TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
REFRESH_TOKEN=$(echo "$LOGIN" | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)
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
curl -s -X POST http://localhost:5101/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}"
```

Kullanılan refresh token rotation ile geçersiz hâle gelir — aynı token ile ikinci kez çağrı
yapılırsa `401` döner.

### Çıkış (logout)

```bash
curl -s -X POST http://localhost:5101/api/auth/logout \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}"
```

Yanıt: `204 No Content`. Verilen refresh token'ı iptal eder.

### Şifremi unuttum (şifre sıfırlama isteği)

```bash
curl -s -X POST http://localhost:5101/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "ayse.yilmaz@catering.local", "channel": "Email"}'
```

`channel`: `Email` veya `Sms`. Yanıt her zaman `204 No Content` (email enumeration'ı önlemek
için). Email varsa, 6 haneli kod üretilir, hash'lenip 15 dakika geçerlilikle saklanır ve
`PasswordResetRequestedIntegrationEvent` (`catering.password-reset-requested-events`)
yayınlanır. NotificationService bunu henüz dinlemiyor (bkz. [Mimari](mimari.md)) — kodu görmek
için Kafka topic'ini doğrudan tüketin:

```bash
docker exec catering-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic catering.password-reset-requested-events --from-beginning
```

### Şifre sıfırlama (kod ile)

```bash
curl -s -X POST http://localhost:5101/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"email": "ayse.yilmaz@catering.local", "code": "967951", "newPassword": "ResetPassw0rd!"}'
```

Kod yanlış/süresi dolmuş/zaten kullanılmışsa `401`. Başarılı sıfırlamada tüm refresh token'lar
iptal edilir ve `PasswordChangedIntegrationEvent` yayınlanır.

## Kullanıcılar (`/api/users`) — hepsi `Authorize` gerektirir

### Kendi profilim

```bash
curl -s http://localhost:5101/api/users/me -H "Authorization: Bearer $ACCESS_TOKEN"
```

### Profilimi güncelle

```bash
curl -s -X PUT http://localhost:5101/api/users/me \
  -H "Authorization: Bearer $ACCESS_TOKEN" -H "Content-Type: application/json" \
  -d '{
    "firstName": "Ayse", "lastName": "Yilmaz-Kaya", "phoneNumber": "+905559999999",
    "address": "Ankara", "birthDate": "1990-05-01", "profilePictureUrl": null
  }'
```

Sadece kendi adı/soyadı/telefon/adres/doğum tarihi/profil resmi güncellenebilir — departman,
pozisyon, maaş, durum admin uçlarındandır.

### Şifremi değiştir

```bash
curl -s -X POST http://localhost:5101/api/users/me/change-password \
  -H "Authorization: Bearer $ACCESS_TOKEN" -H "Content-Type: application/json" \
  -d '{"currentPassword": "Passw0rd!", "newPassword": "NewPassw0rd!"}'
```

Mevcut şifre yanlışsa `401`. Başarılı değişiklikte tüm refresh token'lar iptal edilir (diğer
cihazlardan oturum kapanır) ve `PasswordChangedIntegrationEvent` yayınlanır.

### Kullanıcı listesi — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl -s http://localhost:5101/api/users -H "Authorization: Bearer $ADMIN_TOKEN"
```

### Id ile kullanıcı — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl -s http://localhost:5101/api/users/$USER_ID -H "Authorization: Bearer $ADMIN_TOKEN"
```

### İstihdam bilgilerini güncelle (admin) — `HRAdmin`, `SuperAdmin`

```bash
cat > employment.json << EOF
{
  "departmentId": "$DEPARTMENT_ID",
  "positionId": "$POSITION_ID",
  "salaryCeiling": 50000,
  "hasDisability": false,
  "disabilityDescription": null,
  "notes": "Terfi"
}
EOF

curl -s -X PUT http://localhost:5101/api/users/$USER_ID/employment-details \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  --data-binary @employment.json
```

### Kullanıcı durumunu güncelle (admin) — `HRAdmin`, `SuperAdmin`

```bash
curl -s -X PUT http://localhost:5101/api/users/$USER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"newStatus": "Suspended", "terminationDate": null}'
```

`newStatus`: `Active`, `Inactive`, `OnLeave`, `Suspended`, `Terminated`. `Active` dışına geçişte
kullanıcının tüm refresh token'ları otomatik iptal edilir (zorunlu çıkış). `Terminated`
durumunda `terminationDate` verilmezse otomatik olarak bugünün tarihi atanır.

## Departmanlar (`/api/departments`) — `Authorize` gerektirir

```bash
# Listele — herhangi bir giriş yapmış kullanıcı
curl -s http://localhost:5101/api/departments -H "Authorization: Bearer $ACCESS_TOKEN"

# Oluştur — HRAdmin, SuperAdmin
curl -s -X POST http://localhost:5101/api/departments \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name": "Bilgi Teknolojileri", "description": "Yazılım ve altyapı ekibi"}'
```

## Pozisyonlar (`/api/positions`) — `Authorize` gerektirir

```bash
# Listele — herhangi bir giriş yapmış kullanıcı
curl -s http://localhost:5101/api/positions -H "Authorization: Bearer $ACCESS_TOKEN"

# Oluştur — HRAdmin, SuperAdmin
curl -s -X POST http://localhost:5101/api/positions \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name": "Yazılım Mühendisi", "description": null}'
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
