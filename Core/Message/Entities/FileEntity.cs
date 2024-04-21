namespace SilhouetteDance.Core.Message.Entities;

public class FileEntity : MessageEntity
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
}