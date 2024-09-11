namespace Hostr.Domain;

public static class Charge
{
    public static readonly Event.Type INSERT = new Event.Insert("Insert Charge", Schema.Instance.Charges);
    public static readonly Event.Type UPDATE = new Event.Update("Update Charge", Schema.Instance.Charges);

    public static DB.Record Make(Cx cx, DB.Record to, Models.Product product, decimal amount, bool isGross)
    {
        var c = new DB.Record();
        c.Set(cx.DB.ChargeId, cx.DB.ChargeIds.Next(cx.DBCx));
        c.Set(cx.DB.ChargeProduct, product.Record);
        var at = DateTime.UtcNow;
        c.Set(cx.DB.ChargeAt, at);
        c.Set(cx.DB.ChargeBy, cx.CurrentUser!.Record);
        c.Set(cx.DB.ChargeTo, to);
        Console.WriteLine("CHARGE SALES TAX " + product.SalesTax.Record);
        var tr = product.SalesTax.GetRate(at);

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