using Hostr.Domain;

namespace Hostr.Web.Routes;

public struct Login : Route
{
    public readonly Method Method => Method.Post;
    public readonly string Path => "/login";
    public readonly IEndpointFilter[] Filters => [new CxFilter()];

    public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        var req = hcx.Request;
        var body = req.Json<ReqData>();
        using var tx = cx.DBCx.StartTx();
        var u = cx.Login(body.email, body.password, tx);
        tx.Commit();
        return Task.FromResult<object>(new ResData() { token = User.MakeJwtToken(cx, u) });
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