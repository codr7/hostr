namespace Hostr;

public class Schema
{
    public static readonly int SEQUENCE_OFFS = 100;

    public readonly DB.Sequence UserIds;
    public readonly DB.Table Users;
    public readonly DB.Columns.BigInt UserId;
    public readonly DB.Columns.Text UserName;
    public readonly DB.Key UserNameKey;
    public readonly DB.Columns.Timestamp UserCreatedAt;
    public readonly DB.ForeignKey UserCreatedBy;
    public readonly DB.Columns.Text UserEmail;
    public readonly DB.Key UserEmailKey;
    public readonly DB.Columns.Text UserPassword;

    public Schema()
    {
        UserIds = new DB.Sequence("userIds", SEQUENCE_OFFS);

        Users = new DB.Table("users");
        UserId = new DB.Columns.BigInt(Users, "id") { PrimaryKey = true };
        UserName = new DB.Columns.Text(Users, "name");
        UserNameKey = new DB.Key(Users, "usersNameKey", [UserName]);
        UserCreatedAt = new DB.Columns.Timestamp(Users, "createdAt");
        UserCreatedBy = new DB.ForeignKey(Users, "createdBy", Users) { Nullable = true };
        UserEmail = new DB.Columns.Text(Users, "email");
        UserEmailKey = new DB.Key(Users, "usersEmailKey", [UserEmail]);
        UserPassword = new DB.Columns.Text(Users, "password");

        Users.BeforeInsert += (ref DB.Record rec, DB.Tx tx) =>
        {
            if (!rec.Contains(UserId)) { rec.Set(UserId, UserIds.Next(tx)); }
            rec.Set(UserCreatedAt, DateTime.UtcNow);
        };
    }
}
