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
    }
}