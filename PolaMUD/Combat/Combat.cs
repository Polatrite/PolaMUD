using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public static class Combat
	{
		#region autoattacks, attack skills, damage calculation, killing

		public static Random Random = new Random();

		public static void MultiHit(Mob attacker, Mob target)
		{
			OneHit(attacker, target);
			OneHit(attacker, target);
		}

        /// <summary>
        /// Returns the damage of the provided skill used by the attacker against the target, performing all necessary damage calculations
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static int GetOneHit(Mob attacker, Mob target, Skill skill)
        {
            if (target == null)
                return 0;

            double damage = 0;

            int critDamage = 2000;
            bool crit = false;

            bool autoPhys = false;
            bool autoMag = false;

            if (skill.Components.ContainsKey("Autoattack"))
            {
                int highestStat = 0;
                bool physical = false;

                if (attacker.PhysicalPower >= highestStat)
                {
                    highestStat = attacker.PhysicalPower;
                    physical = true;
                }
                if (attacker.MagicPower > highestStat)
                {
                    highestStat = attacker.MagicPower;
                    physical = false;
                }

                if (physical == true)
                {
                    //addStatMods += attacker.PhysicalPower;
                    autoPhys = true;
                }
                else
                {
                    //addStatMods += attacker.MagicPower;
                    autoMag = true;
                }

                damage *= 100;
            }

            if (skill.Components.ContainsKey("DamageTypePhysical") || autoPhys)
            {
                int damageTypePhysical = 0;
                if (autoPhys)
                    damageTypePhysical = 100;
                else
                    damageTypePhysical = ((int)skill.Components["DamageTypePhysical"]);

                damage += attacker.PhysicalPower * damageTypePhysical;
                damage /= 100;
                damage *= 10000 / ((target.Armor * damageTypePhysical / 100) + 100);
                damage /= 100;

                if (Combat.Random.Next(0, 1000) <= attacker.PhysicalCritical * damageTypePhysical / 100)
                {
                    crit = true;
                    critDamage += attacker.PhysicalCriticalDamage * damageTypePhysical / 100;
                }
            }

            if (skill.Components.ContainsKey("DamageTypeMagical") || autoMag)
            {
                int damageTypeMagical = 0;
                if (autoMag)
                    damageTypeMagical = 100;
                else
                    damageTypeMagical = ((int)skill.Components["DamageTypeMagical"]);

                damage += attacker.MagicPower * damageTypeMagical;
                damage /= 100;
                damage *= 10000 / ((target.Resistance * damageTypeMagical / 100) + 100);
                damage /= 100;

                if (!skill.Components.ContainsKey("NoCritical"))
                {
                    if (Combat.Random.Next(0, 1000) <= attacker.MagicalCritical * damageTypeMagical / 100)
                    {
                        crit = true;
                        critDamage += attacker.MagicalCriticalDamage * damageTypeMagical;
                    }
                }

            }

            if (crit)
            {
                damage *= critDamage;
                damage /= 1000;
                attacker.SendMessage("Crit! ");
            }

            if (skill.Components.ContainsKey("Randomize"))
            {
                int mod = Random.Next(100 - (int)skill.Components["Randomize"] / 2, 100 + (int)skill.Components["Randomize"] / 2);
                damage *= mod;
                damage /= 100;
            }

            if (damage <= 0)
                damage = 1;

            if (skill.Components.ContainsKey("DamageTypePhysical") || autoPhys)
            {
                int damageTypePhysical = 0;
                if (autoPhys)
                    damageTypePhysical = 100;
                else
                    damageTypePhysical = ((int)skill.Components["DamageTypePhysical"]);


            }

            if (skill.Components.ContainsKey("DamageTypeMagical") || autoMag)
            {
                int damageTypeMagical = 0;
                if (autoMag)
                    damageTypeMagical = 100;
                else
                    damageTypeMagical = ((int)skill.Components["DamageTypeMagical"]);
            }

            return Convert.ToInt32(damage);
        }

		/// <summary>
		/// Execute one physical combat strike between the attacker and the target,
		/// dealing damage and notifying the room
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
        public static void OneHit(Mob attacker, Mob target)
        {
            OneHit(attacker, target, Global.SkillTable["Autoattack"]);
        }

        public static void OneHit(Mob attacker, Mob target, Skill skill)
        {
            OneHit(attacker, target, skill, GetOneHit(attacker, target, skill));
        }

		public static void OneHit(Mob attacker, Mob target, Skill skill, int dam)
		{
            if (CheckValidTarget(attacker, target) == false)
                return;

            if (attacker.Battle == null || attacker.Battle is RealtimeBattle)
                StartRealtimeCombat(attacker, target);
            else if (attacker.Battle == null || attacker.Battle is TurnBattle)
                StartTurnCombat(attacker, target);

            DoOneHit(attacker, target, skill, dam);
		}

        public static void DoOneHit(Mob attacker, Mob target, Skill skill, int damage)
		{
			if (damage > 0)
			{
				string damMessage = "";
				if (skill == null)
					damMessage = "hits";
				else
				{
					damMessage = skill.UseMessage;
				}
				target.Room.SendMessage(attacker.Name + " " + damMessage + " " + target.Name + "! (" + damage + ")\n\r", "dupe");
				Damage(attacker, target, damage);
			}
			else
			{
				target.Room.SendMessage(attacker.Name + " misses " + target.Name + ". (-)\n\r", "dupe");
			}
		}

		public static void Damage(Mob attacker, Mob target, int damage)
		{
			target.Health -= damage;

			if (target.Health <= 0)
			{
				Kill(attacker, target);
			}
		}

		public static void Kill(Mob attacker, Mob target)
		{
			Communications.DeliverMessage
				(target, null, MessageVector.Character, "\n\r\n\rYou are " + Colors.BRIGHT_RED + "DEAD!!" + Colors.CLEAR + "\n\r\n\r", "");
			Communications.DeliverMessage
				(target, null, MessageVector.ThirdParty, "$n is " + Colors.BRIGHT_RED + "DEAD!!" + Colors.CLEAR + "\n\r", "");

			if (target is Player)
			{
				//reward

				//clean up
				attacker.TargetEnemy = null;
				target.TargetEnemy = null;

				if (target.CombatType == CombatType.TurnBased)
				{
					target.Battle.Participants.Remove(target);
					target.Menu = null;
					attacker.Menu = null;
				}

				target.Battle = null;
				attacker.Battle = null;
				target.Move(Global.RoomTable[101]);
                target.Restore();

				target.WaitPulses = Global.RoundDuration * 5;
			}
			else if (target is Mob)
			{
				//reward
				attacker.RewardExperience(target.ExperienceReward);
				attacker.AddCoins(target.Coins);

				//clean up
				attacker.TargetEnemy = null;
				target.TargetEnemy = null;

				if (target.CombatType == CombatType.TurnBased)
				{
					target.Battle.Participants.Remove(target);
					target.Menu = null;
					attacker.Menu = null;
				}

				target.Battle = null;
				attacker.Battle = null;
				target.Delete();
			}
		}

		#endregion

		#region healing and buffs

		/// <summary>
		/// Heal one target notifying the room
		/// </summary>
		/// <param name="healer"></param>
		/// <param name="target"></param>
		/// <param name="skill"></param>
		public static void OneHeal(Mob healer, Mob target, Skill skill)
		{
			if (CheckValidTarget(healer, target, TargetType.Ally) == false)
				return;

			SetTargetAlly(healer, target);

			// if either char is IN battle, we need to add the other to that same battle
			if (healer.Battle != null || target.Battle != null)
			{
				AddOrCreateRealtimeBattle(new Mob[] { healer, target });
			}

			DoHeal(healer, target, GetHeal(healer, target, skill), skill);
		}

		public static int GetHeal(Mob attacker, Mob target, Skill skill)
		{
			double heal = 0;

			int critHeal = 2000;
			bool crit = false;

			if (skill.Components.ContainsKey("DamageTypePhysical"))
			{
				int damageTypePhysical = ((int)skill.Components["DamageTypePhysical"]);

				heal += attacker.PhysicalPower * damageTypePhysical;
				heal /= 100;

				if (Combat.Random.Next(0, 1000) <= attacker.PhysicalCritical * damageTypePhysical / 100)
				{
					crit = true;
					critHeal += attacker.PhysicalCriticalDamage * damageTypePhysical / 100;
				}
			}

			if (skill.Components.ContainsKey("DamageTypeMagical"))
			{
				int damageTypeMagical = ((int)skill.Components["DamageTypeMagical"]);

				heal += attacker.MagicPower * damageTypeMagical;
				heal /= 100;

				if (!skill.Components.ContainsKey("NoCritical"))
				{
					if (Combat.Random.Next(0, 1000) <= attacker.MagicalCritical * damageTypeMagical / 100)
					{
						crit = true;
						critHeal += attacker.MagicalCriticalDamage * damageTypeMagical;
					}
				}

			}

			if (crit)
			{
				heal *= critHeal;
				heal /= 1000;
				attacker.SendMessage("Crit! ");
			}

			if (skill.Components.ContainsKey("Randomize"))
			{
				int mod = Random.Next(100 - (int)skill.Components["Randomize"] / 2, 100 + (int)skill.Components["Randomize"] / 2);
				heal *= mod;
				heal /= 100;
			}

			if (heal <= 0)
				heal = 1;

			if (skill.Components.ContainsKey("DamageTypePhysical"))
			{
				int damageTypePhysical = ((int)skill.Components["DamageTypePhysical"]);
			}

			if (skill.Components.ContainsKey("DamageTypeMagical"))
			{
				int damageTypeMagical = ((int)skill.Components["DamageTypeMagical"]);
			}

			return Convert.ToInt32(heal);
		}

		public static void DoHeal(Mob attacker, Mob target, int heal, Skill skill)
		{
			string damMessage = "";
			if (skill == null)
				damMessage = "heals";
			else
			{
				damMessage = skill.UseMessage;
			}
			target.Room.SendMessage(attacker.Name + " " + damMessage + " " + target.Name + "! + " + heal + " +\n\r", "dupe");
			Heal(attacker, target, heal);
		}

		public static void Heal(Mob attacker, Mob target, int heal)
		{
			target.Health += heal;
		}


		#endregion

		#region targeting, starting/ending fights

		/// <summary>
		/// Use to ensure that the mobs are both targeting each other if they don't currently have targets.
		/// </summary>
		/// <param name="mob1"></param>
		/// <param name="mob2"></param>
		public static void SetTargetEnemies(Mob mob1, Mob mob2)
		{
			if (mob1.TargetEnemy == null)
				mob1.TargetEnemy = mob2;
			if (mob2.TargetEnemy == null)
				mob2.TargetEnemy = mob1;
		}

		/// <summary>
		/// Use to ensure that the mobs are both targeting each other if they don't currently have targets.
		/// </summary>
		/// <param name="mob1"></param>
		/// <param name="mob2"></param>
		public static void SetTargetAlly(Mob mob1, Mob mob2)
		{
			mob1.TargetAlly = mob2;
		}

		/// <summary>
		/// Check if one mob can successfully attack the other
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CheckValidTarget(Mob attacker, Mob target)
		{
			return CheckValidTarget(attacker, target, TargetType.Enemy);
		}

		public static bool CheckValidTarget(Mob attacker, Mob target, TargetType type)
		{
			if (type == TargetType.Enemy)
			{
				if (attacker.Room == null)
				{
					attacker.TargetEnemy = null;
					return false;
				}
				if (target == null)
				{
					attacker.TargetEnemy = null;
					return false;
				}
				if (target.Room == null)
				{
					attacker.TargetEnemy = null;
					target.TargetEnemy = null;
					return false;
				}

				if (attacker.Room != target.Room)
				{
					attacker.TargetEnemy = null;
					target.TargetEnemy = null;
					return false;
				}

				if (attacker.CombatType == CombatType.TurnBased && target is Player && attacker is Player)
				{
					attacker.TargetEnemy = null;
					target.TargetEnemy = null;
					return false;
				}

				if (attacker.CombatType == CombatType.Realtime && target.CombatType == CombatType.TurnBased)
				{
					attacker.TargetEnemy = null;
					target.TargetEnemy = null;
					return false;
				}

				return true;
				// TODO: This is where we'd add all the safety checks, safe rooms, non-PVP, can't be attacked, etc.
			}
			else if(type == TargetType.Ally)
			{
				return true;
			}

			return false;
		}

        public static void StartRealtimeCombat(Mob attacker, Mob target)
        {
            if (CheckValidTarget(attacker, target) == false)
                return;

            SetTargetEnemies(attacker, target);

            AddOrCreateRealtimeBattle(new Mob[] { attacker, target });
        }

		public static void StartTurnCombat(Mob attacker, Mob target)
		{
            if (attacker.Battle != null && target.Battle != null)
                return;

			if (CheckValidTarget(attacker, target) == false)
				return;

			SetTargetEnemies(attacker, target);

			attacker.CombatType = CombatType.TurnBased;
			target.CombatType = CombatType.TurnBased;

			TurnBattle battle = new TurnBattle();
			battle.Participants.Add(attacker);
			battle.Participants.Add(target);
			battle.NextTurn = attacker;
			battle.CurrentTurn = target;

			attacker.Battle = battle;
			target.Battle = battle;

			battle.SetNextTurn();

		}

		public static void AddOrCreateRealtimeBattle(Mob[] mobs)
		{
			RealtimeBattle battle = null;
			foreach (Mob mob in mobs)
			{
				if (mob.Battle != null && mob.Battle is RealtimeBattle)
					battle = (RealtimeBattle)mob.Battle;
			}

			if (battle == null)
			{
				battle = new RealtimeBattle();
			}

			foreach (Mob mob in mobs)
			{
                if (!battle.Participants.Contains(mob))
                {
                    battle.Participants.Add(mob);
                }
                mob.Battle = battle;
            }
		}

		#endregion

        public static int LevelModifier(Mob user, bool attack)
        {
            int modifierPercent = 100;

            if (attack == true)
            {
                modifierPercent = user.Level * 3 + 100;
            }

            return modifierPercent;
        }
    }
}
