
using Microsoft.EntityFrameworkCore;
using VideoProcessingApi.Data;
using VideoProcessingApi.Services;
using VideoProcessingApi.Interfaces;
using VideoProcessingApi.BackgroundServices;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Middleware;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using Minio;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.Configure<FFmpegSettings>(builder.Configuration.GetSection("FFmpeg"));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

var storageSettings = builder.Configuration.GetSection("Storage").Get<StorageSettings>() ?? new StorageSettings();

if (storageSettings.Provider.Equals("MinIO", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IMinioClient>(provider =>
    {
        var settings = provider.GetRequiredService<IOptions<StorageSettings>>().Value.MinIO;
        return new MinioClient()
            .WithEndpoint(settings.Endpoint)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(settings.UseSSL)
            .Build();
    });
    builder.Services.AddScoped<IStorageService, MinIOStorageService>();
}
else
{
    builder.Services.AddScoped<IStorageService, FileSystemStorageService>();
}

builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("APIDBConnection")));

builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileValidator, VideoProcessingApi.Validators.FileValidator>();
builder.Services.AddScoped<IFFmpegErrorHandlerService, FFmpegErrorHandlerService>();
builder.Services.AddScoped<IFFmpegService, FFmpegService>();
builder.Services.AddScoped<IEnvironmentValidationService, EnvironmentValidationService>();
builder.Services.AddScoped<FFmpegHealthCheck>();

builder.Services.AddHostedService<VideoProcessingBackgroundService>();
builder.Services.AddHostedService<CleanupBackgroundService>();
builder.Services.AddHostedService<DatabaseInitializationService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Video Processing API", 
        Version = "v1",
        Description = "Asynchronous video processing API using FFmpeg. Supports merge, convert, compress, trim, extract audio and watermarking."
    });
    
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });

    try
    {
        var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch
    {
        // ignore if xml cannot be loaded
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        var corsSettings = builder.Configuration.GetSection("Security:Cors").Get<CorsSettings>();
        policy.WithOrigins(corsSettings?.AllowedOrigins ?? new[] { "*" })
              .WithMethods(corsSettings?.AllowedMethods ?? new[] { "GET", "POST", "PUT", "DELETE" })
              .WithHeaders(corsSettings?.AllowedHeaders ?? new[] { "*" });
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<JobDbContext>()
    .AddCheck<FFmpegHealthCheck>("ffmpeg");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    context.Database.EnsureCreated();
}

Log.Information("Environment is Development - enabling Swagger UI");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Processing API V1");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "Video Processing API Documentation";
});

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

var useHttpsRedirection = builder.Configuration.GetValue<bool>("UseHttpsRedirection", false);
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseCors("DefaultPolicy");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting app Video Processing API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "App start failed unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}