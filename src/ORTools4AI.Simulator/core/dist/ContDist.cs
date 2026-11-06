// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, Johannes Goebel
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.dist
{

    /**
     * Superclass for all distributions returning (near-)continuous samples of type <code>Double</code>.
     */
    public abstract class ContDist : NumericalDist<double>
    {

        /// <summary>
        /// Constructs a distribution returning continuously distributed double samples. Note that the method
        /// <c>Double sample()</c> returning the samples (inherited from NumericalDist&lt;N&gt;) has to be
        /// implemented in subclasses.
        /// </summary>
        /// <param name="owner">The distribution's owner</param>
        /// <param name="name">The distribution's name</param>
        /// <param name="showInReport">Flag to show distribution in report</param>
        /// <param name="showInTrace">Flag to show distribution in trace</param>
        public ContDist(Model owner, string name,
                bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {

        }

        /// <summary>
        /// Convenience method to sample a period of random length by creating a <c>TimeSpan</c> whose length
        /// is determined by sampling this distribution (time unit is the experiment's reference units), thus
        /// replacing the pattern <c>new TimeSpan(distribution.sample())</c>.
        /// </summary>
        /// <returns>The TimeSpan sampled from this distribution</returns>
        public TimeSpan sampleTimeSpan()
        {

            if (!this.getModel().isConnected())
            {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "ContDist: " + getName() + " Method: TimeSpan sampleTimeSpan()",
                        "Model not yet connected to an experiment, thus reference time unit and random number seeds not yet set.",
                        "Make sure your model is connected to an experiment before this method is called.");
                return TimeSpan.ZERO;
            }

            // Obtain a sample and return TimeSpan        
            double sampleVal = sample();
            if (sampleVal < 0)
            {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "ContDist: " + getName() + " Method: TimeSpan sampleTimeSpan()",
                        "The distribution returned a negative sample (" + sampleVal + ").",
                        "Make sure to sample TimeSpans from non-negative distributions only.");
                return TimeSpan.ZERO;
            }

            return new TimeSpan(sampleVal);
        }

        /// <summary>
        /// Convenience method to sample a period of random length by creating a <c>TimeSpan</c> whose length
        /// is determined by sampling this distribution (time unit given explicitly as parameter), thus
        /// replacing the pattern <c>new TimeSpan(distribution.sample(), unit)</c>.
        /// </summary>
        /// <param name="unit">the TimeUnit to assign to the sampled value</param>
        /// <returns>The TimeSpan sampled from this distribution</returns>
        public TimeSpan sampleTimeSpan(TimeUnit unit)
        {

            if (unit == null)
            { // no time unit given
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't create TimeSpan object! Simulation aborted.",
                                "ContDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                                "Time unit passed is null",
                                "Make sure to pass a non-null time unit.",
                                null)));
            }

            if (!this.getModel().isConnected())
            {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "ContDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                        "Model not yet connected to an experiment, thus random number seeds not yet set.",
                        "Make sure your model is connected to an experiment before this method is called.");
                return TimeSpan.ZERO;
            }

            // Obtain a sample 
            double sampleVal = sample();
            if (sampleVal < 0)
            {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "ContDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                        "The distribution returned a negative sample (" + sampleVal + ").",
                        "Make sure to sample TimeSpans from non-negative distributions only.");
                return TimeSpan.ZERO;
            }

            // Return a TimeSpan using the appropriate constructor
            return new TimeSpan(sampleVal, unit);
        }
    }
}