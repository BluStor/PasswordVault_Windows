using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
using KeePassLib;
using System.Drawing;
using System.Diagnostics;

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

            if (BluetoothRadio.PrimaryRadio == null) throw new Exception(CyberGateErrorMessages.BluetoothDisconnected);

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
                        ProcessRequest(new WaitCallback(RemoveResponse), null);
                    }
                    else if (Method == IOConnection.WrmMoveFile)
                    {
                        ProcessRequest(new WaitCallback(MoveResponse), null);
                    }
                    else if (_mLRequestData.Count > 0)
                    {
                        ProcessRequest(new WaitCallback(CreateUploadResponse), null);
                    }
                    else
                    {
                        _cardFileName = _cardDbFileNameTemp;
                        ProcessRequest(new WaitCallback(DownloadResponse), null);
                    }
                }
                else
                {
                    if (Method == IOConnection.WrmDeleteFile)
                    {
                        _cardFileName = _cardDbFileName;
                        ProcessRequest(new WaitCallback(RemoveResponse), null);
                    }
                    else if (Method != IOConnection.WrmDeleteFile && Method != IOConnection.WrmMoveFile &&
                             _mLRequestData.Count == 0)
                    {
                        _cardFileName = _cardDbFileName;
                        ProcessRequest(new WaitCallback(DownloadResponse), null);
                    }
                }
            }
            catch (Exception ex)
            {
                CGCardIntegrateExt.Card.Disconnect();
                CGCardIntegrateExt.Card = null;
                if (!CGCardIntegrateExt.SavingAction) throw ex;
                else throw new Exception("Operation failed. Try again", ex);
            }
            finally
            {
                if(CGCardIntegrateExt.Card != null) CGCardIntegrateExt.Card.Disconnect();
                if (_mEx != null)
                {
                    if (CGCardIntegrateExt.StatusState != null)
                    {
                        StatusUtil.End(CGCardIntegrateExt.StatusState);
                        CGCardIntegrateExt.StatusState = null;
                    }
                }
            }
            return _mWr;
        }

        private void ProcessRequest(WaitCallback a, string message)
        {
            if(CGCardIntegrateExt.StatusState == null && !string.IsNullOrEmpty(message)) CGCardIntegrateExt.StatusState = StatusUtil.Begin(message);
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                a));

            bool bReady = false;
            while (!bReady)
            {
                lock (_mObjSync) { bReady = _mBReady; }

                Thread.Sleep(100);
                Application.DoEvents();
            }

            if (_mEx != null)
            {
                throw _mEx;
            }
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
            //new InvalidOperationException("Operation failed. Try again", exUpload)

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
                if (response.Status != 226)
                {
                    // Database file not found, so create a default
                    response = CreateDefaultDatabase();
                } 
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

        #region CreateDefaultDatabase
        private Response CreateDefaultDatabase()
        {

            string strPassword = "";
            if (InputBox("New Password Vault Database", "Master Password:", ref strPassword) != DialogResult.OK)
            {
                throw new Exception("No password database found on card.");
            }

            // Create a null status logger
            KeePassLib.Interfaces.NullStatusLogger status = new KeePassLib.Interfaces.NullStatusLogger();

            // Create an empty password database
            KeePassLib.Serialization.IOConnectionInfo iocInfo = new KeePassLib.Serialization.IOConnectionInfo();
            KeePassLib.PwDatabase pwDatabase = new KeePassLib.PwDatabase();
            KeePassLib.Keys.CompositeKey key = new KeePassLib.Keys.CompositeKey();
            key.AddUserKey(new KeePassLib.Keys.KcpPassword(strPassword));
            PwGroup pwGroup = new PwGroup(true, true, "Password Vault", PwIcon.FolderOpen);
            pwDatabase.New(iocInfo, key);
            pwDatabase.RootGroup = pwGroup;

            // Serialize the database so we can write it to the card
            KeePassLib.Serialization.KdbxFile KdbxFile = new KeePassLib.Serialization.KdbxFile(pwDatabase);           
            KdbxFile.Save(GetRequestStream(), null, KdbxFormat.Default, status);

            // Attempt to upload the database to the card
            var response = CGCardIntegrateExt.Card.Put(_cardDbFileName, _mLRequestData.ToArray());
            if (response.Status != 213) throw new Exception("Failed to create default database.");

            // Close the database
            pwDatabase.Close();

            //MemoryStream s = new MemoryStream();
            //s.Capacity = 0x800;
            //s.Write(_mLRequestData.ToArray(), 0, _mLRequestData.Count);
            //Response r = new Response(0xe2, "Created New Database", s);
            //s.Close();
            //return r;

            // Build download response for new database
            response = CGCardIntegrateExt.Card.Get(_cardFileName);

            return response;

        }
        #endregion

        #region Password InputBox
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            textBox.UseSystemPasswordChar = true;

            form.AutoSize = true;
            form.AutoSizeMode = AutoSizeMode.GrowOnly;
            form.TopMost = true;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 15, 372, 13);
            textBox.SetBounds(12, 46, 372, 20);
            buttonOk.SetBounds(198, 82, 75, 35);
            buttonCancel.SetBounds(289, 82, 95, 35);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 130);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            //form.AutoScaleDimensions = new SizeF(1F, 1F);
            form.AutoScaleMode = AutoScaleMode.Inherit;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        #endregion




    }

}
