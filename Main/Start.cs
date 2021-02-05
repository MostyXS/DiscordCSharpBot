using System;
using System.Text;

namespace LSSKeeper.Main
{
    class Start
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var bot = new Core();
            bot.MainAsync().GetAwaiter().GetResult();
        }
    }
}
