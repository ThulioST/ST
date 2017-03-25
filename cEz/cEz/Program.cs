using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace cEz
{
    class Program
    {
        private static Menu Menu, EzMenu, DrawMenu, HarassMenu, LaneMenu, JungleMenu, AutoHealMenu, SpellHit;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static float HealthPercent
        {
            get { return _Player.Health / _Player.MaxHealth * 100; }
        }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        private static AIHeroClient User = Player.Instance;
        //Spells
        private static Spell.Skillshot Q;
        private static Spell.Skillshot W;
        private static Spell.Skillshot E;
        private static Spell.Skillshot R;
        private static Spell.Active Heal;
        private static Item HealthPotion;
        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();
      

    private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (User.ChampionName != "Ezreal")
            {
                return;
            }
            Q = new Spell.Skillshot(spellSlot: SpellSlot.Q, spellRange: 1150, skillShotType: SkillShotType.Linear, castDelay: 250, spellSpeed: 200, spellWidth: 600)
            { AllowedCollisionCount = 0 };
            //-------------------------------//
            W = new Spell.Skillshot(spellSlot: SpellSlot.W, spellRange: 1000, skillShotType: SkillShotType.Linear, castDelay: 250, spellSpeed: 1550, spellWidth: 80)
            { AllowedCollisionCount = int.MaxValue };
            //------------------------------//
            E = new Spell.Skillshot(spellSlot: SpellSlot.E, spellRange: 475, skillShotType: SkillShotType.Circular, castDelay: 250, spellSpeed: null, spellWidth: 700);
            //-----------------------------//
            R = new Spell.Skillshot(spellSlot: SpellSlot.R, spellRange: 3000, skillShotType: SkillShotType.Linear, castDelay: 1000, spellSpeed: 2000, spellWidth: 160)
            { AllowedCollisionCount = int.MaxValue };

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            var slot = _Player.GetSpellSlotFromName("summonerheal");
            if (slot != SpellSlot.Unknown)
            {
                Heal = new Spell.Active(slot, 600);
            }
            HealthPotion = new Item(2003, 0);

            //Menu cEz//
            Menu = MainMenu.AddMenu("cEzreal", "cEzreal");
            //Combo
            EzMenu = Menu.AddSubMenu("Combo");
            /////////////////////////////////
            EzMenu.Add("Q", new CheckBox("Use Q", true));
            EzMenu.Add("HitChance Q", new Slider("HitChancePercent", 70));
            EzMenu.Add("W", new CheckBox("Use W", true));
            EzMenu.Add("HitChance W", new Slider("HitChancePercent", 50));
            EzMenu.Add("E", new CheckBox("Use E", true));
            EzMenu.Add("HitChance E", new Slider("HitChancePercent", 30));
            EzMenu.Add("R", new CheckBox("Use R", true));
            EzMenu.Add("HitChance R", new Slider("HitChancePercent", 65));
            //ComboBox SpellHit
            SpellHit = Menu.AddSubMenu("Hit%");
            /////////////////////////////
            SpellHit.Add("hit", new ComboBox("Hit Chance", 1, "Low", "Medium", "High"));

            //DrawSystem
            foreach (var Spell in SpellList)
            {
                DrawMenu.Add(Spell.Slot.ToString(), new CheckBox("Draw" + Spell.Slot));
            }

            //Harass
            HarassMenu = Menu.AddSubMenu("Harass");
            ///////////////////////////////////////
            HarassMenu.Add("Q", new CheckBox("Use Q", true));
            HarassMenu.Add("HitChance Q", new Slider("HitChancePercent", 70));
            HarassMenu.Add("ManaQ", new Slider("Min. Mana Percent:", 20));
            HarassMenu.Add("W", new CheckBox("Use W", true));
            HarassMenu.Add("HitChance W", new Slider("HitChancePercent", 50));
            HarassMenu.Add("ManaW", new Slider("Min. Mana Percent:", 20));
            HarassMenu.Add("R", new CheckBox("Use R", true));
            HarassMenu.Add("HitChance R", new Slider("HitChancePercent", 65));
            //AutoHeal
            AutoHealMenu = Menu.AddSubMenu("Potion & HeaL", "Potion & HeaL");
            AutoHealMenu.AddGroupLabel("Auto pot usage");
            AutoHealMenu.Add("potion", new CheckBox("Use potions"));
            AutoHealMenu.Add("potionminHP", new Slider("Minimum Health {0}(%) to use potion", 40));
            AutoHealMenu.Add("potionMinMP", new Slider("Minimum Mana {0}(%) to use potion", 20));
            AutoHealMenu.AddLabel("AUto Heal Usage");
            AutoHealMenu.Add("UseHeal", new CheckBox("Use Heal"));
            AutoHealMenu.Add("useHealHP", new Slider("Minimum Health {0}(%) to use Heal", 20));
            //Draw
            DrawMenu = Menu.AddSubMenu("Draws");
            //////////////////////////////////
            DrawMenu.Add("Q", new CheckBox("DrawQ", true));
            DrawMenu.Add("W", new CheckBox("DrawW", false));
            DrawMenu.Add("E", new CheckBox("DrawE", true));
            DrawMenu.Add("R", new CheckBox("DrawR", false));
            //LaneClear
            LaneMenu = Menu.AddSubMenu("LaneClear");
            //////////////////////////////////////
            LaneMenu.Add("Q", new CheckBox("Use Q", true));
            LaneMenu.Add("ManaQ", new Slider("Min. Mana Percent:", 20));
            //Jungle
            JungleMenu = Menu.AddSubMenu("JungleClear");
            ////////////////////////////////////////////
            JungleMenu.Add("Q", new CheckBox("Use Q"));
            JungleMenu.Add("ManaQ", new Slider("Min. Mana Percent:", 20));


            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Game_OnDraw;
        }

        private static void Game_OnDraw(EventArgs args)
        {
            foreach (var Spell in SpellList.Where(spell => DrawMenu[spell.Slot.ToString()].Cast<CheckBox>().CurrentValue))
            {
                if (DrawMenu["Q"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, User);
                }
                if (DrawMenu["E"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, User);
                }
                if (DrawMenu["W"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, User);
                }
                if (DrawMenu["R"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Spell.IsReady() ? Color.Chartreuse : Color.OrangeRed, Spell.Range, User);
                }
            }
        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                Autoheal();
            }
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
        }


        private static void Autoheal()
        {
            if (Heal != null && AutoHealMenu["UseHeal"].Cast<CheckBox>().CurrentValue && Heal.IsReady() &&
                HealthPercent <= AutoHealMenu["useHealHP"].Cast<Slider>().CurrentValue
                && _Player.CountEnemiesInRange(600) > 0 && Heal.IsReady())
            {
                Heal.Cast();
            }
        }
        private static void LaneClear()
        {
            var useq = LaneMenu["Q"].Cast<CheckBox>().CurrentValue;
            var mana = LaneMenu["ManaQ"].Cast<Slider>().CurrentValue;


            var qminion =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position,
                    Q.Range)
                    .FirstOrDefault(m =>
                        m.Distance(_Player) <= Q.Range &&
                        m.Health <= _Player.GetSpellDamage(m, SpellSlot.Q) - 20 &&
                        m.IsValidTarget());


            if (Q.IsReady() && useq && qminion != null && mana > Player.Instance.ManaPercent)
            {
                Q.Cast(qminion);
            }
    }
        private static void Harass()
        {
            var targetW2 = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (HarassMenu["W"].Cast<CheckBox>().CurrentValue &&
                W.IsReady() && targetW2.IsValidTarget(W.Range) &&
                Player.Instance.ManaPercent > HarassMenu["W"].Cast<Slider>().CurrentValue)
            {
                W.Cast(targetW2);
            }
            if (HarassMenu["Q"].Cast<CheckBox>().CurrentValue &&
                Q.IsReady() && targetW2.IsValidTarget(Q.Range) &&
                Player.Instance.ManaPercent > HarassMenu["ManaW"].Cast<Slider>().CurrentValue)
            {
                Q.Cast(targetQ);
            }
        var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var wmana = HarassMenu["ManaQandW"].Cast<Slider>().CurrentValue;
            var wharass = HarassMenu["W"].Cast<CheckBox>().CurrentValue;
            var useQharass = HarassMenu["Q"].Cast<CheckBox>().CurrentValue;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                if (wharass && W.IsReady() &&
                    target.Distance(_Player) > _Player.AttackRange &&
                    targetW.IsValidTarget(W.Range) && Player.Instance.ManaPercent > wmana)
                {
                    W.Cast(targetW);
                }
            }
        }
                   
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (EzMenu["Q"].Cast<CheckBox>().CurrentValue)
            {

                var Qpred = Q.GetPrediction(target);

                if (target.IsValidTarget(Q.Range) && Q.IsReady() && Qpred.HitChance >= HitChance.High)
                {
                    Q.Cast(target);
                }

                if (EzMenu["W"].Cast<CheckBox>().CurrentValue)
                {
                    var pred = W.GetPrediction(target);
                    if (target.IsValidTarget(W.Range) && W.IsReady() && pred.HitChance >= HitChance.High)
                    {
                        W.Cast(target);
                    }
                    if (EzMenu["E"].Cast<CheckBox>().CurrentValue)
                    {
                        var Epred = E.GetPrediction(target);

                        if (target.IsValidTarget(E.Range) && E.IsReady() && Epred.HitChance >= HitChance.High)
                        {
                            E.Cast(target);
                        }
                        if (EzMenu["R"].Cast<CheckBox>().CurrentValue)
                        {
                            var Rpred = W.GetPrediction(target);
                            if (target.IsValidTarget(R.Range) && R.IsReady() && Rpred.HitChance >= HitChance.High)
                            {
                                R.Cast(target);
                            }
                        }
                    }
                }
            }
        }
    }
}


