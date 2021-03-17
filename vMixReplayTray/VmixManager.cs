using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using System.Xml;

namespace vMixReplayTray
{
    public class VmixManager
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private VmixState _state;
        private StreamReader _streamReader;

        public void Connect(string ip = "10.16.12.21", Int32 port = 8099)
        {
            try
            {
                TcpClient client = new TcpClient(ip, port);
                _stream = client.GetStream();
                _streamReader = new StreamReader(_stream, Encoding.UTF8);
                Listen();

                Debug.Print("Connected!");

                SendMessage("SUBSCRIBE ACTS");
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
        
        public async Task<bool> Listen()
        {
            while (true)
            {
                string response = await _streamReader.ReadLineAsync();
                if (response == null) continue;

                int commandSpaceIndex = response.IndexOf(' ');
                string command = response.Substring(0, commandSpaceIndex);

                if (command == "XML")
                {
                    string xml = await _streamReader.ReadLineAsync();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlNode replayNode = doc.DocumentElement.GetElementsByTagName("replay")[0];

                    if (replayNode == null)
                    {
                        Debug.Print("No Replay State found");
                        continue;
                    }
                    bool isLive = replayNode.ParentNode.Attributes["state"].Value == "Running";
                    int cameraA = int.Parse(replayNode.Attributes["cameraA"].Value);
                    int cameraB = int.Parse(replayNode.Attributes["cameraB"].Value);
                    int active = Int32.Parse(doc.DocumentElement.SelectNodes("active")[0].InnerText);
                    VmixState state = new VmixState
                    {
                        IsLive = isLive,
                        Active = active,
                        CameraA = cameraA,
                        CameraB = cameraB
                    };

                    _state = state;
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
                Connect();
            }
        }

        public VmixState GetState()
        {
            return _state;
        }
    }
}