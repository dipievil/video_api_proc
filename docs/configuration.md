# Configuration Guide - appsettings.json

This document explains the different configurations available in the project and when to use each one.

## 📁 Available Configuration Files

### 1. 🎯 `appsettings.json` (Main Configuration)
**When to use**: For local development, first setup, or simple environments.

**Features**:
- Basic and user-friendly configuration
- Explanatory comments in English
- Safe default values for development
- Relative paths for local files
- Example API Keys that should be changed

**Usage example**:
```bash
# Simple local development
dotnet run
```

### 2. 🐳 `appsettings.docker.json` (Docker Configuration)
**When to use**: For Docker/containerized environments and local development with low security.

**Features**:
- Absolute paths for containers (`/app/uploads`, `/app/logs`)
- MinIO configuration pointing to `minio:9000` service
- Example API Keys for local environment
- Logs directed to Docker volumes
- Database in persistent volume

**Usage example**:
```bash
# With Docker Compose
ASPNETCORE_ENVIRONMENT=Docker docker-compose up

# Or setting directly
export ASPNETCORE_ENVIRONMENT=Docker
dotnet run
```

### 3. 🚀 `appsettings.Production.json` (Production Configuration)
**When to use**: For production environments with high security.

**Features**:
- HTTPS enabled
- WARNING level logs
- More restrictive rate limits
- CORS configured for specific domains
- Performance-optimized settings

**Usage example**:
```bash
# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

### 4. 🧪 `appsettings.Development.json` (Development Configuration)
**When to use**: Automatically used in development environment.

**Features**:
- Minimal configuration that overrides the main one
- Simplified local storage
- Development bucket in MinIO

## 🔄 Precedence Order

Configurations are applied in the following order (last one overrides previous):

1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables
4. Command line arguments

## ⚙️ Configuring the Environment

### Local Development
```bash
# Uses appsettings.json + appsettings.Development.json
dotnet run
```

### Docker/Containers
```bash
# Uses appsettings.json + appsettings.docker.json
ASPNETCORE_ENVIRONMENT=docker docker-compose up
```

### Production
```bash
# Uses appsettings.json + appsettings.Production.json
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

## 🔧 Key Differences

| Configuration | Database | Storage Paths | API Keys | HTTPS | Logs |
|---------------|----------|---------------|----------|-------|------|
| **Main** | `jobs.db` | Relative (`./uploads`) | Example | ❌ | Local |
| **Docker** | `/app/db/jobs.db` | Absolute (`/app/uploads`) | Example | ❌ | Volume |
| **Production** | `./data/video_proc.db` | - | Production | ✅ | Local |
| **Development** | - | `./uploads` | - | ❌ | - |

## 🛡️ Security

### ⚠️ API Keys
**IMPORTANT**: Always change the default API Keys before using in production!

```json
{
  "Security": {
    "ApiKeys": [
      "your-secure-api-key-here-123456"
    ]
  }
}
```

### 🌐 CORS
Configure allowed domains according to your environment:

```json
{
  "Security": {
    "Cors": {
      "AllowedOrigins": ["https://yourdomain.com"]
    }
  }
}
```

## 🐛 Common Issues

### FFmpeg not found
**Solution**: Configure the correct path:
```json
{
  "FFmpeg": {
    "BinaryPath": "/usr/bin/ffmpeg"  // Linux/Docker
    // "BinaryPath": "C:\\ffmpeg\\bin\\ffmpeg.exe"  // Windows
  }
}
```

### Permission Problems
**Solution**: Check if directories exist and have write permissions:
```bash
mkdir -p uploads processed logs
chmod 755 uploads processed logs
```

### MinIO Connection Issues
**Solution**: Check if the service is running and configuration is correct:
```json
{
  "Storage": {
    "Provider": "MinIO",
    "MinIO": {
      "Endpoint": "localhost:9000",  // or "minio:9000" in Docker
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin123"
    }
  }
}
```

## 📚 Usage Examples

### 1. Simple Local Development
```bash
# Just run - uses default configuration
dotnet run
```

### 2. Development with Docker
```bash
# Docker Compose (recommended)
docker-compose up

# Or manual
ASPNETCORE_ENVIRONMENT=docker dotnet run
```

### 3. Production
```bash
# Configure environment variables first
export ASPNETCORE_ENVIRONMENT=Production
export Security__ApiKeys__0="your-super-secret-key"
export ConnectionStrings__DefaultConnection="your-connection-string"

dotnet run
```

### 4. Override Configurations
```bash
# Via environment variables
export FFmpeg__BinaryPath="/usr/local/bin/ffmpeg"
export Api__MaxConcurrentJobs=5

dotnet run
```

## 🔍 Validating Configuration

To check if your configuration is correct:

1. **Build**: `dotnet build`
2. **Health Check**: Access `http://localhost:5000/health`
3. **Swagger**: Access `http://localhost:5000/swagger`
4. **Logs**: Check logs in `logs/api-{date}.txt`

## 💡 Tips

- Use `appsettings.json` to get started
- Switch to `appsettings.docker.json` when using Docker
- Configure `appsettings.Production.json` for production
- Always use environment variables for sensitive data
- Test each configuration before deployment