namespace Hostr.Domain;

public static class TaxRate
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertTaxRate", "taxRates");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateTaxRate", "taxRates");

    public static DB.Record Make(Cx cx, DB.Record type, decimal percentage)
    {
        var r = new DB.Record();
        r.Set(cx.DB.TaxRateType, type);
        r.Set(cx.DB.TaxRateStartsAt, DateTime.MinValue);
        r.Set(cx.DB.TaxRateEndsAt, DateTime.MaxValue);
        r.Set(cx.DB.TaxRatePercentage, percentage);
        return r;
    }
}