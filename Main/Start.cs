using Volodya.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Volodya.Main
{
    class Start
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var bot = new Core();
            var bdNotifier = new BirthdayNotifier();
            bot.OnInitialize += async () => await bot.AddModuleAsync(bdNotifier);
            Task.WaitAll(bot.MainAsync(), bdNotifier.RunAsync());
        }
    }
}
