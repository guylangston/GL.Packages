using System.IO;

namespace GL.CMS
{
    public static class CMSHelper
    {
        public static MemoryStream ToMemoryStream(string str)
        {
            var ms = new MemoryStream();
            var tw = new StreamWriter(ms);
            tw.Write(str);
            tw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}