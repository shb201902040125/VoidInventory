using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidInventory
{
    internal class VIItem : GlobalItem
    {
        public override bool ItemSpace(Item item, Player player)
        {
            if (VIConfig.grabItemDirectly)
            {
                return true;
            }
            return base.ItemSpace(item, player);
        }
        public override bool OnPickup(Item item, Player player)
        {
            if (VIConfig.grabItemDirectly && player.TryGetModPlayer(out VIPlayer vplayer))
            {
                if (vplayer.vInventory._items.ContainsKey(item.type))
                {
                    vplayer.vInventory.Merge(ref item);
                    return false;
                }
            }
            return true;
        }
    }
}
