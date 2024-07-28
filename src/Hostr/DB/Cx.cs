namespace Hostr.DB;

using Npgsql;

public class Cx
{
    private string host;
    private string database;
    private string user;
    private string password;
    private NpgsqlDataSource? source;
    private Tx? tx = null;


    public Cx(string host, string database, string user, string password)
    {
        this.host = host;
        this.database = database;
        this.user = user;
        this.password = password;
    }

    public void Connect()
    {
        source = NpgsqlDataSource.Create($"Host={host};Database={database};Username={user};Password={password}");
    }

    public void Disconnect()
    {
        source = null;
    }

    public Tx StartTx()
    {
        string? sp = null;

        if (tx is Tx) {
            sp = Guid.NewGuid().ToString();
            Exec($"SAVEPOINT {sp}");
        } else {
            Exec("BEGIN");
        }

        tx = new Tx(this, tx, sp);
        return tx;
    }

    internal NpgsqlCommand PrepareCommand(string statement, params object[] args)
    {
        Console.WriteLine(statement);
        if (source is null) { throw new Exception("Not connected"); }
        var cmd = source.CreateCommand(statement);
        foreach (var a in args) { cmd.Parameters.AddWithValue(a); }
        return cmd;
    }

    internal void Exec(string statement, params object[] args)
    {
        PrepareCommand(statement, args: args).ExecuteNonQuery();
    }

    internal T ExecScalar<T>(string statement, params object[] args)
    {
#pragma warning disable CS8600
#pragma warning disable CS8603
        return (T)PrepareCommand(statement, args: args).ExecuteScalar(); ;
#pragma warning restore CS8603
#pragma warning restore CS8600
    }

    internal void PopTx(Tx tx) {
        if (this.tx != tx) { throw new Exception("Transaction finished out of order"); }
        this.tx = tx.ParentTx;
    }
}