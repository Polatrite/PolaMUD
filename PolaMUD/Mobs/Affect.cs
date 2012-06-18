using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PolaMUD
{
	public class Affect : Thing
	{
		public int Level = 1;
		public Mob Owner;
        public Mob Afflicted;

		public bool PersistOnDeath = false;

		public Dictionary<string, int> Stats = new Dictionary<string, int>();

        public AffectType AffectType;

		public int Duration;
		public TimerType DurationType;

        // NO SOUP FOR YOU!
        public bool CanHasTickMethod;
        public bool CanHasEndMethod;

        public object[] Parameters;

        public Affect()
        {
        }

        public void SetAffect(string name, Mob afflicted, Mob owner, TimerType durationType, int duration, bool tickmethod, bool endmethod)
		{
			Name = name;
			HandlingName = name.ToLower().Trim();
			Duration = duration;
			DurationType = durationType;
			Owner = owner;
            Afflicted = afflicted;
			Level = Owner.Level;

            CanHasTickMethod = tickmethod;
            CanHasEndMethod = endmethod;
		}

        public int Decrement()
        {
            Duration--;

            if (CanHasTickMethod == true)
            {
                Type type = GetType();
                MethodInfo method = type.GetMethod("TickMethod");
                try
                {
                    method.Invoke(this, null);
                }
                catch (TargetInvocationException e)
                { Global.Error(e.ToString() + "\n\r\n\r"); }
            }

            if (Duration <= 0)
            {
                if (CanHasEndMethod == true)
                {
                    Type type = Afflicted.GetType();
                    MethodInfo method = type.GetMethod("EndMethod");
                    method.Invoke(this, null);
                }
            }

            return Duration;
        }

        public virtual void TickMethod()
        {
        }

        public virtual void EndMethod()
        {
        }

    }
}
