using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Thing is the parent of just about everything. Mobs, Players, Rooms - they're all Things!
    /// </summary>
	public class Thing
	{
        /// <summary>
        /// The global IndexNumber (or vnum) of the Thing.
        /// </summary>
		public int IndexNumber = 0;

        /// <summary>
        /// The name to use for displaying messages related to this Thing.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The name to use for command-matching in the parser.
		/// This string is automatically converted to lowercase.
        /// </summary>
		public string HandlingName
		{
			get { return this.handlingName; }
			set { handlingName = value.ToLower(); }
		}
		private string handlingName;


        public Thing()
        {
            Global.Things.Add(this);
        }

        ~Thing()
        {
            Global.Things.Remove(this);
        }
    }
}
