using System.Text;

namespace Hostr.DB;

using RecordId = ulong;

public struct Record
{
    private static RecordId nextId = 0;
    public static RecordId NextId() => Interlocked.Increment(ref nextId);

    private readonly OrderedMap<Value, object> fields = new OrderedMap<Value, object>();

    public Record(RecordId id)
    {
        Id = id;
    }

    public Record()
    {
        Id = NextId();
    }

    public bool Contains(Column col) => fields.ContainsKey(col);

    public void Copy(ref Record to, (Column, Column)[] map, bool force = false)
    {
        foreach (var (fc, tc) in map)
        {
            if (GetObject(fc) is object v)
            {
                to.SetObject(tc, v);
            }
            else if (force)
            {
                throw new Exception($"Missing field: {fc}");
            }
        }
    }

    public Record Copy((Column, Column)[] map, bool force = false)
    {
        var result = new Record();
        Copy(ref result, map, force);
        return result;
    }

    public void Copy(ref Record to, Column[] cols) => Copy(ref to, cols.Zip(cols).ToArray());

    public Record Copy(Column[] cols, bool force = false)
    {
        var c = new Record();
        Copy(ref c, cols.Zip(cols).ToArray());
        return c;
    }

    public Condition Eq(Column[] columns) => Eq(columns.Zip(columns).ToArray());
    public Condition Eq((Column, Column)[] columns)
    {
        var conds = new List<Condition>();

        foreach (var (rc, cc) in columns)
        {
            if (GetObject(rc) is object v)
            {
                conds.Add(cc.Eq(v));
            }
            else
            {
                throw new Exception($"Missing value: {rc}");
            }
        }

        return Condition.And(conds.ToArray());
    }

    public bool Exists(Table table, Tx tx)
    {
        foreach (var c in table.Columns)
        {
            if (tx.GetStoredValue(Id, c) is not null) { return true; }
        }

        return false;
    }

    public (Value, object)[] Fields => fields.Items;

    public T? Get<T>(TypedColumn<T> col) => (GetObject(col) is object v) ? (T)v : default;
    public object? GetObject(Value col) => fields[col];

    public T? GetStored<T>(TypedColumn<T> col, Cx cx) => (cx.Tx!.GetStoredValue(Id, col) is T v) ? v : default;

    public readonly RecordId Id;
    public Record Set<T>(TypedColumn<T> col, T value) => SetObject(col, value);

    public Record Set(ForeignKey key, Record rec)
    {
        foreach (var (c, fc) in key.ColumnMap)
        {
            if (rec.GetObject(fc) is object v)
            {
                SetObject(c, v);
            }
            else
            {
                throw new Exception($"Missing key: {fc}");
            }
        }

        return this;
    }

    public Record SetObject(Value col, object? value)
    {
        fields[col] = value;
        return this;
    }

    public override string ToString()
    {
        var buf = new StringBuilder();
        buf.Append('{');

        var i = 0;
        foreach (var (c, v) in fields)
        {
            if (i > 0) { buf.Append(", "); }
            buf.Append($"{c.ValueString}: {c.ToString(v)}");
            i++;
        }

        buf.Append('}');
        return buf.ToString();
    }

    public Record Update(Record source)
    {
        foreach (var (c, v) in source.fields) { fields[c] = v; }
        return this;
    }
}