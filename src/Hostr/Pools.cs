namespace Hostr;

public static class Pools
{
    public static readonly Events.Type INSERT = new Events.Insert("InsertPool", "pools");
    public static readonly Events.Type UPDATE = new Events.Update("UpdatePool", "pools");

    public static DB.Record MakePool(this Schema db, string name = "")
    {
        var p = new DB.Record();
        p.Set(db.PoolName, name);
        return p;
    }
}