namespace Hostr.DB;

public abstract class Model
{
    public readonly Cx Cx;
    protected Record rec;

    public Model(Cx cx, Record rec)
    {
        Cx = cx;
        this.rec = rec;
    }

    public Model(Cx cx) : this(cx, new Record()) { }

    public bool Modified
    {
        get
        {
            foreach (var t in Tables)
            {
                foreach (var c in t.Columns)
                {
                    if (rec.GetObject(c) is object v)
                    {
                        var sv = Cx.Tx!.GetStoredValue(rec.Id, c);
                        if (sv is null || !sv.Equals(v)) { return true; }
                    }
                }
            }

            return false;
        }
    }

    public abstract Table[] Tables { get; }

    protected void Store(object data)
    {
        foreach (var t in Tables) { t.Store(ref rec, data, Cx); }
    }
}