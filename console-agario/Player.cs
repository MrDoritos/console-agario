using gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace console_agario
{
    class Player : Orb
    {
        public Player(string name, int score, int posX, int posY) : base(score)
        {
            PositionX = posX;
            PositionY = posY;
            Name = name;
            //Score = score;
        }

        public void Consume(Orb orb)
        {
            Score += orb.Score;
            orb.Delete();
        }

        public virtual void Update()
        {

        }
        
        public bool OutOfBounds(int x, int y)
        {
            return !(x >= 0 && y >= 0 && Game.MapSizeX > x + Score && Game.MapSizeY > y + Score);
        }
        public static bool operator> (Player player1, Player player2)
        {
            return player1.Score > player2.Score;
        }
        public static bool operator< (Player player1, Player player2)
        {
            return player1.Score < player2.Score;
        }
       
        public bool Collides(Player player)
        {
            int R0 = this.Score / 2;
            int R1 = player.Score / 2;
            int X0 = this.PositionX;
            int X1 = player.PositionX;
            int Y0 = this.PositionY;
            int Y1 = player.PositionY;
            int F0 = ((R0 - R1) * (R0 - R1));
            int F1 = ((X0 - X1) * (X0 - X1)) + ((Y0 - Y1) * (Y0 - Y1));
            int F2 = ((R0 + R1) * (R0 + R1));
            return (F0 <= F1 && F1 <= F2);
        }

        /// <summary>
        /// Draws the orb to the screen (prolly doesn't work)
        /// </summary>
        /// <param name="screen">Framebuffer</param>
        public void DrawTo(Box screen, Camera camera, char border)
        {

            float scale = camera.Scale;
            //Our score is the diameter / scale
            double diameter = Diameter;
            double radius = Radius;
            double step = ((diameter * scale) / (5.6d * 2 * Math.PI));
            double theta = 0.0d;
            double doublePi = 2 * Math.PI;
            //Here's where we need to do some work (fuck)
            int centerX = (int)Math.Round((PositionX + radius)), centerY = (int)Math.Round((PositionY + radius));

            int x, y;
            int boundsX = screen.SizeX/* + screen.OffsetX*/, boundsY = screen.SizeY/* + screen.OffsetY*/;

            for (theta = 0.0d; theta < doublePi; theta += step)
            {
                x = (int)Math.Round(centerX + radius * Math.Cos(theta)) - camera.PositionX;
                y = (int)Math.Round(centerY - radius * Math.Sin(theta)) - camera.PositionY;
                if (x < boundsX && y < boundsY && y >= 0 && x >= 0)
                {
                    screen._framebuffer[screen.Get(x, y)] = border;
                }
            }

        }        

        /// <summary>
        /// This won't be implemented yet
        /// </summary>
        public enum FillPattern
        {
            CheckerBoard,
            DiagonalCheckerBoard,
            /// <summary>
            /// Degrees from complete vertical position
            /// </summary>
            DegreeStripe0,
            /// <summary>
            /// Degrees from complete vertical position
            /// </summary>
            DegreeStripe45,
            /// <summary>
            /// Degrees from complete vertical position
            /// </summary>
            DegreeStripe90,

        }

        public string Name { get; protected set; }
        //Matherooos
        public float Radius { get => Diameter / 2; }
        public float Diameter { get => Score / SingleMath.PI; }
        public float CenterX { get => PositionX + Radius; }
        public int CenterXAsInt { get => (int)Math.Round(CenterX); }
        public float CenterY { get => PositionY + Radius; }
        public int CenterYAsInt { get => (int)Math.Round(CenterY); }
        public float MaxY { get => PositionY + Diameter; }
        public int MaxYAsInt { get => (int)Math.Round(MaxY); }
        public float MinY { get => PositionY; }
        public int MinYAsInt { get => (int)Math.Round(MinY); }
        public float MinX { get => PositionX; }
        public int MinXAsInt { get => (int)Math.Round(MinX); }
        public float MaxX { get => PositionX + Diameter; }
        public int MaxXAsInt { get => (int)Math.Round(MaxX); }

    }
    
    class UserPlayer : Player
    {
        public UserPlayer(Window win, int score) : base("You", score,0,0)
        {
            _max = win.Max;
        }
        //What
        private BoxSize _max;
        public float GetScale()
        {
            //Our orb should occupy half the screen, up or down
            float x, y, min;
            x = _max.GetFloatScaleX(0.5f);
            y = _max.GetFloatScaleY(0.5f);
            //Take the lesser value
            min = x > y ? y : x;
            return min / Score;
        }
        //don't ask why I made this one better
        public double GetScaleAsDouble()
        {
            double x = _max.GetDoubleScaleX(0.5d), y = _max.GetDoubleScaleY(0.5d), min;
            min = x > y ? y : x;
            return min / Score;
        }
        public bool HandleKeyPress(ConsoleKeyInfo key)
        {
            int newPos;
            switch (key.Key)
            {
                case ConsoleKey.A:
                    newPos = PositionX - 1;
                    if (!OutOfBounds(newPos, PositionY))
                    {
                        PositionX = newPos;
                        return true;
                    }
                    break;
                case ConsoleKey.D:
                    newPos = PositionX + 1;
                    if (!OutOfBounds(newPos, PositionY))
                    {
                        PositionX = newPos;
                        return true;
                    }
                    break;
                case ConsoleKey.W:
                    newPos = PositionY - 1;
                    if (!OutOfBounds(PositionX, newPos))
                    {
                        PositionY = newPos;
                        return true;
                    }
                    break;
                case ConsoleKey.S:
                    newPos = PositionY + 1;
                    if (!OutOfBounds(PositionX, newPos))
                    {
                        PositionY = newPos;
                        return true;
                    }
                    break;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
        }
    }
    class Map
    {
        public int Size { get; }
        public Map(int size)
        {
            Size = size;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        int X;
        int Y;
    }
    class Camera
    {
        public Player Player { get; }
        public Map Map { get; }
        public char[] Framebuffer { get; }
        public Box Screen { get; }
        public Camera(Box screen, Player player, Map map, char[] framebuffer)
        {
            Player = player;
            Map = map;
            Framebuffer = framebuffer;
            Screen = screen;

            UpdateView();
        }
        public int PositionX { get => Screen.OffsetX; }
        public int PositionY { get => Screen.OffsetY; }
        public float AbsoluteMaxX { get => Player.MaxX + Player.Radius; }
        public float AbsoluteMinX { get => Player.MinX - Player.Radius; }
        public float AbsoluteMaxY { get => Player.MaxY + Player.Radius; }
        public float AbsoluteMinY { get => Player.MinY - Player.Radius; }
        public float Scale { get => SizeX / Screen.SizeX; }
        //Assuming the player's location is not negative, this should work
        public float MaxX
        {
            get
            {
                float absBottomX = AbsoluteMaxX;
                float mapBottomX = Map.Size;
                return absBottomX > mapBottomX ? mapBottomX : absBottomX;
            }
        }
        public float MaxY
        {
            get
            {
                float absRightY = AbsoluteMaxY;
                float mapRightY = Map.Size;
                return absRightY > mapRightY ? mapRightY : absRightY;
            }
        }
        public float MinX
        {
            get
            {
                float absTopX = AbsoluteMinX;
                return absTopX >= 0 ? absTopX : 0;
            }
        }
        public int MinXAsInt { get => (int)Math.Round(MinX); }
        public float MinY
        {
            get
            {
                float absLeftY = AbsoluteMinY;
                return absLeftY >= 0 ? absLeftY : 0;
            }
        }
        public int MinYAsInt { get => (int)Math.Round(MinY); }
        public float SizeX
        {
            get
            {
                return Player.Radius * 4;
            }
        }
        public int SizeXAsInt { get => (int)Math.Round(SizeX); }
        public float SizeY
        {
            get
            {
                return Player.Radius * 4;
            }
        }
        public int SizeYAsInt { get => (int)Math.Round(SizeY); }
        public void UpdateView()
        {            
            if (AbsoluteMaxX < Map.Size - Screen.SizeX)
            {
                Screen.OffsetX = MinXAsInt;
            }
            if (AbsoluteMaxY < Map.Size - Screen.SizeY)
            {
                Screen.OffsetY = MinYAsInt;
            }
        }
        public unsafe char[] GetFrame()
        {
            throw new NotImplementedException();
        }
    }
    public static class SingleMath
    {
        public const float PI = (float)Math.PI;
    }
}
