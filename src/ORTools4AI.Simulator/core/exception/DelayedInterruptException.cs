// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Malte Unkrig
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.exception
{


    /**
     * If a DelayedInterruptException is caught in the
     * {@link SimProcess#lifeCycle()} method of a process this means that the
     * process has been interrupted from "within" due to a previously scheduled
     * delayed interrupt. DelayedInterruptExceptions are used
     * in with {@link SimProcess#interruptDelayed(TimeInstant)} and
     * {@link SimProcess#interruptDelayed(TimeSpan)}.<br />
     * 
     * @see InterruptException
     * @see InterruptCode
     * @see SimProcess#interruptDelayed(TimeSpan)
     * @see SimProcess#interruptDelayed(TimeInstant)
     */
    public class DelayedInterruptException : InterruptException
    {
        private static readonly long serialVersionUID = 1L;

        /// <summary>Constructs a DelayedInterruptException with the given InterruptCode.</summary>
        /// <param name="interruptCode">
        /// The InterruptCode to be contained within this DelayedInterruptException.
        /// </param>
        public DelayedInterruptException(InterruptCode interruptCode):base(interruptCode)
        {
           // super(interruptCode);
        }
    }
}
