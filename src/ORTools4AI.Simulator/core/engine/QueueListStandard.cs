// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Justin Neumann
// @author based upon ideas from Tim Lechler, Soenke Claassen, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

using System.Text;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * Is the class summing up all the collective implementation of different
     * queueing strategies for a queue list. It provides all the basic methods
     * for inserting objects in a queue, retrieving objects from a queue and getting
     * basic informations about the queue. It is used in many kinds of queue
     * implementations e.g. in the classes <code>QueueListFifo</code> and
     * <code>QueueListLifo</code>.
     * 
     * @see QueueBased
     * @see Queue
     * @see ProcessQueue
     * @see QueueListFifo
     * @see QueueListLifo
     */
	public abstract class QueueListStandard<E> : QueueList<E> where E : Entity
	{

		/**
		 * An abbreviation to identify the sort of queueing discipline (like FIFO or
		 * LIFO or ...)
		 */
		protected string abbreviation;

		/**
		 * Uses the java.util.LinkedList for implementation
		 */
		protected LinkedList<E> queuelist;


		/// <summary>Initializes an empty standard queue list.</summary>
		/// <remarks>Associate the list with its owning queue through <c>setQueueBased</c>.</remarks>
		public QueueListStandard()
		{

			//super();

			//the here used java.LinkedList
			queuelist = new LinkedList<E>();

			//the here used java.WeakHashMap
			timemap = new Dictionary<E, TimeInstant>();

		}

		/// <summary>Checks whether the process using the QueueList is a valid SimProcess.</summary>
		/// <param name="proc">Is this SimProcess a valid one?</param>
		/// <returns>Returns whether the SimProcess is valid or not.</returns>
		protected bool checkProcess(SimProcess proc)
		{

			if (proc == null) // if proc is a null pointer instead of a process
			{
				sendWarning("A non existing process was stored in a queue. "
						+ "The attempted action is ignored!",
						"QueueListStandardFifo, Method: first(); called by Class: Stock, Method: "
								+ "store(long n) or retrieve(long n)",
						"The process is only a null pointer.",
						"Make sure that only real SimProcesses are stored in a queue.");
				return false;
			}

			if (!getQueueBased().isModelCompatible(proc)) // if proc is not
														  // modelcompatible
			{
				sendWarning(
						"The process trying to use a QueueList object does not "
								+ "belong to this model. The attempted action is ignored!",
						"QueueListStandardFifo, Method: first(); called by Class: Stock, Method: "
								+ "store(long n) or retrieve(long n)",
						"The process is not modelcompatible.",
						"Make sure that processes are using only queues within their model.");
				return false;
			}

			return true;
		}

		/// <summary>Returns true if the given Entity is contained in the list, false otherwise.</summary>
		/// <param name="e">The Entity assumed to be in the list</param>
		/// <returns>True if the given Entity is contained in the list, false otherwise</returns>
		public override bool contains(E e)
		{

			if (e == null)
			{ // check for nullreference
				sendWarning(
						"Can not check if the given Entity is contained in QueueListStandardFifo. "
								+ "Command ignored!",
						"Class: QueueListStandardFifo Method: boolean contains(Entity e).",
						"The Entity reference given as parameter is a null reference.",
						"Be sure to only use valid references.");
				return false;
			}

			return queuelist.Contains(e);

		}

		/// <summary>
		/// Returns the first entity stored in the list. If the queue is empty, <c>null</c> is returned.
		/// </summary>
		/// <returns>The first entity in the list or <c>null</c> if list is empty</returns>
		public override E first()
		{
			if (queuelist.Count==0)
			{
				return null;
			}
			else
			{
				return queuelist.First();
			}
		}

		/// <summary>
		/// Returns the position of the named <c>Entity</c>. The first position is 0, the last one size()-1.
		/// </summary>
		/// <returns>: The position of the <c>Entity</c> or <c>-1</c> if no such exists.</returns>
		public override int get(E element)
		{
			int index = 0;
			foreach (var item in queuelist)
			{
				if (Equals(item, element)) return index;
				index++;
			}
			return -1;
		}

		/// <summary>
		/// Returns the <c>Entity</c> queued at the named position. The first position is 0, the last one size()-1.
		/// </summary>
		/// <returns>
		/// The <c>Entity</c> at the position of <c>int</c> or <c>null</c> if no such position exists.
		/// </returns>
		public override E get(int index)
		{
			if (isEmpty())
			{
				sendWarning("Can not remove Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: boolean remove(Entity e).",
						"The Queue is empty, no Entities are contained.",
						"Check if an entity is enqueued by calling method "
								+ "contains(Entity e).");
				return null;
			}

			if ((index > this.size() - 1) || (index < 0))
			{
				sendWarning("Can not retrieve index. Command ignored.",
						"Class: QueueListStandard Method: Entity get(int index).",
						"The index of the method is out of the list range.",
						"Check correct position in queue.");
				return null;
			}

			int i = 0;
			foreach (var item in queuelist)
			{
				if (i == index) return item;
				i++;
			}
			return null;
		}

		/// <summary>
		/// Returns an abbreviation as a String to identify the sort of queueing discipline (like FIFO or LIFO or
		/// ...). Is used to display the queueing discipline in the report of the QueueBased objects.
		/// </summary>
		/// <returns>
		/// An abbreviation to identify the sort of queueing discipline (like FIFO or LIFO or ...)
		/// </returns>
		public override string getAbbreviation()
		{

			return abbreviation;
		}

		/// <summary>
		/// Adds a new Entity to the QueueList. Entities are inserted according to their priority in descending
		/// order. The highest priority Entity will always be first in the queue. Entities with same priority are
		/// inserted in specified order. Do not forget to call the <c>statisticalInsert()</c> and to set the queue
		/// for each entity as you define this method.
		/// </summary>
		/// <param name="e">The Entity to add to the QueueList</param>
		public abstract override void insert(E e);

		/// <summary>
		/// Inserts the given "e" after the position of "which" in the QueueList. Returns true if "e" is inserted
		/// correctly after "which". If the list is empty or the referenced "which" is not contained, the "e" will
		/// not be inserted and false is returned.
		/// </summary>
		/// <param name="e">The Entity to be inserted</param>
		/// <param name="which">The referenced Entity that the given "e" has to be inserted after</param>
		/// <returns>Is <c>true</c> if inserted correctly, <c>false</c> otherwise</returns>
		public override bool insertAfter(E e, E which)
		{

			if (e == null)
			{ // check for null reference
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: boolean insertAfter(Entity e, "
								+ "Entity which).",
						"The Entity reference 'e' given as parameter is a null "
								+ "reference.",
						"Be sure to only use valid references.");
				return false;
			}

			if (which == null)
			{ // check for null reference
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: boolean insertAfter(Entity e, "
								+ "Entity which).",
						"The Entity reference 'which' given as parameter is a null "
								+ "reference.",
						"Be sure to only use valid references.");
				return false;
			}

			if (contains(e))
			{ // entity must not be contained twice in the queue
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: boolean insertAfter(Entity e, "
								+ "Entity which).",
						"The Entity 'e' given as parameter is already enqueued.",
						"Make sure the entity is not enqueued here by calling "
								+ "method 'contains(Entity e)'.");
				return false;
			}

			queuelist.AddAfter(queuelist.Find(which), e); // is adding the entity
			e.addQueueBased(this.clientQ); // sets entity's queue as this queued

			statisticalInsert(e); // for statistics

			return true; // inserted

		}

		/// <summary>
		/// Inserts the given "e" before the position of "which" in the QueueList. Returns true if "e" is inserted
		/// correctly after "which". If the list is empty or the referenced "which" is not contained, the "e" will
		/// not be inserted and false is returned.
		/// </summary>
		/// <param name="e">The Entity to be inserted</param>
		/// <param name="which">The referenced Entity that the given "e" has to be inserted before</param>
		/// <returns>Is <c>true</c> if inserted correctly, <c>false</c> otherwise</returns>
		public override bool insertBefore(E e, E which)
		{

			if (e == null)
			{ // check for null reference
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: insertBefore(Entity e, "
								+ "Entity which).",
						"The Entity reference 'e' given as parameter is a null "
								+ "reference.",
						"Be sure to only use valid references.");
				return false;
			}

			if (which == null)
			{ // check for null reference
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: insertBefore(Entity e, "
								+ "Entity which).",
						"The Entity reference 'which' given as parameter is a null "
								+ "reference.",
						"Be sure to only use valid references.");
				return false;
			}

			if (contains(e))
			{ // entity must not be contained twice in queue
				sendWarning("Can not insert entity. Command ignored.",
						"Class 'QueueListStandardFifo' Method: insertBefore(Entity e, "
								+ "Entity which).",
						"The Entity 'e' given as parameter is already enqueued.",
						"Make sure the entity is not enqueued here by calling "
								+ "method 'contains(Entity e)'.");
				return false;
			}

			queuelist.AddBefore(queuelist.Find(which), e); // add on top as the rest is being shifted
			e.addQueueBased(this.clientQ); // sets entity's queue as this queued

			statisticalInsert(e); // for statistics

			return true; // inserted

		}

		/// <summary>
		/// Returns <c>true</c>, if no elements are inside the <c>QueueList</c>,<c>false</c> otherwise
		/// </summary>
		/// <returns>true, if no elements are inside the <c>QueueList</c>, false otherwise</returns>
		public override bool isEmpty()
		{

			return queuelist.Count==0;

		}

		/// <summary>
		/// Returns the last Entity stored in the QueueList. If the QueueList is empty, <c>null</c> is returned.
		/// </summary>
		/// <returns>The last Entity in the list or <c>null</c> if QueueList is empty</returns>
		public override E last()
		{
			if (queuelist.Count == 0) return null;
			return queuelist.Last.Value;
		}

		/// <summary>
		/// Returns the predecessor to the given Entity in the QueueList. If there is no predecessor or no Entity,
		/// <c>null</c> is returned.
		/// </summary>
		/// <param name="e">The Entity contained in the QueueList whose predecessor will be returned.</param>
		/// <returns>
		/// The Entity before the given parameter in the QueueList or <c>null</c> if e has no predecessor in the
		/// QueueList or Entity parameter 'e' itself is not contained
		/// </returns>
		public override E pred(E e)
		{

			if (e == null) // check for null reference
			{
				sendWarning("Can not return predecessor Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: Entity pred (Entity e).",
						"The Entity reference 'e' given as parameter is a null "
								+ "reference.",
						"Check if Entity 'e' is enqueued using method "
								+ "'QueueListStandardFifo.contains(e)'.");
				return null;
			}

			if (!this.contains(e)) // check for element contained
			{
				return null;
			}

			if (e.Equals(queuelist.First?.Value)) // check for first element
			{
				return null;
			}

			return queuelist.Find(e)?.Previous?.Value;

		}

		/// <summary>
		/// This method will be called every time the Stock (the number of available units) has changed.
		/// </summary>
		/// <param name="evt">The event specifying the property that has changed ans its old and new value.</param>
		public void propertyChange(PropertyChangeEvent evt)
		{

			// check if the property expected has changed
			if (evt.getPropertyName() == "avail")
			{
				// check if anybody is in the queue
				if (!isEmpty())
				{
					// get the first process in the queue
					SimProcess next = first() as SimProcess;

					// check if the process is not a null pointer or is not
					// modelcompatible
					if (!checkProcess(next))
					{
						return;
					} // just return

					// is the process scheduled already?
					if (next.isScheduled())
					{
						next.skipTraceNote(); // do not tell in the trace, that we
											  // ...
						next.cancel(); // get the process from the event-list
					}

					// remember if the process is blocked
					bool wasBlocked = next.isBlocked();

					// invalidate the block for a moment
					if (wasBlocked)
					{
						next.setBlocked(false);
					}

					next.skipTraceNote(); // do not tell in the trace, that we ...
					next.activateAfter(getQueueBased().current()); // schedule this
																   // process
																   // right after the current

					// set the block back
					if (wasBlocked)
					{
						next.setBlocked(true);
					}
				} // end if isEmpty()
			} // end if propertyName == available

		}

		/// <summary>
		/// Removes the first occurrence of the given Entity from the QueueList. Checks if the given Entity to be
		/// removed does apply to all restrictions on this operation. These are : The given reference to an entity
		/// must not be <c>null</c> This QueueList must not be empty, otherwise there's nothing to remove If all
		/// these restrictions apply, <c>true</c> is returned and the Entity is removed, otherwise <c>false</c> is
		/// the return value because the given Entity could not be removed since one of the restrictions above was
		/// not met.
		/// </summary>
		/// <param name="e">The Entity to be removed from the QueueList</param>
		/// <returns>
		/// Is <c>true</c> if the given Entity is contained in the QueueList, <c>false</c> otherwise
		/// </returns>
		public override bool remove(E e)
		{

			if (e == null)
			{ // check for null reference
				sendWarning(
						"Can not remove Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: boolean remove(Entity e).",
						"The Entity reference given as parameter is a null reference.",
						"Be sure to only use valid references.");
				return false;
			}

			// check if anything can be removed at all
			if (isEmpty())
			{
				sendWarning("Can not remove Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: boolean remove(Entity e).",
						"The Queue is empty, no Entities are contained.",
						"Check if an entity is enqueued by calling method "
								+ "contains(Entity e).");
				return false;
			}

			if (!this.contains(e))
			{ // check for element
				sendWarning("Can not return predecessor Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: Entity remove (Entity e).",
						"The Entity reference 'e' given as parameter is not contained "
								+ "reference.",
						"Insert e first.");
				return false;
			}

			//remove the first occurring Entity using the java.util.LinkedList
			queuelist.Remove(e);
			e.removeQueueBased(this.clientQ); // remove entity's queue note

			statisticalRemove(e); // remove for statistics

			return true; // job done

		}

		/// <summary>
		/// Removes the <c>Entity</c> queued at the named position. The first position is 0, the last one size()-1.
		/// </summary>
		/// <returns>
		/// : The method returns <c>true</c> as the <c>Entity</c> was deleted or <c>false</c> if otherwise.
		/// </returns>
		public override bool remove(int index)
		{
			E e = get(index);
			if (e == null)
			{
				sendWarning("Can not return predecessor Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: Entity remove (int index).",
						"The Entity reference 'e' given as parameter is not contained "
								+ "reference.",
						"Insert element first.");
				return false;
			}

			queuelist.Remove(e);
			e.removeQueueBased(this.clientQ); // remove entity's queue note

			statisticalRemove(e); // remove for statistics

			return true;
		}

		/// <summary>
		/// Removes the first entity from this QueueList and returns <c>true</c> if it was removed successfully. If
		/// the QueueList is empty, <c>false</c> is returned.
		/// </summary>
		/// <returns>
		/// Is <c>true</c>, if the first element has been removed successfully, <c>false</c> if the QueueList
		/// happened to be empty.
		/// </returns>
		public override bool removeFirst()
		{

			if (isEmpty())
			{
				sendWarning(
						"Can not remove first entity in queue. Command ignored.",
						"Class: QueueListStandardFifo Method: boolean removeFirst().",
						"The queue is empty, thus no Entity can be removed.",
						"Check if any Entity 'e' is enqueued using method "
								+ "'QueueListStandardFifo.contains(e)'.");
				return false; // nothing from nothing leaves nothing
			}

			return remove(first()); // delegate to remove()

		}

		/// <summary>
		/// Removes the last Entity from the QueueList and returns <c>true</c> if it was removed successfully. If
		/// the QueueList is empty, <c>false</c> is returned.
		/// </summary>
		/// <returns>
		/// Is <c>true</c>, if the last element has been removed successfully, <c>false</c> if the QueueList
		/// happened to be empty
		/// </returns>
		public override bool removeLast()
		{

			if (isEmpty())
			{
				sendWarning(
						"Can not remove last Entity in queue. Command ignored.",
						"Class: QueueListStandardFifo Method: boolean removeLast().",
						"The queue is empty, thus no Entity can be removed.",
						"Check if any Entity 'e' is enqueued using method "
								+ "'QueueListStandardFifo.contains(e)'.");
				return false; // nothing from nothing leaves nothing
			}

			return remove(last()); // delegate to remove()

		}

		/// <summary>
		/// Sends a warning to the error output by forwarding it to the associated QueueBased's <c>sendwarning</c>
		/// method. Warnings are sent only if the QueueBased's flag for queue implementation warnings is set to
		/// <c>true</c>.
		/// </summary>
		/// <param name="description">describing the error</param>
		/// <param name="location">describing the location the error occurred</param>
		/// <param name="reason">describing the possible cause for this error</param>
		/// <param name="prevention">telling what to do to prevent this error</param>
		/// <remarks>See also QueueBased</remarks>
		public override void sendWarning(string description, string location, string reason,
				string prevention)
		{

			if (clientQ.qImpWarn())
			{
				clientQ.sendWarning(description, location, reason, prevention);
			}

		}

		/// <summary>
		/// Returns the successor to the given Entity in the QueueList. If there is no successor or no Entity in
		/// the QueueList, <c>null</c> is returned.
		/// </summary>
		/// <param name="e">The Entity contained in the QueueList</param>
		/// <returns>
		/// The Entity before the given parameter in the QueueList or <c>null</c> if the given Entity parameter 'e'
		/// has no successor in the QueueList or e itself is not contained in the QueueList
		/// </returns>
		public override E succ(E e)
		{
			if (e == null)
			{
				sendWarning("Can not return successing Entity. Command ignored.",
						"Class: QueueListStandardFifo Method: Entity succ (Entity e).",
						"The Entity reference 'e' given as parameter is a null "
								+ "reference.",
						"Check if Entity 'e' is enqueued using method "
								+ "'QueueListStandardFifo.contains(e)'.");
				return null;
			}

			if (!this.contains(e))
			{
				return null;
			}

			if (e.Equals(queuelist.Last?.Value))
			{
				return null;
			}

			return queuelist.Find(e)?.Next?.Value;
		}

		/// <summary>
		/// Returns a string representation of the QueueList. The string is built by concatenating all string
		/// representations of the contained entities, calling their <c>toString()</c> methods.
		/// </summary>
		/// <returns>The string representation of the QueueList</returns>
		public override string toString()
		{
			StringBuilder s =new StringBuilder();
			int i = 0;
			foreach (var e in this.queuelist)
			{
				i++;
				s.Append(i).Append( ":[").Append( e).Append("]<br>");
			}

			if (isEmpty())
			{
				s.Append("-");
			}

			return s.ToString();

		}

		public override IEnumerator<E> GetEnumerator() => queuelist.GetEnumerator();

	}

    public class PropertyChangeEvent
    {
        private readonly string _propertyName;

        public PropertyChangeEvent(string propertyName)
        {
            _propertyName = propertyName;
        }

        internal string getPropertyName() => _propertyName;
    }

}