namespace Hostr.DB.Columns;

using Npgsql;

public class Boolean : TypedColumn<bool>
{
    public Boolean(Table table, string name,
                   bool defaultValue = false,
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
        new Boolean(table, name,
                    defaultValue: (defaultValue is null) ? false : (bool)defaultValue,
                    nullable: nullable,
                    primaryKey: primaryKey);

    public override string ColumnType => "BOOLEAN";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetBoolean(i);
}