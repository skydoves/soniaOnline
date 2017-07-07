using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace SoniaOnline
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Socket m_ClientSocket;
        Camera2d cam;

        List<Classes.OtherUser> Users = new List<Classes.OtherUser>();
        List<Rectangle> MapDataRect = new List<Rectangle>();
        List<Rectangle> MapPotalRect = new List<Rectangle>();
        List<string> MapPotalData = new List<string>();

        Forms.Login Form_Login;

        System.Windows.Forms.Form Mainform;
        KeyboardState previousKayboardState = Keyboard.GetState();
        bool keyboardVal = false;
 

        #region Objects

        // background
        Texture2D Layer1, Layer2;
        Vector2 vec_Layer1, vec_Layer2;
        public Rectangle viewportRect;

        // actors
        GameObject Actor;
        System.Drawing.Point Actor_activepo = new System.Drawing.Point(0, 0);
        int MovingStack=0;
        int previouspo = 0;
        Forms.Chatting Form_Chat;
        Forms.ChatBallon Form_Ballon;
        Forms.NameTag nametag;

        #endregion


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // Basic Initialize
            base.Initialize();

            // Socket binding
            #region Server Socket Connection

            m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Program.lib.Ip()), 35000);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = ipep;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Complete);
            m_ClientSocket.ConnectAsync(args);

            #endregion

            // System initialize
            #region System Settings

            // 마우스 Visible
            IsMouseVisible = true;

            // Basic Form Setting...
            Mainform = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            Mainform.Move += Mainform_Move;
            Mainform.FormClosing += Mainform_FormClosing;

            // Initialize Camara
            cam = new Camera2d();
            cam.Pos = new Vector2(400, 240);

            // Initialize Login Form
            Form_Login = new Forms.Login();
            Form_Login.Show();
            Form_Login.Location = new System.Drawing.Point(Mainform.Location.X + 280, Mainform.Location.Y + 300);

            #endregion
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // 로그인 배경
            Layer1 = Content.Load<Texture2D>("Objects\\Title");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            #region Keyboard Handling

            if (Properties.Settings.Default.LoginCheck)
            {
                // get keyboard press status
                KeyboardState keyboardstate = Keyboard.GetState();

                // @ acive chatform
                try
                {
                    if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && Form_Chat != null)
                        Form_Chat.Activate();
                }
                catch { }

                // handling users moving
                if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                    ActorMoving(0);
                else if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                    ActorMoving(1);
                else if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                    ActorMoving(2);
                else if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                    ActorMoving(3);
                else if (keyboardVal)
                {
                    keyboardVal = false;

                    Send_Packet(
                    Properties.Settings.Default.UserId + "," +
                    Properties.Settings.Default.MapId + "," +
                    "&%*^%8VdvteMWCqAqzDs7tXdtJA==&%*^%");
                }
                // save the last keyboard status
                previousKayboardState = keyboardstate;
            }

            #endregion

            #region Handling Login

                // pressed login button
                if (Properties.Settings.Default.Login_click)
                {
                    Send_Packet(Form_Login.textBox_id.Text + "," + Form_Login.textBox_pass.Text + ",&%*^%QX9y5TpZR6DOt7pLqpIbQw==&%*^%");
                    Properties.Settings.Default.Login_click = false;
                }

            #endregion

            #region [Networks]

            // drawing other players
            for (int i = 0; i < Users.Count; i++)
                UserMoving(i);

            #endregion


            #region [Conditional Systems] : Chat, Form Control, Exit

                // send a new chat
            if(!Program.Chatstring.Equals(""))
            {
                Send_Packet(Program.Chatstring + "&%*^%Q1SXiLEURCvizN+cfayrWsbtLahNidB/LQpJEauR26Q=&%*^%");

                // # show up a ballon #
                // remove previous ballon
                if (Form_Ballon != null)
                    Form_Ballon.Dispose();

                // a new ballon
                Form_Ballon = new Forms.ChatBallon(Program.Chatstring);
                Form_Ballon.Show();

                // initialize chat data
                Program.Chatstring = "";
            }

            // minimize form size
            if (Properties.Settings.Default.FormMinimize)
            {
                Mainform.WindowState = FormWindowState.Minimized;
                Properties.Settings.Default.FormMinimize = false;
            }

            // exit game
            if (Properties.Settings.Default.gameclose)
                Mainform.Dispose();

            #endregion

            #region [Auto Systems]

            // autosave mainform's position
            Properties.Settings.Default.Point_Mainform = new System.Drawing.Point(Mainform.Location.X, Mainform.Location.Y);

            // autosave cam's position
            Properties.Settings.Default.cam_po = new System.Drawing.Point((int)cam._pos.X, (int)cam._pos.Y);

            #endregion

            #region [Game Object] : Ballon, NameTag

            // ballon positioning
            if(Form_Ballon != null)
                Form_Ballon.Location = new System.Drawing.Point((int)(Mainform.Location.X + Properties.Settings.Default.Actor_x - cam._pos.X + Mainform.Width / 2) - 50 - Form_Ballon.Width/2,
                    (int)(Mainform.Location.Y + Properties.Settings.Default.Actor_y - cam._pos.Y + Mainform.Height / 2) - 115); // 아래 : 35

            // nametag positionong
            if (nametag != null)
            {
                nametag.Location = new System.Drawing.Point((int)(Mainform.Location.X + Properties.Settings.Default.Actor_x - cam._pos.X + Mainform.Width / 2) - 100,
                    (int)(Mainform.Location.Y + Properties.Settings.Default.Actor_y - cam._pos.Y + Mainform.Height / 2) - 95); // 아래 : 35
            }

            #endregion

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            // Sprite Batch
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        cam.get_transformation(GraphicsDevice));

            // before login
            if (!Properties.Settings.Default.LoginCheck)
                  spriteBatch.Draw(Layer1, new Rectangle(0, 0, Layer1.Width, Layer1.Height), Color.White);

            // loginged
            if (Properties.Settings.Default.LoginCheck)
            {
                // drawing Layer1
                spriteBatch.Draw(Layer1, new Vector2(65, 90), new Rectangle((int)vec_Layer1.X, (int)vec_Layer1.Y, Layer1.Width, Layer1.Height), Color.White, 0, Actor.center, 1.0f, SpriteEffects.None, 1);

                // drawing Layer2
                spriteBatch.Draw(Layer2, new Vector2(65, 90), new Rectangle((int)vec_Layer2.X, (int)vec_Layer2.Y, Layer1.Width, Layer1.Height), Color.White, 0, Actor.center, 1.0f, SpriteEffects.None, 0.1f);

                // drawing actor
                spriteBatch.Draw(Actor.sprite, Actor.position, new Rectangle(Actor_activepo.Y * Actor.sprite.Width / 4, Actor_activepo.X * Actor.sprite.Height / 4, Actor.sprite.Width / 4, Actor.sprite.Height / 4),
                    Color.White, Actor.rotation, Actor.center, 1.0f, SpriteEffects.None, 0.2f);

                // drawing other players
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].mapid == Properties.Settings.Default.MapId)
                    {
                        spriteBatch.Draw(Users[i].sprite, new Vector2(Users[i].actor_x, Users[i].actor_y), new Rectangle(Users[i].activepo.Y * Users[i].sprite.Width / 4, Users[i].activepo.X * Users[i].sprite.Height / 4,
                            Users[i].sprite.Width / 4, Users[i].sprite.Height / 4), Color.White, 0, Actor.center, 1.0f, SpriteEffects.None, 0.21f);
                    }
                }
            }

            // end Sprite Batch
            spriteBatch.End();
            base.Draw(gameTime);
        }


        //################################## TCP/IP NETWORK ###########################################
        #region Network / Receiver

        // Connect Complete => Set Events
        private void Connect_Complete(object sender, SocketAsyncEventArgs e)
        {
            m_ClientSocket = (Socket)sender;

            if (m_ClientSocket.Connected)
            {
                SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();

                byte[] szData = new byte[1024];
                _receiveArgs.SetBuffer(szData, 0, szData.Length);
                _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                m_ClientSocket.ReceiveAsync(_receiveArgs);
            }
        }

        // Packet Receive
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender;
            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {
                byte[] szData2 = e.Buffer;
                e.SetBuffer(szData2, 0, szData2.Length);
                string sDatas = Encoding.Unicode.GetString(szData2);
                String sData = sDatas;

                // # Packet Handling #
                try
                {
                    string[] co = { "&%*^%" };
                    string[] dcodata = sData.Split(co, StringSplitOptions.RemoveEmptyEntries);

                    sData = dcodata[0];
                    string pkcode = dcodata[1];

                    #region Login

                    if (pkcode.Equals("UD"))
                    {
                        string[] spear = { "," };
                        string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);

                        // # 계정 데이터 획득 #
                        Properties.Settings.Default.UserId = words[0];
                        Properties.Settings.Default.MapId = Int32.Parse(words[1]);
                        Properties.Settings.Default.Actor_x = Int32.Parse(words[2]);
                        Properties.Settings.Default.Actor_y = Int32.Parse(words[3]);
                        Properties.Settings.Default.Actor_sprite = words[4];

                        Properties.Settings.Default.LoginCheck = true;

                        // get interface and user data
                        GetActorData();

                        // exit login form
                        Mainform.Invoke(new MethodInvoker(
                          delegate()
                          {
                              Form_Login.Dispose();
                          }
                         )
                       );

                    }
                    else if (pkcode.Equals("LoginFail"))
                    {
                        ShowMessage("비밀번호를 확인해 주세요!");
                    }
                    else if (pkcode.Equals("LoginAlready"))
                    {
                        ShowMessage("현재 접속중인 계정입니다.");
                    }

                    #endregion

                    #region [Game Systems]

                    if (Properties.Settings.Default.LoginCheck)
                    {
                        // receive all chat

                        #region Chat systems

                        if (pkcode.Equals("AllChat"))
                        {
                            // update chat
                            Mainform.Invoke(new MethodInvoker(
                            delegate()
                            {
                                // auto initializ chat data
                                try
                                {
                                    if (Form_Chat.panel1.VerticalScroll.Maximum >= 17000) // overall 17,000 - renew
                                    {
                                        Form_Chat.Dispose();
                                        Form_Chat = new Forms.Chatting();
                                        Form_Chat.Show();
                                        Form_Chat.Location = new System.Drawing.Point(Mainform.Location.X + 3, Mainform.Location.Y + Mainform.Height - 131);
                                    }
                                }
                                catch { }

                                // show up a new chat
                                Chatwrite(sData);

                                // receive other player's a new ballon
                                string[] spear = { " : " };
                                string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);

                                if (!words[0].Equals(Properties.Settings.Default.UserId))
                                {
                                    for(int i=0; i<Users.Count; i++)
                                    {
                                        if(Users[i].username.Equals(words[0]))
                                        {
                                            if (Users[i].chatballon != null) Users[i].chatballon.Dispose();

                                            Users[i].chatballon = new Forms.ChatBallon(sData);
                                            Users[i].chatballon.Show();
                                        }
                                    }
                                }
                            }
                           )
                         );
                        }

                        #endregion

                        // drwaing other players

                        #region user map drawing

                        // drawing a new user
                        else if (pkcode.Equals("Mapres"))
                        {
                            string[] spear = { "," };
                            string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);
                            bool isalready = false;

                            // check a player is already exist
                            for (int i = 0; i < Users.Count; i++)
                                if (Users[i].username.Equals(words[0]))
                                    isalready = true;

                            // if a new player, drawing start
                            if (!isalready && !words[0].Equals(Properties.Settings.Default.UserId) && Int32.Parse(words[1]) == Properties.Settings.Default.MapId)
                            {
                                Mainform.Invoke(new MethodInvoker(
                                delegate()
                                {
                                    Classes.OtherUser user = new Classes.OtherUser(words[0], Int32.Parse(words[1]), Int32.Parse(words[2]), Int32.Parse(words[3]), Content.Load<Texture2D>("Sprites\\" + words[4]));
                                    Users.Add(user);
                                }
                             )
                           );
                            }

                            // get other player's position information - used dead reckoning
                            if (keyboardVal)
                            {
                                Send_Packet(
                                    Properties.Settings.Default.UserId + "," +
                                    Properties.Settings.Default.MapId + "," +
                                    Properties.Settings.Default.Actor_x + "," +
                                    Properties.Settings.Default.Actor_y + "," +
                                    Properties.Settings.Default.Actor_sprite + "," +
                                    Actor_activepo.X + "," +
                                    Actor_activepo.Y +
                                    ",&%*^%I+ZWQgko0LYrbcrYuZas5Q==&%*^%");
                            }
                        }

                        // remove a left user
                        else if (pkcode.Equals("MapExit"))
                        {
                            string[] spear = { "," };
                            string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);

                            if (Properties.Settings.Default.MapId == Int32.Parse(words[1]))
                            {
                                for (int i = 0; i < Users.Count; i++)
                                {
                                    if (Users[i].username.Equals(words[0]))
                                    {
                                        Mainform.Invoke(new MethodInvoker(
                                       delegate()
                                       {
                                        Users[i].clear();
                                        Users.Remove(Users[i]);
                                       }
                                      )
                                     );
                                        break;
                                    }
                                }
                            }
                        }

                        // update other players position
                        else if (pkcode.Equals("UserMove"))
                        {
                            string[] spear = { "," };
                            string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);
                            bool isexist = false;

                            for (int i = 0; i < Users.Count; i++)
                            {
                                if (Users[i].username.Equals(words[0]))
                                {
                                    Mainform.Invoke(new MethodInvoker(
                                       delegate()
                                       {
                                           // 현재 좌표의 갱신과 움직임 설정
                                           Users[i].actor_x = Int32.Parse(words[1]);
                                           Users[i].actor_y = Int32.Parse(words[2]);
                                           Users[i].UserMove(Int32.Parse(words[4]));
                                           isexist = true;
                                       }
                                      )
                                     );
                                    break;
                                }
                            }

                            if(isexist == false) // if got user information failed, re-draw a new player
                            {
                                // draw a new player
                                if (!words[0].Equals(Properties.Settings.Default.UserId) && Int32.Parse(words[1]) == Properties.Settings.Default.MapId)
                                {
                                    Mainform.Invoke(new MethodInvoker(
                                    delegate()
                                    {
                                        Classes.OtherUser user = new Classes.OtherUser(words[0], Int32.Parse(words[1]), Int32.Parse(words[2]), Int32.Parse(words[3]), Content.Load<Texture2D>("Sprites\\" + words[4]));
                                        Users.Add(user);
                                    }
                                 )
                               );
                                }
                            }
                        }

                         // User
                        else if (pkcode.Equals("MoveStop"))
                        {
                            string[] spear = { "," };
                            string[] words = sData.Split(spear, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < Users.Count; i++)
                            {
                                if (Users[i].username.Equals(words[0]))
                                {
                                    Mainform.Invoke(new MethodInvoker(
                                       delegate()
                                       {
                                           Users[i].UserStop();
                                       }
                                      )
                                     );
                                    break;
                                }
                            }
                        }

                        #endregion

                    }
                }
                catch { }

                #endregion

                // reactive chat form
                if (Properties.Settings.Default.chat_activated)
                {
                    Mainform.Invoke(new MethodInvoker(
                            delegate()
                            {
                                Form_Chat.Activate();
                            }
                           )
                         );
                }

                // flush buffer and get start async receive mode
                szData2 = new byte[szData2.Length];
                e.SetBuffer(szData2, 0, szData2.Length);
                ClientSocket.ReceiveAsync(e);
            }
        }
        
        // Packet Send
        private void Send_Packet(String Data)
        {
            if (m_ClientSocket.Connected)
            {
                try
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    byte[] szData = Encoding.Unicode.GetBytes(Data);
                    args.SetBuffer(szData, 0, szData.Length);
                    m_ClientSocket.SendAsync(args);
                }
                catch // if server is busy, retry
                {
                    System.Threading.Thread.Sleep(50);
                    Send_Packet(Data);
                }
            }
            else
            {
                MessageBox.Show("서버와의 접속이 끊어졌습니다.");
                Mainform.Dispose();
            }
        }

        #endregion

        // chat coloring
        private void Chatwrite(string sData)
        {
            Mainform.Invoke(new MethodInvoker(
             delegate()
             {
                 // join & exit message
                 if (!sData.Contains(":") && sData.Contains("접속하셨습니다") || sData.Contains("종료하셨습니다"))
                     Form_Chat.addChat(sData, System.Drawing.Color.Orange);
                 // notification message
                 else if(!sData.Contains(":") && sData.Contains("공지사항"))
                     Form_Chat.addChat(sData, System.Drawing.Color.Yellow);
                 // normal message
                 else
                     Form_Chat.addChat(sData, System.Drawing.Color.GhostWhite);

                 // scrollbar auto-drop down
                 Form_Chat.panel1.VerticalScroll.Value = Form_Chat.panel1.VerticalScroll.Maximum;
             }
           )
         );
        }

        // interface initialize
        private void GetActorData()
        {
            Mainform.Invoke(new MethodInvoker(
              delegate()
              {
                  // # interface initialze #       
                  // chat form
                  Form_Chat = new Forms.Chatting();
                  Form_Chat.Show();
                  Form_Chat.Location = new System.Drawing.Point(Mainform.Location.X + 3, Mainform.Location.Y + Mainform.Height - 131);

                  // name tag
                  nametag = new Forms.NameTag(Properties.Settings.Default.UserId);
                  nametag.Show();


                  // # character initialze #
                  // create an actor
                  Actor = new GameObject(Content.Load<Texture2D>("Sprites\\" + Properties.Settings.Default.Actor_sprite));
                  Actor.position = new Vector2(Properties.Settings.Default.Actor_x, Properties.Settings.Default.Actor_y);

                  // get map informations
                  MapChanged(Properties.Settings.Default.MapId, Properties.Settings.Default.Actor_x, Properties.Settings.Default.Actor_y);
              }
            )
         );
        }

        // reset map information
        private void MapChanged(int MapId, int x, int y)
        {
            // # exit map #
            for (int i = 0; i < Users.Count; i++) Users[i].clear();
            Users.Clear();
            MapDataRect.Clear();
            MapPotalRect.Clear();
            MapPotalData.Clear();

            // nofity a player left map
            Send_Packet(Properties.Settings.Default.UserId + "," + Properties.Settings.Default.MapId + ",&%*^%2N3/uHsMJwBgXhL2BFN//A==&%*^%");

            // change map id
            Properties.Settings.Default.MapId = MapId;
            Actor.position = new Vector2(x , y);
            Properties.Settings.Default.Actor_x = x;
            Properties.Settings.Default.Actor_y = y;

            // Get Map Graphic Data
            Layer1 = Content.Load<Texture2D>("Maps\\Map" + MapId);
            Layer2 = Content.Load<Texture2D>("Maps\\Map" + MapId + "_layer2");
            vec_Layer1 = new Vector2(0, 0);
            vec_Layer2 = new Vector2(0, 0);
            viewportRect = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            // # Camera Focusing #
            int cam_x = Mainform.Width / 2, cam_y = Mainform.Height / 2;

            if (Properties.Settings.Default.Actor_x + Mainform.Width / 2 >= Layer1.Width)
                cam_x = cam_x + Layer1.Width - Mainform.Width;
            else if (Properties.Settings.Default.Actor_x <= Mainform.Width / 2)
                cam_x = Mainform.Width / 2;
            else cam_x = cam_x + Properties.Settings.Default.Actor_x - cam_x;

            if (Properties.Settings.Default.Actor_y + Mainform.Height / 2 >= Layer1.Height)
                cam_y = cam_y + Layer1.Height - Mainform.Height;
            else if (Properties.Settings.Default.Actor_y <= Mainform.Height / 2)
                cam_y = Mainform.Height / 2;
            else cam_y = cam_y + Properties.Settings.Default.Actor_y - cam_y;

            cam._pos = new Vector2(cam_x, cam_y);        

            // # loading map data #
            var mapi = new System.Resources.ResourceManager("SoniaOnline.Resource",System.Reflection.Assembly.GetExecutingAssembly());
            var mapdatastring = mapi.GetString("Map" + MapId).ToString();

            List<string> words = mapdatastring.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            char[] Mapdata = words[0].ToCharArray();

            for (int i = 0; i < Mapdata.Length; i++)
            {
                if (Mapdata[i] == 'X')
                {
                    Rectangle Rect = new Rectangle(i % (Layer1.Width / 32) * 32 + 65, i / (Layer1.Width / 32) * 32 + 86, 32, 32);
                    MapDataRect.Add(Rect);
                }
            }

            // set map name and showup
            string[] sp = { "\n" };
            string[] mapn = words[1].Split(sp, StringSplitOptions.RemoveEmptyEntries);
            Properties.Settings.Default.MapName = mapn[1];
            ShowMessage(Properties.Settings.Default.MapName);

            // get map information
            for (int i = 2; i < words.Count; i++)
            {
                string[] spear = { ", " };
                string[] words2 = words[i].Split(spear, StringSplitOptions.RemoveEmptyEntries);

                Rectangle Rect = new Rectangle(Int32.Parse(words2[0]) * 32 + 65, Int32.Parse(words2[1]) * 32 + 86, Int32.Parse(words2[2]) * 32, Int32.Parse(words2[3]) * 32);
                MapPotalRect.Add(Rect);
                MapPotalData.Add(words2[4] + "," + words2[5] + "," + words2[6] + ",");
            }

            // # request map information #
            Send_Packet(Properties.Settings.Default.UserId + "," + Properties.Settings.Default.MapId + "," + Properties.Settings.Default.Actor_x + "," + Properties.Settings.Default.Actor_y + ",&%*^%RCi/3x0D2fMSYZaFCrR2MA==&%*^%");
        }


        // actor & sceen moving
        private void ActorMoving(int position)
        {
            // active mainform
            Mainform.Activate();
            Properties.Settings.Default.chat_activated = false;

            int speed = 3;
            Actor_activepo = new System.Drawing.Point(position, Actor_activepo.Y);

            if (MovingStack >= 6)
            {
                if (Actor_activepo.Y == 0)
                    Actor_activepo = new System.Drawing.Point(position, 1);
                else if (Actor_activepo.Y == 1)
                    Actor_activepo = new System.Drawing.Point(position, 2);
                else if (Actor_activepo.Y == 2)
                    Actor_activepo = new System.Drawing.Point(position, 3);
                else if (Actor_activepo.Y == 3)
                    Actor_activepo = new System.Drawing.Point(position, 0);

                MovingStack = 0;
            }
            MovingStack++;

            // #  change actor's position  #
            #region actor positioning & cam positioning

            // move left
            if (position == 1 && BoundsInspect(position))
            {
                if (Actor.position.X > Mainform.Width / 2 && Actor.position.X <= Layer1.Width - Mainform.Width / 2 && cam._pos.X - Mainform.Width / 2 >= 0)
                    cam._pos = new Vector2(cam._pos.X - speed, cam._pos.Y);

                if (Actor.position.X >= 60)
                {
                    Actor.position.X -= speed;
                    Properties.Settings.Default.Actor_x -= speed; // 실제 액터의 위치
                }
            }

            // move right
            else if (position == 2 && BoundsInspect(position))
            {
                if (Actor.position.X > Mainform.Width / 2 && cam._pos.X + Mainform.Width / 2 <= Layer1.Width)
                    cam._pos = new Vector2(cam._pos.X + speed, cam._pos.Y);

                if (Actor.position.X <= Layer1.Width + 30)
                {
                    Actor.position.X += speed;
                    Properties.Settings.Default.Actor_x += speed; // 실제 액터의 위치
                }
            }

            // move down
            else if (position == 0 && BoundsInspect(position))
            {
                if (Actor.position.Y > Mainform.Height / 2 && cam._pos.Y + Mainform.Height / 2 <= Layer1.Height)
                    cam._pos = new Vector2(cam._pos.X, cam._pos.Y + speed);

                if (Actor.position.Y <= Layer1.Height + 30)
                {
                    Actor.position.Y += speed;
                    Properties.Settings.Default.Actor_y += speed; // 실제 액터의 위치
                }
            }

            // move up
            if (position == 3 && BoundsInspect(position))
            {
                if (Actor.position.Y > Mainform.Height / 2 && Actor.position.Y <= Layer1.Height - Mainform.Height / 2 && cam._pos.Y - Mainform.Height / 2 >= 0)
                    cam._pos = new Vector2(cam._pos.X, cam._pos.Y - speed);

                if (Actor.position.Y >= 90)
                {
                    Actor.position.Y -= speed;
                    Properties.Settings.Default.Actor_y -= speed; // 실제 액터의 위치
                }
            }

            if (Potal() != -1)
            {
                string[] spear = { "," };
                string[] words = MapPotalData[Potal()].Split(spear, StringSplitOptions.RemoveEmptyEntries);

                MapChanged(Int32.Parse(words[0]), Int32.Parse(words[1])*32 + 70, Int32.Parse(words[2])*32 + 70);
            }

            #endregion


            // # send data to server #

            if (previouspo != position || keyboardVal == false)
            {
                Send_Packet(
                    Properties.Settings.Default.UserId + "," +
                    Properties.Settings.Default.MapId + "," +
                    Properties.Settings.Default.Actor_x + "," +
                    Properties.Settings.Default.Actor_y + "," +
                    Properties.Settings.Default.Actor_sprite + "," +
                    Actor_activepo.X + "," +
                    Actor_activepo.Y +
                    ",&%*^%I+ZWQgko0LYrbcrYuZas5Q==&%*^%");

                keyboardVal = true;
            }

            previouspo = position;
        }

        // prevent acotor over moving map
        private bool BoundsInspect(int position)
        {
            bool isgo = true;

            Rectangle Rect_actor = new Rectangle((int)(Actor.position.X) + 10, (int)(Actor.position.Y)+15, Actor.sprite.Width/4 - 15, Actor.sprite.Height/4 - 20);

            if (position == 1) Rect_actor.X -= 3;
            else if (position == 2) Rect_actor.X += 3;
            else if (position == 0) Rect_actor.Y += 3;
            else if (position == 3) Rect_actor.Y -= 3;

            for(int i=0; i<MapDataRect.Count; i++)
            {
                if (MapDataRect[i].Intersects(Rect_actor))
                    isgo = false;
            }
            return isgo;
        }

        // check actor is over a potal
        private int Potal()
        {
            int index = -1;

            Rectangle Rect_actor = new Rectangle((int)(Actor.position.X) + 10, (int)(Actor.position.Y) + 15, Actor.sprite.Width / 4 - 15, Actor.sprite.Height / 4 - 20);

            for (int i = 0; i < MapPotalRect.Count; i++)
            {
                if (MapPotalRect[i].Intersects(Rect_actor))
                    index = i;
            }
            return index;
        }

        // showing up user moving
        private void UserMoving(int index)
        {
            if (Users[index].position != -1)
            {
                Users[index].activepo = new System.Drawing.Point(Users[index].position, Users[index].activepo.Y);

                // move images
                if (BoundsInspect_User(index))
                {
                    if (Users[index].position == 0)
                        Users[index].actor_y += 3;
                    else if (Users[index].position == 1)
                        Users[index].actor_x -= 3;
                    else if (Users[index].position == 2)
                        Users[index].actor_x += 3;
                    else if (Users[index].position == 3)
                        Users[index].actor_y -= 3;
                }

                // renew images
                if (Users[index].MovingStack >= 6)
                {
                    if (Users[index].activepo.Y == 0)
                        Users[index].activepo = new System.Drawing.Point(Users[index].position, 1);
                    else if (Users[index].activepo.Y == 1)
                        Users[index].activepo = new System.Drawing.Point(Users[index].position, 2);
                    else if (Users[index].activepo.Y == 2)
                        Users[index].activepo = new System.Drawing.Point(Users[index].position, 3);
                    else if (Users[index].activepo.Y == 3)
                        Users[index].activepo = new System.Drawing.Point(Users[index].position, 0);

                    Users[index].MovingStack = 0;
                }
                Users[index].MovingStack++;
            }
        }

        // map bounding
        private bool BoundsInspect_User(int index)
        {
            bool isgo = true;

            Rectangle Rect_actor = new Rectangle((int)(Users[index].actor_x) + 10, (int)(Users[index].actor_y) + 15, Users[index].sprite.Width / 4 - 15, Users[index].sprite.Height / 4 - 20);

            for (int i = 0; i < MapDataRect.Count; i++)
            {
                if (MapDataRect[i].Intersects(Rect_actor))
                    isgo = false;
            }
            return isgo;
        }


        #region Systems

        // Show Message : MessageForm
        private void ShowMessage(string text)
        {
            Mainform.Invoke(new MethodInvoker(
               delegate()
               {
                   Forms.ShowMessage mf = new Forms.ShowMessage();
                   mf.MinimumSize = new System.Drawing.Size(150, 25);
                   mf.Size = new System.Drawing.Size(Mainform.Width - 6, 25);
                   mf.MakeMessage(text);
                   mf.Opacity = 0;
                   mf.Show();
                   mf.Location = new System.Drawing.Point(Mainform.Left + 3, Mainform.Top + Mainform.Height / 4 + 20);
                   mf.label1.Location = new System.Drawing.Point(mf.Width / 2 - mf.label1.Text.Length * 6, 6);
               }
             )
          );
        }

        private void Mainform_Move(object sender, EventArgs e)
        {
            // save mainform's position
            Properties.Settings.Default.Point_Mainform = new System.Drawing.Point(Mainform.Location.X, Mainform.Location.Y);

            // login form
            if(Form_Login != null)
                Form_Login.Location = new System.Drawing.Point(Mainform.Location.X + 280, Mainform.Location.Y + 300);

            // chat form
            if(Form_Chat != null)
                Form_Chat.Location = new System.Drawing.Point(Mainform.Location.X + 3, Mainform.Location.Y + Mainform.Height - 131);

            // ballon positioning
            if (Form_Ballon != null)
                Form_Ballon.Location = new System.Drawing.Point((int)(Mainform.Location.X + Properties.Settings.Default.Actor_x - cam._pos.X + Mainform.Width / 2) - 50 - Form_Ballon.Width / 2,
                    (int)(Mainform.Location.Y + Properties.Settings.Default.Actor_y - cam._pos.Y + Mainform.Height / 2) - 115); // 아래 : 35

            // nametag positioning
            if(nametag != null)
                nametag.Location = new System.Drawing.Point((int)(Mainform.Location.X + Properties.Settings.Default.Actor_x - cam._pos.X + Mainform.Width / 2) - 100,
                   (int)(Mainform.Location.Y + Properties.Settings.Default.Actor_y - cam._pos.Y + Mainform.Height / 2) - 95);
        }

        // closed form_
        private void Mainform_FormClosing(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LoginCheck)
            {
                // save user data
                Send_Packet(
                    Properties.Settings.Default.UserId + "," +
                    Properties.Settings.Default.MapId + "," +
                    Properties.Settings.Default.Actor_x + "," +
                    Properties.Settings.Default.Actor_y + "," +
                    Properties.Settings.Default.Actor_sprite +
                    ",&%*^%D3RuaxR52yjZW9QM9isz/Q==&%*^%");

                // # left map #
                Send_Packet(Properties.Settings.Default.UserId + "," + Properties.Settings.Default.MapId + ",&%*^%2N3/uHsMJwBgXhL2BFN//A==&%*^%");

                // notification message
                Send_Packet("<" + Properties.Settings.Default.UserId + "님께서 접속을 종료하셨습니다>" + "&%*^%Q1SXiLEURCvizN+cfayrWsbtLahNidB/LQpJEauR26Q=&%*^%");
            }

            // exit to server
            if (m_ClientSocket.Connected)
            {
                m_ClientSocket.Disconnect(false);
                m_ClientSocket.Dispose();
            }
        }

        #endregion

    }
}
