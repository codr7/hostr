using System.Security.Cryptography;

namespace Hostr.DB;

public class ForeignKey : Key
{
    public enum Action {
        Cascade,
        NoAction,
        Restrict,
        SetDefault,
        SetNull
    }

    public static string ToString(Action a) => a switch {
            Action.Cascade => "CASCADE",
            Action.NoAction => "NO ACTION",
            Action.Restrict => "RESTRICT",
            Action.SetDefault => "SET DEFAULT",
            Action.SetNull => "SET NULL",
            _ => throw new Exception($"Invalid action: {a}")
    };

    public readonly Table ForeignTable;
    public readonly bool Nullable;
    public readonly Action OnDelete;
    public readonly Action OnUpdate;
    public readonly bool PrimaryKey;
    private readonly List<(Column, Column)> columnMap = new List<(Column, Column)>();

    public ForeignKey(Table table, string name, Table foreignTable, (Column, Column)[] columns, 
                      bool nullable = false, 
                      Action onDelete = Action.Restrict, 
                      Action onUpdate = Action.Cascade, 
                      bool primaryKey = false) :
    base(table, name, columns.Select(c => c.Item1).ToArray())
    {
        ForeignTable = foreignTable;
        foreach (var c in columns) { columnMap.Add(c); }
        table.AddForeignKey(this);
        Nullable = nullable;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
        PrimaryKey = primaryKey;
    }

    public ForeignKey(Table table, string name, Table foreignTable, bool nullable = false, bool primaryKey = false) :
    this(table, name, foreignTable, foreignTable.PrimaryKey.Columns.Select(c =>
    {
        var cc = c.Clone(table, $"{name}{c.Name.Capitalize()}", defaultValue: null, nullable: nullable, primaryKey: primaryKey);
        return (cc, c);
    }).ToArray(),
    nullable: nullable,
    primaryKey: primaryKey)
    { }

    public (Column, Column)[] ColumnMap => columnMap.ToArray();

    public override string ConstraintType => "FOREIGN KEY";

    public override string CreateSQL =>
      @$"{base.CreateSQL} 
         REFERENCES {ForeignTable} 
         ({string.Join(", ", values: columnMap.Select(c => $"\"{c.Item2.Name}\""))})
         ON DELETE {ToString(OnDelete)} ON UPDATE {ToString(OnUpdate)}";

    public Column[] ForeignColumns => columnMap.Select(m => m.Item2).ToArray();
}