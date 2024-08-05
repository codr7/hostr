using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace Hostr.Web;

public static class RequestExtensions
{
    public static string? Get(this HttpRequest req, string name)
    {
        var ro = req.Query[name];
        return (ro == StringValues.Empty) ? null : ro[0];
    }

    public static DateTime? GetDateTime(this HttpRequest req, string name) =>
        (req.Get(name) is string v) ? JsonSerializer.Deserialize<DateTime>($"\"{v}\"") : null;

    public static long? GetLong(this HttpRequest req, string name) => (req.Get(name) is string v) ? long.Parse(v) : null;
}