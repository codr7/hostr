namespace Hostr.DB.Columns;

using Npgsql;

public class BigInt : TypedColumn<long>
{
    public BigInt(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, bool nullable = false, bool primaryKey = false) =>
        new BigInt(table, name, nullable: nullable, primaryKey: primaryKey);

    public override string ColumnType => "BIGINT";

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetInt64(i);
}