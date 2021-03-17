using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vMixPanelTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager;
        private readonly ViewModel viewModel;
        private ObservableCollection<PanelLink> _panelLinks;
        public MainWindow(ObservableCollection<PanelLink> panelLinks)
        {
            InitializeComponent();

            _panelLinks = panelLinks;

            this.viewModel = new ViewModel
            {
                PanelLinks = _panelLinks
            };

            this.DataContext = this.viewModel;
        }

        
    }
    public class ViewModel
    {
        public IList<PanelLink> PanelLinks { get; set; }
    }
}
