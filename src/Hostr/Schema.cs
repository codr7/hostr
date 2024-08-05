namespace Hostr;

using Hostr.Domain;

public class Schema : DB.Schema
{
    public static readonly int SEQUENCE_OFFS = 100;
    public static readonly Schema Instance = new Schema();

    public readonly DB.Table Calendars;
    public readonly DB.ForeignKey CalendarPool;
    public readonly DB.Columns.Timestamp CalendarStartsAt;
    public readonly DB.Columns.Timestamp CalendarEndsAt;
    public readonly DB.Columns.Timestamp CalendarUpdatedAt;
    public readonly DB.ForeignKey CalendarUpdatedBy;
    public readonly DB.Columns.Integer CalendarTotal;
    public readonly DB.Columns.Integer CalendarBooked;
    public readonly DB.Columns.Text CalendarLabel;

    public readonly DB.Sequence EventIds;
    public readonly DB.Table Events;
    public readonly DB.Columns.BigInt EventId;
    public readonly DB.Columns.Text EventType;
    public readonly DB.ForeignKey EventParent;
    public readonly DB.Columns.Timestamp EventPostedAt;
    public readonly DB.ForeignKey EventPostedBy;
    public readonly DB.Columns.Jsonb EventKey;
    public readonly DB.Columns.Jsonb EventData;

    public readonly DB.Sequence PoolIds;
    public readonly DB.Table Pools;
    public readonly DB.Columns.BigInt PoolId;
    public readonly DB.Columns.Text PoolName;
    public readonly DB.Key PoolOwnedNameKey;
    public readonly DB.Columns.Timestamp PoolCreatedAt;
    public readonly DB.ForeignKey PoolCreatedBy;

    public readonly DB.ForeignKey PoolOwnedBy;
    public readonly DB.Columns.Boolean PoolInfiniteCapacity;
    public readonly DB.Columns.Boolean PoolCheckIn;
    public readonly DB.Columns.Boolean PoolCheckOut;
    public readonly DB.Columns.Boolean PoolVisible;

    public readonly DB.Table Units;
    public readonly DB.Columns.BigInt UnitId;
    public readonly DB.ForeignKey UnitPool;
    public readonly DB.Columns.Text UnitName;
    public readonly DB.Key UnitOwnedNameKey;
    public readonly DB.Columns.Timestamp UnitCreatedAt;
    public readonly DB.ForeignKey UnitCreatedBy;
    public readonly DB.ForeignKey UnitOwnedBy;

    public readonly DB.Sequence UserIds;
    public readonly DB.Table Users;
    public readonly DB.Columns.BigInt UserId;
    public readonly DB.Columns.Text UserDisplayName;
    public readonly DB.Columns.Timestamp UserCreatedAt;
    public readonly DB.ForeignKey UserCreatedBy;
    public readonly DB.Columns.Timestamp UserLoginAt;
    public readonly DB.Columns.Text UserEmail;
    public readonly DB.Key UserEmailKey;
    public readonly DB.Columns.Text UserPassword;

    public Schema()
    {
        var json = new Json(this);

        UserIds = new DB.Sequence(this, "userIds", SEQUENCE_OFFS);
        Users = new DB.Table(this, "users");
        UserId = new DB.Columns.BigInt(Users, "id", primaryKey: true);
        UserDisplayName = new DB.Columns.Text(Users, "displayName");
        UserCreatedAt = new DB.Columns.Timestamp(Users, "createdAt");
        UserCreatedBy = new DB.ForeignKey(Users, "createdBy", Users, nullable: true);
        UserLoginAt = new DB.Columns.Timestamp(Users, "loginAt", nullable: true);
        UserEmail = new DB.Columns.Text(Users, "email");
        UserEmailKey = new DB.Key(Users, "emailKey", [UserEmail]);
        UserPassword = new DB.Columns.Text(Users, "password");

        Users.BeforeInsert += (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            if (!rec.Contains(UserId)) { rec.Set(UserId, UserIds.Next(tx)); }
            rec.Set(UserCreatedAt, DateTime.UtcNow);
        };

        EventIds = new DB.Sequence(this, "eventIds", SEQUENCE_OFFS);
        Events = new DB.Table(this, "events");
        EventId = new DB.Columns.BigInt(Events, "id", primaryKey: true);
        EventType = new DB.Columns.Text(Events, "type");
        EventParent = new DB.ForeignKey(Events, "parent", Events, nullable: true);
        EventPostedAt = new DB.Columns.Timestamp(Events, "postedAt");
        EventPostedBy = new DB.ForeignKey(Events, "postedBy", Users, nullable: true);
        EventKey = new DB.Columns.Jsonb(Events, "key", json.Options, nullable: true);
        EventData = new DB.Columns.Jsonb(Events, "data", json.Options, nullable: true);

        PoolIds = new DB.Sequence(this, "poolIds", SEQUENCE_OFFS);
        Pools = new DB.Table(this, "pools");
        PoolId = new DB.Columns.BigInt(Pools, "id", primaryKey: true);
        PoolName = new DB.Columns.Text(Pools, "name");
        PoolOwnedNameKey = new DB.Key(Pools, "nameKey", [PoolName]);
        PoolCreatedAt = new DB.Columns.Timestamp(Pools, "createdAt");
        PoolCreatedBy = new DB.ForeignKey(Pools, "createdBy", Users);
        PoolOwnedBy = new DB.ForeignKey(Pools, "ownedBy", Users);
        PoolInfiniteCapacity = new DB.Columns.Boolean(Pools, "infiniteCapacity", defaultValue: false);
        PoolCheckIn = new DB.Columns.Boolean(Pools, "checkIn", defaultValue: false);
        PoolCheckOut = new DB.Columns.Boolean(Pools, "checkOut", defaultValue: false);
        PoolVisible = new DB.Columns.Boolean(Pools, "visible", defaultValue: true);
        PoolOwnedNameKey = new DB.Key(Pools, "ownedNameKey", [PoolOwnedBy, PoolName]);

        Pools.BeforeInsert += (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            if (!rec.Contains(PoolId)) { rec.Set(PoolId, PoolIds.Next(tx)); }
            rec.Set(PoolCreatedAt, DateTime.UtcNow);
            rec.Copy(ref rec, PoolCreatedBy.Columns.Zip(PoolOwnedBy.Columns).ToArray());
        };

        Units = new DB.Table(this, "units");
        UnitId = new DB.Columns.BigInt(Units, "id", primaryKey: true);
        UnitPool = new DB.ForeignKey(Units, "pool", Pools, [(UnitId, PoolId)]);
        UnitName = new DB.Columns.Text(Units, "name");
        UnitCreatedAt = new DB.Columns.Timestamp(Units, "createdAt");
        UnitCreatedBy = new DB.ForeignKey(Units, "createdBy", Users);
        UnitOwnedBy = new DB.ForeignKey(Units, "ownedBy", Users);
        UnitOwnedNameKey = new DB.Key(Units, "ownedNameKey", [UnitOwnedBy, UnitName]);

        Units.BeforeInsert += (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            if (!rec.Contains(UnitId)) { rec.Set(UnitId, PoolIds.Next(tx)); }
            rec.Set(UnitCreatedAt, DateTime.UtcNow);
            rec.Copy(ref rec, UnitCreatedBy.Columns.Zip(UnitOwnedBy.Columns).ToArray());

            var p = new DB.Record();
            p.Set(PoolId, rec.Get(UnitId));
            p.Set(PoolName, Guid.NewGuid().ToString());
            rec.Copy(ref p, UnitCreatedBy.Columns.Zip(PoolCreatedBy.Columns).ToArray());
            p.Set(PoolVisible, false);
            (cx as Cx)!.PostEvent(Pool.INSERT, null, ref p, tx);
        };

        Calendars = new DB.Table(this, "calendars");
        CalendarPool = new DB.ForeignKey(Calendars, "pool", Pools, primaryKey: true);
        CalendarStartsAt = new DB.Columns.Timestamp(Calendars, "startsAt", primaryKey: true);
        CalendarEndsAt = new DB.Columns.Timestamp(Calendars, "endsAt");
        CalendarUpdatedAt = new DB.Columns.Timestamp(Calendars, "updatedAt");
        CalendarUpdatedBy = new DB.ForeignKey(Calendars, "updatedBy", Users);
        CalendarTotal = new DB.Columns.Integer(Calendars, "total");
        CalendarBooked = new DB.Columns.Integer(Calendars, "booked");
        CalendarLabel = new DB.Columns.Text(Calendars, "label");

        DB.Table.BeforeHandler beforeHandler = (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            rec.Set(CalendarUpdatedAt, DateTime.UtcNow);
#pragma warning disable CS8629 
            rec.Set(CalendarUpdatedBy, (DB.Record)(cx as Cx)!.CurrentUser);
#pragma warning restore CS8629
        };

        Calendars.BeforeInsert += beforeHandler;
        Calendars.BeforeUpdate += beforeHandler;

        Pools.AfterInsert += (rec, _cx, tx) =>
        {
            var cx = (Cx)_cx;
            var c = Calendar.Make(cx, rec);
            cx.PostEvent(Calendar.INSERT, null, ref c, tx);
        };
    }
}
