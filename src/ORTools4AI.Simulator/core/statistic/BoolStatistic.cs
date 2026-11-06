// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>BoolStatistic</code> class is providing a statistic analysis about 
     * a boolean value. Statistics include <code>true</code>-ratio and total number of observations.
     */

	public class BoolStatistic : Reportable
	{

		// ****** attributes ******

		/**
		 * Number of <code>true</code>s observed
		 */
		private long _freq_true;

		/// <summary>Constructor for a BoolStatistic object. No connection to a <c>ValueSupplier</c>.</summary>
		/// <param name="ownerModel">The model this BoolStatistic is associated to</param>
		/// <param name="name">The name of this BoolStatistic object</param>
		/// <param name="showInReport">Flag for showing the report about BoolStatistic Tally.</param>
		/// <param name="showInTrace">Flag for showing the trace output of BoolStatistic Tally.</param>
		public BoolStatistic(Model ownerModel, string name, bool showInReport,
				bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
		{
			// call the constructor of ValueStatistics
			//super(ownerModel, name, showInReport, showInTrace);
			this._freq_true = 0;
		}

		// ****** methods ******

		/// <summary>Returns a Reporter to produce a report about this BoolStatistic.</summary>
		/// <returns>The Reporter for this BoolStatistic.</returns>
		protected override Reporter createDefaultReporter()
		{
			return new BoolStatisticReporter(this);
		}

		/// <summary>Returns the number of <c>true</c>s observed so far.</summary>
		/// <returns>The number of <c>true</c>s observed so far.</returns>
		public long getTrueObs()
		{
			return this._freq_true;
		}


		/// <summary>Returns the ratio of <c>true</c>s per total observations so far.</summary>
		/// <returns>The ratio of <c>true</c>s per total observations so far.</returns>
		public double getTrueRatio()
		{
			if (getObservations() == 0)
			{
				sendWarning(
						"Attempt to get ratio of trues per total observations, but there is not "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"BoolStatistic: " + this.getName() + " Method: double getTrueRatio()",
						"You can not calculate a ratio of trues as long as no data is collected.",
						"Make sure to ask for ratio of trues only after some data has been "
								+ "collected already.");

				return StatisticObject.UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the mean value
			double ratio = 1.0 * this.getTrueObs() / getObservations();
			// return the rounded mean value
			return ValueStatistics.round(ratio);
		}

		/// <summary>Resets this <c>BoolStatistic</c> object by deleting a observations this far.</summary>
		public void reset()
		{
			base.reset(); // reset the StatisticObject, too.
			this._freq_true = 0;
		}

		/// <summary>
		/// Updates this <c>BoolStatistic</c> object with a boolean double value given as parameter.
		/// </summary>
		/// <param name="val">The value with which this <c>BoolStatistic</c> will be updated.</param>
		public void update(bool val)
		{

			if (val == true) this._freq_true++;
			this.incrementObservations();
		}
	} // end class BoolStatistic
}