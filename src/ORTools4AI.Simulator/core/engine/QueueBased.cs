// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.statistic;
using static ORTools4AI.Simulator.core.engine.QueueBased;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * Provides the typical statistics common to all ModelComponents based on Queues. It is set abstract to prevent users to use it straight without
     * deriving a class first because it only provides the functionality for statistical data extraction, not the functionality for actually queueing
     * Entities. The statistical values provided are the queue's length and its elements' waiting times with minimum, maximum, mean and standard deviation
     * for each. This class should be used when any type of ModelComponent using Queues is created. In combination with class QueueList an automatic
     * insert/remove mechanism including search functionality with condition checking can be set up within a few lines of code. It also provides full
     * automatic statistical data about the Queue used.
     * 
     * @see QueueList
     */
	//  public abstract class QueueBased :Reportable implements
	//Subject<QueueBased, QueueAction> {

	public abstract class QueueBased : Reportable,Subject<QueueBased, QueueAction>
	{
		/**
		 * 
		 * Enum to representing the actions of adding and removing an item to/from the queue. This enum is usesed
		 * 
		 * @author Malte Unkrig
		 * 
		 */
		public enum QueueAction
		{
			ITEM_ADDED, ITEM_DELETED
		}

		/**
		 * intern Reference to the administration code
		 */
		private readonly SubjectAdministration<QueueBased, QueueAction> subjectAdministration = new SubjectAdministration<QueueBased, QueueAction>();

		/**
		 * Represents the value returned if for a given statistics no valid value can be returned.
		 */
		public static readonly double UNDEFINED = -1;

		/**
		 * Displays the current number of objects waiting inside the queue.
		 */
		private int _currentLength;

		/**
		 * Flag for letting the underlying queue implementation ( <code>QueueList</code>) show own warnings (<code>true</code>) or suppressing them (
		 * <code>false</code>). Is <code>false</code> by default but can be set to <code>true</code> for debugging purposes
		 */
		private bool _qImpWarnings;

		/**
		 * Displays the minimum number of objects that have been waiting inside the queue since the last reset.
		 */
		private int _minimumLength;

		/**
		 * Displays the maximum number of objects that have been waiting inside the queue since the last reset.
		 */
		private int _maximumLength;

		/**
		 * Displays the number of objects that have passed the queue without waiting time. Thus the name "zeros" for zero waiting time. Value is valid for
		 * the span of time since the last reset.
		 */
		private long _zeros;

		/**
		 * Displays the sum of the queue length weighted over the time each object spent waiting in the queue. Value is valid for the span of time since
		 * the last reset.
		 */
		private double _wSumLength;

		/**
		 * Displays the squares of the sums of the queue length weighted over the time each object spent waiting in the queue. Value is valid for the span
		 * of time since the last reset.
		 */
		private double _wSumSquareLength;
		/**
		 * The point in simulation time the queue was last accessed. Value is valid for the span of time since the last reset.
		 */
		private TimeInstant _lastAcc;

		/**
		 * The point in simulation time the queue's minimum length was recorded. Value is valid for the span of time since the last reset.
		 */
		private TimeInstant _minimumLengthAt;

        /**
		 * The point in simulation time the queue's maximum length was recorded. Value is validgetQueueList(); for the span of time since the last reset.
		 */
        private TimeInstant _maximumLengthAt;

		/**
		 * The maximum time an object inside the queue spent waiting. Value is valid for the span of time since the last reset.
		 */
		private TimeSpan _maximumWaitTime;

		/**
		 * The point in simulation time the maximum waiting time of an object inside the queue was recorded at. Value is valid for the span of time since
		 * the last reset.
		 */
		private TimeInstant _maximumWaitTimeAt;

		/**
		 * The sum of the waiting times spent by all objects that have passed through the queue. Value is valid for the span of time since the last reset.
		 */
		private TimeSpan _sumWaitTime;

		/**
		 * The square of the sums of the waiting times spent by all objects that have passed through the queue. Value is valid for the span of time since
		 * the last reset.
		 */
		private double _sumSquareWaitTime;

		/**
		 * Defining a constant for the FIFO (First In First Out) service discipline of the underlying queue: An Entity inserted into the queue is removed
		 * after all entities already enqueued with the same priority.
		 */
		public const  int FIFO = 0;

		/**
		 * Defining a constant for the LIFO (Last In First Out) service discipline of the underlying queue: An Entity inserted into the queue is removed
		 * before all entities already enqueued with the same priority.
		 */
		public const int LIFO = 1;

		/**
		 * Defining a constant for the random service discipline of the underlying queue: An Entity inserted into the queue may be removed before or after
		 * any other Entity with the same priority.
		 */
		public const int RANDOM = 2;

		/**
		 * Represents the maximum number of entities in the queue (default is unlimited capacity).
		 */
		protected int queueLimit = int.MaxValue;// Integer.MAX_VALUE;

		/// <summary>
		/// Creates a QueueBased object and initializes all statistical counters. If this standard constructor is
		/// used a queue with Fifo sort order and no limited capacity will be created.
		/// </summary>
		/// <param name="owner">The model it belongs to</param>
		/// <param name="name">The name for this QueueBased object</param>
		/// <param name="showInReport">Flag if values are shown in report</param>
		/// <param name="showInTrace">Flag if QueueBased writes trace messages</param>
		public QueueBased(Model owner, string name, bool showInReport,
				bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{

			//super(owner, name, showInReport, showInTrace); // create reportable

			// initialize all statistics
			_currentLength = 0; // no one in here at starting time
			_lastAcc = presentTime(); // time of last access is now
			_minimumLength = _currentLength;
			_maximumLength = _currentLength;
			_zeros = 0;
			_wSumSquareLength = _wSumLength = _sumSquareWaitTime = 0.0;
			_sumWaitTime = _maximumWaitTime = new TimeSpan(0);
			_maximumWaitTimeAt = _minimumLengthAt = _maximumLengthAt = presentTime();
			// reset points of simulation time

			_qImpWarnings = false; // no queue implementation warnings

		}

		/// <summary>
		/// Updates the statistics when a new element is inserted into the underlying queue. Note that this method
		/// must always be called whenever an insertion is made. If class <c>QueueList</c> is used in combination
		/// with a QueueBased, this method gets called automatically whenever a new Entity is inserted.
		/// </summary>
		/// <remarks>See also QueueList See also QueueListFifo See also QueueListLifo</remarks>
		public void addItem()
		{

			updateStatistics();
			_currentLength++;

			if (_currentLength > _maximumLength)
			{ // do we have a new record high?
				_maximumLength = _currentLength; // yes, store the record length
				_maximumLengthAt = presentTime(); // and write down the time for
												  // it
			}

			//notifyObservers(this, QueueAction.ITEM_ADDED);
		}

		//public void addObserver(Observer<QueueBased, QueueAction> observer) {
		//	subjectAdministration.addObserver(observer);
		//}

		/// <summary>
		/// Returns the average length of the underlying queue since the last reset. Current length of that queue
		/// will be returned, if the time span since the last reset is smaller than the smallest distinguishable
		/// timespan epsilon.
		/// </summary>
		/// <returns>
		/// The average queue length since last reset or current length of queue if no distinguishable periode of
		/// time has passed
		/// </returns>
		public double averageLength()
		{

			TimeInstant now = presentTime(); // store current time
			TimeSpan deltaTime = TimeOperations.diff(now, resetAt()); // time since
																	  // last
																	  // reset
			if (TimeSpan.isEqual(deltaTime, TimeSpan.ZERO))
			{
				// has no time passed since the last reset?

				return UNDEFINED; // value is not defined
			}

			// calculate the average length
			double avgLength = (_wSumLength + (_currentLength * TimeOperations.diff(now, _lastAcc)
					.getTimeInEpsilon())) / deltaTime.getTimeInEpsilon();
			// not nice to read, but it really does calculate the average!!!

			// round the average length
			double rndAvgLength = StatisticObject.round(avgLength);
			// return the rounded average length

			return rndAvgLength;

		}

		/// <summary>
		/// Returns the average waiting time of all objects who have exited the queue. Value is valid for the time
		/// span since the last reset. Returns 0 (zero) if no objects have exited the queue after the last reset.
		/// </summary>
		/// <returns>
		/// Average waiting time of all objects since last reset or 0 if no objects have exited the queue
		/// </returns>
		public TimeSpan averageWaitTime()
		{

			double obs = getObservations(); // get and store number of observations
											// return rounded average wait time
			if (obs > 0)
			{
				// calculate the resulting average wait time
				TimeSpan avgWaitTime = TimeOperations.divide(_sumWaitTime, obs);
				return avgWaitTime;
			}
			else
			{
				return TimeSpan.ZERO; // no observations -> zero TimeSpan value
			}

		}

		/**
		 * Creates the reporter qualified to produce a report about a class that has been derived from QueueBased. This method is declared abstract since
		 * no real QueueBased is supposed to be instantiated and thus no rReporter can be defined here. Implement this method in the subclasses of
		 * QueueBased such as in class Queue.
		 * 
		 * @see Queue
		 */
		//@Override
		//public abstract desmoj.core.report.Reporter createReporter();

		/**
		 * Updates the statistics when a new element is exiting the underlying queue. Note that this method must always be called whenever an object is
		 * taken from the queue. The simulation time parameter given provides the statistics with the information about the point of time the exiting
		 * object had enterd this queue. This is needed to calculate the waiting times. If a QueueBased is used in conjunction with class queuelist, this
		 * method is automatically called whenever an entity is taken from the queuelist to keep track of
		 * 
		 * @param entryTime
		 *            TimeInstant : Point of simulation time that the object now exiting the QueueBased had entered it
		 */
		public void deleteItem(TimeInstant entryTime)
		{

			updateStatistics();
			TimeInstant now = presentTime(); // Store the actual simulation time
			TimeSpan waitTime = TimeOperations.diff(now, entryTime); // time waited
																	 // in
																	 // queue
			_sumWaitTime = TimeOperations.add(_sumWaitTime, waitTime); // update
																	   // sum

			// calculate square of waitTimes and
			_sumSquareWaitTime += waitTime.getTimeInEpsilon() * waitTime.getTimeInEpsilon();

			if (TimeSpan.isLonger(waitTime, _maximumWaitTime)) // do we have a new
															   // waiting record?
			{
				_maximumWaitTime = waitTime; // store new record
				_maximumWaitTimeAt = now; // and the moment it happened
			}
			if (TimeSpan.isEqual(waitTime, new TimeSpan(0)))
			{
				// waitTime was zero
				_zeros++;
			}

			if (_currentLength <= 0)
			{
				sendWarning("Inconsistent Qeueue length", "QueueBased : " + getName()
						+ " Method: void activateAfter(TimeSpan dt)", "Error in Statistic operations of Queues",
						"Report information to DESMO-J designer Tim Lechler via eMail : "
								+ "1lechler@informatik.uni-hamburg.de");
				return; // bad stuff ?? We got more objects in the queue than
						// registered!!!
			}

			_currentLength--;

			if (_currentLength < _minimumLength)
			{
				_minimumLength = _currentLength;
			}

			incrementObservations();

			notifyObservers(this, QueueAction.ITEM_DELETED);
		}

		//public void deleteObserver(Observer<QueueBased, QueueAction> observer) {
		//	subjectAdministration.deleteObserver(observer);

		//}

		/// <summary>Returns the maximum possible number of entities in the underlying queue.</summary>
		/// <returns>the maximum number of entities in the queue.</returns>
		public int getQueueLimit()
		{
			return queueLimit;
		}

		/// <summary>Returns the current length of the underlying queue.</summary>
		/// <returns>The current queue length, zero if empty.</returns>
		public int length()
		{

			return _currentLength;

		}

		/// <summary>Returns the maximum length of the underlying queue since the last reset.</summary>
		/// <returns>The maximum queue length since last reset</returns>
		public int maxLength()
		{

			return _maximumLength;

		}

		/// <summary>
		/// Returns the point of simulation time with the maximum number of objects waiting inside the underlying
		/// queue. The value is valid for the period since the last reset.
		/// </summary>
		/// <returns>Point of time with maximum queue length since last reset</returns>
		public TimeInstant maxLengthAt()
		{

			return _maximumLengthAt;

		}

		/// <summary>
		/// Returns the maximum duration in simulation time that an object has spent waiting inside the underlying
		/// queue. The value is valid for the period since the last reset.
		/// </summary>
		/// <returns>Longest waiting time of an object in the queue since last reset</returns>
		public TimeSpan maxWaitTime()
		{

			return _maximumWaitTime;

		}

		/// <summary>
		/// Returns the point of simulation time when the object with the maximum waiting time exited the
		/// underlying queue. The value is valid for the period since the last reset.
		/// </summary>
		/// <returns>
		/// The point of simulation time when the object with the maximum waiting time exited the queue
		/// </returns>
		public TimeInstant maxWaitTimeAt()
		{

			return _maximumWaitTimeAt;

		}

		/// <summary>Returns the minimumn length of the underlying queue since the last reset.</summary>
		/// <returns>The minimum queue length since last reset</returns>
		public int minLength()
		{

			return _minimumLength;

		}

		/// <summary>
		/// Returns the point of simulation time with the minimum number of objects waiting inside the underlying
		/// queue. The value is valid for the period since the last reset.
		/// </summary>
		/// <returns>Point of time with minimum queue length since last reset</returns>
		public TimeInstant minLengthAt()
		{

			return _minimumLengthAt;

		}

		//public void notifyObservers(QueueBased subject, QueueAction eventObject) {
		//	subjectAdministration.notifyObservers(subject, eventObject);

		//}

		/// <summary>
		/// Returns a boolean flag telling if the underlying queue implementation should issue own warnings or not.
		/// The warnings from the queue implementation (<c>QueueList</c>) are needed for debugging purposes.
		/// </summary>
		/// <returns>
		/// Is <c>true</c> if the underlying queue implementation should issue warnings, <c>false</c> otherwise
		/// </returns>
		public bool qImpWarn()
		{

			return _qImpWarnings;

		}

		/**
		 * Resets all statistical counters to their default values. The mininum and maximum length of the queue are set to the current number of queued
		 * objects.
		 */
		//@Override
		public void reset()
		{

			//super.reset(); // reset of Reportable

			_lastAcc = presentTime(); // time of last access is now
			_minimumLength = _currentLength;
			_maximumLength = _currentLength;
			_zeros = 0;
			_wSumLength = _wSumSquareLength = _sumSquareWaitTime = 0.0;
			_sumWaitTime = _maximumWaitTime = new TimeSpan(0);
			_maximumWaitTimeAt = _minimumLengthAt = _maximumLengthAt = presentTime();
			// reset points of simulation time

		}

		/// <summary>
		/// Method switches on warnings issued from the underlying queue implementation if parameter given is
		/// <c>true</c>. Warnings are suppressed if <c>false</c> is given. This method is used for internal
		/// debugging only.
		/// </summary>
		/// <param name="warnFlag"><c>true</c> switches warnings on, <c>false</c> switches warnings off</param>
		public void setQueueImpWarning(bool warnFlag)
		{

			_qImpWarnings = warnFlag;

		}



		/// <summary>Returns the standard deviation of the queue's length. Value is weighted over time.</summary>
		/// <returns>The standard deviation for the queue's length weighted over time</returns>
		public double stdDevLength()
		{

			//	TimeInstant now = presentTime(); // store current time
			//TimeSpan deltaTime = TimeOperations.diff(now, resetAt()); // time since

			//	// last
			//	// reset
			//	if (TimeSpan.isEqual(deltaTime, TimeSpan.ZERO)) {
			//		// no time passed since the last reset
			//		return UNDEFINED; // no valid data
			//	} else {
			//		double len = _currentLength; // store and convert length
			//		double mean = averageLength(); // get mean for queuelength
			//		TimeSpan spanSinceLastAcess = TimeOperations.diff(now, _lastAcc); // time
			//																			// span
			//		// since last
			//		// access
			//		return Math.Sqrt(Math
			//				.Abs((_wSumSquareLength + (len * len * spanSinceLastAcess.getTimeInEpsilon()))
			//						/ deltaTime.getTimeInEpsilon() - (mean * mean)));
			//	}

			throw new NotImplementedException();

		}

		/// <summary>Returns the standard deviation of the queue's objects waiting times.</summary>
		/// <returns>The standard deviation for the queue's objects waiting times</returns>
		public TimeSpan stdDevWaitTime()
		{

			//if (getObservations() > 0) {

			//	double mean = averageWaitTime().getTimeInEpsilon(); // get avrg time
			//	double obs = getObservations(); // number of obs exited

			//	return new TimeSpan(java.lang.Math.sqrt( // not nice but functual
			//			java.lang.Math.abs(((obs * _sumSquareWaitTime) - (mean * mean)) / (obs * (obs - 1.0)))),
			//			TimeOperations.getEpsilon()); // as simple as
			//	// that
			//} else {
			//	return TimeSpan.ZERO; // no observations -> no values
			//}

			throw new NotImplementedException();

		}

		/**
		 * Updates the parts of the statistics used by both addItem and deleteItem.
		 */
		protected void updateStatistics()
		{

			TimeInstant now = presentTime(); // store current time
			TimeSpan deltaTime = TimeOperations.diff(now, _lastAcc); // get time
																	 // since last
																	 // xs
			_wSumLength += _currentLength * deltaTime.getTimeInEpsilon(); // weighted
																		  // length sum
			_wSumSquareLength += _currentLength * _currentLength * deltaTime.getTimeInEpsilon();// weighted
																								// square
																								// length
																								// sum
			_lastAcc = now;

		}

		/// <summary>
		/// Returns the number of objects that have passed through the queue without spending time waiting.
		/// </summary>
		/// <returns>The number of elements who have passed the queue without waiting</returns>
		public long zeroWaits()
		{

			return _zeros;

		}

        public void addObserver(Observer<QueueBased, QueueAction> observer)
        {
            subjectAdministration.addObserver(observer);
        }

        public void deleteObserver(Observer<QueueBased, QueueAction> observer)
        {
            subjectAdministration.deleteObserver(observer);
        }

        public void notifyObservers(QueueBased subject, QueueAction eventObject)
        {
            subjectAdministration.notifyObservers(subject, eventObject);
        }
    }

	public enum QueueType { FIFO,LIFO,RANDOM} 
}