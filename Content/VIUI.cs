using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace VoidInventory.Content
{
    public class VIUI : ContainerElement
    {
        public static VIUI VI => VoidInventory.Ins.uis.Elements[NameKey] as VIUI;
        public const string NameKey = "VoidInventory.Content.VIUI";
        public UIPanel bg, fbg;
        public UIPanel left, right;
        public UIContainerPanel leftView, rightView;
        public UIInputBox input;
        public int focusType = ItemID.None;
        private int timer = 30;
        private bool taking;
        private int takeTime;
        private int takeSpeed = 10;
        private int takeStack = 1;
        public static VInventory Inv => Main.LocalPlayer.VIP().vInventory;
        public static Dictionary<int, List<Item>> InvItems => Inv._items;
        public IEnumerable<UIItemTex> Items => leftView.InnerUIE.Cast<UIItemTex>();
        public IEnumerable<UIItemFilter> Filters => fbg.ChildrenElements.Cast<UIItemFilter>();
        public override void OnInitialization()
        {
            base.OnInitialization();
            if (Main.gameMenu) return;
            RemoveAll();
            bg = new(820, 600);
            bg.SetCenter(0, 0, 0.5f, 0.5f);
            bg.CanDrag = true;
            bg.Info.HiddenOverflow = true;
            bg.Events.OnLeftDown += evt => focusType = 0;
            Register(bg);

            UIText RT = new("合成");
            RT.SetSize(RT.TextSize);
            RT.SetCenter(0, 30, 0.5f, 0);
            RT.Events.OnLeftClick += evt =>
            {
                Info.IsVisible = false;
                RTUI rtui = VoidInventory.Ins.uis.Elements[RTUI.NameKey] as RTUI;
                rtui.Info.IsVisible = true;
                rtui.bg.SetPos(bg.Info.TotalLocation);
                rtui.Calculation();
                VIPlayer.vi = false;
            };
            bg.Register(RT);

            left = new(0, 0);
            left.SetSize(-40, -102, 1, 1);
            left.SetPos(20, 82);
            left.drawBoeder = false;
            bg.Register(left);

            leftView = new();
            leftView.Events.OnLeftDown += evt =>
            {
                Item item = Main.mouseItem;
                if (item.type > ItemID.None && item.stack > 0)
                {
                    Inv.Merge(ref Main.mouseItem);
                    SoundEngine.PlaySound(SoundID.Grab);
                    if (item.type == focusType)
                        RefreshRight();
                }
            };
            left.Register(leftView);

            fbg = new(10 + 11 * 35, 40);
            fbg.Info.SetMargin(5);
            fbg.SetPos(20, 42);
            fbg.drawBoeder = false;
            bg.Register(fbg);

            int[] fs = new int[] { 0, 2, 8, 4, 7, 1, 9, 3, 6, 5, 10 };
            for (int i = 0; i < fs.Length; i++)
            {
                UIItemFilter filters = new(fs[i], open => RefreshLeft(null));
                filters.SetPos(i * 35, 0);
                fbg.Register(filters);
            }

            UIPanel inputbg = new(160 + 24, 30, 12, 4, Color.White, 1f);
            inputbg.SetPos(-inputbg.Width - 20, 42, 1);
            bg.Register(inputbg);

            input = new("搜索背包物品", color: Color.Black);
            input.SetSize(-24, 0, 1, 1);
            input.SetPos(12, 2);
            input.OnInputText += () => RefreshLeft(null);
            inputbg.Register(input);

            VerticalScrollbar leftscroll = new(62 * 3);
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(0, 0);
            right.SetSize(-40, -102, 0.5f, 1);
            right.SetPos(20, 82, 0.5f);
            right.drawBoeder = false;
            bg.Register(right);

            rightView = new();
            right.Register(rightView);

            VerticalScrollbar rightscroll = new(62 * 3);
            rightView.SetVerticalScrollbar(rightscroll);
            right.Register(rightscroll);
            right.Info.IsVisible = false;
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            //缩回
            if (focusType > 0 && timer > 0)
            {
                left.Info.Width.Percent = MathF.Pow(--timer / 30f, 3f) / 2f + 0.5f;
                if (timer == 0)
                {
                    right.Info.IsVisible = true;
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                left.Calculation();
                RefreshLeft();
            }
            //展开
            else if (focusType == 0 && timer < 30)
            {
                left.Info.Width.Percent = MathF.Pow(++timer / 30f, 0.33f) / 2f + 0.5f;
                Reversal(ref right.Info.IsVisible, true,(open) => SoundEngine.PlaySound(SoundID.MenuClose));
                left.Calculation();
                RefreshLeft();
            }
        }
        public override void OnSaveAndQuit()
        {
            RemoveAll();
        }
        /// <summary>
        /// 刷新物品索引背包
        /// </summary>
        /// <param name="sortOnly">true排序，false变动，null筛选</param>
        public void RefreshLeft(bool? sortOnly = true)
        {
            if (leftView != null)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                if (sortOnly is null or false)
                {
                    List<Predicate<Item>> matchs = new();
                    foreach (UIItemFilter f in Filters)
                    {
                        if (f.Open)
                        {
                            matchs.Add(f.GetFilter());
                        }
                    }
                    if (sortOnly == null)
                    {
                        leftView.ClearAllElements();
                        foreach (int itemType in InvItems.Keys)
                        {
                            RegisterIndexUI(itemType, matchs);
                        }
                    }
                    else
                    {
                        List<int> hasItems = Items.Select(i => i.ContainedItem.type).ToList();
                        List<int> nowItems = InvItems.Keys.ToList();
                        var remove = hasItems.Except(nowItems);
                        var add = nowItems.Except(hasItems);
                        leftView.InnerUIE.RemoveAll(i => i is UIItemTex t && remove.Contains(t.ContainedItem.type));
                        foreach (int itemType in add)
                        {
                            RegisterIndexUI(itemType, matchs);
                        }
                    }
                    RefreshLeft();
                }
                else
                {
                    int x = 0, y = 0;
                    foreach (UIItemTex item in Items)
                    {
                        item.SetPos(x, y);
                        x += 56;
                        if (x + 56 > leftView.InnerWidth)
                        {
                            x = 0;
                            y += 56;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 刷新实例背包，会重新注册UI
        /// </summary>
        public void RefreshRight()
        {
            if (focusType == 0) return;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            rightView.ClearAllElements();
            int count = 0;
            foreach (Item item in InvItems[focusType])
            {
                if (item.IsAir) continue;
                UIItemSlot slot = new(item)
                {
                    CanTakeOutSlot = new(x => true),
                    CanPutInSlot = new(x => x.type == focusType),
                };
                slot.Events.OnUpdate += evt => TakeItemWithRight(slot);
                slot.Events.OnRightDown += evt =>
                {
                    ref Item i = ref Main.mouseItem;
                    if (i.type > ItemID.None && i.stack == i.maxStack) return;
                    Item s = slot.ContainedItem;
                    if (!s.IsAir && (i.IsAir || s.type == i.type))
                    {
                        Reversal(ref Main.playerInventory, false);
                        if (i.IsAir)
                        {
                            i = s.Clone();
                            i.stack = 1;
                        }
                        else
                        {
                            i.stack++;
                        }
                        s.stack--;
                        if (s.IsAir)
                        {
                            slot.PickItem();
                            return;
                        }
                        takeTime = 0;
                        takeSpeed = 10;
                        takeStack = 1;
                        taking = true;
                    }
                };
                slot.Events.OnMouseOut += evt =>
                {
                    if (taking)
                    {
                        taking = false;
                        slot.PickItem();
                    }
                };
                slot.Events.OnRightUp += evt =>
                {
                    if (taking)
                    {
                        taking = false;
                        slot.PickItem();
                    }
                };
                slot.OnPickItem += evt =>
                {
                    List<Item> target = InvItems[focusType];
                    // 拿走了全部物品，直接移除
                    if (slot.ContainedItem.IsAir)
                    {
                        target.RemoveAt(slot.id);
                    }
                    // 没拿走全部，刷新存储的物品
                    else
                    {
                        target[slot.id] = slot.ContainedItem;
                    }
                };
                slot.OnPutItem += evt =>
                {
                    List<Item> target = InvItems[focusType];
                    // 放入时原本没有物品，则在对应索引插入当前物品
                    if (slot.BackItem.IsAir)
                    {
                        target.Insert(slot.id, slot.ContainedItem);
                    }
                    // 放入时原本有物品
                    else
                    {
                        target[slot.id] = slot.ContainedItem;
                    }
                };
                slot.PostExchangeItem += evt =>
                {
                    InvItems[focusType][slot.id] = slot.ContainedItem;
                };
                slot.SetPos(count % 6 * 56, count / 6 * 56);
                rightView.AddElement(slot);
                count++;
            }
        }
        private void TakeItemWithRight(UIItemSlot slot)
        {
            if (taking && slot.Info.IsMouseHover)
            {
                Item item = InvItems[focusType][slot.id];
                ref Item i = ref Main.mouseItem;
                if (takeSpeed == 0 || (takeTime % takeSpeed == 0 && takeSpeed < 10))
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    for (int j = 0; j < takeStack; j++)
                    {
                        if (i.stack == i.maxStack || item.stack == 0)
                        {
                            taking = false;
                            if (item.stack == 0)
                            {
                                item.SetDefaults();
                            }
                            slot.PickItem();
                            return;
                        }
                        i.stack++;
                        item.stack--;
                    }
                }

                if (takeSpeed > 10)
                {
                    if (takeTime >= 20)
                    {
                        takeSpeed--;
                    }
                }
                else if (takeSpeed > 0)
                {
                    if (takeTime >= takeSpeed * 2)
                    {
                        takeTime = 0;
                        takeSpeed--;
                    }
                }
                else
                {
                    if (takeTime % 10 == 0)
                    {
                        takeStack++;
                        takeTime = 0;
                    }
                }
                takeTime++;
            }
        }
        /// <summary>
        /// 向索引背包注册元素，配合<see cref="RefreshLeft(bool)"/>来整理元素
        /// 过不了当前筛选不注册
        /// </summary>
        /// <param name="type"></param>
        public void RegisterIndexUI(int type, List<Predicate<Item>> matchs)
        {
            UIItemTex tex = new(type);
            bool filter = false;
            if (matchs.Any())
            {
                foreach (Predicate<Item> match in matchs)
                {
                    if (match(tex.ContainedItem))
                    {
                        filter = true;
                        break;
                    }
                }
            }
            else filter = true;
            if (!filter) return;
            if (input.Text.Length > 0 && !tex.ContainedItem.Name.Contains(input.Text))
            {
                return;
            }
            tex.Events.OnLeftDown += evt =>
            {
                focusType = tex.ContainedItem.type;
                RefreshRight();
            };
            leftView.AddElement(tex);
        }
    }
}
