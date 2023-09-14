namespace VoidInventory.UISupport.UIElements
{
    public delegate bool CheckPutSlotCondition(Item mouseItem);
    public delegate void ExchangeItemHandler(BaseUIElement target);
    public class UIItemSlot : BaseUIElement
    {
        /// <summary>
        /// 框贴图
        /// </summary>
        public Texture2D SlotBackTexture { get; set; }
        /// <summary>
        /// 是否可以放置物品
        /// </summary>
        public CheckPutSlotCondition CanPutInSlot { get; set; } = new(x => false);
        /// <summary>
        /// 是否可以拿去物品
        /// </summary>
        public CheckPutSlotCondition CanTakeOutSlot { get; set; } = new(x => false);
        /// <summary>
        /// 物品无限拿取
        /// </summary>
        public bool Infinity;
        public bool IgnoreOne;
        public bool drawStack = true;
        /// <summary>
        /// 框内物品
        /// </summary>
        public Item ContainedItem { get; set; }
        public Item BackItem { get; private set; }
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
        /// 介绍
        /// </summary>
        public string Tooltip { get; set; }
        /// <summary>
        /// 更改物品时调用
        /// </summary>
        public event ExchangeItemHandler PostExchangeItem;
        public void ExchangeItem() => PostExchangeItem?.Invoke(this);
        /// <summary>
        /// 玩家拿取物品时调用
        /// </summary>
        public event ExchangeItemHandler OnPickItem;
        public void PickItem() => OnPickItem?.Invoke(this);

        /// <summary>
        /// 玩家放入物品时调用
        /// </summary>
        public event ExchangeItemHandler OnPutItem;
        public void PutItem() => OnPutItem?.Invoke(this);
        /// <summary>
        /// 透明度
        /// </summary>
        public float Opacity { get; set; }

        /// <param name="texture"></param>
        public UIItemSlot(Item item = null, Texture2D texture = default)
        {
            Opacity = 1f;
            ContainedItem = item ?? new Item();
            if (item != null)
            {
                Main.instance.LoadItem(item.type);
            }
            SlotBackTexture = texture == default ? TextureAssets.InventoryBack.Value : texture;
            StackColor = DrawColor = Color.White;
            CornerSize = new Vector2(10, 10);
            Tooltip = "";
            SetSize(52, 52);
        }
        public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftDown += element =>
            {
                //当鼠标没物品，框里有物品的时候
                BackItem = ContainedItem.Clone();
                if (Main.mouseItem.type == ItemID.None && ContainedItem != null && ContainedItem.type != ItemID.None)
                {
                    //如果可以拿起物品
                    if (CanTakeOutSlot == null || CanTakeOutSlot(ContainedItem))
                    {
                        //开启背包
                        Main.playerInventory = true;
                        //拿出物品
                        Main.mouseItem = ContainedItem.Clone();
                        if (!Infinity)
                        {
                            ContainedItem = new Item();
                            ContainedItem.SetDefaults(0, true);
                        }

                        //调用委托
                        PickItem();

                        //触发放物品声音
                        //SoundEngine.PlaySound(7, -1, -1, 1, 1f, 0.0f);
                    }
                }
                //当鼠标有物品，框里没物品的时候
                else if (Main.mouseItem.type != ItemID.None && (ContainedItem == null || ContainedItem.type == ItemID.None))
                {
                    //如果可以放入物品
                    if (CanPutInSlot == null || CanPutInSlot(Main.mouseItem))
                    {
                        //放入物品
                        ContainedItem = Main.mouseItem.Clone();
                        Main.mouseItem = new Item();
                        Main.mouseItem.SetDefaults(0, true);

                        //调用委托
                        PutItem();

                        //触发放物品声音
                        //SoundEngine.PlaySound(7, -1, -1, 1, 1f, 0.0f);
                    }
                }
                //当鼠标和框都有物品时
                else if (Main.mouseItem.type != ItemID.None && ContainedItem != null && ContainedItem.type != ItemID.None)
                {
                    //如果不能放入物品
                    if (!(CanPutInSlot == null || CanPutInSlot(Main.mouseItem)))
                    {
                        //中断函数
                        return;
                    }

                    //如果框里的物品和鼠标的相同
                    if (Main.mouseItem.type == ContainedItem.type)
                    {
                        //框里的物品数量加上鼠标物品数量
                        ContainedItem.stack += Main.mouseItem.stack;
                        //如果框里物品数量大于数量上限
                        if (ContainedItem.stack > ContainedItem.maxStack)
                        {
                            //计算鼠标物品数量，并将框内物品数量修改为数量上限
                            int exceed = ContainedItem.stack - ContainedItem.maxStack;
                            ContainedItem.stack = ContainedItem.maxStack;
                            Main.mouseItem.stack = exceed;
                        }
                        //反之
                        else
                        {
                            //清空鼠标物品
                            Main.mouseItem = new Item();
                        }
                    }
                    //如果可以放入物品也能拿出物品
                    else if ((CanPutInSlot == null || CanPutInSlot(Main.mouseItem))
                        && (CanTakeOutSlot == null || CanTakeOutSlot(ContainedItem)))
                    {
                        //交换框内物品和鼠标物品
                        Item tmp = Main.mouseItem.Clone();
                        Main.mouseItem = ContainedItem;
                        ContainedItem = tmp;
                    }

                    //触发放物品声音
                    //SoundEngine.PlaySound(7, -1, -1, 1, 1f, 0.0f);
                }
                //反之
                else
                {
                    //中断函数
                    return;
                }

                //调用委托
                ExchangeItem();
            };
        }
        /*public override void LoadEvents()
        {
            base.LoadEvents();
            Events.OnLeftClick += element =>
            {
                bool flag1 = Main.mouseItem.IsAir, flag2 = ContainedItem?.IsAir ?? true;
                if (flag2)
                {
                    Info.NeedRemove = true;
                    return;
                }
                if (flag1)
                {
                    Main.mouseItem = ContainedItem;
                    Main.LocalPlayer.GetModPlayer<VIPlayer>().vInventory._items[ContainedItem.type].Remove(ContainedItem);
                    Main.playerInventory = true;
                    Info.NeedRemove = true;
                }
                else
                {
                    if (Main.mouseItem.type == ContainedItem.type)
                    {
                        int count = Math.Min(Main.mouseItem.maxStack - Main.mouseItem.stack, ContainedItem.stack);
                        Main.mouseItem.stack+=count;
                        ContainedItem.stack-=count;
                        Main.playerInventory = true;
                        if (ContainedItem.IsAir)
                        {
                            Info.NeedRemove = true;
                        }
                    }
                }
            };
            Events.OnMouseHover += evt =>
            {
                if (!Main.mouseRight || ContainedItem.IsAir)
                {
                    return;
                }
                int moveCount = 0;
                RightKeepDown = true;
                if (VInventory.tryDelay > 0)
                {
                    VInventory.tryDelay--;
                }
                if (RightKeepDownTime > 15)
                {
                    if (Main.mouseItem.IsAir)
                    {
                        moveCount = 1;
                        Main.mouseItem = new(ContainedItem.type, moveCount);
                        ContainedItem.stack -= moveCount;
                    }
                    else if (Main.mouseItem.type == ContainedItem.type)
                    {
                        moveCount = Math.Max(1, (int)Math.Pow(Math.Min(10, (RightKeepDownTime - 15) / 60f), 2));
                        moveCount = Math.Min(moveCount, ContainedItem.stack);
                        moveCount = Math.Min(moveCount, Main.mouseItem.maxStack - Main.mouseItem.stack);
                        Main.mouseItem.stack += moveCount;
                        ContainedItem.stack -= moveCount;
                    }
                }
                else
                {
                    if (!PlayerInput.Triggers.Old.MouseRight && PlayerInput.Triggers.Current.MouseRight)
                    {
                        moveCount = 1;
                    }
                    else
                    {
                        return;
                    }
                    if (Main.mouseItem.IsAir)
                    {
                        moveCount = 1;
                        Main.mouseItem = new(ContainedItem.type, moveCount);
                        ContainedItem.stack -= moveCount;
                    }
                    else if (Main.mouseItem.type == ContainedItem.type)
                    {
                        moveCount = 1;
                        Main.mouseItem.stack += moveCount;
                        ContainedItem.stack -= moveCount;
                    }
                }
                if (moveCount > 0)
                {
                    Main.playerInventory = true;
                }
                if (ContainedItem.IsAir)
                {
                    Info.NeedRemove = true;
                }
            };
        }*/
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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

                //绘制物品左下角那个代表数量的数字
                if (drawStack && (ContainedItem.stack > 1 || IgnoreOne))
                {
                    sb.DrawString(font, ContainedItem.stack.ToString(), new Vector2(DrawRectangle.X + 10, DrawRectangle.Y + DrawRectangle.Height - 20), StackColor * Opacity, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
                }
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
