namespace GL.FluentDAL
{
    public class UpdateContext : BaseContext
    {
        public UpdateStatement Statement { get; set; }

        public void Execute()
        {
            Statement.Execute(Context.ConnectionString);
        }
    }
}