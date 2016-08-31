using System;

namespace GL.DAL
{
    //public class Sample
    //{
    //    public void Example()
    //    {
    //        // Old
    //        var db = DBHelper.Utils.MakeConnectionString("GLBlog");
    //        DBHelper.ExecuteQuery(db, x => x.GetStringSafe(0), "SELECT Title FROM cms.Content");
    //        // New
    //        DAL.GetContext()
    //            .Query("SELECT Title FROM cms.Content", 1)
    //            .Bind(r=>r.GetStringSafe(0))
    //            .ToList();
    //    }
    //}
    public static class FluentDAL
    {
        // Extension Point
        public static Func<string, IDatabaseContext> ContextFactory { get; set; } = PrivateDefaultContextFactory;
        
        // Default Database
        public static string DefaultConnectionString { get; set; }

        public static IDatabaseContext GetContext() => ContextFactory(DefaultConnectionString);
        public static IDatabaseContext GetContext(string db) => ContextFactory(db);


        private static IDatabaseContext PrivateDefaultContextFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            return new DatabaseContextDefault(connectionString);
        }
    }


    public interface IDatabaseContext
    {
        string ConnectionString { get; }
        QueryContext Query(string sql, params object[] args);
        InsertContext Insert(string table);
        UpdateContext Update(string table, string where);
    }

    public abstract class BaseContext
    {
        public IDatabaseContext Context { get; set; }
    }
}
