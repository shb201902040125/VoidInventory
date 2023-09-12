using System.Linq;
using Terraria.GameInput;

namespace VoidInventory.Content
{
    public class VIUI : ContainerElement
    {
        public const string NameKey = "VoidInventory.Content.VIUI";
        public UIPanel bg, fbg;
        public UIPanel left, right;
        public UIContainerPanel leftView, rightView;
        public UIInputBox input;
        public int focusType = ItemID.None;
        public int focusFilter = -1;
        private int timer = 30;
        public IEnumerable<UIItemTex> Items => leftView.InnerUIE.Cast<UIItemTex>();
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
            bg.Register(left);

            leftView = new();
            leftView.Events.OnLeftClick += evt =>
            {
                if (Main.mouseItem.type > ItemID.None && Main.mouseItem.stack > 0)
                {
                    int type = Main.mouseItem.type;
                    var inv = Main.LocalPlayer.VIP().vInventory;
                    inv.Merge(ref Main.mouseItem);
                    SortRight(inv._items[type]);
                }
            };
            left.Register(leftView);

            fbg = new(10 + 11 * 35, 40);
            fbg.Info.SetMargin(5);
            fbg.SetPos(20, 42);
            bg.Register(fbg);

            int i = 0;
            void SetFilter(int  filter)
            {
                UIItemFilter filters = new(filter, this);
                filters.SetPos(i++ * 35, 0);
                fbg.Register(filters);
            }
            SetFilter(ItemFilter.Weapon);
            SetFilter(ItemFilter.Armor);
            SetFilter(ItemFilter.Vanity);
            SetFilter(ItemFilter.BuildingBlock);
            SetFilter(ItemFilter.Furniture);
            SetFilter(ItemFilter.Accessory);
            SetFilter(ItemFilter.MiscEquip);
            SetFilter(ItemFilter.Consumable);
            SetFilter(ItemFilter.Tool);
            SetFilter(ItemFilter.Material);
            SetFilter(ItemFilter.Misc);

            UIPanel inputbg = new(160 + 24, 30, 12, 4, Color.White, 1f);
            inputbg.SetPos(-inputbg.Width - 20, 20 + 10, 1);
            bg.Register(inputbg);

            input = new("搜索背包物品", color: Color.Black);
            input.SetSize(-24, 0, 1, 1);
            input.SetPos(12, 2);
            input.OnInputText += () =>
            {
                focusFilter = -1;
                FindInvItem();
            };
            inputbg.Register(input);

            VerticalScrollbar leftscroll = new(62 * 3);
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(0, 0);
            right.SetSize(-40, -102, 0.5f, 1);
            right.SetPos(20, 82, 0.5f);
            bg.Register(right);

            rightView = new();
            rightView.Events.OnLeftClick+= evt =>
            {
                if (Main.mouseItem.type > ItemID.None && Main.mouseItem.stack > 0)
                {
                    int type = Main.mouseItem.type;
                    var inv = Main.LocalPlayer.VIP().vInventory;
                    inv.Merge(ref Main.mouseItem);
                    SortRight(inv._items[type]);
                }
            };
            rightView.Events.OnUpdate += evt =>
            {
                if (rightView.InnerUIE.Count == 0)
                {
                    leftView.InnerUIE.RemoveAll(x => x is UIItemTex tex && tex.ContainedItem.type == focusType);
                    focusType = 0;
                }
            };
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
                if (timer == 0) right.Info.IsVisible = true;
                SortLeft();
            }
            //展开
            else if (focusType == 0 && timer < 30)
            {
                left.Info.Width.Percent = MathF.Pow(++timer / 30f, 0.33f) / 2f + 0.5f;
                Reversal(ref right.Info.IsVisible, true);
                SortLeft();
            }
        }
        public void SortLeft()
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
            Calculation();
        }
        public void SortRight(List<Item> targetItems)
        {
            rightView.ClearAllElements();
            int count = 0;
            foreach (Item item in targetItems)
            {
                UIItemSlot slot = new(item)
                {
                    CanTakeOutSlot = new(x => true),
                };
                slot.SetPos(count % 6 * 56, count / 6 * 56);
                rightView.AddElement(slot);
                count++;
            }
        }
        public void LoadClickEvent(UIItemTex tex, int type, List<Item> targetItems)
        {
            tex.Events.OnLeftDown += evt =>
            {
                focusType = type;
                SortRight(targetItems);
            };
        }
        public void FindInvItem()
        {
            leftView.ClearAllElements();
            VInventory inv = Main.LocalPlayer.VIP().vInventory;
            Dictionary<int, List<Item>> willPut = inv.Filter(item => input.Text.Length == 0 || item.Name.Contains(input.Text));
            List<int> keys = willPut.Keys.ToList();
            keys.Sort();
            foreach (int key in keys)
            {
                UIItemTex tex = new(key);
                LoadClickEvent(tex, key, willPut[key]);
                leftView.AddElement(tex);
            }
            SortLeft();
        }
    }
}
