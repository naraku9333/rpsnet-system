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
        private delegate void UpdateCMove(int move);
        private delegate void IDCALLBACK(int uid, int gid);

        private Thread thrMessaging, thrMove;
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

        //player choice button click
        private void choice_Click(object sender, EventArgs e)
        {
            //pMove = 1;
            pMove = (int)Char.GetNumericValue(((Button)sender).Name.ToString()[6]);
            findWinner(sender);
        }
        
        /**
         * Send and recieve moves and perform logic
         * */
        private void findWinner(object sender){
            ((Button)sender).Enabled = false;
            if (Connected)
            {
                SendMessage("9|" + gameID.ToString() + "|" + idNum.ToString() + "|" + pMove.ToString());//the move
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
            ((Button)sender).Enabled = true;
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
                swSender = new StreamWriter(tcpServer.GetStream());
                swSender.WriteLine(UserName);
                swSender.Flush();
               // list = new Array();

                // Start the thread for receiving messages and further communication
                thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
                thrMessaging.Start();
            }          
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

            // If the first character is a 1, connection was successful
            if (ConResponse[0] == '1')
            {           
                // Update the form to tell it we are now connected
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });            
            }
            else // If the first character is not a 1 (probably a 0), the connection was unsuccessful
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
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { s });

                if (s[0] == '8')//set player id
                {
                    idNum = Convert.ToInt32(Char.GetNumericValue(s[2]));
                    this.Invoke(new IDCALLBACK(this.IDSHOW), new object[] { idNum, gameID });//dbg

                }
                else if(s[0] == '7')//set game id
                {
                    gameID = Convert.ToInt32(Char.GetNumericValue(s[2]));
                    this.Invoke(new IDCALLBACK(this.IDSHOW), new object[] { idNum, gameID });//dbg
                }
                else if (s[0] == '9')
                {
                    int move = (int)Char.GetNumericValue(s[2]);
                    this.Invoke(new UpdateCMove(this.SetCMove), new object[] { move });
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

        //dbg
        private void IDSHOW(int x, int y)
        {
            label3.Text = "ID: "+x.ToString()+"  GID: "+y.ToString();
        }

        //update opponent move from a seperate thread (using a delegate)
        private void SetCMove(int move)
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

        // Closes the current connection
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

        //connect to server
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
                        this.Width = 810;
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
            }
        }

        //send a text message to server
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage(textBox1.Text);
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
                label3.Text = "ID: " + idNum.ToString() + "  GID: " + gameID.ToString();//dbg
            }
        }
    }
}