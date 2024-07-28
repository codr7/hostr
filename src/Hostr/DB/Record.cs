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
        return (T?)fields[col];
    }

    internal object? Get(Column col)
    {
        return fields[col];
    }

    public void Set<T>(TypedColumn<T> col, T value) where T : notnull
    {
        fields[col] = value;
    }

    public override string ToString()
    {
        var buf = new StringBuilder();
        buf.Append('{');

        var i = 0;
        foreach (var (c, v) in fields)
        {
            if (i > 0) { buf.Append(' '); }
            buf.Append($"{c}:{v}");
            i++;
        }

        buf.Append('}');
        return buf.ToString();
    }
}