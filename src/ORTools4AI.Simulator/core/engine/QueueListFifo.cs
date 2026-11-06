// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Justin Neumann
// @author based on ideas from Soenke Claassen, Tim Lechler, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * Contains the implementation with the java.util.LinkedList to represent queueing
     * functionality. The entities are queued first according to their priority and
     * second in FIFO (first in first out) order. The statistic data of the queue will be
     * stored in a <code>QueueBased</code> object. The <code>QueueListStandardFifo</code>
     * has a reference to its <code>QueueBased</code> object. This class needs a 
     * reference to a subclass of QueueBased to update the queue statistics.
     * It is used in many kinds of queue implementations i.e. in classes
     * <code>Queue</code> and <code>ProcessQueue</code>.
     *
     * @see QueueList
     * @see QueueBased
     * @see Queue
     * @see ProcessQueue
     */
	public class QueueListFifo<E> : QueueListStandard<E> where E : Entity

	{

		/// <summary>Initializes an empty first-in, first-out queue list.</summary>
		/// <remarks>Associate the list with its owning queue through <c>setQueueBased</c>.</remarks>
		public QueueListFifo()
		{
			//super();

			// set the abbreviation for this kind of queueing discipline
			this.abbreviation = "FIFO";

			// we don't know the client queue yet.
			// Must be provided later by calling the setQueueBased() method
			clientQ = null;

		}

		/// <summary>
		/// Adds a new Entity to the QueueListStandardFifo. Entities are inserted according to their priority in
		/// descending order. The highest priority Entity will always be first in the queue. Entities with same
		/// priority are inserted in FiFo order.
		/// </summary>
		/// <param name="e">The Entity to add to the QueueListStandardFifo</param>
		public override void insert(E e)
		{

			if (e == null)
			{ // check for null reference
				sendWarning(
						"Can not insert entity. Command ignored.",
						"Class: QueueListStandardFifo Method: insert(Entity e).",
						"The Entity reference given as parameter is a null reference.",
						"Be sure to only use valid references.");
				return;
			}

			if (contains(e))
			{ // entity must not be contained twice in queue
				sendWarning("Can not insert entity. Command ignored.",
						"Class: QueueListStandardFifo Method: insert(Entity e).",
						"The Entity given as parameter is already enqueued.",
						"Make sure the entity is not enqueued here by calling "
								+ "method 'contains(Entity e)'.");
				return;
			}

			// if there are already entities in queue
			if (!this.isEmpty())
			{
				// continuously asks the predecessor (FIFO) entities if they have a lower priority;
				// finally inserts at correct position

				E swap = last(); // swap here references the last element (FIFO)
				while (Entity.isSmaller(swap, e)) // FIFO
				{
					swap = pred(swap); // swap reference to successor of itself
				}

				if (swap == null)
				{
					queuelist.AddFirst(e);
					statisticalInsert(e); // update statistics
					e.addQueueBased(this.clientQ); // sets entity's queue as this queued
				}
				else
				{
					this.insertAfter(e, swap); //inserts the entity at the correct position
											   // with help of the java.util.LinkedList
				}

			}
			else
			{
				queuelist.AddLast(e);
				e.addQueueBased(this.clientQ); // sets entity's queue as this queued

				statisticalInsert(e); // update statistics
			}
		}

    }
}