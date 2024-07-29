namespace Hostr.DB;

using Npgsql;

public abstract class Column: Definition, IComparable<Column>
{
    public bool PrimaryKey;
    public readonly Table Table;
    

    public Column(Table table, string name): base(name)
    {
        Table = table;
        table.AddColumn(this);
    }

    public abstract string ColumnType { get; }

    public int CompareTo(Column? other)
    {
        if (other is Column c) {
            var tr = Table.CompareTo(c.Table);
            return (tr == 0) ? Name.CompareTo(c.Name) : tr;
        }

        return -1;
    }

    public override string DefinitionType => "COLUMN";

    public Condition Eq(object val) {
        return new Condition($"{this} = $?", val);
    }

    public override bool Exists(Tx tx) {
        return tx.ExecScalar<bool>(@$"SELECT EXISTS (
                                     SELECT
                                     FROM pg_attribute 
                                     WHERE attrelid = $?::regclass
                                     AND attname = $?
                                     AND NOT attisdropped
                                   )", Table.Name, Name);
    }

    public abstract object GetObject(NpgsqlDataReader source, int i);

    public override string ToString()
    {
        return $"{Table}.{Name}";
    }
    
    public virtual string ValueToString(object val) {
        return $"{val}";
    }
}