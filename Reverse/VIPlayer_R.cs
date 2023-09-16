using Terraria.ModLoader.IO;

namespace VoidInventory.Reverse
{
    public class VIPlayer_R : ModPlayer
    {
        public VInventory_R vInventory = new();
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
