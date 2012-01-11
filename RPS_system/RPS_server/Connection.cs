using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

/**
 * Message Codes:
 * 0 - error
 * 1 - success
 * 2 - request game list
 * 3 - game list follows - 3|recipient id|num items|item1 gameid|item1 creater name|item2...
 * 4 - create new game - 4|id| <game id will follow>
 * 5 - join game - 5|id|player id
 * 6 - chat message - 6|gid|message
 * 7 - game id - 7|game id
 * 8 - client id - 8|id
 * 9 - player move 9|game id|player id|move
 * */
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
                    swSender.WriteLine("8|" + GameServer.Count.ToString());
                    swSender.Flush();
                    // Add the user to the hash tables and start listening for messages from him
                    GameServer.AddUser(tcpClient, currUser);
                }
            }
            else
            {
                CloseConnection();             
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
                    else if (strResponse[0] == '2')//request game list
                    {
                        int idNum = Convert.ToInt32(Char.GetNumericValue(strResponse[2]));
                        //GameServer.SendGameList(idNum, currUser);

                    }
                    else if(strResponse[0] == '4')//create a new game 
                    {
                        int uid = Convert.ToInt32(Char.GetNumericValue(strResponse[2]));
                        GameServer.AddNewGame(currUser, uid);
                        swSender.WriteLine("7|" + GameServer.Games.ToString());
                        swSender.Flush();
                    }
                    else if (strResponse[0] == '5')//join game
                    {
                        int gid = Convert.ToInt32(char.GetNumericValue(strResponse[2]));
                        int uid = Convert.ToInt32(char.GetNumericValue(strResponse[4]));
                        GameServer.JoinGame(currUser, uid, gid);
                    }
                    else if(strResponse[0] == '9')//parse and foreward move to correct client
                    {
                        int gid = Convert.ToInt32(char.GetNumericValue(strResponse[2]));
                        int uid = Convert.ToInt32(char.GetNumericValue(strResponse[4]));
                        int move = Convert.ToInt32(char.GetNumericValue(strResponse[6]));
                        string p;
                        if(((Game)GameServer.htGames[gid]).player1 == uid)
                        {
                            p = ((Game)GameServer.htGames[gid]).strPlayer2;                                                        
                        }
                        else
                        {
                            p = ((Game)GameServer.htGames[gid]).strPlayer1;
                        }
                        GameServer.SendPrivateMessage(p, "9|" + move.ToString());
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