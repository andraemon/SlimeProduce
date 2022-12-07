using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System.IO;
using SObject = StardewValley.Object;

namespace SlimeProduce
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            // Patch relevant methods
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(SObject), nameof(SObject.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DrawPrefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               postfix: new HarmonyMethod(typeof(SlimeHutchPatches), nameof(SlimeHutchPatches.DayUpdatePostfix))
            );

            // Subscribe event handlers
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Content.AssetRequested += OnAssetRequested;

            // Register console commands
            Helper.ConsoleCommands.Add("spawn_slime", "Spawns slimes of a certain color.\n\nUsage: spawn_slime <r> <g> <b>\n- r/g/b: The values for the red, green or blue components of the slime's color. Should be integers between 0 and 255.", SpawnSlime);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    IRawTextureData sourceImage = Helper.ModContent.Load<IRawTextureData>(Path.Combine("assets", "SlimeBallGray.png"));
                    editor.PatchImage(sourceImage, targetArea: new Rectangle(0, 224, 96, 32));
                });
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Enable Deluxe Grabber Redux integration if it's present
            DeluxeGrabberReduxLoaded = Helper.ModRegistry.IsLoaded("ferdaber.DeluxeGrabberRedux");
            if (DeluxeGrabberReduxLoaded) {
                Helper.Events.World.ObjectListChanged += OnObjectListChanged;
                Monitor.Log("Deluxe Auto-Grabber Redux integration loaded", LogLevel.Debug);
            }
            
            StardewMelon.ColoredObjects = StardewMelon.GetColoredObjects();
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Location is SlimeHutch)
            {
                // Find valid grabber inventory if integration loaded
                Chest grabber = null;
                if (DeluxeGrabberReduxLoaded)
                    foreach (SObject o in e.Location.Objects.Values)
                        if (IsValidGrabber(o))
                        {
                            grabber = (Chest)o.heldObject.Value;
                            break;
                        }

                // Add additional slimeball drops as debris or to grabber
                foreach (KeyValuePair<Vector2, SObject> obj in e.Removed)
                    if (obj.Value.ParentSheetIndex >= 56 && obj.Value.ParentSheetIndex <= 61)
                    {
                        var drops = new SlimeBall(obj.Value).GenerateDrops();

                        if (e.Location.farmers.Count >= 1)
                            foreach (KeyValuePair<int, int> drop in drops)
                                Game1.createMultipleObjectDebris(drop.Key, (int)obj.Value.TileLocation.X, (int)obj.Value.TileLocation.Y, drop.Value, 1f + ((Game1.player.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
                        else if (grabber != null)
                            foreach (KeyValuePair<int, int> drop in drops)
                                grabber.addItem(new SObject(drop.Key, drop.Value, false, -1, 0));
                    }
            }
        }

        private bool IsValidGrabber(SObject obj)
            => obj.ParentSheetIndex == 165 &&
               obj.heldObject.Value != null &&
               obj.heldObject.Value is Chest;

        private void SpawnSlime(string command, string[] args)
        {
            int red, green, blue;

            try {
                red = int.Parse(args[0]);
                green = int.Parse(args[1]);
                blue = int.Parse(args[2]);
            }
            catch {
                Monitor.Log("Could not parse arguments, please ensure they are formatted correctly.");
                return;
            }

            Monitor.Log($"Spawning slime with color {red}, {green}, {blue}.", LogLevel.Debug);
            Game1.currentLocation.characters.Add(new GreenSlime(Game1.player.lastPosition, new Color(red, green, blue)));
        }

        internal static ModConfig Config;
        private static bool DeluxeGrabberReduxLoaded;
    }
}