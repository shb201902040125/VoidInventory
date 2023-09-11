using static VoidInventory.VInventory.Filters;

namespace VoidInventory.Content
{
    public static class ItemFilter
    {
        public const byte Weapon = 0;
        public const byte Accessory = 1;
        public const byte Armor = 2;
        public const byte Potion = 3;
        public const byte Block = 4;
        public const byte Tool = 5;
        public const byte Material = 6;
        public const byte Furniture = 7;
        public const byte Vanity = 8;
        public const byte Attachment = 9;
        //public const byte Favorite = 10;
    }
    public class UIItemFilter : UIImage
    {
        /// <summary>
        /// 0武器，1饰品，2盔甲，3药剂
        /// <br/>4物块，5工具，6材料，7家具
        /// <br/>8时装，9配件
        /// </summary>
        public int Filter { get; private set; }
        public readonly VIUI viui;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">使用<see cref="ItemFilter"/></param>
        /// <param name="tex"></param>
        /// <exception cref="Exception"></exception>
        public UIItemFilter(int filter, VIUI viui, Texture2D tex = null) : base(tex ?? T2D("Terraria/Images/UI/Creative/Infinite_Icons"), new(30), Color.White)
        {
            if (filter > 9 && tex == null)
            {
                throw new Exception("超出原版筛选贴图，请传入");
            }
            this.viui = viui;
            Filter = filter;
        }
        public override void LoadEvents()
        {
            Events.OnLeftDown += evt =>
            {
                if (viui.focusFilter != Filter)
                {
                    viui.focusFilter = Filter;
                    Predicate<Item> filters = Filter switch
                    {
                        ItemFilter.Weapon => (item) => IsWeapon(item) && !IsAccessory(item),
                        ItemFilter.Accessory => IsAccessory,
                        ItemFilter.Armor => IsArmor,
                        ItemFilter.Potion => IsBuff,
                        ItemFilter.Block => IsPlaceable,
                        ItemFilter.Tool => IsTool,
                        ItemFilter.Material => IsMaterial,
                        ItemFilter.Furniture => IsPlaceable,
                        ItemFilter.Vanity => IsVanity,
                        ItemFilter.Attachment => (item) => IsPet(item) && IsDye(item) && IsMount(item),
                        //ItemFilter.Favorite =>,
                        _ => throw new Exception("筛选序列溢出")
                    };
                    viui.leftView.ClearAllElements();
                    foreach ((int type, List<Item> targets) in Main.LocalPlayer.VIP().vInventory.Filter(filters))
                    {
                        UIItemTex item = new(type);
                        viui.LoadClickEvent(item, type, targets);
                        viui.leftView.AddElement(item);
                        if (item.ContainedItem.type == ItemID.Starfury)
                        {
                            Main.NewText(IsTool(item.ContainedItem));
                        }
                    }
                    viui.SortLeft();
                }
                else
                {
                    viui.focusFilter = -1;
                    viui.FindInvItem();
                }
            };
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            SimpleDraw(sb, Tex, HitBox().TopLeft(), Filter > 10 ? null : new(Filter * 30, 0, 30, 30),
                Vector2.Zero, null, viui.focusFilter == Filter ? Color.Gold.SetAlpha(150) : Color.White);
        }
    }
}
