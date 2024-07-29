namespace Hostr;

public class Schema
{
    public static readonly int SEQUENCE_OFFS = 100;

    public readonly DB.Table Calendars;
    public readonly DB.ForeignKey CalendarPool;
    public readonly DB.Columns.Timestamp CalendarStartsAt;
    public readonly DB.Columns.Timestamp CalendarEndsAt;
    public readonly DB.Columns.Timestamp CalendarUpdatedAt;
    public readonly DB.ForeignKey CalendarUpdatedBy;
    public readonly DB.Columns.Integer CalendarTotal;
    public readonly DB.Columns.Integer CalendarBooked;
    public readonly DB.Columns.Text CalendarLabel;

    public readonly DB.Sequence PoolIds;

    public readonly DB.Table Pools;
    public readonly DB.Columns.BigInt PoolId;
    public readonly DB.Columns.Text PoolName;
    public readonly DB.Key PoolNameKey;
    public readonly DB.Columns.Timestamp PoolCreatedAt;
    public readonly DB.ForeignKey PoolCreatedBy;
    public readonly DB.Columns.Boolean PoolInfiniteCapacity;

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
        UserId = new DB.Columns.BigInt(Users, "id", primaryKey: true);
        UserName = new DB.Columns.Text(Users, "name");
        UserNameKey = new DB.Key(Users, "nameKey", [UserName]);
        UserCreatedAt = new DB.Columns.Timestamp(Users, "createdAt");
        UserCreatedBy = new DB.ForeignKey(Users, "createdBy", Users, nullable: true);
        UserEmail = new DB.Columns.Text(Users, "email");
        UserEmailKey = new DB.Key(Users, "emailKey", [UserEmail]);
        UserPassword = new DB.Columns.Text(Users, "password");

        Users.BeforeInsert += (ref DB.Record rec, DB.Tx tx) =>
        {
            if (!rec.Contains(UserId)) { rec.Set(UserId, UserIds.Next(tx)); }
            rec.Set(UserCreatedAt, DateTime.UtcNow);
        };

        PoolIds = new DB.Sequence("poolIds", SEQUENCE_OFFS);

        Pools = new DB.Table("pools");
        PoolId = new DB.Columns.BigInt(Pools, "id", primaryKey: true);
        PoolName = new DB.Columns.Text(Pools, "name");
        PoolNameKey = new DB.Key(Pools, "nameKey", [PoolName]);
        PoolCreatedAt = new DB.Columns.Timestamp(Pools, "createdAt");
        PoolCreatedBy = new DB.ForeignKey(Pools, "createdBy", Users);
        PoolInfiniteCapacity = new DB.Columns.Boolean(Pools, "infiniteCapacity");

        Pools.BeforeInsert += (ref DB.Record rec, DB.Tx tx) =>
        {
            if (!rec.Contains(PoolId)) { rec.Set(PoolId, PoolIds.Next(tx)); }
            rec.Set(PoolCreatedAt, DateTime.UtcNow);
        };

        Calendars = new DB.Table("calendars");
        CalendarPool = new DB.ForeignKey(Calendars, "pool", Pools, primaryKey: true);
        CalendarStartsAt = new DB.Columns.Timestamp(Calendars, "startsAt", primaryKey: true);
        CalendarEndsAt = new DB.Columns.Timestamp(Calendars, "endsAt");
        CalendarUpdatedAt = new DB.Columns.Timestamp(Calendars, "updatedAt");
        CalendarUpdatedBy = new DB.ForeignKey(Calendars, "updatedBy", Users, nullable: true);
        CalendarTotal = new DB.Columns.Integer(Calendars, "total");
        CalendarBooked = new DB.Columns.Integer(Calendars, "booked");
        CalendarLabel = new DB.Columns.Text(Calendars, "label");

        Calendars.BeforeUpdate += (ref DB.Record rec, DB.Tx tx) =>
        {
            rec.Set(CalendarUpdatedAt, DateTime.UtcNow);
        };

        Pools.AfterInsert += (rec, tx) =>
        {
            var cal = new DB.Record();
            CalendarPool.Copy(rec, ref cal);
            CalendarUpdatedBy.Copy(rec, PoolCreatedBy, ref cal);
            cal.Set(CalendarStartsAt, DateTime.MinValue);
            cal.Set(CalendarEndsAt, DateTime.MaxValue);
            Calendars.Insert(cal, tx);
        };
    }
}
