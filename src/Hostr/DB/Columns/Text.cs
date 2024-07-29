namespace Hostr.DB.Columns;

using Npgsql;

public class Text : TypedColumn<string>
{
    public Text(Table table, string name) : base(table, name) { }

    public override Column Clone(Table table, string name)
    {
        return new Text(table, name) { Nullable = Nullable, PrimaryKey = PrimaryKey };
    }

    public override string ColumnType => "TEXT";

    public override object GetObject(NpgsqlDataReader source, int i)
    {
        return source.GetString(i);
    }

    public override string ValueToString(object val)
    {
        return $"\"{val}\"";
    }
}