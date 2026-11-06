// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * An ErrorMessage is used to signal invalid situations to the modeller. An
     * ErrorMessage consists of four Strings that intend to define the kind of error
     * that occurred and describe its reason and what a modeller can do to prevent
     * it. ErrorMessages describe types of errors that can be fixed by the DESMO-J
     * or just give hints on missing or unclear parameters where certain framework
     * assumptions are made. ErrorMessages report these to the modeller. Any type of
     * error consists of the following attributes which have to be set by the
     * ErrorMessage's constructor:
     * <ul>
     * <li>error description : The operation that just failed or does not have the
     * preconditions matched</li>
     * <li>error location : The class and method that the error occurred in</li>
     * <li>error reason : The probable reason why the error occured</li>
     * <li>error prevention : A hint on how to prevent this error to occur again.
     * </li>
     * <li>error time : The point of simulation time that the error occurred.</li>
     * </ul>
     * Note that ErrorMessages relate to the modeller's use of the framework's
     * methods and correct the found problems if possible by making assumptions.
     * These corrections and their assumptions are articulated by an ErrorMessage.
     * In contrast to <code>Exceptions</code> and fatal errors, an ErrorMessage
     * does not stop the simulation run or exit the Java runtime.
     */
	public class ErrorMessage : Message
	{

		/*
		 * The reason that caused the operation to fail. It is a description of the
		 * circumstances leading to the warning. Use not more than one short
		 * sentence.
		 */
		private String _errReason;

		/*
		 * Describes in a few sentences how the warning can be prevented in the
		 * future.
		 */
		private String _errPrevention;

		/**
		 * The location that this message was created as a String consisting of
		 * "Classname.Methodname".
		 */
		private string _errLocation;

		/// <summary>Creates an errormessage setting its parameters with the given values.</summary>
		/// <param name="origin">The model this errormessage evolved from</param>
		/// <param name="errorDescription">Description of the error</param>
		/// <param name="errorLocation">Class and method the error occured in</param>
		/// <param name="errorReason">The probable reason for the error</param>
		/// <param name="errorPrevention">A hint how to prevent the error</param>
		//TODO:
		public ErrorMessage(Model origin, string errorDescription,
				string errorLocation, string errorReason, string errorPrevention,
				TimeInstant errorTime) : base(origin, errorDescription, errorTime)
		{

			// create the Message
			//super(origin, errorDescription, errorTime);

			// set the new attributes
			_errLocation = errorLocation;
			_errReason = errorReason;
			_errPrevention = errorPrevention;

		}

		/// <summary>Returns a String describing class and method that this message was sent from.</summary>
		/// <returns>The location this message was sent from.</returns>
		public String getLocation()
		{

			return _errLocation;

		}

		/// <summary>
		/// Returns a String giving a hint on how to prevent this error to happen. Note that the hint given is
		/// based on the probable reason described but must not always have that reason.
		/// </summary>
		/// <returns>The probable reason for this error</returns>
		public String getPrevention()
		{

			return _errPrevention;

		}

		/// <summary>
		/// Returns the String describing the probable reason for the error to occur. Note that the reason given
		/// here is the most probable reason for the effect to occur here. It might have its origin in other
		/// classes, so bear in mind that the casue for the error might have been created in a class not obviously
		/// related to the location this error occured.
		/// </summary>
		/// <returns>The probable reason for this error</returns>
		public String getReason()
		{

			return _errReason;

		}
	}
}