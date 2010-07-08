namespace Exyll.MotorolaUp
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Media;
    using System.Text;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        UptimeController c;

        public MainForm()
        {
            InitializeComponent();
            notifyIcon1.Icon = Icon;
            notifyIcon1.Visible = true;
            notifyIcon1.Text = Text;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (File.Exists("modem-uptime.log"))
            {
                foreach (var line in File.ReadLines("modem-uptime.log", Encoding.ASCII))
                {
                    listView1.Items.Add(new ListViewItem(line));
                }
            }

            c = new UptimeController();
            c.OnError += c_OnError;
            c.OnReboot += c_OnReboot;
            c.OnUpdate += c_OnUpdate;
        }

        void c_OnUpdate(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                Text = string.Format("Uptime: {0} (Since: {1})", c.Uptime, c.Boottime);
            }));
        }

        void c_OnReboot(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                SystemSounds.Asterisk.Play();
                notifyIcon1.ShowBalloonTip(5000, "Reboot", "Modem rebooted around " + c.Boottime, ToolTipIcon.Warning);
                var item1 = DateTime.Now.ToString();
                listView1.Items.Add(new ListViewItem(item1, c.Uptime.ToString()));
                File.AppendAllText("modem-uptime.log", item1 + Environment.NewLine, Encoding.ASCII); 
            }));
        }

        void c_OnError(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                Text = "Error";
                notifyIcon1.ShowBalloonTip(5000, "Error", "Could not resolve uptime information from modem.", ToolTipIcon.Error);
            }));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
            //this.WindowState = FormWindowState.Minimized;
            
            //var v = Environment.OSVersion.Version;
            //if (Environment.OSVersion.Platform == PlatformID.Win32NT && (v.Major > 6 || (v.Major==6 && v.Minor>0)) )
            //{
            //}
            //else
            //{
            //}
            //ShowInTaskbar = false;
            //this.Visible = false;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x112;
            const int SC_MINIMIZE = 0xF020;

            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam == (IntPtr)SC_MINIMIZE)
                {
                    Hide();
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
        }
    }
}
