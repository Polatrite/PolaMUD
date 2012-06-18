using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class TurnBattle : Battle
    {
        public Mob CurrentTurn;
        public Mob NextTurn;

        public void SetTurn(Mob target)
        {
            CurrentTurn = target;
        }

        public void EndBattle()
        {
            Mob mob = Participants[0];
            mob.SendMessage("You won the battle!\n\r", "dupe");
            mob.Menu = null;
        }

        public bool SetNextTurn()
        {
            if (Participants.Count == 1)
            {
                EndBattle();
                return false;
            }

            CurrentTurn = NextTurn;

            if (CurrentTurn is Player)
            {
                CurrentTurn.SendMessage("\n\rBattle! Your turn, action?\n\r", "t=output~txt=Battle! Your turn.\n\r");
                Skill skill;
                int count = 1;
                foreach(SkillInstance skillInstance in CurrentTurn.SkillSlots)
                {
                    skill = skillInstance.Skill;
                    CurrentTurn.SendMessage("- skill" + count + ": " + skill.Name + "\n\r");
                    count++;
                }
            }

            // Currently turn-based combat only supports two participants, the following will need to be changed to support more
            foreach (Mob mob in Participants)
            {
                if (mob != CurrentTurn)
                {
                    NextTurn = mob;
                }
            }

            return true;
        }

        public void HandleTurn(SkillInstance skill)
        {
            Global.GameLoop.DecrementAffects(CurrentTurn, TimerType.Round);
            for (int i = 0; i <= Global.RoundDuration; i++)
            {
                Global.GameLoop.DecrementAffects(CurrentTurn, TimerType.Pulse);
            }
            CurrentTurn.Skills["Autoattack"].Skill.Action(CurrentTurn, "");
            skill.Skill.Action(CurrentTurn, skill.Arguments);
            if (!SetNextTurn())
                return;

            if (CurrentTurn is Mob)
            {
                SkillInstance[] rolltable = new SkillInstance[1000];
                int index = 0;
                int startIndex = 0;
                foreach (SkillInstance skillInstanceT in CurrentTurn.SkillSlots)
                {
                    for(index = index; index < startIndex + skillInstanceT.AI.RandomWeight; index++)
                        rolltable[index] = skillInstanceT;

                    startIndex = index;
                }
                for (index = index; index <= 999; index++)
                    rolltable[index] = CurrentTurn.Skills["Autoattack"];

                int roll = Combat.Random.Next(0, 999);
                SkillInstance skillInstance = rolltable[roll];

                if (skillInstance != CurrentTurn.Skills["Autoattack"])
                    CurrentTurn.Skills["Autoattack"].Skill.Action(CurrentTurn, "");
                skillInstance.Skill.Action(CurrentTurn, "");
            }

            if (!SetNextTurn())
                return;
        }
    }
}
