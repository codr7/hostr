namespace Hostr.DB;

using System.Text;
using Npgsql;

public abstract class Column : TableDefinition, IComparable<Column>
{
    public bool Nullable = false;
    public bool PrimaryKey;

    public Column(Table table, string name) : base(table, name)
    {
        table.AddColumn(this);
    }

    public abstract string ColumnType { get; }

    public abstract Column Clone(Table table, string name);

    public int CompareTo(Column? other)
    {
        if (other is Column c)
        {
            var tr = Table.CompareTo(c.Table);
            return (tr == 0) ? Name.CompareTo(c.Name) : tr;
        }

        return -1;
    }

    public override string CreateSQL => $"{base.CreateSQL} {DefinitionSQL}";

    public string DefinitionSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append($"{ColumnType}");
            if (!Nullable) { buf.Append(" NOT NULL"); }
            return buf.ToString();
        }
    }

    public override string DefinitionType => "COLUMN";

    public Condition Eq(object val)
    {
        return new Condition($"{this} = $?", val);
    }

    public override bool Exists(Tx tx)
    {
        return tx.ExecScalar<bool>(@"SELECT EXISTS (
                                     SELECT
                                     FROM pg_attribute 
                                     WHERE attrelid = $?::regclass
                                     AND attname = $?
                                     AND NOT attisdropped
                                   )", Table.Name.ToLower(), Name.ToLower());
    }

    public abstract object GetObject(NpgsqlDataReader source, int i);

    public override string ToString()
    {
        return $"{Table}.{Name}";
    }

    public virtual string ValueToString(object val)
    {
        return $"{val}";
    }
}