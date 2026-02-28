using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace PanOpticon.Services;

public class MinioStorage
{
    private readonly PanOpticonOptions _options;
    private IMinioClient? _client;

    public MinioStorage(IOptions<PanOpticonOptions> options)
    {
        _options = options.Value;
    }

    private IMinioClient GetClient()
    {
        if (_client != null) return _client;

        _client = new MinioClient()
            .WithEndpoint(_options.MinioEndpoint)
            .WithCredentials(_options.MinioAccessKey, _options.MinioSecretKey)
            .WithSSL(_options.MinioSecure)
            .Build();

        if (!_client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.MinioBucket)).GetAwaiter().GetResult())
        {
            _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.MinioBucket)).GetAwaiter().GetResult();
        }

        return _client;
    }

    public async Task<string> UploadReportAsync(string objectName, Stream data, string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    {
        var client = GetClient();
        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_options.MinioBucket)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType));
        return $"{_options.MinioBucket}/{objectName}";
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expiresSeconds = 3600)
    {
        var client = GetClient();
        return await client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_options.MinioBucket)
            .WithObject(objectName)
            .WithExpiry(expiresSeconds));
    }
}
