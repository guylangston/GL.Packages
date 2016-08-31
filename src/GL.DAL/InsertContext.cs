namespace GL.DAL
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