// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author edited by Lorna Slawski (changes in implementation and added possibility of non-equidistant sections as well as the Chi-squared test)
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * The <code>Histogram</code> class is providing a statistic analysis about
     * values. An interval is divided into sections with an under-
     * and an overflow section. When a value is updated it will be decided to which
     * section it belongs to and the counter for that section will be updated. In
     * the end the report will show how many values belong to which section. <br />
     */
    public class Histogram : Tally
    {

        // ****** attributes ******

        /**
         * The array holding the lower limits of the interval sections and the upper limit as the final value.
         */
        private double[] _range;

        /**
         * The array representing the sections of the interval.
         */
        private long[] _table;

        /**
         * Remove unused cells on the right from report view, if set to <code>true</code>.
         */
        private bool _condensed;

        // ****** methods ******

        /// <summary>
        /// Constructor for a Histogram object with equidistant interval cells that will NOT be connected to
        /// a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Histogram is associated to.</param>
        /// <param name="name">The name of this Histogram object.</param>
        /// <param name="lowerEnd">The lower end of the interval.</param>
        /// <param name="upperEnd">The upper end of the interval.</param>
        /// <param name="cells">
        /// The number of cells (sections) the given interval will be divided into.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this Histogram.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Histogram.</param>
        public Histogram(Model ownerModel, string name, double lowerEnd,
                double upperEnd, int cells, bool showInReport,
                bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {
            // call the constructor of Tally
            //super(ownerModel, name, showInReport, showInTrace);

            // check the given parameters and initialize the range array
            checkParamAndInitRange(lowerEnd, upperEnd, cells);

            // initialize the table with zero for each cell
            initTable();

            // do not condense data
            _condensed = false;
        }

        /// <summary>
        /// Constructor for a Histogram object with equidistant interval cells that will be connected to a
        /// <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Histogram is associated to.</param>
        /// <param name="name">The name of this Histogram object.</param>
        /// <param name="valSup">
        /// The ValueSupplier providing the value for this Histogram. The given ValueSupplier will be
        /// observed by this Histogram object.
        /// </param>
        /// <param name="lowerEnd">The lower end of the interval.</param>
        /// <param name="upperEnd">The upper end of the interval.</param>
        /// <param name="cells">
        /// The number of cells (sections) the given interval will be divided into.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this Histogram.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Histogram.</param>
        public Histogram(Model ownerModel, string name, ValueSupplier valSup,
                double lowerEnd, double upperEnd, int cells, bool showInReport,
                bool showInTrace) : base(ownerModel, name, valSup, showInReport, showInTrace)
        {
            // call the constructor of Tally
            //super(ownerModel, name, valSup, showInReport, showInTrace);

            // valsup is no valid ValueSupplier
            if (valSup == null)
            {
                sendWarning(
                        "Attempt to produce a Histogram about a non existing "
                                + "ValueSupplier. The command will be ignored!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, String name, ValueSupplier valSup, "
                                        + "double lowerEnd, double upperEnd, int cells, "
                                        + "boolean showInReport, boolean showInTrace)",
                                        "The given ValueSupplier: valSup is only a null pointer.",
                                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                                + "Histogram object.");

                return; // just return
            }

            // check the given parameters and initialize the range array
            checkParamAndInitRange(lowerEnd, upperEnd, cells);

            // initialize the table with zero for each cell
            initTable();

            // do not condense data
            _condensed = false;
        }

        /// <summary>
        /// Constructor for a Histogram object with user-defined interval cells that will NOT be connected to
        /// a <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Histogram is associated to.</param>
        /// <param name="name">The name of this Histogram object.</param>
        /// <param name="range">
        /// The interval with the lower limit of each cell and the upper limit of the interval as the final
        /// value.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this Histogram.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Histogram.</param>
        public Histogram(Model ownerModel, string name, double[] range,
                bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {

            // call the constructor of Tally
            //super(ownerModel, name, showInReport, showInTrace);

            // check the given parameters and initialize the range array
            checkParamAndInitRange(range);

            // initialize the table with zero for each cell
            initTable();

            // do not condense data
            _condensed = false;
        }

        /// <summary>
        /// Constructor for a Histogram object with user-defined interval cells that will be connected to a
        /// <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="ownerModel">The model this Histogram is associated to.</param>
        /// <param name="name">The name of this Histogram object.</param>
        /// <param name="valSup">
        /// The ValueSupplier providing the value for this Histogram. The given ValueSupplier will be
        /// observed by this Histogram object.
        /// </param>
        /// <param name="range">
        /// The interval with the lower limit of each cell and the upper limit of the interval as the final
        /// value.
        /// </param>
        /// <param name="showInReport">Flag for showing the report about this Histogram.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this Histogram.</param>
        public Histogram(Model ownerModel, string name, ValueSupplier valSup,
                double[] range, bool showInReport, bool showInTrace) : base(ownerModel, name, valSup, showInReport, showInTrace)
        {

            // call the constructor of Tally
            //super(ownerModel, name, valSup, showInReport, showInTrace);

            // valsup is no valid ValueSupplier
            if (valSup == null)
            {
                sendWarning(
                        "Attempt to produce a Histogram about a non existing "
                                + "ValueSupplier. The command will be ignored!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, String name, ValueSupplier valSup, "
                                        + "double lowerEnd, double upperEnd, int cells, "
                                        + "boolean showInReport, boolean showInTrace)",
                                        "The given ValueSupplier: valSup is only a null pointer.",
                                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                                + "Histogram object.");

                return; // just return
            }

            // check the given parameters and initialize the range array
            checkParamAndInitRange(range);

            // initialize the table with zero for each cell
            initTable();

            // do not condense data
            _condensed = false;
        }

        /// <summary>
        /// Returns whether output is condensed, i.e.\ empty cells on the right tail not shown.
        /// </summary>
        /// <returns>
        /// Not displaying unused cells in report view (<c>true</c>) or printing all cells (<c>false</c>).
        /// </returns>
        public bool isCondensed()
        {
            return this._condensed;
        }

        /// <summary>
        /// Sets whether output should be condensed, i.e.\ empty cells on the right tail not shown.
        /// </summary>
        /// <param name="condensed">
        /// Not displaying unused cells in report view (<c>true</c>) or printing all cells (<c>false</c>).
        /// </param>
        public void setCondensed(bool condensed)
        {
            this._condensed = condensed;
        }


        /// <summary>
        /// Changes the parameters of the interval and its number of segments. Can only be done after
        /// construction of a Histogram or after a reset.
        /// </summary>
        /// <param name="low">The lower end of the interval.</param>
        /// <param name="up">The upper end of the interval.</param>
        /// <param name="cel">The number of cells (sections) the given interval will be divided into.</param>
        public void changeParameters(double low, double up, int cel)
        {
            if (getObservations() > 0) // Histogram has been used already
            {
                sendWarning("Attempt to change the parameters of a Histogram, but "
                        + "it has been used already. The command will be ignored!",
                        "Histogram: " + this.getName()
                        + " Method: changeParameters( double "
                        + "low, double up, int cel )",
                        "The parameters of a Histogram can not be changed when the Histogram "
                                + "has been used already.",
                                "Make sure to reset the Histogram right before changing its "
                                        + "parameters.");

                return; // do nothing, just return
            }

            // check the given parameters and initialize the range array
            this.checkParamAndInitRange(low, up, cel);

            // initialize the table with zero for each cell
            initTable();
        }

        /// <summary>
        /// Changes the parameters of the interval and its number of segments. Can only be done after
        /// construction of a Histogram or after a reset.
        /// </summary>
        /// <param name="range">
        /// The array holding the lower limits of the interval sections and the upper limit as the final
        /// value.
        /// </param>
        public void changeParameters(double[] range)
        {
            if (getObservations() > 0) // Histogram has been used already
            {
                sendWarning("Attempt to change the parameters of a Histogram, but "
                        + "it has been used already. The command will be ignored!",
                        "Histogram: " + this.getName()
                        + " Method: changeParameters( double "
                        + "low, double up, int cel )",
                        "The parameters of a Histogram can not be changed when the Histogram "
                                + "has been used already.",
                                "Make sure to reset the Histogram right before changing its "
                                        + "parameters.");

                return; // do nothing, just return
            }

            // check the given parameters and initialize the range array
            this.checkParamAndInitRange(range);

            // initialize the table with zero for each cell
            initTable();
        }

        /**
         * Checks the segmentation of the given equidistant interval and then initializes the range array.
         */
        protected void checkParamAndInitRange(double lowerLimit, double upperLimit, int cells)
        {
            if (cells <= 0) // the interval will not be divided into segments
            {
                sendWarning(
                        "Attempt to produce a Histogram about an interval, "
                                + "which is not divided into segments. The number of segments will be "
                                + "set to one!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, String name, ... int cells, ...) "
                                        + "or Method: changeParameters( ..., int cel ).",
                                        "The given number of cells is zero or negative.",
                                        "Make sure to pass a valid number of cells when constructing a new "
                                                + "Histogram object or changing its parameters.");

                cells = 1;
            }

            if (lowerLimit > upperLimit) // lowerLimit is greater than upperLimit
            {
                sendWarning(
                        "Attempt to produce a Histogram about an interval, "
                                + "which lower end is greater than its upper end. The lower and "
                                + "upper end are exchanged!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, ... double lowerEnd, double upperEnd, ...) "
                                        + "or Method: changeParameters( double low, double up, int cel ).",
                                        "The given lower end is greater than the upper end of the interval.",
                                        "Make sure that the lower limit of the interval is smaller than the "
                                                + "upper limit when constructing a new Histogram object or changing "
                                                + "its parameters.");

                double temp = lowerLimit;
                lowerLimit = upperLimit;
                upperLimit = temp;
            }
            else
            {
                // there is no segmentation (all is one segment)
                if (lowerLimit == upperLimit && cells != 1)
                {
                    sendWarning(
                            "Attempt to produce a Histogram about an interval, "
                                    + "which lower and upper limit are the same. The number of cells is "
                                    + "set to one!",
                                    "Histogram: "
                                            + this.getName()
                                            + " Constructor: Histogram"
                                            + " (Model ownerModel, ... double lowerEnd, double upperEnd, ...) "
                                            + "or Method: changeParameters( double low, double up, int cel ).",
                                            "The given lower and upper end of the interval are the same, but "
                                                    + "the interval is divided into more than one segment.",
                                                    "Make sure that in case the lower and upper limit of the interval "
                                                            + "are the same the number of cells is one.");

                    cells = 1;

                } // end inner if
            } // end else
              // Init range array
            this._range = new double[cells + 1];
            for (int i = 0; i < this._range.Length - 1; i++)
                this._range[i] = lowerLimit + i * (upperLimit - lowerLimit) / cells;
            this._range[this._range.Length - 1] = upperLimit;
        }

        /**
         * Checks the segmentation of the given user-defined interval and then initializes the range array.
         */
        protected void checkParamAndInitRange(double[] range)
        {
            double temp;
            if (range.Length == 0 || range.Length == 1) // the interval will not be divided into segments of length > 0
            {
                temp = 0;
                sendWarning(
                        "Attempt to produce a Histogram only containing of the underflow and the overflow. "
                                + "There will an interval of the length of 0 between these. The number of segments will be "
                                + "set to one!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, String name, double[] range, ...) "
                                        + "or Method: changeParameters( double[] range ).",
                                        "There are no given cells. ",
                                        "Make sure to pass a valid number of cells when constructing a new "
                                                + "Histogram object or changing its parameters.");

                // correct and initialize the range array
                if (range.Length == 1)
                {
                    temp = range[0];
                }
                range = new double[2];
                range[0] = range[1] = temp;
            }

            // check the order of the given limits
            bool errorMsgAsc = false;
            // using bubblesort as we can expect the given array to be reasonably sorted
            for (int i = 0; i < range.Length - 1; i++)
            {
                for (int j = i + 1; j < range.Length; j++)
                {
                    if (range[i] > range[j])
                    {
                        errorMsgAsc = true;
                        temp = range[i];
                        range[i] = range[j];
                        range[j] = temp;
                    }
                }
            }
            if (errorMsgAsc) // the limits were not ascending
            {
                sendWarning(
                        "Attempt to produce a Histogram about an interval, "
                                + "which has at least one section whose upper is smaller than its lower limit. "
                                + "The lower and upper limits are being exchanged!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, ... double[] range, ...) "
                                        + "or Method: changeParameters( double[] range ).",
                                        "The given lower end is greater than the upper end of the interval.",
                                        "Make sure to have a strictly ascending order "
                                                + "when constructing a new Histogram object or changing "
                                                + "its parameters.");
            }

            // check for equal entries
            bool errorMsgEq = false;
            double[] rangetemp = new double[range.Length];
            int count = 0;
            for (int i = 0; i < range.Length - 2; i++)
            {
                if (!(range[i] == range[i + 1]))
                {
                    rangetemp[count] = range[i];
                    count++;
                }
                else
                {
                    errorMsgEq = true;
                }
            }
            rangetemp[count] = range[range.Length - 2];
            count++;
            rangetemp[count] = range[range.Length - 1];
            range = new double[count + 1];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = rangetemp[i];
            }
            if (errorMsgEq)
            {
                sendWarning(
                        "Attempt to produce a Histogram about an interval, "
                                + "which has at least two sections in a row whose lower limits are the same. At least one "
                                + "lower limit is being deleted!",
                                "Histogram: "
                                        + this.getName()
                                        + " Constructor: Histogram"
                                        + " (Model ownerModel, ... double[] range, ...) "
                                        + "or Method: changeParameters( double[] range ).",
                                        "The given interval has at least two sections with the same lower limit. ",
                                        "Make sure to have a strictly ascending order "
                                                + "when constructing a new Histogram object or changing "
                                                + "its parameters.");
            }

            // Init range array;
            _range = range;

        }

        /// <summary>
        /// Performs Pearson's Chi-square test on given frequencies, a fixed degree of freedom and desired
        /// probability. The frequencies are given in an array either including under- and overflow cells or
        /// not. The degree of freedom is set to 1 deducted from the number of given cells. On an error the
        /// Chi-squared test is not performed and false is returned. Details on errors are given out in the
        /// error message log. The result is <c>true</c> if the null hypothesis can not be rejected.
        /// </summary>
        /// <param name="values">Array of assumed frequencies for each cell.</param>
        /// <param name="confidence">(1-alpha) probability level.</param>
        /// <returns>
        /// <c>true</c> if the the null hypothesis can not be rejected. <c>false</c> on error or if the null
        /// hypothesis has to be rejected.
        /// </returns>
        public bool chiSquareTest(long[] values, double confidence)
        {

            // check if number of cells is valid
            if (!((values.Length == this.getCells() + 2) || (values.Length == this.getCells()) || (values.Length > 1)))
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test on an invalid number of cells!  ",
                        "chiSquareTest(long[], double) : ",
                        "Too few or too many cells. ",
                        "Make sure to have a valid number of cells "
                                + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check if we have a reasonable amount of observations
            else if (this.getObservations() < 3)
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test on an insufficient data amount,  "
                                + "there are less than three observations!  ",
                                "chiSquareTest(long[], double) : ",
                                "Too few observations. ",
                                "Make sure to have a sufficient amount of observations "
                                        + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check if we have a reasonable probability
            else if (confidence < 0 || confidence > 1)
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test with an illegal desired probability!  ",
                        "chiSquareTest(long[], double) : ",
                        "Illegal desired probability. ",
                        "Make sure to have a valid desired probability "
                                + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check for 0 as expected entry
            foreach (double val in values)
            {
                if (val == 0)
                {         // exit on 0 to avoid zero division
                    sendWarning(
                            "Attempt to perform a Chi-squared test with an expected value of 0!  ",
                            "chiSquareTest(long[], double) : ",
                            "Invalid expected value. ",
                            "Make sure to have a set of valid expected values "
                                    + "when calling the chiSquareTest() method.  ");
                    return false;
                }
            }

            return chiSquareTest(values, values.Length - 1, confidence);
        }

        /// <summary>
        /// Performs Pearson's Chi-square test on given frequencies, degrees of freedom and desired
        /// probability. The frequencies are given in an array either including under- and overflow cells or
        /// not. On error the Chi-squared test is not performed and false is returned. Details on errors are
        /// given out in the error message log. The result is true if the null hypothesis can not be
        /// rejected.
        /// </summary>
        /// <param name="values">Array of assumed frequencies for each cell.</param>
        /// <param name="degFreedom">Degrees of freedom of the test.</param>
        /// <param name="confidence">(1-alpha) probability level.</param>
        /// <returns>
        /// <c>true</c> if the the null hypothesis can not be rejected. <c>false</c> on error or if the null
        /// hypothesis has to be rejected.
        /// </returns>
        public bool chiSquareTest(long[] values, int degFreedom, double confidence)
        {

            // check if number of cells is valid
            if (!((values.Length == this.getCells() + 2) || (values.Length == this.getCells()) || (values.Length > 1)))
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test on an invalid number of cells!  ",
                        "chiSquareTest(long[], int, double) : ",
                        "Too few or too many cells. ",
                        "Make sure to have a valid number of cells "
                                + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check if we have a reasonable amount of observations
            else if (this.getObservations() < 3)
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test on an insufficient data amount,  "
                                + "there are less than three observations!  ",
                                "chiSquareTest(long[], int, double) : ",
                                "Too few observations. ",
                                "Make sure to have a sufficient amount of observations "
                                        + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check if we have a reasonable probability
            else if (confidence < 0 || confidence > 1)
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test with an illegal desired probability!  ",
                        "chiSquareTest(long[], int, double) : ",
                        "Illegal desired probability. ",
                        "Make sure to have a valid desired probability "
                                + "when calling the chiSquareTest() method.  ");
                return false;
            }

            // check if degree of freedom is valid
            else if (degFreedom <= 0 || degFreedom >= values.Length)
            {
                sendWarning(
                        "Attempt to perform a Chi-squared test with an illegal degree of freedom!  ",
                        "chiSquareTest(long[], int, double) : ",
                        "Illegal degree of freedom. ",
                        "Make sure to have a valid degree of freedom "
                                + "when calling the chiSquareTest() method.  ");
                return false;
            }

            else
            {
                long sumValuesObserved = 0;
                long sumValuesExpected = 0;

                // summarize expected values
                double[] expectedEntries = new double[values.Length];
                foreach (double val in values)
                {
                    if (val == 0)
                    {         // exit on 0 to avoid zero division
                        sendWarning(
                                "Attempt to perform a Chi-squared test with an expected value of 0!  ",
                                "chiSquareTest(long[], int, double) : ",
                                "Invalid expected value. ",
                                "Make sure to have a set of valid expected values "
                                        + "when calling the chiSquareTest() method.  ");
                        return false;
                    }
                    else
                    {
                        sumValuesExpected += (long)val;
                    }
                }
                // summarize observed values
                int cell;
                if (values.Length == this.getCells())
                {  // without under- and overflow
                    cell = 1;
                }
                else
                {
                    cell = 0;                           // with under- and overflow
                }
                for (int i = cell; i < values.Length + cell; i++)
                {
                    sumValuesObserved += this.getObservationsInCell(i);
                }
                // expected frequency
                for (int i = 0; i < values.Length; i++)
                {
                    expectedEntries[i] = (double)(values[i]) / (double)(sumValuesExpected) * sumValuesObserved;
                }
                // calculation of chiSquared
                double testStat = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    testStat += Math.Pow((this.getObservationsInCell(cell)) - expectedEntries[i], 2) / expectedEntries[i];
                    cell++;
                }
                // chiSquared for degrees of freedom and  probability level
                //ChiSquaredDistributionImpl chiSquared = new ChiSquaredDistributionImpl(degFreedom);

                // comparison
                bool result = false;
                //try {
                //    result = !(testStat > chiSquared.inverseCumulativeProbability(confidence));
                //} catch (Exception e) {
                //    //e.printStackTrace();
                //}
                // trace not does not make much sense as the test is performed after the simulation has finished.
                // sendTraceNote("result of chi-squared test for " + this.getQuotedName() +  "is being returned. ");
                return result;
            }
        }

        /// <summary>Returns a Reporter to produce a report about this Histogram.</summary>
        /// <returns>The Reporter for this Histogram.</returns>
        protected override Reporter createDefaultReporter()
        {
            return new HistogramReporter(this);
        }

        /// <summary>Returns the number of cells the interval is divided into.</summary>
        /// <returns>The number of cells the interval is divided into.</returns>
        public int getCells()
        {
            return this._range.Length - 1;
        }

        /// <summary>Returns the mean width of all cells.</summary>
        /// <returns>The mean width of all cells.</returns>
        /// <remarks>The same functionality is given by getMeanWidth().</remarks>
        //@Deprecated 
        public double getCellWidth()
        {
            return round((this.getUpperLimit() - this.getLowerLimit(1)) / this.getCells());
        }

        /// <summary>Returns the mean width of all cells.</summary>
        /// <returns>The mean width of all cells.</returns>
        public double getMeanWidth()
        {
            return round((this.getUpperLimit() - this.getLowerLimit(1)) / this.getCells());
        }

        /// <summary>
        /// Returns the lower limit of the given cell. If the given cell is negative, <c>UNDEFINED</c> (-1)
        /// will be returned.
        /// </summary>
        /// <param name="cell">
        /// The cell for which we want to know its lower limit. Should be zero or positive.
        /// </param>
        /// <returns>The lower limit of the given cell.</returns>
        public double getLowerLimit(int cell)
        {
            if (cell < 0 || cell > this.getCells() + 1)
            {
                sendWarning("Attempt to get a lower limit of a not known cell. "
                        + "UNDEFINED (-1) will be returned!", "Histogram: "
                                + this.getName() + " Method: getLowerLimit( int cell ).",
                                "The passed int: cell in this method is negative or greater than "
                                        + "the largest cell number.",
                        "Make sure to ask the lower limit only for valid cell numbers.");

                return UNDEFINED; // return UNDEFINED (-1)
            }

            if (cell == 0)
            {
                return double.MinValue;
            }
            else
            {
                return StatisticObject.round(this._range[cell - 1]);
            }
        }

        /// <summary>Returns the number of the first cell holding the maximum value.</summary>
        /// <returns>The number of one cell holding the maximum value.</returns>
        /// <remarks>
        /// Gives the same result as the first entry of the returned array of getMostFrequentedCells().
        /// </remarks>
        //@Deprecated 
        public int getMostFrequentedCell()
        {
            return this.getMostFrequentedCells()[0];
        }

        /// <summary>Returns the numbers of the most frequented cells, so far.</summary>
        /// <returns>An array with the numbers of the most frequented cells.</returns>
        public int[] getMostFrequentedCells()
        {
            long max = 0; // maximum value
            int numMaxCells = 0;
            int[] maxCellNo = new int[this.getCells() + 2];

            for (int i = 0; i < this.getCells() + 2; i++) // go through all cells
            {
                if (max == this._table[i])
                {
                    numMaxCells++;
                    maxCellNo[numMaxCells - 1] = i;
                }
                else if (max < this._table[i]) // new maximum frequency found?
                {
                    maxCellNo = new int[this.getCells()];
                    numMaxCells = 1;
                    maxCellNo[numMaxCells - 1] = i;
                    max = this._table[i]; // set the new highest frequency
                }
            }

            int[] returnMaxCellNo = new int[numMaxCells];
            for (int i = 0; i < numMaxCells; i++)
            {
                returnMaxCellNo[i] = maxCellNo[i];
            }
            return returnMaxCellNo;
        }

        /// <summary>Returns the observations made for the given cell, so far.</summary>
        /// <param name="cell">The cell of which want to get the number of observations made for.</param>
        /// <returns>The observations made for the given cell.</returns>
        public long getObservationsInCell(int cell)
        {
            if (cell < 0 || cell > this.getCells() + 1)
            {   // cell 0: underflow, cell this.getCells() + 1: overflow
                sendWarning("Attempt to get the number of observations from a not "
                        + "known cell. Zero (0) will be returned!", "Histogram: "
                                + this.getName() + " Method: getObservationsInCell"
                                + "( int cell ).",
                                "The passed int: cell in this method is negative or greater than "
                                        + "the largest cell number.",
                                        "Make sure to ask for the number of observations only for valid "
                                                + "cell numbers.");
                return 0; // return zero (0)
            }

            return _table[cell];
        }

        /// <summary>Returns the upper limit of the whole interval.</summary>
        /// <returns>The upper limit of the whole interval.</returns>
        public double getUpperLimit()
        {
            return this._range[this._range.Length - 1];
        }

        /**
         * Initializes the table by setting each cell counter to zero.
         */
        protected void initTable()
        {

            // make a new table; each cell is set to zero
            this._table = new long[this.getCells() + 2];
        }

        /// <summary>
        /// Resets this Histogram object by resetting the counters for each cell to zero. That means the
        /// array of the cell counters will be reset, but the interval and the number of sections this
        /// interval is divided into will remain the same. The parameters of the interval can be changed with
        /// the <c>changeParameters</c> method after the reset.
        /// </summary>
        public void reset()
        {
            base.reset(); // reset the Tally, too.

            // reset the array of cells #### only if the table already exists! Ruth 24/01/2008
            if (_table != null)
            {
                initTable();
            }
        }

        /// <summary>
        /// Updates this <c>Histogram</c> object by fetching the actual value of the <c>ValueSupplier</c> and
        /// processing it. The <c>ValueSupplier</c> is passed in the constructor of this <c>Histogram</c>
        /// object. This <c>update()</c> method complies with the one described in DESMO, see [Page91].
        /// </summary>
        public void update()
        {
            base.update(); // update Tally

            // get last value
            double val = getLastValue();

            int n = 0; // to which cell does the value belong to?

            if (val < this._range[0])
            {   // underflow
                n = 0;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            //else if(val > this._range[this._range.length - 1]){ // overflow
            else if (val >= this._range[this._range.Length - 1])
            { // overflow
                n = this.getCells() + 1;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            /*
            else if(val >= this._range[this._range.length - 2] && val <= this._range[this._range.length - 1]) { // last segment
                n = this.getCells();
            }
            */
            else
            {
                for (int i = 0; i < this._range.Length - 1; i++)
                {
                    if (val >= this._range[i] && val < this._range[i + 1])
                    {
                        n = i + 1;
                    }
                }
            }

            _table[n]++;
        }

        /// <summary>
        /// Updates this <c>Histogram</c> object with the double value given as parameter. In some cases it
        /// might be more convenient to pass the value this <c>Histogram</c> will be updated with directly
        /// within the <c>update(double val)</c> method instead of going via the <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="val">The value with which this <c>Histogram</c> will be updated.</param>
        public void update(double val)
        {
            base.update(val); // update Tally

            int n = 0; // to which cell does the value belong to?

            if (val < this._range[0])
            {   // underflow
                n = 0;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            //else if(val > this._range[this._range.length - 1]){ // overflow
            else if (val >= this._range[this._range.Length - 1])
            { // overflow
                n = this.getCells() + 1;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            /*
            else if(val >= this._range[this._range.length - 2] && val <= this._range[this._range.length - 1]) { // last segment
                n = this.getCells();
            }
            */
            else
            {
                for (int i = 0; i < this._range.Length - 1; i++)
                {
                    if (val >= this._range[i] && val < this._range[i + 1])
                    {
                        n = i + 1;
                    }
                }
            }

            _table[n]++;
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
        /// The Object with which this <c>Tally</c> is updated. Normally a double number which is added to
        /// the statistics or <c>null</c>.
        /// </param>
        public void update(Observable o, object arg)
        {
            if (o == null) // null was passed instead of an Observable
            {
                sendWarning(
                        "Attempt to update a Histogram with no reference to an "
                                + "Observable. The actual value of '"
                                + getValueSupplier().getName()
                                + "' will be fetched and processed anyway.",
                                "Histogram: " + this.getName()
                                + " Method: update (Observable " + "o, Object arg)",
                                "The passed Observable: o in this method is only a null pointer.",
                                "The update()-method was not called via notifyObservers() from an "
                                        + "Observable. Who was calling it? Why don't you let the Observable do"
                                        + " the work?");
            }

            base.update(o, arg); // update Tally

            // get last value
            double val = getLastValue();

            int n = 0; // to which cell does the value belong to?

            if (val < this._range[0])
            {   // underflow
                n = 0;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            //else if(val > this._range[this._range.length - 1]){ // overflow
            else if (val >= this._range[this._range.Length - 1])
            { // overflow
                n = this.getCells() + 1;
            }
            // changed at 6.12.2012 by Chr. M&uuml;ller (TH Wildau)
            /*
            else if(val >= this._range[this._range.length - 2] && val <= this._range[this._range.length - 1]) { // last segment
                n = this.getCells();
            }
            */
            else
            {
                for (int i = 0; i < this._range.Length - 1; i++)
                {
                    if (val >= this._range[i] && val < this._range[i + 1])
                    {
                        n = i + 1;
                    }
                }
            }

            _table[n]++;
        }
    } // end class Histogram
}