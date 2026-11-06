// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * The <code>ValueStatistics</code> class is the super class for all the
     * classes providing a statistic analysis about one value (e.g. minimum and
     * maximum values). Derived classes are: Tally, Accumulate and Histogram
     * (because it is derived from Tally).
     */

    public abstract class ValueStatistics : StatisticObjectSupportingTimeSpans, Observer
    {

        // ****** attributes ******

        /**
         * The ValueSupplier providing the values which will be processed.
         */
        private ValueSupplier _valSuppl;

        /**
         * The minimum of all values so far
         */
        private double _min;

        /**
         * The maximum of all values so far
         */
        private double _max;

        /**
         * The last value we got from the ValueSupplier
         */
        private double _lastValue;

        // ****** methods ******

        /// <summary>
        /// Constructor for a ValueStatistics object that will be connected to a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this ValueStatistics is associated to</param>
        /// <param name="name">The name of this ValueStatistics object</param>
        /// <param name="valSup">
        /// The ValueSupplier providing the value for this ValueStatistics. The given ValueSupplier will be
        /// observed by this ValueStatistics object.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this ValueStatistics.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this ValueStatistics.</param>
        public ValueStatistics(Model ownerModel, string name, ValueSupplier valSup,
                bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {
            //super(ownerModel, name, showInReport, showInTrace);

            // valSup is no valid ValueSupplier
            if (valSup == null)
            {
                sendWarning(
                        "Attempt to produce a ValueStatistics about a non "
                                + "existing ValueSupplier. The command will be ignored!",
                        "ValueStatistics: "
                                + this.getName()
                                + " Constructor: ValueStatistics"
                                + " (Model ownerModel, String name, ValueSupplier valSup, "
                                + "boolean showInReport, boolean showInTrace)",
                        "The given ValueSupplier: valSup is only a null pointer.",
                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                + "ValueStatistics object.");

                return; // just return
            }

            this._valSuppl = valSup;
            this._max = Double.NaN;
            this._min = Double.NaN;
            this._lastValue = Double.NaN;

            // this ValueStatistics will observe the valSuppl
            _valSuppl.addObserver(this);
        }

        /// <summary>
        /// Constructor for a ValueStatistics object that has NO connection to a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this ValueStatistics is associated to</param>
        /// <param name="name">The name of this ValueStatistics object</param>
        /// <param name="showInReport">Flag for showing the report about this ValueStatistics.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this ValueStatistics.</param>
        public ValueStatistics(Model ownerModel, string name, bool showInReport,
                bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {

            // no ValueSupplier will be observed
            this._valSuppl = null;
            this._max = Double.NaN;
            this._min = Double.NaN;
            this._lastValue = Double.NaN;

        }

        /// <summary>Returns the last observed value of this ValueStatistics object.</summary>
        /// <returns>The last value observed so far.</returns>
        public double getLastValue()
        {
            return this._lastValue;
        }

        /// <summary>Returns the maximum value observed so far.</summary>
        /// <returns>The maximum value observed so far.</returns>
        public double getMaximum()
        {

            if (getObservations() == 0)
            {
                sendWarning(
                        "Attempt to get a maximum value, but there is not "
                                + "sufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "ValueStatistics: " + this.getName() + " Method: double getMaximum()",
                        "You can not obtain a maximum value as long as no data is collected.",
                        "Make sure to ask for a maximum value only after some data has been "
                                + "collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }

            // return the rounded maximum
            return round(this._max);
        }

        /// <summary>
        /// Returns the mean value of all the values observed so far. Has to be implemented in a derived
        /// class.
        /// </summary>
        /// <returns>The mean value of all the values observed so far.</returns>
        public abstract double getMean();

        /// <summary>Returns the minimum value observed so far.</summary>
        /// <returns>The minimum value observed so far.</returns>
        public double getMinimum()
        {

            if (getObservations() == 0)
            {
                sendWarning(
                        "Attempt to get a minimum value, but there is not "
                                + "sufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "ValueStatistics: " + this.getName() + " Method: double getMinimum()",
                        "You can not obtain a minimum value as long as no data is collected.",
                        "Make sure to ask for a minimum value only after some data has been "
                                + "collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }

            // return the rounded minimum
            return round(this._min);
        }

        /// <summary>
        /// Returns the standard deviation of all the values observed so far. Has to be implemented in a
        /// derived class.
        /// </summary>
        /// <returns>The standard deviation of all the values observed so far.</returns>
        public abstract double getStdDev();

        /// <summary>Returns the ValueSupplier object providing all the values.</summary>
        /// <returns>The ValueSupplier object providing the values for this ValueStatistics.</returns>
        protected ValueSupplier getValueSupplier()
        {
            return _valSuppl;
        }

        /**
         * Resets this ValueStatistics object by resetting all variables to 0.0 .
         */
        public void reset()
        {
            base.reset(); // reset the StatisticObject, too.

            this._min = this._max = this._lastValue = Double.NaN;
        }

        //@Override
        /// <inheritdoc/>
        public override void update(TimeSpan t)
        {
            this.setShowTimeSpansInReport(true);
            this.update(t.getTimeAsDouble());
        }

        /// <summary>
        /// Updates this <c>ValueStatistics</c> object by fetching the actual value of the
        /// <c>ValueSupplier</c> and processing it. The <c>ValueSupplier</c> is passed in the constructor of
        /// this <c>ValueStatistics</c> object. This <c>update()</c> method complies with the one described
        /// in DESMO, see [Page91].
        /// </summary>
        public void update()
        {
            if (this._valSuppl == null)
            {
                sendWarning(
                        "Attempt to update a ValueStatistics that is not "
                                + "connected to a ValueSupplier. No value is provided with which "
                                + "the statistic could be updated. The command will be ignored!",
                        "ValueStatistics: " + this.getName() + " Method: update()",
                        "The given ValueSupplier: valSuppl is only a null pointer.",
                        "Make sure to update a ValueStatistics only when it is connected "
                                + "to a valid ValueSupplier. Or use the update(double val) method.");

                return; // that's it
            }

            // get the actual value from the ValueSupplier
            _lastValue = _valSuppl.value();

            incrementObservations(); // use the method from the Reportable class

            if (getObservations() <= 1) // the first observation?
            {
                _min = _max = _lastValue; // update min and max
            }

            if (_lastValue < _min)
            {
                _min = _lastValue; // update min
            }

            if (_lastValue > _max)
            {
                _max = _lastValue; // update max
            }

            traceUpdate(); // leave a message in the trace
        }

        /// <summary>
        /// Updates this <c>ValueStatistics</c> object with the double value given as parameter. In some
        /// cases it might be more convenient to pass the value this <c>ValueStatistics</c> will be updated
        /// with directly within the <c>update(double val)</c> method instead of going via the
        /// <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="val">The value with which this <c>ValueStatistics</c> will be updated.</param>
        public void update(double val)
        {
            _lastValue = val; // update lastValue

            incrementObservations(); // use the method from the Reportable class

            if (getObservations() <= 1) // the first onbservation?
            {
                _min = _max = _lastValue; // update min and max
            }

            if (_lastValue < _min)
            {
                _min = _lastValue; // update min
            }

            if (_lastValue > _max)
            {
                _max = _lastValue; // update max
            }

            traceUpdate(); // leave a message in the trace
        }

        /// <summary>
        /// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
        /// interface. This method will be called automatically from an <c>Observable</c> object within its
        /// <c>notifyObservers()</c> method. If no Object (a<c>null</c> value) is passed as arg, the actual
        /// value of the ValueSupplier will be fetched with the <c>value()</c> method of the ValueSupplier.
        /// Otherwise it is expected that the actual value is passed in the Object arg.
        /// </summary>
        /// <param name="o">
        /// The Observable calling this method within its own <c>notifyObservers()</c> method.
        /// </param>
        /// <param name="arg">
        /// The Object with which this <c>ValueStatistics</c> is updated. Normally a double number or
        /// TimeSpan which is added to the statistics or <c>null</c>.
        /// </param>
        public void update(Observable o, object arg)
        {
            // update was called with no arg Object OR from the SimClock
            if (arg == null || o is SimClock)
            {
                if (_valSuppl == null)
                {
                    sendWarning(
                            "Attempt to update a ValueStatistics that is not "
                                    + "connected to a ValueSupplier. No value is provided with which "
                                    + "the statistic could be updated. The command will be ignored!",
                            "ValueStatistics: " + this.getName()
                                    + " Method: update "
                                    + "(Observable o, Object arg)",
                            "The given ValueSupplier: valSuppl is only a null pointer.",
                            "Make sure to update a ValueStatistics only when it is connected "
                                    + "to a valid ValueSupplier. Or use the update(double val) method.");

                    return; // that's it
                }

                // get the actual value from the ValueSupplier
                _lastValue = _valSuppl.value();

                incrementObservations(); // use the method from the Reportable
                                         // class
            }
            else
            {
                if (arg is double)
                {
                    // get the value out of the Object arg
                    _lastValue = convertToDouble(arg);
                    incrementObservations(); // use the method from the
                                             // Reportable class
                }
                else if (arg is TimeSpan)
                {
                    // get the value out of the Object arg
                    _lastValue = ((TimeSpan)arg).getTimeAsDouble();
                    incrementObservations(); // use the method from the
                                             // Reportable class
                }
                else
                {
                    sendWarning(
                            "Attempt to update a ValueStatistics with an argument "
                                    + "arg, that can not be recognized. The attempted action is ignored!",
                            "ValueStatistics: " + this.getName()
                                    + " Method: update (Observable "
                                    + "o, Object arg)",
                            "The passed Object in the argument arg could not be recognized.",
                            "Make sure to pass null or a Number object as the arg argument.");

                    return; // do nothing, just return
                }
            }

            if (getObservations() <= 1) // the first observation?
            {
                _min = _max = _lastValue; // update min and max
            }

            if (_lastValue < _min)
            {
                _min = _lastValue; // update min
            }

            if (_lastValue > _max)
            {
                _max = _lastValue; // update max
            }

            traceUpdate(); // leave a message in the trace
        }

    } // end class ValueStatistics
}
