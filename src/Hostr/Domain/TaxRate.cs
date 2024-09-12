namespace Hostr.Domain;

public class TaxRate : Model
{
    public static Event.Type INSERT => new Event.Insert("Insert Tax Rate", Schema.Instance.TaxRates);
    public static Event.Type UPDATE => new Event.Update("Update Tax Rate", Schema.Instance.TaxRates);

    public TaxRate(Cx cx, DB.Record fields) : base(cx, fields) { }

    public TaxRate(Cx cx, TaxType type, decimal percentage = 0, DateTime? startsAt = null, DateTime? endsAt = null) : base(cx)
    {
        Record
            .Set(cx.DB.TaxRateType, type.Record)
            .Set(cx.DB.TaxRatePercentage, percentage)
            .Set(cx.DB.TaxRateStartsAt, startsAt ?? DateTime.MinValue)
            .Set(cx.DB.TaxRateEndsAt, endsAt ?? DateTime.MaxValue);
    }

    public TaxType Type => new TaxType(Cx, Record.Copy(Cx.DB.TaxRateType.Columns));

    public DateTime StartsAt {
        get => Record.Get(Cx.DB.TaxRateStartsAt);
        set => Record.Set(Cx.DB.TaxRateStartsAt, value);
    }

    public DateTime EndsAt {
        get => Record.Get(Cx.DB.TaxRateEndsAt);
        set => Record.Set(Cx.DB.TaxRateEndsAt, value);
    }

    public decimal Percentage {
        get => Record.Get(Cx.DB.TaxRatePercentage);
        set => Record.Set(Cx.DB.TaxRatePercentage, value);
    }

    public override DB.Table[] Tables => [Cx.DB.TaxRates];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}