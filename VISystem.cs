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
                /*if (!ui.firstLoad)
                {
                    ui.OnInitialization();
                    ui.firstLoad = true;
                }*/
                //ui.ChangeItem(ItemID.RottenChunk);
                ref bool visible = ref ui.Info.IsVisible;
                visible = !visible;
            }
        }
        public override void PreSaveAndQuit()
        {
            RTUI ui = VoidInventory.Ins.uis.Elements[RTUI.NameKey] as RTUI;
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
