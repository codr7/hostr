namespace Hostr.Domain.Models;

public class Unit : Pool
{
    public new static Event.Type INSERT => new Event.Insert("Insert Unit", Schema.Instance.Units);
    public new static Event.Type UPDATE => new Event.Update("Update Unit", Schema.Instance.Units);

    public Unit(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Unit(Cx cx, long? id = null, string name = "", bool useCheckIn = false, bool useCheckOut = false, bool useClean = false) : base(cx, id: id, name: name)
    {
        Record.Set(cx.DB.UnitId, Record.Get(cx.DB.PoolId));
        Name = name;
        CreatedBy = cx.CurrentUser!;
        UseCheckIn = useCheckIn;
        UseCheckOut = useCheckOut;
        UseClean = useClean;
    }

    public bool UseCheckIn
    {
        get => Record.Get(Cx.DB.UnitUseCheckIn);
        set => Record.Set(Cx.DB.UnitUseCheckIn, value);
    }

    public bool UseCheckOut
    {
        get => Record.Get(Cx.DB.UnitUseCheckOut);
        set => Record.Set(Cx.DB.UnitUseCheckOut, value);
    }

    public bool UseClean
    {
        get => Record.Get(Cx.DB.UnitUseClean);
        set => Record.Set(Cx.DB.UnitUseClean, value);
    }

    public override DB.Table[] Tables => [Cx.DB.Pools, Cx.DB.Units];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}