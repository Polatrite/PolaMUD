using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD.Areas
{
	public class TestArea
	{
		public TestArea()
		{
			Room room;

			room = new Room(3054);
			room.Name = "By the Temple's Altar";
			room.Description = @"You are by the temple's altar in the northern end of the Temple of 
the Gods. A huge altar made of white polished marble is standing in
front of you and behind it is a ten foot tall sitting statue of Odin,
the King of the Gods. From where you stand a back door can be seen
leading out to a ivy covered tower while a tall ornate doorway leads
east of here. The main temple's entrance is south of here and leads
farther into town. What catches your attention however is the start
of a rainbow right behind the altar.";
			room.Exits[Directions.South] = new Exit(3054, 3001);
			room.Exits[Directions.West] = new Exit(3054, 100);
			room.Exits[Directions.Down] = new Exit(3054, 3001);

			Global.Limbo = room;


			room = new Room(3001);
			room.Name = "The Temple Of The Gods";
			room.Description = @"You are in the southern end of the temple hall in the Temple of The Gods. The
temple has been constructed from giant marble blocks, eternal in appearance,
and most of the walls are covered by ancient paintings picturing gods, giants and
peasants. A heavy stone door on the east wall stands open to a steep stairwell
leading down into the darkness.";
			room.Exits[Directions.North] = new Exit(3001, 3054);
			room.Exits[Directions.East] = new Exit(3001, 3054);
			room.Exits[Directions.Up] = new Exit(3001, 3054);

		}
	}
}
