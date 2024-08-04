using System.Text.Json;
using Npgsql;

namespace Hostr.DB;

public interface Value : IComparable<Value>
{
    void AddValueArgs(List<object> result) { }
    Condition Eq(object val) => new Condition($"{ValueSql} = $?", val);
    object GetObject(NpgsqlDataReader source, int i);
    string ToString(object val);
    string ValueSql { get; }
    string ValueString { get; }
    void Write(Utf8JsonWriter writer, object value);
}
