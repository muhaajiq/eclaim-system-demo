namespace MHA.ECLAIM.Framework.Exceptions
{
    public class ClientContextNullReferenceException : Exception
    {
        public ClientContextNullReferenceException()
        {
        }

        public ClientContextNullReferenceException(string message)
            : base(message)
        {
        }
    }
}
