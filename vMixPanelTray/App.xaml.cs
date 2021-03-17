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

namespace vMixPanelTray
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
        private ObservableCollection<PanelLink> _panelLinks;

        public App()
        {}
        protected override void OnStartup(StartupEventArgs e)
        {
            Icon trayIcon = new Icon("icon.ico");
            _contextMenu = new Forms.ContextMenuStrip();
            _contextMenu.Items.Add("Settings", null, OnSettingsClick);
            _portsItem = new Forms.ToolStripMenuItem("Port");
            _contextMenu.Items.Add("Exit", null, OnExitClicked);
            _contextMenu.Items.Add(_portsItem);

            _settingsManager = new SettingsManager("vMixPanelSettings.json");
            _settingsManager.Load();
            _settingsManager.Save();

            _panelManager = new PanelManager();

            ConnectPanel();

            _vmixManager = new VmixManager();
            _vmixManager.Connect(_panelManager.HandleState);
            _panelManager.VmixManager = _vmixManager;
            
            List<PanelLink> panelLinks = new List<PanelLink>(_settingsManager.PanelSettings.Links);
            panelLinks.Sort((PanelLink p1, PanelLink p2) => (int)p1.Control - (int)p2.Control);
            _panelLinks = new ObservableCollection<PanelLink>(panelLinks);
            _panelLinks.CollectionChanged += OnPanelLinkCollectionChanged;
            applyChangeDetection(_panelLinks);
            _panelManager.ApplyLinks(_panelLinks);



            LoadContextMenuPorts();
            this._notifyIcon = new Forms.NotifyIcon
            {
                Visible = true,
                Text = "vMix Panel",
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
                Debug.Print("Failed to connect to Panel");
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

        private void OnSettingsClick(object sender, EventArgs e)
        {
            MainWindow window = new MainWindow(_panelLinks);
            window.Show();
        }

        private void OnPanelLinkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PanelLink item in e.OldItems)
                {
                    item.PropertyChanged -= OnPanelLinkCollectionModified;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                applyChangeDetection(e.NewItems);
            }


            _panelManager.ApplyLinks(_panelLinks);
        }

        private void applyChangeDetection(IList newItems)
        {
            foreach (PanelLink item in newItems)
            {
                item.PropertyChanged += OnPanelLinkCollectionModified;
            }
        }

        private void OnPanelLinkCollectionModified(object sender, PropertyChangedEventArgs e)
        {
            _settingsManager.PanelSettings.Links = new Collection<PanelLink>(_panelLinks);
            _settingsManager.Save();
            _panelManager.ApplyLinks(_panelLinks);
            Debug.Print("modified");
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit?", "vMix Panel", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _notifyIcon.Visible = false;
                Current.Shutdown();
            }
        }
    }
}
