namespace Hostr.Domain;

public static class Charge
{
    public static readonly Event.Type INSERT = new Event.Insert("InsertCharge", "charges");
    public static readonly Event.Type UPDATE = new Event.Update("UpdateCharge", "charges");

    public static DB.Record Make(Cx cx, DB.Record product, decimal amount, bool isGross)
    {
        var c = new DB.Record();
        c.Set(cx.DB.ChargeId, cx.DB.ChargeIds.Next(cx.DBCx.Tx!));
        c.Set(cx.DB.ChargeProduct, product);
        var createdAt = DateTime.UtcNow;
        c.Set(cx.DB.ChargeCreatedAt, createdAt);
#pragma warning disable CS8629 
        c.Set(cx.DB.ChargeCreatedBy, (DB.Record)cx.CurrentUser);
#pragma warning restore CS8629

        var tr = TaxRate.Get(cx, product.Copy(cx.DB.ProductSalesTax.ColumnMap), createdAt);

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