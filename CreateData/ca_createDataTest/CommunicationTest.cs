using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ba_createData;
using ba_createData.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ca_createDataTest
{

    [TestClass]
    public class CommunicationTest
    {

        private SslStream _loggedInCredentials;
        private const string IpAddress = "192.168.1.247";

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) return true;
            Console.WriteLine(@"Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers. 
            return true;
        }




        /// <summary>
        /// Gets the message from server
        /// </summary>
        /// <param name="clientStream"></param>
        /// <returns></returns>
        private static string GetMessageFromServer(SslStream clientStream)
        {
            if (clientStream == null) throw new ArgumentNullException(nameof(clientStream));

            // Make a timeout for 5 second
            clientStream.ReadTimeout = 5000;
            try
            {
                var messageData = new StringBuilder();
                var message = new byte[512];
                int bytes;
                do
                {
                    bytes = clientStream.Read(message, 0, message.Length);
                    var decoder = Encoding.ASCII.GetDecoder();
                    var chars = new char[decoder.GetCharCount(message, 0, bytes)];
                    decoder.GetChars(message, 0, bytes, chars, 0);
                    messageData.Append(chars);
                    // Check for end of file.
                    if (messageData.ToString().IndexOf("\r\n", StringComparison.Ordinal) != -1)
                    {
                        break;
                    }
                } while (bytes != -1);
                return messageData.ToString().Replace("\r\n", string.Empty);
            }
            catch (TimeoutException)
            {
                // A timeout has happened
                return "908 FAIL";
            }
        }


        /// <summary>
        /// Send message to server
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static void SendMessageToServer(SslStream clientStream, string message)
        {
            var EOF = "\r\n";
            var bytesToSend = Encoding.ASCII.GetBytes(message + EOF);
            clientStream.Write(bytesToSend);
        }


        public bool LoginTheClient()
        {
            // Tell program to use test environment
            ba_createData.Properties.Settings.Default.Test = true;

            // Databse substring have length 8
            ba_createData.Properties.Settings.Default.EncryptionLength = 8;

            // Start server - server side
            var serverControl = new MainServerControl();

            // ADD REMOTE ACCESS
            //ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            var clientConnection = new TcpClient();
            clientConnection.Connect(IpAddress, 3000);

            var clientStream = new SslStream(clientConnection.GetStream(), false, ValidateServerCertificate, null);
            var serverCertificate =
                X509Certificate.CreateFromCertFile(Thread.GetDomain().BaseDirectory + "\\rolandio.cer");
            var certificates = new X509CertificateCollection(new[] { serverCertificate });
            clientStream.AuthenticateAsClient(IpAddress, certificates, SslProtocols.Default, false);


            // Login procedure
            var bytesToSend = Encoding.ASCII.GetBytes("LOGIN\r\n");
            clientStream.Write(bytesToSend, 0, bytesToSend.Length);


            // Get Response
            var loginResponse = GetMessageFromServer(clientStream);
            if (loginResponse.Trim() == "200 OK")
            {
                SendMessageToServer(clientStream, "CLIENT_1027\r\n");

                // Get Response
                var userMessage = GetMessageFromServer(clientStream);
                if (userMessage.Equals("PASSWORD"))
                {
                    SendMessageToServer(clientStream, "12345678\r\n");

                    // Get Response
                    var loginIsAcceptedResponse = GetMessageFromServer(clientStream);
                    if (loginIsAcceptedResponse.Equals("LOGIN 200 OK"))
                    {
                        _loggedInCredentials = clientStream;
                        // Client is logged in
                    }
                }
            }


            // We now ask the server if we are logged in and ready to communicate
            SendMessageToServer(clientStream, "AM I ACTIVE\r\n");
            return (GetMessageFromServer(clientStream).Equals("200 OK"));

        }

        [TestMethod]
        public void LoginToServer()
        {

            var success = LoginTheClient();
            Assert.AreEqual(success, true);
        }


        [TestMethod]
        public void AsIfLoggedInWhenNot()
        {

            // Tell program to use test environment
            ba_createData.Properties.Settings.Default.Test = true;

            // Start server - server side
            var serverControl = new MainServerControl();

            // ADD REMOTE ACCESS
            //ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            var clientConnection = new TcpClient();
            clientConnection.Connect(IpAddress, 3000);

            var clientStream = new SslStream(clientConnection.GetStream(), false, ValidateServerCertificate, null);
            var serverCertificate =
                X509Certificate.CreateFromCertFile(Thread.GetDomain().BaseDirectory + "\\rolandio.cer");
            var certificates = new X509CertificateCollection(new[] { serverCertificate });
            clientStream.AuthenticateAsClient(IpAddress, certificates, SslProtocols.Default, false);


            // We now ask the server if we are logged in and ready to communicate
            SendMessageToServer(clientStream, "AM I ACTIVE\r\n");
            Assert.AreEqual(GetMessageFromServer(clientStream), "801");
        }



        [TestMethod]
        public void TestSingleHashHddOnly()
        {
            _loggedInCredentials = null;

            // ACTIVE HDD ONLY
            ba_createData.Properties.Settings.Default.UseHDDOnly = true;
            ba_createData.Properties.Settings.Default.UseCashing = false;

            // TEST OPTION IS SET WHEN CLIENT LOG IN, SEE LoginTheClient()
            if (!LoginTheClient()) return;
            SendMessageToServer(_loggedInCredentials, "LOOK UP SINGLE HASH\r\n");
            if (GetMessageFromServer(_loggedInCredentials).Equals("200 OK"))
            {
                SendMessageToServer(_loggedInCredentials, "skalskor$");
                var success1 = GetMessageFromServer(_loggedInCredentials).Equals("300");
                SendMessageToServer(_loggedInCredentials, "bandidos$");
                var success2 = GetMessageFromServer(_loggedInCredentials).Equals("300");
                SendMessageToServer(_loggedInCredentials, "abrahams$");
                var success3 = GetMessageFromServer(_loggedInCredentials).Equals("300");
                SendMessageToServer(_loggedInCredentials, "ab");
                var success4 = GetMessageFromServer(_loggedInCredentials).Equals("902 FAIL");
                SendMessageToServer(_loggedInCredentials, "abcdefgw$");
                var success5 = GetMessageFromServer(_loggedInCredentials).Equals("301");
                SendMessageToServer(_loggedInCredentials, "");
                var success6 = GetMessageFromServer(_loggedInCredentials).Equals("902 FAIL");
                SendMessageToServer(_loggedInCredentials, "DONE");

                var completeSuccess = success1 && success2 && success3 && success4 && success5 && success6;
                Assert.AreEqual(completeSuccess, true);
            }
        }


        [TestMethod]
        public void TestSingleHashHddOnlyWithCashing()
        {
            // ACTIVE HDD ONLY
            ba_createData.Properties.Settings.Default.UseHDDOnly = true;

            // ACTIVE CACHING
            ba_createData.Properties.Settings.Default.UseCashing = true;

            // TEST OPTION IS SET WHEN CLIENT LOG IN, SEE LoginTheClient()
            if (LoginTheClient())
            {

            }
        }


        [TestMethod]
        public void TestSingleHashRamOnly()
        {

            // Tell program to use test environment
            ba_createData.Properties.Settings.Default.Test = true;

            // Turn off caching
            ba_createData.Properties.Settings.Default.UseCashing = false;

            // Turn on RAM only mode
            ba_createData.Properties.Settings.Default.UseRAMOnly = true;

            // Setup memory database

            // Start server - server side
            var serverControl = new MainServerControl();



            // ADD REMOTE ACCESS
            //ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            var clientConnection = new TcpClient();
            clientConnection.Connect(IpAddress, 3000);

            var clientStream = new SslStream(clientConnection.GetStream(), false, ValidateServerCertificate, null);
            var serverCertificate =
                X509Certificate.CreateFromCertFile(Thread.GetDomain().BaseDirectory + "\\rolandio.cer");
            var certificates = new X509CertificateCollection(new[] { serverCertificate });
            clientStream.AuthenticateAsClient(IpAddress, certificates, SslProtocols.Default, false);


            // We now ask the server if we are logged in and ready to communicate
            SendMessageToServer(clientStream, "AM I ACTIVE\r\n");

            // Load values into memory, this is suitable for servers with loads of RAM
            ba_createData.Scanner.MemoryDatabase.SuffixArray = Database.GetSuffixArrayTableByPattern('a');
            ba_createData.Scanner.MemoryDatabase.LcpArray = Database.GetAllLcpTableContent();
            ba_createData.Scanner.MemoryDatabase.TextFile = Database.GetAllTextFromDatabase();

            //Assert.AreEqual(GetMessageFromServer(clientStream), "801");
        }
    }
}
