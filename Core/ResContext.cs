using System.IO.Compression;
using Microsoft.Extensions.Configuration;

namespace Lagrange.Core;

/// <summary>
/// Resources Context, inject this class to your service in order to access resources, inspired by Microsoft.Extensions.Configuration
/// </summary>
public class ResContext
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;

    public ResContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient();
    }

    public byte[] this[string key] => Get(key);

    public bool Exists(string key) => File.Exists(ResolveKey(key));

    private byte[] Get(string key)
    {
        string path = ResolveKey(key);
        return File.ReadAllBytes(path);
    }
    

    public async Task<byte[]> GetWebRes(string key, string url)
    {
        string path = ResolveKey(key);
        var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(path, bytes);
        return bytes;
    }
    
    public async Task SaveFile(string key, byte[] bytes)
    {
        string directory = ResolveKey(key);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(Path.GetDirectoryName(directory) ?? throw new NullReferenceException("Directory name is null."));
        await File.WriteAllBytesAsync(directory, bytes);
    }
    
    public async Task<byte[]> CompressFolder(string key)
    {
        string directory = ResolveKey(key);
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("Directory not found.");
        string zipPath = Path.Combine(Path.GetDirectoryName(directory) ?? throw new NullReferenceException("Directory name is null."), $"{key.Split(':')[^1]}.zip");
        ZipFile.CreateFromDirectory(directory, zipPath);
        return await File.ReadAllBytesAsync(zipPath);
    }

    public bool IsExists(string key) => File.Exists(ResolveKey(key));

    public string ResolvePath(string key) => ResolveKey(key);

    private string ResolveKey(string key)
    {
        var baseDir = new DirectoryInfo(_configuration["Generic:AssetsDirectory"] ?? throw new NullReferenceException("AssetsDirectory is not set."));
        key = key.Replace(':', Path.DirectorySeparatorChar);
        return Path.Combine(baseDir.FullName, key);
    }
}