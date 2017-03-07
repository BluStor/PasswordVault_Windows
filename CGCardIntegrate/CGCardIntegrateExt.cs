using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;
using KeePass.Ecas;
using KeePass.Plugins;
using KeePassLib;

namespace CGCardIntegrate
{
    public sealed class CGCardIntegrateExt : Plugin
    {
        private static IPluginHost _host = null;
        private static Card _card;
        private static StatusStateInfo _statusState;

        internal static IPluginHost Host => _host;
        public static Card Card
        {
            get { return _card; }
            set { _card = value; }
        }

        public static StatusStateInfo StatusState
        {
            get { return _statusState; }
            set { _statusState = value; }
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
