namespace Hostr.Domain.Models;

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

    public override DB.Table[] Tables => [Cx.DB.TaxTypes];
    protected override Event.Type InsertEventType => INSERT;
    protected override Event.Type UpdateEventType => UPDATE;
}