namespace Hostr.Domain.Models;

public class Pool : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Pool", "pools");
    public static Event.Type UPDATE => new Event.Update("Update Pool", "pools");
    public static readonly int PASSWORD_ITERS = 10000;

    public Pool(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Pool(Cx cx, string name = "") : base(cx)
    {
        Record.Set(cx.DB.PoolId, cx.DB.PoolIds.Next(cx.DBCx));
        Name = name;
#pragma warning disable CS8601
        CreatedBy = cx.CurrentUser;
#pragma warning restore CS8601
    }

    public string Name
    {
        get => Record.Get(Cx.DB.PoolName)!;
        set => Record.Set(Cx.DB.PoolName, value);
    }

    public User CreatedBy
    {
        get => new User(Cx, Record.Copy(Cx.DB.PoolCreatedBy.Columns));
        set => Record.Set(Cx.DB.PoolCreatedBy, value.Record);
    }

    public string Password
    {
        get => Record.Get(Cx.DB.UserPassword)!;
        set => Record.Set(Cx.DB.UserPassword, (value == "") ? "" : Hostr.Password.Hash(value, PASSWORD_ITERS));
    }

    public override DB.Table[] Tables => [Cx.DB.Users];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}