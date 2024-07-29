namespace Hostr.DB.Columns;

using System.ComponentModel;
using Npgsql;

public class Timestamp : TypedColumn<DateTime>
{
    public static readonly DateTime DEFAULT = DateTime.UtcNow;

    public Timestamp(Table table, string name, 
                     DateTime? defaultValue = null, 
                     bool nullable = false, 
                     bool primaryKey = false) :
    base(table, name, 
         defaultValue: defaultValue ?? DateTime.UtcNow, 
         nullable: nullable, 
         primaryKey: primaryKey)
    { }

    public override Column Clone(Table table, string name, 
                                 object? defaultValue = null, 
                                 bool nullable = false, 
                                 bool primaryKey = false) => 
        new Timestamp(table, name,
                      defaultValue: (defaultValue is null) ? DateTime.UtcNow : (DateTime)defaultValue, 
                      nullable: nullable, 
                      primaryKey: primaryKey);

    public override string ColumnType => "TIMESTAMP";

#pragma warning disable CS8605 
    public override string DefaultValueSQL => $"'{(DateTime)DefaultValue:yyyy-MM-ddTHH:mm:ss}'";
#pragma warning restore CS8605

    public override object GetObject(NpgsqlDataReader source, int i) => source.GetDateTime(i);
}