namespace Hostr.Domain;

public static class Unit
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertUnit", "units");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateUnit", "units");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var u = new DB.Record();
        u.Set(cx.DB.UnitName, name);
        return u;
    }
}