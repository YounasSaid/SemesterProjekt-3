namespace AppServer.Utils;

/// <summary>
/// BCrypt implementering af password hashing.
/// Bruger BCrypt algoritme til sikker password hashing og verificering.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
