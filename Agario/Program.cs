using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gui;

namespace Agario
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(40, 40);
            game.GameUpdateThread();
        }
    }
}
