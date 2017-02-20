using KeePass.Plugins;

namespace CGCardIntegrate
{
    public sealed class CGCardIntegrateExt : Plugin
    {
        private static IPluginHost m_host = null;
        internal static IPluginHost Host { get { return m_host; } }
        public override bool Initialize(IPluginHost host)
        {
            if (m_host != null) Terminate();
            m_host = host;
            (new CyberGateWebRequestCreator()).Register();
            return true;
        }

        public override void Terminate()
        {
            if (m_host != null)
            {
                m_host = null;
            }
        }
    }
}
