namespace Hostr.DB.Columns;

using Npgsql;

public class Decimal : TypedColumn<decimal>
{
    public Decimal(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false)
    {
        return new Decimal(table, name, nullable: nullable, primaryKey: primaryKey);
    }

    public override string ColumnType => "DECIMAL(28, 28)";

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetDecimal(i);
}