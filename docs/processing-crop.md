# Video Crop Processing

This document explains how to use the crop feature added to `ProcessingOptions`.

## New fields in ProcessingOptions

- `CropWidth` (int?) - width of crop rectangle in pixels
- `CropHeight` (int?) - height of crop rectangle in pixels
- `CropX` (int?) - x offset (default 0)
- `CropY` (int?) - y offset (default 0)

When `CropWidth` and `CropHeight` are provided, the FFmpeg `-vf "crop=..."` filter will be applied. If both crop and scale are provided, crop runs first then scale (filters are joined with commas).

## How to call the API

Endpoint: `POST /api/jobs` (multipart/form-data)

Fields:
- `ProcessingType` (string) - For example `Convert` or `Compress`
- `Options` (string) - JSON-serialized `ProcessingOptions` object
- `Files` - one or more uploaded files

Example curl (convert with crop):

```bash
curl -X POST "http://localhost:5000/api/jobs" \
  -H "X-API-Key: your-key" \
  -F "ProcessingType=Convert" \
  -F 'Options={"Resolution":"1280:720","Quality":"fast","CropWidth":640,"CropHeight":360,"CropX":10,"CropY":20}' \
  -F "Files=@/path/to/input.mp4"
```

Notes
- The API expects `Options` as JSON in a form field. The server will deserialize it into `ProcessingOptions`.
- Inputs are not fully validated server-side (e.g., crop exceeding input dimensions). Consider validating on client-side or enhancing server with ffprobe checks.
