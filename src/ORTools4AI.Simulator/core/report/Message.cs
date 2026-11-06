// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Ruth Meyer
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{


	/**
     * Base class for all message types used in the DESMO-J framework. Provides the
     * basic functionality and information common to all messages. Other Messages
     * must be derived from this class and extend the attributes to transport the
     * desired information. This allows for adding custom message bearing specific
     * information. Given the references to the objects to be displayed in a
     * message, the constructor extracts their names if possible. In case of
     * <code>null</code> references given as parameters, the String "----" is taken
     * instead.
     */
	public class Message
	{

		/**
		 * The name of the model that produced this message.
		 */
		private string _modName;

		/**
		 * The textual message to be published by a messagereceiver.
		 */
		private string _msgDescription;

		/**
		 * The point of simulation time that this message was created.
		 */
		private TimeInstant _msgTime;

		private string _expName;

		/// <summary>Constructs a message with the given parameters.</summary>
		/// <param name="origin">The model that produced this message</param>
		/// <param name="description">The actual message text</param>
		/// <param name="time">The point in simulation time this message was created</param>
		// TODO:
		public Message(Model origin, string description, TimeInstant time)
		{

			// fill in the attributes, checking vor void parameters
			// 
			// @modified by Nick Denz on 18.9.07
			// Reason: Some clients (e.g. SimTime) use this constructor
			// (indirectly) without passing an origin model. In this case,
			// querying the experiment name also fails. I have therefore
			// added an empty experiment name similar to the empty model name.
			if (origin == null)
			{
				_modName = "----";
				_expName = "----";
			}
			else
			{
				_modName = origin.getName();
				_expName = origin.getExperiment().getName();
			}

			if (description == null)
				_msgDescription = "----";
			else
				_msgDescription = description;

			// save the simulation time the message was created at
			_msgTime = time;

		}

		/// <summary>Returns the textual description of this message as a String.</summary>
		/// <returns>The message's description</returns>
		public string getDescription()
		{

			return _msgDescription;

		}

		/// <summary>Returns the name of the model that produced this message.</summary>
		/// <returns>The the name of the model that produced this message</returns>
		public string getModelName()
		{

			return _modName;

		}

		public string getExperimentName()
		{
			return _expName;
		}

		/// <summary>Returns the time the message was (created) sent as a TimeInstant object.</summary>
		/// <returns>The time the message was sent as a TimeInstant object.</returns>
		public TimeInstant getSendTime()
		{

			return _msgTime;
		}

		/// <summary>Returns the point of simulation time that this message was created as a String.</summary>
		/// <returns>the point of simulation time this message was created</returns>
		public string getTime()
		{

			if (_msgTime == null)
				return "----";
			else
				return _msgTime.toString();

		}

		/// <summary>
		/// Overrides the Object's <c>toString()</c> method to return the message's description when this object is
		/// used as a String.
		/// </summary>
		/// <returns>The message's description</returns>
		public string ToString()
		{

			return _msgDescription;

		}
	}
}