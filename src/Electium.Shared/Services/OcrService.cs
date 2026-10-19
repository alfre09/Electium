using Electium.Application.Helpers;
using Electium.Application.Interfaces;
using Electium.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tesseract;

namespace Electium.Shared.Services;

public class OcrService : IOcrService
{
    private readonly OcrSettings _settings;
    private readonly ILogger<OcrService> _logger;

    public OcrService(IOptions<OcrSettings> settings, ILogger<OcrService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsEnabled => _settings.Enabled;

    public async Task<bool> DocumentMatchesAsync(Stream image, string document)
    {
        var expected = DocumentHelper.Normalize(document);
        if (expected.Length == 0)
            return false;

        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer);
        var bytes = buffer.ToArray();

        // Tesseract es sincrónico y consume CPU: se ejecuta fuera del hilo de la petición.
        var text = await Task.Run(() => ReadText(bytes));

        // Se comparan solo dígitos para tolerar guiones, espacios y saltos de línea.
        var digits = DocumentHelper.Normalize(text);
        return digits.Contains(expected, StringComparison.Ordinal);
    }

    private string ReadText(byte[] imageBytes)
    {
        var path = Path.IsPathRooted(_settings.TessDataPath)
            ? _settings.TessDataPath
            : Path.Combine(AppContext.BaseDirectory, _settings.TessDataPath);

        try
        {
            using var engine = new TesseractEngine(path, _settings.Language, EngineMode.Default);
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(pix);
            return page.GetText() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leyendo la imagen con Tesseract. TessDataPath={Path}, Language={Lang}", path, _settings.Language);
            throw new InvalidOperationException(
                "No se pudo procesar la imagen del documento. Verifique que sea una foto nítida (JPG o PNG).", ex);
        }
    }
}
