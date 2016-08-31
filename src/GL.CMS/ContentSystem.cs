using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    public class ContentSystem : IContentSystem, IContentSystemCommands
    {
        public ContentSystem(string root)
        {
            Root = root;
        }

        public string Root { get; }


        public IContentIdent Qualify(int id)
        {
            throw new NotImplementedException();
        }

        public IContentIdent Qualify(string code)
        {
            throw new NotImplementedException();
        }

        public IContentIdent Qualify(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IEntry Get(int id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<IEntry> GetChildren(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamAsync(int id, bool bodyTrue_headerFalse)
        {
            throw new NotImplementedException();
        }

        public void Render(IEntry entry, IFragment fragment, IRenderContext context, TextWriter outp, RenderFormat format)
        {
            throw new NotImplementedException();
        }

        public void Index(IEntry entry)
        {
            throw new NotImplementedException();
        }

        public void Store(IEntry entry)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}