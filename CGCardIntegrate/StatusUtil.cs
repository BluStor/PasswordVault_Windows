using KeePass.Forms;
using KeePass.UI;
using KeePassLib;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CGCardIntegrate
{

    public sealed class StatusStateInfo
    {
        private ProgressBarStyle m_pbs = ProgressBarStyle.Continuous;
        public ProgressBarStyle Style
        {
            get { return m_pbs; }
            set { m_pbs = value; }
        }

        private bool m_bVisible = true;
        public bool Visible
        {
            get { return m_bVisible; }
            set { m_bVisible = value; }
        }

        private StatusProgressForm m_form = null;
        public StatusProgressForm Form
        {
            get { return m_form; }
            set { m_form = value; }
        }

        public StatusStateInfo()
        {
        }

        public StatusStateInfo(ProgressBarStyle pbs)
        {
            m_pbs = pbs;
        }
    }

    public static class StatusUtil
    {
        private static StatusProgressForm BeginStatusDialog(string strText)
        {
            StatusProgressForm dlg = new StatusProgressForm();
            dlg.InitEx(PwDefs.ShortProductName, false, true, null);
            dlg.Show();
            dlg.StartLogging(strText ?? string.Empty, false);

            return dlg;
        }

        private static void EndStatusDialog(StatusProgressForm dlg)
        {
            if (dlg == null) { Debug.Assert(false); return; }

            dlg.EndLogging();
            dlg.Close();
            UIUtil.DestroyForm(dlg);
        }

        public static StatusStateInfo Begin(string strText)
        {
            StatusStateInfo s = new StatusStateInfo();

            try
            {
                s.Form = BeginStatusDialog(strText);
            }
            catch (Exception) { Debug.Assert(false); }

            return s;
        }

        public static void End(StatusStateInfo s)
        {
            if (s == null) { Debug.Assert(false); return; }

            try
            {
                if (s.Form != null)
                {
                    EndStatusDialog(s.Form);
                    s.Form = null;
                }
                else
                {
                    MainForm mf = CGCardIntegrateExt.Host.MainWindow;
                    mf.MainProgressBar.Style = s.Style;
                    mf.MainProgressBar.Visible = s.Visible;
                }
            }
            catch (Exception) { Debug.Assert(false); }
        }
    }
}
