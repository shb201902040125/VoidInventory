using System.Linq;

namespace VoidInventory.Content
{
    public class VIUI : ContainerElement
    {
        public const string NameKey = "VoidInventory.Content.VIUI";
        public UIPanel bg;
        public UIPanel left, right;
        public UIContainerPanel leftView, rightView;
        public UIInputBox input;
        public int focusType = -1;
        public IEnumerable<UIItemTex> Items => leftView.InnerUIE.Cast<UIItemTex>();
        public override void OnInitialization()
        {
            base.OnInitialization();
            RemoveAll();
            bg = new(800, 600);
            bg.SetCenter(0, 0, 0.5f, 0.5f);
            bg.CanDrag = true;
            Register(bg);

            /*UIPanel inputbg = new(160 + 24, 36, 12, 4, Color.White, 1f);
            inputbg.SetPos(-inputbg.Width - 20, 20 + 10, 1);
            bg.Register(inputbg);

            input = new("搜索背包物品", color: Color.White);
            input.SetSize(-24, 0, 1, 1);
            input.SetPos(12, 0);
            input.OnInputText += FindInvItem;
            inputbg.Register(input);*/

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
            left.SetSize(-40, -102, 0.5f, 1);
            left.SetPos(20, 82);
            bg.Register(left);

            leftView = new();
            leftView.Info.SetMargin(0);
            leftView.Events.OnLeftClick += evt =>
            {
                if (Main.mouseItem.type > ItemID.None)
                {
                    Main.LocalPlayer.VIP().vInventory.Merge(ref Main.mouseItem);
                }
            };
            left.Register(leftView);

            VerticalScrollbar leftscroll = new(62);
            leftscroll.Info.IsHidden = true;
            leftscroll.Info.Left.Pixel += 10;
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(0, 0);
            right.SetSize(-40, -102, 0.5f, 1);
            right.SetPos(20, 82, 0.5f);
            bg.Register(right);

            rightView = new();
            rightView.Info.SetMargin(0);
            right.Register(rightView);

            VerticalScrollbar rightscroll = new(62);
            rightscroll.Info.IsHidden = true;
            rightscroll.Info.Left.Pixel += 10;
            rightView.SetVerticalScrollbar(rightscroll);
            right.Register(rightscroll);
        }
        public void SortLeft()
        {
            int count = 0;
            foreach (UIItemTex item in leftView.InnerUIE.Cast<UIItemTex>())
            {
                item.SetPos((count % 6 * 56) + 10, (count / 6 * 56) + 10);
                count++;
            }
            leftView.Calculation();
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
                slot.SetPos((count % 6 * 56) + 10, (count / 6 * 56) + 10);
                Item target = item;
                slot.OnPickItem += uie =>
                {
                    targetItems.Remove(target);
                    if (targetItems.Count == 0)
                    {
                        rightView.ClearAllElements();
                        focusType = -1;
                        leftView.InnerUIE.RemoveAll(x => x is UIItemTex tex && tex.ContainedItem.type == target.type);
                        SortLeft();
                    }
                    else
                    {
                        SortRight(targetItems);
                    }
                };
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
        private void FindInvItem()
        {

        }
    }
}
