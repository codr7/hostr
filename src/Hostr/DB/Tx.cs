namespace Hostr.DB;

public class Tx {
    public readonly Cx Cx;
    public readonly Tx? ParentTx;
    private string? savePoint = null;

    public Tx(Cx cx, Tx? parentTx) {
        Cx = cx;
        ParentTx = parentTx;
        
        if (parentTx is Tx) {
            savePoint = Guid.NewGuid().ToString();
            Exec($"SAVEPOINT {savePoint}");
        }
    }

    public void Commit() {
        if (savePoint is string sp) {
            Exec($"RELEASE SAVEPOINT {savePoint}");
        } else {
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

    public void Rollback() {
        if (savePoint is string sp) {
            Exec($"ROLLBACK TO SAVEPOINT {sp}");
        } else {
            Exec("ROLLBACK");
        }
    }
}