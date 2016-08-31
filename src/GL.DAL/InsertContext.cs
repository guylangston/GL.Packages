namespace GL.FluentDAL
{
    public class InsertContext : BaseContext
    {
        public InsertStatement Statement { get; set; }

        public int Execute()
        {
            return Statement.Execute(Context.ConnectionString);
        }
    }
}