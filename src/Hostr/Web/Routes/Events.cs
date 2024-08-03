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
        //var req = hcx.Request;
        using var tx = cx.DBCx.StartTx();
        return Task.FromResult<object>(cx.DB.Events.FindAll(null, tx));
    }
}