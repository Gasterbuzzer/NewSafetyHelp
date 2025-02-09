using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSafetyHelp.src.DebugHelper
{
    public static class Debug
    {

        public static void RemoveAllEntries(ref MonsterProfile[] monsterProfiles)
        {
            monsterProfiles = new MonsterProfile[monsterProfiles.Length];
        }
    }
}
