using System;

namespace TlvLib
{
    [Serializable]
    public class TlvException : Exception
    {
        private byte[] m_Data;

        public TlvException(string message)
            : base(message)
        {
        }

        public TlvException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TlvException(string message, byte[] data, Exception innerException)
            : base(message, innerException)
        {
            m_Data = data;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
