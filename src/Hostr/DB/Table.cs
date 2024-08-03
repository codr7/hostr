using Npgsql;
using System.Data;
using System.Text;

namespace Hostr.DB;

public class Table : Definition
{
    public delegate void AfterHandler(Record rec, Tx tx);
    public delegate void BeforeHandler(ref Record rec, Tx tx);

    private readonly List<AfterHandler> afterInsert = new List<AfterHandler>();
    private readonly List<AfterHandler> afterUpdate = new List<AfterHandler>();
    private readonly List<BeforeHandler> beforeInsert = new List<BeforeHandler>();
    private readonly List<BeforeHandler> beforeUpdate = new List<BeforeHandler>();

    private readonly List<Column> columns = new List<Column>();
    private readonly List<Constraint> constraints = new List<Constraint>();
    private readonly Dictionary<string, TableDefinition> lookup = new Dictionary<string, TableDefinition>();
    private readonly List<ForeignKey> foreignKeys = new List<ForeignKey>();

    private Key? primaryKey = null;

    public Table(Schema schema, string name) : base(schema, name)
    {
        schema.AddDefinition(this);
    }

    public TableDefinition? this[string name] => lookup[name];

    public event AfterHandler AfterInsert
    {
        add => afterInsert.Add(value);
        remove => afterInsert.Remove(value);
    }
    public event AfterHandler AfterUpdate
    {
        add => afterUpdate.Add(value);
        remove => afterUpdate.Remove(value);
    }

    public event BeforeHandler BeforeInsert
    {
        add => beforeInsert.Add(value);
        remove => beforeInsert.Remove(value);
    }

    public event BeforeHandler BeforeUpdate
    {
        add => beforeUpdate.Add(value);
        remove => beforeUpdate.Remove(value);
    }

    public Column[] Columns => columns.ToArray();

    public long Count(Condition? where, Tx tx)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT COUNT(*) FROM {this}");
        object[] args = [];

        if (where is Condition w)
        {
            sql.Append($" WHERE {w}");
            args = w.Args;
        }

        return tx.ExecScalar<long>(sql.ToString(), args: args);
    }

    public long Count(Record key, Tx tx) =>
        Count(Condition.And(key.Fields.Select((f) => f.Item1.Eq(f.Item2)).ToArray()), tx);

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

    public override bool Exists(Tx tx) =>
        tx.ExecScalar<bool>($"SELECT EXISTS (SELECT FROM pg_tables WHERE tablename = $?)", Name);


    private NpgsqlDataReader Read(Condition? where, Tx tx)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT {string.Join(", ", columns)} FROM {Name}");
        object[] args = [];

        if (where is Condition w)
        {
            sql.Append($" WHERE {w}");
            args = w.Args;
        }

        return tx.ExecReader(sql.ToString(), args: args);
    }

    public Record[] FindAll(Condition? where, Tx tx)
    {
        using var reader = Read(where, tx);
        var result = new List<Record>();

        while (reader.Read())
        {
            var rec = new Record();
            Load(ref rec, reader, tx);
            result.Add(rec);
        }

        return result.ToArray();
    }

    public Record? FindFirst(Condition? where, Tx tx)
    {
        using var reader = Read(where, tx);
        if (!reader.Read()) { return null; }
        var result = new Record();
        Load(ref result, reader, tx);
        return result;
    }

    public Record? FindFirst(Record key, Tx tx) =>
        FindFirst(Condition.And(key.Fields.Select((f) => f.Item1.Eq(f.Item2)).ToArray()), tx);

    public Record Insert(ref Record rec, Tx tx)
    {
        foreach (var h in beforeInsert) { h(ref rec, tx); }

        var d = rec;
        var cs = columns.Where(c => d.Contains(c)).Select(c => (c, d.GetObject(c)!)).ToArray();
        var sql = @$"INSERT INTO {this} ({string.Join(", ", cs.Select((c) => $"\"{c.Item1.Name}\""))}) 
                     VALUES ({string.Join(", ", Enumerable.Range(0, cs.Length).Select(i => $"${i + 1}").ToArray())})";

        tx.Exec(sql, args: cs.Select(c => c.Item2).ToArray());
        foreach (var (c, v) in cs) { tx.StoreValue(rec.Id, c, v); }
        foreach (var h in afterInsert) { h(rec, tx); }

        var res = new Record(id: rec.Id);
        foreach (var (c, v) in cs) { res.SetObject(c, v); }
        return res;
    }

    public void Load(ref Record rec, NpgsqlDataReader reader, Tx tx)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var c = columns[i];
            if (!reader.IsDBNull(i))
            {
                var v = c.GetObject(reader, i);
                rec.SetObject(c, v);
                tx.StoreValue(rec.Id, c, v);
            }
        }
    }

    public Key PrimaryKey
    {
        get
        {
            if (primaryKey == null)
            {
                primaryKey = new Key(this, "primaryKey", columns.Where(c => c.PrimaryKey).ToArray());
            }

            return primaryKey;
        }
    }

    public Record Store(ref Record rec, Tx tx) =>
        Stored(rec, tx) ? Update(ref rec, tx) : Insert(ref rec, tx);

    public bool Stored(Record rec, Tx tx) =>
        tx.GetStoredValue(rec.Id, PrimaryKey.Columns[0]) != null;

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

    public Record Update(ref Record rec, Tx tx)
    {
        foreach (var h in beforeUpdate) { h(ref rec, tx); }
        var k = rec;

        var cs = columns.
          Where(c => k.Contains(c)).
          Select(c => (c, k.GetObject(c)!)).
          Where(c =>
          {
              var sv = tx.GetStoredValue(k.Id, c.Item1);
              return sv == null || !sv.Equals(c.Item2);
          }).
          ToArray();

        if (cs.Length == 0) { return rec; }
        var wcs = PrimaryKey.Columns.Select(c => (c, tx.GetStoredValue(k.Id, c)!)).ToArray();

        var w = Condition.And(wcs.
          Select((f) =>
          {
              if (f.Item2 is object v) { return f.Item1.Eq(f.Item2); }
              throw new Exception($"Missing key: {f.Item1}");
          }).
          ToArray());

        var sql = @$"UPDATE {this} SET {string.Join(", ", cs.Select((c) => $"\"{c.Item1.Name}\" = $?"))} WHERE {w}";
        tx.Exec(sql, args: cs.Select(f => f.Item2).Concat(wcs.Select(f => f.Item2)).ToArray());
        foreach (var (c, v) in cs) { tx.StoreValue(rec.Id, c, v); }
        foreach (var h in afterUpdate) { h(rec, tx); }

        var res = new Record(id: rec.Id);
        foreach (var (c, v) in cs) { res.SetObject(c, v); }
        return res;
    }

    internal void AddDefinition(TableDefinition def) => lookup[def.Name] = def;
    internal void AddColumn(Column col) => columns.Add(col);
    internal void AddConstraint(Constraint cons) => constraints.Add(cons);
    internal void AddForeignKey(ForeignKey key) => foreignKeys.Add(key);
};