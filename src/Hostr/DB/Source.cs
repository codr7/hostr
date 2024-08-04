namespace Hostr.DB;

public interface Source
{
    string SourceSql { get; }
    void AddSourceArgs(List<object> result) { }
}