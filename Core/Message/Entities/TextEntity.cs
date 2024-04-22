namespace SilhouetteDance.Core.Message.Entities;

public class TextEntity : IMessageEntity
{
    public TextEntity(string text) => Text = text;
    public string Text { get; }
    public string ToPreviewString() => $"[Text]: {Text}";
    public string ToPreviewText() => Text;
}