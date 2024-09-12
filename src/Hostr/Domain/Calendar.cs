namespace Hostr.Domain;

using static Hostr.DB.ValueExtensions;

public class Calendar : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Calendar", Schema.Instance.Calendars);
    public static Event.Type UPDATE => new Event.Update("Update Calendar", Schema.Instance.Calendars);

    public static DB.Record[] Get(Cx cx, DateTime startAt, DateTime endAt, string? poolName = null)
    {
        var q = new DB.Query(cx.DB.Calendars).
            Join(cx.DB.CalendarPool).
            Select(cx.DB.Calendars.Columns).
            Select(cx.DB.PoolId, cx.DB.PoolName).
            Where(cx.DB.PoolIsVisible.Eq(true)).
            Where(cx.DB.CalendarEndsAt.Gt(startAt)).
            Where(cx.DB.CalendarStartsAt.Lt(endAt)).
            OrderBy(cx.DB.PoolName).
            OrderBy(cx.DB.CalendarStartsAt);

        if (poolName is not null) { q.Where(cx.DB.PoolName.Like(poolName!)); }

        return q.FindAll(cx.DBCx);
    }

    public static void Update(Cx cx, DB.Record pool, DateTime startAt, DateTime endAt, int total = 0, int used = 0)
    {
        var q = new DB.Query(cx.DB.Calendars).
                            Join(cx.DB.CalendarPool).
                            Select(cx.DB.Calendars.Columns).
                            Where(cx.DB.CalendarEndsAt.Gt(startAt)).
                            Where(cx.DB.CalendarStartsAt.Lt(endAt)).
                            OrderBy(cx.DB.PoolId).
                            OrderBy(cx.DB.CalendarStartsAt);

        var cs = q.FindAll(cx.DBCx);
        var result = new List<DB.Record>();

        for (var i = 0; i < cs.Length; i++)
        {
            var c = cs[i];

            if (c.Get(cx.DB.CalendarStartsAt).CompareTo(startAt) < 0)
            {
                var prefix = c;
                c = new DB.Record();
                prefix.Copy(ref c, cx.DB.CalendarPool.Columns);
                prefix.Copy(ref c, [cx.DB.CalendarStartsAt, cx.DB.CalendarTotal, cx.DB.CalendarUsed]);
                prefix.Set(cx.DB.CalendarEndsAt, startAt);
                c.Set(cx.DB.CalendarStartsAt, startAt);
                result.Add(prefix);
            }

            result.Add(c);

            if (c.Get(cx.DB.CalendarEndsAt).CompareTo(endAt) > 0)
            {
                var suffix = new DB.Record();
                c.Copy(ref suffix, cx.DB.CalendarPool.Columns);
                c.Copy(ref suffix, [cx.DB.CalendarEndsAt, cx.DB.CalendarTotal, cx.DB.CalendarUsed]);
                suffix.Set(cx.DB.CalendarStartsAt, endAt);
                c.Set(cx.DB.CalendarEndsAt, endAt);
                result.Add(suffix);
            }

            if (total != 0) { c.Set(cx.DB.CalendarTotal, c.Get(cx.DB.CalendarTotal) + total); }
            if (used != 0) { c.Set(cx.DB.CalendarUsed, c.Get(cx.DB.CalendarUsed) + used); }
        }

        foreach (var c in result)
        {
            var cc = c;
            cx.DB.Calendars.Store(ref cc, cx, cx.DBCx);
        }
    }

    public Calendar(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Calendar(Cx cx, Pool pool, DateTime? startsAt = null, DateTime? endsAt = null) : base(cx)
    {
        Record
            .Set(cx.DB.CalendarPool, pool.Record)
            .Set(cx.DB.CalendarStartsAt, startsAt ?? DateTime.MinValue)
            .Set(cx.DB.CalendarEndsAt, endsAt ?? DateTime.MaxValue)
            .Set(cx.DB.CalendarUpdatedBy, cx.CurrentUser!.Record)
            .Set(cx.DB.CalendarUsed, 0)
            .Set(cx.DB.CalendarTotal, pool.Capacity);
    }

    public Pool Pool => new Pool(Cx, Record.Copy(Cx.DB.CalendarPool));

    public DateTime StartsAt
    {
        get => Record.Get(Cx.DB.CalendarStartsAt);
        set => Record.Set(Cx.DB.CalendarStartsAt, value);
    }

    public DateTime EndsAt
    {
        get => Record.Get(Cx.DB.CalendarEndsAt);
        set => Record.Set(Cx.DB.CalendarEndsAt, value);
    }

    public DateTime UpdatedAt => Record.Get(Cx.DB.CalendarUpdatedAt);
    public User UpdatedBy => new User(Cx, Record.Copy(Cx.DB.CalendarUpdatedBy));

    public int Total
    {
        get => Record.Get(Cx.DB.CalendarTotal);
        set => Record.Set(Cx.DB.CalendarTotal, value);
    }

    public int Used
    {
        get => Record.Get(Cx.DB.CalendarUsed);
        set => Record.Set(Cx.DB.CalendarUsed, value);
    }

    public override DB.Table[] Tables => [Cx.DB.Calendars];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}