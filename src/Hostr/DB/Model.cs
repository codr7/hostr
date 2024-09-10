namespace Hostr.DB;

public abstract class Model
{
    public readonly Cx Cx;

    public Model(Cx cx, Record rec)
    {
        Cx = cx;
        Record = rec;
    }

    public Model(Cx cx) : this(cx, new Record()) { }

    public bool IsModified
    {
        get
        {
            foreach (var t in Tables)
            {
                foreach (var c in t.Columns)
                {
                    if (Record.GetObject(c) is object v)
                    {
                        var sv = Cx.Tx!.GetStoredValue(Record.Id, c);
                        if (sv is null || !sv.Equals(v)) { return true; }
                    }
                }
            }

            return false;
        }
    }

    public Record Record;

    public abstract Table[] Tables { get; }

    protected Model Store(object data)
    {
        foreach (var t in Tables) { t.Store(ref Record, data, Cx); }
        return this;
    }
}