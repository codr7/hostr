using static Hostr.DB.ValueExtensions;

namespace Hostr.Domain;

public static class Calendar
{
        public static readonly Event.Type INSERT = new Event.Insert("InsertCalendar", "calendars");
        public static readonly Event.Type UPDATE = new Event.Update("UpdateCalendar", "calendars");

        public static DB.Record Make(Cx cx, DB.Record pool)
        {
                var c = new DB.Record();
#pragma warning disable CS8629
                c.Set(cx.DB.CalendarUpdatedBy, (DB.Record)cx.CurrentUser);
#pragma warning restore CS8629
                c.Set(cx.DB.CalendarPool, pool);
                c.Set(cx.DB.CalendarStartsAt, DateTime.MinValue);
                c.Set(cx.DB.CalendarEndsAt, DateTime.MaxValue);
                c.Set(cx.DB.CalendarUsed, 0);
                c.Set(cx.DB.CalendarTotal, pool.Get(cx.DB.PoolCapacity));
                return c;
        }

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

                return q.FindAll(cx.DBCx.Tx!);
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

                var cs = q.FindAll(cx.DBCx.Tx!);
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
                        cx.DB.Calendars.Store(ref cc, cx, cx.DBCx.Tx!);
                }
        }
}