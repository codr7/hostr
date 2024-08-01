using Npgsql;
using System.Text.Json;

namespace Hostr.DB.Columns;

public class BigInt : TypedColumn<long>
{
    public BigInt(Table table, string name,
                  long? defaultValue = null,
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
        new BigInt(table, name,
                   defaultValue: (defaultValue is null) ? null : (long)defaultValue,
                   nullable: nullable,
                   primaryKey: primaryKey);

    public override string ColumnType => "BIGINT";
    public override object GetObject(NpgsqlDataReader source, int i) => source.GetInt64(i);
    public override object? Read(Utf8JsonReader reader) => reader.GetInt64();
    public override void Write(Utf8JsonWriter writer, object value) => writer.WriteNumberValue((long)value);
}