using System.Text;

namespace Hostr.DB;

using RecordId = ulong;

public struct Record
{
    private static RecordId nextId = 0;
    public static RecordId NextId() => Interlocked.Increment(ref nextId);

    private readonly OrderedMap<Column, object> fields = new OrderedMap<Column, object>();

    public Record(RecordId id)
    {
        Id = id;
    }

    public Record()
    {
        Id = NextId();
    }

    public bool Contains(Column col) => fields.ContainsKey(col);

    public void Copy(ref Record to, (Column, Column)[] map)
    {
        foreach (var (fc, tc) in map)
        {
            if (GetObject(fc) is object v)
            {
                to.SetObject(tc, v);
            }
            else
            {
                throw new Exception($"Missing field: {fc}");
            }
        }
    }

    public Record Copy(Column[] cols)
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


    public (Column, object)[] Fields => fields.Items;

    public T? Get<T>(TypedColumn<T> col) => (T?)GetObject(col);
    public object? GetObject(Column col) => fields[col];

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

    public Record SetObject(Column col, object? value)
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
            buf.Append($"{c.Name}: {c.ToString(v)}");
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