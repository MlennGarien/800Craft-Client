namespace ManicDigger
{
    partial class ServerSelector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.tbpStudentInfo = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.Button2 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.tbpStudentOutput = new System.Windows.Forms.TabPage();
            this.ListBox1 = new System.Windows.Forms.DataGridView();
            this.Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Players = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxPlayers = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Url = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Button4 = new System.Windows.Forms.Button();
            this.Button3 = new System.Windows.Forms.Button();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.TabControl1.SuspendLayout();
            this.tbpStudentInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tbpStudentOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ListBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.tbpStudentInfo);
            this.TabControl1.Controls.Add(this.tbpStudentOutput);
            this.TabControl1.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControl1.Location = new System.Drawing.Point(-4, -30);
            this.TabControl1.Multiline = true;
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(451, 472);
            this.TabControl1.TabIndex = 8;
            // 
            // tbpStudentInfo
            // 
            this.tbpStudentInfo.BackColor = System.Drawing.SystemColors.Control;
            this.tbpStudentInfo.Controls.Add(this.pictureBox1);
            this.tbpStudentInfo.Controls.Add(this.progressBar1);
            this.tbpStudentInfo.Controls.Add(this.Button2);
            this.tbpStudentInfo.Controls.Add(this.checkBox1);
            this.tbpStudentInfo.Controls.Add(this.textBox3);
            this.tbpStudentInfo.Controls.Add(this.Label5);
            this.tbpStudentInfo.Controls.Add(this.textBox2);
            this.tbpStudentInfo.Controls.Add(this.Label1);
            this.tbpStudentInfo.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbpStudentInfo.Location = new System.Drawing.Point(4, 30);
            this.tbpStudentInfo.Name = "tbpStudentInfo";
            this.tbpStudentInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tbpStudentInfo.Size = new System.Drawing.Size(443, 438);
            this.tbpStudentInfo.TabIndex = 0;
            this.tbpStudentInfo.Text = "Student Information";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Start.Properties.Resources.backlogo1;
            this.pictureBox1.Location = new System.Drawing.Point(-14, -30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(474, 197);
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(134, 368);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(168, 33);
            this.progressBar1.TabIndex = 11;
            // 
            // Button2
            // 
            this.Button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Button2.FlatAppearance.BorderSize = 0;
            this.Button2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
            this.Button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.Button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.Font = new System.Drawing.Font("Segoe UI Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button2.ForeColor = System.Drawing.Color.White;
            this.Button2.Location = new System.Drawing.Point(200, 283);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(187, 53);
            this.Button2.TabIndex = 10;
            this.Button2.Text = "Connect";
            this.Button2.UseVisualStyleBackColor = false;
            this.Button2.Click += new System.EventHandler(this.Button2_Click_3);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(37, 300);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(127, 25);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Remember Me";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(200, 243);
            this.textBox3.Name = "textBox3";
            this.textBox3.PasswordChar = '*';
            this.textBox3.Size = new System.Drawing.Size(187, 33);
            this.textBox3.TabIndex = 1;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(32, 246);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(89, 25);
            this.Label5.TabIndex = 5;
            this.Label5.Text = "Password:";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(200, 192);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(187, 33);
            this.textBox2.TabIndex = 0;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(32, 195);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(96, 25);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Username:";
            // 
            // tbpStudentOutput
            // 
            this.tbpStudentOutput.BackColor = System.Drawing.SystemColors.Control;
            this.tbpStudentOutput.Controls.Add(this.ListBox1);
            this.tbpStudentOutput.Controls.Add(this.textBox4);
            this.tbpStudentOutput.Controls.Add(this.Label2);
            this.tbpStudentOutput.Controls.Add(this.Button4);
            this.tbpStudentOutput.Controls.Add(this.Button3);
            this.tbpStudentOutput.Controls.Add(this.SearchBox);
            this.tbpStudentOutput.Controls.Add(this.Label3);
            this.tbpStudentOutput.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbpStudentOutput.Location = new System.Drawing.Point(4, 30);
            this.tbpStudentOutput.Name = "tbpStudentOutput";
            this.tbpStudentOutput.Padding = new System.Windows.Forms.Padding(3);
            this.tbpStudentOutput.Size = new System.Drawing.Size(443, 438);
            this.tbpStudentOutput.TabIndex = 1;
            this.tbpStudentOutput.Text = "Student Output";
            // 
            // ListBox1
            // 
            this.ListBox1.AllowUserToAddRows = false;
            this.ListBox1.AllowUserToDeleteRows = false;
            this.ListBox1.AllowUserToResizeRows = false;
            this.ListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListBox1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.ListBox1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ListBox1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Name,
            this.Players,
            this.MaxPlayers,
            this.Url});
            this.ListBox1.Location = new System.Drawing.Point(3, 108);
            this.ListBox1.MultiSelect = false;
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.ReadOnly = true;
            this.ListBox1.RowHeadersVisible = false;
            this.ListBox1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ListBox1.Size = new System.Drawing.Size(435, 279);
            this.ListBox1.TabIndex = 15;
            this.ListBox1.SelectionChanged += new System.EventHandler(this.ListBox1_SelectionChanged);
            // 
            // Name
            // 
            this.Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Name.FillWeight = 60F;
            this.Name.HeaderText = "Name";
            this.Name.Name = "Name";
            this.Name.ReadOnly = true;
            this.Name.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Players
            // 
            this.Players.HeaderText = "Players";
            this.Players.Name = "Players";
            this.Players.ReadOnly = true;
            this.Players.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Players.Width = 81;
            // 
            // MaxPlayers
            // 
            this.MaxPlayers.HeaderText = "Max Players";
            this.MaxPlayers.Name = "MaxPlayers";
            this.MaxPlayers.ReadOnly = true;
            this.MaxPlayers.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.MaxPlayers.Width = 113;
            // 
            // Url
            // 
            this.Url.HeaderText = "Url";
            this.Url.Name = "Url";
            this.Url.ReadOnly = true;
            this.Url.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Url.Visible = false;
            this.Url.Width = 53;
            // 
            // textBox4
            // 
            this.textBox4.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.Location = new System.Drawing.Point(12, 30);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(319, 33);
            this.textBox4.TabIndex = 14;
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(12, 2);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(104, 25);
            this.Label2.TabIndex = 13;
            this.Label2.Text = "Server URL:";
            // 
            // Button4
            // 
            this.Button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Button4.FlatAppearance.BorderSize = 0;
            this.Button4.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
            this.Button4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.Button4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.Button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button4.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button4.ForeColor = System.Drawing.Color.White;
            this.Button4.Location = new System.Drawing.Point(3, 393);
            this.Button4.Name = "Button4";
            this.Button4.Size = new System.Drawing.Size(435, 39);
            this.Button4.TabIndex = 12;
            this.Button4.Text = "Connect";
            this.Button4.UseVisualStyleBackColor = false;
            this.Button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // Button3
            // 
            this.Button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Button3.FlatAppearance.BorderSize = 0;
            this.Button3.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
            this.Button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.Button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.Button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button3.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button3.ForeColor = System.Drawing.Color.White;
            this.Button3.Location = new System.Drawing.Point(337, 30);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(86, 33);
            this.Button3.TabIndex = 11;
            this.Button3.Text = "Go";
            this.Button3.UseVisualStyleBackColor = false;
            this.Button3.Click += new System.EventHandler(this.Button3_Click_1);
            // 
            // SearchBox
            // 
            this.SearchBox.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchBox.Location = new System.Drawing.Point(81, 69);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(288, 33);
            this.SearchBox.TabIndex = 2;
            this.SearchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged_2);
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(12, 72);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(68, 25);
            this.Label3.TabIndex = 1;
            this.Label3.Text = "Search:";
            // 
            // ServerSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 436);
            this.Controls.Add(this.TabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Text = "800Craft Client";
            this.Load += new System.EventHandler(this.ServerSelector_Load);
            this.TabControl1.ResumeLayout(false);
            this.tbpStudentInfo.ResumeLayout(false);
            this.tbpStudentInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tbpStudentOutput.ResumeLayout(false);
            this.tbpStudentOutput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ListBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TabControl TabControl1;
        internal System.Windows.Forms.TabPage tbpStudentInfo;
        internal System.Windows.Forms.Button Button2;
        internal System.Windows.Forms.CheckBox checkBox1;
        internal System.Windows.Forms.TextBox textBox3;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.TextBox textBox2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TabPage tbpStudentOutput;
        internal System.Windows.Forms.Button Button4;
        internal System.Windows.Forms.Button Button3;
        internal System.Windows.Forms.TextBox SearchBox;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.TextBox textBox4;
        internal System.Windows.Forms.Label Label2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.DataGridView ListBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Players;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxPlayers;
        private System.Windows.Forms.DataGridViewTextBoxColumn Url;



    }
}