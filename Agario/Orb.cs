using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gui;

namespace Agario
{
    class Orb
    {
        public int PositionX { get; internal set; }

        public int PositionY { get; internal set; }

        public Orb(int score)
        {
            Score = score;
        }

        /// <summary>
        /// Also the diameter
        /// </summary>
        public int Score { get; protected set; }

        internal bool disposed;

        public void Delete()
        {
            Score = 0;
            //Avoid any functions doing operations on us
            disposed = true;
        }
    }
}
