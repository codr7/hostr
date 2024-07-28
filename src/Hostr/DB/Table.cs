namespace Hostr.DB;

public class Table: IComparable<Table>
{
    public readonly string Name;
    private readonly List<Column> columns = new List<Column>();
    private readonly List<Constraint> constraints = new List<Constraint>();
    private Key? primaryKey = null;

    public Table(string name)
    {
        Name = name;
    }

    internal void AddColumn(Column col)
    {
        columns.Add(col);
    }

    internal void AddConstraint(Constraint cons)
    {
        constraints.Add(cons);
    }

    public int CompareTo(Table? other)
    {
        if (other is Table t) {
            return Name.CompareTo(t.Name);
        }

        return -1;
    }

    public Key PrimaryKey
    {
        get {
            if (primaryKey == null)
            {
                primaryKey = new Key(this, $"{Name}Primary");
                columns.Where(c => c.PrimaryKey).ToList().ForEach(c => primaryKey.AddColumn(c));
            }

            return primaryKey;
        }
    }

};