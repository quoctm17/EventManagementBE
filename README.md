# 🎫 EventManagementBE

**EventManagementBE** là hệ thống quản lý sự kiện / hội nghị được xây dựng bằng **.NET 9 + EF Core** theo kiến trúc **Clean Architecture**.  
Hệ thống bao gồm các chức năng: quản lý User, Role, Event, Order, Ticket, Payment, Check-in… với **Authentication/Authorization JWT**.

---

## 🏗️ Kiến trúc

Dự án tuân theo mô hình **Clean Architecture** gồm 4 tầng:

```
EventManagementBE
 ├── EventManagement.Domain         # Core Entities, ValueObjects, Enums
 ├── EventManagement.Application    # Business logic (Services, DTOs, Interfaces, Helpers)
 ├── EventManagement.Infrastructure # Persistence (EF Core, Repository, UnitOfWork, Migrations)
 └── EventManagement.API            # Presentation Layer (Controllers, Middleware, Swagger, Auth)
```

### **Domain**
- Chứa Entity: `User`, `Event`, `Order`, `Ticket`, `Payment`...
- Enums chuẩn hóa trạng thái: `OrderStatus`, `TicketStatus`, `PaymentStatus`, `SettlementStatus`, v.v.

### **Application**
- Interfaces cho Repository & Service  
- Services xử lý Use-case: `AuthService`, `OrderService`, `NotificationService`...  
- Helpers: `PasswordHelper (BCrypt)`, `JwtAuthService`, `QrCodeHelper`...  
- DTOs: chia `Requests` (input), `Responses` (output, chuẩn hóa response)

### **Infrastructure**
- EF Core `EventManagementDbContext`  
- RepositoryBase, Repository cụ thể, UnitOfWork  
- Quản lý migration + persistence  

### **API**
- Controllers: `AuthController`, `UsersController`, `EventsController`...  
- Middleware: `ExceptionMiddleware`, Logging  
- Swagger UI + Bearer Token để test API

---

## 🗄️ Database

**Database**: SQL Server  
Tên DB: `EventManagementDB`

### Status chuẩn hóa (ENUM trong Domain):
- Orders.Status = `Pending | Paid | Cancelled | Failed`  
- Tickets.Status = `Reserved | Issued | Cancelled`  
- Payments.Status = `Success | Failed | Pending`  
- Settlements.Status = `Pending | Processing | Completed | Failed`  
- Contracts.Status = `Active | Expired | Terminated`  
- OrganizerRequests.Status = `Pending | Approved | Rejected`  
- Notifications.Type = `Email | SMS`

👉 Toàn bộ schema + script seed mẫu có trong thư mục `DatabaseScript`.

---

## 🔐 Authentication

- **Login**: `POST /api/auth/login`  
- **Register**: `POST /api/auth/register`  
- Sử dụng **JWT Bearer** (HS256).  

---

## ⚙️ Cài đặt & Chạy

### Yêu cầu
- .NET SDK 9
- SQL Server
- Git

### 1. Clone repo
```bash
git clone git@github.com:<your-username>/EventManagementBE.git
cd EventManagementBE
```

### 2. Apply migration (tạo DB schema)
```bash
dotnet ef database update --project EventManagement.Infrastructure --startup-project EventManagement.API
```

### 3. Run API
```bash
dotnet run --project EventManagement.API
```

Mặc định:
- Swagger: [https://localhost:7270/swagger](https://localhost:7270/swagger)  
- API: [https://localhost:7270/api](https://localhost:7270/api)

---

## 🙈 Bảo mật

- `appsettings.json` đã được ignore trong `.gitignore` để tránh leak secret/connection string.  
- Dev có thể config secret cục bộ bằng [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

Ví dụ:
```bash
dotnet user-secrets init --project EventManagement.API
dotnet user-secrets set "Jwt:Secret-Key" "super-secret" --project EventManagement.API
```

---

## 📦 Công nghệ sử dụng

- .NET 9 + ASP.NET Core
- EF Core 9 + SQL Server
- BCrypt.Net-Next (hash password)
- Swagger (OpenAPI 3)
- JWT Authorization + Bearer
- Clean Architecture pattern

---

## 🧑‍💻 Tác giả

- 🚀 BootCamp .NET Cybersoft – Event Management Final Project  

---

## ✅ TODO

- [ ] Xây dựng `OrderService` + `TicketService`  
- [ ] Tích hợp Payment Gateway (Stripe, MoMo)  
- [ ] Tích hợp SignalR để check-in real-time  
- [ ] Xuất báo cáo PDF/Excel
