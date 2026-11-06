// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Johannes G&ouml;bel
// @author based on ideas from Soenke Claassen, Tim Lechler, Justin Neumann
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * Contains the implementation with the java.util.LinkedList to represent queueing
     * functionality. The entities are queued in random order. The statistic data of the queue will be
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
    public class QueueListRandom<E> : QueueListStandard<E>,
        PropertyChangeListener where E : Entity

    {

        /**
         * Uses the java.util.WeakHashMap functionalities to link entities with their entry time.
         */
        private Random _positionGenerator;

        /// <summary>Initializes an empty random-order queue list.</summary>
        /// <remarks>Associate the list with its owning queue through <c>setQueueBased</c>.</remarks>
        public QueueListRandom()
        {
            //super();

            // set the abbreviation for this kind of queueing discipline
            this.abbreviation = "RANDOM";

            // set position generator
            this._positionGenerator = new Random();

            // we don't know the client queue yet.
            // Must be provided later by calling the setQueueBased() method
            clientQ = null;

        }

        /// <summary>
        /// Adds a new Entity to the QueueListStandardFifo. The position on which the entity is inserted is
        /// randomly generated, disregarding priorities.
        /// </summary>
        /// <param name="e">The Entity to add to the QueueListStandardFifo</param>
        public override void insert(E e)
        {

            if (e == null)
            { // check for null reference
                sendWarning(
                        "Can not insert entity. Command ignored.",
                        "Class: QueueListRandom Method: insert(Entity e).",
                        "The Entity reference given as parameter is a null reference.",
                        "Be sure to only use valid references.");
                return;
            }

            if (contains(e))
            { // entity must not be contained twice in queue
                sendWarning("Can not insert entity. Command ignored.",
                        "Class: QueueListRandom Method: insert(Entity e).",
                        "The Entity given as parameter is already enqueued.",
                        "Make sure the entity is not enqueued here by calling "
                                + "method 'contains(Entity e)'.");
                return;
            }

            //int position = this._positionGenerator.Next(this.size() + 1);
            // index between 0 (first) and size() (one position after the last Entity at index size()-1)

            queuelist.AddLast(e);
            e.addQueueBased(this.clientQ); // sets entity's queue as this queued

            statisticalInsert(e); // update statistics

        }

        public override bool insertAfter(E e, E which)
        {
            throw new NotImplementedException();
        }

        public override bool insertBefore(E e, E which)
        {
            throw new NotImplementedException();
        }

        public override E pred(E e)
        {
            throw new NotImplementedException();
        }

        public override bool removeFirst()
        {
            throw new NotImplementedException();
        }

        public override bool removeLast()
        {
            throw new NotImplementedException();
        }

        public override void sendWarning(string description, string location, string reason, string prevention)
        {
            throw new NotImplementedException();
        }

        /// <summary>Sets the seed of this queue list's pseudo random number generator.</summary>
        /// <param name="newSeed">new seed this queue list's pseudo random number generator</param>
        public void setSeed(long newSeed)
        {
            this._positionGenerator = new Random((int)newSeed);
        }

      
    }

    public interface PropertyChangeListener
    {
    }
}

   