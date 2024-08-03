using System.Text.Json;

namespace Hostr;

public class Json {
    public static JsonSerializerOptions GetOptions(Schema schema) {
        var os = new JsonSerializerOptions();
        InitOptions(os, schema);
        return os;
    }
    
    public static void InitOptions(JsonSerializerOptions options, Schema schema) {
        options.Converters.Add(new DB.DocumentConverter());
        options.Converters.Add(new DB.RecordConverter(schema));
    }

    public readonly JsonSerializerOptions Options;

    public Json(Schema schema) {
        Options = GetOptions(schema);
    }
    
    public string ToString(object value) => JsonSerializer.Serialize(value, Options);

    public T? FromString<T>(string value) => JsonSerializer.Deserialize<T>(value, Options);
}