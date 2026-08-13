using GestaoColaboradores.API.Services.Common;

namespace GestaoColaboradores.API.Tests;

public class CodigoSequencialGeneratorTests
{
    [Theory]
    [InlineData(1, "USR", "USR-000001")]
    [InlineData(42, "COL", "COL-000042")]
    [InlineData(123456, "UNI", "UNI-123456")]
    [InlineData(7, "UNI", "UNI-000007")]
    public void Formatar_GeraPrefixoENumeroComSeisDigitos(long numero, string prefixo, string esperado)
    {
        var resultado = CodigoSequencialGenerator.Formatar(numero, prefixo);

        Assert.Equal(esperado, resultado);
    }
}
