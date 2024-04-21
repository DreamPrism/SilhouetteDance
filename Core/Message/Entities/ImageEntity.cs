using System.Numerics;

namespace SilhouetteDance.Core.Message.Entities;

public class ImageEntity : MessageEntity
{
    private static readonly HttpClient client=new ();
    public string Url { get; set; }
    
    public string Path { get; set; }
    
    public Vector2 ImageSize { get; set; }
    
    public byte[] Data { get; set; }
    
    public ImageEntity(string url, Vector2 imageSize)
    {
        Url = url;
        ImageSize = imageSize;
    }
    
    public ImageEntity(string path)
    {
        Path = path;
        Data = File.ReadAllBytes(path);
    }
    
    public ImageEntity(byte[] data, Vector2 imageSize)
    {
        Data = data;
        ImageSize = imageSize;
    }

    public async Task DownloadImageData() => Data = await client.GetByteArrayAsync(Url);
}