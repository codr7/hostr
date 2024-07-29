namespace Hostr.DB.Columns;

using Npgsql;

public class Boolean : TypedColumn<bool>
{
    public Boolean(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false)
    {
        return new Boolean(table, name, nullable: nullable, primaryKey: primaryKey);
    }

    public override string ColumnType => "BOOLEAN";

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetBoolean(i);
}