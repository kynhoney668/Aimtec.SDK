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

    using Menu = Aimtec.SDK.Menu.Menu;

    public class TargetSelectorImpl : ITargetSelector
    {

        private static Menu Config { get; set; }

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        private static List<Weight> Weights = new List<Weight>();

        private static Obj_AI_Hero SelectedHero => HudManager.SelectedUnit as Obj_AI_Hero;

        private static string[] MaxPriority =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Kindred",
                "Leblanc", "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra",
                "Talon", "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "Velkoz", "Viktor",
                "Xerath", "Zed", "Ziggs", "Jhin", "Soraka"
            };


        private static string[] HighPriority =
            {
                "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean"
            };

        private static string[] MediumPriority =
            {
                "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
            };


        private static string[] LowPriority =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                "Taric", "TahmKench", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };


        private static IEnumerable<Obj_AI_Hero> Enemies => ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy);


        internal static void Load()
        {
            CreateMenu();
            CreateWeights();
            //RenderManager.OnRender += RenderManagerOnOnRender;
        }

        private static void RenderManagerOnOnRender()
        {
            var ordered = GetTargetsInOrder(10000).ToList();
            var basepos = new Vector2(RenderManager.Width / 2, 0.10f * RenderManager.Height);
            for (int i = 0; i < ordered.Count(); i++)
            {
                var targ = ordered[i];
                var target = targ.Target;
                if (target != null)
                {
                    RenderManager.RenderText(basepos + new Vector2(0, i * 15), Color.Red, target.Name + " " + targ.TotalWeight);
                }
            }
        }

        public static void AddWeight(Weight weight)
        {
            Weights.Add(weight);

            var weightMenu = (Menu)Config["WeightsMenu"];
            var item = weight.MenuItem;
            weightMenu.Add(item);
        }


        public static IOrderedEnumerable<WeightResult> GetTargetsInOrder(float range)
        {
            List<WeightResult> WeightResults = new List<WeightResult>();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range)))
            {
                var weightsValueTotal = Weights.Sum(x => x.WeightValue);

                double heroWeight = 0;

                foreach (var weight in Weights.Where(x => x.MenuItem.Enabled))
                {
                    heroWeight += weight.ComputeWeight(hero);
                }

                var result = new WeightResult { Target = hero, TotalWeight = heroWeight / weightsValueTotal };


                WeightResults.Add(result);
            }

            if (SelectedHero != null)
            {
                return WeightResults.OrderBy(x => x.Target.NetworkId == SelectedHero.NetworkId).ThenByDescending(x => x.TotalWeight);
            }

            var results = WeightResults.OrderByDescending(x => x.TotalWeight);

            return results;
        }

        public static Obj_AI_Hero GetTarget(float range)
        {
            if (SelectedHero != null && SelectedHero.IsValidTarget(range))
            {
                return SelectedHero;
            }

            if (Config["UseWeights"].Enabled)
            {
                List<WeightResult> WeightResults = new List<WeightResult>();

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range)))
                {
                    var weightsValueTotal = Weights.Sum(x => x.WeightValue);

                    double heroWeight = 0;

                    foreach (var weight in Weights.Where(x => x.MenuItem.Enabled))
                    {
                        heroWeight += weight.ComputeWeight(hero);
                    }

                    var result = new WeightResult { Target = hero };

                    result.TotalWeight = heroWeight / weightsValueTotal;

                    WeightResults.Add(result);
                }

                var bestResult = WeightResults.OrderByDescending(x => x.TotalWeight).FirstOrDefault();

                if (bestResult != null)
                {
                    var bestTarget = bestResult.Target;
                    return bestTarget;
                }

                return null;
            }

            return GetHighestPriority(range);

        }

        public static Obj_AI_Hero GetHighestPriority(float range, bool orbwalkTarget = false)
        {
            var validEnemies = Enemies.Where(x => (orbwalkTarget ? x.IsValidAutoRange() : x.IsValidTarget(range)));
            return validEnemies.OrderByDescending(GetPriority).FirstOrDefault();
        }

       
        Obj_AI_Hero ITargetSelector.GetTarget(float range)
        {
            return GetTarget(range);
        }


        public Obj_AI_Hero GetOrbwalkingTarget()
        {
            if (SelectedHero != null && SelectedHero.IsValidAutoRange())
            {
                return SelectedHero;
            }

            if (Config["UseWeights"].Enabled)
            {
                List<WeightResult> WeightResults = new List<WeightResult>();
 
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidAutoRange()))
                {
                    var weightsValueTotal = Weights.Sum(x => x.WeightValue);
 
                    double heroWeight = 0;

                    foreach (var weight in Weights.Where(x => x.MenuItem.Enabled))
                    {
                        Console.WriteLine(weight.Name + " IS ENABLED");
                        heroWeight += weight.ComputeWeight(hero);
                    }

                    Console.WriteLine(heroWeight);

                    var result = new WeightResult { Target = hero };

                    result.TotalWeight = heroWeight / weightsValueTotal;

                    WeightResults.Add(result);
                }

                var bestResult = WeightResults.OrderByDescending(x => x.TotalWeight).FirstOrDefault();

                if (bestResult != null)
                {
                    var bestTarget = bestResult.Target;
                    return bestTarget;
                }

                return null;
            }

            return GetHighestPriority(0, true);
        }

        public static TargetPriority GetPriority(Obj_AI_Hero hero)
        {
            var name = hero.ChampionName;

            if (MaxPriority.Contains(name))
            {
                return TargetPriority.MaxPriority;
            }

            if (MediumPriority.Contains(name))
            {
                return TargetPriority.MediumPriority;
            }

            if (LowPriority.Contains(name))
            {
                return TargetPriority.LowPriority;
            }

            return TargetPriority.MinPriority;
        }


        //Redefinable weights - these weights can be modified by assemblies so they can be customized per champion if necessary
        public static Weight DistanceToPlayerWeight = new Weight(
            "DistanceToPlayerWeight",
            "Distance to Player",
            true,
            100,
            target => target.Distance(Player),
            WeightEffect.LowerIsBetter);

        public static Weight DistanceToMouseWeight = new Weight(
            "DistanceToMouse",
            "Distance to Mouse",
            true,
            100,
            target => target.Distance(Game.CursorPos),
            WeightEffect.LowerIsBetter);

        public static Weight LeastAttacksWeight = new Weight(
            "LeastAttacksWeight",
            "Least Attacks",
            true,
            100,
            target => (int) Math.Ceiling(target.Health / Player.GetAutoAttackDamage(target)), 
            WeightEffect.LowerIsBetter);


        private static void CreateWeights()
        {
            AddWeight(DistanceToPlayerWeight);
            AddWeight(DistanceToMouseWeight);
           //AddWeight(LeastAttacksWeight); //GetAutoAttackDamage(Obj_AI_Hero) is bugsplatting

            AddWeight(new Weight(
                "PriorityWeight",
                "Most Priority",
                true,
                100,
                target => (int)GetPriority(target), WeightEffect.HigherIsBetter));

            AddWeight(new Weight(
                "MaxAttackDamageWeight",
                "Most AD",
                true,
                100,
                target => target.TotalAttackDamage, WeightEffect.HigherIsBetter));

            AddWeight(new Weight(
                "MaxAbilityPowerWeight",
                "Most AP",
                true,
                100,
                target => target.TotalAbilityDamage, WeightEffect.HigherIsBetter));

            AddWeight(new Weight(
                "MinArmorWeight",
                "Min Armor",
                true,
                100,
                target => target.Armor + target.BonusArmor, WeightEffect.LowerIsBetter));

            AddWeight(new Weight(
                "MinMRWeight",
                "Max MR",
                true,
                100,
                target => target.SpellBlock, WeightEffect.LowerIsBetter));

            AddWeight(new Weight(
                "MinHealthWeight",
                "Min Health",
                true,
                100,
                target => target.TotalAttackDamage, WeightEffect.LowerIsBetter));


        }

        
        private static void CreateMenu()
        {
            Config = new Menu("Aimtec.TS", "Target Selector");

            var weights = new Menu("WeightsMenu", "Weights");

            Config.Add(weights);

            var targetsMenu = new Menu("TargetsMenu", "Targets");

            foreach (var enemy in Enemies)
            {
                targetsMenu.Add(new MenuSlider("priority" + enemy.ChampionName, enemy.ChampionName, (int) GetPriority(enemy), 1, 5));
            }

            Config.Add(targetsMenu);

            Config.Add(new MenuBool("UseWeights", "Use Weights"));

            AimTecMenu.Instance.Add(Config);
        }


        public class WeightResult
        {
            public Obj_AI_Hero Target { get; set; }
            public double TotalWeight { get; set; }
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

    }
}
