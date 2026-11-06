// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>Aggregate</code> class is simply counting (aggregating) a continuous (i.e. double)
     * value during an experiment. Be careful the aggregate can be updated with positive and negative
     * values!
     * 
     * To get this <code>Aggregate</code> object updated automatically every time a
     * <code>ValueSupplier</code> has changed, call the <code>addObserver
     * (Observer)</code>-method from the <code>ValueSupplier</code> of interest, 
     * where Observer is this <code>Aggregate</code> object. <br>
     * This must be done by the user in his model! <br>
     * Consider usage of class <code>Count</code> to aggregate integer values. 
     */

	public class Aggregate : StatisticObjectSupportingTimeSpans
	{

		// ****** attributes ******

		/**
		 * The minimum of all values so far
		 */
		private double _min;

		/**
		 * The maximum of all values so far
		 */
		private double _max;

		/** 
		 * The current value of the aggregate
		 */
		private double _value;

		/**
		 * Flag indicating if this aggregate will retain its value during a reset.
		 * Default is false.
		 */
		private bool _isResetResistant;

		// ****** methods ******

		/// <summary>Constructor for a Aggregate object.</summary>
		/// <param name="ownerModel">The model this Aggregate is associated to</param>
		/// <param name="name">The name of this Aggregate object</param>
		/// <param name="showInReport">
		/// Flag for showing the report Set it to <c>true</c> if this Aggregate should show up in report. Set it to
		/// <c>false</c> if this Aggregate should not be shown in report.
		/// </param>
		/// <param name="showInTrace">
		/// Flag for showing this Aggregate in trace files. Set it to <c>true</c> if this Aggregate should show up
		/// in trace. Set it to <c>false</c> if this Aggregate should not be shown in trace.
		/// </param>
		public Aggregate(Model ownerModel, string name, bool showInReport,
				bool showInTrace) : this(ownerModel, name, showInReport, showInTrace, false)
		{
			//this(ownerModel, name, showInReport, showInTrace, false);
		}

		/// <summary>Constructor for a Aggregate object.</summary>
		/// <param name="ownerModel">The model this Aggregate is associated to</param>
		/// <param name="name">The name of this Aggregate object</param>
		/// <param name="showInReport">
		/// Flag for showing the report Set it to <c>true</c> if this Aggregate should show up in report. Set it to
		/// <c>false</c> if this Aggregate should not be shown in report.
		/// </param>
		/// <param name="showInTrace">
		/// Flag for showing this Aggregate in trace files. Set it to <c>true</c> if this Aggregate should show up
		/// in trace. Set it to <c>false</c> if this Aggregate should not be shown in trace.
		/// </param>
		/// <param name="isResetResistant">
		/// Flag for retaining the value of the aggregate during resets. Set it to <c>true</c> if this Aggregate
		/// should retain its current value and only reset min, max and observations to 0. Set it to <c>false</c>
		/// if this Aggregate should also reset its value to 0.
		/// </param>
		public Aggregate(Model ownerModel, string name, bool showInReport,
				bool showInTrace, bool isResetResistant) : base(ownerModel, name, showInReport, showInTrace)
		{
			//super(ownerModel, name, showInReport, showInTrace);

			this._min = this._max = 0; // no minimum or maximum so far

			this._value = 0; // nothing aggregated so far
			this._isResetResistant = isResetResistant; // set resistance flag
		}

		/// <summary>Returns a Reporter to produce a report about this Aggregate.</summary>
		/// <returns>The Reporter for this Aggregate.</returns>
		protected override Reporter createDefaultReporter()
		{
			return new AggregateReporter(this);
		}

		/// <summary>Returns the maximum value observed so far.</summary>
		/// <returns>The maximum value observed so far.</returns>
		public double getMaximum()
		{
			return this._max;
		}

		/// <summary>Returns the minimum value observed so far.</summary>
		/// <returns>The minimum value observed so far.</returns>
		public double getMinimum()
		{
			return this._min;
		}

		/// <summary>
		/// Resets this Aggregate object by resetting (nearly) all variables to zero. If the flag
		/// <c>isResetResistant</c> is set to <c>true</c> the value of the aggregate will NOT be changed.
		/// </summary>
		public void reset()
		{
			base.reset(); // reset the Reportable, too.

			// really reset the value of the aggregate?
			if (!this._isResetResistant)
			{
				this._min = this._max = 0;
				this._value = 0;
			}
		}

		//@Override
		/// <inheritdoc/>
		public override void update(TimeSpan t)
		{
			this.setShowTimeSpansInReport(true);
			this.update(t.getTimeAsDouble());
		}

		/// <summary>
		/// Increments the value of this <c>Aggregate</c> object by the value given in the parameter n.
		/// </summary>
		/// <param name="n">The number that will be added to the value of this <c>Aggregate</c> object.</param>
		public void update(double n)
		{
			incrementObservations(1); // use the method from the Reportable class

			this._value += n; // update current value

			if (this._value < _min)
			{
				_min = this._value; // update min
			}

			if (this._value > _max)
			{
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
		/// The Object with which this <c>Aggregate</c> is updated. Normally a double number which is added to the
		/// value of the aggregate or <c>null</c>.
		/// </param>
		public void update(Observable o, object arg)
		{
			if (o == null) // null was passed instead of an Observable
			{
				sendWarning(
						"Attempt to update a Aggregate with no reference to an "
								+ "Observable. The attempted action will be ignored!",
						"Aggregate: " + this.getName()
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
				double n = ((ValueSupplier)o).value();
				update(n);

			}
			else
			{
				if (arg is double)
				{
					// get the value out of the Object arg
					double n = convertToDouble(arg);
					update(n);

				}
				else
				{
					sendWarning(
							"Attempt to update a Aggregate with an object, that can "
									+ "not be recognized. The attempted action will be ignored!",
							"Aggregate: " + this.getName()
									+ " Method: update (Observable o, "
									+ "Object arg)",
							"The passed Object: arg in this method is not a Number.",
							"Make sure to pass as Object: arg in the update()-method only "
									+ "objects of the class: java.lang.Number or null!");

					return; // do nothing, just return
				}
			}

		}

		/// <summary>Returns the current value of the aggregate.</summary>
		/// <returns>the current value of the aggregate.</returns>
		public double getValue()
		{
			return this._value;
		}
	} // end class
}