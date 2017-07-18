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

        private readonly string[] HighPriority =
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

        private readonly string[] LowPriority =
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

        private readonly string[] MaxPriority =
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

        private readonly string[] MediumPriority =
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

        private List<Weight> Weights = new List<Weight>();

        #endregion

        #region Constructors and Destructors

        public TargetSelectorImpl()
        {
            this.CreateMenu();
            this.CreateWeights();
            Render.OnRender += this.RenderManagerOnOnRender;
            Game.OnWndProc += this.GameOnOnWndProc;
        }

        #endregion

        #region Enums

        public enum TargetPriority
        {
            MaxPriority = 5,

            HighPriority = 4,

            MediumPriority = 3,

            LowPriority = 2,

            MinPriority = 1,
        }

        public enum WeightEffect
        {
            HigherIsBetter,

            LowerIsBetter
        }

        #endregion

        #region Public Properties

        public TSCache Cache { get; set; } = new TSCache();

        public Menu Config { get; set; }

        public TargetSelectorMode Mode => (TargetSelectorMode) this.Config["TsMode"].As<MenuList>().Value;

        public Obj_AI_Hero SelectedHero { get; set; }

        #endregion

        #region Properties

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        #endregion

        #region Public Methods and Operators

        public void AddWeight(Weight weight)
        {
            this.Weights.Add(weight);
            this.Config["WeightsMenu"].As<Menu>().Add(weight.MenuItem);
        }

        public void Dispose()
        {
            Render.OnRender -= this.RenderManagerOnOnRender;
            Game.OnWndProc -= this.GameOnOnWndProc;
        }

        public TargetPriority GetDefaultPriority(Obj_AI_Hero hero)
        {
            var name = hero.ChampionName;

            if (this.MaxPriority.Contains(name))
            {
                return TargetPriority.MaxPriority;
            }

            if (this.HighPriority.Contains(name))
            {
                return TargetPriority.HighPriority;
            }

            if (this.MediumPriority.Contains(name))
            {
                return TargetPriority.MediumPriority;
            }

            if (this.LowPriority.Contains(name))
            {
                return TargetPriority.LowPriority;
            }

            return TargetPriority.MinPriority;
        }

        public List<Obj_AI_Hero> GetOrderedTargets(float range, bool autoattack = false)
        {
            var key = "GetOrderedTargets" + range + autoattack;

            var cachedValue = this.Cache.GetObject(key);

            if (cachedValue != null)
            {
                return (List<Obj_AI_Hero>) cachedValue;
            }

            if (this.Config["UseWeights"].Enabled)
            {
                var targetWeightDictionary = this.GetTargetsAndWeights(range, autoattack);

                var ordered = targetWeightDictionary.Keys.OrderByDescending(k => targetWeightDictionary[k]).ToList();

                if (this.SelectedHero != null)
                {
                    var containsSelected = ordered.Any(x => x.NetworkId == this.SelectedHero.NetworkId);

                    if (containsSelected)
                    {
                        ordered.RemoveAll(x => x.NetworkId == this.SelectedHero.NetworkId);
                        ordered.Insert(0, this.SelectedHero);
                    }
                }

                this.Cache.AddItem(new TSCache.CacheObject(key, ordered, this.Config["Misc"]["CacheT"].Value));
                return ordered;
            }

            var targets = this.GetOrderedTargetsByMode(range, autoattack).ToList();
            this.Cache.AddItem(new TSCache.CacheObject(key, targets, this.Config["Misc"]["CacheT"].Value));
            return targets;
        }

        public IEnumerable<Obj_AI_Hero> GetOrderedTargetsByMode(float range, bool autoattack = false)
        {
            var key = "GetOrderedTargetsByMode" + range + autoattack;

            var cachedValue = this.Cache.GetObject(key);

            if (cachedValue != null)
            {
                return (IEnumerable<Obj_AI_Hero>) cachedValue;
            }

            var validTargets = this.GetValidTargets(range, autoattack);

            IEnumerable<Obj_AI_Hero> returnValue = null;

            if (this.Mode == TargetSelectorMode.Closest)
            {
                returnValue = validTargets.OrderBy(x => x.Distance(Player));
            }

            else if (this.Mode == TargetSelectorMode.LeastAttacks)
            {
                returnValue = validTargets.OrderBy(x => (int) Math.Ceiling(x.Health / Player.GetAutoAttackDamage(x)))
                                          .ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.LeastSpells)
            {
                returnValue = validTargets
                    .OrderBy(x => (int) (x.Health / Player.CalculateDamage(x, DamageType.Magical, 100)))
                    .ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.LowestHealth)
            {
                returnValue = validTargets.OrderBy(x => x.Health).ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.MostAd)
            {
                returnValue = validTargets.OrderByDescending(x => x.TotalAttackDamage)
                                          .ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.MostAp)
            {
                returnValue = validTargets.OrderByDescending(x => x.TotalAbilityDamage)
                                          .ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.NearMouse)
            {
                returnValue = validTargets.OrderBy(x => x.Distance(Game.CursorPos))
                                          .ThenByDescending(this.GetPriority);
            }

            else if (this.Mode == TargetSelectorMode.MostPriority)
            {
                returnValue = validTargets.OrderBy(this.GetPriority);
            }

            this.Cache.AddItem(new TSCache.CacheObject(key, returnValue, this.Config["Misc"]["CacheT"].Value));

            return returnValue;
        }

        public TargetPriority GetPriority(Obj_AI_Hero hero)
        {
            var slider = this.Config["TargetsMenu"]["priority" + hero.ChampionName].As<MenuSlider>();
            if (slider != null)
            {
                return (TargetPriority) slider.Value;
            }

            return this.GetDefaultPriority(hero);
        }

        public Obj_AI_Hero GetTarget(float range, bool autoattack = false)
        {
            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return this.SelectedHero;
            }

            var target = this.GetOrderedTargets(range, autoattack).FirstOrDefault();

            if (target != null)
            {
                return target;
            }

            return null;
        }

        public IEnumerable<Obj_AI_Hero> GetValidTargets(float range, bool autoattack)
        {
            var enemies = ObjectManager.Get<Obj_AI_Hero>()
                                       .Where(x => autoattack ? x.IsValidAutoRange() : x.IsValidTarget(range));
            return enemies;
        }

        #endregion

        #region Methods

        private void CreateMenu()
        {
            Logger.Info("Constructing Menu for default Target Selector");

            this.Config = new Menu("Aimtec.TS", "Target Selector");

            var weights = new Menu("WeightsMenu", "Weights");

            this.Config.Add(weights);

            var targetsMenu = new Menu("TargetsMenu", "Targets");

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                targetsMenu.Add(new MenuSlider("priority" + enemy.ChampionName, enemy.ChampionName, (int)this.GetDefaultPriority(enemy), 1, 5));
            }

            this.Config.Add(targetsMenu);

            var drawings = new Menu("Drawings", "Drawings")
            {
                new MenuBool("IndicateSelected", "Indicate Selected Target"),
                new MenuBool("ShowOrder", "Show Target Order"),
                new MenuBool("ShowOrderAuto", "Auto range only")
            };
            this.Config.Add(drawings);

            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuSlider("CacheT", "Cache Refresh Time", 200, 0, 500)
            };
            this.Config.Add(miscMenu);

            this.Config.Add(new MenuBool("UseWeights", "Use Weights"));
            this.Config.Add(new MenuList("TsMode", "Mode", Enum.GetNames(typeof(TargetSelectorMode)), 0));
        }

        private void CreateWeights()
        {
            this.AddWeight(
                new Weight(
                    "ClosestToPlayerWeight",
                    "Closest to Player",
                    true,
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
                    target => (int) Math.Ceiling(target.Health / Player.GetAutoAttackDamage(target)),
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "PriorityWeight",
                    "Most Priority",
                    true,
                    100,
                    target => (int) this.GetPriority(target),
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MaxAttackDamageWeight",
                    "Most AD",
                    true,
                    100,
                    target => target.TotalAttackDamage,
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MaxAbilityPowerWeight",
                    "Most AP",
                    true,
                    100,
                    target => target.TotalAbilityDamage,
                    WeightEffect.HigherIsBetter));

            this.AddWeight(
                new Weight(
                    "MinArmorWeight",
                    "Min Armor",
                    true,
                    100,
                    target => target.Armor + target.BonusArmor,
                    WeightEffect.LowerIsBetter));

            this.AddWeight(
                new Weight(
                    "MinMRWeight",
                    "Min MR",
                    true,
                    100,
                    target => target.SpellBlock,
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

        private void GameOnOnWndProc(WndProcEventArgs args)
        {
            var message = args.Message;
            if (message == (ulong) WindowsMessages.WM_LBUTTONDOWN)
            {
                var clickPosition = Game.CursorPos;

                var targets = ObjectManager
                    .Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000)).OrderBy(x => x.Distance(clickPosition));

                var closestHero = targets.FirstOrDefault(x => x.IsHero);

                if (closestHero != null && Game.CursorPos.Distance(closestHero.Position) <= 300)
                {
                    this.SelectedHero = closestHero;
                }

                else
                {
                    this.SelectedHero = null;
                }
            }
        }

        private Dictionary<Obj_AI_Hero, float> GetTargetsAndWeights(float range, bool autoattack = false)
        {
            var key = "GetTargetsAndWeights" + range + autoattack;

            var cachedValue = this.Cache.GetObject(key);

            if (cachedValue != null)
            {
                return (Dictionary<Obj_AI_Hero, float>) cachedValue;
            }

            var enemies = this.GetValidTargets(range, autoattack);

            var enabledWeights = this.Weights.Where(x => x.MenuItem.Enabled);

            var CumulativeResults = new Dictionary<Obj_AI_Hero, float>();

            foreach (var hero in enemies)
            {
                CumulativeResults[hero] = 0;
            }

            foreach (var weight in enabledWeights)
            {
                weight.ComputeWeight(enemies, ref CumulativeResults);
            }

            this.Cache.AddItem(new TSCache.CacheObject(key, CumulativeResults, this.Config["Misc"]["CacheT"].Value));

            return CumulativeResults;
        }

        private IOrderedEnumerable<KeyValuePair<Obj_AI_Hero, float>> GetTargetsAndWeightsOrdered(
            float range,
            bool autoattack)
        {
            var results = this.GetTargetsAndWeights(range, autoattack).ToList();

            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return results.OrderByDescending(x => x.Key.NetworkId == this.SelectedHero.NetworkId)
                              .ThenByDescending(x => x.Value);
            }

            return results.OrderByDescending(x => x.Value);
        }

        private void RenderManagerOnOnRender()
        {
            var indicateSelected = this.Config["Drawings"]["IndicateSelected"].Enabled;
            var showOrder = this.Config["Drawings"]["ShowOrder"].Enabled;
            var ShowOrderAuto = this.Config["Drawings"]["ShowOrderAuto"].Enabled;

            if (indicateSelected)
            {
                if (this.SelectedHero != null && this.SelectedHero.IsValidTarget())
                {
                    Render.Circle(this.SelectedHero.Position, this.SelectedHero.BoundingRadius, 30, Color.Red);
                }
            }

            if (showOrder)
            {
                if (this.Config["UseWeights"].Enabled)
                {
                    var ordered = this.GetTargetsAndWeightsOrdered(50000, ShowOrderAuto).ToList();
                    var basepos = new Vector2(Render.Width / 2f, 0.10f * Render.Height);
                    for (var i = 0; i < ordered.Count(); i++)
                    {
                        var targ = ordered[i];
                        var target = targ.Key;
                        if (target != null)
                        {
                            Render.Text(
                                basepos + new Vector2(0, i * 15),
                                Color.Red,
                                target.ChampionName + " " + targ.Value);
                        }
                    }
                }

                else
                {
                    var ordered = this.GetOrderedTargetsByMode(50000, ShowOrderAuto).ToList();
                    var basepos = new Vector2(Render.Width / 2f, 0.10f * Render.Height);
                    for (var i = 0; i < ordered.Count(); i++)
                    {
                        var target = ordered[i];
                        if (target != null)
                        {
                            Render.Text(basepos + new Vector2(0, i * 15), Color.Red, target.ChampionName);
                        }
                    }
                }
            }
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

                this.MenuItem = new MenuSliderBool(name, displayName, true, defaultWeight, 0, 100);

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
                var TargetResults = new Dictionary<Obj_AI_Hero, float>();

                foreach (var hero in heroes)
                {
                    TargetResults[hero] = this.WeightDefinition(hero);
                }

                var sumValues = TargetResults.Values.Sum();

                foreach (var item in TargetResults)
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
    }

    internal class TSCache
    {
        #region Fields

        private Dictionary<string, CacheObject> CachedObjects = new Dictionary<string, CacheObject>();

        #endregion

        #region Public Methods and Operators

        public void AddItem(CacheObject cObject)
        {
            this.CachedObjects.Add(cObject.Key, cObject);
        }

        public Object GetObject(string key)
        {
            this.RemoveExpiredItems();

            if (!this.CachedObjects.ContainsKey(key))
            {
                return null;
            }

            return this.CachedObjects[key].Object;
        }

        public void RemoveExpiredItems()
        {
            foreach (var obj in this.CachedObjects.Values.ToList())
            {
                if (Game.TickCount > obj.ExpiryTime)
                {
                    this.CachedObjects.Remove(obj.Key);
                }
            }
        }

        public void RemoveItem(CacheObject cObject)
        {
            this.CachedObjects.Remove(cObject.Key);
        }

        #endregion

        public class CacheObject
        {
            #region Constructors and Destructors

            public CacheObject(string key, Object obj, int ttl)
            {
                this.Key = key;
                this.Object = obj;
                this.TTL = ttl;
                this.CreationTime = Game.TickCount;
                this.ExpiryTime = Game.TickCount + ttl;
            }

            #endregion

            #region Public Properties

            public int CreationTime { get; set; }

            public int ExpiryTime { get; set; }

            public string Key { get; set; }

            public Object Object { get; set; }

            public int TTL { get; set; }

            #endregion
        }
    }
}