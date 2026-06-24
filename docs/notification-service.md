# NotificationService

Taban URL (Docker): `http://localhost:5102` — Taban URL (lokal): `http://localhost:5261`

Bu endpoint'ler manuel test/tetikleme amaçlıdır; normalde bildirimler Kafka event'leri üzerinden
otomatik tetiklenir. Hepsi `Authorize` gerektirir ve her biri [UserService](user-service.md) →
"Pozisyonlar" üzerinden Position'a tanımlanan bir **permission flag**'iyle korunur (`send_custom_email`,
`send_custom_sms`, `send_custom_push_notification`, `view_user_notification`) — kullanıcının
giriş yaptığı Position'da o flag yoksa `403` döner; `SuperAdmin`/`HRAdmin` rolündeki kullanıcılar
flag'e bakılmaksızın her uca erişebilir. Token'ı doğrulamak için bu servis UserService ile aynı
JWT secret/issuer/audience'ı kullanır (bkz. `.env` → `JWT_SECRET`/`JWT_ISSUER`/`JWT_AUDIENCE`).

Aşağıdaki istekler, [UserService](user-service.md) sayfasındakiyle aynı mantıkla, Postman'a
doğrudan kopyalanıp **Import → Raw text** ile veya bir isteği düzenlerken **Code** panelinden
yapıştırılabilen eksiksiz `curl` komutlarıdır. URL `http://localhost:5102` olarak gömülüdür —
Docker dışında lokal çalıştırıyorsanız `http://localhost:5261` ile değiştirin.

`{{userId}}` bir Postman **collection variable**'dır — UserService collection'ındaki "Kullanıcı
hesabı oluştur" isteğiyle otomatik dolan değerdir (NotificationService bunu sadece bir etiket olarak saklar,
UserService'te var olup olmadığını doğrulamaz). Bu servis için ayrı bir collection
kullanıyorsanız, `userId` değişkenini elle de girebilirsiniz.

## Kanal durumu

| Kanal | Durum |
|---|---|
| Email | **Gerçek SMTP üzerinden gönderiyor** (`SmtpEmailSender`, MailKit) — aşağıdaki "Email kurulumu" bölümüne bakın. |
| SMS | Stub — `ConsoleSmsSender` sadece konsola loglar, gerçek bir sağlayıcıya (örn. Twilio) gitmez. |
| Push | **Gerçek Firebase Cloud Messaging üzerinden gönderiyor** (`FirebaseCloudMessagingSender`, FirebaseAdmin SDK) — aşağıdaki "Push (Firebase) kurulumu" bölümüne bakın. |

SMS için gerçek sağlayıcı entegrasyonu daha sonra yapılacak; o zamana kadar bu kanal nesne üretip
veritabanına `Sent` olarak kaydediyor ama hiçbir yere fiilen göndermiyor.

## UserCreated tüketimi (geçici şifre teslimi)

[UserService](user-service.md) → "Kullanıcı hesabı oluştur" ucu bir kullanıcı oluşturduğunda
`UserCreatedIntegrationEvent`'i (`catering.user-events`) artık üretilen **geçici şifre** ile
birlikte yayınlar. `UserCreatedConsumer`'ın tetiklediği `UserCreatedIntegrationEventHandler`:

1. Kullanıcıya "Welcome to Catering" konulu, geçici şifreyi içeren bir email gönderir
   (`SendEmailNotificationCommand` üzerinden — gerçek SMTP ile gider, bkz. "Email kurulumu").
2. Telefon numarası varsa, aynı geçici şifreyi içeren bir SMS gönderir
   (`SendSmsNotificationCommand` üzerinden) — **SMS kanalı şu an stub olduğu için bu mesaj
   gerçekte hiçbir yere gitmez**, sadece konsola loglanıp veritabanına `Sent` olarak kaydedilir
   (yukarıdaki "Kanal durumu" tablosuna bakın). Gerçek bir SMS sağlayıcısı bağlanana kadar geçici
   şifreyi pratikte sadece email üzerinden teslim edilmiş sayın.

Bu akış manuel tetiklenen bir HTTP isteği değildir — doğrudan Kafka consumer içinde MediatR
üzerinden çalışır, dolayısıyla `NotificationsController`'daki `send_custom_*` flag'lerinden
bağımsızdır.

## Email kurulumu

Email gönderimi `Catering.NotificationService.Infrastructure.Channels.SmtpEmailSender` ile
MailKit üzerinden gerçek bir SMTP sunucusuna bağlanır. Çalışması için
`appsettings.json`'daki (veya tercihen aşağıda anlatılan secret yönetimiyle) `Smtp` bölümünü
doldurmanız gerekir:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "Username": "ornek@gmail.com",
  "Password": "<app-password>",
  "FromAddress": "ornek@gmail.com",
  "FromName": "Catering"
}
```

| Alan | Açıklama |
|---|---|
| `Host` | SMTP sunucu adresi (Gmail: `smtp.gmail.com`, Outlook: `smtp.office365.com`, Mailtrap: `sandbox.smtp.mailtrap.io`). |
| `Port` | `587` (STARTTLS) yaygın varsayılan; `465` verirseniz `SmtpEmailSender` otomatik olarak SSL-on-connect kullanır. |
| `UseSsl` | `587`/STARTTLS için `true` bırakın. Şifrelemesiz bir test sunucusu kullanıyorsanız `false` yapın. |
| `Username` / `Password` | SMTP kimlik bilgileri. **Gmail kullanıyorsanız normal şifreniz çalışmaz** — Google hesabınızda 2FA açıp bir [App Password](https://myaccount.google.com/apppasswords) üretmeniz gerekir. |
| `FromAddress` / `FromName` | Gönderen olarak görünecek adres/isim. Çoğu sağlayıcı `FromAddress`'in `Username` ile aynı (veya doğrulanmış bir alan adı) olmasını ister. |

> ⚠️ **Gerçek şifreyi `appsettings.json`'a yazıp commit etmeyin** — repo'da JWT secret için de
> aynı bilinen sınırlama var (bkz. [Mimari](mimari.md) → Bilinen sınırlamalar). Lokalde
> geliştirirken kimlik bilgilerini `dotnet user-secrets` ile saklayın:
>
> ```bash
> cd src/Services/Notification/Catering.NotificationService.Api
> dotnet user-secrets init
> dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
> dotnet user-secrets set "Smtp:Username" "ornek@gmail.com"
> dotnet user-secrets set "Smtp:Password" "<app-password>"
> dotnet user-secrets set "Smtp:FromAddress" "ornek@gmail.com"
> ```
>
> Docker Compose ile çalıştırıyorsanız `docker-compose.yml`'i değiştirmeniz gerekmez —
> `notification-api` servisi `Smtp__*` ortam değişkenlerini proje köküdeki `.env` dosyasından
> (`SMTP_HOST`, `SMTP_USERNAME`, `SMTP_PASSWORD` vb.) okuyacak şekilde zaten ayarlı. Kurulum için
> [Başlarken](baslarken.md) → "Seçenek B" bölümüne bakın.

Gerçek bir alıcıya göndermeden test etmek isterseniz `Host`/`Username`/`Password` değerlerine
[Mailtrap](https://mailtrap.io) gibi bir "sandbox" SMTP hesabı tanımlayabilirsiniz — gönderdiğiniz
her email gerçek bir kutuya gitmeden Mailtrap arayüzünde yakalanır.

`Smtp` bölümü boş bırakılırsa (`Host` boş string) bağlantı denemesi başarısız olur;
`SendEmailNotificationCommandHandler` bu hatayı yakalar ve bildirimi `Failed` durumunda kaydeder
— yani servis çökmez, sadece email gönderilmez.

## Push (Firebase) kurulumu

Push gönderimi `Catering.NotificationService.Infrastructure.Channels.FirebaseCloudMessagingSender`
ile FirebaseAdmin SDK üzerinden gerçek bir Firebase projesine bağlanır. Çalışması için
`appsettings.json`'daki (veya tercihen aşağıda anlatılan secret yönetimiyle) `Firebase` bölümünü
doldurmanız gerekir:

```json
"Firebase": {
  "ProjectId": "catering-12345",
  "CredentialsJson": "{\"type\":\"service_account\",...}"
}
```

| Alan | Açıklama |
|---|---|
| `ProjectId` | Firebase projenizin proje kimliği (Firebase Console → Proje Ayarları). |
| `CredentialsJson` | Firebase Console → Proje Ayarları → Hizmet Hesapları → "Yeni özel anahtar oluştur" ile indirdiğiniz JSON dosyasının tüm içeriği. |

> ⚠️ **Bu JSON bir özel anahtardır, commit etmeyin** — SMTP şifresiyle aynı bilinen sınırlamaya
> tabidir (bkz. [Mimari](mimari.md) → Bilinen sınırlamalar). Lokalde geliştirirken
> `dotnet user-secrets` ile saklayın:
>
> ```bash
> cd src/Services/Notification/Catering.NotificationService.Api
> dotnet user-secrets init
> dotnet user-secrets set "Firebase:ProjectId" "catering-12345"
> dotnet user-secrets set "Firebase:CredentialsJson" "$(cat service-account.json)"
> ```
>
> Docker Compose ile çalıştırıyorsanız `docker-compose.yml`'i değiştirmeniz gerekmez —
> `notification-api` servisi `Firebase__*` ortam değişkenlerini proje köküdeki `.env` dosyasından
> (`FIREBASE_PROJECT_ID`, `FIREBASE_CREDENTIALS_JSON`) okuyacak şekilde zaten ayarlı. JSON'ı `.env`
> dosyasına tek satıra sıkıştırarak yazın. Kurulum için [Başlarken](baslarken.md) →
> "Seçenek B" bölümüne bakın.

`Firebase` bölümü boş bırakılırsa (`ProjectId`/`CredentialsJson` boş string) Firebase'e bağlanma
denemesi başarısız olur; `SendPushNotificationCommandHandler` bu hatayı yakalar ve bildirimi
`Failed` durumunda kaydeder — yani servis çökmez, sadece push gönderilmez.

## Email gönder

Flag: `send_custom_email`.

```bash
curl --location 'http://localhost:5102/api/notifications/email' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "recipient": "ayse.yilmaz@catering.local",
  "subject": "Hoş geldiniz",
  "body": "Hesabınız oluşturuldu."
}'
```

## SMS gönder

Flag: `send_custom_sms`.

```bash
curl --location 'http://localhost:5102/api/notifications/sms' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "recipient": "+905551234567",
  "body": "Şifreniz değiştirildi."
}'
```

## Push bildirim gönder

Flag: `send_custom_push_notification`.

```bash
curl --location 'http://localhost:5102/api/notifications/push' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "deviceToken": "device-token-123",
  "title": "Hatırlatma",
  "body": "Yarın vardiyanız var."
}'
```

Bu üç uçtan dönen yanıt `200 OK` + bildirimin `Guid` id'sidir. Email ve push artık yukarıdaki
SMTP/Firebase kurulumlarıyla gerçekten gönderilir; SMS hâlâ stub'tır — `ConsoleSmsSender` sadece
konsola loglar, gerçek bir sağlayıcıya (Twilio vb.) gitmez. Onun için de gerçek entegrasyon eklemek
isterseniz `Catering.NotificationService.Application.Abstractions` altındaki `ISmsSender`
arayüzünün yeni bir implementasyonunu yazıp `DependencyInjection.cs`'te kayıt etmeniz yeterlidir —
`SmtpEmailSender`/`FirebaseCloudMessagingSender` zaten bu desenin email/push için yapılmış hâlidir.

Yukarıdaki `/push` ucu, çağıranın cihaz token'ını doğrudan body'de göndermesini bekler. Token'ı
bilmiyorsanız (örn. sadece `userId` elinizdeyse), [UserService](user-service.md)'e kayıtlı cihaz
token'larına göndermek için aşağıdaki uca bakın.

## Push bildirim gönder (UserId ile, tüm cihazlara)

[UserService](user-service.md) → "Cihaz token'ı kaydet" ile kayıt edilmiş cihaz token'ları,
`DeviceTokenRegisteredIntegrationEvent` üzerinden NotificationService'te yerel bir önbellekte
tutulur (`device_tokens` tablosu). Bu uç, verilen `userId`'ye kayıtlı tüm cihazlara aynı bildirimi
gönderir. Flag: `send_custom_push_notification` (yukarıdaki `/push` ucuyla aynı).

```bash
curl --location 'http://localhost:5102/api/notifications/push/user/{{userId}}' \
--header 'Authorization: Bearer {{accessToken}}' \
--header 'Content-Type: application/json' \
--data '{
  "title": "Hatırlatma",
  "body": "Yarın vardiyanız var."
}'
```

Yanıt, gönderilen her cihaz için oluşturulan bildirim id'lerinin listesidir (`200 OK` + `Guid[]`).
Kullanıcının kayıtlı cihazı yoksa boş liste döner (hata fırlatmaz).

## Kullanıcının bildirimlerini listele

Flag: `view_user_notification`.

```bash
curl --location 'http://localhost:5102/api/notifications/user/{{userId}}' \
--header 'Authorization: Bearer {{accessToken}}'
```

Yanıt:

```json
[
  {
    "id": "...",
    "userId": "...",
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
