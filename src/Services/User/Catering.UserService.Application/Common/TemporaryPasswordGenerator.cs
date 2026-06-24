using System.Security.Cryptography;

namespace Catering.UserService.Application.Common;

public static class TemporaryPasswordGenerator
{
    private const string Charset = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
    private const int Length = 12;

    public static string Generate()
    {
        var chars = new char[Length];

        for (var i = 0; i < Length; i++)
        {
            chars[i] = Charset[RandomNumberGenerator.GetInt32(Charset.Length)];
        }

        return new string(chars);
    }
}
