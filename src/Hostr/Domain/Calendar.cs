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
        c.Set(cx.DB.CalendarBooked, 0);
        c.Set(cx.DB.CalendarTotal, 0);
        return c;
    }
}