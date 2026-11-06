// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.report;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;


namespace ORTools4AI.Simulator.extensions.chaining.abstractions
{

    /* The Station is an abstract super class of all chaining constructs.This class handles most of the processing code like the incoming buffer queue,
    * setup queue and being handled queue.It contains the settings for maxEntitiesToHandle, parallelHandledEntities, setupTime, serviceTime,
    * recoveryTime, and transportTime
    *
    * @param<E>
    *            The Entity which can be handled by the station
    */
    public abstract class Station<E> : Reportable where E : Entity
    {
        /**
         * the max entities a station can handle
         */
        private readonly int maxEntitiesToHandle;
        /**
         * the entities a station has handled so far
         */
        private int handledEntitiesSoFar;
        /**
         * the setuptime a station needs to set up the entity
         */
        private readonly NumericalDist<double> setupTime;
        /**
         * the service time a entitiy is serviced
         */
        private readonly NumericalDist<double> serviceTime;

        /**
         * the time needed to send the entitiy to the succsessor
         */
        private readonly NumericalDist<double> transportTime;
        /**
         * the buffer queue of incomming entitites
         */
        private readonly core.engine.Queue<E> incomingBufferQueue;
        /**
         * the setup queue of entities which are in setup
         */
        private readonly core.engine.Queue<E> setupQueue;
        /**
         * the queue of entities which are in service
         */
        private readonly core.engine.Queue<E> entitiesBeeingServicedQueue;
        /**
         * the queue of free capacities
         */
        private readonly core.engine.Queue<ServiceCapacity> freeServiceCapacityQueue;
        /**
         * the recovery time which a station needsto handle a the next entitiy
         */
        private readonly NumericalDist<double> recoveryTime;
        /**
         * the capacity of max parallel handled entities of a station
         */
        private readonly int maxAvailableServiceCapacity;

        /// <summary>
        /// This Constructor sets the given times, initializes the queues and fills the free service capacity
        /// queue. This Construktor is never called directly but rather via a super call of the inheriting
        /// classes. If a given int is lower than zero the station assumes that there is no resriction. This
        /// applies to incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities
        /// </summary>
        /// <param name="incomingBufferQueueCapacity">the capatiy of the income queue</param>
        /// <param name="maxEntitiesToHandle">the number of max entities the station can handle</param>
        /// <param name="parallelHandledEntities">
        /// the number of max parallel entities a station can handle
        /// </param>
        /// <param name="setupTime">the time the station needs to set up</param>
        /// <param name="serviceTime">the time the station needs for the service</param>
        /// <param name="recoveryTime">the time the station needs tor recover</param>
        /// <param name="transportTime">
        /// the time the station needs to transport the entitiy to the next station
        /// </param>
        /// <param name="owner">the model owner</param>
        /// <param name="name">the name of the station</param>
        /// <param name="showInReport">should this construct be in the report</param>
        /// <param name="showInTrace">should this construct be in the trace</param>
        public Station(int incomingBufferQueueCapacity, int maxEntitiesToHandle, int parallelHandledEntities,
                NumericalDist<double> setupTime, NumericalDist<double> serviceTime, NumericalDist<double> recoveryTime,
                NumericalDist<double> transportTime, Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {

            if (incomingBufferQueueCapacity < 0)
            {
                incomingBufferQueueCapacity = int.MaxValue;
            }

            if (maxEntitiesToHandle < 0)
            {
                maxEntitiesToHandle = int.MaxValue;
            }

            if (parallelHandledEntities < 0)
            {
                parallelHandledEntities = int.MaxValue;
            }

            this.maxEntitiesToHandle = maxEntitiesToHandle;
            this.maxAvailableServiceCapacity = parallelHandledEntities;

            this.setupTime = setupTime;
            this.serviceTime = serviceTime;
            this.recoveryTime = recoveryTime;
            this.transportTime = transportTime;

            incomingBufferQueue = new core.engine.Queue<E>(getModel(), name + "PufferQueue", QueueBased.FIFO,
                    incomingBufferQueueCapacity, false, false);
            setupQueue = new core.engine.Queue<E>(getModel(), name + "SetupQueue", false, false);
            entitiesBeeingServicedQueue = new core.engine.Queue<E>(getModel(), name + "HandleQueue", false, false);
            freeServiceCapacityQueue = new core.engine.Queue<ServiceCapacity>(getModel(), name + "FreeServiceCapacityQueue",
                    false, false);

            for (int i = 0; i < parallelHandledEntities; i++)
            {
                releaseServiceCapacity(); // fill up capacity queue
            }
            freeServiceCapacityQueue.reset();

        }

        /// <summary>Checks if another entitiy can handled by the station</summary>
        /// <returns>true if anthoder entity can be handled</returns>
        private bool canAnotherEntityBeHandled()
        {
            return handledEntitiesSoFar < maxEntitiesToHandle;
        }

        /**
         * occupies a service capacity
         */
        private void claimServiceCapacity()
        {
            var cap = freeServiceCapacityQueue.first();
            if (cap == null)
            {
                throw new Exception("Internal Error. Tried to remove none existing service capacity.");
            }
            freeServiceCapacityQueue.remove(cap);
        }

        /**
         * Creates an SmartReporter and fills it with the relevant statistics of the station
         */

        protected  SmartReporter createDefaultReporter()
        {
            List<HeaderValuePair> pairs;

            pairs = new List<HeaderValuePair>();

            pairs.Add(new HeaderValuePair("Title", getName()));
            pairs.Add(new HeaderValuePair("In", incomingBufferQueue.getObservations()
                    + incomingBufferQueue.length()));
            pairs.Add(new HeaderValuePair("Out", entitiesBeeingServicedQueue.getObservations()));
            pairs.Add(new HeaderValuePair("Waiting", incomingBufferQueue.length()));
            pairs.Add(new HeaderValuePair("In progress", entitiesBeeingServicedQueue.length()
                    + setupQueue.length()));
            pairs.Add(new HeaderValuePair("max.WaitQ", incomingBufferQueue.maxLength()));
            pairs.Add(new HeaderValuePair("avg.WaitQ", incomingBufferQueue.averageLength()));
            pairs.Add(new HeaderValuePair("max.WaitT", incomingBufferQueue.maxWaitTime()));
            pairs.Add(new HeaderValuePair("avg.WaitT", incomingBufferQueue.averageWaitTime()));
            pairs.Add(new HeaderValuePair("avg.SetupT", setupQueue.averageWaitTime()));
            pairs.Add(new HeaderValuePair("avg.ProcessT", entitiesBeeingServicedQueue.averageWaitTime()));
            pairs.Add(new HeaderValuePair("max.avail.Capacity", maxAvailableServiceCapacity));
            pairs.Add(new HeaderValuePair("max.used.Capacity", maxAvailableServiceCapacity
                    - freeServiceCapacityQueue.minLength()));
            pairs.Add(new HeaderValuePair("avg.used.Capacity", maxAvailableServiceCapacity
                    - freeServiceCapacityQueue.averageLength()));

            return new SmartReporter("ServiceStations", 5100, pairs, this);
        }

        /// <summary>gets the recovery time of the station</summary>
        /// <returns>the recovery time</returns>
        private TimeSpan getRecoveryTime()
        {
            if (recoveryTime == null)
            {
                return new TimeSpan(0);
            }
            else
            {
                return recoveryTime.sampleTimeSpan();
            }
        }

        /// <summary>gets the remaining service capacity</summary>
        /// <returns>the remaining service capacity</returns>
        protected int getRemainingServiceCapacity()
        {
            return freeServiceCapacityQueue.length();
        }

        /// <summary>gets the service end</summary>
        /// <returns>the servcie end</returns>
        private ServiceEndEvent<E> getServiceEndEvent()
        {
            return new ServiceEndEvent<E>(getModel(), getName() + "_ServiceEndEvent", this);
        }

        /// <summary>gets the service time of the station</summary>
        /// <returns>the service time</returns>
        private core.engine.TimeSpan getServiceTime()
        {
            if (serviceTime == null)
            {
                return new TimeSpan(0);
            }
            else
            {
                return serviceTime.sampleTimeSpan();
            }
        }

        /// <summary>gets the setup time of the station</summary>
        /// <returns>the setup time</returns>
        protected TimeSpan getSetupTime()
        {
            if (setupTime == null)
            {
                return new TimeSpan(0);
            }
            else
            {
                return setupTime.sampleTimeSpan();
            }
        }

        /// <summary>gets the start event of this station</summary>
        /// <returns>the start event</returns>
        public Event getStartEvent()
        {
            return new StationStartEvent<E>(getModel(), getName() + "_StationStartEvent", this);
        }

        /// <summary>gets the transport time of the station</summary>
        /// <returns>the transport time</returns>
        protected TimeSpan getTransportTime()
        {
            if (transportTime == null)
            {
                return new TimeSpan(0);
            }
            else
            {
                return transportTime.sampleTimeSpan();
            }
        }

        /// <summary>inserts an incoming entitiy into the buffer queue</summary>
        /// <param name="entityToInsert">the entitiy to be inserted</param>
        /// <returns>true if the entitiy is inserted succsessfully</returns>
        public bool insertIncomingEntityIntoQueue(E entityToInsert)
        {
            if (incomingBufferQueue.length() < incomingBufferQueue.getQueueLimit())
            {
                // Puffer voll Warnung ausgeben
            }

            return incomingBufferQueue.insert(entityToInsert);
        }

        /// <summary>checks if service capaciy is remaining</summary>
        /// <returns>true if capacity is remaining</returns>
        private bool isServiceCapacityRemaining()
        {
            return getRemainingServiceCapacity() > 0;
        }

        /// <summary>this method can be overridden by an inherited class</summary>
        /// <param name="who">the entitiy of the scheduled event</param>
        protected virtual void onServiceFinisched(E who)
        {
            // Overridable
        }

        /**
         * this method releases service capacity
         */
       private void releaseServiceCapacity()
        {
            freeServiceCapacityQueue.insert(new ServiceCapacity(this.getModel()));
        }

        /**
         * If the Station can handle another entity and service capacity is remaining this method removes the first entity from the buffer queue,
         * increases handledEntitiesSoFar, insert the entity in the setupQueue and claims service capacity. Than the service start event is scheduled.
         * 
         */
        private void scheduleServiceStartEventIfPossible()
        {
            bool canHandleAnotherEntity = canAnotherEntityBeHandled();
            bool parallelHandlingCapacityRemaining = isServiceCapacityRemaining();

            if (canHandleAnotherEntity && parallelHandlingCapacityRemaining)
            {
                E entityToHandle;

                entityToHandle = incomingBufferQueue.first();

                incomingBufferQueue.remove(entityToHandle);
                setupQueue.insert(entityToHandle);
                handledEntitiesSoFar++;
                claimServiceCapacity();

                this.sendTraceNote(getName() + " inserts " + entityToHandle.getName()
                        + " into internal setup queue to wait for progressing");

                new ServiceStartEvent<E>(getModel(), getName() + "_ServiceStartEvent", this).schedule(
                        entityToHandle, getSetupTime());
            }
        }


        /**
     * Event which is called after the servicestart event. The entitiy is removed from the entitiesBeeingServicedQueue and onServiceFinisched is
     * called. An inheriting class can override the onServiceFinisched Method in which the succsessor event is scheduled. Than ServiceEndEvent
     * schedules a ServiceCapacityReleasedEvent with the defined recoveryTime.
     * 
     * @author Christian Mentz
     * 
     */
        class ServiceEndEvent<E> : Event where E : Entity
        {

            private readonly Station<E> _station;
            public ServiceEndEvent(Model owner, string name, Station<E> station) : base(owner, name, false)
            {
                _station = station;
            }


            public override void eventRoutine(Entity who)
            {
                _station.entitiesBeeingServicedQueue.remove(who);
                _station.sendTraceNote(_station.getName() + " finishes processing of  " + who.getName()
                        + "and removes it from internal handle queue");

                _station.onServiceFinisched((E)who);

                new ServiceCapacityReleasedEvent<E>(getModel(), _station.getName()
                        + "ServiceCapacityReleasedEvent", _station).schedule(_station.getRecoveryTime());
            }
        }

        /**
     * This event is the service event. It removes the entity from the setup queue and inserts it in the entitiesBeeingServicedQueue. Than the service
     * end is scheduled with the prevoius defined timespan.
     * 
     * @author Christian Mentz
     * 
     */
        private class ServiceStartEvent<E> : Event where E : Entity
        {

            private readonly Station<E> _station;
            public ServiceStartEvent(Model owner, string name, Station<E> station) : base(owner, name, false)
            {
                _station = station;
            }


            public override void eventRoutine(Entity who)
            {
                _station.setupQueue.remove(who);
                _station.entitiesBeeingServicedQueue.insert((E)who);
                _station.sendTraceNote(_station.getName() + " inserts " + who.getName()
                        + " into internal handle queue and starts progressing");
               _station.getServiceEndEvent().schedule(who,_station.getServiceTime());
            }
        }

        /**
    * This event is beeing called at first when a predecessor is finished with his process. The passed entitiy is inserted in the buffer queue and
    * the service start is called if possible.
    * 
    * @author Christian Mentz
    * 
    */
        private class StationStartEvent<E> : Event where E : Entity
        {

            private readonly Station<E> _station;

            public StationStartEvent(Model owner, string name, Station<E> station) : base(owner, name, false)
            {
                _station = station;
            }



            public override void eventRoutine(Entity who)
            {
                bool entityInsertedIntoBufferQueue;

                entityInsertedIntoBufferQueue = _station.insertIncomingEntityIntoQueue((E)who);

                if (entityInsertedIntoBufferQueue)
                {
                    _station.scheduleServiceStartEventIfPossible();
                }
            }
        }

        /**
       * A class which is used to handle the free capacity of a station
       * 
       * @author Christian Mentz
       * 
       */

        class ServiceCapacity : Entity
        {
            //private Station<E> _station;
            /**
             * Constructor
             */
            public ServiceCapacity(Model model) : base(model, "ServiceCapacity", false)
            {

            }

        }

        /**
         * Event which is called after a service is finished. It realeases a used capacity and calls scheduleServiceStartEventIfPossible() to handle the
         * remaining entities in the buffer queue.
         * 
         * @author Christian Mentz
         * 
         */
        class ServiceCapacityReleasedEvent<E> : ExternalEvent where E : Entity
        {
            private readonly Station<E> _station;

            /**
             * Constructor
             */
            public ServiceCapacityReleasedEvent(Model owner, string name, Station<E> station) : base(owner, name, false)
            {
                _station = station;
            }


            public override void eventRoutine()
            {
                _station.releaseServiceCapacity();// Inserts a service capacity to the queue
                if (!_station.incomingBufferQueue.isEmpty())
                {
                    _station.scheduleServiceStartEventIfPossible();
                }
            }

        }
    }
}
