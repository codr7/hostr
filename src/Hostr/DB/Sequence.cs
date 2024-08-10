namespace Hostr.DB;

public class Sequence : Definition
{
    private readonly int startValue;

    public Sequence(Schema schema, string name, int startValue) : base(schema, name)
    {
        this.startValue = startValue;
        schema.AddDefinition(this);
    }

    public override string CreateSql => $"{base.CreateSql} START {startValue}";

    public override string DefinitionType => "SEQUENCE";

    public override bool Exists(Cx cx) =>
        cx.Tx!.ExecScalar<bool>(@"SELECT EXISTS (
                                SELECT FROM pg_class
                                WHERE relkind = 'S'
                                AND relname = $?
                              )", Name);

    public long Next(Cx cx) => cx.Tx!.ExecScalar<long>($"SELECT NEXTVAL('\"{Name}\"')");
}