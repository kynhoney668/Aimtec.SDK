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

        private Logger Logger { get; } = LogManager.GetCurrentClassLogger();

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
                var ordered = this.GetTargetsInOrder(50000, ShowOrderAuto).ToList();
                var basepos = new Vector2(RenderManager.Width / 2f, 0.10f * RenderManager.Height);
                for (int i = 0; i < ordered.Count(); i++)
                {
                    var targ = ordered[i];
                    var target = targ.Target;
                    if (target != null)
                    {
                        RenderManager.RenderText(
                            basepos + new Vector2(0, i * 15),
                            Color.Red,
                            target.Name + " " + targ.WeightedAverage);
                    }
                }
            }
        }

        //For testing purposes...
        internal IOrderedEnumerable<WeightResult> GetTargetsInOrder(float range, bool autoRangeOnly)
        {
            List<WeightResult> weightResults = new List<WeightResult>();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => autoRangeOnly ? x.IsValidAutoRange() : x.IsValidTarget(range)))
            {
                var result = this.GetWeightedResult(hero);
                weightResults.Add(result);
            }

            if (this.SelectedHero != null)
            {
                return weightResults.OrderByDescending(x => x.Target.NetworkId == this.SelectedHero.NetworkId).ThenByDescending(x => x.WeightedAverage);
            }

            var results = weightResults.OrderByDescending(x => x.WeightedAverage);

            return results;
        }


        public void AddWeight(Weight weight)
        {
            this.Weights.Add(weight);
            this.Config["WeightsMenu"].As<Menu>().Add(weight.MenuItem);
        }


        public IOrderedEnumerable<Obj_AI_Hero> GetOrderedTargets(float range)
        {
            var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range));
            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return enemies.OrderByDescending(x => x.NetworkId == this.SelectedHero.NetworkId).ThenByDescending(x => GetWeightedResult(x).WeightedAverage);
            }
            return enemies.OrderByDescending(x => GetWeightedResult(x).WeightedAverage);
        }


        public Obj_AI_Hero GetTarget(float range)
        {
            if (this.SelectedHero != null && this.SelectedHero.IsValidTarget(range))
            {
                return this.SelectedHero;
            }

            if (this.Config["UseWeights"].Enabled)
            {
                List<WeightResult> weightResults = new List<WeightResult>();

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range)))
                {
                    var result = this.GetWeightedResult(hero);
                    weightResults.Add(result);
                }

                var bestResult = weightResults.OrderByDescending(x => x.WeightedAverage).FirstOrDefault();

                if (bestResult != null)
                {
                    var bestTarget = bestResult.Target;
                    return bestTarget;
                }

                return null;
            }

            return this.GetHighestPriority(range);

        }

        public WeightResult GetWeightedResult(Obj_AI_Hero hero)
        {
            var enabledWeights = this.Weights.Where(x => x.MenuItem.Enabled);

            var heroWeight = enabledWeights.Sum(x => x.ComputeWeight(hero));

            var weightsValueTotal = enabledWeights.Sum(x => x.WeightValue);

            var weightedAverage = heroWeight / weightsValueTotal;

            return new WeightResult { Target = hero, WeightedAverage = weightedAverage };
        }

        public Obj_AI_Hero GetHighestPriority(float range, bool orbwalkTarget = false)
        {
            var validEnemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => (orbwalkTarget ? x.IsValidAutoRange() : x.IsValidTarget(range)));
            return validEnemies.OrderByDescending(this.GetPriority).FirstOrDefault();
        }


        public Obj_AI_Hero GetOrbwalkingTarget()
        {
            if (this.SelectedHero != null && this.SelectedHero.IsValidAutoRange())
            {
                return this.SelectedHero;
            }

            if (this.Config["UseWeights"].Enabled)
            {
                List<WeightResult> weightResults = new List<WeightResult>();

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidAutoRange()))
                {
                    var result = this.GetWeightedResult(hero);
                    weightResults.Add(result);
                }

                var bestResult = weightResults.OrderByDescending(x => x.WeightedAverage).FirstOrDefault();

                if (bestResult != null)
                {
                    var bestTarget = bestResult.Target;
                    return bestTarget;
                }

                return null;
            }

            return this.GetHighestPriority(0, true);
        }

        public TargetPriority GetPriority(Obj_AI_Hero hero)
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
                "DistanceToPlayerWeight",
                "Distance to Player",
                true,
                100,
                target => target.Distance(Player),
                WeightEffect.LowerIsBetter));


            this.AddWeight(new Weight(
                "DistanceToMouse",
                "Distance to Mouse",
                true,
                100,
                target => target.Distance(Game.CursorPos),
                WeightEffect.LowerIsBetter));

            /*
            this.AddWeight(new Weight(
                "LeastAttacksWeight",
                "Least Attacks",
                true,
                100,
                target => (int)Math.Ceiling(target.Health / Player.GetAutoAttackDamage(target)),
                WeightEffect.LowerIsBetter));
                */

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
            this.Logger.Info("Constructing Menu for default Target Selector");

            this.Config = new Menu("Aimtec.TS", "Target Selector");

            var weights = new Menu("WeightsMenu", "Weights");

            this.Config.Add(weights);

            var targetsMenu = new Menu("TargetsMenu", "Targets");

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                targetsMenu.Add(new MenuSlider("priority" + enemy.ChampionName, enemy.ChampionName, (int)this.GetPriority(enemy), 1, 5));
            }

            this.Config.Add(targetsMenu);

            var drawings = new Menu("Drawings", "Drawings");
            drawings.Add(new MenuBool("IndicateSelected", "Indicate Selected Target"));
            drawings.Add(new MenuBool("ShowOrder", "Show Target Order"));
            drawings.Add(new MenuBool("ShowOrderAuto", "Auto range only"));
            this.Config.Add(drawings);


            this.Config.Add(new MenuBool("UseWeights", "Use Weights"));
        }


        public class WeightResult
        {
            public Obj_AI_Hero Target { get; set; }
            public float WeightedAverage { get; set; }
        }

        public class Weight
        {
            public Weight(string name, string displayName, bool enabled, int defaultWeight, WeightDelegate definition, WeightEffect effect)
            {
                if (defaultWeight > 100)
                {
                    throw new Exception("Weight cannot be more than 100.");
                }

                this.Name = name;

                this.MenuItem = new MenuSliderBool(name, displayName, true, defaultWeight, 0, 100);

                this.WeightDefinition = definition;

                this.Effect = effect;
            }

            public string Name { get; set; }

            public MenuComponent MenuItem { get; set; }

            public int WeightValue => this.MenuItem.Value;

            public string DisplayName { get; set; }

            public delegate float WeightDelegate(Obj_AI_Hero target);

            public WeightDelegate WeightDefinition { get; set; }

            public float ComputeWeight(Obj_AI_Hero unit)
            {
                if (this.Effect == WeightEffect.LowerIsBetter)
                {
                    return this.WeightValue * -this.WeightDefinition(unit);
                }

                return this.WeightValue * this.WeightDefinition(unit);
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
