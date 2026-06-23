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

Bu üç uçtan dönen yanıt `200 OK` + bildirimin `Guid` id'sidir. Gönderim "gerçek" bir sağlayıcıya
(SendGrid, Twilio, FCM vb.) gitmez; `ConsoleEmailSender` / `ConsoleSmsSender` /
`ConsolePushNotificationSender` stub implementasyonları sadece konsola loglar — gerçek sağlayıcı
entegrasyonu eklemek için `Catering.NotificationService.Application.Abstractions` altındaki
`IEmailSender` / `ISmsSender` / `IPushNotificationSender` arayüzlerinin yeni implementasyonlarını
yazıp `DependencyInjection.cs`'te kayıt etmeniz yeterlidir.

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
