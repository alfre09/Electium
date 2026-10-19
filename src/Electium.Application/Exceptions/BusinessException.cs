namespace Electium.Application.Exceptions;

/// <summary>Error de regla de negocio con un mensaje apto para mostrar al usuario.</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}
