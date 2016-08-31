using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    public class LazyLoadedFragment : IFragment
    {
        private readonly IContentSystem cms;
        private readonly int id;

        public LazyLoadedFragment(IContentSystem cms, int id, string format, FragmentType type)
        {
            this.cms = cms;
            this.id = id;
            Format = format;
            Type = type;
        }

        public string Format { get;  }
        public FragmentType Type { get;  }

        public string GetContent()
        {
            using (var sr = new StreamReader(GetContentStreamAsync().Result))
            {
                return sr.ReadToEnd();
            }
        }

        public Task<Stream> GetContentStreamAsync() => cms.GetStreamAsync(id, true);
    }
}