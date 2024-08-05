using static Hostr.DB.ValueExtensions;

namespace Hostr.Web.Routes;

public struct Events : Route
{
    public bool Auth => true;
    public Method Method => Method.Get;
    public string Path => "/events";
    public IEndpointFilter[] Filters => [new CxFilter(), new UserFilter()];

    public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        using var tx = cx.DBCx.StartTx();
        HttpRequest req = hcx.Request;

        var q = new DB.Query(cx.DB.Events).
            Select(cx.DB.Events.Columns).
#pragma warning disable CS8629 
            Where(cx.DB.EventPostedBy.Eq((DB.Record)cx.CurrentUser)).
#pragma warning restore CS8629
            OrderBy(cx.DB.EventPostedAt, DB.Query.Order.Descending);

        if (req.GetDateTime("postedBefore") is DateTime pb) { q.Where(cx.DB.EventPostedAt.Lt(pb)); }
        if (req.GetDateTime("postedAfter") is DateTime pa) { q.Where(cx.DB.EventPostedAt.Gt(pa)); }
        if (req.GetLong("rowOffset") is long ro) { q.Offset(ro); }
        if (req.GetLong("rowLimit") is long rl) { q.Limit(rl); }
 
        var rs = q.FindAll(tx);        
        Array.Sort(rs, (x, y) => x.Get(cx.DB.EventId).CompareTo(y.Get(cx.DB.EventId)));
        return Task.FromResult<object>(rs);
    }
}