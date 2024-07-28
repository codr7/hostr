namespace Hostr.DB;

public abstract class Constraint: TableDefinition
{
    private readonly List<Column> columns = new List<Column>();

    public Constraint(Table table, string name): base(table, name)
    {
        table.AddConstraint(this);
    }

    public void AddColumn(Column col) {
        columns.Add(col);
    }

    public abstract string ConstraintType { get; }

    public override string DefinitionType => "CONSTRAINT";
   
}