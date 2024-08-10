using Hostr.Domain;

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

        string? poolName = null;
        DateTime endAt;
        DateTime startAt;
        long interval;
        
        if (req.Get("poolName") is string pn && !pn.Equals("%")) { poolName = pn; }

        if (req.GetDateTime("startAt") is DateTime sa) { startAt = sa; }
        else { throw new Exception("Missnig startAt"); }

        if (req.GetDateTime("endAt") is DateTime ea) { endAt = ea; }
        else { throw new Exception("Missnig endAt"); }

        if (req.GetInt("interval") is int it) { interval = it; }
        else { throw new Exception("Missnig interval"); }

        var rs = Calendar.Get(cx, startAt, endAt, tx, poolName: poolName);
        var intervals = new List<DateTime>();
        DateTime t = startAt;

        while (t.CompareTo(endAt) < 0)
        {
            intervals.Add(t);
            t = t.AddMinutes(interval);
        }

        var calendars = new Dictionary<long, ResData.Calendar>();
        var capacity = new List<ResData.Capacity>();
        t = startAt;
        
        var total = int.MaxValue;
        var used = int.MinValue;

        for (var i = 0; i < rs.Length; i++)
        {
            var r = rs[i];
            var poolId = r.Get(cx.DB.PoolId);
            total = Math.Min(r.Get(cx.DB.CalendarTotal), total);
            used = Math.Max(r.Get(cx.DB.CalendarUsed), used);

            while (t.CompareTo(endAt) < 0 && t.CompareTo(r.Get(cx.DB.CalendarEndsAt)) < 0)
            {
                capacity.Add(new ResData.Capacity()
                {
                    interval = t,
                    total = total,
                    used = used
                });

                t = t.AddMinutes(interval);
            }

            if (i == rs.Length - 1 || (rs[i + 1].Get(cx.DB.PoolId) != poolId))
            {
#pragma warning disable CS8601 
                calendars[poolId] = new ResData.Calendar()
                {
                    pool = new ResData.Pool
                    {
                        id = poolId,
                        name = r.Get(cx.DB.PoolName),
                    },
                    capacity = capacity.ToArray()
                };
#pragma warning restore CS8601
                t = startAt;
                capacity.Clear();
                total = int.MaxValue;
                used = int.MinValue;
            }
        }

        var result = new ResData()
        {
            intervals = intervals.ToArray(),
            calendars = calendars.Values.ToArray()
        };

        return Task.FromResult<object>(result);
    }

    private struct ResData
    {
        public required DateTime[] intervals { get; set; }
        public required Calendar[] calendars { get; set; }

        public struct Calendar
        {
            public required Pool pool { get; set; }
            public required Capacity[] capacity { get; set; }
        }

        public struct Pool
        {
            public required long id { get; set; }
            public required string name { get; set; }
        }

        public struct Capacity
        {
            public required DateTime interval { get; set; }
            public required int total { get; set; }
            public required int used { get; set; }
        }
    }
}