namespace Hostr.Domain;

public static class Pool
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertPool", "pools");
    public static readonly Event.Type UPDATE = new Event.Update("UpdatePool", "pools");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var p = new DB.Record();
        p.Set(cx.DB.PoolName, name);
        return p;
    }
}