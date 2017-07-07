using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SoniaOnline.Forms
{
    public partial class Chatting : Form
    {
        private bool _altF4Pressed;
        private int chatlength = 0;
        public Label prelabel;

        public Chatting()
        {
            InitializeComponent();
        }

        private void Chatting_Load(object sender, EventArgs e)
        {
            // add event handlers
            this.KeyDown += Chatting_KeyDown;
            this.FormClosing += Chatting_FormClosing;
            textBox1.KeyDown += textBox1_KeyDown;
            label1.Text = "";
            this.Activated += Chatting_Activated;
        }

        private void Chatting_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_altF4Pressed)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
                _altF4Pressed = false;
            }
        }

        public void addChat(string text, Color color)
        {
            if (prelabel == null)
            {
                prelabel = new Label();
                prelabel.Location = new Point(3, -17);
            }
            Label lb = new Label();
            lb.AutoSize = true;
            lb.Parent = panel1;
            lb.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            lb.ForeColor = color;
            lb.Location = new System.Drawing.Point(3, 3 + prelabel.Location.Y + 17);
            lb.Name = "label1";
            lb.Size = new System.Drawing.Size(39, 15);
            lb.Text = text;
            chatlength++;
            prelabel = lb;
        }

        // # prevent form-closeing, send chat data
        private void Chatting_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;

            if (e.KeyCode == Keys.Enter)
            {
                if (!textBox1.Text.Equals(""))
                {
                    Program.Chatstring = Properties.Settings.Default.UserId + (" : " + textBox1.Text);
                    textBox1.Text = "";
                    Properties.Settings.Default.chat_activated = false;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Chatting_Activated(object sender, EventArgs e)
        {
            Properties.Settings.Default.chat_activated = true;
        }  
    }
}
