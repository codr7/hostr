using Npgsql;
using System.Text;
using System.Text.Json;

namespace Hostr.DB;

public abstract class Column : TableDefinition, IComparable<Column>, Value
{
    public readonly bool Nullable;
    public readonly bool PrimaryKey;

    public Column(Table table, string name,
                  bool nullable = false, bool primaryKey = false) : base(table, name)
    {
        table.AddColumn(this);
        Nullable = nullable;
        PrimaryKey = primaryKey;
    }

    public override void AddColumns(List<Column> result) => result.Add(this);
    public abstract string ColumnType { get; }

    public abstract Column Clone(Table table, string name, object? defaultValue = null, bool nullable = false, bool primaryKey = false);

    public int CompareTo(Column? other)
    {
        if (other is Column c)
        {
            var tr = Table.CompareTo(c.Table);
            return (tr == 0) ? Name.CompareTo(c.Name) : tr;
        }

        return -1;
    }

    public override string CreateSql => $"{base.CreateSql} {DefinitionSQL}";

    public virtual string DefinitionSQL
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

    public override bool Exists(Tx tx)
    {
        return tx.ExecScalar<bool>(@"SELECT EXISTS (
                                     SELECT
                                     FROM pg_attribute 
                                     WHERE attrelid = $?::regclass
                                     AND attname = $?
                                     AND NOT attisdropped
                                   )", Table.Name, Name);
    }

    public abstract object GetObject(NpgsqlDataReader source, int i);
    public abstract object? Read(Utf8JsonReader reader);
    public override string ToString() => $"{Table}.\"{Name}\"";
    public virtual string ToString(object val) => $"{val}";
    public string ValueSql => ToString();
    public string ValueString => Name;

    public abstract void Write(Utf8JsonWriter writer, object value);

    public int CompareTo(Value? other)
    {
        if (other is Column c) { return ToString().CompareTo(other.ToString()); }
        if (other is null) { return -1; }
        throw new Exception($"Expected column: {other}");
    }
}