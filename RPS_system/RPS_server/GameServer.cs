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

    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class GameServer
    {
        // This hash table stores users and connections (browsable by user)
        public static Hashtable htUsers = new Hashtable(100); // 100 users at one time limit

        // This hash table stores connections and users (browsable by connection)
        public static Hashtable htConnections = new Hashtable(100); // 100 users at one time limit

        //
        public static Hashtable htRooms = new Hashtable(50);// 25 games at a time

        // Will store the IP address passed to it
        private IPAddress ipAddress;
        private TcpClient tcpClient;

        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;
        private static int count;

        public static int Count { get; set; }

        // The constructor sets the IP address to the one retrieved by the instantiating object
        public GameServer(IPAddress address)
        {
            ipAddress = address;
        }

        // The thread that will hold the connection listener
        private Thread thrListener;

        // The TCP object that listens for connections
        private TcpListener tlsClient;

        // Will tell the while loop to keep monitoring for connections
        bool ServRunning = false;

        // Add the user to the hash tables
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            // First add the username and associated connection to both hash tables
            GameServer.htUsers.Add(strUsername, tcpUser);
            GameServer.htConnections.Add(tcpUser, strUsername);
            
            // Tell of the new connection to all other users and to the server form
            SendAdminMessage(htConnections[tcpUser] + " has joined us");
        }

        // Remove the user from the hash tables
        public static bool RemoveUser(TcpClient tcpUser)
        {
            // If the user is there
            if (htConnections[tcpUser] != null)
            {
                // First show the information and tell the other users about the disconnection
                SendAdminMessage(htConnections[tcpUser] + " has left us");

                // Remove the user from the hash table
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
 
            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClients = new TcpClient[GameServer.htUsers.Count];

            // Copy the TcpClient objects into the array
            GameServer.htUsers.Values.CopyTo(tcpClients, 0);

            // Loop through the list of TCP clients
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
                try
                {
                    // If the message is blank or the connection is null, break out
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Send the message to the current user in the loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);                
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
            //System.Console.WriteLine(From + " says: " + Message);
            e = new StatusChangedEventArgs(From + " says: " + Message);
            OnStatusChanged(e);

            // Create an array of TCP clients, the size of the number of users we have
            TcpClient[] tcpClients = new TcpClient[GameServer.htUsers.Count];

            // Copy the TcpClient objects into the array
            GameServer.htUsers.Values.CopyTo(tcpClients, 0);

            // Loop through the list of TCP clients
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Try sending a message to each
                try
                {
                    // If the message is blank or the connection is null, break out
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Send the message to the current user in the loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    //swSenderSender.WriteLine(From + " says: " + Message);
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

        public void StartListening()
        {
            // Get the IP of the first network device, however this can prove unreliable on certain configurations
            IPAddress ipaLocal = ipAddress;

            // Create the TCP listener object using the IP of the server and the specified port
            tlsClient = new TcpListener(ipaLocal, 9333);

            // Start the TCP listener and listen for connections
            tlsClient.Start();

            // The while loop will check for true in this before checking for connections
            ServRunning = true;

            // Start the new tread that hosts the listener
            thrListener = new Thread(KeepListening);
            thrListener.Start();
            
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
    }
}