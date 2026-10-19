namespace Electium.Shared.Settings;

public class EmailSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "Electium";
}

public class OcrSettings
{
    public bool Enabled { get; set; } = true;

    /// <summary>Carpeta con los archivos .traineddata (relativa al ejecutable o absoluta).</summary>
    public string TessDataPath { get; set; } = "tessdata";

    /// <summary>Idiomas de Tesseract, por ejemplo "eng" o "spa+eng".</summary>
    public string Language { get; set; } = "eng";
}
