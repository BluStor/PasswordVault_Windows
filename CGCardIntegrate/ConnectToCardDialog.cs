using System;
using System.Windows.Forms;

namespace CGCardIntegrate
{
    public partial class ConnectToCardDialog : Form
    {
        private string _result;
        public string Result { get { return _result; } }
        public ConnectToCardDialog()
        {
            InitializeComponent();
        }

        private void btmOK_Click(object sender, EventArgs e)
        {
            _result = tbCardName.Text;
        }
    }
}
