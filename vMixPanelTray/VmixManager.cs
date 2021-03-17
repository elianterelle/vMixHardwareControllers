using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

namespace vMixPanelTray
{
    public class VmixManager
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _streamReader;
        private Action<VmixState> _handleState;

        public void Connect(Action<VmixState> handleState, string ip = "10.16.12.21", Int32 port = 8099)
        {
            try
            {
                TcpClient client = new TcpClient(ip, port);
                _stream = client.GetStream();
                _streamReader = new StreamReader(_stream, Encoding.UTF8);
                _handleState = handleState;
                Listen(_handleState);

                Debug.Print("Connected!");
                SendMessage("SUBSCRIBE ACTS");
                SendMessage("XML");
            }
            catch (ArgumentNullException e)
            {
                Debug.Print("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Debug.Print("SocketException: {0}", e);
            }
        }

        public void Close()
        {
            _stream.Close();
            _client.Close();
        }

        public void SendMessage(string message)
        {
            Debug.Print("Send Vmix Message: " + message);
            Byte[] data = Encoding.ASCII.GetBytes(message + "\r\n");
            try
            {
                _stream.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException e)
            {
                Debug.Print("Failed to Write to Socket");
                Connect(_handleState);
            }
        }

        public async Task<bool> Listen(Action<VmixState> handleState)
        {
            _handleState = handleState;

            while (true)
            {
                string response = await _streamReader.ReadLineAsync();
                if (response == null) continue;

                Debug.Print(response);

                int commandSpaceIndex = response.IndexOf(' ');
                string command = response.Substring(0, commandSpaceIndex);
                Debug.Print("CMD" + command);
                if (command == "XML")
                {
                    string xml = await _streamReader.ReadLineAsync();
                    Debug.Print(xml);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    int preview = Int32.Parse(doc.DocumentElement.SelectNodes("preview")[0].InnerText);
                    int active = Int32.Parse(doc.DocumentElement.SelectNodes("active")[0].InnerText);

                    XmlNodeList overlaysXml = doc.DocumentElement.SelectSingleNode("overlays").SelectNodes("overlay");
                    VmixOverlay[] overlays = new VmixOverlay[4];

                    for (int i = 0; i < 4; i++)
                    {
                        XmlNode overlayXml = overlaysXml[i];
                        if (overlayXml == null) continue;
                        int value = -1;

                        if (overlayXml.InnerText != "")
                        {

                            value = int.Parse(overlayXml.InnerText);
                        }

                        int number = Int32.Parse(overlayXml.Attributes["number"].Value);

                        overlays[i] = new VmixOverlay
                        {
                            Value = value,
                            Number = number
                        };
                    }

                    bool fadeToBlack = doc.DocumentElement.SelectNodes("fadeToBlack")[0].InnerText == "True";

                    Debug.Print(fadeToBlack.ToString());
                    VmixState state = new VmixState
                    {
                        Active = active,
                        Preview = preview,
                        Overlays = overlays,
                        FadeToBlack = fadeToBlack
                    };

                    handleState(state);
                    continue;
                }

                string status = response.Substring(commandSpaceIndex + 1, 2);
                string message = response.Substring(commandSpaceIndex + 4);

                if (status != "OK")
                {
                    Debug.Print("vMix TCP Api Error: " + response + "|" + command + "|" + status + "|");
                    continue;
                }
                Debug.Print(command + " " + message);
                switch (command)
                {
                    case "ACTS":
                        SendMessage("XML");
                        break;
                }
            }
        }
    }
}