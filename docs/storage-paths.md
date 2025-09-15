# Storage paths

This API stores uploads and processed outputs under a single base directory.

Recommended layout inside container:

- /app/videos_cache/uploads
- /app/videos_cache/processed

Configuration:

appsettings.docker.json

```
"Api": {
  "UploadsPath": "/app/videos_cache/uploads",
  "ProcessedPath": "/app/videos_cache/processed"
},
"Storage": {
  "Provider": "FileSystem",
  "FileSystem": { "BasePath": "/app/videos_cache" }
}
```

Volumes (docker-compose.yml):

```
volumes:
  - ./uploads:/app/videos_cache/uploads
  - ./processed:/app/videos_cache/processed
```

Notes:

- Output path stored in DB is resolved against Api.ProcessedPath when relative.
- FileSystemStorageService resolves absolute and relative paths safely.