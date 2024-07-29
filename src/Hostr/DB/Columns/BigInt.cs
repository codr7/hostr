namespace Hostr.DB.Columns;

using Npgsql;

public class BigInt : TypedColumn<long>
{
    public BigInt(Table table, string name,
                  long defaultValue = 0,
                  bool nullable = false,
                  bool primaryKey = false) :
    base(table, name,
         defaultValue: defaultValue,
         nullable: nullable,
         primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name,
                                 object? defaultValue = null,
                                 bool nullable = false,
                                 bool primaryKey = false) =>
        new BigInt(table, name,
                   defaultValue: (defaultValue is null) ? 0 : (long)defaultValue,
                   nullable: nullable,
                   primaryKey: primaryKey);

    public override string ColumnType => "BIGINT";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetInt64(i);
}