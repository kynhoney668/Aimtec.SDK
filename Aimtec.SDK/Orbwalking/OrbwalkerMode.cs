using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Menu.Config;
using Aimtec.SDK.Util;
using System;

namespace Aimtec.SDK.Orbwalking
{
    /// <summary>
    /// Class OrbwalkerMode
    /// </summary>
    public class OrbwalkerMode
    {
        /// <summary>
        /// The delegate for this Mode's logic
        /// </summary>
        public delegate void OrbwalkModeDelegate();

        /// <summary>
        /// The delegate for this Mode's target selection logic
        /// </summary>
        public delegate AttackableUnit TargetDelegate();

        /// <summary>
        /// This Orbwalker Mode's logic
        /// </summary>
        public OrbwalkModeDelegate ModeBehaviour;

        /// <summary>
        /// The target selection logic for this mode
        /// </summary>
        public TargetDelegate GetTargetImplementation = null;

        /// <summary>
        /// The Orbwalker Instance this mode belongs to
        /// </summary>
        public AOrbwalker ParentInstance;

        /// <summary>
        /// The name of this mode
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The MenuKeyBind item associated with this mode 
        /// </summary>
        public MenuKeyBind MenuItem { get; set; }

        /// <summary>
        /// Whether this mode is currently active
        /// </summary>
        public bool Active => this.MenuItem.Enabled;

        /// <summary>
        /// Whether this mode should execute the base Orbwalking Logic
        /// </summary>
        public bool BaseOrbwalkingEnabled { get; set; } = true;

        /// <summary>
        /// Whether this mode is using a Global Key instead of its own KeyBind
        /// </summary>
        public bool UsingGlobalKey { get; set; }


        private bool _attackEnabled;
        private bool _moveEnabled;

        /// <summary>
        /// Whether attacking is currently allowed
        /// </summary>
        public bool AttackingEnabled
        {
            get
            {
                if (this.ParentInstance != null && this.ParentInstance.AttackingEnabled)
                {
                    return true;
                }

                return _attackEnabled;
            }
            set
            {
                _attackEnabled = value;
            }
        }

        /// <summary>
        /// Whether moving is currently enabled
        /// </summary>
        public bool MovingEnabled
        {
            get
            {
                if (this.ParentInstance != null && this.ParentInstance.MovingEnabled)
                {
                    return true;
                }
                return _moveEnabled;
            }
            set
            {
                _moveEnabled = value;
            }
        }

        /// <summary>
        ///     Creates a new instance of an OrbwalkerMode using a Global Key
        /// </summary>
        public OrbwalkerMode(string name, GlobalKey key, TargetDelegate targetDelegate, OrbwalkModeDelegate orbwalkBehaviour)
        {
            if (name == null || key == null)
            {
                throw new Exception("There was an error creating the Orbwalker Mode");
            }

            this.Name = name;
            this.ModeBehaviour = orbwalkBehaviour;
            this.GetTargetImplementation = targetDelegate;
            this.MenuItem = key.KeyBindItem;
            key.Activate();
            this.UsingGlobalKey = true;
        }

        /// <summary>
        ///     Creates a new instance of an OrbwalkerMode using a new Keybind
        /// </summary>
        public OrbwalkerMode(string name, KeyCode key, TargetDelegate targetDelegate, OrbwalkModeDelegate orbwalkBehaviour)
        {
            if (name == null)
            {
                throw new Exception("There was an error creating the Orbwalker Mode");
            }

            this.Name = name;
            this.ModeBehaviour = orbwalkBehaviour;
            this.GetTargetImplementation = targetDelegate;
            this.MenuItem = new MenuKeyBind(name, name, key, KeybindType.Press);
        }

        /// <summary>
        ///     Executes the logic for this Orbwalking Mode
        /// </summary>
        public void Execute()
        {
            if (this.ModeBehaviour != null)
            {
                this.ModeBehaviour();
            }
        }

        public AttackableUnit GetTarget()
        {
            if (this.GetTargetImplementation != null)
            {
                return this.GetTargetImplementation();
            }

            return null;
        }
    }
}
