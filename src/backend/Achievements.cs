using System.Collections.Generic;
using Microsoft.Xna.Framework;

#if !ANDROID
using Steamworks;
#endif

namespace NonsensicalVideoGenerator
{
    public static class Achievements
    {
        public static List<string> awardedAchievements = new List<string>();
        public static void GetCurrentAchievements()
        {
#if !ANDROID
            if(SteamManager.initialized)
            {
                for(int i = 0; i < SteamUserStats.GetNumAchievements(); i++)
                {
                    string name = SteamUserStats.GetAchievementName((uint)i);
                    SteamUserStats.GetAchievement(name, out bool achieved);
                    if(achieved)
                    {
                        awardedAchievements.Add(name);
                    }
                }
            }
#endif
        }
        public static void Award(string achievement)
        {
#if !ANDROID
            if(Debug.debugModePermanent)
            {
                ConsoleOutput.WriteLine("Debug mode enabled, not awarding achievement: "+achievement, Color.LightBlue);
                return;
            }
            if(SteamManager.initialized)
            {
                if(!awardedAchievements.Contains(achievement))
                {
                    awardedAchievements.Add(achievement);
                    ConsoleOutput.WriteLine("Awarding achievement: "+achievement, Color.LightBlue);
                    SteamUserStats.SetAchievement(achievement);
                }
            }
#endif
        }
    }
}
