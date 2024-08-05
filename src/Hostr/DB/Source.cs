namespace Hostr.DB;

public interface Source
{
    void AddSourceArgs(List<object> result) { }
    string SourceSql { get; }
}