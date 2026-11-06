// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * An interface for listeners interested in status changes during an experiment run.
     * Notified when the assigned simulation experiment is (re)started, paused, or stopped.
     */
    public interface ExperimentListener : SimRunListener
    {
        /** Called when the experiment is (re)started. */
        void experimentRunning(SimRunEvent e);

        /** Called when the experiment is finally stopped. */
        void experimentStopped(SimRunEvent e);

        /** Called when the experiment is temporarily paused. */
        void experimentPaused(SimRunEvent e);
    }
}
