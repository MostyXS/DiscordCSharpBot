using DSharpPlus;
using DSharpPlus.Entities;
using LSSKeeper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSKeeper
{
    public class OverwriteUpdateInformation
    {
        /// <summary>
        /// Удаление, изменение или добавление
        /// </summary>
        public string Action { get; private set; }
        DiscordOverwrite AffectedOverwrite;
        public List<string> Changes { get; private set; }
        /// <summary>
        /// List of changes in already human-readable format
        /// </summary>
        /// <summary>
        /// role mention
        /// </summary>
        public DiscordOverwrite GetAffectedOverwrite()
        {
            return AffectedOverwrite;
        }
        public OverwriteUpdateInformation(IReadOnlyList<DiscordOverwrite> rolesOwsBefore, IReadOnlyList<DiscordOverwrite> rolesOwsAfter)
        {
            if (TryFindDistinctOverwrite(rolesOwsBefore, rolesOwsAfter, out AffectedOverwrite)) { Action = "Удаление"; return; }
            else if (TryFindDistinctOverwrite(rolesOwsAfter, rolesOwsBefore, out AffectedOverwrite)) { Action = "Добавление"; return; }


            foreach (var owsB in rolesOwsBefore) //Initializing array of permissions for some roles
            {
                var rawOverwrite = rolesOwsAfter.Where((x) => owsB.Id == x.Id && !(owsB.Allowed == x.Allowed && owsB.Denied == x.Denied)); // no elements
                if (rawOverwrite.Count() > 0)
                {
                    AffectedOverwrite = rawOverwrite.First();
                    CalculateChangedOverwrite(owsB, AffectedOverwrite);
                    Action = "Изменение";
                    return;
                }
            }
        }
        private void CalculateChangedOverwrite(DiscordOverwrite owsB, DiscordOverwrite owsA)
        {

            var deniedBefore = Array.ConvertAll(owsB.Denied.ToPermissionString().Split(','), x => x.Trim());
            var allowedBefore = Array.ConvertAll(owsB.Allowed.ToPermissionString().Split(','), x => x.Trim());
            var deniedAfter = Array.ConvertAll(owsA.Denied.ToPermissionString().Split(','), x => x.Trim());
            var allowedAfter = Array.ConvertAll(owsA.Allowed.ToPermissionString().Split(','), x => x.Trim());

            Changes = new List<string>();
            
            foreach (string allPermA in allowedAfter) //calc allowed
            {
                if (allowedBefore.Contains(allPermA)) continue;

                //we have overwrite(a or d or n) we need to find where he has before
                if (deniedBefore.Contains(allPermA)) // d -> a
                {
                    Changes.Add($"{allPermA}: :x: -> :white_check_mark:");
                }
                else //n -> a
                {
                    Changes.Add($"{allPermA}: :white_large_square: -> :white_check_mark:");
                }     
            }
            foreach (string denPermA in deniedAfter) // calc denied
            {
                if (deniedBefore.Contains(denPermA)) continue;

                if (allowedBefore.Contains(denPermA)) // a -> d
                {
                    Changes.Add($"{denPermA}: :white_check_mark: -> :x:");
                }
                else // d -> a
                {
                    Changes.Add($"{denPermA}: :white_large_square: -> :x:");
                }
                
            }
            foreach(string denPermB in deniedBefore)
            {
                if(!deniedAfter.Contains(denPermB) && !allowedAfter.Contains(denPermB)) //den
                {
                    Changes.Add($"{denPermB}: :x: -> :white_large_square:");
                }
            }
            foreach(string allPermB in allowedBefore)
            {
                if(!deniedAfter.Contains(allPermB) && !allowedAfter.Contains(allPermB)) //den
                {
                    Changes.Add($"{allPermB}: :white_check_mark: -> :white_large_square:");
                }
            }
        }
        /// <summary>
        /// Finds SINGLE value in this list that distinct from otherList
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private bool TryFindDistinctOverwrite(IReadOnlyList<DiscordOverwrite> firstList, IReadOnlyList<DiscordOverwrite> secondList, out DiscordOverwrite distinctValue)
        {
            distinctValue = null;
            foreach (var v in firstList)
            {
                if (!secondList.Any((x) => x.Id == v.Id)) //if no values from other list does not match potentially new value(v), then returns true
                {
                    distinctValue = v;
                    return true;
                }

            }
            return false;
        }
    }
}
