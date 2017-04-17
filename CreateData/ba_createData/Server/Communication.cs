using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ba_createData.Server
{
    public static class Communication
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <returns></returns>
        public static string GetMessageFromClient(SslStream clientStream)
        {
            if (clientStream == null) throw new ArgumentNullException(nameof(clientStream));
            var messageData = new StringBuilder();
            var message = new byte[4096];
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void SendMessageToClient(SslStream clientStream, string message)
        {
            var EOF = "\r\n";
            var bytesToSend = Encoding.ASCII.GetBytes(message + EOF);
            clientStream.Write(bytesToSend);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void SendFileToClient(SslStream clientStream, FileInfo file)
        {
            var EOF = "\r\n";
            //var bytesToSend = Encoding.ASCII.GetBytes(message + EOF);
            //FileStream inputStream = File.OpenRead(filePath);
            //FileInfo f = new FileInfo(file);
            var size = unchecked((int)file.Length);
            byte[] byteSize = Encoding.ASCII.GetBytes(size.ToString());
            clientStream.Write(byteSize);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="client"></param>
        public static void ClientTalk(SslStream clientStream, TcpClient client)
        {
            while (client.Connected)
            {
                try
                {
                    //blocks until a client sends a message
                    var result = GetMessageFromClient(clientStream);
                    switch (result)
                    {
                        case "LOOK UP SINGLE HASH":
                            // Send go ahead
                            SendMessageToClient(clientStream, "200 OK");
                            var keeplookup = true;
                            while (keeplookup)
                            {
                                var message = GetMessageFromClient(clientStream);
                                if (!message.Equals("DONE"))
                                {
                                    // Ensure that hash is proper length and have a termination symbol appended
                                    if (message.Length.Equals(Properties.Settings.Default.EncryptionLength + 1) &&
                                        message.Last().Equals('$'))
                                    {
                                        // create a new process so the client can send a new request
                                        try
                                        {
                                            // 300 - true, 301 - false
                                            SendMessageToClient(clientStream,
                                                Scanner.Scanner.ScanFile(message) ? "300" : "301");
                                        }
                                        catch (Exception ex)
                                        {
                                            // Scanning failed with ex.message
                                            SendMessageToClient(clientStream, "901 FAIL");
                                        }
                                    }
                                    else
                                    {
                                        // 902 Uncorrect format
                                        SendMessageToClient(clientStream, "902 FAIL");
                                    }
                                }
                                else
                                {
                                    if (GetMessageFromClient(clientStream).Equals("DONE"))
                                    {
                                        keeplookup = false;
                                    }
                                }
                            }
                            break;
                        case "AM I ACTIVE":
                            if (MainServerControl.ActiveTcpClients.Contains(client))
                            {
                                SendMessageToClient(clientStream, "200 OK");
                            }
                            break;
                        case "LOOK UP STARTUPFILES":
                            break;
                        case "IS CLIENT ALIVE":
                            // check Connection
                            break;
                        default:
                            SendMessageToClient(clientStream, "900 FAIL");
                            break;
                    }
                }
                catch
                {
                    // A socket error has occurred, remove client and log the error
                    MainServerControl.ActiveTcpClients.Remove(client);
                    break;
                }
            }

            // We remove client from active connections
            MainServerControl.ActiveTcpClients.Remove(client);
        }
    }
}
