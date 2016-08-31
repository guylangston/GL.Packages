using System;

namespace GL.DAL
{
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message)
            : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        public string Connection { get; set; }
        public string SQL { get; set; }
    }
}