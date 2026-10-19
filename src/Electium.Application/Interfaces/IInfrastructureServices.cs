namespace Electium.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public interface IOcrService
{
    /// <summary>True si el OCR está habilitado en la configuración.</summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Lee la imagen del documento y comprueba que contenga el número de
    /// documento indicado.
    /// </summary>
    Task<bool> DocumentMatchesAsync(Stream image, string document);
}
