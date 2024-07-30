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
    public readonly DB.Columns.Boolean PoolCheckIn;
    public readonly DB.Columns.Boolean PoolCheckOut;
    public readonly DB.Columns.Boolean PoolVisible;

    public readonly DB.Table Units;
    public readonly DB.Columns.BigInt UnitId;
    public readonly DB.ForeignKey UnitPool;
    public readonly DB.Columns.Text UnitName;
    public readonly DB.Key UnitNameKey;
    public readonly DB.Columns.Timestamp UnitCreatedAt;
    public readonly DB.ForeignKey UnitCreatedBy;

    public readonly DB.Sequence UserIds;

    public readonly DB.Table Users;
    public readonly DB.Columns.BigInt UserId;
    public readonly DB.Columns.Text UserName;
    public readonly DB.Key UserNameKey;
    public readonly DB.Columns.Timestamp UserCreatedAt;
    public readonly DB.ForeignKey UserCreatedBy;
    public readonly DB.Columns.Timestamp UserLoginAt;
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
        UserLoginAt = new DB.Columns.Timestamp(Users, "loginAt");
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
        PoolCheckIn = new DB.Columns.Boolean(Pools, "checkIn", false);
        PoolCheckOut = new DB.Columns.Boolean(Pools, "checkOut", false);
        PoolVisible = new DB.Columns.Boolean(Pools, "visible", defaultValue: true);

        Pools.BeforeInsert += (ref DB.Record rec, DB.Tx tx) =>
        {
            if (!rec.Contains(PoolId)) { rec.Set(PoolId, PoolIds.Next(tx)); }
            rec.Set(PoolCreatedAt, DateTime.UtcNow);
        };

        Units = new DB.Table("units");
        UnitId = new DB.Columns.BigInt(Units, "id", primaryKey: true);
        UnitPool = new DB.ForeignKey(Units, "pool", Pools, [(UnitId, PoolId)]);
        UnitName = new DB.Columns.Text(Units, "name");
        UnitNameKey = new DB.Key(Units, "nameKey", [UnitName]);
        UnitCreatedAt = new DB.Columns.Timestamp(Units, "createdAt");
        UnitCreatedBy = new DB.ForeignKey(Units, "createdBy", Users);

        Units.BeforeInsert += (ref DB.Record rec, DB.Tx tx) =>
        {
            if (!rec.Contains(UnitId)) { rec.Set(UnitId, PoolIds.Next(tx)); }
            rec.Set(UnitCreatedAt, DateTime.UtcNow);

            var p = new DB.Record();
            p.Set(PoolId, rec.Get(UnitId));
            p.Set(PoolName, Guid.NewGuid().ToString());
            rec.Copy(ref p, UnitCreatedBy.Columns.Zip(PoolCreatedBy.Columns).ToArray());
            p.Set(PoolVisible, false);
            Pools.Insert(p, tx);
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
            var c = new DB.Record();
            rec.Copy(ref c, CalendarPool.ForeignColumns.Zip(CalendarPool.Columns).ToArray()); 
            rec.Copy(ref c, PoolCreatedBy.Columns.Zip(CalendarUpdatedBy.Columns).ToArray());
            c.Set(CalendarStartsAt, DateTime.MinValue);
            c.Set(CalendarEndsAt, DateTime.MaxValue);
            Calendars.Insert(c, tx);
        };
    }
}
