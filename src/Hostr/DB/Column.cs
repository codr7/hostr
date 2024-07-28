namespace Hostr.DB;

public abstract class Column: IComparable<Column>
{
    public readonly string Name;
    public bool PrimaryKey;
    public readonly Table Table;
    

    public Column(Table table, string name)
    {
        Table = table;
        Name = name;
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
}