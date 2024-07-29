namespace Hostr.DB.Columns;

using Npgsql;

public class BigInt : TypedColumn<long>
{
    public BigInt(Table table, string name) : base(table, name) { }

    public override Column Clone(Table table, string name)
    {
        return new BigInt(table, name) { Nullable = Nullable, PrimaryKey = PrimaryKey };
    }

    public override string ColumnType => "BIGINT";

    public override object GetObject(NpgsqlDataReader source, int i)
    {
        return source.GetInt64(i);
    }
}