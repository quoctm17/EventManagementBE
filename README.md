Quá chuẩn anh 👍. Một repo GitHub \*\*cần README.md\*\* để mô tả dự án, kiến trúc, cách setup \& run. Mình sẽ viết một file \*\*README.md\*\* phù hợp cho project \*\*EventManagementBE\*\* theo kiến trúc Clean Architecture + có DB + Auth (JWT) mà mình vừa làm nhé 🚀.  



---



\## 📄 README.md



```markdown

\# 🎫 EventManagementBE



\*\*EventManagementBE\*\* là hệ thống quản lý sự kiện / hội nghị được xây dựng bằng \*\*.NET 9 + EF Core\*\* theo kiến trúc \*\*Clean Architecture\*\*.  

Hệ thống hỗ trợ: quản lý User, Role, Event, Order, Ticket, Payment, Checkin,… với \*\*Authentication/Authorization JWT\*\*.



---



\## 🏗️ Kiến trúc



Dự án tuân theo mô hình \*\*Clean Architecture\*\* 4 lớp:



```

/EventManagementBE

&nbsp;├── EventManagement.Domain        # Chứa Entity, ValueObjects, Enums (core business model)

&nbsp;├── EventManagement.Application   # Business logic, Interfaces, Services, DTOs, Helpers

&nbsp;├── EventManagement.Infrastructure# Persistence (EF Core), Repository implementations, UnitOfWork

&nbsp;└── EventManagement.API           # ASP.NET Core WebAPI (Controllers, Middleware, Swagger,…)

```



\### 📂 Domain

\- Các \*\*Entities\*\*: `User`, `Event`, `Order`, `Ticket`, `Payment`,…  

\- \*\*Enums\*\* chuẩn hóa status: `OrderStatus`, `TicketStatus`, `PaymentStatus`, `SettlementStatus`,…  



\### 📂 Application

\- \*\*Interfaces\*\* cho Repository, Services  

\- \*\*Services\*\*: xử lý use-case (AuthService, UserService, OrderService, …)  

\- \*\*Helpers\*\*: `PasswordHelper (BCrypt)`, `JwtAuthService`, `QrCodeHelper`,…  

\- \*\*DTOs\*\*: chia thành `Requests` (input) và `Responses` (output, chuẩn hóa response HTTP)



\### 📂 Infrastructure

\- \*\*EF Core DbContext\*\* (`EventManagementDbContext`)  

\- \*\*RepositoryBase\*\*, \*\*UnitOfWork\*\*, Repositories cụ thể (UserRepository, TicketRepository, …)  

\- Migration \& cấu hình persistence  



\### 📂 API

\- \*\*Controllers\*\*: expose use-cases (`AuthController`, `UsersController`, `EventsController`, …)  

\- \*\*Middleware\*\*: Exception Handler, Logging  

\- \*\*Swagger UI\*\* + Bearer Token nhập trực tiếp để test API  



---



\## 🗄️ Database



Database: \*\*SQL Server\*\*  

Tên DB: `EventManagementDB`



\### Entities chính và trạng thái chuẩn hóa:

\- \*\*Orders.Status\*\* ∈ { Pending, Paid, Cancelled, Failed }

\- \*\*Tickets.Status\*\* ∈ { Reserved, Issued, Cancelled }

\- \*\*Payments.Status\*\* ∈ { Success, Failed, Pending }

\- \*\*Contracts.Status\*\* ∈ { Active, Expired, Terminated }

\- \*\*Settlements.Status\*\* ∈ { Pending, Processing, Completed, Failed }

\- \*\*OrganizerRequests.Status\*\* ∈ { Pending, Approved, Rejected }

\- \*\*Notification.NotificationType\*\* ∈ { Email, SMS }



📜 \*\*Script tạo DB + Schema\*\* có sẵn trong repo (file `.sql`).



📜 \*\*Seed data\*\*:

\- 4 Roles: `Admin`, `Event Organizer`, `Attendee`, `Staff`

\- 10 Users dạng hashed password BCrypt-`123456`

\- Map UserRoles chuẩn



---



\## 🔐 Authentication



\- Login: `/api/auth/login`  

\- Register: `/api/auth/register`  

\- Dùng \*\*JWT Bearer\*\* (`HS256`)  

\- Secret, Issuer, Audience: cấu hình trong `appsettings.json`



Ví dụ cấu hình:



```json

"Jwt": {

&nbsp; "Secret-Key": "i7ZpEzCWnVenUTyUFWsxFVXLty2cCTnq",

&nbsp; "Issuer": "EventManagement.API",

&nbsp; "Audience": "EventManagement.Client"

}

```



Trong Swagger, nhập \*\*Bearer Token\*\* để truy cập các endpoint bảo vệ `\[Authorize]`.



---



\## ⚙️ Cài đặt \& Chạy



\### Yêu cầu

\- .NET 9 SDK

\- SQL Server

\- Git



\### Bước 1: Clone repo

```bash

git clone git@github.com:your-username/EventManagementBE.git

cd EventManagementBE

```



\### Bước 2: Apply Migration

```bash

dotnet ef database update --project EventManagement.Infrastructure --startup-project EventManagement.API

```



\### Bước 3: Run API

```bash

dotnet run --project EventManagement.API

```



Mặc định chạy tại:  

\- Swagger UI: \[https://localhost:7270/swagger](https://localhost:7270/swagger)  

\- API: \[https://localhost:7270/api](https://localhost:7270/api)  



---



\## 🙈 Ghi chú bảo mật



\- Đã ignore `appsettings.json` (`.gitignore`) để tránh leak `ConnectionString`, `JWT Secret`.  

\- Dev nên config `dotnet user-secrets` khi làm việc cá nhân:  

&nbsp; ```bash

&nbsp; dotnet user-secrets init --project EventManagement.API

&nbsp; dotnet user-secrets set "Jwt:Secret-Key" "super-long-secret" --project EventManagement.API

&nbsp; ```



---



\## 📦 Công nghệ sử dụng



\- .NET 9

\- EF Core 9, SQL Server

\- BCrypt.Net (hash password)

\- Swagger (OpenAPI 3)

\- JWT Bearer + Authorization (Microsoft.IdentityModel.Tokens)

\- Clean Architecture pattern



---



\## 🧑‍💻 Author



\- 🚀 BootCamp .NET Cybersoft – Event Management Final Project



---



\## ✅ TODO next



\- \[ ] Xây dựng Order + Ticket service hoàn chỉnh

\- \[ ] Realtime check-in bằng SignalR

\- \[ ] Xuất báo cáo PDF/Excel

\- \[ ] Tích hợp Payment Gateway (Stripe / Momo)



```



---



