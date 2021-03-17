using System;
using System.Xml.Serialization;

namespace vMixPanelTray
{
    [Serializable]
    public class VmixState
    {
        public VmixState() {}
        /*[XmlElement("version")]
        public string Version  { get; set; }

        [XmlElement("edition")]
        public string Edition  { get; set; }

        [XmlElement("inputs")]
        public VmixInput[] Inputs  { get; set; }*/

        [XmlElement("preview")]
        public int Preview { get; set; }

        [XmlElement("active")]
        public int Active { get; set; }

        [XmlElement("overlays")]
        public VmixOverlay[] Overlays { get; set; }

        [XmlElement("fadeToBlack")]
        public bool FadeToBlack { get; set; }
    }

    public class VmixOverlay
    {
        public int Number { get; set; }

        public int Value { get; set; }
    }
    /*
    public class VmixInput
    {
        [XmlElement("key")]
        public string Key  { get; set; }

        [XmlElement("number")]
        public int Number  { get; set; }

        [XmlElement("type")]
        public string Type  { get; set; }

        [XmlElement("title")]
        public string Title  { get; set; }

        [XmlElement("shortTitle")]
        public string ShortTitle  { get; set; }

        [XmlElement("state")]
        public string State  { get; set; }

        [XmlElement("position")]
        public int Position  { get; set; }

        [XmlElement("duration")]
        public int Duration  { get; set; }

        [XmlElement("loop")]
        public string Loop  { get; set; }
    }*/
}