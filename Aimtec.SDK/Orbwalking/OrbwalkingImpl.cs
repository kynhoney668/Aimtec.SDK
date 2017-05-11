using System;

namespace Aimtec.SDK.Orbwalking
{
    using Aimtec.SDK.Menu;

    internal class OrbwalkingImpl : IOrbwalker
    {

        public bool DisableAttacks { get; set; }
        public bool DisableMove { get; set; }

        public float AnimationTime { get; } = Player.AttackCastDelay * 1000;
        public float AutoAttackRange => Player.BoundingRadius + Player.AttackRange;
        public bool CanMove { get; private set; }
        public bool CanAttack { get; private set; }
        public float WindUpTime { get{throw new NotImplementedException();}}

        private static int LastAutoAttackTick = 0;

        public OrbwalkingMode Mode { get; set; }

        private string[] Attacks =
        {
            "caitlynheadshotmissile", "frostarrow", "garenslash2",
            "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced",
            "renektonexecute", "renektonsuperexecute",
            "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust",
            "xenzhaothrust2", "xenzhaothrust3", "viktorqbuff",
            "lucianpassiveshot"
        };

        private string[] BlacklistedAttacks =
        {
            "volleyattack", "volleyattackwithsound",
            "jarvanivcataclysmattack", "monkeykingdoubleattack",
            "shyvanadoubleattack", "shyvanadoubleattackdragon",
            "zyragraspingplantattack", "zyragraspingplantattack2",
            "zyragraspingplantattackfire", "zyragraspingplantattack2fire",
            "viktorpowertransfer", "sivirwattackbounce", "asheqattacknoonhit",
            "elisespiderlingbasicattack", "heimertyellowbasicattack",
            "heimertyellowbasicattack2", "heimertbluebasicattack",
            "annietibbersbasicattack", "annietibbersbasicattack2",
            "yorickdecayedghoulbasicattack", "yorickravenousghoulbasicattack",
            "yorickspectralghoulbasicattack", "malzaharvoidlingbasicattack",
            "malzaharvoidlingbasicattack2", "malzaharvoidlingbasicattack3",
            "kindredwolfbasicattack", "gravesautoattackrecoil"
        };


        private string[] AttackResets =
        {
            "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove",
            "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge",
            "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack",
            "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze",
            "netherblade", "gangplankqwrapper", "powerfist",
            "renektonpreexecute", "rengarq", "shyvanadoubleattack",
            "sivirw", "takedown", "talonnoxiandiplomacy",
            "trundletrollsmash", "vaynetumble", "vie", "volibearq",
            "xenzhaocombotarget", "yorickspectral", "reksaiq",
            "itemtitanichydracleave", "masochism", "illaoiw",
            "elisespiderw", "fiorae", "meditate", "sejuaninorthernwinds",
            "asheq", "camilleq", "camilleq2"
        };

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public OrbwalkingImpl()
        {
            
        }

        public Obj_AI_Base GetTarget()
        {
            return null;
        }

        public void ResetAutoAttackTimer()
        {
            LastAutoAttackTick = 0;
        }

        public void AddToMenu(IMenu menu)
        {
            
        }

        public void AutoAttack(AttackableUnit unit)
        {
            if (this.CanAttack)
            {
            }
        }

        public void ForceTarget(AttackableUnit unit)
        {
            
        }

        public void Orbwalk()
        {
            if (!this.CanMove)
            {
                return;
            }

            //Player.IssueOrder
        }

        public bool IsAutoAttack(string missileName)
        {
            return false;
        }

        public event EventHandler<BeforeAttackEventArgs> BeforeAttack;
        public event EventHandler<AttackEventArgs> Attack;
        public event EventHandler<AfterAttackEventArgs> AfterAttack;
        
    }
}