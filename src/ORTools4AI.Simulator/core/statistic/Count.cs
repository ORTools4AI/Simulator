// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.statistic
{
	/**
     * The <code>Count</code> class is simply counting something during an
     * experiment. Be careful the counter can be updated with positive and negative
     * integer numbers!
     * 
     * To get this <code>Count</code> object updated automatically every time a
     * <code>ValueSupplier</code> has changed, call the <code>addObserver
     * (Observer)</code> -method from the <code>ValueSupplier</code> of interest,
     * where Observer is this <code>Count</code> object. <br>
     * This must be done by the user in his model! <br>
     * Consider usage of class <code>Aggregate</code> to count non-integer (i.e.
     * double) values.
     */

	public class Count : StatisticObject {

		// ****** attributes ******

		/**
		 * The minimum of all values so far
		 */
		private long _min;

		/**
		 * The maximum of all values so far
		 */
		private long _max;

		/**
		 * The current counter value
		 */
		private long _value;

		/**
		 * Flag indicating if this count will retain its value during a reset.
		 * Default is false.
		 */
		private bool isResetResistant;

		/**
		 * Flag, used to know if an inter-arrival-time Tally should be created.
		 */
		private bool _interArrivalTimeActivated = false;

		/**
		 * A Tally for the timespans between calls of the update-method.
		 */
		private Tally _interArrivalTally = null;

		/**
		 * The last point of time, the update-method was called.
		 */
		private TimeInstant _lastInterArrivalTime = null;

		// ****** methods ******

		/// <summary>Constructor for a Count object.</summary>
		/// <param name="ownerModel">The model this Count is associated to</param>
		/// <param name="name">The name of this Count object</param>
		/// <param name="showInReport">
		/// Flag for showing the report Set it to <c>true</c> if this Count should show up in report. Set it to
		/// <c>false</c> if this Count should not be shown in report.
		/// </param>
		/// <param name="showInTrace">
		/// Flag for showing this Count in trace files. Set it to <c>true</c> if this Count should show up in
		/// trace. Set it to <c>false</c> if this Count should not be shown in trace.
		/// </param>
		public Count(Model ownerModel, string name, bool showInReport,
				bool showInTrace) : this(ownerModel, name, showInReport, showInTrace, false) {

		}

		/// <summary>Constructor for a Count object.</summary>
		/// <param name="ownerModel">The model this Count is associated to</param>
		/// <param name="name">The name of this Count object</param>
		/// <param name="showInReport">
		/// Flag for showing the report Set it to <c>true</c> if this Count should show up in report. Set it to
		/// <c>false</c> if this Count should not be shown in report.
		/// </param>
		/// <param name="showInTrace">
		/// Flag for showing this Count in trace files. Set it to <c>true</c> if this Count should show up in
		/// trace. Set it to <c>false</c> if this Count should not be shown in trace.
		/// </param>
		/// <param name="isResetResistant">
		/// Flag for retaining the counter value during resets. Set it to <c>true</c> if this Count should retain
		/// its current value and only reset min, max and observations to 0. Set it to <c>false</c> if this Count
		/// should also reset its value to 0.
		/// </param>
		public Count(Model ownerModel, string name, bool showInReport,
				bool showInTrace, bool isResetResistant) : base(ownerModel, name, showInReport, showInTrace) {


			this._min = this._max = 0; // no minimum or maximum so far

			this._value = 0; // nothing counted so far
			this.isResetResistant = isResetResistant; // set resistance flag
		}

		/// <summary>
		/// Converts a <c>double</c> value to a <c>long</c> value. If this can not be done without rounding a
		/// warning will be produced.
		/// </summary>
		/// <param name="dbleValue">The double value which will be converted to a long value.</param>
		/// <returns>The long value the given double is converted to.</returns>
		protected long convertDoubleToLong(double dbleValue) {
			long lng = (long)(dbleValue); // rounded long value

			double rndDbleValue = lng;
			// rounded double value

			if ((rndDbleValue - dbleValue) != 0.0) {
				// the given double value must be rounded to produce a long
				sendWarning(
						"Attempt to convert a double value, which contains "
								+ "fractions, to a long value. The double value, rounded to a long "
								+ "value, will be returned!", "Count: " + getName()
								+ " Method: long convertDoubleToLong(double "
								+ "dbleValue) converts: " + dbleValue + " to: "
								+ lng,
						"You will lose accuracy when rounding a double value with fractions"
								+ " to a long value!",
						"Make sure to convert only double values with no fractions to long "
								+ "values.");
			}

			return lng; // return the rounded long value
		}

		/// <summary>Returns a Reporter to produce a report about this Count.</summary>
		/// <returns>The Reporter for this Count.</returns>
		protected override Reporter createDefaultReporter() {
			return new CountReporter(this);
		}

		/// <inheritdoc/>
		//@Override
		public void setCorrespondingSchedulable(Schedulable correspondingSchedulable) {
			base.setCorrespondingSchedulable(correspondingSchedulable);

			if (_interArrivalTally != null)
				_interArrivalTally.setCorrespondingSchedulable(correspondingSchedulable);
		}

		/// <summary>Returns the maximum value observed so far.</summary>
		/// <returns>The maximum value observed so far.</returns>
		public long getMaximum() {
			return this._max;
		}

		/// <summary>Returns the minimum value observed so far.</summary>
		/// <returns>The minimum value observed so far.</returns>
		public long getMinimum() {
			return this._min;
		}

		/// <summary>
		/// Resets this Count object by resetting (nearly) all variables to zero. If the flag
		/// <c>isResetResistant</c> is set to <c>true</c> the counter value will NOT be changed.
		/// </summary>
		public void reset() {
			base.reset(); // reset the Reportable, too.

			if (_interArrivalTimeActivated && _interArrivalTally != null)
				_interArrivalTally.reset();

			// really reset the counter value?
			if (!this.isResetResistant) {
				this._value = 0;
				this._min = this._max = 0;
			}
		}

		/// <summary>Increments the counter of this <c>Count</c> object by one.</summary>
		public void update() {
			update(1);
		}

		/// <summary>
		/// Increments the counter of this <c>Count</c> object by the value given in the parameter n.
		/// </summary>
		/// <param name="n">The number that will be added to the counter of this <c>Count</c> object.</param>
		public void update(long n) {
			if (_interArrivalTimeActivated) {
				TimeInstant t = presentTime();

				if (_lastInterArrivalTime != null) {
					TimeSpan dt = TimeOperations.diff(_lastInterArrivalTime, t);
					_interArrivalTally.update(dt.getTimeAsDouble());
				}

				_lastInterArrivalTime = t;
			}

			incrementObservations(1); // use the method from the Reportable class

			this._value += n; // update current value

			if (this._value < _min) {
				_min = this._value; // update min
			}

			if (this._value > _max) {
				_max = this._value; // update max
			}

			traceUpdate(); // leave a message in the trace
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
		/// The Object with which this <c>Count</c> is updated. Normally a long number which is added to the
		/// counter or <c>null</c>.
		/// </param>
		public void update(Observable o, object arg) {
			if (o == null) // null was passed instead of an Observable
			{
				sendWarning(
						"Attempt to update a Count with no reference to an "
								+ "Observable. The attempted action will be ignored!",
						"Count: " + this.getName()
								+ " Method: update (Observable o, " + "Object arg)",
						"The passed Observable: o in this method is only a null pointer.",
						"The update()-method was not called via notifyObservers() from an "
								+ "Observable. Who was calling it? Why don't you let the Observable do"
								+ " the work?");

				return; // do nothing, just return
			}

			if (arg == null) // update was called with no arg Object
			{
				// get the actual value from the ValueSupplier (= Observable)
				double actualValue = ((ValueSupplier)o).value();

				long n = convertDoubleToLong(actualValue);

				update(n);
			} else {
				if (arg is double) {
					// get the value out of the Object arg
					double val = convertToDouble(arg);

					long n = convertDoubleToLong(val);

					update(n);

				} else {
					sendWarning(
							"Attempt to update a Count with an object, that can "
									+ "not be recognized. The attempted action will be ignored!",
							"Count: " + this.getName()
									+ " Method: update (Observable o, "
									+ "Object arg)",
							"The passed Object: arg in this method is not a Number.",
							"Make sure to pass as Object: arg in the update()-method only "
									+ "objects of the class: java.lang.Number or null!");

					return; // do nothing, just return
				}
			}

		}

		/// <summary>Returns the current counter value.</summary>
		/// <returns>the current counter value.</returns>
		public long getValue() {
			return this._value;
		}

		/// <summary>Is the time between calls of the update-method be observed by a Tally?</summary>
		/// <returns>true if the inter-arrival-time will be observed, false if not.</returns>
		public bool isInterArrivalTimeTallyActivated() {
			return _interArrivalTimeActivated;
		}

		/// <summary>
		/// Activates the inter-arrival-time Tally to observe the timespans between calls of the update-Method.
		/// </summary>
		/// <param name="reporterClass">A reporter to be used by the Tally.</param>
		/// <param name="showTimeSpansInReport">Should the Reporter for the Tally use TimeSpans in Report?</param>
		public void activateInterArrivalTimeTally(
				Type reporterClass,
				bool showTimeSpansInReport) {
			this._interArrivalTimeActivated = true;

			if (_interArrivalTally == null) {
				_interArrivalTally = new Tally(this.getModel(), this.getName()
						+ " Interarrivaltime", this.reportIsOn(), this.traceIsOn());
				_interArrivalTally.setShowTimeSpansInReport(showTimeSpansInReport);
				if (reporterClass != null)
					_interArrivalTally.setReporter(reporterClass);

				Schedulable schedulable = getCorrespondingSchedulable();

				if (schedulable != null)
					_interArrivalTally.setCorrespondingSchedulable(schedulable);
			}
		}

		/// <summary>
		/// Activates the inter-arrival-time Tally to observe the timespans between calls of the update-Method.
		/// </summary>
		/// <param name="showTimeSpansInReport">Should the Reporter for the Tally use TimeSpans in Report?</param>
		public void activateInterArrivalTimeTally(bool showTimeSpansInReport) {
			activateInterArrivalTimeTally(null, showTimeSpansInReport);
		}

		/**
		 * Activates the inter-arrival-time Tally to observe the timespans between
		 * calls of the update-Method.
		 */
		public void activateInterArrivalTimeTally() {
			activateInterArrivalTimeTally(false);
		}

		/**
		 * Deactivates the inter-arrival-time Tally to stop observing the timespans
		 * between calls of the update-Method.
		 */
		public void deactivateInterArrivalTimeTally() {
			this._interArrivalTimeActivated = false;
		}
	} // end class
}