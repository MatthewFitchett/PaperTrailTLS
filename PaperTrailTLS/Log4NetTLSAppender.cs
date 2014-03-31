using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net.Appender;
using log4net.Core;

namespace PaperTrailTLS
{
    public class Log4NetTLSAppender : AppenderSkeleton
    {
        public string DestinationIpAddress { get; set; }
        public int DestinationPort { get; set; }
        public string DestinationHostName { get; set; }

        // The following method is invoked by the RemoteCertificateValidationDelegate in the Append() method. 
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

        protected override void Append(LoggingEvent loggingEvent)
        {
            // Create a TCP/IP client socket. 
            var client = new TcpClient(DestinationHostName, DestinationPort);

            // Create an SSL stream that will close the client's stream.
            var sslStream = new SslStream(
                client.GetStream(),
                false,
                ValidateServerCertificate,
                null);

            // The server name must match the name on the server certificate. 
            try
            {
                sslStream.AuthenticateAsClient(DestinationHostName);
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
                return;
            }

            // Encode a test message into a byte array. 
            var msg = RenderLoggingEvent(loggingEvent);
            byte[] messsage = Encoding.UTF8.GetBytes(msg);

            // Send message to the server. 
            sslStream.Write(messsage);

            sslStream.Flush();
            sslStream.Close();
            client.Close();
        }
    }
}
