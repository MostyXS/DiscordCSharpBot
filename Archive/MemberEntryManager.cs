using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOSCKeeper
{
    public class MemberEntryManager
    {
        const int MAX_SIZE = 5;
        Queue<DiscordAuditLogEntry> lastMemberEntries = new Queue<DiscordAuditLogEntry>();
        public DiscordAuditLogEntry UpdatedEntry { get; private set; }

        //First we should try find newEntry as member update entry
        //We should try find entry as discord member move entry in 5 closest, then we should try to find member disconnect entry in closest five and in both cases check if they are updated
        //Discord not creating new entries when moving from channel to channel in close entries
        public void CheckIfNewEntry(DiscordAuditLogEntry[] newEntries)
        {
            foreach(var e in newEntries)
            {
                if(lastMemberEntries.Any((x) => x.Id == e.Id))
                {
                    
                    switch(e.ActionType)
                    {
                        
                        case (AuditLogActionType.MemberMove):
                            {
                                var mmEntry = e as DiscordAuditLogMemberMoveEntry;
                            }
                            break;
                        case (AuditLogActionType.MemberDisconnect):
                            {
                                var mdEntry = e as DiscordAuditLogMemberDisconnectEntry;
                            }
                            break;
                        default:
                            break;

                    }
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Works only when audit type mdisconnect or mmove
        /// </summary>
        /// <param name="currentEntry"></param>
        /// <param name="otherEntry"></param>
        /// <returns></returns>
        /*   public static DiscordUser FindRemovedUser(this DiscordChannel channelBefore, DiscordChannel channelAfter)
           {
               foreach (var u in channelBefore.Users)
               {
                   if (!channelAfter.Users.Any((x) => x.Id == u.Id))
                   {
                       return u;
                   }
               }
               return null;

           }*/
        public void AddToLastEntries(DiscordAuditLogEntry e)
        {
            lastMemberEntries.Enqueue(e);
            if (lastMemberEntries.Count == MAX_SIZE) lastMemberEntries.Dequeue();
            


        }
    }
    

}
