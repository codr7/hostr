using Hostr;
using Hostr.DB;

public static class Events
{
    public interface Type
    {
        void Exec(Schema db, Record evt, Record key, Record data, Tx tx);
        string Id { get; }
        Table Table(Schema db);
    }
}