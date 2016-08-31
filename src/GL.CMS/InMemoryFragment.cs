using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    public class InMemoryFragment : IFragment
    {
        private readonly IContentSystem cms;
        private readonly string data;

        public InMemoryFragment(IContentSystem cms, string data)
        {
            this.cms = cms;
            this.data = data;
        }

        public string Format { get; set; }
        public FragmentType Type { get; set; }

        public string GetContent() => data;
        public Task<Stream> GetContentStreamAsync() => Task.FromResult<Stream>(CMSHelper.ToMemoryStream(data));
    }
}