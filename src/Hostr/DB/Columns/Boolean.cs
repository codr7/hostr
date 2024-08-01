using Npgsql;
using System.Text.Json;

namespace Hostr.DB.Columns;

public class Boolean : TypedColumn<bool>
{
    public Boolean(Table table, string name,
                   bool? defaultValue = false,
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
                    defaultValue: (defaultValue is null) ? null : (bool)defaultValue,
                    nullable: nullable,
                    primaryKey: primaryKey);

    public override string ColumnType => "BOOLEAN";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetBoolean(i);
    public override object? Read(Utf8JsonReader reader) => reader.GetBoolean();
    public override void Write(Utf8JsonWriter writer, object value) => writer.WriteBooleanValue((bool)value);
}