namespace Hostr.DB.Columns;

using System.Text.Json;
using Npgsql;

public class Decimal : TypedColumn<decimal>
{
     public Decimal(Table table, string name,
                    decimal? defaultValue = 0,
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
         new Decimal(table, name,
                     defaultValue: (defaultValue is null) ? null : (decimal)defaultValue,
                     nullable: nullable,
                     primaryKey: primaryKey);

     /* 28 is C#'s internal precision for decimal. */
     public override string ColumnType => "DECIMAL(28, 28)";
     public override object GetObject(NpgsqlDataReader source, int i) => source.GetDecimal(i);
     public override object? Read(Utf8JsonReader reader) => reader.GetDecimal();
     public override void Write(Utf8JsonWriter writer, object value) => writer.WriteNumberValue((decimal)value);
}