using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace RPS_server
{
    // This class handels connections; there will be as many instances of it as there will be connected users
    class Connection
    {
        TcpClient tcpClient;

        // The thread that will send information to the client
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string currUser;
        private string strResponse;

        // The constructor of the class takes in a TCP connection
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;

            // The thread that accepts the client and awaits messages
            thrSender = new Thread(AcceptClient);

            // The thread calls the AcceptClient() method
            thrSender.Start();
        }

        private void CloseConnection()
        {
            thrSender.Abort();
            // Close the currently open objects
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        // Occures when a new client is accepted
        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            // Read the account information from the client
            currUser = srReceiver.ReadLine();           

            // We got a response from the client
            if (currUser != "")
            {
                // Store the user name in the hash table
                if (GameServer.htUsers.Contains(currUser) == true)
                {
                    // 0 means not connected
                    swSender.WriteLine("0|This username already exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }

                else if (currUser == "Administrator")
                {
                    // 0 means not connected
                    swSender.WriteLine("0|This username is reserved.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }

                else
                {
                    ++GameServer.Count;
                    // 1 means connected successfully
                    swSender.WriteLine("1");
                    swSender.Flush();

                    //send (VERY BASIC) id number
                    swSender.WriteLine("8" + "|" + GameServer.Count.ToString());
                    swSender.Flush();
                    // Add the user to the hash tables and start listening for messages from him
                    GameServer.AddUser(tcpClient, currUser);
                }
            }
            else
            {
                CloseConnection();
                return;
            }
            
            try
            {
                bool flag = true;
                // Keep waiting for a message from the user
                while (flag && (strResponse = srReceiver.ReadLine()) != "")
                {
                    // If it's invalid, remove the user
                    if (strResponse == null)
                    {
                        flag = GameServer.RemoveUser(tcpClient);
                    }
                    else
                    {
                        // Otherwise send the message to all the other users
                        GameServer.SendMessage(currUser, strResponse);
                    }
                }
            }

            catch
            {
                // If anything went wrong with this user, disconnect him
                GameServer.RemoveUser(tcpClient);
            }
        }
    }
}
