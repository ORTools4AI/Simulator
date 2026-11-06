// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using System.Collections;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * ProcessQueue provides models with a ready-to-use element to enqueue
     * <code>SimProcess/Entity</code>es in. The sort order of the ProcessQueue is
     * determined first by the priorities of the enqueued SimProcesses and second by
     * the given sort order. The default sort order is FIFO (first in, first out)
     * but others like LIFO (last in, first out) can be chosen, too. See the
     * constants in class <code>QueueBased</code> and the derived classes from
     * <code>QueueList</code>. The capacity of the ProcessQueue, that is the
     * maximum number of SimProcesses enqueued, can be chosen, too. Note that in
     * contrast to the 'plain' queue, this ProcessQueue always expects and returns
     * objects that are derived from class <code>SimProcess/Entity</code>. When
     * modelling using the process-, activity-, or transaction-oriented paradigm
     * where SimProcesses are used to represent the model's entities, this
     * ProcessQueue can be used instead of the standard Queue to reduce the amount
     * of casts needed otherwise.
     * 
     * @see QueueBased
     * @see QueueList
     * @see QueueListFifo
     * @see QueueListLifo
     */
	public class ProcessQueue<P> : QueueBased,IEnumerable<P> where P:Entity
	{

		/**
		 * The queue implementation that actually stores the entities
		 */
		private QueueList<P> _ql;

		/**
		 * Counter for the SimProcesses which are refused to be enqueued, because
		 * the queue capacity is full.
		 */
		private long _refused;

		/// <summary>
		/// Constructs a simple priority based waiting-queue for SimProcesses, the kind of queue implementation
		/// (FIFO or LIFO) and the capacity of the queue can be chosen. The usage of the generic version
		/// ProcessQueue&lt;Type&gt; where <c>Type</c> is derived from <c>SimProcess</c> is recommended for type
		/// safety. Using the raw type <c>ProcessQueue</c> yields a queue in which any <c>SimProcess/Entity</c> can
		/// be enqueued, potentially requiring type casting on accessing processes enqueued.
		/// </summary>
		/// <param name="owner">The model this ProcessQueue is associated to</param>
		/// <param name="name">The process-queue's name</param>
		/// <param name="sortOrder">
		/// determines the sort order of the underlying queue implementation. Choose a constant from
		/// <c>QueueBased</c>: <c>QueueBased.FIFO</c>, <c>QueueBased.LIFO</c> or QueueBased.Random.
		/// </param>
		/// <param name="qCapacity">
		/// The capacity of the Queue, that is how many processes can be enqueued. Zero (0) can be used as shortcut
		/// for for a capacity of <c>Integer.MAX_VALUE</c> = 2,147,483,647, which should approximate an infinite
		/// queue sufficiently well for most purposes.
		/// </param>
		/// <param name="showInReport">Flag if process-queue should produce a report</param>
		/// <param name="showInTrace">Flag for process-queue to produce trace messages</param>
		public ProcessQueue(Model owner, string name, int sortOrder, int qCapacity,
				bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{

			//super(owner, name, showInReport, showInTrace); // create the QBased
			// object
			reset();

			// determine the queueing strategy
			switch (sortOrder)
			{
				case FIFO: 
					_ql = new QueueListFifo<P>(); break;
				case QueueBased.LIFO:
					_ql = new QueueListLifo<P>(); break;
				case QueueBased.RANDOM:
					_ql = new QueueListRandom<P>(); break;
				default:
					sendWarning(
							"The given sortOrder parameter " + sortOrder + " is not valid! "
									+ "A queue with Fifo sort order will be created.",
							"ProcessQueueQueue : "
									+ getName()
									+ " Constructor: ProcessQueue(Model owner, String name, "
									+ "int sortOrder, long qCapacity, boolean showInReport, "
									+ "boolean showInTrace)",
							"A valid positive integer number must be provided to "
									+ "determine the sort order of the queue.",
							"Make sure to provide a valid positive integer number "
									+ "by using the constants in the class QueueBased, like "
									+ "QueueBased.FIFO, QueueBased.LIFO or QueueBased.RANDOM.");
					_ql = new QueueListFifo<P>();
					break;
			}

			// give the QueueList a reference to this QueueBased
			_ql.setQueueBased(this);

			// set the capacity of the queue
			queueLimit = qCapacity;

			// check if it the capacity does make sense
			if (qCapacity < 0)
			{
				sendWarning(
						"The given capacity of the queue is negative! "
								+ "A queue with maximum capacity (2,147,483,647) will be created instead.",
						"ProcessQueue : "
								+ getName()
								+ " Constructor: ProcessQueue(Model owner, String name, "
								+ "int sortOrder, long qCapacity, boolean showInReport, "
								+ "boolean showInTrace)",
						"A negative capacity for a queue does not make sense.",
						"Make sure to provide a valid positive capacity "
								+ "for the queue.");
				// set the capacity to the maximum value
				queueLimit = int.MaxValue;
			}

			// check if qCapacity is zero (that means unlimited capacity)
			if (qCapacity == 0)
			{
				// set the capacity to the maximum value
				queueLimit = int.MaxValue;
			}

		}

		/// <summary>
		/// Constructs a simple priority and FIFO based waiting-queue for SimProcesses with a maximum capacity of
		/// 2,147,483,647 waiting processes, which should serve as an approximation of infinite queues sufficiently
		/// well for most purposes. The usage of the generic version ProcessQueue&lt;Type&gt; where <c>Type</c> is
		/// derived from <c>SimProcess/Entity</c> is recommended for type safety. Using the raw type
		/// <c>ProcessQueue</c> yields a queue in which any <c>SimProcess/Entity</c> can be enqueued, potentially
		/// requiring type casting on accessing processes enqueued.
		/// </summary>
		/// <param name="owner">The model this process-queue is associated to</param>
		/// <param name="name">The process-queue's name</param>
		/// <param name="showInReport">Flag if process-queue should produce a report</param>
		/// <param name="showInTrace">Flag for process-queue to produce trace messages</param>
		public ProcessQueue(Model owner, string name, bool showInReport,
				bool showInTrace):base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace); // create the QBased
														   // object
			reset();

			// make the queue with Fifo queueing discipline and unlimited capacity
			_ql = new QueueListFifo<P>();
			_ql.setQueueBased(this);

		}

		/// <summary>Returns a process-queue-reporter to produce a report about this process-queue.</summary>
		/// <returns>The reporter for this process-queue</returns>
		public Reporter createReporter()
		{

			return new ProcessQueueReporter(this);

		}

		/// <summary>
		/// Returns the first SimProcess/Entity queued in this process-queue or <c>null</c> in case the queue is
		/// empty.
		/// </summary>
		/// <returns>
		/// The first SimProcess/Entity in the process-queue or <c>null</c> if the process-queue is empty
		/// </returns>
		public P first()
		{

			return _ql.first(); // straight design

		}

		/// <summary>
		/// Returns the first SimProcess/Entity queued in this process-queue that applies to the given condition.
		/// The process-queue is searched from front to end and the first SimProcess/Entity that returns
		/// <c>true</c> when the condition is applied to it is returned by this method. If no SimProcess/Entity
		/// applies to the given condition or the process-queue is empty, <c>null</c> will be returned.
		/// </summary>
		/// <param name="c">The condition that the SimProcess/Entity returned must confirm</param>
		/// <returns>
		/// The first process queued in this process-queue applying to the given condition or <c>null</c>
		/// </returns>
		public P first(Condition<P> c)
		{

			if (c == null)
			{
				sendWarning(
						"Can not return first SimProcess/Entity fulfilling the condition!",
						"ProcessQueue : " + getName()
								+ " Method: void first(Condition c)",
						"The Condition 'c' given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}
			if (_ql.isEmpty())
				return null; // nobody home to be checked
			for (P tmp = _ql.first(); tmp != null; tmp = _ql.succ(tmp))
			{
				if (c.check(tmp))
					return tmp;
			}

			// if no SimProcess/Entity fulfills the condition just return null
			return null;

		}

		/// <summary>
		/// Returns <c>true</c> if the given <c>SimProcess/Entity</c> is contained in the queue; <c>false</c>
		/// otherwise.
		/// </summary>
		/// <param name="p">The process to find in the queue.</param>
		/// <returns>
		/// <c>True</c> if the given <c>SimProcess/Entity</c> is contained in the queue; <c>false</c> otherwise.
		/// </returns>
		public bool contains(P p)
		{

			return _ql.contains(p); // delegated to the internal queue list
		}

		/// <summary>Returns the current length of the Queue.</summary>
		/// <returns>The number of processes enqueued.</returns>
		public int size()
		{
			return _ql.size();
		}

		///**
		// * Returns the queue index of a given <code>SimProcess/Entity</code>.
		// * 
		// * @return int :The position of the process as an <code>int</code>. 
		// * 				Returns -1 if no such position exists.
		// */
		//public int get(P p)
		//{

		//	return _ql.get(p);

		//}

		///**
		// * Returns the <code>SimProcess/Entity</code> queued at the named position.
		// * The first position is 0, the last one size()-1.
		// * 
		// * @return P :The <code>SimProcess/Entity</code> at the position of
		// *         <code>int</code> or <code>null</code> if no such position exists.
		// */
		//public P get(int index)
		//{
		//	return _ql.get(index);
		//}

		/// <summary>
		/// Returns the underlying queue implementation, providing access to the QueueList implementation, e.g. to
		/// add PropertyChangeListeners.
		/// </summary>
		/// <returns>The underlying queue implementation of this ProcessQueue.</returns>
		public QueueList<P> getQueueList()
		{

			return _ql; // that's all
		}

		/// <summary>
		/// Returns the implemented queueing discipline of the underlying queue as a String, so it can be displayed
		/// in the report.
		/// </summary>
		/// <returns>The String indicating the queueing discipline.</returns>
		public string getQueueStrategy()
		{

			return _ql.getAbbreviation(); // that's it

		}

		/// <summary>
		/// Returns the number of entities refused to be enqueued in the queue, because the capacity limit is
		/// reached.
		/// </summary>
		/// <returns>The number of entities refused to be enqueued in the queue.</returns>
		public long getRefused()
		{

			return _refused; // that's it
		}

		/// <summary>
		/// Enters a new SimProcess/Entity into the ProcessQueue. If the capacity of the ProcessQueue is full, the
		/// entity will not be enqueued and <c>false</c> will be returned. The SimProcess/Entity will be stored in
		/// the ProcessQueue until method <c>remove(SimProcess/Entity p)</c> is called with this specific
		/// SimProcess/Entity. Simprocesses are ordered according to their priority. Higher priorities are sorted
		/// in front of lower priorities. Simprocesses with same priority are orderer according to the strategy
		/// specified in the constructor. The first SimProcess/Entity inside the process-queue will always be the
		/// one with the highest priority.
		/// </summary>
		/// <param name="p">The SimProcess/Entity to be added to the ProcessQueue</param>
		/// <returns>
		/// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is reached).
		/// </returns>
		public bool insert(P p)
		{

			if (p == null)
			{ // null returns with warning
				sendWarning("Can not insert SimProcess/Entity!", "ProcessQueue : "
						+ getName() + " Method: boolean insert" + "(P p)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when enqueueing "
								+ "Entities");
				return false; // no proper parameter
			}

			if (!isModelCompatible(p))
			{
				sendWarning("Can not insert SimProcess/Entity!", "ProcessQueue : "
						+ getName() + " Method: boolean insert" + "(P p)",
						"The SimProcess/Entity 'p' (" + p.getQuotedName() + ") given as parameter is not compatible to "
								+ "the model this process-queue belongs to!",
						"Check if your submodels are allowed to mingle with other "
								+ "model's components.");
				return false; // not of my model type!!!
			}

			if (queueLimit <= length())
			{

				if (currentlySendDebugNotes())
				{
					sendDebugNote("refuses to insert " + p.getQuotedName()
						+ " because the "
						+ "capacity limit is reached. ProcessQueue:<br>"
						+ _ql.toString());
				}

				if (currentlySendTraceNotes())
				{
					sendTraceNote("is refused to be enqueued in "
						+ this.getQuotedName() + "because the capacity limit ("
						+ getQueueLimit() + ") of this "
						+ "ProcessQueue is reached");
				}

				_refused++; // count the refused ones

				return false; // capacity limit is reached
			}

			_ql.insert(p); // that's it

			if (currentlySendDebugNotes())
			{
				sendDebugNote("inserts " + p.getQuotedName()
						+ " in the ProcessQueue:<br>" + _ql.toString());
			}

			// produce trace output
			if (currentlySendTraceNotes())
			{
				if (p == currentEntity() && currentEntityAll().Count == 1)
				{
					sendTraceNote("inserts itself into " + this.getQuotedName());
				}
				else
				{
					sendTraceNote("inserts " + p.getName() + " into "
							+ this.getQuotedName());
				}
			}

			return true;
		}

		/// <summary>
		/// Enters a new SimProcess/Entity into the process-queue and places it after the given SimProcess/Entity.
		/// If the capacity of the ProcessQueue is full, the entity will not be enqueued and <c>false</c> will be
		/// returned. Make sure that the SimProcess/Entity given as reference is already queued inside the
		/// process-queue, else the SimProcess/Entity will not be enqueued and <c>false</c> will be returned. The
		/// SimProcess/Entity will be stored in the ProcessQueue until method <c>remove(SimProcess/Entity p)</c> is
		/// called with this specific SimProcess/Entity.
		/// </summary>
		/// <param name="p">The SimProcess/Entity to be added to the process-queue</param>
		/// <param name="after">The SimProcess/Entity after which SimProcess/Entity 'p' is to be inserted</param>
		/// <returns>
		/// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is reached).
		/// </returns>
		public bool insertAfter(P p, P after)
		{

			if (p == null)
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertAfter(P p, P after)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when enqueueing Entities");
				return false; // no proper parameter
			}

			if (after == null)
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertAfter(P e, P after)",
						"The SimProcess/Entity 'after' given as parameter is a null reference!",
						"Check to always have valid references when enqueueing Entities");
				return false; // no proper parameter
			}

			if (!isModelCompatible(p))
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertAfter(P p, P after)",
						"The SimProcess/Entity 'p' given as parameter is not compatible to "
								+ "the model this process-queue belongs to!",
						"Check if your submodels are allowed to mingle with other "
								+ "model's components.");
				return false; // not of my model type!!!
			}

			if (queueLimit <= length())
			{

				if (currentlySendDebugNotes())
				{
					sendDebugNote("refuses to insert " + p.getQuotedName()
						+ " because the "
						+ "capacity limit is reached. ProcessQueue:<br>"
						+ _ql.toString());
				}

				if (currentlySendTraceNotes())
				{
					sendTraceNote("is refused to be enqueued in "
						+ this.getQuotedName() + "because the capacity limit ("
						+ getQueueLimit() + ") of this "
						+ "ProcessQueue is reached");
				}

				_refused++; // count the refused ones

				return false; // capacity limit is reached
			}

			bool successful = _ql.insertAfter(p, after); // elegantly done...

			if (currentlySendDebugNotes())
			{
				sendDebugNote("inserts " + p.getQuotedName() + " after "
						+ after.getQuotedName() + " in the ProcessQueue:<br>"
						+ _ql.toString());
			}

			// produce trace output
			if (currentlySendTraceNotes())
			{
				if (p == currentEntity() && currentEntityAll().Count == 1)
				{
					sendTraceNote("inserts itself into " + this.getQuotedName()
							+ " after " + after.getName());
				}
				else
				{
					sendTraceNote("inserts " + p.getName() + " into "
							+ this.getQuotedName() + " after " + after.getName());
				}
			}

			return successful;

		}

		/// <summary>
		/// Enters a new SimProcess/Entity into the ProcessQueue and places it in front of the given
		/// SimProcess/Entity. If the capacity of the ProcessQueue is full, the Entity will not be enqueued and
		/// <c>false</c> will be returned. Make sure that the SimProcess/Entity given as reference is already
		/// queued inside the ProcessQueue, else the SimProcess/Entity will not be enqueued and <c>false</c> will
		/// be returned. The SimProcess/Entity will be stored in the ProcessQueue until method
		/// <c>remove(SimProcess/Entity p)</c> is called with this specific SimProcess/Entity.
		/// </summary>
		/// <param name="p">The SimProcess/Entity to be added to the processqQueue</param>
		/// <param name="before">
		/// The SimProcess/Entity before which the SimProcess/Entity 'p' is to be inserted
		/// </param>
		/// <returns>
		/// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is reached).
		/// </returns>
		public bool insertBefore(P p, P before)
		{

			if (p == null)
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertBefore(P p, P before)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when enqueueing Entities");
				return false; // no proper parameter
			}

			if (before == null)
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertBefore(P p, P before)",
						"The SimProcess/Entity 'before' given as parameter is a null reference!",
						"Check to always have valid references when enqueueing Entities");
				return false; // no proper parameter
			}

			if (!isModelCompatible(p))
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : "
								+ getName()
								+ " Method: boolean insertBefore(P p, P before)",
						"The SimProcess/Entity 'p' given as parameter is not compatible to "
								+ "the model this process-queue belongs to!",
						"Check if your submodels are allowed to mingle with other "
								+ "model's components.");
				return false; // not of my model type!!!
			}

			if (queueLimit <= length())
			{

				if (currentlySendDebugNotes())
				{
					sendDebugNote("refuses to insert " + p.getQuotedName()
						+ " because the "
						+ "capacity limit is reached. ProcessQueue:<br>"
						+ _ql.toString());
				}

				if (currentlySendTraceNotes())
				{
					sendTraceNote("is refused to be enqueued in "
						+ this.getQuotedName() + "because the capacity limit ("
						+ getQueueLimit() + ") of this "
						+ "ProcessQueue is reached");
				}

				_refused++; // count the refused ones

				return false; // capacity limit is reached
			}

			bool successful = _ql.insertBefore(p, before); // elegantly done...

			if (currentlySendDebugNotes())
			{
				sendDebugNote("inserts " + p.getQuotedName() + " before "
						+ before.getQuotedName() + " in the ProcessQueue:<br>"
						+ _ql.toString());
			}

			// produce trace output
			if (currentlySendTraceNotes())
			{
				if (p == currentEntity() && currentEntityAll().Count == 1)
				{
					sendTraceNote("inserts itself into " + this.getQuotedName()
							+ " before " + before.getName());
				}
				else
				{
					sendTraceNote("inserts " + p.getName() + " into "
							+ this.getQuotedName() + " before " + before.getName());
				}
			}

			return successful;
		}

		/// <summary>
		/// Returns a boolean value indicating if the process-queue is empty or if any number of SimProcess/Entity
		/// is currently enqueued in it.
		/// </summary>
		/// <returns>Is <c>true</c> if the process-queue is empty, <c>false</c> otherwise</returns>
		public bool isEmpty()
		{

			return _ql.isEmpty();

		}

		/// <summary>
		/// Returns the last SimProcess/Entity queued in this process-queue or <c>null</c> in case the
		/// process-queue is empty.
		/// </summary>
		/// <returns>
		/// The last SimProcess/Entity in the process-queue or <c>null</c> if the process-queue is empty
		/// </returns>
		public P last()
		{

			return _ql.last(); // straight design again

		}

		/// <summary>
		/// Returns the last SimProcess/Entity queued in this process-queue that applies to the given condition.
		/// The process-queue is searched from end to front and the first SimProcess/Entity that returns
		/// <c>true</c> when the condition is applied to it is returned by this method. If no SimProcess/Entity
		/// applies to the given condition or the process-queue is empty, <c>null</c> will be returned.
		/// </summary>
		/// <param name="c">The condition that the SimProcess/Entity returned must fulfill</param>
		/// <returns>
		/// The last SimProcess/Entity queued in this process-queue applying to the given condition or <c>null</c>
		/// </returns>
		public P last(Condition<P> c)
		{

			if (c == null)
			{
				sendWarning(
						"Can not insert SimProcess/Entity!",
						"ProcessQueue : " + getName()
								+ " Method: SimProcess/Entity last(Condition c)",
						"The Condition -c- given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}

			if (_ql.isEmpty())
				return null; // nobody home to be checked

			for (P tmp = _ql.last(); tmp != null; tmp = _ql.pred(tmp))
			{
				if (c.check(tmp))
					return tmp;
			}

			// if no SimProcess/Entity fulfills the condition just return null
			return null;

		}

		/// <summary>
		/// Returns the SimProcess/Entity enqueued directly before the given SimProcess/Entity in the
		/// process-queue. If the given SimProcess/Entity is not contained in this process-queue or is at the first
		/// position thus having no possible predecessor, <c>null</c> is returned.
		/// </summary>
		/// <param name="p">An SimProcess/Entity in the process-queue</param>
		/// <returns>
		/// The SimProcess/Entity directly before the given SimProcess/Entity in the process-queue or <c>null</c>.
		/// </returns>
		public P pred(P p)
		{

			if (p == null)
			{
				sendWarning(
						"Can not find predecessor of SimProcess/Entity in Queue!",
						"ProcessQueue : " + getName()
								+ " Method: SimProcess/Entity pred(P p)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when querying for Entities");
				return null; // no proper parameter
			}

			return _ql.pred(p);

		}

		/// <summary>
		/// Returns the SimProcess/Entity enqueued before the given SimProcess/Entity in the process-queue that
		/// also fulfills the condition given. If the given SimProcess/Entity is not contained in this
		/// process-queue or is at the first position thus having no possible predecessor, <c>null</c> is returned.
		/// If no other SimProcess/Entity before the given one fulfills the condition, <c>null</c> is returned,
		/// too.
		/// </summary>
		/// <param name="p">A SimProcess/Entity in the process-queue</param>
		/// <param name="c">The condition that the preceeding SimProcess/Entity has to fulfill</param>
		/// <returns>
		/// The SimProcess/Entity before the given SimProcess/Entity in the process-queue fulfilling to the
		/// condition or <c>null</c>.
		/// </returns>
		public P pred(P p, Condition<P> c)
		{

			if (p == null)
			{
				sendWarning(
						"Can not find predecessor of SimProcess/Entity in Queue!",
						"ProcessQueue : "
								+ getName()
								+ " Method: SimProcess/Entity pred(P p, Condition c)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when querying for Entities");
				return null; // no proper parameter
			}

			if (c == null)
			{
				sendWarning(
						"Can not return previous SimProcess/Entity fulfilling condition!",
						"ProcessQueue : "
								+ getName()
								+ " Method: SimProcess/Entity pred(P p, Condition c)",
						"The Condition 'c' given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}

			for (P tmp = pred(p); tmp != null; tmp = pred(tmp))
			{
				if (c.check(tmp))
					return tmp;
			}

			return null; // obviously not found here, empty or doesn't fulfill

		}

		/// <summary>
		/// Removes the given SimProcess/Entity from the process-queue. If the given SimProcess/Entity is not in
		/// the process-queue, a warning will be issued but nothing else will be changed.
		/// </summary>
		/// <param name="p">The SimProcess/Entity to be removed from the process-queue</param>
		public void remove(P p)
		{

			if (p == null)
			{
				sendWarning(
						"Can not remove SimProcess/Entity from Queue!",
						"ProcessQueue : " + getName()
								+ " Method:  void remove(SimProcess/Entity p)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when removing "
								+ "Entities");
				return; // no proper parameter
			}

			if (!_ql.remove(p))
			{ // watch out, removes SimProcess/Entity as a side
			  // effect!!!
				sendWarning("Can not remove SimProcess/Entity from Queue!",
						"ProcessQueue : " + getName()
								+ " Method:  void remove(SimProcess/Entity p)",
						"The SimProcess/Entity 'p' (" + p.getQuotedName() + ") given as parameter is not enqueued in "
								+ "this queue!",
						"Make sure the SimProcess/Entity is inside the queue you want it "
								+ "to be removed.");
				return; // not enqueued here
			}
			else // done
			{

			}

			if (currentlySendDebugNotes())
			{
				sendDebugNote("remove " + p.getQuotedName() + "<br>"
						+ _ql.toString());
			}

			// produce trace output
			if (currentlySendTraceNotes())
			{
				if (p == currentEntity() && currentEntityAll().Count == 1)
				{
					sendTraceNote("removes itself from " + this.getQuotedName());
				}
				else
				{
					sendTraceNote("removes " + p.getQuotedName() + " from "
							+ this.getQuotedName());
				}
			}

		}

		/**
		 * Removes all processes from the Queue. Has no effect on empty queues.
		 */
		public void removeAll()
		{

			while (!isEmpty())
			{
				removeFirst();
			}
		}

		/// <summary>
		/// Removes the first process from the queue and provides a reference to this process. If the queue is
		/// empty, null is returned.
		/// </summary>
		/// <returns>
		/// The first process in this queue, which has been removed, or <c>null</c> in case the queue was empty
		/// </returns>
		public P removeFirst()
		{

			P first = this.first();

			if (first != null)
			{

				_ql.remove(first);

				// produce trace output
				if (currentlySendTraceNotes())
				{
					if (first == currentEntity() && currentEntityAll().Count == 1)
					{
						sendTraceNote("removes itself from " + this.getQuotedName());
					}
					else
					{
						sendTraceNote("removes " + first.getQuotedName() + " from "
								+ this.getQuotedName());
					}
				}

			}

			return first;
		}


		/// <summary>
		/// Removes the first process from the queue that fulfills to the given condition. Also provides a
		/// reference to this process. If the queue does not contain a process that fulfills the condition (e.g. if
		/// the queue is empty), null is returned.
		/// </summary>
		/// <param name="c">The condition that the process returned must fulfill</param>
		/// <returns>
		/// The first process in this queue fulfilling the condition, which has been removed from the queue.
		/// <c>Null</c> in case no process fulfills the condition.
		/// </returns>
		public P removeFirst(Condition<P> c)
		{

			if (c == null)
			{
				sendWarning(
						"Can not remove the first process fulfilling a condition!",
						"Queue : " + getName() + " Method: void removeFirst(Condition c)",
						"The Condition 'c' given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}

			P first = this.first(c);

			if (first != null)
			{

				_ql.remove(first);

				// produce trace output
				if (currentlySendTraceNotes())
				{
					if (first == currentEntity() && currentEntityAll().Count == 1)
					{
						sendTraceNote("removes itself from " + this.getQuotedName());
					}
					else
					{
						sendTraceNote("removes " + first.getQuotedName() + " from "
								+ this.getQuotedName());
					}
				}

			}

			return first;
		}

		/// <summary>
		/// Removes the last process from the queue and provides a reference to this process. If the queue is
		/// empty, <c>null</c> is returned.
		/// </summary>
		/// <returns>
		/// The last process in this queue, which has been removed, or <c>null</c> in case the queue was empty
		/// </returns>
		public P removeLast()
		{

			P last = this.last();

			if (last != null)
			{

				_ql.remove(last);

				// produce trace output
				if (currentlySendTraceNotes())
				{
					if (last == currentEntity() && currentEntityAll().Count == 1)
					{
						sendTraceNote("removes itself from " + this.getQuotedName());
					}
					else
					{
						sendTraceNote("removes " + last.getQuotedName() + " from "
								+ this.getQuotedName());
					}
				}

			}

			return last;
		}

		/// <summary>
		/// Removes the last process from the queue that fulfills to the given condition, determined by traversing
		/// the queue from last to first until a process fulfilling the condition is found. Also provides a
		/// reference to this process. If the queue does not contain a process that fulfills the condition (e.g. if
		/// the queue is empty), <c>null</c> is returned.
		/// </summary>
		/// <param name="c">The condition that the entity returned must fulfill</param>
		/// <returns>
		/// The last process in this queue fulfilling the condition, which has been removed from the queue.
		/// <c>Null</c> in case no process fulfills the condition.
		/// </returns>
		public P removeLast(Condition<P> c)
		{

			if (c == null)
			{
				sendWarning(
						"Can not remove the last entity fulfilling a condition!",
						"Queue : " + getName() + " Method: void removeLast(Condition c)",
						"The Condition 'c' given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}

			P last = this.last(c);

			if (last != null)
			{

				_ql.remove(last);

				// produce trace output
				if (currentlySendTraceNotes())
				{
					if (last == currentEntity() && currentEntityAll().Count == 1)
					{
						sendTraceNote("removes itself from " + this.getQuotedName());
					}
					else
					{
						sendTraceNote("removes " + last.getQuotedName() + " from "
								+ this.getQuotedName());
					}
				}

			}

			return last;
		}

		///**
		// * Removes the process queued at the given position.
		// * The first position is 0, the last one length()-1.
		// * 
		// * @return : The method returns <code>true</code> if a <code>SimProcess/Entity</code>
		// *           exists at the given position or <code>false></code> if otherwise.
		// */
		//public bool remove(int index)
		//{
		//	if (index < 0 || index >= this.length()) return false;

		//	P p = get(index);
		//	if (p == null)
		//	{
		//		return false;
		//	}
		//	else
		//	{
		//		remove(p);
		//		return true;
		//	}
		//}

		/**
		 * Resets all statistical counters to their default values. The mininum and
		 * maximum length of the queue are set to the current number of queued
		 * objects. The counter for the entities refused to be enqueued will be
		 * reset.
		 */
		public void reset()
		{

			base.reset(); // reset of QueueBased

			_refused = 0;

		}

		/// <summary>
		/// Sets the queue capacity to a new value. Only if the new capacity is equal or larger than the current
		/// length of the queue!
		/// </summary>
		/// <param name="newCapacity">The new capacity of this ProcessQueue.</param>
		public void setQueueCapacity(int newCapacity)
		{

			// check if the new capacity is negative or larger than the current
			// length
			// of the queue
			if (newCapacity < length() || newCapacity < 0)
			{
				sendWarning(
						"The new capacity is negative or would be smaller than the "
								+ "number of entities already enqueued in this ProcessQueue. The capacity "
								+ "will remain unchanged!",
						GetType().ToString() + ": " + getQuotedName()
								+ ", Method: "
								+ "void setQueueCapacity(int newCapacity)",
						"The ProcessQueue already contains more entities than the new capacity "
								+ "could hold. What should happen to the remaining entities?",
						"Make sure to change the capacity only to a non negative value larger "
								+ "than the current length of this ProcessQueue.");

				return; // ignore that rubbish and just return
			}

			// set the capacity of the queue to the new value
			queueLimit = newCapacity;

		}

		/// <summary>
		/// Sets the sort order of this ProcessQueue to a new value and makes this ProcessQueue use another
		/// <c>QueueList</c> with the specified queueing discipline. Please choose a constant from
		/// <c>QueueBased</c> (<c>QueueBased.FIFO</c>, <c>QueueBased.FIFO</c> or <c>QueueBased.Random</c>) The sort
		/// order of a ProcessQueue can only be changed if the queue is empty.
		/// </summary>
		/// <param name="sortOrder">
		/// determines the sort order of the underlying <c>QueueList</c> implementation (<c>QueueBased.FIFO</c>,
		/// <c>QueueBased.FIFO</c> or <c>QueueBased.Random</c>)
		/// </param>
		public void setQueueStrategy(int sortOrder)
		{

			// check if the queue is empty
			if (!isEmpty())
			{
				sendWarning(
						"The ProcessQueue for which the queueing discipline should be "
								+ "changed is not empty. The queueing discipline will remain unchanged!",
						GetType().ToString() + ": " + getQuotedName()
								+ ", Method: "
								+ "void setQueueStrategy(int sortOrder)",
						"The ProcessQueue already contains some processes ordered according a "
								+ "certain order.",
						"Make sure to change the sort order only for an empty ProcessQueue.");

				return; // ignore that rubbish and just return
			}

			// determine the queueing strategy
			switch (sortOrder)
			{
				case QueueBased.FIFO:
					_ql = new QueueListFifo<P>(); break;
				case QueueBased.LIFO:
					_ql = new QueueListLifo<P>(); break;
				case QueueBased.RANDOM:
					_ql = new QueueListRandom<P>(); break;
				default:
					sendWarning(
							"The given sortOrder parameter is negative or too big! "
									+ "The sort order of the ProcessQueue will remain unchanged!",
							GetType().ToString() + ": " + getQuotedName()
									+ ", Method: "
									+ "void setQueueStrategy(int sortOrder)",
							"A valid positive integer number must be provided to "
									+ "determine the sort order of the queue.",
							"Make sure to provide a valid positive integer number "
									+ "by using the constants in the class QueueBased, like "
									+ "QueueBased.FIFO, QueueBased.LIFO or QueueBased.RANDOM.");
					return;
			}
			_ql.setQueueBased(this);

		}

		/// <summary>
		/// Sets the number of entities refused to be enqueued in the queue because the capacity limit is reached
		/// to a new value.
		/// </summary>
		/// <param name="n">
		/// the new number of entities refused to be enqueued in the queue because the capacity limit is reached.
		/// </param>
		public void setRefused(long n)
		{
			// check if n is negative
			if (n < 0)
			{
				sendWarning(
						"Attempt to set the number of entities refused to enqueue in "
								+ "the ProcessQueue to a negative value. The attempted action "
								+ "is ignored!", "ProcessQueue : " + getName()
								+ " Method: void setRefused(long n)",
						"The number given as parameter n is negative! That makes no "
								+ "sense!",
						"Make sure to provide only positive numbers as parameter n.");
				return;
			}

			this._refused = n; // save the new value

		}

		/// <summary>
		/// Returns the SimProcess/Entity enqueued directly after the given SimProcess/Entity in the process-queue.
		/// If the given SimProcess/Entity is not contained in this process-queue or is at the last position thus
		/// having no possible successor, <c>null</c> is returned.
		/// </summary>
		/// <param name="p">A SimProcess/Entity in the process-queue</param>
		/// <returns>
		/// The SimProcess/Entity directly after the given SimProcess/Entity in the ProcessQueue or <c>null</c>
		/// </returns>
		public P succ(P p)
		{

			if (p == null)
			{
				sendWarning(
						"Can not find successor of SimProcess/Entity in Queue!",
						"ProcessQueue : " + getName()
								+ " Method: SimProcess/Entity succ(P p)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when querying for "
								+ "Entities");
				return null; // no proper parameter
			}

			return _ql.succ(p);

		}

		/// <summary>
		/// Returns the SimProcess/Entity enqueued after the given SimProcess/Entity in the process-queue that also
		/// fulfills the condition given. If the given SimProcess/Entity is not contained in this process-queue or
		/// is at the last position thus having no possible successor, <c>null</c> is returned. If no other
		/// SimProcess/Entity after the given one fulfills the condition, <c>null</c> is returned, too.
		/// </summary>
		/// <param name="p">A SimProcess/Entity in the process-queue</param>
		/// <param name="c">The condition that the succeeding SimProcess/Entity has to fulfill</param>
		/// <returns>
		/// The SimProcess/Entity after the given SimProcess/Entity in the process-queue fulfilling the condition
		/// or <c>null</c>.
		/// </returns>
		public P succ(P p, Condition<P> c)
		{

			if (p == null)
			{
				sendWarning(
						"Can not find predecessor of SimProcess/Entity in Queue!",
						"ProcessQueue : "
								+ getName()
								+ " Method: SimProcess/Entity succ(P p, Condition c)",
						"The SimProcess/Entity 'p' given as parameter is a null reference!",
						"Check to always have valid references when querying for Entities");
				return null; // no proper parameter
			}

			if (c == null)
			{
				sendWarning(
						"Can not return previous SimProcess/Entity fulfilling condition!",
						"ProcessQueue : "
								+ getName()
								+ " Method: SimProcess/Entity succ(P p, Condition c)",
						"The Condition 'c' given as parameter is a null reference!",
						"Check to always have valid references when querying Queues.");
				return null; // no proper parameter
			}

			for (P tmp = succ(p); tmp != null; tmp = succ(tmp))
			{
				if (c.check(tmp))
					return tmp;
			}

			return null; // obviously not found here, empty or doesn't fulfill

		}

        public IEnumerator<P> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /**
		 * Returns an iterator over the processes enqueued.
		 *
		 * @return java.lang.Iterator&lt;P&gt; : An iterator over the processes enqueued.
		 */
        //public Iterator<P> iterator() {
        //    return new ProcessQueueIterator(this);
        //}

        ///**
        // * Private queue iterator, e.g. required for processing all queue elements in a 
        // * for-each-loop.
        // */
        //private class ProcessQueueIterator implements Iterator<P> {

        //    ProcessQueue<P> clientQ; 
        //    P next, lastReturned;

        //    public ProcessQueueIterator(ProcessQueue<P> clientQ) {
        //        this.clientQ = clientQ;
        //        next = clientQ.first();
        //        lastReturned = null;
        //    }
        //    public boolean hasNext() {
        //        return next != null;
        //    }
        //    public P next() {
        //        lastReturned = next;
        //        next = clientQ.succ(next);
        //        return lastReturned;
        //    }
        //    public void remove() {
        //        clientQ.remove(lastReturned);
        //    }
        //}
    }
}