namespace Hostr.DB;

public class Schema
{
    public Definition? this[string name] => defLookup[name];

    public void DropIfExists(Tx tx)
    {
        foreach (var d in defs.ToArray().Reverse()) { d.DropIfExists(tx); }
    }

    public void Sync(Tx tx)
    {
        foreach (var d in defs) { d.Sync(tx); }
    }

    internal void AddDefinition(Definition d)
    {
        defs.Add(d);
        defLookup[d.Name] = d;
    }

    private readonly List<Definition> defs = new List<Definition>();

    private readonly Dictionary<string, Definition> defLookup = new Dictionary<string, Definition>();
}