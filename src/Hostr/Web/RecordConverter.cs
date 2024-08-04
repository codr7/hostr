using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hostr.Web;

public class RecordConverter : JsonConverter<DB.Record>
{
    public override bool CanConvert(Type objectType) => objectType == typeof(DB.Record);

    public override DB.Record Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, DB.Record value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        foreach(var (c, v) in value.Fields) {
            writer.WritePropertyName(c.ValueString);
            c.Write(writer, v);
        }

        writer.WriteEndObject();
    }
}
