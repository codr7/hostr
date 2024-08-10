namespace Hostr.Domain;

public static class Event
{
    public struct Insert : Type
    {
        public Insert(string id, string tableName)
        {
            this.id = id;
            this.tableName = tableName;
        }

        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data)
        {
            return Table(cx).Insert(ref data, cx, cx.DBCx.Tx!);
        }

#pragma warning disable CS8600
#pragma warning disable CS8603
        public DB.Table Table(Cx cx) => (DB.Table)cx.DB[TableName];
#pragma warning restore CS8603
#pragma warning restore CS8600

        public readonly string Id => id;
        public readonly string TableName => tableName;

        private readonly string id;
        private readonly string tableName;
    }

    public struct Update : Type
    {
        public Update(string id, string tableName)
        {
            this.id = id;
            this.tableName = tableName;
        }

        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data)
        {
            if (key is null) { throw new Exception("Null key"); }
            var t = Table(cx);
            var rec = t.FindFirst((DB.Record)key, cx.DBCx.Tx!);

            if (rec is DB.Record r)
            {
                r.Update(data);
                return t.Update(ref r, cx, cx.DBCx.Tx!);
            }

            throw new Exception($"Record not found: {key}");
        }

#pragma warning disable CS8600
#pragma warning disable CS8603
        public DB.Table Table(Cx cx) => (DB.Table)cx.DB[TableName];
#pragma warning restore CS8603
#pragma warning restore CS8600

        public readonly string Id => id;
        public readonly string TableName => tableName;
        private readonly string id;
        private readonly string tableName;
    }

    public interface Type
    {
        DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data);
        string Id { get; }
        DB.Table Table(Cx cx);
    }
}