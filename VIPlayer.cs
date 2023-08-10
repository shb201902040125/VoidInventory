using Terraria.ModLoader.IO;

namespace VoidInventory
{
    public class VIPlayer : ModPlayer
    {
        public VInventory vInventory = new();
        public override void PostUpdate()
        {
            vInventory.NormalUpdateCheck();
        }
        public override void SaveData(TagCompound tag)
        {
            vInventory.Save(tag);
        }
        public override void LoadData(TagCompound tag)
        {
            vInventory.Load(tag);
        }
    }
}
