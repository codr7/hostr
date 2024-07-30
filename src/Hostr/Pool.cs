namespace Hostr;

public static class Pool
{
    public static DB.Record MakePool(this Schema db, string name = "")
    {
        var p = new DB.Record();
        p.Set(db.PoolName, name);
        return p;
    }
}