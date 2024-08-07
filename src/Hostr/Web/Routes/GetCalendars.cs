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
            Select(cx.DB.PoolId, cx.DB.PoolName, cx.DB.PoolHasInfiniteCapacity).
            Where(cx.DB.PoolIsVisible.Eq(true)).
            OrderBy(cx.DB.PoolName).
            OrderBy(cx.DB.CalendarStartsAt);

        DateTime endAt;
        DateTime startAt;
        long interval;

        if (req.GetLong("poolId") is long pid) { q.Where(cx.DB.PoolId.Eq(pid)); }
        if (req.GetDateTime("startAt") is DateTime sa)
        {
            startAt = sa;
            q.Where(cx.DB.CalendarEndsAt.Gt(sa));
        }
        else { throw new Exception("Missnig startAt"); }

        if (req.GetDateTime("endAt") is DateTime ea)
        {
            endAt = ea;
            q.Where(cx.DB.CalendarStartsAt.Lt(ea));
        }
        else { throw new Exception("Missnig endAt"); }

        if (req.GetInt("interval") is int it)
        {
            interval = it;
            q.Where(cx.DB.CalendarStartsAt.Lt(ea));
        }
        else { throw new Exception("Missnig interval"); }

        var rs = q.FindAll(tx);
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

        for (var i = 0; i < rs.Length; i++)
        {
            var r = rs[i];
            Console.WriteLine(r);
            var poolId = r.Get(cx.DB.PoolId);
                        
            while (t.CompareTo(endAt) < 0 && t.CompareTo(r.Get(cx.DB.CalendarEndsAt)) < 0)
            {
                capacity.Add(new ResData.Capacity()
                {
                    total = r.Get(cx.DB.CalendarTotal),
                    used = r.Get(cx.DB.CalendarUsed)
                });

                t = t.AddMinutes(interval);
            }

            if (i == rs.Length-1 || (rs[i + 1].Get(cx.DB.PoolId) != poolId))
            {
#pragma warning disable CS8601 
                calendars[poolId] = new ResData.Calendar()
                {
                    pool = new ResData.Pool {
                        id = poolId,
                        name = r.Get(cx.DB.PoolName),
                        hasInfiniteCapacity = r.Get(cx.DB.PoolHasInfiniteCapacity)
                    },
                    capacity = capacity.ToArray()
                };
#pragma warning restore CS8601
                t = startAt;
                capacity.Clear();
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
            public required bool hasInfiniteCapacity { get; set; }
        }

        public struct Capacity
        {
            public required int total { get; set; }
            public required int used { get; set; }
        }
    }
}