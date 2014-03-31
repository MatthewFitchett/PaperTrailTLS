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

        private SslStream sslStream = null;
        private TcpClient client = null;

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
            // Encode a test message into a byte array. 
            var msg = RenderLoggingEvent(loggingEvent);
            byte[] message = Encoding.UTF8.GetBytes(msg);

            // Send message to the server. 
            sslStream.Write(message);
            sslStream.Flush();
        }
        
        private static object syncRoot = new object();
        protected override bool PreAppendCheck()
        {
            // May need dbl check locking here..
            if (sslStream == null || client == null)
            {
                lock (syncRoot)
                {
                    if (sslStream == null || client == null)
                    {
                        client = new TcpClient(DestinationHostName, DestinationPort);

                        // Create an SSL stream that will close the client's stream.
                        sslStream = new SslStream(
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
                        }
                    }
                }
            }
            return base.PreAppendCheck();
        }

        protected override void OnClose()
        {

            if (sslStream != null)
                sslStream.Close();
            if (client != null)
                client.Close();

            base.OnClose();
        }
    }
}
