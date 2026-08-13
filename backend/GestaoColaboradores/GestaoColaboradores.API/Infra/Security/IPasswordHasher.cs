namespace GestaoColaboradores.API.Infra.Security;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verify(string senha, string hash);
}
