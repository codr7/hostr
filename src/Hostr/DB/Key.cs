namespace Hostr.DB;

public class Key : Constraint
{
    public Key(Table table, string name, params Column[] columns) : base(table, name, columns)
    { }

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";

    public override bool Exists(Tx tx)
    {
        return tx.ExecScalar<bool>(@$"SELECT EXISTS (
                                     SELECT constraint_name 
                                     FROM information_schema.constraint_column_usage 
                                     WHERE table_name = $? and constraint_name = $?
                                   )", Table.Name, Name.ToLower());
    }
}