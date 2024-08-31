using Hostr.DB;

namespace Hostr.Domain;

public abstract class Model : DB.Model
{
    public new readonly Cx Cx;

    public Model(Cx cx, DB.Record fields) : base(cx.DBCx, fields)
    {
        Cx = cx;
    }

    public Model(Cx cx) : base(cx.DBCx)
    {
        Cx = cx;
    }


    protected abstract Event.Type InsertEventType { get; }
    protected abstract Event.Type UpdateEventType { get; }

    public void Store()
    {
        foreach (var t in Tables)
        {
            var exists = Record.Exists(t, Cx.DBCx.Tx!);
            Cx.PostEvent(exists ? UpdateEventType : InsertEventType, exists ? Record.Copy(t.PrimaryKey.Columns) : null, ref Record);
        }
    }
}