using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;

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
            LoadPassword();
            Invoke(new MethodInvoker(UpdateChecker.UpdateCheck));
        }
        public string SelectedServer = null;
        public bool SelectedServerMinecraft = false;
        public string Cookie;
        public string SinglePlayer = null;

        void SearchServerList(string Cont)
        {
            if(Cont == null || Cont.Length < 1) return;
            if (items.Count == 0)
            {
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name.ToLower().Contains(Cont.ToLower()))
                {
                    string[] row = { items[i].Name, items[i].Players.ToString(), items[i].PlayersMax.ToString(),
                                   items[i].Url};
                    ListBox1.Rows.Add(row);
                }
            }
            foreach (DataGridViewColumn column in ListBox1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }
        void DrawServerList()
        {
            if (items == null || items.Count < 1)
            {
                items = c.ServerList(textBox2.Text, textBox3.Text);
            }
            if (items.Count == 0)
            {
                MessageBox.Show("Could not retrieve server list\n " +
                    "Either Minecraft.net is down or your username and password\nare incorrect");
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                string[] row = { items[i].Name, items[i].Players.ToString(), items[i].PlayersMax.ToString(),
                                   items[i].Url};
                ListBox1.Rows.Add(row);
            }
            foreach (DataGridViewColumn column in ListBox1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
            SearchBox.Enabled = true;
        }
        void c_Progress(object sender, ProgressEventArgs e)
        {
            progressBar1.Refresh();
            progressBar1.Value = e.ProgressPercent;
            progressBar1.CreateGraphics().DrawString(e.ProgressPercent+"%",
            new Font("Times New Roman", (float)12, FontStyle.Bold),
            Brushes.Black, new PointF(progressBar1.Width / 2 - 10,
                progressBar1.Height / 2 - 7));
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
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MCP.data");
        }

        private void Button2_Click_3(object sender, EventArgs e)
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

                if (checkBox1.Checked)
                {
                    RememberPassword(textBox2.Text, textBox3.Text);
                }
                DrawServerList();
                TabControl1.SelectTab(1);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void SearchBox_TextChanged_2(object sender, EventArgs e)
        {
            if (c == null) return;
            if (SearchBox.Text.Length < 1)
            {
                Invoke(new MethodInvoker(DrawServerList));
                return;
            }
            ListBox1.Rows.Clear();
            SearchServerList(SearchBox.Text);
        }

        private void Button3_Click_1(object sender, EventArgs e)
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

        private void Button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Enter username and password.");
                return;
            }
            if (textBox4.Text == "" || !textBox4.Text.StartsWith("http://"))
            {
                MessageBox.Show("Invalid server address.");
                return;
            }
            SelectedServer = textBox4.Text;
            SelectedServerMinecraft = true;
            SetLoginData(textBox4.Text);
            Close();
        }

        private void ListBox1_SelectionChanged(object sender, EventArgs e)
        {
            if (ListBox1.SelectedRows.Count < 1) return;
            int index = ListBox1.SelectedRows[0].Index;
            if (index == -1) return;
            textBox4.Text = ListBox1.Rows[index].Cells[3].Value.ToString();
        }
    }
}