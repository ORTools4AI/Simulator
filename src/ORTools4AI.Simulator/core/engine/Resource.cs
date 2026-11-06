// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures;
using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.engine
{


	//34567890123456789012345678901234567890123456789012345678901234567890123456

	/**
     * Resources are objects needed by SimProcesses to perform certain tasks. Every
     * Resource has its unique ID number, so it can be identified. Each Resource
     * belongs to a certain resource category (resource pool) and has the status
     * ready (can be used) or <code>outOfOrder</code> if it is broken down.
     */
	public class Resource : ModelComponent
	{

		// ****** attributes ******

		/**
		 * The reference to the resource pool this resource belongs to.
		 */
		private Res _resPool;

		/**
		 * The number identifying a Resource. Because it is a class variable each
		 * Resource will get its own ID number starting by zero.
		 */
		private static long resourceNumber = 0;

		/**
		 * The ID number of this Resource object.
		 */
		private long _idNumber;

		/**
		 * Indicating if this resource is out of order (broken down) and therefore
		 * can not be used at the moment.
		 */
		private bool _outOfOrder;

		// ****** methods ******

		/// <summary>
		/// Constructs a resource object with the given String as name and the given model as the associated owner
		/// of this component. Components can only be created after the corresponding model object has been
		/// instantiated. The default preset for the showInTrace option is <c>false</c>.
		/// </summary>
		/// <param name="ownerModel">The model this resource is associated to.</param>
		/// <param name="name">The name of the resource.</param>
		/// <param name="resPool">The resource pool this resource belongs to.</param>
		/// <param name="showInTrace">
		/// Flag for showing this resource in trace files. Set it to <c>true</c> if resource should show up in
		/// trace. Set it to <c>false</c> if resource should not be shown in trace.
		/// </param>
		public Resource(Model ownerModel, string name, Res resPool,
				bool showInTrace) : base(ownerModel, name, showInTrace)
		{
			// create the ModelComponent
			_idNumber = resourceNumber++; // increment the ID number
			rename(name + " resource No. " + _idNumber); // set the name
			this._resPool = resPool; // set the reference to the resource pool
									 // this resource belongs to
			this._outOfOrder = false; // this resource can be used
		}

		/// <summary>Returns the ID number of this resource object.</summary>
		/// <returns>The ID number of this resource object.</returns>
		public long getidNumber()
		{
			return _idNumber;
		}

		/// <summary>Returns the resource pool (Res) this resource belongs to.</summary>
		/// <returns>The resource pool (Res) this resource belongs to.</returns>
		public Res getResPool()
		{
			return _resPool;
		}

		/// <summary>Is this resource out of order at the moment and therefore can not be used?</summary>
		/// <returns>Is this resource out of order at the moment?</returns>
		public bool isOutOfOrder()
		{
			return _outOfOrder;
		}

		/// <summary>
		/// This resource can be set to out of order (<c>true</c>) if it is broken down and can not be used at the
		/// moment.
		/// </summary>
		/// <param name="brokenDown">
		/// Flag for showing if this resource is out of order and therefore is not available at the moment. Set it
		/// to <c>true</c> if the resource is broken down. Set it to <c>false</c> if the resource is ready to use.
		/// </param>
		public void setOutOfOrder(bool brokenDown)
		{
			_outOfOrder = brokenDown;
		}
	}
}