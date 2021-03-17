using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace vMixPanelTray
{
    public class PanelLink : INotifyPropertyChanged
    {
        private PanelControl _control;

        public PanelControl Control
        {
            get => _control;
            set
            {
                _control = value;
                NotifyPropertyChanged();
            }
        }

        private PanelLinkType _type;
        public PanelLinkType Type
        {
            get => _type;
            set
            {
                _type = value;
                SetValues(value);
            }
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> _values;
        public List<string> Values
        {
            get => _values;
            set
            {
                _values = value;
                NotifyPropertyChanged();
            }
        }

        public PanelLink()
        {
            Values = new List<string>();
            SetValues(Type);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetValues(PanelLinkType type)
        {
            this.Values.Clear();

            switch (type)
            {
                case PanelLinkType.Overlay1:
                case PanelLinkType.Overlay2:
                case PanelLinkType.Overlay3:
                case PanelLinkType.Overlay4:
                case PanelLinkType.PreviewInput:
                case PanelLinkType.ProgramInput:
                case PanelLinkType.ProgramPreviewInput:
                    for (int i = 1; i <= 16; i++)
                    {
                        this.Values.Add(i.ToString());
                    }
                    break;
                case PanelLinkType.Transition:
                    this.Values.Add("Fade");
                    this.Values.Add("Merge");
                    break;
            }
            NotifyPropertyChanged();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum PanelLinkType
    {
        PreviewInput,
        ProgramInput,
        ProgramPreviewInput,
        Cut,
        Transition,
        Overlay1,
        Overlay2,
        Overlay3,
        Overlay4,
        FadeToBlack
    }

    public enum PanelControl
    {
        LeftTop1, LeftTop2, LeftTop3, LeftTop4, LeftTop5, LeftTop6, LeftTop7, LeftTop8,
        LeftBottom1, LeftBottom2, LeftBottom3, LeftBottom4, LeftBottom5, LeftBottom6, LeftBottom7, LeftBottom8,
        MiddleTop1, MiddleTop2, RMiddleTop3,
        MiddleBottom1, MiddleBottom2,
        RightTop1, RightTop2,
        RightBottom1
    }
}