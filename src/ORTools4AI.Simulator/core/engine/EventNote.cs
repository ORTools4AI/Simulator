// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Justin Neumann
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Event notes contain any information needed for any type of <Code>Schedulable</Code>, event
     * or process. Since <Code>EventNote</Code> are tightly coupled with the <Code>EventList</Code>, a change
     * of the <Code>EventList</Code> might alo force the <Code>EventNote</Code> to contain other information
     * especially concerning the data structures used in the <Code>EventList</Code> as well. To adopt
     * the <Code>Event notes</Code> extend them to carry the specific data and methods you need
     * for a possible change of the <Code>EventList</Code>. The implementation here includes
     * data and methods for event information only, since the default implementation
     * of class <code>EventVector</code> needs no special information stored
     * inside the <Code>Event notes</Code>.
     */
	public class EventNote : IComparable<EventNote>
	{

		/**
		 * The Entity associated to an event and a point of simulation time. Can be
		 * <code>null</code> in case of an external event associated to this
		 * EventNote.
		 */
		private Entity _myEntity1; // Entity associated with this event-note

		/**
		 * The Entity associated to an event and a point of simulation time. Can be
		 * <code>null</code> in case of an external event associated to this
		 * EventNote.
		 */
		private Entity _myEntity2; // Entity associated with this event-note

		/**
		 * The Entity associated to an event and a point of simulation time. Can be
		 * <code>null</code> in case of an external event associated to this
		 * EventNote.
		 */
		private Entity _myEntity3; // Entity associated with this event-note

		/**
		 * The event associated to an entity and a point of simulation time. Can be
		 * <code>null</code> in case of a SimProcess associated to this event-note.
		 */
		private EventAbstract _myEvent; // type of Event that is going to happen

		/**
		 * The point of simulation time associated to an event and an entity. Must
		 * never be <code>null</code> since changes in the state of a model always
		 * happen to a certain discrete point of time.
		 */
		private TimeInstant _myTimeInstant; // time that the event is supposed to happen

		/**
		 * The priority associated to this EventNote.
		 */
		private int _myPriority; // time that the event is supposed to happen

		/**
		 * The Schedulable that has created this EventNote.
		 */
		private Schedulable _mySource;

		/**
		 * If an event-note is connected, <code>RandomizingEventVector</code> or
		 * <code>RandomizingEventTreeList</code> will not insert another event-note 
		 * (but not explicitly scheduled using
		 * <code>scheduleBefore()</code> or <code>scheduelAfter()</code>) 
		 * between this event-note and its predecessor scheduled for
		 * the same instant.
		 */
		private bool _isConnected; // flag for connection to predecessor

		// Fields used by BinaryHeapList — direct field access avoids Dictionary<T,…>
		// lookups on the scheduler hot path.
		internal int  HeapIndex;                       // 1-based position in heap array; 0 = not in heap
		internal long HeapSeq;                         // FIFO tiebreaker assigned on insertion
		internal long HeapAfterNext  = long.MinValue;  // next seqNo for insertAfter(this,…)  — MinValue = never used
		internal long HeapBeforeNext = long.MinValue;  // next seqNo for insertBefore(this,…) — MinValue = never used

		/// <summary>
		/// Event notes can only be created if all relevant data can be supplied at creation time. Note that all
		/// relevant associations of the given Schedulables towards this event-note are built in this constructor
		/// ensuring that each scheduled Schedulable has access to its associated EventNote.
		/// </summary>
		/// <param name="who1">
		/// The first entity that is scheduled to be change during the course of the event or the SimProcess (or
		/// null, if such an entity is not defined)
		/// </param>
		/// <param name="who2">
		/// The second entity that is scheduled to be change during the course of the event or the SimProcess (or
		/// null, if such an entity is not defined)
		/// </param>
		/// <param name="who3">
		/// The third entity that is scheduled to be change during the course of the event or the SimProcess (or
		/// null, if such an entity is not defined)
		/// </param>
		/// <param name="what">The type of Event that scheduled to happen to the Entity or entities</param>
		/// <param name="when">
		/// The point of time that this event-note is supposed to be processed by the scheduling mechanism
		/// </param>
		/// <param name="howImportant">The scheduling priority of this event note</param>
		/// <param name="source">The Schedulable responsible for creating this event note</param>
		public EventNote(Entity who1, Entity who2, Entity who3, EventAbstract what, TimeInstant when, int howImportant, Schedulable source)
		{
			_myEntity1 = who1;
			_myEntity2 = who2;
			_myEntity3 = who3;
			_myEvent = what;
			_myTimeInstant = when;
			_myPriority = howImportant;
			_mySource = source;

			if (what != null)
				what.addEventNote(this);

		}

		/// <summary>Copies the event-note. WARNING: No additional reference is set to linked Entity.</summary>
		/// <param name="source">The Schedulable responsible for copying this event note</param>
		/// <returns>The Entity associated to this event-note</returns>
		public EventNote copy(Schedulable source)
		{
			EventNote evn = null;

			if (getNumberOfEntities() <= 1)
			{
				evn = new EventNote(_myEntity1, null, null, _myEvent, _myTimeInstant, _myPriority, source);
				evn._isConnected = this.isConnected();
			}
			else if (getNumberOfEntities() == 2)
			{
				evn = new EventNote(_myEntity1, _myEntity2, null, _myEvent, _myTimeInstant, _myPriority, source);
				evn._isConnected = this.isConnected();
			}
			else if (getNumberOfEntities() == 3)
			{
				evn = new EventNote(_myEntity1, _myEntity2, _myEntity3, _myEvent, _myTimeInstant, _myPriority, source);
				evn._isConnected = this.isConnected();
			}
			{
				return evn;
			}
		}

		/// <summary>Returns whether this <c>EventNote</c> equals another one or not.</summary>
		/// <returns><c>true</c> or <c>false</c>.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is not EventNote note) return false;
			return _myTimeInstant.Equals(note._myTimeInstant)
				&& (_myEvent == note._myEvent || (_myEvent != null && _myEvent.Equals(note._myEvent)))
				&& (_myEntity1 == note._myEntity1 || (_myEntity1 != null && _myEntity1.Equals(note._myEntity1)))
				&& (_myEntity2 == note._myEntity2 || (_myEntity2 != null && _myEntity2.Equals(note._myEntity2)))
				&& (_myEntity3 == note._myEntity3 || (_myEntity3 != null && _myEntity3.Equals(note._myEntity3)))
				&& _myPriority == note._myPriority;
		}

		public override int GetHashCode()
		{
			return (_myEntity1 == null ?      13 : _myEntity1.GetHashCode()) ^
			       (_myEntity2 == null ?     983 : _myEntity2.GetHashCode()) ^
			       (_myEntity3 == null ?   33637 : _myEntity3.GetHashCode()) ^
			       (_myEvent   == null ? 7288583 : _myEvent.GetHashCode()) ^
			       _myTimeInstant.GetHashCode();
		}

		/// <summary>
		/// Compares the given EventNote to this event-note. This method implements the Comparable&lt;EventNote&gt;
		/// Interface
		/// </summary>
		/// <param name="note">The event-note to be compared to this event-note</param>
		/// <returns>
		/// Returns a negative integer, zero, or a positive integer as this event-note is before, at the same time,
		/// or after the given EventNote.
		/// </returns>
		public int CompareTo(EventNote? note)
		{
			if (note == null)
			{
				return 0; //TODO?
			}
			if ((note.getTime() == null) && this.getTime() == null)
			{
				return 0; //TODO?
			}
			else if (note == null | note.getTime() == null)
			{
				return -1; //TODO?
			}
			else if (this.getTime() == null)
			{
				return +1; //TODO?
			}
			int time_comparison = this.getTime().compareTo(note.getTime());
			if (time_comparison != 0)
				return time_comparison;
			else
				return -_myPriority.CompareTo(note._myPriority);
			//return -Integer.compare(this._myPriority, note._myPriority);
		}

		/// <summary>Returns the entity that is associated with this event-note.</summary>
		/// <returns>The Entity associated to this event-note</returns>
		public Entity getEntity1()
		{

			return _myEntity1;

		}

		/// <summary>Returns the entity that is associated with this event-note.</summary>
		/// <returns>The Entity associated to this event-note</returns>
		public Entity getEntity2()
		{

			return _myEntity2;

		}

		/// <summary>Returns the entity that is associated with this event-note.</summary>
		/// <returns>The Entity associated to this event-note</returns>
		public Entity getEntity3()
		{

			return _myEntity3;

		}

		/// <summary>Returns the event associated with this event-note.</summary>
		/// <returns>The event associated with this event-note</returns>
		public EventAbstract getEvent()
		{

			return _myEvent;

		}

		/// <summary>Returns the number of included entities.</summary>
		/// <returns><c>int</c> : The number of entities</returns>
		public long getNumberOfEntities()
		{
			int i = 0;
			if (_myEntity1 != null)
			{
				i++;
			}
			if (_myEntity2 != null)
			{
				i++;
			}
			if (_myEntity3 != null)
			{
				i++;
			}

			return i;

		}

		/// <summary>Returns the point of time associated with this event-note.</summary>
		/// <returns>Point of time in simulation associated with this EventNote</returns>
		public TimeInstant getTime()
		{

			return _myTimeInstant;

		}

		/// <summary>Returns the priority of this event-note.</summary>
		/// <returns>The scheduling priority of this EventNote</returns>
		public int getPriority()
		{
			return _myPriority;
		}

		internal void setPriority(int priority)
		{
			_myPriority = priority;
		}

		/// <summary>
		/// Associates this event-note with the given Entity. This is a package visibility method for internal
		/// framework use only.
		/// </summary>
		/// <param name="e">The Entity to be associated with this event-note</param>
		void setEntity1(Entity e)
		{

			_myEntity1 = e;

		}

		/// <summary>
		/// Associates this event-note with the given Entity. This is a package visibility method for internal
		/// framework use only.
		/// </summary>
		/// <param name="e">The Entity to be associated with this event-note</param>
		void setEntity2(Entity e)
		{

			_myEntity2 = e;

		}

		/// <summary>
		/// Associates this event-note with the given Entity. This is a package visibility method for internal
		/// framework use only.
		/// </summary>
		/// <param name="e">The Entity to be associated with this event-note</param>
		void setEntity3(Entity e)
		{

			_myEntity3 = e;

		}

		/// <summary>
		/// Associates this event-note with the given event. This is a package visibility method for internal
		/// framework use only.
		/// </summary>
		/// <param name="e">The event to be associated with this event-note</param>
		void setEvent(EventAbstract e)
		{

			_myEvent = e;

		}

		/// <summary>
		/// Sets the point of time for this event-note to the time given as parameter. This method is to be used by
		/// the scheduler to correct the point of time of an event-note after inserting it relative to some other
		/// EventNote to preserve the temporal order of the event-list. This is a package visibility method for
		/// internal framework use only.
		/// </summary>
		/// <param name="time">the new point of simulation time this event-note is associated with.</param>
		internal void setTime(TimeInstant time)
		{

			_myTimeInstant = time;

		}

		/// <summary>
		/// Tests if this event-note is connected to its predecessor. If a connection exists (true),
		/// <c>RandomizingEventVector</c> or <c>RandomizingEventTreeList</c> will not insert another event-note
		/// scheduled for the same instant (but not explicity scheduled using <c>scheduleBefore()</c> or
		/// <c>scheduelAfter()</c>) between this event-note and its predecessor scheduled for the same instant.
		/// This is a package visibility method for internal framework use only.
		/// </summary>
		/// <returns>Is <c>true</c> if a connection to the predecessor exists, <c>false</c> otherwise.</returns>
		internal bool isConnected()
		{
			return _isConnected;
		}

		/// <summary>Returns the Schedulable that has created this event note.</summary>
		/// <returns>The Schedulable that has created this event note</returns>
		public Schedulable getSource()
		{
			return _mySource;
		}

		/// <summary>
		/// Sets this event node connected to its predecessor (true), so that <c>RandomizingEventVector</c> or
		/// <c>RandomizingEventTreeList</c> will not insert another EventNote (but not explicity scheduled using
		/// <c>scheduleBefore()</c> or <c>scheduelAfter()</c>) between this event-note and its predecessor
		/// scheduled for the same instant or removes such a connection (false). This is a package visibility
		/// method for internal framework use only.
		/// </summary>
		/// <param name="isConnected">
		/// establishes (true) or removes (false) a connection between this event-note and its predecessor.
		/// </param>
		internal void setConnected(bool isConnected)
		{
			this._isConnected = isConnected;
		}

		internal void Reset(Entity who1, Entity who2, Entity who3, EventAbstract what, TimeInstant when, int howImportant, Schedulable source)
		{
			_myEntity1 = who1;
			_myEntity2 = who2;
			_myEntity3 = who3;
			_myEvent = what;
			_myTimeInstant = when;
			_myPriority = howImportant;
			_mySource = source;
			_isConnected = false;
			HeapIndex      = 0;
			HeapSeq        = 0;
			HeapAfterNext  = long.MinValue;
			HeapBeforeNext = long.MinValue;

			if (what != null)
				what.addEventNote(this);
		}

		internal void ClearForPool()
		{
			_myEntity1 = null;
			_myEntity2 = null;
			_myEntity3 = null;
			_myEvent = null;
			_myTimeInstant = null;
			_mySource = null;
			_isConnected = false;
			HeapIndex      = 0;
			HeapSeq        = 0;
			HeapAfterNext  = long.MinValue;
			HeapBeforeNext = long.MinValue;
		}

		/// <summary>
		/// Returns a string representing the elements bundled in this event-note. It calls the <c>toString()</c>
		/// methods of every element putting each in brackets containing one or two letters to indicate the type of
		/// element. En: Entity, SimProcess or <c>null</c> Ev: Event, external event or <c>null</c> t: simulation
		/// time
		/// </summary>
		/// <returns>representing the contained elements.</returns>
		public string toString()
		{

			string EntityString = "";

			if (getNumberOfEntities() == 1)
			{
				EntityString = "En: " + _myEntity1 + " ";
			}
			if (getNumberOfEntities() == 2)
			{
				EntityString = "En:" + _myEntity1 + "," + _myEntity2 + " ";
			}
			if (getNumberOfEntities() == 3)
			{
				EntityString = "En:" + _myEntity1 + "," + _myEntity2 + "," + _myEntity3 + " ";
			}

			return (EntityString + "Ev:" + _myEvent + " t:" + _myTimeInstant);

		}


	}
}