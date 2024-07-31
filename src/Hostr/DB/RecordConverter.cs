namespace Hostr.DB;

using System.Text.Json;
using System.Text.Json.Serialization;

public class RecordConverter : JsonConverter<Record>
{
    private Schema db;

    public RecordConverter(Schema db)
    {
        this.db = db;
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(Record);

    public override Record Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var rec = new Record();
        reader.Read();
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) { throw new Exception("Start of object not found"); }

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.GetString() is string pn)
            {
                var pns = pn.Split('.');

                if (db[pns[0]] is Table t && t[pns[1]] is Column c) {
                    rec.SetObject(c, c.Read(reader));
                } else {
                    throw new Exception("Invalid property");
                }
            }
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject) { throw new Exception("Start of object not found"); }
        return rec;
    }

    public override void Write(Utf8JsonWriter writer, Record value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        foreach(var (c, v) in value.Fields) {
            writer.WritePropertyName($"{c.Table.Name}.{c.Name}");
            c.Write(writer, v);
        }

        writer.WriteEndObject();
    }
}
