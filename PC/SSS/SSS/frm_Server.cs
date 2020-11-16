using SSS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SSS
{
    public partial class frm_Server : Form
    {
        //  Timer
        static int Second;
        static int Minute;
        static int Hour;
        string Sec;
        string Min;
        string Hr;


        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        public Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public frm_Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }

        private void frm_Server_Load(object sender, EventArgs e)
        {
            SetupServer();
        }

        private void SetupServer()
        {
            txt_Text.Text = App.CurrentInstance.sssDevice.ContentDirectory.GetWasapiUris(Audio.AudioFormats.Format.Mp3).FirstOrDefault();
            lb_stt.Text = " Setting up Server  ...  ...  ...         ";
            lb_soluong.Text = "                 No Student Connected Yet  ";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));
            list_Client.Items.Add(socket.RemoteEndPoint.ToString());

            lb_soluong.Text = "                 Number of Connected Students : " + __ClientSockets.Count.ToString();
            lb_stt.Text = " Student connected  ...  ...  ...     ";
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            lb_soluong.Text = "                 Number of Connected Students : " + __ClientSockets.Count.ToString();
                        }
                    }
                    return;
                }

                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    //lb_stt.Text = "Text received: " + text;

                    string reponse = string.Empty;

                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()) && text != "bye")
                        {
                            rich_Text.AppendText( __ClientSockets[i]._Name + "" + text + "\n");
                        }
                    }


                    if (text == "bye")
                    {
                        return;
                    }
                    Sendata(socket, reponse);
                }

                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            lb_soluong.Text = "                 Number of Connected Students : " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            try
            {
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            }
            catch (Exception)
            {
                
            }
        }

        void Sendata(Socket socket, string content)
        {
            byte[] data = Encoding.ASCII.GetBytes(content);
            try
            {
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
            catch (Exception)
            {

            }
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            try
            {
                socket.EndSend(AR);
            }
            catch (Exception)
            {

            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("No Students Selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else if (rich_Text.Lines.Count() == 0)
            {
                MessageBox.Show("Let the students submit their information first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                for (int i = 0; i < list_Client.SelectedItems.Count; i++)
                {
                    Sendata(__ClientSockets[i]._Socket, txt_Text.Text);

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                for (int i = 0; i < list_Client.SelectedItems.Count; i++)
                {
                    Sendata(__ClientSockets[i]._Socket, "bye");

                }
                openFileDialog1.ShowDialog();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                axWindowsMediaPlayer1.URL = openFileDialog1.FileName;
                axWindowsMediaPlayer1.Ctlcontrols.play();

                //  Timer
                Timer.Enabled = true;
                Timer.Start();
                Second = 0;
                Minute = 0;
                Hour = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();

                Timer.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
                axWindowsMediaPlayer1.Ctlcontrols.pause();

            Timer.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();

                Timer.Stop();
                Time.Text = "0" + " : " + "0" + " : " + "0";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.volume = trackBar1.Value;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Second++;

            if (Second == 60)
            {
                Second = 0;
                Minute++;
            }

            if (Minute == 60)
            {
                Minute = 0;
                Hour++;
            }

            Sec = Convert.ToString(Second);
            Min = Convert.ToString(Minute);
            Hr = Convert.ToString(Hour);

            Time.Text = Hr + " : " + Min + " : " + Sec;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (list_Client.SelectedItems.Count == 0)
            {
                MessageBox.Show("Host a Test First", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Text Documents|*.txt", ValidateNames = true })
                {
                    if(sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName))
                        {
                            sw.Write(rich_Text.Text);
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}