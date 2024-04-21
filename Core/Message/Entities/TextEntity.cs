namespace SilhouetteDance.Core.Message.Entities;

public class TextEntity : MessageEntity
{
    public TextEntity(string text) => Text = text;
    public string Text { get; }
}