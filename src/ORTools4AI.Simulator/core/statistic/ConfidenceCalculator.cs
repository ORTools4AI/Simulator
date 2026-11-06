// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>ConfidenceCalculator</code> class has the same functionality as the
     * <code>Tally</code> class. Additionally, it calculates a confidence interval for
     * the mean of the samples passed to the <code>ConfidenceCalculator</code>.
     * Note that the confidence interval computation assumes that the samples
     * are (approximately) identically and independently distributed. Observe
     * that the confidence interval estimations of this class are meaningless should
     * this assumption not hold!  
     * 
     * @see Tally
     */
	public class ConfidenceCalculator : Tally
	{

		/**
		 * The confidence level
		 */
		private double _confidenceLevel;

		/**
		 * The tDistribution for the t-quantile
		 */
		//private TDistribution _tDistribution;

		/**
		 * Constructor for a ConfidenceCalculator object that is connected to a
		 * <code>ValueSupplier</code>. The confidence level is set to 0.95.
		 * 
		 * @param ownerModel
		 *            Model : The model this ConfidenceCalculator is associated to
		 * @param name
		 *            java.lang.String : The name of this ConfidenceCalculator
		 *            object
		 * @param valSup
		 *            ValueSupplier : The ValueSupplier providing the value for this
		 *            ConfidenceCalculator. The given ValueSupplier will be observed
		 *            by this ConfidenceCalculator object.
		 * @param showInReport
		 *            boolean : Flag for showing the report about this
		 *            ConfidenceCalculator.
		 * @param showInTrace
		 *            boolean : Flag for showing the trace output of this
		 *            ConfidenceCalculator.
		 */
		public ConfidenceCalculator(Model ownerModel, String name,
				ValueSupplier valSup, bool showInReport, bool showInTrace)
			:base(ownerModel, name, valSup, showInReport, showInTrace)
        {
			// call the constructor of ValueStatistics
			//super(ownerModel, name, valSup, showInReport, showInTrace);
			//_tDistribution = new TDistributionImpl(1);
			_confidenceLevel = 0.95;
		}

		/// <summary>
		/// Constructor for a ConfidenceCalculator object that has no connection to a <c>ValueSupplier</c>. The
		/// confidence level is set to 0.95.
		/// </summary>
		/// <param name="ownerModel">The model this ConfidenceCalculator is associated to</param>
		/// <param name="name">The name of this ConfidenceCalculator object</param>
		/// <param name="showInReport">Flag for showing the report about this ConfidenceCalculator.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this ConfidenceCalculator.</param>
		public ConfidenceCalculator(Model ownerModel, string name,
				bool showInReport, bool showInTrace):base(ownerModel, name, showInReport, showInTrace)
        {
			// call the constructor of ValueStatistics
			//super(ownerModel, name, showInReport, showInTrace);
			//_tDistribution = new TDistributionImpl(1);
			_confidenceLevel = 0.95;
		}

		/// <summary>Returns a Reporter to produce a report about this ConfidenceCalculator.</summary>
		/// <returns>The Reporter for this ConfidenceCalculator.</returns>
		protected override Reporter createDefaultReporter()
		{
			return new ConfidenceCalculatorReporter(this);
		}

		/// <summary>
		/// Sets the confidence level; default value is 0.95. Note: The confidence interval computation assumes
		/// that the input values are (approximately) identically and independently distributed.
		/// </summary>
		/// <param name="level">the confidence level</param>
		public void setConfidenceLevel(double level)
		{
			_confidenceLevel = level;
		}

		/// <summary>Returns the confidence level.</summary>
		/// <returns>the confidence level</returns>
		public double getConfidenceLevel()
		{
			return round(_confidenceLevel);
		}

		/// <summary>Calculates the upper bound of the confidence interval of the mean.</summary>
		/// <returns>The upper bound of the confidence interval of the mean</returns>
		public double getConfidenceIntervalOfMeanUpperBound()
		{
			if (this.getObservations() < 2)
			{
				sendWarning(
						"Attempt to determine a confidence interval, but there is no "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"ConfidenceCalculator: " + this.getName()
								+ " Method: double getConfidenceIntervalOfMeanUpperBound()",
						"You cannot obtain a confidence interval based on less than two observations.",
						"Make sure to update the ConfidenceCalculator at least twice.");

				return UNDEFINED; // return UNDEFINED = -1.0

			}
			else
			{
				return round(getMean() + calcConfidenceIntervalHalfWidth());
			}

		}

		/// <summary>Calculates the lower bound of the confidence interval of the mean.</summary>
		/// <returns>The lower bound of the confidence interval of the mean</returns>
		public double getConfidenceIntervalOfMeanLowerBound()
		{
			if (this.getObservations() < 2)
			{
				sendWarning(
						"Attempt to determine a confidence interval, but there is no "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"ConfidenceCalculator: " + this.getName()
								+ " Method: double getConfidenceIntervalOfMeanLowerBound()",
						"You cannot obtain a confidence interval based on less than two observations.",
						"Make sure to update the ConfidenceCalculator at least twice.");

				return UNDEFINED; // return UNDEFINED = -1.0

			}
			else
			{
				return round(getMean() - calcConfidenceIntervalHalfWidth());
			}
		}

		/// <summary>Internal helper method to determine the confidence interval half width.</summary>
		/// <returns>The half width of the confidence interval</returns>
		private double calcConfidenceIntervalHalfWidth()
		{

			if (this.getObservations() < 2)
				return UNDEFINED;

			double n = getObservations();
			double s = getStdDev();

			//_tDistribution.setDegreesOfFreedom(n - 1);

			double z = Double.NaN;
			//try
			//{
			//	z = _tDistribution.inverseCumulativeProbability(
			//			0.5 + _confidenceLevel / 2);
			//}
			//catch (Exception e)
			//{
			//	return UNDEFINED;
			//}

			return z * s / Math.Sqrt(n);
		}
	}
}