namespace Hostr.DB;

public class Tx
{
    public readonly Cx Cx;
    public readonly Tx? ParentTx;
    private string? savePoint = null;

    public Tx(Cx cx, Tx? parentTx, string? savePoint)
    {
        Cx = cx;
        ParentTx = parentTx;
        this.savePoint = savePoint;
    }

    public void Commit()
    {
        Cx.PopTx(this);

        if (savePoint is string sp)
        {
            Exec($"RELEASE SAVEPOINT {savePoint}");
        }
        else
        {
            Exec("COMMIT");
        }
    }

    public void Exec(string statement, params object[] args)
    {
        Cx.Exec(statement, args: args);
    }

    public T ExecScalar<T>(string statement, params object[] args)
    {
        return Cx.ExecScalar<T>(statement, args: args);
    }

    public void Rollback()
    {
        Cx.PopTx(this);

        if (savePoint is string sp)
        {
            Exec($"ROLLBACK TO SAVEPOINT {sp}");
        }
        else
        {
            Exec("ROLLBACK");
        }
    }
}