using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Xml;

namespace vMixReplayTray
{
    public class PanelManager
    {
        private SerialPort _serialPort;

        private int[] _lastState = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
        public VmixManager VmixManager;
        public string PortName;

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
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            string message = serialPort.ReadLine();


            string[] stringParts = message.Split(",");


            if (stringParts.Length != 9) return;

            if (VmixManager == null) return;

            int[] state = Array.ConvertAll(stringParts, part =>
            {
                if (part == "")
                {
                    return 0;
                }

                return int.Parse(part);
            });

            /*if (_lastState[0] != state[0] && state[0] == 1) // Pause
            {
                // VmixManager.SendMessage("FUNCTION ReplayChangeSpeed Value=0");
            }

            if (_lastState[1] != state[1] && state[1] == 1) // Reset / IN
            {
                VmixManager.SendMessage("FUNCTION ReplayJumpToNow");
                Thread.Sleep(150);
                VmixManager.SendMessage("FUNCTION ReplayMarkIn");
            }

            if (_lastState[2] != state[2] && state[2] == 1) // Out / Next
            {
                if (VmixManager.GetState().IsLive)
                {
                    VmixManager.SendMessage("FUNCTION ReplayPlayNext");
                }
                else
                {
                    VmixManager.SendMessage("FUNCTION ReplayMarkOut");
                }
            }

            if (_lastState[3] != state[3] && state[3] == 1) // Play / Stop
            {
                if (VmixManager.GetState().IsLive)
                {
                    VmixManager.SendMessage("FUNCTION ReplayStopEvents");
                }
                else
                {
                    VmixManager.SendMessage("FUNCTION ReplayPlayEventToOutput Value=0");
                }
            }

            if (_lastState[4] != state[4] && state[4] == 1) // Input 1
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera1");
            }

            if (_lastState[5] != state[5] && state[5] == 1) // Input 2
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera2");
            }

            if (_lastState[6] != state[6] && state[6] == 1) // Input 3
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera3");
            }

            if ((state[7] > 1 || state[7] < -1) && state[0] == 1) // Jog Wheel
            {
                int value = -1 * state[7] / 2;
                VmixManager.SendMessage("FUNCTION ReplayJumpFrames Value=" + value);
            }

            if ((_lastState[8] != state[8] && state[0] == 0) || (_lastState[0] != state[0] && state[0] == 0)) // Slider
            {
                float newValue = (float)(100 - state[8]) / (float)100;
                float lastValue = (float)(100 - _lastState[8]) / (float)100;
                float value = newValue - lastValue;
                Debug.WriteLine(value);
                VmixManager.SendMessage("FUNCTION ReplayChangeSpeed Value=" + value.ToString("0.00").Replace(",", "."));
            }*/

            if (VmixManager.GetState() == null)
            {
                return;
            }

            if (_lastState[0] != state[0]) // Pause
            {
                if ((VmixManager.GetState().IsLive && state[0] == 1) || (!VmixManager.GetState().IsLive && state[0] == 0))
                {
                    VmixManager.SendMessage("FUNCTION ReplayPlayPause");
                }
            }

            if (_lastState[1] != state[1] && state[1] == 1) // Preview
            {
                VmixManager.SendMessage("FUNCTION PreviewInput Input=16");
            }

            if (_lastState[2] != state[2] && state[2] == 1) // Reset
            {
                VmixManager.SendMessage("FUNCTION ReplayJumpToNow");

                Thread.Sleep(100);
                if (!VmixManager.GetState().IsLive)
                {
                    VmixManager.SendMessage("FUNCTION ReplayPlayPause");
                }
            }

            if (_lastState[3] != state[3] && state[3] == 1) // CUT
            {
                if (!VmixManager.GetState().IsLive)
                {
                    VmixManager.SendMessage("FUNCTION ReplayPlayPause");
                }

                VmixManager.SendMessage("FUNCTION Transition4");
            }

            if (_lastState[4] != state[4] && state[4] == 1) // Input 1
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera1");
            }

            if (_lastState[5] != state[5] && state[5] == 1) // Input 2
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera2");
            }

            if (_lastState[6] != state[6] && state[6] == 1) // Input 3
            {
                VmixManager.SendMessage("FUNCTION ReplayACamera3");
            }

            if ((state[7] > 1 || state[7] < -1) && state[0] == 1) // Jog Wheel
            {
                int value = -1 * state[7] / 2;
                VmixManager.SendMessage("FUNCTION ReplayJumpFrames Value=" + value);
            }

            if ((_lastState[8] != state[8] && state[0] == 0) || (_lastState[0] != state[0] && state[0] == 0)) // Slider
            {
                float newValue = (float)(100 - state[8]) / (float)100;
                float lastValue = (float)(100 - _lastState[8]) / (float)100;
                float value = newValue - lastValue;
                Debug.WriteLine(value);
                VmixManager.SendMessage("FUNCTION ReplayChangeSpeed Value=" + value.ToString("0.00").Replace(",", "."));
            }

            _lastState = state;
        }
    }
}