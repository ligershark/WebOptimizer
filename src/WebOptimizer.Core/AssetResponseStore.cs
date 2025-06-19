using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebOptimizer;

internal class AssetResponseStore : IAssetResponseStore
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AssetResponseStore> _logger;
    private readonly WebOptimizerOptions _options = new();

    public AssetResponseStore(ILogger<AssetResponseStore> logger, IWebHostEnvironment env, IConfigureOptions<WebOptimizerOptions> options)
    {
        _logger = logger;
        _env = env;
        options.Configure(_options);
    }

    public async Task AddAsync(string bucket, string cachekey, AssetResponse assetResponse)
    {
        string name = CleanName(bucket);
        _ = Directory.CreateDirectory(_options.CacheDirectory);

        // First delete old cached files
        var oldCachedFiles = Directory.EnumerateFiles(_options.CacheDirectory, $"{name}__*.cache");
        foreach (string oldFile in oldCachedFiles)
        {
            await DeleteFileAsync(oldFile).ConfigureAwait(false);
        }

        // Then serialize to disk
        string json = JsonSerializer.Serialize(assetResponse);
        string filePath = GetPath(bucket, cachekey);
        await WriteFileAsync(filePath, json).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string bucket, string cachekey)
    {
        string filePath = GetPath(bucket, cachekey);
        await DeleteFileAsync(filePath);
    }

    public bool TryGet(string bucket, string cachekey, [NotNullWhen(true)] out AssetResponse? assetResponse)
    {
        assetResponse = null;
        string filePath = GetPath(bucket, cachekey);

        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                assetResponse = ParseJson(json);
                //TODO: Simplify this when System.Text.Json is fully baked
                //assetResponse = JsonSerializer.Deserialize<AssetResponse>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                DeleteFileAsync(filePath).GetAwaiter().GetResult();
            }
        }

        return assetResponse is not null;
    }

    internal AssetResponse? ParseJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            string? ck = document.RootElement.GetProperty("CacheKey").GetString();
            string? b = document.RootElement.GetProperty("Body").GetString();
            byte[]? bytes = JsonSerializer.Deserialize<byte[]>($"\"{b}\"");
            var ar = new AssetResponse(bytes, ck);
            string headersString = document.RootElement.GetProperty("Headers").GetRawText();
            if (!string.IsNullOrEmpty(headersString))
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersString) ?? [];
                foreach (var d in headers)
                {
                    ar.Headers.Add(d.Key, d.Value);
                }
            }

            return ar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred parsing the AssetResponse");
            return null;
        }
    }

    private static string CleanName(string route) => route.Replace('\\', '/').Replace('/', '\'');

    private static async Task DeleteFileAsync(string filePath, int attempts = 5)
    {
        await TryAsync(
            attempts,
            async () =>
            {
                await Task.Run(() => File.Delete(filePath));
            }).ConfigureAwait(false);
    }

    private string GetPath(string bucket, string cachekey)
    {
        // cachekey is Base64 encoded which uses / as one of the characters. So for Linux we need to
        // clean both the bucket and the cachekey.
        return Path.Combine(_options.CacheDirectory, CleanName($"{bucket}__{cachekey}.cache"));
    }

    private static async Task TryAsync(int attempts, Func<Task> callback)
    {
        try
        {
            await callback().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.Write(ex);
            if (attempts > 0)
            {
                await Task.Delay(10).ConfigureAwait(false);
                attempts--;
                await TryAsync(attempts, callback).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteFileAsync(string filePath, string content, int attempts = 5)
    {
        await TryAsync(attempts, async () =>
        {
            using var writer = new StreamWriter(filePath);
            await writer.WriteAsync(content).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
