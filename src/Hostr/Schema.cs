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
    public readonly DB.Columns.Integer CalendarUsed;

    public readonly DB.Sequence EventIds;
    public readonly DB.Table Events;
    public readonly DB.Columns.BigInt EventId;
    public readonly DB.Columns.Text EventType;
    public readonly DB.ForeignKey EventParent;
    public readonly DB.Columns.Timestamp EventPostedAt;
    public readonly DB.Index EventPostedAtIndex;
    public readonly DB.ForeignKey EventPostedBy;
    public readonly DB.Index EventPostedByIndex;
    public readonly DB.Columns.Jsonb EventKey;
    public readonly DB.Columns.Jsonb EventData;

    public readonly DB.Sequence PoolIds;
    public readonly DB.Table Pools;
    public readonly DB.Columns.BigInt PoolId;
    public readonly DB.Columns.Text PoolName;
    public readonly DB.Index PoolNameIndex;
    public readonly DB.Key PoolOwnedByNameKey;
    public readonly DB.Columns.Timestamp PoolCreatedAt;
    public readonly DB.ForeignKey PoolCreatedBy;

    public readonly DB.ForeignKey PoolOwnedBy;
    public readonly DB.Columns.Boolean PoolHasInfiniteCapacity;
    public readonly DB.Columns.Integer PoolDefaultInterval;
    public readonly DB.Columns.Boolean PoolIsVisible;

    public readonly DB.Table Units;
    public readonly DB.Columns.BigInt UnitId;
    public readonly DB.ForeignKey UnitPool;
    public readonly DB.Columns.Text UnitName;
    public readonly DB.Index UnitNameIndex;
    public readonly DB.Key UnitOwnedByNameKey;
    public readonly DB.Columns.Timestamp UnitCreatedAt;
    public readonly DB.ForeignKey UnitCreatedBy;
    public readonly DB.ForeignKey UnitOwnedBy;
    public readonly DB.Columns.Boolean UnitUseCheckIn;
    public readonly DB.Columns.Boolean UnitUseCheckOut;
    public readonly DB.Columns.Boolean UnitUseCleaning;

    public readonly DB.Sequence UserIds;
    public readonly DB.Table Users;
    public readonly DB.Columns.BigInt UserId;
    public readonly DB.Columns.Text UserDisplayName;
    public readonly DB.Key UserDisplayNameKey;
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
        UserDisplayNameKey = new DB.Key(Users, "displayNameKey", [UserDisplayName]);
        UserCreatedAt = new DB.Columns.Timestamp(Users, "createdAt");
        UserCreatedBy = new DB.ForeignKey(Users, "createdBy", Users, nullable: true);
        UserLoginAt = new DB.Columns.Timestamp(Users, "loginAt", nullable: true);
        UserEmail = new DB.Columns.Text(Users, "email");
        UserEmailKey = new DB.Key(Users, "emailKey", [UserEmail]);
        UserPassword = new DB.Columns.Text(Users, "password");

        Users.BeforeInsert += (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            if (!rec.Contains(UserId)) { rec.Set(UserId, UserIds.Next(tx)); }

            if ((!rec.Contains(UserDisplayName) || rec.Get(UserDisplayName) == "") && rec.Contains(UserEmail))
            {
                rec.Set(UserDisplayName, rec.Get(UserEmail!)!);
            }

            rec.Set(UserCreatedAt, DateTime.UtcNow);
        };

        EventIds = new DB.Sequence(this, "eventIds", SEQUENCE_OFFS);
        Events = new DB.Table(this, "events");
        EventId = new DB.Columns.BigInt(Events, "id", primaryKey: true);
        EventType = new DB.Columns.Text(Events, "type");
        EventParent = new DB.ForeignKey(Events, "parent", Events, nullable: true);
        EventPostedAt = new DB.Columns.Timestamp(Events, "postedAt");
        EventPostedAtIndex = new DB.Index(Events, "postedAtIndex", [EventPostedAt]);
        EventPostedBy = new DB.ForeignKey(Events, "postedBy", Users, nullable: true);
        EventPostedByIndex = new DB.Index(Events, "postedByIndex", [EventPostedBy]);
        EventKey = new DB.Columns.Jsonb(Events, "key", json.Options, nullable: true);
        EventData = new DB.Columns.Jsonb(Events, "data", json.Options, nullable: true);

        PoolIds = new DB.Sequence(this, "poolIds", SEQUENCE_OFFS);
        Pools = new DB.Table(this, "pools");
        PoolId = new DB.Columns.BigInt(Pools, "id", primaryKey: true);
        PoolName = new DB.Columns.Text(Pools, "name");
        PoolNameIndex = new DB.Index(Pools, "nameIndex", [PoolName]);
        PoolCreatedAt = new DB.Columns.Timestamp(Pools, "createdAt");
        PoolCreatedBy = new DB.ForeignKey(Pools, "createdBy", Users);
        PoolOwnedBy = new DB.ForeignKey(Pools, "ownedBy", Users);
        PoolOwnedByNameKey = new DB.Key(Pools, "ownedByNameKey", [PoolOwnedBy, PoolName]);
        PoolHasInfiniteCapacity = new DB.Columns.Boolean(Pools, "hasInfiniteCapacity", defaultValue: false);
        PoolDefaultInterval = new DB.Columns.Integer(Pools, "defaultInterval", defaultValue: 24 * 60);
        PoolIsVisible = new DB.Columns.Boolean(Pools, "isVisible", defaultValue: true);

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
        UnitNameIndex = new DB.Index(Units, "nameIndex", [UnitName]);
        UnitCreatedAt = new DB.Columns.Timestamp(Units, "createdAt");
        UnitCreatedBy = new DB.ForeignKey(Units, "createdBy", Users);
        UnitOwnedBy = new DB.ForeignKey(Units, "ownedBy", Users);
        UnitOwnedByNameKey = new DB.Key(Units, "ownedByNameKey", [UnitOwnedBy, UnitName]);
        UnitUseCheckIn = new DB.Columns.Boolean(Units, "useCheckIn", defaultValue: false);
        UnitUseCheckOut = new DB.Columns.Boolean(Units, "useCheckOut", defaultValue: false);
        UnitUseCleaning = new DB.Columns.Boolean(Units, "useCleaning", defaultValue: false);


        Units.BeforeInsert += (ref DB.Record rec, object cx, DB.Tx tx) =>
        {
            if (!rec.Contains(UnitId)) { rec.Set(UnitId, PoolIds.Next(tx)); }
            rec.Set(UnitCreatedAt, DateTime.UtcNow);
            rec.Copy(ref rec, UnitCreatedBy.Columns.Zip(UnitOwnedBy.Columns).ToArray());

            var p = new DB.Record();
            p.Set(PoolId, rec.Get(UnitId));
            p.Set(PoolName, Guid.NewGuid().ToString());
            rec.Copy(ref p, UnitCreatedBy.Columns.Zip(PoolCreatedBy.Columns).ToArray());
            p.Set(PoolIsVisible, false);
            (cx as Cx)!.PostEvent(Pool.INSERT, null, ref p, tx);
        };

        Calendars = new DB.Table(this, "calendars");
        CalendarPool = new DB.ForeignKey(Calendars, "pool", Pools, primaryKey: true);
        CalendarStartsAt = new DB.Columns.Timestamp(Calendars, "startsAt", primaryKey: true);
        CalendarEndsAt = new DB.Columns.Timestamp(Calendars, "endsAt");
        CalendarUpdatedAt = new DB.Columns.Timestamp(Calendars, "updatedAt");
        CalendarUpdatedBy = new DB.ForeignKey(Calendars, "updatedBy", Users);
        CalendarTotal = new DB.Columns.Integer(Calendars, "total");
        CalendarUsed = new DB.Columns.Integer(Calendars, "used");

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
