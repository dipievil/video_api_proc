namespace VideoProcessingApi.Configuration;

public class StorageSettings
{
    public string Provider { get; set; } = "FileSystem";
    public FileSystemStorageSettings FileSystem { get; set; } = new();
    public MinIOStorageSettings MinIO { get; set; } = new();
}

public class FileSystemStorageSettings
{
    public string BasePath { get; set; } = "./";
}

public class MinIOStorageSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
    public string BucketName { get; set; } = "video-processing";
    public bool CreateBucketIfNotExists { get; set; } = true;
}