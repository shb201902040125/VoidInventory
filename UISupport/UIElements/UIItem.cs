namespace VoidInventory.UISupport.UIElements
{
    public class UIItem : BaseUIElement
    {
        public int itemid;
        public int stack;
        public Vector2 scale = Vector2.One;
        /// <summary>
        /// 是否忽视堆叠显示限制
        /// </summary>
        public bool Ignore;
        public UIItem(int itemid = -1, int stack = 1)
        {
            this.itemid = itemid;
            this.stack = stack;
            SetSize(32, 32);
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            //调用原版物品介绍
            if (itemid >= 0 && ContainsPoint(Main.MouseScreen))
            {
                Item item = new(itemid, stack);
                Main.hoverItemName = item.Name;
                Main.HoverItem = item;
            }
            if (itemid >= 0)
            {
                Main.instance.LoadItem(itemid);
                Rectangle frame = Main.itemAnimations[itemid] != null ? Main.itemAnimations[itemid].GetFrame(TextureAssets.Item[itemid].Value) : Item.GetDrawHitbox(itemid, null);
                //绘制物品贴图
                Vector2 center = Center();
                SimpleDraw(sb, TextureAssets.Item[itemid].Value, center, frame, frame.Size() / 2f, scale * frame.AutoScale(24));
                /* sb.Draw(TextureAssets.Item[itemid].Value, new Vector2(HitBox().X + HitBox().Width / 2,
                     HitBox().Y + HitBox().Height / 2) - (new Vector2(frame.Width, frame.Height) / 2f * scale),
                     new Rectangle?(frame), Color.White * opacity, 0f, Vector2.Zero, scale, 0, 0);*/

                //绘制物品左下角那个代表数量的数字
                if (stack > 1 || Ignore)
                {
                    ChatManager.DrawColorCodedString(sb, FontAssets.MouseText.Value, stack.ToString(),
                            new Vector2(HitBox().X + 10, HitBox().Y + HitBox().Height - 20),
                            Color.White, 0f, Vector2.Zero, scale * 0.8f);
                }
            }
        }
    }
}
