using System.Linq;

namespace VoidInventory.Content
{
    public class VIUI : ContainerElement
    {
        public const string NameKey = "VoidInventory.Content.VIUI";
        public UIPanel bg, dbg;
        public UIBottom left, right;
        public UIContainerPanel leftView, rightView;
        public UIInputBox input;
        public int focusType = -1;
        public Dictionary<int, UIItemTex> items = new();
        public override void OnInitialization()
        {
            base.OnInitialization();
            RemoveAll();
            bg = new(default, 800, 600);
            bg.SetCenter(0, 0, 0.5f, 0.5f);
            bg.CanDrag = true;
            Register(bg);

            input = new(color: Color.White);
            input.SetSize(160, 36);
            input.SetPos(-input.Width - 20, 20 + 10, 1);
            input.OnInputText += () =>
            {
            };
            input.DrawRec[0] = Color.Red;
            bg.Register(input);

            UIText RT = new("合成");
            RT.SetSize(RT.TextSize);
            RT.SetCenter(0, 30, 0.5f, 0);
            RT.Events.OnLeftClick += evt =>
            {
                Info.IsVisible = false;
                var rtui = VoidInventory.Ins.uis.Elements[RTUI.NameKey];
                rtui.Info.IsVisible = true;
                ((RTUI)rtui).bg.SetPos(bg.Info.TotalLocation);
                rtui.Calculation();
            };
            bg.Register(RT);

            left = new(-40, -102, 0.5f, 1f);
            left.SetPos(20, 82);
            left.DrawRec[0] = Color.White;
            bg.Register(left);

            leftView = new();
            leftView.Info.SetMargin(0);
            leftView.Events.OnLeftClick += evt =>
            {
                if (Main.mouseItem.type > ItemID.None)
                {
                    Main.LocalPlayer.VIP().vInventory.Merga(ref Main.mouseItem);
                }
            };
            left.Register(leftView);

            VerticalScrollbar leftscroll = new(62);
            leftscroll.Info.IsHidden = true;
            leftscroll.Info.Left.Pixel += 10;
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(-40, -102, 0.5f, 1f);
            right.SetPos(20, 82, 0.5f);
            right.DrawRec[0] = Color.White;
            bg.Register(right);

            rightView = new();
            rightView.Info.SetMargin(0);
            right.Register(rightView);

            VerticalScrollbar rightscroll = new(62);
            rightscroll.Info.IsHidden = true;
            rightscroll.Info.Left.Pixel += 10;
            rightView.SetVerticalScrollbar(rightscroll);
            right.Register(rightscroll);

            dbg = new(default, 320, 500);
            dbg.SetPos(0, 0, 0.1f, 0.35f);
            dbg.Info.IsVisible = false;
            dbg.CanDrag = true;
            Register(dbg);
        }
        public void SortVI(int id)
        {
            foreach (UIItemTex item in leftView.InnerUIE.Cast<UIItemTex>())
            {
                if (item.id > id)
                {
                    item.id--;
                    item.SetPos(id % 6 * 56 + 10, id / 6 * 56 + 10);
                }
            }
            leftView.Calculation();
        }
    }
}
