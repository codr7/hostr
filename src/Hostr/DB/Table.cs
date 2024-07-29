namespace Hostr.DB;

using System.Data;
using System.Text;
using Npgsql;

public class Table : Definition
{
    public delegate void BeforeInsertHandler(ref Record rec, Tx tx);

    private readonly List<BeforeInsertHandler> beforeInsert = new List<BeforeInsertHandler>();
    private readonly List<Column> columns = new List<Column>();
    private readonly List<Constraint> constraints = new List<Constraint>();
    private readonly List<ForeignKey> foreignKeys = new List<ForeignKey>();
    private Key? primaryKey = null;

    public Table(string name) : base(name)
    { }

    public event BeforeInsertHandler BeforeInsert
    {
        add => beforeInsert.Add(value);
        remove => beforeInsert.Remove(value);
    }

    public Column[] Columns => columns.ToArray();

    public override void Create(Tx tx)
    {
        base.Create(tx);
        PrimaryKey.Create(tx);

        foreach (var c in constraints)
        {
            if (c != PrimaryKey) { c.Create(tx); }
        }
    }

    public override string CreateSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.CreateSQL);
            buf.Append(" (");
            buf.Append(string.Join(", ", values: columns.Select(c => $"\"{c.Name}\" {c.DefinitionSQL}")));
            buf.Append(')');
            return buf.ToString();
        }
    }

    public override string DefinitionType => "TABLE";

    public override bool Exists(Tx tx)
    {
        return tx.ExecScalar<bool>($"SELECT EXISTS (SELECT FROM pg_tables WHERE tablename = $?)", Name);
    }

    public Record? Find(Record key, Tx tx)
    {
        var w = Condition.And(key.Fields.Select((f) => f.Item1.Eq(f.Item2)).ToArray());
        var sql = $"SELECT {string.Join(", ", columns)} FROM {Name} WHERE {w.SQL}";
        using var reader = tx.ExecReader(sql, args: w.Args);
        if (!reader.Read()) { return null; }
        var result = new Record();
        Load(ref result, reader);
        return result;
    }

    public void Insert(Record rec, Tx tx)
    {
        foreach (var h in beforeInsert) { h(ref rec, tx); }

        var cs = columns.Where(c => rec.Contains(c)).Select(c => (c, rec.GetObject(c))).ToArray();
        var sql = @$"INSERT INTO {this} ({string.Join(", ", cs.Select((c) => $"\"{c.Item1.Name}\""))}) 
                     VALUES ({string.Join(", ", Enumerable.Range(0, cs.Length).Select(i => $"${i + 1}").ToArray())})";

#pragma warning disable CS8620
        tx.Exec(sql, args: cs.Select(c => c.Item2).ToArray());
#pragma warning restore CS8620
    }

    public void Load(ref Record rec, NpgsqlDataReader reader)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var c = columns[i];
            if (!reader.IsDBNull(i)) { rec.SetObject(c, (object)c.GetObject(reader, i)); }
        }
    }

    public Key PrimaryKey
    {
        get
        {
            if (primaryKey == null)
            {
                primaryKey = new Key(this, $"{Name}PrimaryKey", columns.Where(c => c.PrimaryKey).ToArray());
            }

            return primaryKey;
        }
    }

    public override void Sync(Tx tx)
    {
        if (Exists(tx))
        {
            foreach (var c in columns) { c.Sync(tx); }
            PrimaryKey.Sync(tx);

            foreach (var c in constraints)
            {
                if (c != PrimaryKey) { c.Sync(tx); }
            }
        }
        else
        {
            Create(tx);
        }
    }

    internal void AddColumn(Column col) => columns.Add(col);
    internal void AddConstraint(Constraint cons) => constraints.Add(cons);
    internal void AddForeignKey(ForeignKey key) => foreignKeys.Add(key);
};