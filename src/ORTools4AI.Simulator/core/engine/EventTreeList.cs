// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, Ruth Meyer, modified by Johannes Göbel
// Modified for ORTools4AI.Simulator.

using PriorityQueues;
using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;


namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Alternative Implementation of the interface <code>EventList</code> using a
     * tree-based list as a container for the event-notes, yielding both access and
     * removal of event-list entries in O(log n) time.
     * 
     * Disadvantages compared to <code>EventVector</code> include
     * non-thread-safeness (however, discrete Event simulation should never attempt
     * concurrent modifications of the event-list) and the slightly higher memory
     * requirement.
     * 
     * The internal tree-based list is provided by the class
     * <code>org.apache.commons.collections.list.TreeList</code> from the Commons
     * Collections package from the Apache Jakarta Commons Project (see
     * http://jakarta.apache.org/commons/index.html). Thus, his product includes
     * software developed by The Apache Software Foundation
     * (http://www.apache.org/). For License see
     * http://www.apache.org/licenses/LICENSE-2.0 (of which a copy can be found in
     * the root directory of this distribtuon).
     * 
     * @see org.apache.commons.collections.list.TreeList
     * @see EventVectorList
     * @see EventNote
     */
	public class EventTreeList : EventList {

		/**
		 * The tree list container used to store the event-notes.
		 */
		public ITreeList<EventNote> eTreeList; // The tree list containing all Event notes.

		/**
		 * Constructs an empty event-list.
		 */
		public EventTreeList() {

			// create event-list
			//eTreeList = new TreeList<EventNote>();
            eTreeList=new PriorityQueue<EventNote>();
		}

		/// <summary>
		/// Returns the first event-note in the event-list. It is the event-note with the lowest (nearest)
		/// associated point of simulation time of all Event notes contained in the evnet-list. Note that the
		/// event-note is not removed from the event-list.
		/// </summary>
		/// <returns>
		/// the event-note to be processed next in the order of time. Returns <c>null</c> if the event-list is
		/// empty.
		/// </returns>
		public override EventNote firstNote() {

            if (isEmpty())
                return null; // nothing there, nothing returned
            else
                return eTreeList.getFirst();
			

		}


		/// <summary>
		/// Inserts the new event-note preserving the temporal order of the event-notes contained in the
		/// event-list. It uses binary search to determine the position where to insert the new event-note to
		/// increase performance.
		/// </summary>
		/// <param name="newNote">the new note to be inserted in the event-list keeping the temporal order</param>
		public override void insert(EventNote newNote) {

			// code for adding EventNote to all possible entities

			Entity who1 = newNote.getEntity1();
			if (who1 != null)
			{
				who1.addEventNote(newNote);
			}

			Entity who2 = newNote.getEntity2();
			if (who2 != null)
			{
				who2.addEventNote(newNote);
			}

			Entity who3 = newNote.getEntity3();
			if (who3 != null)
			{
				who3.addEventNote(newNote);
			}

			EventAbstract Event = newNote.getEvent();
			if (Event != null) {
				Event.addEventNote(newNote);
			}

			eTreeList.insert(newNote);
			
		}

		/// <summary>
		/// Inserts a new event-note after another EventNote specified. Note that to keep the temporal order of the
		/// event-list, the scheduled time will be set to the same time as the referred "afterNote". Note also,
		/// that afterNote must be contained in the event-list. If the referred "where" is not contained in the
		/// event-list, there is no chance to determine the time that the new note is intended to be scheduled at.
		/// Thus the new event-note will not be inserted and a <c>EventNotScheduledException</c> will be thrown,
		/// stopping the simulation.
		/// </summary>
		/// <param name="where">
		/// The event-note containing the event after which the new note is supposed to be inserted into the
		/// event-list.
		/// </param>
		/// <param name="newNote">
		/// The new event-note to be inserted after the specified EventNote in the event-list.
		/// </param>
		/// <exception cref="ORTools4AI.Simulator.core.exception.SimAbortedException">
		/// : if referred EventNote is not contained in the event-list
		/// </exception>
		public override void insertAfter(EventNote where, EventNote newNote) {

			// code for adding EventNote to all possible entities

			Entity who1 = newNote.getEntity1();
			if (who1 != null)
			{
				who1.addEventNote(newNote);
			}

			Entity who2 = newNote.getEntity2();
			if (who2 != null)
			{
				who2.addEventNote(newNote);
			}

			Entity who3 = newNote.getEntity3();
			if (who3 != null)
			{
				who3.addEventNote(newNote);
			}

			EventAbstract Event = newNote.getEvent();
			if (Event != null) {
				Event.addEventNote(newNote);
			}

			eTreeList.insertAfter(where, newNote);

		
		}

		/// <summary>
		/// Inserts the given EventNote at the first position in the event-list. The Event encapsulated in that
		/// EventNote will probably be the next event to be processed by the scheduler (unless some other calls to
		/// this method are made before). Note that for consistency the time of the new event-note is set to the
		/// time of the next entry, if the time of the next entry is earlier.
		/// </summary>
		/// <param name="newNote">The event-note to be inserted at the first position in the event-list.</param>
		public override void insertAsFirst(EventNote newNote) {

			if (!isEmpty()) { // if notes in EventList, ensure time is not later than second node
				TimeInstant next = (eTreeList.getFirst()).getTime();
				if (TimeInstant.isBefore(next, newNote.getTime())) {
					newNote.setTime(next);
				}
			}

			eTreeList.addFirst(newNote);

			// code for adding EventNote to all possible entities

			Entity who1 = newNote.getEntity1();
			if (who1 != null)
			{
				who1.addEventNote(newNote);
			}

			Entity who2 = newNote.getEntity2();
			if (who2 != null)
			{
				who2.addEventNote(newNote);
			}

			Entity who3 = newNote.getEntity3();
			if (who3 != null)
			{
				who3.addEventNote(newNote);
			}

			EventAbstract Event = newNote.getEvent();
			if (Event != null) {
				Event.addEventNote(newNote);
			}

		}

		/// <summary>
		/// Inserts a new event-note before another EventNote specified. Note that this could disturb the temporal
		/// order of the event-list. So this method should only be used carefully. Note also, that EventNote
		/// 'where' must be contained in the event-list or otherwise an exception will be thrown.
		/// </summary>
		/// <param name="where">
		/// The event-note containing the event before which the newNote is supposed to be inserted into the
		/// event-list.
		/// </param>
		/// <param name="newNote">
		/// The new event-note to be inserted before the specified EventNote in the event-list
		/// </param>
		/// <exception cref="ORTools4AI.Simulator.core.exception.SimAbortedException">
		/// : if referred EventNote is not contained in the event-list
		/// </exception>
		public override void insertBefore(EventNote where, EventNote newNote) {

			// code for adding EventNote to all possible entities

			Entity who1 = newNote.getEntity1();
			if (who1 != null)
			{
				who1.addEventNote(newNote);
			}

			Entity who2 = newNote.getEntity2();
			if (who2 != null)
			{
				who2.addEventNote(newNote);
			}

			Entity who3 = newNote.getEntity3();
			if (who3 != null)
			{
				who3.addEventNote(newNote);
			}

			EventAbstract Event = newNote.getEvent();
			if (Event != null) {
				Event.addEventNote(newNote);
			}

			eTreeList.insertBefore(where, newNote);
			

		}

		/// <summary>
		/// Tests if there are any scheduled events contained in the event-list. If the event-list happens to be
		/// empty during the run of a simulation, this is a criterium to stop the simulation, since no further
		/// action is scheduled.
		/// </summary>
		/// <returns>True if there are no Event notes contained in the event-list, false otherwise.</returns>
		public override bool isEmpty() {

			return eTreeList.isEmpty();
			// simply pass the call through to the tree list.

		}

        public override List<EventNote> iterators()
        {
            return eTreeList.iterator();
        }

        public override bool isRandomizingConcurrentEvents() => false;

        /// <summary>
        /// Returns the last EventNote in the event-list. If the event-list is empty, <c>null</c> will be
        /// returned.
        /// </summary>
        /// <returns>the last EventNote in the event-list, null if the event-list is empty</returns>
        public override EventNote lastNote() {

			if (isEmpty())
				return null; // Nothing here, nothign to return...
			else
                return (EventNote)eTreeList.getLast();
         
			// return last

		}

		/// <summary>
		/// Returns the next event-note in the event-list relative to the given EventNote. If the given EventNote
		/// is not contained in the event-list or happens to be the last EventNote in the event-list, null will be
		/// returned.
		/// </summary>
		/// <param name="origin">The event-note whose successor is wanted</param>
		/// <returns>
		/// The event-note following the given EventNote or &lt;ocde&gt;null if the given EventNote was last or not
		/// found
		/// </returns>
		public override EventNote nextNote(EventNote origin) {

            return eTreeList.nextNote(origin);
		}

		/// <summary>
		/// Returns the previous EventNote in the event-list relative to the given EventNote. If the given
		/// EventNote is not contained in the event-list or happens to be the first event-note in the event-list,
		/// null will be returned.
		/// </summary>
		/// <param name="origin">The event-note whose predecessor is wanted</param>
		/// <returns>
		/// The event-note following the given EventNote or &lt;ocde&gt;null if the given EventNote was first or
		/// not found
		/// </returns>
		public override EventNote prevNote(EventNote origin) {

            return eTreeList.prevNote(origin);
			

		}

		/// <summary>
		/// Removes the given EventNote from the event-list. Warning: Make sure to tell the entity of the
		/// event-note to delete the Note from its List as well. Warning: Make sure to tell the entity of the
		/// event-note to delete the Note from its List as well.
		/// </summary>
		/// <param name="note">The event-note to be removed from the event-list</param>
		public override void remove(EventNote note) {

			//int pos = eTreeList.indexOf(note);

			if (!eTreeList.contains(note))
				return; // do nothing if it doesn't exist
			else
			{
				eTreeList.remove(note); // go ahead and crunch it!

				if (note.getEntity1() != null) // if an entity exists (no external event)
				{
					note.getEntity1().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEntity2() != null) // if an entity exists (no external event)
				{
					note.getEntity2().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEntity3() != null) // if an entity exists (no external event)
				{
					note.getEntity3().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEvent() != null)   // if an event exists
				{
					note.getEvent().removeEventNote(note);      // remove EventNote
				}
			}

		}

		/**
         * Removes the first event-note from the event-list. Does nothing if the
         * event-list is already empty.
         */
		public override EventNote removeFirst() {

			if (!eTreeList.isEmpty())
			{
				EventNote note = (EventNote)eTreeList.getFirst();
                eTreeList.removeFirst();

				if (note.getEntity1() != null) // if an entity exists (no external event)
				{
					note.getEntity1().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEntity2() != null) // if an entity exists (no external event)
				{
					note.getEntity2().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEntity3() != null) // if an entity exists (no external event)
				{
					note.getEntity3().removeEventNote(note); // removes list entry in Entity!
				}

				if (note.getEvent() != null)   // if an event exists
				{
					note.getEvent().removeEventNote(note);      // remove EventNote
				}

				return note;
			}
			return null;
		}

		/**
		 * Returns a string representing the entries of this tree list in a row. The
		 * resulting string includes all Event notes in ascending order as they are
		 * placed inside the event tree list.
		 */
		public string toString() {

			StringBuilder textBuffer = new StringBuilder();
			// faster than String and '+'
			var notes = eTreeList.iterator(); // get all elements

			foreach(var note in notes) { // loop through all elements
				textBuffer.Append("[");
				textBuffer.Append(note);
				textBuffer.Append("]"); // compose Note
			}

			return textBuffer.ToString();
			// return String representation of StringBuffer

		}

	}

    public class TreeList<T>:ITreeList<T> where T:EventNote
    {
		

		private List<T> _data;
        
		public TreeList()
		{
			_data = new List<T>();
		}
        public TreeList(Comparison<T> comparer)
		{
			//priority_queue = new BinaryPriorityQueue<T>(comparer);

        }
        public  void add(T newNote)
        {
			_data.Add(newNote);
        }

		public void addFirst(T newNote)
		{

			add(0, newNote);
        }
        private void add(int v, T newNote)
        {
			_data.Insert(v, newNote);
        }

		public void insert(T newNote)
		{
            if (isEmpty())
            { // no worry about order if it's first
                add(newNote); // easy if empty ;-)
                return; // no need to continue
            }
            else
            { // now here comes the binary sorting

                int left = 0; // left border of search partition
                int right = size() - 1; // right border of search
                                                  // partition
                int index = 0; // current position in tree list
                TimeInstant refTime = newNote.getTime();
                long refPrio = newNote.getPriority();
                // shortcut for call to newNote

                do
                {
                    index = (left + right) / 2; // center on searchable partition
                                                // check if EventNote at index has smaller or equal time

                    if (TimeInstant.isBefore((get(index)).getTime(), refTime) ||
                            (TimeInstant.isEqual((get(index)).getTime(), refTime) &&
                                    (get(index)).getPriority() >= refPrio))
                    {
                        if (index < (size() - 1))
                        {
                            // is there a note to the right
                            if (TimeInstant.isAfter((get(index + 1)).getTime(), refTime) ||
                                    (TimeInstant.isEqual((get(index + 1)).getTime(), refTime) &&
                                            (get(index)).getPriority() < refPrio)
                                    )
                            {
                                // if note to the right is larger
                                add(index + 1, newNote);
                                // found position
                                return; // everything done, no need to continue
                            }
                            else
                            {
                                left = index + 1;
                                // no hit, so set new boundaries and go on
                            }
                        } // no Event notes right of the index, so all
                        else
                        { // notes are smaller, thus append to end
                            add(newNote);
                            return; // everything done, get out of here fast!
                        }
                    }
                    else
                    { // EventNote at index has larger time
                        if (index > 0)
                        { // is there a note left of the index?
                            if (TimeInstant.isBefore((get(index - 1)).getTime(), refTime) ||
                                    (TimeInstant.isEqual((get(index - 1)).getTime(), refTime)
                                    &&
                                    (
                                            get(index - 1)).getPriority() >= refPrio)
                                    )
                            {
                                // if note to the left is smallerOrEqual
                                add(index, newNote);
                                // found position
                                return; // everything done, no need to continue
                            }
                            else
                            {
                                right = index - 1;
                                // no hit, so set new boundaries and go on
                            }
                        } // no Event notes left of the index, so all
                        else
                        { // notes are larger, thus insert at pos. 0
                            add(0, newNote);
                            return; // everything done, get out of here!
                        }
                    }
                } while ((left <= right));
                add(newNote);
            }


        }

		public void insertAfter(T where, T newNote)
		{
            int i = indexOf(where);

            if (i < 0)
            { // negative index means, that where is not contained
                Model mBuffer = null; // buffer current model
                if (newNote.getEntity1() != null)
                {
                    mBuffer = newNote.getEntity1().getModel();
                }
                if (newNote.getEvent() != null)
                {
                    mBuffer = newNote.getEvent().getModel();
                }
                throw new SimAbortedException(
                        new ErrorMessage(
                                mBuffer,
                                "Can not insert new event-note after given EventNote! "
                                        + "Simulation aborted",
                                "Internal DESMO-J class : EventTreeList Method : "
                                        + "insertAfter(EventNote where, EventNote newNote)",
                                "The event-note to insert the new note after is not contained "
                                        + "in the event tree list.",
                                "This is a fatal error. Contact DESMOJ support",
                                newNote.getTime()));
            }
            else
            { // if where is contained, put newNote at next position
                newNote.setTime(where.getTime());
                // synchronize times to keep order
                add(i + 1, newNote);
                // everything fine, exit...
            }
        }

		public void insertBefore(T where, T newNote)
		{
            int i = indexOf(where);

            if (i < 0)
            {
                Model mBuffer = null; // buffer current model
                if (newNote.getEntity1() != null)
                {
                    mBuffer = newNote.getEntity1().getModel();
                }
                if (newNote.getEvent() != null)
                {
                    mBuffer = newNote.getEvent().getModel();
                }
                throw new SimAbortedException(
                        new ErrorMessage(
                                mBuffer,
                                "Can not insert new event-note before given EventNote! "
                                        + "Simulation aborted",
                                "Internal DESMO-J class : EventTreeList Method : "
                                        + "insertBefore(EventNote where, EventNote newNote)",
                                "The event-note to insert the new note before is not contained "
                                        + "in the event tree list.",
                                "This is a fatal error. Contact DESMOJ support",
                                newNote.getTime()));
            }
            else
            {
                newNote.setTime(where.getTime());
                // synchronize times to keep order
                add(i, newNote);
                // insert newN. & push afterN. one up

                // code for adding EventNote to Entity
                //variable was not used
                //Entity who = newNote.getEntity1();
                //variable was not used
                //Object o = who;
            }
        }
        public bool contains(T origin)
        {
		   return _data.Contains(origin);
        }

        private T get(int v)
        {
			return _data[v];
        }

        private int indexOf(T where)
        {
			return _data.IndexOf(where);
        }

        public bool isEmpty()
        {
			return _data.Count==0;
        }

        public List<T> iterator()
        {
			return _data;
        }


        private void remove(int pos)
        {
           _data.RemoveAt(pos);
        }

        public void remove(T where)
        {
            var i=indexOf(where);
            remove(i); // go ahead and crunch it!
        }

        public int size()
        {
			return _data.Count;
        }

        public void randomizedInsert(T newNote,Random _positionGenerator)
        {
            if (isEmpty())
            {
                insert(newNote);
                newNote.setConnected(false);
                // notes inserted via insert() are not connected to other notes
                return; // no need to continue
            }
            // use binary search to determine first event-note with same time
            TimeInstant refTime = newNote.getTime();
            long refPrio = newNote.getPriority();
            int firstIndexForInsert, lastIndexForInsert;
            int left = 0;
            int right = size();
            while (left < right)
            {
                int middle = (left + right) / 2;
                if (TimeInstant.isBefore(get(middle).getTime(),
                        refTime) ||
                    (TimeInstant.isEqual(((EventNote)get(middle)).getTime(),
                        refTime) &&
                        ((EventNote)get(middle)).getPriority() > refPrio))
                {
                    left = middle + 1;
                }
                else
                {
                    right = middle;
                }
            }
            if (right < size()
                    && TimeInstant.isEqual(((EventNote)get(right)).getTime(),
                            refTime)
                    && get(right).getPriority() == refPrio)
            {
                // same time/prio found
                firstIndexForInsert = right;
                // look for last event-note with same time/prio; last position to insert
                // is AFTER last concurrent note 
                lastIndexForInsert = findLastIndex(firstIndexForInsert) + 1;
            }
            else
            {
                // same time/prio not found, but right still holds the insert position
                firstIndexForInsert = right;
                lastIndexForInsert = firstIndexForInsert;
            }
            // do we need to generate a random insert position?
            if (firstIndexForInsert != lastIndexForInsert)
            {
                // yeah, so here we go
                firstIndexForInsert += _positionGenerator.Next(lastIndexForInsert - firstIndexForInsert + 1);
                // defer in case connection violated
                while (firstIndexForInsert < size() && ((EventNote)get(firstIndexForInsert)).isConnected()) firstIndexForInsert++;
            }
            // at last do the actual inserting
           add(firstIndexForInsert, newNote);
            newNote.setConnected(false);

        }

        /**
     * This helper method determines the position of the last event-note with
     * the same time/priority as the event-note at position firstIndex doing a simple
     * linear search from firstIndex.
     */
        private int findLastIndex(int firstIndex)
        {
            TimeInstant refTime = get(firstIndex).getTime();
            int refPrio = get(firstIndex).getPriority();
            int lastIndex = firstIndex + 1;
            while (lastIndex < size()
                    && TimeInstant.isEqual(refTime,
                            ((EventNote)get(lastIndex)).getTime())
                    && refPrio == get(lastIndex).getPriority())
            {
                lastIndex++;
            }
            return lastIndex - 1;
        }

        public T nextNote(T origin)
        {
            if (contains(origin))
            {
                if (origin == get(size() - 1))
                {
                    return null;
                }
                else
                    return get(indexOf(origin) + 1);
            }
            return null;
        }

        public T prevNote(T origin)
        {

            if (contains(origin))
            {
                if (origin == get(0))
                {
                    return null;
                }
                return get(indexOf(origin) - 1);
            }
            return null;
        }

        public void removeFirst()
        {
           remove(0); // no comment ;-)
        }

        public T getFirst()
        {
            return get(0); // return but not remove
        }

        public T getLast()
        {
            return  get(size() - 1);
        }
    }

    public class PriorityQueue<T> : ITreeList<T> where T : EventNote
    {
        // Level 1: Fibonacci min-heap for O(1) amortized insert, O(log n) dequeue-min
        private readonly IPriorityQueue<PriorityData> _heap;
        // Level 1b: sorted set for ordered traversal (nextNote, prevNote, getLast, iterator)
        private readonly SortedSet<PriorityData> _sortedKeys;
        // Level 1c: per-bucket doubly-linked lists
        private readonly Dictionary<PriorityData, LinkedList<T>> _buckets;
        // Level 2: O(1) node lookup by object identity
        private readonly Dictionary<T, (LinkedList<T> List, LinkedListNode<T> Node)> _nodeIndex;

        public PriorityQueue()
        {
            _heap = new FibonacciPriorityQueue<PriorityData>((x, y) => x.Value.CompareTo(y.Value));
            _sortedKeys = new SortedSet<PriorityData>();
            _buckets = new Dictionary<PriorityData, LinkedList<T>>();
            _nodeIndex = new Dictionary<T, (LinkedList<T>, LinkedListNode<T>)>(ReferenceEqualityComparer.Instance);
        }

        private static PriorityData getKey(T note) => new PriorityData
        {
            timeInEpsion = note.getTime().getTimeInEpsilon(),
            priority = note.getPriority()
        };

        private void registerInBucket(PriorityData key, T note, bool prepend)
        {
            if (!_buckets.TryGetValue(key, out var list))
            {
                list = new LinkedList<T>();
                _buckets[key] = list;
                _heap.Enqueue(key);
                _sortedKeys.Add(key);
            }
            var node = prepend ? list.AddFirst(note) : list.AddLast(note);
            _nodeIndex[note] = (list, node);
        }

        public void add(T newNote) => registerInBucket(getKey(newNote), newNote, false);

        public void addFirst(T newNote)
        {
            if (isEmpty())
            {
                add(newNote);
                return;
            }
            var firstKey = _sortedKeys.Min;
            var newKey = getKey(newNote);
            int cmp = newKey.CompareTo(firstKey);
            if (cmp < 0)
            {
                registerInBucket(newKey, newNote, false);
            }
            else
            {
                if (cmp > 0)
                {
                    var firstNote = _buckets[firstKey].First!.Value;
                    newNote.setTime(firstNote.getTime());
                    newNote.setPriority(firstNote.getPriority());
                }
                registerInBucket(firstKey, newNote, true);
            }
        }

        public bool contains(T origin) => _nodeIndex.ContainsKey(origin);

        public T getFirst() => _buckets[_heap.Peek()].First!.Value;

        public T getLast() => _buckets[_sortedKeys.Max].Last!.Value;

        public void insert(T newNote) => add(newNote);

        public void insertAfter(T where, T newNote)
        {
            if (!_nodeIndex.TryGetValue(where, out var entry))
                throwNotFound(newNote, "insertAfter");
            newNote.setTime(where.getTime());
            var newNode = entry.List.AddAfter(entry.Node, newNote);
            _nodeIndex[newNote] = (entry.List, newNode);
        }

        public void insertBefore(T where, T newNote)
        {
            if (!_nodeIndex.TryGetValue(where, out var entry))
                throwNotFound(newNote, "insertBefore");
            newNote.setTime(where.getTime());
            var newNode = entry.List.AddBefore(entry.Node, newNote);
            _nodeIndex[newNote] = (entry.List, newNode);
        }

        public bool isEmpty() => _heap.IsEmpty();

        public List<T> iterator()
        {
            var result = new List<T>(_nodeIndex.Count);
            foreach (var key in _sortedKeys)
                foreach (var note in _buckets[key])
                    result.Add(note);
            return result;
        }

        public T nextNote(T origin)
        {
            if (!_nodeIndex.TryGetValue(origin, out var entry)) return null;
            if (entry.Node.Next != null) return entry.Node.Next.Value;
            var key = getKey(origin);
            bool found = false;
            foreach (var k in _sortedKeys)
            {
                if (found) return _buckets[k].First!.Value;
                if (k.CompareTo(key) == 0) found = true;
            }
            return null;
        }

        public T prevNote(T origin)
        {
            if (!_nodeIndex.TryGetValue(origin, out var entry)) return null;
            if (entry.Node.Previous != null) return entry.Node.Previous.Value;
            var key = getKey(origin);
            PriorityData? prevKey = null;
            foreach (var k in _sortedKeys)
            {
                if (k.CompareTo(key) == 0) break;
                prevKey = k;
            }
            return prevKey.HasValue ? _buckets[prevKey.Value].Last!.Value : null;
        }

        public void randomizedInsert(T newNote, Random _positionGenerator)
        {
            var key = getKey(newNote);
            if (!_buckets.TryGetValue(key, out var list))
            {
                add(newNote);
                newNote.setConnected(false);
                return;
            }

            // Pick a random start position [0, count]; then advance past connected notes
            int startPos = _positionGenerator.Next(list.Count + 1);
            LinkedListNode<T>? cursor;
            if (startPos == list.Count)
            {
                cursor = null; // would insert at end
            }
            else
            {
                cursor = list.First;
                for (int i = 0; i < startPos; i++)
                    cursor = cursor!.Next;
            }
            // Advance while the cursor note is connected (can't insert before it)
            while (cursor != null && cursor.Value.isConnected())
                cursor = cursor.Next;

            LinkedListNode<T> newNode = cursor == null
                ? list.AddLast(newNote)
                : list.AddBefore(cursor, newNote);

            _nodeIndex[newNote] = (list, newNode);
            newNote.setConnected(false);
        }

        public void remove(T where)
        {
            if (!_nodeIndex.TryGetValue(where, out var entry)) return;
            entry.List.Remove(entry.Node);
            if (entry.List.Count == 0)
            {
                var key = getKey(where);
                _buckets.Remove(key);
                _heap.Remove(key);
                _sortedKeys.Remove(key);
            }
            _nodeIndex.Remove(where);
        }

        public void removeFirst()
        {
            var minKey = _heap.Peek();
            var list = _buckets[minKey];
            var node = list.First!;
            _nodeIndex.Remove(node.Value);
            list.RemoveFirst();
            if (list.Count == 0)
            {
                _buckets.Remove(minKey);
                _heap.Dequeue();
                _sortedKeys.Remove(minKey);
            }
        }

        public int size() => _nodeIndex.Count;

        private static void throwNotFound(T newNote, string method)
        {
            Model mBuffer = newNote.getEntity1()?.getModel() ?? newNote.getEvent()?.getModel();
            throw new SimAbortedException(new ErrorMessage(
                mBuffer,
                $"Can not insert new event-note {method} given EventNote! Simulation aborted",
                $"Internal DESMO-J class : PriorityQueue Method : {method}(EventNote where, EventNote newNote)",
                "The event-note to insert the new note after is not contained in the event tree list.",
                "This is a fatal error. Contact DESMOJ support",
                newNote.getTime()));
        }

        struct PriorityData : IComparable<PriorityData>, IEquatable<PriorityData>
        {
            public long timeInEpsion;
            public int priority;

            public int CompareTo(PriorityData other)
            {
                var reval = timeInEpsion.CompareTo(other.timeInEpsion);
                if (reval != 0) return reval;
                return -priority.CompareTo(other.priority);
            }

            public bool Equals(PriorityData other) =>
                timeInEpsion == other.timeInEpsion && priority == other.priority;

            public override bool Equals(object? obj) => obj is PriorityData pd && Equals(pd);

            public override int GetHashCode() => HashCode.Combine(timeInEpsion, priority);
        }
    }
    /// <summary>
    /// Array-backed binary min-heap implementation of ITreeList.
    /// O(log n) insert/remove, O(1) peek-min — cache-friendly, no pointer-chasing.
    /// Sequence numbers provide stable FIFO ordering for equal time+priority events.
    /// insertAfter/insertBefore and traversal (nextNote/prevNote/iterator) work correctly
    /// but are O(n log n) — acceptable since they are called only for reporting/chaining,
    /// not on the scheduler hot path.
    /// </summary>
    public sealed class BinaryHeapList<T> : ITreeList<T> where T : EventNote
    {
        private const int InitialCapacity = 1024;
        private const long NormalStep = 1L << 20;

        private T[] _heap;  // 1-based: root at [1], left child of i at [2i], right at [2i+1]
        private int  _count;
        private long _nextSeq;

        public BinaryHeapList()
        {
            _heap = new T[InitialCapacity + 1]; // slot 0 unused
        }

        // ── Ordering ──────────────────────────────────────────────────────────────

        private static int CompareNotes(T a, T b)
        {
            long ta = a.getTime().getTimeInEpsilon(), tb = b.getTime().getTimeInEpsilon();
            if (ta != tb) return ta.CompareTo(tb);
            int pa = a.getPriority(), pb = b.getPriority();
            if (pa != pb) return pb.CompareTo(pa); // higher int priority fires first
            return a.HeapSeq.CompareTo(b.HeapSeq);
        }

        private int CompareAt(int i, int j) => CompareNotes(_heap[i], _heap[j]);

        // ── Heap primitives ───────────────────────────────────────────────────────

        private void Swap(int i, int j)
        {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
            _heap[i].HeapIndex = i;
            _heap[j].HeapIndex = j;
        }

        private void SiftUp(int i)
        {
            while (i > 1 && CompareAt(i, i >> 1) < 0)
            {
                Swap(i, i >> 1);
                i >>= 1;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int smallest = i, l = i << 1, r = l | 1;
                if (l <= _count && CompareAt(l, smallest) < 0) smallest = l;
                if (r <= _count && CompareAt(r, smallest) < 0) smallest = r;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
        }

        private void Grow()
        {
            var bigger = new T[_heap.Length * 2];
            Array.Copy(_heap, bigger, _heap.Length);
            _heap = bigger;
        }

        private void HeapInsert(T note)
        {
            if (_count + 1 >= _heap.Length) Grow();
            _heap[++_count] = note;
            note.HeapIndex = _count;
            SiftUp(_count);
        }

        // ── ITreeList<T> ──────────────────────────────────────────────────────────

        public void add(T newNote)
        {
            newNote.HeapSeq = _nextSeq;
            _nextSeq += NormalStep;
            HeapInsert(newNote);
        }

        public void addFirst(T newNote)
        {
            if (isEmpty()) { add(newNote); return; }
            T first = _heap[1];
            if (TimeInstant.isEqual(newNote.getTime(), first.getTime()) &&
                newNote.getPriority() < first.getPriority())
                newNote.setPriority(first.getPriority());
            newNote.HeapSeq = first.HeapSeq - 1;
            HeapInsert(newNote);
        }

        public void insert(T newNote) => add(newNote);

        public void insertAfter(T where, T newNote)
        {
            if (where.HeapIndex == 0) ThrowNotFound(newNote, "insertAfter");
            newNote.setTime(where.getTime());
            long seq = where.HeapAfterNext == long.MinValue ? where.HeapSeq + 1 : where.HeapAfterNext;
            where.HeapAfterNext = seq + 1;
            newNote.HeapSeq = seq;
            HeapInsert(newNote);
        }

        public void insertBefore(T where, T newNote)
        {
            if (where.HeapIndex == 0) ThrowNotFound(newNote, "insertBefore");
            newNote.setTime(where.getTime());
            long seq = where.HeapBeforeNext == long.MinValue ? where.HeapSeq - 1 : where.HeapBeforeNext - 1;
            where.HeapBeforeNext = seq;
            newNote.HeapSeq = seq;
            HeapInsert(newNote);
        }

        public void remove(T where)
        {
            int i = where.HeapIndex;
            if (i == 0) return;
            where.HeapIndex = 0;
            if (i == _count) { _heap[_count--] = default; return; }
            T last = _heap[_count];
            _heap[i] = last;
            _heap[_count--] = default;
            last.HeapIndex = i;
            SiftUp(i);
            SiftDown(last.HeapIndex); // HeapIndex updated by SiftUp if last moved up
        }

        public void removeFirst()
        {
            if (_count == 0) return;
            _heap[1].HeapIndex = 0;
            if (_count == 1) { _heap[1] = default; _count = 0; return; }
            T last = _heap[_count];
            _heap[1] = last;
            _heap[_count--] = default;
            last.HeapIndex = 1;
            SiftDown(1);
        }

        public T getFirst() => _count > 0 ? _heap[1] : null;

        public T getLast()
        {
            if (_count == 0) return null;
            T max = _heap[1];
            for (int i = 2; i <= _count; i++)
                if (CompareNotes(_heap[i], max) > 0) max = _heap[i];
            return max;
        }

        public bool contains(T origin) => origin.HeapIndex > 0;
        public bool isEmpty()          => _count == 0;
        public int  size()             => _count;

        public List<T> iterator()
        {
            var list = new List<T>(_count);
            for (int i = 1; i <= _count; i++) list.Add(_heap[i]);
            list.Sort(CompareNotes);
            return list;
        }

        public T nextNote(T origin)
        {
            if (origin.HeapIndex == 0) return null;
            var sorted = iterator();
            int idx = sorted.IndexOf(origin);
            return idx >= 0 && idx + 1 < sorted.Count ? sorted[idx + 1] : null;
        }

        public T prevNote(T origin)
        {
            if (origin.HeapIndex == 0) return null;
            var sorted = iterator();
            int idx = sorted.IndexOf(origin);
            return idx > 0 ? sorted[idx - 1] : null;
        }

        public void randomizedInsert(T newNote, Random rng)
        {
            newNote.HeapSeq = _nextSeq + rng.NextInt64(0, NormalStep);
            _nextSeq += NormalStep;
            newNote.setConnected(false);
            HeapInsert(newNote);
        }

        private void ThrowNotFound(T note, string method)
        {
            Model m = note.getEntity1()?.getModel() ?? note.getEvent()?.getModel();
            throw new SimAbortedException(new ErrorMessage(m,
                $"Cannot insert event-note {method} given note — it is not in the heap.",
                $"BinaryHeapList.{method}", "Reference note not scheduled.",
                "Fatal internal error.", note.getTime()));
        }
    }

    public interface ITreeList<T> where T:EventNote
    {
        void add(T newNote);
        void addFirst(T newNote);
        void insert(T newNote);
        void insertAfter(T where, T newNote);
        void insertBefore(T where, T newNote);
        bool isEmpty();
        List<T> iterator();
        void remove(T where);
        int size();
        bool contains(T origin);
        void randomizedInsert(T newNote, Random _positionGenerator);
        T nextNote(T origin);

        T prevNote(T origin);

        void removeFirst();

        T getFirst();
        T getLast();
        
    }
}