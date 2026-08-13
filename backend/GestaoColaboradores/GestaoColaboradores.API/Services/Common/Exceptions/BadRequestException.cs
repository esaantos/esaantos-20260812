namespace GestaoColaboradores.API.Services.Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
