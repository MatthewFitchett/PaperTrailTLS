﻿using System;
using System.Collections;
using System.IO;
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
            Console.WriteLine("Client connected.");

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
                sslStream.AuthenticateAsClient(serverName, X509Certs(),SslProtocols.Tls,true);
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
            var msg = "Hello from the client";
            byte[] messsage = Encoding.UTF8.GetBytes(msg);

            // Send hello message to the server. 
            sslStream.Write(messsage);
            Console.WriteLine("Sending : " + msg);
            
            sslStream.Flush();
            sslStream.Close();
            client.Close();
            
            Console.WriteLine("Client closed.");
            Console.ReadKey();
        }




        private static X509Certificate2Collection X509Certs()
        {
            var paperTrailCertString = @"MIID1TCCAr2gAwIBAgIDAjbRMA0GCSqGSIb3DQEBBQUAMEIxCzAJBgNVBAYTAlVT
MRYwFAYDVQQKEw1HZW9UcnVzdCBJbmMuMRswGQYDVQQDExJHZW9UcnVzdCBHbG9i
YWwgQ0EwHhcNMTAwMjE5MjI0NTA1WhcNMjAwMjE4MjI0NTA1WjA8MQswCQYDVQQG
EwJVUzEXMBUGA1UEChMOR2VvVHJ1c3QsIEluYy4xFDASBgNVBAMTC1JhcGlkU1NM
IENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAx3H4Vsce2cy1rfa0
l6P7oeYLUF9QqjraD/w9KSRDxhApwfxVQHLuverfn7ZB9EhLyG7+T1cSi1v6kt1e
6K3z8Buxe037z/3R5fjj3Of1c3/fAUnPjFbBvTfjW761T4uL8NpPx+PdVUdp3/Jb
ewdPPeWsIcHIHXro5/YPoar1b96oZU8QiZwD84l6pV4BcjPtqelaHnnzh8jfyMX8
N8iamte4dsywPuf95lTq319SQXhZV63xEtZ/vNWfcNMFbPqjfWdY3SZiHTGSDHl5
HI7PynvBZq+odEj7joLCniyZXHstXZu8W1eefDp6E63yoxhbK1kPzVw662gzxigd
gtFQiwIDAQABo4HZMIHWMA4GA1UdDwEB/wQEAwIBBjAdBgNVHQ4EFgQUa2k9ahhC
St2PAmU5/TUkhniRFjAwHwYDVR0jBBgwFoAUwHqYaI2J+6sFZAwRfap9ZbjKzE4w
EgYDVR0TAQH/BAgwBgEB/wIBADA6BgNVHR8EMzAxMC+gLaArhilodHRwOi8vY3Js
Lmdlb3RydXN0LmNvbS9jcmxzL2d0Z2xvYmFsLmNybDA0BggrBgEFBQcBAQQoMCYw
JAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmdlb3RydXN0LmNvbTANBgkqhkiG9w0B
AQUFAAOCAQEAq7y8Cl0YlOPBscOoTFXWvrSY8e48HM3P8yQkXJYDJ1j8Nq6iL4/x
/torAsMzvcjdSCIrYA+lAxD9d/jQ7ZZnT/3qRyBwVNypDFV+4ZYlitm12ldKvo2O
SUNjpWxOJ4cl61tt/qJ/OCjgNqutOaWlYsS3XFgsql0BYKZiZ6PAx2Ij9OdsRu61
04BqIhPSLT90T+qvjF+0OJzbrs6vhB6m9jRRWXnT43XcvNfzc9+S7NIgWW+c+5X4
knYYCnwPLKbK3opie9jzzl9ovY8+wXS7FXI6FoOpC+ZNmZzYV+yoAVHHb1c0XqtK
LEL2TxyJeN4mTvVvk0wVaydWTQBUbHq3tw==";

            var geoTrustCertString = @"MIIDVDCCAjygAwIBAgIDAjRWMA0GCSqGSIb3DQEBBQUAMEIxCzAJBgNVBAYTAlVT
MRYwFAYDVQQKEw1HZW9UcnVzdCBJbmMuMRswGQYDVQQDExJHZW9UcnVzdCBHbG9i
YWwgQ0EwHhcNMDIwNTIxMDQwMDAwWhcNMjIwNTIxMDQwMDAwWjBCMQswCQYDVQQG
EwJVUzEWMBQGA1UEChMNR2VvVHJ1c3QgSW5jLjEbMBkGA1UEAxMSR2VvVHJ1c3Qg
R2xvYmFsIENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2swYYzD9
9BcjGlZ+W988bDjkcbd4kdS8odhM+KhDtgPpTSEHCIjaWC9mOSm9BXiLnTjoBbdq
fnGk5sRgprDvgOSJKA+eJdbtg/OtppHHmMlCGDUUna2YRpIuT8rxh0PBFpVXLVDv
iS2Aelet8u5fa9IAjbkU+BQVNdnARqN7csiRv8lVK83Qlz6cJmTM386DGXHKTubU
1XupGc1V3sjs0l44U+VcT4wt/lAjNvxm5suOpDkZALeVAjmRCw7+OC7RHQWa9k0+
bw8HHa8sHo9gOeL6NlMTOdReJivbPagUvTLrGAMoUgRx5aszPeE4uwc2hGKceeoW
MPRfwCvocWvk+QIDAQABo1MwUTAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBTA
ephojYn7qwVkDBF9qn1luMrMTjAfBgNVHSMEGDAWgBTAephojYn7qwVkDBF9qn1l
uMrMTjANBgkqhkiG9w0BAQUFAAOCAQEANeMpauUvXVSOKVCUn5kaFOSPeCpilKIn
Z57QzxpeR+nBsqTP3UEaBU6bS+5Kb1VSsyShNwrrZHYqLizz/Tt1kL/6cdjHPTfS
tQWVYrmm3ok9Nns4d0iXrKYgjy6myQzCsplFAMfOEVEiIuCl6rYVSAlk6l5PdPcF
PseKUgzbFbS9bZvlxrFUaKnjaZC2mqUPuLk/IH2uSrW4nOQdtqvmlKXBx4Ot2/Un
hw4EbNX/3aBd7YdStysVAq45pmp06drE57xNNB6pXE0zX5IJL4hmXXeXxx12E6nV
5fEWCRE11azbJHFwLJhWC9kXtNHjUStedejV0NxPNO3CBWaAocvmMw==";

            var equifaxCertString = @"MIIDIDCCAomgAwIBAgIENd70zzANBgkqhkiG9w0BAQUFADBOMQswCQYDVQQGEwJV
UzEQMA4GA1UEChMHRXF1aWZheDEtMCsGA1UECxMkRXF1aWZheCBTZWN1cmUgQ2Vy
dGlmaWNhdGUgQXV0aG9yaXR5MB4XDTk4MDgyMjE2NDE1MVoXDTE4MDgyMjE2NDE1
MVowTjELMAkGA1UEBhMCVVMxEDAOBgNVBAoTB0VxdWlmYXgxLTArBgNVBAsTJEVx
dWlmYXggU2VjdXJlIENlcnRpZmljYXRlIEF1dGhvcml0eTCBnzANBgkqhkiG9w0B
AQEFAAOBjQAwgYkCgYEAwV2xWGcIYu6gmi0fCG2RFGiYCh7+2gRvE4RiIcPRfM6f
BeC4AfBONOziipUEZKzxa1NfBbPLZ4C/QgKO/t0BCezhABRP/PvwDN1Dulsr4R+A
cJkVV5MW8Q+XarfCaCMczE1ZMKxRHjuvK9buY0V7xdlfUNLjUA86iOe/FP3gx7kC
AwEAAaOCAQkwggEFMHAGA1UdHwRpMGcwZaBjoGGkXzBdMQswCQYDVQQGEwJVUzEQ
MA4GA1UEChMHRXF1aWZheDEtMCsGA1UECxMkRXF1aWZheCBTZWN1cmUgQ2VydGlm
aWNhdGUgQXV0aG9yaXR5MQ0wCwYDVQQDEwRDUkwxMBoGA1UdEAQTMBGBDzIwMTgw
ODIyMTY0MTUxWjALBgNVHQ8EBAMCAQYwHwYDVR0jBBgwFoAUSOZo+SvSspXXR9gj
IBBPM5iQn9QwHQYDVR0OBBYEFEjmaPkr0rKV10fYIyAQTzOYkJ/UMAwGA1UdEwQF
MAMBAf8wGgYJKoZIhvZ9B0EABA0wCxsFVjMuMGMDAgbAMA0GCSqGSIb3DQEBBQUA
A4GBAFjOKer89961zgK5F7WF0bnj4JXMJTENAKaSbn+2kmOeUJXRmm/kEd5jhW6Y
7qj/WsjTVbJmcVfewCHrPSqnI0kBBIZCe/zuf6IWUrVnZ9NA2zsmWLIodz2uFHdh
1voqZiegDfqnc1zqcPGUIWVEX/r87yloqaKHee9570+sB3c4";

            var result = new X509Certificate2Collection
            {
                GetCert(paperTrailCertString),
                GetCert(geoTrustCertString),
                GetCert(equifaxCertString)
            };

            return result;
        }

        private static X509Certificate2 GetCert(string certString)
        {
            byte[] numArray = Convert.FromBase64String(certString);
            string tempFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                File.WriteAllBytes(tempFileName, numArray);
                var certificate = new X509Certificate2(tempFileName, default(string), X509KeyStorageFlags.Exportable);
                return certificate;
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
}