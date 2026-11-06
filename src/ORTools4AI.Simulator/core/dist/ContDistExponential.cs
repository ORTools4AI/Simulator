// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
	/**
     * Negative-exponentially distributed stream of pseudo random numbers of type
     * double.
     */
	public class ContDistExponential : ContDist
	{

		/**
		 * The mean value of the negative-exponential distribution.
		 */
		protected double mean;

		/// <summary>
		/// Constructs a simple negative-exponentially distributed pseudo random generator with the given value as
		/// mean of the distribution. Only positive values are allowed.
		/// </summary>
		/// <param name="owner">The distribution's owner</param>
		/// <param name="name">The distribution's name</param>
		/// <param name="mean">The mean value for this distribution</param>
		/// <param name="showInReport">Flag for producing reports</param>
		/// <param name="showInTrace">Flag for producing trace output</param>
		public ContDistExponential(Model owner, string name, double mean,
				bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{

			this.mean = mean;
		}

		/// <summary>Creates the default reporter for the RealDistExponential distribution.</summary>
		/// <returns>The reporter for the RealDistExponential distribution</returns>
		/// <remarks>See also ContDistExponReporter</remarks>
		protected override Reporter createDefaultReporter()
		{

			return new ContDistExponReporter(this);

		}

		/// <summary>
		/// Returns the inverse of the cumulative probability function (quantile function). For the exponential
		/// distribution: F^-1(p) = -ln(1-p) * mean.
		/// </summary>
		/// <param name="p">probability in [0,1)</param>
		/// <returns>the value x such that P(X &lt;= x) = p</returns>
		public override double getInverseOfCumulativeProbabilityFunction(double p)
		{
			return -Math.Log(1 - p) * mean;
		}

		/// <summary>Returns the mean value of the negative-exponential distribution.</summary>
		/// <returns>the mean value of the negative-exponential distribution</returns>
		public double getMean()
		{

			return mean; // a mean machine ;-)

		}

		/// <summary>
		/// Returns the next negative exponential pseudo random number. The algorithm used is taken from DESMO-C
		/// from Thomas Schniewind [Schni98] Volume 2, page 221, file realdist.cc. It has been adapted to Java and
		/// extended to handle antithetic random numbers if antithetic mode is switched on.
		/// </summary>
		/// <returns>The next negative exponential pseudo random number</returns>
		public override double sample()
		{

			incrementObservations(); // increase the number of randoms given to
									 // the
									 // client
			double newSample; // aux variable

			if (isAntithetic())
				newSample = Math.Log(randomGenerator.nextDouble())
						* mean;
			else
				newSample = -Math.Log(1 - randomGenerator.nextDouble())
						* mean;

			if (this.currentlySendTraceNotes())
				this.traceLastSample(newSample.ToString());

			return newSample;

		}
	}
}