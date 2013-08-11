using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolaMUD.Skills;
using PolaMUD.Affects;

namespace PolaMUD.Skills
{
    #region Autoattack
    public class Autoattack : Skill
    {
        public Autoattack()
        {
            Name = "Autoattack";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "hits";

            Components.Add("Autoattack", true);
            Components.Add("Randomize", 20);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            Combat.MultiHit(user, user.TargetEnemy);

            return true;
        }

    }
#endregion
    #region Ice Blast
	public class IceBlast : Skill
	{
		public IceBlast()
		{
			Name = "Ice Blast";
			SkillType = SkillType.Damage;
			LearnedLevel = 1;
			UseMessage = "blasts ice at";

			Components.Add("Randomize", 20);
			Components.Add("DamageTypeMagical", 100);
			Components.Add("DamagePercentLevel", 500);
			Components.Add("DamagePercentMagical", 100);
		}

		public override bool Action(Mob user, string text)
		{
			if (!SetTargetEnemyAsFirstArg(user, text))
				return false;

			user.WaitPulses += Global.RoundDuration;
			Combat.OneHit(user, user.TargetEnemy, this);

			if (user.TargetEnemy != null)
				user.TargetEnemy.Affects.Add(new Affects.Freezing(user.TargetEnemy, user, 5));
			return true;
		}
	}
    #endregion
    #region Frostbolt
    public class Frostbolt : Skill
	{
		public Frostbolt()
		{
			Name = "Frostbolt";
			SkillType = SkillType.Damage;
			LearnedLevel = 1;
			UseMessage = "hurls a bolt of frost at";

			Components.Add("Randomize", 20);
			Components.Add("DamageTypeMagical", 100);
			Components.Add("DamagePercentLevel", 500);
			Components.Add("DamagePercentMagical", 100);
		}

		public override bool Action(Mob user, string text)
		{
			if (!SetTargetEnemyAsFirstArg(user, text))
				return false;

			user.WaitPulses += Global.RoundDuration;
			Combat.OneHit(user, user.TargetEnemy, this);

			if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Freezing(user.TargetEnemy, user, 5));
			return true;
		}
	}
#endregion
    #region Ignite
	public class Ignite : Skill
    {
		public Ignite()
        {
			Name = "Ignite";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "ignites";

            Components.Add("Randomize", 10);
			Components.Add("DamageTypeMagical", 100); // 120% damage DOT
			Components.Add("DamagePercentLevel", 500);
			Components.Add("DamagePercentMagical", 120);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration;
            Combat.OneHit(user, user.TargetEnemy, this);

            if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Burning(user.TargetEnemy, user, 3, Combat.GetOneHit(user, user.TargetEnemy, this) / 3));
            return true;
        }
    }
#endregion
    #region Fireball
    public class Fireball : Skill
    {
        public Fireball()
        {
            Name = "Fireball";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "hurls a fireball at";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100); // +30% DOT
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentMagical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration;
            Combat.OneHit(user, user.TargetEnemy, this);

            if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Burning(user.TargetEnemy, user, 3, Combat.GetOneHit(user, user.TargetEnemy, this) / 10));
            return true;
        }
    }
    #endregion
    #region Blind
    public class Blind : Skill
    {
        public Blind()
        {
            Name = "Blind";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "blinds";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;
            if (Combat.CheckValidTarget(user, user.TargetEnemy) == false)
                return false;
            Combat.StartRealtimeCombat(user, user.TargetEnemy);

            user.WaitPulses += Global.RoundDuration;

            if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Blind(user.TargetEnemy, user, 3));
            return true;
        }
    }
    #endregion
    #region Plague
    public class Plague : Skill
    {
        public Plague()
        {
            Name = "Plague";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "summons a plague on";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100);
            Components.Add("DamagePercentLevel", 300);
            Components.Add("DamagePercentMagical", 60);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;
            if (Combat.CheckValidTarget(user, user.TargetEnemy) == false)
                return false;
            Combat.StartRealtimeCombat(user, user.TargetEnemy);

            user.WaitPulses += Global.RoundDuration;

            if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Plague(user.TargetEnemy, user, 8, Combat.GetOneHit(user, user.TargetEnemy, this)/8));
            return true;
        }
    }
    #endregion
    #region Poison
    public class Poison : Skill
    {
        public Poison()
        {
            Name = "Poison";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "poisons";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100);
            Components.Add("DamagePercentLevel", 550);
            Components.Add("DamagePercentMagical", 110);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;
            if (Combat.CheckValidTarget(user, user.TargetEnemy) == false)
                return false;
            Combat.StartRealtimeCombat(user, user.TargetEnemy);

            user.WaitPulses += Global.RoundDuration;

            if (user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Poison(user.TargetEnemy, user, 15, Combat.GetOneHit(user, user.TargetEnemy, this)/15));
            return true;
        }
    }
    #endregion
    #region Thunderbolt
    public class Thunderbolt : Skill
    {
        public Thunderbolt()
        {
            Name = "Thunderbolt";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "hurls a thunderbolt at";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100);
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentMagical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration;
            Combat.OneHit(user, user.TargetEnemy, this);

            return true;
        }
    }
    #endregion
    #region LightningBlast
    public class LightningBlast : Skill
    {
        public LightningBlast()
        {
            Name = "Lightning Blast";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "sends a blast of lightning at";

            Components.Add("Randomize", 30);
            Components.Add("DamageTypeMagical", 100);
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentMagical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration;
            Combat.OneHit(user, user.TargetEnemy, this);

            return true;
        }
    }
    #endregion
    #region Backstab
	public class Backstab : Skill
    {
        public Backstab()
        {
            Name = "Backstab";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "backstabs";

            Components.Add("Randomize", 20);
            Components.Add("DamageTypePhysical", 100); // 100% of damage dealt is physical
			Components.Add("DamagePercentLevel", 1000);
			Components.Add("DamagePercentPhysical", 200); // 50% of your str
		}

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration * 2;
            Combat.OneHit(user, user.TargetEnemy, this);

            if(user.TargetEnemy != null)
                user.TargetEnemy.Affects.Add(new Affects.Bleeding(user.TargetEnemy, user, 20, Combat.GetOneHit(user, user.TargetEnemy, this) / 4));
            return true;
        }
    }
#endregion
    #region Savage Strike
    public class SavageStrike : Skill
    {
        public SavageStrike()
        {
            Name = "Savage Strike";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "savagely strikes";

            Components.Add("Randomize", 20);
            Components.Add("DamageTypePhysical", 100);
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentPhysical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration * 1;
            Combat.OneHit(user, user.TargetEnemy, this);
            return true;
        }
    }
#endregion
    #region Bite
    public class Bite : Skill
    {
        public Bite()
        {
            Name = "Bite";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "bites";

            Components.Add("Randomize", 20);
            Components.Add("DamageTypePhysical", 100);
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentPhysical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration * 1;
            Combat.OneHit(user, user.TargetEnemy, this);
            return true;
        }
    }
    #endregion
    #region Kick
    public class Kick : Skill
    {
        public Kick()
        {
            Name = "Kick";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "kicks";

            Components.Add("Randomize", 20);
            Components.Add("DamageTypePhysical", 100);
            Components.Add("DamagePercentLevel", 500);
            Components.Add("DamagePercentPhysical", 100);
        }

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration * 1;
            Combat.OneHit(user, user.TargetEnemy, this);
            return true;
        }
    }
    #endregion
    #region Flurry
    public class Flurry : Skill
    {
        public Flurry()
        {
            Name = "Flurry";
            SkillType = SkillType.Damage;
            LearnedLevel = 1;
            UseMessage = "unleashes a flurry on";

            Components.Add("Randomize", 20);
            Components.Add("DamageTypePhysical", 100);
			Components.Add("DamagePercentLevel", 500);
			Components.Add("DamagePercentPhysical", 100);
		}

        public override bool Action(Mob user, string text)
        {
            if (!SetTargetEnemyAsFirstArg(user, text))
                return false;

            user.WaitPulses += Global.RoundDuration * 1;
            Combat.OneHit(user, user.TargetEnemy, this, Combat.GetOneHit(user, user.TargetEnemy, this)/3);
			Combat.OneHit(user, user.TargetEnemy, this, Combat.GetOneHit(user, user.TargetEnemy, this)/3);
			Combat.OneHit(user, user.TargetEnemy, this, Combat.GetOneHit(user, user.TargetEnemy, this)/3);
            return true;
        }
    }
#endregion
}

namespace PolaMUD.Affects
{
    public class Bleeding : Affect
    {
        /// <summary>
        /// A bleed affect that deals damage every round.
        /// </summary>
        /// <param name="afflicted"></param>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="tickdamage"></param>
        public Bleeding(Mob afflicted, Mob owner, int duration, int tickdamage)
        {
            SetAffect("bleeding", afflicted, owner, TimerType.Round, duration, true, false);
            Parameters = new object[] { tickdamage };
        }

        public override void TickMethod()
        {
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + " suffers from wounds (" + (int)Parameters[0] + ")\n\r", "dupe");
            Combat.Damage(Owner, Afflicted, (int)Parameters[0]);
        }
    }

    public class Burning : Affect
    {
        /// <summary>
        /// A bleed affect that deals damage every round.
        /// </summary>
        /// <param name="afflicted"></param>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="tickdamage"></param>
        public Burning(Mob afflicted, Mob owner, int duration, int tickdamage)
        {
            SetAffect("burning", afflicted, owner, TimerType.Round, duration, true, false);
            Parameters = new object[] { tickdamage };
        }

        public override void TickMethod()
        {
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + " is on fire! (" + (int)Parameters[0] + ")\n\r", "dupe");
            Combat.Damage(Owner, Afflicted, (int)Parameters[0]);
        }
    }

	public class Freezing : Affect
	{
		/// <summary>
		/// A bleed affect that deals damage every round.
		/// </summary>
		/// <param name="afflicted"></param>
		/// <param name="owner"></param>
		/// <param name="duration"></param>
		/// <param name="tickdamage"></param>
		public Freezing(Mob afflicted, Mob owner, int duration)
		{
            SetAffect("freezing", afflicted, owner, TimerType.Round, duration, true, false);
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + " is enveloped in ice!\n\r", "dupe");
            Parameters = new object[] { };
		}
	}

    public class Blind : Affect
    {
        /// <summary>
        /// A bleed affect that deals damage every round.
        /// </summary>
        /// <param name="afflicted"></param>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="tickdamage"></param>
        public Blind(Mob afflicted, Mob owner, int duration)
        {
            SetAffect("blind", afflicted, owner, TimerType.Round, duration, true, false);
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + " is blinded!\n\r", "dupe");
            Parameters = new object[] { };
        }
    }

    public class Plague : Affect
    {
        /// <summary>
        /// A bleed affect that deals damage every round.
        /// </summary>
        /// <param name="afflicted"></param>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="tickdamage"></param>
        public Plague(Mob afflicted, Mob owner, int duration, int tickdamage)
        {
            SetAffect("plague", afflicted, owner, TimerType.Round, duration, true, false);
            Parameters = new object[] { tickdamage };
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + "'s skin erupts in sores! (" + (int)Parameters[0] + ")\n\r", "dupe");
        }

        public override void TickMethod()
        {
            Combat.Damage(Owner, Afflicted, (int)Parameters[0]);
        }
    }

    public class Poison : Affect
    {
        /// <summary>
        /// A bleed affect that deals damage every round.
        /// </summary>
        /// <param name="afflicted"></param>
        /// <param name="owner"></param>
        /// <param name="duration"></param>
        /// <param name="tickdamage"></param>
        public Poison(Mob afflicted, Mob owner, int duration, int tickdamage)
        {
            SetAffect("poison", afflicted, owner, TimerType.Round, duration, true, false);
            Parameters = new object[] { tickdamage };
            ((Room)Afflicted.Location).SendMessage(Afflicted.Name + " shivers and looks ill. (" + (int)Parameters[0] + ")\n\r", "dupe");
        }

        public override void TickMethod()
        {
            Combat.Damage(Owner, Afflicted, (int)Parameters[0]);
        }
    }

}