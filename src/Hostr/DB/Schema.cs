using System.Reflection.Metadata.Ecma335;
using Npgsql.Replication;

namespace Hostr.DB;

public class Schema
{
    public Definition? this[string name] => definitions[name];

    internal void AddDefinition(Definition d) => definitions[d.Name] = d;

    private readonly Dictionary<string, Definition> definitions = new Dictionary<string, Definition>();
}