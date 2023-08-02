using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VoidInventory
{
    public class VIPlayer:ModPlayer
    {
        VInventory vInventory = new();
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
