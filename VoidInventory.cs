using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using VoidInventory.UISupport;

namespace VoidInventory
{
    public class VoidInventory : Mod
    {
        internal static VoidInventory Ins;
        public UISystem uis;
        public VoidInventory()
        {
            Ins = this;
        }
        public override void Load()
        {
        }
    }
}