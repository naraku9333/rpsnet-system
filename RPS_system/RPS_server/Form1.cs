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
    public partial class Form1 : Form
    {
        private ChatServer mainServer;

        public Form1()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
        }

        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {
           
        }

        private delegate void UpdateStatusCallback(string strMessage);

        private void btnStartListening_Click(object sender, EventArgs e)
        {
            //string txtIp = "127.0.0.1";
            // Parse the server's IP address out of the TextBox
            IPAddress ipAddr = IPAddress.Parse(txtIp.Text.ToString());

            // Create a new instance of the ChatServer object
            mainServer = new ChatServer(ipAddr);

            // Hook the StatusChanged event handler to mainServer_StatusChanged
            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);

            // Start listening for connections
            mainServer.StartListening();

            // Show that we started to listen for connections
            txtLog.AppendText("Monitoring for connections...\r\n");
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Call the method that updates the form
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // Updates the log with the message
            txtLog.AppendText(strMessage + "\r\n");
        }
    }
}
