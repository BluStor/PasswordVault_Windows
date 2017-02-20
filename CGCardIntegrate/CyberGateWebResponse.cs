using System;
using System.IO;
using System.Net;

namespace CGCardIntegrate
{
    public sealed class CyberGateWebResponse : WebResponse
    {
        private Stream m_sResponse = null;

        private long m_lSize = 0;
        public override long ContentLength
        {
            get { return m_lSize; }
            set { throw new InvalidOperationException(); }
        }

        public override string ContentType
        {
            get { return "application/octet-stream"; }
            set { throw new InvalidOperationException(); }
        }

        public override Uri ResponseUri
        {
            get { return null; }
        }

        public override WebHeaderCollection Headers
        {
            get { return null; }
        }

        public CyberGateWebResponse(bool isEmptyResponce)
        {
            if (isEmptyResponce)
            {
                m_sResponse = new MemoryStream();
            }
            GetResponseStream();
        }

        public override Stream GetResponseStream()
        {
            if (m_sResponse != null) return m_sResponse;

            m_sResponse = new CyberGateJitStream();
            return m_sResponse;
        }

        public override void Close()
        {
            if (m_sResponse != null) { m_sResponse.Close(); m_sResponse = null; }
        }
    }
}
