using System;
using System.Net;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
using KeePass.Ecas;
using KeePassLib;

namespace CGCardIntegrate
{
    public class CyberGateWebRequestCreator : IWebRequestCreate
    {
        public void Register()
        {
            WebRequest.RegisterPrefix("cybergate", this);
        }

        public WebRequest Create(Uri uri)
        {
            var cardName = uri.Host.Replace(".tmp", "");

            try
            {
                if (CGCardIntegrateExt.Card == null ||
                    (CGCardIntegrateExt.Card.BluetoothName != null &&
                     !CGCardIntegrateExt.Card.BluetoothName.ToLower().Equals(cardName.ToLower())))
                {
                    CGCardIntegrateExt.StatusState = StatusUtil.Begin("Connecting to the card...");
                    CGCardIntegrateExt.Card = new Card(cardName, BluetoothRadio.PrimaryRadio.LocalAddress.ToString(),
                        true);
                    StatusUtil.End(CGCardIntegrateExt.StatusState);
                    CGCardIntegrateExt.StatusState = null;
                }
            }
            catch
            {
                if(CGCardIntegrateExt.Card != null) CGCardIntegrateExt.Card.Dispose();
                if (CGCardIntegrateExt.StatusState != null)
                {
                    StatusUtil.End(CGCardIntegrateExt.StatusState);
                    CGCardIntegrateExt.StatusState = null;
                }

                GC.Collect();
                throw;
            }

            return new CyberGateWebRequest(uri);
        }
    }
}