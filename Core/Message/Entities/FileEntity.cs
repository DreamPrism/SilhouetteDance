namespace SilhouetteDance.Core.Message.Entities;

public class FileEntity : IMessageEntity
{
    public string FileName { get; set; }

    public Stream Data { get; set; }

    public FileEntity(string path)
    {
        Data = new FileStream(path, FileMode.Open);
        FileName = Path.GetFileName(path);
    }

    public FileEntity(byte[] data, string fileName)
    {
        Data = new MemoryStream(data);
        FileName = fileName;
    }

    public string ToPreviewString() => $"[File] {FileName} ({Data.Length / 1024.0 / 1024.0:F1}MB)";
    public string ToPreviewText() => $"[文件] {FileName}";
}