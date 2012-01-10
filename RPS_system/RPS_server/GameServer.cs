using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

/**
 * Message Codes:
 * 0 - no connection
 * 1 - connection succesfull
 * 2 - unused
 * 3 - unused
 * 4 - unused
 * 5 - unused
 * 6 - unused
 * 7 - unused
 * 8 - client id follows
 * 9 - player move
 * */
namespace RPS_server
{
    // Holds the arguments for the StatusChanged event
    public class StatusChangedEventArgs : EventArgs
    {
        // The argument we're interested in is a message describing the event
        private string EventMsg;

        // Property for retrieving and setting the event message
        public string EventMessage
        {
            get
            {
                return EventMsg;
            }

            set
            {
                EventMsg = value;
            }
        }

        // Constructor for setting the event message
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }

    //hold individual game data
    [Serializable]
    class Game
    {
        public Game(int gid, int uid, string name)
        {
            gameid = gid;
            player1 = uid;
            strPlayer1 = name;
        }

        public int gameid,//the game id, key in hashtable
            player1,//player1 id (game creater)
            player2;//player2 id

        public string strPlayer1,//usernames
            strPlayer2;
    }

   public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class GameServer
    {
        // hash tables to store users and connections (browsable by user or connection)
        //games table (browsable by game ID)
        public static Hashtable htUsers = new Hashtable(100); // 100 users at one time limit
        public static Hashtable htConnections = new Hashtable(100); // 100 users at one time limit     
        public static Hashtable htGames = new Hashtable(50);// 25 games at a time

        // Will store the IP address passed to it
        private IPAddress ipAddress;
        private TcpClient tcpClient;

        private static ArrayList list = new ArrayList();

        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        private static int count;
        public static int Count { get; set; }

        private static int games;
        public static int Games { get; set; }

        // The constructor sets the IP address to the one retrieved by the instantiating object
        public GameServer(IPAddress address)
        {
            ipAddress = address;
        }

        // The thread that will hold the connection listener
        // The TCP object that listens for connections
        private Thread thrListener, thrList;
        private TcpListener tlsClient;

        bool ServRunning = false;

        // Add the user to the hash tables
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            // First add the username and associated connection to both hash tables
            GameServer.htUsers.Add(strUsername, tcpUser);
            GameServer.htConnections.Add(tcpUser, strUsername);
            
            SendAdminMessage(htConnections[tcpUser] + " has joined us");
        }

        // Remove the user from the hash tables
        public static bool RemoveUser(TcpClient tcpUser)
        {
            // If the user is there
            if (htConnections[tcpUser] != null)
            {
                SendAdminMessage(htConnections[tcpUser] + " has left us");

                GameServer.htUsers.Remove(GameServer.htConnections[tcpUser]);
                GameServer.htConnections.Remove(tcpUser);
                return true;
            }
            else
                return false;
        }
 
        // This is called when we want to raise the StatusChanged even
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                // Invoke the delegate
                statusHandler(null, e);
            }
        }
 
        // Send administrative messages to all clients
        public static void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;

            // First of all, show in our application who says what
            e = new StatusChangedEventArgs("Administrator: " + Message);
            OnStatusChanged(e);
 
            TcpClient[] tcpClients = new TcpClient[GameServer.htUsers.Count];

            GameServer.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(Message);                
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // If there was a problem, the user is not there anymore, remove him
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }
 
        // Send messages from one user to all the others
        //TODO change to send to only one client
        public static void SendMessage(string From, string Message)
        {
            StreamWriter swSenderSender;
 
            // First of all, show in our application who says what
            e = new StatusChangedEventArgs(From + " says: " + Message);
            OnStatusChanged(e);

            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClients = new TcpClient[GameServer.htUsers.Count];

            GameServer.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // If there was a problem, the user is not there anymore, remove him
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public static void SendPrivateMessage(string to, string msg)
        {
            StreamWriter sw;            
            
            try
            {
                sw = new StreamWriter(((TcpClient)GameServer.htUsers[to]).GetStream());
                sw.WriteLine(msg);
                sw.Flush();
            }
            catch // If there was a problem, the user is not there anymore, remove him
            {
                RemoveUser((TcpClient)GameServer.htUsers[to]);
            }            
        }

        public void StartListening()
        {
            // Get the IP of the first network device, however this can prove unreliable on certain configurations
            IPAddress ipaLocal = ipAddress;

            tlsClient = new TcpListener(ipaLocal, 9333);
            tlsClient.Start();
            ServRunning = true;

            thrListener = new Thread(KeepListening);
            thrListener.Start();

            //broadcast gamelist to all clients on a new thread
            thrList = new Thread(SendGameList);
            thrList.Start();
        }

        public void StopListening()
        {
            thrListener.Abort();
            tlsClient.Stop();
            ServRunning = false;
        }

        private void KeepListening()
        {
            // While the server is running
            while (ServRunning == true)
            {
                // Accept a pending connection
                tcpClient = tlsClient.AcceptTcpClient();

                // Create a new instance of Connection
                Connection newConnection = new Connection(tcpClient);
            }
        }

        public static void AddNewGame(string currUser, int uid)
        {
            ++GameServer.Games;
            Game g = new Game(GameServer.Games, uid, currUser);
            GameServer.htGames.Add(GameServer.Games, g);
            list.Add(g);
        }

        public static void JoinGame(string currUser, int uid, int gid)
        {
            ((Game)GameServer.htGames[gid]).strPlayer2 = currUser;
            ((Game)GameServer.htGames[gid]).player2 = uid;

        }
        
        private void SendGameList()
        {
            while (ServRunning == true)
            {
                // 3|recipient id|num items, 0=all|item1 gameid|item1 creater name|item2...
                string msg = "3|0|" + list.Count.ToString()+"|";
                for (int i = 0; i < list.Count; ++i)
                    msg += ((Game)list[i]).gameid.ToString() + "|" + ((Game)list[i]).strPlayer1+"|"+((Game)list[i]).strPlayer2+"|";// bf.Serialize(swSenderSender,list[i]);
                SendAdminMessage(msg);
                Thread.Sleep(3000);
            }
        }
    }
}