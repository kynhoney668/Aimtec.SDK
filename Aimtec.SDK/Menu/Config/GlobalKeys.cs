namespace Aimtec.SDK.Menu.Config
{
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;

    public class GlobalKeys
    {
        internal static Menu KeyConfig;

        //Main Keys - To be enabled by default
        public static Key ComboKey { get; set; }
        public static Key MixedKey { get; set; }
        public static Key WaveClearKey { get; set; }
        public static Key LastHitKey { get; set; }

        //Additional Keys that can be enabled by assemblies if they require it
        public static Key HarassKey { get; set; }
        public static Key FreezeKey { get; set; }
        public static Key BurstKey { get; set; }
        public static Key FleeKey { get; set; }
        public static Key ComboNoOrbwalkKey { get; set; }
        public static Key TowerDiveKey { get; set; }


        internal static void Load()
        {
            KeyConfig = new Menu("Keys", "Keys");

            KeyConfig.Add(new MenuSeperator("seperator", "Main Keys"));

            ComboKey = new Key("Combo", "Combo", KeyCode.Space, KeybindType.Press, true);
            MixedKey = new Key("Mixed", "Mixed", KeyCode.C, KeybindType.Press, true);
            WaveClearKey = new Key("WaveClear", "Waveclear", KeyCode.V, KeybindType.Press, true);
            LastHitKey = new Key("LastHit", "LastHit", KeyCode.X, KeybindType.Press, true);

            KeyConfig.Add(new MenuSeperator("seperator2", "Additional Keys"));

            HarassKey = new Key("Harass", "Harass", KeyCode.H, KeybindType.Toggle, false);
            FreezeKey = new Key("Freeze", "Freeze", KeyCode.M, KeybindType.Toggle, false);
            BurstKey = new Key("Burst", "Burst", KeyCode.K, KeybindType.Press, false);
            FleeKey = new Key("Flee", "Flee", KeyCode.L, KeybindType.Press, false);
            TowerDiveKey = new Key("TowerDive", "Tower Dive", KeyCode.T, KeybindType.Press, false);

            ComboNoOrbwalkKey = new Key("ComboNoOrbwalk", "Combo - No Orbwalk", KeyCode.J, KeybindType.Toggle, false);

            AimtecMenu.Instance.Add(KeyConfig);
        }

        public class Key
        {
            internal Key(string internalName, string displayName, KeyCode keyCode, KeybindType type, bool enabled)
            {
                this.KeyBindItem = new MenuKeyBind(internalName, displayName, keyCode, type);

                if (enabled)
                {
                    this.Activate();
                }
            }

            private bool AddedToMenu { get; set; }

            //Gets whether the keybind is active
            public bool Active
            {
                get
                {
                    if (!this.AddedToMenu)
                    {
                        this.Activate();
                    }

                    return this.KeyBindItem.Value;
                }
            }

            //The Menu item associated with this Key
            public MenuKeyBind KeyBindItem { get; }


            //Enables the key by adding it to the keylist
            public void Activate()
            {
                if (!this.AddedToMenu)
                {
                    KeyConfig.Add(this.KeyBindItem);
                    this.AddedToMenu = true;
                }
            }
        }
    }
}
