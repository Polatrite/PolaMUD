using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;

namespace PolaMUD
{
    public class GameLoop
    {
        public decimal Ticks = 0;
        public decimal Rounds = 0;
        public decimal Pulses = 0;

        int TickCounter = 0;
        int RoundCounter = 0;

        public List<Event> TickEvents = new List<Event>();
        public List<Event> RoundEvents = new List<Event>();
        public List<Event> PulseEvents = new List<Event>();

        public GameLoop()
        {
            Timer pulseTimer = new Timer(Global.PulseDuration);
            pulseTimer.Elapsed += new ElapsedEventHandler(PulseTimerEvent);
            pulseTimer.Start();
        }

        /// <summary>
        /// A wrapper for the PulseTimer event. Do not use this function for anything but calling
		/// PulseTimer().
        /// </summary>
        /// <param name="state"></param>
        /// <param name="e"></param>
        void PulseTimerEvent(object state, ElapsedEventArgs e)
        {
            PulseTimer();
        }

        /// <summary>
        /// Signifies a game tick. This function is timed internally by PulseTimer().
        /// </summary>
        void TickTimer()
        {
            Global.SendToAll("Tick!\n\r", "");
            Ticks++;

            DecrementEvents(TickEvents);

            foreach (Mob mob in Global.Mobs)
            {
                DecrementAffects(mob, TimerType.Tick);
            }

			foreach (Area areas in Global.AreaTable.Values)
			{
                areas.Spawn();
			}
        }

        /// <summary>
        /// Signifies a combat round. This function is timed internally by PulseTimer().
        /// </summary>
        void RoundTimer()
        {
            //Global.SendToAll("(Round)\n\r");
            Rounds++;

            DecrementEvents(RoundEvents);

            foreach (Mob mob in Global.Mobs)
            {
                if (mob.CombatType == CombatType.Realtime)
                {
                    DecrementAffects(mob, TimerType.Round);
                }
            }

            foreach (Mob mob in Global.PlayersMobs)
            {
                if (mob.TargetEnemy != null)
                {
                    if (mob.CombatType == CombatType.Realtime)
                    {
                        Combat.MultiHit(mob, mob.TargetEnemy);
                    }
                }
            }
		}

        /// <summary>
        /// Called every game pulse, handling player I/O buffers and other events, as well as timing
		/// the Round and Tick Timers
        /// </summary>
        void PulseTimer()
        {
            //Global.Server.SendToAll(".");
            Pulses++;

            DecrementEvents(PulseEvents);

            foreach (Mob mob in Global.Mobs)
            {
                DecrementAffects(mob, TimerType.Pulse);
            }

            foreach (Player player in Global.Players)
            {
                player.SendOutgoingBuffer();

                if (player.WaitPulses > 0)
                {
                    player.WaitPulses--;

                    if (player.CombatType == CombatType.TurnBased)
                        player.WaitPulses = 0;
                }
                else if(player.IncomingBuffer.Count > 0)
                {
                    Parser.Interpret(player.Connection, player.IncomingBuffer.Dequeue());
                }
            }

            RoundCounter++;
            if (RoundCounter == Global.RoundDuration)
            {
                RoundTimer();
                RoundCounter = 0;
            }

            TickCounter++;
            if (TickCounter == Global.TickDuration)
            {
                TickTimer();
                TickCounter = 0;
            }
        }

        public void DecrementEvents(List<Event> list)
        {
            List<Event> Events = PulseEvents;
            List<Event> ToRemove = new List<Event>();
            foreach (Event even in Events)
            {
                if (even.Decrement() == 0)
                    ToRemove.Add(even);
            }
            foreach (Event even in ToRemove)
            {
                RoundEvents.Remove(even);
            }
        }

        public void DecrementAffects(Mob mob, TimerType type)
        {
            List<Affect> ToRemove = new List<Affect>();
            foreach (Affect affect in mob.Affects)
            {
                if (affect.DurationType == type)
                    if (affect.Decrement() == 0)
                        ToRemove.Add(affect);
            }
            foreach (Affect affect in ToRemove)
            {
                mob.Affects.Remove(affect);
            }
        }

		/// <summary>
		/// Adds an event to the timing stack. This will execute the actor's specified method after the delay. The method will be given the provided arguments.
		/// </summary>
		/// <param name="actor">The actor object which will receive the method call</param>
		/// <param name="delay">The delay in pulses (default)</param>
		/// <param name="method"></param>
		/// <param name="arguments"></param>
		public void AddEvent(object actor, int delay, string method, object[] arguments)
		{
            AddEvent(actor, TimerType.Pulse, delay, method, arguments);
		}

        public void AddEvent(object actor, TimerType type, int delay, string method, object[] arguments)
		{
            switch (type)
            {
                case TimerType.Pulse:
                    PulseEvents.Add(new Event(actor, delay, type, method, arguments));
                    break;
                case TimerType.Round:
                    RoundEvents.Add(new Event(actor, delay, type, method, arguments));
                    break;
                case TimerType.Tick:
                    TickEvents.Add(new Event(actor, delay, type, method, arguments));
                    break;
            }
		}

	}

    public enum TimerType
    {
        Pulse,
        Round,
        Tick
    }

    public class Event
    {
        public object Actor;
        public int Delay;
        public TimerType Type;
        public string Method;
        public object[] Arguments;

        public Event(object actor, int delay, TimerType type, string method, object[] arguments)
        {
            Actor = actor;
            Delay = delay;
            Type = type;
            Method = method;
            Arguments = arguments;
        }

        public int Decrement()
        {
            Delay--;

            if (Delay <= 0)
            {
                Type type = Actor.GetType();
                MethodInfo method = type.GetMethod(Method);
                method.Invoke(Actor, Arguments);
            }

            return Delay;
        }
    }
}
