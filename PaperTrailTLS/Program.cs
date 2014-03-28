using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PaperTrailTLS
{
    class Program
    {
        private const string MachineName = "logs.papertrailapp.com";
        private const string PaperTrailIp = "67.214.212.101";
        private const int Port = 24919;

        private const string TCPMessageToSend = "Heres a TCP message for ya!";
        private const string UDPMessageToSend = "LOL UDP! HAHAH UDP. Seems legit.";

        public static int Main(string[] args)
        {
            RunMenu();
            return 0;
        }

        private static void RunMenu()
        {
            Console.Clear();
            Console.WriteLine("-****************************");
            Console.Write("*  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Welcome to the internet");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  *");
            Console.WriteLine("*  Please choose :          *");
            Console.WriteLine("* 1) Send TLS message       *");
            Console.WriteLine("* 2) Send Clear UDP message *");
            Console.WriteLine("* 3) Send Clear TCP message *");
            Console.WriteLine("*                           *");
            Console.WriteLine("*****************************");

            try
            {
                var choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------");
                        SendTLS();
                        Console.WriteLine("Done. Press a key.");
                        Console.WriteLine("-----------------------------");
                        Console.ReadKey();
                        RunMenu();
                        break;
                    case '2':
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------");
                        SendPlainUDPMessage();
                        Console.WriteLine("Done. Press a key.");
                        Console.WriteLine("-----------------------------");
                        Console.ReadKey();
                        RunMenu();
                        break;
                    case '3':
                        Console.WriteLine();
                        Console.WriteLine("-----------------------------");
                        SendPlainTCPMessage();
                        Console.WriteLine("Done. Press a key.");
                        Console.WriteLine("-----------------------------");
                        Console.ReadKey();
                        RunMenu();
                        break;
                    default:
                        RunMenu();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The internet hates you and has thrown an exception: ");
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine("Press a key.");
                Console.ReadKey();
                RunMenu();
            }
        }

        private static void SendPlainUDPMessage()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
            var serverAddr = IPAddress.Parse(PaperTrailIp);
            var endPoint = new IPEndPoint(serverAddr, Port);

            Console.WriteLine("Sending : " + UDPMessageToSend);
            byte[] send_buffer = Encoding.ASCII.GetBytes(UDPMessageToSend);

            socket.SendTo(send_buffer, endPoint);
            Console.WriteLine("Sent.");
        }

        private static void SendPlainTCPMessage()
        {
            var client = new TcpClient(PaperTrailIp, Port);
            Byte[] message = Encoding.ASCII.GetBytes(TCPMessageToSend);
            Console.WriteLine("Sending : " + TCPMessageToSend);

            NetworkStream stream = client.GetStream();
            stream.Write(message, 0, message.Length);
            stream.Flush();
            stream.Close();
            client.Close();
        }

        public static void SendTLS()
        {
            // serverCertificateName name must match the name on the server's certificate. 
            string serverCertificateName = MachineName;
            SslTcpClient.RunClient(MachineName, Port, serverCertificateName);
        }
    }
}
