# Başlarken

## Gereksinimler

- .NET 10 SDK
- Docker Desktop (Kafka + PostgreSQL için)

## Seçenek A — Sadece altyapı Docker'da, servisler lokalde

```bash
docker compose up -d kafka user-db notification-db
```

```bash
dotnet run --project src/Services/User/Catering.UserService.Api
dotnet run --project src/Services/Notification/Catering.NotificationService.Api
```

Her API, Development ortamında açılışta veritabanı şemasını otomatik oluşturur
(`EnsureCreatedAsync`) ve `localhost:9092`'deki Kafka'ya bağlanır. Varsayılan portlar
`launchSettings.json`'dan gelir:

| Servis | HTTP | HTTPS |
|---|---|---|
| UserService.Api | http://localhost:5049 | https://localhost:7105 |
| NotificationService.Api | http://localhost:5261 | https://localhost:7180 |

## Seçenek B — Tüm stack Docker Compose ile

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
