namespace Hostr.Domain;

public static class Charge
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertCharge", "charges");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateCharge", "charges");

    public static DB.Record Make(Cx cx, DB.Record to, DB.Record product, decimal amount, bool isGross)
    {
        var c = new DB.Record();
        c.Set(cx.DB.ChargeId, cx.DB.ChargeIds.Next(cx.DBCx.Tx!));
        c.Set(cx.DB.ChargeProduct, product);
        var at = DateTime.UtcNow;
        c.Set(cx.DB.ChargeAt, at);
#pragma warning disable CS8629 
        c.Set(cx.DB.ChargeBy, (DB.Record)cx.CurrentUser);
#pragma warning restore CS8629
        c.Set(cx.DB.ChargeTo, to);
        var tr = TaxRate.Get(cx, product.Copy(cx.DB.ProductSalesTax.ColumnMap), at);

        if (isGross)
        {
            var na = amount / (1 + tr);
            c.Set(cx.DB.ChargeNetAmount, na);
            c.Set(cx.DB.ChargeTaxAmount, amount - na);
        }
        else
        {
            c.Set(cx.DB.ChargeNetAmount, amount);
            c.Set(cx.DB.ChargeTaxAmount, tr * amount);
        }

        return c;
    }
}