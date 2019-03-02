using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using gui;

namespace console_agario
{
    class BlankBox : Box
    {
        public BlankBox(Window window, BoxSize size, char[] framebuffer) : base(window, size, framebuffer)
        {

        }

        public override void Init()
        {

        }
    }
    public class MainWin : Window
    {
        #region PINVOKE
        [DllImport("Kernel32.dll")]
        internal static extern IntPtr CreateConsoleScreenBuffer(
     UInt32 dwDesiredAccess,
     UInt32 dwShareMode,
     IntPtr secutiryAttributes,
     UInt32 flags,
     IntPtr screenBufferData
     );
        [DllImport("Kernel32.dll")]
        internal static extern void SetConsoleActiveScreenBuffer(IntPtr handle);
        [DllImport("kernel32.dll")]
        internal static unsafe extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, byte* lpCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };
        #endregion PINVOKE
        public MainWin(IntPtr handle) : base(Console.WindowWidth, Console.WindowHeight, false)
        {
            _consoleFileHandle = handle;
            _writeCOORD = new COORD(0, 0);
            Init();
        }
        public void Init()
        {
            SetConsoleActiveScreenBuffer(_consoleFileHandle);
        }
        public void Write()
        {
            Write(_framebuffer, 0, Characters);
        }
        protected unsafe override void Write(char[] buffer, int offset, int count)
        {
            byte* buf = stackalloc byte[count];
            fixed (char* b = buffer)
            {
                GetBytes(b, buf, count);
                uint f;
                WriteConsoleOutputCharacter(_consoleFileHandle, buf, (uint)count, _writeCOORD, out f);
            }

        }
        private unsafe void GetBytes(char* src, byte* dest, int count)
        {
            for (int i = 0; i < count; i++)
            {
                dest[i] = (byte)src[i];
            }
        }

        private IntPtr _consoleFileHandle;
        private COORD _writeCOORD;
    }
    class Game
    {
        public Game(int width, int height)
        {
            //o_O
            Console.WindowHeight = height;
            Console.WindowWidth = width;

            MainWin = new MainWin(MainWin.CreateConsoleScreenBuffer(0x80000000U | 0x40000000U, 0, (IntPtr)(0), 0x00000001, (IntPtr)(0)));

            Screen = MainWin._framebuffer;

            Window = new BlankBox(MainWin, MainWin.Max, Screen);
            Window.OffsetX = 35;
            Window.OffsetX = 35;

            Map = new Map(MapSizeX);

            //bot = new Player("Bot 1", 5);
            Reset();
            
            //Start threads
            StartDrawThread();
            //Now on the main thread
            //StartGameUpdates();
            StartUserInputThread();
        }

        public static MainWin MainWin;
        public Box Window;
        public Camera Camera;
        public Map Map;

        public char[] Screen;

        private DateTime game_a;
        private DateTime game_b;
        private DateTime draw_a;
        private DateTime draw_b;

        public UserPlayer user;
        public Player[] players;

        public const int MapSizeX = 100;
        public const int MapSizeY = 100;

        public void Reset()
        {
            players = new Player[3];
            user = new UserPlayer(MainWin, 12);
            user.PositionX = 10;
            user.PositionY = 10;
            players[0] = user;
            for (int i = 1; i < players.Length; i++)
            {
                players[i] = new Player($"Bot {i}", 10, 20, 20);
            }
            Camera = new Camera(Window, user, Map, Screen);
            //UpdateCamera(Window, user);
        }

        public void StartUserInputThread()
        {
            new Thread(() => UserInputThread()).Start();
        }

        public void StartDrawThread()
        {
            new Thread(() => DrawThread()).Start();
        }

        public void StartGameUpdates()
        {
            game_a = DateTime.Now;
            game_b = DateTime.Now;
            new Thread(() => GameUpdateThread());

        }

        public void GameUpdateThread()
        {
            while (true)
            {
                Update();
                GamePause();
            }
        }

        private void DrawThread()
        {
            while (true)
            {
                DrawPause();
                Draw();
            }
        }

        private void Draw()
        {
            //Clear board
            Window.Clear();
            //Enumerate orbs
            float scale = user.GetScale();
            //bot.DrawTo(Window, scale, 'b');
            Player cur;
            for (int i = 1; i < players.Length; i++)
            {                
                if (!(cur = players[i]).disposed)
                    cur.DrawTo(Window, Camera, 'b');
            }
            user.DrawTo(Window, Camera, 'u');
            Window.Set(user.Score.ToString(), 0, 0);
            Window.Set(user.PositionX.ToString(), 0, 1);
            Window.Set(user.PositionY.ToString(), 4, 1);
            Window.Set(Camera.PositionX.ToString(), 0, 2);
            Window.Set(Camera.PositionY.ToString(), 4, 2);
            //Raster
            //Obsolete, use a console screen buffer
            //MainWin.Draw();
            MainWin.Write();
        }
        
        //private void UpdateCamera(BoxSize screen, UserPlayer player)
        //{
        //    float scale = player.GetScale();
        //    //The size should be a constant half of the screen
        //    //The diameter
        //    //int size = (int)Math.Round(screen.GetScaleX(0.5f) / scale);
        //    //int size = player.Score;
        //    //int radius = size / 2;
        //    int size = player.Score * 2;
        //    int radius = player.Score;
        //    int posX = player.PositionX, posY = player.PositionY;
        //    int maxX = screen.SizeX, maxY = screen.SizeY;
        //    int cameraX = screen.OffsetX, cameraY = screen.OffsetY;
        //    int mapMaxX = MapSizeX, mapMaxY = MapSizeY;
        //    int cameraCenterX = posX + size, cameraCenterY = posY + size;
        //    int cameraNewX = posX - radius, cameraNewY = posY - radius;
        //    int cameraOtherNewX = posX + maxX, cameraOtherNewY = posY + maxY;
                        
        //    //Free floating X axis
        //    if (cameraNewX > 0 && cameraOtherNewX < mapMaxX)
        //    {
        //        screen.OffsetX = (int)Math.Round(cameraNewX * scale);
        //    }
        //    //Top-left (Top left working)
        //    else
        //        if (cameraNewX <= 0)
        //    {
        //        screen.OffsetX = 0;
        //    }
        //    //Lower-left (Not working)
        //    /*
        //    else
        //        if (cameraCenterX >= mapMaxX - size)
        //    {
        //        screen.OffsetX = mapMaxX - maxX;
        //    }
        //    */

        //    //Free floating Y axis
        //    if (cameraNewY > 0 && cameraOtherNewY < mapMaxY)
        //    {
        //        screen.OffsetY = (int)Math.Round(cameraNewY * scale);
        //    }
        //    //Top-left
        //    else
        //        if (cameraNewY <= 0)
        //    {
        //        screen.OffsetY = 0;
        //    }

        //    /*
        //    //Y axis
        //    if (posY + size < mapMaxY - maxY)
        //    {
        //        screen.OffsetY = posY - size;
        //    }
        //    else
        //    {
        //        //Near Y bounds in positive
        //        screen.OffsetY = mapMaxY - maxY;
        //    }   
        //    //X axis
        //    if (posX + size < mapMaxX - maxX)
        //    {
        //        screen.OffsetX = posX - size;
        //    }
        //    else
        //    {
        //        //Near X bounds in positive
        //        screen.OffsetX = mapMaxX - maxX;
        //    }
        //    */
        //}

        private void DrawPause()
        {
            draw_a = DateTime.Now;

            var worktime = draw_a - draw_b;

            if (worktime.Milliseconds < 50)
            {
                TimeSpan deltaMS = new TimeSpan(0,0,0,0,50 - worktime.Milliseconds);
                Thread.Sleep(deltaMS);
            }

            draw_b = DateTime.Now;
        }

        private void GamePause()
        {
            game_a = DateTime.Now;
            
            var worktime = game_a - game_b;
            if (worktime.Milliseconds < 50)
            {
                TimeSpan deltaMS = new TimeSpan(0,0,0,0,50 - worktime.Milliseconds);
                Thread.Sleep(deltaMS);
            }

            game_b = DateTime.Now;
        }

        private void UserInputThread()
        {
            while (true)
            {
                lastKey = Console.ReadKey(true);
            }
        }

        protected ConsoleKeyInfo lastKey;
        
        public bool DoCollisionDetection()
        {
            int count = players.Length;
            int count0 = count - 1;
            bool res = false;
            Player P0, P1;
            for (int i = 0; i < count0; i++)
            {
                P0 = players[i];
                for (int b = i + 1; b < count; b++)
                {
                    P1 = players[b];
                    if (P0.Collides(P1))
                    {
                        if (res = (P0 > P1 || P0 < P1))
                        {
                            if (P0 > P1)
                            {
                                P0.Consume(P1);
                            }
                            else 
                            if (P1 > P0)
                            {
                                P1.Consume(P0);
                            }
                        }
                    }
                }
            }
            return res;
        }

        //Game tick, not for drawing
        public void Update()
        {
            if (user.HandleKeyPress(lastKey) || DoCollisionDetection())
            {
                //If any movement happened, update the camera position (also do when size increase)
                //UpdateCamera(Window, user);
                Camera.UpdateView();
                if (user.disposed)
                {
                    Reset();
                }
                for (int i = 1; i < players.Length; i++)
                {
                    if (players[i].disposed)
                    players[i] = new Player($"Bot {i}", 15, 20, 20);
                }
            }
            lastKey = new ConsoleKeyInfo();
        }
    }

    class SpeedQueue<T>
    {
        T[] nodes;
        int current;
        int emptySpot;
        public int Current { get { return current; } set { current = value; } }
        public SpeedQueue(int size)
        {
            nodes = new T[size];
            this.current = 0;
            this.emptySpot = 0;
        }

        public void Enqueue(T value)
        {
            nodes[emptySpot] = value;
            emptySpot++;
            if (emptySpot >= nodes.Length)
            {
                emptySpot = 0;
            }
        }
        public T Dequeue()
        {
            int ret = current;
            current++;
            if (current >= nodes.Length)
            {
                current = 0;
            }
            return nodes[ret];
        }
    }
}
