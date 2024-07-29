namespace Hostr.DB.Columns;

using Npgsql;

public class Text : TypedColumn<string>
{
    public Text(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false)
    {
        return new Text(table, name, nullable: nullable, primaryKey: primaryKey);
    }

    public override string ColumnType => "TEXT";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetString(i);
    public override string ValueToString(object val) => $"\"{val}\"";
}