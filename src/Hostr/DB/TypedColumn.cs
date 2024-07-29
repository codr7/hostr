using System.Text;

namespace Hostr.DB;

public abstract class TypedColumn<T> : Column
{
    public readonly T DefaultValue;

    public TypedColumn(Table table, string name, T defaultValue, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { 
        DefaultValue = defaultValue;
    }

   public virtual string DefaultValueSQL => $"{DefaultValue}";

   public override string DefinitionSQL
    {
        get
        {
            var buf = new StringBuilder();
            buf.Append(base.DefinitionSQL);
            if (DefaultValue != null) { buf.Append($" DEFAULT {DefaultValueSQL}"); }
            return buf.ToString();
        }
    }    
}