using System.Text.Json;
using Hostr.DB;

namespace Hostr;

public class Json {
    public static JsonSerializerOptions GetOptions(Schema schema) {
        var os = new JsonSerializerOptions();
        os.Converters.Add(new DocumentJsonConverter());
        os.Converters.Add(new RecordJsonConverter(schema));
        return os;
    }
    
    public readonly JsonSerializerOptions Options;

    public Json(Schema schema) {
        Options = GetOptions(schema);
    }
    
    public string ToString(object value) => JsonSerializer.Serialize(value, Options);
}