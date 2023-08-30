namespace VoidInventory.Content
{
    public class UIItemTex : BaseUIElement
    {
        /// <summary>
        /// 框贴图
        /// </summary>
        public Texture2D SlotBackTexture { get; set; }
        /// <summary>
        /// 框内物品
        /// </summary>
        public Item ContainedItem { get; set; }
        /// <summary>
        /// 框的绘制的拐角尺寸
        /// </summary>
        public Vector2 CornerSize { get; set; }
        /// <summary>
        /// 绘制颜色
        /// </summary>
        public Color DrawColor { get; set; }
        public Color StackColor { get; set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public float Opacity { get; set; }
        public int id;
        public UIItemTex(int itemType, Texture2D texture = default)
        {
            Opacity = 1f;
            Main.instance.LoadItem(itemType);
            ContainedItem = new Item(itemType);
            SlotBackTexture = texture == default ? TextureAssets.InventoryBack.Value : texture;
            StackColor = DrawColor = Color.White;
            CornerSize = new Vector2(10, 10);
            SetSize(52, 52);
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);

            //float scale = Info.Size.X / 52f;
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            //调用原版的介绍绘制
            if (Info.IsMouseHover && ContainedItem != null && ContainedItem.type != ItemID.None)
            {
                Main.hoverItemName = ContainedItem.Name;
                Main.HoverItem = ContainedItem.Clone();
            }
            //获取当前UI部件的信息
            Rectangle DrawRectangle = Info.TotalHitBox;
            DrawRectangle.Width = 52;
            DrawRectangle.Height = 52;
            //绘制物品框
            DrawAdvBox(sb, DrawRectangle.X, DrawRectangle.Y, 52, 52,
                DrawColor * Opacity, SlotBackTexture, CornerSize, 1f);
            if (ContainedItem != null && ContainedItem.type != ItemID.None)
            {
                Rectangle frame = Main.itemAnimations[ContainedItem.type] != null ? Main.itemAnimations[ContainedItem.type]
                    .GetFrame(TextureAssets.Item[ContainedItem.type].Value) : Item.GetDrawHitbox(ContainedItem.type, null);
                //绘制物品贴图
                sb.Draw(TextureAssets.Item[ContainedItem.type].Value, new Vector2(DrawRectangle.X + (DrawRectangle.Width / 2),
                    DrawRectangle.Y + (DrawRectangle.Height / 2)), frame, Color.White * Opacity, 0f,
                    new Vector2(frame.Width, frame.Height) / 2f, 1 * frame.AutoScale(), 0, 0);
                //sb.DrawString(font, ContainedItem.stack.ToString(), new Vector2(DrawRectangle.X + 10, DrawRectangle.Y + DrawRectangle.Height - 20), StackColor * Opacity, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            }
        }
        /// <summary>
        /// 绘制物品框
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="c"></param>
        /// <param name="img"></param>
        /// <param name="size4"></param>
        /// <param name="scale"></param>
        public void DrawAdvBox(SpriteBatch sp, int x, int y, int w, int h, Color c, Texture2D img, Vector2 size4, float scale = 1f)
        {
            Texture2D box = img;
            int nw = (int)(w * scale);
            int nh = (int)(h * scale);
            x += (w - nw) / 2;
            y += (h - nh) / 2;
            w = nw;
            h = nh;
            int width = (int)size4.X;
            int height = (int)size4.Y;
            if (w < size4.X)
            {
                w = width;
            }
            if (h < size4.Y)
            {
                h = width;
            }
            sp.Draw(box, new Rectangle(x, y, width, height), new Rectangle(0, 0, width, height), c);
            sp.Draw(box, new Rectangle(x + width, y, w - (width * 2), height), new Rectangle(width, 0, box.Width - (width * 2), height), c);
            sp.Draw(box, new Rectangle(x + w - width, y, width, height), new Rectangle(box.Width - width, 0, width, height), c);
            sp.Draw(box, new Rectangle(x, y + height, width, h - (height * 2)), new Rectangle(0, height, width, box.Height - (height * 2)), c);
            sp.Draw(box, new Rectangle(x + width, y + height, w - (width * 2), h - (height * 2)), new Rectangle(width, height, box.Width - (width * 2), box.Height - (height * 2)), c);
            sp.Draw(box, new Rectangle(x + w - width, y + height, width, h - (height * 2)), new Rectangle(box.Width - width, height, width, box.Height - (height * 2)), c);
            sp.Draw(box, new Rectangle(x, y + h - height, width, height), new Rectangle(0, box.Height - height, width, height), c);
            sp.Draw(box, new Rectangle(x + width, y + h - height, w - (width * 2), height), new Rectangle(width, box.Height - height, box.Width - (width * 2), height), c);
            sp.Draw(box, new Rectangle(x + w - width, y + h - height, width, height), new Rectangle(box.Width - width, box.Height - height, width, height), c);
        }
    }
}
