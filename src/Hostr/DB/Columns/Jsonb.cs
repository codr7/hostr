using Npgsql;
using System.Text.Json;

namespace Hostr.DB.Columns;

public class Jsonb : TypedColumn<JsonDocument>
{
    private JsonSerializerOptions SerializerOptions;

    public Jsonb(Table table, string name,
                 JsonSerializerOptions serializerOptions,
                 bool nullable = false,
                 bool primaryKey = false) :
    base(table, name,
         defaultValue: null,
         nullable: nullable,
         primaryKey: primaryKey)
    {
        SerializerOptions = serializerOptions;
    }

    public override Column Clone(Table table, string name,
                                 object? defaultValue = null,
                                 bool nullable = false,
                                 bool primaryKey = false) =>
        new Jsonb(table, name,
                  SerializerOptions,
                  nullable: nullable, primaryKey: primaryKey);

    public override string ColumnType => "JSONB";
    public override object GetObject(NpgsqlDataReader source, int i) => JsonDocument.Parse(source.GetString(i));

    public override string ToString(object value) =>
        JsonSerializer.Serialize((JsonDocument)value, SerializerOptions);

    public override object? Read(Utf8JsonReader reader) => JsonDocument.ParseValue(ref reader);
    public override void Write(Utf8JsonWriter writer, object value) => ((JsonDocument)value).WriteTo(writer);
}