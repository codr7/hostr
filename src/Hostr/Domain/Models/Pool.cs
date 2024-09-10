namespace Hostr.Domain.Models;

public class Pool : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Pool", Schema.Instance.Pools);
    public static Event.Type UPDATE => new Event.Update("Update Pool", Schema.Instance.Pools);

    public Pool(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Pool(Cx cx, long? id = null, string name = "", TimeSpan? defaultInterval = null) : base(cx)
    {
        Record.Set(cx.DB.PoolId, id ?? cx.DB.PoolIds.Next(cx.DBCx));
        Name = name;
        CreatedBy = cx.CurrentUser!;
        DefaultInterval = defaultInterval ?? TimeSpan.FromMinutes(24*60);
    }

    public string Name
    {
        get => Record.Get(Cx.DB.PoolName)!;
        set => Record.Set(Cx.DB.PoolName, value);
    }

    public int Capacity
    {
        get => Record.Get(Cx.DB.PoolCapacity)!;
        set => Record.Set(Cx.DB.PoolCapacity, value);
    }

    public User CreatedBy
    {
        get => new User(Cx, Record.Copy(Cx.DB.PoolCreatedBy.Columns));
        set => Record.Set(Cx.DB.PoolCreatedBy, value.Record);
    }

    public TimeSpan DefaultInterval {
        get => TimeSpan.FromMinutes(Record.Get(Cx.DB.PoolDefaultInterval));
        set => Record.Set(Cx.DB.PoolDefaultInterval, value.Minutes);
    }

    public override DB.Table[] Tables => [Cx.DB.Pools];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}