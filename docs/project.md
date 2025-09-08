# Spec Kit: API de Processamento de Vídeo com .NET 9 e FFmpeg

## 1. 🎯 Visão do Projeto

*   **Nome do Projeto:** API de Processamento de Vídeo Assíncrono
*   **Versão:** 1.0.0
*   **Objetivo:** Criar um serviço de back-end robusto, escalável e seguro que aceite o upload de múltiplos vídeos, processe-os em uma fila assíncrona usando FFmpeg, e permita ao usuário consultar o status e baixar o resultado.
*   **Público-alvo:** Desenvolvedores que precisam integrar funcionalidades de manipulação de vídeo em suas aplicações sem lidar diretamente com a complexidade do FFmpeg.
*   **Tecnologias Principais:** .NET 9, ASP.NET Core Web API, Entity Framework Core, SQLite, FFmpeg, Docker
*   **Arquitetura:** Monolítica com padrão Background Services para processamento assíncrono

## 2. 📋 Requisitos

### Requisitos Funcionais (RF)

*   **RF01:** A API deve permitir o upload de um ou mais arquivos de vídeo (formatos: MP4, AVI, MOV, MKV) com validação de tipo MIME e tamanho máximo.
*   **RF02:** A API deve ter um endpoint para iniciar um novo "job" de processamento, retornando imediatamente um `job_id` UUID.
*   **RF03:** A API deve prover um endpoint para consultar o status de um job (`PENDING`, `PROCESSING`, `COMPLETED`, `FAILED`) usando o `job_id`.
*   **RF04:** Após a conclusão bem-sucedida de um job, a API deve disponibilizar um link seguro e temporário para o download do vídeo processado.
*   **RF05:** A API deve suportar diferentes tipos de processamento: merge de vídeos, conversão de formato, compressão e corte de vídeo.
*   **RF06:** A API deve implementar autenticação via API Key para controle de acesso.
*   **RF07:** A API deve registrar logs detalhados de todas as operações para auditoria e debugging.
*   **RF08:** A API deve permitir o cancelamento de jobs em processamento.

### Requisitos Não Funcionais (RNF)

*   **RNF01:** A solução completa (API + Banco de Dados) deve ser executável com um único comando `docker-compose up`.
*   **RNF02:** O banco de dados SQLite deve ser usado para persistir o estado dos jobs. O arquivo do banco deve ser salvo em um volume Docker para persistência.
*   **RNF03:** O projeto deve incluir scripts de validação (`.sh` para Linux, `.ps1` para Windows) para verificar a instalação do FFmpeg no ambiente.
*   **RNF04:** A API deve ser desenvolvida utilizando .NET 9, seguindo as melhores práticas de desenvolvimento da plataforma.
*   **RNF05:** A API deve suportar processamento simultâneo de até 5 jobs para otimizar uso de recursos.
*   **RNF06:** O sistema deve ser resiliente a falhas, com retry automático para jobs falhados.
*   **RNF07:** A API deve ter documentação OpenAPI/Swagger integrada.
*   **RNF08:** Arquivos temporários devem ser limpos automaticamente após processamento.
*   **RNF09:** O sistema deve implementar rate limiting para prevenir abuso.
*   **RNF10:** Logs estruturados devem ser implementados usando Serilog.

### Requisitos de Segurança (RS)

*   **RS01:** Validação rigorosa de tipos de arquivo para prevenir upload de arquivos maliciosos.
*   **RS02:** Sanitização de nomes de arquivos para prevenir path traversal attacks.
*   **RS03:** Implementação de CORS configurável para controle de origem das requisições.
*   **RS04:** Rate limiting por IP e por API Key.
*   **RS05:** Headers de segurança implementados (HSTS, X-Content-Type-Options, etc.).

## 3. 🗺️ Épicos & Histórias de Usuário

### EP01: Configuração do Ambiente e MVP da Infraestrutura

*   **US01:** Como desenvolvedor, quero um arquivo `docker-compose.yml` para subir a API e o banco de dados de forma simples e rápida, garantindo um ambiente de desenvolvimento consistente.
*   **US02:** Como desenvolvedor, quero ter scripts que verifiquem se o FFmpeg está instalado e acessível no PATH, para diagnosticar problemas de ambiente rapidamente.
*   **US03:** Como desenvolvedor, quero uma configuração de logging estruturado para facilitar o debugging e monitoramento da aplicação.

### EP02: Gestão de Jobs de Processamento de Vídeo

*   **US04:** Como usuário da API, quero enviar uma requisição para um endpoint com meus vídeos e receber um `job_id`, para que eu possa rastrear o processo de forma assíncrona.
*   **US05:** Como usuário da API, quero usar o `job_id` para consultar um endpoint de status e saber em que etapa meu vídeo está.
*   **US06:** Como usuário da API, quando o status do meu job for "COMPLETED", quero receber um link para fazer o download do vídeo final.
*   **US07:** Como desenvolvedor, quero que todos os detalhes do job (ID, status, arquivos de entrada/saída, timestamps) sejam salvos em uma tabela no SQLite para auditoria e controle.
*   **US08:** Como usuário da API, quero poder cancelar um job que ainda não foi processado.

### EP03: Segurança e Validação

*   **US09:** Como administrador da API, quero implementar autenticação via API Key para controlar o acesso aos endpoints.
*   **US10:** Como desenvolvedor, quero validar os tipos de arquivo enviados para garantir que apenas vídeos válidos sejam processados.
*   **US11:** Como administrador da API, quero implementar rate limiting para prevenir abuso do serviço.

### EP04: Processamento Avançado de Vídeo

*   **US12:** Como usuário da API, quero poder especificar diferentes tipos de processamento além do merge (conversão, compressão, corte).
*   **US13:** Como usuário da API, quero poder especificar parâmetros de qualidade para a conversão de vídeo.
*   **US14:** Como desenvolvedor, quero implementar processamento paralelo para otimizar o throughput da aplicação.

## 4. 💻 Arquitetura e Estrutura Técnica

### 4.1 Arquitetura da Aplicação

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Applications                     │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTP/REST API
┌─────────────────────▼───────────────────────────────────────┐
│                  ASP.NET Core Web API                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Controllers │  │ Middleware  │  │   Authentication    │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Application Services                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ Job Service │  │File Service │  │   FFmpeg Service    │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Background Services (Queue)                   │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │         Video Processing Background Service           │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Data Layer (EF Core)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Entities   │  │ DbContext   │  │    Repositories     │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   SQLite Database                          │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Estrutura de Pastas Sugerida

```
src/
├── VideoProcessingApi/
│   ├── Controllers/
│   │   ├── JobsController.cs
│   │   ├── FilesController.cs
│   │   └── HealthController.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IJobService.cs
│   │   │   ├── IFileService.cs
│   │   │   └── IFFmpegService.cs
│   │   ├── JobService.cs
│   │   ├── FileService.cs
│   │   └── FFmpegService.cs
│   ├── BackgroundServices/
│   │   └── VideoProcessingBackgroundService.cs
│   ├── Data/
│   │   ├── Entities/
│   │   │   ├── VideoJob.cs
│   │   │   └── ProcessingOperation.cs
│   │   ├── JobDbContext.cs
│   │   └── Migrations/
│   ├── Models/
│   │   ├── DTOs/
│   │   │   ├── CreateJobRequest.cs
│   │   │   ├── JobStatusResponse.cs
│   │   │   └── ProcessingOptions.cs
│   │   └── Enums/
│   │       ├── JobStatus.cs
│   │       └── ProcessingType.cs
│   ├── Middleware/
│   │   ├── ApiKeyMiddleware.cs
│   │   ├── RateLimitingMiddleware.cs
│   │   └── ErrorHandlingMiddleware.cs
│   ├── Configuration/
│   │   ├── ApiSettings.cs
│   │   ├── FFmpegSettings.cs
│   │   └── SecuritySettings.cs
│   ├── Validators/
│   │   ├── FileValidator.cs
│   │   └── JobRequestValidator.cs
│   └── Program.cs
├── VideoProcessingApi.Tests/
│   ├── Unit/
│   ├── Integration/
│   └── E2E/
└── scripts/
    ├── validate_ffmpeg.sh
    ├── validate_ffmpeg.ps1
    └── setup_environment.sh
```

### 4.3 Trechos de Código Principais

#### `docker-compose.yml` Melhorado

```yaml
version: '3.8'

services:
  video-processing-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    volumes:
      - ./uploads:/app/uploads
      - ./processed:/app/processed
      - ./db:/app/db
      - ./logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__APIDBConnection=Data Source=/app/db/jobs.db
      - FFmpeg__BinaryPath=/usr/bin/ffmpeg
      - Security__ApiKeys__0=your-api-key-here
      - Logging__LogLevel__Default=Information
      - RateLimit__MaxRequestsPerMinute=30
    depends_on:
      - ffmpeg-service
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  ffmpeg-service:
    image: jrottenberg/ffmpeg:4.4-alpine
    command: ["sleep", "infinity"]
    volumes:
      - ./uploads:/uploads
      - ./processed:/processed
```

#### Modelo de Dados Expandido com Entity Framework Core (C#)

```csharp
// Entidades principais
public class VideoJob
{
    public Guid Id { get; set; } // job_id
    public JobStatus Status { get; set; }
    public ProcessingType ProcessingType { get; set; }
    public List<string> InputFilePaths { get; set; } = new();
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public ProcessingOptions? Options { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? CreatedBy { get; set; } // API Key identifier
    public long? OutputFileSizeBytes { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
    public int RetryCount { get; set; } = 0;
    public bool IsCanceled { get; set; } = false;
    
    // Navigation properties
    public List<ProcessingOperation> Operations { get; set; } = new();
}

public class ProcessingOperation
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorDetails { get; set; }
    
    // Navigation properties
    public VideoJob Job { get; set; } = null!;
}

public enum JobStatus 
{ 
    Pending, 
    Processing, 
    Completed, 
    Failed, 
    Canceled 
}

public enum ProcessingType
{
    Merge,
    Convert,
    Compress,
    Trim,
    ExtractAudio,
    AddWatermark
}

public class ProcessingOptions
{
    public string? OutputFormat { get; set; }
    public string? Quality { get; set; }
    public int? BitrateKbps { get; set; }
    public string? Resolution { get; set; }
    public double? StartTime { get; set; }
    public double? EndTime { get; set; }
    public string? WatermarkText { get; set; }
    public string? WatermarkImagePath { get; set; }
}

// Contexto do EF Core expandido
public class JobDbContext : DbContext
{
    public DbSet<VideoJob> Jobs { get; set; }
    public DbSet<ProcessingOperation> Operations { get; set; }
    
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração da entidade VideoJob
        modelBuilder.Entity<VideoJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InputFilePaths)
                  .HasConversion(
                      v => string.Join(';', v),
                      v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Options)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v),
                      v => JsonSerializer.Deserialize<ProcessingOptions>(v));
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // Configuração da entidade ProcessingOperation
        modelBuilder.Entity<ProcessingOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Job)
                  .WithMany(e => e.Operations)
                  .HasForeignKey(e => e.JobId);
        });
    }
}
```

#### DTOs e Modelos de Request/Response

```csharp
// DTOs para API
public class CreateJobRequest
{
    public ProcessingType ProcessingType { get; set; }
    public ProcessingOptions? Options { get; set; }
    public List<IFormFile> Files { get; set; } = new();
}

public class JobStatusResponse
{
    public Guid JobId { get; set; }
    public JobStatus Status { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
    public long? OutputFileSizeBytes { get; set; }
    public List<ProcessingOperationDto> Operations { get; set; } = new();
}

public class ProcessingOperationDto
{
    public string OperationType { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorDetails { get; set; }
}

// Configurações da aplicação
public class ApiSettings
{
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; set; }
    public int MaxConcurrentJobs { get; set; }
    public string UploadsPath { get; set; } = string.Empty;
    public string ProcessedPath { get; set; } = string.Empty;
    public TimeSpan FileRetentionPeriod { get; set; }
}

public class FFmpegSettings
{
    public string BinaryPath { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; }
    public string DefaultQuality { get; set; } = string.Empty;
    public Dictionary<string, string> QualityPresets { get; set; } = new();
}

public class SecuritySettings
{
    public string[] ApiKeys { get; set; } = Array.Empty<string>();
    public RateLimitSettings RateLimit { get; set; } = new();
    public CorsSettings Cors { get; set; } = new();
}

public class RateLimitSettings
{
    public int MaxRequestsPerMinute { get; set; }
    public int MaxRequestsPerHour { get; set; }
    public int MaxRequestsPerDay { get; set; }
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
}
```

#### Serviços Principais

```csharp
// Interface do serviço de jobs
public interface IJobService
{
    Task<Guid> CreateJobAsync(CreateJobRequest request, string apiKey);
    Task<JobStatusResponse?> GetJobStatusAsync(Guid jobId);
    Task<bool> CancelJobAsync(Guid jobId);
    Task<Stream?> GetJobOutputAsync(Guid jobId);
    Task CleanupExpiredJobsAsync();
}

// Interface do serviço FFmpeg
public interface IFFmpegService
{
    Task<string> MergeVideosAsync(List<string> inputPaths, string outputPath, ProcessingOptions? options = null);
    Task<string> ConvertVideoAsync(string inputPath, string outputPath, ProcessingOptions options);
    Task<string> CompressVideoAsync(string inputPath, string outputPath, ProcessingOptions options);
    Task<string> TrimVideoAsync(string inputPath, string outputPath, double startTime, double endTime);
    Task<string> ExtractAudioAsync(string inputPath, string outputPath, ProcessingOptions? options = null);
    Task<bool> ValidateVideoFileAsync(string filePath);
}

// Implementação do serviço FFmpeg
public class FFmpegService : IFFmpegService
{
    private readonly FFmpegSettings _settings;
    private readonly ILogger<FFmpegService> _logger;

    public FFmpegService(IOptions<FFmpegSettings> settings, ILogger<FFmpegService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> MergeVideosAsync(List<string> inputPaths, string outputPath, ProcessingOptions? options = null)
    {
        var quality = options?.Quality ?? _settings.DefaultQuality;
        var inputList = string.Join(" ", inputPaths.Select((path, index) => $"-i \"{path}\""));
        var filterComplex = string.Join("", inputPaths.Select((_, index) => $"[{index}:v][{index}:a]"));
        filterComplex += $"concat=n={inputPaths.Count}:v=1:a=1[outv][outa]";

        var arguments = $"{inputList} -filter_complex \"{filterComplex}\" -map \"[outv]\" -map \"[outa]\" -c:v libx264 -preset {quality} -c:a aac \"{outputPath}\"";

        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<string> ConvertVideoAsync(string inputPath, string outputPath, ProcessingOptions options)
    {
        var arguments = $"-i \"{inputPath}\"";
        
        if (!string.IsNullOrEmpty(options.Resolution))
        {
            arguments += $" -vf scale={options.Resolution}";
        }
        
        if (options.BitrateKbps.HasValue)
        {
            arguments += $" -b:v {options.BitrateKbps}k";
        }
        
        arguments += $" -c:v libx264 -preset {options.Quality ?? _settings.DefaultQuality} \"{outputPath}\"";
        
        return await ExecuteFFmpegAsync(arguments);
    }

    private async Task<string> ExecuteFFmpegAsync(string arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _settings.BinaryPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null) outputBuilder.AppendLine(args.Data);
        };
        
        process.ErrorDataReceived += (sender, args) => {
            if (args.Data != null) errorBuilder.AppendLine(args.Data);
        };

        _logger.LogInformation("Executing FFmpeg with arguments: {Arguments}", arguments);
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_settings.TimeoutMinutes));
        await process.WaitForExitAsync(cancellationTokenSource.Token);

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (process.ExitCode != 0)
        {
            _logger.LogError("FFmpeg execution failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
            throw new InvalidOperationException($"FFmpeg execution failed: {error}");
        }

        _logger.LogInformation("FFmpeg execution completed successfully");
        return output;
    }

    public async Task<bool> ValidateVideoFileAsync(string filePath)
    {
        try
        {
            var arguments = $"-v error -select_streams v:0 -show_entries stream=codec_name -of csv=p=0 \"{filePath}\"";
            var result = await ExecuteFFmpegAsync(arguments);
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }
}
```

#### Background Service para Processamento

```csharp
public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public VideoProcessingBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<VideoProcessingBackgroundService> logger,
        IOptions<ApiSettings> apiSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _semaphore = new SemaphoreSlim(apiSettings.Value.MaxConcurrentJobs, apiSettings.Value.MaxConcurrentJobs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background processing");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
        var ffmpegService = scope.ServiceProvider.GetRequiredService<IFFmpegService>();

        var pendingJobs = await dbContext.Jobs
            .Where(j => j.Status == JobStatus.Pending && !j.IsCanceled)
            .OrderBy(j => j.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var tasks = pendingJobs.Select(job => ProcessJobAsync(job, ffmpegService, dbContext, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessJobAsync(VideoJob job, IFFmpegService ffmpegService, JobDbContext dbContext, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Starting processing for job {JobId}", job.Id);
            
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            var outputPath = Path.Combine("processed", $"{job.Id}.mp4");
            
            switch (job.ProcessingType)
            {
                case ProcessingType.Merge:
                    await ffmpegService.MergeVideosAsync(job.InputFilePaths, outputPath, job.Options);
                    break;
                case ProcessingType.Convert:
                    await ffmpegService.ConvertVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!);
                    break;
                case ProcessingType.Compress:
                    await ffmpegService.CompressVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!);
                    break;
                case ProcessingType.Trim:
                    await ffmpegService.TrimVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!.StartTime!.Value, job.Options.EndTime!.Value);
                    break;
            }

            job.Status = JobStatus.Completed;
            job.OutputFilePath = outputPath;
            job.FinishedAt = DateTime.UtcNow;
            job.ProcessingDuration = job.FinishedAt - job.StartedAt;
            
            if (File.Exists(outputPath))
            {
                job.OutputFileSizeBytes = new FileInfo(outputPath).Length;
            }

            _logger.LogInformation("Completed processing for job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process job {JobId}", job.Id);
            
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
            job.RetryCount++;
        }
        finally
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _semaphore.Release();
        }
    }
}
```


## 5. � Scripts de Configuração e Validação

### Script de Validação do FFmpeg (Linux/macOS)

```bash
#!/bin/bash
# validate_ffmpeg.sh

echo "🔍 Validando instalação do FFmpeg..."

if ! command -v ffmpeg &> /dev/null; then
    echo "❌ FFmpeg não encontrado no PATH."
    echo "📋 Instruções de instalação:"
    echo "   Ubuntu/Debian: sudo apt update && sudo apt install ffmpeg"
    echo "   CentOS/RHEL:   sudo yum install epel-release && sudo yum install ffmpeg"
    echo "   macOS:         brew install ffmpeg"
    exit 1
fi

echo "✅ FFmpeg encontrado:"
ffmpeg -version | head -n 1

echo ""
echo "🎥 Testando codecs essenciais..."

# Verificar codecs necessários
REQUIRED_CODECS=("libx264" "aac" "libmp3lame")
for codec in "${REQUIRED_CODECS[@]}"; do
    if ffmpeg -codecs 2>/dev/null | grep -q "$codec"; then
        echo "✅ Codec $codec disponível"
    else
        echo "❌ Codec $codec não encontrado"
        exit 1
    fi
done

echo ""
echo "📊 Verificando formatos suportados..."
REQUIRED_FORMATS=("mp4" "avi" "mov" "mkv")
for format in "${REQUIRED_FORMATS[@]}"; do
    if ffmpeg -formats 2>/dev/null | grep -q "$format"; then
        echo "✅ Formato $format suportado"
    else
        echo "⚠️  Formato $format pode não estar totalmente suportado"
    fi
done

echo ""
echo "🚀 FFmpeg está pronto para uso!"
```

### Script de Validação do FFmpeg (Windows PowerShell)

```powershell
# validate_ffmpeg.ps1

Write-Host "🔍 Validando instalação do FFmpeg..." -ForegroundColor Cyan

try {
    $ffmpegVersion = & ffmpeg -version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ FFmpeg encontrado:" -ForegroundColor Green
        Write-Host ($ffmpegVersion | Select-Object -First 1) -ForegroundColor White
    }
    else {
        throw "FFmpeg não encontrado"
    }
}
catch {
    Write-Host "❌ FFmpeg não encontrado no PATH." -ForegroundColor Red
    Write-Host "📋 Instruções de instalação:" -ForegroundColor Yellow
    Write-Host "   1. Baixe FFmpeg de: https://ffmpeg.org/download.html#build-windows" -ForegroundColor White
    Write-Host "   2. Extraia para C:\ffmpeg" -ForegroundColor White
    Write-Host "   3. Adicione C:\ffmpeg\bin ao PATH do sistema" -ForegroundColor White
    Write-Host "   Ou use Chocolatey: choco install ffmpeg" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "🎥 Testando codecs essenciais..." -ForegroundColor Cyan

$requiredCodecs = @("libx264", "aac", "libmp3lame")
foreach ($codec in $requiredCodecs) {
    $codecCheck = & ffmpeg -codecs 2>$null | Select-String $codec
    if ($codecCheck) {
        Write-Host "✅ Codec $codec disponível" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Codec $codec não encontrado" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "📊 Verificando formatos suportados..." -ForegroundColor Cyan
$requiredFormats = @("mp4", "avi", "mov", "mkv")
foreach ($format in $requiredFormats) {
    $formatCheck = & ffmpeg -formats 2>$null | Select-String $format
    if ($formatCheck) {
        Write-Host "✅ Formato $format suportado" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Formato $format pode não estar totalmente suportado" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "🚀 FFmpeg está pronto para uso!" -ForegroundColor Green
```

### Script de Setup do Ambiente de Desenvolvimento

```bash
#!/bin/bash
# setup_environment.sh

echo "🏗️  Configurando ambiente de desenvolvimento da API de Processamento de Vídeo..."

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não encontrado. Por favor, instale o Docker primeiro."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose não encontrado. Por favor, instale o Docker Compose primeiro."
    exit 1
fi

echo "✅ Docker e Docker Compose encontrados"

# Criar diretórios necessários
echo "📁 Criando estrutura de diretórios..."
mkdir -p uploads processed db logs scripts tests

# Definir permissões corretas
chmod 755 uploads processed db logs
chmod +x scripts/*.sh

echo "✅ Estrutura de diretórios criada"

# Validar FFmpeg
echo "🔍 Validando FFmpeg..."
if [ -f "./scripts/validate_ffmpeg.sh" ]; then
    bash ./scripts/validate_ffmpeg.sh
else
    echo "⚠️  Script de validação do FFmpeg não encontrado"
fi

# Verificar .NET SDK
echo "🔍 Verificando .NET SDK..."
if command -v dotnet &> /dev/null; then
    dotnet_version=$(dotnet --version)
    echo "✅ .NET SDK encontrado: $dotnet_version"
    
    # Verificar se é .NET 9 ou superior
    major_version=$(echo $dotnet_version | cut -d. -f1)
    if [ "$major_version" -ge 9 ]; then
        echo "✅ Versão do .NET compatível"
    else
        echo "⚠️  Recomenda-se .NET 9 ou superior"
    fi
else
    echo "❌ .NET SDK não encontrado"
    echo "📋 Baixe em: https://dotnet.microsoft.com/download"
fi

# Criar arquivo .env de exemplo
if [ ! -f ".env" ]; then
    echo "📝 Criando arquivo .env de exemplo..."
    cat > .env << EOF
# API Configuration
ASPNETCORE_ENVIRONMENT=Development
API_MAX_FILE_SIZE_MB=500
API_MAX_CONCURRENT_JOBS=3

# Database
CONNECTION_STRING=Data Source=/app/db/jobs.db

# FFmpeg
FFMPEG_BINARY_PATH=/usr/bin/ffmpeg
FFMPEG_TIMEOUT_MINUTES=30

# Security
API_KEYS=dev-key-123,another-key-456
RATE_LIMIT_MAX_REQUESTS_PER_MINUTE=30

# CORS
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:8080

# Logging
LOG_LEVEL=Information
EOF
    echo "✅ Arquivo .env criado"
else
    echo "ℹ️  Arquivo .env já existe"
fi

echo ""
echo "🎉 Ambiente configurado com sucesso!"
echo "📋 Próximos passos:"
echo "   1. Ajuste as configurações no arquivo .env se necessário"
echo "   2. Execute: docker-compose up --build"
echo "   3. Acesse a API em: http://localhost:8080"
echo "   4. Documentação Swagger em: http://localhost:8080/swagger"
```

## 6. 🏗️ Configuração do Projeto (.NET 9)

### Program.cs Completo

```csharp
using Microsoft.EntityFrameworkCore;
using VideoProcessingApi.Data;
using VideoProcessingApi.Services;
using VideoProcessingApi.Services.Interfaces;
using VideoProcessingApi.BackgroundServices;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Middleware;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurações
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.Configure<FFmpegSettings>(builder.Configuration.GetSection("FFmpeg"));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));

// Entity Framework
builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("APIDBConnection")));

// Serviços
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFFmpegService, FFmpegService>();

// Background Services
builder.Services.AddHostedService<VideoProcessingBackgroundService>();
builder.Services.AddHostedService<CleanupBackgroundService>();

// Controllers e JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Video Processing API", 
        Version = "v1",
        Description = "API para processamento assíncrono de vídeos usando FFmpeg"
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
});

// CORS
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

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<JobDbContext>()
    .AddCheck<FFmpegHealthCheck>("ffmpeg");

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Processing API V1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware customizado
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Garantir que o banco de dados seja criado
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    context.Database.EnsureCreated();
}

try
{
    Log.Information("Iniciando aplicação Video Processing API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
```

### arquivo .csproj atualizado

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>video-processing-api-secrets</UserSecretsId>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="9.0.0" />
    <PackageReference Include="System.Text.Json" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include="logs\" />
    <Folder Include="uploads\" />
    <Folder Include="processed\" />
  </ItemGroup>

</Project>
```

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["VideoProcessingApi/VideoProcessingApi.csproj", "VideoProcessingApi/"]
RUN dotnet restore "VideoProcessingApi/VideoProcessingApi.csproj"
COPY . .
WORKDIR "/src/VideoProcessingApi"
RUN dotnet build "VideoProcessingApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoProcessingApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/uploads /app/processed /app/db /app/logs && \
    chmod 755 /app/uploads /app/processed /app/db /app/logs

ENTRYPOINT ["dotnet", "VideoProcessingApi.dll"]
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "APIDBConnection": "Data Source=db/jobs.db"
  },
  "Api": {
    "AllowedFileTypes": [".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv"],
    "MaxFileSizeBytes": 524288000,
    "MaxConcurrentJobs": 3,
    "UploadsPath": "uploads",
    "ProcessedPath": "processed",
    "FileRetentionPeriod": "7.00:00:00"
  },
  "FFmpeg": {
    "BinaryPath": "/usr/bin/ffmpeg",
    "TimeoutMinutes": 30,
    "DefaultQuality": "medium",
    "QualityPresets": {
      "low": "fast",
      "medium": "medium", 
      "high": "slow",
      "ultra": "slower"
    }
  },
  "Security": {
    "ApiKeys": [
      "dev-key-12345",
      "prod-key-67890"
    ],
    "RateLimit": {
      "MaxRequestsPerMinute": 30,
      "MaxRequestsPerHour": 1000,
      "MaxRequestsPerDay": 10000
    },
    "Cors": {
      "AllowedOrigins": ["http://localhost:3000", "https://yourdomain.com"],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
      "AllowedHeaders": ["Content-Type", "Authorization", "X-API-Key"]
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/api-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId"]
  }
}
```
## 7. 🔗 Links de Referência

*   **Documentação do .NET 9:** [docs.microsoft.com/dotnet/core](https://docs.microsoft.com/en-us/dotnet/core/)
*   **EF Core com SQLite:** [docs.microsoft.com/ef/core/providers/sqlite](https://docs.microsoft.com/en-us/ef/core/providers/sqlite)
*   **Documentação do FFmpeg:** [ffmpeg.org/documentation.html](https://ffmpeg.org/documentation.html)
*   **Guia do Docker Compose:** [docs.docker.com/compose/](https://docs.docker.com/compose/)
*   **ASP.NET Core Web API:** [docs.microsoft.com/aspnet/core/web-api](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
*   **Serilog para .NET:** [serilog.net](https://serilog.net/)
*   **Background Services:** [docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
*   **Rate Limiting em ASP.NET Core:** [docs.microsoft.com/aspnet/core/performance/rate-limit](https://docs.microsoft.com/en-us/aspnet/core/performance/rate-limit)

## 8. 🧪 Testes e Validação

### Estrutura de Testes

```
tests/
├── VideoProcessingApi.Tests.Unit/
│   ├── Services/
│   │   ├── JobServiceTests.cs
│   │   ├── FFmpegServiceTests.cs
│   │   └── FileServiceTests.cs
│   ├── Controllers/
│   │   ├── JobsControllerTests.cs
│   │   └── FilesControllerTests.cs
│   └── Validators/
│       ├── FileValidatorTests.cs
│       └── JobRequestValidatorTests.cs
├── VideoProcessingApi.Tests.Integration/
│   ├── Controllers/
│   │   └── JobsControllerIntegrationTests.cs
│   ├── BackgroundServices/
│   │   └── VideoProcessingBackgroundServiceTests.cs
│   └── TestFixtures/
│       └── ApiTestFixture.cs
└── VideoProcessingApi.Tests.E2E/
    ├── Scenarios/
    │   ├── VideoMergeE2ETests.cs
    │   ├── VideoConversionE2ETests.cs
    │   └── ErrorHandlingE2ETests.cs
    └── TestData/
        ├── sample_video_1.mp4
        ├── sample_video_2.mp4
        └── invalid_file.txt
```

### Exemplo de Teste de Integração

```csharp
// JobsControllerIntegrationTests.cs
public class JobsControllerIntegrationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public JobsControllerIntegrationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", "test-api-key");
    }

    [Fact]
    public async Task CreateJob_WithValidVideos_ReturnsJobId()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var videoFile1 = new ByteArrayContent(File.ReadAllBytes("TestData/sample_video_1.mp4"));
        videoFile1.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        content.Add(videoFile1, "files", "video1.mp4");

        var videoFile2 = new ByteArrayContent(File.ReadAllBytes("TestData/sample_video_2.mp4"));
        videoFile2.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        content.Add(videoFile2, "files", "video2.mp4");

        content.Add(new StringContent("Merge"), "processingType");

        // Act
        var response = await _client.PostAsync("/api/jobs", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateJobResponse>();
        result.JobId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetJobStatus_WithValidJobId_ReturnsCorrectStatus()
    {
        // Arrange
        var jobId = await CreateTestJobAsync();

        // Act
        var response = await _client.GetAsync($"/api/jobs/{jobId}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobStatusResponse>();
        result.JobId.Should().Be(jobId);
        result.Status.Should().BeOneOf(JobStatus.Pending, JobStatus.Processing, JobStatus.Completed);
    }
}
```

## 9. 🚀 Deployment e Monitoramento

### Docker Compose para Produção

```yaml
version: '3.8'

services:
  video-processing-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - video_uploads:/app/uploads
      - video_processed:/app/processed
      - video_db:/app/db
      - video_logs:/app/logs
      - ./certs:/app/certs:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/cert.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
      - ConnectionStrings__APIDBConnection=Data Source=/app/db/jobs.db
    env_file:
      - .env.production
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 4G
        reservations:
          cpus: '1.0'
          memory: 2G

  nginx:
    image: nginx:alpine
    ports:
      - "8080:80"
      - "8443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./certs:/etc/nginx/certs:ro
      - video_logs:/var/log/nginx
    depends_on:
      - video-processing-api
    restart: unless-stopped

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'

volumes:
  video_uploads:
  video_processed:
  video_db:
  video_logs:
  prometheus_data:
```

### Configuração do Nginx

```nginx
events {
    worker_connections 1024;
}

http {
    upstream video_api {
        server video-processing-api:80;
    }

    server {
        listen 80;
        server_name localhost;

        client_max_body_size 500M;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;

        location / {
            proxy_pass http://video_api;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /health {
            proxy_pass http://video_api/health;
            access_log off;
        }
    }
}
```

## 10. ✅ Checklist de Aceitação Expandido

### Funcionalidades Básicas
- [ ] `docker-compose up` executa a API e cria o volume do banco de dados sem erros
- [ ] O endpoint de upload de vídeo aceita arquivos e cria um novo job com status `Pending`
- [ ] Um processo em background (background service) pega o job da fila e muda o status para `Processing`
- [ ] A chamada ao FFmpeg é executada com sucesso, fundindo os vídeos
- [ ] O status do job é atualizado para `Completed` e o link de download é válido
- [ ] Em caso de erro no FFmpeg, o status do job é atualizado para `Failed`
- [ ] O endpoint de status retorna o estado atual correto do job

### Segurança e Validação
- [ ] Autenticação via API Key funciona corretamente
- [ ] Rate limiting bloqueia requisições excessivas
- [ ] Validação de tipos de arquivo impede upload de arquivos inválidos
- [ ] Sanitização de nomes de arquivos previne path traversal
- [ ] Headers de segurança são aplicados corretamente
- [ ] CORS está configurado adequadamente

### Processamento Avançado
- [ ] Conversão de formato de vídeo funciona
- [ ] Compressão de vídeo com diferentes qualidades
- [ ] Corte de vídeo (trim) com timestamps específicos
- [ ] Extração de áudio de vídeos
- [ ] Processamento paralelo de múltiplos jobs
- [ ] Cancelamento de jobs funciona corretamente

### Monitoramento e Logging
- [ ] Logs estruturados são gerados corretamente
- [ ] Health checks respondem adequadamente
- [ ] Métricas de performance são coletadas
- [ ] Cleanup de arquivos temporários funciona
- [ ] Retry automático para jobs falhados

### Testes
- [ ] Testes unitários cobrem serviços principais
- [ ] Testes de integração validam endpoints
- [ ] Testes E2E cobrem cenários completos
- [ ] Performance tests validam throughput esperado

### Deployment
- [ ] Docker build cria imagem sem erros
- [ ] Aplicação roda corretamente em containers
- [ ] Nginx proxy funciona adequadamente
- [ ] SSL/TLS está configurado (se aplicável)
- [ ] Backup e restore do banco de dados funcionam

## 11. 📈 Próximos Passos e Melhorias Futuras

### Fase 2 - Escalabilidade
- [ ] Implementar Redis para cache e fila distribuída
- [ ] Adicionar suporte a múltiplas instâncias da API
- [ ] Implementar storage distribuído (AWS S3, Azure Blob)
- [ ] Adicionar métricas avançadas com Prometheus/Grafana

### Fase 3 - Funcionalidades Avançadas
- [ ] Suporte a mais formatos de vídeo
- [ ] Processamento com IA (detecção de objetos, transcrição)
- [ ] Watermarks dinâmicos e logos
- [ ] Streaming de vídeo adaptável
- [ ] Webhooks para notificações de status

### Fase 4 - Enterprise Features
- [ ] Multi-tenancy com isolamento de dados
- [ ] Dashboard administrativo
- [ ] Relatórios de uso e analytics
- [ ] Integração com serviços de pagamento
- [ ] API versioning e backward compatibility

---

*Última atualização: Setembro 2025*
*Versão do documento: 2.0*