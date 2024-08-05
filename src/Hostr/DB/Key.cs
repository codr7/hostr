namespace Hostr.DB;

public class Key : Constraint
{
    public Key(Table table, string name, Definition[] columns) : base(table, $"{table.Name}{name.Capitalize()}", columns)
    { }

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";
    public Condition Eq(Record rec) => rec.Eq(Columns);
}