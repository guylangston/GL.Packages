namespace GL.DAL
{
    public class InsertContext : BaseContext
    {
        public InsertStatement Statement { get; set; }

        public InsertContext Add<T>(string field, T value)
        {
            Statement.Add<T>(field, value);
            return this;
        }
        
        public int Execute()
        {
            return Statement.Execute(Context.ConnectionString);
        }
    }
}