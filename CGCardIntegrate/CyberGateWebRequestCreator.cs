using System;
using System.Net;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;

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
            if(CGCardIntegrateExt.Card == null) CGCardIntegrateExt.Card = new Card(Constants.Password, null, BluetoothRadio.PrimaryRadio.LocalAddress.ToString(), false);
            var cardName = uri.Host.Replace(".tmp", "");
            if (CGCardIntegrateExt.Card.BluetoothName != null && !CGCardIntegrateExt.Card.BluetoothName.ToLower().Equals(cardName.ToLower()))
            {
                CGCardIntegrateExt.Card = new Card(Constants.Password, cardName, CGCardIntegrateExt.Card.Mac, true);
            }
            else
            {
                CGCardIntegrateExt.Card.BuildConnection();
            }
            return new CyberGateWebRequest(uri);
        }
    }
}