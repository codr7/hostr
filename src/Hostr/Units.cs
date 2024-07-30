namespace Hostr;

public static class Units
{
    public static DB.Record MakeUnit(this Schema db, string name = "")
    {
        var u = new DB.Record();
        u.Set(db.UnitName, name);
        return u;
    }
}