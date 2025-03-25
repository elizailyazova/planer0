using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    // Хеширование пароля с использованием SHA256
    public static string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte byteValue in bytes)
            {
                builder.Append(byteValue.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
