namespace Hostr.DB;

public class Index : TableDefinition
{
    private readonly List<Column> columns = new List<Column>();

    public Index(Table table, string name, Definition[] columns) : base(table, $"{table.Name}{name.Capitalize()}")
    {
        foreach (var c in columns) { c.AddColumns(this.columns); }
        table.AddIndex(this);
    }

    public override string CreateSql =>
        $"CREATE INDEX {this} ON {Table} ({string.Join(", ", columns.Select(c => $"\"{c.Name}\""))})";

    public override string DefinitionType => $"DROP INDEX {this}";
    public override bool Exists(Tx tx) =>
        tx.ExecScalar<bool>(@"SELECT EXISTS (
                                SELECT FROM pg_class t, pg_class i, pg_index ix
                                WHERE
                                  t.oid = ix.indrelid
                                  and i.oid = ix.indexrelid
                                  and t.relname = $?
                                  and i.relname = $?
                              )", Table.Name, Name);
}