using static Hostr.DB.ValueExtensions;

namespace Hostr.Domain;

public static class TaxRate
{
    public static readonly Event.Type INSERT = new Event.Insert("Insert Tax Rate", Schema.Instance.TaxRates);
    public static readonly Event.Type UPDATE = new Event.Update("Update Tax Rate", Schema.Instance.TaxRates);

    public static DB.Record Make(Cx cx, DB.Record type, decimal percentage)
    {
        var r = new DB.Record();
        r.Set(cx.DB.TaxRateType, type);
        r.Set(cx.DB.TaxRateStartsAt, DateTime.MinValue);
        r.Set(cx.DB.TaxRateEndsAt, DateTime.MaxValue);
        r.Set(cx.DB.TaxRatePercentage, percentage);
        return r;
    }

    public static decimal Get(Cx cx, DB.Record taxType, DateTime timestamp)
    {
        var rs = new DB.Query(cx.DB.TaxRates).
            Select(cx.DB.TaxRates.Columns).
            Where(cx.DB.TaxRateType.Eq(taxType)).
            Where(cx.DB.TaxRateStartsAt.Lte(timestamp)).
            Where(cx.DB.TaxRateEndsAt.Gt(timestamp)).
            FindAll(cx.DBCx);

        if (rs.Length > 1) { throw new Exception("Multiple tax rates found"); }
        return rs[0].Get(cx.DB.TaxRatePercentage) / 100M;
    }

}