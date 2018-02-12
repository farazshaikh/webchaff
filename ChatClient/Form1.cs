using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        // Will hold the user name
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);
        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;
        private WebChaffEncoder Encoder;

        public Form1()
        {
            // On application exit, don't forget to disconnect first
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            
            InitializeComponent();
        }

        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Closes the connections, streams, etc.
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // If we are not currently connected but awaiting to connect
            if (Connected == false)
            {
                // Initialize the connection
                InitializeConnection();
            }
            else // We are connected, thus disconnect
            {
                CloseConnection("Disconnected at user's request.");
            }
        }

        private void InitializeConnection()
        {
            Encoder = new WebChaffEncoder();
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(txtIp.Text);
            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, 1986);

            // Helps us track whether we're connected or not
            Connected = true;
            // Prepare the form
            UserName = txtUser.Text;

            // Disable and enable the appropriate fields
            txtIp.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Disconnect";

            // Send the desired username to the server
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
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
            // While we are successfully connected, read incoming lines from the server
            while (Connected)
            {
                string rawMessage =  srReceiver.ReadLine();
                // Show the messages in the log TextBox
                // so here is the format of our message
                // <username>|<strlen(chaff)>|<chaff><mac>
                int index1,index2;
                index1 = rawMessage.IndexOf('|');
                if (index1 == -1)
                {
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { rawMessage });
                    continue;
                }

                index2 = rawMessage.IndexOf('|',index1 + 1);
                if (index2 == -1)
                {
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { rawMessage });
                    continue;
                }

                // ok now we have the 2 indices get the decoded messages
                string username;
                string chafflen;
                string chaff;
                string hmac;
                string decodedString;
                int chafflennr;
                username = rawMessage.Substring(0,index1);
                chafflen = rawMessage.Substring(index1 + 1, index2 - index1 - 1);
                chafflennr = Convert.ToInt32(chafflen);

                chaff = rawMessage.Substring(index2 + 1, chafflennr);
                hmac = rawMessage.Substring(chafflennr + 1 + index2, rawMessage.Length - (chafflennr + 1 + index2));
                decodedString = 
                    Encoder.DecodeMessage(chaff, hmac);

                decodedString = username + " : " + decodedString;
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { decodedString });


            }
        }

        // This method is called from a different thread in order to update the log TextBox
        private void UpdateLog(string strMessage)
        {
            // Append text also scrolls the TextBox to the bottom each time
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // Show the reason why the connection is ending
            txtLog.AppendText(Reason + "\r\n");
            // Enable and disable the appropriate controls on the form
            txtIp.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // Close the objects
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        // Sends the message typed in to the server
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                char[] arraySplit = { ' ' };
                String MacString = "";


                String EncodedText = Encoder.EncodeMessage(txtMessage.Text.Split(arraySplit), out MacString);

                EncodedText = EncodedText.Length.ToString() + '|'.ToString() + EncodedText + MacString;

                swSender.WriteLine(EncodedText);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        // We want to send the message when the Send button is clicked
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // But we also want to send the message once Enter is pressed
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the key is Enter
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}