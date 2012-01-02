﻿using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RockPaperScissorsGUI
{
    public partial class ClientForm : Form
    {
        private Random rand;
        private int cMove, pMove;

        // Will hold the user name
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
   
        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);

        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);

        private delegate void UpdateCMove(int move);

        private Thread thrMessaging, thrMove;
        private IPAddress ipAddr;
        private bool Connected;
        private int idNum;
        private string opponent;
        private enum choices {
            Rock = 1,
            Paper,
            Scissor
        }

        public ClientForm()
        {
            cMove = 0;
            InitializeComponent();
            rand = new Random();//create the RNG instance
            Application.ApplicationExit +=new EventHandler(OnApplicationExit);
           // InitializeConnection();
        }

        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Closes the connections, streams, etc.
                Connected = false;
                srReceiver.Close();
                srReceiver.Dispose();
                thrMessaging.Abort();
                swSender.Close();
                swSender.Dispose();
                tcpServer.Close();
            }
        }

        //player choice button click
        private void choice_Click(object sender, EventArgs e)
        {
            //pMove = 1;
            pMove = (int)Char.GetNumericValue(((Button)sender).Name.ToString()[6]);
            findWinner();
        }
        
        /**
         * Send and recieve moves and perform logic
         * */
        private void findWinner(){
            if (Connected)
            {
                SendMove();
                opponent = "Player2";
                //wait until move recieved, this will lock thread!
                //TODO: add time limit to recieve ie: wait 30 secs then send random move
                while (cMove == 0)
                    cMove = ReceiveMove();
            }
            else
            {
                opponent = "Computer";
                cMove = rand.Next(1, 3);
            }
           
            if (cMove != 0)
            {
                //game logic
                if (cMove == pMove)
                    txtResult.Text = (choices)cMove + " vs. " + (choices)pMove + " \r\nTie Game!!!";

                else if (((cMove > pMove) && ((cMove - pMove) < 2))
                    || ((cMove < pMove) && (pMove - cMove) > 1))
                {
                    txtResult.Text = (choices)cMove + " beats " + (choices)pMove + " \r\n"+opponent+" Wins!!!";
                }
                else
                    txtResult.Text = (choices)pMove + " beats " + (choices)cMove + " \r\nYou Win!!!";
                cMove = 0;
            }
        }

        private void InitializeConnection()
        {
            //TcpListener listener = new TcpListener(IPAddress.Any, 9333);
            //listener.Start();
            //TcpClient client = listener.AcceptTcpClient();

            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(txtIP.Text);//"127.0.0.1");

            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            try
            {
                tcpServer.Connect(ipAddr, 9333);
            }
            catch(Exception e)
            {
                MessageBox.Show("exception: "+e.ToString());
            }

            // Helps us track whether we're connected or not
            Connected = true;

            UserName = txtUser.Text.ToString();

            // Send the desired username to the server
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(UserName);
            swSender.Flush();

            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
            //Thread.Sleep(10000);
            //thrMessaging.Suspend();

            // Start the thread for receiving other player move
            //thrMove = new Thread(new ThreadStart(ReceiveMove));
            //thrMove.Start();
        }

        //thread version
        //private void ReceiveMove()
        //{
        //    srReceiver = new StreamReader(tcpServer.GetStream());
        //    string s = srReceiver.ReadLine();
        //    if (s[0] == '9' && s[2] != Char.Parse(idNum.ToString()))
        //    {
        //        cMove =  (int)Char.GetNumericValue(s[4]);
        //    }
        //}

        //ONLY HANDLES SINGLE DIGIT ID
        private int ReceiveMove()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string s = srReceiver.ReadLine();
            if (s[0] == '9' && s[2] != Char.Parse(idNum.ToString()))
            {
                return (int)Char.GetNumericValue(s[4]);
            }
            return 0;
        }

        private void SendMove()
        {
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine("9|" + idNum.ToString() + "|" + pMove.ToString());
            swSender.Flush();
        }

        private void ReceiveMessages()
        {
            // Receive the response from the server
            srReceiver = new StreamReader(tcpServer.GetStream());

            // If the first character of the response is 1, connection was successful
            string ConResponse = srReceiver.ReadLine();

            // If the first character is a 1, connection was successful
            if (ConResponse[0] == '1')
            {           
                // Update the form to tell it we are now connected
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });            
            }
            else // If the first character is not a 1 (probably a 0), the connection was unsuccessful
            {
                string Reason = "Not Connected: ";

                // Extract the reason out of the response message. The reason starts at the 3rd character
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);

                // Update the form with the reason why we couldn't connect
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });

                // Exit the method
                return;
            }
            //While we are successfully connected, read incoming lines from the server
            while (Connected)
            {
                // Show the messages in the log TextBox
                string s = srReceiver.ReadLine();
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { s });
                
                if(s[0] == '8')
                    idNum = (int)Char.GetNumericValue(s[2]);

                if (s[0] == '9' && s[2] != Char.Parse(idNum.ToString()))
                {
                    int move = (int)Char.GetNumericValue(s[4]);
                    this.Invoke(new UpdateCMove(this.SetCMove), new object[] { move }); 
                }
            }
        }

        private void SetCMove(int move)
        {
            cMove = move;
        }

        private void UpdateLog(string strMessage)
        {
            // Append text also scrolls the TextBox to the bottom each time
            //dont show ID's or moves
            if(strMessage[0] != '8' && strMessage[0] != '9')
                txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // Show the reason why the connection is ending
            txtLog.AppendText(Reason + "\r\n");

            // Close the objects
            Connected = false;
            srReceiver.Close();
            srReceiver.Dispose();
            thrMessaging.Abort();
            swSender.Close();
            swSender.Dispose();
            tcpServer.Close();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                if (txtUser.Text.ToString() != "" && txtIP.Text.ToString() != "")
                {
                    InitializeConnection();
                    if(Connected)
                        btnConnect.Text = "Disconnect";
                }
                else
                    MessageBox.Show("Enter a valid username");
            }
            else
            {
                CloseConnection("User request");
                btnConnect.Text = "Connect";
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(textBox1.Text);
            swSender.Flush();
            textBox1.Text = "";
        }
    }
}