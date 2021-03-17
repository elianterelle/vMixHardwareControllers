using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace vMixPanelTray
{
    public class PanelSettings
    {
        public ICollection<PanelLink> Links { get; set; } = new Collection<PanelLink>();
        public string ComPort { get; set; } = "COM5";
    }
}