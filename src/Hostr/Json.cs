using System.Text.Json;

namespace Hostr;

public class Json {
    public static JsonSerializerOptions GetOptions(Schema schema) {
        var os = new JsonSerializerOptions();
        os.Converters.Add(new DB.DocumentConverter());
        os.Converters.Add(new DB.RecordConverter(schema));
        return os;
    }
    
    public readonly JsonSerializerOptions Options;

    public Json(Schema schema) {
        Options = GetOptions(schema);
    }
    
    public string ToString(object value) => JsonSerializer.Serialize(value, Options);

    public T? FromString<T>(string value) => JsonSerializer.Deserialize<T>(value, Options);
}