namespace Aimtec.SDK.TargetSelector
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;

    using NLog;

    internal class TargetSelectorImpl : ITargetSelector
    {
        #region Fields

        private TargetLockingManager TargetLockManager { get; set; }

        #region Hero Priorities
        private readonly string[] highPriority =
        {
            "Akali",
            "Diana",
            "Ekko",
            "Fiddlesticks",
            "Fiora",
            "Fizz",
            "Heimerdinger",
            "Jayce",
            "Kassadin",
            "Kayle",
            "Kha'Zix",
            "Lissandra",
            "Mordekaiser",
            "Nidalee",
            "Riven",
            "Shaco",
            "Vladimir",
            "Yasuo",
            "Zilean"
        };

        private readonly string[] lowPriority =
        {
            "Alistar",
            "Amumu",
            "Bard",
            "Blitzcrank",
            "Braum",
            "Cho'Gath",
            "Dr. Mundo",
            "Garen",
            "Gnar",
            "Hecarim",
            "Janna",
            "Jarvan IV",
            "Leona",
            "Lulu",
            "Malphite",
            "Nami",
            "Nasus",
            "Nautilus",
            "Nunu",
            "Olaf",
            "Rammus",
            "Renekton",
            "Sejuani",
            "Shen",
            "Shyvana",
            "Singed",
            "Sion",
            "Skarner",
            "Sona",
            "Taric",
            "TahmKench",
            "Thresh",
            "Volibear",
            "Warwick",
            "MonkeyKing",
            "Yorick",
            "Zac",
            "Zyra"
        };

        private readonly string[] maxPriority =
        {
            "Ahri",
            "Anivia",
            "Annie",
            "Ashe",
            "Azir",
            "Brand",
            "Caitlyn",
            "Cassiopeia",
            "Corki",
            "Draven",
            "Ezreal",
            "Graves",
            "Jinx",
            "Kalista",
            "Karma",
            "Karthus",
            "Katarina",
            "Kennen",
            "KogMaw",
            "Kindred",
            "Leblanc",
            "Lucian",
            "Lux",
            "Malzahar",
            "MasterYi",
            "MissFortune",
            "Orianna",
            "Quinn",
            "Sivir",
            "Syndra",
            "Talon",
            "Teemo",
            "Tristana",
            "TwistedFate",
            "Twitch",
            "Varus",
            "Vayne",
            "Veigar",
            "Velkoz",
            "Viktor",
            "Xerath",
            "Zed",
            "Ziggs",
            "Jhin",
            "Soraka"
        };

        private readonly string[] mediumPriority =
        {
            "Aatrox",
            "Darius",
            "Elise",
            "Evelynn",
            "Galio",
            "Gangplank",
            "Gragas",
            "Irelia",
            "Jax",
            "Lee Sin",
            "Maokai",
            "Morgana",
            "Nocturne",
            "Pantheon",
            "Poppy",
            "Rengar",
            "Rumble",
            "Ryze",
            "Swain",
            "Trundle",
            "Tryndamere",
            "Udyr",
            "Urgot",
            "Vi",
            "XinZhao",
            "RekSai"
        };

        #endregion

        public readonly List<Weight> Weights = new List<Weight>();

        #endregion

        #region Constructors and Destructors

        public TargetSelectorImpl()
        {
            this.Config = new Menu("Aimtec.TS", "Target Selector");
            this.TargetLockManager = new TargetLockingManager(this);
            this.CreateMenu();
            this.CreateWeights();
            Render.OnRender += this.RenderManagerOnOnRender;
        }

        #endregion

        #region Enums

        public enum TargetPriority
        {
            MaxPriority = 5,

            HighPriority = 4,

            MediumPriority = 3,

            LowPriority = 2,

            MinPriority = 1
        }

        public enum WeightEffect
        {
            HigherIsBetter,

            LowerIsBetter
        }

        #endregion

        #region Public Properties

        public Menu Config { get; set; }

        public TargetSelectorMode Mode => (TargetSelectorMode)this.Config["TsMode"].As<MenuList>().Value;

        #endregion

        #region Properties

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        #endregion

        #region Public Methods and Operators

        public Obj_AI_Hero GetSelectedTarget()
        {
            return this.TargetLockManager.SelectedTarget;
        }

        public void AddWeight(Weight weight)
        {
            this.Weights.Add(weight);
            this.Config["WeightsMenu"].As<Menu>().Add(weight.MenuItem);
        }

        public void Dispose()
        {
            Render.OnRender -= this.RenderManagerOnOnRender;
            this.TargetLockManager.Dispose();
        }

        public TargetPriority GetDefaultPriority(Obj_AI_Hero hero)
        {
            var name = hero.ChampionName;

            if (this.maxPriority.Contains(name))
            {
                return TargetPriority.MaxPriority;
            }

            if (this.highPriority.Contains(name))
            {
                return TargetPriority.HighPriority;
            }

            if (this.mediumPriority.Contains(name))
            {
                return TargetPriority.MediumPriority;
            }

            if (this.lowPriority.Contains(name))
            {
                return TargetPriority.LowPriority;
            }

            return TargetPriority.MinPriority;
        }

        public void MoveToFront(Obj_AI_Hero target, ref List<Obj_AI_Hero> targets)
        {
            var containsSelected = targets.Any(x => x.NetworkId == target.NetworkId); // if selected target is in target list
            if (containsSelected)
            {
                targets.RemoveAll(x => x.NetworkId == target.NetworkId);
            }

            // Put selected target at beginning of list even if there weight is lower
            targets.Insert(0, target);
        }

        public List<Obj_AI_Hero> GetOrderedTargets(float range, bool autoattack = false)
        {
            List<Obj_AI_Hero> orderedTargets;

            if (this.Config["UseWeights"].Enabled)
            {
                var targetWeightDictionary = this.GetTargetsAndWeights(range, autoattack);
                orderedTargets = targetWeightDictionary.Keys.OrderByDescending(k => targetWeightDictionary[k]).ToList();
            }
            else
            {
                orderedTargets = this.GetOrderedTargetsByMode(range, autoattack).ToList();
            }

            var lockState = this.TargetLockManager.TargetLockState;

            if (lockState.HasFlag(TargetLockingManager.LockState.Prioritize))
            {
                var selected = this.GetSelectedTarget();

                if (selected.IsValidTarget())
                {
                    var distance = Player.Distance(selected);
                    var inRange = autoattack ? distance < Player.GetFullAttackRange(selected) : distance < range;

                    if (lockState == TargetLockingManager.LockState.Prioritize)
                    {
                        if (inRange)
                        {
                            this.MoveToFront(selected, ref orderedTargets);
                            return orderedTargets;
                        }
                    }

                    else if (lockState.HasFlag(TargetLockingManager.LockState.OnlyAttackTarget))
                    {
                        if (inRange || lockState.HasFlag(TargetLockingManager.LockState.Magnetize))
                        {
                            return new List<Obj_AI_Hero>
                                       { selected }; // Return only the selected target if we assasin mode
                        }

                        return new List<Obj_AI_Hero>() { }; // Return nothing if the target is out of range
                    }
                }
            }

            return orderedTargets;
        }

        public IEnumerable<Obj_AI_Hero> GetOrderedTargetsByMode(float range, bool autoattack = false)
        {
            var validTargets = this.GetValidTargets(range, autoattack);

            IEnumerable<Obj_AI_Hero> returnValue = null;

            switch (this.Mode)
            {
                case TargetSelectorMode.Closest:
                    returnValue = validTargets.OrderBy(x => x.Distance(Player));
                    break;

                case TargetSelectorMode.LeastAttacks:
                    returnValue = validTargets
                        .OrderBy(x => (int)Math.Ceiling(x.Health / Player.GetAutoAttackDamage(x)))
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.LeastSpells:
                    returnValue = validTargets
                        .OrderBy(x => (int)(x.Health / Player.CalculateDamage(x, DamageType.Magical, 100)))
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.LowestHealth:
                    returnValue = validTargets
                        .OrderBy(x => x.Health)
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.MostAd:
                    returnValue = validTargets
                        .OrderByDescending(x => x.TotalAttackDamage)
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.MostAp:
                    returnValue = validTargets
                        .OrderByDescending(x => x.TotalAbilityDamage)
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.NearMouse:
                    returnValue = validTargets
                        .OrderBy(x => x.Distance(Game.CursorPos))
                        .ThenByDescending(this.GetPriority);
                    break;

                case TargetSelectorMode.MostPriority:
                    returnValue = validTargets.OrderByDescending(this.GetPriority);
                    break;
            }

            return returnValue;
        }

        private bool IsWhiteListed(Obj_AI_Hero hero)
        {
            var sbool = this.Config["TargetsMenu"]["target" + hero.ChampionName];
            if (sbool != null)
            {
                return sbool.Enabled;
            }

            return true;
        }


        public TargetPriority GetPriority(Obj_AI_Hero hero)
        {
            var slider = this.Config["TargetsMenu"]["target" + hero.ChampionName];
            if (slider != null)
            {
                return (TargetPriority)slider.Value;
            }

            return this.GetDefaultPriority(hero);
        }

        public Obj_AI_Hero GetTarget(float range, bool autoattack = false)
        {
            var target = this.GetOrderedTargets(range, autoattack).FirstOrDefault();

            if (target != null)
            {
                return target;
            }

            return null;
        }

        public IEnumerable<Obj_AI_Hero> GetValidTargets(float range, bool autoattack)
        {
            var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => this.IsWhiteListed(x) && autoattack ? x.IsValidAutoRange() : x.IsValidTarget(range));
            return enemies;
        }

        #endregion

        #region Methods

        private void CreateMenu()
        {
            Logger.Info("Constructing Menu for default Target Selector");

            var weights = new Menu("WeightsMenu", "Weights");
            this.Config.Add(weights);

            var targetsMenu = new Menu("TargetsMenu", "Targets");
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                targetsMenu.Add(new MenuSliderBool("target" + enemy.ChampionName, enemy.ChampionName, true, (int)this.GetDefaultPriority(enemy), 1, 5));
            }
            this.Config.Add(targetsMenu);

            var drawings = new Menu("Drawings", "Drawings")
            {
                new MenuBool("IndicateSelected", "Indicate Selected Target"),
                new MenuBool("ShowLineToSelected", "Show Line to Selected"),
                new MenuBool("ShowOrder", "Show Target Order"),
                new MenuBool("ShowOrderAuto", "Auto range only")
            };

            this.Config.Add(drawings);

            this.Config.Add(new MenuBool("UseWeights", "Use Weights"));
            this.Config.Add(new MenuList("TsMode", "Mode", Enum.GetNames(typeof(TargetSelectorMode)), 0));
        }

        private void CreateWeights()
        {
            this.AddWeight(
                new Weight(
                    "ClosestToPlayerWeight",
                    "Closest to Player",
                    false,
                    100,
                    target => target.Distance(Player),
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "ClosestToMouse",
                    "Closest to Mouse",
                    true,
                    100,
                    target => target.Distance(Game.CursorPos),
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "LeastAttacksWeight",
                    "Least Auto Attacks",
                    true,
                    100,
                    target => (int)Math.Ceiling(target.Health / Player.GetAutoAttackDamage(target)),
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "PriorityWeight",
                    "Most Priority",
                    true,
                    100,
                    target => (int)this.GetPriority(target),
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MaxAttackDamageWeight",
                    "Most AD",
                    false,
                    100,
                    target => target.TotalAttackDamage,
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MaxAbilityPowerWeight",
                    "Most AP",
                    false,
                    100,
                    target => target.TotalAbilityDamage,
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MinArmorWeight",
                    "Min Armor",
                    false,
                    100,
                    target => target.Armor + target.BonusArmor,
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "MinMRWeight",
                    "Min MR",
                    false,
                    100,
                    target => target.SpellBlock + target.BonusSpellBlock,
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "MinHealthWeight",
                    "Min Health",
                    true,
                    100,
                    target => target.Health,
                    WeightEffect.LowerIsBetter));
        }


        private Dictionary<Obj_AI_Hero, float> GetTargetsAndWeights(float range, bool autoattack = false)
        {
            var enemies = this.GetValidTargets(range, autoattack);
            var enabledWeights = this.Weights.Where(x => x.MenuItem.Enabled);
            var cumulativeResults = new Dictionary<Obj_AI_Hero, float>();

            var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
            foreach (var hero in objAiHeroes)
            {
                cumulativeResults[hero] = 0;
            }

            foreach (var weight in enabledWeights)
            {
                weight.ComputeWeight(objAiHeroes, ref cumulativeResults);
            }

            return cumulativeResults;
        }

        // Only used for drawing target list
        private IEnumerable<KeyValuePair<Obj_AI_Hero, float>> GetTargetsAndWeightsOrdered(
            float range,
            bool autoattack)
        {
            var results = this.GetTargetsAndWeights(range, autoattack).ToList();

            if (this.TargetLockManager.FocusSelected)
            {
                var selected = this.GetSelectedTarget();

                if (selected != null)
                {
                    return results.OrderByDescending(x => x.Key.NetworkId == selected.NetworkId)
                                  .ThenByDescending(x => x.Value);
                }
            }

            return results.OrderByDescending(x => x.Value);
        }

        private void RenderManagerOnOnRender()
        {
            var showOrder = this.Config["Drawings"]["ShowOrder"].Enabled;
            var showOrderAuto = this.Config["Drawings"]["ShowOrderAuto"].Enabled;

            if (showOrder)
            {
                if (this.Config["UseWeights"].Enabled)
                {
                    var ordered = this.GetTargetsAndWeightsOrdered(50000, showOrderAuto).ToList();
                    var basepos = new Vector2(Render.Width / 2f, 0.10f * Render.Height);
                    for (var i = 0; i < ordered.Count; i++)
                    {
                        var targ = ordered[i];
                        var target = targ.Key;
                        if (target != null)
                        {
                            Render.Text(target.ChampionName + " " + targ.Value, basepos + new Vector2(0, i * 15), RenderTextFlags.None, Color.Red);
                        }
                    }
                }
                else
                {
                    var ordered = this.GetOrderedTargetsByMode(50000, showOrderAuto).ToList();
                    var basepos = new Vector2(Render.Width / 2f, 0.10f * Render.Height);
                    for (var i = 0; i < ordered.Count; i++)
                    {
                        var target = ordered[i];
                        if (target != null)
                        {
                            Render.Text(target.ChampionName, basepos + new Vector2(0, i * 15), RenderTextFlags.None, Color.Red);
                        }
                    }
                }
            }

            this.TargetLockManager.OnRender();
        }

        #endregion

        public class Weight
        {
            #region Constructors and Destructors

            public Weight(
                string name,
                string displayName,
                bool enabled,
                int defaultWeight,
                WeightDelegate definition,
                WeightEffect effect)
            {
                if (defaultWeight > 100)
                {
                    Logger.Error("Weight cannot be more than 100.");
                    throw new Exception("Weight cannot be more than 100.");
                }

                this.Name = name;

                this.MenuItem = new MenuSliderBool(name, displayName, enabled, defaultWeight);

                this.WeightDefinition = definition;

                this.Effect = effect;
            }

            #endregion

            #region Delegates

            public delegate float WeightDelegate(Obj_AI_Hero target);

            #endregion

            #region Public Properties

            public string DisplayName { get; set; }

            public WeightEffect Effect { get; set; }

            public MenuComponent MenuItem { get; set; }

            public string Name { get; set; }

            public WeightDelegate WeightDefinition { get; set; }

            public float WeightValue => this.MenuItem.Value / 100f;

            #endregion

            #region Public Methods and Operators

            public void ComputeWeight(
                IEnumerable<Obj_AI_Hero> heroes,
                ref Dictionary<Obj_AI_Hero, float> weightedTotals)
            {
                var targetResults = new Dictionary<Obj_AI_Hero, float>();

                foreach (var hero in heroes)
                {
                    targetResults[hero] = this.WeightDefinition(hero);
                }

                var sumValues = (int)targetResults.Values.Sum();
                foreach (var item in targetResults)
                {
                    if (sumValues == 0)
                    {
                        continue;
                    }

                    var percent = item.Value / sumValues * 100;

                    if (this.Effect == WeightEffect.LowerIsBetter)
                    {
                        percent = 100 - percent;
                    }

                    var result = this.WeightValue * percent;

                    weightedTotals[item.Key] += result;
                }
            }

            #endregion
        }

        class TargetLockingManager
        {
            internal bool FocusSelected => this.Config["FocusSelected"].Enabled;

            public LockState TargetLockState { get; set; } = LockState.None;

            public Obj_AI_Hero SelectedTarget { get; set; }

            public Menu Config { get; set; }

            public TargetSelectorImpl TSInstance { get; set; }

            public TargetLockingManager(TargetSelectorImpl ins)
            {
                this.TSInstance = ins;

                this.Config = new Menu("TargetLocking", "Target Locking")
                                  {
                                      new MenuSeperator("info1", "Click Target To Focus"),
                                      new MenuSeperator("info2", "1st Click = Priority Target"),
                                      new MenuSeperator("info3", "2nd Click = Only Attack Target"),
                                      new MenuSeperator("info4", "3rd Click = Magnet To Target"),
                                      new MenuBool("FocusSelected", "Focus Selected Target"),
                                  };

                this.TSInstance.Config.Add(this.Config);

                Game.OnWndProc += this.Game_OnWndProc;
            }

            private void Game_OnWndProc(WndProcEventArgs e)
            {
                if (!this.FocusSelected)
                {
                    return;
                }

                var message = e.Message;

                if (message == (ulong)WindowsMessages.WM_LBUTTONDOWN)
                {
                    var clickPosition = Game.CursorPos;

                    var targets = ObjectManager
                        .Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000)).OrderBy(x => x.Distance(clickPosition));

                    var closestHero = targets.FirstOrDefault(x => x.IsHero);

                    if (closestHero != null && Game.CursorPos.Distance(closestHero.Position) <= 300)
                    {
                        if (this.SelectedTarget == null || this.SelectedTarget.NetworkId != closestHero.NetworkId)
                        {
                            this.SelectedTarget = closestHero;
                            this.TargetLockState = LockState.None;
                        }

                        if (this.TargetLockState == LockState.None)
                        {
                            this.TargetLockState = LockState.Prioritize;
                        }

                        else if (this.TargetLockState == LockState.Prioritize)
                        {
                            this.TargetLockState = LockState.Prioritize | LockState.OnlyAttackTarget;
                        }

                        else
                        {
                            this.TargetLockState = LockState.Prioritize | LockState.OnlyAttackTarget | LockState.Magnetize;
                        }
                    }

                    else
                    {
                        this.SelectedTarget = null;
                        this.TargetLockState = LockState.None;
                    }
                }
            }

            internal void OnRender()
            {
                if (!this.SelectedTarget.IsValidTarget() || this.TargetLockState == LockState.None)
                {
                    return;
                }

                var indicateSelected = this.TSInstance.Config["Drawings"]["IndicateSelected"].Enabled;
                var showLine = this.TSInstance.Config["Drawings"]["ShowLineToSelected"].Enabled;
                if (indicateSelected)
                {
                    Color color = Color.White;
                    string text = "";

                    if (this.TargetLockState.HasFlag(LockState.Prioritize))
                    {
                        color = Color.YellowGreen;
                        Render.Circle(this.SelectedTarget.Position, 150, 100, Color.YellowGreen);
                        var position = this.SelectedTarget.Position + new Vector3(135, 0, 135);
                        if (Render.WorldToScreen(position, out Vector2 screenposition))
                        {
                            text = "Priority Target";
                        }
                    }

                    if (this.TargetLockState.HasFlag(LockState.OnlyAttackTarget))
                    {
                        text = "Only Attack Target";
                        color = Color.Orange;
                        Render.Circle(this.SelectedTarget.Position, 100, 100, color);
                    }

                    if (this.TargetLockState.HasFlag(LockState.Magnetize))
                    {
                        text = "Magnet Target";
                        color = Color.Red;
                        ;
                        Render.Circle(this.SelectedTarget.Position, 150, 5, color);
                    }

                    if (showLine)
                    {
                        var distance = Player.Position.Distance(this.SelectedTarget.Position);
                        var midDistance = distance / 2;
                        var midp = Player.Position.Extend(this.SelectedTarget.Position, midDistance);

                        if (Render.WorldToScreen(Player.Position, out Vector2 pscreenposition) && Render.WorldToScreen(
                                this.SelectedTarget.Position,
                                out Vector2 tscreenposition))
                        {
                            Render.Line(pscreenposition, tscreenposition, 5, true, color);
                        }

                        if (Render.WorldToScreen(midp, out Vector2 mscreenposition))
                        {
                            Render.Text(text, mscreenposition, RenderTextFlags.Center, color);
                        }
                    }
                }
            }

            [Flags]
            public enum LockState
            {
                None = 0,
                Prioritize = 1, //Focus this target if in range
                OnlyAttackTarget = 2, //Only attack this target
                Magnetize = 4  // Force this target no matter what (Assasin/Outofrange)
            }

            internal void Dispose()
            {
                Game.OnWndProc -= this.Game_OnWndProc;
            }
        }
    }
}