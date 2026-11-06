// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Represents a DebugNote produced by a simulation run whenever the modeller
     * enables the debug mode for a specific ModelComponent. Provides the basic
     * information needed to debug a model's changes of state:
     * <ul>
     * <li>The Model this DebugNote originates from</li>
     * <li>The point in simulation time that this DebugNote was issued</li>
     * <li>The name of the ModelComponent that produced the DebugNote</li>
     * <li>The textual debug information produced by that ModelComponent</li>
     * </ul>
     */
	public class DebugNote : Message
	{

		/**
		 * The ModelComponent that produced this DebugNote.
		 */
		private string _dbgComponent;

		/// <summary>Constructs a debugnote with the given parameters.</summary>
		/// <param name="origin">The model that produced this debugnote</param>
		/// <param name="time">The point of simulation time this debugnote was created</param>
		/// <param name="componentName">The name of the modelcomponent the debugnote evolved from</param>
		/// <param name="description">The actual debug information</param>
		//TODO:
		public DebugNote(Model origin, TimeInstant time, string componentName,
				string description) : base(origin, description, time)
		{

			//super(origin, description, time);
			_dbgComponent = componentName;

		}

		/// <summary>Returns the name of the modelcomponent that produced this debugnote.</summary>
		/// <returns>The model that produced this debugnote</returns>
		public string getOrigin()
		{

			return _dbgComponent;

		}
	}
}