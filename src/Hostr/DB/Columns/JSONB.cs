namespace Hostr.DB.Columns;

using Npgsql;

public class JSONB : TypedColumn<string>
{
    public JSONB(Table table, string name,
                string defaultValue = "null",
                bool nullable = false,
                bool primaryKey = false) :
    base(table, name,
         defaultValue: defaultValue,
         nullable: nullable,
         primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name,
                                 object? defaultValue = null,
                                 bool nullable = false,
                                 bool primaryKey = false) =>
        new JSONB(table, name,
                 defaultValue: (defaultValue is null) ? "null" : (string)defaultValue,
                 nullable: nullable, primaryKey: primaryKey);

    public override string ColumnType => "JSONB";
    public override string DefaultValueSQL => $"NULL";
    public override object GetObject(NpgsqlDataReader source, int i) =>  source.GetString(i);
    public override string ValueToString(object val) => $"\'{val}\'";
}