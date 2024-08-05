using Npgsql;
using System.Text.RegularExpressions;

namespace Hostr.DB;

public class Cx : ValueStore
{
    public static string MakeSavePoint() => $"SP{new string(Guid.NewGuid().ToString().Where(c => c != '-').ToArray())}";

    private string host;
    private string database;
    private string user;
    private string password;
    private NpgsqlDataSource? source;
    private NpgsqlConnection? connection;
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
        connection = source.OpenConnection();
    }

    public void Disconnect()
    {
        source = null;
    }

    public Tx StartTx()
    {
        string? sp = null;

        if (tx is null)
        {
            Exec("BEGIN", []);
        }
        else
        {
            sp = MakeSavePoint();
            Exec($"SAVEPOINT {sp}", []);
        }

        tx = new Tx(this, tx, sp);
        return tx;
    }

    internal NpgsqlCommand PrepareCommand(string statement, params object[] args)
    {
        statement = Regex.Replace(statement, @"\s+", " ");
        var ss = statement;
        var ai = 0;

        while (true)
        {
            var i = ss.IndexOf("$?");
            if (i == -1) { break; }
            ss = ss.Remove(i, 2).Insert(i, $"[{args[ai]}]");
            ai++;
        }

        Console.WriteLine(ss);
        var argIndex = 1;

        while (true)
        {
            var i = statement.IndexOf("$?");
            if (i == -1) { break; }
            i++;
            statement = statement.Remove(i, 1).Insert(i, $"{argIndex}");
            argIndex++;
        }

        if (connection is null) { throw new Exception("Not connected"); }
        var cmd = connection.CreateCommand();
        cmd.CommandText = statement;
        foreach (var a in args) { cmd.Parameters.AddWithValue(a); }
        return cmd;
    }

    internal void Exec(string statement, object[] args)
    {
        PrepareCommand(statement, args).ExecuteNonQuery();
    }

    internal NpgsqlDataReader ExecReader(string statement, object[] args)
    {
        return PrepareCommand(statement, args).ExecuteReader();
    }

    internal T ExecScalar<T>(string statement, object[] args)
    {
#pragma warning disable CS8600
#pragma warning disable CS8603
        return (T)PrepareCommand(statement, args).ExecuteScalar(); ;
#pragma warning restore CS8603
#pragma warning restore CS8600
    }

    internal void PopTx(Tx tx)
    {
        if (this.tx != tx) { throw new Exception("Transaction finished out of order"); }
        this.tx = tx.ParentTx;
    }
}