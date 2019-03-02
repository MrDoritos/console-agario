using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gui;

namespace console_agario
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(100, 100, 500);
            game.GameUpdateThread();
        }
    }
}
