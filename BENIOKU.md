# Catering

.NET 10 ile geliştirilmiş, **CQRS (Command Query Responsibility Segregation)** ve **Event-Driven (Olay Tabanlı)** mikroservis mimarisine sahip bir Catering ERP/CRM sistemidir.

Servisler:

- Kendi sorumluluk alanlarındaki işlemler için HTTP üzerinden senkron iletişim kurar.
- Servisler arası bildirim ve olay aktarımı için Kafka üzerinden asenkron iletişim kullanır.

---

# Mimari Yapı

```text
src/
  BuildingBlocks/
    Catering.BuildingBlocks.Domain
    Catering.BuildingBlocks.CQRS
    Catering.BuildingBlocks.Messaging

  Services/
    User/
      Domain
      Application
      Infrastructure
      Api

    Notification/
      Domain
      Application
      Infrastructure
      Api
```

## BuildingBlocks Katmanı

Tüm servislerin ortak olarak kullandığı temel bileşenleri içerir.

### Catering.BuildingBlocks.Domain

Domain katmanında kullanılan temel yapılar:

- BaseEntity → Temel varlık sınıfı
- AggregateRoot → Aggregate Root yapısı
- IDomainEvent → Domain Event arayüzü

### Catering.BuildingBlocks.CQRS

CQRS mimarisinin temel arayüzlerini içerir:

- ICommand
- ICommand<TResponse>
- IQuery<TResponse>

Bu yapı MediatR üzerine kurulmuştur.

### Catering.BuildingBlocks.Messaging

Mesajlaşma altyapısını içerir:

- IEventBus
- Kafka Producer
- Kafka Consumer
- Integration Event sınıfları
- Kafka Topic tanımları

---

# User Servisi

Kullanıcı işlemlerinden sorumludur.

## Domain Katmanı

İçerdiği yapılar:

- User Aggregate
- UserRole
- Domain Event'ler

## Application Katmanı

### Komutlar (Commands)

- CreateUserCommand

### Sorgular (Queries)

- GetUserByIdQuery
- GetUsersQuery

### Eventler

- UserCreatedIntegrationEvent

Kullanıcı oluşturulduğunda Kafka'ya yayınlanır.

## Infrastructure Katmanı

Veritabanı işlemleri:

- Entity Framework Core
- PostgreSQL

## API Katmanı

ASP.NET Core Web API katmanıdır.

Controller'lar burada bulunur.

---

# Notification Servisi

Bildirim işlemlerinden sorumludur.

## Domain Katmanı

Notification Aggregate içerir.

Desteklenen bildirim türleri:

- Email
- SMS
- Push Notification

## Application Katmanı

### Komutlar

- SendEmailNotificationCommand
- SendSmsNotificationCommand
- SendPushNotificationCommand

### Sorgular

- GetNotificationsForUserQuery

### Event Handler

User servisinden gelen:

```text
UserCreatedIntegrationEvent
```

eventini dinler.

## Infrastructure Katmanı

İçerdiği bileşenler:

- Entity Framework Core
- PostgreSQL
- Kafka Consumer
- Konsola yazan örnek Email/SMS göndericileri

## API Katmanı

ASP.NET Core Web API katmanıdır.

---

# Veritabanı Yapısı

Her servis kendi veritabanına sahiptir.

## User Service

```text
catering_users
```

## Notification Service

```text
catering_notifications
```

### Temel Kurallar

✅ Her servis bağımsız deploy edilir.

✅ Ortak veritabanı kullanılmaz.

✅ Servisler birbirlerinin veritabanına erişmez.

✅ Servisler arası iletişim Kafka eventleri ile gerçekleştirilir.

---

# CQRS Yapısı

Sistem CQRS prensibine göre tasarlanmıştır.

## Command

Veriyi değiştiren işlemleri temsil eder.

Örnek:

```csharp
CreateUserCommand
```

## Query

Veri okuma işlemlerini temsil eder.

Örnek:

```csharp
GetUserByIdQuery
```

Komutlar ve sorgular MediatR aracılığıyla çalıştırılır.

### CQRS Kullanımının Amaçları

- Okuma ve yazma işlemlerini ayırmak
- Kodun bakımını kolaylaştırmak
- Ölçeklenebilirliği artırmak
- İş mantığını daha net hale getirmek

---

# Kafka Event Yapısı

| Topic | Yayınlayan Servis | Dinleyen Servis | İçerik |
|---------|---------|---------|---------|
| catering.user-events | User Service | Notification Service | UserCreatedIntegrationEvent |
| catering.notification-events | Notification Service | Gelecekteki servisler | NotificationSentIntegrationEvent |

---

# Örnek İş Akışı

Kullanıcı oluşturulduğunda sistem aşağıdaki adımları izler:

## 1. Kullanıcı Oluşturulur

```http
POST /api/users
```

## 2. CreateUserCommand Çalışır

Kullanıcı veritabanına kaydedilir.

## 3. Event Yayınlanır

```text
UserCreatedIntegrationEvent
```

Kafka üzerindeki:

```text
catering.user-events
```

konusuna gönderilir.

## 4. Notification Service Event'i Yakalar

```text
UserCreatedConsumer
```

Kafka'dan gelen olayı tüketir.

## 5. Bildirim Komutu Çalışır

```text
SendEmailNotificationCommand
```

tetiklenir.

## 6. Hoş Geldiniz Bildirimi Oluşturulur

Örnek projede gerçek e-posta yerine konsola log yazılır.

## 7. Bildirim Veritabanına Kaydedilir

Notification servisinin veritabanına eklenir.

## 8. Yeni Event Yayınlanır

```text
NotificationSentIntegrationEvent
```

Kafka'ya gönderilir.

---

# Event Sözleşmeleri Neden Ortak Değil?

Her servis kendi event sınıfını içerir.

Örneğin:

```text
UserCreatedIntegrationEvent
```

hem User Service'te hem Notification Service'te ayrı ayrı tanımlanır.

Bu yaklaşımın amacı:

- Servislerin bağımsız geliştirilmesi
- Servislerin bağımsız deploy edilmesi
- Bağımlılıkların azaltılması

Bu, mikroservis mimarisinde yaygın olarak kullanılan bir yaklaşımdır.

---

# Outbox Pattern Notu

Şu an eventler:

```csharp
SaveChangesAsync()
```

sonrasında doğrudan Kafka'ya gönderilmektedir.

Geliştirme ortamı için yeterlidir ancak üretim ortamında önerilen yaklaşım:

## Transactional Outbox Pattern

Avantajları:

- Veri kaydedildiği halde event kaybolmaz.
- Servis çökse bile event daha sonra gönderilebilir.
- En az bir kez teslim garantisi (At-Least-Once Delivery) sağlar.

---

# Docker Kullanmadan Çalıştırma

Önce Kafka ve PostgreSQL servislerini başlatın:

```bash
docker compose up -d kafka user-db notification-db
```

Ardından servisleri ayrı terminallerde çalıştırın:

```bash
dotnet run --project src/Services/User/Catering.UserService.Api

dotnet run --project src/Services/Notification/Catering.NotificationService.Api
```

Geliştirme ortamında servisler veritabanını otomatik oluşturur.

Kafka adresi:

```text
localhost:9092
```

---

# Docker ile Tüm Sistemi Çalıştırma

```bash
docker compose up --build
```

Bu komut:

- Kafka
- User PostgreSQL
- Notification PostgreSQL
- User API
- Notification API

servislerini ayağa kaldırır.

---

# Swagger Adresleri

## User API

```text
http://localhost:5101/swagger
```

## Notification API

```text
http://localhost:5102/swagger
```

---

# Sistemi Test Etme

Kullanıcı oluştur:

```bash
curl -X POST http://localhost:5101/api/users \
-H "Content-Type: application/json" \
-d "{\"firstName\":\"Ada\",\"lastName\":\"Lovelace\",\"email\":\"ada@example.com\",\"phoneNumber\":\"+10000000000\",\"role\":\"Customer\"}"
```

Bildirimleri görüntüle:

```bash
curl http://localhost:5102/api/notifications/user/<userId>
```

### Beklenen Sonuç

- Kullanıcı oluşturulur.
- Kafka event yayınlar.
- Notification Service eventi alır.
- Hoş geldiniz bildirimi oluşturulur.
- Bildirim veritabanına kaydedilir.
- Sonuç sorguda görüntülenir.

Tüm süreç servisler arasında doğrudan HTTP çağrısı yapılmadan, tamamen Kafka Event mekanizması ile gerçekleşir.

---

# Yeni Bir Mikroservis Ekleme

## 1. Yeni Servis Klasörü Oluştur

```text
src/Services/<ServiceName>/
```

## 2. Alt Projeleri Oluştur

```text
Domain
Application
Infrastructure
Api
```

## 3. Application Katmanına Referansları Ekle

```text
Catering.BuildingBlocks.Domain
Catering.BuildingBlocks.CQRS
Catering.BuildingBlocks.Messaging
```

## 4. Yeni Kafka Topiclerini Tanımla

```csharp
KafkaTopics
```

içerisine gerekli topicleri ekle.

## 5. Projeye Dahil Et

Aşağıdaki dosyalara ekleme yap:

```text
Catering.slnx
docker-compose.yml
```

---

# Teknoloji Yığını

- .NET 10
- ASP.NET Core Web API
- MediatR
- CQRS
- Kafka
- PostgreSQL
- Entity Framework Core
- Docker
- Event-Driven Architecture
- Microservices Architecture

Bu proje, modern mikroservis mimarisini öğrenmek ve temel bir Event-Driven CQRS altyapısı oluşturmak için iyi bir başlangıç örneğidir.