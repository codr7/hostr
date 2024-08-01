namespace Hostr;

public static class Pools
{
    public struct Insert : Events.Type
    {
        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            return cx.DB.Pools.Insert(ref data, tx);
        }

        public string Id => "InsertPool";

        public DB.Table Table(Cx cx) => cx.DB.Pools;
    }

    public static readonly Insert INSERT = new Insert();

    public struct Update : Events.Type
    {
        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx)
        {
            if (key is null) { throw new Exception("Null pool key"); }
            var rec = cx.DB.Pools.FindFirst((DB.Record)key, tx);

            if (rec is DB.Record r)
            {
                r.Update(data);
                return cx.DB.Users.Update(ref r, tx);
            }

            throw new Exception($"Pool not found: {key}");
        }

        public string Id => "UpdatePool";
        public DB.Table Table(Cx cx) => cx.DB.Pools;
    }

    public static readonly Update UPDATE = new Update();

    public static DB.Record MakePool(this Schema db, string name = "")
    {
        var p = new DB.Record();
        p.Set(db.PoolName, name);
        return p;
    }
}