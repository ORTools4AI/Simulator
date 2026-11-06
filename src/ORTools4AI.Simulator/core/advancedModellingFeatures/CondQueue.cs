// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures.report;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures
{

	/**
     * In a CondQueue processes are waiting for a specific condition to become true.
     * Process synchronisation happens when processes are waiting in a queue for a
     * specific condition to become true. Each process which uses
     * <code>waitUntil(condition)</code> and does not find this condition to be
     * true, is inserted in a waiting-queue automatically. Whenever something
     * happens which might influence the condition to become <code>true</code>,
     * <code>signal()</code> should be used to check if the first entity (or, if
     * the attribute <code>checkAll</code> is set to true, all entities) in the
     * waiting-queue finds the desired condition now and therefore can continue. The
     * designer of the model is responsible that this check takes place! He also has
     * to implement the condition, this can easily be done by deriving it from the
     * interface <code>Condition</code>. The flag, if only the first or all
     * entities in the queue are checking their conditions again, can be checked and
     * changed using <code>getCheckAll()</code> and <code>setCheckAll()</code>.
     * The first sort criteria of the queue is always highest priorities first, the
     * second queueing discipline of the underlying queue and the capacity limit can
     * be determined by the user (default is FIFO and unlimited capacity). CondQueue
     * is derived from QueueBased, which provides all the statistical functionality
     * for a queue.
     * 
     * @see QueueBased
     */

	public class CondQueue<P> : QueueBased where P : SimProcess
	{

		// ****** attributes ******

		/**
		 * The queue, actually storing the processes waiting for the condition to
		 * become true
		 */
		protected QueueList<P> _queue;

		/**
		 * Check all the entities in the queue?
		 */
		private bool _checkAll;

		/**
		 * Indicates the method where something has gone wrong. Is passed as a
		 * parameter to the method <code>checkProcess()</code>.
		 */
		private string _where;

		/**
		 * Counter for the SimProcesses which are refused to be enqueued, because
		 * the queue capacity is full.
		 */
		private long _refused;

		/// <summary>
		/// Constructor for a CondQueue where entities can wait for a certain condition (to become true). The
		/// queueing discipline and the capacity limit of the underlying queue can be chosen. Highest priority are
		/// always first in the queue.
		/// </summary>
		/// <param name="owner">The model this CondQueue is associated to.</param>
		/// <param name="name">The CondQueue's name</param>
		/// <param name="sortOrder">
		/// determines the sort order of the underlying queue implementation. Choose a constant from
		/// <c>QueueBased</c> like <c>QueueBased.FIFO</c> or <c>QueueBased.LIFO</c> or ...
		/// </param>
		/// <param name="qCapacity">
		/// The capacity of the queue, that is how many processes can be enqueued. Zero (0) means unlimited
		/// capacity.
		/// </param>
		/// <param name="showInReport">Flag, if CondQueue should produce a report or not.</param>
		/// <param name="showInTrace">Flag for trace to produce trace messages.</param>
		public CondQueue(Model owner, string name,
				int sortOrder, int qCapacity, bool showInReport,
				bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace); // construct QueueBased
			reset();

			_refused = 0; // reset the statistics

			// determine the queueing strategy
			switch (sortOrder)
			{
				case QueueBased.FIFO:
					_queue = new QueueListFifo<P>(); break;
				case QueueBased.LIFO:
					_queue = new QueueListLifo<P>(); break;
				case QueueBased.RANDOM:
					_queue = new QueueListRandom<P>(); break;
				default:
					sendWarning(
							"The given sortOrder parameter " + sortOrder + " is not valid! "
									+ "A queue with Fifo sort order will be created.",
							"CondQueue : "
									+ getName()
									+ " Constructor: CondQueue (desmoj.Model owner, String name, "
									+ "int sortOrder, long qCapacity, boolean showInReport, "
									+ "boolean showInTrace)",
							"A valid positive integer number must be provided to "
									+ "determine the sort order of the queue.",
							"Make sure to provide a valid positive integer number "
									+ "by using the constants in the class QueueBased, like "
									+ "QueueBased.FIFO, QueueBased.LIFO or QueueBased.RANDOM.");
					_queue = new QueueListFifo<P>();
					break;
			}

			// give the QueueList a reference to this QueueBased
			_queue.setQueueBased(this);

			// set the capacity of the queue
			queueLimit = qCapacity;

			// check if it the capacity does make sense
			if (qCapacity < 0)
			{
				sendWarning(
						"The given capacity of the queue is negative! "
								+ "A queue with unlimited capacity will be created instead.",
						"CondQueue : "
								+ getName()
								+ " Constructor: CondQueue (desmoj.Model owner, String name, "
								+ "int sortOrder, long qCapacity, boolean showInReport, "
								+ "boolean showInTrace)",
						"A negative capacity for a queue does not make sense.",
						"Make sure to provide a valid positive capacity "
								+ "for the underlying queue.");
				// set the capacity to the maximum value
				queueLimit = int.MaxValue;
			}

			// check if qCapacity is zero (that means unlimited capacity)
			if (qCapacity == 0)
			{
				// set the capacity to the maximum value
				queueLimit = int.MaxValue;
			}

			this._checkAll = false; // only the first entity in the queue is checked
		}

		// ****** methods ******

		/// <summary>
		/// Constructor for a CondQueue where entities can wait for a certain condition (to become true). The
		/// underlying queue has a Fifo queueing discipline and unlimited capacity.
		/// </summary>
		/// <param name="owner">The model this CondQueue is associated to.</param>
		/// <param name="name">The CondQueue's name</param>
		/// <param name="showInReport">Flag, if CondQueue should produce a report or not.</param>
		/// <param name="showInTrace">Flag for trace to produce trace messages.</param>
		public CondQueue(Model owner, string name,
				bool showInReport, bool showInTrace):base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace); // construct QueueBased
			reset();

			_queue = new QueueListFifo<P>(); // make an actual queue and give it a
			_queue.setQueueBased(this); // reference of this "QueueBased"-CondQueue

			this._checkAll = false; // only the first entity in the queue is checked
		}

		/// <summary>
		/// Activates the given SimProcess and schedules him right after the current process. Then the process is
		/// responsible to check if the desired condition has become true. This is done in the method
		/// <c>waitUntil()</c>.
		/// </summary>
		/// <param name="process">The process that will be activated now to check its condition.</param>
		protected void activateAsNext(SimProcess process)
		{
			_where = "protected void activateAsNext (SimProcess process)";

			if (process != null) // if queue is not empty or a successor is found
			{
				if (!checkProcess(process, _where)) // if next is a null pointer or
				{
					return;
				} // not modelcompatible just return

				if (process.isScheduled()) // different from DESMO, see DESMO-C
				{
					process.skipTraceNote(); // don't tell the user, that we ...
					process.cancel(); // get the process from the event-list
				}

				bool wasBlocked = process.isBlocked();
				if (wasBlocked)
				{
					process.setBlocked(false); // the process is not blocked
											   // anymore
											   // and
				} // ready to become activated

				process.skipTraceNote(); // don't tell the user, that we ...
				process.activateAfter(current());// activate process after
												 // current
												 // process

				if (wasBlocked)
				{
					process.setBlocked(true); // the process status is still
											  // "blocked"
				} // end inner if
			} // end outer if otherwise no one is to be activated
		}

		/// <summary>Checks whether the entity using the CondQueue is a valid process.</summary>
		/// <param name="p">Is this SimProcess a valid one?</param>
		/// <returns>Returns whether the SimProcess is valid or not.</returns>
		protected bool checkProcess(SimProcess p, string where)
		{
			if (p == null) // if p is a null pointer instead of a process
			{
				sendWarning("A non existing process is trying to use a CondQueue  "
						+ "object. The attempted action is ignored!", "CondQueue: "
						+ getName() + " Method: " + where,
						"The process is only a null pointer.",
						"Make sure that only real SimProcesses are using CondQueues.");
				return false;
			}

			if (!isModelCompatible(p)) // if p is not modelcompatible
			{
				sendWarning(
						"The process trying to use a CondQueue object does not "
								+ "belong to this model. The attempted action is ignored!",
						"CondQueue: " + getName() + " Method: " + where,
						"The process is not modelcompatible.",
						"Make sure that processes are using only CondQueues within their model.");
				return false;
			}
			return true;
		}

		/// <summary>Returns a Reporter to produce a report about this CondQueue.</summary>
		/// <returns>The Reporter for the queue inside this CondQueue.</returns>
		protected override Reporter createDefaultReporter()
		{
			return new CondQueueReporter(
					this);
		}

		/// <summary>
		/// Returns if all entities or only the first one in the queue are getting a signal to check their
		/// conditions.
		/// </summary>
		/// <returns>
		/// Are all entities in the queue checking their conditions? Default is <c>false</c>, so that only the
		/// first process in the queue is checking its condition.
		/// </returns>
		public bool getCheckAll()
		{
			return this._checkAll;
		}

		/// <summary>
		/// Returns the implemented queueing discipline of the underlying queue as a String, so it can be displayed
		/// in the report.
		/// </summary>
		/// <returns>The String indicating the queueing discipline.</returns>
		public string getQueueStrategy()
		{

			return _queue.getAbbreviation(); // that's it

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
		/// Sets the flag <c>checkAll</c>: if all entities or only the first one in the queue are getting a signal
		/// to check their conditions.
		/// </summary>
		/// <param name="chckall">
		/// Flag if all entities in the queue should check their conditions? Default is <c>false</c>, so that only
		/// the first entity in the queue is checking its condition.
		/// </param>
		public void setCheckAll(bool chckall)
		{
			_checkAll = chckall;
		}

		/// <summary>
		/// A <c>signal ()</c> should be sent every time when a condition has changed and might be true now. It
		/// activates the first process in the queue to check its condition again. Other processes might follow.
		/// </summary>
		public void signal()
		{
			if (currentlySendTraceNotes())
			{
				// tell in the trace that the CondQueue gets a signal
				sendTraceNote("signals '" + this.getName() + "'");
			}

			// activate the first process in the queue, others might follow...
			activateAsNext(_queue.first());
		}

		/// <summary>
		/// Returns the first process waiting in the queue. If there is no process waiting, <c>null</c> is
		/// returned.
		/// </summary>
		/// <returns>Returns the first process in the queue (or <c>null</c> if no process is waiting).</returns>
		public P first()
		{
			if (_queue.isEmpty())
			{ // nobody home to be checked
				return null;
			}
			else
			{
				return _queue.first();
			}

		} // end method	

		/// <summary>
		/// Returns the first process waiting in the queue that complies to the given condition. If there is no
		/// such process waiting, <c>null</c> is returned.
		/// </summary>
		/// <param name="cond">
		/// The Condition <c>cond</c> is describing the condition to which the process must comply to. This has to
		/// be implemented by the user in the class: <c>Condition</c> in the method: <c>check()</c>.
		/// </param>
		/// <returns>Returns the first process in the queue which complies to the given condition.</returns>
		public P first(Condition<P> cond)
		{
			if (_queue.isEmpty())
			{ // nobody home to be checked
				return null;
			} // return null

			for (P process = _queue.first(); process != null; process = _queue
					.succ(process))
			{
				if (cond.check(process))
					return process;
			}

			// if no SimProcess complies to the condition just return null
			return null;

		} // end method

		/// <summary>
		/// Removes the given SimProcess from the Queue. The process no longer waits for its condition to become
		/// true and resumes its lifecycle.
		/// </summary>
		/// <param name="p">The P to be removed from the queue</param>
		public void waitCancel(P p)
		{

			if (p == null)
			{
				sendWarning("Can not cancel waiting of SimProcess in Queue!", "CondQueue : "
						+ getName() + " Method:  void remove(P p)",
						"The SimProcess 'p' given as parameter is a null reference!",
						"Check to always have valid references when removing "
								+ "processes");
				return; // no proper parameter
			}
			if (!_queue.contains(p))
			{
				sendWarning("Can not cancel waiting of SimProcess in Queue!", "CondQueue : "
						+ getName() + " Method:  void remove(P p)",
						"The SimProcess 'p' given as parameter is not enqueued in this "
								+ "CondQueue!",
						"Make sure the process is inside the queue.");
				return; // not enqueued here
			}

			// make sure process is passive
			if (p.isScheduled()) // different from DESMO, see DESMO-C
			{
				p.skipTraceNote(); // don't tell the user, that we ...
				p.cancel(); // get the process from the event-list
			}

			// unblock
			p.setBlocked(false);
			p.skipTraceNote(); p.activate();  // don't tell we do an ordinary activation here

			// produce trace output
			if (currentlySendTraceNotes())
			{
				sendTraceNote("cancels waiting of " + p.getQuotedName() + " in "
						+ this.getQuotedName());
			}
		}


		/// <summary>
		/// Lets the current process wait in the CondQueue until a certain condition, given as a parameter, has
		/// become true. When the process finds its condition to have become true or <c>checkAll</c> is set to
		/// <c>true</c> the next process in the queue will be activated, too. If the capacity limit of the queue is
		/// reached, the process will not be enqueued and <c>false</c> returned.
		/// </summary>
		/// <param name="cond">The condition that has to become true before the process can continue.</param>
		/// <returns>
		/// Is <c>true</c> if the process can be enqueued successfully, <c>false</c> otherwise (i.e. capacity limit
		/// of the queue is reached).
		/// </returns>
		/// <remarks>See also Condition</remarks>
		public bool waitUntil(Condition<P> cond)
		{
			_where = "boolean waitUntil (desmoj.core.simulator.Condition cond)";

			P currentProcess = (P)currentSimProcess();

			if (!checkProcess(currentProcess, _where)) // check the current process
			{
				return false;
			} // if it is not valid just return

			if (!this.isModelCompatible(cond)) // if cond is not modelcompatible
			{
				sendWarning("Attempt to use a Condition object that does not "
						+ "belong to this model. The attempted action is ignored!",
						"CondQueue: " + getName()
								+ " Method: boolean waitUntil (Condition cond)",
						"The condition is not modelcompatible.",
						"Make sure that conditions given in a CondQueue waitUntil() method "
								+ "are modelcompatible with the CondQueue object.");
				return false;
			}

			if (queueLimit <= length()) // check if capac. limit of queue is reached
			{
				if (currentlySendDebugNotes())
				{
					sendDebugNote("refuses to insert "
						+ currentProcess.getQuotedName()
						+ " in waiting-queue, because the capacity limit is reached. ");
				}

				if (currentlySendTraceNotes())
				{
					sendTraceNote("is refused to be enqueued in "
						+ this.getQuotedName() + "because the capacity limit ("
						+ getQueueLimit() + ") of the " + "queue is reached");
				}

				_refused++; // count the refused ones

				return false; // capacity limit is reached
			}

			_queue.insert(currentProcess); // insert every process in the queue for
										   // statistic reasons

			bool waitingCanceled = false;

			if (!cond.check(currentProcess)) // condition is not true
			{
				if (currentlySendTraceNotes())
				{
					sendTraceNote("waits in '" + getName() + "'" + " until '"
							+ cond.getName() + "' "); // send a traceNote
				}

				bool proceed = false; // can we proceed? is the condition
										 // true?

				do
				{ // the process is stuck in here
					currentProcess.setBlocked(true); // as long as ...see while
					currentProcess.skipTraceNote(); // don't tell the user, that we
						
					//TODO:how to implement the passivate with yield ...
					// (currentProcess as SimProcess).passivate(); // passivate the current process

					// waiting canceled? 
					if (!currentProcess.isBlocked())
					{
						waitingCanceled = true;
						break;
					}

					proceed = cond.check(currentProcess); // has the condition
														  // become true?

					if (proceed || _checkAll) // check also the next process in
											  // the
											  // q?
					{
						activateAsNext(_queue.succ(currentProcess));
						// activate the next process in the queue
					}
				} while (!proceed); // as long as the condition is not true
			} // end if

			// the condition is true now, so we are free...yeah!
			if (currentlySendTraceNotes())
			{
				if (waitingCanceled)
				{
					sendTraceNote("resumes after waiting in '" + getName()
							+ "' canceled"); // send a traceNote
				}
				else
				{
					sendTraceNote("leaves '" + getName() + "', because '"
							  + cond.getName() + "' is true"); // send a traceNote
				}
			}
			_queue.remove(currentProcess); // get the process out of the queue
			currentProcess.setBlocked(false); // we are not blocked (anymore),
											  // yeah!

			return true;
		}
	} // end class CondQueue
}
