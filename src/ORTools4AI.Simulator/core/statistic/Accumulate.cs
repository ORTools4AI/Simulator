// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * The <code>Accumulate</code> class is providing a statistic analysis about
     * one value. The mean value and the standard deviation is weighted over time.<br />
     */

    public class Accumulate : ValueStatistics
    {

        // ****** attributes ******

        /**
         * Instant at which the current pause has started (<code>null</code> if not paused at the moment) 
         */
        private TimeInstant _pausedSince;

        /**
         * The length of the period during which this Accumulate was paused since the last reset  
         */
        private TimeSpan _pausedPeriodSinceLastReset;

        /**
         * A value to assume initially and after each reset.
         */
        private double _initialValue;

        /**
         * The time-weighted mean of all values so far
         */
        private double _mean;

        /**
         * The time-weighted sum of the squares of the differences from the mean of all values so far
         */
        private double _sumOfSquaredDevsFromMean;

        /**
         * This flag indicates if the accumulate's last value is
         * retained on reset (true) or nullified (false)
         */
        private bool _retainLastValueOnReset;

        /**
         * The point of time this Accumulate was updated first
         */
        private TimeInstant _firstValueReadAt;

        /**
         * The point of time this Accumulate was updated last
         */
        private TimeInstant _lastUpdate;

        /// <summary>
        /// Constructor for a Accumulate object that will be connected to a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Accumulate is associated to</param>
        /// <param name="name">The name of this Accumulate object</param>
        /// <param name="valSup">
        /// The ValueSupplier providing the value for this Accumulate. The given ValueSupplier will be
        /// observed by this <c>Accumulate</c> object, if it is not observed automatically at every tick of
        /// the SimClock.
        /// </param>
        /// <param name="automatic">
        /// Flag for observing the ValueSupplier at every tick of the SimClock. Set it to <c>true</c> to have
        /// it observed at every SimClock tick.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this Accumulate.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Accumulate.</param>
        public Accumulate(Model ownerModel, string name, ValueSupplier valSup,
                bool automatic, bool showInReport, bool showInTrace) : base(ownerModel, name, valSup, showInReport, showInTrace)
        {
            // call the constructor of ValueStatistics
            // super(ownerModel, name, valSup, showInReport, showInTrace);

            this._retainLastValueOnReset = true;
            this._pausedSince = null;
            this._pausedPeriodSinceLastReset = new TimeSpan(0);
            this._mean = Double.NaN;
            this._sumOfSquaredDevsFromMean = 0;

            // valSup is no valid ValueSupplier
            if (valSup == null)
            {
                sendWarning(
                        "Attempt to produce a Accumulate about a non existing "
                                + "ValueSupplier. The command will be ignored!",
                        "Accumulate: "
                                + this.getName()
                                + " Constructor: Accumulate"
                                + " (Model ownerModel, String name, ValueSupplier valSup, "
                                + "boolean showInReport, boolean showInTrace)",
                        "The given ValueSupplier: valSup is only a null pointer.",
                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                + "Accumulate object.");

                return; // just return
            }

            if (automatic) // update at every tick of the SimClock?
            {
                // cancel the observation of the ValueSupplier
                getValueSupplier().deleteObserver(this);

                Observable simClock = this.getModel().getExperiment().getSimClock();

                simClock.addObserver(this); // observe the SimClock
            }
        }

        // ****** methods ******

        /// <summary>
        /// Constructor for a Accumulate object that will not be connected to a <c>ValueSupplier</c>
        /// automatically.
        /// </summary>
        /// <param name="ownerModel">The model this Accumulate is associated to</param>
        /// <param name="name">The name of this Accumulate object.</param>
        /// <param name="showInReport">Flag for showing the report about this Accumulate.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Accumulate.</param>
        public Accumulate(Model ownerModel, string name, bool showInReport,
                bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {
            // call the constructor of ValueStatistics without a connection to a
            // ValueSupplier
            // super(ownerModel, name, showInReport, showInTrace);

            this._retainLastValueOnReset = true;
            this._pausedSince = null;
            this._pausedPeriodSinceLastReset = new TimeSpan(0);
            this._initialValue = double.NaN;
            this._mean = double.NaN;
            this._sumOfSquaredDevsFromMean = 0;

        }

        /// <summary>
        /// Constructor for an Accumulate that starts with an explicit initial value. The initial value is
        /// also re-applied on every reset.
        /// </summary>
        /// <param name="ownerModel">The model this Accumulate is associated to</param>
        /// <param name="name">The name of this Accumulate object</param>
        /// <param name="initialValue">Value to assume initially and after resets</param>
        /// <param name="showInReport">Flag for showing the report about this Accumulate</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Accumulate</param>
        public Accumulate(Model ownerModel, string name, double initialValue,
                bool showInReport, bool showInTrace)
            : this(ownerModel, name, showInReport, showInTrace)
        {
            this._initialValue = initialValue;
            if (!double.IsNaN(_initialValue))
            {
                this.update(_initialValue);
            }
        }

        /// <summary>Returns a Reporter to produce a report about this Accumulate.</summary>
        /// <returns>The Reporter for this Accumulate.</returns>
        protected override Reporter createDefaultReporter()
        {
            return new AccumulateReporter(this);
        }

        /// <summary>Returns the period measured (excluding pauses).</summary>
        /// <returns>The period measured (excluding pauses).</returns>
        public TimeSpan getPeriodMeasured()
        {
            TimeInstant now = presentTime(); // what's the time?

            // has no time passed?
            if (_firstValueReadAt == null || getObservations() == 0)
            {
                return new TimeSpan(0);
            }

            // determine overall period measured, excluding all past and current pauses
            TimeInstant start = _firstValueReadAt;
            TimeInstant end = (this._pausedSince == null ? this.presentTime() : this._pausedSince);

            TimeSpan period = TimeOperations.diff(start, end);
            period = TimeOperations.diff(period, this._pausedPeriodSinceLastReset);
            return period;
        }

        /// <summary>Returns the mean value of all the values observed so far, weighted over time.</summary>
        /// <returns>The mean value of all the values observed so far, weighted over time.</returns>
        public override double getMean()
        {
            TimeInstant now = presentTime(); // what's the time?

            // has no time passed?
            if (_firstValueReadAt == null || TimeSpan.isEqual(TimeOperations.diff(now, _firstValueReadAt), TimeSpan.ZERO)
                    || getObservations() == 0) // OR no observations are made
            {
                sendWarning(
                        "Attempt to get a mean value, but there is not "
                                + "sufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "Accumulate: " + this.getName()
                                + " Method: double getMean()",
                        "You can not calculate a mean value as long as no data is collected.",
                        "Make sure to ask for the mean value only after some data has been "
                                + "collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }

            // determine overall period measured, excluding all past and current pauses
            long periodMeasured = this.getPeriodMeasured().getTimeInEpsilon();

            // fetch current mean (treat NaN as 0 when only one update has been
            // recorded — the first update sets _firstValueReadAt but not _mean)
            double storedMean = double.IsNaN(_mean) ? 0.0 : _mean;
            double current_mean = storedMean;

            // update mean to reflect the period since the last update
            // (only necessary if not paused at the moment)
            if (this._pausedSince == null)
            {
                long periodCurrentValue =
                    TimeOperations.diff(this.presentTime(), _lastUpdate).getTimeInEpsilon();
                current_mean += (getLastValue() - storedMean) / periodMeasured * periodCurrentValue;
            }

            // return the rounded mean value
            return round(current_mean);
        }

        /// <summary>
        /// Returns the standard deviation of all the values observed so far, weighted over time.
        /// </summary>
        /// <returns>The standard deviation of all the values observed so far, weighted over time.</returns>
        public override double getStdDev()
        {
            TimeInstant now = presentTime(); // what's the time?

            // is totalTimeDiff less than the minimum distinguishable span of
            // time?
            if (_firstValueReadAt == null || TimeSpan.isEqual(TimeOperations.diff(now, _firstValueReadAt), TimeSpan.ZERO)
                    || getObservations() < 2) // OR not enough observations are
            {
                sendWarning(
                        "Attempt to get a standard deviation value, but there is "
                                + "insufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "Accumulate: " + this.getName()
                                + " Method: double getStdDev()",
                        "You can not calculate a standard deviation as long as no data is "
                                + "collected.",
                        "Make sure to ask for the standard deviation only after some data "
                                + "has been collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }


            // determine overall period measured, excluding all past and current pauses
            long periodMeasured = this.getPeriodMeasured().getTimeInEpsilon();
            double currentSumOfSquaredDevsFromMean = _sumOfSquaredDevsFromMean;

            // update mean and sum of squares of... to reflect the period since the last update
            // (only necessary if not paused at the moment)
            if (this._pausedSince == null)
            {
                long periodCurrentValue =
                    TimeOperations.diff(this.presentTime(), _lastUpdate).getTimeInEpsilon();
                double old_mean = _mean;
                double current_mean = _mean + (getLastValue() - _mean) / periodMeasured * periodCurrentValue;
                currentSumOfSquaredDevsFromMean +=
                    (getLastValue() - old_mean) * (getLastValue() - current_mean) * periodCurrentValue;
            }

            // calculate the standard deviation
            double stdDev = Math.Sqrt(currentSumOfSquaredDevsFromMean / periodMeasured);

            // return the rounded standard deviation
            return round(stdDev);

        }

        /**
         * Resets this Accumulate object by resetting all variables to 0.0.
         * If the flag retainLastValueOnReset is set to true, the last value
         * is not nullified but remains unchanged. 
         */
        public void reset()
        {
            double lastValue = this.getLastValue();
            bool observationsPriorToReset = (getObservations() > 0);

            base.reset(); // reset the ValueStatistics, too.

            this._mean = Double.NaN;
            this._sumOfSquaredDevsFromMean = 0;

            _firstValueReadAt = null;
            _lastUpdate = presentTime();
            _pausedSince = null;
            _pausedPeriodSinceLastReset = new TimeSpan(0);

            if (doesRetainLastValueOnReset() && observationsPriorToReset)
            {
                this.update(lastValue);
            }
        }

        /// <summary>
        /// This method determines whether if the accumulate's last value is retained on reset (true) or
        /// nullified (false). The default value is true.
        /// </summary>
        /// <returns>value of the retainLastValueOnReset flag</returns>
        public bool doesRetainLastValueOnReset()
        {
            return _retainLastValueOnReset;
        }

        /// <summary>Sets the value of the retainLastValueOnResetFlag.</summary>
        /// <param name="retainValue">new value of the flag.</param>
        public void setRetainLastValueOnReset(bool retainValue)
        {
            this._retainLastValueOnReset = retainValue;
        }

        /// <summary>
        /// Updates this <c>Accumulate</c> object by fetching the actual value of the <c>ValueSupplier</c>
        /// and processing it. The <c>ValueSupplier</c> can be passed in the constructor of this
        /// <c>Accumulate</c> object. This <c>update()</c> method complies with the one described in DESMO,
        /// see [Page91].
        /// </summary>
        public void update()
        {
            // check if the experiment is already running
            if (!getModel().getExperiment().isRunning())
            {
                return; // experiment is not running, don't update, just return
            }

            TimeInstant now = presentTime(); // what's the time?

            if (_lastUpdate == null)
                _lastUpdate = now;

            // how long since the last update or reset?
            long periodValueValidEps = TimeOperations.diff(now, _lastUpdate).getTimeInEpsilon();

            // get hold of the value that was valid until now
            double untilNowVal = getLastValue();

            // update the ValueStatistics
            base.update();

            // process update
            this.internalUpdate(untilNowVal, periodValueValidEps);
        }

        /// <summary>
        /// Updates this <c>Accumulate</c> object with the double value given as parameter. In some cases it
        /// might be more convenient to pass the value this <c>Accumulate</c> will be updated with directly
        /// within the <c>update(double val)</c> method instead of going via the <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="val">The value with which this <c>Accumulate</c> will be updated.</param>
        public void update(double val)
        {

            // check if the experiment is already running
            if (!getModel().getExperiment().isRunning())
            {
                return; // experiment is not running, don't update, just return
            }

            TimeInstant now = presentTime(); // what's the time?

            // initialize _lastUpdate on first call (reset() was never called)
            if (_lastUpdate == null)
                _lastUpdate = now;

            // how long since the last update or reset?
            long periodValueValidEps = TimeOperations.diff(now, _lastUpdate).getTimeInEpsilon();

            // get hold of the value that was valid until now
            double untilNowVal = getLastValue();

            // update the ValueStatistics
            base.update(val);

            // process update
            this.internalUpdate(untilNowVal, periodValueValidEps);
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
        /// The Object with which this <c>Accumulate</c> is updated. Normally a Double value which is added
        /// to the statistics or <c>null</c>.
        /// </param>
        public void update(Observable o, object arg)
        {

            if (o == null) // null was passed instead of an Observable
            {
                sendWarning(
                        "Attempt to update a Accumulate with no reference to an "
                                + "Observable. The actual value of '"
                                + getValueSupplier().getName()
                                + "' will be fetched and processed anyway.",
                        "Accumulate: " + this.getName()
                                + " Method: update (Observable o," + " Object arg)",
                        "The passed Observable: o in this method is only a null pointer.",
                        "The update()-method was not called via notifyObservers() from an "
                                + "Observable. Who was calling it? Why don't you let the Observable do"
                                + " the work?");
            }

            // check if the experiment is already running
            if (!getModel().getExperiment().isRunning())
            {
                return; // experiment is not running, don't update, just return
            }

            TimeInstant now = presentTime(); // what's the time?

            if (_lastUpdate == null)
                _lastUpdate = now;

            // how long since the last update or reset?
            long periodValueValidEps = TimeOperations.diff(now, _lastUpdate).getTimeInEpsilon();

            // get hold of the value that was valid until now
            double untilNowVal = getLastValue();

            // update the ValueStatistics
            base.update(o, arg);

            // process update
            this.internalUpdate(untilNowVal, periodValueValidEps);
        }

        /// <summary>
        /// Internal method to update the time-weighted mean and sum of the squares of the differences from
        /// the mean of values so far with a new sample.
        /// </summary>
        /// <param name="value">The new sample.</param>
        /// <param name="periodValueValid">
        /// The length of the period in epsilon during which the sample was valid.
        /// </param>
        private void internalUpdate(double value, long periodValueValid)
        {

            // end of a pause?
            if (this._pausedSince != null)
            {

                // determine pause duration
                TimeSpan pauseDuration = TimeOperations.diff(_pausedSince, presentTime());

                // update total pause duration
                _pausedPeriodSinceLastReset = TimeOperations.add(_pausedPeriodSinceLastReset, pauseDuration);

                // no longer paused
                this._pausedSince = null;

                // no pause: maybe is the first update=     
            }
            else if (this._firstValueReadAt == null)
            {

                // just store the instant as there is no old value to apply to past period
                this._firstValueReadAt = this.presentTime();

                // normal case: second+ call, no pause    
            }
            else
            {

                // ignore values valid for a zero period
                if (periodValueValid > 0L)
                {

                    if (double.IsNaN(this._mean))
                    { // First entry: _mean not yet computed, seed it directly
                        _mean = value;
                        _sumOfSquaredDevsFromMean = 0.0;
                    }
                    else
                    { // Further entries: incremental weighted-mean update
                        long periodMeasured = getPeriodMeasured().getTimeInEpsilon();
                        double _m_old = _mean;
                        _mean += (value - _mean) / periodMeasured * periodValueValid;
                        _sumOfSquaredDevsFromMean += (value - _m_old) * (value - _mean) * periodValueValid;
                    }
                }

            }

            // store update instant
            _lastUpdate = this.presentTime(); // update the time of the last change
        }

        /**
         * Temporarily suspend data collection to exclude a period, e.g. to disregard
         * the night period in which a plant is closed from machine utilization statistics.
         * Data collection will be resumed automatically on calling an update(...)-method.       
         */
        public void pause()
        {

            if (this._pausedSince != null)
            {
                sendWarning(
                        "Attempt to pause an Accumulate which is already paused."
                                + " Method call will be ignored.",
                        "Accumulate: " + this.getName()
                                + " Method: pause()",
                        "Multiple calls to pause this Accumulate.",
                        "Make sure to call pause() only once before resuming data collection "
                                + " by calling the appropriate update(...)-method.");
                return;
            }

            if (this._firstValueReadAt == null)
            {
                sendWarning(
                        "Attempt to pause an Accumulate which is not yet collecting data."
                                + " Method call will be ignored.",
                        "Accumulate: " + this.getName()
                                + " Method: pause()",
                        "Multiple calls to pause this Accumulate.",
                        "No need to pause an Accumulate that has not yet collected any data as"
                                + " data collection not will start before first update anyway.");
                return;
            }

            // assume an Update now to reflect the period between last Update and now,
            // unless there has already been an update at this instant.
            if (!TimeInstant.isEqual(this._lastUpdate, this.presentTime()))
            {
                this.update(this.getLastValue());
                this.incrementObservations(-1); //...not counting as observation
            }

            // remember instant at which the pause has started 
            this._pausedSince = this.presentTime();
        }
    } // end class Accumulate
}