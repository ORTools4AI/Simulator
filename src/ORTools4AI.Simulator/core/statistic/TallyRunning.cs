// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen, Johannes G&ouml;bel
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * The <code>TallyRunning</code> class is extended from
     * <code>Tally</code>. Same as <code>Tally</code>, it provides statistic 
     * analysis about one value (mean, standard deviation, minimum, maximum).
     * In addition, this class is able to calculate this data based on 
     * the last <i>n</i> samples only, subject to <i>n</i> passed to the
     * constructor.
     * 
     * @see Tally
     */

    public class TallyRunning : Tally
    {

        // ****** attributes ******

        /**
         * The sum of the last n values so far
         */
        private double _sumLastN;

        /**
         * The sum of the last n values so far
         */
        private double _sumSquareLastN;

        /**
         * The last n values
         */
        private double[] _valuesLastN;

        /**
         * Number of samples to include in running statistic data   
         */
        private int _n;

        /// <summary>
        /// Constructor for a TallyRunning object that is connected to a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Tally is associated to</param>
        /// <param name="name">The name of this Tally object</param>
        /// <param name="valSup">
        /// The ValueSupplier providing the value for this Tally. The given ValueSupplier will be observed by
        /// this Tally object.
        /// </param>
        /// <param name="n">Number of samples to include in running statistic data.</param>
        /// <param name="showInReport">Flag for showing the report about this Tally.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Tally.</param>
        public TallyRunning(Model ownerModel, string name, ValueSupplier valSup,
                int n, bool showInReport, bool showInTrace) : base(ownerModel, name, valSup, showInReport, showInTrace)
        {

            // call the constructor of Tally
            // super(ownerModel, name, valSup, showInReport, showInTrace);

            // n is less than 2
            if (n < 2)
            {
                sendWarning("Attempt to produce a TallyRunning based on the last n=" +
                        +n + " samples. Sample size will be increased to 2!", "TallyRunning: "
                        + this.getName() + " Constructor: TallyRunning"
                        + " (Model ownerModel, String name, ValueSupplier valSup, "
                        + "int n, boolean showInReport, boolean showInTrace)",
                        "The given sample size is too small.",
                        "Make sure to pass a sample size greater or equal to 2 to a new "
                        + "TallyRunning object.");

                n = 2;
            }

            // initialize storage of last n values
            this._n = n;
            this._valuesLastN = new double[n];
            this._sumLastN = 0;
            this._sumSquareLastN = 0;
        }

        // ****** methods ******

        /// <summary>
        /// Constructor for a Tally object that has no connection to a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Tally is associated to</param>
        /// <param name="name">The name of this Tally object</param>
        /// <param name="n">Number of samples to include in running statistic data.</param>
        /// <param name="showInReport">Flag for showing the report about this Tally.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Tally.</param>
        public TallyRunning(Model ownerModel, string name, int n,
                bool showInReport, bool showInTrace):base(ownerModel, name, showInReport, showInTrace)
        {

            // call the constructor of Tally
           // super(ownerModel, name, showInReport, showInTrace);

            // n is less than 2
            if (n < 2)
            {
                sendWarning("Attempt to produce a TallyRunning based on the last n=" +
                        +n + " samples. Sample size will be increased to 2!", "TallyRunning: "
                        + this.getName() + " Constructor: TallyRunning"
                        + " (Model ownerModel, String name, ValueSupplier valSup, "
                        + "int n, boolean showInReport, boolean showInTrace)",
                        "The given sample size is too small.",
                        "Make sure to pass a sample size greater or equal to 2 to a new "
                        + "TallyRunning object.");

                n = 2;
            }

            // initialize storage of last n values
            this._n = n;
            this._valuesLastN = new double[n];
            this._sumLastN = 0;
            this._sumSquareLastN = 0;
        }

        /// <summary>Returns a Reporter to produce a report about this Tally.</summary>
        /// <returns>The Reporter for this Tally.</returns>
        protected override Reporter createDefaultReporter()
        {
            return new TallyReporter(this);
        }

        /// <summary>
        /// Returns the mean value of the last n&lt;/n&gt; values observed. Note that if less than n samples
        /// values are observed this far, the value returned will only include fewer (i.e. all) samples.
        /// </summary>
        /// <returns>The mean value of the last n&lt;/n&gt; values observed so far.</returns>
        public double getMeanLastN()
        {
            if (getObservations() == 0)
            {
                sendWarning(
                        "Attempt to get a mean value, but there is not "
                                + "sufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "TallyRunning: " + this.getName() + " Method: double getMean()",
                        "You can not calculate a mean value as long as no data is collected.",
                        "Make sure to ask for the mean value only after some data has been "
                                + "collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }

            // calculate the mean value
            double meanValue = this._sumLastN / Math.Min(this.getObservations(), this._n);
            // return the rounded mean value
            return round(meanValue);
        }

        /// <summary>
        /// Returns the standard deviation of the last n&lt;/n&gt; values observed. Note that if less than n
        /// samples values are observed this far, the value returned will only include fewer (i.e. all)
        /// samples.
        /// </summary>
        /// <returns>The standard deviation of the last n&lt;/n&gt; values observed so far.</returns>
        public double getStdDevLastN()
        {
            long obs = Math.Min(this.getObservations(), this._n);

            if (obs < 2)
            {
                sendWarning(
                        "Attempt to get a standard deviation, but there is not "
                                + "sufficient data yet. UNDEFINED (-1.0) will be returned!",
                        "TallyRunning: " + this.getName() + " Method: double getStdDev()",
                        "A standard deviation can not be calculated as long as no data is "
                                + "collected.",
                        "Make sure to ask for the standard deviation only after some data "
                                + "has been collected already.");

                return UNDEFINED; // return UNDEFINED = -1.0
            }

            // calculate the standard deviation
            double stdDev = Math.Sqrt(Math.Abs(obs * this._sumSquareLastN - this._sumLastN * this._sumLastN)
                    / (obs * (obs - 1)));
            // return the rounded standard deviation
            return round(stdDev);
        }

        /**
         * Resets this TallyRunning object by resetting all variables to 0.0 .
         */
        public void reset()
        {
            base.reset(); // reset the Tally, too.

            this._sumLastN = this._sumSquareLastN = 0.0;
        }

        /// <summary>
        /// Updates this <c>TallyRunning</c> object by fetching the actual value of the <c>ValueSupplier</c>
        /// and processing it. The <c>ValueSupplier</c> is passed in the constructor of this
        /// <c>TallyRunning</c> object. This <c>update()</c> method complies with the one described in DESMO,
        /// see [Page91].
        /// </summary>
        public void update()
        {
            base.update(); // call the update() method of Tally

            long obs = getObservations();
            double lastVal = getLastValue();
            int index = (int)((obs - 1) % this._n);

            this._sumLastN += lastVal - this._valuesLastN[index];
            this._sumSquareLastN += lastVal * lastVal - this._valuesLastN[index] * this._valuesLastN[index];
            // note: in case obs < n, this.valuesLastN[index] will be zero

            this._valuesLastN[index] = lastVal;
        }

        /// <summary>
        /// Updates this <c>TallyRunning</c> object with the double value given as parameter. In some cases
        /// it might be more convenient to pass the value this <c>TallyRunning</c> will be updated with
        /// directly within the <c>update(double val)</c> method instead of going via the
        /// <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="val">The value with which this <c>TallyRunning</c> will be updated.</param>
        public void update(double val)
        {
            // call the update(double val) method of Tally
            base.update(val);

            long obs = getObservations();
            double lastVal = getLastValue();
            int index = (int)((obs - 1) % this._n);

            this._sumLastN += lastVal - this._valuesLastN[index];
            this._sumSquareLastN += lastVal * lastVal - this._valuesLastN[index] * this._valuesLastN[index];
            // note: in case obs < n, this.valuesLastN[index] will be zero

            this._valuesLastN[index] = lastVal;
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
        /// The Object with which this <c>TallyRunning</c> is updated. Normally a double number which is
        /// added to the statistics or <c>null</c>.
        /// </param>
        public void update(Observable o, Object arg)
        {
            if (o == null) // null was passed instead of an Observable
            {
                sendWarning(
                        "Attempt to update a TallyRunning with no reference to an "
                                + "Observable. The actual value of '"
                                + getValueSupplier().getName()
                                + "' will be fetched and processed anyway.",
                        "TallyRunning: " + this.getName() + " Method: update (Observable "
                                + "o, Object arg)",
                        "The passed Observable: o in this method is only a null pointer.",
                        "The update()-method was not called via notifyObservers() from an "
                                + "Observable. Who was calling it? Why don't you let the Observable do"
                                + " the work?");
            }

            base.update(o, arg); // call the update() method of Tally

            long obs = getObservations();
            double lastVal = getLastValue();
            int index = (int)((obs - 1) % this._n);

            this._sumLastN += lastVal - this._valuesLastN[index];
            this._sumSquareLastN += lastVal * lastVal - this._valuesLastN[index] * this._valuesLastN[index];
            // note: in case obs < n, this.valuesLastN[index] will be zero

            this._valuesLastN[index] = lastVal;
        }


        /// <summary>Returns the maximum of the last n values.</summary>
        /// <returns>The maximum value of the last n values.</returns>
        public double getMaximumLastN()
        {

            long obs = getObservations();
            if (obs == 0) return 0;

            double maximum = double.MinValue;
            for (int i = 0; i < Math.Min(this._n, obs); i++)
            {
                if (this._valuesLastN[i] > maximum) maximum = this._valuesLastN[i];
            }

            // return the rounded maximum
            return round(maximum);
        }

        /// <summary>Returns the minimum of the last n values.</summary>
        /// <returns>The minimum value of the last n values.</returns>
        public double getMinimumLastN()
        {

            long obs = getObservations();
            if (obs == 0) return 0;

            double minimum = double.MinValue;
            for (int i = 0; i < Math.Min(this._n, obs); i++)
            {
                if (this._valuesLastN[i] < minimum) minimum = this._valuesLastN[i];
            }
            // return the rounded minimum
            return round(minimum);
        }

        /// <summary>Returns the sample size n.</summary>
        /// <returns>The sample size n.</returns>
        public int getSampleSizeN()
        {
            return this._n;
        }
    } // end class TallyRunning
}