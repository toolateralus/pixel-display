using System;
using System.Windows; 
using System.Runtime.Serialization;

namespace pixel_renderer
{
    [Serializable]
    internal class NullStageException : NullReferenceException
    {
        public NullStageException()
        {
        }

        public NullStageException(string? message) : base(message + "Stage Not Found.")
        {
            Application.Current.MainWindow.Close(); 
        }

        public NullStageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NullStageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}