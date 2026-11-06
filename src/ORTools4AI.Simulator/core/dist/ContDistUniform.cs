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
     * Uniformly distributed stream of pseudo random numbers of type double. Values
     * produced by this distribution are uniformly distributed in the range
     * specified as parameters of the constructor.
     */
	public class ContDistUniform : ContDist
	{

		/**
		 * Stores the lower border of the distribution range
		 */
		private double _min;

		/**
		 * Stores the upper border of the distribution range
		 */
		private double _max;

		/// <summary>
		/// Creates a stream of pseudo random numbers following a uniform distribution between the lower and the
		/// upper value parameter. The specific lower and upper borders of the range of this distribution have to
		/// be given here at creation time. Note that the lower border in fact has to be lower than the upper
		/// border. If not, the parameters will be swapped to assign the higher value to the upper border of the
		/// distribution. Two equal values for upper and lower will be accepted, but result in a constant
		/// distribution that will invariably return that given value.
		/// </summary>
		/// <param name="owner">The distribution's owner</param>
		/// <param name="name">The distribution's name</param>
		/// <param name="lowerBorder">The minimum value for this distribution</param>
		/// <param name="upperBorder">The maximum value for this distribution</param>
		/// <param name="showInReport">Flag for producing reports</param>
		/// <param name="showInTrace">Flag for producing trace output</param>
		public ContDistUniform(Model owner, string name, double lowerBorder,
				double upperBorder, bool showInReport, bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{
			//super(owner, name, showInReport, showInTrace);

			if (lowerBorder <= upperBorder)
			{
				_min = lowerBorder;
				_max = upperBorder;
			}
			else
			{
				_min = upperBorder;
				_max = lowerBorder;
			}
		}

		/// <summary>Creates the default reporter for the RealDistUniform distribution.</summary>
		/// <returns>The reporter for the RealDistUniform distribution</returns>
		/// <remarks>See also ContDistUniformReporter</remarks>
		protected override Reporter createDefaultReporter()
		{

			return new ContDistUniformReporter(this);

		}

		/// <summary>
		/// Returns the inverse of the cumulative probability function (quantile function). For the uniform
		/// distribution: F^-1(p) = lower + p * (upper - lower).
		/// </summary>
		/// <param name="p">probability in [0,1]</param>
		/// <returns>the value x such that P(X &lt;= x) = p</returns>
		public override double getInverseOfCumulativeProbabilityFunction(double p)
		{
			return _min + p * (_max - _min);
		}

		/// <summary>Returns the lower border of the range of this uniform distribution.</summary>
		/// <returns>The lower border of the range of this uniform distribution</returns>
		public double getLower()
		{

			return _min;

		}

		/// <summary>Returns the upper border of the range of this distribution.</summary>
		/// <returns>The upper border of the range of this uniform distribution</returns>
		public double getUpper()
		{

			return _max;

		}

		/// <summary>
		/// Returns the next floating point sample from this uniform distribution. The value returned is basically
		/// the uniformly distributed pseudo random number produced by the underlying random generator stretched to
		/// match the range specified by the client via construtor parameters.
		/// </summary>
		/// <returns>The next floating point sample from this uniform distribution</returns>
		public override double sample()
		{

			incrementObservations(); // increase count of samples

			double newSample; // aux variable

			do
			{
				if (isAntithetic()) // check if antithetic mode is on
					newSample = (_min + ((_max - _min) * (1 - randomGenerator
							.nextDouble())));
				else
					newSample = (_min + ((_max - _min) * randomGenerator.nextDouble()));
			} while (nonNegative && newSample < 0); // get a new sample if it should
													// be
													// non negative but actually is negative.

			if (this.currentlySendTraceNotes())
				this.traceLastSample(newSample.ToString());

			return newSample;
		}

		/// <summary>
		/// Overrides the same method of <c>Distribution</c>. It makes no sense to set a <c>RealDistUniform</c>
		/// object with a negative lower border to nonNegative. In this case a warning will be sent. Sets the
		/// nonNegative switch to the given value. If nonNegative is set to <c>true</c> the distribution returns
		/// positive samples only, otherwise it also produces negative samples, if possible.
		/// </summary>
		/// <param name="newValue">
		/// If <c>true</c> the distribution is set to return positive samples only, otherwise it also produces
		/// negative samples, if possible.
		/// </param>
		public void setNonNegative(bool newValue)
		{
			if (newValue == true && _min < 0)
			{
				sendWarning(
						"Attempt to set a RealDistUniform distribution with a negative "
								+ "lowerBorder to nonNegative. This will be done, but doesn't make sense!",
						"RealDistUniform: " + this.getName()
								+ " Method: public void "
								+ "setNonNegative(boolean newValue)",
						"The given distribution has a negative border but all negative values "
								+ "will be ignored.",
						"Make sure not to set a RealDistUniform distribution with a negative "
								+ "lowerBorder to nonNegative.");
			}

			if (newValue == true && _max < 0)
			{
				sendWarning(
						"Attempt to set a RealDistUniform distribution with a negative "
								+ "upperBorder to nonNegative. The command will be ignored!",
						"RealDistUniform: " + this.getName()
								+ " Method: public void "
								+ "setNonNegative(boolean newValue)",
						"The given distribution has a negative upper border. When all negative "
								+ "values will be ignored, one won't get any values.",
						"Make sure not to set a RealDistUniform distribution with a negative "
								+ "upperBorder to nonNegative.");

				return; // just do nothing but return
			}

			this.nonNegative = newValue;
		}
	}
}