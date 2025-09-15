# Pastas de armazenamento

- Base: /app/videos_cache
- Uploads: /app/videos_cache/uploads
- Processados: /app/videos_cache/processed

Compose:

```
volumes:
  - ./uploads:/app/videos_cache/uploads
  - ./processed:/app/videos_cache/processed
```

Dica: garanta permissões de escrita no host.
