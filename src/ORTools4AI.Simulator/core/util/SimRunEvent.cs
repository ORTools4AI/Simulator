// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.util
{
    /**
     * Base class for events occurring while a simulation runs.
     * Wraps an Experiment as the event source and provides convenience accessors
     * for the associated model and current simulation time.
     */
    public class SimRunEvent : EventArgs
    {
        private readonly Experiment _source;

        public SimRunEvent(Experiment source)
        {
            _source = source;
        }

        /** Returns the experiment that emitted this event. */
        public Experiment getExperiment() => _source;

        /** Returns the model connected to the assigned experiment. */
        public Model getModel() => _source.getModel();

        /** Returns the source experiment's current simulation time. */
        public TimeInstant getCurrentTime() => getModel().presentTime();
    }
}
