using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    public enum RenderFormat
    {
        Text,
        Markdown,
        Html,
        Image
    }

    public interface IRenderContext
    {
        string GetPropValue(string name);
    }

    // Command-Query Separation: Read
    // Reads are REST-Like
    public interface IContentSystem
    {
        IContentIdent Qualify(int id);
        IContentIdent Qualify(string code);
        IContentIdent Qualify(Uri uri);

        IEntry Get(int id);
        IReadOnlyCollection<IEntry> GetChildren(int id);

        Task<Stream> GetStreamAsync(int id, bool bodyTrue_headerFalse);

        void Render(
            // what
            IEntry entry, IFragment fragment, IRenderContext context,
            // where to
            TextWriter outp, RenderFormat format);
    }

    // Command-Query Separation: Command
    public interface IContentSystemCommands
    {
        void Index(IEntry entry);
        void Store(IEntry entry);
        void Delete(IEntry entry); // Use the full version to allow race-condition checks
    }
}