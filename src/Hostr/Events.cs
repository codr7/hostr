namespace Hostr;

public static class Events
{
    public interface Type
    {
        DB.Record Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx);
        string Id { get; }
        DB.Table Table(Cx cx);
    }
}