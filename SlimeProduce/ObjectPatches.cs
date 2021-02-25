using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SlimeProduce
{
    class ObjectPatches
    {
        public static bool draw_Prefix(StardewValley.Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (__instance.Name == "Slime Ball")
            {
                Color color = Color.Lime;
                if (!string.IsNullOrEmpty(__instance.orderData))
                {
                    color = ModEntry.StrToColor(__instance.orderData);
                }
                Vector2 scaleFactor = __instance.getScale();
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * 64), (y * 64 - 64)));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + (x * 1E-05f);
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, new Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(__instance.showNextIndex ? (__instance.ParentSheetIndex + 1) : __instance.ParentSheetIndex)), color * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                return false;
            } 
            else
            {
                return true;
            }
        }
    }
}
