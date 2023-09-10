using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace VoidInventory
{
    internal class VIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(false)]
        [LabelKey("$Mods.VoidInventory.Configs.VIConfig.SubstituteVanillaCurrencySystem.Label")]
        public bool SubstituteVanillaCurrencySystem;

        [DefaultValue(false)]
        [LabelKey("$Mods.VoidInventory.Configs.VIConfig.GrabItemDirectly.Label")]
        public bool GrabItemDirectly;
        internal static bool grabItemDirectly;

        [DefaultValue(5)]
        [Range(1,60)]
        [LabelKey("$Mods.VoidInventory.Configs.VIConfig.NormalUpdateCheck.Label")]
        public int NormalUpdateCheckTime;
        internal static int normalUpdateCheckTime;

        [DefaultValue(true)]
        [LabelKey("$Mods.VoidInventory.Configs.VIConfig.EnableReciptTaskReport.Label")]
        public bool EnableRecipeTaskReport;
        internal static bool enableRecipeTaskReport;

        public override void OnChanged()
        {
            if(SubstituteVanillaCurrencySystem)
            {
                VInventory.Hook.LoadCurrencyHook();
            }
            else
            {
                VInventory.Hook.UnloadCurrencyHook();
            }
            grabItemDirectly = GrabItemDirectly;
            normalUpdateCheckTime = NormalUpdateCheckTime;
            enableRecipeTaskReport = EnableRecipeTaskReport;
        }
    }
}
