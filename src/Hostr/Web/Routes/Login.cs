using Hostr.Domain;

namespace Hostr.Web.Routes;

public struct Login : Route
{
    public readonly Method Method => Method.Post;
    public readonly string Path => "/login";
    public readonly IEndpointFilter[] Filters => [new CxFilter()];

    async public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        var req = hcx.Request;
        var stream = new StreamReader(req.Body);
        var body = await stream.ReadToEndAsync();
        var data = cx.Json.FromString<ReqData>(body)!;
        using var tx = cx.DBCx.StartTx();
        var u = cx.Login(data.email, data.password, tx);
        tx.Commit();
        return new ResData() { token = User.MakeJwtToken(cx, u) };
    }

    private struct ReqData
    {
        public required string email { get; set; }
        public required string password { get; set; }
    }

    private struct ResData
    {
        public required string token { get; set; }
    }
}