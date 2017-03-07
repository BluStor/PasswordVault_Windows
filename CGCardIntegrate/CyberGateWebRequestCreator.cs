using System;
using System.Net;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
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
            if (BluetoothRadio.PrimaryRadio == null) throw new Exception("Local bluetooth device is disconnected");
            var cardName = uri.Host.Replace(".tmp", "");

            if (CGCardIntegrateExt.Card == null || 
                (CGCardIntegrateExt.Card.BluetoothName != null && !CGCardIntegrateExt.Card.BluetoothName.ToLower().Equals(cardName.ToLower())))
            {
                CGCardIntegrateExt.StatusState = StatusUtil.Begin("Connecting to the card.");
                CGCardIntegrateExt.Card = new Card(Constants.Password, cardName, BluetoothRadio.PrimaryRadio.LocalAddress.ToString(), true);
                StatusUtil.End(CGCardIntegrateExt.StatusState);
            }

            return new CyberGateWebRequest(uri);
        }
    }
}