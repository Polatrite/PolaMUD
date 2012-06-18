using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public partial class Mob
    {
        public void SelectSkill(Mob target)
        {
            if (target == null)
            {
                target = TargetEnemy;
            }


        }

        public void SelectSkill()
        {
            SelectSkill(null);
        }
    }
}
