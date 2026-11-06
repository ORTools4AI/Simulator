// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using System.ComponentModel;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.dist
{

    public abstract class NumericalDist<N> : Distribution 

    {

        public NumericalDist(Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace) {

        }

        /**
         * Returns the next sample of type N. Uses inverse-transform sampling via
         * getInverseOfCumulativeProbabilityFunction(). Subclasses may override.
         */
        public virtual N sample()
        {
            incrementObservations();
            N result;
            do
            {
                double p = randomGenerator.nextDouble();
                if (antithetic) p = 1.0 - p;
                result = getInverseOfCumulativeProbabilityFunction(p);
            } while (nonNegative && Convert.ToDouble(result) < 0);
            if (currentlySendTraceNotes()) traceLastSample(result!.ToString()!);
            return result;
        }

        /**
         * Maps a probability p in [0,1] to the distribution's domain (quantile/inverse CDF).
         */
        public abstract N getInverseOfCumulativeProbabilityFunction(double p);

        public double GetSampleValue()
        {
            var s = sample();
            // Return a TimeSpan using the appropriate constructor
            if (s is double || s is float)
            {
                return Convert.ToDouble (s);
            }
            else
            {
                return Convert.ToInt64(s);
            }
        }


        /// <summary>
        /// Convenience method to sample a period of random length by creating a <c>TimeSpan</c> whose length
        /// is determined by sampling this distribution (time unit is the experiment's reference units), thus
        /// replacing the pattern <c>new TimeSpan(distribution.sample())</c>.
        /// </summary>
        /// <returns>The TimeSpan sampled from this distribution</returns>
        public TimeSpan sampleTimeSpan() {

            if (!getModel().isConnected()) {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "NumericalDist: " + getName() + " Method: TimeSpan sampleTimeSpan()",
                        "Model not yet connected to an experiment, thus reference time unit and random number seeds not yet set.",
                        "Make sure your model is connected to an experiment before this method is called.");
                return TimeSpan.ZERO;
            }

            // Obtain a sample 
            N sample = this.sample();
            if ( ( sample is not double) ) {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "NumericalDist: " + getName() + " Method: TimeSpan sampleTimeSpan()",
                        "The distribution returned a negative sample (" + Convert.ToDouble(sample) + ").",
                        "Make sure to sample TimeSpans from non-negative distributions only.");
                return TimeSpan.ZERO;
            }

            // Return a TimeSpan using the appropriate constructor
            if (sample is double || sample is float) {
                
                return new TimeSpan(Convert.ToDouble(sample));
            } else {
                return new TimeSpan(Convert.ToInt64(sample));
            }
        }

        /// <summary>
        /// Convenience method to sample a period of random length by creating a <c>TimeSpan</c> whose length
        /// is determined by sampling this distribution (time unit given explicitly as parameter), thus
        /// replacing the pattern <c>new TimeSpan(distribution.sample(), unit)</c>.
        /// </summary>
        /// <param name="unit">the TimeUnit to assign to the sampled value</param>
        /// <returns>The TimeSpan sampled from this distribution</returns>
        public TimeSpan sampleTimeSpan(TimeUnit unit) {

            if (unit == null) { // no time unit given
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't create TimeSpan object! Simulation aborted.",
                                "NumericalDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                                "Time unit passed is null",
                                "Make sure to pass a non-null time unit.",
                                null)));
            }

            if (!getModel().isConnected()) {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "NumericalDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                        "Model not yet connected to an experiment, thus random number seeds not yet set.",
                        "Make sure your model is connected to an experiment before this method is called.");
                return TimeSpan.ZERO;
            }

            // Obtain a sample 
            N sample = this.sample();
            if (sample is not double) {
                sendWarning(
                        "Failed to sample a TimeSpan. Returning TimeSpan.ZERO instead.",
                        "NumericalDist: " + getName() + " Method: TimeSpan sampleTimeSpan(TimeUnit unit)",
                        "The distribution returned a negative sample (" +Convert.ToDouble(sample) + ").",
                        "Make sure to sample TimeSpans from non-negative distributions only.");
                return TimeSpan.ZERO;
            }

            // Return a TimeSpan using the appropriate constructor
            if (sample is double || sample is float) {
                return new TimeSpan(Convert.ToDouble(sample), unit);
            } else {
                return new TimeSpan(Convert.ToInt64( sample), unit);
            }
        }


        /// <summary>
        /// Convenience method to return the distribution's sample as <c>Object</c>. For type safety, method
        /// <c>sample()</c> should be preferred. However, this method is useful for environments requiring a
        /// non-genetic access point to obtain samples from any distribution.
        /// </summary>
        /// <returns>A sample from this this distribution wrapped as <c>Object</c>.</returns>
        public override object sampleObject() {
            return sample();
        }
    }
}