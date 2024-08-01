namespace Hostr.Web;

public static class Login
{
    public class Data
    {
        public required string email { get; set; }
        public required string password { get; set; }
    }

    async public static Task<object> Handler(HttpContext http)
    {
#pragma warning disable CS8600
        var cx = (Cx)http.Items["cx"]!;
#pragma warning restore CS8600
        var request = http.Request;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();
        var data = cx.Json.FromString<Data>(body)!;
        using var tx = cx.DBCx.StartTx();
        var u = cx.Login(data.email, data.password, tx);
        return Users.MakeJwtToken(cx, u);
    }
}