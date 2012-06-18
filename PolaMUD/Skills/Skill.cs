using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public class Skill : Thing
	{
		public SkillType SkillType;
		public int LearnedLevel = 999;
        public int Rank = 0;
        public int RankCostMultiplier = 100;

        public string UseMessage = "(null)";

        public Dictionary<string, object> Components = new Dictionary<string, object>();

        /// <summary>
        /// Lag that occurs after PreAction(), but before Action()
        /// </summary>
        public int PreLag = 0;
        /// <summary>
        /// Lag that occurs after Action(), but before PostAction()
        /// </summary>
        public int PostLag = 0;

        /// <summary>
        /// This is executed at the very moment the user uses the ability
        /// </summary>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual bool PreAction(Mob user, string text)
        {
            return true;
        }

        /// <summary>
        /// This is executed at a specific time based on the PreLag and PostLag variables
        /// </summary>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual bool Action(Mob user, string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is executed at the very end of the of the action, after both PreLag and PostLag
        /// </summary>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual bool PostAction(Mob user, string text)
        {
            return true;
        }

        public bool SetTargetEnemyAsFirstArg(Mob user, string text)
        {
            Type type = typeof(Mob);
            Argument arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
            Mob target = (Mob)arg1.Reference;
            if (target == null)
            {
                if (user.TargetEnemy == null)
                {
                    user.SendMessage("They aren't here!\n\r");
                    return false;
                }
                else
                {
                    target = user.TargetEnemy;
                }
            }

            if (user.TargetEnemy == null)
                user.TargetEnemy = target;

            return true;
        }

        public bool SetTargetAllyAsFirstArg(Mob user, string text)
        {
            Type type = typeof(Mob);
            Argument arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
            Mob target = (Mob)arg1.Reference;
            if (target == null)
            {
                if (user.TargetAlly == null)
                {
                    user.SendMessage("They aren't here!\n\r");
                    return false;
                }
                else
                {
                    target = user.TargetAlly;
                }
            }

            if (user.TargetAlly == null)
                user.TargetAlly = target;

            return true;
        }
    }

	public enum SkillType
	{
		Damage,
		DamageArea,
		Healing,
		HealingArea,
		Buff,
		Debuff,
		Cure,
		Dispel,
		Transportation,
		Disrupt,
		Trap
	}
}
