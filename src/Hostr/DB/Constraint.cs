using System.Text;

namespace Hostr.DB;

public abstract class Constraint : TableDefinition
{
    private readonly List<Column> columns = new List<Column>();

    public Constraint(Table table, string name, Column[] columns) : base(table, name)
    {
        foreach (var c in columns) { this.columns.Add(c); }
        table.AddConstraint(this);
    }

    public Column[] Columns => columns.ToArray();
    public abstract string ConstraintType { get; }

    public override string CreateSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.CreateSQL);
            buf.Append($" {ConstraintType} ({string.Join(", ", values: columns.Select(c => $"\"{c.Name}\""))})");
            return buf.ToString();
        }
    }

    public override string DefinitionType => "CONSTRAINT";

    public override bool Exists(Tx tx)
    {
        return tx.ExecScalar<bool>(@$"SELECT EXISTS (
                                     SELECT constraint_name 
                                     FROM information_schema.constraint_column_usage 
                                     WHERE constraint_name = $?
                                   )", Name);
    }

    internal void AddColumn(Column col) => columns.Add(col);
}