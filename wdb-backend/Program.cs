using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Notification;
using wdb_backend.Services;
using wdb_backend.Usecases;

var builder = WebApplication.CreateBuilder(args);

// ============================
// services
// ============================

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Core services
builder.Services.AddScoped<IWorkerService, WorkerServiceImpl>();
builder.Services.AddScoped<IRequestService, RequestServiceImpl>();
builder.Services.AddScoped<IPermissionService, PermissionServiceImpl>();
builder.Services.AddScoped<IEmployerService, EmployerServiceImpl>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();

builder.Services.AddScoped<IEmployerSentRequestService, EmployerSentRequestServiceImpl>();
builder.Services.AddScoped<IActiveAccessService, ActiveAccessServiceImpl>();
builder.Services.AddScoped<IEmployerActiveAccessService, EmployerActiveAccessServiceImpl>();
builder.Services.AddScoped<IWorkerAuditLogService, WorkerAuditLogServiceImpl>();

// Use cases
builder.Services.AddScoped<ICreateDataAccessRequestUsecase, CreateDataAccessRequestUsecaseImpl>();
builder.Services.AddScoped<IFindWorkerInfosByEmailUsecase, FindWorkerInfosByEmailUsecaseImpl>();
builder.Services.AddScoped<IAddFlexibleWorkerInfoUsecase, AddFlexibleWorkerInfoUsecaseImpl>();


// Repositories
builder.Services.AddScoped<IWorkerRepository, WorkerRepoImpl>();
builder.Services.AddScoped<IRequestRepository, RequestRepoImpl>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepoImpl>();
builder.Services.AddScoped<IWorkerInfoRepository, WorkerInfoRepoImpl>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepoImpl>();

// Dashboard services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();
builder.Services.AddScoped<IEmployerDashboardService, EmployerDashboardServiceImpl>();

// Blockchain service
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();

// DbContext
builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers with JSON enum converter
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// register SignalR
builder.Services.AddSignalR();

// register MediatR (scan current app and find out all Handlers)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<INotificationRepository, NotificationRepoImpl>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();

var app = builder.Build();

// ============================
// middleware - order matters
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");  // map notification hub
app.MapOpenApi();

app.Run();
