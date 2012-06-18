using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class SkillInstance
    {
        public Skill Skill;
        public string Arguments = "";
        public SkillAI AI;

        public SkillInstance(Skill skill)
        {
            Skill = skill;
        }

        public SkillInstance(Skill skill, SkillAI ai)
        {
            Skill = skill;
            AI = ai;
        }
    }
}
