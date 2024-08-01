namespace Hostr.Web.Routes;

public class Login: Route
{
    private struct Data
    {
        public required string email { get; set; }
        public required string password { get; set; }
    }

    public Login(): base(Method.Post, "/login", new CxFilter()) {}

    async public override Task<object> Exec(HttpContext hcx) {
        var cx = (Cx)hcx.Items["cx"]!;
        var request = hcx.Request;
        var stream = new StreamReader(request.Body);
        var body = await stream.ReadToEndAsync();
        var data = cx.Json.FromString<Data>(body)!;
        using var tx = cx.DBCx.StartTx();
        var u = cx.Login(data.email, data.password, tx);
        return Users.MakeJwtToken(cx, u);
    }
}