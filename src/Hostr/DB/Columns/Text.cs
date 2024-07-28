namespace Hostr.DB.Columns;

public class Text : TypedColumn<string>
{
    public Text(Table table, string name): base(table, name) {}

    public override string ColumnType => "TEXT";
}