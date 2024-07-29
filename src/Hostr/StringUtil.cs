public static class StringUtil
{
    public static string Capitalize(this string s) =>
        s switch
        {
            null =>
              throw new ArgumentNullException(nameof(s)),
            "" =>
              throw new ArgumentException($"{nameof(s)} cannot be empty"),
            _ =>
              string.Concat(s[0].ToString().ToUpper(), s.AsSpan(1))
        };
}