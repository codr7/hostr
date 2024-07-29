namespace Hostr;

using System.Security.Cryptography;

public static class Password
{
    public static readonly HashAlgorithmName HASH_ALGO = HashAlgorithmName.SHA256;
    public static readonly int HASH_LENGTH = 32;
    public static readonly int SALT_LENGTH = 16;
    public static readonly string TAG = "HOSTR";
    public static readonly int VERSION = 1;

    public static string Hash(string password, int iters)
    {
        /* Static RandomNumberGenerator methods are expected to be thread safe. */
        var s = RandomNumberGenerator.GetBytes(SALT_LENGTH);
        var h = GetHash(password, s, iters);
        var bs = new byte[SALT_LENGTH + HASH_LENGTH];
        Array.Copy(s, 0, bs, 0, SALT_LENGTH);
        Array.Copy(h, 0, bs, SALT_LENGTH, HASH_LENGTH);
        var b64 = Convert.ToBase64String(bs);
        return $"{TAG}:{VERSION}:{iters}:{b64}";
    }

    public static bool Check(string expected, string actual)
    {
        var ps = expected.Split(':');
        if (ps[0] != TAG) { throw new Exception("Invalid tag"); }
        var version = int.Parse(ps[1]);
        if (version != 1) { throw new Exception("Invalid version"); }
        var iters = int.Parse(ps[2]);
        var b64 = ps[3];
        var bs = Convert.FromBase64String(b64);
        var s = new byte[SALT_LENGTH];
        Array.Copy(bs, 0, s, 0, SALT_LENGTH);
        var h = GetHash(actual, s, iters);
        return bs.AsSpan(SALT_LENGTH, HASH_LENGTH).SequenceEqual(h.AsSpan());
    }

    private static byte[] GetHash(string password, byte[] salt, int iters) {
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iters, HASH_ALGO);
        return pbkdf2.GetBytes(HASH_LENGTH);
    }
}