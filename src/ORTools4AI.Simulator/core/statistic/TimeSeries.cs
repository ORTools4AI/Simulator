// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author modified by Philip Joschko
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using System.Xml.Linq;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * The <code>TimeSeries</code> class is recording data from a
     * <code>ValueSupplier</code> object and saving it to a file, making data available
     * to a TimeSeriesPlotter or doing both. Either every
     * time the <code>ValueSupplier</code> object has changed or at every tick of
     * the SimClock. See the automatic parameter in the constructor. 
     * If writing to a file is claimed, the first line of the file contains the name of the file.
     * Then each line of the file contains a SimTime value and the value of the <code>ValueSupplier</code>
     * object at that point of time in the simulation, separated by a ";". <br>
     * One can choose when the recording of the values will start and stop. See the
     * constructor for further information. <br>
     * Reseting the <code>TimeSeries</code> object will delete the contents of the
     * file. <br>
     * This <code>TimeSeries</code> object will be the observer of the
     * <code>ValueSupplier</code> object given in the constructor. When
     * constructing a new <code>TimeSeries</code> object it will be added to the
     * list of observers of the <code>ValueSupplier</code>. That means this
     * <code>TimeSeries</code> object will be updated automatically every time the
     * observed <code>ValueSupplier</code> has changed and has called its
     * <code>notifyStatistics()</code> method.
     * 
     * As separator between different values (e.g. SimTime and value of the
     * Valuesupplier) ";" is used. See also FileOutput.
     * 
     * @see desmoj.core.report.FileOutput
     */

    public class TimeSeries : StatisticObjectSupportingTimeSpans
    {

        // ****** attributes ******

        /**
         * The name of the file where all the data will be saved to.
         * Empty if no file output is wanted.
         */
        private String _fileName;

        /**
         * Boolean indicating whether to write data to file or not.
         */
        bool hasToWriteToFile;

        /**
         * Boolean indicating whether to store data or not.
         */
        bool hasToRecordValues;

        /**
         * The graphical observer (ChartPlotter) this time series is connected to.
         * 'null' if there is no observer.
         * If there is a connected plotter, the values have to be kept in memory and
         * not only written to a file. 
         */
        Observer plotter = null;

        /**
         * List for saving the time values in the memory, needed for the GraphicalObserver.
         */
        List<double> timeValues = null;

        /**
         * List for saving the data values in the memory, needed for the GraphicalObserver.
         */
        List<double> dataValues = null;

        /**
         * The ValueSupplier which values will be recorded and saved in the file.
         */
        private ValueSupplier _valSuppl;

        /**
         * From this TimeInstant on the TimeSeries will start to save the data in the
         * file.
         */
        private TimeInstant _start;

        /**
         * From this TimeInstant on the TimeSeries will stop to save the data in the
         * file.
         */
        private TimeInstant _end;

        /**
         * The group ID used for HTML report clustering.
         */
        private string _group = "default";

        /**
         * Save the actual value of the ValueSupplier at every tick of the SimClock?
         */
        private bool _automatic;

        /**
         * Save the actual values of the ValueSupplier for all the time the
         * simulation is running? ( Is end <= start ?)
         */
        private bool _always;

        /**
         * The file where all the data will be saved to.
         */
        private FileOutput _file;

        /**
         * Flag, used to know if values should be printed as TimeSpans in report.
         */
        private bool _showTimeSpansInReport = false;

        // ****** methods ******

        /// <summary>
        /// Constructor for a TimeSeries object that has NO connection to a <c>ValueSupplier</c> but will
        /// write data to the given file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="fileName">
        /// The name of the file the values will be saved to. Must comply to the naming rules of the
        /// underlying Operating System. This name will be saved in the first line of the file.
        /// </param>
        /// <param name="start">
        /// The instant at which this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The instant at which this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        public TimeSeries(Model ownerModel, string name, string fileName,
                TimeInstant start, TimeInstant end, bool showInTrace) : this(ownerModel, name, start, end, showInTrace)
        {
            //this(ownerModel, name, start, end, showInTrace);

            this._fileName = fileName; // get hold of the file name      
            this._file = new FileOutput();
            hasToWriteToFile = true;
            hasToRecordValues = false;
            // fileName contains no proper name
            if (fileName == null)
            {
                sendWarning(
                        "Attempt to write to a file which has no name. "
                                + "The file will be named: 'unnamed_TimeSeries_File'!",
                        "TimeSeries: "
                                + this.getName()
                                + " Constructor: TimeSeries(Model "
                                + "ownerModel, String name, String fileName, ValueSupplier valSup,"
                                + " SimTime start, SimTime end, boolean automatic, boolean "
                                + "showInTrace)",
                        "A file with no name will be lost in deep space of your harddisk.",
                        "Make sure to give output files a useful name.");

                this._fileName = "unnamed_TimeSeries_File";
            }
        }

        /// <summary>
        /// Constructor for a TimeSeries object that has NO connection to a <c>ValueSupplier</c> but will
        /// write data to the given file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="fileName">
        /// The name of the file the values will be saved to. Must comply to the naming rules of the
        /// underlying Operating System. This name will be saved in the first line of the file.
        /// </param>
        /// <param name="start">
        /// The point of SimTime this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The point of SimTime this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        /// <remarks>Types of start and end (SimTime) to be replaced with TimeInstant.</remarks>
        public TimeSeries(Model ownerModel, string name, string fileName,
                SimTime start, SimTime end, bool showInTrace) : this(ownerModel, name, fileName,
                SimTime.toTimeInstant(start), SimTime.toTimeInstant(end), showInTrace)
        {
        }

        /// <summary>
        /// Constructor for a TimeSeries object that will observe a <c>ValueSupplier</c> and write data to
        /// the given file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="fileName">
        /// The name of the file the values will be saved to. Must comply to the naming rules of the
        /// underlying Operating System. This name will be saved in the first line of the file.
        /// </param>
        /// <param name="valSup">
        /// The values from this ValueSupplier will be recorded and saved in the file. The given
        /// ValueSupplier will be observed by this TimeSeries object.
        /// </param>
        /// <param name="start">
        /// The instant at which this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The instant at which this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="automatic">
        /// Shall the values be recorded automatically at every tick of the SimClock?
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        public TimeSeries(Model ownerModel, string name, string fileName,
                ValueSupplier valSup, TimeInstant start, TimeInstant end,
                bool automatic, bool showInTrace) : this(ownerModel, name, valSup, start, end, automatic, showInTrace)
        {


            this._fileName = fileName; // get hold of the file name      
            this._file = new FileOutput();
            hasToWriteToFile = true;
            hasToRecordValues = false;

            // fileName contains no proper name
            if (fileName.Equals("") || fileName == null)
            {
                sendWarning(
                        "Attempt to write to a file which has no name. "
                                + "The file will be named: 'unnamed_TimeSeries_File'!",
                        "TimeSeries: "
                                + this.getName()
                                + " Constructor: TimeSeries(Model "
                                + "ownerModel, String name, String fileName, ValueSupplier valSup,"
                                + " SimTime start, SimTime end, boolean automatic, boolean "
                                + "showInTrace)",
                        "A file with no name will be lost in deep space of your harddisk.",
                        "Make sure to give output files a useful name.");

                this._fileName = "unnamed_TimeSeries_File";
            }

        }

        /// <summary>
        /// Constructor for a TimeSeries object that will observe a <c>ValueSupplier</c> and write data to
        /// the given file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="fileName">
        /// The name of the file the values will be saved to. Must comply to the naming rules of the
        /// underlying Operating System. This name will be saved in the first line of the file.
        /// </param>
        /// <param name="valSup">
        /// The values from this ValueSupplier will be recorded and saved in the file. The given
        /// ValueSupplier will be observed by this TimeSeries object.
        /// </param>
        /// <param name="start">
        /// The point of SimTime this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The point of SimTime this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="automatic">
        /// Shall the values be recorded automatically at every tick of the SimClock?
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        /// <remarks>Types of start and end (SimTime) to be replaced with TimeInstant.</remarks>
        public TimeSeries(Model ownerModel, String name, String fileName,
                ValueSupplier valSup, SimTime start, SimTime end,
                bool automatic, bool showInTrace) : this(ownerModel, name, fileName,
                    valSup, SimTime.toTimeInstant(start), SimTime.toTimeInstant(end),
                    automatic, showInTrace)
        {

        }

        /// <summary>
        /// Constructor for a TimeSeries object that has NO connection to a <c>ValueSupplier</c> and will NOT
        /// write data into a file. If you will not connect this object to a TimeSeriesPlotter after using
        /// this constructor, it will be useless. Choose another constructor for writing data to file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="start">
        /// The instant at which this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The instant at which this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        public TimeSeries(Model ownerModel, string name, TimeInstant start,
            TimeInstant end, bool showInTrace) : base(ownerModel, name, false, showInTrace)
        {
            //super(ownerModel, name, false, showInTrace);

            this._valSuppl = null; // there is no ValueSupplier to observe
            this._start = start;
            this._end = end;
            this._automatic = false; // the values can not be recorded automatically

            this._always = TimeInstant.isBefore(end, start);

            hasToWriteToFile = false;
            hasToRecordValues = false;
        }

        /// <summary>
        /// Constructor for a TimeSeries object that has NO connection to a <c>ValueSupplier</c> and will NOT
        /// write data into a file. If you will not connect this object to a TimeSeriesPlotter after using
        /// this constructor, it will be useless. Choose another constructor for writing data to file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="start">
        /// The point of SimTime this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The point of SimTime this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        /// <remarks>Types of start and end (SimTime) to be replaced with TimeInstant.</remarks>
        public TimeSeries(Model ownerModel, String name, SimTime start,
            SimTime end, bool showInTrace) : this(ownerModel, name,
                SimTime.toTimeInstant(start), SimTime.toTimeInstant(end), showInTrace)
        {
            //this(ownerModel, name, SimTime.toTimeInstant(start), SimTime.toTimeInstant(end), showInTrace);    
        }

        /// <summary>
        /// Constructor for a TimeSeries object that will observe a <c>ValueSupplier</c> but will NOT write
        /// data into a file. If you will not connect this object to a TimeSeriesPlotter after using this
        /// constructor, it will be useless! Choose another constructor for writing data to file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="valSup">
        /// The values from this ValueSupplier will be recorded and saved in the file. The given
        /// ValueSupplier will be observed by this TimeSeries object.
        /// </param>
        /// <param name="start">
        /// The point of SimTime this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The point of SimTime this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="automatic">
        /// Shall the values be recorded automatically at every tick of the SimClock?
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        public TimeSeries(Model ownerModel, String name,
                ValueSupplier valSup, TimeInstant start, TimeInstant end,
                bool automatic, bool showInTrace) : base(ownerModel, name, false, showInTrace)
        {


            // valSup is no valid ValueSupplier
            if (valSup == null)
            {
                sendWarning(
                        "Attempt to produce a TimeSeries about a non existing "
                                + "ValueSupplier. The command will be ignored!",
                        "TimeSeries: "
                                + this.getName()
                                + " Constructor: TimeSeries(Model "
                                + "ownerModel, String name, String fileName, ValueSupplier valSup,"
                                + " SimTime start, SimTime end, boolean automatic, boolean "
                                + "showInTrace)",
                        "The given ValueSupplier: valSup is only a null pointer.",
                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                + "TimeSeries object.");

                return; // just return
            }

            this._valSuppl = valSup;
            this._start = start;
            this._end = end;
            this._automatic = automatic;

            this._always = TimeInstant.isBefore(end, start);

            hasToWriteToFile = false;
            hasToRecordValues = false;

            if (automatic) // update at every tick of the SimClock?
            {
                Observable simClock = this.getModel().getExperiment().getSimClock();

                simClock.addObserver(this); // observe the SimClock
            }
            else
            {
                _valSuppl.addObserver(this); // observe the valSuppl
            }
        }

        /// <summary>
        /// Constructor for a TimeSeries object that will observe a <c>ValueSupplier</c> but will NOT write
        /// data into a file. If you will not connect this object to a TimeSeriesPlotter after using this
        /// constructor, it will be useless! Choose another constructor for writing data to file.
        /// </summary>
        /// <param name="ownerModel">The model this TimeSeries object is associated to</param>
        /// <param name="name">The name of this TimeSeries object</param>
        /// <param name="valSup">
        /// The values from this ValueSupplier will be recorded and saved in the file. The given
        /// ValueSupplier will be observed by this TimeSeries object.
        /// </param>
        /// <param name="start">
        /// The point of SimTime this TimeSeries will start to save the values to the file.
        /// </param>
        /// <param name="end">
        /// The point of SimTime this TimeSeries will stop to save the values to the file. Choose an end time
        /// that lies before the start time to record the values for all the time.
        /// </param>
        /// <param name="automatic">
        /// Shall the values be recorded automatically at every tick of the SimClock?
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this TimeSeries in trace files. Set it to <c>true</c> if TimeSeries should show
        /// up in trace. Set it to <c>false</c> if TimeSeries should not be shown in trace.
        /// </param>
        /// <remarks>Types of start and end (SimTime) to be replaced with TimeInstant.</remarks>
        public TimeSeries(Model ownerModel, String name,
                ValueSupplier valSup, SimTime start, SimTime end,
                bool automatic, bool showInTrace) : this(ownerModel, name,
                    valSup, SimTime.toTimeInstant(start), SimTime.toTimeInstant(end),
                    automatic, showInTrace)
        {

        }

        /// <summary>Gets the start instant of this TimeSeries.</summary>
        /// <returns>The start instant of this TimeSeries.</returns>
        public TimeInstant get_start() => _start;

        /// <summary>Gets the end instant of this TimeSeries.</summary>
        /// <returns>The end instant of this TimeSeries.</returns>
        public TimeInstant get_end() => _end;

        /// <summary>Gets the group ID used for HTML report clustering.</summary>
        /// <returns>The group ID used for HTML report clustering.</returns>
        public string getGroup() => _group;

        /// <summary>Sets the group ID used for HTML report clustering.</summary>
        /// <param name="groupID">The group ID used for HTML report clustering.</param>
        public void setGroup(string groupID) { _group = groupID; }

        /// <summary>
        /// Returns a null pointer and no Reporter! All the data (values) will be saved in a file or
        /// displayed in the GraphicalObserver, so no extra report is needed. Overrides the method:
        /// createReporter() from Reportable.
        /// </summary>
        /// <returns><c>null</c> will be returned</returns>
        protected override Reporter createDefaultReporter()
        {
            return null;
        }

        /// <summary>
        /// Returns the actual value of the <c>ValueSupplier</c> which values are recorded from this
        /// TimeSeries object.
        /// </summary>
        /// <returns>The actual value of the observed <c>ValueSupplier</c> object.</returns>
        public double getValue()
        {
            // there is no valid ValueSupplier
            if (_valSuppl == null)
            {
                sendWarning("Attempt to get a value for a TimeSeries from a non "
                        + "existing ValueSupplier. UNDEFINED will be returned!",
                        "TimeSeries: " + this.getName() + " Method: getValue() ",
                        "The given ValueSupplier: valSuppl is only a null pointer. So no "
                                + "value can be fetched from it.",
                        "Make sure to pass a valid ValueSupplier when constructing a new "
                                + "TimeSeries object.");

                return UNDEFINED; // just return
            }

            // get the value from the ValueSupplier
            double value = _valSuppl.value();

            // return the rounded value
            return round(value);
        }

        /**
         * Resets this TimeSeries object by deleting the contents of the file to
         * which all the data is saved to. The name of the file will remain the
         * same. This is done by closing and opening the existing file.
         */
        public void reset()
        {
            base.reset(); // reset the Reportable, too.

            if (hasToWriteToFile)
            {
                if (_file.isOpen()) _file.close();
                // register the FileOutput object at the experiment, so it will be
                // closed properly when the experiment is over
                this.getModel().getExperiment().registerFileOutput(_file);
                _file.open(_fileName); // this will overwrite the already
                                       // existing file
                                       // with the file name: fileName.
                                       // write the fileName in the first line of the file
                _file.writeln(_fileName);
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
        /// Updates this <c>TimeSeries</c> object by fetching the actual value of the <c>ValueSupplier</c>
        /// and processing it. The <c>ValueSupplier</c> is passed in the constructor of this
        /// <c>TimeSeries</c> object. This <c>update()</c> method complies with the one described in DESMO,
        /// see [Page91].
        /// </summary>
        public void update()
        {
            if (hasToWriteToFile)
            {
                if (!_file.isOpen()) // the file is opened for the first time
                {
                    // register the FileOutput object at the experiment, so it will be
                    // closed properly when the experiment is over
                    this.getModel().getExperiment().registerFileOutput(_file);

                    // open the file to write data to it and give it a name
                    _file.open(_fileName);

                    // write the fileName in the first line of the file
                    _file.writeln(_fileName);
                }
            }

            TimeInstant actualTime = presentTime(); // get hold of the current time

            if (!_always) // if not always
            { // check boundaries of interval (start - end)

                // start of interval is not reached yet
                if (TimeInstant.isBefore(actualTime, _start))
                {
                    return;
                } // do nothing, just return

                // already beyond end of interval
                if (TimeInstant.isBefore(_end, actualTime))
                {
                    if (_automatic) // is the SimClock observed?
                    {
                        Observable simClock = this.getModel().getExperiment()
                                .getSimClock();

                        simClock.deleteObserver(this); // stop observing the
                                                       // SimClock
                    }

                    return; // do nothing, just return

                } // end beyond end of interval

            } // end always

            // there is no valid ValueSupplier
            if (_valSuppl == null)
            {
                sendWarning(
                        "Attempt to update a TimeSeries with a non existing "
                                + "ValueSupplier. Which value should be written to the file? "
                                + "The command will be ignored!",
                        "TimeSeries: " + this.getName() + " Method: update() ",
                        "The given ValueSupplier: valSuppl is only a null pointer.",
                        "Make sure to update the TimeSeries only if there is a valid "
                                + "ValueSupplier. Or use the other update(double val) method.");

                return; // just return
            }

            // get the value from the ValueSupplier
            double actualValue = getValue();

            // get double representation of current time (rounding!)
            double actTime = actualTime.getTimeAsDouble();

            if (hasToWriteToFile)
            {
                // make the string which will be saved in the file
                String record = actTime + FileOutput.getSeparator() + actualValue;
                _file.writeln(record); // write the String: record to the file
            }

            if (hasToRecordValues)
            {
                if (timeValues == null)
                    timeValues = new List<Double>();
                if (dataValues == null)
                    dataValues = new List<Double>();
                dataValues.Add(actualValue);
                timeValues.Add(actTime);
                if (plotter != null) plotter.update(null, null);
            }

            incrementObservations(); // increment the observations (see Reportable)
            traceUpdate(); // leave a message in the trace
        }

        /// <summary>
        /// Updates this <c>TimeSeries</c> object with the double value given as parameter. In some cases it
        /// might be more convenient to pass the value this <c>ValueStatistics</c> will be updated with
        /// directly within the <c>update(double val)</c> method instead of going via the
        /// <c>ValueSupplier</c>.
        /// </summary>
        /// <param name="val">The value with which this <c>TimeSeries</c> will be updated.</param>
        public void update(double val)
        {
            if (hasToWriteToFile)
            {
                if (!_file.isOpen()) // the file is opened for the first time
                {
                    // register the FileOutput object at the experiment, so it will be
                    // closed properly when the experiment is over
                    this.getModel().getExperiment().registerFileOutput(_file);

                    // open the file to write data to it and give it a name
                    _file.open(_fileName);

                    // write the fileName in the first line of the file
                    _file.writeln(_fileName);
                }
            }

            TimeInstant actualTime = presentTime(); // get hold of the current time

            if (!_always) // if not always
            { // check boundaries of interval (start - end)

                // start of interval is not reached yet
                if (TimeInstant.isBefore(actualTime, _start))
                {
                    return;
                } // do nothing, just return

                // already beyond end of interval
                if (TimeInstant.isBefore(_end, actualTime))
                {
                    if (_automatic) // is the SimClock observed?
                    {
                        Observable simClock = this.getModel().getExperiment()
                                .getSimClock();

                        simClock.deleteObserver(this); // stop observing the
                                                       // SimClock
                    }

                    return; // do nothing, just return

                } // end beyond end of interval

            } // end always

            // get double representation of current time (rounding!)
            double actTime = round(actualTime.getTimeAsDouble());

            if (hasToWriteToFile)
            {
                // make the string which will be saved in the file
                String record = actTime + FileOutput.getSeparator() + val;
                _file.writeln(record); // write the String: record to the file
            }

            if (hasToRecordValues)
            {
                if (timeValues == null)
                    timeValues = new List<double>();
                if (dataValues == null)
                    dataValues = new List<double>();
                dataValues.Add(val);
                timeValues.Add(actTime);
                if (plotter != null) plotter.update(null, null);
            }

            incrementObservations(); // increment the observations (see Reportable)
            traceUpdate(); // leave a message in the trace
        }

        /// <summary>
        /// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
        /// interface. This method will be called automatically from an <c>Observable</c> object within its
        /// <c>notifyObservers()</c> method.
        /// </summary>
        /// <param name="o">
        /// The Observable calling this method within its own <c>notifyObservers()</c> method.
        /// </param>
        /// <param name="arg">The Object with which this <c>TimeSeries</c> is updated.</param>
        public void update(Observable o, Object arg)
        {
            if (o == null) // null was passed instead of an Observable
            {
                sendWarning(
                        "Attempt to update a TimeSeries with no reference to an "
                                + "Observable. The value of '" + _valSuppl.getName()
                                + "' will be "
                                + "recorded and saved in the file anyway.",
                        "TimeSeries: " + this.getName()
                                + " Method: update (Observable o, " + "Object arg)",
                        "The passed Observable: o in this method is only a null pointer.",
                        "The update()-method was not called via notifyObservers() from an "
                                + "Observable. Who was calling it? Why don't you let the Observable do"
                                + " the work?");
            }
            if (hasToWriteToFile)
            {
                if (!_file.isOpen()) // the file is opened for the first time
                {
                    // register the FileOutput object at the experiment, so it will be
                    // closed properly when the experiment is over
                    this.getModel().getExperiment().registerFileOutput(_file);

                    // open the file to write data to it and give it a name
                    _file.open(_fileName);

                    // write the fileName in the first line of the file
                    _file.writeln(_fileName);
                }
            }

            // get hold of the current time
            TimeInstant actualTime = presentTime(); // get hold of the current time

            if (!_always) // if not always
            { // check boundaries of interval (start - end)

                // start of interval is not reached yet
                if (TimeInstant.isBefore(actualTime, _start))
                {
                    return;
                } // do nothing, just return

                // already beyond end of interval
                if (TimeInstant.isBefore(_end, actualTime))
                {
                    if (_automatic) // is the SimClock observed?
                    {
                        Observable simClock = this.getModel().getExperiment()
                                .getSimClock();

                        simClock.deleteObserver(this); // stop observing the
                                                       // SimClock
                    }

                    return; // do nothing, just return

                } // end beyond end of interval

            } // end always

            double actualValue = -1.0; // get hold of the actual value of the
                                       // ValueSupplier. undefined so far

            // notified by a ValueSupplier
            if (arg == null) // update was called with no arg Object
            {
                actualValue = getValue(); // get the value from the ValueSupplier
            }
            else
            {
                // update was called from a ValueSupplier
                if (arg is double)
                {
                    // convert the object given as arg to a double value
                    double actVal = convertToDouble(arg);

                    // round the value
                    actualValue = round(actVal);
                }
                else
                {
                    // notified by the SimClock
                    if (arg is TimeInstant) // time instant passed
                    {
                        // get the value from the ValueSupplier
                        actualValue = getValue();

                        // get the time provided by an instant
                        actualTime = (TimeInstant)arg;

                    }
                    else if (arg is SimTime) // the SimClock was ticking
                    {
                        // get the value from the ValueSupplier
                        actualValue = getValue();

                        // get the time provided by the SimClock
                        actualTime = SimTime.toTimeInstant((SimTime)arg);

                    }
                    else
                    {
                        sendWarning(
                                "Attempt to update a TimeSeries with an argument arg, "
                                        + "that can not be recognized. The attempted action is ignored!",
                                "TimeSeries: " + this.getName()
                                        + " Method: update (Observable o, "
                                        + "Object arg)",
                                "The passed Object in the argument arg could not be recognized.",
                                "Make sure to pass null, an TimeInstant, a SimTime or a Number object as the arg "
                                        + "argument.");

                        return; // do nothing, just return
                    }
                }
            }

            // round the time reasonably
            double actTime = round(actualTime.getTimeAsDouble());

            if (hasToWriteToFile)
            {
                // make the string which will be saved in the file
                String record = actTime + FileOutput.getSeparator() + actualValue;
                _file.writeln(record); // write the String: record to the file
            }

            if (hasToRecordValues)
            {
                if (timeValues == null)
                    timeValues = new List<double>();
                if (dataValues == null)
                    dataValues = new List<double>();
                dataValues.Add(actualValue);
                timeValues.Add(actTime);
                if (plotter != null) plotter.update(null, null);
            }

            incrementObservations(); // increment the observations (see Reportable)
            traceUpdate(); // leave a message in the trace
        }



        /// <summary>
        /// Connects this statistic object with its GraphicalObserver. Advises this object to keep the values
        /// in the memory, because the TimeSeriesPlotter requires this.
        /// </summary>
        /// <param name="plotter">The TimeSeriesPlotter which will display this TimeSeries object.</param>
        public void connectToPlotter(Observer plotter)
        {
            this.plotter = plotter;
            if (plotter != null)
                startRecordingValues();
        }

        /**
         * Lets this TimeSeries record all values to memory, even if
         * no TimeSeriesPlotter is present. Useful for connecting the plotter
         * later in an experiment evaluation environment, e.g. after experiment conduction. 
         */
        public void startRecordingValues()
        {
            this.hasToRecordValues = true;
        }

        /// <summary>Gets the list of data values.</summary>
        /// <returns>The list with all the data values.</returns>
        public List<Double> getDataValues()
        {
            return dataValues;
        }

        /// <summary>Gets the list of time values.</summary>
        /// <returns>The list of all the time values.</returns>
        public List<Double> getTimeValues()
        {
            return timeValues;
        }

    } // end class TimeSeries
}