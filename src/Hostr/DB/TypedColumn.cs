namespace Hostr.DB;

public abstract class TypedColumn<T>: Column {
    public TypedColumn(Table table, string name): base(table, name) {}
}