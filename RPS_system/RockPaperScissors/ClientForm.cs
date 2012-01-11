using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RockPaperScissorsGUI
{
    public partial class ClientForm : Form
    {
        private static ArrayList list = new ArrayList();
        public static ArrayList List{ get; set; }

        private Random rand;
        private int cMove, pMove;

        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
   
        //delegates needed for form variable updates
        private delegate void UpdateLogCallback(string strMessage);
        private delegate void CloseConnectionCallback(string strReason);
        private delegate void UpdateListBoxCallback(string msg);
        private delegate void ClearListBoxCallback();
        private delegate void UpdateOpponentMoveCallback(int move);

        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;
        private int idNum, gameID;
        private string opponent;

        private enum choices 
        {
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

        public void Dispose()
        {
            Dispose(true);
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
        
        /**
         * Send and recieve moves and perform logic
         * */
        private void findWinner(){

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            if (Connected)
            {
                SendMessage("9|" + gameID.ToString() + "|" + idNum.ToString() + "|" + pMove.ToString());//the move
                if (opponent == null)
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
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void InitializeConnection()
        {            
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(txtIP.Text);

            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            try
            {
                tcpServer.Connect(ipAddr, 9333);
                Connected = true;
            }
            catch(Exception e)
            {
                Connected = false;
                MessageBox.Show("exception: "+e.ToString());
            }
            if (Connected)
            {
                UserName = txtUser.Text.ToString();

                // Send the desired username to the server                
                SendMessage(UserName);

                // Start the thread for receiving messages and further communication
                thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
                thrMessaging.Start();
            }          
        }

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

        private int ReceiveMove()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string s = srReceiver.ReadLine();
            if (s[0] == '9' && s.Length > 2)
            {
                return (int)Char.GetNumericValue(s[2]);
            }
            return 0;
        }

        //send any string to server
        //does not validate string
        private void SendMessage(string msg)
        {
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(msg);
            swSender.Flush();
        }

        //receive messages from server
        //parse code (first digit) for action to take
        private void ReceiveMessages()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string ConResponse = srReceiver.ReadLine();

            //connection was successful
            if (ConResponse[0] == '1')
            {           
                // Update the form to tell it we are now connected
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });            
            }
            else //connection was unsuccessful
            {
                string Reason = "Not Connected: ";
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);

                // Update the form with the reason why we couldn't connect
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });

                return;
            }

            while (Connected)
            {
                // Show the messages in the log TextBox
                string s = srReceiver.ReadLine();
                //this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { s });

                if (s[0] == '8')//set player id
                {
                    idNum = Convert.ToInt32(Char.GetNumericValue(s[2]));

                }
                else if(s[0] == '7')//set game id
                {
                    gameID = Convert.ToInt32(Char.GetNumericValue(s[2]));
                }
                else if (s[0] == '9' && s.Length > 2)
                {
                    int move = (int)Char.GetNumericValue(s[2]);
                    this.Invoke(new UpdateOpponentMoveCallback(this.SetOpponentMove), new object[] { move });
                }// 3|recipient id|num items|item1 gameid|item1 creater name|item2...
                else if (s[0] == '3')
                {
                    int j = 0;//number of items
                    if ((j = Convert.ToInt32(Char.GetNumericValue(s[4]))) > 0)
                    {
                        int gameid; 
                        String name1,
                            name2;
                        string msg;
                        int k = 6;//extra counter
                        this.Invoke(new ClearListBoxCallback(ClearListBox));
                        for(int i = 0; i < j; ++i)
                        {
                            gameid = Convert.ToInt32(Char.GetNumericValue(s[k]));
                            name1 = s.Substring(k+2, s.IndexOf('|', k+2) - (k+2));
                            k = s.IndexOf('|', k + 2) +1;
                            name2 = s.Substring(k, s.IndexOf('|', k) - k);
                            msg = "GameID: " + gameid.ToString() + "    " + "P1: " + name1+" P2: "+name2;
                            this.Invoke(new UpdateListBoxCallback(UpdateListBox), new object[] { msg });
                            k = s.IndexOf('|', k) +1;
                        }
                    }
                }
                else if (s[0] == '6' && s.Length > 2)
                {
                    string txtmsg = s.Substring(2, s.Length - 2);
                    if (txtmsg.Length > 0)
                        this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { txtmsg });
                }
            }
        }

        //clear listbox from a seperate thread (using a delegate)
        private void ClearListBox() { listBox1.Items.Clear(); }

        //update listbox from a seperate thread (using a delegate)
        private void UpdateListBox(string s)
        {
            listBox1.Items.Add(s);
            listBox1.Invalidate();
        }

        //update opponent move from a seperate thread (using a delegate)
        private void SetOpponentMove(int move)
        {
            cMove = move;
        }

        //update log textbox from a seperate thread (using a delegate)
        private void UpdateLog(string strMessage)
        {
            // Append text also scrolls the TextBox to the bottom each time
            //dont show ID's or moves
            if(strMessage[0] != '8' && strMessage[0] != '9')
                txtLog.AppendText(strMessage + "\r\n");
        }
        
        //button clicks
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!Connected)
            {
                if (txtUser.Text.ToString() != "" && txtIP.Text.ToString() != "")
                {
                    InitializeConnection();
                    if (Connected)
                    {
                        btnConnect.Text = "Disconnect";
                        ArrayList list = new ArrayList();
                        while (list == null) { }
                       
                        listBox1.Visible = true;
                        this.Width = 510;
                        this.Height = 670;
                    }
                }
                else
                    MessageBox.Show("Enter a valid username");
            }
            else
            {
                CloseConnection("User request");
                btnConnect.Text = "Connect";
                listBox1.Visible = false;
                this.Width = 510;
                this.Height = 305;
            }
        }

        private void choice_Click(object sender, EventArgs e)
        {
            pMove = (int)Char.GetNumericValue(((Button)sender).Name.ToString()[6]);
            findWinner();
        }        

        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = UserName+" says: "+textBox1.Text;
            SendMessage("6|"+msg);
           // txtLog.AppendText(msg+"\r\n");
            textBox1.Text = "";
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            SendMessage("4|" + idNum.ToString());
        }

        private void btnJoinGame_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count != 0)
            {
                string s = listBox1.SelectedItem.ToString();
                int gameid = Convert.ToInt32(Char.GetNumericValue(s[8]));
                gameID = gameid;
                SendMessage("5|" + gameid.ToString() + "|" + idNum.ToString());
                opponent = s.Substring(17, s.IndexOf('P',17)-18);
                label3.Text = "ID: " + idNum.ToString() + "  GID: " + gameID.ToString();//dbg                
            }
        }
    }
}