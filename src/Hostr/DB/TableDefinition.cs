namespace Hostr.DB;

public abstract class TableDefinition : Definition
{
    public readonly Table Table;

    public TableDefinition(Table table, string name) : base(name)
    {
        Table = table;
    }
}