using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using HDScheduler.API.Data;
using HDScheduler.API.Repositories;
using HDScheduler.API.Services;
using HDScheduler.API.Services.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON properties to match JavaScript/Angular conventions
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "HD Scheduler API", 
        Version = "v1",
        Description = "Hemodialysis Scheduler System API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://lively-pond-08e4f7c00.3.azurestaticapps.net",
                "https://dev.dialyzeflow.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register Dapper Context
builder.Services.AddSingleton<DapperContext>();

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IHDScheduleRepository, HDScheduleRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IHDLogRepository, HDLogRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<EquipmentUsageService>();
builder.Services.AddScoped<IRecurringSessionService, RecurringSessionService>();
builder.Services.AddScoped<IBedAssignmentService, BedAssignmentService>();
builder.Services.AddScoped<IHDCycleService, HDCycleService>();

// Register AI services
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>();
builder.Services.AddScoped<IAIRepository, AIRepository>();
builder.Services.AddScoped<IAIService, AIService>();

// Register background services
builder.Services.AddHostedService<SessionHistoryBackgroundService>();
builder.Services.AddHostedService<SessionCompletionService>(); // Auto-marks sessions as Ready-For-Discharge

var app = builder.Build();

// Initialize SQL Server database (commented out - use migration scripts instead)
// For production: Run Database/SqlServer/01_CreateSchema.sql and 02_SeedData.sql manually
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString != null)
{
    // Temporarily disabled - Use SQL Server migration scripts
    // DatabaseInitializer.Initialize(connectionString);
    
    // Apply migration to add HD treatment fields to Patients table
    // try
    // {
    //     PatientFieldsMigration.ApplyMigration(connectionString);
    // }
    // catch (Exception ex)
    // {
    //     Console.WriteLine($"⚠️  Migration already applied or error: {ex.Message}");
    // }
    
    Console.WriteLine("📋 Database initialization skipped. Run SQL Server migration scripts manually.");
    Console.WriteLine("   1. Database/SqlServer/01_CreateSchema.sql");
    Console.WriteLine("   2. Database/SqlServer/02_SeedData.sql");
}

// Configure the HTTP request pipeline
// Enable Swagger in all environments for development
app.UseSwagger();
app.UseSwaggerUI();

// Disable HTTPS redirection for development
// app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();

// Add a root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();
