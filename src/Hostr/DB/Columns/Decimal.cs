namespace Hostr.DB.Columns;

using Npgsql;

public class Decimal : TypedColumn<decimal>
{
    public Decimal(Table table, string name) : base(table, name) { }

    public override Column Clone(Table table, string name)
    {
        return new Decimal(table, name) { Nullable = Nullable, PrimaryKey = PrimaryKey };
    }

    public override string ColumnType => "DECIMAL(28, 28)";

    public override object GetObject(NpgsqlDataReader source, int i)
    {
        return source.GetDecimal(i);
    }
}