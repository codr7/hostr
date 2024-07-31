namespace Hostr;

public static class Units
{
    public struct Insert : Events.Type
    {
        public void Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            cx.DB.Units.Insert(ref data, tx);
        }

        public string Id => "InsertUnit";

        public DB.Table Table(Cx cx) => cx.DB.Units;
    }

    public static readonly Insert INSERT = new Insert();

    public struct Update : Events.Type
    {
        public void Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            if (key is null) { throw new Exception("Null unit key"); }
            var rec = cx.DB.Units.Find((DB.Record)key, tx);

            if (rec is DB.Record r)
            {
                r.Update(data);
                cx.DB.Units.Update(ref r, tx);
            }
            else
            {
                throw new Exception($"Unit not found: {key}");
            }
        }

        public string Id => "UpdateUnit";
        public DB.Table Table(Cx cx) => cx.DB.Pools;
    }

    public static readonly Update UPDATE = new Update();


    public static DB.Record MakeUnit(this Schema db, string name = "")
    {
        var u = new DB.Record();
        u.Set(db.UnitName, name);
        return u;
    }
}