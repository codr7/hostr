using System.Text;

namespace Hostr.DB;

public struct Record
{
    private OrderedMap<Column, object> fields = new OrderedMap<Column, object>();

    public Record() { }

    public bool Contains(Column col) => fields.ContainsKey(col);
    public T? Get<T>(TypedColumn<T> col) => (T?)GetObject(col);
    public object? GetObject(Column col) => fields[col];
    public (Column, object)[] Fields => fields.Items;

    public Record Set<T>(TypedColumn<T> col, T value) where T : notnull => SetObject(col, value);

    public Record Set(ForeignKey key, Record rec) {
        foreach (var (c, fc) in key.ColumnMap) { 
            if (rec.GetObject(fc) is object v) {
                SetObject(c, v);
            } else {
                throw new Exception($"Missing key: {fc}");
            } 
        }

        return this;
    }

    public Record SetObject(Column col, object value)
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
            buf.Append($"{c.Name}: {c.ValueToString(v)}");
            i++;
        }

        buf.Append('}');
        return buf.ToString();
    }
}