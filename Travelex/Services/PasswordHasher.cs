using System.Globalization;
using System.Security.Cryptography;

namespace Travelex.Services;

internal static class PasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"$TRAVELEX${Algorithm}${Iterations.ToString(CultureInfo.InvariantCulture)}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedValue, out bool needsUpgrade)
    {
        needsUpgrade = false;

        if (!storedValue.StartsWith("$TRAVELEX$", StringComparison.Ordinal))
        {
            needsUpgrade = FixedTimeEquals(password, storedValue);
            return needsUpgrade;
        }

        var parts = storedValue.Split('$', StringSplitOptions.None);
        if (parts.Length != 6 || !string.Equals(parts[2], Algorithm, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[3], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) ||
            iterations <= 0 || iterations > 1_000_000)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[4]);
            var expectedHash = Convert.FromBase64String(parts[5]);
            if (salt.Length < 8 || expectedHash.Length < 16) return false;

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            needsUpgrade = iterations < Iterations;
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(left));
        var rightHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(right));
        return CryptographicOperations.FixedTimeEquals(leftHash, rightHash);
    }
}
