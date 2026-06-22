# NotificationService

Taban URL (Docker): `http://localhost:5102` — Taban URL (lokal): `http://localhost:5261`

Bu endpoint'ler `AllowAnonymous`'tur (manuel test/tetikleme amaçlıdır); normalde bildirimler
Kafka event'leri üzerinden otomatik tetiklenir. `$USER_ID` değişkeni, [UserService](user-service.md)
sayfasındaki "Kayıt" adımında elde edilen kullanıcı id'sidir (NotificationService bunu sadece
bir etiket olarak saklar, UserService'te var olup olmadığını doğrulamaz).

## Email gönder

```bash
curl -s -X POST http://localhost:5102/api/notifications/email \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"recipient\": \"ayse.yilmaz@catering.local\",
    \"subject\": \"Hoş geldiniz\",
    \"body\": \"Hesabınız oluşturuldu.\"
  }"
```

## SMS gönder

```bash
curl -s -X POST http://localhost:5102/api/notifications/sms \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"recipient\": \"+905551234567\",
    \"body\": \"Şifreniz değiştirildi.\"
  }"
```

## Push bildirim gönder

```bash
curl -s -X POST http://localhost:5102/api/notifications/push \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"deviceToken\": \"device-token-123\",
    \"title\": \"Hatırlatma\",
    \"body\": \"Yarın vardiyanız var.\"
  }"
```

Bu üç uçtan dönen yanıt `200 OK` + bildirimin `Guid` id'sidir. Gönderim "gerçek" bir sağlayıcıya
(SendGrid, Twilio, FCM vb.) gitmez; `ConsoleEmailSender` / `ConsoleSmsSender` /
`ConsolePushNotificationSender` stub implementasyonları sadece konsola loglar — gerçek sağlayıcı
entegrasyonu eklemek için `Catering.NotificationService.Application.Abstractions` altındaki
`IEmailSender` / `ISmsSender` / `IPushNotificationSender` arayüzlerinin yeni implementasyonlarını
yazıp `DependencyInjection.cs`'te kayıt etmeniz yeterlidir.

## Kullanıcının bildirimlerini listele

```bash
curl -s http://localhost:5102/api/notifications/user/$USER_ID
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
