namespace Hostr.DB;

public class Sequence : Definition
{
    private readonly int startValue;

    public Sequence(string name, int startValue) : base(name)
    {
        this.startValue = startValue;
    }

    public override string CreateSQL => $"{base.CreateSQL} START {startValue}";

    public override string DefinitionType => "SEQUENCE";

    public override bool Exists(Tx tx) => 
        tx.ExecScalar<bool>(@"SELECT EXISTS (
                                SELECT FROM pg_class
                                WHERE relkind = 'S'
                                AND relname = $?
                              )", Name.ToLower());

    public long Next(Tx tx) => tx.ExecScalar<long>(@$"SELECT NEXTVAL('{Name}')");
}