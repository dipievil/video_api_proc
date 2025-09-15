# MinIO Storage Configuration

Esta API suporta dois tipos de storage para arquivos: sistema de arquivos local e MinIO (S3-compatible).

## Configuração

### Sistema de Arquivos Local (Padrão)

```json
{
  "Storage": {
    "Provider": "FileSystem",
    "FileSystem": {
      "BasePath": "./videos_cache"
    }
  }
}
```

### MinIO/S3

```json
{
  "Storage": {
    "Provider": "MinIO",
    "MinIO": {
      "Endpoint": "localhost:9000",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin123",
      "UseSSL": false,
      "BucketName": "video-processing",
      "CreateBucketIfNotExists": true
    }
  }
}
```

## Executando com Docker Compose

O docker-compose.yml inclui um serviço MinIO configurado com segurança baixa para desenvolvimento:

```bash
# Iniciar todos os serviços
docker-compose up -d

# Acessar interface web do MinIO
# URL: http://localhost:9001
# Login: minioadmin
# Senha: minioadmin123
```

## Configurações MinIO

### Parâmetros

- **Provider**: `"FileSystem"` ou `"MinIO"`
- **Endpoint**: Endereço do servidor MinIO (ex: `localhost:9000`)
- **AccessKey**: Chave de acesso do MinIO
- **SecretKey**: Chave secreta do MinIO
- **UseSSL**: `true` para HTTPS, `false` para HTTP
- **BucketName**: Nome do bucket onde os arquivos serão armazenados
- **CreateBucketIfNotExists**: Criar bucket automaticamente se não existir

### Troca de Provider

Para alternar entre os provedores de storage:

1. Pare a aplicação
2. Altere o valor de `Storage.Provider` no `appsettings.json`
3. Configure as credenciais apropriadas
4. Reinicie a aplicação

## Segurança

⚠️ **Aviso**: As configurações padrão do MinIO usam credenciais de desenvolvimento. Em produção:

1. Altere `MINIO_ROOT_USER` e `MINIO_ROOT_PASSWORD`
2. Configure SSL/TLS (`UseSSL: true`)
3. Use redes isoladas
4. Configure políticas de acesso adequadas

## Troubleshooting

### MinIO não conecta

1. Verifique se o serviço MinIO está rodando: `docker-compose ps`
2. Confirme o endpoint e porta na configuração
3. Teste conectividade: `curl http://localhost:9000/minio/health/live`

### Bucket não é criado

1. Verifique as credenciais de acesso
2. Confirme que `CreateBucketIfNotExists` está `true`
3. Verifique os logs da aplicação para erros de permissão