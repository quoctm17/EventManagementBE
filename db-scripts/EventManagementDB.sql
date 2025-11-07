/* ===========================================================
   DROP & CREATE DATABASE EventManagementDB
   =========================================================== */
USE master;
GO

IF DB_ID('EventManagementDB') IS NOT NULL
BEGIN
    ALTER DATABASE EventManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EventManagementDB;
END;
CREATE DATABASE EventManagementDB;
GO

USE EventManagementDB;
GO

SET NOCOUNT ON;
GO
/* ===========================================================
   I. Roles, Users, UserRoles
   =========================================================== */

CREATE TABLE Roles
(
    RoleId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Additional NVARCHAR(MAX) NULL
);

CREATE TABLE Users
(
    UserId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    AvatarUrl NVARCHAR(500) NULL,
    Address NVARCHAR(300) NULL,
    Birthdate DATE NULL,
    Description NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsActive BIT DEFAULT 1,
    Additional NVARCHAR(MAX) NULL
);

CREATE TABLE UserRoles
(
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    AssignedAt DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL,
    PRIMARY KEY (UserId, RoleId)
);

ALTER TABLE UserRoles
ADD CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE UserRoles
ADD CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId);


/* ===========================================================
   II. Venues & Seats
   =========================================================== */

CREATE TABLE Venues
(
    VenueId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    VenueName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    Province NVARCHAR(100) NOT NULL DEFAULT N'Hà Nội',
    TotalSeats INT,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL
);

CREATE TABLE VenueImages
(
    ImageId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(200) NULL,
    DisplayOrder INT NULL,
    IsMain BIT DEFAULT 0,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE VenueImages
ADD CONSTRAINT FK_VenueImages_Venue FOREIGN KEY (VenueId) REFERENCES Venues(VenueId);
GO

CREATE TABLE Seats
(
    SeatId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    RowLabel NVARCHAR(10) NOT NULL,
    SeatNumber INT NOT NULL,
    Additional NVARCHAR(MAX) NULL,
    CONSTRAINT UQ_Seats UNIQUE (VenueId, RowLabel, SeatNumber)
);

ALTER TABLE Seats
ADD CONSTRAINT FK_Seats_Venues FOREIGN KEY (VenueId) REFERENCES Venues(VenueId);


/* ===========================================================
   III. Events
   =========================================================== */

CREATE TABLE Events
(
    EventId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrganizerId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    EventName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    SaleStartTime DATETIME2 NOT NULL,
    SaleEndTime DATETIME2 NOT NULL,
    EventDate DATE NOT NULL,
    EventStartTime DATETIME2 NOT NULL,
    EventEndTime DATETIME2 NOT NULL,
    CoverImageUrl NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsPublished BIT DEFAULT 0,
    Status NVARCHAR(30) CHECK (Status IN ('Scheduled','TicketSaleOpen','TicketSaleClosed','Completed','Cancelled')) NOT NULL DEFAULT 'Scheduled',
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Events
ADD CONSTRAINT FK_Events_Organizer FOREIGN KEY (OrganizerId) REFERENCES Users(UserId);

ALTER TABLE Events
ADD CONSTRAINT FK_Events_Venue FOREIGN KEY (VenueId) REFERENCES Venues(VenueId);


CREATE TABLE Categories
(
    CategoryId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(300) NULL
);

CREATE TABLE EventCategories
(
    EventId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (EventId, CategoryId)
);

ALTER TABLE EventCategories
ADD CONSTRAINT FK_EventCategories_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

ALTER TABLE EventCategories
ADD CONSTRAINT FK_EventCategories_Category FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId);

/* ===========================================================
   IV. EventImages
   =========================================================== */

CREATE TABLE EventImages
(
    ImageId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(200) NULL,
    DisplayOrder INT NULL,
    IsCover BIT DEFAULT 0,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE EventImages
ADD CONSTRAINT FK_EventImages_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);


/* ===========================================================
   V. EventSeatMapping
   =========================================================== */

CREATE TABLE EventSeatMapping
(
    EventId UNIQUEIDENTIFIER NOT NULL,
    SeatId UNIQUEIDENTIFIER NOT NULL,
    TicketCategory NVARCHAR(50) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    IsAvailable BIT DEFAULT 1,
    Additional NVARCHAR(MAX) NULL,
    RowVersion rowversion NOT NULL,
    PRIMARY KEY (EventId, SeatId)
);

ALTER TABLE EventSeatMapping
ADD CONSTRAINT FK_EventSeat_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

ALTER TABLE EventSeatMapping
ADD CONSTRAINT FK_EventSeat_Seat FOREIGN KEY (SeatId) REFERENCES Seats(SeatId);

/* ===========================================================
   VI. Orders
   =========================================================== */

CREATE TABLE Orders
(
    OrderId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Paid','Cancelled','Failed','Refunded', 'PendingRefund', 'PartiallyRefunded')) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    OrderPendingExpires DATETIME2 NULL,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Orders
ADD CONSTRAINT FK_Orders_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

/* ===========================================================
   VII. SeatHolds
   =========================================================== */
CREATE TABLE SeatHolds
(
    HoldId UNIQUEIDENTIFIER PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    SeatId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    HoldExpiresAt DATETIME2 NOT NULL,
    OrderId UNIQUEIDENTIFIER NULL,
    CONSTRAINT UQ_Hold UNIQUE (EventId, SeatId)
);

ALTER TABLE SeatHolds
ADD CONSTRAINT FK_SeatHolds_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

ALTER TABLE SeatHolds
ADD CONSTRAINT FK_SeatHolds_Seat FOREIGN KEY (SeatId) REFERENCES Seats(SeatId);

ALTER TABLE SeatHolds
ADD CONSTRAINT FK_SeatHolds_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE SeatHolds
ADD CONSTRAINT FK_SeatHolds_Order FOREIGN KEY (OrderId)REFERENCES Orders(OrderId);

/* ===========================================================
   VIII. Tickets
   =========================================================== */

CREATE TABLE Tickets
(
    TicketId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    SeatId UNIQUEIDENTIFIER NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    AttendeeId UNIQUEIDENTIFIER NULL,
    QRCode NVARCHAR(500),
    QrImageUrl NVARCHAR(500) NULL,
    PurchaseDate DATETIME2 DEFAULT SYSDATETIME(),
    Status NVARCHAR(20) CHECK (Status IN ('Reserved','Issued','Cancelled','Refunded','CheckedIn','NoShow', 'PendingRefund')) NOT NULL DEFAULT 'Reserved',
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Tickets
ADD CONSTRAINT FK_Tickets_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId);

ALTER TABLE Tickets
ADD CONSTRAINT FK_Tickets_EventSeat FOREIGN KEY (EventId, SeatId) REFERENCES EventSeatMapping(EventId, SeatId);

ALTER TABLE Tickets
ADD CONSTRAINT FK_Tickets_Attendee FOREIGN KEY (AttendeeId) REFERENCES Users(UserId);


/* ===========================================================
   IX. PaymentMethods
   =========================================================== */

CREATE TABLE PaymentMethods
(
    PaymentMethodId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    GatewayKey NVARCHAR(100) NULL,
    MethodName NVARCHAR(50) NOT NULL,
    Provider NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    Additional NVARCHAR(MAX) NULL
);


/* ===========================================================
   X. Payments
   =========================================================== */

CREATE TABLE Payments
(
    PaymentId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    PaymentMethodId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Success','Failed','Pending')) NOT NULL,
    TransactionRef NVARCHAR(200),
    TransactionDate DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Payments
ADD CONSTRAINT FK_Payments_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId);

ALTER TABLE Payments
ADD CONSTRAINT FK_Payments_Method FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(PaymentMethodId);


/* ===========================================================
   XI. Checkins
   =========================================================== */

CREATE TABLE Checkins
(
    CheckinId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    TicketId UNIQUEIDENTIFIER NOT NULL,
    StaffId UNIQUEIDENTIFIER NOT NULL,
    CheckinTime DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Checkins
ADD CONSTRAINT FK_Checkins_Ticket FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId);

ALTER TABLE Checkins
ADD CONSTRAINT FK_Checkins_Staff FOREIGN KEY (StaffId) REFERENCES Users(UserId);


/* ===========================================================
   XII. Notifications, SystemConfigs, SystemLogs
   =========================================================== */

CREATE TABLE Notifications
(
    NotificationId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NULL,
    NotificationType NVARCHAR(20) CHECK (NotificationType IN ('Email','SMS')) NOT NULL,
    Subject NVARCHAR(200),
    Message NVARCHAR(MAX),
    SentAt DATETIME2 NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    Additional NVARCHAR(MAX) NULL
);

CREATE TABLE SystemConfigs
(
    ConfigId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ConfigKey NVARCHAR(100) UNIQUE NOT NULL,
    ConfigValue NVARCHAR(500) NOT NULL,
    Additional NVARCHAR(MAX) NULL
);

CREATE TABLE SystemLogs
(
    LogId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(255),
    Details NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Notifications
ADD CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE Notifications
ADD CONSTRAINT FK_Notifications_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

ALTER TABLE SystemLogs
ADD CONSTRAINT FK_SystemLogs_User FOREIGN KEY (UserId) REFERENCES Users(UserId);


/* ===========================================================
   XIII. OrganizerRequests
   =========================================================== */

CREATE TABLE OrganizerRequests
(
    RequestId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Reason NVARCHAR(500) NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Signing','Approved','Rejected')) NOT NULL DEFAULT 'Pending',
    RequestedAt DATETIME2 DEFAULT SYSDATETIME(),
    ProcessedAt DATETIME2 NULL,
    ProcessedBy UNIQUEIDENTIFIER NULL,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE OrganizerRequests
ADD CONSTRAINT FK_OrganizerRequests_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE OrganizerRequests
ADD CONSTRAINT FK_OrganizerRequests_ProcessedBy FOREIGN KEY (ProcessedBy) REFERENCES Users(UserId);


/* ===========================================================
   XIV. EventStaffs
   =========================================================== */

CREATE TABLE EventStaffs
(
    EventId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    AssignedBy UNIQUEIDENTIFIER NOT NULL,
    AssignedAt DATETIME2 DEFAULT SYSDATETIME(),
    IsActive BIT DEFAULT 1,
    Additional NVARCHAR(MAX) NULL,
    PRIMARY KEY (EventId, UserId)
);

ALTER TABLE EventStaffs
ADD CONSTRAINT FK_EventStaffs_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

ALTER TABLE EventStaffs
ADD CONSTRAINT FK_EventStaffs_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE EventStaffs
ADD CONSTRAINT FK_EventStaffs_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserId);


/* ===========================================================
   XV. Contracts
   =========================================================== */

CREATE TABLE Contracts
(
    ContractId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrganizerId UNIQUEIDENTIFIER NOT NULL,
    ContractType NVARCHAR(20) CHECK (ContractType IN ('Framework','EventSpecific')) NOT NULL,
    EventId UNIQUEIDENTIFIER NULL,
    ContractFileUrl NVARCHAR(500) NOT NULL,
    EffectiveDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ExpiryDate DATETIME2 NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Active','Expired','Terminated')) DEFAULT 'Active',
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE Contracts
ADD CONSTRAINT FK_Contracts_Organizer FOREIGN KEY (OrganizerId) REFERENCES Users(UserId);

ALTER TABLE Contracts
ADD CONSTRAINT FK_Contracts_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);


/* ===========================================================
   XVI. Settlements
   =========================================================== */

CREATE TABLE Settlements
(
    SettlementId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrganizerId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    TotalRevenue DECIMAL(18,2) NOT NULL,
    CommissionFee DECIMAL(18,2) NOT NULL,
    NetAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Processing','Completed','Failed')) DEFAULT 'Pending',
    SettlementDate DATETIME2 NULL,
    ProcessedBy UNIQUEIDENTIFIER NULL,
    Note NVARCHAR(500) NULL,
    Additional NVARCHAR(MAX) NULL
);


ALTER TABLE Settlements
ADD CONSTRAINT FK_Settlements_Organizer FOREIGN KEY (OrganizerId) REFERENCES Users(UserId);

ALTER TABLE Settlements
ADD CONSTRAINT FK_Settlements_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);

/* ===========================================================
    XVII. UserBankAccounts — lưu tài khoản ngân hàng người dùng (tùy chọn nhớ lại)
   =========================================================== */

CREATE TABLE UserBankAccounts
(
    BankAccountId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    BankName NVARCHAR(200) NOT NULL,
    AccountNumber NVARCHAR(50) NOT NULL,
    AccountHolderName NVARCHAR(150) NOT NULL,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    Additional NVARCHAR(MAX) NULL,

    CONSTRAINT UQ_UserBankAccounts UNIQUE (UserId, BankName, AccountNumber)
);

ALTER TABLE UserBankAccounts
ADD CONSTRAINT FK_UserBankAccounts_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
GO

/* ===========================================================
    XVIII. RefundRequests — yêu cầu hoàn tiền
   =========================================================== */

CREATE TABLE RefundRequests
(
    RefundRequestId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    BankAccountId UNIQUEIDENTIFIER NULL,
    -- nếu chọn tài khoản đã lưu
    BankName NVARCHAR(200) NULL,
    -- nếu nhập tay
    AccountNumber NVARCHAR(50) NULL,
    AccountHolderName NVARCHAR(150) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Approved','Rejected','Paid')) DEFAULT 'Pending',
    Reason NVARCHAR(500) NULL,
    AdminNote NVARCHAR(500) NULL,
    ReceiptImageUrl NVARCHAR(500) NULL,
    -- admin upload biên lai trả tiền
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    ProcessedAt DATETIME2 NULL,
    ProcessedBy UNIQUEIDENTIFIER NULL,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE RefundRequests
ADD CONSTRAINT FK_RefundRequests_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId);

ALTER TABLE RefundRequests
ADD CONSTRAINT FK_RefundRequests_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE RefundRequests
ADD CONSTRAINT FK_RefundRequests_Bank FOREIGN KEY (BankAccountId) REFERENCES UserBankAccounts(BankAccountId);

ALTER TABLE RefundRequests
ADD CONSTRAINT FK_RefundRequests_Admin FOREIGN KEY (ProcessedBy) REFERENCES Users(UserId);
GO

/* ===========================================================
    XVIII. RefundRequests — yêu cầu hoàn tiền
   =========================================================== */

CREATE TABLE Transactions
(
    TransactionId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL,
    PaymentId UNIQUEIDENTIFIER NULL,
    RefundRequestId UNIQUEIDENTIFIER NULL,
    SettlementId UNIQUEIDENTIFIER NULL,
    OrderId UNIQUEIDENTIFIER NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Direction NVARCHAR(10) CHECK (Direction IN ('In','Out')) NOT NULL,
    Purpose NVARCHAR(50) CHECK (Purpose IN ('TicketPayment','Refund','SystemAdjustment','Settlement')) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Success','Failed')) NOT NULL DEFAULT 'Success',
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    SystemBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(500) NULL,
    Additional NVARCHAR(MAX) NULL
);

-- FK liên kết đến các bảng liên quan
ALTER TABLE Transactions
ADD CONSTRAINT FK_Transactions_User FOREIGN KEY (UserId) REFERENCES Users(UserId);

ALTER TABLE Transactions
ADD CONSTRAINT FK_Transactions_Payment FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId);

ALTER TABLE Transactions
ADD CONSTRAINT FK_Transactions_Refund FOREIGN KEY (RefundRequestId) REFERENCES RefundRequests(RefundRequestId);

ALTER TABLE Transactions
ADD CONSTRAINT FK_Transactions_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId);
GO

/* ===========================================================
    XIX. RefundPolicies — Chính sách hoàn tiền
   =========================================================== */

CREATE TABLE RefundPolicies (
    RefundPolicyId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NULL,        -- NULL = policy mặc định toàn hệ thống
    TicketCategory NVARCHAR(50) NULL,     -- NULL = áp dụng cho mọi loại vé
    IsEnabled BIT DEFAULT 1,
    EffectiveFrom DATETIME2 DEFAULT SYSDATETIME(),
    EffectiveTo DATETIME2 NULL,
    Note NVARCHAR(500) NULL,
    Additional NVARCHAR(MAX) NULL
);

ALTER TABLE RefundPolicies
ADD CONSTRAINT FK_RefundPolicies_Event FOREIGN KEY (EventId) REFERENCES Events(EventId);


/* ===========================================================
    XX. RefundPolicyRules — Các điều kiện trong chính sách
   =========================================================== */
CREATE TABLE RefundPolicyRules (
    RefundPolicyRuleId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    RefundPolicyId UNIQUEIDENTIFIER NOT NULL,
    CutoffMinutesBeforeStart INT NOT NULL,    -- Số phút trước giờ bắt đầu event
    RefundPercent DECIMAL(5,2) CHECK (RefundPercent BETWEEN 0 AND 100) NOT NULL,
    FlatFee DECIMAL(18,2) DEFAULT 0,
    RuleOrder INT NOT NULL
);
ALTER TABLE RefundPolicyRules
ADD CONSTRAINT FK_RefundPolicyRules_RefundPolicies FOREIGN KEY (RefundPolicyId) REFERENCES RefundPolicies(RefundPolicyId);

/* ===========================================================
    XXI. RefundRequestItems — Gắn vé cụ thể với yêu cầu hoàn tiền (partial refund)
   =========================================================== */
CREATE TABLE RefundRequestItems (
    RefundRequestItemId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    RefundRequestId UNIQUEIDENTIFIER NOT NULL,
    TicketId UNIQUEIDENTIFIER NOT NULL,
    RefundRuleId UNIQUEIDENTIFIER NULL,
    RefundPercentApplied DECIMAL(5,2) NULL,
    RefundAmount DECIMAL(18,2) NULL
);
    ALTER TABLE RefundRequestItems
    ADD CONSTRAINT FK_RefundRequestItems_RefundRequests FOREIGN KEY (RefundRequestId) REFERENCES RefundRequests(RefundRequestId);

    ALTER TABLE RefundRequestItems
    ADD CONSTRAINT FK_RefundRequestItems_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId);

    ALTER TABLE RefundRequestItems
    ADD CONSTRAINT FK_RefundRequestItems_RefundPolicyRules FOREIGN KEY (RefundRuleId) REFERENCES RefundPolicyRules(RefundPolicyRuleId)

/* ===========================================================
   SEED Roles
   =========================================================== */
INSERT INTO Roles
    (RoleId, RoleName)
VALUES
    ('aa1e2015-b087-4e02-bc2f-0c2d83a58393', N'Admin'),
    ('bac224e0-0e8e-40fc-b9fc-1d3ebe85d10f', N'Event Organizer'),
    ('d6858787-4578-48c3-8e54-4747e172894c', N'Attendee'),
    ('64e20240-2862-4836-8c40-55be5d215eba', N'Staff');
GO

/* ===========================================================
   Lấy RoleId các role để join
   =========================================================== */
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT RoleId
FROM Roles
WHERE RoleName = N'Admin');
DECLARE @OrganizerId UNIQUEIDENTIFIER = (SELECT RoleId
FROM Roles
WHERE RoleName = N'Event Organizer');
DECLARE @AttendeeId UNIQUEIDENTIFIER = (SELECT RoleId
FROM Roles
WHERE RoleName = N'Attendee');
DECLARE @StaffId UNIQUEIDENTIFIER = (SELECT RoleId
FROM Roles
WHERE RoleName = N'Staff');

/* BCrypt hash cho mật khẩu '123456' (work factor 12) */
DECLARE @Hash NVARCHAR(255) = '$2y$12$IwLxvzleouevH3v98zIhe.s4oBpTiBaBp/o5Mzha.tx79jhVaGRuO';

/* ===========================================================
   SEED Users (with AvatarUrl)
   =========================================================== */
-- Admins (2)
INSERT INTO Users
    (UserId, FullName, Email, PasswordHash, Phone, AvatarUrl, Address, Birthdate, Description)
VALUES
    ('ae0990e9-cb85-4518-9935-1aab022fb46f',
        N'Lê Thị Huỳnh Như', 'tranminhquoc0711@gmail.com', @Hash, '0901234567',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/ae0990e9-cb85-4518-9935-1aab022fb46f.jpg',
        N'180 Nguyễn Văn Cừ, Q.5, TP.HCM', '1992-07-12',
        N'Quản trị viên tổng hệ thống, người có hơn 10 năm kinh nghiệm trong điều phối sự kiện và quản lý dữ liệu.'),
    ('97da7d73-b0e6-4b23-9f73-5772eb90240c',
        N'Trần Thị Bình', 'binh.tran@demo.vn', @Hash, '0912233445',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/97da7d73-b0e6-4b23-9f73-5772eb90240c.jpg',
        N'25 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội', '1989-03-20',
        N'Admin phụ trách nội dung và kiểm duyệt, luôn đảm bảo mọi sự kiện diễn ra suôn sẻ và minh bạch.');

-- Organizers (2)
INSERT INTO Users
    (UserId, FullName, Email, PasswordHash, Phone, AvatarUrl, Address, Birthdate, Description)
VALUES
    ('c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28',
        N'Lê Hoàng Nam', 'nam.le@demo.vn', @Hash, '0908889991',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28.jpg',
        N'10 Nguyễn Văn Linh, Đà Nẵng', '1990-11-05',
        N'Nhà tổ chức sự kiện chuyên nghiệp trong lĩnh vực công nghệ và khởi nghiệp. '
        + N'Anh từng quản lý hơn 50 hội thảo quốc tế về AI, Blockchain, và Cloud tại Việt Nam.'),
    ('1db21752-adc3-4eb7-a8f2-c0a83ac3082e',
        N'Phạm Thu Hà', 'ha.pham@demo.vn', @Hash, '0933344455',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/1db21752-adc3-4eb7-a8f2-c0a83ac3082e.jpg',
        N'55 Nguyễn Trãi, Q.1, TP.HCM', '1993-09-14',
        N'Chuyên gia tổ chức các sự kiện văn hóa – giáo dục. '
        + N'Đam mê lan tỏa tri thức và giao lưu nghệ thuật, đã hợp tác cùng nhiều đơn vị đào tạo lớn.');

-- Attendees (3)
INSERT INTO Users
    (UserId, FullName, Email, PasswordHash, Phone, AvatarUrl, Address, Birthdate, Description)
VALUES
    ('4f07db8f-b3c8-4403-a557-d6d12c426c14',
        N'Ngô Quang Huy', 'huy.ngo@demo.vn', @Hash, '0967788990',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/4f07db8f-b3c8-4403-a557-d6d12c426c14.jpg',
        N'75 Mai Chí Thọ, TP.Thủ Đức, TP.HCM', '1998-04-02',
        N'Kỹ sư trẻ yêu công nghệ, thường xuyên tham gia các hội thảo về AI và dữ liệu lớn.'),
    ('21412353-2343-4d01-a7d6-e1e0f2ac5bc6',
        N'Đặng Thị Lan', 'lan.dang@demo.vn', @Hash, '0974455667',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/21412353-2343-4d01-a7d6-e1e0f2ac5bc6.jpg',
        N'32 Lạch Tray, Hải Phòng', '1999-08-19',
        N'Nhà thiết kế trẻ yêu thích nghệ thuật và âm nhạc, tham dự nhiều chương trình biểu diễn quốc tế.'),
    ('e6369c1b-39d1-4239-ad4e-e404b61c5e59',
        N'Hoàng Minh Tuấn', 'tuan.hoang@demo.vn', @Hash, '0981122334',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/e6369c1b-39d1-4239-ad4e-e404b61c5e59.jpg',
        N'210 Trường Chinh, Đà Nẵng', '1997-12-11',
        N'Chuyên viên du lịch trẻ, đam mê khám phá và học hỏi trong các hội nghị quốc tế về phát triển bền vững.');

-- Staff (3)
INSERT INTO Users
    (UserId, FullName, Email, PasswordHash, Phone, AvatarUrl, Address, Birthdate, Description)
VALUES
    ('0f4f6f91-9df2-4f9d-bb79-6c9efd868037',
        N'Vũ Thị Hoa', 'hoa.vu@demo.vn', @Hash, '0934567890',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/0f4f6f91-9df2-4f9d-bb79-6c9efd868037.jpg',
        N'48 Nguyễn Huệ, Huế', '1995-02-23',
        N'Nhân viên hậu cần chu đáo, phụ trách kiểm soát vé và hỗ trợ khách tại khu vực miền Trung.'),
    ('8d8b1240-d478-4b58-b882-59c288e4c6db',
        N'Nguyễn Khắc Duy', 'duy.nguyen@demo.vn', @Hash, '0945566778',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/8d8b1240-d478-4b58-b882-59c288e4c6db.jpg',
        N'90 Nguyễn Hữu Thọ, TP.HCM', '1994-06-15',
        N'Trưởng nhóm kỹ thuật sân khấu, chuyên vận hành ánh sáng và âm thanh sự kiện.'),
    ('fc2dd930-807e-469a-a0de-b2a3283c02d5',
        N'Phan Văn Long', 'long.phan@demo.vn', @Hash, '0956677889',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/users/avatars/fc2dd930-807e-469a-a0de-b2a3283c02d5.jpg',
        N'145 Võ Văn Kiệt, Đà Nẵng', '1996-10-08',
        N'Nhân sự quản lý cổng check-in, tận tâm hỗ trợ khách và đảm bảo an ninh cho sự kiện.');


/* ===========================================================
   SEED UserRoles (map từng User với Role)
   =========================================================== */
-- Map Admins
INSERT INTO UserRoles
    (UserId, RoleId)
SELECT UserId, @AdminId
FROM Users
WHERE Email IN ('nhu.le@demo.vn','binh.tran@demo.vn');

-- Map Organizers
INSERT INTO UserRoles
    (UserId, RoleId)
SELECT UserId, @OrganizerId
FROM Users
WHERE Email IN ('nam.le@demo.vn','ha.pham@demo.vn');

-- Map Attendees
INSERT INTO UserRoles
    (UserId, RoleId)
SELECT UserId, @AttendeeId
FROM Users
WHERE Email IN ('huy.ngo@demo.vn','lan.dang@demo.vn','tuan.hoang@demo.vn');

-- Map Staff
INSERT INTO UserRoles
    (UserId, RoleId)
SELECT UserId, @StaffId
FROM Users
WHERE Email IN ('hoa.vu@demo.vn','duy.nguyen@demo.vn','long.phan@demo.vn');

/* ===========================================================
   SEED OrganizerRequests
   =========================================================== */
-- Giả sử Admin có UserId: 'ae0990e9-cb85-4518-9935-1aab022fb46f' (Lê Thị Huỳnh Như)
-- Organizer Users đã seed:
-- 'c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28' (Lê Hoàng Nam)
-- '1db21752-adc3-4eb7-a8f2-c0a83ac3082e' (Phạm Thu Hà)

INSERT INTO OrganizerRequests
    (RequestId, UserId, Reason, Status, RequestedAt, ProcessedAt, ProcessedBy)
VALUES
    ('3b975914-1572-4ecf-b5a7-af7ab9358d21',
        'c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28',
        N'Tôi mong muốn tổ chức sự kiện công nghệ trong năm nay',
        'Approved',
        '2025-01-05T10:00:00',
        '2025-01-07T15:00:00',
        'ae0990e9-cb85-4518-9935-1aab022fb46f'),

    ('99487220-3d1a-44f1-a879-658cf1df9a1f',
        '1db21752-adc3-4eb7-a8f2-c0a83ac3082e',
        N'Tôi muốn trở thành đối tác để tổ chức các hội thảo giáo dục',
        'Approved',
        '2025-01-10T14:30:00',
        '2025-01-12T09:00:00',
        'ae0990e9-cb85-4518-9935-1aab022fb46f');

/* ===========================================================
   SEED Contracts (tương ứng OrganizerRequests đã approved)
   =========================================================== */
INSERT INTO Contracts
    (ContractId, OrganizerId, ContractType, EventId, ContractFileUrl, EffectiveDate, ExpiryDate, Status)
VALUES
    ('a5c4f64a-d199-4f15-a155-4d409cf77dd4',
        'c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28',
        'Framework',
        NULL,
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/contracts/c11a2083-b8ea-4f51-aaa6-9c8b8d0c6a28/framework.pdf',
        '2025-01-07T15:00:00',
        '2027-01-07T15:00:00',
        'Active'),

    ('0eaf7c5e-dfbe-466c-b712-5874cc573ff0',
        '1db21752-adc3-4eb7-a8f2-c0a83ac3082e',
        'Framework',
        NULL,
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/contracts/1db21752-adc3-4eb7-a8f2-c0a83ac3082e/framework.pdf',
        '2025-01-12T09:00:00',
        '2027-01-12T09:00:00',
        'Active');
GO

/* ===========================================================
   SEED Venues
   =========================================================== */
INSERT INTO Venues
    (VenueId, VenueName, Address, Province, TotalSeats)
VALUES
    ('31ab3970-0167-462d-9adb-dead57d901aa', N'Trung tâm Hội nghị Quốc gia', N'Số 57 đường Phạm Hùng, Mễ Trì, Nam Từ Liêm, Hà Nội', N'Hà Nội', 200),
    ('7acfb891-6639-46a0-b6e6-994a87c0c922', N'Nhà hát Thành phố Hồ Chí Minh', N'07 Công Trường Lam Sơn, Quận 1, TP Hồ Chí Minh', N'TP. Hồ Chí Minh', 120),
    ('42a642d1-9393-41be-b635-b659f368e3bb', N'Trung tâm Hội nghị Đà Nẵng', N'Số 1 Trường Sa, Ngũ Hành Sơn, Đà Nẵng', N'Đà Nẵng', 150);

/* ===========================================================
   SEED VenueImages
   =========================================================== */
-- Images for Trung tâm Hội nghị Quốc gia
INSERT INTO VenueImages
    (ImageId, VenueId, ImageUrl, Caption, DisplayOrder, IsMain)
VALUES
    (NEWID(), '31ab3970-0167-462d-9adb-dead57d901aa',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/31ab3970-0167-462d-9adb-dead57d901aa/main.jpg',
        N'Ảnh chính - Trung tâm Hội nghị Quốc gia', 0, 1),
    (NEWID(), '31ab3970-0167-462d-9adb-dead57d901aa',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/31ab3970-0167-462d-9adb-dead57d901aa/gallery1.jpg',
        N'Ảnh gallery 1', 1, 0),
    (NEWID(), '31ab3970-0167-462d-9adb-dead57d901aa',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/31ab3970-0167-462d-9adb-dead57d901aa/gallery2.jpg',
        N'Ảnh gallery 2', 2, 0),
    (NEWID(), '31ab3970-0167-462d-9adb-dead57d901aa',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/31ab3970-0167-462d-9adb-dead57d901aa/gallery3.jpg',
        N'Ảnh gallery 3', 3, 0),
    (NEWID(), '31ab3970-0167-462d-9adb-dead57d901aa',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/31ab3970-0167-462d-9adb-dead57d901aa/gallery4.jpg',
        N'Ảnh gallery 4', 4, 0);


-- Images for Nhà hát Thành phố Hồ Chí Minh
INSERT INTO VenueImages
    (ImageId, VenueId, ImageUrl, Caption, DisplayOrder, IsMain)
VALUES
    (NEWID(), '7acfb891-6639-46a0-b6e6-994a87c0c922',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/7acfb891-6639-46a0-b6e6-994a87c0c922/main.jpg',
        N'Ảnh chính - Nhà hát TP.HCM', 0, 1),
    (NEWID(), '7acfb891-6639-46a0-b6e6-994a87c0c922',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/7acfb891-6639-46a0-b6e6-994a87c0c922/gallery1.jpg',
        N'Ảnh gallery 1', 1, 0),
    (NEWID(), '7acfb891-6639-46a0-b6e6-994a87c0c922',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/7acfb891-6639-46a0-b6e6-994a87c0c922/gallery2.jpg',
        N'Ảnh gallery 2', 2, 0),
    (NEWID(), '7acfb891-6639-46a0-b6e6-994a87c0c922',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/7acfb891-6639-46a0-b6e6-994a87c0c922/gallery3.jpg',
        N'Ảnh gallery 3', 3, 0),
    (NEWID(), '7acfb891-6639-46a0-b6e6-994a87c0c922',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/7acfb891-6639-46a0-b6e6-994a87c0c922/gallery4.jpg',
        N'Ảnh gallery 4', 4, 0);


-- Images for Trung tâm Hội nghị Đà Nẵng
INSERT INTO VenueImages
    (ImageId, VenueId, ImageUrl, Caption, DisplayOrder, IsMain)
VALUES
    (NEWID(), '42a642d1-9393-41be-b635-b659f368e3bb',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/42a642d1-9393-41be-b635-b659f368e3bb/main.jpg',
        N'Ảnh chính - Trung tâm Hội nghị Đà Nẵng', 0, 1),
    (NEWID(), '42a642d1-9393-41be-b635-b659f368e3bb',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/42a642d1-9393-41be-b635-b659f368e3bb/gallery1.jpg',
        N'Ảnh gallery 1', 1, 0),
    (NEWID(), '42a642d1-9393-41be-b635-b659f368e3bb',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/42a642d1-9393-41be-b635-b659f368e3bb/gallery2.jpg',
        N'Ảnh gallery 2', 2, 0),
    (NEWID(), '42a642d1-9393-41be-b635-b659f368e3bb',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/42a642d1-9393-41be-b635-b659f368e3bb/gallery3.jpg',
        N'Ảnh gallery 3', 3, 0),
    (NEWID(), '42a642d1-9393-41be-b635-b659f368e3bb',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/venues/42a642d1-9393-41be-b635-b659f368e3bb/gallery4.jpg',
        N'Ảnh gallery 4', 4, 0);

/* ===========================================================
   Lấy VenueId để seed ghế
   =========================================================== */
DECLARE @VenueHN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Quốc gia');
DECLARE @VenueHCM UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Nhà hát Thành phố Hồ Chí Minh');
DECLARE @VenueDN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Đà Nẵng');


/* ===========================================================
   SEED Seats cho Hà Nội: 10 hàng (A-J), mỗi hàng 20 ghế
   =========================================================== */
DECLARE @row INT = 1;
WHILE @row <= 10
BEGIN
    DECLARE @rowLabel NVARCHAR(10) = CHAR(64 + @row);
    -- 65='A'
    DECLARE @seat INT = 1;
    WHILE @seat <= 20
    BEGIN
        INSERT INTO Seats
            (VenueId, RowLabel, SeatNumber)
        VALUES
            (@VenueHN, @rowLabel, @seat);
        SET @seat = @seat + 1;
    END
    SET @row = @row + 1;
END;


/* ===========================================================
   SEED Seats cho HCM: 8 hàng (A-H), mỗi hàng 15 ghế
   =========================================================== */
SET @row = 1;
WHILE @row <= 8
BEGIN
    DECLARE @rowLabelHCM NVARCHAR(10) = CHAR(64 + @row);
    DECLARE @seatHCM INT = 1;
    WHILE @seatHCM <= 15
    BEGIN
        INSERT INTO Seats
            (VenueId, RowLabel, SeatNumber)
        VALUES
            (@VenueHCM, @rowLabelHCM, @seatHCM);
        SET @seatHCM = @seatHCM + 1;
    END
    SET @row = @row + 1;
END;


/* ===========================================================
   SEED Seats cho Đà Nẵng: 10 hàng (A-J), mỗi hàng 15 ghế
   =========================================================== */
SET @row = 1;
WHILE @row <= 10
BEGIN
    DECLARE @rowLabelDN NVARCHAR(10) = CHAR(64 + @row);
    DECLARE @seatDN INT = 1;
    WHILE @seatDN <= 15
    BEGIN
        INSERT INTO Seats
            (VenueId, RowLabel, SeatNumber)
        VALUES
            (@VenueDN, @rowLabelDN, @seatDN);
        SET @seatDN = @seatDN + 1;
    END
    SET @row = @row + 1;
END;
GO
/* ===========================================================
   SEED Categories
   =========================================================== */
INSERT INTO Categories
    (CategoryId, CategoryName, Description)
VALUES
    -- Lĩnh vực
    ('11111111-aaaa-4bbb-8ccc-111111111111', N'Công nghệ', N'Sự kiện, hội thảo, triển lãm về lĩnh vực công nghệ.'),
    ('22222222-bbbb-4ccc-8ddd-222222222222', N'Âm nhạc', N'Các chương trình, đêm nhạc, hòa nhạc, biểu diễn.'),
    ('33333333-cccc-4ddd-8eee-333333333333', N'Du lịch', N'Hội nghị, triển lãm, xúc tiến du lịch.'),
    ('44444444-dddd-4eee-8fff-444444444444', N'Giáo dục', N'Hội thảo, workshop hoặc hội nghị chuyên đề giáo dục.'),
    -- Loại hình (mới)
    ('55555555-eeee-4fff-8aaa-555555555555', N'Hội thảo', N'Sự kiện chuyên đề, tập trung chia sẻ tri thức.'),
    ('66666666-ffff-4aaa-8bbb-666666666666', N'Triển lãm', N'Sự kiện trưng bày, giới thiệu sản phẩm, công nghệ.'),
    ('77777777-9999-4ccc-8ddd-777777777777', N'Giải trí', N'Sự kiện mang tính thư giãn, biểu diễn, show nghệ thuật.'),
    ('88888888-aaaa-4ddd-8eee-888888888888', N'Workshop', N'Sự kiện huấn luyện, thực hành kỹ năng chuyên sâu.');
GO

/* ===========================================================
   SEED Events 
   =========================================================== */

-- Lấy OrganizerId (ví dụ: Nam + Hà)
DECLARE @OrgNam UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email = 'nam.le@demo.vn');
DECLARE @OrgHa  UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email = 'ha.pham@demo.vn');

-- Lấy VenueId
DECLARE @VenueHN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Quốc gia');
DECLARE @VenueHCM UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Nhà hát Thành phố Hồ Chí Minh');
DECLARE @VenueDN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Đà Nẵng');


-- Event: Hội thảo Công nghệ 2025 (Nam tổ chức tại Hà Nội)
INSERT INTO Events
    (EventId, OrganizerId, VenueId, EventName, Description, SaleStartTime, SaleEndTime, EventDate, EventStartTime, EventEndTime, CoverImageUrl, IsPublished, Status)
VALUES
    ('e6ab287a-ff85-4c18-8159-109f49616636',
        @OrgNam,
        @VenueHN,
        N'Hội thảo Công nghệ 2025',
        N'Sự kiện quy tụ các chuyên gia về AI, Blockchain, Cloud...',
        '2025-09-28T09:00:00',
        '2025-12-28T23:59:00',
        '2026-01-10',
        '2026-01-10T08:30:00',
        '2026-01-12T17:30:00',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/e6ab287a-ff85-4c18-8159-109f49616636/cover.jpg',
        1,
        'TicketSaleOpen');


-- Event: Đêm nhạc Giao Hưởng (Hà tổ chức tại HCM)
INSERT INTO Events
    (EventId, OrganizerId, VenueId, EventName, Description, SaleStartTime, SaleEndTime, EventDate, EventStartTime, EventEndTime, CoverImageUrl, IsPublished, Status)
VALUES
    ('6cb86389-78f2-4d53-8bdf-36991e3b1321',
        @OrgHa,
        @VenueHCM,
        N'Đêm nhạc Giao Hưởng',
        N'Một đêm nhạc đặc biệt với dàn nhạc giao hưởng quốc tế.',
        '2025-09-30T10:00:00',
        '2025-12-30T22:00:00',
        '2026-01-15',
        '2026-01-15T19:00:00',
        '2026-01-15T22:30:00',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/6cb86389-78f2-4d53-8bdf-36991e3b1321/cover.jpg',
        1,
        'TicketSaleOpen');


-- Event: Hội nghị Du lịch Biển (Nam tổ chức tại Đà Nẵng)
INSERT INTO Events
    (EventId, OrganizerId, VenueId, EventName, Description, SaleStartTime, SaleEndTime, EventDate, EventStartTime, EventEndTime, CoverImageUrl, IsPublished, Status)
VALUES
    ('bafb2b62-89da-4c79-af42-373ebad7f993',
        @OrgNam,
        @VenueDN,
        N'Hội nghị Du lịch Biển Việt Nam',
        N'Hội nghị chia sẻ giải pháp phát triển du lịch biển bền vững.',
        '2025-10-01T08:00:00',
        '2025-12-31T18:00:00',
        '2026-01-20',
        '2026-01-20T08:30:00',
        '2026-01-22T17:00:00',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/bafb2b62-89da-4c79-af42-373ebad7f993/cover.jpg',
        1,
        'TicketSaleOpen');
GO

/* ===========================================================
   SEED EventImages
   =========================================================== */
-- Event: Hội thảo Công nghệ 2025
INSERT INTO EventImages
    (ImageId, EventId, ImageUrl, Caption, DisplayOrder, IsCover)
VALUES
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49616636',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/e6ab287a-ff85-4c18-8159-109f49616636/cover.jpg',
        N'Ảnh bìa', 0, 1),
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49616636',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/e6ab287a-ff85-4c18-8159-109f49616636/image1.jpg',
        N'Khán giả sự kiện', 1, 0),
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49616636',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/e6ab287a-ff85-4c18-8159-109f49616636/image2.jpg',
        N'Diễn giả chính', 2, 0);


-- Event: Đêm nhạc Giao Hưởng
INSERT INTO EventImages
    (ImageId, EventId, ImageUrl, Caption, DisplayOrder, IsCover)
VALUES
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b1321',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/6cb86389-78f2-4d53-8bdf-36991e3b1321/cover.png',
        N'Ảnh bìa', 0, 1),
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b1321',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/6cb86389-78f2-4d53-8bdf-36991e3b1321/image1.jpg',
        N'Sân khấu hòa nhạc', 1, 0),
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b1321',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/6cb86389-78f2-4d53-8bdf-36991e3b1321/image2.jpg',
        N'Dàn nhạc biểu diễn', 2, 0);


-- Event: Hội nghị Du lịch Biển
INSERT INTO EventImages
    (ImageId, EventId, ImageUrl, Caption, DisplayOrder, IsCover)
VALUES
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad7f993',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/bafb2b62-89da-4c79-af42-373ebad7f993/cover.jpg',
        N'Ảnh bìa', 0, 1),
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad7f993',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/bafb2b62-89da-4c79-af42-373ebad7f993/image1.jpg',
        N'Gian triển lãm', 1, 0),
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad7f993',
        'https://res.cloudinary.com/dnrs0dvt4/image/upload/event-management/events/bafb2b62-89da-4c79-af42-373ebad7f993/image2.jpg',
        N'Khách tham dự', 2, 0);
GO

/* ===========================================================
   SEED EventCategories
   =========================================================== */
-- Hội thảo Công nghệ 2025 => Công nghệ + Hội thảo + Giáo dục
INSERT INTO EventCategories
    (EventId, CategoryId)
VALUES
    ('e6ab287a-ff85-4c18-8159-109f49616636', '11111111-aaaa-4bbb-8ccc-111111111111'),
    -- Công nghệ
    ('e6ab287a-ff85-4c18-8159-109f49616636', '44444444-dddd-4eee-8fff-444444444444'),
    -- Giáo dục
    ('e6ab287a-ff85-4c18-8159-109f49616636', '55555555-eeee-4fff-8aaa-555555555555');
-- Hội thảo


-- Đêm nhạc Giao Hưởng => Âm nhạc + Giải trí + Triển lãm
INSERT INTO EventCategories
    (EventId, CategoryId)
VALUES
    ('6cb86389-78f2-4d53-8bdf-36991e3b1321', '22222222-bbbb-4ccc-8ddd-222222222222'),
    -- Âm nhạc
    ('6cb86389-78f2-4d53-8bdf-36991e3b1321', '77777777-9999-4ccc-8ddd-777777777777'),
    -- Giải trí
    ('6cb86389-78f2-4d53-8bdf-36991e3b1321', '66666666-ffff-4aaa-8bbb-666666666666');
-- Triển lãm


-- Hội nghị Du lịch Biển Việt Nam => Du lịch + Triển lãm + Công nghệ
INSERT INTO EventCategories
    (EventId, CategoryId)
VALUES
    ('bafb2b62-89da-4c79-af42-373ebad7f993', '33333333-cccc-4ddd-8eee-333333333333'),
    -- Du lịch
    ('bafb2b62-89da-4c79-af42-373ebad7f993', '66666666-ffff-4aaa-8bbb-666666666666'),
    -- Triển lãm
    ('bafb2b62-89da-4c79-af42-373ebad7f993', '11111111-aaaa-4bbb-8ccc-111111111111'); -- Công nghệ
GO

/* ===========================================================
   SEED EventSeatMapping (gán giá theo khu vực chỗ ngồi)
   =========================================================== */
DECLARE @EventHN UNIQUEIDENTIFIER = (SELECT EventId
FROM Events
WHERE EventName = N'Hội thảo Công nghệ 2025');
DECLARE @EventHCM UNIQUEIDENTIFIER = (SELECT EventId
FROM Events
WHERE EventName = N'Đêm nhạc Giao Hưởng');
DECLARE @EventDN UNIQUEIDENTIFIER = (SELECT EventId
FROM Events
WHERE EventName = N'Hội nghị Du lịch Biển Việt Nam');

-- Lấy VenueId
DECLARE @VenueHN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Quốc gia');
DECLARE @VenueHCM UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Nhà hát Thành phố Hồ Chí Minh');
DECLARE @VenueDN UNIQUEIDENTIFIER = (SELECT VenueId
FROM Venues
WHERE VenueName = N'Trung tâm Hội nghị Đà Nẵng');

-- Hội thảo Công nghệ 2025 (HN) - 200 seats
-- VIP: Rows A-C (500k), Standard: Rows D-G (300k), Economy: H-J (150k)
INSERT INTO EventSeatMapping
    (EventId, SeatId, TicketCategory, Price)
SELECT @EventHN, SeatId,
    CASE 
         WHEN RowLabel IN ('A','B','C') THEN N'VIP'
         WHEN RowLabel IN ('D','E','F','G') THEN N'Standard'
         ELSE N'Economy'
       END,
    CASE 
         WHEN RowLabel IN ('A','B','C') THEN 500000
         WHEN RowLabel IN ('D','E','F','G') THEN 300000
         ELSE 150000
       END
FROM Seats
WHERE VenueId = @VenueHN;

-- Đêm nhạc Giao Hưởng (HCM) - 120 seats
-- VIP: Rows A-B (800k), Standard: Rows C-E (500k), Economy: F-H (300k)
INSERT INTO EventSeatMapping
    (EventId, SeatId, TicketCategory, Price)
SELECT @EventHCM, SeatId,
    CASE 
         WHEN RowLabel IN ('A','B') THEN N'VIP'
         WHEN RowLabel IN ('C','D','E') THEN N'Standard'
         ELSE N'Economy'
       END,
    CASE 
         WHEN RowLabel IN ('A','B') THEN 800000
         WHEN RowLabel IN ('C','D','E') THEN 500000
         ELSE 300000
       END
FROM Seats
WHERE VenueId = @VenueHCM;

-- Hội nghị Du lịch Biển (Đà Nẵng) - 150 seats
-- VIP: Rows A-B (400k), Standard: C-F (250k), Economy: G-J (120k)
INSERT INTO EventSeatMapping
    (EventId, SeatId, TicketCategory, Price)
SELECT @EventDN, SeatId,
    CASE 
         WHEN RowLabel IN ('A','B') THEN N'VIP'
         WHEN RowLabel IN ('C','D','E','F') THEN N'Standard'
         ELSE N'Economy'
       END,
    CASE 
         WHEN RowLabel IN ('A','B') THEN 400000
         WHEN RowLabel IN ('C','D','E','F') THEN 250000
         ELSE 120000
       END
FROM Seats
WHERE VenueId = @VenueDN;

/* ===========================================================
   SEED PaymentMethods
   =========================================================== */
INSERT INTO PaymentMethods
    (PaymentMethodId, GatewayKey, MethodName, Provider, IsActive)
VALUES
    ('b21f1c96-ced6-4abc-9704-cdd2d4e5a72a', N'PAY2S', N'Pay2S', N'Pay2S Gateway', 1),
    ('49bb43cf-00b9-4b96-8079-1b4d4f033c28', N'BANK_TRANSFER', N'Chuyển khoản ngân hàng', N'NAPAS', 1);

/* ===========================================================
   SEED Orders & Tickets (with QRCode + QrImageUrl)
   =========================================================== */
DECLARE @Huy UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email='huy.ngo@demo.vn');
DECLARE @Lan UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email='lan.dang@demo.vn');
DECLARE @Tuan UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email='tuan.hoang@demo.vn');
DECLARE @Nhu UNIQUEIDENTIFIER = (SELECT UserId
FROM Users
WHERE Email='tranminhquoc0711@gmail.com');

DECLARE @DefaultQrImage NVARCHAR(500) = 'https://res.cloudinary.com/dnrs0dvt4/image/upload/qrcode-default_afzsls.png';
DECLARE @ExpUnix BIGINT = 1735689600;
-- giả định hết hạn 01/01/2025
DECLARE @Sig NVARCHAR(64) = 'seeded_signature';

-- Order 1: Ngô Quang Huy
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    ('11111111-1111-4111-8111-111111111111', @Huy, 1000000, 'Paid');

-- Ticket 1 (VIP A1)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId, QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    'aaaa1111-bbbb-4ccc-8ddd-eeeeeeee0001',
    '11111111-1111-4111-8111-111111111111',
    @EventHN,
    SeatId,
    500000,
    @Huy,
    CONCAT('EMQR|1|', REPLACE('aaaa1111-bbbb-4ccc-8ddd-eeeeeeee0001','-',''), '|', REPLACE(CONVERT(varchar(36),@EventHN),'-',''), '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage,
    'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId='11111111-1111-4111-8111-111111111111')
FROM Seats
WHERE VenueId=@VenueHN AND RowLabel='A' AND SeatNumber=1;

-- Ticket 2 (VIP A2)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId, QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    'aaaa1111-bbbb-4ccc-8ddd-eeeeeeee0002',
    '11111111-1111-4111-8111-111111111111',
    @EventHN,
    SeatId,
    500000,
    @Huy,
    CONCAT('EMQR|1|', REPLACE('aaaa1111-bbbb-4ccc-8ddd-eeeeeeee0002','-',''), '|', REPLACE(CONVERT(varchar(36),@EventHN),'-',''), '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage,
    'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId='11111111-1111-4111-8111-111111111111')
FROM Seats
WHERE VenueId=@VenueHN AND RowLabel='A' AND SeatNumber=2;


-- Order 2: Đặng Thị Lan
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    ('22222222-2222-4222-8222-222222222222', @Lan, 800000, 'Paid');

-- Ticket 3 (Standard C1)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId, QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    'bbbb1111-cccc-4ddd-8eee-ffffffff0001',
    '22222222-2222-4222-8222-222222222222',
    @EventHCM,
    SeatId,
    500000,
    @Lan,
    CONCAT('EMQR|1|', REPLACE('bbbb1111-cccc-4ddd-8eee-ffffffff0001','-',''), '|', REPLACE(CONVERT(varchar(36),@EventHCM),'-',''), '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage,
    'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId='22222222-2222-4222-8222-222222222222')
FROM Seats
WHERE VenueId=@VenueHCM AND RowLabel='C' AND SeatNumber=1;

-- Ticket 4 (Economy F1)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId, QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    'bbbb1111-cccc-4ddd-8eee-ffffffff0002',
    '22222222-2222-4222-8222-222222222222',
    @EventHCM,
    SeatId,
    300000,
    @Lan,
    CONCAT('EMQR|1|', REPLACE('bbbb1111-cccc-4ddd-8eee-ffffffff0002','-',''), '|', REPLACE(CONVERT(varchar(36),@EventHCM),'-',''), '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage,
    'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId='22222222-2222-4222-8222-222222222222')
FROM Seats
WHERE VenueId=@VenueHCM AND RowLabel='F' AND SeatNumber=1;


-- Order 3: Hoàng Minh Tuấn
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    ('33333333-3333-4333-8333-333333333333', @Tuan, 360000, 'Paid');

-- Ticket 5,6,7 tương tự:
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId, QRCode, QrImageUrl, Status, PurchaseDate)
SELECT t.TicketId, '33333333-3333-4333-8333-333333333333', @EventDN, s.SeatId, t.Price, @Tuan,
    CONCAT('EMQR|1|', REPLACE(t.TicketId,'-',''), '|', REPLACE(CONVERT(varchar(36),@EventDN),'-',''), '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId='33333333-3333-4333-8333-333333333333')
FROM (VALUES
        ('cccc1111-dddd-4eee-8fff-aaaaaaaa0001', 120000, 'H', 1),
        ('cccc1111-dddd-4eee-8fff-aaaaaaaa0002', 120000, 'H', 2),
        ('cccc1111-dddd-4eee-8fff-aaaaaaaa0003', 120000, 'H', 3)
) AS t(TicketId,Price,RowLabel,SeatNumber)
    JOIN Seats s ON s.VenueId=@VenueDN AND s.RowLabel=t.RowLabel AND s.SeatNumber=t.SeatNumber;

/* ===========================================================
   1️⃣ Order 4: nhu.le@demo.vn - Hội thảo Công nghệ 2025
   =========================================================== */
DECLARE @OrderHN UNIQUEIDENTIFIER = '59c8e3b4-7b4a-4d46-985f-0acb5d62f230';
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    (@OrderHN, @Nhu, 950000, 'Paid');

-- VIP Row A3 (500k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHN, @EventHN, SeatId, 500000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHN)
FROM Seats
WHERE VenueId=@VenueHN AND RowLabel='A' AND SeatNumber=3;

-- Standard Row E1 (300k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHN, @EventHN, SeatId, 300000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHN)
FROM Seats
WHERE VenueId=@VenueHN AND RowLabel='E' AND SeatNumber=1;

-- Economy Row H1 (150k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHN, @EventHN, SeatId, 150000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHN)
FROM Seats
WHERE VenueId=@VenueHN AND RowLabel='H' AND SeatNumber=1;


/* ===========================================================
   2️⃣ Order 5: nhu.le@demo.vn - Đêm nhạc Giao Hưởng
   =========================================================== */
DECLARE @OrderHCM UNIQUEIDENTIFIER = 'e37c2daf-9403-4cf2-b8d5-d7a233f03f0b';
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    (@OrderHCM, @Nhu, 1600000, 'Paid');

-- VIP Row A3 (800k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHCM, @EventHCM, SeatId, 800000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHCM),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHCM)
FROM Seats
WHERE VenueId=@VenueHCM AND RowLabel='A' AND SeatNumber=3;

-- Standard Row D1 (500k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHCM, @EventHCM, SeatId, 500000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHCM),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHCM)
FROM Seats
WHERE VenueId=@VenueHCM AND RowLabel='D' AND SeatNumber=1;

-- Economy Row G1 (300k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderHCM, @EventHCM, SeatId, 300000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventHCM),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderHCM)
FROM Seats
WHERE VenueId=@VenueHCM AND RowLabel='G' AND SeatNumber=1;


/* ===========================================================
   3️⃣ Order 6: nhu.le@demo.vn - Hội nghị Du lịch Biển Việt Nam
   =========================================================== */
DECLARE @OrderDN UNIQUEIDENTIFIER = 'a9b7b4f6-2dd1-4468-ae2d-c9a183497ac1';
INSERT INTO Orders
    (OrderId, UserId, TotalAmount, Status)
VALUES
    (@OrderDN, @Nhu, 770000, 'Paid');

-- VIP Row A3 (400k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderDN, @EventDN, SeatId, 400000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventDN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderDN)
FROM Seats
WHERE VenueId=@VenueDN AND RowLabel='A' AND SeatNumber=3;

-- Standard Row D1 (250k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderDN, @EventDN, SeatId, 250000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventDN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderDN)
FROM Seats
WHERE VenueId=@VenueDN AND RowLabel='D' AND SeatNumber=1;

-- Economy Row H1 (120k)
INSERT INTO Tickets
    (TicketId, OrderId, EventId, SeatId, Price, AttendeeId,
    QRCode, QrImageUrl, Status, PurchaseDate)
SELECT
    NEWID(), @OrderDN, @EventDN, SeatId, 120000, @Nhu,
    CONCAT('EMQR|1|', REPLACE(CONVERT(varchar(36), NEWID()),'-',''),
           '|', REPLACE(CONVERT(varchar(36),@EventDN),'-',''),
           '|', @ExpUnix, '|', @Sig),
    @DefaultQrImage, 'Issued',
    (SELECT CreatedAt
    FROM Orders
    WHERE OrderId = @OrderDN)
FROM Seats
WHERE VenueId=@VenueDN AND RowLabel='H' AND SeatNumber=1;

GO

/* ===========================================================
   SEED Payments cho tất cả Orders (dùng PayOS)
   =========================================================== */
-- Lấy PaymentMethodId của PayOS
DECLARE @PayOSId UNIQUEIDENTIFIER = (SELECT PaymentMethodId
FROM PaymentMethods
WHERE MethodName = N'Pay2S');

-- Seed Payments
INSERT INTO Payments
    (PaymentId, OrderId, PaymentMethodId, Amount, Status, TransactionRef, TransactionDate)
SELECT
    NEWID() AS PaymentId,
    o.OrderId,
    @PayOSId,
    o.TotalAmount,
    'Success' AS Status,
    CONCAT('Pay2S-', LEFT(CONVERT(VARCHAR(36), o.OrderId), 8)) AS TransactionRef,
    DATEADD(MINUTE, ROW_NUMBER() OVER (ORDER BY o.CreatedAt), SYSDATETIME()) AS TransactionDate
FROM Orders o;
GO

/* ===========================================================
   UPDATE EventSeatMapping - Đánh dấu ghế đã đặt: IsAvailable = 0
   =========================================================== */
UPDATE esm
SET esm.IsAvailable = 0
FROM EventSeatMapping AS esm
    INNER JOIN Tickets AS t
    ON t.EventId = esm.EventId
        AND t.SeatId  = esm.SeatId;
GO

/* ===========================================================
   SEED Transactions (Inflow từ các Payments thành công)
   =========================================================== */

-- Tính SystemBalance tăng dần theo thời điểm Payment
;WITH OrderedPayments AS (
    SELECT 
        p.PaymentId, 
        p.OrderId,
        o.UserId,
        p.Amount,
        p.Status,
        p.TransactionDate,
        pm.MethodName,
        p.TransactionRef,
        SUM(p.Amount) OVER (ORDER BY p.TransactionDate 
                            ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS RunningBalance
    FROM Payments p
    INNER JOIN Orders o ON o.OrderId = p.OrderId
    INNER JOIN PaymentMethods pm ON p.PaymentMethodId = pm.PaymentMethodId
    WHERE p.Status = 'Success'
)
INSERT INTO Transactions
    (TransactionId, UserId, PaymentId, OrderId, Amount, Direction, Purpose, Status, CreatedAt, SystemBalance, Note)
SELECT
    NEWID() AS TransactionId,
    UserId,
    PaymentId,
    OrderId,
    Amount,
    'In' AS Direction,
    'TicketPayment' AS Purpose,
    Status,
    TransactionDate,
    RunningBalance AS SystemBalance,
    CONCAT(N'Thanh toán vé qua ', MethodName, N' - Mã GD: ', TransactionRef)
FROM OrderedPayments;
GO

/* ===========================================================
   SEED RefundPolicies
   =========================================================== */
-- Chính sách mặc định toàn hệ thống
INSERT INTO RefundPolicies
    (RefundPolicyId, EventId, TicketCategory, IsEnabled, EffectiveFrom, Note)
VALUES
    ('00000000-aaaa-4bbb-8ccc-000000000001', NULL, NULL, 1, SYSDATETIME(),
        N'Chính sách hoàn tiền mặc định: hoàn 80% nếu huỷ sớm, 50% cận ngày, 0% sau khi bắt đầu.');

-- Hội thảo Công nghệ 2025
INSERT INTO RefundPolicies
    (RefundPolicyId, EventId, TicketCategory, IsEnabled, EffectiveFrom, Note)
VALUES
    ('e6ab287a-ff85-4c18-8159-109f49619999', 
        'e6ab287a-ff85-4c18-8159-109f49616636', NULL, 1, SYSDATETIME(),
        N'Chính sách hoàn tiền áp dụng riêng cho Hội thảo Công nghệ 2025.');

-- Đêm nhạc Giao Hưởng
INSERT INTO RefundPolicies
(RefundPolicyId, EventId, TicketCategory, IsEnabled, EffectiveFrom, Note)
VALUES
    ('6cb86389-78f2-4d53-8bdf-36991e3b9999',
     '6cb86389-78f2-4d53-8bdf-36991e3b1321', NULL, 1, SYSDATETIME(),
     N'Chính sách hoàn tiền riêng cho Đêm nhạc Giao Hưởng: vì vé cao cấp nên quy định chặt hơn.');
GO

-- Hội nghị Du lịch Biển Việt Nam
INSERT INTO RefundPolicies
    (RefundPolicyId, EventId, TicketCategory, IsEnabled, EffectiveFrom, Note)
VALUES
    ('bafb2b62-89da-4c79-af42-373ebad79999',
        'bafb2b62-89da-4c79-af42-373ebad7f993', NULL, 1, SYSDATETIME(),
        N'Chính sách hoàn tiền linh hoạt cho Hội nghị Du lịch Biển Việt Nam.');
GO


/* ===========================================================
   SEED RefundPolicyRules
   =========================================================== */
-- Chính sách mặc định (hoàn càng sớm càng nhiều)
INSERT INTO RefundPolicyRules
    (RefundPolicyRuleId, RefundPolicyId, CutoffMinutesBeforeStart, RefundPercent, FlatFee, RuleOrder)
VALUES
    (NEWID(), '00000000-aaaa-4bbb-8ccc-000000000001', 14400, 80, 0, 1), -- 10 ngày trước: hoàn 80%
    (NEWID(), '00000000-aaaa-4bbb-8ccc-000000000001', 2880, 50, 0, 2),  -- 2 ngày trước: hoàn 50%
    (NEWID(), '00000000-aaaa-4bbb-8ccc-000000000001', 0, 0, 0, 3);       -- Sau khi bắt đầu: 0%

-- Hội thảo Công nghệ 2025 (chặt chẽ hơn chút)
INSERT INTO RefundPolicyRules
    (RefundPolicyRuleId, RefundPolicyId, CutoffMinutesBeforeStart, RefundPercent, FlatFee, RuleOrder)
VALUES
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49619999', 10080, 70, 10000, 1), -- 7 ngày trước: hoàn 70% trừ 10k phí
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49619999', 2880, 40, 10000, 2),  -- 2 ngày trước: hoàn 40%
    (NEWID(), 'e6ab287a-ff85-4c18-8159-109f49619999', 0, 0, 0, 3);          -- Gần giờ: không hoàn

-- Đêm nhạc Giao Hưởng (vé biểu diễn: rất hạn chế)
INSERT INTO RefundPolicyRules
    (RefundPolicyRuleId, RefundPolicyId, CutoffMinutesBeforeStart, RefundPercent, FlatFee, RuleOrder)
VALUES
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b9999', 4320, 30, 0, 1),  -- 3 ngày trước: hoàn 30%
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b9999', 1440, 10, 0, 2),  -- 1 ngày trước: hoàn 10%
    (NEWID(), '6cb86389-78f2-4d53-8bdf-36991e3b9999', 0, 0, 0, 3);      -- Sau: 0%

-- Hội nghị Du lịch Biển Việt Nam (linh hoạt hơn)
INSERT INTO RefundPolicyRules
    (RefundPolicyRuleId, RefundPolicyId, CutoffMinutesBeforeStart, RefundPercent, FlatFee, RuleOrder)
VALUES
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad79999', 20160, 90, 0, 1), -- 14 ngày trước: hoàn 90%
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad79999', 4320, 60, 0, 2),  -- 3 ngày trước: hoàn 60%
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad79999', 1440, 20, 0, 3),  -- 1 ngày trước: hoàn 20%
    (NEWID(), 'bafb2b62-89da-4c79-af42-373ebad79999', 0, 0, 0, 4);      -- Sau: 0%
GO


-- /* ===========================================================
--    --------------------QUERY TESTING--------------------------
--    =========================================================== */
SELECT s.SeatId, s.RowLabel, s.SeatNumber, e.EventId, e.EventName, esm.IsAvailable
FROM Seats s
    JOIN EventSeatMapping esm ON s.SeatId = esm.SeatId
    JOIN Events e ON esm.EventId = e.EventId
WHERE e.EventName = N'Hội thảo Công nghệ 2025' AND esm.IsAvailable = 1