using System.Text;

namespace Hostr.DB;

public struct Record
{
    private OrderedMap<Column, object> fields = new OrderedMap<Column, object>();

    public Record() { }

    public bool Contains(Column col)
    {
        return fields.ContainsKey(col);
    }

    public T? Get<T>(TypedColumn<T> col)
    {
        return (T?)GetObject(col);
    }

    public object? GetObject(Column col)
    {
        return fields[col];
    }

    public (Column, object)[] Fields => fields.Items;

    public Record Set<T>(TypedColumn<T> col, T value) where T : notnull
    {
        return SetObject(col, value);
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