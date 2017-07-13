using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace CGCardIntegrate
{
    public class CyberGateFirmwareUpdater
    {
        private readonly string _mFirmwarePath = "/device/firmware";
        private readonly object _mObjSync = new object();
        private bool _mBReady;
        private Exception _mEx;
        private byte[] _mLRequestData = null;
        private readonly int _mWaitForUpdateFirmware = 25000;//time in milliseconds
        private readonly int _step = 100;
        private string _mVersion = "";
        public void Process()
        {
            CheckCardConnection();
            //file dialog for selecting firmware file
            using (var theDialog = new OpenFileDialog())
            {
                theDialog.Title = "Open Firmware File";
                theDialog.Filter = "Bin files|*.bin";
                theDialog.InitialDirectory = @"C:\";
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var fileStream = theDialog.OpenFile())
                    {

                        using (var memoryStream = new MemoryStream())
                        {
                            fileStream.CopyTo(memoryStream);
                            _mLRequestData = memoryStream.ToArray();

                            // start uploading firmware file to card
                            CGCardIntegrateExt.StatusState = StatusUtil.Begin("Uploading firmware...");
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                CreateUploadResponse));

                            bool bReady = false;
                            while (!bReady)
                            {
                                lock (_mObjSync) { bReady = _mBReady; }

                                Thread.Sleep(_step);
                                Application.DoEvents();
                            }

                            StatusUtil.End(CGCardIntegrateExt.StatusState);
                            if (_mEx != null)
                            {
                                throw _mEx;
                            }
                            //waiting for updating card firmware
                            CGCardIntegrateExt.StatusState = StatusUtil.Begin("Updating firmware...");
                            var current = 0;
                            while (current < _mWaitForUpdateFirmware)
                            {

                                Thread.Sleep(_step);
                                Application.DoEvents();
                                current += _step;
                            }
                            StatusUtil.End(CGCardIntegrateExt.StatusState);
                            CGCardIntegrateExt.StatusState = null;
                        }
                    }
                }
            }
        }

        private void CheckCardConnection()
        {
            try
            {
                if (CGCardIntegrateExt.Card == null)
                {
                    //try open connection to card
                    var dialog = new ConnectToCardDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CGCardIntegrateExt.StatusState = StatusUtil.Begin("Connecting to the card...");
                        CGCardIntegrateExt.Card = new Card(dialog.Result, BluetoothRadio.PrimaryRadio.LocalAddress.ToString(),
                            true);
                        StatusUtil.End(CGCardIntegrateExt.StatusState);
                        CGCardIntegrateExt.StatusState = null;
                    }
                }
                else
                {
                    CGCardIntegrateExt.Card.Disconnect();
                    CGCardIntegrateExt.Card.BuildConnection();
                }
                if (CGCardIntegrateExt.StatusState != null)
                {
                    StatusUtil.End(CGCardIntegrateExt.StatusState);
                }
            }
            catch
            {
                if (CGCardIntegrateExt.Card != null) CGCardIntegrateExt.Card.Dispose();
                if (CGCardIntegrateExt.StatusState != null)
                {
                    StatusUtil.End(CGCardIntegrateExt.StatusState);
                    CGCardIntegrateExt.StatusState = null;
                }

                GC.Collect();
                throw;
            }
            if (CGCardIntegrateExt.Card == null)
            {
                throw new Exception("Not connected to cybergate card!");
            }
        }
        private void CreateUploadResponse(object objState)
        {
            try
            {
                var response = CGCardIntegrateExt.Card.Put(_mFirmwarePath, _mLRequestData);
                if (response.Status != 213) throw new Exception("File was not uploaded.");
            }
            catch (Exception exUpload) { _mEx = exUpload; }

            lock (_mObjSync)
            {
                _mBReady = true;
            }
        }

        public void CurrentFirmwareVersion()
        {
            var t = Regex.Match("boot 4.5\r\nFIRM: 5.6.4 fsdf", @"(?<=FIRM: )((\d|\.)*)").Value;
            //CheckCardConnection();
            // start uploading firmware file to card
            CGCardIntegrateExt.StatusState = StatusUtil.Begin("Getting firmware version...");
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                FirmwareResponse));

            bool bReady = false;
            while (!bReady)
            {
                lock (_mObjSync) { bReady = _mBReady; }

                Thread.Sleep(_step);
                Application.DoEvents();
            }

            StatusUtil.End(CGCardIntegrateExt.StatusState);
            if (_mEx != null)
            {
                throw _mEx;
            }
            MessageBox.Show("Current firmware version: " + Regex.Match(_mVersion, @"(?<=FIRM: )((\d|\.)*)").Value);
        }
        private void FirmwareResponse(object objState)
        {

            try
            {
                var response = CGCardIntegrateExt.Card.Get(_mFirmwarePath);
                if (response.Status != 213) throw new Exception("File was not downloaded.");
                _mVersion = response.ToString();
            }
            catch (Exception exUpload) { _mEx = exUpload; }

            lock (_mObjSync)
            {
                _mBReady = true;
            }
        }
    }
}
