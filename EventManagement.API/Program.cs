using AutoMapper;
using EventManagement.Application.Helpers;
using System.Text;
using EventManagement.API.Middleware;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
// service usings are above
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Services;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.Persistence.Repositories;
using EventManagement.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EventManagement.Application.Services.BackgroundService;
using Microsoft.AspNetCore.SignalR;
using EventManagement.API.Hubs;
using EventManagement.API.Realtime;

var builder = WebApplication.CreateBuilder(args);


// Lấy connectionString từ appsettings.json của API
var connectionString = builder.Configuration.GetConnectionString("EventManagementDB");

// Kết nối DBContext với SQL Server
builder.Services.AddDbContext<EventManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories + UnitOfWork
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICheckinRepository, CheckinRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IEventImageRepository, EventImageRepository>();
builder.Services.AddScoped<IEventSeatMappingRepository, EventSeatMappingRepository>();
builder.Services.AddScoped<IEventStaffRepository, EventStaffRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrganizerRequestRepository, OrganizerRequestRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IRefundRequestRepository, RefundRequestRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserBankAccountRepository, UserBankAccountRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
builder.Services.AddScoped<ISystemLogRepository, SystemLogRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IVenueImageRepository, VenueImageRepository>();
builder.Services.AddScoped<ISeatHoldRepository, SeatHoldRepository>();

// Services
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IRefundService, RefundService>();

// Hosted Services
builder.Services.AddHostedService<CleanupService>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<ISeatRealtimeService, SeatRealtimeHubService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Swagger + JWT
// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret-Key"] ?? string.Empty))
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "EventManagement API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();

// CORS for FE app (Blazor) on localhost:5000
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy => policy
            .WithOrigins("https://localhost:5000", "http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});


var app = builder.Build();

// Add Middleware
app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS before auth
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Hubs
app.MapHub<SeatHub>("/hubs/seats");


app.Run();
