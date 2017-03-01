using System;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
using KeePass.Plugins;

namespace CGCardIntegrate
{
    public sealed class CGCardIntegrateExt : Plugin
    {
        private static IPluginHost _host = null;
        private static Card _card;
        internal static IPluginHost Host { get { return _host; } }

        public static Card Card
        {
            get { return _card; }
            set { _card = value; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (_host != null) Terminate();
            _host = host;
            (new CyberGateWebRequestCreator()).Register();
            return true;
        }

        public override void Terminate()
        {
            if (_host != null)
            {
                _host = null;
                Card.Disconnect();
                Card = null;
            }
        }
    }
}
