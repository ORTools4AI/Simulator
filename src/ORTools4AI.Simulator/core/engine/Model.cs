// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.


using ORTools4AI.Simulator.core.ai;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * The model is supposed to carry references to all static modelcomponents
     * associated to a model. These are modelcomponents like distributions and
     * statistic counters. Note that placing all essential components inside the
     * model class allows you to use object orientation to reuse this model to
     * implement different versions of it using subclassing for changes necessary
     * for using other modelling paradigms. This way models can be easily
     * reimplemented following the process-, event-, transaction- or activity-
     * oriented paradigm. Note that transaction- and activity-oriented modelling
     * requires the higher process synchronization mechanisms (Res, Bin, WaitQueue,
     * CondQueue) by Soenke Claassen.
     * <p>
     * The modelcomponents belonging to a model have to be instantiated in the
     * <code>init()</code> method. They must not be instantiated in the model's
     * constructor since most components need access to a valid experiment object.
     * This can only be accessed after a model has been connected to an experiment
     * using method <code>connectToExperiment(Experiment exp)</code> which
     * implicitly calls the <code>init()</code> method after receiving the valid
     * reference to an experiment.
     */



    public abstract class Model : Reportable
    {

        /**
         * The experiment this model is connected to.
         */
        private Experiment _myExperiment;

        /**
         * The map with all entities and their identifiers.
         */
        private Dictionary<long,Entity> _entityMap;

        /**
         * True if this is the main model (not a submodel to another model), false 
         * for submodels.
         */
        //bool isMainModel;

        /**
         * The current number of the created entities.
         */
        private long _numberOfEntitiesCreated;

        /**
         * Dynamic List containing all associated submodels.
         */
        private List<Model> _subModels;

        /**
         * Dynamic List containing all reportable components of this model.
         */
        private List<Reportable> _reportables;

        /**
         * The parameter manager
         */
        private ModelParameterManager _paramManager;

        // AI episode listeners registered by AiSimProcess instances.
        private readonly List<IAiEpisodeListener> _aiListeners = [];

        /// <summary>
        /// Constructs a model, with the given name and parameters for report and trace files.
        /// </summary>
        /// <param name="owner">The main model this model is associated to</param>
        /// <param name="name">The name of this model</param>
        /// <param name="showInTrace">
        /// Flag for showing this model in trace-files. Set it to <c>true</c> if model should show up in
        /// trace, <c>false</c> if model should not be shown in trace.
        /// </param>
        public Model(Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {

            //super(owner, name, showInReport, showInTrace); // create a reportable

            // init data structures
            _subModels = new List<Model>();// create empty Vector for subModels
            _reportables = new List<Reportable>(); // create empty Vector for reportables

            if (owner != null)
            { // check if this is a submodel
                this.isMainModel = false;
                owner.registerSubModel(this);
                // register as submodel at owner model
            }
            else
            {
                this.isMainModel = true;
            }

            _paramManager = new ParameterManager(); // create ParameterManager

            _numberOfEntitiesCreated = 0L;

            _entityMap = new Dictionary<long, Entity>();
        }

        /// <summary>
        /// Checks the given modelcomponent for compatibility. Needed exclusively when submodels are present
        /// that want to share modelcomponents between them. The standard implementation only checks if the
        /// modelcomponent <c>other</c> belongs to <c>this</c> model. For use with multiple models, the user
        /// has to specify which other models are compatible to this model by overloading this method.
        /// </summary>
        /// <param name="other">
        /// The modelcomponent that needs to be checked for compatibility to this model.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the given modelcomponent is compatible to this model, <c>false</c>
        /// otherwise.
        /// </returns>
        public bool checkCompatibility(ModelComponent other)
        {

            return (this == other.getModel()); // check for same owner

        }

        /**
         * Connects this model to an experiment. The given experiment must not
         * already be connected to some other model. Otherwise an errormessage will
         * be given and the experiment will be stopped. Since a model that is not
         * yet connected has no access to the experiment's messaging subsystem,
         */
        public void connectToExperiment(Experiment exp)
        {

            if (exp == null)
            {
                sendWarning("Can not connect to a <code>null</code> Experiment! "
                        + "Command ignored.", "Model : " + getName()
                        + " Method: void connectToExperiment" + "(Experiment exp)",
                        "The experiment reference passed is a null reference!",
                        "Make sure to use valid references only.");
                return; // no connection possible.
            }

            if (!isMainModel)
            { // only main model should register at experiment
                sendWarning("Can not connect to Experiment " + exp.getName() + "! "
                        + "Command ignored.", "Model : " + getName()
                        + " Method: void connectToExperiment" + "(Experiment exp)",
                        "The model to be connected is already submodel to model : "
                                + getModel().getName(),
                        "Only ,main models are allowed to be connected to an "
                                + "experiment");
                return; // model is no main model
            }

            if (exp.isConnected())
            {
                sendWarning("Can not connect to Experiment " + exp.getName() + "! "
                        + "Command ignored.", "Model : " + getName()
                        + " Method: void connectToExperiment" + "(Experiment exp)",
                        "The experiment to connect to is itself already connected "
                                + "to model : " + exp.getModel(),
                        "Only one main model can be connected to an experiment at "
                                + "a time.");
                return; // no connection possible.
            }

            if (_myExperiment != null)
            {
                sendWarning("Can not connect to Experiment " + exp.getName() + "! "
                        + "Command ignored.", "Model : " + getName()
                        + " Method: void connectToExperiment" + "(Experiment exp)",
                        "The model is already connected to Experiment : "
                                + _myExperiment.getName() + "!",
                        "A model can only be connected to just one Experiment.");
                return;
            }

            // set the experiment reference
            _myExperiment = exp;

            // register at Experiment
            _myExperiment.registerModel(this);

            // set the Experiment parameter at all submodels if necessary
            if (_subModels.Count >0)
            {

                foreach (Model m in _subModels)
                {
                    m.setConnectedExperiment(exp);
                }

            }

            // initialize the components of the main model
            init();
            // initialize the submodels recursively
            doSubmodelInit();
            // reset the model and all submodels recursively
            reset();

        }

        /// <summary>
        /// Creates the default reporter associated with this model. The default reporter returned by this
        /// method just prints the model's description as implemented in method <c>description()</c> and
        /// appends the simulation time of the last reset. If the user's model offers more information,
        /// create a special class derived from class <c>ModelReporter</c> to extract and present that extra
        /// information.
        /// </summary>
        /// <returns>The reporter associated with this model</returns>
        public ModelReporter createReporter()
        {

            // has to be the special reporter designed for this model
            return new ModelReporter(this);

        }

        /// <summary>
        /// Should return the description of the model. Implement this method to document the model's intent,
        /// creator, date and all other information that needs to be published about this model in a report.
        /// Please use it generously, other people interested in your model will thank you.
        /// </summary>
        /// <returns>The description of the model as a text String</returns>
        public abstract string description();

        /// <summary>
        /// Implement this method to schedule the entities and/or events needed for your model to start in a
        /// defined setting. If a model is connected to an experiment and started without any initial
        /// schedules, it will not run since no events to process are on the scheduler's event-list. This
        /// method is called automatically by the framework when the experiment's <c>start()</c> method is
        /// called.
        /// </summary>
        public abstract void doInitialSchedules();

        /// <summary>
        /// Calls the <c>init()</c> method of all registered submodels of the current model. Subsequently
        /// calls the <c>init()</c> method of all registered submodels of every submodel.
        /// </summary>
        void doSubmodelInit()
        {

            if (!this.hasSubModels())
                return; // no submodels registered here
            else
            {

                foreach (Model m in _subModels)
                { // loop submodels
                    m.init(); // init submodel
                    m.doSubmodelInit(); // init submodels of submodels
                }
            }
        }

        /// <summary>
        /// Calls the <c>doInitialSchedules()</c> method of all registered submodels of the current model.
        /// Subsequently calls the <c>doSubmodelSchedules()</c> method of all registered submodels of every
        /// submodel.
        /// </summary>
       public void doSubmodelSchedules()
        {

            if (!this.hasSubModels())
                return; // no submodels registered here
            else
            {

                foreach (Model m in _subModels)
                { // loop submodels
                    m.doInitialSchedules(); // initial schedules for the submodel
                    m.doSubmodelSchedules(); // initial schedules for potential submodels of the submodel
                }

            }
        }

        /// <summary>Returns the entity to the belonging identifier.</summary>
        /// <returns>The class will return the <c>Entity</c> or null if no such exists.</returns>
        public Entity getEntity(long ident)
        {
            return (Entity)_entityMap[ident];
        }

        /// <summary>Returns a list of this model's entities.</summary>
        /// <param name="includeSubmodels">
        /// Do include (true) or exclude (false) submodels' entities in the list.
        /// </param>
        /// <returns>
        /// The class will return a list of all existing objects of <c>Entity</c> or null if no list exists.
        /// </returns>
        public List<Entity> getEntities(bool includeSubmodels)
        {
            var list = new List<Entity>();
            list.AddRange(_entityMap.Values);

            if (includeSubmodels)
            {
                foreach (Model submodel in this._subModels)
                {
                    list.AddRange(submodel.getEntities(true));
                }
            }

            return list;
        }

        /// <summary>
        /// Returns the experiment that is connected to this model or <c>null</c> if the model is not yet
        /// connected to an experiment.
        /// </summary>
        /// <returns>
        /// The experiment that this model is connected to or <c>null</c> if no connection is established
        /// </returns>
        public Experiment getExperiment()
        {

            if (this.isMainModel)
            {

                return _myExperiment;

            }
            else
            {

                return this.getModel().getExperiment();

            }
        }

        /// <summary>
        /// Returns a copy of the dynamic list containing all reportable components of this model.
        /// </summary>
        /// <returns>A list containing all reportable components of this model.</returns>
        public List<Reportable> getReportables()
        {
            return new List<Reportable>(this._reportables);
        }

        /// <summary>Returns the Model's viewpoint of the ParameterManager</summary>
        /// <returns>The ParameterManager</returns>
        public ModelParameterManager getParameterManager()
        {
            return _paramManager;
        }

        /// <summary>Shows, if there are submodels registered with this model.</summary>
        /// <returns>
        /// True, if there is at least one submodel registered at this Model, flase if there are no submodels
        /// </returns>
        public bool hasSubModels()
        {

            return _subModels.Count >0; // checks for some submodels in vector

        }

        /// <summary>
        /// Implement this method to do initialization work for your model. All ModelComponents associated
        /// with this model are supposed to be instantiated in this method. Caution! Do not use the Model's
        /// constructor method to instantiate your modelcomponents! This prevents the modelcomponents to get
        /// contact to the associated Experiment since a Model can only be connected to an Experiment after
        /// itself has been instantiated correctly. Thus it is necessary to put all initialization and
        /// instantiation work into this method. Do not use this method to do the initial schedules needed
        /// for the scheduler to start with. Use method <c>doInitialSchedules()</c> instead. If there are no
        /// events scheduled in the event-list, the scheduler will stop the simulation immediately because
        /// there is nothing for him to do. If you are using submodels, take care that each of the submodels
        /// has its own <c>init</c> method properly set up.
        /// </summary>
        public abstract void init();

        /// <summary>
        /// Initializes the model by calling method <c>init()</c> to set up all model related components as
        /// specified in that method. After initializing the model, the given ModelOption's
        /// <c>setOption()</c> method is called with this model as parameter. This enables the user to
        /// specify different model parameter settings for different experiments. It even allows automated
        /// parameter variation to give a small but handy aid for model optimization. Implement the
        /// ModelOptions to set the parameters for your model. Make sure to react properly on the
        /// ModelOptions given. Note that specifiying ModelOptions is up to the user.
        /// </summary>
        /// <param name="options">The parameter settings for this model</param>
        public void init(ModelOptions options)
        {

            this.init(); // just call the other init after dealing with the
                         // options
            options.setOptions(this);

        }

        /// <summary>Shows if this model has already been connected to an experiment.</summary>
        /// <returns>Is <c>true</c>, if model is connected to an experiment, <c>false</c> otherwise</returns>
        public bool isConnected()
        {

            return (_myExperiment != null); // no experiment connected

        }

        /**
         * Shows if this model is a main model and thus is not submodel of other
         * models.
         * 
         * @return boolean : Returns <code>true</code> if the model is a main
         *         model, <code>false</code> if it is a submodel of other models
         */
        public bool isMainModel { get; set; }
        

        /// <summary>
        /// Shows if this model is submodel to another model and thus is not the main models.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the model is a submodel of other models, <c>false</c> if it is a main
        /// model
        /// </returns>
        public bool isSubModel()
        {

            return !this.isMainModel;
        }

        /// <summary>Links an entity with an unique identification number.</summary>
        /// <param name="e">The new Entity to link with an unique identification number.</param>
        /// <returns>Unique identification number.</returns>
        public long linkWithIdentNumber(Entity e)
        {
            _numberOfEntitiesCreated++;
            _entityMap.Add(_numberOfEntitiesCreated, e);
            return _numberOfEntitiesCreated;
        }

        /// <summary>
        /// Registers a reportable at this model. Reports can only be drawn, if the reportbale is registered
        /// at his model.
        /// </summary>
        /// <param name="r">The reportable to be registered</param>
        public void register(Reportable r)
        {

            if (r == null)
                return; // do not accept null values
            else
                _reportables.Add(r);

        }

        /// <summary>Registers another model as submodel of this model.</summary>
        /// <param name="subModel">The model to be registerd as a submodel</param>
        void registerSubModel(Model subModel)
        {

            if (subModel == null)
            {
                sendWarning("Can't register Submodel!", "Model : " + getName()
                        + " Method: registerSubModel(Model " + "subModel)",
                        "The SubModel given as parameter is a null reference.",
                        "Be sure to have a valid SubModel reference before calling "
                                + "this method.");
                return; // exit
            }

            _subModels.Add(subModel);

        }

        /// <summary>
        /// Produces a report about the model and all its related modelcomponents and submodels. It registers
        /// the model's reporter and all other reporters at the experiment's reportmanager to produce the
        /// report output. The reporters are ordered into the reportmanager returned with this model's
        /// reporter first, followed by all reportable's reporters. After this section, all submodel's
        /// reporters are inserted according to the order they were attached to this model. Each submodel's
        /// section is sorted using the criteria above. Thus all reporters a re separated by model to prevent
        /// confusion.
        /// </summary>
        /// <returns>The Enumeration carrying all reporters available from this model</returns>
        public List<Reporter> report()
        {

            // create the reportmanager necessary for sorting all reporters
            ReportManager repMan = new ReportManager(getName() + "_ReportManager");

            // register this model's reporter
            repMan.register(createReporter());

            // register all modelcomponent's reporters
            // in case they are set to produce a report output
            foreach (var r in _reportables)
            { // loop reportables
                if (r.reportIsOn())
                    repMan.register(r.getReporter());
            }

            // register all submodel's reporters
            List<Reporter> subReporters;
            // buffer for submodel-reportmanagers returned

            foreach (Model m in _subModels)
            { // loop submodels

                subReporters = m.report();

                // add all reporters from the ordered enumeration at last position
                // of repMan
                foreach (Reporter r in subReporters)
                {
                    repMan.addLast(r);
                }
            }

            return repMan.elements();

        }

        /// <summary>
        /// Resets the statistic counters of this model and of all its registered reportables. Also calls all
        /// submodels's <c>reset()</c> methods.
        /// </summary>
        public void reset()
        {

            // reset this model
            base.reset(); // reset the own obs & resetAt variables

            // reset all reportables registered here
            if (_reportables.Count==0)
                return; // no reportables registered
            else
            {

                foreach (Reportable r in _reportables)
                { // loop reportables
                    r.reset();
                    // reset each registered
                }

            }

            // reset all registered submodels
            if (_subModels.Count==0)
                return; // no reportables registered
            else
            {

                foreach (Model m in _subModels)
                { // loop submodels
                    m.reset();
                    // reset each registered
                }

            }

        }

        /// <summary>
        /// Connects submodels to the experiment given. This method is called by the main model, when
        /// submodels register themselves at the main model.
        /// </summary>
        /// <param name="e">The experiment to connect this model to</param>
        void setConnectedExperiment(Experiment e)
        {

            if (e == null)
            {
                sendWarning(
                        "Can not connect to experiment! Command ignored.",
                        "Model : " + getName() + " Method: "
                                + "void setConnectedExperiment (Experiment e)",
                        "The given Esperiment parameter contained a null reference",
                        "Be sure to always give valid parameters.");
                return; // do nothing
            }

            if (isSubModel())
            { // this is a submodel
                _myExperiment = e;
                return;
            }
            else
            { // this is no submodel
                sendWarning("Can not connect to experiment! Command ignored.",
                        "Model : " + getName() + " Method: void "
                                + "method setConnectedExperiment"
                                + "(Experiment e)",
                        "This model is not a submodel.",
                        "Only submodels can be connected to an experiment using "
                                + "this method.");
                return; // do nothing
            }

        }

        /// <summary>
        /// Sets the current model to be the main model by setting the <c>owner</c> reference to itself.
        /// </summary>
        public void setMain()
        {

            setOwner(this);

        }

        // ── Distribution factory helpers ──────────────────────────────────────────

        protected ContDistExponential Exponential(double mean) =>
            new(this, "Exp", mean, false, false);

        protected ContDistUniform Uniform(double lo, double hi) =>
            new(this, "Unif", lo, hi, false, false);

        protected ContDistNormal Normal(double mean, double std) =>
            new(this, "Norm", mean, std, false, false);

        protected ContDistTriangular Triangular(double lo, double mid, double hi) =>
            new(this, "Tri", lo, mid, hi, false, false);

        // ── Fluent run helper ─────────────────────────────────────────────────────

        /// <summary>
        /// Connects this model to a fresh <see cref="Experiment"/>, runs it to
        /// <paramref name="duration"/> <paramref name="unit"/>s, then finishes.
        /// Suitable for tests and quick scripts; use the full Experiment API for
        /// production runs that need trace output or custom stop conditions.
        /// </summary>
        public void Run(double duration, TimeUnit unit = null, long seed = 42)
        {
            unit ??= TimeUnit.SECONDS;
            var exp = new Experiment(getName(), unit, unit, null);
            exp.setSeedGenerator(seed);
            exp.setSilent(true);
            connectToExperiment(exp);
            exp.stop(new TimeInstant(duration, unit));
            exp.start();
            exp.finish();
            NotifyEpisodeEnd();
        }

        /// <summary>Called by <see cref="AiSimProcess{TObs,TAction}"/> to register
        /// itself for truncation notification when <see cref="Run"/> ends.</summary>
        internal void RegisterAiProcess(IAiEpisodeListener listener) =>
            _aiListeners.Add(listener);

        private void NotifyEpisodeEnd()
        {
            foreach (var l in _aiListeners) l.NotifyTruncated();
            _aiListeners.Clear();
        }

    }
}