// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures.report;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using System.ComponentModel;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures
{

    /**
     * Stock is the place where a certain kind of product can be stored by producers
     * and retrieved by consumers. There is no difference between the units of one
     * kind of product, the units are indistinguishable. Producers are producing
     * products and store them using <code>store()</code> in the Stock. Consumers
     * make the Stock <code>retrieve()</code> the products to use them up. A Stock
     * is also providing a process synchronization between producers and consumers.
     * If no or not enough units of a product are available for the consumers, they
     * have to wait in a queue until new units are delivered by a producer. The
     * Stock has a certain capacity. If the Stock is filled to it's capacity
     * producers have to wait in a queue until consumers are arriving and taking
     * units out of the Stock. Then the producers can fill up the freed space. This
     * is the major difference to the Bin. The first sort criteria of the queues is
     * always highest priorities first, the second queueing discipline of the
     * underlying queues and the capacity limit of every queue can be determined by
     * the user (default is FIFO and unlimited capacity). Stock is derived from
     * QueueBased, which provides all the statistical functionality for the consumer
     * queue. The producer queue is managed by an internal ProcessQueue.
     * 
     * @see QueueBased
     * @see Bin
     */
    public class Stock : QueueBased {

        /**
         * The initial number of units in the Stock
         */
        private long _fieldInitial = 0;

        /**
         * The maximum number of units in the Stock
         */
        private long _fieldMaximum = 0;

        /**
         * The minimum number of units in the Stock
         */
        private long _fieldMinimum = 0;

        /**
         * Number of units available at the moment
         */
        private long _fieldAvail = 0;

        /**
         * The capacity of this stock. That is the number of units this stock can
         * hold at max. If set to zero the capacity is almost unlimited (=
         * 9223372036854775807).
         */
        private long _fieldCapacity;

        /**
         * The number of producers having used this Stock to store products
         */
        private long _fieldProducers = 0;

        /**
         * The number of consumers having retrieved products from this Stock
         */
        private long _fieldConsumers = 0;

        /**
         * The time in the simultion this Stock was used for last.
         */
        private TimeInstant _lastUsage;

        /**
         * The queue, actually storing the consumer processes waiting for products
         */
        protected QueueList<SimProcess> _consumerQueue;

        /**
         * Indicates the method where something has gone wrong. Is passed as a
         * parameter to the method <code>checkProcess()</code>.
         */
        private string _fieldWhere;

        /**
         * Weighted sum of available units in the Stock over the time (must be
         * divided by the total time to get the average available units!)
         */
        private double _wSumAvail = 0.0;

        protected PropertyChangeSupport propertyChange;

        /**
         * Counter for the consumer SimProcesses which are refused to be enqueued,
         * because the queue capacity is full.
         */
        private long _fieldRefused = 0;

        /**
         * The queue, actually storing the producer processes waiting to store their
         * units in the stock
         */
        protected ProcessQueue<SimProcess> _producerQueue;

        /**
         * Flag to indicate whether an entity can pass by other entities which are
         * enqueued before that entity in the producer queue. Is <code>false</code>
         * as default value.
         */
        private bool _passByProds = false;

        /**
         * Flag to indicate whether an entity can pass by other entities which are
         * enqueued before that entity in the consumer queue. Is <code>false</code>
         * as default value.
         */
        private bool _passByCons = false;

        /// <summary>
        /// Constructor for a Stock with an initial number of units and a certain capacity; the queueing
        /// discipline and the capacity limit of the underlying queues for the producers and consumers can be
        /// chosen, too. Highest priority are always first in the queue.
        /// </summary>
        /// <param name="owner">The model this Stock is associated to.</param>
        /// <param name="name">
        /// The Stock's name, should indicate the kind of product stored in this Stock.
        /// </param>
        /// <param name="prodSortOrder">
        /// determines the sort order of the underlying queue implementation for the producers. Choose a
        /// constant from <c>QueueBased</c> like <c>QueueBased.FIFO</c> or <c>QueueBased.LIFO</c> or ...
        /// </param>
        /// <param name="prodQCapacity">
        /// The capacity of the producers queue, that is how many processes can be enqueued. Zero (0) means
        /// unlimited capacity.
        /// </param>
        /// <param name="consSortOrder">
        /// determines the sort order of the underlying queue implementation for the consumers. Choose a
        /// constant from <c>QueueBased</c> like <c>QueueBased.FIFO</c> or <c>QueueBased.LIFO</c> or ...
        /// </param>
        /// <param name="consQCapacity">
        /// The capacity of the consumers queue, that is how many processes can be enqueued. Zero (0) means
        /// unlimited capacity.
        /// </param>
        /// <param name="initialUnits">The units of a product the Stock starts with. Must be positive!</param>
        /// <param name="capacity">The maximum capacity of products this Stock can hold.</param>
        /// <param name="showInReport">Flag, if this Stock should produce a report or not.</param>
        /// <param name="showInTrace">Flag for trace to produce trace messages.</param>
        public Stock(Model owner, string name, int prodSortOrder,
                int prodQCapacity, int consSortOrder, int consQCapacity,
                long initialUnits, long capacity, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)

        {
            //super(owner, name, showInReport, showInTrace); // construct QueueBased

            // check the parameters for the producer queue
            // make a copy of them
            int pSortOrder = prodSortOrder;
            int pQCapacity = prodQCapacity;

            // check if a valid sortOrder is given
            if (prodSortOrder < 0 || prodSortOrder >= 3) {
                sendWarning(
                        "The given prodSortOrder parameter is negative or "
                                + "to big! "
                                + "A queue with Fifo sort order will be created instead.",
                        "Stock : "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, int "
                                + "prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A valid positive integer number must be provided to "
                                + "determine the sort order of the underlying queue.",
                        "Make sure to provide a valid positive integer number "
                                + "by using the constants in the class QueueBased, like "
                                + "QueueBased.FIFO or QueueBased.LIFO.");
                // make a Fifo queue
                pSortOrder = QueueBased.FIFO; // better than nothing
            }

            // check if it the queue capacity does make sense
            if (prodQCapacity < 0) {
                sendWarning(
                        "The given capacity of the producer queue is negative! "
                                + "A queue with unlimited capacity will be created instead.",
                        "Stock : "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, int "
                                + "prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A negative capacity for a queue does not make sense.",
                        "Make sure to provide a valid positive capacity "
                                + "for the underlying queue.");
                // set the capacity to the maximum value
                pQCapacity = int.MaxValue;
            }

            // create the queue for the producers
            _producerQueue = new ProcessQueue<SimProcess>(owner, name + "_P", pSortOrder,
                    pQCapacity, false, false);
            // add the underlying QueueList* of the producer queue to the
            // PropertyChangeListeners
            addPropertyChangeListener("avail", _producerQueue.getQueueList());

            // check the parameters for the consumer queue
            // check if a valid sortOrder is given
            switch (consSortOrder) {
                case QueueBased.FIFO:
                    _consumerQueue = new QueueListFifo<SimProcess>(); break;
                case QueueBased.LIFO:
                    _consumerQueue = new QueueListLifo<SimProcess>(); break;
                case QueueBased.RANDOM:
                    _consumerQueue = new QueueListRandom<SimProcess>(); break;
                default:
                    sendWarning(
                            "The given consSortOrder parameter " + consSortOrder + " is not valid! "
                                    + "A queue with Fifo sort order will be created instead.",
                            "Stock : "
                                    + getName()
                                    + " Constructor: Stock(Model owner, String name, int "
                                    + "prodSortOrder, long prodQCapacity, int consSortOrder, "
                                    + "long consQCapacity, long initialUnits, long capacity, "
                                    + "boolean showInReport, boolean showInTrace)",
                            "A valid positive integer number must be provided to "
                                    + "determine the sort order of the underlying queue.",
                            "Make sure to provide a valid positive integer number "
                                    + "by using the constants in the class QueueBased, like "
                                    + "QueueBased.FIFO or QueueBased.LIFO.");
                    _consumerQueue = new QueueListFifo<SimProcess>();
                    break;
            }
            // give the QueueList a reference to this QueueBased
            _consumerQueue.setQueueBased(this);

            // add the QueueList* to the PropertyChangeListeners
            addPropertyChangeListener("avail", _consumerQueue);

            // set the capacity of the queue
            queueLimit = consQCapacity;

            // check if it the capacity does make sense
            if (consQCapacity < 0) {
                sendWarning(
                        "The given capacity of the consumer queue is negative! "
                                + "A queue with unlimited capacity will be created instead.",
                        "Stock : "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, int "
                                + "prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A negative capacity for a queue does not make sense.",
                        "Make sure to provide a valid positive capacity "
                                + "for the underlying queue.");
                // set the capacity to the maximum value
                queueLimit = int.MaxValue;
            }

            // check if qCapacity is zero (that means unlimited capacity)
            if (consQCapacity == 0) {
                // set the capacity to the maximum value
                queueLimit = int.MaxValue;
            }

            // set the capacity of the Stock
            _fieldCapacity = capacity;

            if (capacity == 0)
                _fieldCapacity = long.MaxValue;

            if (capacity < 0) {
                sendWarning(
                        "Attempt to construct a Stock with a negativ capacity."
                                + " The capacity will be converted to the positive value!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "int prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A negative capacity does not make sense for a stock.",
                        "Make sure to initialize a Stock always with a positive capacity.");

                // set it to the positive value of capacity
                _fieldCapacity = Math.Abs(capacity);
            }

            // set the number of initial units
            _fieldInitial = initialUnits;

            if (initialUnits < 0) // there can't be less than nothing
            {
                sendWarning(
                        "Attempt to construct a Stock with a negativ number of"
                                + " units. Initial number of units will be set to zero!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "int prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A negative number of units does not make sense here.",
                        "Make sure to initialize a Stock always with a positive number of "
                                + "initialUnits.");

                // set it to 0, that makes more sense
                _fieldInitial = 0;
            }

            // check if there should be more units stored than the capacity can hold
            if (_fieldInitial > _fieldCapacity) {
                sendWarning(
                        "Attempt to construct a Stock with initially more units"
                                + " in stock than the capacity can hold. The capacity will be increased "
                                + "to hold all the initial units!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "int prodSortOrder, long prodQCapacity, int consSortOrder, "
                                + "long consQCapacity, long initialUnits, long capacity, "
                                + "boolean showInReport, boolean showInTrace)",
                        "A capacity lower than the initial number of units in the stock does not "
                                + "make sense.",
                        "Make sure to initialize a Stock always with a capacity greater or equal "
                                + "to the initial number of stored units.");

                // set the capacity to the initial stock
                _fieldCapacity = _fieldInitial;
            }

            // set the number of available and maximum units, so far
            _fieldAvail = _fieldMaximum = _fieldMinimum = _fieldInitial;

            // set the statistics
            this._wSumAvail = 0.0;
            this._lastUsage = presentTime();
            _fieldProducers = _fieldConsumers = 0;
            _fieldRefused = 0;
        }

        /// <summary>Constructor for a Stock with an initial number of units and a certain capacity.</summary>
        /// <param name="owner">The model this Stock is associated to.</param>
        /// <param name="name">
        /// The Stock's name, should indicate the kind of product stored in this Stock.
        /// </param>
        /// <param name="initialUnits">The units of a product the Stock starts with. Must be positive!</param>
        /// <param name="capacity">The maximum capacity of products this Stock can hold.</param>
        /// <param name="showInReport">Flag, if this Stock should produce a report or not.</param>
        /// <param name="showInTrace">Flag for trace to produce trace messages.</param>
        public Stock(Model owner, string name, long initialUnits, long capacity,
                bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace) {

            //super(owner, name, showInReport, showInTrace); // construct QueueBased

            // make the actual consumer queue and
            // give it a reference to this "QueueBased"-Stock
            _consumerQueue = new QueueListFifo<SimProcess>();
            _consumerQueue.setQueueBased(this);

            // add the QueueList* to the PropertyChangeListeners
            addPropertyChangeListener("avail", _consumerQueue);

            // make the queue for the producers
            _producerQueue = new ProcessQueue<SimProcess>(owner, name + "_P", false, false);

            // add the underlying QueueList* of the producer queue to the
            // PropertyChangeListeners
            addPropertyChangeListener("avail", _producerQueue.getQueueList());

            // set the capacity
            _fieldCapacity = capacity;

            if (capacity == 0)
                _fieldCapacity = long.MaxValue;

            if (capacity < 0) {
                sendWarning(
                        "Attempt to construct a Stock with a negativ capacity."
                                + " The capacity will be converted to the positive value!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "long initialUnits, long capacity, boolean showInReport, "
                                + "boolean showInTrace)",
                        "A negative capacity does not make sense for a stock.",
                        "Make sure to initialize a Stock always with a positive capacity.");

                // set it to the positive value of capacity
                _fieldCapacity = Math.Abs(capacity);
            }

            // set the number of initial units
            _fieldInitial = initialUnits;

            if (initialUnits < 0) // there can't be less than nothing
            {
                sendWarning(
                        "Attempt to construct a Stock with a negativ number of"
                                + " units. Initial number of units will be set to zero!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "long initialUnits, long capacity, boolean showInReport, "
                                + "boolean showInTrace)",
                        "A negative number of units does not make sense here.",
                        "Make sure to initialize a Stock always with a positive number of "
                                + "initialUnits.");

                // set it to 0, that makes more sense
                _fieldInitial = 0;
            }

            // check if there should be more units stored than the capacity can hold
            if (_fieldInitial > _fieldCapacity) {
                sendWarning(
                        "Attempt to construct a Stock with initially more units"
                                + " in stock than the capacity can hold. The capacity will be increased "
                                + "to hold all the initial units!",
                        "Stock: "
                                + getName()
                                + " Constructor: Stock(Model owner, String name, "
                                + "long initialUnits, long capacity, boolean showInReport, "
                                + "boolean showInTrace)",
                        "A capacity lower than the initial number of units in the stock does not "
                                + "make sense.",
                        "Make sure to initialize a Stock always with a capacity greater or equal "
                                + "to the initial number of stored units.");

                // set the capacity to the initial stock
                _fieldCapacity = _fieldInitial;
            }

            // set the number of available and maximum units, so far
            _fieldAvail = _fieldMaximum = _fieldMinimum = _fieldInitial;

            // set the statistics
            this._wSumAvail = 0.0;
            this._lastUsage = presentTime();
        }

        /// <summary>
        /// Activates the SimProcess <c>process</c>, given as a parameter of this method, as the next
        /// process. This process should be a SimProcess waiting in a queue for some products.
        /// </summary>
        /// <param name="process">The process that is to be activated as next.</param>
        protected void activateAsNext(SimProcess process) {
            string where = "protected void activateAsNext(SimProcess process)";

            if (process != null) {
                // if the given process is not valid just return
                if (!checkProcess(process, where)) {
                    return;
                }

                // if the process is scheduled (on the event-list) already
                if (process.isScheduled()) {
                    process.skipTraceNote(); // don't tell the user, that we ...
                    process.cancel(); // get the process from the event-list
                }

                // remember if the process is blocked at the moment
                bool wasBlocked = process.isBlocked();

                // unblock the process to be able to activate him
                if (wasBlocked) {
                    process.setBlocked(false); // the process is not blocked
                                               // anymore
                                               // and
                } // ready to become activated

                // don't tell the user, that we activate the process after the
                // current process
                process.skipTraceNote();
                process.activateAfter(current());

                // the process status is still "blocked"
                if (wasBlocked) {
                    process.setBlocked(true);
                }
            } // end outer if
        }

        /**
         * Activates the first process waiting in the consumer queue. That is a
         * process which was trying to take products, but it could not get any
         * because there were not enough products for it or another process was
         * first in the queue to be served. This method is called every time a
         * consumer is arriving at the waiting-queue and it is possible for him to
         * pass other processes in front of him in the queue. Then we have to check
         * if one of the other process first in the queue could be satisfied before
         * the newly arrived one.
         */
        protected void activateFirstConsumer() {
            string where = "protected void activateFirstConsumer()";

            // first is the first process in the queue (or null if none is in the
            // queue)
            SimProcess first = _consumerQueue.first();

            if (first != null) {
                // if first is not modelcompatible just return
                if (!checkProcess(first, where)) {
                    return;
                }

                // if first is scheduled (on the event-list) already
                if (first.isScheduled()) {
                    first.skipTraceNote(); // don't tell the user, that we ...
                    first.cancel(); // get the process from the event-list
                }

                // remember if first is blocked at the moment
                bool wasBlocked = first.isBlocked();

                // unblock the process to be able to activate him
                if (wasBlocked) {
                    first.setBlocked(false);
                }

                // don't tell the user, that we activate first after the current
                // process
                first.skipTraceNote();
                first.activateAfter(current());

                // the status of first is still "blocked"
                if (wasBlocked) {
                    first.setBlocked(true);
                }
            } // end outer if
        }

        /**
         * Activates the first process waiting in the producer queue. That is a
         * process which was trying to store products, but it could store them
         * because the capacity limit of this stock is reached. This method is
         * called every time a producer is arriving at the waiting-queue and it is
         * possible for him to pass other processes in front of him in the queue.
         * Then we have to check if one of the other process first in the queue
         * could store his units before the newly arrived one.
         */
        protected void activateFirstProducer() {
            string where = "protected void activateFirstProducer()";

            // first is the first process in the queue (or null if none is in the
            // queue)
            SimProcess first = _producerQueue.first();

            if (first != null) {
                // if first is not modelcompatible just return
                if (!checkProcess(first, where)) {
                    return;
                }

                // if first is scheduled (on the event-list) already
                if (first.isScheduled()) {
                    first.skipTraceNote(); // don't tell the user, that we ...
                    first.cancel(); // get the process from the event-list
                }

                // remember if first is blocked at the moment
                bool wasBlocked = first.isBlocked();

                // unblock the process to be able to activate him
                if (wasBlocked) {
                    first.setBlocked(false);
                }

                // don't tell the user, that we activate first after the current
                // process
                first.skipTraceNote();
                first.activateAfter(current());

                // the status of first is still "blocked"
                if (wasBlocked) {
                    first.setBlocked(true);
                }
            } // end outer if
        }

        /**
         * The addPropertyChangeListener method was generated to support the
         * propertyChange field.
         */
        public void addPropertyChangeListener(
                PropertyChangeListener listener) {
            getPropertyChange().addPropertyChangeListener(listener);
        }

        /**
         * The addPropertyChangeListener method was generated to support the
         * propertyChange field.
         */
        public void addPropertyChangeListener(string propertyName,
                PropertyChangeListener listener) {
            getPropertyChange().addPropertyChangeListener(propertyName, listener);
        }

     

        /// <summary>
        /// Returning the average number of units available in the Stock over the time since the last reset
        /// of the Stock.
        /// </summary>
        /// <returns>
        /// The average number of units available in the Stock over the time since the last reset of the
        /// Stock.
        /// </returns>
        public double avgAvail() {
            /* Perform the avgAvail method. */

            TimeInstant now = presentTime(); // what is the time?
                                             // how long since the last reset
            TimeSpan diff = TimeOperations.diff(now, resetAt());

            // update the weighted sum of available units
            double wSumAvl = _wSumAvail
                    + ((double)_fieldAvail * TimeOperations.diff(now, _lastUsage)
                            .getTimeInEpsilon());
            if (TimeSpan.isEqual(diff, TimeSpan.ZERO)) // diff is not long enough
            {
                sendWarning("A Division-by-Zero error occured in a calculation. "
                        + "The UNDEFINED Value: -1.0 is returned as result.",
                        "Stock: " + getName() + " Method: double avgAvail ()",
                        "The Time difference is zero.",
                        "Make sure not to use avgAvail() right after a reset.");
                return UNDEFINED; // see QueueBased: UNDEFINED = -1
            }
            // return the rounded average
            return StatisticObject.round(wSumAvl / diff.getTimeInEpsilon());
        }

        /// <summary>Checks whether the SimProcess using the Stock is a valid process.</summary>
        /// <param name="p">Is this SimProcess a valid one?</param>
        /// <returns>Returns whether the SimProcess is valid or not.</returns>
        protected bool checkProcess(SimProcess p, string where) {
            if (p == null) // if p is a null pointer instead of a process
            {
                sendWarning(
                        "A non existing process is trying to use a Stock object. "
                                + "The attempted action is ignored!", "Stock: "
                                + getName() + " Method: " + where,
                        "The process is only a null pointer.",
                        "Make sure that only real SimProcesses are using Stocks.");
                return false;
            }

            if (!isModelCompatible(p)) // if p is not modelcompatible
            {
                sendWarning("The process trying to use a Stock object does not "
                        + "belong to this model. The attempted action is ignored!",
                        "Stock: " + getName() + " Method: " + where,
                        "The process is not modelcompatible.",
                        "Make sure that processes are using only Stocks within their model.");
                return false;
            }

            return true;
        }

        /// <summary>Returns a Reporter to produce a report about this Stock.</summary>
        /// <returns>
        /// The Reporter reporting about the statistics of the two queues (producer and consumer) of this
        /// Stock.
        /// </returns>
        protected override Reporter createDefaultReporter()
        {
            return new StockReporter(
                    this);
        }

        /**
         * The firePropertyChange method was generated to support the propertyChange
         * field.
         */
        public void firePropertyChange(PropertyChangeEvent evt) {
            getPropertyChange().firePropertyChange(evt);
        }

        /**
         * The firePropertyChange method was generated to support the propertyChange
         * field.
         */
        public void firePropertyChange(String propertyName, int oldValue,
                int newValue) {
            getPropertyChange()
                    .firePropertyChange(propertyName, oldValue, newValue);
        }

        /**
         * The firePropertyChange method was generated to support the propertyChange
         * field.
         */
        public void firePropertyChange(String propertyName, Object oldValue,
                Object newValue) {
            getPropertyChange()
                    .firePropertyChange(propertyName, oldValue, newValue);
        }

        /**
         * The firePropertyChange method was generated to support the propertyChange
         * field.
         */
        public void firePropertyChange(string propertyName, bool oldValue,
                bool newValue) {
            getPropertyChange()
                    .firePropertyChange(propertyName, oldValue, newValue);
        }

        /// <summary>Gets the available number of products in the Stock at the moment.</summary>
        /// <returns>The available number of products in the Stock at the moment.</returns>
        public long getAvail() {
            return _fieldAvail;
        }

        /// <summary>
        /// Gets the capacity property (long) value of this Stock, that is the number of units this stock can
        /// hold at max.
        /// </summary>
        /// <returns>
        /// The capacity property value, that is the number of units this stock can hold at max.
        /// </returns>
        public long getCapacity() {
            return _fieldCapacity;
        }

        /// <summary>
        /// Returns the implemented queueing discipline of the underlying consumer queue as a String, so it
        /// can be displayed in the report.
        /// </summary>
        /// <returns>The String indicating the queueing discipline.</returns>
        public string getConsQueueStrategy() {

            return _consumerQueue.getAbbreviation(); // that's it

        }

        /// <summary>
        /// Returns the number of consumers that were refused to be enqueued because the queue for consumers
        /// was full.
        /// </summary>
        /// <returns>
        /// The number of consumers that were refused to be enqueued because the queue for consumers was
        /// full.
        /// </returns>
        public long getConsRefused() {
            /* Perform the getConsRefused method. */
            return getRefused(); // get this refused
        }

        /// <summary>Gets the consumers property (long) value.</summary>
        /// <returns>The consumers property value.</returns>
        public long getConsumers() {
            return _fieldConsumers;
        }

        /// <summary>Gets the initial number of products the Stock starts with.</summary>
        /// <returns>The initial number of products the Stock starts with.</returns>
        public long getInitial() {
            return _fieldInitial;
        }

        /// <summary>Gets the maximum number of products in the Stock.</summary>
        /// <returns>The maximum number of products in the Stock.</returns>
        public long getMaximum() {
            return _fieldMaximum;
        }

        /// <summary>Gets the minimum number of units in the Stock so far (since the last reset).</summary>
        /// <returns>The minimum number of units in the Stock since the last reset.</returns>
        public long getMinimum() {
            return _fieldMinimum;
        }

        /// <summary>
        /// Returns whether entities can pass by other entities which are enqueued before them in the queue.
        /// </summary>
        /// <returns>
        /// Indicates whether entities can pass by other entities which are enqueued before them in the
        /// queue.
        /// </returns>
        public bool getPassByConsumers() {
            return _passByCons;
        }

        /// <summary>
        /// Returns whether entities can pass by other entities which are enqueued before them in the queue.
        /// </summary>
        /// <returns>
        /// Indicates whether entities can pass by other entities which are enqueued before them in the
        /// queue.
        /// </returns>
        public bool getPassByProducers() {
            return _passByProds;
        }

        /// <summary>
        /// Returns the implemented queueing discipline of the underlying queue for producers as a String, so
        /// it can be displayed in the report.
        /// </summary>
        /// <returns>The String indicating the queueing discipline.</returns>
        public string getProdQueueStrategy() {
            /* Perform the getProdQueueStrategy method. */

            return _producerQueue.getQueueStrategy(); // ask the producer queue
        }

        /// <summary>
        /// Returns the number of producers that were refused to be enqueued because the queue for producers
        /// was full.
        /// </summary>
        /// <returns>
        /// The number of producers that were refused to be enqueued because the queue for producers was
        /// full.
        /// </returns>
        public long getProdRefused() {
            /* Perform the getProdRefused method. */
            return getProducerQueue().getRefused();
        }

        /// <summary>Returns the queue where the producers are waiting to deliver their units.</summary>
        /// <returns>the queue where the producers are waiting to deliver their units.</returns>
        public ProcessQueue<SimProcess> getProducerQueue() {
            /* Perform the getProducerQueue method. */

            return this._producerQueue; // that's all
        }

        /// <summary>Gets the producers property (long) value.</summary>
        /// <returns>The producers property value.</returns>
        public long getProducers() {
            return _fieldProducers;
        }

        /**
         * Accessor for the propertyChange field.
         */
        protected PropertyChangeSupport getPropertyChange() {
            if (propertyChange == null) {
                propertyChange = new PropertyChangeSupport(this);
            }
            ;
            return propertyChange;
        }

        /// <summary>Gets the refused property (long) value.</summary>
        /// <returns>The refused property value.</returns>
        public long getRefused() {
            return _fieldRefused;
        }

        /// <summary>
        /// Gets the where property (String) value denoting the method, where something has gone wrong.
        /// </summary>
        /// <returns>The where property value denoting the method, where something has gone wrong.</returns>
        public string getWhere() {
            return _fieldWhere;
        }

        /**
         * The hasListeners method was generated to support the propertyChange
         * field.
         */
        public bool hasListeners(string propertyName) {
            return getPropertyChange().hasListeners(propertyName);
        }

        /**
         * The removePropertyChangeListener method was generated to support the
         * propertyChange field.
         */
        public void removePropertyChangeListener(
                PropertyChangeListener listener) {
            getPropertyChange().removePropertyChangeListener(listener);
        }

        /**
         * The removePropertyChangeListener method was generated to support the
         * propertyChange field.
         */
        public void removePropertyChangeListener(String propertyName,
                PropertyChangeListener listener) {
            getPropertyChange()
                    .removePropertyChangeListener(propertyName, listener);
        }

        /**
         * Resets all statistical counters of this Stock to their default values.
         * Both, producer queue and consumer queue are reset. The number of
         * available units at this moment and the consumer and producer processes
         * waiting in their queues remain unchanged. The maximum length of the
         * queues are set to the current number of enqueued processes.
         */
        public void reset() {
            /* Perform the reset method. */

            base.reset(); // reset the QueueBased statistics of this Stock

            _producerQueue.reset(); // reset the statistics for the producer queue

            _fieldMaximum = _fieldMinimum = _fieldAvail;
            _fieldProducers = _fieldConsumers = 0;
            _wSumAvail = 0.0;
            _lastUsage = presentTime();
            _fieldRefused = 0;
        }

        /// <summary>
        /// Method for consumers to make the Stock retrieve a number of n units. When there are not enough
        /// units available or another consumer is first in the queue to be served (and it is not possible to
        /// pass by), the current consumer process will be blocked and inserted in the waiting-queue (for
        /// consumers).
        /// </summary>
        /// <param name="n">
        /// The number of units the Stock is retrieving for the consumer; must be positive.
        /// </param>
        /// <returns>
        /// Is <c>true</c> if the specified number of units can been retrieved successfully, <c>false</c>
        /// otherwise (i.e. capacity limit of the queue is reached).
        /// </returns>
        public Next<bool> retrieve(long n) {
            /* Perform the retrieve method. */

            _fieldWhere = "boolean retrieve(long n)";

            SimProcess currentProcess = currentSimProcess();

            if (!checkProcess(currentProcess, _fieldWhere)) // check the current
                                                            // process
            {
                return new Next<bool>(false);
            } // if it is not valid return false

            if (n <= 0) // if the process is taking nothing or less
            {
                sendWarning(
                        "Attempt to take nothing or a negative number of units"
                                + " out of a Stock. The attempted action is ignored!",
                        "Stock: " + getName() + " Method: " + _fieldWhere,
                        "It does not make sense to take nothing or less out of a Stock. "
                                + "The statistic will be corrupted with negative numbers!",
                        "Make sure to take at least one unit out of the Stock.");
                return new Next<bool>(false); // go to where you came from; ignore that rubbish
            }

            if (n > _fieldCapacity) // if the process is trying to take more than
                                    // the
                                    // capacity
            {
                sendWarning(
                        "Attempt to take more units than the capacity of this Stock"
                                + " can ever hold. The attempted action is ignored!",
                        "Stock: " + getName() + " Method: " + _fieldWhere,
                        "The Stock can never retrieve more units than its capacity can hold.",
                        "Make sure not to take more units out of the Stock than its capacity.");
                return new Next<bool>(false); // go to where you came from; ignore that rubbish
            }

            // check if capacity limit of the consumer queue is reached
            if (queueLimit <= length()) {
                if (currentlySendDebugNotes()) {
                    sendDebugNote("refuses to insert "
                            + currentProcess.getQuotedName()
                            + " in consumer waiting-queue, because the capacity limit is reached. ");
                }

                if (currentlySendTraceNotes()) {
                    sendTraceNote("is refused to be enqueued in "
                            + this.getQuotedName() + "because the capacity limit ("
                            + getQueueLimit() + ") of the "
                            + "consumer queue is reached");
                }

                _fieldRefused++; // count the refused ones

                return new Next<bool>(false); // capacity limit is reached
            }

            // insert every process in the queue for statistic reasons
            _consumerQueue.insert(currentProcess);

            var next =new Next<bool>(true);

            // is it possible for this process to pass by?
            if (_passByCons == false) {
                // not enough products available OR other process is first in the
                // queue
                if (n > _fieldAvail || currentProcess != _consumerQueue.first()) {
                    // tell in the trace what the process is waiting for
                    if (currentlySendTraceNotes()) {
                        sendTraceNote("awaits " + n + " of " + this.getQuotedName());
                    }

                    // for debugging purposes
                    if (currentlySendDebugNotes()) {
                        sendDebugNote("can not retrieve " + n + " units for "
                                + currentProcess.getQuotedName() + "<br>"
                                + "because stock (" + getAvail()
                                + " units) is too low.");
                    }

                    do { // the process is stuck in here
                        currentProcess.setBlocked(true); // as long as ...see
                                                         // while
                        currentProcess.skipTraceNote(); // don't tell the user, that
                                                        // we ...
                        //TODO:how to implement the passivate with yield ...
                        //((SimProcess)currentProcess).passivate(); // passivate the current process
                    }
                    // not enough products available OR other process is first in
                    // the queue
                    while (n > _fieldAvail
                            || currentProcess != _consumerQueue.first());

                } // end if

            } // end if (passBy = false)

            else // the process is allowed to pass by other processes (passBy =
                 // true)
            {
                // not enough products available OR other process is first in the
                // queue
                if (n > _fieldAvail || currentProcess != _consumerQueue.first()) {
                    // if this consumer is not the first in the consumer queue
                    if (currentProcess != _consumerQueue.first()) {
                        // we have to make sure that no other process in front of
                        // this current
                        // process in the wait queue could be satisfied, so activate
                        // the first Process in the queue to see what he can do. He
                        // will pass
                        // the activation on to his successors until this process
                        // will be
                        // activated again to get his products. (hopefully)
                        activateFirstConsumer();
                    }

                    // only if not enough products are available the process has to
                    // wait
                    if (n > _fieldAvail) {
                        // tell in the trace what the process is waiting for
                        if (currentlySendTraceNotes()) {
                            sendTraceNote("awaits " + n + " of "
                                    + this.getQuotedName());
                        }

                        // for debugging purposes
                        if (currentlySendDebugNotes()) {
                            sendDebugNote("can not retrieve " + n + " units for "
                                    + currentProcess.getQuotedName() + "<br>"
                                    + "because stock (" + getAvail()
                                    + " units) is too low.");
                        }
                    } // end if not enough products are available

                    // block and passivate the process until enough products are
                    // available
                    do { // the process is stuck in here
                        currentProcess.setBlocked(true); // as long as ...see
                                                         // while
                        currentProcess.skipTraceNote(); // don't tell the user, that
                                                        // we ...
                        //TODO:how to implement the passivate with yield ...
                        //((SimProcess)currentProcess).passivate(); // passivate the current process
                        next.passivate=true;
                        // activate the next process in the consumer queue to see
                        // what he can do
                        activateAsNext(_consumerQueue.succ(currentProcess));
                    } while (n > _fieldAvail); // not enough products available

                } // end if
            } // end else (passBy = true)

            // the current process has got the products he wanted ...

            _consumerQueue.remove(currentProcess); // get the process out of the
                                                   // queue
            currentProcess.setBlocked(false); // we are not blocked (anymore),
                                              // yeah!

            /*
             * this will be done with a PropertyChangeEvent when the fieldAvail is
             * updated. Hopefully ... activateNext(); // give waiting successors a
             * chance
             */

            updateStatistics(-n); // statistics will be updated
                                  // with a negative n for deliver(), remember?!

            // tell in the trace what the process is taking from the Stock
            if (currentlySendTraceNotes()) {
                sendTraceNote("takes " + n + " units from " + this.getQuotedName());
            }

            // for debugging purposes
            if (currentlySendDebugNotes()) {
                sendDebugNote("retrieves " + n + " units for "
                        + currentProcess.getQuotedName() + "<br>" + "and has now "
                        + getAvail() + " units on stock.");
            }

            return next;
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
        public void setPassByConsumers(bool newPassBy) {
            this._passByCons = newPassBy; // that's all!
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
        public void setPassByProducers(bool newPassBy) {
            this._passByProds = newPassBy; // that's all!
        }

        /// <summary>
        /// Method for producers to make the Stock store a number of n units. When the capacity of the stock
        /// can not hold the additional incoming units or another producer is already waiting at first
        /// position in the queue (and it is not possible to pass by), the current producer process will be
        /// blocked and inserted in the queue (for producers).
        /// </summary>
        /// <param name="n">
        /// The number of units the Stock is receiving from the producer to store. n must be positive.
        /// </param>
        /// <returns>
        /// Is <c>true</c> if the specified number of units can been stored successfully, <c>false</c>
        /// otherwise (i.e. capacity limit of the queue is reached).
        /// </returns>
        public bool store(long n) {
            /* Perform the store method. */

            _fieldWhere = "boolean store(long n)";

            SimProcess currentProcess = currentSimProcess();

            if (!checkProcess(currentProcess, _fieldWhere)) // check the current
                                                            // process
            {
                return false;
            } // if it is not valid return false

            if (n <= 0) // if the process is giving nothing or less
            {
                sendWarning(
                        "Attempt to store nothing or a negative number of units"
                                + " in a Stock. The attempted action is ignored!",
                        "Stock: " + getName() + " Method: " + _fieldWhere,
                        "It does not make sense to store nothing or less in a Stock.",
                        "Make sure to store at least one unit in the Stock.");
                return false; // go to where you came from; ignore that rubbish
            }

            // is the process trying to store more than the capacity can ever hold?
            if (n > _fieldCapacity) {
                sendWarning(
                        "Attempt to store more units than the capacity of this Stock"
                                + " can hold. The attempted action is ignored!",
                        "Stock: " + getName() + " Method: " + _fieldWhere,
                        "The Stock can never store more units than its capacity. "
                                + "Units to store: " + n
                                + " exceeds the capacity of: " + getCapacity(),
                        "Make sure not to store more units in a Stock than its capacity "
                                + "can hold.");
                return false; // go to where you came from; ignore that rubbish
            }

            // check if capacity limit of the producer queue is reached
            if (_producerQueue.getQueueLimit() <= _producerQueue.length()) {
                if (currentlySendDebugNotes()) {
                    sendDebugNote("refuses to insert "
                            + currentProcess.getQuotedName()
                            + " in producer waiting-queue, because the capacity limit is reached. ");
                }

                if (currentlySendTraceNotes()) {
                    sendTraceNote("is refused to be enqueued in "
                            + this.getQuotedName() + "because the capacity limit ("
                            + getQueueLimit() + ") of the "
                            + "producer queue is reached");
                }

                // count the refused producers
                _producerQueue.setRefused(_producerQueue.getRefused() + 1);

                return false; // capacity limit is reached
            }

            // insert every process in the queue for statistic reasons
            _producerQueue.insert(currentProcess);

            // is it possible for this process to pass by?
            if (_passByProds == false) {

                // not enough space for the new units left OR other process is first
                // in the q
                if (n + _fieldAvail > _fieldCapacity
                        || currentProcess != _producerQueue.first()) {
                    // tell in the trace what the process is waiting for
                    if (currentlySendTraceNotes()) {
                        sendTraceNote("is waiting to store " + n + " units to '"
                                + this.getName() + "'");
                    }

                    // for debugging purposes
                    if (currentlySendDebugNotes()) {
                        sendDebugNote("can not store " + n + " units from "
                                + currentProcess.getQuotedName() + "<br>"
                                + "because capacity limit (" + getCapacity()
                                + ") is reached.");
                    }

                    do { // the process is stuck in here
                        currentProcess.setBlocked(true); // as long as ...see
                                                         // while
                        currentProcess.skipTraceNote(); // don't tell the user, that
                                                        // we ...
                        //TODO:how to implement the passivate with yield ...
                        //((SimProcess)currentProcess).passivate(); // passivate the current process
                    }
                    // not enough space available OR other process is first in the
                    // queue
                    while (n + _fieldAvail > _fieldCapacity
                            || currentProcess != _producerQueue.first());

                } // end if

            } // end if (passBy = false)

            else // the process is allowed to pass by other processes (passBy =
                 // true)
            {
                // not enough space for the new units left OR other process is first
                // in the q
                if (n + _fieldAvail > _fieldCapacity
                        || currentProcess != _producerQueue.first()) {
                    // if this producer is not the first in the producer queue
                    if (currentProcess != _producerQueue.first()) {
                        // we have to make sure that no other process in front of
                        // this current
                        // process in the wait queue could be satisfied, so activate
                        // the first Process in the queue to see what he can do. He
                        // will pass
                        // the activation on to his successors until this process
                        // will be
                        // activated again to get his products. (hopefully)
                        activateFirstProducer();
                    }

                    // only if not enough space is left for the units the process
                    // has to wait
                    if (n + _fieldAvail > _fieldCapacity) {
                        // tell in the trace what the process is waiting for
                        if (currentlySendTraceNotes()) {
                            sendTraceNote("is waiting to store " + n
                                    + " units to '" + this.getName() + "'");
                        }

                        // for debugging purposes
                        if (currentlySendDebugNotes()) {
                            sendDebugNote("can not store " + n + " units from "
                                    + currentProcess.getQuotedName() + "<br>"
                                    + "because capacity limit (" + getCapacity()
                                    + ") is reached.");
                        }
                    } // end if not enough space is left for the units

                    // block and passivate the process until enough space is
                    // available
                    do { // the process is stuck in here
                        currentProcess.setBlocked(true); // as long as ...see
                                                         // while
                        currentProcess.skipTraceNote(); // don't tell the user, that

                        //TODO:how to implement the passivate with yield ...
                        //((SimProcess)currentProcess).passivate(); // passivate the current process
                    } while (n + _fieldAvail > _fieldCapacity); // not enough space
                                                                // available

                }
            } // end else (passBy = true)

            // the currrent process can store his products in the stock ...

            _producerQueue.remove(currentProcess); // get the process out of the
                                                   // queue
            currentProcess.setBlocked(false); // we are not blocked (anymore),
                                              // yeah!

            /*
             * this will be done with a PropertyChangeEvent when the fieldAvail is
             * updated. Hopefully ... activateNext(); // give waiting successors a
             * chance
             */

            updateStatistics(n); // statistics will be updated

            // tell in the trace what the process is storing in the Stock
            if (currentlySendTraceNotes()) {
                sendTraceNote("stores " + n + " units to '" + this.getName() + "'");
            }

            // for debugging purposes
            if (currentlySendDebugNotes()) {
                sendDebugNote("stores " + n + " units from "
                        + currentProcess.getQuotedName() + "<br>" + "and has now "
                        + getAvail() + " units on stock.");
            }

            return true;
        }

        /// <summary>
        /// Updates the statistics when producers or consumers access the Stock. Changes the fieldAvail and
        /// fires the PropertyChangeEvent(s).
        /// </summary>
        /// <param name="n">
        /// Is positive when producers <c>store()</c> new units in the Stock and negative when the Stock
        /// <c>retrieve()</c>'s units for the consumer process.
        /// </param>
        protected void updateStatistics(long n) {
            TimeInstant now = presentTime(); // what's the time?
            _wSumAvail = _wSumAvail
                    + ((double)_fieldAvail * TimeOperations.diff(now, _lastUsage)
                            .getTimeInEpsilon());
            _lastUsage = now;

            // remember old number of available units
            long oldAvail = _fieldAvail;
            // update number of available units
            _fieldAvail += n; // n can be positive or negative
                              // fire PropertyChange to get all listeners (QueueList*) informed
            firePropertyChange("avail",oldAvail, _fieldAvail);

            if (n > 0) // it is a real producer
            {
                _fieldProducers++;
                if (_fieldAvail > _fieldMaximum) {
                    _fieldMaximum = _fieldAvail;
                }
            } else // it is a consumer
            {
                _fieldConsumers++;
                if (_fieldAvail < _fieldMinimum) {
                    _fieldMinimum = _fieldAvail;
                }
            }

        }
    }

    public class PropertyChangeSupport
    {
        public PropertyChangeSupport(Stock stock)
        {
            Stock = stock;
        }

        public Stock Stock { get; }

        internal void addPropertyChangeListener(PropertyChangeListener listener)
        {
            throw new NotImplementedException();
        }

        internal void addPropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            throw new NotImplementedException();
        }

        internal void firePropertyChange(PropertyChangeEvent evt)
        {
            throw new NotImplementedException();
        }

        internal void firePropertyChange(string propertyName, int oldValue, int newValue)
        {
            throw new NotImplementedException();
        }

        internal void firePropertyChange(String propertyName, Object oldValue,
                Object newValue)
        {
            throw new NotImplementedException();
        }

        internal bool hasListeners(string propertyName)
        {
            throw new NotImplementedException();
        }

        internal void removePropertyChangeListener(PropertyChangeListener listener)
        {
            throw new NotImplementedException();
        }

        internal void removePropertyChangeListener(string propertyName, PropertyChangeListener listener)
        {
            throw new NotImplementedException();
        }
    }
}