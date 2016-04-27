using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace HCWX
{
    public partial class SettngsForm : Form
    {
        public SettngsForm()
        {
            InitializeComponent();
        }

        static string[] actions = new string[] { "-", "Switch windows", "Show Strat menu", "Show Desktop", "Show Notifications" };
        static bool exit = false;
        int hkid = 234;

        private void SettngsForm_Load(object sender, EventArgs e)
        {
            cbTL.Items.AddRange(actions);
            cbTR.Items.AddRange(actions);
            cbBL.Items.AddRange(actions);
            cbBR.Items.AddRange(actions);
            LoadSettings();
            ApplySettings();
            RegisterHotKey(this.Handle, hkid, (int)KeyModifier.WinKey, Keys.F5.GetHashCode());
        }

        private void SettngsForm_Shown(object sender, EventArgs e)
        {
            ApplySettings();
            this.Hide();
        }

        #region MenuActions
        private void menuAbout_Click(object sender, EventArgs e)
        {
            ShowMe();
            tpc.SelectedTab = tpAbout;
        }

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!this.Visible)
            {
                ShowMe();
            }
            else
                this.Hide();
        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                ShowMe();
                tpc.SelectedTab = tpHotCorners;
            }
            else
                this.Hide();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            exit = true;
            Application.Exit();
        }
        #endregion

        #region Settings
        private void cbON_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Enabled = cbON.Checked;
            Properties.Settings.Default.Save();
            ApplySettings();
        }

        private void cbTray_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowIcon = cbTray.Checked;
            Properties.Settings.Default.Save();
            ApplySettings();
        }

        private void cbAutostart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Autostart = cbAutostart.Checked;
            Properties.Settings.Default.Save();
            ApplySettings();
        }

        private void cbTL_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TL = cbTL.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void cbTR_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TR = cbTR.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void cbBL_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.BL = cbBL.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void cbBR_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.BR = cbBR.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void numTL_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.numTL = (int)numTL.Value;
            Properties.Settings.Default.Save();
        }

        private void numTR_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.numTR = (int)numTR.Value;
            Properties.Settings.Default.Save();
        }

        private void numBL_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.numBL = (int)numBL.Value;
            Properties.Settings.Default.Save();
        }

        private void numBR_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.numBR = (int)numBR.Value;
            Properties.Settings.Default.Save();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        #endregion

        private void LoadSettings()
        {
            tray.Visible = Properties.Settings.Default.ShowIcon;
            cbON.Checked = Properties.Settings.Default.Enabled;
            cbAutostart.Checked = Properties.Settings.Default.Autostart;
            cbTray.Checked = Properties.Settings.Default.ShowIcon;
            cbTL.SelectedIndex = Properties.Settings.Default.TL;
            cbTR.SelectedIndex = Properties.Settings.Default.TR;
            cbBL.SelectedIndex = Properties.Settings.Default.BL;
            cbBR.SelectedIndex = Properties.Settings.Default.BR;
            numBL.Value = Properties.Settings.Default.numBL;
            numTL.Value = Properties.Settings.Default.numTL;
            numTR.Value = Properties.Settings.Default.numTR;
            numBR.Value = Properties.Settings.Default.numBR;
        }

        private void ApplySettings()
        {
            if (Properties.Settings.Default.Enabled)
                MouseHook.Hook();
            else
                MouseHook.unHook();

            tray.Visible = Properties.Settings.Default.ShowIcon;

            //Set autostart reg key
            if (Properties.Settings.Default.Autostart)
            {
                //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
                RegistryKey Sy = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
                Sy.SetValue("SyAPP", Application.ExecutablePath.ToString());
            }
            else
            {
                try
                {
                    RegistryKey Sy = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
                    Sy.DeleteValue("SyAPP");
                }
                catch { }
            }
        }

        private void SettngsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exit)
            {
                MouseHook.unHook();
                UnregisterHotKey(this.Handle, hkid);
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        #region HotKey
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

                if (!this.Visible)
                {
                    ShowMe();
                }
                else
                    this.Hide();
            }
        }
        #endregion

        private void ShowMe()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            exit = true;
            Application.Exit();
        }

        private void btnPayPal_Click(object sender, EventArgs e)
        {
            string url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=ZN5W4BAMH4VSW&lc=UA&item_name=HotCorners%20App%20for%20Windows%2010&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted";
            System.Diagnostics.Process.Start(url);
        }

        private void linkMail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "mailto:y.savchenko@outlook.com";
            System.Diagnostics.Process.Start(url);
        }
    }
}
