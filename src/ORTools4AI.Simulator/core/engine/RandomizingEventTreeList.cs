// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Ruth Meyer, modified by Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * A specialized Event tree list providing random order for concurrent Event notes.
     * Random order is achieved by computing a random insert position within the
     * range of simultaneous (concurrent) events. Existing connections between
     * events are maintained, i.e. a new event-note will never be inserted between
     * two connected event-notes. Connections are only possible between to
     * successive concurrent Event notes where one of the notes was inserted by call
     * of the insertBefore() or the insertAfter() method. Most of the methods
     * inherited from the super class
     * {@link desmoj.core.simulator.EventTreeList EventTreeList}are only overwritten to
     * keep track of the existing connections.
     */
	public class RandomizingEventTreeList : EventTreeList
	{

		// -------------------------------------------------------------------
		// Fields

		/** the random position generator. */
		private Random _positionGenerator;

		// -------------------------------------------------------------
		// Constructors

		/**
		 * Constructs a new randomizing Event tree list. Initializes the event tree list
		 * and the random position generator.
		 */
		public RandomizingEventTreeList()
		{
			//super();
			_positionGenerator = new Random();
		}

		// ------------------------------------------------------------------
		// Methods

		/// <summary>
		/// Inserts the given new event-note directly before the specified Event note. Registers <c>where</c> as
		/// connected to <c>newNote</c>.
		/// </summary>
		/// <param name="where">: the event-note before which the new note shall be inserted</param>
		/// <param name="newNote">: the new event-note to be inserted</param>
		public override void insertBefore(EventNote where, EventNote newNote)
		{
			base.insertBefore(where, newNote);
			// insertBefore means a "backward" connection: newNote is connected to
			// its successor where
			// this is translated to the "forward" connection: where is connected to
			// its predecessor newNote
			//int i = this.eTreeList.indexOf(where);
			if (this.eTreeList.contains(where))
				where.setConnected(true);
		}

		/// <summary>
		/// Inserts the given new event-note directly behind the specified Event note. Registers <c>newNote</c> as
		/// connected to <c>where</c>.
		/// </summary>
		/// <param name="where">: the event-note after which the new note shall be inserted</param>
		/// <param name="newNote">: the new event-note to be inserted</param>
		public override void insertAfter(EventNote where, EventNote newNote)
		{
			base.insertAfter(where, newNote);
			// insertAfter means a "forward" connection: newNote is connected to its
			// predecessor where
			//int i = this.eTreeList.indexOf(newNote);
			if (this.eTreeList.contains(newNote))
				newNote.setConnected(true);
		}

		/// <summary>Inserts the given event-note at the front of the event tree list.</summary>
		/// <param name="newNote">the new event-note to be inserted as first note.</param>
		public override void insertAsFirst(EventNote newNote)
		{
			base.insertAsFirst(newNote);
			newNote.setConnected(false);
		}


		/// <summary>
		/// Removes the given note from the event tree list. A connection between the note's previous and next note
		/// is established if and only if the given note was connnect to both the previous and next node.
		/// </summary>
		/// <param name="note">the event-note to be removed</param>
		public override void remove(EventNote note)
		{
			//int i = this.eTreeList.indexOf(note);
			if (this.eTreeList.contains(note))
			{
				EventNote prev = this.prevNote(note);
				EventNote next = this.nextNote(note);
				if (prev != null && next != null)
				{
					if (note.isConnected() && next.isConnected())
						next.setConnected(true);
					else
						next.setConnected(false);
				}
				base.remove(note);
			}
		}

		/**
		 * Removes the first event-note (if any).
		 */
		public override EventNote removeFirst()
		{
			if (!this.isEmpty())
			{
				EventNote note = base.removeFirst();
				if (!this.isEmpty())
					this.firstNote().setConnected(false);
				return note;
			}
			return null;
		}

		/// <summary>
		/// Inserts the given event-note into the event tree list. Overwrites the inherited insert() method to
		/// achieve random insert for concurrent Events. Takes possible connections between existing event-notes
		/// into account, i.e. will not insert the new note between connected events. Connections may only exist
		/// between two events of the same time where one of the events has been inserted via insertBefore() or
		/// insertAfter().
		/// </summary>
		/// <param name="newNote">the event-note to be inserted</param>
		public override void insert(EventNote newNote)
		{
			eTreeList.randomizedInsert(newNote, _positionGenerator);
		}

		public override bool isRandomizingConcurrentEvents() => true;

	} /* end of class RandomizingEventTreeList */
}