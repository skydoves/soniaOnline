using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SoniaOnline.Classes
{
    class OtherUser
    {
        public string username;
        public int mapid;
        public int actor_x;
        public int actor_y;
        public Texture2D sprite;
        public System.Drawing.Point activepo;
        Forms.NameTag nametag;
        public Forms.ChatBallon chatballon;
        public int position;
        public int MovingStack = 0;

        // timer
        Timer T_Update = new Timer();
        Timer T_Moving = new Timer();

        // @overriding
        public OtherUser(string username, int mapid, int actor_x, int actor_y, Texture2D sprite)
        {
            this.username = username;
            this.mapid = mapid;
            this.actor_x = actor_x;
            this.actor_y = actor_y;
            this.sprite = sprite;
            this.activepo = new System.Drawing.Point(0, 0);
            this.position = -1;

            // initialize name Tag
            nametag = new Forms.NameTag(username);
            nametag.Show();

            // update timer
            T_Update.Interval = 10;
            T_Update.Tick += new EventHandler(Update);
            T_Update.Start();
        }

        // update user interface GUI - it based on dead reackoning
        private void Update(object sender, EventArgs e)
        {
            // user name tag
            int x = (int)(Properties.Settings.Default.Point_Mainform.X + actor_x - Properties.Settings.Default.cam_po.X + 405) - 100;
            int y = (int)(Properties.Settings.Default.Point_Mainform.Y + actor_y - Properties.Settings.Default.cam_po.Y + 250) - 95;

            if (x <= Properties.Settings.Default.Point_Mainform.X - 45 || x >= Properties.Settings.Default.Point_Mainform.X + 745 ||
                y <= Properties.Settings.Default.Point_Mainform.Y || y >= Properties.Settings.Default.Point_Mainform.Y + 480)
                nametag.Opacity = 0;
            else
            {
                nametag.Opacity = 1;
                nametag.Location = new System.Drawing.Point(x, y);
            }

            // chat ballon
            if(chatballon != null)
            {
                int x2 = (int)(Properties.Settings.Default.Point_Mainform.X + actor_x - Properties.Settings.Default.cam_po.X + 355) - chatballon.Width / 2;
                int y2 = (int)(Properties.Settings.Default.Point_Mainform.Y + actor_y - Properties.Settings.Default.cam_po.Y + 135);

                chatballon.Location = new System.Drawing.Point(x2, y2);

                if (x2 <= Properties.Settings.Default.Point_Mainform.X - 45 - chatballon.Width / 2 || x2 >= Properties.Settings.Default.Point_Mainform.X + 745 + chatballon.Width / 2 ||
                y2 <= Properties.Settings.Default.Point_Mainform.Y || y2 >= Properties.Settings.Default.Point_Mainform.Y + 480)
                    chatballon.Opacity = 0;
                else
                    chatballon.Location = new System.Drawing.Point(x2, y2);
            }
        }

        // user moving inferface
        public void UserMove(int position)
        {
            // set position
            activepo = new System.Drawing.Point(position, 0);
            this.position = position;
        }

        // stop user move
        public void UserStop()
        {
            this.position = -1;
        }

        // user clear
        public void clear()
        {
            nametag.Dispose();

            if (chatballon != null)
            chatballon.Dispose();
        }
    }
}
