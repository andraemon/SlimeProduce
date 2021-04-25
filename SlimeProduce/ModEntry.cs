using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using Harmony;

namespace SlimeProduce
{
    public class ModEntry : Mod, IAssetEditor
    {
        public override void Entry(IModHelper helper)
        {
            help = helper;
            SlimeHutchPatches.Initialize(Monitor);
            Config = help.ReadConfig<ModConfig>();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               postfix: new HarmonyMethod(typeof(SlimeHutchPatches), nameof(SlimeHutchPatches.DayUpdate_Postfix))
            );
                        
            help.Events.World.ObjectListChanged += World_ObjectListChanged;
            help.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            help.ConsoleCommands.Add("spawn_slime", "Spawns slimes of a certain color.\n\nUsage: spawn_slime <r> <g> <b>\n- r/g/b: The values for the red, green or blue components of the slime's color. Should be integers between 0 and 255.", SpawnSlime);
            Monitor.Log("Loaded", LogLevel.Debug);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            DeluxeGrabberLoaded = help.ModRegistry.IsLoaded("stokastic.DeluxeGrabber");
            if (DeluxeGrabberLoaded == true)
            {
                Monitor.Log("Deluxe Auto-Grabber integration loaded", LogLevel.Debug);
            }
            foreach (KeyValuePair<int, string> pair in Game1.objectInformation)
            {
                StardewValley.Object obj = new StardewValley.Object(pair.Key, 1, false, -1, 0);
                if (TailoringMenu.GetDyeColor(obj) != null)
                {
                    try
                    {
                        ColoredObjects[TailoringMenu.GetDyeColor(obj) ?? default].Add(pair.Key);
                    }
                    catch (KeyNotFoundException)
                    {
                        ColoredObjects[TailoringMenu.GetDyeColor(obj) ?? default] = new List<int>
                        {
                            pair.Key
                        };
                    }
                }
            }
        }

        private void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Location is SlimeHutch)
            {
                Dictionary<int, int> ItemsToDrop = new Dictionary<int,int>();
                StardewValley.Object grabber = null;
                foreach (KeyValuePair<Vector2, StardewValley.Object> o in e.Location.Objects.Pairs)
                {
                    if (o.Value.Name.Contains("Grabber") && grabber == null)
                    {
                        grabber = o.Value;
                    }
                }
                foreach (KeyValuePair<Vector2, StardewValley.Object> p in e.Removed)
                {
                    ItemsToDrop.Clear();
                    if (p.Value.Name.Contains("Slime Ball") && !string.IsNullOrEmpty(p.Value.orderData))
                    {
                        Random r = new Random((int)(Game1.stats.DaysPlayed + Game1.uniqueIDForThisGame + p.Value.tileLocation.X * 77 + p.Value.tileLocation.Y * 777));
                        if (Config.EnableSpecialColorDrops && ColoredObjects.ContainsKey(StrToColor(p.Value.orderData)))
                        {
                            if (r.NextDouble() < Config.SpecialColorDropChance)
                            {
                                List<int> i = ColoredObjects[StrToColor(p.Value.orderData)];
                                ItemsToDrop.Add(i[r.Next(i.Count)], r.Next(Config.SpecialColorMinDrop, Config.SpecialColorMaxDrop + 1));
                            }
                        }
                        if (Config.EnableSpecialTigerSlimeDrops && StrToColor(p.Value.orderData).Equals(new Color(255, 128, 0)) && int.Parse(p.Value.orderData.Value.Split('.')[4]) == 420)
                        {
                            ItemsToDrop.Add(92, r.Next(15, 26));
                            ItemsToDrop.Add(70, r.Next(1, 3));
                            if (r.NextDouble() < 0.65)
                            {
                                ItemsToDrop.Add(829, r.Next(4, 9));
                            }
                            if (r.NextDouble() < 0.33 && Convert.ToBoolean(p.Value.orderData.Value.Split('.')[5]))
                            {
                                ItemsToDrop.Add(852, r.Next(1, 3));
                            }
                            if (r.NextDouble() < 0.33 && Convert.ToBoolean(p.Value.orderData.Value.Split('.')[5]))
                            {
                                ItemsToDrop.Add(851, r.Next(1, 3));
                            }
                            if (r.NextDouble() < 0.5 && Convert.ToBoolean(p.Value.orderData.Value.Split('.')[5]))
                            {
                                ItemsToDrop.Add(848, r.Next(5, 11));
                            }
                            goto DropItems;
                        }
                        else if (Config.EnableSpecialWhiteSlimeDrops && white.Contains(StrToColor(p.Value.orderData)))
                        {
                            if (StrToColor(p.Value.orderData).R % 2 == 1)
                            {
                                ItemsToDrop.Add(338, r.Next(2, 5));
                                if (StrToColor(p.Value.orderData).G % 2 == 1)
                                {
                                    ItemsToDrop[338] += r.Next(2, 5);
                                }
                            }
                            else
                            {
                                ItemsToDrop.Add(380, r.Next(10, 21));
                            }
                            if (StrToColor(p.Value.orderData).R % 2 == 0 && StrToColor(p.Value.orderData).G % 2 == 0 && StrToColor(p.Value.orderData).B % 2 == 0 || StrToColor(p.Value.orderData).Equals(Color.White))
                            {
                                ItemsToDrop.Add(72, r.Next(1, 3));
                            }
                            goto DropItems;
                        }
                        else if (Config.EnableSpecialPurpleSlimeDrops && purple.Contains(StrToColor(p.Value.orderData)) && int.Parse(p.Value.orderData.Value.Split('.')[4]) % (Convert.ToBoolean(p.Value.orderData.Value.Split('.')[5]) ? 4 : 2) == 0)
                        {
                            ItemsToDrop.Add(386, r.Next(5, 11));
                            if (Convert.ToBoolean(p.Value.orderData.Value.Split('.')[5]) && Game1.random.NextDouble() < 0.072)
                            {
                                ItemsToDrop.Add(485, 1);
                            }
                            goto DropItems;
                        }
                        foreach (DropTable p2 in Config.DropTables){
                            if (p2.colorRange.Contains(StrToColor(p.Value.orderData)))
                            {
                                foreach (ItemDrop d in p2.itemDrops)
                                {
                                    if (r.NextDouble() < d.dropChance)
                                    {
                                        ItemsToDrop.Add(d.parentSheetIndex, r.Next(d.minDrop, d.maxDrop + 1));
                                    }
                                }
                                goto DropItems;
                            }
                        }

                        DropItems:
                        if (grabber != null && e.Location.farmers.Count == 0 && DeluxeGrabberLoaded)
                        {
                            bool full = (grabber.heldObject.Value as Chest).items.CountIgnoreNull() >= Chest.capacity;
                            foreach (KeyValuePair<int, int> p2 in ItemsToDrop)
                            {
                                if (!full)
                                {
                                    (grabber.heldObject.Value as Chest).addItem(new StardewValley.Object(p2.Key, p2.Value, false, -1, 0));
                                }
                            }
                        } 
                        else if (e.Location.farmers.Count >= 1)
                        {
                            foreach (KeyValuePair<int, int> p3 in ItemsToDrop)
                            {
                                Game1.createMultipleObjectDebris(p3.Key, (int)p.Value.tileLocation.X, (int)p.Value.tileLocation.Y, p3.Value, 1f + ((Game1.player.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
                            }
                        }
                    }
                }
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("TileSheets/Craftables"))
            {
                return true;
            }

            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            var editor = asset.AsImage();

            Texture2D sourceImage = help.Content.Load<Texture2D>("assets/SlimeBallGray.png", ContentSource.ModFolder);
            editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 224, 96, 32));
        }

        private void SpawnSlime(string command, string[] args)
        {
            int red = int.Parse(args[0]);
            int green = int.Parse(args[1]);
            int blue = int.Parse(args[2]);
            Color color = new Color(red, green, blue);
            GreenSlime slime = new GreenSlime(Game1.player.lastPosition, color);
            Monitor.Log($"Spawning slime with color {red}, {green}, {blue}.", LogLevel.Debug);
            Game1.currentLocation.characters.Add(slime);
        }

        public static string ColorFormat(string c)
        {
            c = c.Replace("{R:", "");
            c = c.Replace(" G:", ".");
            c = c.Replace(" B:", ".");
            c = c.Replace(" A:", ".");
            c = c.Replace("}", "");
            return c;
        }

        public static Color StrToColor(string s)
        {
            Color c = new Color();
            string[] cs = s.Split('.');
            c.R = Convert.ToByte(cs[0]);
            c.G = Convert.ToByte(cs[1]);
            c.B = Convert.ToByte(cs[2]);
            c.A = Convert.ToByte(cs[3]);
            return c;
        }

        public static IModHelper help;
        internal ModConfig Config;
        readonly ColorRange purple = new ColorRange(new int[] { 151, 255 }, new int[] { 0, 49 }, new int[] { 181, 255 });
        readonly ColorRange white = new ColorRange(new int[] { 231, 255 }, new int[] { 231, 255 }, new int[] { 231, 255 });
        private static bool DeluxeGrabberLoaded;
        private Dictionary<Color, List<int>> ColoredObjects = new Dictionary<Color, List<int>>();

    }
}