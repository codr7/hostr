namespace Hostr.DB.Columns;

using Npgsql;

public class Integer : TypedColumn<int>
{
    public Integer(Table table, string name): base(table, name) {}

    public override string ColumnType => "INTEGER";

    public override object GetObject(NpgsqlDataReader source, int i) {
        return source.GetInt32(i);
    }
}