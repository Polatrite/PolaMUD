using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolaMUD.Skills;
using PolaMUD.Affects;

namespace PolaMUD.Skills
{
	public class Heal : Skill
	{
		public Heal()
		{
			Name = "Heal";
			SkillType = SkillType.Damage;
			LearnedLevel = 1;
			UseMessage = "heals";

			Components.Add("Randomize", 10);
			Components.Add("DamageTypeMagical", 100);
			Components.Add("DamagePercentLevel", 700);
			Components.Add("DamagePercentMagical", 140);
		}

		public override bool Action(Mob user, string text)
		{
			if (!SetTargetAllyAsFirstArg(user, text))
				return false;

			user.WaitPulses += Global.RoundDuration;
			Combat.OneHeal(user, user.TargetAlly, this);

			return true;
		}
	}
}