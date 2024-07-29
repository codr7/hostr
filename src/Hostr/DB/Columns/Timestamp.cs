namespace Hostr.DB.Columns;

using Npgsql;

public class Timestamp : TypedColumn<DateTime>
{
    public Timestamp(Table table, string name): base(table, name) {}

    public override Column Clone(Table table, string name)
    {
        return new Timestamp(table, name) { Nullable = Nullable, PrimaryKey = PrimaryKey };
    }

    public override string ColumnType => "TIMESTAMP";

    public override object GetObject(NpgsqlDataReader source, int i) {
        return source.GetDateTime(i);
    }
}