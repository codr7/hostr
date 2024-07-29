namespace Hostr.DB.Columns;

using Npgsql;

public class Integer : TypedColumn<int>
{
    public Integer(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false)
    {
        return new Integer(table, name, nullable: nullable, primaryKey: primaryKey);
    }

    public override string ColumnType => "INTEGER";

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetInt32(i);
}