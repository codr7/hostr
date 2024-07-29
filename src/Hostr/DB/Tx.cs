using Npgsql;

namespace Hostr.DB;

public class Tx: IDisposable
{
    public readonly Cx Cx;
    public readonly Tx? ParentTx;
    private string? savePoint = null;
    private bool finished = false;

    public Tx(Cx cx, Tx? parentTx, string? savePoint)
    {
        Cx = cx;
        ParentTx = parentTx;
        this.savePoint = savePoint;
    }

    public void Commit()
    {
        if (finished) { throw new Exception("Commit in finished transaction"); }
        Cx.PopTx(this);

        if (savePoint is string sp)
        {
            Exec($"RELEASE SAVEPOINT {savePoint}");
        }
        else
        {
            Exec("COMMIT");
        }

        finished = true;
    }

    public void Dispose() {
        if (!finished) { Rollback(); }
    }

    public void Exec(string statement, params object[] args)
    {
        Cx.Exec(statement, args: args);
    }

    public NpgsqlDataReader ExecReader(string statement, params object[] args)
    {
        return Cx.ExecReader(statement, args: args);
    }

    public T ExecScalar<T>(string statement, params object[] args)
    {
        return Cx.ExecScalar<T>(statement, args: args);
    }

    public void Rollback()
    {
        if (finished) { throw new Exception("Rollback in finished transaction"); }
        Cx.PopTx(this);

        if (savePoint is string sp)
        {
            Exec($"ROLLBACK TO SAVEPOINT {sp}");
        }
        else
        {
            Exec("ROLLBACK");
        }

        finished = true;
    }
}