namespace GestaoColaboradores.API.Services.Common.Exceptions;

public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message) : base(message)
    {
    }
}
