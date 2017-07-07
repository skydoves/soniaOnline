using System;
using System.Windows.Forms;
using System.Threading;  

namespace SoniaOnline
{
    static class Program
    {
        public static commonlib.Info lib;
        public static string padd = "&%*^%";
        public static string Chatstring = "";
        //public static string DBInfo;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool bnew;

            using (Game1 game = new Game1())
            {
                lib = new commonlib.Info(Properties.Settings.Default.Info_k);
                game.Run();
            }

            Mutex mutex = new Mutex(true, "MutexName", out bnew);
            if (bnew)
            {
                lib = new commonlib.Info(Properties.Settings.Default.Info_k);
                //DBInfo = lib.DBInfo();

                using (Game1 game = new Game1())
                {
                    game.Run();
                }
            }
            else
            {
                MessageBox.Show("게임이 이미 실행중입니다.");
                Application.Exit();
            }  
        }
    }
}

