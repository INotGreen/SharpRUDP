using RUDP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



public class Program
{       
    public static string Host = "10.212.202.87";
    public static int Port = Convert.ToInt32("8880");   
    public static long Received = 0;
    
    
    
    public static async Task Main()
    {
        ReliableUDPClient rudp = new ReliableUDPClient();
        rudp.ConnectAsync();

        while (true) { Thread.Sleep(1000); }
    }
}
