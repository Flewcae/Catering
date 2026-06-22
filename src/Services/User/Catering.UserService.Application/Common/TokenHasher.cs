using System.Security.Cryptography;
using System.Text;

namespace Catering.UserService.Application.Common;

public static class TokenHasher
{
    public static string Hash(string rawValue) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawValue)));
}
