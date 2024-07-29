namespace Hostr.DB.Columns;

using Npgsql;

public class Timestamp : TypedColumn<DateTime>
{
    public Timestamp(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false)
    {
        return new Timestamp(table, name, nullable: nullable, primaryKey: primaryKey);
    }

    public override string ColumnType => "TIMESTAMP";

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetDateTime(i);
}