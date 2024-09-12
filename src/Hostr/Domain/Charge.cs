namespace Hostr.Domain;

public class Charge : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Charge", Schema.Instance.Charges);
    public static Event.Type UPDATE => new Event.Update("Update Charge", Schema.Instance.Charges);

    public Charge(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Charge(Cx cx, User to, Product product, decimal amount, bool isGross, DateTime? at = null, User? by = null) : base(cx)
    {
        var t = at ?? DateTime.UtcNow;

        Record
            .Set(cx.DB.ChargeId, cx.DB.ChargeIds.Next(cx.DBCx))
            .Set(cx.DB.ChargeAt, t)
            .Set(cx.DB.ChargeBy, (by ?? cx.CurrentUser!).Record)
            .Set(cx.DB.ChargeProduct, product.Record)
            .Set(cx.DB.ChargeTo, to.Record);


        var tr = product.SalesTax.GetRate(t);

        if (isGross)
        {
            var na = amount / (1 + tr);

            Record
                .Set(cx.DB.ChargeNetAmount, na)
                .Set(cx.DB.ChargeTaxAmount, amount - na);
        }
        else
        {
            Record
                .Set(cx.DB.ChargeNetAmount, amount)
                .Set(cx.DB.ChargeTaxAmount, tr * amount);
        }
    }

    public DateTime At => Record.Get(Cx.DB.ChargeAt);
    public User By => new User(Cx, Record.Copy(Cx.DB.ChargeBy));
    public User To => new User(Cx, Record.Copy(Cx.DB.ChargeTo));
    public Product Product => new Product(Cx, Record.Copy(Cx.DB.ChargeProduct));    
    public decimal NetAmount => Record.Get(Cx.DB.ChargeNetAmount);
    public decimal TaxAmount => Record.Get(Cx.DB.ChargeTaxAmount);

    public override DB.Table[] Tables => [Cx.DB.Charges];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}