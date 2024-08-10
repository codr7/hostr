using static Hostr.DB.ValueExtensions;

namespace Hostr.Domain;

public static class TaxType
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertTaxType", "taxTypes");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateTaxType", "taxTypes");

    public static DB.Record Make(Cx cx, string name = "")
    {
        var t = new DB.Record();
        t.Set(cx.DB.TaxTypeName, name);
        return t;
    }

    public static decimal CalculateTax(Cx cx, DB.Record taxType, DateTime timestamp, decimal netAmount)
    {
        var rs = new DB.Query(cx.DB.TaxRates).
            Select(cx.DB.TaxRates.Columns).
            Where(cx.DB.TaxRateType.Eq(taxType)).
            Where(cx.DB.TaxRateStartsAt.Lte(timestamp)).
            Where(cx.DB.TaxRateEndsAt.Gt(timestamp)).
            FindAll(cx.DBCx.Tx!);

        if (rs.Length > 1) { throw new Exception("Multiple tax rates found"); }
        return rs[0].Get(cx.DB.TaxRatePercentage) / 100M;
    }
}