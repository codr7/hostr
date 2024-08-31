namespace Hostr.Domain;

public static class Event
{
    public struct Insert : Type
    {
        public Insert(string id, DB.Table table)
        {
            Id = id;
            Table = table;
        }

        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data)
        {
            return Table.Insert(ref data, cx, cx.DBCx);
        }

        public readonly string Id { get; }
        public readonly DB.Table Table { get; }
    }

    public struct Update : Type
    {
        public Update(string id, DB.Table table)
        {
            Id = id;
            Table = table;
        }

        public DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data)
        {
            if (key is null) { throw new Exception("Null key"); }
            var rec = Table.FindFirst((DB.Record)key, cx.DBCx);

            if (rec is DB.Record r)
            {
                r.Update(data);
                return Table.Update(ref r, cx, cx.DBCx);
            }

            throw new Exception($"Record not found: {key}");
        }

        public readonly string Id { get; }
        public readonly DB.Table Table { get; }
    }

    public interface Type
    {
        DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data);
        string Id { get; }
        DB.Table Table { get; }
    }
}