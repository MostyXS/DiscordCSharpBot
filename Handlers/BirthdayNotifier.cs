using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volodya.Extensions;

namespace Volodya.Handlers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BirthdayNotifier 
    {
        private DiscordGuild _defaultGuild;
        private DiscordChannel _birthdayChannel;

        #region Config Params
        [JsonProperty]
        Dictionary<ulong, BirthdayInfo> membersBirthdays = new Dictionary<ulong, BirthdayInfo>();
        [JsonProperty]
        ulong? bdChannelId;
        [JsonProperty]
        List<string> notifyAdditionals = new List<string>();
        #endregion
        #region Notify Process
        public async Task RunAsync()
        {
            while (true)
            {
                if (_defaultGuild == null) continue;
                foreach(var member in membersBirthdays)
                {
                    var info = member.Value;
                    var currentDate = DateTime.Now;
                    if (IsBirthdayToday(info, currentDate) && currentDate.TimeOfDay.TotalHours > 10)
                    {
                        var user = await _defaultGuild.GetMemberAsync(member.Key);
                        var age = currentDate.Year - info.BirthYear;
                        await _birthdayChannel.SendMessageAsync($"У {info.Name} {user.Mention} сегодня день рождения," +
                            $" ему исполнилось {age.ToRusAge()}, поздравляем {GetRandomAdditional()} " +
                            $":birthday:");
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1d));
            }
        }

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
        #region Commands
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
        public async Task<bool> AddInfo(ulong userId, string name, int birthDay, int birthMonth, int birthYear)
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


        public async Task<bool> RemoveInfo(ulong userId)
        {
            if (membersBirthdays.ContainsKey(userId))
            {
                membersBirthdays.Remove(userId);
                return true;
            }
            await SaveAsync();
            return false;

        }
        #endregion

        #region Config Management
        public void TryInitializeAsync(DiscordGuild defaultGuild)
        {
            _defaultGuild = defaultGuild;
            try
            {
                var jsonString = File.ReadAllText(ConfigNames.BIRTHDNOTY);
                var json = JsonConvert.DeserializeObject<BirthdayNotifier>(jsonString);
                bdChannelId = json.bdChannelId;
                if (bdChannelId != null)
                    _birthdayChannel = _defaultGuild.GetChannel((ulong)json.bdChannelId);
                notifyAdditionals = json.notifyAdditionals;
                membersBirthdays = json.membersBirthdays;

            }
            catch (Exception e)
            {
                Console.WriteLine("Не удалось инициализировать конфиг оповещений дней рождения: " + e.Message);
            }
        }
        private async Task SaveAsync()
        {

            var jsonString = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync(ConfigNames.BIRTHDNOTY, jsonString);

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
