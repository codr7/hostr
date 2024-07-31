namespace Hostr.DB.Columns;

using System.Text.Json;
using Npgsql;

public class Integer : TypedColumn<int>
{
    public Integer(Table table, string name,
                   int? defaultValue = 0,
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
        new Integer(table, name,
                    defaultValue: (defaultValue is null) ? null : (int)defaultValue,
                    nullable: nullable,
                    primaryKey: primaryKey);

    public override string ColumnType => "INTEGER";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetInt32(i);
    public override object? Read(Utf8JsonReader reader) => reader.GetInt32();
    public override void Write(Utf8JsonWriter writer, object value) => writer.WriteNumberValue((int)value);
}