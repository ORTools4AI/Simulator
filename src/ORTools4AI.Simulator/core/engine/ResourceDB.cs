// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.report;
using System;
using System.Text;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * In the resource database every Simprocess and the resources it requests or
     * holds at the moment are stored. This information is needed i.e. for detecting
     * deadlocks. An instance will be created, when an experiment is created.
     */

	public class ResourceDB {

		// ****** attributes ******

		/**
		 * The reference to the experiment this ResourceDB belongs to.
		 */
		private Experiment _owner;

		/**
		 * Flag indicating if this resourceDB should be listed in the debug output
		 * file.
		 */
		private bool _debugMode;

		/**
		 * Stores for every resource pool (the <code>Res</code> is the key to the
		 * hashtable) a Vector holding all the pairs of a SimProcess and the number
		 * of resources the SimProcess holds at the moment (see the inner class
		 * <code>AssignedResources</code>).
		 */
		private Dictionary<Res, List<AssignedResources>> _assignmentTable;

		/**
		 * Stores for every SimProcess the resource pool (<code>Res</code>) he
		 * is requesting resources from and the number of requested resources (see
		 * the inner class <code>RequestedResources</code>). The SimProcess is
		 * building the key to the table entries which in turn are holding objects
		 * of the inner class <code>RequestedResources</code>.
		 */
		private Dictionary<SimProcess, RequestedResources> _requestTable;

		/**
		 * Stores for every resource pool (the <code>Res</code> is the key to the
		 * hashtable) the effective available capacity of the resource as an
		 * <code>Integer</code> object. Which is the sum of the current available
		 * capactiy of resources in the Res pool and the returned units of that Res
		 * pool due to reduction (the units which might be returned in the future
		 * because they are not stuck in a deadlock).
		 */
		private Dictionary<Res, int> _effCapacity;

		/**
		 * Stores all the visited resources when traversing the resource allocation
		 * graph. Maybe another implementation is performing better. You are invited
		 * to think about it.
		 */
		private List<Res> _visitedRes;

		/**
		 * Stores all the visited SimProcesses when traversing the resource
		 * allocation graph. Maybe another implementation is performing better. You
		 * are invited to think about it.
		 */
		private List<SimProcess> _visitedProcs;

		/**
		 * Stores all the SimProcesses which are involved in a deadlock. Is filled
		 * when the method <code>checkForDeadlock()</code> is called.
		 */
		private List<SimProcess> _deadlockedProcs;

		/**
		 * Stores all the resource pools (<code>Res</code>) which are involved
		 * in a deadlock. The Vector is filled when the method
		 * <code>checkForDeadlock()</code> is called.
		 */
		private List<Res> _deadlockedRes;

		/**
		 * Stores all the visited resources when traversing the resource allocation
		 * graph is done with examining all it's children vertices. Maybe another
		 * implementation is performing better. You are invited to think about it.
		 */
		private List<Res> _doneRes;

		/**
		 * Stores all the visited SimProcesses when traversing the resource
		 * allocation graph is done with examining all it's children vertices. Maybe
		 * another implementation is performing better. You are invited to think
		 * about it.
		 */
		private List<SimProcess> _doneProcs;

		/**
		 * Stores the cycle in the resource allocation graph as a string
		 * representation. Is built up as the graph containing the cycle (deadlock)
		 * is drawn.
		 */
		private StringBuilder _resAllocGraph;

		/**
		 * Stores all the elements not belonging to the the cycle in the resource
		 * allocation graph as a string representation. Is built up as the graph
		 * containing the cycle (deadlock) is drawn.
		 */
		private StringBuilder _nonCycleGraph;

		/**
		 * Indicates the method where something has gone wrong. Is passed as a
		 * parameter to i.e. the method <code>checkProcess()</code>.
		 */
		private string _where;

		/**
		 * Indicates whether a cycle in the resource allocation graph is found or
		 * not. Is set to <code>true</code> if a cycle (and therefore a possible
		 * deadlock) is found and <code>false</code> if no cycle is found.
		 */
		private bool _cycleFound;



		/**
		 * This private constructor is defined to prevent the compiler from
		 * generating a default public constructor.
		 */
		/* Constructor is never used 
		private ResourceDB() {

			// make the hashtables to store all the information
			_assignmentTable = new Hashtable<Res, Vector<AssignedResources>>();
			_requestTable = new Hashtable<SimProcess, RequestedResources>();
			_effCapacity = new Hashtable<Res, Integer>();
		}
		*/

		/**
		 * This constructor is called from the constructor of
		 * <code>Experiment</code> to make sure that for every experiment there is
		 * a new resource DB. The debug output for this <code>ResourceDB</code> is
		 * turned on by default. But you have to make sure, that the default output
		 * for the experiment is turned on, too.
		 * 
		 * @param owner
		 *            desmoj.Experiment : the experiment this resource database
		 *            belongs to.
		 */
		public ResourceDB(Experiment owner) {

			this._owner = owner;

			// make the hashtables to store all the information
			_assignmentTable = new Dictionary<Res, List<AssignedResources>>();
			_requestTable = new Dictionary<SimProcess, RequestedResources>();
			_effCapacity = new Dictionary<Res, int>();

			// turn the debug output on, so the ResourceDB can produce debug output
			debugOn();
		}

		/// <summary>
		/// Checks the additional status of a deadlock, i.e. if the deadlock is a pending or a transient one.
		/// Returns <c>true</c> if it is a pending deadlock and <c>false</c> if it is a transient deadlock.
		/// </summary>
		/// <returns>
		/// <c>true</c> if it is a pending deadlock and <c>false</c> if it is a transient deadlock.
		/// </returns>
		private  bool additionalStatus() {

			// we have to update the effCapacity for every Res
			// get all the resource pools
			foreach (var rs in _assignmentTable.Keys) {
				// get hold of the resouce pool
				//Res rs = (Res)resPools.nextElement();

				// put the number of available resource in the effCapacity hashtable
				_effCapacity[rs]= rs.getAvail();
			}

			// don't mess with the original, get yourself a copy of the visited
			// processes
			List<SimProcess> listOfProcs = new List<SimProcess>(_visitedProcs);

			// do we have to search the whole list of SimProcesses from the
			// beginning
			// again, after some reduction has taken place?
			bool startAgain = false;

			do // do while some reduction was made
			{
				// reset startAgain to false
				startAgain = false;

				// for every SimProcess in the list
				for (int i = 0; i < listOfProcs.Count; i++) {
					SimProcess crntProc = (SimProcess)listOfProcs[i];

					// get the request resources for this SimProcess
					_requestTable.TryGetValue(crntProc, out RequestedResources reqRes);

					if (reqRes == null) {
						reduce(crntProc);

						// delete the SimProcess from the list
						listOfProcs.RemoveAt(i);

						// start again
						startAgain = true;
						break; // break the for loop
					}

					// get the Res pool
					Res crntRes = reqRes.getResPool();
					// and the requested units
					int vReq = reqRes.getRequestedUnits();

					// get the virtual requested units of resources by the
					// SimProcess.
					// that is the maximum request size of all SimProcesses ahead of
					// it in
					// the queue (needed when the queue has a no-pass filling rule)

					// check if the Res queue has a no-pass filling rule
					if (!crntRes.getPassBy()) {
						// loop through all the processes in the queue of this Res
						// pool
						for (SimProcess proc = (SimProcess)crntRes.getQueue()
								.first(); proc != crntProc; proc = (SimProcess)crntRes
								.getQueue().succ(proc)) {
							// how many resources is the proc requesting from
							// crntRes?
							int procReq = 0;
							RequestedResources reqR = (RequestedResources)_requestTable[proc]
									;

							// is the Res pool the same?
							if (crntRes == reqR.getResPool()) {
								// get the number of requested units
								procReq = reqR.getRequestedUnits();

								// if the number is greater
								if (vReq < procReq) {
									vReq = procReq; // change the virtual requested
													// units
								} // end inner if
							} // end if

						} // end for loop through the queue of this Res pool
					} // end if the Res has a no-pass filling rule

					// if the SimProcess can be satisfied in the future
					// i.e. vReq < effCapacity of crntRes
					if (vReq <= ((int)_effCapacity[crntRes])) {
						reduce(crntProc);

						// delete the SimProcess from the list
						listOfProcs.RemoveAt(i);

						// start again
						startAgain = true;
					} // end if

				} // end outer for

			} while (startAgain);

			// for debugging purposes during SW development only
			// System.out.println("The list of processes: " +
			// listOfProcs.toString());

			// are there still elements in the list (the list is NOT empty)
			if (listOfProcs.Count>0) {
				return true; // pending deadlock
			} else {
				return false; // transient deadlock
			}
		}

		/// <summary>
		/// This method is called when a SimProcess can not get the resources desired to check if a possible
		/// deadlock situation has occured.
		/// </summary>
		/// <param name="unsatProc">
		/// The SimProcess which can not get the resources desired. The unsatisfied process.
		/// </param>
		/// <returns>is <c>true</c> if a deadlock is found, <c>false</c> otherwise.</returns>
		public bool checkForDeadlock(SimProcess unsatProc) {

			_where = "protected synchronized boolean checkForDeadlock(SimProcess "
					+ "unsatProc)";

			// check for null reference
			if (!checkProcess(unsatProc, _where))
				return false; // if the SimProcess is not valid just return

			_visitedProcs = new List<SimProcess>();
			_visitedRes = new List<Res>();
			_doneProcs = new List<SimProcess>();
			_doneRes = new List<Res>();
			_deadlockedProcs = new List<SimProcess>();
			_deadlockedRes = new List<Res>();

			_cycleFound = false;

			_resAllocGraph = new StringBuilder();
			_nonCycleGraph = new StringBuilder();

			// look for a cycle starting with the unsatisfied process
			findCycleProc(unsatProc);

			if (_cycleFound) {
				// check what kind of deadlock is discovered

				// check if a total deadlock is discovered (all the visited
				// processes
				// are involved in the deadlock)
				if (_deadlockedProcs.Count == _visitedProcs.Count) {
					Console.WriteLine("A total deadlock was detected in your model "
								+ "at simulation time "
								+ _owner.getSimClock().getTime() + " .");
                    Console.WriteLine("Please check the error file!");
                    Console.WriteLine("The debug file also might help to learn more about "
								+ "this deadlock");

					sendTraceNote("has detected a <b>total deadlock</b> situation. Please "
							+ "check the error file! Turn the debug mode on (if not "
							+ "done already) and check the debug file to learn more "
							+ "about this deadlock.");

					sendDebugNote("A <b>total deadlock</b> situation is detected in your "
							+ "simulation! <br>Examine the information provided by the "
							+ "resource database above.");

					// same information is provide by noteResourceRequest() already,
					// if debugIsOn
					if (!debugIsOn()) {
						sendDebugNote(this.toHtmlString());
					}

					sendWarning(
							"A <b>total deadlock</b> is detected in the resource "
									+ "allocation graph. <br>The simulation can not continue properly due "
									+ "to this deadlock.",
							"ResourceDB Method: " + _where,
							"The SimProcess '"
									+ unsatProc.getName()
									+ "' can not get the "
									+ "resources desired, because they are occupied by another "
									+ "SimProcess. <br>The following chain of resource allocations and "
									+ "requests has lead to the deadlock: <br>"
									+ _resAllocGraph,
							"Check if a situation as described above can happen in the real "
									+ "system, too. <br>Check if your model may not be implemented "
									+ "correctly!");
				} else // no total deadlock
				{
					// check if a pending or transient deadlock is discovered
					if (additionalStatus()) {
                        // pending deadlock
                        Console.WriteLine("A pending deadlock was detected in your model "
									+ "at simulation time "
									+ _owner.getSimClock().getTime() + " .");
                        Console.WriteLine("Please check the error file!");
                        Console.WriteLine("The debug file also might help to learn more about "
									+ "this deadlock");

						sendTraceNote("has detected a <b>pending deadlock</b> situation. Please "
								+ "check the error file! <br>Turn the debug mode on (if not "
								+ "done already) and check the debug file to learn more "
								+ "about this deadlock.");

						sendDebugNote("A <b>pending deadlock</b> situation is detected in your "
								+ "simulation! <br>Examine the information provided by the "
								+ "resource database above.");

						// same information is provided by noteResourceRequest()
						// already, if debugIsOn
						if (!debugIsOn()) {
							sendDebugNote(this.toHtmlString());
						}

						sendWarning(
								"A <b>pending deadlock</b> is detected in the resource "
										+ "allocation graph. <br>Some of the processes in the "
										+ "simulation are stuck in the deadlock and can not "
										+ "continue properly.",
								"ResourceDB Method: " + _where,
								"The SimProcess '"
										+ unsatProc.getName()
										+ "' can not get the "
										+ "resources desired, because they are occupied by another "
										+ "SimProcess. <br>The following chain of resource allocations and "
										+ "requests has lead to the deadlock: <br>"
										+ _resAllocGraph
										+ "<br>Furthermore the following resource allocations and requests "
										+ "are present: <br>" + _nonCycleGraph,
								"Check if a situation as described above can happen in the real "
										+ "system, too. <br>Check if your model may not be implemented "
										+ "correctly!");
					} else // transient deadlock
					{
                        Console.WriteLine("A transient deadlock was detected in your model "
									+ "at simulation time "
									+ _owner.getSimClock().getTime() + " . ");
                        Console.WriteLine("Although this situation may self-resolve, better "
									+ "check the error and debug files!");

						sendTraceNote("has detected a <b>transient deadlock</b> situation. <br>"
								+ "Although this situation may self-resolve, better "
								+ "check the error and debug files!");

						sendDebugNote("A <b>transient deadlock</b> situation is detected in your "
								+ "simulation! <br>Better examine the information provided "
								+ "by the resource database above.");

						// same information is provide by noteResourceRequest()
						// already, if debugIsOn
						if (!debugIsOn()) {
							sendDebugNote(this.toHtmlString());
						}

						sendWarning(
								"A <b>transient deadlock</b> is detected in the resource "
										+ "allocation graph. <br>This situation may self-resolve, "
										+ "but at the moment some processes are blocked.",
								"ResourceDB Method: " + _where,
								"The SimProcess '"
										+ unsatProc.getName()
										+ "' can not get the "
										+ "resources desired, because they are occupied by another "
										+ "SimProcess at the moment. <br>The following chain of resource "
										+ "allocations and requests has lead to the deadlock: <br>"
										+ _resAllocGraph
										+ "<br>Furthermore the following resource allocations and "
										+ "requests are present: <br>"
										+ _nonCycleGraph,
								"Check if a situation as described above can happen in the real "
										+ "system, too. <br>Check if your model may not be implemented "
										+ "correctly!");
					}
				}

				// set the deadlockDetected field for all the Res's involved to true
				foreach (var rs in _deadlockedRes) {
					rs.setDeadlockDetected(true); // that's it
				}

				// for debugging purposes during SW development only
				/*
				 * System.out.println("The following SimProcesses have been visited " +
				 * "to determine the resource allocation graph: " +
				 * visitedProcs.toString());
				 * 
				 * System.out.println("The following SimProcesses are involved in
				 * the " + "deadlock: " + deadlockedProcs.toString());
				 */

			} // end if cycle found

			return _cycleFound;

		}

		/// <summary>Checks whether the process using the ResourceDB is a valid process.</summary>
		/// <param name="p">Is this SimProcess a valid one?</param>
		/// <returns>Returns whether the SimProcess is valid or not.</returns>
		private bool checkProcess(SimProcess p, string where) {
			if (p == null) // if p is a null pointer instead of a process
			{
				sendWarning("Attempt to insert a non existing process into a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB Method: " + where,
						"The given process is only a null pointer.",
						"Make sure that only real SimProcesses are using resources.");
				return false;
			}

			// the resource database is a singleton and therefore has to store
			// information for all resource pools -> no checking for model
			// compatibility

			/*
			 * if (!isModelCompatible( p ) ) // if p is not modelcompatible {
			 * sendWarning ( "The process which should be inserted in the "+
			 * "ResourceDB does not belong to this model. The attempted action " +
			 * "is ignored!" , "ResourceDB: " + getName() + " Method: " + where ,
			 * "The given process is not modelcompatible.", "Make sure that
			 * processes are using only resources within" + " their model."); return
			 * false; }
			 */
			return true;
		}

		/// <summary>
		/// Checks whether the resource pool <c>Res</c> using the ResourceDB is a valid <c>Res</c>.
		/// </summary>
		/// <param name="r">Is this resource pool <c>Res</c> a valid one?</param>
		/// <returns>Returns whether the <c>Res</c> is valid or not.</returns>
		private bool checkRes(Res r, string where) {
			if (r == null) // if r is a null pointer instead of a Res
			{
				sendWarning("Attempt to insert a non existing Res into a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB: Method: " + where,
						"The given Res is only a null pointer.",
						"Make sure that only real resource pools (Res) are using the "
								+ "resource database.");
				return false;
			}

			// the resource database is a singleton and therefore has to store
			// information for all resource pools -> no checking for model
			// compatibility
			/*
			 * if (!isModelCompatible( r ) ) // if r is not modelcompatible {
			 * sendWarning ( "The Res which should be inserted in the "+ "ResourceDB
			 * does not belong to this model. The attempted action " + "is ignored!" ,
			 * "ResourceDB: " + getName() + " Method: " + where , "The given Res is
			 * not modelcompatible.", "Make sure that resource pools (Res) are using
			 * only a resource " + "database within their model."); return false; }
			 */
			return true;
		}

		/// <summary>Shows if this resourceDB currently produces debug output.</summary>
		/// <returns>true, if resourceDB shows in debug, false if not</returns>
		public bool debugIsOn() {

			return _debugMode; // that's all

		}

		/**
		 * Switches off debug output for this resourceDB.
		 */
		public void debugOff() {

			_debugMode = false; // yep, that's it!

		}

		/**
		 * Switches on debug output for this resourceDB.
		 */
		public void debugOn() {

			_debugMode = true; // yep, that's true!

		}

		/// <summary>
		/// Deletes an entry in the resource data base (should be called when a Sim-process is done with it's
		/// requested number of resources from a resource pool (<c>Res</c> )). The SimProcess and the number of
		/// requested resources will be deleted in the corresponding Vector (of the Res pool) in the assignment
		/// hashtable. The Res pool is the key to this hashtable, specifying the Vector which holds all the
		/// (SimProcess/number of requested resources)-pairs.
		/// </summary>
		/// <param name="resPool">
		/// The resource pool which has provided the resources. (The key to the hashtable)
		/// </param>
		/// <param name="doneProc">
		/// The Simprocess giving back his allocated resources. He should be done with the resources now.
		/// </param>
		/// <param name="quantity">The number of resources the SimProcess has used.</param>
		public void deleteResAllocation(Res resPool, SimProcess doneProc,
				int quantity) {
			_where = "protected void deleteResAllocation(Res resPool, "
					+ "SimProcess doneProc, int quantity)";

			// checks for null references and negative quantity
			if (!checkProcess(doneProc, _where))
				return; // if the SimProcess is not valid just return

			if (!checkRes(resPool, _where))
				return; // if the Res is not valid just return

			if (quantity <= 0) {
				sendWarning("Attempt to delete a negative or zero quantity from a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given quantity is zero or negative.",
						"Make sure to only use positive quantities which are more than "
								+ "nothing.");
				return; // if the quantity is negative or zero just return
			}

			// check if there is an entry for the given Res pool in the assignment
			// hashtable
			if (!_assignmentTable.ContainsKey(resPool)) { // if no entry -> error
				sendWarning(
						"Attempt to delete an entry in the resource database "
								+ "for a resource pool (Res), but the Res pool does not exist in the "
								+ "database. The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given resource pool has no entry in the database.",
						"Make sure that the given resource pool has provided resources "
								+ "before you try to delete it's entry in the database.");
				return; // the Res pool is not registered in the resourceDB
			} else // get hold of the corresponding Vector
			{
				var resPoolVec = _assignmentTable[resPool];

				// flag to indicate whether the given SimProcess is found in the
				// Vector
				bool foundInVec = false;

				// search the whole Vector to find the given SimProcess
				for (int i = 0; i < resPoolVec.Count; i++) {
					// get hold of the pair (SimProcess / number of resources)
					AssignedResources assigRes = (AssignedResources)resPoolVec[i];
							

					// is the given SimProcess found?
					if (assigRes.getProcess() == doneProc) {
						// note that the given SimProcess is found in the Vector
						foundInVec = true;

						// delete the whole entry in the Vector
						resPoolVec.Remove(assigRes);

						// is the given quantity not exactly the same as found in
						// the Vector?
						if (!(assigRes.getSeizedUnits() == quantity)) {
							// will be more resources deleted than the process has
							// allocated?
							if (quantity > assigRes.getSeizedUnits()) {
								sendWarning(
										"Attempt to delete more resources than "
												+ "the SimProcess has allocated. Only all the formerly "
												+ "allocated resources will be deleted in the database and "
												+ "no more!",
										"ResourceDB Method: " + _where,
										"The entry in the database has not registered as many "
												+ "allocated resources as there should be deleted now.",
										"Make sure to only delete the same quantity of "
												+ "resources as there were allocated once.");
							} else // there will be less resources deleted than
								   // once
								   // allocated
							{
								// reduce the quantity of allocated resources
								assigRes.setSeizedUnits(assigRes.getSeizedUnits()
										- quantity);

								// put the updated pair of (SimProcess/number of
								// resources)
								// back in the Vector
								resPoolVec.Add(assigRes);
							} // end inner else

						} // end outer if
					} // end outer outer if
				} // end for loop

				// is the given SimProcess not found in the Vector?
				if (!foundInVec) {
					sendWarning(
							"Can't find the SimProcess for which there should be "
									+ "deleted allocated resources. The attempted action can't be "
									+ "performed!", "ResourceDB Method: " + _where,
							"The entry in the database for the given SimProcess "
									+ "can not be found.",
							"Make sure to only delete resources which a SimProcess "
									+ "has allocated already.");
				} // end if

				// is the Vector empty now?
				if (resPoolVec.Count==0) {
					// remove the Res pool Vector from the assignment hashtable
					_assignmentTable.Remove(resPool);
				} else // there is still something in the Vector, so...
				{
					// put the updated Vector back in the assignment hashtable
					_assignmentTable[resPool] = resPoolVec;
				}

			} // end outer else

			// update the effective available capacity of the Res pool
			// effCapacity.put( resPool, new Integer( resPool.getAvail() ) );

			// for debugging purposes
			if (debugIsOn())
				sendDebugNote(this.toHtmlString());
		}

		/// <summary>
		/// Deletes an entry in the resource data base (should be called when a Sim-process receives it's requested
		/// number of resources from a resource pool (<c>Res</c> )). The resource pool and the number of requested
		/// resources will be deleted in the request hashtable. The Sim-process is the key to this hashtable,
		/// specifying the RequestedResources object which holds the (resource pool/number of requested
		/// resources)-pair.
		/// </summary>
		/// <param name="gainProc">
		/// The Simprocess giving up his request for the resources. Hopefully he gets satisfied.
		/// </param>
		/// <param name="resPool">The resource pool which provides the resources.</param>
		/// <param name="quantity">The number of resources the SimProcess gets.</param>
		public void deleteResRequest(SimProcess gainProc, Res resPool, int quantity) {
			_where = "protected void deleteResRequest(SimProcess gainProc, "
					+ "Res resPool,	int quantity)";

			// checks for null references and negative quantity
			if (!checkProcess(gainProc, _where))
				return; // if the SimProcess is not valid just return

			if (!checkRes(resPool, _where))
				return; // if the Res is not valid just return

			if (quantity <= 0) {
				sendWarning("Attempt to delete a negative or zero quantity from a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given quantity is zero or negative.",
						"Make sure to only use positive quantities which are more than "
								+ "nothing.");
				return; // if the quantity is negative or zero just return
			}

			// check if there is an entry for the given SimProcess in the request
			// hashtable
			if (!_requestTable.ContainsKey(gainProc)) { // if no entry -> error
				sendWarning(
						"Attempt to delete an entry in the resource database "
								+ "for a SimProcess, but the SimProcess does not exist in the "
								+ "database. The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given SimProcess has no entry in the database.",
						"Make sure that the given SimProcess has requested resources "
								+ "before you try to delete it's entry in the database.");
				return; // the SimProcess is not registered in the resourceDB
			}

			// get hold of the pair (resource pool / number of resources)
			RequestedResources reqRes = (RequestedResources)_requestTable[gainProc];

			// is the given resource not the same as in the database?
			if (reqRes.getResPool() != resPool) {
				sendWarning(
						"Can't find the resource pool which requested resources "
								+ "should be deleted. The attempted action can't be performed!",
						"ResourceDB Method: " + _where,
						"The entry in the database for the given resource pool "
								+ "can not be found.",
						"Make sure to only delete requested resources of a  "
								+ "resource pool from which resources are requested already.");
			}

			// will be more resources deleted than the process has requested?
			if (quantity > reqRes.getRequestedUnits()) {
				sendWarning(
						"Attempt to delete more requested resources than "
								+ "the SimProcess has requested. Only all the formerly "
								+ "requested resources will be deleted in the database and "
								+ "no more!",
						"ResourceDB Method: " + _where,
						"The entry in the database has not registered as many "
								+ "requested resources as there should be deleted now.",
						"Make sure to only delete the same quantity of requested "
								+ "resources as there were requested once.");

				// trick the database
				quantity = reqRes.getRequestedUnits();
			}

			// is the given quantity exactly the same as found in the reqRes?
			if ((reqRes.getRequestedUnits() == quantity)) {
				// remove the entry for the SimProcess from the request hashtable
				_requestTable.Remove(gainProc);
			} else // there will be less resources deleted than once requested
			{
				// reduce the quantity of requested resources
				reqRes.setRequestedUnits(reqRes.getRequestedUnits() - quantity);

				// put the updated pair of (resource pool/number of resources)
				// back in the request hashtable
				_requestTable[gainProc] = reqRes;
			} // end else

			// for debugging purposes
			if (debugIsOn())
				sendDebugNote(this.toHtmlString());
		}

		/// <summary>
		/// Try to find a cycle in the resource allocation graph starting with the vertex specified in the
		/// parameter <c>SimProcess vertex</c>. If we can spot a cycle in the resource allocation graph it is
		/// likely that we have found a deadlock.
		/// </summary>
		/// <param name="vertex">
		/// The point to start the search of the resource allocation graph to find a deadlock (cycle in the graph).
		/// </param>
		private void findCycleProc(SimProcess vertex) {
			// mark the given SimProcess vertex as visited
			_visitedProcs.Add(vertex);

			// mark the given SimProcess vertex as possible member of a cycle
			// as no cycle is found yet
			if (!_cycleFound) {
				_deadlockedProcs.Add(vertex);
			}

			// get the adjacent Res vertex to this SimProcess vertex
			_requestTable.TryGetValue(vertex, out RequestedResources reqRes);

			// if the RequestedResources for this SimProcess contains an adjacent
			// Res
			if (reqRes != null) {
				// get hold of the adjacent Res
				Res resVertex = reqRes.getResPool();

				// get the number of requested resources
				int nReqRes = reqRes.getRequestedUnits();

				// is the Res vertex not visited already
				if (!_visitedRes.Contains(resVertex)) {
					// remember if there is a cycle found already or not
					bool oldCycleStatus = _cycleFound;

					// get the length of resAllocGraph and nonCycleGraph so far
					int len = _resAllocGraph.Length;
					int lenNoCycle = _nonCycleGraph.Length;

					// draw the nonCycle part of the resource request graph
					_nonCycleGraph.Append("SimProcess '" + vertex.getName()
							+ "' is waiting for " + nReqRes
							+ " unit(s) from the resource pool '"
							+ resVertex.getName() + "' , but <br>");

					// 'draw' the cycle of the resource request graph only as no
					// cycle is found yet
					if (!_cycleFound) {
						// draw the cycle
						_resAllocGraph.Append("SimProcess '" + vertex.getName()
								+ "' is waiting for " + nReqRes
								+ " unit(s) from the resource pool '"
								+ resVertex.getName() + "' , <br>");
					}

					// get the length of resAllocGraph and nonCycleGraph now (after
					// appending it)
					int m = _resAllocGraph.Length;
					int k = _nonCycleGraph.Length;

					// look for a cycle in the subsequent graph
					findCycleRes(resVertex);

					// if no cycle is found in the subsequent graph delete the cycle
					// 'drawing'
					if (!_cycleFound) {
						// delete the drawing from the cycle graph
						_resAllocGraph.Remove(len, m - len);
					}

					// if a new cycle is found in the newly investigated subsequent
					// graph, that means
					// the status has changed while investigating the subsequent
					// path
					if (oldCycleStatus != _cycleFound) {
						// delete the drawing from the nonCycle graph
						_nonCycleGraph.Remove(lenNoCycle, k - lenNoCycle);
					}

				} else // the vertex is visited
				{
					if (!_doneRes.Contains(resVertex) && // but is not done with his
														 // sons AND
							!_cycleFound) // no cycle is found so far
					{
						// deadlock cycle found
						_cycleFound = true;

						// "draw" the cycle part of the resource request graph
						_resAllocGraph.Append("SimProcess '" + vertex.getName()
								+ "' is waiting for " + nReqRes
								+ " unit(s) from the resource pool '"
								+ resVertex.getName() + "' , <br>");
					} // end inner if
					else // is done with his sons (everything is invetigated from
						 // here on)
					{
						// draw the nonCycle path of the resource allocation graph
						// how you came here
						_nonCycleGraph.Append("SimProcess '" + vertex.getName()
								+ "' is waiting for " + nReqRes
								+ " unit(s) from the resource pool '"
								+ resVertex.getName() + "' . <br>");
					} // end inner else

				} // end outer else (the vertex is visited)
			} // end outer if

			// mark the given SimProcess as "done with the investigation
			// of all his sons"
			_doneProcs.Add(vertex);

			// if no cycle is found yet delete the given SimProcess vertex as
			// possible member of a cycle
			if (!_cycleFound) {
				_deadlockedProcs.Remove(vertex);
			}

		}

		/// <summary>
		/// Try to find a cycle in the resource allocation graph starting with the vertex specified in the
		/// parameter <c>Res vertex</c>. If we can spot a cycle in the resource allocation graph it is likely that
		/// we have found a deadlock.
		/// </summary>
		/// <param name="vertex">
		/// The point to start the search of the resource allocation graph to find a deadlock (cycle in the graph).
		/// </param>
		private void findCycleRes(Res vertex) {

			// mark the given Res vertex as visited
			_visitedRes.Add(vertex);

			// mark the given Res vertex as possible member of a cycle
			// as no cycle is found yet
			if (!_cycleFound) {
				_deadlockedRes.Add(vertex);
			}

			// get all the adjacent SimProcess vertices to this Res vertex
			_assignmentTable.TryGetValue(vertex, out List<AssignedResources> assigToProcVec);

			// if the Vector for this Res contains some adjacent SimProcesses
			if (assigToProcVec != null) {
				// for every adjacent SimProcess
				foreach (var enumAssigProc in assigToProcVec) {
					// start a new path and remember if there is a cycle found in
					// the old path or not
					bool oldCycleStatus = _cycleFound;

					// get hold of the adjacent AssignedResource
					AssignedResources assigRes = enumAssigProc;

					// get hold of the adjacent SimProcess
					SimProcess procVertex = assigRes.getProcess();

					// get the number of assigned resources
					int nAssigRes = assigRes.getSeizedUnits();

					// is the vertex not visited already
					if (!_visitedProcs.Contains(procVertex)) {
						// get the length of resAllocGraph and nonCycleGraph so far
						int len = _resAllocGraph.Length;
						int lenNoCycle = _nonCycleGraph.Length;

						// draw the nonCycle part of the resource allocation graph
						_nonCycleGraph.Append(nAssigRes + " unit(s) from '"
								+ vertex.getName() + "' is/are currently "
								+ "used by '" + procVertex.getName() + "' . <br>");

						// 'draw' the cycle part of the resource allocation graph
						// only as no cycle is found yet
						if (!_cycleFound) {
							// draw the cycle
							_resAllocGraph.Append("but " + nAssigRes
									+ " unit(s) from '" + vertex.getName()
									+ "' is/are currently " + "used by '"
									+ procVertex.getName() + "' and <br>");
						}

						// get the length of resAllocGraph and nonCycleGraph now
						// (after appending it)
						int m = _resAllocGraph.Length;
						int k = _nonCycleGraph.Length;

						// look for a cycle in the subsequent graph
						findCycleProc(procVertex);

						// if no cycle is found in the subsequent graph delete the
						// 'drawing'
						if (!_cycleFound) {
							// delete the drawing from the cycle graph
							_resAllocGraph.Remove(len, m - len);
						}

						// if a new cycle is found in the newly investigated
						// subsequent graph, that means
						// the status has changed while investigating the subsequent
						// path
						if (oldCycleStatus != _cycleFound) {
							// delete the drawing from the nonCycle graph
							_nonCycleGraph.Remove(lenNoCycle, k - lenNoCycle);
						}

					} else // the vertex is visited
					{
						if (!_doneProcs.Contains(procVertex) && // but is not done
																// with his sons AND
								!_cycleFound) // no cycle is found so far
						{
							// deadlock cycle found!
							_cycleFound = true;

							// "draw" the cycle part of the resource allocation
							// graph
							_resAllocGraph.Append("but " + nAssigRes
									+ " unit(s) from '" + vertex.getName()
									+ "' is/are currently " + "used by '"
									+ procVertex.getName() + "'.<br>");
						} // end inner if
						else // is done with his sons (everything form here is
							 // investigated)
						{
							// draw the nonCycle path of the resource allocation
							// graph how you came here
							_nonCycleGraph.Append(nAssigRes + " unit(s) from '"
									+ vertex.getName() + "' is/are currently "
									+ "used by '" + procVertex.getName()
									+ "' . <br>");
						} // end inner else

					} // end else
				} // end for loop
			} // end outer if

			// mark the given Res as "done with the investigation
			// of all his sons"
			_doneRes.Add(vertex);

			// if no cycle is found yet delete the given Res vertex as
			// possible member of a cycle
			if (!_cycleFound) {
				_deadlockedRes.Remove(vertex);
			}

		}

		/// <summary>
		/// Makes an entry in the resource data base when a SimProcess is allocating a number of resources from a
		/// resource pool (<c>Res</c>). The Sim-process and the number of allocated resources will be saved in the
		/// assignment hashtable. The resource pool is the key to this hashtable.
		/// </summary>
		/// <param name="resourcePool">The resource pool which is providing the resources.</param>
		/// <param name="allocatingProcess">The Simprocess which is allocating the resources.</param>
		/// <param name="quantity">The number of resources the SimProcess is allocating.</param>
		public void noteResourceAllocation(Res resourcePool,
				SimProcess allocatingProcess, int quantity) {
			_where = "protected void noteResourceAllocation(Res resourcePool,"
					+ "SimProcess allocatingProcess, int quantity)";

			// checks for null references and negative quantity
			if (!checkProcess(allocatingProcess, _where))
				return; // if the SimProcess is not valid just return

			if (!checkRes(resourcePool, _where))
				return; // if the Res is not valid just return

			if (quantity <= 0) {
				sendWarning("Attempt to insert a negative or zero quantity into a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given quantity is zero or negative.",
						"Make sure to only use positive quantities which are more than "
								+ "nothing.");
				return; // if the quantity is negative or zero just return
			}

			// make the AssignedResources object which will be inserted in the
			// assignment hashtable, see innerclass AssignedResources
			AssignedResources assigResources = new AssignedResources(
					allocatingProcess, quantity);

			// is for the given resource pool existing an entry in the
			// assignment hashtable already?
			if (_assignmentTable.ContainsKey(resourcePool)) // there is an entry ...
			{
				// flag to indicate whether there is already an entry in the Vector
				// for this SimProcess
				bool simProcAlreadyAlloc = false;

				// get the Vector containing all pairs of SimProcesses and number of
				// assigned resources for the given resource pool
				var arrayOfAssigResources = _assignmentTable[resourcePool];

				// search the Vector of the given resource pool to find if the
				// SimProcess is already allocating resources from this resource
				// pool
				for (int i = 0; i < arrayOfAssigResources.Count; i++) {
					// get hold of the pair (SimProcess / number of resources)
					AssignedResources alreadyAssigRes = (AssignedResources)arrayOfAssigResources[i];
							

					// is the SimProcess already allocating resources of this
					// resource
					// pool? Then add the newly assigned number of resources.
					if (alreadyAssigRes.getProcess() == allocatingProcess) {
						// delete the old entry in the Vector
						arrayOfAssigResources.Remove(alreadyAssigRes);

						// add the new number of assigned resources to the old
						// number
						alreadyAssigRes.setSeizedUnits(alreadyAssigRes
								.getSeizedUnits()
								+ quantity);

						// put the updated pair of SimProcess and number of
						// allocated
						// resources back in the Vector
						arrayOfAssigResources.Add(alreadyAssigRes);

						// note that the SimProcess is allocating resources already
						// from this
						// resource pool
						simProcAlreadyAlloc = true;

					} // end inner if
					else
						;
				} // end for

				// make a new entry in the existing Vector (of this SimProcess) if
				// there is none already
				if (!simProcAlreadyAlloc) {
					arrayOfAssigResources.Add(assigResources);
				}

				else
					; // nothing in this else branch

				// put the updated Vector back in the Hashtable
				_assignmentTable[resourcePool]= arrayOfAssigResources;

			} // end outer if

			else { // there is no entry for this Res pool in the assignment
				   // hashtable
				   // make a new Vector as a new entry in the assignment hashtable
				var resPoolVector = new List<AssignedResources>();
				// insert the pair (SimProcess/number of assigned resources) in
				// the Vector
				resPoolVector.Add(assigResources);
				// store the Vector as a new entry of the Res pool in the
				// assignment hashtable
				_assignmentTable[resourcePool] = resPoolVector;
			} // end outer else

			// update the effective available capacity of the Res pool
			// effCapacity.put( resourcePool, new Integer( resourcePool.getAvail() )
			// );

			// for debugging purposes
			if (debugIsOn())
				sendDebugNote(this.toHtmlString());
		}

		/// <summary>
		/// Makes an entry in the resource data base when a SimProcess is requesting a number of resources from a
		/// resource pool (<c>Res</c>). The resource pool and the number of requested resources will be saved in
		/// the request hashtable. The SimProcess is the key to this hashtable.
		/// </summary>
		/// <param name="requestingProcess">The Simprocess which is requesting the resources.</param>
		/// <param name="resourcePool">The resource pool which should provide the resources.</param>
		/// <param name="quantity">The number of resources the SimProcess wants to get.</param>
		public void noteResourceRequest(SimProcess requestingProcess,
				Res resourcePool, int quantity) {
			_where = "protected void noteResourceRequest(SimProcess requestingProcess, "
					+ "Res resourcePool, int quantity)";

			// checks for null references and negative quantity
			if (!checkProcess(requestingProcess, _where))
				return; // if the SimProcess is not valid just return

			if (!checkRes(resourcePool, _where))
				return; // if the Res is not valid just return

			if (quantity <= 0) {
				sendWarning("Attempt to insert a negative or zero quantity into a "
						+ "ResourceDB . The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"The given quantity is zero or negative.",
						"Make sure to only use positive quantities which are more than "
								+ "nothing.");
				return; // if the quantity is negative or zero just return
			}

			// make the RequestedResources object which will be inserted in the
			// request hashtable, see innerclass RequestedResources
			RequestedResources reqResources = new RequestedResources(resourcePool,
					quantity);
			// is for the given SimProcess existing an entry in the
			// request hashtable already?
			if (_requestTable.ContainsKey(requestingProcess)) // there is an entry
															  // ...
			{
				sendWarning(
						"Attempt to make a SimProcess request resources from "
								+ "more than one resource pool. The attempted action is ignored!",
						"ResourceDB Method: " + _where,
						"A SimProcess can request resource from only one resource pool, "
								+ "because then he is blocked until he gets the desired resources.",
						"The SimProcess '"
								+ requestingProcess
								+ "' should be blocked in "
								+ "a queue for some other resource request already. Don't manipulate "
								+ "these internal queues.");
				return; // if the process is requesting other resources already
			}

			// insert the RequestedResources pair (resource pool/#requested
			// resources)
			// in the request hashtable
			_requestTable[requestingProcess]= reqResources;

			// for debugging purposes
			if (debugIsOn())
				sendDebugNote(this.toHtmlString());
		}

		/// <summary>
		/// Calculates the new effective available capacity of all resources the given SimProcess holds units from.
		/// The given SimProcess will return the resources he holds in the future, so the returning resource units
		/// will be added to the effective available capacity of the resources.
		/// </summary>
		/// <param name="process">
		/// the SimProcess which will return the resources it holds in the future. So the returning resource units
		/// can be added to the effective available capacity of the resources.
		/// </param>
		private void reduce(SimProcess process) {

			// get the Vector of all the resources this SimProcess holds at the
			// moment
			var usedResVec = process.getUsedResources();

			// for every resource unit the SimProcess holds
			foreach (var resource in usedResVec) {
				// get the resource
				//Resource resource = e.nextElement();

				// get the Res pool the resource belongs to
				Res resPool = resource.getResPool();

				// increment the effective available capacity of the Res pool
				int effCap = _effCapacity[resPool];
				effCap++;

				// save the newly calculated effective capacity in the hashtable
				_effCapacity[resPool]=effCap;
			} // end for

		}

		/// <summary>
		/// Creates and sends a debugnote to the experiment's messagedistributor. Debugnotes express the internal
		/// state of the ResourceDB to visualize the changes of state to help find bugs. The information about the
		/// simulation time is extracted from the experiment and must not be given as a parameter.
		/// </summary>
		/// <param name="description">
		/// The description of the ResourceDB's internal state to be passed with this debugnote
		/// </param>
		protected void sendDebugNote(string description) {

			sendMessage(new DebugNote(_owner.getModel(), _owner.getSimClock()
					.getTime(), "resource database", description));

		}

		/// <summary>Sends a message to the experiment or writes it to standard output when no experiment is connected.</summary>
		/// <param name="m">The message to send.</param>
		protected void sendMessage(Message m) {

			if (m == null) {
				sendWarning("Can't send Message!",
						"ResourceDB Method: SendMessage(Message m)",
						"The Message given as parameter is a null reference.",
						"Be sure to have a valid Message reference.");
				return; // no proper parameter
			}

			if (_owner != null) { // is ResourceDB connected to Experiment?
				_owner.getMessageManager().receive(m);
				return;
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
		protected void sendTraceNote(string description) {

			sendMessage(new TraceNote(_owner.getModel(), description, _owner
					.getSimClock().getTime(), _owner.getScheduler()
					.getCurrentEntity(), _owner.getScheduler().getCurrentEvent()));

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
		protected void sendWarning(string description, string location,
				string reason, string prevention) {

			// compose the ErrorMessage and send it in one command
			sendMessage(new ErrorMessage(_owner.getModel(), description, location,
					reason, prevention, _owner.getSimClock().getTime()));

		}

		/// <summary>
		/// Returns a string representation of the resource database with some HTML formatting tags. Lists first
		/// for every SimProcess all the resources he is requesting and second for every resource pool (<c>Res</c>)
		/// all the SimProcesses that are using resources from that resource pool. This information is useful for
		/// debugging purposes.
		/// </summary>
		/// <returns>The string representation of this resource database.</returns>
		public string toHtmlString() {

			// make a StringBuffer to store all the information
			var buffer = new StringBuilder(); // strings

			buffer.Append("<u>List of all SimProcesses requesting resources:</u>");

			// list for every SimProcess all the resources he is requesting
			if (_requestTable.Count==0) { // no SimProcess is requesting any
										   // resources
				buffer.Append("<br>---   (empty)   ---<br>");
			} else { // there are some SimProcesses requesting some resources

				// loop through all the SimProcesses waiting on resources
				foreach (var  waitingProcs in _requestTable.Keys) {
					SimProcess process = waitingProcs;
					buffer.Append("<br>SimProcess '" + process.toString()
							+ "' is waiting on: " + "<ul>"); // start indent list

					RequestedResources reqRes = (RequestedResources)_requestTable[process];

					if (reqRes == null) {
						buffer.Append("nothing!");
					} else {
						buffer.Append(reqRes.getRequestedUnits() + " resource(s) "
								+ "from '" + reqRes.getResPool().toString()
								+ "'<br>");
					}

					buffer.Append("</ul>"); // end indent list

				} // end for loop through all SimProcesses
			} // end all the entries in the requestTable

			// list for every resource pool all the SimProcesses holding his
			// resources
			buffer
					.Append("<u>List of all resource pools with resources used at the "
							+ "moment:</u>");

			if (_assignmentTable.Count==0) {// no resources are allocated by any
											 // process
				buffer.Append("<br>---   (empty)   ---<br>");
			} else { // list the resources with their allocating processes

				// loop through all the resource pools used at the moment
				foreach (var resPool in _assignmentTable.Keys) {
					
					buffer.Append("<br>resource pool '" + resPool.toString()
							+ "' is " + "providing: <ul>"); // start indent list

					// get the Vector holding all the pairs of
					// (SimProcess/allocated resources) for this resource pool
					var allocVector = _assignmentTable[resPool];

					if (allocVector == null || allocVector.Count==0) {
						buffer.Append("nothing!");
					} else {
						// loop through the Vector
						foreach (var assignedRes in allocVector) {
							
							buffer
									.Append(assignedRes.getSeizedUnits()
											+ " resource(s) " + "to '"
											+ assignedRes.getProcess().toString()
											+ "'<br>");
						}
					}

					buffer.Append("</ul>"); // end indent list

				} // end for loop through all resource pools

			} // end outer else

			return buffer.ToString();
		}
	}

    // ****** inner class ******

    /**
	 * AssignedResources is an inner class of <code>ResourceDB</code> to
	 * encapsulate the pairs of: SimProcess and the number of resources it
	 * holds. These pairs are stored in the vector
	 * <code>arrayOfAssignedResources</code>.
	 */
    public class AssignedResources
    {

        // ****** attributes of inner class ******

        /**
		 * The SimProcess using the resources at the moment.
		 */
        private SimProcess process;

        /**
		 * The number of resources seized by the SimProcess.
		 */
        private int seizedUnits;

        // ****** methods of inner class ******

        /// <summary>Constructor for a AssignedResources object.</summary>
        /// <param name="sProc">The SimProcess holding the resources.</param>
        /// <param name="seizedRes">The number of resources occupied by the SimProcess.</param>
        public AssignedResources(SimProcess sProc, int seizedRes)
        {
            // init variables
            this.process = sProc;
            this.seizedUnits = seizedRes;
        }

        /// <summary>Returns the SimProcess which holds a number of resources.</summary>
        /// <returns>The SimProcess which holds a number of resources.</returns>
        public SimProcess getProcess()
        {
            return this.process;
        }

        /// <summary>Returns the number of resources seized by the SimProcess.</summary>
        /// <returns>The number of resources seized by the SimProcess.</returns>
        public int getSeizedUnits()
        {
            return this.seizedUnits;
        }

        /// <summary>Sets the number of resources seized by the SimProcess to the given value.</summary>
        /// <param name="newQuantity">The new number of resources held by the SimProcess.</param>
        public void setSeizedUnits(int newQuantity)
        {
            this.seizedUnits = newQuantity;
        }

    } // end inner class

    // ****** inner class ******

    /**
	 * RequestedResources is an inner class of <code>ResourceDB</code> to
	 * encapsulate the pairs of: a resource pool <code>Res</code> and the
	 * number of resources requested by a SimProcess. These pairs are stored in
	 * the vector <code>arrayOfRequestedResources</code>.
	 */
    public class RequestedResources
    {

        // ****** attributes of inner class ******

        /**
		 * The Res from which the resources are requested.
		 */
        private Res resourcePool;

        /**
		 * The number of resources requested from the resource pool Res.
		 */
        private int requestedUnits;

        // ****** methods of inner class ******

        /// <summary>Constructor for a RequestedResources object.</summary>
        /// <param name="resPool">The resource pool from which resources are requested.</param>
        /// <param name="requestedRes">
        /// The number of resources requested from the resource pool <c>Res</c>.
        /// </param>
        public RequestedResources(Res resPool, int requestedRes)
        {
            // init variables
            this.resourcePool = resPool;
            this.requestedUnits = requestedRes;
        }

        /// <summary>
        /// Returns the resource pool <c>Res</c> from which a number of resources are requested.
        /// </summary>
        /// <returns>The resource pool <c>Res</c> from which a number of resources are requested.</returns>
        public Res getResPool()
        {
            return this.resourcePool;
        }

        /// <summary>Returns the number of resources requested from the resource pool <c>Res</c>.</summary>
        /// <returns>The number of resources requested from the resource pool <c>Res</c>.</returns>
       public int getRequestedUnits()
        {
            return this.requestedUnits;
        }

        /// <summary>
        /// Sets the number of resources requested from a certain resource pool <c>Res</c> to the given
        /// value.
        /// </summary>
        /// <param name="newQuantity">
        /// The new number of resources requested from the resource pool <c>Res</c>.
        /// </param>
        public void setRequestedUnits(int newQuantity)
        {
            this.requestedUnits = newQuantity;
        }

    } // end inner class

    // ****** methods ******
}