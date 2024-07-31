using System.Text.Json;
using Hostr.DB;

namespace Hostr;

public class Json {
    public static JsonSerializerOptions GetOptions(Schema schema) {
        var os = new JsonSerializerOptions();
        os.Converters.Add(new DocumentConverter());
        os.Converters.Add(new RecordConverter(schema));
        return os;
    }
    
    public readonly JsonSerializerOptions Options;

    public Json(Schema schema) {
        Options = GetOptions(schema);
    }
    
    public string ToString(object value) => JsonSerializer.Serialize(value, Options);
}