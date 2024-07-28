using System.Text;

namespace Hostr.DB;

public class Table : Definition
{
    private readonly List<Column> columns = new List<Column>();
    private readonly List<Constraint> constraints = new List<Constraint>();
    private Key? primaryKey = null;

    public Table(string name) : base(name)
    { }

    public override string DefinitionType => "TABLE";

    public Key PrimaryKey
    {
        get
        {
            if (primaryKey == null)
            {
                primaryKey = new Key(this, $"{Name}Primary");
                columns.Where(c => c.PrimaryKey).ToList().ForEach(c => primaryKey.AddColumn(c));
            }

            return primaryKey;
        }
    }

    internal void AddColumn(Column col)
    {
        columns.Add(col);
    }

    internal void AddConstraint(Constraint cons)
    {
        constraints.Add(cons);
    }

    public override string CreateSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.CreateSQL);
            buf.Append(" (");
            buf.Append(string.Join(", ", values: columns.Select(c => $"{c.Name} {c.ColumnType}")));
            buf.Append(')');
            return buf.ToString();
        }
    }

    public override bool Exists(Tx tx) {
        return tx.ExecScalar<bool>($"SELECT EXISTS (SELECT FROM pg_tables WHERE tablename  = $1)", Name);
    }
};