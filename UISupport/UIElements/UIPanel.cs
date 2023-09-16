namespace VoidInventory.UISupport.UIElements
{
    public class UIPanel : UIBottom
    {
        public Texture2D Tex;
        public Color color;
        public bool drawBoeder = true;
        public float opacity = 0.5f;
        public int? cornerSize;
        public int barSize;
        private Texture2D border;
        public UIPanel(float x, float y, int cornerSize = 12, int barSize = 4, Color? color = null, float opacity = 0.7f) : base(x, y)
        {
            this.cornerSize = cornerSize;
            this.barSize = barSize;
            this.color = color ?? new Color(63, 82, 151);
            this.opacity = opacity;
            Tex = T2D("Terraria/Images/UI/PanelBackground");
            border = T2D("Terraria/Images/UI/PanelBorder");
            Info.CanBeInteract = true;
            CanDrag = false;
        }
        public UIPanel(float x, float y, string texKey, Color? color = null, float opacity = 0.5f) : base(x, y)
        {
            texKey = texKey == default ? "VoidInventory/UISupport/Asset/SkillTree_bg9" : texKey;
            Tex = T2D(texKey);
            Info.CanBeInteract = true;
            CanDrag = false;
            this.color = color ?? Color.White;
            this.opacity = opacity;
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            Rectangle rec = HitBox();
            if (cornerSize.HasValue)
            {
                if (drawBoeder) VanillaDraw(sb, rec, border, Color.Black, cornerSize.Value, barSize);
                VanillaDraw(sb, rec, Tex, color * opacity, cornerSize.Value, barSize);
                return;
            }
            int dis = Tex.Width / 3;
            Rectangle[] coords = Rec3x3(dis, dis);
            Vector2 size = new(Tex.Width / 6f);
            sb.Draw(TextureAssets.MagicPixel.Value, rec, new(0, 0, 1, 1), color * opacity);
            sb.Draw(Tex, NewRec(rec.TopLeft() - new Vector2(0, dis / 2), rec.Width, dis), coords[1], Color.White);
            sb.Draw(Tex, NewRec(rec.TopLeft() - new Vector2(dis / 2, 0), dis, rec.Height), coords[3], Color.White);
            sb.Draw(Tex, NewRec(rec.TopRight() - new Vector2(dis / 2, 0), dis, rec.Height), coords[5], Color.White);
            sb.Draw(Tex, NewRec(rec.BottomLeft() - new Vector2(0, dis / 2), rec.Width, dis), coords[7], Color.White);
            SimpleDraw(sb, Tex, rec.TopLeft(), coords[0], size);
            SimpleDraw(sb, Tex, rec.TopRight(), coords[2], size);
            SimpleDraw(sb, Tex, rec.BottomLeft(), coords[6], size);
            SimpleDraw(sb, Tex, rec.BottomRight(), coords[8], size);
        }
        private static void VanillaDraw(SpriteBatch spriteBatch, Rectangle rec, Texture2D texture, Color color, int cornerSize, int barSize)
        {
            Point point = new(rec.X, rec.Y);
            Point point2 = new(point.X + rec.Width - cornerSize, point.Y + rec.Height - cornerSize);
            int width = point2.X - point.X - cornerSize;
            int height = point2.Y - point.Y - cornerSize;
            spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, cornerSize, cornerSize), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + cornerSize, point.Y, width, cornerSize), new Rectangle(cornerSize, 0, barSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + cornerSize, point2.Y, width, cornerSize), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), color);
            spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + cornerSize, cornerSize, height), new Rectangle(0, cornerSize, cornerSize, barSize), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + cornerSize, cornerSize, height), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + cornerSize, point.Y + cornerSize, width, height), new Rectangle(cornerSize, cornerSize, barSize, barSize), color);
        }
    }
}
