using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace RPS_server
{
    public partial class ServerForm : Form
    {
        private GameServer mainServer;

        public ServerForm()
        {
            InitializeComponent();
        }        

        private delegate void UpdateStatusCallback(string strMessage);

        private void btnStartListening_Click(object sender, EventArgs e)
        {
            if (btnStartListening.Text.ToString() == "Start Listening")
            {
                //string txtIp = "127.0.0.1";
                // Parse the server's IP address out of the TextBox
                IPAddress ipAddr = IPAddress.Parse(txtIp.Text.ToString());

                // Create a new instance of the ChatServer object
                mainServer = new GameServer(ipAddr);

                // Hook the StatusChanged event handler to mainServer_StatusChanged
                GameServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);

                // Start listening for connections
                mainServer.StartListening();

                // Show that we started to listen for connections
                txtLog.AppendText("Monitoring for connections...\r\n");
                btnStartListening.Text = "Stop Listening";
            }
            else
            {
                mainServer.StopListening();
                btnStartListening.Text = "Start Listening";
            }
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            try
            {
                // Call the method that updates the form
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
            }
            catch (Exception ex) { System.Console.WriteLine(ex); }
        }

        private void UpdateStatus(string strMessage)
        {
            // Updates the log with the message
            txtLog.AppendText(strMessage + "\r\n");
        }
    }
}
