// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
     * The ServiceStation represents a standard operating station which can process entities. It has exactly the same behaviour as the Station but
     * implements HasPredecessor and HasSuccessor so it can be placed between other chaining constructs.
     *
     * @param <E>  The entity which is passed through the ServiceStation
     */
    public class ServiceStation<E> : Station<E>, HasPredecessor<E>, HasSuccessor<E>
        where E : Entity
    {
        private SuccessorAdministration<E> successorAdministration;

        public ServiceStation(int incomingBufferQueueCapacity, int maxEntitiesToHandle,
                int parallelHandledEntities, NumericalDist<double> setupTime, NumericalDist<double> serviceTime,
                NumericalDist<double> recoveryTime, NumericalDist<double> transportTime, Model owner, string name,
                bool showInReport, bool showInTrace)
            : base(incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities, setupTime,
                  serviceTime, recoveryTime, transportTime, owner, name, showInReport, showInTrace)
        {
            successorAdministration = new SuccessorAdministration<E>(
                getModel(), nameof(ServiceStation<E>) + ": " + name);
        }

        protected override void onServiceFinisched(E who)
        {
            sendTraceNote(getName() + " starts transporting of  " + who.getName()
                + " to the successor (next station)");

            successorAdministration.getSuccessorEvent().clone().schedule(who, getTransportTime());
        }

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            successorAdministration.setSuccessor(eventToScheduleWhenFinished);
        }

        public void setSuccessor(HasPredecessor<E> successor)
        {
            successorAdministration.setSuccessor(successor);
        }
    }
}
