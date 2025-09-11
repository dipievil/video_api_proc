# 🎬 Video Processing API

A simple and powerful API for video processing using FFmpeg. Perform merge, conversion, compression, and trimming operations asynchronously.

> 🇧🇷 **[Versão em português disponível aqui](README.md)**

## ✨ Features

- **📁 Multiple Video Upload**: Upload multiple files at once
- **🔄 Asynchronous Processing**: Track progress in real-time
- **🎥 Video Merging**: Combine multiple videos into one
- **🔄 Format Conversion**: Convert between MP4, AVI, MOV, MKV
- **📉 Compression**: Reduce video file sizes
- **✂️ Video Trimming**: Extract specific segments
- **🎵 Audio Extraction**: Extract audio from videos

## 🚀 Quick Start

### Prerequisites

- Docker and Docker Compose installed
- At least 2GB of available RAM
- Disk space for video files

### Installation

1. **Download the project**:
   ```bash
   git clone https://github.com/your-username/video_api_proc.git
   cd video_api_proc
   ```

2. **Configure variables (optional)**:
   ```bash
   cp .env.example .env
   # Edit the .env file if needed
   ```

3. **Start the application**:
   ```bash
   docker-compose up -d
   ```

4. **Access the API**:
   - API: http://localhost:5000 (HTTP by default)
   - Documentation: http://localhost:5000/swagger
   - For HTTPS: https://localhost:5001 (if enabled)

## 📖 How to Use

### 1. Get an API Key

By default, use one of the configured keys:
- `dev-key-12345` (development)
- `prod-key-67890` (production)

### 2. Upload and Process Videos

#### Video Merging
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Merge" \
  -F "files=@video1.mp4" \
  -F "files=@video2.mp4"
```

#### Format Conversion
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Convert" \
  -F "files=@video.avi" \
  -F "options={\"outputFormat\":\"mp4\",\"quality\":\"medium\"}"
```

#### Compression
```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: dev-key-12345" \
  -F "processingType=Compress" \
  -F "files=@video.mp4" \
  -F "options={\"quality\":\"low\",\"bitrateKbps\":1000}"
```

### 3. Check Job Status

```bash
curl -X GET "http://localhost:5000/api/jobs/{job-id}/status" \
  -H "X-API-Key: dev-key-12345"
```

### 4. Download the Result

When the status is `Completed`, use the link provided in the response:

```bash
curl -X GET "http://localhost:5000/api/jobs/{job-id}/download" \
  -H "X-API-Key: dev-key-12345" \
  -o processed_video.mp4
```

## 🎛️ Processing Options

### Available Qualities
- `low` - Fast processing, basic quality
- `medium` - Balanced (default)
- `high` - High quality, slower
- `ultra` - Maximum quality

### Supported Formats
- **Input**: MP4, AVI, MOV, MKV, WMV, FLV
- **Output**: MP4 (default), AVI, MOV, MKV

### Trimming Options
```json
{
  "startTime": 10.5,
  "endTime": 60.0
}
```

### Resolution Options
```json
{
  "resolution": "1920x1080"
}
```

## 📊 Job Status

- `Pending` - Queued for processing
- `Processing` - Being processed
- `Completed` - Successfully finished
- `Failed` - Processing error
- `Canceled` - Canceled by user

## 🛠️ Advanced Configuration

### HTTPS Configuration

By default, the application runs only on HTTP. To enable HTTPS:

**Development (appsettings.Development.json):**
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

**Production (appsettings.Production.json):**
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

> **💡 Tip**: To generate a development certificate, use:
> ```bash
> dotnet dev-certs https --export-path certificate.pfx --password YourPassword
> ```

### ⚙️ Configuration Files

The application offers different configurations for different environments:

| File | When to Use | Features |
|------|-------------|----------|
| `appsettings.json` | 🎯 **Local Development** | User-friendly with comments |
| `appsettings.docker.json` | 🐳 **Docker/Containers** | Absolute paths, low security |
| `appsettings.Production.json` | 🚀 **Production** | HTTPS, high security |

#### How to Choose Configuration

```bash
# Local development (default)
dotnet run

# Docker/Containers
ASPNETCORE_ENVIRONMENT=docker docker-compose up

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

> 📖 **For more details**, see the [complete configuration documentation](docs/configuration.md)


### Environment Variables

Edit the `.env` file to customize:

```env
# File limits
API_MAX_FILE_SIZE_MB=500
API_MAX_CONCURRENT_JOBS=3

# Security
API_KEYS=your-key-here,another-key
RATE_LIMIT_MAX_REQUESTS_PER_MINUTE=30

# FFmpeg
FFMPEG_TIMEOUT_MINUTES=30
```

### Logs

Logs are available at:
- Container: `docker-compose logs video-processing-api`
- File: `./logs/api-*.txt`

## 🔧 Troubleshooting

### Common Issues

**❌ Error 429 - Too Many Requests**
- Wait a moment before making a new request
- Check the configured rate limiting

**❌ Error 413 - File Too Large**
- Reduce the file size
- Adjust `API_MAX_FILE_SIZE_MB` in `.env`

**❌ Job with "Failed" status**
- Check logs: `docker-compose logs video-processing-api`
- Confirm the video file is not corrupted

**❌ FFmpeg not found**
- Restart containers: `docker-compose restart`
- Check if the image was built correctly

### Check API Health

```bash
curl http://localhost:5000/health
```

### Cancel a Job

```bash
curl -X DELETE "http://localhost:5000/api/jobs/{job-id}" \
  -H "X-API-Key: dev-key-12345"
```

## 📝 Practical Examples

### Simple Web Interface (HTML + JavaScript)

```html
<!DOCTYPE html>
<html>
<head>
    <title>Video Processor</title>
</head>
<body>
    <h1>Video Processor</h1>
    
    <form id="uploadForm">
        <input type="file" id="videoFiles" multiple accept="video/*">
        <select id="processingType">
            <option value="Merge">Merge Videos</option>
            <option value="Convert">Convert</option>
            <option value="Compress">Compress</option>
        </select>
        <button type="submit">Process</button>
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
                document.getElementById('status').innerHTML = `Error: ${error.message}`;
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
                    `<br><a href="${API_URL}/jobs/${jobId}/download" download>Download Result</a>`;
            }
        }
    </script>
</body>
</html>
```

## 🔒 Security

- Use HTTPS in production
- Keep API Keys secure
- Configure appropriate rate limiting
- Monitor logs regularly

## 📞 Support

- **Logs**: `docker-compose logs -f video-processing-api`
- **Health Check**: `http://localhost:5000/health`
- **Documentation**: `http://localhost:5000/swagger`

## 📄 License

This project is under the MIT license. See the [LICENSE](LICENSE) file for more details.

---

**🎬 Developed with ❤️ to make video processing easier**