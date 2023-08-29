﻿using Terraria.GameInput;
using Terraria.ModLoader.IO;
using VoidInventory.Content;

namespace VoidInventory
{
    public class VIPlayer : ModPlayer
    {
        internal static bool vi = true;
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
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (VISystem.Keybind.JustPressed)
            {
                RTUI r = GetUI(RTUI.NameKey) as RTUI;
                VIUI v = GetUI(VIUI.NameKey) as VIUI;
                if (!r.firstLoad)
                {
                    r.OnInitialization();
                    foreach (int item in vInventory.items.Keys)
                    {
                        UIItemTex tex = new(item);
                        v.LoadClickEvent(tex, item, vInventory.items[item]);
                        v.leftView.AddElement(tex);
                    }
                    v.SortLeft();
                    r.firstLoad = true;
                }
                BaseUIElement u = vi ? v : r;
                ref bool visible = ref u.Info.IsVisible;
                visible = !visible;
            }
        }
        static BaseUIElement GetUI(string name) => VoidInventory.Ins.uis.Elements[name];
    }
}
