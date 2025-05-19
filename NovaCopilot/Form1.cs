using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NovaCopilot
{
    public partial class Form1: Form
    {
        private NovaCopilotAddon.NovaCopilot _copilot;
        public Form1()
        {
            InitializeComponent();
            _copilot = new NovaCopilotAddon.NovaCopilot(this.Handle);
            _copilot.Start();
        }
        protected override void WndProc(ref Message m)
        {
            System.Diagnostics.Debugger.Log(0, "General", $"{m.Msg}\n");
            if (m.Msg == 0x0402) // WM_USER_SIMCONNECT
            {
                _copilot?.SimConnectMessage(); // You’ll need to expose this method
            }
            base.WndProc(ref m);
        }

    }
}
