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

    public int CompareTo(Definition? other) => (other is Definition o) ? Name.CompareTo(o.Name) : -1;

    public virtual void Create(Tx tx) => tx.Exec(CreateSql);

    public virtual string CreateSql => $"CREATE {DefinitionType} \"{Name}\"";

    public abstract string DefinitionType { get; }

    public virtual void Drop(Tx tx) => tx.Exec(DropSql);

    public virtual void DropIfExists(Tx tx) {
        if (Exists(tx)) { Drop(tx); }
    }

    public virtual string DropSql => $"DROP {DefinitionType} \"{Name}\"";

    public abstract bool Exists(Tx tx);
 
    public virtual void Sync(Tx tx)
    {
        if (!Exists(tx)) { Create(tx); }
    }
 
    public override string ToString() => $"\"{Name}\"";
}