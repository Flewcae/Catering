# Başlarken

## Gereksinimler

- .NET 10 SDK
- Docker Desktop (Kafka + PostgreSQL için)

## Seçenek A — Sadece altyapı Docker'da, servisler lokalde

```bash
docker compose up -d kafka user-db notification-db center-db
```

```bash
dotnet run --project src/Services/User/Catering.UserService.Api
dotnet run --project src/Services/Notification/Catering.NotificationService.Api
dotnet run --project src/Services/Center/Catering.CenterService.Api
```

Her API, Development ortamında açılışta veritabanı şemasını otomatik oluşturur
(`EnsureCreatedAsync`) ve `localhost:9092`'deki Kafka'ya bağlanır. Varsayılan portlar
`launchSettings.json`'dan gelir:

| Servis | HTTP | HTTPS |
|---|---|---|
| UserService.Api | http://localhost:5049 | https://localhost:7105 |
| NotificationService.Api | http://localhost:5261 | https://localhost:7180 |
| CenterService.Api | http://localhost:5050 | https://localhost:7106 |

## Seçenek B — Tüm stack Docker Compose ile

Email gönderimi (bkz. [NotificationService](notification-service.md) → "Email kurulumu") gerçek
bir SMTP sunucusu, push gönderimi (bkz. [NotificationService](notification-service.md) → "Push
(Firebase) kurulumu") gerçek bir Firebase projesi gerektirir. Docker Compose bu bilgileri proje
köküdeki `.env` dosyasından okur (`.env`, `.gitignore`'da olduğu için git'e girmez). İlk kurulumda:

```bash
cp .env.example .env
```

ve `.env` içindeki `SMTP_HOST`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_ADDRESS`,
`FIREBASE_PROJECT_ID`, `FIREBASE_CREDENTIALS_JSON` alanlarını gerçek bilgilerinizle doldurun.
Bu alanlar boş bırakılırsa ilgili gönderim başarısız olur ama servis çökmez — bildirim sadece
`Failed` durumunda kaydedilir. `JWT_SECRET`/`JWT_ISSUER`/`JWT_AUDIENCE` boş bırakılırsa geliştirme
varsayılanı kullanılır (üç servis de aynı değeri paylaşmalı).

```bash
docker compose up --build
```

Kafka'yı (KRaft modu, Zookeeper'sız), üç PostgreSQL veritabanını ve üç API'yi başlatır.

| Servis | URL |
|---|---|
| UserService.Api | http://localhost:5101/swagger |
| NotificationService.Api | http://localhost:5102/swagger |
| CenterService.Api | http://localhost:5103/swagger |

> **Şema değişikliği uyarısı:** Veritabanı şeması `EnsureCreatedAsync` ile oluşturulur; bu metot
> **var olan** bir veritabanını migrate etmez. Domain modelinde yapısal bir değişiklik
> yaptıysanız ve eski bir veritabanı volume'ünüz varsa, ilgili veritabanını silip yeniden
> oluşturmanız gerekir (örn. `docker compose down -v`).

## Varsayılan (seed) hesap

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
yönetebilirsiniz. Örnek istekler için [UserService](user-service.md) ve
[NotificationService](notification-service.md) sayfalarına bakın.
