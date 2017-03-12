using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using KeePassLib;

namespace CGCardIntegrate
{
    public sealed class CyberGateWebRequest : WebRequest
    {
        private readonly Uri _mUri;
        private readonly string _cardDbFileNameTemp = "/passwordvault/keepass_temp.kdbx";
        private readonly string _cardDbFileName = "/passwordvault/db.kdbx";
        private string _cardFileName = string.Empty;
        private WebResponse _mWr = null;
        private readonly object _mObjSync = new object();
        private bool _mBReady;
        private Exception _mEx;
        private string _mStrMethod = string.Empty;
        private WebHeaderCollection _mWhcHeaders = new WebHeaderCollection();
        private long _mLContentLength = 0;
        private string _mStrContentType = string.Empty;
        private ICredentials _mCred = null;
        private bool _mBPreAuth = true;
        private IWebProxy _mPrx = null;
        private readonly List<byte> _mLRequestData = new List<byte>();

        public override Uri RequestUri
        {
            get { return _mUri; }
        }

        public override string Method
        {
            get { return _mStrMethod; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _mStrMethod = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get { return _mWhcHeaders; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _mWhcHeaders = value;
            }
        }

        public override long ContentLength
        {
            get { return _mLContentLength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                _mLContentLength = value;
            }
        }

        public override string ContentType
        {
            get { return _mStrContentType; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _mStrContentType = value;
            }
        }

        public override ICredentials Credentials
        {
            get { return _mCred; }
            set { _mCred = value; }
        }

        public override bool PreAuthenticate
        {
            get { return _mBPreAuth; }
            set { _mBPreAuth = value; }
        }

        public override IWebProxy Proxy
        {
            get { return _mPrx; }
            set { _mPrx = value; }
        }

        public CyberGateWebRequest(Uri uri)
        {
            _mUri = uri;
        }

        public override Stream GetRequestStream()
        {
            _mLRequestData.Clear();
            return new CopyMemoryStream(_mLRequestData);
        }

        public override WebResponse GetResponse()
        {
            if (_mWr != null) return _mWr;

            var isTemp = _mUri.Authority.EndsWith(".tmp");
            bool connectedInternal = false, closeProgress = false;
            if (!CGCardIntegrateExt.Card.IsAvailableSerial)
            {
                CGCardIntegrateExt.Card.BuildConnection();
                connectedInternal = true;
            }

            var h = CGCardIntegrateExt.Host.Database.IOConnectionInfo.IsLocalFile();

            try
            {
                if (isTemp)
                {
                    if (Method == IOConnection.WrmDeleteFile)
                    {
                        _cardFileName = _cardDbFileNameTemp;
                        ProcessRequest(new WaitCallback(RemoveResponse), "Saving database to the card...");
                    }
                    else if (Method == IOConnection.WrmMoveFile)
                    {
                        ProcessRequest(new WaitCallback(MoveResponse), "Replacing temp file...");
                    }
                    else if (_mLRequestData.Count > 0)
                    {
                        ProcessRequest(new WaitCallback(CreateUploadResponse), "Uploading temp file...");
                    }
                    else
                    {
                        _cardFileName = _cardDbFileNameTemp;
                        ProcessRequest(new WaitCallback(DownloadResponse), "Downloading temp file...");
                    }
                }
                else
                {
                    if (Method == IOConnection.WrmDeleteFile)
                    {
                        _cardFileName = _cardDbFileName;
                        ProcessRequest(new WaitCallback(RemoveResponse), "Removing old file...");
                    }
                    else if (Method != IOConnection.WrmDeleteFile && Method != IOConnection.WrmMoveFile && _mLRequestData.Count == 0)
                    {
                        _cardFileName = _cardDbFileName;
                        ProcessRequest(new WaitCallback(DownloadResponse), "Downloading file...");
                        closeProgress = true;
                    }
                }
            }
            finally
            {
                CGCardIntegrateExt.Card.Disconnect();
                if (closeProgress)
                {
                    StatusUtil.End(CGCardIntegrateExt.StatusState);
                }
            }
            return _mWr;
        }

        private void ProcessRequest(WaitCallback a, string message)
        {
            if(CGCardIntegrateExt.StatusState == null ) CGCardIntegrateExt.StatusState = StatusUtil.Begin(message);
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                a));

            bool bReady = false;
            while (!bReady)
            {
                lock (_mObjSync) { bReady = _mBReady; }

                Thread.Sleep(100);
                Application.DoEvents();
            }

            if (_mEx != null) throw _mEx;
        }

        #region Upload file
        private void CreateUploadResponse(object objState)
        {
            CyberGateWebResponse wr = null;

            try
            {
                var response = CGCardIntegrateExt.Card.Put(_cardDbFileNameTemp, _mLRequestData.ToArray());
                if (response.Status != 213) throw new Exception("File was not uploaded.");
                wr = new CyberGateWebResponse();
            }
            catch (Exception exUpload) { _mEx = exUpload; }

            lock (_mObjSync)
            {
                _mWr = wr;
                _mBReady = true;
            }
        }
        #endregion

        #region Move file
        private void MoveResponse(object objState)
        {
            CyberGateWebResponse wr = null;

            try
            {
                var response = CGCardIntegrateExt.Card.Rename(_cardDbFileNameTemp, _cardDbFileName);
                wr = new CyberGateWebResponse();
            }
            catch (Exception exMove) { _mEx = exMove; }

            lock (_mObjSync)
            {
                _mWr = wr;
                _mBReady = true;
            }
        }
        #endregion

        #region Download file
        private void DownloadResponse(object objState)
        {
            CyberGateWebResponse wr = null;

            try
            {
                var response = CGCardIntegrateExt.Card.Get(_cardFileName);
                if (response.DataFile == null)
                {
                    wr = new CyberGateWebResponse();
                }
                else
                {
                    response.DataFile.Position = 0;
                    wr = new CyberGateWebResponse(response.DataFile);
                }
            }
            catch (Exception exDownload) { _mEx = exDownload; }

            lock (_mObjSync)
            {
                _mWr = wr;
                _mBReady = true;
            }
        }
        #endregion

        #region Remove file
        private void RemoveResponse(object objState)
        {
            CyberGateWebResponse wr = null;

            try
            {
                var response = CGCardIntegrateExt.Card.Delete(_cardFileName);
                wr = new CyberGateWebResponse();
            }
            catch (Exception exRemove) { _mEx = exRemove; }

            lock (_mObjSync)
            {
                _mWr = wr;
                _mBReady = true;
            }
        }
        #endregion
    }
}