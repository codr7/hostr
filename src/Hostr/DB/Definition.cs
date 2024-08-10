namespace Hostr.DB;

public abstract class Definition : IComparable<Definition>
{
    public readonly string Name;
    public readonly Schema Schema;

    public Definition(Schema schema, string name)
    {
        Schema = schema;
        Name = name;
    }

    public virtual void AddColumns(List<Column> result) {}

    public int CompareTo(Definition? other) => (other is Definition o) ? Name.CompareTo(o.Name) : -1;

    public virtual void Create(Cx cx) => cx.Tx!.Exec(CreateSql);

    public virtual string CreateSql => $"CREATE {DefinitionType} \"{Name}\"";

    public abstract string DefinitionType { get; }

    public virtual void Drop(Cx cx) => cx.Tx!.Exec(DropSql);

    public virtual void DropIfExists(Cx cx) {
        if (Exists(cx)) { Drop(cx); }
    }

    public virtual string DropSql => $"DROP {DefinitionType} \"{Name}\"";

    public abstract bool Exists(Cx cx);
 
    public virtual void Sync(Cx cx)
    {
        if (!Exists(cx)) { Create(cx); }
    }
 
    public override string ToString() => $"\"{Name}\"";
}