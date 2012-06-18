using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class Constants
    {
        
    }

    /// <summary>
    /// Directions a character can travel
    /// </summary>
    public enum Directions
    {
        North,
        South,
        East,
        West,
        Up,
        Down
    }

    /// <summary>
    /// Types of messages that can be sent within a single room.
    /// Character - Sends a message to a single character
    /// Target - Sends a message to the victim of a single character
    /// Room - Sends a message to an entire room
    /// Not_Character - Sends a message to everyone but the character
    /// Not_Target - Sends a message to everyone but the victim
    /// Third_Party - Sends a message to everyone but the victim & character
    /// </summary>
    public enum MessageVector
    {
        Character,
        Target,
        Room,
        NotCharacter,
        NotTarget,
        ThirdParty
    }

    public enum Gender
    {
        Male,
        Neutral,
        Female,
    }
}
