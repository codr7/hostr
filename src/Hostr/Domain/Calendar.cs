using Hostr.DB;
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
                c.Set(cx.DB.CalendarTotal, 0);
                return c;
        }

        public static DB.Record[] Get(Cx cx, DateTime startAt, DateTime endAt, Tx tx, string? poolName = null)
        {
                var q = new DB.Query(cx.DB.Calendars).
                    Join(cx.DB.CalendarPool).
                    Select(cx.DB.Calendars.Columns).
                    Select(cx.DB.PoolId, cx.DB.PoolName, cx.DB.PoolHasInfiniteCapacity).
                    Where(cx.DB.PoolIsVisible.Eq(true)).
                    Where(cx.DB.CalendarEndsAt.Gt(startAt)).
                    Where(cx.DB.CalendarStartsAt.Lt(endAt)).
                    OrderBy(cx.DB.PoolName).
                    OrderBy(cx.DB.CalendarStartsAt);

                if (poolName is not null) { q.Where(cx.DB.PoolName.Like(poolName!)); }

                return q.FindAll(tx);
        }
}