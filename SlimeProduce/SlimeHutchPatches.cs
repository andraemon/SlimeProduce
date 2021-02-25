using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SlimeProduce
{
    class SlimeHutchPatches
    {
        public static void DayUpdate_Postfix(SlimeHutch __instance)
        {
            SlimeHutch hutch = __instance;
            Building building = hutch.getBuilding();
            Random r = new Random((int)(Game1.stats.DaysPlayed + (uint)((int)Game1.uniqueIDForThisGame / 10) + (uint)(building.tileX * 77) + (uint)(building.tileY * 777)));
            List<GreenSlime> slimes = new List<GreenSlime>();

            foreach (NPC npc in hutch.characters)
            {
                if (npc is GreenSlime)
                {
                    slimes.Add(npc as GreenSlime);
                }
            }

            for (int i = 0; i < hutch.objects.Count(); i++)
            {
                StardewValley.Object o = hutch.objects.Pairs.ElementAt(i).Value;
                if (o.Name.Contains("Slime Ball"))
                {
                    if (string.IsNullOrEmpty(o.orderData))
                    {
                        int j = r.Next(slimes.Count);
                        if (slimes[j].Name.Contains("Tiger Slime"))
                        {
                            o.orderData.Value = ModEntry.ColorFormat(new Color(255, 128, 0).ToString()) + $".420" + $".{slimes[j].firstGeneration}";
                        }
                        else
                        {
                            o.orderData.Value = ModEntry.ColorFormat(slimes[j].color.ToString()) + $".{slimes[j].specialNumber}" + $".{slimes[j].firstGeneration}";
                        }
                    }
                }
            }
        }


        private static IMonitor Monitor;
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
    }
}
