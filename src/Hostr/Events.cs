namespace Hostr;

public static class Events
{
    public interface Type
    {
        void Exec(Cx cx, DB.Record evt, DB.Record? key, ref DB.Record data, DB.Tx tx);
        string Id { get; }
        DB.Table Table(Cx cx);
    }
}