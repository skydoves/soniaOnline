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
    public partial class Login : Form
    {
        private bool _altF4Pressed;
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

        public Login()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            this.FormClosed += Login_FormClosed;
            this.KeyDown += Login_KeyDown;
            this.FormClosing += Login_FormClosing;
            textBox_id.KeyDown += Login_KeyDown;
            textBox_pass.KeyDown += Login_KeyDown;

            //// Form의 배경을 투명하게 조절한다.
            //this.TransparencyKey = Color.Turquoise;
            //this.BackColor = Color.Turquoise;

        }

        private void Login_Load(object sender, EventArgs e)
        {
            textBox_pass.PasswordChar = '*';
        }

        private void Login_FormClosed(object sender, EventArgs e)
        {

        }

        private void LoginClick()
        {
            if (!textBox_id.Text.Contains(Program.padd))
            {
                if (!textBox_id.Text.Equals("") && !textBox_pass.Text.Equals(""))
                   Properties.Settings.Default.Login_click = true;
            }
        }

        #region Login Button & Handling Login Success

        // Login Button Click
        private void button1_Click(object sender, EventArgs e)
        {
            LoginClick();
        }

        // Form Key Down
        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;

            if (e.KeyCode == Keys.Enter)
            {
                LoginClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_altF4Pressed)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
                _altF4Pressed = false;
            }
        }

        #endregion

        // button Registrarion
        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.FormMinimize = true;
            Process.Start("IExplore", "http://www.rurimosoft.com");
        }
    }
}
