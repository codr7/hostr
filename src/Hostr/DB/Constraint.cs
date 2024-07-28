namespace Hostr.DB;

public abstract class Constraint
{
    public readonly string Name;
    public readonly Table Table;
    private readonly List<Column> columns = new List<Column>();

    public Constraint(Table table, string name)
    {
        Table = table;
        Name = name;
        table.AddConstraint(this);
    }

    public void AddColumn(Column col) {
        columns.Add(col);
    }

    public abstract string ConstraintType { get; }
}