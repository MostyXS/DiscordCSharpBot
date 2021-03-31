using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using LSSKeeper.Commands;
using LSSKeeper.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volodya.Extensions;

namespace Volodya.Modules
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BirthdayNotifier : KeeperModule
    {
        private DiscordChannel _birthdayChannel;

        [JsonProperty]
        Dictionary<ulong, BirthdayInfo> membersBirthdays = new Dictionary<ulong, BirthdayInfo>();
        [JsonProperty]
        ulong? bdChannelId;
        [JsonProperty]
        List<string> notifyAdditionals = new List<string>();

        #region Module Methods
        public async Task RunAsync()
        {
            while (true)
            {

                ulong? notifiedMember = null;
                foreach(var member in membersBirthdays)
                {
                    var info = member.Value;
                    var currentDate = DateTime.Now;
                    if (IsBirthdayToday(info, currentDate) && currentDate.TimeOfDay.TotalHours > 10)
                    {
                        var user = await DefaultGuild.GetMemberAsync(member.Key);
                        var age = currentDate.Year - info.BirthYear;
                        if (_birthdayChannel != null)
                        {
                            await _birthdayChannel.SendMessageAsync($"У {info.Name} {user.Mention} сегодня день рождения," +
                                $" ему исполнилось {age.ToRusAge()}, поздравляем {GetRandomAdditional()} " +
                                $":birthday:");
                            notifiedMember = member.Key;
                        }
                    }
                }
                if(notifiedMember != null)
                {
                    var newInfo = membersBirthdays[(ulong)notifiedMember];
                    newInfo.LastYearNotified = DateTime.Now.Year;
                    membersBirthdays[(ulong)notifiedMember] = newInfo;
                    await SaveAsync();
                }
                await Task.Delay(TimeSpan.FromMinutes(.25d));
            }
        }

        public override async Task InitializeAsync(DiscordClient c, DiscordGuild guild)
        {
            await base.InitializeAsync(c, guild);
        }
        public override void RegisterCommands(CommandsNextExtension commands)
        {
            BirthdayNotifierCommands.BNotifier = this;
            CommandsType = typeof(BirthdayNotifierCommands);
            base.RegisterCommands(commands);
        }
        protected override async Task InitializeConfigAsync()
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(ConfigNames.BIRTHDNOTY);
                var json = JsonConvert.DeserializeObject<BirthdayNotifier>(jsonString);
                bdChannelId = json.bdChannelId;
                if (bdChannelId != null)
                    _birthdayChannel = DefaultGuild.GetChannel((ulong)json.bdChannelId);
                notifyAdditionals = json.notifyAdditionals;
                membersBirthdays = json.membersBirthdays;

            }
            catch (Exception e)
            {
                Console.WriteLine("Не удалось инициализировать конфиг оповещений дней рождения: " + e.Message);
            }
        }

        protected override async Task SaveAsync()
        {
            var jsonString = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync(ConfigNames.BIRTHDNOTY, jsonString);
        }

        #endregion

        #region Commands

        public async Task SetBirthdayChannel(DiscordChannel channel)
        {
            _birthdayChannel = channel;
            bdChannelId = channel.Id;
            await SaveAsync();
        }
        public async Task<bool> AddNotifyAdditional(string additional)
        {
            if (additional.Contains(additional)) return false;
            notifyAdditionals.Add(additional);
            await SaveAsync();
            return true;
        }
        public async Task<bool> RemoveNotifyAdditional(string additional)
        {
            if (!additional.Contains(additional)) return false;
            notifyAdditionals.Remove(additional);
            await SaveAsync();
            return true;
        }
        public async Task<bool> TryAddInfoAsync(ulong userId, string name, int birthDay, int birthMonth, int birthYear)
        {
            if (membersBirthdays.ContainsKey(userId)) return false;
            var info = new BirthdayInfo()
            {
                Name = name,
                BirthDay = birthDay,
                BirthMonth = birthMonth,
                BirthYear = birthYear,
                LastYearNotified = DateTime.Now.Year - 1
            };
            membersBirthdays.Add(userId, info);
            await SaveAsync();
            return true;
        }

        public async Task<bool> TryRemoveInfoAsync(ulong userId)
        {
            if (membersBirthdays.ContainsKey(userId))
            {
                membersBirthdays.Remove(userId);
                return true;
            }
            await SaveAsync();
            return false;

        }
        public DiscordChannel GetBirthdayChannel()
        {
            return _birthdayChannel;
        }
        #endregion

        #region Private Methods
        private string GetRandomAdditional()
        {
            if (notifyAdditionals.Count == 0) return "";
            Random random = new Random();
            return notifyAdditionals[random.Next(0, random.Next(notifyAdditionals.Count))];
        }

        private bool IsBirthdayToday(BirthdayInfo info, DateTime currentDate)
        {
            return currentDate.Year != info.LastYearNotified &&
                                    currentDate.Day == info.BirthDay &&
                                    currentDate.Month == info.BirthMonth;
        }
        #endregion

        struct BirthdayInfo
        {
            public string Name;
            public int LastYearNotified;
            public int BirthDay;
            public int BirthMonth;
            public int BirthYear;
        }
    }
}
