using static Hostr.DB.ValueExtensions;

namespace Hostr.Web.Routes;

public struct GetCalendars : Route
{
    public readonly bool Auth => true;
    public readonly Method Method => Method.Get;
    public readonly string Path => "/calendars";
    public readonly IEndpointFilter[] Filters => [new CxFilter(), new UserFilter()];

    public Task<object> Exec(HttpContext hcx)
    {
        var cx = (Cx)hcx.Items["cx"]!;
        using var tx = cx.DBCx.StartTx();
        HttpRequest req = hcx.Request;

        var q = new DB.Query(cx.DB.Calendars).
            Join(cx.DB.CalendarPool).
            Select(cx.DB.Calendars.Columns).
            Select(cx.DB.PoolName).
            OrderBy(cx.DB.PoolName).
            OrderBy(cx.DB.CalendarStartsAt);

        if (req.GetLong("poolId") is long pid) { q.Where(cx.DB.PoolId.Eq(pid)); }
        if (req.GetDateTime("startAt") is DateTime sa) { q.Where(cx.DB.CalendarEndsAt.Gt(sa)); }
        if (req.GetDateTime("endAt") is DateTime ea) { q.Where(cx.DB.CalendarStartsAt.Lt(ea)); }
 
        var rs = q.FindAll(tx);        
        Array.Sort(rs, (x, y) => x.Get(cx.DB.EventId).CompareTo(y.Get(cx.DB.EventId)));
        return Task.FromResult<object>(rs);
    }
}