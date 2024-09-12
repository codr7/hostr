namespace Hostr.Domain;

using static Hostr.DB.ValueExtensions;

public class TaxType : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Tax Type", Schema.Instance.TaxTypes);
    public static Event.Type UPDATE => new Event.Update("Update Tax Type", Schema.Instance.TaxTypes);

    public TaxType(Cx cx, DB.Record fields) : base(cx, fields) { }

    public TaxType(Cx cx, string name = "") : base(cx)
    {
        Name = name;
    }

    public string Name
    {
        get => Record.Get(Cx.DB.TaxTypeName)!;
        set => Record.Set(Cx.DB.TaxTypeName, value);
    }

    public decimal GetRate(DateTime timestamp)
    {
        var rs = new DB.Query(Cx.DB.TaxRates).
            Select(Cx.DB.TaxRatePercentage).
            Where(Cx.DB.TaxRateType.Eq(Record)).
            Where(Cx.DB.TaxRateStartsAt.Lte(timestamp)).
            Where(Cx.DB.TaxRateEndsAt.Gt(timestamp)).
            FindAll(Cx.DBCx);

        if (rs.Length > 1) { throw new Exception("Multiple tax rates found"); }
        return rs[0].Get(Cx.DB.TaxRatePercentage) / 100M;
    }

    public override DB.Table[] Tables => [Cx.DB.TaxTypes];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}