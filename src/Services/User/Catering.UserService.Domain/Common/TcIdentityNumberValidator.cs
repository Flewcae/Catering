namespace Catering.UserService.Domain.Common;

public static class TcIdentityNumberValidator
{
    public static bool IsValid(string? tcIdentityNumber)
    {
        if (string.IsNullOrWhiteSpace(tcIdentityNumber) || tcIdentityNumber.Length != 11)
        {
            return false;
        }

        if (tcIdentityNumber[0] == '0')
        {
            return false;
        }

        var digits = new int[11];
        for (var i = 0; i < 11; i++)
        {
            if (!char.IsDigit(tcIdentityNumber[i]))
            {
                return false;
            }

            digits[i] = tcIdentityNumber[i] - '0';
        }

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        var digit10 = (oddSum * 7 - evenSum) % 10;
        if (digit10 < 0)
        {
            digit10 += 10;
        }

        if (digit10 != digits[9])
        {
            return false;
        }

        var digit11 = (oddSum + evenSum + digits[9]) % 10;

        return digit11 == digits[10];
    }
}
