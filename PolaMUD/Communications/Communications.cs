using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class Communications
    {
        /// <summary>
        /// Narrates a single action, calling the DeliverMessage method for
        /// MessageVectors Character, Target, and Third_Party.  This can be
        /// used if the message does not change at all between these three calls,
        /// save for the arguments of who-receives-what.
        /// 
        /// Maybe. :D
        /// 
        /// Work in progress.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool NarrateAction(Mob actor, Mob target, String message, String mobileMessage)
        {
            bool returner = true;
            if (DeliverMessage(actor, target, MessageVector.Character, message, mobileMessage) == false)
                returner = false;
            if (DeliverMessage(actor, target, MessageVector.Target, message, mobileMessage) == false)
                returner = false;
            if (DeliverMessage(actor, target, MessageVector.ThirdParty, message, mobileMessage) == false)
                returner = false;

            return returner;
        }

        /// <summary>
        /// Delivers a single message, calling the SendMessage method of all appropriate
        /// Mobs in both the actor's and target's rooms ("appropriate" determined by
        /// messageType).
        /// </summary>
        /// <param name="actor">Mob performing the narrated action (if any!)</param>
        /// <param name="target">Mob receiving the narrated action (if any!)</param>
        /// <param name="messageType">Vector for the message (who it is sent to)</param>
        /// <returns>false if any error conditions are met or any messages fail to send</returns>
        public static bool DeliverMessage(Mob actor, Mob target, MessageVector messageType, String message, String mobileMessage)
        {
            // Early exit conditions
            if (message == null || message.Equals(""))
                return false;
            if (actor == null && target == null)
                return false;
            if (messageType == MessageVector.NotCharacter && actor == null)
                return false;
            if (messageType == MessageVector.NotTarget && target == null)
                return false;

            bool returner = true;

            // Format all of the $ arguments in message
            message = formatMessage(actor, target, message, mobileMessage);

            // Handle the easy ones first: Character & Target;  no need to loop for these.
            if (messageType == MessageVector.Character)
            {
                if (actor == null)
                    return false;

                return actor.SendMessage(message, mobileMessage);
            }
            else if (messageType == MessageVector.Target)
            {
                if (target == null)
                    return false;

                return target.SendMessage(message, mobileMessage);
            }

            // Now we loop through the contents of the room.
            if (actor != null)
            {
                if (actor.Room != null && actor.Room.Contents != null)
                {
                    foreach (Mob audience in actor.Room.Contents)
                    {
                        if (messageType == MessageVector.NotCharacter && audience == actor)
                            continue;
                        if (messageType == MessageVector.NotTarget && audience == target)
                            continue;
                        if (messageType == MessageVector.ThirdParty
                            && (audience == actor || audience == target))
                            continue;

                        if (audience.SendMessage(message, mobileMessage) == false)
                            returner = false;
                    }
                }
                else
                {
                    return false;
                }
            }

            // If the actor is null (for whatever reason) or the target is in a different room, we
            // need to display the message to the appropriate audience in the target's room.
            if (target != null && (actor == null || target.Room.IndexNumber != actor.Room.IndexNumber))
            {
                if (target.Room != null && target.Room.Contents != null)
                {
                    foreach (Mob audience in target.Room.Contents)
                    {
                        if (messageType == MessageVector.NotCharacter && audience == actor)
                            continue;
                        if (messageType == MessageVector.NotTarget && audience == target)
                            continue;
                        if (messageType == MessageVector.ThirdParty
                            && (audience == actor || audience == target))
                            continue;

                        if (audience.SendMessage(message, mobileMessage) == false)
                            returner = false;
                    }
                }
                else
                {
                    return false;
                }
            }
                
            return returner;
        }

        /// <summary>
        /// Formats any $ arguments contained within a message string.
        /// </summary>
        /// <param name="actor">Mob performing the action</param>
        /// <param name="target">Mob receiving the action</param>
        /// <param name="message">Message string to format</param>
        public static String formatMessage(Mob actor, Mob target, String message, String mobileMessage)
        {
            StringBuilder buffer = new StringBuilder();
            String[] he = { "he", "it", "she" };
            String[] him = { "him", "it", "her" };
            String[] his = { "his", "its", "hers" };

            int index = 0;

            for (int c = 0; c < message.Length; c++)
            {
                if (message[c] == '$')
                {
                    if (c != 0)
                    {
                        buffer.Append(message.Substring(index, (c - index)));
                        index = c;
                    }
                        

                    if (message.Length >= c+1)
                    {
                        switch (message[c + 1])
                        {
                            case 'e':
                                if (actor != null)
                                    buffer.Append(he[(int)(actor.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 'E':
                                if (target != null)
                                    buffer.Append(he[(int)(target.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 's':
                                if (actor != null)
                                    buffer.Append(his[(int)(actor.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 'S':
                                if (target != null)
                                    buffer.Append(his[(int)(target.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 'm':
                                if (actor != null)
                                    buffer.Append(him[(int)(actor.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 'M':
                                if (target != null)
                                    buffer.Append(him[(int)(target.Gender)]);
                                c++;
                                index = c+1;
                                break;
                            case 'n':
                                if (actor != null)
                                    buffer.Append(actor.Name);
                                c++;
                                index = c+1;
                                break;
                            case 'N':
                                if (target != null)
                                    buffer.Append(target.Name);
                                c++;
                                index = c+1;
                                break;
                            // TODO:  Not sure what to do with these just yet.
                            case 't':
                                break;
                            case 'T':
                                break;
                            case 'p':
                                break;
                            case 'P':
                                break;
                            case '$':
                                buffer.Append('$');
                                c++;
                                index = c+1;
                                break;
                            default:
                                buffer.Append('$');
                                break;
                        }
                    }
                }
            }
            buffer.Append(message.Substring(index));

            return buffer.ToString();
        }
    }
}
