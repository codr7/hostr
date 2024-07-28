namespace Hostr.DB;

public class Key: Constraint {
    public Key(Table table, string name): base(table, name) {}

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";
    
}