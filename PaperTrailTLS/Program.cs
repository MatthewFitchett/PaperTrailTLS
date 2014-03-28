namespace PaperTrailTLS
{
    class Program
    {
        public static int Main(string[] args)
        {
            string serverCertificateName = null;
            string machineName = "logs.papertrailapp.com";
            int port = 24919;

            // serverCertificateName name must match the name on the server's certificate. 
            serverCertificateName = machineName;

            SslTcpClient.RunClient(machineName,port, serverCertificateName);
            return 0;
        }
    }
}
