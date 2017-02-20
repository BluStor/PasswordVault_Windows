using System.Collections.Generic;
using System.IO;

namespace CGCardIntegrate
{
    class CopyMemoryStream : MemoryStream
    {
        private List<byte> m_lCopyBuffer;

        public CopyMemoryStream(List<byte> lCopyBuffer) : base()
        {
            m_lCopyBuffer = lCopyBuffer;
        }

        public override void Close()
        {
            if (m_lCopyBuffer != null)
            {
                m_lCopyBuffer.AddRange(this.ToArray());
                m_lCopyBuffer = null; // Copy once only
            }

            base.Close();
        }
    }
}
