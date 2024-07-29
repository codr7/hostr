namespace Hostr.DB;

public abstract class Definition : IComparable<Definition>
{
    public readonly string Name;

    public Definition(string name)
    {
        Name = name;
    }

    public int CompareTo(Definition? other)
    {
        if (other is Definition o)
        {
            return Name.CompareTo(o.Name);
        }

        return -1;
    }

    public virtual void Create(Tx tx)
    {
        tx.Exec(CreateSQL);
    }

    public virtual string CreateSQL => $"CREATE {DefinitionType} \"{Name}\"";

    public abstract string DefinitionType { get; }

    public virtual void Drop(Tx tx)
    {
        tx.Exec(DropSQL);
    }

    public virtual void DropIfExists(Tx tx) {
        if (Exists(tx)) { Drop(tx); }
    }

    public virtual string DropSQL => $"DROP {DefinitionType} \"{Name}\"";

    public abstract bool Exists(Tx tx);
 
    public virtual void Sync(Tx tx)
    {
        if (!Exists(tx)) { Create(tx); }
    }
 
    public override string ToString() => $"\"{Name}\"";
}