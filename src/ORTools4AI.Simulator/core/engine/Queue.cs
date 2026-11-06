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
  * Queue provides models with a ready-to-use modelling element to enqueue
  * entities in. The sort order of the queue is determined first
  * by the priorities of the enqueued entities and second by the given sort
  * order. The default sort order is FIFO (first in, first out) but others like
  * LIFO (last in, first out) can be chosen, too. See the constants in class
  * <code>QueueBased</code> and the derived classes from <code>QueueList</code>.
  * The capacity of the Queue, that is the maximum number of entities enqueued,
  * can be chosen, too.
  * <p>
  * For queueing <code>SimProcess</code>es the usage of class
  * <code>ProcessQueue</code> is recommended.  
  * 
  * 
  * @see QueueBased
  * @see QueueList
  * @see QueueListFifo
  * @see QueueListLifo
  * @see ProcessQueue
  */
    public class Queue<E> : QueueBased, IEnumerable<E> where E : Entity
    {

        /**
         * The queue implementation (data structure) that actually stores the
         * entities
         */
        private QueueList<E> _ql;

        /**
         * Counter for the entities which are refused to be enqueued, because the
         * queue capacity is full.
         */
        private long _refused;

        /// <summary>
        /// Constructs a simple priority based waiting-queue for entities, the kind of queue sort order and
        /// the capacity of the queue can be chosen. Note that since SimProcesses are derived from Entities,
        /// they can be queued inside this queue, too. Queues can be sorted as follows:
        /// <c>QueueBased.FIFO</c> sorts entities by their priority, highest priority first. Entities with
        /// the same priority are enqueued based on "first in, first out". <c>QueueBased.LIFO</c> also sorts
        /// entities by their priority, highest priority first. However, entities with the same priority are
        /// enqueued based on "last in, first out". <c>QueueBased.Random</c> assigns a random position to
        /// each entity entering the queue, disregarding priority. The usage of the generic version
        /// Queue&lt;Type&gt; where <c>Type</c> is derived from <c>Entity</c> is recommended for type safety.
        /// Using the raw type <c>Queue</c> yields a queue in which any <c>Entity</c> can be enqueued,
        /// typically requiring type casting on accessing entities enqueued.
        /// </summary>
        /// <param name="owner">The model this queue is associated to</param>
        /// <param name="name">The queue's name</param>
        /// <param name="sortOrder">
        /// determines the sort order of the underlying queue implementation. Choose a constant from
        /// <c>QueueBased</c>: <c>QueueBased.FIFO</c>, <c>QueueBased.LIFO</c> or QueueBased.Random.
        /// </param>
        /// <param name="qCapacity">
        /// The capacity of the Queue, that is how many entities can be enqueued. Zero (0) can be used as
        /// shortcut for for a capacity of <c>Integer.MAX_VALUE</c> = 2,147,483,647, which should approximate
        /// an infinite queue sufficiently well for most purposes.
        /// </param>
        /// <param name="showInReport">Flag if queue should produce a report</param>
        /// <param name="showInTrace">Flag for queue to produce trace messages</param>
        public Queue(Model owner, string name, int sortOrder, int qCapacity,
                bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {


            reset();

            // determine the queueing strategy
            switch (sortOrder)
            {
                case QueueBased.FIFO:
                    _ql = new QueueListFifo<E>(); break;
                case QueueBased.LIFO:
                    _ql = new QueueListLifo<E>(); break;
                case QueueBased.RANDOM:
                    _ql = new QueueListRandom<E>(); break;
                default:
                    sendWarning(
                            "The given sortOrder parameter " + sortOrder + " is not valid! "
                                    + "A queue with Fifo sort order will be created.",
                            "Queue : "
                                    + getName()
                                    + " Constructor: Queue(Model owner, String name, "
                                    + "int sortOrder, long qCapacity, boolean showInReport, "
                                    + "boolean showInTrace)",
                            "A valid positive integer number must be provided to "
                                    + "determine the sort order of the queue.",
                            "Make sure to provide a valid positive integer number "
                                    + "by using the constants in the class QueueBased, like "
                                    + "QueueBased.FIFO, QueueBased.LIFO or QueueBased.RANDOM.");
                    _ql = new QueueListFifo<E>();
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
                        "Queue : "
                                + getName()
                                + " Constructor: Queue(Model owner, String name, "
                                + "int sortOrder, long qCapacity, boolean showInReport, "
                                + "boolean showInTrace)",
                        "A negative capacity for a queue does not make sense.",
                        "Make sure to provide a valid positive capacity "
                                + "for the queue.");
                // set the capacity to the maximum value
                queueLimit = int.MaxValue;
            }

            // check if qCapacity is zero (that means maximum capacity)
            if (qCapacity == 0)
            {
                // set the capacity to the maximum value
                queueLimit = int.MaxValue;
            }

        }

        /// <summary>
        /// Constructs a simple priority and FIFO based waiting-queue for entities with a maximum capacity of
        /// 2,147,483,647 waiting entities, which should serve as an approximation of infinite queues
        /// sufficiently well for most purposes.Note that since SimProcesses are derived from Entities, they
        /// can be queued inside this queue, too. The usage of the generic version Queue&lt;Type&gt; where
        /// <c>Type</c> is derived from <c>Entity</c> is recommended for type safety. Using the raw type
        /// <c>Queue</c> yields a queue in which any <c>Entity</c> can be enqueued, typically requiring type
        /// casting on accessing entities enqueued.
        /// </summary>
        /// <param name="owner">The model this queue is associated to</param>
        /// <param name="name">The queue's name</param>
        /// <param name="showInReport">Flag if queue should produce a report</param>
        /// <param name="showInTrace">Flag for queue to produce trace messages</param>
        public Queue(Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {


            reset();

            // make the queue with Fifo queueing discipline and unlimited capacity
            _ql = new QueueListFifo<E>();
            _ql.setQueueBased(this);

        }

        /// <summary>Returns a special queue reporter to produce a report about this queue.</summary>
        /// <returns>The reporter for this queue</returns>
        protected override Reporter createDefaultReporter()
        {

            return new QueueReporter(this);

        }

        /// <summary>
        /// Returns the first entity queued in this queue or <c>null</c> in case the queue is empty.
        /// </summary>
        /// <returns>The first entity in the queue or <c>null</c> if the queue is empty</returns>
        public E first()
        {

            return _ql.first(); // straight design

        }

        /// <summary>
        /// Returns the first entity queued in this queue that fulfills the given condition. The queue is
        /// searched from front to end and the first entity that returns <c>true</c> when the condition is
        /// applied to it is returned by this method. If no Entity fulfills the given condition or the queue
        /// is empty, <c>null</c> will be returned.
        /// </summary>
        /// <param name="c">The condition that the entity returned must fulfill</param>
        /// <returns>
        /// The first entity queued in this queue applying to the given condition or <c>null</c>
        /// </returns>
        public E first(Condition<E> c)
        {

            if (c == null)
            {
                sendWarning(
                        "Can not return first entity fulfilling the condition!",
                        "Queue : " + getName() + " Method: void first(Condition c)",
                        "The Condition 'c' given as parameter is a null reference!",
                        "Check to always have valid references when querying Queues.");
                return null; // no proper parameter
            }
            if (_ql.isEmpty())
                return null; // nobody home to be checked
            for (E tmp = _ql.first(); tmp != null; tmp = _ql.succ(tmp))
            {
                if (c.check(tmp))
                    return tmp;
            }
            // if no Entity fulfills to the condition just return null
            return null;

        }

        /// <summary>
        /// Returns <c>true</c> if the given <c>Entity</c> is contained in the queue; <c>false</c> otherwise.
        /// </summary>
        /// <param name="e">The <c>Entity</c> we are looking for in the queue.</param>
        /// <returns>
        /// <c>True</c> if the given <c>Entity</c> is contained in the queue; <c>false</c> otherwise.
        /// </returns>
        public bool contains(E e)
        {

            return _ql.contains(e); // delegated to the internal queue list
        }

        /// <summary>Returns the current length of the Queue.</summary>
        /// <returns>The number of entities enqueued.</returns>
        public int size()
        {
            return _ql.size();
        }

        /// <summary>Returns the queue index of a given <c>Entity</c>.</summary>
        /// <returns>
        /// The position of the entity as an <c>int</c>. Returns -1 if no such position exists.
        /// </returns>
        public int get(E e)
        {

            return _ql.get(e);

        }

        /// <summary>
        /// Returns the <c>Entity</c> queued at the named position. The first position is 0, the last one
        /// size()-1.
        /// </summary>
        /// <returns>
        /// The <c>Entity</c> at the position of <c>int</c> or <c>null</c> if no such position exists.
        /// </returns>
        public E get(int index)
        {

            return _ql.get(index);

        }


        /// <summary>
        /// Returns the implemented queueing discipline of the underlying queue as a String, so it can be
        /// displayed in the report.
        /// </summary>
        /// <returns>The String indicating the queueing discipline.</returns>
        public string getQueueStrategy()
        {

            return _ql.getAbbreviation(); // that's it

        }

        /// <summary>Returns the number of entities refused to be enqueued in the queue.</summary>
        /// <returns>The number of entities refused to be enqueued in the queue.</returns>
        public long getRefused()
        {

            return _refused; // that's it
        }

        /// <summary>
        /// Enters a new Entity into the queue. If the capacity of the queue is full, the entity will not be
        /// enqueued and <c>false</c> will be returned. The Entity will be stored in the queue until method
        /// <c>remove(Entity e)</c> is called with this specific Entity. Entities inside the queue are
        /// ordered according to their priority. Higher priorities are sorted in front of lower priorities.
        /// Entities with same priority are orderer according to the specified strategy. The first Entity
        /// inside the queue will always be the one with the highest priority.
        /// </summary>
        /// <param name="e">The Entity to be added to the queue.</param>
        /// <returns>
        /// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is
        /// reached).
        /// </returns>
        public bool insert(E e)
        {

            if (e == null)
            { // null returns with warning
                sendWarning("Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName()
                                + " Method: boolean insert(E e)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when enqueueing Entities");
                return false; // no proper parameter
            }

            if (!isModelCompatible(e))
            {
                sendWarning("Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName() + " Method: boolean insert"
                                + "(E e)",
                        "The Entity 'e' (" + e.getQuotedName() + ") given as parameter is not compatible to "
                                + "the model this queue belongs to!",
                        "Check if your submodels are allowed to mingle with other "
                                + "model's components.");
                return false; // not of my model type!!!
            }

            if (queueLimit <= length())
            {

                if (currentlySendDebugNotes())
                {
                    sendDebugNote("refuses to insert " + e.getQuotedName()
                        + " because the " + "capacity limit is reached. Queue:<br>"
                        + _ql.toString());
                }

                if (currentlySendTraceNotes())
                {
                    sendTraceNote("is refused to be enqueued in "
                        + this.getQuotedName() + "because the capacity limit ("
                        + getQueueLimit() + ") of this queue is " + "reached");
                }

                _refused++; // count the refused ones

                return false; // capacity limit is reached
            }

            _ql.insert(e); // that's it

            if (currentlySendDebugNotes())
            {
                sendDebugNote("inserts " + e.getQuotedName() + " in the queue:<br>"
                        + _ql.toString());
            }

            // produce trace output
            if (currentlySendTraceNotes())
            {
                if (e == currentEntity() && currentEntityAll().Count == 1)
                {
                    sendTraceNote("inserts itself into " + this.getQuotedName());
                }
                else
                {
                    sendTraceNote("inserts " + e.getName() + " into "
                            + this.getQuotedName());
                }
            }

            return true;
        }

        /// <summary>
        /// Enters a new Entity into the queue and places it after the given Entity. If the capacity of the
        /// queue is full, the entity will not be enqueued and <c>false</c> will be returned. Make sure that
        /// the entity given as reference is already queued inside the queue, else the entity will not be
        /// enqueued and <c>false</c> will be returned. The Entity will be stored in the queue until method
        /// <c>remove(Entity e)</c> is called with this specific Entity.
        /// </summary>
        /// <param name="e">The Entity to be added to the queue</param>
        /// <param name="after">The Entity after which the entity e is to be inserted</param>
        /// <returns>
        /// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is
        /// reached).
        /// </returns>
        public bool insertAfter(E e, E after)
        {

            if (e == null)
            {
                sendWarning(
                        "Can not insert entity into Queue! Command ignored.",
                        "Queue : "
                                + getName()
                                + " Method: boolean insertAfter(E e, E after)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when enqueueing Entities");
                return false; // no proper parameter
            }

            if (after == null)
            {
                sendWarning(
                        "Can not insert entity into Queue! Command ignored.",
                        "Queue : "
                                + getName()
                                + " Method: boolean insertAfter(E e, E after)",
                        "The Entity 'after' given as parameter is a null reference!",
                        "Check to always have valid references when enqueueing Entities");
                return false; // no proper parameter
            }

            if (!isModelCompatible(e))
            {
                sendWarning("Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName() + " Method: boolean insertAfter"
                                + "(E e, E after)",
                        "The Entity 'e' (" + e.getQuotedName() + ") given as parameter is not compatible to "
                                + "the model this queue belongs to!",
                        "Check if your submodels are allowed to mingle with other "
                                + "model's components.");
                return false; // not of my model type!!!
            }

            if (queueLimit <= length())
            {

                if (currentlySendDebugNotes())
                {
                    sendDebugNote("refuses to insert " + e.getQuotedName()
                        + " because the " + "capacity limit is reached. Queue:<br>"
                        + _ql.toString());
                }

                if (currentlySendTraceNotes())
                {
                    sendTraceNote("is refused to be enqueued in "
                        + this.getQuotedName() + "because the capacity limit ("
                        + getQueueLimit() + ") of this queue is " + "reached");
                }

                _refused++; // count the refused ones

                return false; // capacity limit is reached
            }

            bool successful = _ql.insertAfter(e, after); // that's the point

            if (currentlySendDebugNotes())
            {
                sendDebugNote("inserts " + e.getQuotedName() + " after "
                        + after.getQuotedName() + "<br>" + _ql.toString());
            }

            // produce trace output
            if (currentlySendTraceNotes())
            {
                if (e == currentEntity() && currentEntityAll().Count == 1)
                {
                    sendTraceNote("inserts itself into " + this.getQuotedName()
                            + " after " + after.getName());
                }
                else
                {
                    sendTraceNote("inserts " + e.getName() + " into "
                            + this.getQuotedName() + " after " + after.getName());
                }
            }

            return successful;

        }

        /// <summary>
        /// Enters a new Entity into the queue and places it in front of the given Entity. If the capacity of
        /// the queue is full, the entity will not be enqueued and <c>false</c> will be returned. Make sure
        /// that the Entity given as reference is already queued inside the queue, else the Entity will not
        /// be queued and <c>false</c> will be returned. The Entity will be stored in the queue until method
        /// <c>remove(Entity e)</c> is called with this specific Entity.
        /// </summary>
        /// <param name="e">The Entity to be added to the queue</param>
        /// <param name="before">The Entity before which the entity e is to be inserted</param>
        /// <returns>
        /// Is <c>true</c> if insertion was successful, <c>false</c> otherwise (i.e. capacity limit is
        /// reached).
        /// </returns>
        public bool insertBefore(E e, E before)
        {

            if (e == null)
            {
                sendWarning("Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName()
                                + " Method: boolean insertBefore(E e, "
                                + "Entity before)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when enqueueing Entities");
                return false; // no proper parameter
            }
            if (before == null)
            {
                sendWarning(
                        "Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName()
                                + " Method: boolean insertBefore(E e, "
                                + "Entity before)",
                        "The Entity 'before' given as parameter is a null reference!",
                        "Check to always have valid references when enqueueing Entities");
                return false; // no proper parameter
            }

            if (!isModelCompatible(e))
            {
                sendWarning("Can not insert entity into Queue! Command ignored.",
                        "Queue : " + getName() + " Method: boolean insertBefore"
                                + "(E e, Entity before)",
                        "The Entity 'e' (" + e.getQuotedName() + ") given as parameter is not compatible to "
                                + "the model this queue belongs to!",
                        "Check if your submodels are allowed to mingle with other "
                                + "model's components.");
                return false; // not of my model type!!!
            }

            if (queueLimit <= length())
            {

                if (currentlySendDebugNotes())
                {
                    sendDebugNote("refuses to insert " + e.getQuotedName()
                        + " because the " + "capacity limit is reached. Queue:<br>"
                        + _ql.toString());
                }

                if (currentlySendTraceNotes())
                {
                    sendTraceNote("is refused to be enqueued in "
                        + this.getQuotedName() + "because the capacity limit ("
                        + getQueueLimit() + ") of this queue is " + "reached");
                }

                _refused++; // count the refused ones

                return false; // capacity limit is reached
            }

            bool successful = _ql.insertBefore(e, before); // that's the point

            if (currentlySendDebugNotes())
            {
                sendDebugNote("inserts " + e.getQuotedName() + " before "
                        + before.getQuotedName() + "<br>" + _ql.toString());
            }

            // produce trace output
            if (currentlySendTraceNotes())
            {
                if (e == currentEntity() && currentEntityAll().Count == 1)
                {
                    sendTraceNote("inserts itself into " + this.getQuotedName()
                            + " before " + before.getName());
                }
                else
                {
                    sendTraceNote("inserts " + e.getName() + " into "
                            + this.getQuotedName() + " before " + before.getName());
                }
            }

            return successful;

        }

        /// <summary>
        /// Returns a boolean value indicating if the queue is empty or if any number of entities is
        /// currently enqueued in it.
        /// </summary>
        /// <returns>Is <c>true</c> if the Queue is empty, <c>false</c> otherwise</returns>
        public bool isEmpty()
        {

            return _ql.isEmpty();

        }

        /// <summary>
        /// Returns the last Entity queued in this queue or <c>null</c> in case the queue is empty.
        /// </summary>
        /// <returns>The last Entity in the queue or <c>null</c> if the queue is empty</returns>
        public E last()
        {

            return _ql.last(); // straight design again

        }

        /// <summary>
        /// Returns the last Entity queued in this queue that fulfills the given condition. The queue is
        /// searched from end to front and the first entity that returns <c>true</c> when the condition is
        /// applied to it is returned. If no Entity fulfills the given condition or the queue is empty,
        /// <c>null</c> will be returned.
        /// </summary>
        /// <param name="c">The condition that the entity returned must fulfill</param>
        /// <returns>
        /// The last Entity queued in this queue applying to the given condition or <c>null</c>
        /// </returns>
        public E last(Condition<E> c)
        {

            if (c == null)
            {
                sendWarning(
                        "Can not insert entity!",
                        "Queue : " + getName()
                                + " Method: Entity last(Condition c)",
                        "The Condition -c- given as parameter is a null reference!",
                        "Check to always have valid references when querying Queues.");
                return null; // no proper parameter
            }

            if (_ql.isEmpty())
                return null; // nobody home to be checked
            for (E tmp = _ql.last(); tmp != null; tmp = _ql.pred(tmp))
            {
                if (c.check(tmp))
                    return tmp;
            }

            // if no Entity fulfills to the condition just return null
            return null;

        }

        /// <summary>
        /// Returns the entity enqueued directly before the given Entity in the queue. If the given Entity is
        /// not contained in this queue or is at the first position thus having no possible predecessor,
        /// <c>null</c> is returned.
        /// </summary>
        /// <param name="e">An Entity in the queue</param>
        /// <returns>The Entity directly before the given Entity in the queue or <c>null</c>.</returns>
        public E pred(E e)
        {

            if (e == null)
            {
                sendWarning("Can not find predecessor of Entity in Queue!",
                        "Queue : " + getName() + " Method: Entity pred(E e)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when querying for Entities");
                return null; // no proper parameter
            }

            return _ql.pred(e);

        }

        /// <summary>
        /// Returns the entity enqueued before the given Entity in the queue that also fulfills the condition
        /// given. If the given Entity is not contained in this queue or is at the first position thus having
        /// no possible predecessor, <c>null</c> is returned. If no other Entity before the given one
        /// fulfills the condition, <c>null</c> is returned, too.
        /// </summary>
        /// <param name="e">An Entity in the queue</param>
        /// <param name="c">The condition that the preceeding Entity has to fulfill</param>
        /// <returns>
        /// The Entity before the given Entity in the queue fulfilling the condition or <c>null</c>.
        /// </returns>
        public E pred(E e, Condition<E> c)
        {

            if (e == null)
            {
                sendWarning("Can not find predecessor of Entity in Queue!",
                        "Queue : " + getName()
                                + " Method: Entity pred(E e, Condition c)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when querying "
                                + "for Entities");
                return null; // no proper parameter
            }

            if (c == null)
            {
                sendWarning(
                        "Can not return previous Entity fulfilling the condition!",
                        "Queue : " + getName()
                                + " Method: Entity pred(E e, Condition c)",
                        "The Condition 'c' given as parameter is a null reference!",
                        "Check to always have valid references when querying Queues.");
                return null; // no proper parameter
            }

            for (E tmp = pred(e); tmp != null; tmp = pred(tmp))
            {
                if (c.check(tmp))
                    return tmp;
            }

            return null; // obviously not found here, empty or doesn't fulfill
        }

        /// <summary>
        /// Removes the given Entity from the Queue. If the given Entity is not in the Queue, a warning will
        /// be issued but nothing else will be changed.
        /// </summary>
        /// <param name="e">The Entity to be removed</param>
        public void remove(Entity e)
        {

            if (e == null)
            {
                sendWarning("Can not remove Entity from Queue!", "Queue : "
                        + getName() + " Method:  void remove(Entity e)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when removing "
                                + "Entities");
                return; // no proper parameter
            }
            if (!_ql.remove((E)e))
            { // watch out, already removed as a side
              // effect!!!
                sendWarning("Can not remove Entity from Queue!", "Queue : "
                        + getName() + " Method:  void remove(Entity e)",
                        "The Entity 'e' (" + e.getQuotedName() + ") given as parameter is not enqueued in this "
                                + "queue!",
                        "Make sure the entity is inside the queue you want it to "
                                + "be removed.");
                return; // not enqueued here
            }

            if (currentlySendDebugNotes())
            {
                sendDebugNote("remove " + e.getQuotedName() + "<br>"
                        + _ql.toString());
            }

            // produce trace output
            if (currentlySendTraceNotes())
            {
                if (e == currentEntity() && currentEntityAll().Count == 1)
                {
                    sendTraceNote("removes itself from " + this.getQuotedName());
                }
                else
                {
                    sendTraceNote("removes " + e.getQuotedName() + " from "
                            + this.getQuotedName());
                }
            }

        }

        /**
         * Removes all entities from the Queue. Has no effect on empty queues.
         */
        public void removeAll()
        {

            while (!isEmpty())
            {
                removeFirst();
            }
        }

        /// <summary>
        /// Removes the first entity from the queue and provides a reference to this entity. If the queue is
        /// empty, <c>null</c> is returned.
        /// </summary>
        /// <returns>
        /// The first entity in this queue, which has been removed, or <c>null</c> in case the queue was
        /// empty
        /// </returns>
        public E removeFirst()
        {

            E first = this.first();

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
        /// Removes the first entity from the queue that fulfills the given condition. Also provides a
        /// reference to this entity. If the queue does not contain an entity that fulfills the condition
        /// (e.g. if the queue is empty), <c>null</c> is returned.
        /// </summary>
        /// <param name="c">The condition that the entity returned must fulfill</param>
        /// <returns>
        /// The first entity in this queue fulfilling the condition, which has been removed from the queue.
        /// <c>Null</c> in case no entity fulfills the condition.
        /// </returns>
        public E removeFirst(Condition<E> c)
        {

            if (c == null)
            {
                sendWarning(
                        "Can not remove the first entity fulfilling a condition!",
                        "Queue : " + getName() + " Method: void removeFirst(Condition c)",
                        "The Condition 'c' given as parameter is a null reference!",
                        "Check to always have valid references when querying Queues.");
                return null; // no proper parameter
            }

            E first = this.first(c);

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
        /// Removes the last entity from the queue and provides a reference to this entity. If the queue is
        /// empty, <c>null</c> is returned.
        /// </summary>
        /// <returns>
        /// The last entity in this queue, which has been removed, or <c>null</c> in case the queue was empty
        /// </returns>
        public E removeLast()
        {

            E last = this.last();

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
        /// Removes the last entity from the queue that fulfills to the given condition, determined by
        /// traversing the queue from last to first until an entity fulfilling the condition is found. Also
        /// provides a reference to this entity. If the queue does not contain an entity that fulfills the
        /// condition (e.g. if the queue is empty), <c>null</c> is returned.
        /// </summary>
        /// <param name="c">The condition that the entity returned must fulfill</param>
        /// <returns>
        /// The last entity in this queue fulfilling the condition, which has been removed from the queue.
        /// <c>Null</c> in case no entity fulfills the condition.
        /// </returns>
        public E removeLast(Condition<E> c)
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

            E last = this.last(c);

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
        /// Removes the entity queued at the given position. The first position is 0, the last one
        /// length()-1.
        /// </summary>
        /// <returns>
        /// : The method returns <c>true</c> if an <c>Entity</c> exists at the given position or <c>false</c>
        /// if otherwise.
        /// </returns>
        public bool remove(int index)
        {
            if (index < 0 || index >= this.length()) return false;

            E e = get(index);
            if (e == null)
            {
                return false;
            }
            else
            {
                remove(e);
                return true;
            }
        }

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
        /// Sets the sort order of this Queue to a new value and makes this Queue use another
        /// <c>QueueList</c> with the specified queueing discipline. Please choose a constant from
        /// <c>QueueBased</c> (<c>QueueBased.FIFO</c>, <c>QueueBased.FIFO</c> or <c>QueueBased.Random</c>)
        /// The sort order of a Queue can only be changed if the queue is empty.
        /// </summary>
        /// <param name="sortOrder">
        /// determines the sort order of the underlying <c>QueueList</c> implementation
        /// (<c>QueueBased.FIFO</c>, <c>QueueBased.FIFO</c> or <c>QueueBased.Random</c>)
        /// </param>
        public void setQueueStrategy(int sortOrder)
        {

            // check if the queue is empty
            if (!isEmpty())
            {
                sendWarning(
                        "The Queue for which the queueing discipline should be "
                                + "changed is not empty. The queueing discipline will remain unchanged!",
                        GetType() + ": " + getQuotedName()
                                + ", Method: "
                                + "void setQueueStrategy(int sortOrder)",
                        "The Queue already contains some entities ordered according a "
                                + "certain order.",
                        "Make sure to change the sort order only for an empty ProcessQueue.");

                return; // ignore that rubbish and just return
            }

            // determine the queueing strategy
            switch (sortOrder)
            {
                case QueueBased.FIFO:
                    _ql = new QueueListFifo<E>(); break;
                case QueueBased.LIFO:
                    _ql = new QueueListLifo<E>(); break;
                case QueueBased.RANDOM:
                    _ql = new QueueListRandom<E>(); break;
                default:
                    sendWarning(
                            "The given sortOrder parameter is negative or too big! "
                                    + "The sort order of the ProcessQueue will remain unchanged!",
                            GetType() + ": " + getQuotedName()
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
        /// Returns the entity enqueued directly after the given Entity in the queue. If the given Entity is
        /// not contained in this queue or is at the last position thus having no possible successor,
        /// <c>null</c> is returned.
        /// </summary>
        /// <param name="e">An Entity in the queue</param>
        /// <returns>The Entity directly after the given Entity in the queue or <c>null</c>.</returns>
        public E succ(E e)
        {

            if (e == null)
            {
                sendWarning("Can not find successor of Entity in Queue!",
                        "Queue : " + getName() + " Method: Entity succ(E e)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when querying for Entities");
                return null; // no proper parameter
            }

            return _ql.succ(e);

        }

        /// <summary>
        /// Returns the entity enqueued after the given Entity in the queue that also fulfills the condition
        /// given. If the given Entity is not contained in this queue or is at the last position thus having
        /// no possible successor, <c>null</c> is returned. If no other Entity after the given one fulfills
        /// the condition, <c>null</c> is returned, too.
        /// </summary>
        /// <param name="e">An Entity in the queue</param>
        /// <param name="c">The condition that the succeeding Entity has to fulfill</param>
        /// <returns>
        /// The Entity after the given Entity in the queue fulfilling the condition or <c>null</c>.
        /// </returns>
        public E succ(E e, Condition<E> c)
        {

            if (e == null)
            {
                sendWarning("Can not find predecessor of Entity in Queue!",
                        "Queue : " + getName()
                                + " Method: Entity succ(E e, Condition c)",
                        "The Entity 'e' given as parameter is a null reference!",
                        "Check to always have valid references when querying for Entities");
                return null; // no proper parameter
            }

            if (c == null)
            {
                sendWarning(
                        "Can not return previous Entity fulfilling the condition!",
                        "Queue : " + getName()
                                + " Method: Entity succ(E e, Condition c)",
                        "The Condition 'c' given as parameter is a null reference!",
                        "Check to always have valid references when querying Queues.");
                return null; // no proper parameter
            }

            for (E tmp = succ(e); tmp != null; tmp = succ(tmp))
            {
                if (c.check(tmp))
                    return tmp;
            }

            return null; // obviously not found here, empty or doesn't fulfills

        }

        /// <summary>
        /// Returns the underlying queue implementation, providing access to the QueueList implementation,
        /// e.g. to add PropertyChangeListeners.
        /// </summary>
        /// <returns>The underlying queue implementation of this Queue.</returns>
        public QueueList<E> getQueueList()
        {

            return _ql; // that's all
        }


        /// <summary>
        /// Sets the seed of the underlying queue list's pseudo random number generator. Useful for queues
        /// with random sort order only; to other queues, calling this method has no effect, resulting in a
        /// warning.
        /// </summary>
        /// <param name="newSeed">
        /// new seed of the underlying queue list's pseudo random number generator
        /// </param>
        public void setSeed(long newSeed)
        {
            if (_ql is QueueListRandom<E>)
            {
                ((QueueListRandom<E>)_ql).setSeed(newSeed);
            }
            else
            {
                sendWarning(
                        "Cannot set seed of queue!",
                        "Queue : " + getName()
                                + " Method: setSeed(long newSeed)",
                        "The queue does not randomize entries.",
                        "Make sure to call setSeed(long newSeed) " +
                                "on queues with <tt>sortOrder == QueueBased.RANDOM</tt> only.");
            }
        }

        public IEnumerator<E> GetEnumerator()
        {
            return _ql.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        //  /**
        //   * Returns an iterator over the entities enqueued.
        //   *
        //   * @return java.lang.Iterator&lt;E&gt; : An iterator over the entities enqueued.
        //   */
        //  public E<E> iterator()
        //  {
        //      return new QueueIterator(this);
        //  }

        //  /**
        //* Private queue iterator, e.g. required for processing all queue elements in a 
        //* for-each-loop.
        //*/
        //  private class QueueIterator implements Iterator<E> {

        //      Queue<E> clientQ;
        //      E next, lastReturned;


        //      public QueueIterator(Queue<E> clientQ)
        //  {
        //      this.clientQ = clientQ;
        //      next = clientQ.first();
        //      lastReturned = null;
        //  }
        //  public boolean hasNext()
        //  {
        //      return next != null;
        //  }
        //  public E next()
        //  {
        //      lastReturned = next;
        //      next = clientQ.succ(next);
        //      return lastReturned;
        //  }
        //  public void remove()
        //  {
        //      clientQ.remove(lastReturned);
        //  }

        //      public IEnumerator<E> GetEnumerator()
        //      {
        //          throw new NotImplementedException();
        //      }

        //      IEnumerator IEnumerable.GetEnumerator()
        //      {
        //          throw new NotImplementedException();
        //      }
        //  }
    }
}
