# Catering

.NET 10 üzerinde kurulu, CQRS mimarisi ve Kafka tabanlı olay (event) iletişimi kullanan
mikroservis tabanlı bir catering ERP/CRM altyapısı. Her servis kendi veritabanına sahiptir,
servisler arası senkron HTTP çağrısı yapılmaz; servisler arası tüm reaksiyonlar Kafka üzerinden
yayınlanan integration event'ler ile gerçekleşir.

İçinde iki servis var:

- **UserService** — kullanıcı/çalışan yönetimi: kayıt, giriş, şifre işlemleri, profil, departman/pozisyon, yetkilendirme (JWT + rol bazlı).
- **NotificationService** — UserService'ten gelen olayları dinler ve email/SMS/push bildirim gönderir (şu an konsola loglayan stub gönderici implementasyonları ile).

## İçindekiler

- [Mimari](#mimari)
- [Kafka Event Akışı](#kafka-event-akışı)
- [Gereksinimler](#gereksinimler)
- [Servisleri Başlatma](#servisleri-başlatma)
- [Varsayılan (Seed) Hesap](#varsayılan-seed-hesap)
- [UserService API Referansı](#userservice-api-referansı)
- [NotificationService API Referansı](#notificationservice-api-referansı)
- [Hata Formatı](#hata-formatı)
- [Bilinen Sınırlamalar / Production Notları](#bilinen-sınırlamalar--production-notları)
- [Yeni Servis Ekleme](#yeni-servis-ekleme)

## Mimari

```
src/
  BuildingBlocks/
    Catering.BuildingBlocks.Domain      - BaseEntity, AggregateRoot, IDomainEvent
    Catering.BuildingBlocks.CQRS        - MediatR üzerine ICommand/IQuery/handler işaretleyici arayüzleri
    Catering.BuildingBlocks.Messaging   - IEventBus (Kafka producer), KafkaConsumerBackgroundService<T>,
                                           IntegrationEvent, KafkaTopics
  Services/
    User/
      Catering.UserService.Domain         - User, Department, Position, RefreshToken,
                                             PasswordResetRequest, enum'lar, domain event'ler
      Catering.UserService.Application    - Command/Query'ler, DTO'lar, repository/servis arayüzleri
      Catering.UserService.Infrastructure - EF Core (PostgreSQL), BCrypt, JWT üretimi, seed
      Catering.UserService.Api            - ASP.NET Core Web API, JWT auth, controller'lar
    Notification/
      Catering.NotificationService.Domain         - Notification aggregate (Email/Sms/Push)
      Catering.NotificationService.Application    - Command/Query'ler, UserCreated event handler'ı
      Catering.NotificationService.Infrastructure - EF Core (PostgreSQL), konsola loglayan kanal
                                                     gönderenler, Kafka consumer hosted service
      Catering.NotificationService.Api            - ASP.NET Core Web API, controller'lar
```

Her servis kendi veritabanına sahiptir (`catering_users`, `catering_notifications`) ve bağımsız
olarak deploy edilebilir. Servisler arası paylaşılan veritabanı veya doğrudan HTTP çağrısı yoktur.

### CQRS

Command ve query'ler MediatR üzerinden dispatch edilir. `Catering.BuildingBlocks.CQRS`,
`ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>` gibi ince işaretleyici arayüzler tanımlar;
bu sayede handler'ları bulmak kolaylaşır ve yazma/okuma niyeti (command/query) tip üzerinden
açıkça görülür.

## Kafka Event Akışı

| Topic | Yayınlayan | Dinleyen | Payload |
|---|---|---|---|
| `catering.user-events` | UserService | NotificationService | `UserCreatedIntegrationEvent` |
| `catering.notification-events` | NotificationService | (şu an dinleyen yok, ileride audit/reporting servisi için ayrılmış) | `NotificationSentIntegrationEvent` |
| `catering.password-reset-requested-events` | UserService | (henüz dinleyen yok — bu görev için ayrıldı) | `PasswordResetRequestedIntegrationEvent` |
| `catering.password-changed-events` | UserService | (henüz dinleyen yok) | `PasswordChangedIntegrationEvent` |

Örnek akış: `POST /api/auth/register` → `RegisterUserCommand` kullanıcıyı kaydeder ve
`UserCreatedIntegrationEvent`'i `catering.user-events`'e yayınlar → NotificationService'in
`UserCreatedConsumer`'ı bunu yakalar → `SendEmailNotificationCommand`'ı tetikler → karşılama
e-postası "gönderilir" (stub `IEmailSender` tarafından konsola loglanır) ve kaydedilir →
`NotificationSentIntegrationEvent`'i `catering.notification-events`'e yayınlar.

`catering.password-reset-requested-events` ve `catering.password-changed-events` topic'leri
UserService tarafından zaten yayınlanıyor (event payload'ları aşağıdaki API referansında
gösteriliyor); NotificationService tarafında bunları dinleyip email/SMS gönderecek consumer
henüz yazılmadı — bu, NotificationService'in bir sonraki genişletme adımı.

Integration event kontratları her serviste kasıtlı olarak ayrı tanımlanır (paylaşılan bir
contracts paketi kullanılmaz) — bu, servisleri birbirinden bağımsız deploy edilebilir tutmak
için standart bir mikroservis tercihidir.

> **Not:** Event'ler, command handler'lar içinde `SaveChangesAsync` sonrasında doğrudan
> yayınlanıyor; transactional outbox pattern kullanılmıyor. Bu, base/geliştirme ortamı için
> kabul edilebilir; production'da süreç commit ile publish arasında çökerse mesaj kaybını
> önlemek için bir outbox tablosu eklenmelidir.

## Gereksinimler

- .NET 10 SDK
- Docker Desktop (Kafka + PostgreSQL için)

## Servisleri Başlatma

### Seçenek A — Sadece altyapı Docker'da, servisler lokalde

```bash
docker compose up -d kafka user-db notification-db
```

```bash
dotnet run --project src/Services/User/Catering.UserService.Api
dotnet run --project src/Services/Notification/Catering.NotificationService.Api
```

Her API, Development ortamında açılışta veritabanı şemasını otomatik oluşturur
(`EnsureCreatedAsync`) ve `localhost:9092`'deki Kafka'ya bağlanır (bkz. `appsettings.json`).
Varsayılan portlar `launchSettings.json`'dan gelir:

| Servis | HTTP | HTTPS |
|---|---|---|
| UserService.Api | http://localhost:5049 | https://localhost:7105 |
| NotificationService.Api | http://localhost:5261 | https://localhost:7180 |

Farklı bir port kullanmak isterseniz: `dotnet run --project ... --urls http://localhost:5201`

### Seçenek B — Tüm stack Docker Compose ile

```bash
docker compose up --build
```

Kafka'yı (KRaft modu, Zookeeper'sız), iki PostgreSQL veritabanını ve iki API'yi başlatır.

| Servis | URL |
|---|---|
| UserService.Api | http://localhost:5101/swagger |
| NotificationService.Api | http://localhost:5102/swagger |

> **Şema değişikliği uyarısı:** Veritabanı şeması `EnsureCreatedAsync` ile oluşturulur; bu metot
> **var olan** bir veritabanını migrate etmez. Domain modelinde yapısal bir değişiklik
> yaptıysanız ve eski bir veritabanı volume'ünüz varsa, ilgili veritabanını silip yeniden
> oluşturmanız gerekir (örn. `docker compose down -v` veya container içinde
> `DROP DATABASE` + `CREATE DATABASE`).

## Varsayılan (Seed) Hesap

UserService, Development ortamında ilk açılışta otomatik olarak şu verileri oluşturur (kayıt
akışı her zaman `Employee` rolü atadığı için, departman/pozisyon yönetimi yapabilecek bir admin
hesabı olmadan sistem kullanılamaz hâlde kalırdı):

| Alan | Değer |
|---|---|
| Departman | Genel Müdürlük |
| Pozisyon | Sistem Yöneticisi |
| Email | `admin@catering.local` |
| Şifre | `Admin123!` |
| Rol | `SuperAdmin` |

Bu hesapla giriş yapıp gerçek departman/pozisyon verilerinizi oluşturabilir, diğer kullanıcıları
yönetebilirsiniz.

## UserService API Referansı

Taban URL (Docker): `http://localhost:5101` — Taban URL (lokal): `http://localhost:5049`

Aşağıdaki örneklerde `http://localhost:5101` kullanılmıştır, kendi ortamınıza göre değiştirin.

### Kimlik Doğrulama (`/api/auth`) — hepsi `AllowAnonymous`

#### Kayıt

```bash
curl -X POST http://localhost:5101/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ayse.yilmaz@catering.local",
    "password": "Passw0rd!",
    "firstName": "Ayse",
    "lastName": "Yilmaz",
    "tcIdentityNumber": "12345678950",
    "phoneNumber": "+905551234567",
    "birthDate": "1990-05-01",
    "address": "Istanbul",
    "departmentId": "<departman-id>",
    "positionId": "<pozisyon-id>",
    "hireDate": "2026-01-15",
    "hasDisability": false,
    "disabilityDescription": null,
    "salaryCeiling": null,
    "notes": null
  }'
```

Yanıt: `200 OK` → yeni kullanıcının `Guid` id'si. Olası hatalar: TC kimlik no algoritması
geçersizse `400`, email veya TC kimlik no zaten kayıtlıysa `409`, departman/pozisyon yoksa `404`.

`tcIdentityNumber` alanı gerçek TC kimlik no checksum algoritmasıyla doğrulanır — rastgele 11
haneli bir sayı kabul edilmez.

#### Giriş

```bash
curl -X POST http://localhost:5101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@catering.local", "password": "Admin123!"}'
```

Yanıt:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "accessTokenExpiresAt": "2026-06-22T13:00:45.48Z",
  "refreshToken": "eorPg4OJ3D77mdORmG60PR7W6Tv8z79f...",
  "user": {
    "id": "ad163248-...",
    "email": "admin@catering.local",
    "role": "SuperAdmin",
    "...": "..."
  }
}
```

`accessToken` 60 dakika geçerlidir (`Jwt:AccessTokenExpirationMinutes`), JWT içinde `role`,
`departmentId`, `positionId` claim'leri bulunur. `refreshToken` 7 gün geçerlidir
(`Jwt:RefreshTokenExpirationDays`) ve veritabanında hash'lenerek saklanır.

5 başarısız giriş denemesi sonrası hesap 15 dakika kilitlenir (`401`). Hesap `Active` dışında bir
durumdaysa (`Suspended`, `Terminated` vb.) giriş `401` döner.

#### Access Token Yenileme (refresh)

```bash
curl -X POST http://localhost:5101/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "eorPg4OJ3D77mdORmG60PR7W6Tv8z79f..."}'
```

Yanıt, `login` ile aynı şekildedir (yeni `accessToken` + yeni `refreshToken`). Kullanılan
refresh token rotation ile geçersiz hâle gelir — aynı token ile ikinci kez çağrı yapılırsa `401`
döner.

#### Çıkış (logout)

```bash
curl -X POST http://localhost:5101/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "eorPg4OJ3D77mdORmG60PR7W6Tv8z79f..."}'
```

Yanıt: `204 No Content`. Verilen refresh token'ı iptal eder.

#### Şifremi Unuttum (şifre sıfırlama isteği)

```bash
curl -X POST http://localhost:5101/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "ayse.yilmaz@catering.local", "channel": "Email"}'
```

`channel` değeri `Email` veya `Sms` olabilir (`NotificationChannelPreference` enum'u). Yanıt her
zaman `204 No Content` döner — email sistemde olsun olmasın aynı yanıt verilir (email
enumeration'ı önlemek için). Email varsa, 6 haneli bir kod üretilir, hash'lenerek 15 dakika
geçerlilikle saklanır ve `PasswordResetRequestedIntegrationEvent`
(`catering.password-reset-requested-events` topic'i) üzerinden yayınlanır:

```json
{
  "userId": "1a21d488-...",
  "firstName": "Ayse",
  "email": "ayse.yilmaz@catering.local",
  "phoneNumber": "+905551234567",
  "code": "967951",
  "channel": "Email",
  "expiresAt": "2026-06-22T12:21:04.91Z"
}
```

NotificationService bu event'i dinleyip kullanıcıya email/SMS olarak iletecek consumer'ı henüz
içermiyor — bu kodu test ortamında görmek için Kafka topic'ini doğrudan tüketebilirsiniz:

```bash
docker exec catering-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic catering.password-reset-requested-events \
  --from-beginning
```

#### Şifre Sıfırlama (kod ile)

```bash
curl -X POST http://localhost:5101/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ayse.yilmaz@catering.local",
    "code": "967951",
    "newPassword": "ResetPassw0rd!"
  }'
```

Yanıt: `204 No Content`. Kod yanlış/süresi dolmuş/zaten kullanılmışsa `401` döner. Başarılı
sıfırlamada kullanıcının tüm refresh token'ları iptal edilir ve
`PasswordChangedIntegrationEvent` (`catering.password-changed-events`) yayınlanır.

### Kullanıcılar (`/api/users`) — hepsi `Authorize` gerektirir

JWT'yi `Authorization: Bearer <accessToken>` header'ı ile gönderin.

#### Kendi Profilim

```bash
curl http://localhost:5101/api/users/me \
  -H "Authorization: Bearer <accessToken>"
```

#### Profilimi Güncelle

```bash
curl -X PUT http://localhost:5101/api/users/me \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Ayse",
    "lastName": "Yilmaz-Kaya",
    "phoneNumber": "+905559999999",
    "address": "Ankara",
    "birthDate": "1990-05-01",
    "profilePictureUrl": null
  }'
```

Yanıt: `204 No Content`. Sadece kendi adı/soyadı/telefon/adres/doğum tarihi/profil resmi
güncellenebilir — departman, pozisyon, maaş, durum gibi alanlar admin uçlarındandır.

#### Şifremi Değiştir

```bash
curl -X POST http://localhost:5101/api/users/me/change-password \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"currentPassword": "Passw0rd!", "newPassword": "NewPassw0rd!"}'
```

Yanıt: `204 No Content`. Mevcut şifre yanlışsa `401`. Başarılı değişiklikte tüm refresh
token'lar iptal edilir (diğer cihazlardan oturum kapanır) ve `PasswordChangedIntegrationEvent`
yayınlanır.

#### Kullanıcı Listesi — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl http://localhost:5101/api/users \
  -H "Authorization: Bearer <admin-accessToken>"
```

#### Id ile Kullanıcı — `Manager`, `HRAdmin`, `SuperAdmin`

```bash
curl http://localhost:5101/api/users/<id> \
  -H "Authorization: Bearer <admin-accessToken>"
```

#### İstihdam Bilgilerini Güncelle (Admin) — `HRAdmin`, `SuperAdmin`

```bash
curl -X PUT http://localhost:5101/api/users/<id>/employment-details \
  -H "Authorization: Bearer <admin-accessToken>" \
  -H "Content-Type: application/json" \
  -d '{
    "departmentId": "<departman-id>",
    "positionId": "<pozisyon-id>",
    "salaryCeiling": 50000,
    "hasDisability": false,
    "disabilityDescription": null,
    "notes": "Terfi"
  }'
```

#### Kullanıcı Durumunu Güncelle (Admin) — `HRAdmin`, `SuperAdmin`

```bash
curl -X PUT http://localhost:5101/api/users/<id>/status \
  -H "Authorization: Bearer <admin-accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"newStatus": "Suspended", "terminationDate": null}'
```

`newStatus` değerleri: `Active`, `Inactive`, `OnLeave`, `Suspended`, `Terminated`. `Active`
dışına geçişte kullanıcının tüm refresh token'ları otomatik iptal edilir (zorunlu çıkış).
`Terminated` durumunda `terminationDate` verilmezse otomatik olarak bugünün tarihi atanır.

### Departmanlar (`/api/departments`) — `Authorize` gerektirir

```bash
# Listele — herhangi bir giriş yapmış kullanıcı
curl http://localhost:5101/api/departments -H "Authorization: Bearer <accessToken>"

# Oluştur — HRAdmin, SuperAdmin
curl -X POST http://localhost:5101/api/departments \
  -H "Authorization: Bearer <admin-accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"name": "Bilgi Teknolojileri", "description": "Yazılım ve altyapı ekibi"}'
```

### Pozisyonlar (`/api/positions`) — `Authorize` gerektirir

```bash
# Listele — herhangi bir giriş yapmış kullanıcı
curl http://localhost:5101/api/positions -H "Authorization: Bearer <accessToken>"

# Oluştur — HRAdmin, SuperAdmin
curl -X POST http://localhost:5101/api/positions \
  -H "Authorization: Bearer <admin-accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"name": "Yazılım Mühendisi", "description": null}'
```

## NotificationService API Referansı

Taban URL (Docker): `http://localhost:5102` — Taban URL (lokal): `http://localhost:5261`

Bu endpoint'ler `AllowAnonymous`'tur (manuel test/tetikleme amaçlıdır); normalde bildirimler
Kafka event'leri üzerinden otomatik tetiklenir.

#### Email Gönder

```bash
curl -X POST http://localhost:5102/api/notifications/email \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "1a21d488-52bf-420e-b86f-3fa6a21370ab",
    "recipient": "ayse.yilmaz@catering.local",
    "subject": "Hoş geldiniz",
    "body": "Hesabınız oluşturuldu."
  }'
```

#### SMS Gönder

```bash
curl -X POST http://localhost:5102/api/notifications/sms \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "1a21d488-52bf-420e-b86f-3fa6a21370ab",
    "recipient": "+905551234567",
    "body": "Şifreniz değiştirildi."
  }'
```

#### Push Bildirim Gönder

```bash
curl -X POST http://localhost:5102/api/notifications/push \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "1a21d488-52bf-420e-b86f-3fa6a21370ab",
    "deviceToken": "device-token-123",
    "title": "Hatırlatma",
    "body": "Yarın vardiyanız var."
  }'
```

Bu üç uçtan dönen yanıt `200 OK` + bildirimin `Guid` id'sidir. Gönderim "gerçek" bir sağlayıcıya
(SendGrid, Twilio, FCM vb.) gitmez; `ConsoleEmailSender` / `ConsoleSmsSender` /
`ConsolePushNotificationSender` stub implementasyonları sadece konsola loglar — gerçek sağlayıcı
entegrasyonu eklemek için `Catering.NotificationService.Application.Abstractions` altındaki
`IEmailSender` / `ISmsSender` / `IPushNotificationSender` arayüzlerinin yeni implementasyonlarını
yazıp `DependencyInjection.cs`'te kayıt etmeniz yeterlidir.

#### Kullanıcının Bildirimlerini Listele

```bash
curl http://localhost:5102/api/notifications/user/1a21d488-52bf-420e-b86f-3fa6a21370ab
```

Yanıt:

```json
[
  {
    "id": "...",
    "userId": "1a21d488-...",
    "channel": "Email",
    "recipient": "ayse.yilmaz@catering.local",
    "subject": "Welcome to Catering, Ayse!",
    "body": "Hi Ayse, your account has been created successfully.",
    "status": "Sent",
    "sentAt": "2026-06-22T12:00:46Z",
    "errorMessage": null
  }
]
```

## Hata Formatı

Her iki API de hataları [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110) `ProblemDetails`
formatında döner:

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
| `500` | Beklenmeyen hata — detay döndürülmez, sunucu loglarında tam stack trace bulunur |

## Bilinen Sınırlamalar / Production Notları

- **JWT secret** şu an `appsettings.json`'da düz metin olarak duruyor
  (`Jwt:Secret`). Production'da bir secret manager'a (Azure Key Vault, AWS Secrets Manager,
  ortam değişkeni vb.) taşınmalı.
- **Outbox pattern yok**: integration event'ler `SaveChangesAsync` sonrası doğrudan Kafka'ya
  yayınlanıyor. Süreç commit ile publish arasında çökerse mesaj kaybolabilir; production'da
  outbox tablosu + ayrı bir publisher eklenmelidir.
- **`Register` herkese açık** (`AllowAnonymous`): self-service kayıt senaryosu için uygundur.
  Eğer kayıt sadece İK tarafından yapılmalıysa, bu uca `[Authorize(Roles = "HRAdmin,SuperAdmin")]`
  eklenmesi gerekir.
- **NotificationService, şifre event'lerini henüz dinlemiyor**: `catering.password-reset-requested-events`
  ve `catering.password-changed-events` topic'leri UserService tarafından yayınlanıyor ama
  NotificationService'te bunlara karşılık gelen consumer/handler henüz yazılmadı.
- **Migration yerine `EnsureCreatedAsync` kullanılıyor**: şema değişikliklerinde var olan
  veritabanı migrate edilmez, yeniden oluşturulması gerekir (bkz. yukarıdaki uyarı).

## Yeni Servis Ekleme

1. `src/Services/<Servis>/` altında User veya Notification servisini şablon alarak
   `Domain` / `Application` / `Infrastructure` / `Api` projelerini oluşturun.
2. `Application` projesinden `Catering.BuildingBlocks.Domain`, `.CQRS` ve `.Messaging`'e referans
   verin.
3. Yeni topic'leri `Catering.BuildingBlocks.Messaging.KafkaTopics`'e ekleyin.
4. Servisi `Catering.slnx` ve `docker-compose.yml`'e ekleyin.
