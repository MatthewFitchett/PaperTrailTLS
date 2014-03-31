using System;
using System.Collections;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PaperTrailTLS
{
    public class SslTcpClient
    {
        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate. 
        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }

        public static void RunClient(string machineName, int port, string serverName)
        {
            // Create a TCP/IP client socket. 
            // machineName is the host running the server application.
            var client = new TcpClient(machineName, port);
            Console.WriteLine("Client connected to {0} on port {1}.",machineName, port);

            // Create an SSL stream that will close the client's stream.
            var sslStream = new SslStream(
                client.GetStream(),
                false,
                ValidateServerCertificate,
                null
                );
            // The server name must match the name on the server certificate. 
            try
            {
                sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                Console.ReadKey();
                return;
            }
            
            // Encode a test message into a byte array. 
            // for message format please see : http://help.papertrailapp.com/discussions/questions/7580-fao-leon-c-log4net-tls
            var msg = "Hello from the client\r\n";
            byte[] messsage = Encoding.UTF8.GetBytes(msg);

            // Send hello message to the server. 
            sslStream.Write(messsage);
            Console.WriteLine("Sending : " + msg);
            
            sslStream.Flush();
            sslStream.Close();
            client.Close();
            
            Console.WriteLine("Client closed.");
        }
    }
}