namespace Hostr.DB.Columns;

public class Integer : TypedColumn<int>
{
    public Integer(Table table, string name): base(table, name) {}

    public override string ColumnType => "INTEGER";
}