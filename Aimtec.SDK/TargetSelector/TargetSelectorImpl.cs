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
    using Aimtec.SDK.Menu.Config;
    using Aimtec.SDK.Util;
    using NLog;

    internal class TargetSelectorImpl : ITargetSelector
    {
        public Menu Config { get; set; }

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private List<Weight> Weights = new List<Weight>();

        public Obj_AI_Hero SelectedHero { get; set; }

        private readonly string[] MaxPriority =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Kindred",
                "Leblanc", "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra",
                "Talon", "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "Velkoz", "Viktor",
                "Xerath", "Zed", "Ziggs", "Jhin", "Soraka"
            };


        private readonly string[] HighPriority =
            {
                "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean"
            };

        private readonly string[] MediumPriority =
            {
                "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
            };


        private readonly string[] LowPriority =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Taric", "TahmKench", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };


        public TargetSelectorImpl()
        {
            this.CreateMenu();
            this.CreateWeights();
            RenderManager.OnRender += this.RenderManagerOnOnRender;
            Game.OnWndProc += this.GameOnOnWndProc;
        }

        private void GameOnOnWndProc(WndProcEventArgs args)
        {
            var message = args.Message;

            if (message == (ulong)WindowsMessages.WM_LBUTTONDOWN)
            {
                var clickPosition = Game.CursorPos;

                var targets = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000)).OrderBy(x => x.Distance(clickPosition));

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

        private void RenderManagerOnOnRender()
        {
            var indicateSelected = this.Config["Drawings"]["IndicateSelected"].Enabled;
            var showOrder = this.Config["Drawings"]["ShowOrder"].Enabled;
            var ShowOrderAuto = this.Config["Drawings"]["ShowOrderAuto"].Enabled;

            if (indicateSelected)
            {
                if (this.SelectedHero != null && this.SelectedHero.IsValidTarget())
                {
                    RenderManager.RenderCircle(
                        this.SelectedHero.Position,
                        this.SelectedHero.BoundingRadius,
                        30,
                        Color.Red);
                }
            }

            
            if (showOrder)
            {
                if (this.Config["UseWeights"].Enabled)
                {
                    var ordered = this.GetTargetsAndWeightsOrdered(50000, ShowOrderAuto).ToList();
                    var basepos = new Vector2(RenderManager.Width / 2f, 0.10f * RenderManager.Height);
                    for (int i = 0; i < ordered.Count(); i++)
                    {
                        var targ = ordered[i];
                        var target = targ.Key;
                        if (target != null)
                        {
                            RenderManager.RenderText(
                                basepos + new Vector2(0, i * 15),
                                Color.Red,
                                target.ChampionName + " " + targ.Value);
                        }
                    }
                }

                else
                {
                    var ordered = this.GetOrderedTargetsByMode(50000, ShowOrderAuto).ToList();
                    var basepos = new Vector2(RenderManager.Width / 2f, 0.10f * RenderManager.Height);
                    for (int i = 0; i < ordered.Count(); i++)
                    {
                        var target = ordered[i];
                        if (target != null)
                        {
                            RenderManager.RenderText(
                                basepos + new Vector2(0, i * 15),
                                Color.Red,
                                target.ChampionName);
                        }
                    }
                }
            } 
        }


        private IOrderedEnumerable<KeyValuePair<Obj_AI_Hero, float>> GetTargetsAndWeightsOrdered(float range, bool autoattack)
        {
            var results = GetTargetsAndWeights(range, autoattack).ToList();

            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return results.OrderByDescending(x => x.Key.NetworkId == this.SelectedHero.NetworkId).ThenByDescending(x => x.Value);
            }

            return results.OrderByDescending(x => x.Value);
        }

        public void AddWeight(Weight weight)
        {
            this.Weights.Add(weight);
            this.Config["WeightsMenu"].As<Menu>().Add(weight.MenuItem);
        }


        public IEnumerable<Obj_AI_Hero> GetValidTargets(float range, bool autoattack)
        {
            var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => autoattack ? x.IsValidAutoRange() : x.IsValidTarget(range));
            return enemies;
        }

        private Dictionary<Obj_AI_Hero, float> CachedTargetWeightRatings;
        private float LastCacheTargetWeightTime;

        private Dictionary<Obj_AI_Hero, float> GetTargetsAndWeights(float range, bool autoattack = false)
        {
            if (Game.TickCount - LastCacheTargetWeightTime < Config["Misc"]["CacheT"].Value)
            {
                return CachedTargetWeightRatings;
            }

            this.LastCacheTargetWeightTime = Game.TickCount;

            var enemies = GetValidTargets(range, autoattack);

            var enabledWeights = this.Weights.Where(x => x.MenuItem.Enabled);

            Dictionary<Obj_AI_Hero, float> CumulativeResults = new Dictionary<Obj_AI_Hero, float>();

            foreach (var hero in enemies)
            {
                CumulativeResults[hero] = 0;
            }

            foreach (var weight in enabledWeights)
            {
                weight.ComputeWeight(enemies, ref CumulativeResults);
            }

            return CachedTargetWeightRatings = CumulativeResults;
        }

        public List<Obj_AI_Hero> GetOrderedTargets(float range, bool autoattack = false)
        {
            if (this.Config["UseWeights"].Enabled)
            {
                var targetWeightDictionary = GetTargetsAndWeights(range, autoattack);

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

                return ordered;
            }

            var targets = GetOrderedTargetsByMode(range, autoattack).ToList();
            return targets;
        }


        public Obj_AI_Hero GetTarget(float range, bool autoattack = false)
        {
            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return this.SelectedHero;
            }

            var target = GetOrderedTargets(range, autoattack).FirstOrDefault();

            if (target!= null)
            {
                return target;
            }

            return null;
        }

        private IEnumerable<Obj_AI_Hero> CachedTargetRatingsByMode;
        private float LastCacheTargetRatingsModeTime;

        public IEnumerable<Obj_AI_Hero> GetOrderedTargetsByMode(float range, bool autoattack = false)
        {
            if (Game.TickCount - LastCacheTargetRatingsModeTime < Config["Misc"]["CacheT"].Value)
            {
                return CachedTargetRatingsByMode;
            }

            var validTargets = GetValidTargets(range, autoattack);

            if (this.Mode == TargetSelectorMode.Closest)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => x.Distance(Player));
            }

            else if (this.Mode == TargetSelectorMode.LeastAttacks)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => (int)Math.Ceiling(x.Health / Player.GetAutoAttackDamage(x))).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.LeastSpells)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => (int)(x.Health / Damage.CalculateDamage(Player, x, DamageType.Magical, 100))).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.LowestHealth)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => x.Health).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.MostAd)
            {
                return CachedTargetRatingsByMode = validTargets.OrderByDescending(x => x.TotalAttackDamage).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.MostAp)
            {
                return CachedTargetRatingsByMode = validTargets.OrderByDescending(x => x.TotalAbilityDamage).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.NearMouse)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => x.Distance(Game.CursorPos)).ThenByDescending(x => GetPriority(x));
            }

            else if (this.Mode == TargetSelectorMode.MostPriority)
            {
                return CachedTargetRatingsByMode = validTargets.OrderBy(x => GetPriority(x));
            }

            return null;
        }


        public TargetPriority GetPriority(Obj_AI_Hero hero)
        {
            var name = hero.ChampionName;
            MenuSlider slider = this.Config["TargetsMenu"]["priority" + hero.ChampionName].As<MenuSlider>();
            if (slider != null)
            {
                return (TargetPriority)slider.Value;
            }

            return GetDefaultPriority(hero);
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

        private void CreateWeights()
        {
            this.AddWeight(new Weight(
                "ClosestToPlayerWeight",
                "Closest to Player",
                true,
                100,
                target => target.Distance(Player),
                WeightEffect.LowerIsBetter));

            this.AddWeight(new Weight(
                "ClosestToMouse",
                "Closest to Mouse",
                true,
                100,
                target => target.Distance(Game.CursorPos),
                WeightEffect.LowerIsBetter));
           
            this.AddWeight(new Weight(
                "LeastAttacksWeight",
                "Least Auto Attacks",
                true,
                100,
                target => (int)Math.Ceiling(target.Health / Player.GetAutoAttackDamage(target)),
                WeightEffect.LowerIsBetter));
                
            this.AddWeight(new Weight(
                "PriorityWeight",
                "Most Priority",
                true,
                100,
                target => (int)GetPriority(target), WeightEffect.HigherIsBetter));

            this.AddWeight(new Weight(
                "MaxAttackDamageWeight",
                "Most AD",
                true,
                100,
                target => target.TotalAttackDamage, WeightEffect.HigherIsBetter));

            this.AddWeight(new Weight(
                "MaxAbilityPowerWeight",
                "Most AP",
                true,
                100,
                target => target.TotalAbilityDamage, WeightEffect.HigherIsBetter));

            this.AddWeight(new Weight(
                "MinArmorWeight",
                "Min Armor",
                true,
                100,
                target => target.Armor + target.BonusArmor, WeightEffect.LowerIsBetter));

            this.AddWeight(new Weight(
                "MinMRWeight",
                "Min MR",
                true,
                100,
                target => target.SpellBlock, WeightEffect.LowerIsBetter));

            this.AddWeight(new Weight(
                "MinHealthWeight",
                "Min Health",
                true,
                100,
                target => target.Health, WeightEffect.LowerIsBetter));
        }

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

            var drawings = new Menu("Drawings", "Drawings");
            drawings.Add(new MenuBool("IndicateSelected", "Indicate Selected Target"));
            drawings.Add(new MenuBool("ShowOrder", "Show Target Order"));
            drawings.Add(new MenuBool("ShowOrderAuto", "Auto range only"));
            this.Config.Add(drawings);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuSlider("CacheT", "Cache Refresh Time", 0, 0, 500));
            this.Config.Add(miscMenu);

            this.Config.Add(new MenuBool("UseWeights", "Use Weights"));

            this.Config.Add(new MenuList("TsMode", "Mode", Enum.GetNames(typeof(TargetSelectorMode)), 0));
        }

        public TargetSelectorMode Mode => (TargetSelectorMode) this.Config["TsMode"].As<MenuList>().Value;

        public class Weight
        {
            public Weight(string name, string displayName, bool enabled, int defaultWeight, WeightDelegate definition, WeightEffect effect)
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

            public string Name { get; set; }

            public MenuComponent MenuItem { get; set; }

            public float WeightValue => this.MenuItem.Value / 100f;

            public string DisplayName { get; set; }

            public delegate float WeightDelegate(Obj_AI_Hero target);

            public WeightDelegate WeightDefinition { get; set; }

            public void ComputeWeight(IEnumerable<Obj_AI_Hero> heroes, ref Dictionary<Obj_AI_Hero, float> weightedTotals)
            {
                Dictionary<Obj_AI_Hero, float> TargetResults = new Dictionary<Obj_AI_Hero, float>();

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

                    
                    var percent = (item.Value / sumValues) * 100;

                    if (this.Effect == WeightEffect.LowerIsBetter)
                    {
                        percent = 100 - percent;
                    }

                    var result = this.WeightValue * percent;

                    weightedTotals[item.Key] += result;
                }
            }

            public WeightEffect Effect { get; set; }
        }

        public enum WeightEffect
        {
            HigherIsBetter,
            LowerIsBetter
        }

        public enum TargetPriority
        {
            MaxPriority = 5,
            HighPriority = 4,
            MediumPriority = 3,
            LowPriority = 2,
            MinPriority = 1,
        }

        public void Dispose()
        {
            RenderManager.OnRender -= this.RenderManagerOnOnRender;
            Game.OnWndProc -= this.GameOnOnWndProc;
        }
    }
}
