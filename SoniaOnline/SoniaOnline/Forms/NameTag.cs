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
    public partial class NameTag : Form
    {
        private bool _altF4Pressed;

        public NameTag(string name)
        {
            InitializeComponent();
            this.label_name.Text = name;  
        }

        private void UserNameLabel_Load(object sender, EventArgs e)
        {
            // add event handlers
            this.KeyDown += NameTag_KeyDown;
            this.FormClosing += NameTag_FormClosing;      

            // set form min & max size
            this.MinimumSize = new System.Drawing.Size(120, 15);
            this.Size = new Size(120, 15);
            label_name.Location = new Point(this.Width / 2 - (label_name.Text.Length * 11) / 2, label_name.Location.Y);         

            // set form opacity
            this.TransparencyKey = Color.Turquoise;
            this.BackColor = Color.Turquoise;
        }


        // # prevent form-closeing #
        private void NameTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;
        }

        private void NameTag_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_altF4Pressed)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
                _altF4Pressed = false;
            }
        }

    }
}
