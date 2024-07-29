namespace Hostr.DB;

public class ForeignKey : Key
{
    public readonly Table ForeignTable;
    public bool Nullable = false;
    private readonly List<(Column, Column)> columnMap = new List<(Column, Column)>();
    private readonly string prefix;

    public ForeignKey(Table table, string name, Table foreignTable, (Column, Column)[] columns) :
    base(table, $"{table.Name}{name.Capitalize()}", columns.Select(c => c.Item1).ToArray())
    {
        ForeignTable = foreignTable;
        foreach (var c in columns) { columnMap.Add(c); }
        prefix = name;
        table.AddForeignKey(this);
    }

    public ForeignKey(Table table, string name, Table foreignTable) :
    this(table, name, foreignTable, [])
    { }

    public (Column, Column)[] ColumnMap => columnMap.ToArray();
    
    public override string ConstraintType => "FOREIGN KEY";

    public override string CreateSQL => 
      @$"{base.CreateSQL} 
         REFERENCES {ForeignTable} 
         ({string.Join(", ", values: columnMap.Select(c => c.Item2.Name))})";

     internal void InitColumnMap() {
        if (columnMap.Count == 0) {
            foreach (var c in ForeignTable.PrimaryKey.Columns) {
                var cc = c.Clone(Table, $"{prefix}{c.Name.Capitalize()}");
                cc.Nullable = Nullable;
                columnMap.Add((cc, c));
                AddColumn(cc);
            }
        }
    }        
}