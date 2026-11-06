// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, modified by Chr. M&uuml;ller (TH Wildau) 28.11.2012
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;


namespace ORTools4AI.Simulator.core.engine
{

    /**
     * All classes that want to publish their information have to extend this class
     * in order to provide the necessary functions to represent their information in
     * reports.
     */
    public abstract class Reportable : ModelComponent {

        /**
         * Flag indicating if this reportable should produce a report.
         */
        private bool _reportMode;

        /**
         * The number of observations that this reportable can report about
         */
        private long _observations;

        /**
         * stores the last time that this reportable object was reset.
         */
        private TimeInstant _lastReset;

        /**
         * schedulable (e.g. entity) corresponding to this reportable
         * maybe null if no schedulable corresponds to this reportable  
         */
        private Schedulable _correspondingSchedulable = null;

        /**
         * A description (optional) / 21.11.12 extended bx cm
         */
        private string description;

        /**
         * A type to instantiate a custom reporter from
         */
        private Type _reporter = null;

        /**
         * This Reportable's Reporter
         */
        private Reporter _myReporter = null;

        /// <summary>
        /// Creates a reportable object with all parameters required. The reportable registers itself at the
        /// given model
        /// </summary>
        /// <param name="owner">The model this reportable is associated to</param>
        /// <param name="name">The name of this reportable</param>
        /// <param name="showInReport">
        /// Flag for showing the report Set it to <c>true</c> if reportable should show up in report. Set it
        /// to <c>false</c> if reportable should not be shown in report.
        /// </param>
        /// <param name="showInTrace">
        /// Flag for showing this reportable in trace files. Set it to <c>true</c> if reportable should show
        /// up in trace. Set it to <c>false</c> if reportable should not be shown in trace.
        /// </param>
        public Reportable(Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace); // create the ModelComponent
            _reportMode = showInReport; // set report flag
            _observations = 0; // reset observations counter

            // Registration even if showInReport=false! 
            if ((owner != null)) {
                owner.register(this);
            }

            // register current time as reset
            // in case experiment running
            if (owner != null && owner.getExperiment() != null && owner.getExperiment().isRunning()) {
                _lastReset = presentTime();
            }
        }

        /// <summary>This method provides a reporter to generate report data about this reportable.</summary>
        /// <returns>A specific reporter for this reportable</returns>
        /// <remarks>Use <c>createDefaultReporter()</c> instead.</remarks>
        //@Deprecated
        public Reporter createReporter() {

            return createDefaultReporter();

        }

        /// <summary>
        /// This method provides a reporter to generate report data about this reportable. In this general
        /// implementation, a <c>StandardReporter</c> is returned. Lacking further knowledge about a specific
        /// reportable, the <c>StandardReporter</c> is only able to read out the reportable's name, number of
        /// observations and time of last reset. Thus, it is highly recommended that subclasses override this
        /// method to return an appropriate default reporter for the specific reportable object. All
        /// reportable objects in DESMO-J like <c>Accumulate</c>, <c>Tally</c>, <c>Count</c>, <c>Queue</c>,
        /// and many others provide such reporters. If you want to extend the framework with new reportable
        /// objects, provide a suitable reporter as well and override this method to return an instance of
        /// the reporter. If you just need to replace the reporter used by an existing reportable (while not
        /// requiring any changes to the reportable itself), there is no need to create a subclass of the
        /// reportable: Instead of overriding this method, just call <c>setReporter()</c> to have this
        /// reportable use a custom reporter instead of the default reporter.
        /// </summary>
        /// <returns>A specific reporter for this reportable</returns>
        protected virtual Reporter createDefaultReporter() {

            return new StandardReporter(this);
        }

        /// <summary>
        /// Returns an instance of a reporter for this reportable. Note that every reportable has to provide
        /// a default reporter, see method <c>createDefaultReporter()</c>. The reporter returned by this
        /// method is such a default reporter, unless the user has called <c>setReporter()</c> to specify a
        /// custom reporter class to use to instantiate a Reporter from instead of the default reporter.
        /// </summary>
        /// <returns>A reporter for this reportable</returns>
        public Reporter getReporter() {

            if (_myReporter != null)
                return _myReporter;

            if (_reporter == null)
            {
                _myReporter = createDefaultReporter();

                // createDefaultReporter() is probably not overridden. Try the old
                // createReporter() method for compatibility issues.
                //if (_myReporter.getClass().equals(StandardReporter.class))
                //    _myReporter = createReporter();

                return _myReporter;
            }

            try
            {
                _myReporter = (Reporter)Activator.CreateInstance(_reporter, this);
            }
            catch (Exception)
            {
                this.sendWarning(
                    "Instanciating the user-specified reporter for this reportable caused an exception. Using the default reporter instead.",
                    "Reportable : " + getName() + " Method: Reporter getReporter()",
                    "User-specified class is not accessible or constructor cannot be invoked.",
                    "Make sure to provide an appropriate reporter class. "
                       + "Such a reporter must have a constructor taking this reportable as its only parameter."
                    );
                _myReporter = createDefaultReporter();
            }

            return _myReporter;
        }

        /// <summary>
        /// Specifies a Reporter-Class to be used as reporter by this reportable. Note that such a Reporter
        /// has to provide a constructor requiring a reference to this object (i.e. the reportable to report
        /// about) as only parameter. If this method is never called, a default reporter as obtained from
        /// <c>createDefaultReporter()</c> will be used.
        /// </summary>
        /// <param name="reporterClass">the reporter's class</param>
        public void setReporter(Type reporterClass)
        {
            _reporter = reporterClass;
        }

        /// <summary>Returns the number of observations made by the reportable object.</summary>
        /// <returns>The number of observations made by the reportable object.</returns>
        public long getObservations() {

            return _observations;

        }

        /**
         * Increments the number of observations made by this reportable by one (1).
         */
        public void incrementObservations() {

            _observations++;

        }

        /// <summary>Increments the number of observations by the amount given as parameter.</summary>
        /// <param name="multiObservations">The number to increase the number of observations by</param>
        public void incrementObservations(long multiObservations) {

            _observations += multiObservations;

        }

        /// <summary>Checks if this reportable produces a report.</summary>
        /// <returns>true if report will be produced, false otherwise</returns>
        public bool reportIsOn() {

            return _reportMode;

        }

        /**
         * Switches report mode to prevent this reportable to produce reports.
         */
        public void reportOff() {

            _reportMode = false;

        }

        /**
         * Switches report mode of this reportable on to produce a report.
         */
        public void reportOn() {

            _reportMode = true;

        }

        /// <summary>
        /// Resets the counter for observations made by this reportable. The point of simulation time this
        /// method was called will be stored and can be retrieved using method <c>resetAt()</c>.
        /// </summary>
        public void reset() {

            _observations = 0; // reset observations
            _lastReset = presentTime(); // register the reset time
        }

        /// <summary>
        /// Shows the point in simulation time when the last reset of this reportable was made.
        /// </summary>
        /// <returns>The point of simulation time of the last reset.</returns>
        public TimeInstant resetAt() {

            return _lastReset;
        }

        /// <summary>
        /// Gets the schedulable (e.g. entity) corresponding to this reportable; may be null if no
        /// schedulable corresponds to this reportable.
        /// </summary>
        /// <returns>
        /// The schedulable (e.g. entity) corresponding to this reportable (may be null if no schedulable
        /// corresponds to this reportable!)
        /// </returns>
        public Schedulable getCorrespondingSchedulable() {
            return _correspondingSchedulable;
        }

        /// <summary>
        /// Sets the schedulable (e.g. entity) corresponding to this Reportable. May be null if no
        /// schedulable corresponds to this reportable. If set, the schedulable must have the same model as
        /// this reportable! A model may not have a corresponding schedulable.
        /// </summary>
        /// <param name="correspondingSchedulable">The Schedulable corresponding to this Reportable.</param>
        public void setCorrespondingSchedulable(Schedulable correspondingSchedulable) {

            if (GetType() == typeof(Model)) {
                this.sendWarning(
                   "A Model may not have a corresponding schedulable. Method call ignored.",
                   "Reportable.setCorrespondingSchedulable(Schedulable)",
                   "A Model may not have a corresponding schedulable, because Model contains many corresponding schedulables.",
                   "Do not set corresponding schedulable to a model!"
                );
                return;
            };

            if (correspondingSchedulable != null && this.getModel() != correspondingSchedulable.getModel()) {
                this.sendWarning(
                   "Schedulable to correspond to this Reportable must belong to the same model!",
                   "Reportable.setCorrespondingSchedulable(Schedulable)",
                   "Model of Reportable and corresponding schedulable must be identical.",
                   "Do not set a corresponding schedulable to another model's Schedulable.");
                return;
            }
            this._correspondingSchedulable = correspondingSchedulable;
        }

        //   Extension by Chr. M&uuml;ller (TH Wildau) 28.11.12 -------------------------------------

        /// <summary>
        /// Set an optional description of reported value. Default is null. This value is shown in
        /// description reports, only when set.
        /// </summary>
        public void setDescription(string description) {
            this.description = description;
        }

        /// <summary>
        /// Get an optional description of reported value. Default is null. This value is shown in
        /// description reports, only when set.
        /// </summary>
        public string getDescription() {
            return this.description;
        }

    }
}

