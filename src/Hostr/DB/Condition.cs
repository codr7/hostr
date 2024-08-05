namespace Hostr.DB;

public struct Condition
{
    public readonly string Sql;
    public readonly object[] Args;

    public Condition(string sql, object[] args)
    {
        Sql = sql;
        Args = args;
    }
    public static Condition And(params Condition[] conditions) => conditions.Aggregate((c, r) => r = r.And(c));

    public void AddArgs(List<object> result)
    {
        foreach (var a in Args) { result.Add(a); }
    }

    public Condition And(Condition other) => new Condition($"({Sql}) AND ({other.Sql})", Args.Concat(other.Args).ToArray());

    public override string ToString() => Sql;
}