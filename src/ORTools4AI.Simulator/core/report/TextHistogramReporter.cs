// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Lorna Slawski based on the class HistogramReporter
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{
    /**
     * Captures all relevant information about the TextHistogram.
     */
    public class TextHistogramReporter : Reporter {

        // ****** attributes ******

        /**
         * The column headings of the histogram part of this TextHistogramReporter.
         * Entries should contain in the elements in the same order as the
         * <codehistEntries[]</code.
         */
        private string[] _textHistColumns;

        /**
         * The data entries of the histogram part of this TextHistogramReporter. The
         * first (leftmost) dimension of this array represents the number of
         * objects which have been counted.
         * The second dimension of this array represents each column
         * entry of the specified object. So the second dimension entries should
         * contain the data elements in the same order as defined in the
         * <codetextHistColumns[]</codearray.
         */
        private string[][] _textHistEntries;

        /**
         * The number of columns of the histogram part (table) of this.
         * TextHistogramReporter.
         */
        private int _textHistNumColumns;

        /**
         * The counted objects.
         */
        private string[] _textHistObjects;

        // ****** methods ******
        /// <summary>
        /// Constructor for a new TextHistogramReporter. Note that although any Reportable is accepted you
        /// should make sure that only subtypes of TextHistogram are passed to this constructor. Otherwise
        /// the number of column titles and their individual headings will differ from the actual content
        /// collected by this reporter.
        /// </summary>
        /// <param name="informationSource">The TextHistogram to report about.</param>

        public TextHistogramReporter(Reportable informationSource) : base(informationSource) {
            //super(informationSource); // make a Reporter (source =
            // informationSource)

            groupID = 1561; // see Reporter for more information about groupID

            numColumns = 5;
            columns = new string[numColumns];
            columns[0] = "Title";
            columns[1] = "(Re)set";
            columns[2] = "Obs";
            columns[3] = "Least";
            columns[4] = "Most";
            groupHeading = "TextHistograms";

            entries = new string[numColumns];

            // *** histogram part ***

            _textHistNumColumns = 7;
            _textHistObjects = ((TextHistogram)source).getStringsObserved();

            _textHistColumns = new string[_textHistNumColumns];
            _textHistColumns[0] = "Cell";
            _textHistColumns[1] = "Object";
            _textHistColumns[2] = "n";
            _textHistColumns[3] = "%";
            _textHistColumns[4] = "Cum. %";
            _textHistColumns[5] = "|";
            _textHistColumns[6] = "Graph";

            _textHistEntries = new string[_textHistObjects.Length + 3][];
            for (int i = 0; i < _textHistObjects.Length + 3; i++)
                _textHistEntries[i] = new string[_textHistNumColumns];
        }

        /// <summary>
        /// Returns an array of Strings each containing the data for the corresponding column in array
        /// &lt;codecolumns[]&lt;/code. Implement this method in a way, that an array of the same length as
        /// the columntitles is produced containing the data at the point of time this method is called by
        /// someone else to produce up-to-date information.
        /// </summary>
        /// <returns>Array containing the data for reporting.</returns>
        public override String[] getEntries() {
            if (source is TextHistogram) {
                // the TextHistogram we report about (source = informationSource)
                var textHist = (TextHistogram)source;

                // Title
                entries[0] = textHist.getName();
                // (Re)set
                entries[1] = textHist.resetAt().toString();
                // Obs
                entries[2] = textHist.getObservations().ToString();

                // Least
                // no observations made, so there is no least observed object
                if (textHist.getObservations() == 0) {
                    entries[3] = "Insufficient data";
                } else // return least
                {
                    entries[3] = textHist.getLeastFrequentedString();
                }

                // Most
                // no observations made, so there is no most observed object
                if (textHist.getObservations() == 0) {
                    entries[4] = "Insufficient data";
                } else // return most
                {
                    entries[4] = textHist.getMostFrequentedString();
                }
            } else {
                for (int i = 0; i < numColumns; i++) {
                    entries[i] = "Invalid source!";
                } // end for
            } // end else

            return entries;
        }

        /// <summary>
        /// Returns an array of Strings each containing the title for the corresponding column of the
        /// histogram part (table).
        /// </summary>
        /// <returns>Array containing column titles of the histogram part (table).</returns>
        public string[] getTextHistColumnTitles() {
            return (string[])_textHistColumns.Clone();
        }

        /// <summary>
        /// Returns a two-dimensional array of Strings containing the data for the histogram part of this
        /// TextHistogramReporter. Implement this method in a way, that the data is collected at the point of
        /// time this method is called by someone else to produce up-to-date information.
        /// </summary>
        /// <returns>
        /// string[][] : Array containing the data for reporting about the histogram part of this
        /// TextHistogramReporter.
        /// </returns>
        public String[][] getTextHistEntries() {
            // the Histogram we report about (source = informationSource)
            var textHist = (TextHistogram)source;

            // get hold of the accumulated percentage
            double cumPerc = 0.0;

            // flag if all remaining cells are empty
            bool tailIsEmpty = false;

            for (int j = 0; j < _textHistObjects.Length; j++) // loop through all cells
            {
                if (source is TextHistogram) {
                    // Cell
                    _textHistEntries[j][0] = (j + 1).ToString();

                    // Objectname
                    _textHistEntries[j][1] = _textHistObjects[j];

                    // n
                    _textHistEntries[j][2] = (textHist.getObservationsOfString(_textHistObjects[j]).ToString());

                    // % rounded percentage
                    // calculate the percentage with 4 digits after the decimal
                    // point
                    double perc = StatisticObject.round(100.0 * textHist
                            .getObservationsOfString(_textHistObjects[j]) / textHist
                            .getObservations());

                    cumPerc += perc; // update the accumulated percentage
                                     // to display the perc. round it to 2 digits after the decimal
                                     // point
                    perc = StatisticObject.round(perc);

                    _textHistEntries[j][3] = perc.ToString();

                    // Cum. %
                    // round the accumulated percentage
                    double rdCumPerc = StatisticObject.round(cumPerc);

                    _textHistEntries[j][4] = rdCumPerc.ToString();

                    _textHistEntries[j][5] = "|";

                    // Graph number of asterix's
                    // if all remaining cells are empty
                    if (tailIsEmpty) {
                        _textHistEntries[j][6] = "the remaining cells<brare all empty";
                        // = j; // set the no. of cells to the actual value
                        break; // the for-loop for all cells
                    }

                    // calculate the number of asterix's (one asterix per 2 percent)
                    int ast = (int)(perc / 2.0);

                    String lineOfAsterix = ""; // make an empty String

                    // if percentage between zero and 2 percent
                    if (textHist.getObservationsOfString(_textHistObjects[j]) > 0) {
                        lineOfAsterix = "*"; // start with one asterix
                    }

                    for (int k = 0; k < ast; k++) // fill the String with
                                                  // asterix's
                    {
                        lineOfAsterix = lineOfAsterix + "*";
                    }

                    _textHistEntries[j][6] = lineOfAsterix;
                } else {
                    for (int i = 0; i < _textHistNumColumns; i++) {
                        _textHistEntries[j][i] = "Invalid source!";
                    } // end for
                } // end else
            } // end for
            return _textHistEntries;
        }

        /// <summary>
        /// Returns the number of columns of the histogram part (table) of this TextHistogramReporter.
        /// </summary>
        /// <returns>
        /// The number of columns of the histogram part (table) of this TextHistogramReporter.
        /// </returns>
        public int getTextHistNumColumns() {
            return _textHistNumColumns;
        }

        /// <summary>Returns the number of Strings the given TextHistogram has saved.</summary>
        /// <returns>The number of cells the interval of the given Histogram is divided into.</returns>
        public int getNoOfStrings() {
            return _textHistObjects.Length;
        }

        /// <summary>
        /// Returns the number of observations made by the TextHistogram object. This method call is passed
        /// on to the TextHistogram object.
        /// </summary>
        /// <returns>The number of observations made by the TextHistogram object.</returns>
        public long getObservations() {

            return source.getObservations(); // that's all
        }

        /*@TODO: Comment */
        public bool isContinuingReporter() {
            return true;
        }

    }
}