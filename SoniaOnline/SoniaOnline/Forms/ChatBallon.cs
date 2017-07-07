using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SoniaOnline.Forms
{
    public partial class ChatBallon : Form
    {
        private bool _altF4Pressed;
        Timer Waitting = new Timer();
        Timer Timer_erase = new Timer();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );

        public ChatBallon(string text)
        {
            InitializeComponent();

            this.label1.Text = text;
        }

        private void ChatBallon_Load(object sender, EventArgs e)
        {
            // add event handlers
            this.KeyDown += ChatBallon_KeyDown;
            this.FormClosing += ChatBallon_FormClosing;

            // set min, max winform size
            this.MinimumSize = new System.Drawing.Size(label1.Width + 10, 17);
            this.Size = new Size(label1.Width + 10, 17);

            // make drawing round
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 3, 3));

            // fadein chatting bar
            Waitting.Interval = 3500;
            Waitting.Tick += new EventHandler(FirstWait);
            Waitting.Start();
        }

        // waitting
        private void FirstWait(object sender, EventArgs e)
        {
            // erase timer
            Timer_erase.Interval = 30;
            Timer_erase.Tick += new EventHandler(eraseOpacity);
            Timer_erase.Start();

            // disose waitting timer
            Waitting.Stop();
            Waitting.Dispose();
        }

        // fadeOff chatting bar
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

        // # prevent form-closing #
        private void ChatBallon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;
        }

        private void ChatBallon_FormClosing(object sender, FormClosingEventArgs e)
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
