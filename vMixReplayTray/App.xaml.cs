using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using System.Drawing;
using System.IO.Ports;
using System.Collections;

namespace vMixReplayTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Forms.NotifyIcon _notifyIcon;
        private Forms.ContextMenuStrip _contextMenu;
        private Forms.ToolStripMenuItem _portsItem;
        private SettingsManager _settingsManager;
        private VmixManager _vmixManager;
        private PanelManager _panelManager;

        public App() {}
        protected override void OnStartup(StartupEventArgs e)
        {
            Icon trayIcon = new Icon("icon.ico");
            _contextMenu = new Forms.ContextMenuStrip();
            _portsItem = new Forms.ToolStripMenuItem("Port");
            _contextMenu.Items.Add("Exit", null, OnExitClicked);
            _contextMenu.Items.Add(_portsItem);

            _settingsManager = new SettingsManager("vMixReplaySettings.json");
            _settingsManager.Load();
            _settingsManager.Save();

            _panelManager = new PanelManager();

            ConnectPanel();

            _vmixManager = new VmixManager();
            _vmixManager.Connect();
            _panelManager.VmixManager = _vmixManager;
            

            LoadContextMenuPorts();
            this._notifyIcon = new Forms.NotifyIcon
            {
                Visible = true,
                Text = "vMix Replay",
                Icon = trayIcon,
                ContextMenuStrip = _contextMenu
            };
            
            base.OnStartup(e);
            
        }

        private void ConnectPanel()
        {
            try
            {
                _panelManager.Connect(_settingsManager.PanelSettings.ComPort);
            }
            catch (Exception)
            {
                Debug.Print("Failed to connect to Replay");
            }

            LoadContextMenuPorts();
        }

        private void LoadContextMenuPorts(object sender = null, EventArgs e = null)
        {
            string[] ports = _panelManager.GetPorts();
            _portsItem.DropDownItems.Clear();
            foreach (string port in ports)
            {
                Forms.ToolStripMenuItem portItem = new Forms.ToolStripMenuItem(port, null, ConnectPanelCom);
                portItem.Checked = _panelManager.PortName == port;
                _portsItem.DropDownItems.Add(portItem);
            }

            _portsItem.DropDownItems.Add("Reload", null, LoadContextMenuPorts);
            _portsItem.DropDownItems.Add("Reconnect", null, ReconnectPanel);
        }
        private void ReconnectPanel(object sender, EventArgs e)
        {
            ConnectPanel();
        }

        private void ConnectPanelCom(object sender, EventArgs e)
        {
            _settingsManager.PanelSettings.ComPort = sender.ToString();
            _settingsManager.Save();
            ConnectPanel();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit?", "vMix Replay", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _notifyIcon.Visible = false;
                Current.Shutdown();
            }
        }
    }
}
