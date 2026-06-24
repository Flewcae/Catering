# Kafka Olayları

| Topic | Yayınlayan | Dinleyen | Payload |
|---|---|---|---|
| `catering.user-events` | UserService | NotificationService | `UserCreatedIntegrationEvent` |
| `catering.notification-events` | NotificationService | (şu an dinleyen yok, ileride audit/reporting servisi için ayrılmış) | `NotificationSentIntegrationEvent` |
| `catering.password-reset-requested-events` | UserService | (henüz dinleyen yok — bkz. [Mimari](mimari.md)) | `PasswordResetRequestedIntegrationEvent` |
| `catering.password-changed-events` | UserService | (henüz dinleyen yok) | `PasswordChangedIntegrationEvent` |
| `catering.device-token-registered-events` | UserService | NotificationService | `DeviceTokenRegisteredIntegrationEvent` |
| `catering.device-token-revoked-events` | UserService | NotificationService | `DeviceTokenRevokedIntegrationEvent` |
| `catering.center-created-events` | CenterService | UserService | `CenterCreatedIntegrationEvent` |

## Payload örnekleri

### `UserCreatedIntegrationEvent`

```json
{
  "userId": "...",
  "firstName": "Ayse",
  "lastName": "Yilmaz",
  "email": "ayse.yilmaz@catering.local",
  "phoneNumber": "+905551234567",
  "role": "Employee",
  "temporaryPassword": "Xk9#mPqR2!tZ"
}
```

> ⚠️ Geçici şifre bu event'in içinde düz metin olarak taşınır — Kafka log'larına/retention'a
> erişimi olan herkes bunu görebilir. Bu, projenin kabul edilmiş düz-metin-secret sınırlamasıyla
> aynı kategoridedir (bkz. [Mimari](mimari.md) → Bilinen sınırlamalar); production'da bu event'in
> kısa retention'lı, erişimi kısıtlı bir topic'te tutulması veya şifrenin event içinde taşınmayıp
> ayrı, tek kullanımlık bir bağlantı/kod üzerinden iletilmesi değerlendirilmelidir.

### `PasswordResetRequestedIntegrationEvent`

```json
{
  "userId": "...",
  "firstName": "Ayse",
  "email": "ayse.yilmaz@catering.local",
  "phoneNumber": "+905551234567",
  "code": "967951",
  "channel": "Email",
  "expiresAt": "2026-06-22T12:21:04.91Z"
}
```

### `PasswordChangedIntegrationEvent`

```json
{
  "userId": "...",
  "email": "ayse.yilmaz@catering.local"
}
```

### `DeviceTokenRegisteredIntegrationEvent`

```json
{
  "userId": "...",
  "token": "device-token-123",
  "platform": "android",
  "registeredAt": "2026-06-23T10:00:00Z"
}
```

### `DeviceTokenRevokedIntegrationEvent`

```json
{
  "userId": "...",
  "token": "device-token-123"
}
```

### `CenterCreatedIntegrationEvent`

```json
{
  "centerId": "...",
  "name": "Merkez Şube",
  "address": "Atatürk Cad. No:1, İstanbul"
}
```

## Topic'i doğrudan tüketmek (debug)

```bash
docker exec catering-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic catering.password-reset-requested-events \
  --from-beginning
```

`--topic` değerini ihtiyacınıza göre değiştirin.

## Notlar

- Integration event kontratları her serviste kasıtlı olarak ayrı tanımlanır (paylaşılan bir
  contracts paketi kullanılmaz).
- Event'ler `SaveChangesAsync` sonrası doğrudan yayınlanıyor; outbox pattern yok (bkz.
  [Mimari](mimari.md) → Bilinen sınırlamalar).
- Her consumer, `KafkaConsumerBackgroundService` üzerinden kendi topic'ini `AdminClient` ile
  açılışta otomatik oluşturur (`auto.create.topics.enable`'a güvenilmez).
