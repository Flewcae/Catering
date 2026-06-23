# Mimari

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
                                             PasswordResetRequest, DeviceToken, enum'lar, domain event'ler
      Catering.UserService.Application    - Command/Query'ler, DTO'lar, repository/servis arayüzleri
      Catering.UserService.Infrastructure - EF Core (PostgreSQL), BCrypt, JWT üretimi, seed
      Catering.UserService.Api            - ASP.NET Core Web API, JWT auth, controller'lar
    Notification/
      Catering.NotificationService.Domain         - Notification aggregate (Email/Sms/Push), DeviceToken (cache)
      Catering.NotificationService.Application    - Command/Query'ler, UserCreated/DeviceToken event handler'ları
      Catering.NotificationService.Infrastructure - EF Core (PostgreSQL), SMTP/Firebase kanal
                                                     gönderenler, Kafka consumer hosted service'ler
      Catering.NotificationService.Api            - ASP.NET Core Web API, controller'lar
```

Her servis kendi veritabanına sahiptir (`catering_users`, `catering_notifications`) ve bağımsız
olarak deploy edilebilir. Servisler arası paylaşılan veritabanı veya doğrudan HTTP çağrısı yoktur
— tüm reaksiyonlar Kafka üzerinden yayınlanan integration event'ler ile gerçekleşir.

## CQRS

Command ve query'ler MediatR üzerinden dispatch edilir. `Catering.BuildingBlocks.CQRS`,
`ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>` gibi ince işaretleyici arayüzler tanımlar;
bu sayede handler'ları bulmak kolaylaşır ve yazma/okuma niyeti (command/query) tip üzerinden
açıkça görülür.

## Kafka event akışı

Örnek akış: `POST /api/auth/register` → `RegisterUserCommand` kullanıcıyı kaydeder ve
`UserCreatedIntegrationEvent`'i `catering.user-events`'e yayınlar → NotificationService'in
`UserCreatedConsumer`'ı bunu yakalar → `SendEmailNotificationCommand`'ı tetikler → karşılama
e-postası `SmtpEmailSender` ile gerçekten gönderilir ve kaydedilir →
`NotificationSentIntegrationEvent`'i `catering.notification-events`'e yayınlar.

Tüm topic/event listesi için [Kafka Olayları](kafka-olaylari.md) sayfasına bakın.

Integration event kontratları her serviste kasıtlı olarak ayrı tanımlanır (paylaşılan bir
contracts paketi kullanılmaz) — bu, servisleri birbirinden bağımsız deploy edilebilir tutmak
için standart bir mikroservis tercihidir.

> **Not:** Event'ler, command handler'lar içinde `SaveChangesAsync` sonrasında doğrudan
> yayınlanıyor; transactional outbox pattern kullanılmıyor. Bu, base/geliştirme ortamı için
> kabul edilebilir; production'da süreç commit ile publish arasında çökerse mesaj kaybını
> önlemek için bir outbox tablosu eklenmelidir.

## Bilinen sınırlamalar / production notları

- **JWT secret** şu an `appsettings.json`'da düz metin olarak duruyor (`Jwt:Secret`).
  Production'da bir secret manager'a (Azure Key Vault, AWS Secrets Manager, ortam değişkeni vb.)
  taşınmalı. Aynı durum `Smtp:Password` ve `Firebase:CredentialsJson` için de geçerlidir — kurulum
  için [NotificationService](notification-service.md) → "Email kurulumu" / "Push (Firebase)
  kurulumu" sayfalarına bakın.
- **SMS hâlâ stub**: `ConsoleSmsSender` sadece konsola loglar, gerçek bir sağlayıcıya gitmez.
  Email ve push artık sırasıyla `SmtpEmailSender` / `FirebaseCloudMessagingSender` ile gerçek
  SMTP/Firebase üzerinden gönderiliyor.
- **Device token önbelleği eventual-consistent**: NotificationService'teki `device_tokens` tablosu,
  UserService'in yayınladığı `DeviceTokenRegisteredEvents`/`DeviceTokenRevokedEvents`'e dayanan bir
  okuma önbelleğidir — UserService'teki kayıt ile NotificationService'in onu görmesi arasında kısa
  bir gecikme olabilir (outbox yokluğunun bir başka sonucu, aşağıya bakın).
- **Outbox pattern yok**: yukarıda açıklandı.
- **`Register` herkese açık** (`AllowAnonymous`): self-service kayıt senaryosu için uygundur.
  Eğer kayıt sadece İK tarafından yapılmalıysa, bu uca `[Authorize(Roles = "HRAdmin,SuperAdmin")]`
  eklenmesi gerekir.
- **NotificationService, şifre event'lerini henüz dinlemiyor**: `catering.password-reset-requested-events`
  ve `catering.password-changed-events` topic'leri UserService tarafından yayınlanıyor ama
  NotificationService'te bunlara karşılık gelen consumer/handler henüz yazılmadı.
- **Migration yerine `EnsureCreatedAsync` kullanılıyor**: şema değişikliklerinde var olan
  veritabanı migrate edilmez, yeniden oluşturulması gerekir.

## Yeni servis ekleme

1. `src/Services/<Servis>/` altında User veya Notification servisini şablon alarak
   `Domain` / `Application` / `Infrastructure` / `Api` projelerini oluşturun.
2. `Application` projesinden `Catering.BuildingBlocks.Domain`, `.CQRS` ve `.Messaging`'e referans
   verin.
3. Yeni topic'leri `Catering.BuildingBlocks.Messaging.KafkaTopics`'e ekleyin.
4. Servisi `Catering.slnx` ve `docker-compose.yml`'e ekleyin.
5. Bu dokümantasyona (`/docs`) yeni servis için bir sayfa ekleyip `SUMMARY.md`'ye satır ekleyin.
