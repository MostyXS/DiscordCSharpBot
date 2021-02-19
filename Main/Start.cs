using Valera.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Valera.Main
{
    class Start
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var bot = new Core();
            var tasks = new List<Task>
            {
                bot.MainAsync(),
                BirthdayNotifier.
            }
        }
    }
}
