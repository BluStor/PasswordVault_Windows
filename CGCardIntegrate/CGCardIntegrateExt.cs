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
        public static bool SavingAction;
        public static readonly PwUuid SavingDatabaseFile = new PwUuid(new byte[] {
            0x95, 0xC1, 0xA6, 0xFD, 0x72, 0x7C, 0x40, 0xC7,
            0xA2, 0xF9, 0x5B, 0x0F, 0xA0, 0x99, 0x63, 0x1C
        });
        public static readonly PwUuid SavedDatabaseFile = new PwUuid(new byte[] {
            0xB3, 0xA8, 0xFD, 0xFE, 0x78, 0x13, 0x4A, 0x6A,
            0x9C, 0x5D, 0xD5, 0xBA, 0x84, 0x3A, 0x9B, 0x8E
        });


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
            Host.TriggerSystem.RaisingEvent += this.OnEcasSaveEvent;
            Host.TriggerSystem.RaisingEvent += this.OnEcasSavedEvent;
            (new CyberGateWebRequestCreator()).Register();
            return true;
        }

        public override void Terminate()
        {
            if (_host != null)
            {
                Host.TriggerSystem.RaisingEvent -= this.OnEcasSaveEvent;
                Host.TriggerSystem.RaisingEvent -= this.OnEcasSavedEvent;
                _host = null;
                Card.Disconnect();
                Card = null;
            }
        }
        private void OnEcasSaveEvent(object sender, EcasRaisingEventArgs e)
        {
            if (e.Event.Type.Equals(SavingDatabaseFile))
                SavingAction = true;
        }
        private void OnEcasSavedEvent(object sender, EcasRaisingEventArgs e)
        {
            if (e.Event.Type.Equals(SavedDatabaseFile))
                SavingAction = false;
        }
    }
}
