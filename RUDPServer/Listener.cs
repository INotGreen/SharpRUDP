using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RUDPServer
{
    internal class Listener
    {
        private Socket listener { get; set; }
        public void Start(int port)
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                Console.WriteLine($"Start Port:{port}");

                new ReliableUDPServer(listener);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }
        }

       
    }
}
