# 🎬 Video Processing API

Uma API simples e poderosa para processamento de vídeos usando FFmpeg. Faça merge, conversão, compressão e corte de vídeos de forma assíncrona.

> 🇺🇸 **[English version available here](README.en.md)**

## ✨ Funcionalidades

- **📁 Upload de Múltiplos Vídeos**: Envie vários arquivos de uma vez
- **🔄 Processamento Assíncrono**: Acompanhe o progresso em tempo real
- **🎥 Merge de Vídeos**: Junte vários vídeos em um só
- **🔄 Conversão de Formato**: Converta entre MP4, AVI, MOV, MKV
- **📉 Compressão**: Reduza o tamanho dos vídeos
- **✂️ Corte de Vídeo**: Extraia trechos específicos
- **🎵 Extração de Áudio**: Extraia áudio dos vídeos

## 🚀 Início Rápido

### Pré-requisitos

- Docker e Docker Compose instalados
- Pelo menos 2GB de RAM disponível
- Espaço em disco para os vídeos

### Instalação

1. **Baixe o projeto**:
   ```bash
   git clone https://github.com/seu-usuario/video_api_proc.git
   cd video_api_proc
   ```

2. **Configure as variáveis (opcional)**:
   ```bash
   cp .env.example .env
   # Edite o arquivo .env se necessário
   ```

3. **Inicie a aplicação**:
   ```bash
   docker-compose up -d
   ```

4. **Acesse a API**:
   - API: http://localhost:5000 (HTTP por padrão)
   - Documentação: http://localhost:5000/swagger
   - Para HTTPS: https://localhost:5001 (se habilitado)

## 📖 Como Usar

### 1. Obter uma API Key

Por padrão, use uma das chaves configuradas:
- `dev-key-12345` (desenvolvimento)
- `prod-key-67890` (produção)

### 2. Fazer Upload e Processar Vídeos

#### Merge de Vídeos
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Merge" \
  -F "files=@video1.mp4" \
  -F "files=@video2.mp4"
```

#### Conversão de Formato
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Convert" \
  -F "files=@video.avi" \
  -F "options={\"outputFormat\":\"mp4\",\"quality\":\"medium\"}"
```

#### Compressão
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Compress" \
  -F "files=@video.mp4" \
  -F "options={\"quality\":\"low\",\"bitrateKbps\":1000}"
```

### 3. Consultar Status do Job

```bash
curl -X GET "http://localhost:5000/api/jobs/{job-id}/status" \
  -H "X-API-Key: dev-key-12345"
```

### 4. Baixar o Resultado

Quando o status for `Completed`, use o link fornecido na resposta:

```bash
curl -X GET "http://localhost:5000/api/jobs/{job-id}/download" \
  -H "X-API-Key: dev-key-12345" \
  -o video_processado.mp4
```

## 🎛️ Opções de Processamento

### Qualidades Disponíveis
- `low` - Processamento rápido, qualidade básica
- `medium` - Balanceado (padrão)
- `high` - Alta qualidade, mais lento
- `ultra` - Máxima qualidade

### Formatos Suportados
- **Entrada**: MP4, AVI, MOV, MKV, WMV, FLV
- **Saída**: MP4 (padrão), AVI, MOV, MKV

### Opções de Corte
```json
{
  "startTime": 10.5,
  "endTime": 60.0
}
```

### Opções de Resolução
```json
{
  "resolution": "1920x1080"
}
```

## 📊 Status dos Jobs

- `Pending` - Na fila para processamento
- `Processing` - Sendo processado
- `Completed` - Finalizado com sucesso
- `Failed` - Erro no processamento
- `Canceled` - Cancelado pelo usuário

## 🛠️ Configuração Avançada

### Configuração HTTPS

Por padrão, a aplicação roda apenas em HTTP. Para habilitar HTTPS:

**Desenvolvimento (appsettings.Development.json):**
```json
{
  "UseHttpsRedirection": false,
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

**Produção (appsettings.Production.json):**
```json
{
  "UseHttpsRedirection": true,
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "YourCertificatePassword"
        }
      }
    }
  }
}
```

> **💡 Dica**: Para gerar um certificado de desenvolvimento, use:
> ```bash
> dotnet dev-certs https --export-path certificate.pfx --password YourPassword
> ```

### Variáveis de Ambiente

Edite o arquivo `.env` para personalizar:

```env
# Limites de arquivo
API_MAX_FILE_SIZE_MB=500
API_MAX_CONCURRENT_JOBS=3

# Segurança
API_KEYS=sua-chave-aqui,outra-chave
RATE_LIMIT_MAX_REQUESTS_PER_MINUTE=30

# FFmpeg
FFMPEG_TIMEOUT_MINUTES=30
```

### Logs

Os logs ficam disponíveis em:
- Container: `docker-compose logs video-processing-api`
- Arquivo: `./logs/api-*.txt`

## 🔧 Troubleshooting

### Problemas Comuns

**❌ Erro 429 - Too Many Requests**
- Aguarde um momento antes de fazer nova requisição
- Verifique o rate limiting configurado

**❌ Erro 413 - File Too Large**
- Reduza o tamanho do arquivo
- Ajuste `API_MAX_FILE_SIZE_MB` no `.env`

**❌ Job com status "Failed"**
- Verifique os logs: `docker-compose logs video-processing-api`
- Confirme se o arquivo de vídeo não está corrompido

**❌ FFmpeg não encontrado**
- Reinicie os containers: `docker-compose restart`
- Verifique se a imagem foi construída corretamente

### Verificar Saúde da API

```bash
curl http://localhost:5000/health
```

### Cancelar um Job

```bash
curl -X DELETE "http://localhost:5000/api/jobs/{job-id}" \
  -H "X-API-Key: dev-key-12345"
```

## 📝 Exemplos Práticos

### Interface Web Simples (HTML + JavaScript)

```html
<!DOCTYPE html>
<html>
<head>
    <title>Video Processor</title>
</head>
<body>
    <h1>Processador de Vídeo</h1>
    
    <form id="uploadForm">
        <input type="file" id="videoFiles" multiple accept="video/*">
        <select id="processingType">
            <option value="Merge">Juntar Vídeos</option>
            <option value="Convert">Converter</option>
            <option value="Compress">Comprimir</option>
        </select>
        <button type="submit">Processar</button>
    </form>
    
    <div id="status"></div>
    
    <script>
        const API_KEY = 'dev-key-12345';
        const API_URL = 'http://localhost:5000/api';
        
        document.getElementById('uploadForm').onsubmit = async (e) => {
            e.preventDefault();
            
            const formData = new FormData();
            const files = document.getElementById('videoFiles').files;
            const processingType = document.getElementById('processingType').value;
            
            for (let file of files) {
                formData.append('files', file);
            }
            formData.append('processingType', processingType);
            
            try {
                const response = await fetch(`${API_URL}/jobs`, {
                    method: 'POST',
                    headers: { 'X-API-Key': API_KEY },
                    body: formData
                });
                
                const result = await response.json();
                checkStatus(result.jobId);
            } catch (error) {
                document.getElementById('status').innerHTML = `Erro: ${error.message}`;
            }
        };
        
        async function checkStatus(jobId) {
            const response = await fetch(`${API_URL}/jobs/${jobId}/status`, {
                headers: { 'X-API-Key': API_KEY }
            });
            
            const status = await response.json();
            document.getElementById('status').innerHTML = `Status: ${status.status}`;
            
            if (status.status === 'Processing' || status.status === 'Pending') {
                setTimeout(() => checkStatus(jobId), 2000);
            } else if (status.status === 'Completed') {
                document.getElementById('status').innerHTML += 
                    `<br><a href="${API_URL}/jobs/${jobId}/download" download>Baixar Resultado</a>`;
            }
        }
    </script>
</body>
</html>
```

## 🧪 Integration Tests

This project includes comprehensive integration tests that cover all main video processing workflows using Docker Compose to simulate a realistic end-to-end environment.

### 📋 Test Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK
- At least 4GB of available RAM
- 2GB of free disk space

### 🚀 Running Tests

#### Manual execution (for development)
```bash
# 1. Build the test project
dotnet build tests/VideoProcessingApi.IntegrationTests/

# 2. Run all tests
dotnet test tests/VideoProcessingApi.IntegrationTests/ --logger "console;verbosity=normal"

# 3. Run specific tests
dotnet test tests/VideoProcessingApi.IntegrationTests/ --filter "VideoMergeTests"
```

### 🎯 Test Coverage

Integration tests cover:

#### ✅ Processing Operations
- **Video Merge**: Combining multiple MP4 files
- **Format Conversion**: MP4 → AVI, different qualities
- **Compression**: Bitrate reduction and size optimization
- **Video Trimming**: Extracting specific time ranges
- **Audio Extraction**: MP3, WAV, AAC output

#### ✅ Job Management
- Job creation for processing
- Real-time status monitoring
- Downloading processed results
- Canceling pending jobs

#### ✅ Error Scenarios
- Non-existent jobs
- Downloads from incomplete jobs
- Parameter validation

### 🔧 Test Configuration

Tests use an isolated Docker environment with:
- **API on port 5002** (to avoid conflicts)
- **MinIO on ports 9002/9003** (test storage)
- **Test API Key**: `test-api-key-12345`
- **Isolated data** in `./tests/` (auto-cleanup)

### 📁 Test Structure

```
tests/
├── VideoProcessingApi.IntegrationTests/
│   ├── Infrastructure/
│   │   ├── DockerComposeFixture.cs      # Docker management
│   │   ├── ApiTestClient.cs             # HTTP client for tests
│   │   └── IntegrationTestBase.cs       # Base test class
│   └── Tests/
│       ├── VideoMergeTests.cs           # Merge tests
│       ├── VideoConvertTests.cs         # Conversion tests
│       ├── VideoCompressTests.cs        # Compression tests
│       ├── VideoTrimTests.cs            # Trimming tests
│       ├── AudioExtractionTests.cs      # Audio extraction tests
│       └── JobDownloadTests.cs          # Download tests
└── videos/
    └── test_video.mp4                   # Test video file
```

### 🚨 Test Troubleshooting

**❌ Error: Docker not found**
```bash
# Install Docker on Ubuntu/Debian
sudo apt update && sudo apt install docker.io docker-compose
sudo usermod -aG docker $USER
# Logout and login again
```

**❌ Error: Permission denied on Docker**
```bash
sudo systemctl start docker
sudo usermod -aG docker $USER
# Restart terminal
```

**❌ Tests failing due to timeout**
- Increase Docker resources (4GB+ RAM)
- Check that no other containers are consuming resources
- Wait for initial Docker image downloads

**❌ Port 5002 in use**
```bash
# Check process using the port
sudo lsof -i :5002
# Stop old containers
docker-compose -f docker-compose.test.yml down --volumes
```

### 📊 Test Output Example

```
🎬 Video Processing API - Integration Tests
=============================================
✅ Prerequisites check passed
🧹 Cleaning up existing test environment...
🔧 Building test project...
🧪 Running all integration tests...

Test run for VideoProcessingApi.IntegrationTests.dll (.NETCoreApp,Version=v8.0)
Starting test execution, please wait...

[xUnit.net 00:00:00.00] Starting: VideoProcessingApi.IntegrationTests
[xUnit.net 00:00:05.23] VideoProcessingApi.IntegrationTests.Tests.VideoMergeTests.CreateMergeJob_WithMultipleVideos_ShouldReturnSuccess [PASS]
[xUnit.net 00:00:08.45] VideoProcessingApi.IntegrationTests.Tests.VideoConvertTests.CreateConvertJob_WithOutputFormat_ShouldReturnSuccess [PASS]
...

Test Run Successful.
Total tests: 15
     Passed: 15
     Failed: 0
     Skipped: 0
     Total time: 2.5 Minutes

✅ All integration tests passed!
```

## 🔒 Segurança

- Use HTTPS em produção
- Mantenha as API Keys seguras
- Configure rate limiting adequado
- Monitore os logs regularmente

## 📞 Suporte

- **Logs**: `docker-compose logs -f video-processing-api`
- **Health Check**: `http://localhost:5000/health`
- **Documentação**: `http://localhost:5000/swagger`

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

**🎬 Desenvolvido com ❤️ para facilitar o processamento de vídeos**
