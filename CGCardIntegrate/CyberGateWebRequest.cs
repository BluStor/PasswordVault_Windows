using GateKeeperSDK;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace CGCardIntegrate
{
    public sealed class CyberGateWebRequest : WebRequest
    {
        private Uri m_uri;
        public override Uri RequestUri
        {
            get { return m_uri; }
        }

        private string m_strMethod = string.Empty;
        public override string Method
        {
            get { return m_strMethod; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                m_strMethod = value;
            }
        }

        private WebHeaderCollection m_whcHeaders = new WebHeaderCollection();
        public override WebHeaderCollection Headers
        {
            get { return m_whcHeaders; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                m_whcHeaders = value;
            }
        }

        private long m_lContentLength = 0;
        public override long ContentLength
        {
            get { return m_lContentLength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                m_lContentLength = value;
            }
        }

        private string m_strContentType = string.Empty;
        public override string ContentType
        {
            get { return m_strContentType; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                m_strContentType = value;
            }
        }

        private ICredentials m_cred = null;
        public override ICredentials Credentials
        {
            get { return m_cred; }
            set { m_cred = value; }
        }

        private bool m_bPreAuth = true;
        public override bool PreAuthenticate
        {
            get { return m_bPreAuth; }
            set { m_bPreAuth = value; }
        }

        private IWebProxy m_prx = null;
        public override IWebProxy Proxy
        {
            get { return m_prx; }
            set { m_prx = value; }
        }

        public CyberGateWebRequest(Uri uri)
        {
            m_uri = uri;
        }

        private List<byte> m_lRequestData = new List<byte>();
        public override Stream GetRequestStream()
        {
            m_lRequestData.Clear();
            return new CopyMemoryStream(m_lRequestData);
        }

        private WebResponse m_wr = null;

        public override WebResponse GetResponse()
        {
            if (m_wr != null) return m_wr;
            
            var isTemp = m_uri.Authority.EndsWith(".tmp");
            StatusStateInfo st = null;

            try
            {
                if (isTemp)
                {
                    if (Method == IOConnection.WrmDeleteFile)
                    {
                        if (File.Exists("C://projects//temp//newdatabase1.kdbx"))
                        {
                            File.Delete("C://projects//temp//newdatabase1.kdbx");
                            var t = File.Create("C://projects//temp//newdatabase1.kdbx");
                            t.Close();
                        }
                        m_wr = new CyberGateWebResponse(true);
                    }
                    else if (Method == IOConnection.WrmMoveFile)
                    {
                        m_wr = new CyberGateWebResponse(true);
                    }
                    else if (m_lRequestData.Count > 0)
                    {
                        st = StatusUtil.Begin("Uploading file...");
                        object objState = new object();
                        //CreateUploadResponse(objState);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            CreateUploadResponse));

                        bool bReady = false;
                        while (!bReady)
                        {
                            lock (m_objDownloadSync) { bReady = m_bUploadResponseReady; }

                            Thread.Sleep(100);
                            Application.DoEvents();
                        }
                        m_wr = m_wswrUpload;
                        if (m_exUpload != null) throw m_exUpload;
                    }
                    else
                    {
                        st = StatusUtil.Begin("Downloading file...");
                        m_wr = new CyberGateWebResponse(false);
                    }
                }
                else
                {
                    if (Method != IOConnection.WrmDeleteFile && Method != IOConnection.WrmMoveFile && m_lRequestData.Count == 0)
                    {
                        st = StatusUtil.Begin("Downloading file...");
                        m_wr = new CyberGateWebResponse(false);
                    }
                }
            }
            finally
            {
                if (st != null) StatusUtil.End(st);
            }
            return m_wr;
        }

        private object m_objDownloadSync = new object();
        private bool m_bUploadResponseReady = false;
        private CyberGateWebResponse m_wswrUpload = null;
        private Exception m_exUpload = null;
        private void CreateUploadResponse(object objState)
        {
            CyberGateWebResponse wr = null;

            try
            {
                using (var card = new Card(Constants.Password))
                {
                    using (var response = card.FreeMemory())
                    {
                        StatusUtil.Begin(response.ReadDataFile());
                    }
                }
                //todo upload data
                File.WriteAllBytes("C://projects//temp//newdatabase1.kdbx", m_lRequestData.ToArray());
                wr = new CyberGateWebResponse(true);
            }
            catch (Exception exUpload) { m_exUpload = exUpload; }

            lock (m_objDownloadSync)
            {
                m_wswrUpload = wr;
                m_bUploadResponseReady = true;
            }
        }
    }
}
