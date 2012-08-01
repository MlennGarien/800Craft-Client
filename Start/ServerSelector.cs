using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace ManicDigger
{
    public partial class ServerSelector : Form
    {
        public ServerSelector()
        {
            InitializeComponent();
        }
        public LoginClientMinecraft c;
        public List<ServerInfo> items;

        private void ServerSelector_Load(object sender, EventArgs e)
        {
            webBrowser2.Navigating += new WebBrowserNavigatingEventHandler(webBrowser2_Navigating);
            LoadPassword();
        }
        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

        }
        void webBrowser2_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.AbsoluteUri.StartsWith("http://www.minecraft.net/"))
            {
                return;
            }
            e.Cancel = true;
            SelectedServer = e.Url.AbsoluteUri;
            SelectedServerMinecraft = true;
            SetLoginData(SelectedServer);
            Close();
        }
        public string SelectedServer = null;
        public bool SelectedServerMinecraft = false;
        public string Cookie;
        public string SinglePlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click(object sender, EventArgs e)
        {
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
        }
        private void button2_Click_2(object sender, EventArgs e)
        {
            SinglePlayer = "Mine";
            Close();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
        private void label2_Click(object sender, EventArgs e)
        {
        }
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            SinglePlayer = "Fortress";
            Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }
            if (c == null)
            {
                c = new LoginClientMinecraft();
                c.Progress += new EventHandler<ProgressEventArgs>(c_Progress);
            }
            if (checkBox1.Checked)
            {
                RememberPassword(textBox2.Text, textBox3.Text);
            }
            DrawServerList();
        }

        void SearchServerList(string Cont)
        {
            if(Cont == null || Cont.Length < 1) return;
            StringBuilder html = new StringBuilder();
            if (items.Count == 0)
            {
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                var item = new ListViewItem();
                if (items[i].Name.ToLower().Contains(Cont.ToLower()))
                {
                    item.Text = items[i].Name;
                    item.SubItems.Add(items[i].Players.ToString());
                    item.SubItems.Add(items[i].PlayersMax.ToString());
                    html.AppendLine(string.Format("<a href=\"{0}\"><b>{1}</b></a> {2}/{3} <br>",
                        items[i].Url, items[i].Name, items[i].Players, items[i].PlayersMax));
                }
            }
            if (html.Length < 1)
            {
                webBrowser2.DocumentText = "No matches found";
            }
            else
            {
                webBrowser2.DocumentText = html.ToString();
            }
        }

        void DrawServerList()
        {
            if (items == null)
            {
                items = c.ServerList(textBox2.Text, textBox3.Text);
            }
            if (items.Count == 0)
            {
                MessageBox.Show("Could not retrieve server list");
                return;
            }
            StringBuilder html = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                var item = new ListViewItem();
                item.Text = items[i].Name;
                item.SubItems.Add(items[i].Players.ToString());
                item.SubItems.Add(items[i].PlayersMax.ToString());
                html.AppendLine(string.Format("<a href=\"{0}\"><b>{1}</b></a> {2}/{3} <br>",
                    items[i].Url, items[i].Name, items[i].Players, items[i].PlayersMax));
            }
            webBrowser2.DocumentText = html.ToString();
            SearchBox.Enabled = true;
        }
        void c_Progress(object sender, ProgressEventArgs e)
        {
            progressBar1.Value = e.ProgressPercent;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }
            if (textBox4.Text == "")
            {
                MessageBox.Show("Invalid server address.");
                return;
            }
            SelectedServer = textBox4.Text;
            SelectedServerMinecraft = true;
            SetLoginData(textBox4.Text);
            Close();
        }
        private void SetLoginData(string url)
        {
            LoginClientMinecraft c = new LoginClientMinecraft();
            LoginData logindata = c.Login(textBox2.Text, textBox3.Text, url);
            this.LoginIp = logindata.serveraddress;
            this.LoginPassword = logindata.mppass;
            this.LoginPort = logindata.port.ToString();
            this.LoginUser = textBox2.Text;
        }
        public string LoginIp;
        public string LoginPort;
        public string LoginUser;
        public string LoginPassword;
        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                string filename = GetMinecraftPasswordFilePath();
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                RememberPassword(textBox2.Text, textBox3.Text);
            }
        }
        private void RememberPassword(string user, string password)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(user);
            b.AppendLine(password);
            File.WriteAllText(GetMinecraftPasswordFilePath(), b.ToString());
        }
        void LoadPassword()
        {
            string filename = GetMinecraftPasswordFilePath();
            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);
                textBox2.Text = lines[0];
                textBox3.Text = lines[1];
                checkBox1.Checked = true;
            }
        }
        private static string GetMinecraftPasswordFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MinecraftPassword.txt");
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void SearchBox_TextChanged_1(object sender, EventArgs e)
        {
            if (c == null) return;
            if (SearchBox.Text.Length < 1)
            {
                DrawServerList();
                return;
            }
            SearchServerList(SearchBox.Text);
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }
    }
}