using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUDPServer
{
    internal class Program
    {
        public static long Received = 0;
        public static  void Main(string[] args)
        {
            var listener = new Listener();
            listener.Start(8880);
            while (true) { Thread.Sleep(1000); }
        }
    }
}
