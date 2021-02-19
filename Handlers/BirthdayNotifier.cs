using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Valera.Handlers
{
    [JsonObject(MemberSerialization.OptIn)]
    class BirthdayNotifier
    {
        DiscordChannel bdChannel;
        const double interval15Minutes = 60*15*1000; //milliseconds to half of an hour


        #region Config Params
        Dictionary<ulong, BirthdayInfo> membersBirthdays = new Dictionary<ulong, BirthdayInfo>();

        #endregion
        #region Config Management
        private async Task SaveAsync()
        {
            


        }
        public async Task RunAsync()
        {



            while(true)
            {

            }
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        struct BirthdayInfo
        {
            string name;
            int lastYearNotified;
            int birthDay;

        }

    }
}
