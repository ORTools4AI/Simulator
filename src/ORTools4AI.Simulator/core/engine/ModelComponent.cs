// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Encapsulates all information relevant to each component of a model. Its basic
     * intention is to connect each modelcomponent to a single Model object as the
     * owner of this modelcomponent. Through this connection all relevant
     * information about that Model can be retrieved. It is part of the composite
     * design pattern as described in [Gamm97] page 163 in which it represents the
     * component class.
     */
	public class ModelComponent : NamedObject
	{

		/**
		 * The reference to the model that this modelcomponent belongs to.
		 */
		private Model _owner;

		/**
		 * Flag indicating if this modelcomponent should be listed in the trace
		 * output file.
		 */
		private bool _traceMode;

		/**
		 * Flag indicating if this modelcomponent should be listed in the debug
		 * output file.
		 */
		private bool _debugMode;

		/// <summary>
		/// Constructs a modelcomponent with the given String as name and the given model as the associated owner
		/// of this component. Components can only be created after the corresponding model object has been
		/// instantiated. The default preset for the showInTrace option is <c>false</c>.
		/// </summary>
		/// <param name="ownerModel">The model this component is associated to.</param>
		/// <param name="name">The name of the component</param>
		public ModelComponent(Model ownerModel, string name) : this(ownerModel, name, false)
		{

			// create a ModelComponent with the given name and no trace output
			//this(ownerModel, name, false);
		}

		/// <summary>
		/// Constructs a modelcomponent with the given String as name and the given model as the associated owner
		/// of this component. Components can thus only be created after the corresponding model object has been
		/// instantiated.
		/// </summary>
		/// <param name="ownerModel">The model this component is associated to</param>
		/// <param name="name">The name of the component</param>
		/// <param name="showInTrace">
		/// Flag for showing component in trace-files. Set it to <c>true</c> if component should show up in trace.
		/// Set it to <c>false</c> if component should not be shown in trace.
		/// </param>
		public ModelComponent(Model ownerModel, string name, bool showInTrace) : base(name)
		{

			//super(name); // create the namedObject with given name
			_owner = ownerModel; // set the owner of this component
			_traceMode = showInTrace; // set the tracemode for this component

		}

		/// <summary>Returns the currently active Schedulable object that is handled by the scheduler.</summary>
		/// <returns>The current Schedulable object.</returns>
		public Schedulable current()
		{

			return _owner.getExperiment().getScheduler().getCurrentSchedulable();

		}

		/// <summary>
		/// Returns the currently active Entity. Returns <c>null</c> if the current Schedulable happens to be an
		/// external event or a SimProcess. Note that in case the current Event refers to more than one entity
		/// (<c>EventTwoEntitties</c>, <c>EventThreeEntitties</c>), only the first entity is returned; to obtain
		/// all such entities, use <c>getAllCurrentEntities()</c> instead.
		/// </summary>
		/// <returns>
		/// The currently active Entity or <c>null</c> in case of an external event or a SimProcess being the
		/// currently active Schedulable
		/// </returns>
		public Entity currentEntity()
		{

			return _owner.getExperiment().getScheduler().getCurrentEntity();

		}

		/// <summary>
		/// Returns the currently active entities. Returns an empty list if the current Schedulable happens to be
		/// an external event or a SimProcess.
		/// </summary>
		/// <returns>A list containing the currently active entities</returns>
		public List<Entity> currentEntityAll()
		{

			return _owner.getExperiment().getScheduler().getAllCurrentEntities();

		}

		/// <summary>
		/// Returns the currently active Event that is handled by the scheduler. It returns <c>null</c> if a
		/// process Event is the current active Schedulable, thus no Event is active.
		/// </summary>
		/// <returns>
		/// The current active Event or <c>null</c> if the current active Schedulable is a process
		/// </returns>
		public EventAbstract currentEvent()
		{

			return _owner.getExperiment().getScheduler().getCurrentEvent();

		}

		/// <summary>
		/// Returns the model that the currently active Event or Entity handled by the scheduler belongs to or the
		/// main model connected to the experiment, if no model can be returned by the scheduler.
		/// </summary>
		/// <returns>
		/// The current active or the main model connected to the experiment, if no model can be returned by the
		/// scheduler
		/// </returns>
		public Model currentModel()
		{

			Model mBuff = _owner.getExperiment().getScheduler().getCurrentModel();

			if (mBuff != null)
				return mBuff;
			else
				return _owner.getExperiment().getModel();

		}

		/// <summary>Returns the currently active SimProcess that is handled by the scheduler.</summary>
		/// <returns>The current active SimProcess.</returns>
		public SimProcess currentSimProcess()
		{

			return _owner.getExperiment().getScheduler().getCurrentSimProcess();

		}

		/// <returns>The current simulation time</returns>
		/// <remarks>
		/// Replaced by presentTime(). Returns the current simulation time as displayed by the simulation clock
		/// responsible for this modelcomponent.
		/// </remarks>
		//@Deprecated
		public SimTime currentTime()
		{
			return SimTime.toSimTime(_owner.getExperiment().getSimClock()
					.getTime());
		}

		/// <summary>
		/// Returns the current simulation time as displayed by the simulation clock responsible for this
		/// modelcomponent.
		/// </summary>
		/// <returns>The current point of simulation time</returns>
		public TimeInstant presentTime()
		{
			return _owner.getExperiment().getSimClock().getTime();
		}

		/// <summary>Shows if this modelcomponent currently produces debug output.</summary>
		/// <returns>true, if modelcomponent shows in debug, false if not</returns>
		public bool debugIsOn()
		{
			
			return _debugMode; // has anybody ever returned from a debugMode...

		}

		/**
		 * Switches off debug output for this modelcomponent. Does nothing if trace
		 * is already switched off.
		 */
		public void debugOff()
		{

			_debugMode = false; // yep, that's it!

		}

		/**
		 * Switches on debug output for this modelcomponent. Does nothing if debug
		 * is already switched on.
		 */
		public void debugOn()
		{

			_debugMode = true; // yep, that's true!

		}

		/// <returns>The minimum ditinguishable span of simulation time</returns>
		/// <remarks>
		/// Use TimeOperations.getEpsilon(). Returns the minimum distinguishable span of simulation time for this
		/// experiment.
		/// </remarks>
		public SimTime epsilon()
		{

			return SimTime.toSimTime(new TimeSpan(1L, TimeOperations.getEpsilon()));

		}

		/// <summary>Returns the model that owns this component.</summary>
		/// <returns>The model that this component is associated to</returns>
		public Model getModel()
		{

			return _owner; // "Make all things as simple as possible : but not
						   // simpler!"
						   // Albert Einstein

		}

		/// <summary>
		/// Tests if the modelcomponent given as parameter is a component of the same experiment as this
		/// modelcomponent.
		/// </summary>
		/// <param name="other">the other modelcomponent to check compatibility with</param>
		/// <returns>
		/// true, if this modelcomponent belongs to the same experiment as this modelcomponent, false otherwise
		/// </returns>
		public bool isExperimentCompatible(ModelComponent other)
		{

			// Checks if this modelcomponent has same experiment as other
			// modelcomponent
			return (_owner.getExperiment() == other.getModel().getExperiment());

		}

		/// <summary>
		/// Tests if the modelcomponent given as parameter is a component of the same model as this modelcomponent.
		/// </summary>
		/// <param name="other">the other modelcomponent to check compatibility with</param>
		/// <returns>
		/// <c>true</c>, if this modelcomponent belongs to the same model as the given modelcomponent, <c>false</c>
		/// otherwise
		/// </returns>
		public bool isModelCompatible(ModelComponent other)
		{

			// since checking for compatibility is the models's responsibility,
			// we just pass checking on to our owner.
			return _owner.checkCompatibility(other);

		}

		/// <summary>
		/// Creates and sends a debugnote to the experiment's messagedistributor. Debugnotes express the internal
		/// state of a modelcomponent to visualize the changes of state to help find bugs. Classes <c>Scheduler</c>
		/// and <c>Queue</c> both produce debugnotes if set to do so representing the data stored inside them. The
		/// information about the simulation time is extracted from the experiment and must not be given as a
		/// parameter.
		/// </summary>
		/// <param name="description">
		/// The description of a modelcomponent's internal state to be passed with this debugnote
		/// </param>
		public void sendDebugNote(string description)
		{
			// send debug message only if debug mode of this model component
			// and debug output is activated
			// This bugfix was contributed by Heine Kolltveit
			if (currentlySendDebugNotes())
			{
				sendMessage(new DebugNote(getModel(), presentTime(), getName(),
						description));
			}
		}

		/// <summary>
		/// returns true if this model component should currently send debug notes (i.e. experiment and the
		/// component are both in debug mode).
		/// </summary>
		protected bool currentlySendDebugNotes()
		{
			return debugIsOn() && getModel().getExperiment().debugIsOn();
		}

		/// <summary>Sends a message to the experiment or writes it to standard output when no experiment is connected.</summary>
		/// <param name="m">The message to send.</param>
		public void sendMessage(Message m)
		{

			if (m == null)
			{
				sendWarning("Can't send Message!", "ModelComponent : " + getName()
						+ " Method: SendMessage(Message m)",
						"The Message given as parameter is a null reference.",
						"Be sure to have a valid Message reference.");
				return; // no proper parameter
			}

			if (_owner != null)
			{ // is modelcomponent connected to model?

				if (_owner.getExperiment() != null)
				{ // is model connected to
				  // Experiment?

					getModel().getExperiment().getMessageManager().receive(m);
					return;
				}
			}

			// if not connected to messaging system, write to standard out
			Console.WriteLine(m);

		}

		/// <summary>
		/// Creates and sends a tracenote to the experiment's messagedistributor. The information about the
		/// simulation time, model and component producing this tracenote is extracted from the experiment and must
		/// not be given as parameters.
		/// </summary>
		/// <param name="description">The description of the tracenote</param>
		public void sendTraceNote(string description)
		{
			// send trace message only if trace mode of this model component
			// and trace output is activated
			// This bugfix was contributed by Heine Kolltveit

			string mode = ""; // no special mode

			if (currentModel().isConnected() && currentModel().getExperiment().isPreparing())
				mode = "initially ";

			if (currentlySendTraceNotes())
			{
				sendMessage(new TraceNote(currentModel(), mode + description,
						presentTime(), currentEntityAll(), currentEvent()));
			}
		}

		/// <summary>
		/// returns true if this model component should currently send trace notes (i.e. experiment and the
		/// component are both in trace mode).
		/// </summary>
		public bool currentlySendTraceNotes()
		{
			return traceIsOn() && getModel().getExperiment().traceIsOn();
		}

		/// <summary>
		/// Creates and sends an error message to warn about a erroneous condition in the DESMO-J framework to the
		/// experiment's messagedistributor. Be sure to have a correct location, since the object and method that
		/// the error becomes apparent is not necessary the location it was produced in. The information about the
		/// simulation time is extracted from the Experiment and must not be given as a parameter.
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

			// comnpose the ErrorMessage and send it in one command
			sendMessage(new ErrorMessage(getModel(), description, location, reason,
					prevention, presentTime()));

		}

		/// <summary>
		/// Sets the owner of a modelcomponent to the given reference. This is exclusively needed to build a
		/// self-reference for the main model of an experiment. Has to be delegated to class <c>modelcomponent</c>
		/// since the owner attribute is encapsulated in this class.
		/// </summary>
		/// <param name="newOwner">The modelcomponent's new owner</param>
		public void setOwner(Model newOwner)
		{

			_owner = newOwner;

		}

		/**
		 * Skips the next tracenote. The next tracenote produced by any object in
		 * the DESMO-J framework will not be distributed by the experiment's
		 * messagemanager. This is necessary for some operations to hide the
		 * framework's actions and thus not confuse the modeller.
		 */
		public void skipTraceNote()
		{

			skipTraceNote(1);

		}

		/// <summary>
		/// Skips the next number of tracenotes. The next -numSkipped - number of tracenotes produced by any object
		/// in the DESMO-J framework will not be distributed by the experiment's messagemanager. This is necessary
		/// for some operations to hide the framework's actions and thus not confuse the modeller.
		/// </summary>
		/// <param name="numSkipped">The number of future tracenotes to be skipped</param>
		public void skipTraceNote(int numSkipped)
		{

			if (numSkipped < 1)
				return; // nothing to do or negative (illegal) param.

			if (!currentlySendTraceNotes())
				return; // not sending trance notes anyway

			try
			{
				getModel().getExperiment().getMessageManager().skip(
						typeof(TraceNote), numSkipped);
			}
			catch (Exception cnfx)
			{
				throw new SimAbortedException(
						new ErrorMessage(
								getModel(),
								"Can not skip tracenotes! Simulation aborted.",
								"ModelComponent : " + getName()
										+ " Method : skipTraceNote()",
								"The file for class desmoj.report.TraceNote can not be found by "
										+ "the Java runtime. Following exception was caught : "
										+ cnfx.ToString(),
								"Check that all pathnames for DESMOJ are set in your environment.",
								presentTime()));

			}

		}

		/// <summary>Shows if this modelcomponent currently produces trace output.</summary>
		/// <returns>true, if modelcomponent shows in trace, false if not</returns>
		public bool traceIsOn()
		{

			return _traceMode;

		}

		/**
		 * Switches off trace output for this modelcomponent. Does nothing if trace
		 * is already switched off.
		 */
		public void traceOff()
		{

			_traceMode = false; // yep, that's it!

		}

		/**
		 * Switches on trace output for this modelcomponent. Does nothing if trace
		 * is already switched on.
		 */
		public void traceOn()
		{

			_traceMode = true; // yep, that's it!

		}
	}
}