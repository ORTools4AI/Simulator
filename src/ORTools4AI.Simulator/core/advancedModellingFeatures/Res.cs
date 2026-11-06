// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures.report;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures
{

    /**
     * Res is the place where resources are stored in a pool. Processes can come by
     * and the resource pool will <code>provide()</code> resources to them. Each
     * process has to give back the same resources it once has acquired by calling
     * the <code>takeBack()</code> method of the Res. Res is used to implement
     * process synchronization between processes, which are using resources. The
     * resource pool has a limited capacity. A process can acquire one or more
     * resources and use them. After usage the process must release this or these
     * same resources to make them available to other processes. If a process can
     * not get the number of resources needed, it has to wait in a queue until
     * enough resources are released by other processes. A process can release its
     * resources anytime. After the resourcepool has <code>"takenBack"()</code> the
     * used resources the waiting-queue is checked for processes waiting for them.
     * The first sort criteria of the queue is always highest priorities first, the
     * second queueing discipline of the underlying queue and the capacity limit can
     * be determined by the user (default is FIFO and unlimited capacity). Under
     * certain circumstances a deadlock might block some waiting
     * 
     * @see QueueBased
     */

    public class Res : QueueBased
    {

        // ****** attributes ******

        /**
		 * The number identifying a Res object. Because it is a class variable each
		 * <code>Res</code> will get its own ID number starting by zero.
		 */
        private static long resNumber = 0;

        /**
		 * The ID number of this <code>Res</code> object.
		 */
        private long _idNumber;

        /**
		 * The queue, actually storing the processes waiting for resources
		 */
        protected QueueList<SimProcess> _queue;

        /**
		 * The vector holding all the pairs (used resources of this Res, the
		 * Sim-process which holds the resources at the moment). See: inner class
		 * UsedResources
		 */
        private List<UsedResources> _arrayOfUsedResources;

        /**
		 * The vector holding all the resources of this resource pool not used at
		 * the moment.
		 */
        private List<Resource> _unUsedResources;

        /**
		 * The resource database keeping track of which SimProcesses holding which
		 * resources and SimPorcesses requesting resources.
		 */
        private ResourceDB _resourceDB;

        /**
		 * To indicate whether the check for deadlocks is active or not. Default is
		 * <code>true</code>= deadlock check enabled.
		 */
        private bool _deadlockCheck = true;

        /**
		 * Is set to <code>true</code> if a deadlock is detected where this Res is
		 * involved in. Otherwise it remains <code>false</code>. Default is
		 * <code>false</code>.
		 */
        private bool _deadlockDetected = false;

        /**
		 * The number of resources in the Res (capacity)
		 */
        private int _limit;

        /**
		 * The minimum number of resources being available
		 */
        private int _minimum;

        /**
		 * Number of resources available at the moment
		 */
        private int _avail;

        /**
		 * Number of processes having acquired and released one or more resources
		 */
        private long _users;

        /**
		 * Weighted sum of available resources (in the Res over the time)
		 */
        private double _wSumAvail;

        /**
		 * The last time the Res has been used
		 */
        private TimeInstant _lastUsage;

        /**
		 * Counter for the SimProcesses which are refused to be enqueued, because
		 * the queue capacity is full.
		 */
        private long _refused;

        /**
		 * Indicates the method where something has gone wrong. Is passed as a
		 * parameter to the method <code>checkProcess()</code>.
		 */
        private string _where;

        /**
		 * Flag to indicate whether an entity can pass by other entities in the
		 * queue which are enqueued before that entity in the queue. Is
		 * <code>false</code> as default value.
		 */
        private bool _passBy = false;

        // ****** inner class ******

        /**
		 * UsedResources is an inner class of Res to encapsulate the pairs of:
		 * Sim-process and an array of resources it holds. These pairs are stored in
		 * the vector <code>arrayOfUsedResources</code>.
		 */

        /**
		 * Constructor for a Res with a number of initial resources in it. The
		 * queueing discipline and the capacity limit of the underlying queue can be
		 * chosen, too.
		 * 
		 * @param owner
		 *            Model : The model this Res is associated to.
		 * @param name
		 *            java.lang.String : The Res's name
		 * @param sortOrder
		 *            int : determines the sort order of the underlying queue
		 *            implementation. Choose a constant from <code>QueueBased</code>
		 *            like <code>QueueBased.FIFO</code> or
		 *            <code>QueueBased.LIFO</code> or ...
		 * @param qCapacity
		 *            int : The capacity of the queue, that is how many processes
		 *            can be enqueued. Zero (0) means unlimited capacity.
		 * @param capacity
		 *            int : The number of resources the Res starts with. Must be
		 *            positive and greater than 0.
		 * @param showInReport
		 *            boolean : Flag, if Res should produce a report or not.
		 * @param showInTrace
		 *            boolean : Flag for trace to produce trace messages.
		 */
        public Res(Model owner, string name, int sortOrder, int qCapacity,
                int capacity, bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {
            // construct QueueBased

            _idNumber = resNumber++; // increment the resNumber and get it as
                                     // IDNumber

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
                            "Res : "
                                    + getName()
                                    + " Constructor: Res (Model owner, String name, int "
                                    + "sortOrder, long qCapacity, int capacity, boolean "
                                    + "showInReport, boolean showInTrace)",
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
                        "Res : "
                                + getName()
                                + " Constructor: Res (Model owner, String name, int "
                                + "sortOrder, long qCapacity, int capacity,	boolean "
                                + "showInReport, boolean showInTrace)",
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

            // construct a vector to hold all the UnUsedResources at the moment
            _unUsedResources = new List<Resource>();

            // construct a vector to hold the UsedResources (see the inner class)
            _arrayOfUsedResources = new List<UsedResources>();

            // get a reference to the resource database
            _resourceDB = owner.getExperiment().getResourceDB();

            this._limit = capacity;
            this._minimum = capacity;
            this._avail = capacity;
            this._users = 0;
            this._wSumAvail = 0.0;
            this._refused = 0;
            this._lastUsage = presentTime();

            if (capacity <= 0) // nothing or less in the resource pool, you fool!
            {
                sendWarning(
                        "Attempt to construct a Res with nothing or a negativ"
                                + " number of resources. Initial number of resources is set to one!",
                        "Res: "
                                + getName()
                                + " Constructor: Res (Model owner, String name, int sortOrder, "
                                + "long qCapacity, int capacity, boolean showInReport, "
                                + "boolean showInTrace)",
                        "A negative number of resources does not make sense here.",
                        "Make sure to initialize the capacity of a Res always with"
                                + " a positive number of resources.");

                _limit = _minimum = _avail = 1; // set it to 1, that makes more
                                                // sense
            }

            // make the resource objects and store them in the vector of unused
            // resources
            for (int i = 0; i < capacity; i++)
            {
                // make the resources and give them the name of the Res pool
                Resource aResource = new Resource(owner, name, this, true);

                _unUsedResources.Add(aResource);
            }
        }

        // ****** methods ******

        /// <summary>
        /// Constructor for a Res with a number of initial resources in it. The underlying queue has a Fifo
        /// queueing discipline and unlimited capacity.
        /// </summary>
        /// <param name="owner">The model this Res is associated to.</param>
        /// <param name="name">The Res's name</param>
        /// <param name="capacity">
        /// The number of resources the Res starts with. Must be positive and greater than 0.
        /// </param>
        /// <param name="showInReport">Flag, if Res should produce a report or not.</param>
        /// <param name="showInTrace">Flag for trace to produce trace messages.</param>
        public Res(Model owner, string name, int capacity, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {
            //super(owner, name, showInReport, showInTrace); // construct QueueBased

            _idNumber = resNumber++; // increment the resNumber and get it as
                                     // IDNumber

            // make an actual queue and give it a reference of this "QueueBased"-Res
            _queue = new QueueListFifo<SimProcess>();
            _queue.setQueueBased(this);

            // construct a vector to hold all the UnUsedResources at the moment
            _unUsedResources = new List<Resource>();

            // construct a vector to hold the UsedResources (see the inner class)
            _arrayOfUsedResources = new List<UsedResources>();

            // get a reference to the resource database
            _resourceDB = owner.getExperiment().getResourceDB();

            this._limit = capacity;
            this._minimum = capacity;
            this._avail = capacity;
            this._users = 0;
            this._wSumAvail = 0.0;
            this._refused = 0;
            this._lastUsage = presentTime();

            if (capacity <= 0) // nothing or less in the resource pool, you fool!
            {
                sendWarning(
                        "Attempt to construct a Res with nothing or a negativ"
                                + " number of resources. Initial number of resources is set to one!",
                        "Res: "
                                + getName()
                                + " Constructor: Res (desmoj.Model owner, String name, "
                                + "int capacity, boolean showInReport, boolean showInTrace)",
                        "A negative number of resources does not make sense here.",
                        "Make sure to initialize the capacity of a Res always with"
                                + " a positive number of resources.");

                _limit = _minimum = _avail = 1; // set it to 1, that makes more
                                                // sense
            }

            // make the resource objects and store them in the vector of unused
            // resources
            for (int i = 0; i < capacity; i++)
            {
                // make the resources and give them the name of the Res pool
                Resource aResource = new Resource(owner, name, this, true);

                _unUsedResources.Add(aResource);
            }
        }

        /// <summary>
        /// Activates the SimProcess <c>process</c>, given as a parameter of this method, as the next
        /// process. This process should be a SimProcess waiting in the queue for some resources.
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
		 * was trying to acquire resources, but there were not enough left in the
		 * Res. Or another process was first in the queue to be served. This method
		 * is called every time a process returns resources or when a process in the
		 * waiting-queue is satisfied.
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
        /// Returns the average usage of the Res. That means: in average, which percentage of the resources
        /// were in use over the time?
        /// </summary>
        /// <returns>the average usage of the resources in the Res.</returns>
        public double avgUsage()
        {
            TimeInstant now = presentTime(); // what is the time?

            // how long since the last reset
            var diff = TimeOperations.diff(now, resetAt());

            double wSumAvl = _wSumAvail
                    + ((double)_avail * TimeOperations.diff(now, _lastUsage)
                            .getTimeInEpsilon());
            if (TimeSpan.isEqual(diff, TimeSpan.ZERO)) // just reseted
            {
                sendWarning(
                        "A division by zero error occured.",
                        "Res: " + this.getName() + " Method: double avgUsage ()",
                        "The time difference between the last reset and now is zero.",
                        "Do not reset any model component at the same time the simulation is "
                                + "over or will be stopped.");

                return UNDEFINED; // see QueueBased: UNDEFINED = -1
            }

            // calculate the average usage
            double avgUsg = 1.0 - ((wSumAvl / diff.getTimeInEpsilon()) / _limit);
            // return the rounded average usage
            return StatisticObject.round(avgUsg);
        }

        /// <summary>
        /// Changes the limit of the available resources in the Res. Sets the number of the maximum available
        /// resources to m. m must be positive. This is only allowed as long as the Res has not been used or
        /// the Res has just been reset.
        /// </summary>
        /// <param name="m">The new limit (capacity) of the Res. Must be positive.</param>
        public void changeLimit(int m)
        {
            if (_limit != _minimum || _users != 0) // if Res is already used
            {
                sendWarning(
                        "Attempt to change the limit of a Res already"
                                + " in use. The limit will remain unchanged!",
                        "Res: " + this.getName()
                                + " Method: void changeLimit (long m)",
                        "The limit of a Res which has already be used can not"
                                + " be changed afterwards.",
                        "Do not try to change the limit of a Res which might have been"
                                + " used already. Or reset the Res before changing its limit.");

                return; // without changing the limit
            }

            if (m <= 0) // Trying to set the limit to 0 or a negative value.
            {
                sendWarning(
                        "Attempt to change the limit of a Res to zero"
                                + " or a negative number. The limit will remain unchanged!",
                        "Res: " + this.getName() + " Method: void changeLimit "
                                + "(long m)",
                        "The limit of a Res can not be set to zero or a negative"
                                + " number. That would make no sense.",
                        "Do not try to change the limit of a Res to negative "
                                + "or zero. Choose a positive integer instead.");

                return; // ignore this rubbish
            }

            // adjust the number of resources stored in the array of unused
            // resources
            if (m > _limit) // the limit is increasing
            {
                for (int i = _limit; i < m; i++)
                {
                    // make the resources and give them the name of the Res pool
                    Resource aResource = new Resource(getModel(), getName(), this,
                            true);
                    _unUsedResources.Add(aResource);
                }
            }

            if (m < _limit) // the limit is decreasing
                for (int i = m; i < _limit; i++)
                {
                    _unUsedResources.RemoveAt(i);
                }

            // set the limit and the minimum to the new value
            _limit = _minimum = _avail = m;

        }

        /// <summary>Checks whether the process using the Res is a valid process.</summary>
        /// <param name="p">Is this SimProcess a valid one?</param>
        /// <returns>Returns whether the SimProcess is valid or not.</returns>
        protected bool checkProcess(SimProcess p, string where)
        {
            if (p == null) // if p is a null pointer instead of a process
            {
                sendWarning("A non existing process is trying to use a Res "
                        + "object. The attempted action is ignored!", "Res: "
                        + getName() + " Method: " + where,
                        "The process is only a null pointer.",
                        "Make sure that only real SimProcesses are using Res's.");
                return false;
            }

            if (!isModelCompatible(p)) // if p is not modelcompatible
            {
                sendWarning(
                        "The process trying to use a Res object does"
                                + " not belong to this model. The attempted action is ignored!",
                        "Res: " + getName() + " Method: " + where,
                        "The process is not modelcompatible.",
                        "Make sure that processes are using only Res's within"
                                + " their model.");
                return false;
            }
            return true;
        }

        /// <summary>Returns a Reporter to produce a report about this Res.</summary>
        /// <returns>The Reporter for the queue inside this Res.</returns>
        protected override Reporter createDefaultReporter()
        {
            return new ResourceReporter(
                    this);
        }

        /**
		 * Turns the deadlock check off. So whenever a SimProcess can not get the
		 * resources desired, there won't be checked if a deadlock situation might
		 * have occured.
		 */
        public void deadlockCheckOff()
        {

            _deadlockCheck = false; // that's all

            // send a warning if the resource pool has been used already
            if (_limit != _minimum || _users != 0) // if Res is already used
            {
                sendWarning(
                        "The deadlock check for the resource pool: "
                                + this.getName() + " is turned off!",
                        "Res: " + this.getName()
                                + " Method: void deadlockCheckOff()",
                        "The deadlock check for this resource pool is turned off, but "
                                + "some resources are already in use.",
                        "Make sure, that you really want to turn the deadlock check off "
                                + " even after some resources have been used already.");
            }

            // for debugging purposes
            if (currentlySendDebugNotes())
            {
                sendDebugNote("The deadlock check for '" + getName()
                        + "' is turned " + "off now.");
            }
        }

        /**
		 * Turns the deadlock check on. So whenever a SimProcess can not get the
		 * resources desired, it will be checked if a deadlock situation might
		 * occur.
		 */
        public void deadlockCheckOn()
        {

            _deadlockCheck = true; // that's all

            // send a warning if the resource pool has been used already
            if (_limit != _minimum || _users != 0) // if Res is already used
            {
                sendWarning(
                        "The deadlock check for the resource pool: "
                                + this.getName()
                                + " is turned on. But some resources have been "
                                + "used already!",
                        "Res: " + this.getName()
                                + " Method: void deadlockCheckOn()",
                        "The deadlock check for this resource pool is turned on again, "
                                + "but some resources are already in use. So the deadlock check can "
                                + "not be performed correctly!",
                        "Make sure to turn the deadlock check on before the resources will "
                                + "be used.");
            }

            // for debugging purposes
            if (currentlySendDebugNotes())
            {
                sendDebugNote("The deadlock check for '" + getName()
                        + "' is turned " + "on again from now on.");
            }
        }

        /// <summary>
        /// Takes a number of n resources from the Res pool and delivers this array of resources to the
        /// Simprocess to use them. Is called from the method <c>provide (int n)</c>.
        /// </summary>
        /// <param name="n">
        /// The number of resources the resourcepool will <c> deliver()</c> to the SimProcess.
        /// </param>
        /// <returns>the array of resources which will be delivered to the SimProcess.</returns>
        private Resource[] deliver(int n)
        {
            SimProcess currentProcess = currentSimProcess();

            // get the resources from the unused resources pool
            Resource[] resArray = new Resource[n]; // make the array of resources

            // fill the array of resources
            for (int i = 0; i < n; i++)
            {
                // put first res in array
                resArray[i] = _unUsedResources[0];
                // delete first res
                _unUsedResources.Remove(_unUsedResources[0]);
            }

            // note which SimProcess is holding how many Resources
            updateProvidedRes(currentProcess, resArray);

            // for debugging purposes
            if (currentlySendDebugNotes())
            {
                // make a string including all elements of the array of provided
                // res.
                StringBuilder s = new StringBuilder();
                s.Append("delivers to SimProcess '" + currentProcess.getName() + "': ");

                for (int j = 0; j < n; j++)
                {
                    s.Append("<br>" + resArray[j].getName());
                }

                sendDebugNote(s.ToString());

                // make a string including all the resource that are left in the Res
                StringBuilder t = new StringBuilder();
                t.Append("In this Res pool are left: ");

                if (_unUsedResources.Count == 0) // anything left ?
                {
                    t.Append("<br>none");
                }

                foreach (var e in _unUsedResources)
                {
                    t.Append("<br>" + e.getName());
                }

                sendDebugNote(t.ToString());
            }

            return resArray; // return the array of resources
        }

        /// <summary>Returns the number of resources available in the pool at the moment.</summary>
        /// <returns>The number of resources available at the moment.</returns>
        public int getAvail()
        {
            return this._avail;
        }

        /// <summary>Returns if the deadlock check is enabled (<c>true</c>) or not ( <c>false</c>).</summary>
        /// <returns>
        /// <c>true</c> if the deadlock check is enabled, <c>false</c> if the deadlock check is not enabled
        /// </returns>
        public bool getDeadlockCheck()
        {

            return _deadlockCheck; // that's all
        }

        /// <summary>Returns the ID number of this <c>Res</c> object.</summary>
        /// <returns>The ID number of this <c>Res</c> object.</returns>
        public long getidNumber()
        {
            return _idNumber;
        }

        /// <summary>Returns the initial number of resources in the Res pool.</summary>
        /// <returns>The initial number of resources in the Res pool at the beginning.</returns>
        public int getLimit()
        {
            return this._limit;
        }

        /// <summary>Returns the minimum number of resources in the Res.</summary>
        /// <returns>The minimum number of resources in the Res.</returns>
        public int getMinimum()
        {
            return this._minimum;
        }

        /// <summary>
        /// Returns whether entities can pass by other entities which are enqueued before them in the queue.
        /// </summary>
        /// <returns>
        /// Indicates whether entities can pass by other entities which are enqueued before them in the
        /// queue.
        /// </returns>
        public bool getPassBy()
        {
            return _passBy;
        }

        /// <summary>
        /// Returns the <c>QueueList</c> actually storing the <c>SimProcesses</c> waiting for resources.
        /// </summary>
        /// <returns>the queue actually storing the <c>SimProcesses</c> waiting for resources.</returns>
        public QueueList<SimProcess> getQueue()
        {

            return _queue; // that's it
        }

        /// <summary>
        /// Returns the implemented queueing discipline of the underlying queue as a String, so it can be
        /// displayed in the report.
        /// </summary>
        /// <returns>The String indicating the queueing discipline.</returns>
        public String getQueueStrategy()
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

        /// <summary>Returns the number of users.</summary>
        /// <returns>The number of Users. That are processes having acquired and released resources.</returns>
        public long getUsers()
        {
            return this._users;
        }

        /// <summary>Returns the number of resources held by the given SimProcess at this time.</summary>
        /// <param name="sProc">The SimProcess which is expected to hold some Resources.</param>
        /// <returns>The number of resources held by the given SimProcess at this time.</returns>
        protected int heldResources(SimProcess sProc)
        {
            int j = 0; // to count the resources held

            for (int i = 0; i < _arrayOfUsedResources.Count; i++)
            {
                // get hold of the UsedResources pair (SimProcess/number of
                // resources)
                UsedResources procHoldRes = _arrayOfUsedResources[i];

                if (procHoldRes.getProcess() == sProc)
                {
                    j += procHoldRes.getOccupiedResources().Count;
                } // end if
            } // end for

            return j; // all the resources the SimProcess holds at the moment
        }

        /// <summary>Returns <c>true</c> if a deadlock is detected, <c>false</c> otherwise.</summary>
        /// <returns>is <c>true</c> if a deadlock is detected, <c>false</c> otherwise.</returns>
        public bool isDeadlockDetected()
        {

            return _deadlockDetected; // that's it
        }

        /// <summary>
        /// Gets a number of n resources from the Res pool and provides them to the Sim-process to use them.
        /// Hint for developers: calls the private method <c>deliver()</c>. As not enough resources are
        /// available at the moment the SimProcess has to wait in a queue until enough products are available
        /// again.
        /// </summary>
        /// <param name="n">
        /// The number of resources the resourcepool will <c> provide()</c> to the SimProcess.
        /// </param>
        /// <returns>
        /// Is <c>true</c> if the specified number of resources have been provided successfully, <c>false</c>
        /// otherwise (i.e. capacity limit of the queue is reached).
        /// </returns>
        /// <summary>
        /// Creates a <see cref="ResLease"/> for <paramref name="n"/> units.
        /// Pass the lease to <see cref="ORTools4AI.Simulator.core.engine.SimProcess.Acquire"/>
        /// inside a <c>Run()</c> override to block until the units are available,
        /// then dispose the lease to release them.
        /// </summary>
        public ResLease Request(int n = 1) => new ResLease(this, n);

        public bool provide(int n)
        {
            _where = " boolean provide (int n)";

            SimProcess currentProcess = currentSimProcess();

            if (!checkProcess(currentProcess, _where)) // if the current process
            {
                return false;
            } // is not valid: return

            if (n <= 0) // trying to provide nothing or less
            {
                sendWarning(
                        "Attempt from a Res to provide nothing or a negative "
                                + "number of resources . The attempted action is ignored!",
                        "Res: " + getName() + " Method: provide (int n)",
                        "It does not make sense to provide nothing or a negative number "
                                + "of resources. The statistic will be corrupted with negative numbers!",
                        "Make sure to provide at least one resource from the Res.");

                return false; // ignore that rubbish
            }

            // total of resources acquired and already held by the current
            // SimProcess
            int total = n + heldResources(currentProcess);

            if (total > _limit) // trying to provide (in total) more than the
            { // capacity of the Res
                sendWarning(
                        "Attempt from a Res to provide more resources than its "
                                + "capacity holds. The attempted action is ignored!",
                        "Res: " + getName() + " Method: provide (int n)",
                        "The requested resources [" + total
                                + "] could never be provided by the Res"
                                + ", because the capacity of this Res [" + _limit
                                + "] is not that big. <br>"
                                + "Therefore the process '"
                                + currentProcess.getName() + "' might be blocked "
                                + "for ever.",
                        "Make sure never to let the Res provide more resources than its "
                                + "capacity.");

                return false; // ignore that rubbish
            }

            // Initial entry: capacity check and queue insert only on first call.
            // On re-entry after yield (isBlocked==true), the process is already queued.
            if (!currentProcess.isBlocked())
            {
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

                // insert every process in the queue for statistical reasons
                _queue.insert(currentProcess);
            }

            // is it possible for this process to pass by?
            if (_passBy == false)
            {
                // see if the SimProcess can be satisfied or has to wait in the
                // queue
                if (n > _avail || // not enough resources available OR
                        currentProcess != _queue.first()) // other process is
                                                          // first
                                                          // in the q
                {
                    // Only emit trace/debug and note the request on first wait entry.
                    if (!currentProcess.isBlocked())
                    {
                        if (currentlySendTraceNotes())
                        {
                            sendTraceNote("awaits " + n + " of ' " + this.getName() + " '");
                        }
                        if (currentlySendDebugNotes())
                        {
                            sendDebugNote("has not enough resources left to provide "
                                    + n + " unit(s) to '" + currentProcess.getName() + "'");
                        }
                        if (getDeadlockCheck())
                        {
                            _resourceDB.noteResourceRequest(currentProcess, this, n);
                            _deadlockDetected = _resourceDB.checkForDeadlock(currentProcess);
                        }
                    }

                    // Suspend: caller must do `while (!provide(n)) yield return RunStatus.passivate`
                    currentProcess.setBlocked(true);
                    currentProcess.skipTraceNote();
                    return false;

                } // end if — resources not yet available

                // Resources are available and process is first: clean up deadlock tracking
                if (getDeadlockCheck() && currentProcess.isBlocked())
                {
                    _resourceDB.deleteResRequest(currentProcess, this, n);
                }
            }
            else // the process can pass by other processes in the queue, passBy
                 // =
                 // true
            {
                if (n > _avail || // not enough resources available OR
                        currentProcess != _queue.first()) // other process is
                                                          // first
                                                          // in the q
                {
                    // is the current process the first in the queue?
                    if (currentProcess != _queue.first()) // no it's not the first
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

                    // only if not enough units are available the process has to wait
                    if (n > _avail)
                    {
                        if (!currentProcess.isBlocked())
                        {
                            if (currentlySendTraceNotes())
                            {
                                sendTraceNote("awaits " + n + " of ' " + this.getName() + " '");
                            }
                            if (currentlySendDebugNotes())
                            {
                                sendDebugNote("has not enough resources left to provide "
                                        + n + " unit(s) to '" + currentProcess.getName() + "'");
                            }
                            if (getDeadlockCheck())
                            {
                                _resourceDB.noteResourceRequest(currentProcess, this, n);
                                _deadlockDetected = _resourceDB.checkForDeadlock(currentProcess);
                            }
                        }

                        // Suspend: caller must do `while (!provide(n)) yield return RunStatus.passivate`
                        currentProcess.setBlocked(true);
                        currentProcess.skipTraceNote();
                        activateAsNext(_queue.succ(currentProcess));
                        return false;
                    }

                } // end outer if (n > _avail || ...)

                // Resources are available: clean up deadlock tracking if we were waiting
                if (getDeadlockCheck() && currentProcess.isBlocked())
                {
                    _resourceDB.deleteResRequest(currentProcess, this, n);
                }
            } // end else (passBy = true)

            // the current process has got the resources he wanted ...

            // the Res provides all the resources the SimProcess wants
            _queue.remove(currentProcess); // get the process out of the queue
            currentProcess.setBlocked(false); // we are not blocked (anymore),
                                              // yeah!

            // give the new first process in the queue a chance
            activateFirst();

            // hand the resources over to the SimProcess
            currentProcess.obtainResources(deliver(n));

            updateStatistics(-n); // statistics will be updated

            // check for deadlock?
            if (getDeadlockCheck())
            {
                // update the resourceDB: resources are assigned to the SimProcess
                // now
                _resourceDB.noteResourceAllocation(this, currentProcess, n);
            }

            if (currentlySendTraceNotes())
            {
                sendTraceNote("seizes " + n + " from " + this.getQuotedName());
            } // tell in the trace what the process is taking from the resources

            // a debug message is generated in the method deliver(), see some lines
            // above

            return true;
        }

        /// <summary>
        /// Resets the statistics of this Res. The number of available resources at this moment and the
        /// processes waiting in the queue are not changed. But all statistic counters are reset. The parent
        /// <c>QueueBased</c> is also reset.
        /// </summary>
        public void reset()
        {
            base.reset(); // reset the QueueBased also

            _minimum = _limit; // not quite correct, but needed for changeLimit()
            _users = 0;
            _wSumAvail = 0.0;
            _refused = 0;
            _lastUsage = presentTime();
        }

        /// <summary>
        /// Sets the boolean field <c>deadlockDetected</c> to the given value. If a deadlock for this
        /// <c>Res</c> is detected when an unsuccessfull seize statement for a resource has taken place, then
        /// the value of <c>deadlockDetected</c> will be set to <c>true</c>. The value will also been shown
        /// in the report of this <c>Res</c>.
        /// </summary>
        /// <param name="dlDetected">
        /// the new value for the field <c>deadlockDetected</c>. Should be <c>true</c> if this <c>Res</c> is
        /// involved in a deadlock.
        /// </param>
        public void setDeadlockDetected(bool dlDetected)
        {

            _deadlockDetected = dlDetected; // that's all
        }

        /// <summary>
        /// Sets the flag passBy to a new value. PassBy is indicating whether entities can pass by other
        /// entities which are enqueued before them in the queue.
        /// </summary>
        /// <param name="newPassBy">
        /// The new value of passBy. Set it to <c>true</c> if you want entities to pass by other entities
        /// which are enqueued before them in the queue. Set it to <c>false</c> if you don't want entities to
        /// overtake other entities in the queue.
        /// </param>
        public void setPassBy(bool newPassBy)
        {
            this._passBy = newPassBy; // that's all!
        }

        /// <summary>
        /// A process is using this method to put resources it has used back in the Res pool. The process can
        /// not put more resources back than it has acquired once. The array of returning resources can be
        /// provided by the method <c>returnResources()</c> of the class <c>SimProcess</c>.
        /// </summary>
        /// <param name="returnedRes">
        /// The array of resources a process is returning to the resource pool. Can't be more resources than
        /// it once has acquired!
        /// </param>
        public void takeBack(Resource[] returnedRes)
        {
            _where = "void takeBack (Resource[] returnedRes)	";

            SimProcess currentProcess = currentSimProcess();

            if (!checkProcess(currentProcess, _where)) // check the current process
            {
                return;
            } // if it is not valid just return

            if (returnedRes.Length <= 0) // if the process is releasing nothing
            {
                sendWarning(
                        "The array of returned resources is empty! "
                                + "The attempted action is ignored!",
                        "Res: " + this.getName()
                                + " Method: void takeBack (Resource[] "
                                + "returnedRes)",
                        "It makes no sense to take back an empty array of resources.",
                        "Make sure to return at least one resource to the Res pool.");

                return; // go to where you came from
            }

            // the process is trying to release more resources than it holds
            if (returnedRes.Length > heldResources(currentProcess))
            {
                sendWarning(
                        "Attempt to make the Res take back more resources than "
                                + "the process is holding at the moment. The attempted action is "
                                + "ignored!", "Res: " + this.getName()
                                + " Method: void takeBack (Resource[] "
                                + "returnedRes)",
                        "A process can not release more resources than it holds.",
                        "Make sure not to take back more resources than the process is holding.");

                return; // go to where you came from
            }

            // put the used resources back in the unused resources pool
            for (int i = 0; i < returnedRes.Length; i++)
            {
                _unUsedResources.Add(returnedRes[i]);
            }

            // update which SimProcess is holding which Resources
            updateTakenBackRes(currentProcess, returnedRes);

            updateStatistics(returnedRes.Length); // statistics will be updated
            _users++; // update users

            // update the resource database / check for deadlock?
            if (getDeadlockCheck())
            {
                _resourceDB.deleteResAllocation(this, currentProcess,
                        returnedRes.Length);
            }

            // tell in the trace what the process is returning to the Res pool
            if (currentlySendTraceNotes())
            {
                sendTraceNote("releases " + returnedRes.Length + " to "
                        + this.getQuotedName());
            }

            // for debugging purposes
            if (currentlySendDebugNotes())
            {
                // make a string including all elements of the array of returned
                // res.
                StringBuilder s = new StringBuilder();
                s.Append("SimProcess '" + currentProcess.getName() + "' <b>returns</b>: ");

                for (int j = 0; j < returnedRes.Length; j++)
                {
                    s.Append("<br>" + returnedRes[j].getName());
                }

                sendDebugNote(s.ToString());
            }

            activateFirst(); // give the new first process in the queue a chance
        }

        /// <summary>
        /// A process is using this method to put resources it has used back in the Res pool. The process can
        /// not put more resources back than it has acquired once. This method can be used as an alternative
        /// to the method <c>takeBack(Resource[] returnedRes)</c> in cases that the user does not want to
        /// provide an array of returning resources. This method is also compatible with older DESMO-J
        /// Versions.
        /// </summary>
        /// <param name="n">
        /// The number of resources which should be returned to the Res pool. Can't be more than once were
        /// acquired!
        /// </param>
        public void takeBack(int n)
        {
            _where = "void takeBack (int n) ";

            SimProcess currentProcess = currentSimProcess();

            if (!checkProcess(currentProcess, _where)) // check the current process
            {
                return;
            } // if it is not valid just return

            if (n <= 0) // if the process is releasing nothing
            {
                sendWarning(
                        "The number of returned resources is negative or zero! "
                                + "The attempted action is ignored!",
                        "Res: " + this.getName() + " Method: void takeBack (int n)",
                        "It makes no sense to take back nothing or a negative number of "
                                + "resources.",
                        "Make sure to return at least one resource to the Res pool.");

                return; // go to where you came from
            }

            // the process is trying to release more resources than it holds
            if (n > heldResources(currentProcess))
            {
                sendWarning("Attempt to make the Res take back more resources ["
                        + n + "] than the process '" + currentProcess.getName()
                        + "' is holding at the " + "moment ["
                        + heldResources(currentProcess) + "]. <br>"
                        + "The attempted action is ignored!", "Res: "
                        + this.getName() + " Method: void takeBack (int n)",
                        "A process can not release more resources than it holds.",
                        "Make sure not to bring back more resources than the process is holding.");

                return; // go to where you came from
            }

            // get the array of returned resources from the SimProcess
            Resource[] returnedRes = currentProcess.returnResources(this, n);

            // put the used resources back in the unused resources pool
            for (int i = 0; i < n; i++)
            {
                _unUsedResources.Add(returnedRes[i]);
            }

            // update which SimProcess is holding which Resources
            updateTakenBackRes(currentProcess, returnedRes);

            updateStatistics(n); // statistics will be updated
            _users++; // update users

            // update the resource database / check for deadlock?
            if (getDeadlockCheck())
            {
                _resourceDB.deleteResAllocation(this, currentProcess, n);
            }

            // tell in the trace what the process is returning to the Res pool
            if (currentlySendTraceNotes())
            {
                sendTraceNote("releases " + n + " to " + this.getQuotedName());
            }

            // for debugging purposes
            if (currentlySendDebugNotes())
            {
                // make a string including all elements of the array of returned
                // res.
                var s = new StringBuilder();
                s.Append("SimProcess '" + currentProcess.getName() + "' <b>returns</b>: ");

                for (int j = 0; j < returnedRes.Length; j++)
                {
                    s.Append("<br>" + returnedRes[j].getName());
                }

                sendDebugNote(s.ToString());
            }

            activateFirst(); // give the new first process in the queue a chance
        }

        /**
		 * 
		 * Muss durch eine andere Methode in SimProcess ersetzt werden ???
		 * 
		 * Soenke ????
		 * 
		 * 
		 * A process is using this method to return all the resources it holds to
		 * the Res pool. The process can not put more resources back than it has
		 * acquired once. This method can be used if a Process is about to be
		 * terminated.
		 * 
		 * public void takeBackAll () { where = "void takeBackAll ()";
		 * 
		 * int n = 0; // how many resources will be taken back
		 * 
		 * Sim-process currentProcess = currentSimProcess();
		 * 
		 * if (!checkProcess(currentProcess, where)) //check the current process {
		 * return; } // if it is not valid just return // delete the entry of the
		 * currentSimProcess in the arrayOfUsedResources // search the whole vector
		 * for ( int i = 0; i < arrayOfUsedResources.size(); i++) { // get hold of
		 * the UsedResources pair (SimProcess/number of resources) UsedResources
		 * procHoldRes = (UsedResources)arrayOfUsedResources.elementAt(i);
		 * 
		 * if (procHoldRes.getProcess() == currentProcess) { // number of resources
		 * held by the currentProcess n = procHoldRes.getOccupiedResources ();
		 * 
		 * arrayOfUsedResources.removeElementAt(i); // delete the entry } } // end
		 * for
		 * 
		 * updateStatistics ( n ); // statistics will be updated
		 * 
		 * users++; // update users // tell in the trace that the process is
		 * releasing all its resources if ( traceIsOn() ) { sendTraceNote (
		 * "releases all its " + n + " " + this.getName() ); }
		 * 
		 * activateNext(); // give waiting process in the queue a chance }
		 */

        /**
		 * Updates the arrayOfUsedResources for this Res whenever resources are
		 * <code>provided</code>.
		 * 
		 * @param crntProcess
		 *            SimProcess : The current SimProcess acquiring resources.
		 * @param providedRes
		 *            Resource[] : The array of resources the Res is providing to
		 *            the current SimProcess.
		 */
        protected void updateProvidedRes(SimProcess crntProcess,
                Resource[] providedRes)
        {
            // is the SimProcess already holding resources?
            bool holdsResources = false; // not yet

            // search the whole vector
            for (int i = 0; i < _arrayOfUsedResources.Count; i++)
            {
                // get hold of the UsedResources pair (SimProcess/number of
                // resources)
                UsedResources procHoldRes = _arrayOfUsedResources[i];

                // is the SimProcess already holding resources?
                if (procHoldRes.getProcess() == crntProcess)
                {
                    // update the held resources of the current SimProcess
                    for (int j = 0; j < providedRes.Length; j++)
                    {
                        procHoldRes.getOccupiedResources().Add(
                                providedRes[j]);
                    }

                    holdsResources = true; // the SimProcess already holds
                                           // resources
                } // end if
            } // end for

            if (!holdsResources) // the process does not hold any resources yet
            {
                // make a new Vector
                var occupiedRes = new List<Resource>();

                // copy all elements of the array to the Vector
                for (int i = 0; i < providedRes.Length; i++)
                {
                    occupiedRes.Add(providedRes[i]);
                }

                // construct a new UsedResources object with the Vector
                UsedResources ur = new UsedResources(crntProcess, occupiedRes);

                // put ur in the arrayOfUsedResources
                _arrayOfUsedResources.Add(ur);
            }
        }

        /// <summary>
        /// Updates the statistics for the Res whenever resources are <c>provided</c> or <c>"takenBack"</c>.
        /// </summary>
        /// <param name="n">
        /// Is positive when the Res <c>takeBack()</c> resources and negative when the Res <c>provides()</c>
        /// resources.
        /// </param>
        protected void updateStatistics(int n)
        {
            TimeInstant now = presentTime();

            _wSumAvail = _wSumAvail
                    + ((double)_avail * TimeOperations.diff(now, _lastUsage)
                            .getTimeInEpsilon());
            _lastUsage = now;

            _avail += n; // n can be positive or negative (remember ?!)

            if (_avail < _minimum) // update minimum, if necessary
            {
                _minimum = _avail;
            }
        }

        /// <summary>
        /// Updates the arrayOfUsedResources for this Res whenever resources are taken back.
        /// </summary>
        /// <param name="crntProcess">The current SimProcess releasing resources.</param>
        /// <param name="returnedRes">
        /// The array of resources the Res will take back from the current SimProcess.
        /// </param>
        protected void updateTakenBackRes(SimProcess crntProcess,
                Resource[] returnedRes)
        {
            // search the whole vector
            for (int i = 0; i < _arrayOfUsedResources.Count; i++)
            {
                // get hold of the UsedResources pair (SimProcess/number of
                // resources)
                UsedResources procHoldRes = _arrayOfUsedResources[i];

                if (procHoldRes.getProcess() == crntProcess)
                {
                    // remove the resources from the Vector of used resources
                    for (int j = 0; j < returnedRes.Length; j++)
                    {
                        procHoldRes.getOccupiedResources().Remove(
                                returnedRes[j]);
                    }

                    // are all resources from this SimProcess taken back
                    if (procHoldRes.getOccupiedResources().Count == 0)
                    {
                        _arrayOfUsedResources.RemoveAt(i);
                    }
                } // end if
            } // end for
        }
    } // end class Res



    public class UsedResources
    {

        // ****** attributes of inner class ******

        /**
		 * The SimProcess using the resources at the moment.
		 */
        private SimProcess process;

        /**
         * The array of resources occupied by the SimProcess. In fact the array
         * is a java.util.Vector because one does not know how many resources
         * the SimProcess holds and the number of held resources might vary.
         */
        private List<Resource> occupiedResources;

        // ****** methods of inner class ******

        /// <summary>Constructor for a UsedResources object.</summary>
        /// <param name="sProc">The SimProcess holding the resources.</param>
        /// <param name="occupiedRes">The resources occupied by the SimProcess.</param>
        public UsedResources(SimProcess sProc, List<Resource> occupiedRes)
        {
            // init variables
            this.process = sProc;
            this.occupiedResources = occupiedRes;
        }

        /// <summary>Returns the SimProcess which holds a number of resources.</summary>
        /// <returns>The SimProcess which holds a number of resources.</returns>
        public SimProcess getProcess()
        {
            return this.process;
        }

        /// <summary>Returns the array of resources occupied by the SimProcess.</summary>
        /// <returns>The array of resources occupied by the SimProcess.</returns>
        public List<Resource> getOccupiedResources()
        {
            return this.occupiedResources;
        }

        /// <summary>
        /// Sets the array of resources occupied by the SimProcess to the given array
        /// <c>newArrayOfOccupiedResources</c>.
        /// </summary>
        /// <param name="newArrayOfOccupiedResources">
        /// The new array of resources held by the SimProcess.
        /// </param>
        protected void setOccupiedResources(List<Resource> newArrayOfOccupiedResources)
        {
            this.occupiedResources = newArrayOfOccupiedResources;
        }


    } // end inner class

}