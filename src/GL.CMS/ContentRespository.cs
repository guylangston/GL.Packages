using GL.DAL;

namespace GL.CMS
{
    public class ContentRespository
    {
        public IEntry Get(int id) => 
            FluentDAL.GetContext()
                .Query("SELECT * FROM Content WHERE ContentId={0}", id)
                .BindByName<EntryFlat>((reader, lookup, ctx) => new EntryFlat()
                {
                   ContentId = reader.GetInt32(lookup["ContentId"]),
                })
                .FirstOrDefault();
    }
}