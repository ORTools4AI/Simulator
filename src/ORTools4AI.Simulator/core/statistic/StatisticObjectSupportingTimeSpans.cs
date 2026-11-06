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
     * <code>StatisticObjectSupportingTimeSpans</code> class is the super class of the
     * specific data collector classes able to handle <code>TimeSpan</code> objects.
     * If the data collector should be used to measure the lengths of 
     * <code>TimeSpan</code>s, the method <code>setShowTimeSpansInReport(true)</code> 
     * should be called to appropriately assign the (reference) unit.
     */

    public abstract class StatisticObjectSupportingTimeSpans : StatisticObject, Observer
    {

        // ****** attributes ******

        /**
         * Flag, used to know if values should be printed as TimeSpans in report.
         */
        private bool _showTimeSpansInReport = false;


        // ****** constructor ******

        /// <summary>
        /// Constructor for a StatisticObjectSupportingTimeSpans, preliminarily without a unit assigned
        /// </summary>
        /// <param name="ownerModel">The model this StatisticObject is associated to.</param>
        /// <param name="name">The name of this StatisticObject</param>
        /// <param name="showInReport">Flag for showing the report about this StatisticObject.</param>
        /// <param name="showInTrace">Flag for showing this StatisticObject in trace files.</param>
        public StatisticObjectSupportingTimeSpans(Model ownerModel, string name, bool showInReport,
                bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
        {
            //super(ownerModel, name, showInReport, showInTrace);
        }

        // ****** methods ******


        /// <summary>Updates this data collector object with a specific <c>TimeSpan</c>.</summary>
        /// <param name="t">The time span to update this data collector.</param>
        public abstract void update(TimeSpan t);

        //@Override
        /// <inheritdoc/>
        public string getUnit()
        {
            string unit = "";
            // Using time-based output, no unit explicitly assigned
            if (this.getShowTimeSpansInReport() && (getUnit() == null || getUnit().Length == 0)
                && TimeOperations.getTimeFormatter() != null)
            {
                unit = TimeOperations.getTimeFormatter().getUnit();
            }
            else
            {
                // Otherwise, use the unit explicitly set
                unit = getUnit();
            }
            return unit;
        }

        /// <summary>
        /// Sets if values should be interpreted and printed as TimeSpans (subject to the experiment's
        /// reference unit) in the Report. Unless a unit is explicitly otherwise using <c>setUnit(...)</c>,
        /// this class assumes updates refer to the reference time unit.
        /// </summary>
        /// <param name="value">true, if values should be printed as TimeSpans, false if not.</param>
        public void setShowTimeSpansInReport(bool value)
        {
            _showTimeSpansInReport = value;
        }

        /// <summary>Are values printed as TimeSpans in the report?</summary>
        /// <returns>true if values are printed as TimeSpans, false if not.</returns>
        public bool getShowTimeSpansInReport()
        {
            return _showTimeSpansInReport;
        }

        public void update(object value1, object value2)
        {
            throw new NotImplementedException();
        }
    } // end class StatisticObjectSupportingTimeSpans
}
