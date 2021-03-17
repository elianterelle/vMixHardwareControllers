using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Xml;

namespace vMixPanelTray
{
    public class PanelManager
    {
        private SerialPort _serialPort;

        private UInt32 _lastButtonState;
        private ObservableCollection<PanelLink> _panelLinks;
        private VmixState _vmixState;
        public VmixManager VmixManager;
        public string PortName;
        public void ApplyLinks(ObservableCollection<PanelLink> panelLinks)
        {
            _panelLinks = panelLinks;
            UpdateButtons();
        }
        
        public void HandleState(VmixState state)
        {
            _vmixState = state;

            UpdateButtons();
        }

        public void Connect(string port)
        {
            this.PortName = "";

            if (_serialPort != null)
            {
                _serialPort.Close();
            }

            this._serialPort = new SerialPort
            {
                PortName = port,
                BaudRate = 9600
            };

            this._serialPort.DataReceived += this.OnSerialDataReceived;
            this._serialPort.Open();
            this.PortName = port;
            UpdateButtons();
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            int bytesToRead = serialPort.BytesToRead;

            if (bytesToRead < 4) return;

            byte[] buffer = new byte[bytesToRead];
            serialPort.Read(buffer, 0, buffer.Length);
            UInt32 buttonState = BitConverter.ToUInt32(buffer, 0);
            this.OnButtonStateChange(buttonState, _lastButtonState);
            this._lastButtonState = buttonState;
        }

        private void OnButtonStateChange(UInt32 oldState, UInt32 state)
        {
            if (oldState == state) return;

            for (int i = 0; i < 32; i++)
            {
                bool oldBit = ((oldState >> i) & 1) == 1;
                bool bit = ((state >> i) & 1) == 1;
                if (oldBit == bit) continue;

                if (bit)
                {
                    OnButtonRelease(i);
                }
                else
                {
                    OnButtonPress(i);
                }
            }
        }

        private void OnButtonPress(int button)
        {
            if (VmixManager == null) return;
            if (_panelLinks == null) return;

            foreach (PanelLink link in _panelLinks)
            {
                if ((int) link.Control != button) continue;

                switch (link.Type)
                {
                    case PanelLinkType.ProgramPreviewInput:
                    case PanelLinkType.PreviewInput:
                        VmixManager.SendMessage("FUNCTION PreviewInput Input=" + link.Value);
                        break;

                    case PanelLinkType.ProgramInput:
                        VmixManager.SendMessage("FUNCTION ActiveInput Input=" + link.Value);
                        break;

                    case PanelLinkType.Cut:
                        VmixManager.SendMessage("FUNCTION Cut");
                        break;

                    case PanelLinkType.Overlay1:
                        VmixManager.SendMessage("FUNCTION OverlayInput1 Input=" + link.Value);
                        break;

                    case PanelLinkType.Overlay2:
                        VmixManager.SendMessage("FUNCTION OverlayInput2 Input=" + link.Value);
                        break;

                    case PanelLinkType.Overlay3:
                        VmixManager.SendMessage("FUNCTION OverlayInput3 Input=" + link.Value);
                        break;

                    case PanelLinkType.Overlay4:
                        VmixManager.SendMessage("FUNCTION OverlayInput4 Input=" + link.Value);
                        break;
                    case PanelLinkType.FadeToBlack:
                        VmixManager.SendMessage("FUNCTION FadeToBlack");
                        break;
                }
            }
        }

        private void OnButtonRelease(int button)
        {
            // Currently unused
        }

        private void UpdateButtons()
        {
            UInt64 buttonState = 0;
            
            if (_panelLinks == null) return;
            if (_vmixState == null) return;

            foreach (PanelLink link in _panelLinks)
            {
                int i = (int) link.Control;
                //Debug.Print(i.ToString());
                int redBitPosition = i * 2;
                int greenBitPosition = i * 2 + 1;

                UInt64 redBit = 0;
                UInt64 greenBit = 0;

                

                switch (link.Type)
                {
                    case PanelLinkType.PreviewInput:
                        if (link.Value == null) continue;

                        if (_vmixState.Preview == int.Parse(link.Value))
                        {
                            greenBit = 1;
                        }
                        break;

                    case PanelLinkType.ProgramInput:
                        if (link.Value == null) continue;

                        if (_vmixState.Active == int.Parse(link.Value))
                        {
                            redBit = 1;
                        }
                        break;

                    case PanelLinkType.ProgramPreviewInput:
                        if (link.Value == null) continue;

                        if (_vmixState.Active == int.Parse(link.Value))
                        {
                            redBit = 1;
                            break; // Do not display Yellow if Input is Preview and Program
                        }

                        if (_vmixState.Preview == int.Parse(link.Value))
                        {
                            greenBit = 1;
                        }

                        break;

                    case PanelLinkType.Overlay1:
                    case PanelLinkType.Overlay2:
                    case PanelLinkType.Overlay3:
                    case PanelLinkType.Overlay4:
                        if (link.Value == null) continue;

                        int overlayNumber = 0;

                        switch (link.Type)
                        {
                            case PanelLinkType.Overlay1:
                                overlayNumber = 1;
                                break;
                            case PanelLinkType.Overlay2:
                                overlayNumber = 2;
                                break;
                            case PanelLinkType.Overlay3:
                                overlayNumber = 3;
                                break;
                            case PanelLinkType.Overlay4:
                                overlayNumber = 4;
                                break;
                        }

                        foreach (VmixOverlay overlay in _vmixState.Overlays)
                        {
                            if (overlay.Number != overlayNumber) continue;

                            if (overlay.Value == int.Parse(link.Value))
                            {
                                redBit = 1;
                            }
                            break;
                        }
                        break;

                    case PanelLinkType.FadeToBlack:
                        Debug.Print("_vmixState.FadeToBlack.ToString()");
                        if (_vmixState.FadeToBlack)
                        {
                            redBit = 1;
                        }
                        break;
                }

                redBit = redBit << redBitPosition;
                greenBit = greenBit << greenBitPosition;
                buttonState = buttonState | redBit;
                buttonState = buttonState | greenBit;
            }
            
            byte[] bytes = BitConverter.GetBytes(buttonState);
            try
            {
                _serialPort.Write(bytes, 0, 8);
            }
            catch (Exception e)
            {
                Connect(PortName);
            }
        }
    }
}