global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Graphics;
global using System;
global using System.Collections.Generic;
global using System.Reflection;
global using Terraria;
global using Terraria.GameContent;
global using Terraria.GameContent.UI;
global using Terraria.ID;
global using Terraria.ModLoader;
global using Terraria.UI.Chat;
global using VoidInventory.UISupport.UIElements;
global using static VoidInventory.MiscHelper;
using Terraria.Localization;

namespace VoidInventory
{
    public static class MiscHelper
    {
        public static string GTV(string key) => Language.GetTextValue("Mods.VoidInventory." + key);
        public const string Asset = "VoidInventory/UISupport/Asset/";
        public static Rectangle NewRec(Vector2 start, Vector2 size) => NewRec(start.ToPoint(), size.ToPoint());
        public static Rectangle NewRec(Vector2 start, float width, float height)
        {
            return new Rectangle((int)start.X, (int)start.Y, (int)width, (int)height);
        }
        public static Rectangle NewRec(Point start, Point size)
        {
            return new Rectangle(start.X, start.Y, size.X, size.Y);
        }
        public static Rectangle NewRec(Point start, int width, int height)
        {
            return new Rectangle(start.X, start.Y, width, height);
        }
        public static Rectangle RecCenter(Vector2 center, int width, int height)
        {
            Point p = center.ToPoint();
            return new Rectangle(p.X - width / 2, p.Y - height / 2, width, height);
        }
        public static Rectangle RecCenter(Vector2 center, float width, float height) => RecCenter(center, (int)width, (int)height);

        public static int CalculatePhase(int value, int min, int max, int dis)
        {
            return value >= max ? (max - min) / dis : value <= min ? 0 : (value - min) / dis;
        }
        public static int Loop(int value, int min, int max)
        {
            int range = max - min + 1;
            if (value < min)
            {
                int diff = min - value;
                int mod = diff % range;
                return max - mod + 1;
            }
            if (value > max)
            {
                int diff = value - max;
                int mod = diff % range;
                return min + mod - 1;
            }
            return value;
        }
        public static float Loop(float value, float min, float max)
        {
            float range = max - min;
            if (value < min)
            {
                float diff = min - value;
                float mod = diff % range;
                return max - mod;
            }
            if (value > max)
            {
                float diff = value - max;
                float mod = diff % range;
                return min + mod;
            }
            return value;
        }
        public static Vector2 GetStringSize(this DynamicSpriteFont font, string text, Vector2 scale)
        {
            return font.MeasureString(text) * scale;
        }
        /// <summary>
        /// 自动命名空间路径
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">是否自带类名</param>
        /// <returns></returns>
        public static string Path(this object type, bool name = false)
        {
            Type target = type.GetType();
            return target.Namespace.Replace(".", "/") + $"/{(name ? target.Name : null)}";
        }
        /// <summary>
        /// 上面那个的静态版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Path<T>(bool name = false)
        {
            Type target = typeof(T);
            return target.Namespace.Replace(".", "/") + $"/{(name ? target.Name : null)}";
        }
        /// <summary>
        /// 自动命名空间路径
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string Path(this object type, string fileName) => type.Path(false) + fileName;
        public static Texture2D Tex(this Entity entity)
        {
            if (entity is Projectile proj)
            {
                return TextureAssets.Projectile[proj.type].Value;
            }
            if (entity is NPC npc)
            {
                return TextureAssets.Npc[npc.type].Value;
            }
            if (entity is Item item)
            {
                return TextureAssets.Item[item.type].Value;
            }
            throw new Exception("不支持的Entity类型");
        }
        public static Rectangle ScaleRec(this Rectangle r, Vector2 scale)
        {
            r.X = (int)(r.X * scale.X);
            r.Y = (int)(r.Y * scale.Y);
            r.Width = (int)(r.Width * scale.X);
            r.Height = (int)(r.Height * scale.Y);
            return r;
        }
        /// <summary>
        /// 让矩形整体被矩阵缩放
        /// </summary>
        /// <param name="r"></param>
        /// <param name="matrix">万恶的缩放矩阵</param>
        /// <returns></returns>
        public static Rectangle ScaleRec(this Rectangle r, Matrix matrix) => ScaleRec(r, new Vector2(matrix.M11, matrix.M22));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="w">横向单个</param>
        /// <param name="h">纵向单个</param>
        /// <param name="offset"></param>
        /// <returns>从左到右从上到下的裁剪</returns>
        public static Rectangle[] Rec3x3(int w, int h, int offset = 2)
        {
            return new Rectangle[9]
            {
                new(0, 0, w, h),
                new(w + offset, 0, w, h),
                new((w + offset) * 2, 0, w, h),
                new(0, h + offset, w, h),
                new(w + offset, h + offset, w, h),
                new((w + offset) * 2, h + offset, w, h),
                new(0, (h + offset) * 2, w, h),
                new(w, (h + offset) * 2, w, h),
                new((w + offset) * 2, (h + offset) * 2, w, h),
            };
        }
        public static Color SetAlpha(this Color c, byte alpha)
        {
            c.A = alpha;
            return c;
        }
        public static Texture2D T2D(string path) => ModContent.Request<Texture2D>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        public static void DrawRec(SpriteBatch sb, Rectangle rec, float width, Color color, bool worldPos = true)
        {
            Vector2 scrPos = worldPos ? Vector2.Zero : Main.screenPosition;
            DrawLine(sb, rec.TopLeft() + scrPos, rec.TopRight() + scrPos, width, color);
            DrawLine(sb, rec.TopRight() + scrPos, rec.BottomRight() + scrPos, width, color);
            DrawLine(sb, rec.BottomRight() + scrPos, rec.BottomLeft() + scrPos, width, color);
            DrawLine(sb, rec.BottomLeft() + scrPos, rec.TopLeft() + scrPos, width, color);
        }
        /// <summary>
        /// 简易画线
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="wide">粗细</param>
        /// <param name="color">颜色</param>
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float wide, Color color)
        {
            Texture2D texture = TextureAssets.MagicPixel.Value;
            Vector2 unit = end - start;
            spriteBatch.Draw(texture, start + unit / 2 - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, unit.ToRotation() + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(wide, unit.Length()), SpriteEffects.None, 0f);
        }
        public static bool CurrencyType(int currencyID, out int itemType)
        {
            itemType = -1;
            if (currencyID == -1) return false;
            if (CustomCurrencyManager.TryGetCurrencySystem(currencyID, out CustomCurrencySystem system))
            {
                var coinDict = system.GetType().GetField("_valuePerUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                if (coinDict != null)
                {
                    foreach ((int coinType, int value) in coinDict.GetValue(system) as Dictionary<int, int>)
                    {
                        if (value == 1)
                        {
                            itemType = coinType;
                            return true;
                        }
                    }

                }
            }
            return false;
        }
        /// <summary>
        /// 获取相对于给定大小的自动缩放修正
        /// </summary>
        /// <returns>用于乘算的修正值</returns>
        public static float AutoScale(this Rectangle drawRec, float size = 52, float scale = 0.75f)
        {
            float ZoomX = drawRec.Size().X / (size * scale);
            float ZoomY = drawRec.Size().Y / (size * scale);
            return 1f / Math.Max(MathF.Sqrt(ZoomX * ZoomX + ZoomY * ZoomY), 1);
        }
        public static IEnumerable<(int itemId, int count)> ToCoins(int money)
        {
            int copper = money % 100;
            money /= 100;
            int silver = money % 100;
            money /= 100;
            int gold = money % 100;
            money /= 100;
            int plat = money;

            yield return (ItemID.PlatinumCoin, plat);
            yield return (ItemID.GoldCoin, gold);
            yield return (ItemID.SilverCoin, silver);
            yield return (ItemID.CopperCoin, copper);
        }
        public static bool TryGetNPCShop(int npcType, out AbstractNPCShop shop)
        {
            shop = null;
            foreach (AbstractNPCShop nshop in NPCShopDatabase.AllShops)
            {
                if (nshop.NpcType == npcType)
                {
                    shop = nshop;
                    return true;
                }
            }
            return false;
        }
    }
}
