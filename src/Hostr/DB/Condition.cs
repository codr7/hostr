namespace Hostr.DB;

public struct Condition {
    public readonly string SQL;
    public readonly object[] Args;

    public Condition(string sql, params object[] args) {
        SQL = sql;
        Args = args;
    }
    public static Condition And(params Condition[] conditions) {
        return conditions.Aggregate((c, r) => r = r.And(c));
    }

    public Condition And(Condition other) {
        return new Condition($"({SQL}) AND ({other.SQL})", Args.Concat(other.Args));
    }
}