namespace VoidInventory.Content
{
    public class UIGroupItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool isGroupItem;
        public bool protect;
        public static readonly string Locked = GTV("Locked");
        public static readonly string Tolocked = GTV("ToLock");
        public static readonly string Protect = GTV("Protect");
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (isGroupItem)
            {
                string text = protect ? Locked : Tolocked;
                tooltips.Add(new(Mod, "Protect", text + "\n" + Protect));
            }
        }
    }
}
