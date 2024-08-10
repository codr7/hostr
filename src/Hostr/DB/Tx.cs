using Npgsql;

namespace Hostr.DB;

using RecordId = ulong;

public class Tx : ValueStore, IDisposable
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
            Cx.Exec($"RELEASE SAVEPOINT {savePoint}");
        }
        else
        {
            Cx.Exec("COMMIT");
        }

        ValueStore? s = Cx.Tx;
        if (s is null) { s = Cx; }
        MoveStoredValues(s);
        finished = true;
    }

    public void Dispose()
    {
        if (!finished) { Rollback(); }
    }

    public override object? GetStoredValue(RecordId recId, Column col)
    {
        if (base.GetStoredValue(recId, col) is object v) { return v; }
        if (ParentTx is Tx ptx) { return ptx.GetStoredValue(recId, col); }
        return Cx.GetStoredValue(recId, col);
    }

    public void Rollback()
    {
        if (finished) { throw new Exception("Rollback in finished transaction"); }
        Cx.PopTx(this);

        if (savePoint is string sp)
        {
            Cx.Exec($"ROLLBACK TO SAVEPOINT {sp}");
        }
        else
        {
            Cx.Exec("ROLLBACK");
        }

        finished = true;
    }
}