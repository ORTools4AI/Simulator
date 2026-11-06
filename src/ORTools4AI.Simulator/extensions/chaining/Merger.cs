// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using System;
using System.Collections.Generic;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
     * The Merger merges the in the Mergerconfig defined entities into the EEx entity. The Merger waits until enough entities of the required types are in
     * the internal partsQueue and than creates a PartsContainer. With this PartsContainer the StationStartevent is scheduled instantly and the common run
     * through the internal Station stages can be conducted.
     *
     * @param <EIn>  the entity which comes in to the Merger
     * @param <EEx>  the entity which leaves the Merger
     */
    public abstract class Merger<EIn, EEx> : Station<EIn>,
            HasPredecessor<EIn>, HasSuccessor<EEx>
        where EIn : Entity
        where EEx : Entity
    {

        /**
         * The MergerStartEvent takes an incoming entity and puts it into the queue for the entity's type. If there is no queue for the entity's type the
         * entity is discarded. If enough entities of the required ratio to create the outgoing entity are available, the StationStartEvent is scheduled.
         */
        class MergerStartEvent : Event
        {
            private readonly Merger<EIn, EEx> _merger;

            public MergerStartEvent(Model owner, string name, Merger<EIn, EEx> merger)
                : base(owner, name, false)
            {
                _merger = merger;
            }

            public override void eventRoutine(Entity who)
            {
                core.engine.Queue<EIn> partsQueue = _merger.getPartsQueueForIncomingEntityClass(who.GetType());

                if (partsQueue == null)
                {
                    sendWarning(
                        "The Merger received a non specified entity and did not handle it",
                        "MergerStartEvent : " + getName() + " Method: eventRoutine()",
                        "The received entity is not defined in the MergerConfig",
                        "Most likely you forgot to define the MergerConfig properly or the Merger is not appropriately connected to the previous construct");

                    throw new Exception("No PartsQueue found for entity type: " + who.GetType().Name);
                }

                partsQueue.insert((EIn)who);

                PartsContainer partsContainer = _merger.tryToComposePartsContainerFromAvailableParts();
                if (partsContainer != null)
                {
                    _merger.scheduleStationStartEventForPartsContainer(partsContainer);
                }
            }
        }

        /**
         * Utility class which is created to run through the Station if enough entities are in the internal parts queues.
         * Keys are runtime Types; values are the lists of entities of that type collected for one merge cycle.
         */
        public class PartsContainer : Entity
        {
            private Dictionary<Type, List<EIn>> parts;

            public PartsContainer(Model model, Dictionary<Type, List<EIn>> parts)
                : base(model, "PartsContainer", true)
            {
                this.parts = parts;
            }

            public Dictionary<Type, List<EIn>> getParts() => parts;
        }

        /**
         * a map of queues which contains the incoming entities separated by runtime type of the entity
         */
        private Dictionary<Type, core.engine.Queue<EIn>> incomingEntitiesQueueContainer;

        /**
         * the MergerConfig which holds the required entity types and ratios
         */
        private readonly MergerConfig<EIn> mergerConfig;

        /**
         * Helper class for handling the successor of this station
         */
        private SuccessorAdministration<EEx> successorAdministration;

        public Merger(MergerConfig<EIn> mergerConfig, int incomingBufferQueueCapacity, int maxEntitiesToHandle,
                int parallelHandledEntities, NumericalDist<double> setupTime, NumericalDist<double> serviceTime,
                NumericalDist<double> recoveryTime, NumericalDist<double> transportTime, Model owner, string name,
                bool showInReport, bool showInTrace)
            : base(incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities, setupTime,
                  serviceTime, recoveryTime, transportTime, owner, name, showInReport, showInTrace)
        {
            this.mergerConfig = mergerConfig;

            incomingEntitiesQueueContainer = new Dictionary<Type, core.engine.Queue<EIn>>();

            foreach (var pk in mergerConfig)
            {
                incomingEntitiesQueueContainer.Add(pk.Key, new core.engine.Queue<EIn>(
                    getModel(), getName() + "Incoming" + pk.Key.Name + "PufferQueue", false, false));
            }

            successorAdministration = new SuccessorAdministration<EEx>(
                getModel(), typeof(Merger<EIn, EEx>).Name + ": " + name);
        }

        /// <summary>
        /// Gets the queue for the given entity type. If no exact match is found, searches for a registered
        /// superclass.
        /// </summary>
        /// <param name="entityType">the runtime type to look up</param>
        /// <returns>the appropriate parts queue, or null if not found</returns>
        private core.engine.Queue<EIn> getPartsQueueForIncomingEntityClass(Type entityType)
        {
            if (incomingEntitiesQueueContainer.TryGetValue(entityType, out var partsQueue))
                return partsQueue;

            foreach (var registeredType in incomingEntitiesQueueContainer.Keys)
            {
                if (registeredType.IsAssignableFrom(entityType))
                    return incomingEntitiesQueueContainer[registeredType];
            }

            return null;
        }

        /**
         * Returns the start event of the Merger (overrides Station.getStartEvent).
         */
        public new Event getStartEvent()
        {
            return new MergerStartEvent(getModel(), getName() + "_MergerStartEvent", this);
        }

        /// <summary>
        /// Must be overridden by the user to create the outgoing entity from the collected parts.
        /// </summary>
        /// <param name="parts">map from runtime type to list of entities of that type</param>
        /// <returns>the outgoing merged entity</returns>
        protected abstract EEx mergeEntities(Dictionary<Type, List<EIn>> parts);

        /**
         * Called when service is finished. Creates the merged output entity and schedules the successor event.
         */
        protected void onServiceFinisched(PartsContainer who)
        {
            EEx mergedEntity = mergeEntities(who.getParts());

            sendTraceNote(getName() + " starts transporting of  " + mergedEntity.getName()
                + " to the successor (next station)");

            successorAdministration.getSuccessorEvent().clone().schedule(mergedEntity, getTransportTime());
        }

        /**
         * Schedules the StationStartEvent with the internally created PartsContainer.
         */
        private void scheduleStationStartEventForPartsContainer(PartsContainer partsContainer)
        {
            base.getStartEvent().schedule(partsContainer, new TimeSpan(0));
        }

        /// <summary>Tries to compose a PartsContainer from entities currently in the parts queues.</summary>
        /// <returns>the PartsContainer on success, or null if not enough entities are available</returns>
        private PartsContainer tryToComposePartsContainerFromAvailableParts()
        {
            foreach (var entityType in mergerConfig.Keys)
            {
                int requiredCount = mergerConfig[entityType];
                var queue = getPartsQueueForIncomingEntityClass(entityType);
                if (queue == null || requiredCount > queue.length())
                    return null;
            }

            var allParts = new Dictionary<Type, List<EIn>>();

            foreach (var entityType in mergerConfig.Keys)
            {
                int requiredCount = mergerConfig[entityType];
                var partsQueue = getPartsQueueForIncomingEntityClass(entityType);
                var parts = new List<EIn>();

                for (int i = 0; i < requiredCount; i++)
                {
                    EIn entity = partsQueue.first();
                    partsQueue.remove(entity);
                    parts.Add(entity);
                }

                allParts.Add(entityType, parts);
            }

            return new PartsContainer(getModel(), allParts);
        }

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            successorAdministration.setSuccessor(eventToScheduleWhenFinished);
        }

        public void setSuccessor(HasPredecessor<EEx> successor)
        {
            successorAdministration.setSuccessor(successor);
        }
    }
}
