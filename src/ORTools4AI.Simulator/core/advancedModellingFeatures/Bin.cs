// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures.report;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures
{


	/**
	 * Bin is the place where producers can store their products for consumers to
	 * come and use them up. Bin is used to implement process synchronization
	 * between producers and consumers. Producers are producing products and store
	 * them using <code>store()</code> in the bin. Consumers make the Bin
	 * <code>deliver()</code> the products to use them up. If no or not enough units
	 * of a product are available for the consumers, they have to wait in a queue
	 * until new units are delivered by a producer. The first sort criteria of the
	 * queue is always highest priorities first, the second queueing discipline of
	 * queue is always highest priorities first, the second queueing discipline of
	 * the underlying queue and the capacity limit can be determined by the user
	 * (default is FIFO and unlimited capacity). Bin is derived from QueueBased,
	 * which provides all the statistical functionality for a queue.
	 * 
	 * @see QueueBased
	 */

	public class Bin : QueueBased
	{

		// ****** attributes ******

		/**
		 * The queue, actually storing the processes waiting for products
		 */
		protected QueueList<SimProcess> _queue;

		/**
		 * The initial number of products in the Bin
		 */
		private long _initial;

		/**
		 * The maximum number of products in the Bin
		 */
		private long _maximum;

		/**
		 * Number of products available at the moment
		 */
		private long _avail;

		/**
		 * Number of producers having visited the Bin
		 */
		private long _producers;

		/**
		 * Number of consumers having visited the Bin
		 */
		private long _consumers;

		/**
		 * Weighted sum of available products in the Bin over the time (must be
		 * divided by the total time to get the average available units!)
		 */
		private double _wSumAvail;

		/**
		 * The last time the Bin has been used
		 */
		private TimeInstant _lastUsage;

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

		/**
		 * Flag to indicate whether an entity can pass by other entities in the
		 * queue which are enqueued before that entity in the queue. Is
		 * <code>false</code> per default.
		 */
		private bool _passBy = false;

		// ****** methods ******

		/// <summary>
		/// Constructor for a Bin with a number of initial units of a product in it. The queueing discipline and
		/// the capacity limit of the underlying queue can be chosen, too. Highest priority are always first in the
		/// queue.
		/// </summary>
		/// <param name="owner">The model this Bin is associated to.</param>
		/// <param name="name">The Bin's name</param>
		/// <param name="sortOrder">
		/// determines the sort order of the underlying queue implementation. Choose a constant from
		/// <c>QueueBased</c> like <c>QueueBased.FIFO</c> or <c>QueueBased.LIFO</c> or ...
		/// </param>
		/// <param name="qCapacity">
		/// The capacity of the queue, that is how many processes can be enqueued. Zero (0) means unlimited
		/// capacity.
		/// </param>
		/// <param name="initialUnits">The units of a product the Bin starts with. Must be positive.</param>
		/// <param name="showInReport">Flag, if Bin should produce a report or not.</param>
		/// <param name="showInTrace">Flag for trace to produce trace messages.</param>
		public Bin(Model owner, string name, int sortOrder,
				int qCapacity, long initialUnits, bool showInReport,
				bool showInTrace):base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace); // construct QueueBased
			reset();

			this._initial = initialUnits;
			this._maximum = initialUnits;
			this._avail = initialUnits;

			if (initialUnits < 0) // there can't be less than nothing
			{
				sendWarning("Attempt to construct a Bin with a negativ number of"
						+ " units. Initial number of units set to zero!", "Bin: "
						+ getName() + " Constructor: Bin (desmoj.Model owner, "
						+ "String name, long initialUnits, boolean showInReport, "
						+ "boolean showInTrace)",
						"A negative number of units does not make sense here.",
						"Make sure to initialize a Bin always with a positive number of "
								+ "initialUnits.");

				_initial = _maximum = _avail = 0; // set it to 0, that makes more
												  // sense
			}

			// determine the queueing strategy
			switch (sortOrder)
			{
				case QueueBased.FIFO:
					_queue = new QueueListFifo<SimProcess>(); break;
				case QueueBased.LIFO:
					_queue = new QueueListLifo<SimProcess>(); break;
				case QueueBased.RANDOM:
					_queue = new QueueListRandom<SimProcess>(); break;
				default:
					sendWarning(
							"The given sortOrder parameter " + sortOrder + " is not valid! "
									+ "A queue with Fifo sort order will be created.",
							"Bin: "
									+ getName() + " Constructor: Bin (desmoj.Model owner, "
									+ "String name, long initialUnits, boolean showInReport, "
									+ "boolean showInTrace)",
							"A valid positive integer number must be provided to "
									+ "determine the sort order of the queue.",
							"Make sure to provide a valid positive integer number "
									+ "by using the constants in the class QueueBased, like "
									+ "QueueBased.FIFO, QueueBased.LIFO or QueueBased.RANDOM.");
					_queue = new QueueListFifo<SimProcess>();
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
						"Bin : "
								+ getName()
								+ " Constructor: Bin (desmoj.Model owner, String name, "
								+ "int sortOrder, long qCapacity, long initialUnits, "
								+ "boolean showInReport, boolean showInTrace)",
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

		}

		// ****** methods ******

		/// <summary>
		/// Constructor for a Bin with a number of initial units of a product in it. The underlying queue has a
		/// Fifo queueing discipline and unlimited capacity.
		/// </summary>
		/// <param name="owner">The model this Bin is associated to.</param>
		/// <param name="name">The Bin's name</param>
		/// <param name="initialUnits">The units of a product the Bin starts with. Must be positive.</param>
		/// <param name="showInReport">Flag, if Bin should produce a report or not.</param>
		/// <param name="showInTrace">Flag for trace to produce trace messages.</param>
		public Bin(Model owner, string name,
				long initialUnits, bool showInReport, bool showInTrace):base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace); // construct QueueBased
			reset();

			_queue = new QueueListFifo<SimProcess>(); // make an actual queue and give it a
			_queue.setQueueBased(this); // reference of this "QueueBased"-Bin

			this._initial = initialUnits;
			this._maximum = initialUnits;
			this._avail = initialUnits;

			if (initialUnits < 0) // there can't be less than nothing
			{
				sendWarning("Attempt to construct a Bin with a negativ number of"
						+ " units. Initial number of units set to zero!", "Bin: "
						+ getName() + " Constructor: Bin (desmoj.Model owner, "
						+ "String name, long initialUnits, boolean showInReport, "
						+ "boolean showInTrace)",
						"A negative number of units does not make sense here.",
						"Make sure to initialize a Bin always with a positive number of "
								+ "initialUnits.");

				_initial = _maximum = _avail = 0; // set it to 0, that makes more
												  // sense
			}
		}

		/// <summary>
		/// Activates the SimProcess <c>process</c>, given as a parameter of this method, as the next process. This
		/// process should be a SimProcess waiting in the queue for some products.
		/// </summary>
		/// <param name="process">The process that is to be activated as next.</param>
		protected void activateAsNext(SimProcess process)
		{
			_where = "protected void activateAsNext(SimProcess process)";

			if (process != null)
			{
				// if the given process is not valid just return
				if (!checkProcess(process, _where))
				{
					return;
				}

				// if the process is scheduled (on the event-list) already
				if (process.isScheduled())
				{
					process.skipTraceNote(); // don't tell the user, that we ...
					process.cancel(); // get the process from the event-list
				}

				// remember if the process is blocked at the moment
				bool wasBlocked = process.isBlocked();

				// unblock the process to be able to activate him
				if (wasBlocked)
				{
					process.setBlocked(false); // the process is not blocked
											   // anymore
											   // and
				} // ready to become activated

				// don't tell the user, that we activate the process after the
				// current process
				process.skipTraceNote();
				process.activateAfter(current());

				// the process status is still "blocked"
				if (wasBlocked)
				{
					process.setBlocked(true);
				}
			} // end outer if
		}

		/**
		 * Activates the first process waiting in the queue. That is a process which
		 * was trying to take products, but it could not get any because there were
		 * not enough products for it or another process was first in the queue to
		 * be served. This method is called every time a producer has given new
		 * products or a consumer in the waiting-queue is satisfied.
		 */
		protected void activateFirst()
		{
			_where = "protected void activateFirst()";

			// first is the first process in the queue (or null if none is in the
			// queue)
			SimProcess first = _queue.first();

			if (first != null)
			{
				// if first is not modelcompatible just return
				if (!checkProcess(first, _where))
				{
					return;
				}

				// if first is scheduled (on the event-list) already
				if (first.isScheduled())
				{
					first.skipTraceNote(); // don't tell the user, that we ...
					first.cancel(); // get the process from the event-list
				}

				// remember if first is blocked at the moment
				bool wasBlocked = first.isBlocked();

				// unblock the process to be able to activate him
				if (wasBlocked)
				{
					first.setBlocked(false);
				}

				// don't tell the user, that we activate first after the current
				// process
				first.skipTraceNote();
				first.activateAfter(current());

				// the status of first is still "blocked"
				if (wasBlocked)
				{
					first.setBlocked(true);
				}
			} // end outer if
		}

		/// <summary>
		/// Returning the average number of products available in the bin over the time since the last reset of the
		/// bin.
		/// </summary>
		/// <returns>
		/// The average number of products available in the bin over the time since the last reset of the bin.
		/// </returns>
		public double avgAvail()
		{
			TimeInstant now = presentTime(); // what is the time?
											 // how long since the last reset
			TimeSpan diff = TimeOperations.diff(now, resetAt());

			// update the weighted sum of available units
			double wSumAvl = _wSumAvail
					+ ((double)_avail * TimeOperations.diff(now, _lastUsage)
							.getTimeInEpsilon());

			if (TimeSpan.isEqual(diff, TimeSpan.ZERO)) // diff is not long enough
			{
				sendWarning("A Division-by-Zero error occured in a calculation. "
						+ "The UNDEFINED Value: -1.0 is returned as result.",
						"Bin: " + getName() + " Method: double avgAvail ()",
						"The Time difference is shorter than epsilon.",
						"Make sure not to use avgAvail() right after a reset.");
				return UNDEFINED; // see QueueBased: UNDEFINED = -1
			}
			// return the rounded average
			return StatisticObject.round(wSumAvl / diff.getTimeInEpsilon());
		}

		/// <summary>Checks whether the entity using the bin is a valid process.</summary>
		/// <param name="p">Is this SimProcess a valid one?</param>
		/// <param name="where">The method having called <c>checkProcess()</c> as a String.</param>
		/// <returns>Returns whether the SimProcess is valid or not.</returns>
		protected bool checkProcess(SimProcess p, String where)
		{
			if (p == null) // if p is a null pointer instead of a process
			{
				sendWarning(
						"A non existing process is trying to use a Bin object. "
								+ "The attempted action is ignored!", "Bin: "
								+ getName() + " Method: " + where,
						"The process is only a null pointer.",
						"Make sure that only real SimProcesses are using Bins.");
				return false;
			}

			if (!isModelCompatible(p)) // if p is not modelcompatible
			{
				sendWarning("The process trying to use a Bin object does not "
						+ "belong to this model. The attempted action is ignored!",
						"Bin: " + getName() + " Method: " + where,
						"The process is not modelcompatible.",
						"Make sure that processes are using only Bins within their model.");
				return false;
			}

			return true;
		}

		/// <summary>Returns a Reporter to produce a report about this Bin.</summary>
		/// <returns>The Reporter for the queue inside this bin</returns>
		protected override Reporter createDefaultReporter()
		{
			return new BinReporter(
					this);
		}

		/// <summary>
		/// Method for consumers to make the Bin deliver a number of n products. When there are not enough products
		/// available or another consumer is first in the queue to be served (and it is not possible to pass by),
		/// the current consumer process will be blocked and inserted in the waiting-queue.
		/// </summary>
		/// <param name="n">The number of products the Bin is delivering to the consumer.</param>
		/// <returns>
		/// Is <c>true</c> if the specified number of units have been delivered successfully, <c>false</c>
		/// otherwise (i.e. capacity limit of the queue is reached).
		/// </returns>
		public bool retrieve(long n)
		{
			_where = "boolean retrieve (long n)";

			SimProcess currentProcess = currentSimProcess();

			if (!checkProcess(currentProcess, _where)) // check the current process
			{
				return false;
			} // if it is not valid return false

			if (n <= 0) // if the process is taking nothing or less
			{
				sendWarning(
						"Attempt to take nothing or a negative number of units"
								+ " out of a Bin. The attempted action is ignored!",
						"Bin: " + getName() + " Method: " + _where,
						"It does not make sense to take nothing or less out of a Bin. "
								+ "The statistic will be corrupted with negative numbers!",
						"Make sure to take at least one unit out of the Bin.");
				return false; // go to where you came from; ignore that rubbish
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

			// insert every process in the queue for statistic reasons
			_queue.insert(currentProcess);

			// is it possible for this process to pass by?
			if (_passBy == false)
			{
				if (n > _avail || // not enough products available OR
						currentProcess != _queue.first()) // other process is
														  // first
														  // in the q
				{
					// tell in the trace what the process is waiting for
					if (currentlySendTraceNotes())
					{
						sendTraceNote("awaits " + n + " of '" + this.getName()
								+ "'");
					}

					// for debugging purposes
					if (currentlySendDebugNotes())
					{
						sendDebugNote("can not deliver " + n + " units for "
								+ currentProcess.getQuotedName() + "<br>"
								+ "because there are only " + getAvail()
								+ " units " + "right now.");
					}

					do
					{ // the process is stuck in here
						currentProcess.setBlocked(true); // as long as ...see
														 // while
						currentProcess.skipTraceNote(); // don't tell the user, that

						//TODO: How to passivate a process with yield
						//((SimProcess)currentProcess).passivate(); // passivate the current process
					} while (n > _avail || // not enough products available OR
							currentProcess != _queue.first()); // other process is
															   // first
				} // end if

			} // end if (passBy = false)

			else // the process can pass by other processes in the queue, passBy
				 // =
				 // true
			{
				if (n > _avail || // not enough products available OR
						currentProcess != _queue.first()) // other process is
														  // first
														  // in the q
				{
					// if this process is not the first in the queue
					if (currentProcess != _queue.first())
					{
						// we have to make sure that no other process in front of
						// this current
						// process in the wait queue could be satisfied, so activate
						// the first Process in the queue to see what he can do. He
						// will pass
						// the activation on to his successors until this process
						// will be
						// activated again to get his products. (hopefully)
						activateFirst();
					}

					// only if not enough units are available the process has to
					// wait
					if (n > _avail)
					{
						// tell in the trace what the process is waiting for
						if (currentlySendTraceNotes())
						{
							sendTraceNote("awaits " + n + " of '" + this.getName()
									+ "'");
						}

						// for debugging purposes
						if (currentlySendTraceNotes())
						{
							sendDebugNote("can not deliver " + n + " units for "
									+ currentProcess.getQuotedName() + "<br>"
									+ "because there are only " + getAvail()
									+ " units " + "right now.");
						}
					} // end if not enough units are available

					// block and passivate the process until enough products are
					// available
					do
					{ // the process is stuck in here
						currentProcess.setBlocked(true); // as long as ...see
														 // while
						currentProcess.skipTraceNote(); // don't tell the user, that
							
						// TODO: how to implement a process by yield.
						//((SimProcess)currentProcess).passivate(); // passivate the current process

						// activate the next process in the queue to see what he can
						// do
						activateAsNext(_queue.succ(currentProcess));
					} while (n > _avail); // not enough products available
				}
			} // end else (passBy = true)

			// the current process has got the products he wanted ...

			// tell in the trace what the process is taking from the Bin
			if (currentlySendTraceNotes())
			{
				sendTraceNote("takes " + n + " from '" + this.getName() + "'");
			}

			_queue.remove(currentProcess); // get the process out of the queue
			currentProcess.setBlocked(false); // we are not blocked (anymore),
											  // yeah!

			// activate the new first process in the queue
			activateFirst();

			updateStatistics(-n); // statistics will be updated
								  // with a negative n for deliver(), remember?!
			return true;
		}

		/// <summary>Returns the number of units available at the moment.</summary>
		/// <returns>The number of units available at the moment.</returns>
		public long getAvail()
		{
			return this._avail;
		}

		/// <summary>Returns the number of consumers having visited the Bin.</summary>
		/// <returns>The number of consumers having visited the bin.</returns>
		public long getConsumers()
		{
			return this._consumers;
		}

		/// <summary>Returns the initial number of products in the bin.</summary>
		/// <returns>The number of products the bin started with.</returns>
		public long getInitial()
		{
			return this._initial;
		}

		/// <summary>Returns the maximum number of products in the bin.</summary>
		/// <returns>The maximum number of products in the bin.</returns>
		public long getMaximum()
		{
			return this._maximum;
		}

		/// <summary>
		/// Returns whether entities can pass by other entities which are enqueued before them in the queue.
		/// </summary>
		/// <returns>
		/// Indicates whether entities can pass by other entities which are enqueued before them in the queue.
		/// </returns>
		public bool getPassBy()
		{
			return _passBy;
		}

		/// <summary>Returns the number of producers having used the Bin.</summary>
		/// <returns>The number of producers having put products in the bin.</returns>
		public long getProducers()
		{
			return this._producers;
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
		/// To reset the statistics of this bin. The number of available products at this moment and the processes
		/// waiting in the queue are not changed. But all statistic counters are reset. The <c>QueueBased</c> is
		/// also reset.
		/// </summary>
		public void reset()
		{
			base.reset(); // reset the QueueBased also

			_maximum = _avail;
			_producers = 0;
			_consumers = 0;
			_wSumAvail = 0.0;
			_lastUsage = presentTime();
			_refused = 0;
		}

		/// <summary>
		/// Sets the flag passBy to a new value. PassBy is indicating whether entities can pass by other entities
		/// which are enqueued before them in the queue.
		/// </summary>
		/// <param name="newPassBy">
		/// The new value of passBy. Set it to <c>true</c> if you want entities to pass by other entities which are
		/// enqueued before them in the queue. Set it to <c>false</c> if you don't want entities to overtake other
		/// entities in the queue.
		/// </param>
		public void setPassBy(bool newPassBy)
		{
			this._passBy = newPassBy; // that's all!
		}

		/// <summary>
		/// Method for producers to put a number of n new products in the Bin. When producers <c>store()</c> new
		/// products n is positive. Producers can always store products into the Bin and will never be inserted in
		/// a queue and blocked, because the Bin has no capacity limit..
		/// </summary>
		/// <param name="n">The number of products put into the Bin.</param>
		public void store(long n)
		{
			_where = "void store (long n)";

			SimProcess currentProcess = currentSimProcess();

			if (!checkProcess(currentProcess, _where)) // if the current process
			{
				return;
			} // is not valid: just return

			if (n <= 0) // trying to store nothing or less, don't fool me!
			{
				sendWarning(
						"Attempt to store nothing or a negative number of units"
								+ " in a Bin. The attempted action is ignored!",
						"Bin: " + getName() + " Method: void store (long n)",
						"It does not make sense to put nothing or less into a Bin. "
								+ "The statistic will be corrupted with negative numbers!",
						"Make sure to store at least one unit in the Bin.");
				return; // ignore it and just return
			}

			if (currentlySendTraceNotes())
			{
				sendTraceNote("stores " + n + " to '" + getName() + "'");
			} // tell in the trace what the process is giving into the Bin

			updateStatistics(n);

			// see if someone is in the queue waiting for products
			activateFirst();
		}

		/// <summary>Updates the statistics for producers and consumers.</summary>
		/// <param name="n">
		/// Is positive when producers <c>store()</c> new products in the bin and negative when the Bin
		/// <c>deliver()</c>'s products to the consumer process.
		/// </param>
		protected void updateStatistics(long n)
		{
			TimeInstant now = presentTime();
			_wSumAvail = _wSumAvail
					+ ((double)_avail * TimeOperations.diff(now, _lastUsage)
							.getTimeInEpsilon());

			_lastUsage = now;
			_avail += n; // n can be positive or negative

			if (n > 0) // it is a real producer
			{
				_producers++;
				if (_avail > _maximum)
				{
					_maximum = _avail;
				}
			}
			else // it is a consumer
			{
				_consumers++;
			}
		}
	}
}