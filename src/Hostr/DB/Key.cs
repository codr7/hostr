namespace Hostr.DB;

public class Key : Constraint
{
    public Key(Table table, string name, Column[] columns) : base(table, $"{table.Name}{name.Capitalize()}", columns)
    { }

    public override string ConstraintType => (this == Table.PrimaryKey) ? "PRIMARY KEY" : "UNIQUE";

    public Condition Eq(Record rec)
    {
        var conds = new List<Condition>();

        foreach (var c in Columns)
        {
            if (rec.GetObject(c) is object v)
            {
                conds.Add(c.Eq(v));
            }
            else
            {
                throw new Exception($"Missing key: {c}");
            }
        }

        return Condition.And(conds.ToArray());
    }
}