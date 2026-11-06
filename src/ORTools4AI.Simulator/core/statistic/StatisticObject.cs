// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.engine;
using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * <code>StatisticObject</code> class is the super class of all
     * other classes collecting statistical data. <br>
     * It extends the <code>desmoj.core.simulator.Reportable</code> class so that it can provide
     * a reporter to represent the statistical data in the report. <br>
     * It also implements the <code>java.util.Observer</code> interface. So this
     * is the observer part of the observer pattern as described in [Gamm95] that is
     * observing the <code>desmoj.statistic.ValueSupplier</code>. Whenever the
     * <code>ValueSupplier</code> is changing it calls the
     * <code>update(Observable, Object)</code> method of this
     * <code>StatisticObject</code>. Which happens inside the
     * <code>notifyObservers()</code> method of the observable
     * <code>ValueSupplier</code>.
     * <p>
     * 
     * The virtual method <code>update(Observable, Object)</code> has to be
     * implemented in all derived classes so that the <code>StatisticObject</code>
     * is updated when the observed <code>ValueSupplier</code> has changed and its
     * <code>notifyObservers()</code> method is called from its
     * <code>notifyStatistics()</code> method.
     * <p>
     * 
     * @see desmoj.core.statistic.ValueSupplier
     */

	public abstract class StatisticObject : Reportable
	{

		// ****** attributes ******

		/**
		 * A unit (optional) / 21.11.12 extended bx cm
		 */
		private String unit;

		/**
		 * Represents the value returned in case of an error. If no valid value can
		 * be returned.
		 */
		public static readonly double UNDEFINED = -1.0;

		/**
		 * The number of digits after the decimal point which will be displayed for
		 * the numbers in the reports. A precision of more than that is obsolete.
		 */
		protected static readonly double FRACTION_DIGITS = 5.0;

		/**
		 * The number needed for rounding the results to the desired precision.
		 */
		protected static readonly double PRECISION = Math.Pow(10.0,
				FRACTION_DIGITS);

		// ****** methods ******

		/// <summary>Constructor for a StatisticObject, preliminarily without a unit assigned</summary>
		/// <param name="ownerModel">The model this StatisticObject is associated to.</param>
		/// <param name="name">The name of this StatisticObject</param>
		/// <param name="showInReport">Flag for showing the report about this StatisticObject.</param>
		/// <param name="showInTrace">Flag for showing this StatisticObject in trace files.</param>
		public StatisticObject(Model ownerModel, string name, bool showInReport,
				bool showInTrace) : base(ownerModel, name, showInReport, showInTrace)
		{

		}

		/// <summary>
		/// Converts an <c>Object</c> to a <c>double</c> -value. If the given object is not an instance of a number
		/// wrapper class a warning is produced and the UNDEFINED-value (-1.0) is returned. So the
		/// <c>StatisticObject</c> can be notified by a <c>ValueSupplier</c> with the <c>notifyStatistics(Object
		/// arg)</c> method, where the value is passed as an object in this method.
		/// </summary>
		/// <param name="obj">The Object which will be converted to a double value.</param>
		/// <returns>The double value the given object is converted to.</returns>
		protected double convertToDouble(object obj)
		{
			if (obj == null) // the given object is a null pointer
			{
				sendWarning("Attempt to convert a null pointer to a double value."
						+ " Zero (0.0) is returned!", "StatisticObject: "
						+ getName() + " Method: double convertToDouble"
						+ "(Object obj)",
						"A null pointer can not be converted to a double value. Zero is "
								+ "assumed and will be returned.",
						"Make sure not to pass a null pointer where an Object is expected.");

				return 0.0; // that makes sense (doesn't it?)
			}

			if (obj is byte)
			{
				return (double)((byte)obj);
			}

			if (obj is short)
			{
				return ((short)obj);
			}

			if (obj is int)
			{
				return (double)((int)obj);
			}

			if (obj is long)
			{
				return ((long)obj);
			}

			if (obj is float)
			{
				return ((float)obj);
			}

			if (obj is double)
			{
				return ((Double)obj);
			}

			// the given object is not an instance of a number wrapper class
			sendWarning(
					"Attempt to convert an object which is not a number wrapper"
							+ " class to a double value. The UNDEFINED value (-1.0) is returned!",
					"StatisticObject: " + getName()
							+ " Method: double convertToDouble" + "(Object obj)",
					"The given Object is an instance of the class: "
							+ obj.GetType().ToString()
							+ ". This can not be converted to a double.",
					"Make sure to use a number wrapper class for numeric values.");

			return UNDEFINED; // return (-1.0) in case of an error
		}

		/**
		 * Leaves a message in the trace that this StatisticObject has been updated.
		 */
		protected void traceUpdate()
		{
			if (currentlySendTraceNotes())
			{
				sendTraceNote("updates " + this.getQuotedName());
			} // tell in the trace which StatisticObject is updated
		}

		/// <summary>Rounds a double value with repect to the <c>PRECISION</c>.</summary>
		/// <param name="d">The value to be rounded</param>
		/// <returns>The rounded value.</returns>
		public static double round(double d)
		{
			return Math.Round(PRECISION * d) / PRECISION;
		}

		//   Extension by Chr. M&uuml;ller (TH Wildau) 28.11.12 -------------------------------------

		/**
		 * Set an optional unit of reported value. Default is null.
		 * This value is shown in description reports.
		 */
		public void setUnit(string unit)
		{
			this.unit = unit;
		}

		/// <summary>
		/// Get an optional unit of reported value. Default is null. This value is shown in description reports.
		/// </summary>
		/// <returns>The optional unit</returns>
		public string getUnit()
		{

			return this.unit;
		}

		/// <summary>
		/// Textually wraps the output of <c>getUnit()</c>, adding brackets and displaying "none" if case unit is
		/// <c>null</c>.
		/// </summary>
		public String getUnitText()
		{
			String unit = this.getUnit();
			if (unit == null || unit.Length == 0)
				return "none";
			else
				return "[" + unit + "]";
		}
	} // end class StatisticObject
}