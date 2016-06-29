using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Core {
    class MainClass {
        public static void Main(string[] args) {
            var bot = new IRCBot();
            bot.Run();
        }
    }
}
