namespace Aimtec.SDK.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Menu.Theme.Default;
    using Aimtec.SDK.Util;

    internal class MenuManager
    {
        #region Fields

        private int lastXMosPos;

        private int lastYMosPos;

        private bool visible;

        #endregion

        #region Constructors and Destructors

        private MenuManager()
        {
            if (!Directory.Exists(AppDataConfigPath))
            {
                Directory.CreateDirectory(AppDataConfigPath);
            }

            if (!Directory.Exists(MenuSettingsPath))
            {
                Directory.CreateDirectory(MenuSettingsPath);
            }

            RenderManager.OnPresent += () => this.Render(this.Position);
            Game.OnWndProc += args => this.WndProc(args.Message, args.WParam, args.LParam);

            Game.OnEnd += delegate { this.Save(); };
            AppDomain.CurrentDomain.DomainUnload += delegate { this.Save(); };
            AppDomain.CurrentDomain.ProcessExit += delegate { this.Save(); };

            // TODO: Make this load from settings
            this.Theme = new DefaultMenuTheme();
        }

        #endregion

        #region Internal Properties

        internal string AppDataConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimTec.SDK");

        internal string MenuSettingsPath => Path.Combine(AppDataConfigPath, "MenuSettings");

        internal string SharedSettingsPath => Path.Combine(MenuSettingsPath, "SharedConfig");

        #endregion

        #region Public Properties

        public static MenuManager Instance { get; } = new MenuManager();

        public Dictionary<string, MenuComponent> Children { get; } = new Dictionary<string, MenuComponent>();

        public T As<T>()
            where T : MenuComponent
        {
            throw new NotImplementedException();
        }

        public IMenuComponent this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DisplayName { get; set; } = string.Empty;

        public string InternalName { get; set; } = "AimtecSDK-RootMenu";

        public bool IsMenu
        {
            get
            {
                return false;
            }
        }

        public Menu Parent { get; set; }

        public Vector2 Position { get; set; } = new Vector2(10, 10);

        public bool Enabled { get; }

        public int Value { get; }

        public bool Root { get; set; }

        public MenuTheme Theme { get; set; }

        public bool Toggled { get; set; }

        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                foreach (var child in this.Menus)
                {
                    child.Visible = value;
                }

                this.visible = true;
            }
        }

        #endregion

        #region Properties

        internal IReadOnlyList<MenuComponent> Menus => this.Children.Values.Where(x => x.IsMenu).ToList();


        #endregion

        #region Public Methods and Operators

        public void Add(MenuComponent menu)
        {
            this.Children.Add(menu.InternalName, menu);
        }

        public Rectangle GetBounds(Vector2 pos)
        {
            return new Rectangle(
                (int) pos.X,
                (int) pos.Y,
                this.Root ? this.Theme.RootMenuWidth : this.Theme.ComponentWidth,
                this.Theme.MenuHeight * this.Menus.Count);
        }

        public IRenderManager GetRenderManager()
        {
            return null;
        }

        public void Save()
        {
            foreach (var mc in Menus)
            {
                var m = (Menu)mc;
                m.SaveValue();
            }
        }

        public void Render(Vector2 pos)
        {
            if (!this.Visible)
            {
                return;
            }

            for (var i = 0; i < this.Menus.Count; i++)
            {
                var position = this.Position + new Vector2(0, i * this.Theme.MenuHeight);

                this.Menus[i].Position = position;
                this.Menus[i].Render(this.Menus[i].Position);
            }
        }

        public void WndProc(uint message, uint wparam, int lparam)
        {
            // Drag menu
            if (message == (int) WindowsMessages.WM_KEYDOWN && wparam == (ulong) Keys.ShiftKey)
            {
                //Console.WriteLine("visible?? key = {0}", (Keys) wparam);
                this.Visible = true;
            }

            if (message == (int) WindowsMessages.WM_KEYUP && wparam == (ulong) Keys.ShiftKey)
            {
                //Console.WriteLine("not visible?? key = {0}", (Keys) wparam);
                this.Visible = false;
            }

            //Save Menu if F5 is pressed (upon reloading)
            if (message == (int)WindowsMessages.WM_KEYUP && wparam == (ulong)Keys.F5)
            {
                this.Save();
            }

            foreach (var menu in this.Menus)
            {
                menu.WndProc(message, wparam, lparam);
            }

        }

        #endregion

        #region Explicit Interface Methods

        #endregion
    }
}