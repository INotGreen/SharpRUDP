using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace RUDP
{
    public class ReliableUDPClient
    {
        private Socket UDPsocket;
        private EndPoint serverEndPoint;

        public bool ActivatePong { get; set; }
        private MemoryStream ClientMS { get; set; }
        private byte[] ClientBuffer { get; set; }
        public object SendSync { get; } = new object();
        public bool IsConnected =false;
        private System.Threading.Timer KeepAlive { get; set; }
        private System.Threading.Timer HeartbeatTimer { get; set; }
        private readonly int HeartTime = 5000; 
        public async void ConnectAsync()
        {
            try
            {
                InitializeClient(Program.Host, Program.Port);
                await Task.Delay(3000);


                while (!IsConnected)
                {
                    Thread.Sleep(10);
                }

                if (IsConnected)
                {
                    BeginSend(Encoding.UTF8.GetBytes("Hello,world"));
                }
                else
                {
                    ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed");
                await Task.Delay(3000);  
                ConnectAsync();
            }
        }


        

        public void InitializeClient(string serverAddress, int serverPort)
        {
            UDPsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            ClientBuffer = new byte[1024];
            ClientMS = new MemoryStream();
            BeginSend(Encoding.UTF8.GetBytes("ACK"));
            HeartbeatTimer = new System.Threading.Timer(HeartbeatTimeout, null, Timeout.Infinite, Timeout.Infinite);
            KeepAlive = new System.Threading.Timer(new System.Threading.TimerCallback(KeepAlivePacket), null, 4000, 4000);
            UDPsocket.BeginReceiveFrom(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ref serverEndPoint, ReadClientData, null);
        }

        

        public void ReadClientData(IAsyncResult ar)
        {
            try
            {

                int Recevied = UDPsocket.EndReceiveFrom(ar, ref serverEndPoint);

                if (Recevied > 0)
                {
                    string receivedData = Encoding.UTF8.GetString(ClientBuffer, 0, Recevied);

                    ClientMS.WriteAsync(ClientBuffer, 0, Recevied);
                    //MessageBox.Show(Encoding.UTF8.GetString(ClientMS.ToArray()));
                    switch (Encoding.UTF8.GetString(ClientMS.ToArray()))
                    {
                        case "SYN":
                            {
                                
                                IsConnected = true;
                                Console.WriteLine("Connect sucessfully!");
                                break;
                            }
                        case "SYNACK":
                            {
                                IsConnected = true;
                                Console.WriteLine("Connect sucessfully!");
                                break;
                            }
                        case "HEARTBACK":
                            {
                                IsConnected = true;

                                // Reset the timer when receiving a heartbeat packet and set a timeout of 6 seconds
                                HeartbeatTimer.Change(6000, Timeout.Infinite);

                                Console.WriteLine("HEARTBACK received.");
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
                                break;
                            }
                    }
                    ClientMS.Dispose();
                    ClientMS = new MemoryStream();
                    ClientBuffer = new byte[1024];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                UDPsocket.BeginReceiveFrom(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ref serverEndPoint, ReadClientData, null);
            }
        }

       


        public void BeginSend(object msg)
        {
            lock (SendSync)
            {
                try
                {
                    if ((byte[])msg == null) return;

                    byte[] buffer = ((byte[])msg);
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    UDPsocket.SendTo(buffersize, serverEndPoint);
                    UDPsocket.SendTo(buffer, serverEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        private void HeartbeatTimeout(object state)
        {
            // 如果计时器超时，将IsConnected设置为false
            IsConnected = false;
            Console.WriteLine("Connection lost: no heartbeat received.");
            
        }
        public void Reconnect()
        {
            try
            {
                KeepAlive?.Dispose();
                ClientMS?.Dispose();
            }
            finally
            {
                InitializeClient(Program.Host,Program.Port);
            }
        }
        public void KeepAlivePacket(object obj)
        {
            BeginSend(Encoding.UTF8.GetBytes("HEART"));
            GC.Collect();
            ActivatePong = true;
        }
    }
}
