using System.Collections.ObjectModel;
using Hostr.Domain;
using Hostr.Domain.Models;
using static Hostr.DB.ValueExtensions;

namespace Hostr;

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

    public readonly DB.Sequence ChargeIds;
    public readonly DB.Table Charges;
    public readonly DB.Columns.BigInt ChargeId;
    public readonly DB.Columns.Timestamp ChargeAt;
    public readonly DB.ForeignKey ChargeBy;
    public readonly DB.ForeignKey ChargeTo;
    public readonly DB.ForeignKey ChargeProduct;
    public readonly DB.Columns.Decimal ChargeNetAmount;
    public readonly DB.Columns.Decimal ChargeTaxAmount;

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
    public readonly DB.Columns.Integer PoolCapacity;
    public readonly DB.Columns.Integer PoolDefaultInterval;
    public readonly DB.Columns.Boolean PoolIsVisible;

    public readonly DB.Table Products;
    public readonly DB.Columns.BigInt ProductId;
    public readonly DB.ForeignKey ProductPool;
    public readonly DB.ForeignKey ProductSalesTax;


    public readonly DB.Table TaxRates;
    public readonly DB.ForeignKey TaxRateType;
    public readonly DB.Columns.Timestamp TaxRateStartsAt;
    public readonly DB.Columns.Timestamp TaxRateEndsAt;
    public readonly DB.Columns.Decimal TaxRatePercentage;


    public readonly DB.Table TaxTypes;
    public readonly DB.Columns.Text TaxTypeName;

    public readonly DB.Table Units;
    public readonly DB.Columns.BigInt UnitId;
    public readonly DB.ForeignKey UnitPool;
    public readonly DB.Columns.Boolean UnitUseCheckIn;
    public readonly DB.Columns.Boolean UnitUseCheckOut;
    public readonly DB.Columns.Boolean UnitUseClean;

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

        Users.BeforeInsert += (ref DB.Record rec, object cx) =>
        {
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

        TaxTypes = new DB.Table(this, "taxTypes");
        TaxTypeName = new DB.Columns.Text(TaxTypes, "name", primaryKey: true);

        TaxRates = new DB.Table(this, "taxRates");
        TaxRateType = new DB.ForeignKey(TaxRates, "type", TaxTypes, primaryKey: true);
        TaxRateStartsAt = new DB.Columns.Timestamp(TaxRates, "startsAt", primaryKey: true);
        TaxRateEndsAt = new DB.Columns.Timestamp(TaxRates, "endsAt");
        TaxRatePercentage = new DB.Columns.Decimal(TaxRates, "percentage");

        PoolIds = new DB.Sequence(this, "poolIds", SEQUENCE_OFFS);
        Pools = new DB.Table(this, "pools");
        PoolId = new DB.Columns.BigInt(Pools, "id", primaryKey: true);
        PoolName = new DB.Columns.Text(Pools, "name");
        PoolNameIndex = new DB.Index(Pools, "nameIndex", [PoolName]);
        PoolCreatedAt = new DB.Columns.Timestamp(Pools, "createdAt");
        PoolCreatedBy = new DB.ForeignKey(Pools, "createdBy", Users);
        PoolOwnedBy = new DB.ForeignKey(Pools, "ownedBy", Users);
        PoolOwnedByNameKey = new DB.Key(Pools, "ownedByNameKey", [PoolOwnedBy, PoolName]);
        PoolCapacity = new DB.Columns.Integer(Pools, "capacity", defaultValue: 0);
        PoolDefaultInterval = new DB.Columns.Integer(Pools, "defaultInterval", defaultValue: 24 * 60);
        PoolIsVisible = new DB.Columns.Boolean(Pools, "isVisible", defaultValue: true);

        Pools.BeforeInsert += (ref DB.Record rec, object cx) =>
        {
            rec.Set(PoolCreatedAt, DateTime.UtcNow);
            rec.Copy(ref rec, PoolCreatedBy.Columns.Zip(PoolOwnedBy.Columns).ToArray(), force: true);
        };

        Products = new DB.Table(this, "products");
        ProductId = new DB.Columns.BigInt(Products, "id", primaryKey: true);
        ProductPool = new DB.ForeignKey(Products, "pool", Pools, [(ProductId, PoolId)]);
        ProductSalesTax = new DB.ForeignKey(Products, "salesTax", TaxTypes);

        Products.BeforeInsert += (ref DB.Record rec, object cx) =>
         {
             var p = new DB.Record();
             rec.Copy(ref p, Pools.Columns);
             p.Set(PoolId, rec.Get(ProductId));
             (cx as Cx)!.PostEvent(Pool.INSERT, null, ref p);
         };

        ChargeIds = new DB.Sequence(this, "chargeIds", SEQUENCE_OFFS);
        Charges = new DB.Table(this, "charges");
        ChargeId = new DB.Columns.BigInt(Charges, "id", primaryKey: true);
        ChargeAt = new DB.Columns.Timestamp(Charges, "at");
        ChargeBy = new DB.ForeignKey(Charges, "by", Users);
        ChargeTo = new DB.ForeignKey(Charges, "to", Users);
        ChargeProduct = new DB.ForeignKey(Charges, "product", Products);
        ChargeNetAmount = new DB.Columns.Decimal(Charges, "netAmount");
        ChargeTaxAmount = new DB.Columns.Decimal(Charges, "taxAmount");

        Units = new DB.Table(this, "units");
        UnitId = new DB.Columns.BigInt(Units, "id", primaryKey: true);
        UnitPool = new DB.ForeignKey(Units, "pool", Pools, [(UnitId, PoolId)]);
        UnitUseCheckIn = new DB.Columns.Boolean(Units, "useCheckIn", defaultValue: false);
        UnitUseCheckOut = new DB.Columns.Boolean(Units, "useCheckOut", defaultValue: false);
        UnitUseClean = new DB.Columns.Boolean(Units, "useClean", defaultValue: false);

        Units.BeforeInsert += (ref DB.Record rec, object cx) =>
        {
            var p = new DB.Record();
            rec.Copy(ref p, Pools.Columns);
            p.Set(PoolId, rec.Get(UnitId));
            p.Set(PoolCapacity, 1);
            (cx as Cx)!.PostEvent(Pool.INSERT, null, ref p);
        };

        Units.AfterUpdate += (rec, cx) =>
        {
            var id = rec.Get(UnitId);
            var p = Pools.FindFirst(PoolId.Eq(id), (cx as Cx)!.DBCx);
            if (p is null) { throw new Exception($"Pool not found for unit: {id}"); }
            var pp = (DB.Record)p;
            rec.Copy(ref pp, Pools.Columns);
            (cx as Cx)!.PostEvent(Pool.UPDATE, null, ref pp);
        };

        Calendars = new DB.Table(this, "calendars");
        CalendarPool = new DB.ForeignKey(Calendars, "pool", Pools, primaryKey: true);
        CalendarStartsAt = new DB.Columns.Timestamp(Calendars, "startsAt", primaryKey: true);
        CalendarEndsAt = new DB.Columns.Timestamp(Calendars, "endsAt");
        CalendarUpdatedAt = new DB.Columns.Timestamp(Calendars, "updatedAt");
        CalendarUpdatedBy = new DB.ForeignKey(Calendars, "updatedBy", Users);
        CalendarTotal = new DB.Columns.Integer(Calendars, "total");
        CalendarUsed = new DB.Columns.Integer(Calendars, "used");

        DB.Table.BeforeHandler calendarsBefore = (ref DB.Record rec, object cx) =>
        {
            rec.Set(CalendarUpdatedAt, DateTime.UtcNow);
            rec.Set(CalendarUpdatedBy, (cx as Cx)!.CurrentUser!.Record);
        };

        Calendars.BeforeInsert += calendarsBefore;
        Calendars.BeforeUpdate += calendarsBefore;

        Pools.AfterInsert += (rec, _cx) =>
        {
            var cx = (Cx)_cx;
            var c = Calendar.Make(cx, rec);
            cx.PostEvent(Calendar.INSERT, null, ref c);
        };

        Pools.BeforeUpdate += (ref DB.Record rec, object cx) =>
        {
            Calendar.Update((Cx)cx, rec, DateTime.MinValue, DateTime.MaxValue, total: rec.Get(PoolCapacity) - rec.GetStored(PoolCapacity, (cx as Cx)!.DBCx));
        };
    }
}
