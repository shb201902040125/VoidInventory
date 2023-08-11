using Terraria.UI;
using VoidInventory.Content;

namespace VoidInventory
{
    [Autoload(Side = ModSide.Client)]
    public class VISystem : ModSystem
    {
        public static ModKeybind Keybind { get; private set; }
        public static Vector2? drawIME;
        public Point size;
        public override void Load()
        {
            Keybind = KeybindLoader.RegisterKeybind(Mod, Mod.Name, "K");
            VoidInventory.Ins.uis = new();
            VoidInventory.Ins.uis.Load();
            Main.OnResolutionChanged += (evt) =>
            {
                VoidInventory.Ins.uis.Calculation();
                VoidInventory.Ins.uis.OnResolutionChange();
            };
            /*if (ModLoader.TryGetMod("XXX", out Mod mod))
            {
                if (mod.TryFind("X", out GlobalItem item))
                {
                    Item i = new(77);
                    if (i.TryGetGlobalItem(item, out GlobalItem result))
                    {

                    }

                }

            }*/

        }
        public override void UpdateUI(GameTime gt)
        {
            base.UpdateUI(gt);
            if (size != Main.ScreenSize)
            {
                size = Main.ScreenSize;
                VoidInventory.Ins.uis.Calculation();
            }
            VoidInventory.Ins.uis.Update(gt);
            if (Keybind.JustPressed)
            {
                VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
                ui.OnInitialization();
                ui.ChangeItem(ItemID.RottenChunk);
                ref bool visible = ref ui.Info.IsVisible;
                visible = true;
            }
        }
        public override void PreSaveAndQuit()
        {
            VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
            ui.Info.IsVisible = false;
            ui.firstLoad = false;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                   "VoidInventory: VISystem",
                   delegate
                   {
                       VoidInventory.Ins.uis.Draw(Main.spriteBatch);
                       return true;
                   },
                   InterfaceScaleType.UI)
               );
            }
        }
    }
}
