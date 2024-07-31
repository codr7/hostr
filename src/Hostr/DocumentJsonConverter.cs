namespace Hostr.DB;

using System.Text.Json;
using System.Text.Json.Serialization;

public class DocumentJsonConverter : JsonConverter<JsonDocument>
{
    public DocumentJsonConverter() {}

    public override bool CanConvert(Type objectType) => objectType == typeof(JsonDocument);

    public override JsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonDocument.ParseValue(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, JsonDocument value, JsonSerializerOptions options)
    {
        value.WriteTo(writer);
    }
}
