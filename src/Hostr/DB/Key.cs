namespace Hostr.DB;

public class Key : Constraint
{
    public Key(Table table, string name, Column[] columns) : base(table, name, columns)
    { }

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";
}