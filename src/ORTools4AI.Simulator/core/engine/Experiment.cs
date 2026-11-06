// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Soenke Claassen, Ruth Meyer, Nicolas Knaak, Gunnar
// @author Xiufeng Li
// @author Soenke Claassen
// @author Tim Janz
// @author Ruth Meyer
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;


namespace ORTools4AI.Simulator.core.engine
{


    /**
     * Experiment is the class that provides the infrastructure for running the
     * simulation of a model. It contains all data structures necessary to simulate
     * the model and takes care of all necessary output. To actually run an
     * experiment, a new instance of the experiment class and a new instance of the
     * desired model have to be created. To link both instances, call the
     * <code>connectToExperiment(Experiment e)</code> method of the model instance
     * and pass the new experiment as a parameter.
     * 
     *         Kiesel,Felix Klueckmann
     * 
     *
     * @modifier new Example Variable _traceOutput, _reportOutput for Outputclasscollection
     * 			 new class constructors 
     * 			 Experiment(String , String , ArrayList<String> , ArrayList<String> ,ArrayList<String> , ArrayList<String> ) and
     * 			 Experiment(String , String , TimeUnit ,TimeUnit , TimeFormatter ,ArrayList<String> , 
     * 					ArrayList<String> ,ArrayList<String> ,ArrayList<String> )
     */
    public class Experiment : NamedObject
    {

        /**
    	 * The experiment's name catalog for ensuring uniqueness of simulation
    	 * object names within a single experiment.
    	 */
        private NameCatalog _nameCatalog = new NameCatalog();

        /** 
         * The default report output 
         */
        public static readonly string DEFAULT_REPORT_OUTPUT_TYPE = "ORTools4AI.Simulator.core.report.HTMLReportOutput";

    	/** 
    	 * The default trace output 
    	 */
    	public static readonly string DEFAULT_TRACE_OUTPUT_TYPE = "ORTools4AI.Simulator.core.report.HTMLTraceOutput";

    	/** 
    	 * The default error output 
    	 */
    	public static readonly string DEFAULT_ERROR_OUTPUT_TYPE = "ORTools4AI.Simulator.core.report.HTMLErrorOutput";

    	/** 
    	 * The default debug output 
    	 */
    	public static readonly string DEFAULT_DEBUG_OUTPUT_TYPE = "ORTools4AI.Simulator.core.report.HTMLDebugOutput";

    	/**
    	 * Status of an Experiment just created without any accessories created yet.
    	 */
    	public static readonly int NOT_INITIALIZED = -3;

        /**
     * Status of an Experiment instantiated with all needed accessories
     * available.
     */
        public static readonly int INITIALIZED = -2;

        /**
         * Status of an Experiment connected to a Model and ready to be started.
         */
        public static readonly int CONNECTED = -1;

        /**
         * Status of an Experiment being started. Only if an Experiment is
         */
        public static readonly int STARTED = 0;

        /**
         * Status of an Experiment stopped after having run.
         */
        public static readonly int STOPPED = 1;

        /**
         * Status of an Experiment currently running the simulation.
         */
        public static readonly int RUNNING = 2;

        /**
         * Status of an Experiment finished and to be cleared.
         */
        public static readonly int ABORTED = 3;

        /**
         * The last suffix used with filenames when creating multiple batch runs of
         * an experiment.
         */
        private static int lastSuffix;

        /**
         * The class reference to messages of type desmoj.core.report.TraceNote
         */
        public static Type tracenote = typeof(TraceNote);

        ///**
        // * The class reference to messages of type desmoj.core.report.DebugNote
        // */
        public static Type debugnote = typeof(DebugNote);

        ///**
        // * The class reference to messages of type desmoj.core.report.ErrorMessage
        // */
        public static Type errormessage = typeof(ErrorMessage);

        ///**
        // * The class reference to messages of type desmoj.core.report.Reporter
        // */
        public static Type reporter = typeof(Reporter);

        /**
         * Flag indicating at least one error or warning has occurred.
         */
        private bool _error;

        /**
         * Flag indicating to suppress notifications like 'experiment started'.
         */
        private bool _silent;

        /**
         * The description of this Experiment
         */
        private string _description = null;

        /**
         * Flag indicating if the simulation is running.
         */
        private int _status;

        /**
         * The model to be run by this experiment.
         */
        private Model _client;

        /**
         * The scheduler used for this experiment.
         */
        protected Scheduler clientScheduler;

        /**
         * The distribution manager for the model's distributions.
         */
        private DistributionManager _distMan;

        /**
         * The message manager for the model's messages.
         */
        private MessageDistributor _messMan;

        /**
         * The list to register all OutputType objects to close them after finishing
         * the Experiment.
         */
        private List<OutputType> _registryOutputType;

        /**
         * The list to register all FileOutput objects to close them after finishing
         * the Experiment.
         */
        private List<FileOutput> _registryFileOutput;

        private EventListLogger? _eventListLogger;

        /**
         * The resource database storing all resource allocations and requests. Also
         * needed to detect deadlocks.
         */
        private ResourceDB _resDB;

        /**
         * The TimeInstant when the experiment is supposed to stop. Is initially
         * <code>null</code> and will be set only if the user provides a time limit.
         */
        private TimeInstant _stopTime = null;

        /**
          * The event to stop the experiment. Is initially <code>null</code> and will 
          * be set only if the user provides a time limit.
          */
        private ExternalEventStop _stopTimeEvent = null;

        /**
         * A list of <code>Condition</code>s which cause the experiment to stop. The
         * user has to implement the <code>check()</code> method of the 
         * <code>Condition</code>s in order to effectively stop an experiment.
         */
        private List<ModelCondition> _stopConditions;

        /**
         * Flag indicating whether a progress bar for this experiment should be
         * displayed or not.
         */
        private bool _showProgressBar;

        /**
         * Flag indicating whether the progress bar (if shown) will be autoclosed
         * once the experiment terminates or not.
         */
        private bool _showProgressBarAutoclose;

        /**
         * Specifies an output path for the report files (Modification by Nicolas
         * Knaak, 02/2001)
         */
        private string _pathName;

        /**
         * Delay between steps of the scheduler in milliseconds. Necessary for
         * online observation of experiments in FAMOS. (Modification by Nicolas
         * Knaak, 07/2001)
         */
        private long _delayInMillis = 0;

        /**
         * The real time (wallclock time) start time of the simulation run.
         * (Modification by Felix Klueckmann, 05/2009)
         */
        private long _realTimeStartTime;

        /**
         * The output types of the debug channel.
         */
        private List<OutputType> _debugOutput;

        /**
         * The output types of the report channel.
         */
        private List<OutputType> _reportOutput;

        /**
         * The output types of the error channel.
         */
        private List<OutputType> _errorOutput;

        /**
         * The output types of the trance channel.
         */
        private List<OutputType> _traceOutput;

        /**
         * A potential interruption of experiment executions (otherwise <code>null</code>).
         */
        private DESMOJException _interruptingException;

        /// <summary>
        /// Constructs a new Experiment with a given name. Data channel output (report, error, debug, trace)
        /// will either be written to HTML files in the current directory. Epsilon (granularity of
        /// simulation) defaults to a microsecond, reference time (default time unit) to a second.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        public Experiment(string name) : this(name, true)
        {

        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. Experiment name can be specified. Data
        /// channel output (report, error, debug, trace) will either be suppressed or written to HTML files
        /// in the current directory. Epsilon (granularity of simulation) defaults to a microsecond,
        /// reference time (default time unit) to a second.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="output">
        /// This flag indicates if the experiment should write output files in the default format (HTML) or
        /// no output files at all.
        /// </param>
        public Experiment(string name, bool output) : this(name, ".", TimeUnit.MICROSECONDS, TimeUnit.SECONDS, null,
                    output ? DEFAULT_REPORT_OUTPUT_TYPE : null,
                    output ? DEFAULT_TRACE_OUTPUT_TYPE : null,
                    output ? DEFAULT_ERROR_OUTPUT_TYPE : null,
                    output ? DEFAULT_DEBUG_OUTPUT_TYPE : null)
        {
            // call most special
            // constructor
        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. Experiment name and output path can be
        /// specified. Data channel output (report, error, debug, trace) will either be written to HTML
        /// files, epsilon (granularity of simulation) defaults to a microsecond, reference time (default
        /// time unit) to a second.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="pathName">The output path for report files</param>
        public Experiment(string name, string pathName) : this(name, pathName, DEFAULT_REPORT_OUTPUT_TYPE,
                    DEFAULT_TRACE_OUTPUT_TYPE, DEFAULT_ERROR_OUTPUT_TYPE,
                    DEFAULT_DEBUG_OUTPUT_TYPE)
        {


            // call more special constructor

        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. This is a shortcut constructor. Parameters
        /// for the name, the granularity(epsilon), the reference time unit and a time formatter are needed.
        /// All other possible settings are set to default values. These settings for an experiment without
        /// special ExperimentOptions are: seed = 979 : The initial seed setting for the seed-generator The
        /// default stop condition for this experiment will never interfere, always returning false.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="epsilon">The granularity of simulation time.</param>
        /// <param name="referenceUnit">
        /// In statements without an explicit declaration of a TimeUnit the reference unit is used.
        /// </param>
        /// <param name="formatter">Defines how time values will be formatted in the output files.</param>
        public Experiment(string name, TimeUnit epsilon, TimeUnit referenceUnit,
                TimeFormatter formatter) : this(name, ".", epsilon, referenceUnit, formatter,
                    DEFAULT_REPORT_OUTPUT_TYPE, DEFAULT_TRACE_OUTPUT_TYPE,
                    DEFAULT_ERROR_OUTPUT_TYPE, DEFAULT_DEBUG_OUTPUT_TYPE)
        {
            // call most special constructor


        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. Experiment name, output path and a single
        /// file type per output channel can be specified. Epsilon (granularity of simulation) defaults to a
        /// microsecond, reference time (default time unit) to a second.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="pathName">The output path for report files</param>
        /// <remarks>See also Units</remarks>
        public Experiment(string name, string pathName, string reportOutputType,
                string traceOutputType, string errorOutputType,
                string debugOutputType) : this(name, pathName, TimeUnit.MICROSECONDS, TimeUnit.SECONDS, null,
                    reportOutputType, traceOutputType, errorOutputType,
                    debugOutputType)
        {

        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. Experiment name, output path, epsilon,
        /// reference time unit, time format, can be specified. Same holds for file output channels, though
        /// this constructor assumes a single file type per output channel.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="pathName">The output path for report files</param>
        /// <param name="epsilon">The granularity of simulation time.</param>
        /// <param name="referenceUnit">
        /// In statements without an explicit declaration of a TimeUnit the reference unit is used.
        /// </param>
        /// <param name="formatter">Defines how time values will be formatted in the output files.</param>
        public Experiment(string name, string pathName, TimeUnit epsilon,
                TimeUnit referenceUnit, TimeFormatter formatter,
                string reportOutputType, string traceOutputType,
                string errorOutputType, string debugOutputType) : base(name)
        {
            //super(name); // create a NamedObject with an attitude ;-)

            var reportOutputs = new List<string>(); if (reportOutputType != null) reportOutputs.Add(reportOutputType);
            var traceOutputs = new List<string>(); if (traceOutputType != null) traceOutputs.Add(traceOutputType);
            var errorOutputs = new List<string>(); if (errorOutputType != null) errorOutputs.Add(errorOutputType);
            var debugOutputs = new List<string>(); if (debugOutputType != null) debugOutputs.Add(debugOutputType);

            setupExperiment(name, pathName, epsilon, referenceUnit, formatter, reportOutputs, traceOutputs, errorOutputs, debugOutputs);
        }

        /// <summary>
        /// Constructs a new Experiment with the given parameters. This is the most flexible constructor.
        /// Experiment name, output path, epsilon, reference time unit, time format, and multiple file types
        /// per output channel can be specified.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="outputPath">The output path for report files</param>
        /// <param name="epsilon">The granularity of simulation time.</param>
        /// <param name="referenceUnit">
        /// In statements without an explicit declaration of a TimeUnit the reference unit is used.
        /// </param>
        /// <param name="formatter">Defines how time values will be formatted in the output files.</param>
        public Experiment(string name, string outputPath, TimeUnit epsilon,
                TimeUnit referenceUnit, TimeFormatter formatter,
                List<string> reportOutputs, List<string> traceOutputs,
                List<string> errorOutputs, List<string> debugOutputs) : base(name)
        {
            //super(name);
            setupExperiment(name, outputPath, epsilon, referenceUnit, formatter, reportOutputs, traceOutputs, errorOutputs, debugOutputs);
        }
        /// <summary>
        /// Constructs a new Experiment with the given parameters. Experiment name, output path and a
        /// multiple file type per output channel can be specified. Epsilon (granularity of simulation)
        /// defaults to a microsecond, reference time (default time unit) to a second.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="outputPath">The output path for report files</param>
        /// <remarks>See also Units</remarks>
        public Experiment(string name, string outputPath,
                List<string> reportOutputs, List<string> traceOutputs,
                List<string> errorOutputs, List<string> debugOutputs) :
            this(name, outputPath, TimeUnit.MICROSECONDS, TimeUnit.SECONDS, null,
                    reportOutputs, traceOutputs, errorOutputs,
                    debugOutputs)
        {
        }


        /// <summary>
        /// Private helper method to initialize the experiment; should be called from all constructors.
        /// </summary>
        /// <param name="name">
        /// The name of the experiment determining the outputfile's names, too. So please avoid characters
        /// that your local filesystem does not support in filenames.
        /// </param>
        /// <param name="outputPath">The output path for report files</param>
        /// <param name="epsilon">The granularity of simulation time.</param>
        /// <param name="referenceUnit">
        /// In statements without an explicit declaration of a TimeUnit the reference unit is used.
        /// </param>
        /// <param name="formatter">Defines how time values will be formatted in the output files.</param>
        private void setupExperiment(string name, string outputPath, TimeUnit epsilon,
                TimeUnit referenceUnit, TimeFormatter formatter,
                List<string> reportOutputs, List<string> traceOutputs,
                List<string> errorOutputs, List<string> debugOutputs)
        {
            // initialize variables
            _traceOutput = new List<OutputType>();
            _debugOutput = new List<OutputType>();
            _errorOutput = new List<OutputType>();
            _reportOutput = new List<OutputType>();
            _status = NOT_INITIALIZED;
            _stopConditions = new List<ModelCondition>(); // empty, i.e. no Stopper
                                                          // can be set at
                                                          // instantiation time
            _registryFileOutput = new List<FileOutput>();
            _registryOutputType = new List<OutputType>();
            lastSuffix = 0; // no batches have run so far ;-)
            _showProgressBar = true; // display a progress bar for this experiment
            _showProgressBarAutoclose = false; // do not autoclose progress bar for this experiment
            _error = false; // no error or warning so far ;-)
            _silent = false; // notify the user about what is going on
            _interruptingException = null; // no interruption yet

            // Check and set output path
            //      if (pathName == null
            //              || (pathName != null && (pathName.isEmpty() || pathName
            //                      .equals("."))))
            //          this.pathName = System.getProperty("user.dir", ".");
            //      else
            this._pathName = outputPath;

            // set class variables for basic messagetypes
            try
            {
                tracenote = typeof(TraceNote);
                debugnote = typeof(DebugNote);
                errormessage = typeof(ErrorMessage);
                reporter = typeof(Reporter);
            }
            catch (Exception cnfEx)
            {
                Console.WriteLine("Can not create Experiment!");
                Console.WriteLine("Constructor of desmoj.core.Experiment.");
                Console.WriteLine("Classes are probably not installed correctly.");
                Console.WriteLine("Check your CLASSPATH setting.");
                Console.WriteLine("Exception caught : " + cnfEx);
            }

            // create output system first
            _messMan = new MessageDistributor();

            // create and register the debug output
            foreach (var debugOutputType in debugOutputs)
            {
                try
                {
                    //Class<OutputType> debugOType = (Class<OutputType>)Class
                    //        .forName((debugOutputType != null) ? debugOutputType
                    //                : DEFAULT_DEBUG_OUTPUT_TYPE);

                    Type t = Type.GetType(debugOutputType);
                    var dbg = Activator.CreateInstance(t) as OutputType;

                    _debugOutput.Add(dbg);
                    if (debugOutputType != null)
                        dbg.open(_pathName, name);
                    _messMan.register(dbg, debugnote);
                    _messMan.switchOff(debugnote);
                    register(dbg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            // create and register the report output
            foreach (string reportOutputType in reportOutputs)
            {
                try
                {
                    //Class<OutputType> reportOType = (Class<OutputType>)Class
                    //        .forName((reportOutputType != null) ? reportOutputType
                    //                : DEFAULT_REPORT_OUTPUT_TYPE);
                    Type t = Type.GetType(reportOutputType);
                    OutputType rpt = Activator.CreateInstance(t) as OutputType;
                    _reportOutput.Add(rpt);
                    if (reportOutputType != null)
                        rpt.open(_pathName, name);
                    _messMan.register(rpt, reporter);
                    register(rpt);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            // create and register the error output
            foreach (string errorOutputType in errorOutputs)
            {
                try
                {
                    //Class<OutputType> errorOType = (Class<OutputType>)Class
                    //        .forName((errorOutputType != null) ? errorOutputType
                    //                : DEFAULT_ERROR_OUTPUT_TYPE);
                    Type t = Type.GetType(errorOutputType);
                    OutputType err = Activator.CreateInstance(t) as OutputType;
                    _errorOutput.Add(err);
                    // err.setTimeFloats(timeFloats);
                    if (errorOutputType != null)
                        err.open(_pathName, name);
                    _messMan.register(err, errormessage);
                    register(err);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            // create and register the trace output
            foreach (string traceOutputType in traceOutputs)
            {
                try
                {

                    //Class<OutputType> traceOType = (Class<OutputType>)Class
                    //        .forName((traceOutputType != null) ? traceOutputType
                    //                : DEFAULT_TRACE_OUTPUT_TYPE);
                    Type t = Type.GetType(traceOutputType);
                    OutputType trc = Activator.CreateInstance(t) as OutputType;
                    _traceOutput.Add(trc);
                    if (traceOutputType != null)
                        trc.open(_pathName, name);
                    _messMan.register(trc, tracenote);
                    _messMan.switchOff(tracenote);
                    register(trc);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            // create the distributionmanager to register distributions at
            _distMan = new DistributionManager(name, 979);

            // now create the simulation runtime accessories
            _client = null; // no object connected

            // check for null reference
            if (epsilon == null)
            {
                // set to default unit
                epsilon = TimeOperations.getEpsilon();
            }
            if (referenceUnit == null)
            {
                // set to default unit
                referenceUnit = TimeOperations.getReferenceUnit();
            }

            // swap epsilon and reference unit if the reference unit has a
            // finer granularity than epsilon
            if (referenceUnit.CompareTo(epsilon) < 0)
            {
                TimeUnit buffer = referenceUnit;
                referenceUnit = epsilon;
                epsilon = buffer;
            }
            // set epsilon and referenceUnit
            TimeOperations.setEpsilon(epsilon);
            TimeOperations.setReferenceUnit(referenceUnit);

            // set time formatter (use default if null passed)
            if (formatter == null)
                TimeOperations.setTimeFormatter(TimeOperations.getDefaultTimeFormatter(), false);
            else
                TimeOperations.setTimeFormatter(formatter, true);

            // building the scheduler: prepare event list...
            // (for efficiency reasons, we use the TreeList-based implementation)
            EventList eventList = new EventHeapList();

            // create the scheduler (and clock)
            clientScheduler = createScheduler(name, eventList);

            // create a resource database and tell it that it belongs to this
            // experiment
            _resDB = new ResourceDB(this);

            // set status to first valid value - initialized, but not connected
            _status = INITIALIZED;
        }

        /// <summary>Creates a scheduler for this experiment.</summary>
        /// <param name="name">experiment name</param>
        /// <returns>a new scheduler</returns>
        protected Scheduler createScheduler(string name, EventList evl)
        {
            Scheduler s = new Scheduler(this, name, evl);
            return s;
        }

        /// <summary>
        /// Adds a messagereceiver for debugnotes to the experiment. Whenever a model produces a message of
        /// that type, it will also be sent to the given messagereceiver for further processing. Note that
        /// the given receiver must be capable of handling debugnotes.
        /// </summary>
        /// <param name="trcRec">The new messagereceiver for the given type of messages</param>
        public void addDebugReceiver(MessageReceiver trcRec)
        {

            if (trcRec == null)
            {
                sendWarning("Can not add receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', method 'void addDebugReceiver("
                                + "MessageReceiver trcRec)'",
                        "The parameter 'trc' passed was a null reference.",
                        "Make sure to construct a valid MessageReciever before adding it to "
                                + "the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.register(trcRec, debugnote);

        }

        /// <summary>
        /// Adds a messagereceiver for error messages to the experiment. Whenever a model produces a message
        /// of that type, it will also be sent to the given messagereceiver for further processing. Note that
        /// the given receiver must be capable of handling messagereceiver.
        /// </summary>
        /// <param name="trcRec">The new messagereceiver for the given type of messages</param>
        public void addErrorReceiver(MessageReceiver trcRec)
        {

            if (trcRec == null)
            {
                sendWarning("Can not add receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', method 'void addErrorReceiver("
                                + "MessageReceiver trcRec)'",
                        "The parameter 'trc' passed was a null reference.",
                        "Make sure to construct a valid MessageReciever before adding it to "
                                + "the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.register(trcRec, errormessage);

        }

        /**
         * Returns the experiments name catalog for ensuring unique names of
         * simulation objects within a single experiment.
         */
        public NameCatalog getNameCatalog()
        {
            return _nameCatalog;
        }

        /// <summary>
        /// Adds a messagereceiver for the given subtype of message to the experiment. Whenever a model
        /// produces a message of that type, it will also be sent to the given messagereceiver for further
        /// processing.
        /// </summary>
        /// <param name="trcRec">The new messagereceiver for the given type of messages</param>
        /// <param name="messageType">The type of message to be sent to the given messagereceiver</param>
        public void addReceiver(MessageReceiver trcRec, Type messageType)
        {

            if (trcRec == null)
            {
                sendWarning("Can not add receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', method 'void addReceiver(MessageReceiver "
                                + "trcRec, Class messageType)'",
                        "The parameter 'trc' passed was a null reference.",
                        "Make sure to construct a valid MessageReciever before adding it to "
                                + "the experiment's messaging system.");
                return; // do nothing
            }

            if (messageType == null)
            { // again these damned null values
                sendWarning("Can not add receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', method 'void addReceiver(MessageReceiver "
                                + "trcRec, Class messageType)'",
                        "The parameter 'messageType' passed was a null reference.",
                        "Make sure to construct a valid Class object before adding it to "
                                + "the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.register(trcRec, messageType);

        }

        /// <summary>
        /// Adds a messagereceiver for tracenotes to the experiment. Whenever a model produces a message of
        /// that type, it will also be sent to the given messagereceiver for further processing. Note that
        /// the given Receiver must be capable of handling tracenotes.
        /// </summary>
        /// <param name="trcRec">The new messagereceiver for the given type of messages</param>
        public void addTraceReceiver(MessageReceiver trcRec)
        {

            if (trcRec == null)
            {
                sendWarning("Can not add receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', method 'void addTraceReceiver("
                                + "MessageReceiver trcRec)'",
                        "The parameter 'trc' passed was a null reference.",
                        "Make sure to construct a valid MessageReciever before adding it to "
                                + "the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.register(trcRec, tracenote);

        }

        /// <summary>
        /// Returns a boolean indicating whether debug notes are forwarded to the debug ouput or not. Debug
        /// ouput can be switched on and off using the methods <c>debugOn(TimeInstant startTime)</c> or
        /// <c>debugOff(TimeInstant stopTime)</c>
        /// </summary>
        /// <returns>boolean</returns>
        public bool debugIsOn()
        {

            return _messMan.isOn(debugnote);

        }

        /// <summary>Switches the debug output off at the given point of simulation time.</summary>
        /// <param name="stopTime">The point in simulation time to switch off debug</param>
        public void debugOff(TimeInstant stopTime)
        {

            // check initial TimeInstant parameter
            if (stopTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for debug output given! "
                                + "StopTime is set to current time.",
                        "Experiment '" + getName()
                                + "', method 'void debugOn(TimeInstant startTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object, otherwise use method "
                                + "start() without TimeInstant parameter.");
                stopTime = clientScheduler.presentTime();
            }

            // check if parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), stopTime))
            {
                sendWarning("Invalid start time parameter for debug output given! "
                        + "StopTime is set to current time.", "Experiment '"
                        + getName()
                        + "', method 'void debugOn(TimeInstant stopTime)'",
                        "The stopTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                stopTime = clientScheduler.presentTime();
            }

            ExternalEvent debugOff = new ExternalEventDebugOff(_client, true);

            debugOff.schedule(stopTime);

        }

        /// <param name="stopTime">The point in simulation time to switch debug off</param>
        /// <remarks>
        /// Use debugOff(TimeInstant startTime). Switches the debug output off at the given point of
        /// simulation time.
        /// </remarks>
        public void debugOff(SimTime stopTime)
        {
            this.debugOff(SimTime.toTimeInstant(stopTime));
        }

        /// <summary>Switches the debug output on at the given point of simulation time.</summary>
        /// <param name="startTime">The point in simulation time to switch on debug</param>
        public void debugOn(TimeInstant startTime)
        {

            // check initial TimeInstant parameter
            if (startTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for debug output given! "
                                + "StartTime is set to current time.",
                        "Experiment '" + getName()
                                + "', method 'void debugOn(TimeInstant startTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object, otherwise use method "
                                + "start() without TimeInstant parameter.");
                startTime = clientScheduler.presentTime();
            }

            // check if parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), startTime))
            {
                sendWarning("Invalid start time parameter for debug output given! "
                        + "StartTime is set to current time.", "Experiment '"
                        + getName()
                        + "', method 'void debugOn(TimeInstant startTime)'",
                        "The startTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                startTime = clientScheduler.presentTime();
            }

            // if parameter equals current time, set trace on immediately, e.g. 
            // to include initial scheduling
            if (TimeInstant.isEqual(clientScheduler.presentTime(), startTime))
            {
                getMessageManager().switchOn(debugnote);
                _client.sendTraceNote("Debug switched on");
                // Otherwise schedule an appropriate event    
            }
            else
            {
                ExternalEvent debugOn = new ExternalEventDebugOn(_client, true);
                debugOn.schedule(startTime);
            }

        }

        /// <param name="startTime">The point in simulation time to switch debug on</param>
        /// <remarks>
        /// Use debugOn(TimeInstant startTime). Switches the debug output on at the given point of simulation
        /// time.
        /// </remarks>
        public void debugOn(SimTime startTime)
        {
            this.debugOn(SimTime.toTimeInstant(startTime));
        }

        /// <summary>
        /// Switches the debug output on for the given period of simulation time. If the second parameter
        /// (off) is "sooner" then the first parameter (on), they will be swapped automatically. Same
        /// parameters will result in no debug output at all!
        /// </summary>
        /// <param name="startTime">The point in simulation time to switch debug on</param>
        /// <param name="stopTime">The point in simulation time to switch debug off</param>
        public void debugPeriod(TimeInstant startTime, TimeInstant stopTime)
        {
            // check initial TimeInstant parameter
            if (startTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for debug output given! Command ignored",
                        "Experiment '" + getName()
                                + "', Method 'debugPeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object.");
                return;
            }

            // check initial TimeInstant parameter
            if (stopTime == null)
            {
                sendWarning(
                        "Invalid stop time parameter for debug output given! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'debugPeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object.");
                return;
            }

            // check for correct order in parameters
            if (TimeInstant.isAfter(startTime, stopTime))
            {

                // swap parameters
                TimeInstant buffer = stopTime;
                stopTime = startTime;
                startTime = buffer;

            }

            // check if stop parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), stopTime))
            {
                sendWarning(
                        "Invalid stop time parameter for debug output given! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'debugPeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "The stopTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                return;
            }

            // check if start parameter is in past
            if (TimeInstant.isAfter(clientScheduler.presentTime(), startTime))
            {
                sendWarning("Invalid start time parameter for debug output given! "
                        + "Debug output has been set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'debugPeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "The startTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                startTime = clientScheduler.presentTime();
            }

            // set debug to switch on
            debugOn(startTime);

            // set debug to switch off
            debugOff(stopTime);
        }

        /// <param name="startTime">The point in simulation time to switch debug on</param>
        /// <param name="stopTime">The point in simulation time to switch debug off</param>
        /// <remarks>
        /// Replaced by debugTime(TimeInstant a, TimeInstant b). Switches the debug output on for the given
        /// period of simulation time. If the second parameter (off) is "sooner" then the first parameter
        /// (on), they will be swapped automatically. Same parameters will result in no debug output at all!
        /// </remarks>
        //@Deprecated
        public void debugPeriod(SimTime startTime, SimTime stopTime)
        {
            debugPeriod(SimTime.toTimeInstant(startTime), SimTime
                    .toTimeInstant(stopTime));
        }

        /// <summary>
        /// De-registers a file at the experiment. Registered files will be flushed and closed after the
        /// experiment has finished. If the file is manually closed by the user and has been registered at
        /// the Experiment, deRegister it
        /// </summary>
        /// <param name="file">The file to be closed with the end of an Experiment</param>
        public void deRegister(FileOutput file)
        {

            if (file == null)
            {
                sendWarning("Can not de-register FileOutput! Command ignored.",
                        "Experiment '" + getName()
                                + "' method 'void deRegister(FileOutput file).'",
                        "The parameter given was a null reference.",
                        "Make sure to only connect valid FileOutputs at the Experiment.");
                return;
            }

            _registryFileOutput.Remove(file);
            // remove whether it was inside or not

        }

        /**
         * Stopps all running simprocesses that might still be scheduled and closes
         * the output files.
         */
        public void finish()
        {

            // check if experiment has not been aborted before
            if (_status >= ABORTED)
            {
                return;
            }

            if (_traceOutput != null)
            {
                foreach (var trc in _traceOutput)
                {
                    if (trc is OutputTypeEndToExport)				{
                        ((OutputTypeEndToExport)trc).export(_pathName, getName());
                    }
                }
            }
            if (_debugOutput != null)
            {
                foreach (OutputType dbg in _debugOutput)
                {
                    if (dbg is OutputTypeEndToExport) {
                        ((OutputTypeEndToExport)dbg).export(_pathName, getName());
                    }
                }
            }

            if (_errorOutput != null)
            {
                foreach (OutputType err in _errorOutput)
                {
                    if (err is OutputTypeEndToExport) {
                        ((OutputTypeEndToExport)err).export(_pathName, getName());
                    }
                }
            }
            if (_reportOutput != null)
            {
                foreach (OutputType rpt in _reportOutput)
                {
                    if (rpt is OutputTypeEndToExport){
                        ((OutputTypeEndToExport)rpt).export(_pathName, getName());
                    }
                }
            }

            // set status to let all simthreads be killed
            _status = ABORTED;

            // close all files still open
            foreach (OutputType o in _registryOutputType)
                o.close();
            foreach (FileOutput f in _registryFileOutput)
                f.close();

            // kill all SimThreads still active
            //MyThread[] survivors = new MyThread[_expThreads.activeCount()];
            //_expThreads.enumerate(survivors);

            //for (int i = 0; i < survivors.Length; i++)
            //{

            //    // print existing threads for controlling purposes only
            //    // System.out.println(survivors[i]);

            //    // if we get the enumeration of survivors, some of them
            //    // might not have made it until here and die in between
            //    // so an occasional NullPointerException is perfectly
            //    // alright and no reason to worry -> we just dump it.
            //    //if (survivors[i] == typeof(SimThread))			{
            //    //    try
            //    //    {
            //    //        ((SimThread)survivors[i]).kill();
            //    //    }
            //    //    catch (NullPointerException e)
            //    //    {
            //    //        ; // forget it anyway...
            //    //    }
            //    //}   
            //}

        }

        /// <summary>
        /// Returns the distributionmanager for this experiment. Distributions need access to the
        /// distributionmanager for handling antithetic modes, resetting and their initial seeds.
        /// </summary>
        /// <returns>The distributionmanager for this experiment</returns>
        public DistributionManager getDistributionManager()
        {

            return _distMan;

        }

        /// <summary>
        /// Returns the epsilon value representing the granularity of simulation time for this experiment. So
        /// far, Hour, Minute, Second and Millisecond are supported. Default (unless set explicitly) is
        /// TimeUnit.MICROSECONDS.
        /// </summary>
        /// <returns>The Granularity of the simulation time</returns>
        public TimeUnit getEpsilonUnit()
        {

            return TimeOperations.getEpsilon();
        }

        /// <returns>The Granularity of the simulation time</returns>
        /// <remarks>
        /// Use getEpsilonUnit(). Returns a SimTime representation of the granularity of simulation time for
        /// this experiment. So far, Hour, Minute, Second and Millisecond are supported.
        /// </remarks>
        public SimTime getEpsilon()
        {

            return SimTime.toSimTime(new TimeSpan(1L, TimeOperations.getEpsilon()));
        }

        /// <summary>Returns the current execution Speed Rate.</summary>
        /// <returns>The current execution speed rate.</returns>
        public double getExecutionSpeedRate()
        {
            return this.clientScheduler.getExecutionSpeedRate();
        }

        /// <summary>
        /// Returns the messagemanager for this experiment. Messages need access to the MessageManager for
        /// distributing the messages to one or more specified output streams.
        /// </summary>
        /// <returns>The messagemanager for this experiment</returns>
        public MessageDistributor getMessageManager()
        {

            return _messMan;

        }

        /// <summary>
        /// Returns the model that is connected to this experiment or <c>null</c> if no model is connected so
        /// far.
        /// </summary>
        /// <returns>
        /// The model that this experiment is connected to or <c>null</c> if no connection is established.
        /// </returns>
        public Model getModel()
        {

            return _client;

        }

        /// <summary>
        /// Returns the name of the path the experiment's report-, trace-, debug- and error-files are written
        /// to.
        /// </summary>
        /// <returns>the experiment's output path</returns>
        public string getOutputPath()
        {
            //return File(_pathName) .getAbsolutePath();
            return _pathName;
        }

        public List<List<string>> getOutputAppendixes()
        {

            List<List<string>> appendixes = new List<List<string>>();

            var debugAppendixes = new List<string>();
            foreach (OutputType o in this._debugOutput)
            {
                debugAppendixes.Add(o.getAppendix());
            }

            appendixes.Add(debugAppendixes);

            var traceAppendixes = new List<string>();
            foreach (OutputType o in this._traceOutput)
            {
                traceAppendixes.Add(o.getAppendix());
            }
            appendixes.Add(traceAppendixes);

            var errorAppendixes = new List<string>();
            foreach (OutputType o in this._errorOutput)
            {
                errorAppendixes.Add(o.getAppendix());
            }
            appendixes.Add(errorAppendixes);

            var reportAppendixes = new List<string>();
            foreach (OutputType o in this._reportOutput)
            {
                reportAppendixes.Add(o.getAppendix());
            }
            appendixes.Add(reportAppendixes);

            return appendixes;
        }

        public long getRealTimeStartTime()
        {
            return _realTimeStartTime;
        }

        /// <summary>
        /// Returns the reference unit for this experiment. This is the time unit mapped to a time step of
        /// 1.0 in simulation time. So far, Hour, Minute, Second and Millisecond are supported. Default
        /// (unless set explicitly) is TimeUnit.SECONDS.
        /// </summary>
        /// <returns>The reference unit.</returns>
        public TimeUnit getReferenceUnit()
        {
            return TimeOperations.getReferenceUnit();
        }

        public static void setEpsilon(TimeUnit epsilon)
        {
            if (epsilon != TimeOperations.getEpsilon() && TimeOperations.timeObjectsCreated)
                Console.WriteLine("Warning: epsilon changed after time objects were already created.");
            TimeOperations.setEpsilon(epsilon);
        }

        public static void setReferenceUnit(TimeUnit referenceUnit)
        {
            TimeOperations.setReferenceUnit(referenceUnit);
        }

        /// <summary>
        /// Returns the resource database for this experiment. The <c>Res</c> objects need access to the
        /// resource database to note their resource allocations and requests and for deadlock detection.
        /// </summary>
        /// <returns>the resource database storing all resource allocations and requests.</returns>
        /// <remarks>author: Soenke Claassen</remarks>
        public ResourceDB getResourceDB()
        {

            return _resDB;
        }

        /// <summary>
        /// Returns the scheduler for this experiment. ModelComponents need access to the scheduler for
        /// identifying the current active entity or process and to schedule themselves or other schedulables
        /// to activate at a given time in the future.
        /// </summary>
        /// <returns>The scheduler for this experiment</returns>
        public Scheduler getScheduler()
        {

            return clientScheduler;

        }

        /// <summary>
        /// Returns the simclock for this experiment. ModelComponents need access to the simclock for
        /// retrieveing the current simulation time.
        /// </summary>
        /// <returns>The simclock for this experiment</returns>
        public SimClock getSimClock()
        {

            return clientScheduler.getSimClock();

        }

        /// <summary>Returns the TimeInstant when the experiment is expected to stop running.</summary>
        /// <returns>The time at which the experiment is expected to stop running.</returns>
        public TimeInstant getStopTime()
        {

            return _stopTime;
        }

        /// <summary>
        /// Returns the Conditions which can cause an experiment to stop. May be empty if there are no such
        /// Conditions.
        /// </summary>
        /// <returns>Condition</returns>
        /// <remarks>author: Tim Janz</remarks>
        public List<ModelCondition> getStopConditions()
        {

            return new List<ModelCondition>(this._stopConditions);
        }

        /**
         * Removes all conditions set to stop the experiment.
         */
        public void removeStopConditions()
        {

            this._stopConditions.Clear();
        }

        /// <summary>/**</summary>
        /// <returns>
        /// The number of floating point digits of simulation time to be displayed in output files
        /// </returns>
        /// <remarks>
        /// Depends on TimeFormatter in use. Returns the experiment's number of floating point digits of
        /// simulation time that are displayed in the various output files, as read from the
        /// SingleUnitTimeFormatter, if in use. Otherwise, 0 will be returned.
        /// </remarks>
        public int getTimeFloats()
        {

            //if (TimeOperations.getTimeFormatter() instanceof SingleUnitTimeFormatter) {
            //    return (int)((SingleUnitTimeFormatter)TimeOperations.getTimeFormatter())._floats;
            //} else
            return 0;
        }

        /// <summary>
        /// Displays the current state of the simulation run. If an experiment is aborted, it can not be
        /// proceeded. All SimThreads still active are stopped, the main routine can finish.
        /// </summary>
        /// <returns>
        /// Is <c>true</c> if the simulation is aborted, <c>false</c> if it has not started yet or is still
        /// running
        /// </returns>
        public bool isAborted()
        {

            return (_status >= ABORTED);

        }

        /// <summary>Shows if this experiment has already been connected to a model.</summary>
        /// <returns>Is <c>true</c>, if experiment is connected to a model, <c>false</c> otherwise</returns>
        public bool isConnected()
        {

            return (_status >= CONNECTED); // model connected

        }

        /// <summary>Shows if this experiment has not run yet.</summary>
        /// <returns>Is <c>true</c>, if experiment is not running yet, <c>false</c> otherwise</returns>
        public bool isPreparing()
        {

            return (_status < RUNNING);

        }

        /// <summary>
        /// Returns if the event-list processes concurrent Events in random order or not. Default is not.
        /// </summary>
        /// <returns><c>true</c> if concurrent Events are randomized, <c>false</c> otherwise</returns>
        /// <remarks>author: Ruth Meyer</remarks>
        public bool isRandomizingConcurrentEvents()
        {
            return clientScheduler.isRandomizingConcurrentEvents();
        }

        /// <summary>Displays the current state of the simulation run.</summary>
        /// <returns>
        /// Is <c>true</c> if the simulation is running, <c>false</c> if it has not started yet or has
        /// already finished
        /// </returns>
        public bool isRunning()
        {

            return (_status == RUNNING);

        }

        /// <summary>Returns if a progress bar should be displayed for this experiment or not.</summary>
        /// <returns>
        /// <c>true</c> if a progress bar should be displayed for this experiment, <c>false</c> otherwise.
        /// </returns>
        public bool isShowProgressBar()
        {

            return _showProgressBar;
        }

        /// <summary>
        /// Displays the current state of the simulation run. If an experient is stopped, it can be proceeded
        /// by calling proceed().
        /// </summary>
        /// <returns>Is <c>true</c>, if experiment is stopped, <c>false</c> otherwise</returns>
        public bool isStopped()
        {

            return (_status == STOPPED); // model stopped

        }

        /// <summary>
        /// Returns the current status of this experiment as an integer constant.
        /// Compare against <c>Experiment.NOT_INITIALIZED</c>, <c>INITIALIZED</c>,
        /// <c>CONNECTED</c>, <c>STARTED</c>, <c>RUNNING</c>, <c>STOPPED</c>,
        /// or <c>ABORTED</c>.
        /// </summary>
        public int getStatus()
        {
            return _status;
        }

        /// <summary>
        /// Determines whether or not an error or warning has yet occurred during this experiment.
        /// </summary>
        /// <returns>
        /// <c>True</c> if at least one error has occurred in the model connected to this experiment or one
        /// of its submodels, <c>false</c> otherwise
        /// </returns>
        public bool hasError()
        {

            return _error;

        }

        /// <summary>
        /// Interrupts experiment execution in case a fatal problem prevents the experiment from being
        /// continued.
        /// </summary>
        /// <param name="e">The exception that caused the interrupt</param>
        public void interrupt(DESMOJException e)
        {

            if (_status != RUNNING)
            {
                sendWarning(
                        "Can not interrupt Experiment! Command ignored.",
                        "Experiment: " + getName() + " Method: void interrupt(Exception e).",
                        "The Experiment is not running.",
                        "Only Experiments that are running can be interruped.");
                return;
            }

            if (e == null)
            {
                sendWarning(
                        "Can not interrupt Experiment! Command ignored.",
                        "Experiment: " + getName() + " Method: void interrupt(Exception e).",
                        "No exception (i.e. <code>null</code>) has been passed.",
                        "Submit a <code>DESMOJException</code> object describing why "
                           + "the experiment has to be terminated immediately.");
                return;
            }

            // Interrupt proceeding
            this._interruptingException = e;
        }

        /// <summary>
        /// Proceeds with a stopped experiment. An experiment can be stopped, if either its status is changed
        /// from <c>RUNNING</c> to some other state, the scheduler runs out of scheduled events or if the
        /// <c>check()</c> method of the given stop <c>Condition</c> returns <c>true</c> after an event has
        /// been processed.
        /// </summary>
        public void proceed()
        {

            if (_status < STARTED)
            {
                sendWarning(
                        "Can not proceed with Experiment! Command ignored.",
                        "Experiment: " + getName() + " Method: void proceed().",
                        "The Experiment has not been started yet.",
                        "Only Experiments that have been stopped after method 'start()' has "
                                + "been called can use method 'proceed()' to continue.");
                return;
            }

            if (_status > STOPPED)
            {
                sendWarning("Can not proceed with Experiment! Command ignored.",
                        "Experiment " + getName() + " Method: void proceed().",
                        "The Experiment has already been aborted.",
                        "Use method 'proceed()' only on stopped experiments.");
                return;
            }
            if (_status == STARTED)
            {
                // print status message to calm users waiting long, long, long
                // hours...
                if (!_silent) Console.WriteLine("***** DESMO-J version " + getDesmoJVersion()
                        + " ***** \n" + getName() + " starts at simulation time "
                        + getScheduler().presentTime() + ".\n ...please wait...");
            }
            else
            {
                if (!_silent) Console.WriteLine(getName() + " resumes at simulation time "
                        + getScheduler().presentTime() + ".\n ...please wait...");
            }
            // display a progress bar if stop time is known and showProgressBar is
            // true
            //if (_stopTime != null && _showProgressBar)
            //{
            //    JFrame frame = new ExpProgressBar(this, _showProgressBarAutoclose);

            //    frame.addWindowListener(new WindowAdapter()
            //    {

            //                public void windowClosing(WindowEvent e)
            //    {
            //        System.exit(0);
            //    }
            //});

            //frame.pack();
            //// frame.setSize(380,90);
            //frame.setVisible(true);
            //}

            _status = RUNNING; // now checked to run
            bool gotEvent = false; // buffer to check if scheduler works

            try
            {

                while (_status == RUNNING)
                {

                    // infinite loop until condition/time expired
                    gotEvent = clientScheduler.processNextEventNote();

                    // exception set, e.g. by a process thread? If so, throw it!
                    if (_interruptingException != null) throw _interruptingException;

                    // out of events?
                    if (gotEvent == false)
                    {
                        _status = STOPPED;
                    }

                    // check potential stop conditions
                    if (_stopConditions.Count > 0)
                    {
                        foreach (ModelCondition c in _stopConditions)
                        {
                            if (c.check())
                            {
                                _status = STOPPED;
                                break;
                            }
                        }
                    }

                    // Sleep a while (modified by N. Knaak)
                    if (_status == RUNNING && _delayInMillis != 0)
                        System.Threading.Thread.Sleep((int)_delayInMillis);
                }
            }
            catch (DESMOJException e)
            {
                Console.WriteLine("disaster recovery");
                // this is the disaster recovery routine to stop simulation and save
                // the report to disc before exiting the faulty experiment
                _messMan.receive(e.getErrorMessage());
                report();
                finish();
                _status = ABORTED;
                //e.printStackTrace();
            }
            catch (Exception e)
            {
                Console.WriteLine("disaster recovery");
                Console.Error.WriteLine(e.ToString());
                // this is the disaster recovery routine to stop simulation and save
                // the report to disc before exiting the faulty experiment
                // messMan.receive(e.getMessage());
                report();
                finish();
                _status = ABORTED;
                return; // skip gotEvent check — streams already closed by finish()
                //e.printStackTrace();
            }

            // give warning if reason for stopping was empty EventList
            if (gotEvent == false)
            {
                sendWarning("No more events scheduled! Experiment is stopped.",
                        "Experiment '" + getName() + "' method void proceed().",
                        "The scheduler has run out of events to handle.",
                        "Make sure to always have events to be scheduled i.e. by letting an "
                                + "Entity create and schedule its successor.");
            }

            // print status message to user...
            if (!_silent)
            {
                Console.WriteLine(getName() + " stopped at simulation time "
                    + getScheduler().presentTime() + ".");
                if (hasError())
                {
                    Console.WriteLine("At least one error or warning has occurred.");
                    if (_errorOutput.Count > 0)
                    {
                        Console.WriteLine("Please re-run the siumulation with error output enabled.");
                    }
                    else
                    {
                        Console.WriteLine("Please refer to the error output for details.");
                    }
                }
            }
        }

        /// <summary>Sets the delay between each step of the scheduler.</summary>
        /// <param name="delay">: Delay time in milliseconds as a long value</param>
        /// <remarks>author: Nicolas Knaak</remarks>
        public void setDelayInMillis(long delay)
        {
            _delayInMillis = delay;
        }

        /// <summary>Returns the delay between each step of the scheduler</summary>
        /// <returns>A long value representing the delay time in milliseconds</returns>
        /// <remarks>author: Nicolas Knaak</remarks>
        public long getDelayInMillis()
        {
            return _delayInMillis;
        }


        /// <summary>
        /// Registers a file output (Report, Trace, Error, Debug) in specific formats (e.g. HTML, ASCII, XML)
        /// at the experiment. Registered files will be flushed and closed after the experiment has finished.
        /// This is handy for modellers producing their own output who want their files to be closed at the
        /// end of the experiment.
        /// </summary>
        /// <param name="file">The file to be closed with the end of an experiment</param>
        public void register(OutputType file)
        {

            if (file == null)
            {
                sendWarning("Can not register OutputType! Command ignored.",
                        "Experiment '" + getName()
                                + "' method void register(OutputType file).",
                        "The parameter given was a null reference.",
                        "Make sure to only connect valid OutputType at the Experiment.");
                return;
            }

            if (_registryOutputType.Contains(file))
                return; // file already registered

            _registryOutputType.Add(file);

        }

        /// <summary>
        /// Registers a custom file output at the experiment, e.g. TimeSeries plotting data to a file.
        /// Registered files will be flushed and closed after the experiment has finished. This is handy for
        /// modellers producing their own output who want their files to be closed at the end of the
        /// experiment.
        /// </summary>
        /// <param name="file">The file to be closed with the end of an experiment</param>
        public void registerFileOutput(FileOutput file)
        {

            if (file == null)
            {
                sendWarning("Can not register FileOutput! Command ignored.",
                        "Experiment '" + getName()
                                + "' method void register(OutputType file).",
                        "The parameter given was a null reference.",
                        "Make sure to only connect valid FileOutput at the Experiment.");
                return;
            }

            if (_registryFileOutput.Contains(file))
                return; // file already registered

            _registryFileOutput.Add(file);

        }

        /**
         * Connects a model to this experiment. The given model must not be submodel
         * of other models and not already be connected to some other experiment.
         * Otherwise an errormessage will be given and the experiment will be
         * stopped.
         */
        public void registerModel(Model mainModel)
        {

            if (mainModel == null)
            {
                sendWarning(
                        "Can not register model at experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void registerModel(Model mainModel)'",
                        "The parameter passed was a null reference.",
                        "Make sure to connect a valid main model to this experiment.");
                return; // no connection possible.
            }

            if (mainModel.getModel() != null)
            {
                sendWarning(
                        "Can not register model at experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void registerModel(Model mainModel)'",
                        "The model references another model as its owner, thus can not be the "
                                + "main model.",
                        "Make sure to connect a valid main model to this experiment.");
                return; // no connection possible.
            }

            if (isConnected())
            {
                sendWarning(
                        "Can not register model at experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void registerModel(Model mainModel)'",
                        "This experiment is already connected to model : "
                                + _client.getName(),
                        "An experiment may only be connected to one main model at a time.");
                return; // no connection possible.
            }

            _status = CONNECTED;
            _client = mainModel;
            _client.setMain();

        }

        /// <summary>
        /// Removes a messagereceiver for debugnotes from the experiment's messagedistributor. Whenever a
        /// model produces a message of that type, it will not be sent to the given messagereceiver anymore.
        /// Note that if the messagereceiver is also registered for other types of messages, these will not
        /// be affected. Use method <c>removeReceiverAll(MessageReceiver msgRec)</c> to remove a
        /// messagereceiver from all types of messages.
        /// </summary>
        /// <param name="msgRec">
        /// The new messagereceiver to be removed from the messagedistributor's list for the given
        /// messagetype
        /// </param>
        public void removeDebugReceiver(MessageReceiver msgRec)
        {

            if (msgRec == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeDebugReceiver"
                                + "(MessageReceiver msgRec)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.deRegister(msgRec, debugnote);

        }

        /// <summary>
        /// Removes a messagereceiver for errormessages from the experiment's messagedistributor. Whenever a
        /// model produces a message of that type, it will not be sent to the given messagereceiver anymore.
        /// Note that if the messagereceiver is also registered for other types of messages, these will not
        /// be affected. Use method <c>removeReceiverAll(MessageReceiver msgRec)</c> to remove a
        /// messagereceiver from all types of messages.
        /// </summary>
        /// <param name="msgRec">
        /// The new messagereceiver to be removed from the vessagedistributor's list for the given
        /// messagetype
        /// </param>
        public void removeErrorReceiver(MessageReceiver msgRec)
        {

            if (msgRec == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeErrorReceiver"
                                + "(MessageReceiver msgRec)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.deRegister(msgRec, errormessage);

        }

        /// <summary>
        /// Removes a messagereceiver from the experiment's messagedistributor. The given messagereceiver
        /// will not receive messages of any type any more Use method <c>removeReceiver(MessageReceiver
        /// msgRec, Class messageType)</c> to remove the messagereceiver from one type of messages only.
        /// </summary>
        /// <param name="msgRec">
        /// The new messagereceiver to be removed from the messagedistributor's list for the given
        /// messagetype
        /// </param>
        public void removeReceiver(MessageReceiver msgRec)
        {

            if (msgRec == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeReceiver(MessageReceiver "
                                + "msgRec)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.deRegister(msgRec);

        }

        /// <summary>
        /// Removes a messagereceiver for the given subtype of message from the Experiment's
        /// messagedistributor. Whenever a model produces a message of that type, it will not be sent to the
        /// given messagereceiver anymore. Note that if the messagereceiver is also registered for other
        /// types of messages, these will not be affected. Use method <c>removeReceiverAll(MessageReceiver
        /// msgRec)</c> to remove a messagereceiver from all types of messages.
        /// </summary>
        /// <param name="msgRec">
        /// The new messagereceiver to be removed from the messagedistributor's list for the given
        /// messagetype
        /// </param>
        /// <param name="messageType">The type of message not to be sent to the given messagereceiver</param>
        public void removeReceiver(MessageReceiver msgRec, Type messageType)
        {

            if (msgRec == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeReceiver(MessageReceiver "
                                + "msgRec, Class messageType)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            if (messageType == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeReceiver(MessageReceiver "
                                + "msgRec, Class messageType)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.deRegister(msgRec, messageType);

        }

        /// <summary>
        /// Removes a messagereceiver for tracenotes from the experiment's messagedistributor. Whenever a
        /// model produces a message of that type, it will not be sent to the given messagereceiver anymore.
        /// Note that if the messagereceiver is also registered for other types of messages, these will not
        /// be affected. Use method <c>removeReceiverAll(MessageReceiver msgRec)</c> to remove a
        /// messagereceiver from all types of messages.
        /// </summary>
        /// <param name="msgRec">
        /// The new messagereceiver to be removed from the messagedistributor's list for the given
        /// messagetype
        /// </param>
        public void removeTraceReceiver(MessageReceiver msgRec)
        {

            if (msgRec == null)
            {
                sendWarning(
                        "Can not remove receiver to experiment! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'void removeTraceReceiver"
                                + "(MessageReceiver msgRec)'",
                        "The parameter 'msgRec' passed was a null reference.",
                        "Make sure to give a valid MessageReciever reference before removing it "
                                + "from the experiment's messaging system.");
                return; // do nothing
            }

            _messMan.deRegister(msgRec, tracenote);

        }

        /// <summary>
        /// Overrides inherited <c>NamedObjectImp.rename(String newName)</c> method to prevent the user from
        /// changing the experiment's name during an experiment. Renaming is not allowed with experiments,
        /// since it would not allow the user to identify the reports produced by an experiment. The method
        /// simply returns without changing the experiment's name, ignoring the given parameter.
        /// </summary>
        /// <param name="newName">
        /// The parameter given is not taken as the new name, method simply returns
        /// </param>
        public void rename(string newName)
        {

            // do nothing since renaming experiments is not allowed
            // would do too much confusion

        }

        /**
         * Writes a report about the model connected top this experiment, its
         * reportable components and all related submodels into the report output.
         * Note that a report can only be produced, if a valid main model is already
         * connected to the experiment.
         */
        public void report()
        {

            // just pass on the call with main model as parameter
            report(_client);

        }

        /**
         * Writes a report about the given model which has to be connected to this
         * experiment as main model or as a submodel. Note that this will report
         * about a branch of the tree of submodels constructed. A report will only
         * be produced, if the model given is connected to this experiment. All
         * reportable components of this model and all related submodels will be
         * sent to the report output configured at the experiment's
         * messagedistributor. Note that a report can only be produced, if a valid
         * main model is already connected to the experiment.
         */
        public void report(Model m)
        {

            List<Reporter> reporters;
            // buffer for the reportmanager returned by client

            if (_status < CONNECTED)
            {
                sendWarning(
                        "Can not produce report! Command ignored.",
                        "Experiment: " + getName()
                                + " Method: void report(Model m).",
                        "The Experiment has not been connected to a model to report about yet.",
                        "Connect a model to the experiment first using the model's method "
                                + "connectToExperiment(Experiment exp).");
                return; // no client there to be reported
            }

            if (_status >= ABORTED)
            {
                // do nothing since experiment has already been aborted and all
                // output channels are already shut down
                return; // Experiment aborted
            }

            if (m == null)
            {
                sendWarning("Can not produce report! Command ignored.",
                        "Experiment: " + getName()
                                + " Method: void report(Model m).",
                        "The model parameter given is a null reference.",
                        "Always make sure to use valid references.");
                return; // no model there to be reported
            }

            if (m.getExperiment() != this)
            {
                sendWarning(
                        "Can not produce report! Command ignored.",
                        "Experiment: " + getName()
                                + " Method: void report(Model m).",
                        "The model parameter given is connected to a different experiment.",
                        "Only experiments connected to theat model can produce reports "
                                + "about that model.");
                return; // model connected to other experiment
            }

            // get the client's reportmanager containing all reporters in sorted
            // order
            reporters = m.report();

            // get all out according to sorted order and send them to the report
            // output
            // registered at the experiment's messagemanager
            foreach (Reporter r in reporters)
            {

                _messMan.receive(r);

            }
        }

        /// <summary>
        /// Creates and sends a debugnote to the messagedistributor. Be sure to have a correct location,
        /// since the object and method that the error becomes apparent is not necessary the location it was
        /// produced in. The information about the simulation time is extracted from the Experiment and must
        /// not be given as a parameter.
        /// </summary>
        /// <param name="description">The description of the error that occured</param>
        void sendDebugNote(string component, string description)
        {

            // comnpose the DebugNote and send it in one command
            sendMessage(new DebugNote(clientScheduler.getCurrentModel(),
                    clientScheduler.getSimClock().getTime(), component, description));

        }

        /// <summary>
        /// Sends a message to the messagedistributor. Note that there are other shorthands for sending the
        /// standard DESMO-J messages.
        /// </summary>
        /// <param name="m">The Message to be transmitted</param>
        /// <remarks>
        /// See also ModelComponent.sendTraceNote See also ModelComponent.sendDebugNote See also
        /// ModelComponent.sendWarning
        /// </remarks>
        void sendMessage(Message m)
        {

            if (m == null)
            {
                sendWarning("Can't send Message!", "Experiment :" + getName()
                        + " Method: SendMessage(Message m)",
                        "The Message given as parameter is a null reference.",
                        "Be sure to have a valid Message reference.");
                return; // no proper parameter
            }

            if (!_error && m.GetType() == typeof(ErrorMessage))
            {
                _error = true;
            }

            _messMan.receive(m);
        }

        /// <summary>
        /// Creates and sends an error message to the messagedistributor to warn the modeller that some
        /// conditions required by the framework are not met. Be sure to have a correct location, since the
        /// object and method that the error becomes apparent is not necessary the location it was produced
        /// in. The information about the simulation time is extracted from the experiment and must not be
        /// given as a parameter.
        /// </summary>
        /// <param name="description">The description of the error that occured</param>
        /// <param name="location">The class and method the error occured in</param>
        /// <param name="reason">The reason most probably responsible for the error to occur</param>
        /// <param name="prevention">
        /// The measures a user should take to prevent this warning to be issued again
        /// </param>
        public void sendWarning(string description, string location, string reason,
                string prevention)
        {

            // comnpose the WarningMessage and send it in one command
            sendMessage(new ErrorMessage(clientScheduler.getCurrentModel(),
                    description, location, reason, prevention, clientScheduler
                            .getSimClock().getTime()));

        }

        /// <summary>
        /// Sets the <c>TimeFormatter</c> to be used for output of time strings. Observe that if this method
        /// is called before creating an <c>Experiment</c> object, a non-null <c>TimeFormatter</c> passed to
        /// the <c>Experiment</c>'s constructor will overwrite the setting of this method-
        /// </summary>
        /// <param name="format">the formatter to be used for formatting time Strings.</param>
        public static void setTimeFormatter(TimeFormatter format)
        {
            TimeOperations.setTimeFormatter(format, true);
        }

        /// <summary>
        /// Determines if the event-list processes concurrent Events in random order or not. Default is not,
        /// i.e. when a new experiment is constructed, the event-list is set to "linear" order. Note: If you
        /// want the event-list to randomize concurrent Events you should call this method BEFORE scheduling
        /// any events. Otherwise any connections between events established via scheduleBefore() or
        /// scheduleAfter() are lost. So it's a good idea to call this method only once and right after
        /// constructing the experiment.
        /// </summary>
        /// <param name="randomizing">
        /// <c>true</c> forces random order, <c>false</c> forces "linear" order
        /// </param>
        /// <remarks>author: Ruth Meyer</remarks>
        public void randomizeConcurrentEvents(bool randomizing)
        {
            clientScheduler.setRandomizingConcurrentEvents(randomizing);
        }

        /// <summary>
        /// Sets the speed rate for an execution that is proportional to wall-clock time (real time). Set the
        /// speed rate to a value bigger than zero for a simulation that will progress proportional to
        /// wall-clock time. The following equation applies for speed rates &gt;0 : rate*simulation time =
        /// wallclock-time. If the speed rate is 0 or less the simulation will be executed as fast as
        /// possible. Default is 0 (as-fast-as-possible).
        /// </summary>
        /// <param name="rate">The execution speed rate</param>
        public void setExecutionSpeedRate(double rate)
        {
            clientScheduler.setExecutionSpeedRate(rate);
        }

        /// <summary>
        /// Sets the seed of the SeedGenerator to the given value. If the seed is not set here, its default
        /// is 979, unless specified different in the ExperimentOptions.
        /// </summary>
        /// <param name="seed">The seed for the SeedGenerator</param>
        public void setSeedGenerator(long seed)
        {

            _distMan.setSeed(seed);

        }

        /// <summary>
        /// Enables event-list logging for this experiment.
        /// Must be called after connectToExperiment() and before start().
        /// The log file is opened immediately and closed automatically by finish().
        /// </summary>
        /// <param name="filePath">
        /// Full output file path. Defaults to "&lt;experimentName&gt;_eventlist.txt"
        /// in the current directory when null or empty.
        /// </param>
        public void enableEventListLogging(string? filePath = null)
        {
            if (_eventListLogger != null) return;
            _eventListLogger = new EventListLogger();
            _eventListLogger.open(this, filePath);
            clientScheduler.setEventListLogger(_eventListLogger);
            registerFileOutput(_eventListLogger);
        }

        /// <summary>
        /// Sets the underlying pseudo random number generator to be used by all distributions created from
        /// now on. The default generator is LinearCongruentialRandomGenerator; any other generator to be
        /// used must implement the interface UniformRandomGenerator.
        /// </summary>
        /// <param name="randomNumberGenerator">The random number generator class to be used</param>
        /// <remarks>See also LinearCongruentialRandomGenerator See also UniformRandomGenerator</remarks>
        public void setRandomNumberGenerator(Type randomNumberGenerator)
        {
            bool classValid = randomNumberGenerator != null
                && !randomNumberGenerator.IsAbstract
                && !randomNumberGenerator.IsInterface
                && typeof(UniformRandomGenerator).IsAssignableFrom(randomNumberGenerator);

            if (classValid)
            {
                UniformRandomGenerator instance =
                    (UniformRandomGenerator)Activator.CreateInstance(randomNumberGenerator);
                this._distMan.setRandomNumberGenerator(instance);
            }
            else
            {
                this.sendWarning(
                    "Invalid random number generator given! Method call ignored!",
                    "Experiment '" + getName()
                        + "', Method 'setRandomNumberGenerator(Type randomNumberGenerator)'",
                    "The type provided '"
                        + (randomNumberGenerator?.Name ?? "null")
                        + "' is abstract, an interface, or does not implement UniformRandomGenerator.",
                    "Make sure to use a non-abstract class that implements UniformRandomGenerator.");
            }
        }

        /// <summary>
        /// Sets the new value for showing the progress bar for this experiment or not. Must be called before
        /// the experiment is started.
        /// </summary>
        /// <param name="newShowProgressBar">
        /// set it to <c>true</c> if a progress bar should be displayed; for not showing the progress bar of
        /// this experiment set it to <c>false</c>.
        /// </param>
        public void setShowProgressBar(bool newShowProgressBar)
        {

            this._showProgressBar = newShowProgressBar;
        }

        /// <summary>
        /// Sets the new value for automatically dismissing the progress bar for this experiment after
        /// completion. Not that a call to this method has no impact if the progress bar is suppessed by
        /// invoking <c>setShowProgressBar(false)</c>. Must be called before the experiment is started.
        /// </summary>
        /// <param name="autoclose">
        /// if set to <c>true</c>, the progress bar will be automaticalled closed one the experiment
        /// terminates. If set to <c>false</c> (default), the progress bar window persists after the
        /// experiment terminates until closed by the user.
        /// </param>
        public void setShowProgressBarAutoclose(bool autoclose)
        {

            this._showProgressBarAutoclose = autoclose;
        }

        /// <summary>
        /// Returns if the progress bar windows should be automatically closed after the experiment or not.
        /// Note that the result of this method is meaningless if displaying the progress bar has been
        /// suppessed by invoking <c>setShowProgressBar(false)</c>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a progress bar should be automatically closed after this experiment, while
        /// <c>false</c> (default) indicates that the progress bar window persists after the experiment
        /// terminates until closed by the user..
        /// </returns>
        public bool isShowProgressBarAutoclose()
        {

            return _showProgressBarAutoclose;
        }


        /// <summary>
        /// Sets the new value for displaying basic experiment notifications like 'experiment started',
        /// 'experiment stopped' oder 'experiment resumed' at the system output.
        /// </summary>
        /// <param name="silent">
        /// set it to <c>true</c> to suppress notifications or <c>false</c> to print them.
        /// </param>
        public void setSilent(bool silent)
        {

            this._silent = silent;
        }

        /// <summary>
        /// Sets the experiment's status to the given integer value. The value must be in the legal range of
        /// [-1,5], otherwise a warning is issued.
        /// </summary>
        /// <param name="newStatus">The integer value of the experiments' new status</param>
        public void setStatus(int newStatus)
        {

            if ((newStatus < -1) || (newStatus > 5))
            {
                sendWarning(
                        "Can not start experiment! Command ignored.",
                        "Experiment '" + getName() + "', Method 'start'",
                        "No main model's connectToExperiment(Experiment e) method was called.",
                        "Make sure to connect a valid main model first before starting "
                                + "this experiment.");
                return;
            }
            else
                _status = newStatus;

        }

        /// <summary>
        /// Starts the simulation with default start time 0. This method can only be used once on an
        /// experiment. it initializes the connected model and starts the simulation. Note that in order to
        /// stop the simulation, the <c>stop(TimeInstant stopTime)</c> method has to be called first!
        /// </summary>
        public void start()
        {

            // this allows us to use start() in loops for multiple experiment runs
            // in other words, this is a shortcut for the lazy programmer
            if (_status == STOPPED)
                proceed();

            // here's what start was supposed to be at first
            // a shortcut for startig an Experiment at TimeInstant(0)
            else
            {
                // now prepare connected model to start simulation
                start(new TimeInstant(0));
            }

        }

        /// <summary>
        /// Starts the experiment with the given simulation time as starting time. The experiment will not
        /// start unless a valid model has been connected to it before. Note that in order to stop the
        /// simulation at some point of time, the <c>stop</c> method has to be called first.
        /// <c>StopCondition</c> s can be given alternatively.
        /// </summary>
        /// <param name="initTime">The starting time instant</param>
        public void start(TimeInstant initTime)
        { 

            if (_status < CONNECTED)
            {
                sendWarning(
                        "Can not start experiment! Command ignored.",
                        "Experiment: " + getName()
                                + " Method: void start(SimTime initTime)",
                        "The Experiment has not been connected to a model to report about yet.",
                        "Connect a model to the experiment first using the model's method "
                                + "connectToExperiment(Experiment exp).");
                return;
            }
            if (_status > CONNECTED)
            {
                sendWarning(
                        "Can not start experiment! Command ignored.",
                        "Experiment: " + getName()
                                + " Method: void start(SimTime initTime)",
                        "The Experiment has already been started before.",
                        "An experiment can only be started once. If it has been stopped, "
                                + "it can be issued to continue using method proceed()");
                return;
            }

            // check initial TimeInstant parameter
            if (initTime != null)
            {
                clientScheduler.getSimClock().setInitTime(initTime);
                if (!TimeInstant.isEqual(initTime, new TimeInstant(0)))
                {
                    _client.reset();
                }
            }
            else
            {
                clientScheduler.getSimClock().setTime(new TimeInstant(0));
                sendWarning(
                        "Invalid start time parameter given! Start time set to "
                                + clientScheduler.presentTime() + ".",
                        "Experiment: '" + getName()
                                + "', Method: void start(SimTime initTime)",
                        "A null calue or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstnat object, otherwise use method "
                                + "start() without TimeInstant parameter.");
            }

            // client.init(); already done in connectToExperiment
            _client.doInitialSchedules();
            _client.doSubmodelSchedules();
            TimeOperations.setStartTime(initTime);
            var simRunReporterProvider = new SimulationRunReporterProvider(_client);
            simRunReporterProvider.reset();
            _client.register(simRunReporterProvider);
            // now everything is set up, go on and process events
            _status = STARTED;
            _realTimeStartTime = (long)(System.Diagnostics.Stopwatch.GetTimestamp()
                * (1_000_000_000.0 / System.Diagnostics.Stopwatch.Frequency));

            proceed();
        }

        /// <param name="initTime">The starting time instant</param>
        /// <remarks>
        /// Use start(TimeInstant initTime). Starts the experiment with the given simulation time as starting
        /// time. The experiment will not start unless a valid model has been connected to it before. Note
        /// that in order to stop the simulation at some point of time, the <c>stop</c> method has to be
        /// called first. <c>StopCondition</c> s can be given alternatively.
        /// </remarks>
        public void start(SimTime initTime)
        {
            start(SimTime.toTimeInstant(initTime));
        }

        /// <summary>
        /// Specifies a ModelCondition to stop the simulation. Note that this methode can be called muliple
        /// times, defining alternative conditions to terminate the simulation. Once at least one of the
        /// conditions passed using this method returns true, the experiment will stop. Beware that the
        /// simulation will run endlessly if none of the conditions are met; thus it is recommended to
        /// additionally always use a time limit if none of the conditions in question can be proven to be
        /// met during the run of the simulation!
        /// </summary>
        /// <param name="stopCond">
        /// A condition to stop the simulation once it's check() methode returns true.
        /// </param>
        public void stop(ModelCondition stopCond)
        {

            if (stopCond == null)
            {
                sendWarning("Can not set stop-condition! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'stop(Condition stopCond)'",
                        "The parameter passed was either null or a not initialized "
                                + "Condition reference.",
                        "Make sure to provide a valid stop Condition for "
                                + "this experiment.");
            }
            else
            {
                this._stopConditions.Add(stopCond);
            }

        }

        //@Deprecated
        /// <param name="stopCond">
        /// A condition to stop the simulation once it's check() methode returns true.
        /// </param>
        /// <remarks>
        /// Replaced by <c>stop(ModelCondition stopCond)</c> Specifies a Condition to stop the simulation.
        /// </remarks>
        public void stop(Condition<Entity> stopCond)
        {

            if (stopCond == null)
            {
                sendWarning("Can not set stop-condition! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'stop(Condition stopCond)'",
                        "The parameter passed was either null or a not initialized "
                                + "Condition reference.",
                        "Make sure to provide a valid stop Condition for "
                                + "this experiment.");
            }
            else
            {
                var condition = new ModelCondition.ConditionWrapper<Entity>(getModel(), stopCond);
                this.stop(condition);
            }

        }



        /// <summary>
        /// Stops the simulation at the given point of simulation time. If no valid simulation time is given,
        /// the default is 0 which would not let the simulation run past that time. Repeatedly calling this
        /// method will override stop times specified before.
        /// </summary>
        /// <param name="stopTime">The point of simulation time to stop the simulation</param>
        public void stop(TimeInstant stopTime)
        {
            if (stopTime == null)
            {
                sendWarning(
                        "Can not set stop-time! The stop-time will be set to 0",
                        "Experiment '" + getName()
                                + "', Method: 'stop(TimeInstant stopTime)'",
                        "The parameter passed was either null or a not initialized "
                                + "TimeInstance reference.",
                        "Pass an initialized TimeInstant object as stop time.");

                ExternalEventStop stopper = new ExternalEventStop(_client,
                        "Simulation stopped", true);
                stopper.schedule(new TimeInstant(0));

            }
            else
            {

                this._stopTime = stopTime;
                if (this._stopTimeEvent != null) this._stopTimeEvent.cancel();

                this._stopTimeEvent = new ExternalEventStop(_client, "Simulation stopped", true);
                _stopTimeEvent.schedule(stopTime);
            }
        }

        /// <param name="stopTime">The point of simulation time to stop the simulation</param>
        /// <remarks>
        /// Stops the simulation at the given point of simulation time. If no valid simulation time is given,
        /// the default is 0.0 which would not let the simulation run past that time.
        /// </remarks>
        //@Deprecated
        public void stop(SimTime stopTime)
        {
            stop(SimTime.toTimeInstant(stopTime));
        }

        /**
         * Stops the simulation at the current simulation time (immediately). A
         * stopped Simulation run can be resumed by calling proceed().
         */
        public void stop()
        {
            setStatus(STOPPED);
            clientScheduler.signalStop();
        }

        /// <summary>
        /// Returns a boolean indicating whether trace notes are forwarded to the trace ouput or not. Trace
        /// ouput can be switched on and off using the methods <c>traceOn(TimeInstant startTime)</c> and
        /// <c>traceOff(TimeInstant stopTime)</c>
        /// </summary>
        /// <returns>Is <c>true</c></returns>
        public bool traceIsOn()
        {

            return _messMan.isOn(tracenote);

        }

        /// <summary>Switches the trace output off at the given point of simulation time.</summary>
        /// <param name="stopTime">The point in simulation time to switch trace off</param>
        public void traceOff(TimeInstant stopTime)
        {

            // check initial TimeInstant parameter
            if (stopTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for trace output given! "
                                + "Trace output is set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'traceOn(TimeInstant startTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object, otherwise use method "
                                + "start() without TimeInstant parameter.");
                stopTime = clientScheduler.presentTime();
            }

            // check if parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), stopTime))
            {
                sendWarning("Invalid start time parameter for trace output given! "
                        + "Trace output is set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'traceOn(TimeInstant stopTime)'",
                        "The stopTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                stopTime = clientScheduler.presentTime();
            }

            ExternalEvent traceOff = new ExternalEventTraceOff(_client, true);
            traceOff.schedule(stopTime);
        }

        /// <param name="stopTime">The point in simulation time to switch trace off</param>
        /// <remarks>
        /// Use traceOff(TimeInstant startTime). Switches the trace output off at the given point of
        /// simulation time.
        /// </remarks>
        public void traceOff(SimTime stopTime)
        {
            this.traceOff(SimTime.toTimeInstant(stopTime));
        }

        /// <summary>Switches the trace output on at the given point of simulation time.</summary>
        /// <param name="startTime">The point in simulation time to switch trace on</param>
        public void traceOn(TimeInstant startTime)
        {

            // check initial TimeInstant parameter
            if (startTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for trace output given! "
                                + "Trace output is set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'traceOn(TimeInstant startTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object, otherwise use method "
                                + "start() without TimeInstant parameter.");
                startTime = clientScheduler.presentTime();
            }

            // check if parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), startTime))
            {
                sendWarning("Invalid start time parameter for trace output given! "
                        + "Trace output is set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'traceOn(TimeInstant startTime)'",
                        "The startTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                startTime = clientScheduler.presentTime();
            }

            // if parameter equals current time, set trace on immediately, e.g. 
            // to include initial scheduling
            if (TimeInstant.isEqual(clientScheduler.presentTime(), startTime))
            {
                this.getMessageManager().switchOn(Experiment.tracenote);
                _client.sendTraceNote("Trace switched on");
                // Otherwise schedule an appropriate event    
            }
            else
            {
                ExternalEvent traceOn = new ExternalEventTraceOn(_client, true);
                traceOn.schedule(startTime);
            }

        }

        /// <param name="startTime">The point in simulation time to switch trace on</param>
        /// <remarks>
        /// Use traceOn(TimeInstant startTime). Switches the trace output on at the given point of simulation
        /// time.
        /// </remarks>
        public void traceOn(SimTime startTime)
        {
            this.traceOn(SimTime.toTimeInstant(startTime));
        }

        /// <summary>
        /// Switches the trace output on for the given period of simulation time. If the second parameter
        /// (off) is "sooner" then the first parameter (on), they will be swapped automatically. Same
        /// parameters will result in no trace output at all.
        /// </summary>
        /// <param name="startTime">The point in simulation time to switch trace on</param>
        /// <param name="stopTime">The point in simulation time to switch trace off</param>
        public void tracePeriod(TimeInstant startTime, TimeInstant stopTime)
        {
            if (startTime == null)
            {
                sendWarning(
                        "Invalid start time parameter for trace output given! Command ignored",
                        "Experiment '" + getName()
                                + "', Method 'tracePeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object.");
                return;
            }

            // check initial TimeInstant parameter
            if (stopTime == null)
            {
                sendWarning(
                        "Invalid stop time parameter for trace output given! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'tracePeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "A null value or a not initialized TimeInstant reference has been passed.",
                        "Make sure to have a valid TimeInstant object.");
                return;
            }

            // check for correct order in parameters
            if (TimeInstant.isAfter(startTime, stopTime))
            {

                // swap parameters
                TimeInstant buffer = stopTime;
                stopTime = startTime;
                startTime = buffer;

            }

            // check if stop parameter is in future
            if (TimeInstant.isAfter(clientScheduler.presentTime(), stopTime))
            {
                sendWarning(
                        "Invalid stop time parameter for trace output given! Command ignored.",
                        "Experiment '" + getName()
                                + "', Method 'tracePeriod(TimeInstant startTime, "
                                + "TimeInstant stopTime)'",
                        "The stopTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                return;
            }

            // check if start parameter is in past
            if (TimeInstant.isAfter(clientScheduler.presentTime(), startTime))
            {
                sendWarning("Invalid start time parameter for trace output given! "
                        + "Trace output has been set to start immediately.",
                        "Experiment '" + getName()
                                + "', Method 'tracePeriod(TimeInstant startTime, "
                                + "TimeInstant startTime)'",
                        "The startTime given is in the past.",
                        "Make sure to give a TimeInstant parameter larger than the current time.");
                startTime = clientScheduler.presentTime();
            }

            // set trace to switch on
            traceOn(startTime);

            // set trace to switch off
            traceOff(stopTime);
        }

        /// <param name="startTime">The point in simulation time to switch trace on</param>
        /// <param name="stopTime">The point in simulation time to switch trace off</param>
        /// <remarks>
        /// Replaced by tracePeriod(TimeInstant startTime, TimeInstant stopTime). Switches the trace output
        /// on for the given period of simulation time. If the second parameter (off) is "sooner" then the
        /// first parameter (on), they will be swapped automatically. Same parameters will result in no trace
        /// output at all.
        /// </remarks>
        //@Deprecated
        public void tracePeriod(SimTime startTime, SimTime stopTime)
        {
            tracePeriod(SimTime.toTimeInstant(startTime), SimTime
                    .toTimeInstant(stopTime));
        }

        /// <summary>
        /// Triggers the reporters of the given model or submodel to write their report data into the report
        /// output registered at the experiment's messagemanager. The string given will be added as a suffix
        /// to the report filename to help identify teh report when more than one report is produced by one
        /// Experiment at differnet points of simulation time.
        /// </summary>
        /// <param name="m">Model</param>
        /// <param name="suffix">Suffix for report filename if multiple reports are drawn</param>
        public void writeReport(Model m, String suffix)
        {

            if (suffix == null)
                suffix = "";

            // buffer used for storing the filename in
            string nameBuffer = null;

            // now flush and close all files and reopen with new names
            foreach (FileOutput f in _registryFileOutput)
            {

                // remember the name the file had
                nameBuffer = f.getFileName();

                // flush buffer and close file
                f.close();

                // open new file with old name stripping off old suffix and
                // adding new suffix
                nameBuffer = nameBuffer.Substring(0, nameBuffer.LastIndexOf("."));
                f.open(nameBuffer.Substring(0, (nameBuffer.Length - lastSuffix))
                        + suffix + "html");
            }
            lastSuffix = suffix.Length; // remember last suffix length
            report(m);
        }

        /// <summary>
        /// Triggers the reporters to write their data into the report output registered at the experiment's
        /// messagemanager. The string given will be added as a suffix to the report filename to help
        /// identification when more than one report is produced by one Experiment at differnet points of
        /// simulation time.
        /// </summary>
        /// <param name="suffix">Suffix for report filename if multiple reports are drawn</param>
        public void writeReport(string suffix)
        {

            if (suffix == null)
                suffix = "";

            // write report data about main model
            report(_client);

            // buffer used for storing the filename in
            string nameBuffer = null;

            // now flush and close all files and reopen with new names
            foreach (FileOutput f in _registryFileOutput)
            {

                // remember the name the file had
                nameBuffer = f.getFileName();

                // flush buffer and close file
                f.close();

                // open new file with old name stripping off old suffix and
                // adding new suffix
                // strip the _debug / _error / _trace / _report part
                nameBuffer = nameBuffer.Substring(0, nameBuffer.LastIndexOf("_"));
                // strip the previous suffix
                nameBuffer = nameBuffer.Substring(0, nameBuffer.Length
                        - lastSuffix);
                // add new suffix
                nameBuffer = nameBuffer + suffix;
                // now open file with new name
                f.open(nameBuffer);
            }
            lastSuffix = suffix.Length; // remember last suffix length
        }

        /// <summary>Returns the current DESMO-J version</summary>
        /// <returns>The string "2.4.1".</returns>
        public static string getDesmoJVersion()
        {
            return "2.4.1";
        }

        /// <summary>Returns the DESMO-J license</summary>
        /// <param name="html">Include link (HTML, true) or not (plain text, false)</param>
        /// <returns>
        /// The string "Apache License, Version 2.0", embedded in a HTML link tag (currently
        /// http://www.apache.org/licenses/LICENSE-2.0) if <c>html</c> is set true.
        /// </returns>
        public static string getDesmoJLicense(bool html)
        {
            return html ? "<A HREF=http://www.apache.org/licenses/LICENSE-2.0>Apache License, Version 2.0</A>"
                    : "Apache License, Version 2.0";
        }

        public void setDescription(string description)
        {
            this._description = description;
        }

        public string getDescription()
        {
            return _description;
        }
    }

}