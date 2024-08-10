namespace Hostr.Domain.Models;

public class User : Model
{
    public static Event.Type INSERT => new Event.Insert("InsertUser", "users");
    public static Event.Type UPDATE => new Event.Update("UpdateUser", "users");
    public static readonly int PASSWORD_ITERS = 10000;

    public User(Cx cx, DB.Record fields) : base(cx, fields) { }

    public User(Cx cx, string name = "", string email = "", string password = "") : base(cx)
    {
        rec.Set(cx.DB.UserId, cx.DB.UserIds.Next(cx.DBCx));
        DisplayName = name;
        Email = email;
        Password = password; ;
    }

    public string DisplayName
    {
        get => rec.Get(Cx.DB.UserDisplayName)!;
        set => rec.Set(Cx.DB.UserDisplayName, value);
    }

    public string Email
    {
        get => rec.Get(Cx.DB.UserEmail)!;
        set => rec.Set(Cx.DB.UserEmail, value);
    }

    public string Password
    {
        get => rec.Get(Cx.DB.UserPassword)!;
        set => rec.Set(Cx.DB.UserPassword, (value == "") ? "" : Hostr.Password.Hash(value, PASSWORD_ITERS));
    }

    public override DB.Table[] Tables => [Cx.DB.Users];

    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}