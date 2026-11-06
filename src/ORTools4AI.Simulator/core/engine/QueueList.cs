// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based upon ideas from Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using System.Collections;

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * Is the abstract superclass for all the classes implementing different
     * queueing strategies for a waiting-queue. It provides all the basic methods
     * for inserting objects in a queue, retrieving objects from a queue and getting
     * basic informations about the queue. It is used in many kinds of queue
     * implementations where collective functionalities are implemented by
     * <code>QueueListStandard</code> and are specified e.g. in <code>QueueListFifo</code>
     * or <code>QueueListLifo</code>.
     * 
     * @see QueueBased
     * @see Queue
     * @see ProcessQueue
     * @see QueueListFifo
     * @see QueueListLifo
     */
    public abstract class QueueList<E> : PropertyChangeListener, IEnumerable<E>
	where E : Entity
    {

	/**
	 * The QueueBased object this queuelist serves as a container for.
	 */
	protected QueueBased clientQ;
	
	/**
	 * Uses the java.util.WeakHashMap functionalities to link entities with their entry time.
	 */
	protected Dictionary<E,TimeInstant> timemap;

	/// <summary>
	/// Should return <c>true</c> if the given <c>Entity</c> is contained in the queue; <c>false</c> otherwise.
	/// </summary>
	/// <param name="e">The <c>Entity</c> we are looking for in the queue.</param>
	/// <returns>
	/// Should be <c>true</c> if the given <c>Entity</c> is contained in the queue; <c>false</c> otherwise.
	/// </returns>
	public abstract bool contains(E e);

	/// <summary>Should return the first entity in the queue.</summary>
	/// <returns>The first entity in the queue</returns>
	public abstract E first();
	
	/// <summary>
	/// Returns the <c>Entity</c> queued at the named position. The first position is 0, the last one size()-1.
	/// </summary>
	/// <returns>
	/// The <c>Entity</c> at the position of <c>int</c> or <c>null</c> if no such position exists.
	/// </returns>
	public abstract E get(int index);

	/// <summary>
	/// Returns the position of the named <c>Entity</c>. The first position is 0, the last one size()-1.
	/// </summary>
	/// <returns>: The position of the <c>Entity</c> or <c>-1</c> if no such exists.</returns>
	public abstract int get(E element);

	/// <summary>
	/// Should return an abbreviation as a String to identify the sort of queueing discipline (like FIFO or LIFO
	/// or ...). Is used to display the queueing discipline in the report of the QueueBased objects.
	/// </summary>
	/// <returns>An abbreviation to identify the sort of queueing discipline (like FIFO or LIFO or ...)</returns>
	public abstract string getAbbreviation();

	/// <summary>
	/// Returns the <c>QueueBased</c> object this <c>QueueList</c> serves as a queue implementation for.
	/// </summary>
	/// <returns>The <c>QueueBased</c> object this <c>QueueList</c> serves as a container for.</returns>
	protected QueueBased getQueueBased() {
		return clientQ;
	}

	/// <summary>Should add a new Entity to the queue.</summary>
	/// <param name="e">The Entity which will be added to the queue</param>
	public abstract void insert(E e);
	
	/// <summary>
	/// Should insert the <c>Entity</c> "e" right after the position of <c>Entity</c> "which" in the queue.
	/// Should return <c>true</c> if this was done successfully. Be careful with this operation. It might
	/// disrupt your order of priorities.
	/// </summary>
	/// <param name="e">The <c>Entity</c> which will be inserted.</param>
	/// <param name="which">
	/// The <c>Entity</c> determining the position after which the <c>Entity</c> "e" will be inserted in the
	/// queue.
	/// </param>
	/// <returns>Is <c>true</c> if insertion was successfull, <c>false</c> otherwise.</returns>
	public abstract bool insertAfter(E e, E which);

	/// <summary>
	/// Should insert the <c>Entity</c> "e" right before the position of <c>Entity</c> "which" in the queue.
	/// Should return <c>true</c> if this was done successfully. Be careful with this operation. It might
	/// disrupt your order of priorities.
	/// </summary>
	/// <param name="e">The <c>Entity</c> which will be inserted.</param>
	/// <param name="which">
	/// The <c>Entity</c> determining the position before which the <c>Entity</c> "e" will be inserted in the
	/// queue.
	/// </param>
	/// <returns>Is <c>true</c> if insertion was successfull, <c>false</c> otherwise.</returns>
	public abstract bool insertBefore(E e, E which);

	/// <summary>
	/// Should return <c>true</c> if no entities are stored in the queue at the moment, <c>false</c> otherwise.
	/// </summary>
	/// <returns>
	/// Is <c>true</c> if no entities are stored in the queue at the moment, <c>false</c> otherwise.
	/// </returns>
	public abstract bool isEmpty();

	/// <summary>Should return the last <c>Entity</c> in the queue.</summary>
	/// <returns>The last <c>Entity</c> in the queue.</returns>
	public abstract E last();

	/// <summary>Should return the predecessor of the given <c>Entity</c> "e" in the queue.</summary>
	/// <param name="e">The predecessor of this <c>Entity</c> will be returned.</param>
	/// <returns>The <c>Entity</c> before the given <c>Entity</c> "e" in the queue.</returns>
	public abstract E pred(E e);

	/// <summary>
	/// Should remove the given <c>Entity</c> "e" from the queue. If this is done successfully <c>true</c> is
	/// returned, <c>false</c> otherwise.
	/// </summary>
	/// <param name="e">The <c>Entity</c> which is to be removed from the queue.</param>
	/// <returns>
	/// Is <c>true</c> if the given <c>Entity</c> is removed successfully, <c>false</c> otherwise.
	/// </returns>
	public abstract bool remove(E e);
	
	/// <summary>
	/// Removes the <c>Entity</c> queued at the named position. The first position is 0, the last one size()-1.
	/// </summary>
	/// <returns>
	/// : The method returns <c>true</c> as the <c>Entity</c> exists or <c>false</c> if otherwise.
	/// </returns>
	public abstract bool remove(int index);

 //       /**
 //        * Should remove the first <code>Entity</code> in the queue and returns
 //        * <code>true</code> if done successfully, <code>false</code> otherwise.
 //        * 
 //        * @return boolean : Is <code>true</code> if the first <code>Entity</code>
 //        *         in the queue is removed successfully, <code>false</code>
 //        *         otherwise.
 //        */
    public abstract bool removeFirst();

	/// <summary>
	/// Should remove the last <c>Entity</c> in the queue and returns <c>true</c> if done successfully,
	/// <c>false</c> otherwise.
	/// </summary>
	/// <returns>
	/// Is <c>true</c> if the last <c>Entity</c> in the queue is removed successfully, <c>false</c> otherwise.
	/// </returns>
	public abstract bool removeLast();

	/// <summary>
	/// Should send a warning to the error output by forwarding it to the associated <c>QueueBased's
	/// sendWarning</c> method. Warnings should only be sent if the <c>QueueBased</c>'s flag for queue
	/// implementation warnings is set to <c>true</c>.
	/// </summary>
	/// <param name="description">describing the error</param>
	/// <param name="location">describing the location the error occurred</param>
	/// <param name="reason">describing the possible cause for this error</param>
	/// <param name="prevention">telling what to do to prevent this error</param>
	/// <remarks>See also QueueBased</remarks>
	public abstract void sendWarning(string description, string location,
			string reason, string prevention);

		/// <summary>
		/// Sets the client queue for which the entities are stored. Is needed, because this can not be done in the
		/// no-arg constructor.
		/// </summary>
		/// <param name="queueBase">The QueueBased using this queueing system.</param>
		public void setQueueBased(QueueBased queueBase)
		{

			// DOA if no real QueueBased object is given
			if (queueBase == null)
			{
				throw (new SimAbortedException(new ErrorMessage(null,
						"Can not create QueueListStandardFifo! Simulation aborted.",
						"Class : QueueListStandardFifo / Method : setClientQueue"
								+ "(QueueBased queueBase) ",
						"The reference to the QueueBased object needed is a null "
								+ "reference.",
						"Always check to give valid references only.", null)));
			}

			clientQ = queueBase; // set reference to my client using me as a
								 // container

		}

		/// <summary>Returns the actual size of the QueueList.</summary>
		/// <returns>
		/// : The method returns the size as an <c>int</c>. The value is 0 if no Entity is in line.
		/// </returns>
		public int size()
		{
			return clientQ.length();
		}

		/**
		 * This method is used for statistical data in the class QueueBased.
		 * 
		 */
		protected void statisticalInsert(E e)
		{

			timemap[e] = clientQ.presentTime(); // saves time of insertion

			clientQ.addItem(); // update statistics
		}

		/**
		 * This method is used for statistical data in the class QueueBased.
		 * 
		 */
		protected void statisticalRemove(E e)
		{
			if (timemap.TryGetValue(e, out TimeInstant entryTime))
			{
				clientQ.deleteItem(entryTime); // tell QueueBased the entry time for update statistics
				timemap.Remove(e); // removes the entity from timemap
			}
		}

		/// <summary>Should return the successor of the given <c>Entity</c> "e" in the queue.</summary>
		/// <param name="e">The successor of this <c>Entity</c> will be returned.</param>
		/// <returns>The <c>Entity</c> after the given <c>Entity</c> "e" in the queue.</returns>
		public abstract E succ(E e);

		/// <summary>Should return a string representation of the queue.</summary>
		/// <returns>The string representation of the queue.</returns>
		public abstract string toString();
	
	
        public abstract IEnumerator<E> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}