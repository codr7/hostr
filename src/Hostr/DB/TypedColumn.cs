namespace Hostr.DB;

public abstract class TypedColumn<T> : Column
{
    public TypedColumn(Table table, string name, bool nullable = false, bool primaryKey = false) :
    base(table, name, nullable: nullable, primaryKey: primaryKey)
    { }
}