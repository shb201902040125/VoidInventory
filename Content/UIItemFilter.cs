using static VoidInventory.VInventory.Filters;

namespace VoidInventory.Content
{
    public static class ItemFilter
    {
        public const byte Weapon = 0;
        public const byte Accessory = 1;
        public const byte Armor = 2;
        public const byte Consumable = 3;
        public const byte BuildingBlock = 4;
        public const byte Material = 5;
        public const byte Tool = 6;
        public const byte Furniture = 7;
        public const byte Vanity = 8;
        public const byte MiscEquip = 9;
        public const byte Misc = 10;
    }
    public class UIItemFilter : UIImage
    {
        /// <summary>
        /// 0武器，1饰品，2盔甲，3药剂
        /// <br/>4物块，5工具，6材料，7家具
        /// <br/>8时装，9配件
        /// </summary>
        public int Filter { get; private set; }
        private bool open;
        public bool Open => open;
        private readonly Action<bool> FilterEvent;
        public readonly string prompt;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">使用<see cref="ItemFilter"/></param>
        /// <param name="tex"></param>
        /// <exception cref="Exception"></exception>
        public UIItemFilter(int filter, Action<bool> filterEvent, Texture2D tex = null) : base(tex ?? T2D("Terraria/Images/UI/Creative/Infinite_Icons"), new(30), Color.White)
        {
            if (filter > 10 && tex == null)
            {
                throw new Exception("超出原版筛选贴图，请传入");
            }
            FilterEvent = filterEvent;
            Filter = filter;
            prompt = Filter switch
            {
                ItemFilter.Weapon => GTV("IsWeapon"),
                ItemFilter.Accessory => GTV("IsAccessory"),
                ItemFilter.Armor => GTV("IsArmor"),
                ItemFilter.Consumable => GTV("IsConsumable"),
                ItemFilter.BuildingBlock => GTV("IsBuildingBlock"),
                ItemFilter.Material => GTV("IsMaterial"),
                ItemFilter.Tool => GTV("IsTool"),
                ItemFilter.Furniture => GTV("IsFurniture"),
                ItemFilter.Vanity => GTV("IsVanity"),
                ItemFilter.MiscEquip => GTV("IsMiscEquip"),
                ItemFilter.Misc => GTV("IsMisc"),
                _ => throw new Exception("筛选序列溢出")
            };
        }
        public override void LoadEvents()
        {
            Events.OnLeftDown += evt =>
            {
                Reversal(ref open);
                FilterEvent.Invoke(Open);
            };
        }
        public Predicate<Item> GetFilter()
        {
            return Filter switch
            {
                ItemFilter.Weapon => IsWeapon,
                ItemFilter.Accessory => IsAccessory,
                ItemFilter.Armor => IsArmor,
                ItemFilter.Consumable => IsConsumable,
                ItemFilter.BuildingBlock => IsBuildingBlock,
                ItemFilter.Material => IsMaterial,
                ItemFilter.Tool => IsTool,
                ItemFilter.Furniture => IsFurniture,
                ItemFilter.Vanity => IsVanity,
                ItemFilter.MiscEquip => IsMiscEquip,
                ItemFilter.Misc => Misc(IsWeapon, IsAccessory, IsArmor, IsConsumable, IsBuildingBlock, IsMaterial, IsTool, IsFurniture, IsVanity, IsMiscEquip),
                _ => throw new Exception("筛选序列溢出")
            };
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            SimpleDraw(sb, Tex, HitBox().TopLeft(), Filter > 10 ? null : new(Filter * 30, 0, 30, 30),
                Vector2.Zero, null, open ? Color.Gold.SetAlpha(100) : Color.White);
            if (Info.IsMouseHover) Main.hoverItemName = prompt;
        }
    }
}
