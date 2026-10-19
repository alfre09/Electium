namespace Electium.Application.Helpers;

public static class DocumentHelper
{
    public const int DocumentLength = 11;

    /// <summary>Deja solo los dígitos del documento (quita guiones y espacios).</summary>
    public static string Normalize(string? document)
        => new string((document ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Formato 000-0000000-0.</summary>
    public static string Format(string? document)
    {
        var digits = Normalize(document);
        return digits.Length == DocumentLength
            ? $"{digits[..3]}-{digits.Substring(3, 7)}-{digits[10]}"
            : digits;
    }
}
