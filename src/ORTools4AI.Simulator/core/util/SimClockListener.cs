// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * A listener to the sim clock of an experiment running in an experiment runner.
     */
    public interface SimClockListener : SimRunListener
    {
        /** Called when the simulation clock is advanced. */
        void clockAdvanced(SimRunEvent e);
    }
}
