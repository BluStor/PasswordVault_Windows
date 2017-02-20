using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CGCardIntegrate
{
    public sealed class CyberGateJitStream : Stream
    {
        private MemoryStream m_s = null;
        private object m_objDownloadSync = new object();
        private bool m_bFileDownloaded = false;
        private Exception m_exDownload = null;

        public override bool CanRead
        {
            get { return ((m_s != null) ? m_s.CanRead : true); }
        }

        public override bool CanSeek
        {
            get { return ((m_s != null) ? m_s.CanSeek : true); }
        }

        public override bool CanWrite
        {
            get { return ((m_s != null) ? m_s.CanWrite : false); }
        }

        public override long Length
        {
            get
            {
                EnsureStream();
                return m_s.Length;
            }
        }

        public override long Position
        {
            get { return ((m_s != null) ? m_s.Position : 0); }
            set
            {
                if (m_s != null) m_s.Position = value;
                else if (value > 0)
                {
                    EnsureStream();
                    m_s.Position = value;
                }
            }
        }

        public CyberGateJitStream()
        {
            EnsureStream();
        }

        private void EnsureStream()
        {
            if (m_s != null) return;
            
            try
            {
                m_bFileDownloaded = false;
                m_exDownload = null;
                //DownloadFileProc(new object());
                WaitCallback wc = new WaitCallback(DownloadFileProc);
                ThreadPool.QueueUserWorkItem(wc);

                bool bFinished = false;
                while (!bFinished)
                {
                    lock (m_objDownloadSync) { bFinished = m_bFileDownloaded; }

                    Thread.Sleep(100);
                    Application.DoEvents();
                }

                if (m_exDownload != null) throw m_exDownload;
            }
            finally { }
        }

        private void DownloadFileProc(object state)
        {
            try
            {
                //todo download
                using (var file = new FileStream("C://projects//temp//newdatabase1.kdbx", FileMode.Open))
                {
                    m_s = new MemoryStream();
                    file.CopyTo(m_s);
                    m_s.Position = 0;
                }
            }
            catch (Exception exDl) { m_exDownload = exDl; }

            lock (m_objDownloadSync) { m_bFileDownloaded = true; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureStream();
            return m_s.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureStream();
            return m_s.Seek(offset, origin);
        }

        public override void Flush()
        {
            EnsureStream();
            m_s.Flush();
        }

        public override void SetLength(long value)
        {
            EnsureStream();
            m_s.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureStream();
            m_s.Write(buffer, offset, count);
        }

        public override void Close()
        {
            if (m_s != null) { m_s.Close(); m_s = null; }

            base.Close();
        }
    }
}
