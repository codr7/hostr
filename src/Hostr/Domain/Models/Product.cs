namespace Hostr.Domain.Models;

public class Product : Pool
{
    public new static Event.Type INSERT => new Event.Insert("Insert Product", Schema.Instance.Products);
    public new static Event.Type UPDATE => new Event.Update("Update Product", Schema.Instance.Products);

    public Product(Cx cx, DB.Record fields) : base(cx, fields) { }

    public Product(Cx cx, long? id = null, string name = "", TaxType? salesTax = null) : base(cx, id: id, name: name)
    {
        Record.Set(cx.DB.ProductId, Record.Get(cx.DB.PoolId));
        if (salesTax is TaxType st) { Record.Set(cx.DB.ProductSalesTax, st.Record); }
    }

    public TaxType SalesTax {
        get => new TaxType(Cx, Record.Copy(Cx.DB.ProductSalesTax));
        set => Record.Set(Cx.DB.ProductSalesTax, value.Record);
    } 

    public override DB.Table[] Tables => [Cx.DB.Pools, Cx.DB.Products];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}