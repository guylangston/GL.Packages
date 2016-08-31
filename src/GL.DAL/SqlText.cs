namespace GL.DAL
{
    public class SqlText
    {
        public SqlText(string text)
        {
            Text = text;
        }

        public static SqlText Create(string text)
        {
            return new SqlText(text);
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}