// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>Tally</code> class is providing a statistic analysis about one
     * value. The mean value and the standard deviation is calculated on basis of
     * the total number of observations. <br />
     * 
     * @see TallyRunning
     */

	public class Tally : ValueStatistics
	{

		// ****** attributes ******

		/**
		 * The mean of all values so far
		 */
		private double _mean;

		/**
		 * The sum of the squares of the differences from the mean of all values so far
		 */
		private double _sumOfSquaredDevsFromMean;

		/// <summary>Constructor for a Tally object that is connected to a <c>ValueSupplier</c>.</summary>
		/// <param name="ownerModel">The model this Tally is associated to</param>
		/// <param name="name">The name of this Tally object</param>
		/// <param name="valSup">
		/// The ValueSupplier providing the value for this Tally. The given ValueSupplier will be observed by this
		/// Tally object.
		/// </param>
		/// <param name="showInReport">Flag for showing the report about this Tally.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Tally.</param>
		public Tally(Model ownerModel, string name, ValueSupplier valSup,
				bool showInReport, bool showInTrace):base(ownerModel, name, valSup, showInReport, showInTrace)
		{
			// call the constructor of ValueStatistics
			//super(ownerModel, name, valSup, showInReport, showInTrace);

			// valSup is no valid ValueSupplier
			if (valSup == null)
			{
				sendWarning("Attempt to produce a Tally about a non existing "
						+ "ValueSupplier. The command will be ignored!", "Tally: "
						+ this.getName() + " Constructor: Tally"
						+ " (Model ownerModel, String name, ValueSupplier valSup, "
						+ "boolean showInReport, boolean showInTrace)",
						"The given ValueSupplier: valSup is only a null pointer.",
						"Make sure to pass a valid ValueSupplier when constructing a new "
								+ "Tally object.");

				return; // just return
			}

			this._mean = this._sumOfSquaredDevsFromMean = 0.0;
		}

		// ****** methods ******

		/// <summary>Constructor for a Tally object that has no connection to a <c>ValueSupplier</c>.</summary>
		/// <param name="ownerModel">The model this Tally is associated to</param>
		/// <param name="name">The name of this Tally object</param>
		/// <param name="showInReport">Flag for showing the report about this Tally.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Tally.</param>
		public Tally(Model ownerModel, string name, bool showInReport,
				bool showInTrace):base(ownerModel, name, showInReport, showInTrace)
		{
			// call the constructor of ValueStatistics
			//super(ownerModel, name, showInReport, showInTrace);

			this._mean = this._sumOfSquaredDevsFromMean = 0.0;
		}

		/// <summary>Returns a Reporter to produce a report about this Tally.</summary>
		/// <returns>The Reporter for this Tally.</returns>
		protected override Reporter createDefaultReporter()
		{
			var result = new TallyReporter(this);
			return result;
		}

		/// <summary>Returns the mean value of all the values observed so far.</summary>
		/// <returns>The mean value of all the values observed so far.</returns>
		public override double getMean()
		{
			if (getObservations() == 0)
			{
				sendWarning(
						"Attempt to get a mean value, but there is not "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"Tally: " + this.getName() + " Method: double getMean()",
						"You can not calculate a mean value as long as no data is collected.",
						"Make sure to ask for the mean value only after some data has been "
								+ "collected already.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// return the rounded mean value
			return round(_mean);
		}


		/// <summary>Returns the standard deviation of all the values observed so far.</summary>
		/// <returns>The standard deviation of all the values observed so far.</returns>
		public override double getStdDev()
		{
			long n = getObservations();

			if (n < 2)
			{
				sendWarning(
						"Attempt to get a standard deviation, but there is not "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"Tally: " + this.getName() + " Method: double getStdDev()",
						"A standard deviation can not be calculated as long as no data is "
								+ "collected.",
						"Make sure to ask for the standard deviation only after some data "
								+ "has been collected already.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the standard deviation
			double stdDev = Math.Sqrt(_sumOfSquaredDevsFromMean / (n - 1));

			// return the rounded standard deviation
			return round(stdDev);
		}

		/**
		 * Resets this Tally object by resetting all variables to 0.0 .
		 */
		public void reset()
		{
			base.reset(); // reset the ValueStatistics, too.
			this._mean = this._sumOfSquaredDevsFromMean = 0.0;
		}

		/// <summary>
		/// Updates this <c>Tally</c> object by fetching the actual value of the <c>ValueSupplier</c> and
		/// processing it. The <c>ValueSupplier</c> is passed in the constructor of this <c>Tally</c> object. This
		/// <c>update()</c> method complies with the one described in DESMO, see [Page91].
		/// </summary>
		public void update()
		{
			base.update(); // call the update() method of ValueStatistics

			double lastVal = getLastValue();
			this.internalUpdate(lastVal);
		}

		/// <summary>
		/// Updates this <c>Tally</c> object with the double value given as parameter. In some cases it might be
		/// more convenient to pass the value this <c>Tally</c> will be updated with directly within the
		/// <c>update(double val)</c> method instead of going via the <c>ValueSupplier</c>.
		/// </summary>
		/// <param name="val">The value with which this <c>Tally</c> will be updated.</param>
		public void update(double val)
		{
			// call the update(double val) method of ValueStatistics
			base.update(val);
			this.internalUpdate(val);
		}

		/// <summary>
		/// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
		/// interface. This method will be called automatically from an <c>Observable</c> object within its
		/// <c>notifyObservers()</c> method. If no Object (a<c>null</c> value) is passed as arg, the actual value
		/// of the ValueSupplier will be fetched with the <c>value()</c> method of the ValueSupplier. Otherwise it
		/// is expected that the actual value is passed in the Object arg.
		/// </summary>
		/// <param name="o">
		/// The Observable calling this method within its own <c>notifyObservers()</c> method.
		/// </param>
		/// <param name="arg">
		/// The Object with which this <c>Tally</c> is updated. Normally a double number which is added to the
		/// statistics or <c>null</c>.
		/// </param>
		public void update(Observable o, object arg)
		{
			if (o == null) // null was passed instead of an Observable
			{
				sendWarning(
						"Attempt to update a Tally with no reference to an "
								+ "Observable. The actual value of '"
								+ getValueSupplier().getName()
								+ "' will be fetched and processed anyway.",
						"Tally: " + this.getName() + " Method: update (Observable "
								+ "o, Object arg)",
						"The passed Observable: o in this method is only a null pointer.",
						"The update()-method was not called via notifyObservers() from an "
								+ "Observable. Who was calling it? Why don't you let the Observable do"
								+ " the work?");
			}

			update(o, arg); // call the update() method of ValueStatistics
			this.internalUpdate(getLastValue());
		}

		/// <summary>
		/// Internal method to update the mean and sum of the squares of the differences from the mean of values so
		/// far with a new sample.
		/// </summary>
		/// <param name="value">The new sample.</param>
		private void internalUpdate(double value)
		{
			if (this.getObservations() == 1)
			{ // First entry
				_mean = value;
				_sumOfSquaredDevsFromMean = 0.0;
			}
			else
			{ // Further entries
				double _m_old = _mean;
				_mean += (value - _mean) / this.getObservations();
				_sumOfSquaredDevsFromMean += (value - _m_old) * (value - _mean);
			}
		}

	} // end class Tally
}