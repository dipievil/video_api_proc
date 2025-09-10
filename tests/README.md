# Integration Tests - Video Processing API

Este diretório contém testes de integração completos para a Video Processing API, testando todos os fluxos principais de processamento de vídeo em um ambiente Docker isolado.

## 🎯 Objetivo

Os testes de integração validam:
- ✅ Criação e processamento de jobs end-to-end
- ✅ Integração com FFmpeg e Docker
- ✅ Persistência de dados e gestão de arquivos
- ✅ APIs REST e códigos de resposta
- ✅ Tratamento de erros e cenários edge-case

## 🚀 Como Executar

### Execução Rápida
```bash
# Linux/macOS
./scripts/run-integration-tests.sh

# Windows
.\scripts\run-integration-tests.ps1
```

### Execução Manual
```bash
# 1. Construir os testes
dotnet build tests/VideoProcessingApi.IntegrationTests/

# 2. Executar todos os testes
dotnet test tests/VideoProcessingApi.IntegrationTests/ --logger "console;verbosity=normal"

# 3. Executar testes específicos
dotnet test tests/VideoProcessingApi.IntegrationTests/ --filter "VideoMergeTests"
```

## 📊 Cenários Testados

### Video Merge Tests
- ✅ Upload de múltiplos vídeos
- ✅ Criação de job de merge
- ✅ Aguardar conclusão do processamento
- ✅ Consulta de status
- ✅ Tratamento de jobs inexistentes

### Video Convert Tests
- ✅ Conversão para diferentes formatos (MP4, AVI)
- ✅ Diferentes qualidades (low, medium, high, ultra)
- ✅ Configuração de resolução personalizada
- ✅ Validação de parâmetros

### Video Compress Tests
- ✅ Compressão com bitrate personalizado
- ✅ Diferentes níveis de compressão
- ✅ Aguardar conclusão e validar resultado
- ✅ Teste com múltiplos bitrates

### Video Trim Tests
- ✅ Corte com tempo inicial e final
- ✅ Extração dos primeiros 10 segundos
- ✅ Corte de seção do meio
- ✅ Diferentes intervalos de tempo

### Audio Extraction Tests
- ✅ Extração básica de áudio
- ✅ Diferentes formatos de saída (MP3, WAV, AAC)
- ✅ Aguardar conclusão e validar resultado
- ✅ Configurações de qualidade

### Job Download Tests
- ✅ Download de resultados processados
- ✅ Validação de content-type
- ✅ Download de jobs inexistentes (404)
- ✅ Download de jobs pendentes (404)
- ✅ Cancelamento de jobs
- ✅ Cancelamento de jobs inexistentes

## 🏗️ Arquitetura dos Testes

### DockerComposeFixture
- Gerencia o ciclo de vida dos containers Docker
- Setup e teardown automático do ambiente
- Health checks para garantir serviços prontos
- Isolamento completo entre execuções

### ApiTestClient
- Cliente HTTP especializado para a API
- Métodos helpers para cada operação
- Aguardar conclusão de jobs automaticamente
- Serialização/deserialização JSON automática

### IntegrationTestBase
- Classe base para todos os testes
- Inicialização compartilhada do ambiente
- Utilitários para carregar vídeos de teste
- Cleanup automático de recursos

## 🔧 Configuração do Ambiente

### Docker Compose de Teste
```yaml
# docker-compose.test.yml
services:
  video-processing-api-test:
    ports: ["5002:80"]  # Porta diferente para evitar conflitos
    environment:
      - Security__ApiKeys__0=test-api-key-12345
      - RateLimit__MaxRequestsPerMinute=100
    
  minio-test:
    ports: ["9002:9000", "9003:9001"]  # Portas alternativas
```

### Variáveis de Ambiente
- `ASPNETCORE_ENVIRONMENT=Development`
- `Security__ApiKeys__0=test-api-key-12345`
- `RateLimit__MaxRequestsPerMinute=100`

## 📁 Dados de Teste

### Vídeo de Teste
- **Arquivo**: `tests/videos/test_video.mp4`
- **Uso**: Base para todos os testes de processamento
- **Backup**: Fallback para vídeo dummy se não encontrado

### Diretórios Temporários
- `tests/uploads/` - Arquivos enviados
- `tests/processed/` - Resultados processados
- `tests/db/` - Banco de dados SQLite
- `tests/logs/` - Logs da aplicação

## 🚨 Troubleshooting

### Problemas Comuns

**Erro: Docker não está rodando**
```bash
sudo systemctl start docker
docker --version
```

**Erro: Porta em uso**
```bash
# Parar containers existentes
docker-compose -f docker-compose.test.yml down --volumes

# Verificar portas
sudo lsof -i :5002
```

**Erro: Permissão negada**
```bash
sudo usermod -aG docker $USER
# Fazer logout e login novamente
```

**Timeout nos testes**
- Aumentar recursos do Docker (4GB+ RAM)
- Verificar logs: `docker-compose -f docker-compose.test.yml logs`
- Executar testes individuais para isolar problemas

### Logs de Debug

```bash
# Ver logs da API durante testes
docker-compose -f docker-compose.test.yml logs -f video-processing-api-test

# Ver logs do FFmpeg
docker-compose -f docker-compose.test.yml logs ffmpeg-service-test
```

## 📈 Métricas e Cobertura

### Tempo de Execução Típico
- **Setup inicial**: ~30 segundos (download de imagens)
- **Execução completa**: ~3-5 minutos
- **Cleanup**: ~10 segundos

### Recursos Necessários
- **RAM**: 4GB+ recomendado
- **Disco**: 2GB temporário
- **CPU**: 2+ cores para paralelização

### Cobertura de Código
```bash
# Executar com cobertura
dotnet test tests/VideoProcessingApi.IntegrationTests/ --collect:"XPlat Code Coverage"
```

## 🔄 CI/CD Integration

### GitHub Actions
```yaml
jobs:
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run Integration Tests
        run: ./scripts/run-integration-tests.sh
```

### Execução Local vs CI
- **Local**: Usa Docker Desktop
- **CI**: Usa Docker Engine nativo
- **Diferenças**: Recursos e performance podem variar

## 📚 Recursos Adicionais

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)