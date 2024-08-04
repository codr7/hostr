using Microsoft.Extensions.Primitives;

namespace Hostr.Web.Routes;

public struct Events : Route
{
    public bool Auth => !Config.Dev;
    public Method Method => Method.Get;
    public string Path => "/events";
    public IEndpointFilter[] Filters => [new CxFilter(), new UserFilter()];

    public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        using var tx = cx.DBCx.StartTx();
#pragma warning disable CS8629 
        var q = new DB.Query(cx.DB.Events).
            Select(cx.DB.Events.Columns).
            Where(cx.DB.EventPostedBy.Eq((DB.Record)cx.CurrentUser)).
            OrderBy(cx.DB.EventPostedAt, DB.Query.Order.Descending);
        HttpRequest req = hcx.Request;

        var ro = req.Query["rowOffset"];
#pragma warning disable CS8604 .
        if (ro != StringValues.Empty) { q.Offset(Int64.Parse(ro[0])); }
#pragma warning restore CS8604 

        var rl = req.Query["rowLimit"];
#pragma warning disable CS8604 
        if (rl != StringValues.Empty) { q.Limit(Int64.Parse(rl[0])); }
#pragma warning restore CS8604

        var rs = q.FindAll(tx);        
        Array.Sort(rs, (x, y) => x.Get(cx.DB.EventId).CompareTo(y.Get(cx.DB.EventId)));
        return Task.FromResult<object>(rs);
#pragma warning restore CS8629
    }
}