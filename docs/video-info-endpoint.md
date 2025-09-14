# Video Info Endpoint Documentation

## Overview
The video info endpoint allows you to upload a video file and get detailed metadata information including dimensions, duration, codecs, bitrate, and more.

## Endpoint
- **URL**: `POST /api/videos/info`
- **Content-Type**: `multipart/form-data`
- **Authentication**: Requires `X-API-Key` header

## Request Parameters
- `file` (required): Video file to analyze

## Response Format
```json
{
  "width": 1920,
  "height": 1080,
  "aspectRatio": "16:9",
  "duration": 120.5,
  "bitrate": 2500,
  "frameRate": 30.0,
  "codec": "h264",
  "audioCodec": "aac",
  "fileSize": 15728640,
  "filename": "sample.mp4",
  "format": "mp4"
}
```

## Response Fields
- `width`: Video width in pixels
- `height`: Video height in pixels
- `aspectRatio`: Calculated aspect ratio (e.g., "16:9", "4:3")
- `duration`: Video duration in seconds
- `bitrate`: Video bitrate in kilobits per second
- `frameRate`: Frame rate in frames per second
- `codec`: Video codec (e.g., "h264", "h265", "vp9")
- `audioCodec`: Audio codec (e.g., "aac", "mp3", "opus")
- `fileSize`: File size in bytes
- `filename`: Original filename
- `format`: Container format (e.g., "mp4", "avi", "mov")

## Example Usage

### cURL
```bash
curl -X POST "http://localhost:5000/api/videos/info" \
  -H "X-API-Key: dev-key-12345" \
  -F "file=@/path/to/your/video.mp4"
```

### Response Example
```json
{
  "width": 1920,
  "height": 1080,
  "aspectRatio": "16:9",
  "duration": 125.5,
  "bitrate": 2800,
  "frameRate": 29.97,
  "codec": "h264",
  "audioCodec": "aac",
  "fileSize": 52428800,
  "filename": "sample_video.mp4",
  "format": "mp4"
}
```

### JavaScript/Fetch
```javascript
const formData = new FormData();
formData.append('file', videoFile);

const response = await fetch('/api/videos/info', {
  method: 'POST',
  headers: {
    'X-API-Key': 'your-api-key'
  },
  body: formData
});

const videoInfo = await response.json();
console.log('Video info:', videoInfo);
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "File type '.txt' is not allowed. Allowed types: .mp4, .avi, .mov, .mkv"
}
```

### 500 Internal Server Error
```json
{
  "error": "Error processing video file",
  "details": "FFprobe execution failed: No such file or directory"
}
```

## Supported Video Formats
- MP4 (.mp4)
- AVI (.avi)
- MOV (.mov)
- MKV (.mkv)
- And other formats supported by FFmpeg

## Technical Details
- Uses FFprobe (part of FFmpeg) to extract metadata
- Temporary files are automatically cleaned up after processing
- File size limits apply as configured in the API settings
- Processing is synchronous and returns results immediately