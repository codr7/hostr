namespace Hostr.DB;

public readonly record struct Join(Source left, Source right, Condition cond): Source {
    public void AddSourceArgs(List<object> result) { 
        left.AddSourceArgs(result);
        right.AddSourceArgs(result);
        cond.AddArgs(result);
    }

    public string SourceSql => $"{left.SourceSql} JOIN {right.SourceSql} ON {cond.Sql}";

}