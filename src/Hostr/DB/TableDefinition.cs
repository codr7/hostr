namespace Hostr.DB;

public abstract class TableDefinition : Definition
{
    public readonly Table Table;

    public TableDefinition(Table table, string name) : base(name)
    {
        Table = table;
    }

    public override string CreateSQL => $"ALTER TABLE {Table} ADD {DefinitionType} \"{Name}\"";
    public override string DropSQL => $"ALTER TABLE {Table} DROP {DefinitionType}";
}