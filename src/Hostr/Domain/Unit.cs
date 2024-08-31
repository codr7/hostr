namespace Hostr.Domain;

public static class Unit
{
    public static readonly Event.Type INSERT = new Event.Insert("Insert Unit", Schema.Instance.Units);
    public static readonly Event.Type UPDATE = new Event.Update("Update Unit", Schema.Instance.Units);

    public static DB.Record Make(Cx cx, string name = "")
    {
        var u = new DB.Record();
        u.Set(cx.DB.UnitId, cx.DB.PoolIds.Next(cx.DBCx));
        u.Set(cx.DB.PoolName, name);
        u.Set(cx.DB.PoolCreatedBy, cx.CurrentUser!.Record);
        return u;
    }
}