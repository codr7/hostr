namespace Hostr.Domain;

public static class Unit
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertUnit", "units");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateUnit", "units");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var u = new DB.Record();
        u.Set(cx.DB.UnitId, cx.DB.PoolIds.Next(cx.DBCx.Tx!));
        u.Set(cx.DB.PoolName, name);
#pragma warning disable CS8629 
        u.Set(cx.DB.PoolCreatedBy, (DB.Record)cx.CurrentUser);
#pragma warning restore CS8629
        return u;
    }
}