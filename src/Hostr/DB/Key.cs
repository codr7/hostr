namespace Hostr.DB;

public class Key: Constraint {
    public Key(Table table, string name): base(table, name) {}

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";

    public override bool Exists(Tx tx) {
        return tx.ExecScalar<bool>(@$"SELECT EXISTS (
                                     SELECT constraint_name 
                                     FROM information_schema.constraint_column_usage 
                                     WHERE table_name = $1  and constraint_name = $2
                                   )", Table.Name, Name);
    }
}