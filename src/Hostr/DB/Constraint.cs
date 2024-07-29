using System.Text;

namespace Hostr.DB;

public abstract class Constraint: TableDefinition
{
    private readonly List<Column> columns = new List<Column>();

    public Constraint(Table table, string name, params Column[] columns): base(table, name)
    {
        foreach (var c in columns) { AddColumn(c); }
        table.AddConstraint(this);
    }

    public void AddColumn(Column col) {
        columns.Add(col);
    }

    public Column[] Columns => columns.ToArray();
    
    public abstract string ConstraintType { get; }

    public override string CreateSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.CreateSQL);
            buf.Append($" {ConstraintType} (");
            buf.Append(string.Join(", ", values: columns.Select(c => c.Name)));
            buf.Append(')');
            return buf.ToString();
        }
    }
    
    public override string DefinitionType => "CONSTRAINT";
   
}