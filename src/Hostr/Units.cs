namespace Hostr;

public static class Units
{
    public static readonly Events.Type INSERT = new Events.Insert("InsertUnit", "units");
    public static readonly Events.Type UPDATE = new Events.Update("UpdateUnit", "units");

    public static DB.Record MakeUnit(this Schema db, string name = "")
    {
        var u = new DB.Record();
        u.Set(db.UnitName, name);
        return u;
    }
}