using System;
using System.Text;
using System.Threading.Tasks;

namespace LOSCKeeper
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
