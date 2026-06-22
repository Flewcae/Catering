# Catering

.NET 10 üzerinde kurulu, CQRS mimarisi ve Kafka tabanlı olay (event) iletişimi kullanan
mikroservis tabanlı bir catering ERP/CRM altyapısı.

Bu dokümantasyon GitBook Git Sync ile bu reponun `/docs` klasöründen otomatik olarak
senkronize edilir — kaynak her zaman bu repodur, GitBook sadece bir yansımadır (mirror).

İçinde iki servis var:

- **UserService** — kullanıcı/çalışan yönetimi: kayıt, giriş, şifre işlemleri, profil,
  departman/pozisyon, JWT + rol bazlı yetkilendirme.
- **NotificationService** — UserService'ten gelen olayları dinler, email/SMS/push bildirim
  gönderir.

Sol menüden ilgili sayfaya geçebilirsiniz:

- [Mimari](mimari.md) — proje yapısı, CQRS, Kafka event akışı
- [Başlarken](baslarken.md) — servisleri Docker veya lokalde çalıştırma
- [UserService](user-service.md) — tüm endpoint'ler ve örnek istekler
- [NotificationService](notification-service.md) — tüm endpoint'ler ve örnek istekler
- [Kafka Olayları](kafka-olaylari.md) — topic/event katalogu
- [Sorun Giderme](sorun-giderme.md) — sık karşılaşılan hatalar ve çözümleri
