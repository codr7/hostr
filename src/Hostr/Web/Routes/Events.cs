namespace Hostr.Web.Routes;

public struct Events : Route
{
    public bool Auth => !Config.Dev;
    public Method Method => Method.Get;
    public string Path => "/events";
    public IEndpointFilter[] Filters => [new CxFilter()];

    public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        var req = hcx.Request;
        req.Headers.TryGetValue("Authorization", out var auth);
        var userId = Users.ValidateJwtToken(cx, auth!);
        
        using var tx = cx.DBCx.StartTx();
#pragma warning disable CS8629 
        return Task.FromResult<object>(cx.DB.Events.FindAll(cx.DB.Users.PrimaryKey.Eq((DB.Record)cx.CurrentUser), tx));
#pragma warning restore CS8629
    }
}