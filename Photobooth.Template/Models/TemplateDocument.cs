using System.Text.Json.Serialization;

namespace Photobooth.Template.Models;

public class TemplateDocument
{
    public string Name { get; set; } = "Default";
    public double WidthMm { get; set; } = 150;
    public double HeightMm { get; set; } = 100;
    public int Dpi { get; set; } = 300;
    public List<TemplateElement> Elements { get; set; } = new();
}

[JsonDerivedType(typeof(ImagePlaceholderElement), typeDiscriminator: "image")]
[JsonDerivedType(typeof(TextElement), typeDiscriminator: "text")]
[JsonDerivedType(typeof(QrElement), typeDiscriminator: "qr")]
[JsonDerivedType(typeof(RectangleElement), typeDiscriminator: "rect")]
public abstract class TemplateElement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 50;
    public double Height { get; set; } = 50;
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
}

public class ImagePlaceholderElement : TemplateElement
{
    public int PhotoIndex { get; set; }
    public string? SampleImagePath { get; set; }
}

public class TextElement : TemplateElement
{
    public string Text { get; set; } = "Sample Text";
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 24;
    public string Color { get; set; } = "#000000";
    public bool Bold { get; set; }
}

public class QrElement : TemplateElement
{
    public string Value { get; set; } = "{SessionId}";
}

public class RectangleElement : TemplateElement
{
    public string Fill { get; set; } = "#FFFFFF00";
    public string Stroke { get; set; } = "#FF000000";
    public double StrokeThickness { get; set; } = 1;
}
