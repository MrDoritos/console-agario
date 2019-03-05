using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using gui;


namespace console_agario
{
    class Program
    {
        //[DllImport("olcConsoleGameEngine.dll")]
        //extern class olcConsoleGameEngine olcEngine;

        static void Main(string[] args)
        {
            
            Game game = new Game(100, 100, 500);
            game.GameUpdateThread();
        }
    }
}
