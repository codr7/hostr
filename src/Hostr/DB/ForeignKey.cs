namespace Hostr.DB;

public class ForeignKey : Key
{
    public readonly Table ForeignTable;
    public readonly bool Nullable;
    public readonly bool PrimaryKey;
    private readonly List<(Column, Column)> columnMap = new List<(Column, Column)>();

    public ForeignKey(Table table, string name, Table foreignTable, (Column, Column)[] columns, bool nullable = false, bool primaryKey = false) :
    base(table, $"{table.Name}{name.Capitalize()}", columns.Select(c => c.Item1).ToArray())
    {
        ForeignTable = foreignTable;
        foreach (var c in columns) { columnMap.Add(c); }
        table.AddForeignKey(this);
        Nullable = nullable;
        PrimaryKey = primaryKey;
    }

    public ForeignKey(Table table, string name, Table foreignTable, bool nullable = false, bool primaryKey = false) :
    this(table, name, foreignTable, foreignTable.PrimaryKey.Columns.Select(c =>
    {
        var cc = c.Clone(table, $"{name}{c.Name.Capitalize()}", nullable: nullable, primaryKey: primaryKey);
        return (cc, c);
    }).ToArray(),
    nullable: nullable, primaryKey: primaryKey)
    { }

    public (Column, Column)[] ColumnMap => columnMap.ToArray();

    public override string ConstraintType => "FOREIGN KEY";

    public override string CreateSQL =>
      @$"{base.CreateSQL} 
         REFERENCES {ForeignTable} 
         ({string.Join(", ", values: columnMap.Select(c => c.Item2.Name))})";
}