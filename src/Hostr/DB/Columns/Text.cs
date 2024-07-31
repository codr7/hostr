namespace Hostr.DB.Columns;

using System.Text.Json;
using Npgsql;

public class Text : TypedColumn<string>
{
    public Text(Table table, string name,
                string defaultValue = "",
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
        new Text(table, name,
                 defaultValue: (defaultValue is null) ? "" : (string)defaultValue,
                 nullable: nullable, primaryKey: primaryKey);

    public override string ColumnType => "TEXT";
    public override string DefaultValueSQL => $"'{DefaultValue}'";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetString(i);
    public override object? Read(Utf8JsonReader reader) => reader.GetString();
    public override string ToString(object val) => $"'{val}'";
    public override void Write(Utf8JsonWriter writer, object value) => writer.WriteStringValue((string)value);
}