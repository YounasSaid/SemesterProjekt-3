namespace AppServer.Utils;

/// <summary>
/// Interface for password hashing services.
/// Definerer kontrakt for hash og verificering af passwords.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
