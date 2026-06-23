# NotificationService

Taban URL (Docker): `http://localhost:5102` — Taban URL (lokal): `http://localhost:5261`

Bu endpoint'ler `AllowAnonymous`'tur (manuel test/tetikleme amaçlıdır); normalde bildirimler
Kafka event'leri üzerinden otomatik tetiklenir.

Aşağıdaki istekler, [UserService](user-service.md) sayfasındakiyle aynı mantıkla, Postman'a
doğrudan kopyalanıp **Import → Raw text** ile veya bir isteği düzenlerken **Code** panelinden
yapıştırılabilen eksiksiz `curl` komutlarıdır. URL `http://localhost:5102` olarak gömülüdür —
Docker dışında lokal çalıştırıyorsanız `http://localhost:5261` ile değiştirin.

`{{userId}}` bir Postman **collection variable**'dır — UserService collection'ındaki "Kayıt"
isteğiyle otomatik dolan değerdir (NotificationService bunu sadece bir etiket olarak saklar,
UserService'te var olup olmadığını doğrulamaz). Bu servis için ayrı bir collection
kullanıyorsanız, `userId` değişkenini elle de girebilirsiniz.

## Kanal durumu

| Kanal | Durum |
|---|---|
| Email | **Gerçek SMTP üzerinden gönderiyor** (`SmtpEmailSender`, MailKit) — aşağıdaki "Email kurulumu" bölümüne bakın. |
| SMS | Stub — `ConsoleSmsSender` sadece konsola loglar, gerçek bir sağlayıcıya (örn. Twilio) gitmez. |
| Push | Stub — `ConsolePushNotificationSender` sadece konsola loglar, gerçek bir sağlayıcıya (örn. FCM) gitmez. |

SMS ve push için gerçek sağlayıcı entegrasyonu daha sonra yapılacak; o zamana kadar bu iki kanal
nesne üretip veritabanına `Sent` olarak kaydediyor ama hiçbir yere fiilen göndermiyor.

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
> Docker Compose ile çalıştırıyorsanız bunun yerine `docker-compose.yml`'de
> `environment: Smtp__Host=...`, `Smtp__Username=...` vb. ortam değişkenleri tanımlayın
> (ASP.NET Core konfigürasyonu `__` ile iç içe section'ları ortam değişkeninden okur).

Gerçek bir alıcıya göndermeden test etmek isterseniz `Host`/`Username`/`Password` değerlerine
[Mailtrap](https://mailtrap.io) gibi bir "sandbox" SMTP hesabı tanımlayabilirsiniz — gönderdiğiniz
her email gerçek bir kutuya gitmeden Mailtrap arayüzünde yakalanır.

`Smtp` bölümü boş bırakılırsa (`Host` boş string) bağlantı denemesi başarısız olur;
`SendEmailNotificationCommandHandler` bu hatayı yakalar ve bildirimi `Failed` durumunda kaydeder
— yani servis çökmez, sadece email gönderilmez.

## Email gönder

```bash
curl --location 'http://localhost:5102/api/notifications/email' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "recipient": "ayse.yilmaz@catering.local",
  "subject": "Hoş geldiniz",
  "body": "Hesabınız oluşturuldu."
}'
```

## SMS gönder

```bash
curl --location 'http://localhost:5102/api/notifications/sms' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "recipient": "+905551234567",
  "body": "Şifreniz değiştirildi."
}'
```

## Push bildirim gönder

```bash
curl --location 'http://localhost:5102/api/notifications/push' \
--header 'Content-Type: application/json' \
--data '{
  "userId": "{{userId}}",
  "deviceToken": "device-token-123",
  "title": "Hatırlatma",
  "body": "Yarın vardiyanız var."
}'
```

Bu üç uçtan dönen yanıt `200 OK` + bildirimin `Guid` id'sidir. Email artık yukarıdaki SMTP
kurulumuyla gerçekten gönderilir; SMS ve push hâlâ stub'tır — `ConsoleSmsSender` /
`ConsolePushNotificationSender` sadece konsola loglar, gerçek bir sağlayıcıya (Twilio, FCM vb.)
gitmez. Onlar için de gerçek entegrasyon eklemek isterseniz `Catering.NotificationService.Application.Abstractions`
altındaki `ISmsSender` / `IPushNotificationSender` arayüzlerinin yeni implementasyonlarını yazıp
`DependencyInjection.cs`'te kayıt etmeniz yeterlidir — `SmtpEmailSender` zaten bu desenin email
için yapılmış hâlidir.

## Kullanıcının bildirimlerini listele

```bash
curl --location 'http://localhost:5102/api/notifications/user/{{userId}}'
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
