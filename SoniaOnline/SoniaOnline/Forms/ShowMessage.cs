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
    public partial class ShowMessage : Form
    {
        // Timer
        Timer Start_opacity = new Timer(); // start opacity
        Timer Timer_erase = new Timer(); // dispose opacity
        Timer T_Update = new Timer();
        private bool _altF4Pressed;

        public ShowMessage()
        {
            InitializeComponent();
        }

        private void ShowMessage_Load(object sender, EventArgs e)
        {
            // update timer
            T_Update.Interval = 1;
            T_Update.Tick += new EventHandler(Update);
            T_Update.Start();

            this.KeyDown += Form_KeyDown;
            this.FormClosing += Form_FormClosing;
        }

        public void MakeMessage(string text)
        {
            this.Opacity = 0;
            label1.Text = text;

            // fadein
            Start_opacity.Interval = 30;
            Start_opacity.Tick += new EventHandler(MessOpacity);
            Start_opacity.Start();
        }

        public void slowErase()
        {
            Timer_erase.Interval = 30;
            Timer_erase.Tick += new EventHandler(eraseOpacity);
            Timer_erase.Start();
        }

        public void erase()
        {
            this.Dispose();
        }


        // # Timer implements #

        // timer : MessageForm - fadein
        private void MessOpacity(object sender, EventArgs e)
        {
            if (this.Opacity >= 0.8)
            {
                Start_opacity.Stop();
                Start_opacity.Dispose();

                slowErase();
            }
            else
            {
                this.Opacity += 0.025;
            }
        }

        // timer - fadein
        private void eraseOpacity(object sender, EventArgs e)
        {
            if (this.Opacity == 0)
            {
                Timer_erase.Stop();
                Timer_erase.Dispose();
                this.Dispose();
            }
            else
            {
                this.Opacity -= 0.025;
            }
        }

        // timer - update position
        private void Update(object sender, EventArgs e)
        {
            Point p_main = Properties.Settings.Default.Point_Mainform;
            this.Location = new System.Drawing.Point(p_main.X + 3, p_main.Y + 480/20 + 121);
        }

        // # prevent form-closeing #
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
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
