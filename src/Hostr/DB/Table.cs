using Npgsql;
using System.Data;
using System.Text;

namespace Hostr.DB;

public class Table : Definition, Source
{
    public delegate void AfterHandler(Record rec, object cx);
    public delegate void BeforeHandler(ref Record rec, object cx);

    private readonly List<AfterHandler> afterInsert = new List<AfterHandler>();
    private readonly List<AfterHandler> afterUpdate = new List<AfterHandler>();
    private readonly List<BeforeHandler> beforeInsert = new List<BeforeHandler>();
    private readonly List<BeforeHandler> beforeUpdate = new List<BeforeHandler>();

    private readonly List<Column> columns = new List<Column>();
    private readonly List<Constraint> constraints = new List<Constraint>();
    private readonly Dictionary<string, TableDefinition> lookup = new Dictionary<string, TableDefinition>();
    private readonly List<ForeignKey> foreignKeys = new List<ForeignKey>();
    private readonly List<Index> indexes = new List<Index>();

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

    public long Count(Condition? where, Cx cx)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT COUNT(*) FROM {this}");
        object[] args = [];

        if (where is Condition w)
        {
            sql.Append($" WHERE {w}");
            args = w.Args;
        }

        return cx.Tx!.ExecScalar<long>(sql.ToString(), args: args);
    }

    public long Count(Record key, Cx cx) =>
        Count(Condition.And(key.Fields.Select((f) => f.Item1.Eq(f.Item2)).ToArray()), cx);

    public override void Create(Cx cx)
    {
        base.Create(cx);
        PrimaryKey.Create(cx);

        foreach (var c in constraints)
        {
            if (c != PrimaryKey) { c.Create(cx); }
        }

        foreach (var i in indexes) { i.Create(cx); }
    }

    public override string CreateSql
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.CreateSql);
            buf.Append(" (");
            buf.Append(string.Join(", ", values: columns.Select(c => $"\"{c.Name}\" {c.DefinitionSQL}")));
            buf.Append(')');
            return buf.ToString();
        }
    }

    public override string DefinitionType => "TABLE";

    public override bool Exists(Cx cx) =>
        cx.Tx!.ExecScalar<bool>($"SELECT EXISTS (SELECT FROM pg_tables WHERE tablename = $?)", Name);

    public Record[] FindAll(Condition? where, Cx cx)
    {
        using var reader = Read(where, cx);
        var result = new List<Record>();

        while (reader.Read())
        {
            var rec = new Record();
            Load(ref rec, reader, cx);
            result.Add(rec);
        }

        return result.ToArray();
    }

    public Record? FindFirst(Condition? where, Cx cx)
    {
        using var reader = Read(where, cx);
        if (!reader.Read()) { return null; }
        var result = new Record();
        Load(ref result, reader, cx);
        return result;
    }

    public Record? FindFirst(Record key, Cx cx) =>
        FindFirst(Condition.And(key.Fields.Select((f) => f.Item1.Eq(f.Item2)).ToArray()), cx);

    public Record Insert(ref Record rec, object data, Cx cx)
    {
        foreach (var h in beforeInsert) { h(ref rec, data); }

        var d = rec;
        var cs = columns.Where(c => d.Contains(c)).Select(c => (c, d.GetObject(c)!)).ToArray();
        var sql = @$"INSERT INTO {this} ({string.Join(", ", cs.Select((c) => $"\"{c.Item1.Name}\""))}) 
                     VALUES ({string.Join(", ", Enumerable.Repeat("$?", cs.Length))})";

        cx.Tx!.Exec(sql, args: cs.Select(c => c.Item2).ToArray());
        foreach (var h in afterInsert) { h(rec, data); }
        foreach (var (c, v) in cs) { cx.Tx!.StoreValue(rec.Id, c, v); }

        var res = new Record(id: rec.Id);
        foreach (var (c, v) in cs) { res.SetObject(c, v); }
        return res;
    }

    public void Load(ref Record rec, NpgsqlDataReader reader, Cx cx)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var c = columns[i];
            if (!reader.IsDBNull(i))
            {
                var v = c.GetObject(reader, i);
                rec.SetObject(c, v);
                cx.Tx!.StoreValue(rec.Id, c, v);
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

    public string SourceSql => $"\"{Name}\"";

    public Record Store(ref Record rec, object data, Cx cx) =>
        Stored(rec, cx) ? Update(ref rec, data, cx) : Insert(ref rec, data, cx);

    public bool Stored(Record rec, Cx cx) =>
        cx.Tx!.GetStoredValue(rec.Id, PrimaryKey.Columns[0]) != null;

    public override void Sync(Cx cx)
    {
        if (Exists(cx))
        {
            foreach (var c in columns) { c.Sync(cx); }
            PrimaryKey.Sync(cx);

            foreach (var c in constraints)
            {
                if (c != PrimaryKey) { c.Sync(cx); }
            }
        }
        else
        {
            Create(cx);
        }
    }

    public Record Update(ref Record rec, object data, Cx cx)
    {
        foreach (var h in beforeUpdate) { h(ref rec, data); }
        var k = rec;

        var cs = columns.
          Where(c => k.Contains(c)).
          Select(c => (c, k.GetObject(c)!)).
          Where(c =>
          {
              var sv = cx.Tx!.GetStoredValue(k.Id, c.Item1);
              return sv == null || !sv.Equals(c.Item2);
          }).
          ToArray();

        if (cs.Length == 0) { return rec; }
        var wcs = PrimaryKey.Columns.Select(c => (c, cx.Tx!.GetStoredValue(k.Id, c)!)).ToArray();

        var w = Condition.And(wcs.
          Select((f) =>
          {
              if (f.Item2 is object v) { return f.Item1.Eq(f.Item2); }
              throw new Exception($"Missing key: {f.Item1}");
          }).
          ToArray());

        var sql = @$"UPDATE {this} SET {string.Join(", ", cs.Select((c) => $"\"{c.Item1.Name}\" = $?"))} WHERE {w}";
        cx.Tx!.Exec(sql, args: cs.Select(f => f.Item2).Concat(wcs.Select(f => f.Item2)).ToArray());
        foreach (var h in afterUpdate) { h(rec, data); }
        foreach (var (c, v) in cs) { cx.Tx!.StoreValue(rec.Id, c, v); }

        var res = new Record(id: rec.Id);
        foreach (var (c, v) in cs) { res.SetObject(c, v); }
        return res;
    }

    internal void AddDefinition(TableDefinition def) => lookup[def.Name] = def;
    internal void AddColumn(Column col) => columns.Add(col);
    internal void AddConstraint(Constraint cons) => constraints.Add(cons);
    internal void AddForeignKey(ForeignKey key) => foreignKeys.Add(key);
    internal void AddIndex(Index idx) => indexes.Add(idx);

    private NpgsqlDataReader Read(Condition? where, Cx cx)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT {string.Join(", ", columns)} FROM {Name}");
        object[] args = [];

        if (where is Condition w)
        {
            sql.Append($" WHERE {w}");
            args = w.Args;
        }

        return cx.Tx!.ExecReader(sql.ToString(), args: args);
    }

};