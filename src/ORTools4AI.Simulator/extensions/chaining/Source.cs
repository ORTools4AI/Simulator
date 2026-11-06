// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.report;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
  * The source creates new Entities and send them to the successor. The Sink inherits all behavior from the station except the possible restriction of
  * the buffer queue. The user has to override the createEntity Method the create the Entity the source should send to the successor.
  * 
  * @param <E>
  *            The Entitiy which is created by the source
  */
    public abstract class Source<E> : Station<E>, HasSuccessor<E>
    where E : Entity, new()
    {

        /**
         * Dummy entity class used internally
         * 
         * @author Malte Unkrig
         * 
         */
        class SourceDummyEntity : Entity
        {


            public SourceDummyEntity(Model model) : base(model, "SourceDummyEntity", false)
            {

            }

        }

        /**
         * Helper class for handling the successor of this station
         */
        private SuccessorAdministration<E> successorAdministration;

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            successorAdministration.setSuccessor(eventToScheduleWhenFinished);
        }

        public void setSuccessor(HasPredecessor<E> successor)
        {
            successorAdministration.setSuccessor(successor);
        }

        private bool started;

        /// <summary>
        /// This Constructor sets the given times, initializes the queues and fills the free service capacity
        /// queue. If a given int is lower than zero the Source assumes that there is no restriction. This
        /// applies to incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities
        /// </summary>
        /// <param name="maxEntitiesToProduce">the number of max entities the station can handle</param>
        /// <param name="parallelProducedEntities">
        /// the number of max parallel entities a station can handle
        /// </param>
        /// <param name="setupTime">the time the station needs to set up</param>
        /// <param name="productionTime">the time the station needs for the service</param>
        /// <param name="recoveryTime">the time the station needs tor recover</param>
        /// <param name="transportTime">
        /// the time the station needs to transport the entitiy to the next station
        /// </param>
        /// <param name="owner">the model owner</param>
        /// <param name="name">the name of the station</param>
        /// <param name="showInReport">should this construct be in the report</param>
        /// <param name="showInTrace">should this construct be in the trace</param>
        public Source(int maxEntitiesToProduce, int parallelProducedEntities, NumericalDist<double> setupTime,
                NumericalDist<double> productionTime, NumericalDist<double> recoveryTime, NumericalDist<double> transportTime,
                Model owner, string name, bool showInReport, bool showInTrace) : base(int.MaxValue, maxEntitiesToProduce, parallelProducedEntities, setupTime, productionTime,
                    recoveryTime, transportTime, owner, name, showInReport, showInTrace)
        {



            successorAdministration = new SuccessorAdministration<E>(getModel(), this.GetType().Name
            + ": " + name);
        }

        /// <summary>Used to run through the Station class to generate proper statistics.</summary>
        /// <returns>a dummy entity</returns>
        //@SuppressWarnings("unchecked")

        private E createDummyEntity()
        {
            return new SourceDummyEntity(getModel()) as E;
        }

        protected abstract E createEntity();

        /**
         * Schedules the successor event with a the created entity and the defined transport time
         */
        //@Override
        protected void onServiceFinisched(E who)
        {
            try
            {
                this.sendTraceNote(getName() + " starts transporting of  " + who.getName()
                        + " to the successor (next station)");
                successorAdministration.getSuccessorEvent().clone().schedule(createEntity(), getTransportTime());
            }
            catch (Exception e)
            {
                throw e;
            }

            insertIncomingEntityIntoQueue(createDummyEntity());
        }

        /**
         * The source starts with creating entities if this method is not called previously.
         */
        public void startCreatingEntities()
        {
            if (started)
            {
                sendWarning("SourceOld already started!", "Source : " + getName()
                        + " Method: startCreatingEntities()",
                        "the source has already started to create entities",
                        "dont call this method more than once on a single object");
            }

            started = true;
            for (int i = 0; i < getRemainingServiceCapacity(); i++)
            {
                getStartEvent().schedule(createDummyEntity(), new TimeSpan(0));
            }
        }

        //@Override
        protected SmartReporter createDefaultReporter()
        {

            SmartReporter reporter = base.createDefaultReporter();
            reporter.overrideValueAt("N/A", 1);
            return reporter;
        }
    }
}
