using System.Data.SqlTypes;
using System.Text;

namespace Hostr.DB;

public class Query : Source
{
    public enum Order { Ascending, Descending };
    private Source from;
    private long limit = -1;
    private long offset = -1;
    private readonly List<(Value, Order)> order = new List<(Value, Order)>();
    private readonly List<Value> select = new List<Value>();
    private Condition? where;

    public Query(Source from)
    {
        this.from = from;
    }

    public void AddArgs(List<object> result)
    {
        foreach (var v in select) { v.AddValueArgs(result); }
        from.AddSourceArgs(result);
        if (where is Condition w) { w.AddArgs(result); }
        foreach (var v in order) { v.Item1.AddValueArgs(result); }
    }

    public void AddSourceArgs(List<object> result) => AddArgs(result);

    public Record[] FindAll(Tx tx)
    {
        var args = new List<object>();
        AddArgs(args);
        using var reader = tx.ExecReader(Sql, args.ToArray());
        var result = new List<Record>();

        while (reader.Read())
        {
            var rec = new Record();

            for (var i = 0; i < select.Count; i++)
            {
                var v = select[i];
                if (!reader.IsDBNull(i)) { rec.SetObject(v, v.GetObject(reader, i)); }
            }

            result.Add(rec);
        }

        return result.ToArray();
    }

    public Query Limit(long n)
    {
        limit = n;
        return this;
    }
    
    public Query Offset(long n)
    {
        offset = n;
        return this;
    }

    public Query Select(params Value[] values)
    {
        foreach (var v in values) { select.Add(v); }
        return this;
    }

    public Query OrderBy(Column col, Order ord)
    {
        order.Add((col, ord));
        return this;
    }


    public string SourceSql => Sql;

    public string Sql
    {
        get
        {
            var sql = new StringBuilder();
            sql.Append($"SELECT {string.Join(", ", select.Select(v => v.ValueSql).ToArray())} FROM {from.SourceSql}");
            if (where is Condition w) { sql.Append($" WHERE {w.Sql}"); }

            if (order.Count > 0)
            {
                sql.Append($" ORDER BY {string.Join(", ", order.Select(v => $"{v.Item1} {((v.Item2 == Order.Ascending) ? "ASC" : "DESC")}").ToArray())}");
            }
            
            if (offset > -1) { sql.Append($" OFFSET {offset}"); }
            if (limit > -1) { sql.Append($" LIMIT {limit}"); }
            return sql.ToString();
        }
    }

    public Query Where(Condition cond)
    {
        where = (where is Condition w) ? w.And(cond) : cond;
        return this;
    }
}