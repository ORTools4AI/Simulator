// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>Regression</code> class is producing a linear regression for two
     * <code>ValueSupplier</code> objects called x and y.
     */

	public class Regression : StatisticObject {

		// ****** attributes ******

		/**
		 * Values have changed considerably only, if they have changed for more than
		 * this constant.
		 */
		protected static readonly double C_EPSILON = 0.00001;

		/**
		 * The ValueSupplier for the x-Value.
		 */
		private ValueSupplier _xSupplier;

		/**
		 * The ValueSupplier for the y-Value.
		 */
		private ValueSupplier _ySupplier;

		/**
		 * The name of the ValueSupplier for the x-Value
		 */
		private String _xName;

		/**
		 * The name of the ValueSupplier for the y-Value
		 */
		private String _yName;

		/**
		 * The actual value of the x-variable
		 */
		private double _x;

		/**
		 * The actual value of the y-variable
		 */
		private double _y;

		/**
		 * The sum of all x-values so far
		 */
		private double _sumX;

		/**
		 * The sum of all y-values so far
		 */
		private double _sumY;

		/**
		 * The sum of all x-squares so far
		 */
		private double _sumSquareX;

		/**
		 * The sum of all y-squares so far
		 */
		private double _sumSquareY;

		/**
		 * The sum of x times y for all the pairs of x and y values so far
		 */
		private double _sumXtimesY;

		// ****** methods ******

		/**
		 * Constructor for a Regression object with no names for the x and y values.
		 * The names will be set to "X" and "Y".
		 */
		public Regression(Model ownerModel, string name, ValueSupplier xValsup,
				ValueSupplier yValsup, bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			//super(ownerModel, name, showInReport, showInTrace); // StatisticObject

			// xValsup is no valid ValueSupplier
			if (xValsup == null) {
				sendWarning(
						"Attempt to produce a Regression analysis about a non "
								+ "existing ValueSupplier for the x-value. The command will be ignored!",
						"Regression: "
								+ this.getName()
								+ " Constructor: Regression(Model "
								+ "ownerModel, String name, String xName, String yName, "
								+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
								+ "showInReport, boolean showInTrace)",
						"The given ValueSupplier: xValsup is only a null pointer.",
						"Make sure to pass a valid x-ValueSupplier when constructing a new "
								+ "Regression object.");

				return; // just return
			}

			// yValsup is no valid ValueSupplier
			if (yValsup == null) {
				sendWarning(
						"Attempt to produce a Regression analysis about a non "
								+ "existing ValueSupplier for the y-value. The command will be ignored!",
						"Regression: "
								+ this.getName()
								+ " Constructor: Regression(Model "
								+ "ownerModel, String name, String xName, String yName, "
								+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
								+ "showInReport, boolean showInTrace)",
						"The given ValueSupplier: yValsup is only a null pointer.",
						"Make sure to pass a valid y-ValueSupplier when constructing a new "
								+ "Regression object.");

				return; // just return
			}

			this._xName = "X";
			this._yName = "Y";

			this._xSupplier = xValsup;
			this._ySupplier = yValsup;

			// this Regression will observe the ValueSuppliers of the x and y values
			//_xSupplier.addObserver(this);
			//_ySupplier.addObserver(this);

			this._x = this._sumX = this._sumSquareX = 0.0;
			this._y = this._sumY = this._sumSquareY = this._sumXtimesY = 0.0;
		}

		/// <summary>
		/// Constructor for a Regression object that is observing two <c>ValueSuppliers</c> for the x and y values.
		/// </summary>
		/// <param name="ownerModel">The model this Regression object is associated to</param>
		/// <param name="name">The name of this Regression object</param>
		/// <param name="xName">The name for the x values.</param>
		/// <param name="yName">The name for the y values.</param>
		/// <param name="xValsup">
		/// The ValueSupplier providing the x values for the regression analysis. The given ValueSupplier will be
		/// observed by this Regression object.
		/// </param>
		/// <param name="yValsup">
		/// The ValueSupplier providing the y values for the regression analysis. The given ValueSupplier will be
		/// observed by this Regression object.
		/// </param>
		/// <param name="showInReport">Flag for showing the report about this Regression.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Regression.</param>
		public Regression(Model ownerModel, string name, string xName,
				string yName, ValueSupplier xValsup, ValueSupplier yValsup,
				bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			//super(ownerModel, name, showInReport, showInTrace); // StatisticObject

			// xValsup is no valid ValueSupplier
			if (xValsup == null) {
				sendWarning(
						"Attempt to produce a Regression analysis about a non "
								+ "existing ValueSupplier for the x-value. The command will be ignored!",
						"Regression: "
								+ this.getName()
								+ " Constructor: Regression(Model "
								+ "ownerModel, String name, String xName, String yName, "
								+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
								+ "showInReport, boolean showInTrace)",
						"The given ValueSupplier: xValsup is only a null pointer.",
						"Make sure to pass a valid x-ValueSupplier when constructing a new "
								+ "Regression object.");

				return; // just return
			}

			// yValsup is no valid ValueSupplier
			if (yValsup == null) {
				sendWarning(
						"Attempt to produce a Regression analysis about a non "
								+ "existing ValueSupplier for the y-value. The command will be ignored!",
						"Regression: "
								+ this.getName()
								+ " Constructor: Regression(Model "
								+ "ownerModel, String name, String xName, String yName, "
								+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
								+ "showInReport, boolean showInTrace)",
						"The given ValueSupplier: yValsup is only a null pointer.",
						"Make sure to pass a valid y-ValueSupplier when constructing a new "
								+ "Regression object.");

				return; // just return
			}

			this._xName = xName;
			this._yName = yName;

			this._xSupplier = xValsup;
			this._ySupplier = yValsup;

			// this Regression will observe the ValueSuppliers of the x and y values
			//_xSupplier.addObserver(this);
			//_ySupplier.addObserver(this);

			if ((xName != null && xName.Length == 0) || xName == null) // no proper name for x
			{
				sendWarning("The x-value of a regression analysis has no name. "
						+ "It will be named: 'X'!", "Regression: " + this.getName()
						+ " Constructor: Regression(Model "
						+ "ownerModel, String name, String xName, String yName, "
						+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
						+ "showInReport, boolean showInTrace)",
						"A x-value with no name is hard to find back.",
						"Make sure to give the x-value a useful name.");

				this._xName = "X";
			}

			if ((yName != null && yName.Length == 0) || yName == null) // no proper name for y
			{
				sendWarning("The y-value of a regression analysis has no name. "
						+ "It will be named: 'Y'!", "Regression: " + this.getName()
						+ " Constructor: Regression(Model "
						+ "ownerModel, String name, String xName, String yName, "
						+ "ValueSupplier xValsup, ValueSupplier yValsup, boolean "
						+ "showInReport, boolean showInTrace)",
						"A y-value with no name is hard to find back.",
						"Make sure to give the y-value a useful name.");

				this._yName = "Y";
			}

			this._x = this._sumX = this._sumSquareX = 0.0;
			this._y = this._sumY = this._sumSquareY = this._sumXtimesY = 0.0;
		}

		/// <summary>
		/// Constructor for a Regression object that is NOT connected to any <c>ValueSuppliers</c> for the x and y
		/// values.
		/// </summary>
		/// <param name="ownerModel">The model this Regression object is associated to</param>
		/// <param name="name">The name of this Regression object</param>
		/// <param name="xName">The name for the x values.</param>
		/// <param name="yName">The name for the y values.</param>
		/// <param name="showInReport">Flag for showing the report about this Regression.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Regression.</param>
		public Regression(Model ownerModel, string name, string xName,
				string yName, bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			//super(ownerModel, name, showInReport, showInTrace); // StatisticObject

			this._xName = xName;
			this._yName = yName;

			this._xSupplier = null;
			this._ySupplier = null;

			if ((xName != null && xName.Length == 0) || xName == null) // no proper name for x
			{
				sendWarning("The x-value of a regression analysis has no name. "
						+ "It will be named: 'X'!", "Regression: " + this.getName()
						+ " Constructor: Regression(Model "
						+ "ownerModel, String name, String xName, String yName, "
						+ "boolean showInReport, boolean showInTrace)",
						"A x-value with no name is hard to find back.",
						"Make sure to give the x-value a useful name.");

				this._xName = "X";
			}

			if ((yName != null && yName.Length == 0) || yName == null) // no proper name for y
			{
				sendWarning("The y-value of a regression analysis has no name. "
						+ "It will be named: 'Y'!", "Regression: " + this.getName()
						+ " Constructor: Regression(Model "
						+ "ownerModel, String name, String xName, String yName, "
						+ "boolean showInReport, boolean showInTrace)",
						"A y-value with no name is hard to find back.",
						"Make sure to give the y-value a useful name.");

				this._yName = "Y";
			}

			this._x = this._sumX = this._sumSquareX = 0.0;
			this._y = this._sumY = this._sumSquareY = this._sumXtimesY = 0.0;
		}

		/// <summary>Returns the correlation coefficient.</summary>
		/// <returns>The correlation coefficient.</returns>
		public double correlationCoeff() {
			long n = getObservations(); // get the number of observations so far

			if (n <= 5) // not enough data yet
			{
				sendWarning(
						"Attempt to get the correlation coefficient, but there "
								+ "is insufficient data yet to calculate it. UNDEFINED (-1.0) "
								+ "will be returned!", "Regression: "
								+ this.getName() + " Method: double "
								+ "correlationCoeff().",
						"The correlation coefficient can not be calculated, because there "
								+ "is insufficient data collected so far.",
						"Make sure to ask for the correlation coefficient only after "
								+ "enough data has been collected.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double dx = Math.Abs(n * _sumSquareX - _sumX * _sumX);
			double dy = Math.Abs(n * _sumSquareY - _sumY * _sumY);

			if (dx < C_EPSILON || dy < C_EPSILON) // not changed considerably
			{
				sendWarning(
						"The x or y values have not changed considerably. The "
								+ "data seems to be degenerated. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName() + " Method: double "
								+ "correlationCoeff().",
						"The x or y values have not changed considerably. Some failure "
								+ "might have occured.",
						"One or both values are almost constant. It seems that nothing "
								+ "really happens. Check that!");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double squareDiff = (n * _sumXtimesY - _sumX * _sumY);

			// calculate the rounded result
			double rndResult = round(squareDiff * squareDiff / (dx * dy));
			return rndResult;
		}

		/// <summary>Returns a Reporter to produce a report about this Regression analysis.</summary>
		/// <returns>The Reporter for this Regression.</returns>
		protected override Reporter createDefaultReporter() {
			return new RegressionReporter(this);
		}

		/// <summary>Returns the mean x-value.</summary>
		/// <returns>The mean x-value.</returns>
		public double getXMean() {
			long n = getObservations(); // get the number of observations so far

			if (n == 0) // nothing observed yet
			{
				sendWarning(
						"Attempt to get a mean x-value, but there is not "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double getXMean()",
						"You can not calculate a mean value as long as no data is "
								+ "collected.",
						"Make sure to ask for the mean value only after some data "
								+ "has been collected already.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the rounded result
			double rndResult = StatisticObject.round(_sumX / n);

			return rndResult;
		}

		/// <summary>Returns the name of the x-value.</summary>
		/// <returns>The name of the x-value.</returns>
		public String getXName() {
			return this._xName;
		}

		/// <summary>Returns the actual value of the <c>ValueSupplier</c> supplying the x-value.</summary>
		/// <returns>The actual value of the observed <c>ValueSupplier</c> object supplying the x-value.</returns>
		public double getXValue() {
			if (_xSupplier == null) // no x-Supplier observed
			{
				sendWarning(
						"Attempt to get a x-value, but there is no "
								+ "ValueSupplier for x values. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double getXValue()",
						"A x value can not be provided, because there is no x "
								+ "ValueSupplier observed from which the value could be fetched.",
						"Make sure to ask for the x value only if there is a x "
								+ "ValueSupplier observed.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			return _xSupplier.value();
		}

		/// <summary>Returns the mean y-value.</summary>
		/// <returns>The mean y-value.</returns>
		public double getYMean() {
			long n = getObservations(); // get the number of observations so far

			if (n == 0) // nothing observed yet
			{
				sendWarning(
						"Attempt to get a mean y-value, but there is not "
								+ "sufficient data yet. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double getYMean()",
						"You can not calculate a mean value as long as no data is "
								+ "collected.",
						"Make sure to ask for the mean value only after some data "
								+ "has been collected already.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the rounded result
			double rndResult = StatisticObject.round(_sumY / n);

			return rndResult;
		}

		/// <summary>Returns the name of the y-value.</summary>
		/// <returns>The name of the y-value.</returns>
		public String getYName() {
			return this._yName;
		}

		/// <summary>Returns the actual value of the <c>ValueSupplier</c> supplying the y-value.</summary>
		/// <returns>The actual value of the observed <c>ValueSupplier</c> object supplying the y-value.</returns>
		public double getYValue() {
			if (_ySupplier == null) // no y-Supplier observed
			{
				sendWarning(
						"Attempt to get a y-value, but there is no "
								+ "ValueSupplier for y values. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double getYValue()",
						"A y value can not be provided, because there is no y "
								+ "ValueSupplier observed from which the value could be fetched.",
						"Make sure to ask for the y value only if there is a y "
								+ "ValueSupplier observed.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			return _ySupplier.value();
		}

		/// <summary>Returns the interception of the X-axis.</summary>
		/// <returns>The interception of the X-axis.</returns>
		public double intercept() {
			long n = getObservations(); // get the number of observations so far

			if (n <= 5) // not enough data yet
			{
				sendWarning(
						"Attempt to get the interception of the X-axis, but "
								+ "there is insufficient data yet to calculate it. UNDEFINED "
								+ "(-1.0) will be returned!", "Regression: "
								+ this.getName() + " Method: double intercept()",
						"The interception of the X-axis can not be calculated, because "
								+ "there is insufficient data collected so far.",
						"Make sure to ask for the interception of the X-axis only after "
								+ "enough data has been collected.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double dx = Math.Abs(n * _sumSquareX - _sumX * _sumX);
			double dy = Math.Abs(n * _sumSquareY - _sumY * _sumY);

			if (dx < C_EPSILON || dy < C_EPSILON) // not changed considerably
			{
				sendWarning(
						"The x or y values have not changed considerably. The "
								+ "data seems to be degenerated. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double intercept()",
						"The x or y values have not changed considerably. Some failure "
								+ "might have occured.",
						"One or both values are almost constant. It seems that nothing "
								+ "really happens. Check that!");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the rounded result
			double rndResult = round((_sumY * _sumSquareX - _sumX * _sumXtimesY) / dx);
			return rndResult;
		}

		/// <summary>Returns the regression coefficient.</summary>
		/// <returns>The regression coefficient.</returns>
		public double regCoeff() {
			long n = getObservations(); // get the number of observations so far

			if (n <= 5) // not enough data yet
			{
				sendWarning(
						"Attempt to get the regression coefficient, but there "
								+ "is insufficient data yet to calculate it. UNDEFINED (-1.0) "
								+ "will be returned!", "Regression: "
								+ this.getName() + " Method: double regCoeff()",
						"The regression coefficient can not be calculated, because there "
								+ "is insufficient data collected so far.",
						"Make sure to ask for the regression coefficient only after "
								+ "enough data has been collected.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double dx = Math.Abs(n * _sumSquareX - _sumX * _sumX);
			double dy = Math.Abs(n * _sumSquareY - _sumY * _sumY);

			if (dx < C_EPSILON || dy < C_EPSILON) // not changed considerably
			{
				sendWarning(
						"The x or y values have not changed considerably. The "
								+ "data seems to be degenerated. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double regCoeff()",
						"The x or y values have not changed considerably. Some failure "
								+ "might have occured.",
						"One or both values are almost constant. It seems that nothing "
								+ "really happens. Check that!");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the rounded result
			double rndResult = round((n * _sumXtimesY - _sumX * _sumY) / dx);
			return rndResult;
		}

		/**
		 * Resets this Regression object by resetting all variables of x and y to
		 * 0.0 . The names remain the same.
		 */
		public void reset() {
			base.reset(); // reset the StatisticObject, too.

			this._x = this._sumX = this._sumSquareX = 0.0;
			this._y = this._sumY = this._sumSquareY = this._sumXtimesY = 0.0;
		}

		/// <summary>Returns the residuale standard deviation.</summary>
		/// <returns>The residuale standard deviation.</returns>
		public double residualStdDev() {
			long n = getObservations(); // get the number of observations so far

			if (n <= 5) // not enough data yet
			{
				sendWarning(
						"Attempt to get the residual standard deviation, but "
								+ "there is insufficient data yet to calculate it. UNDEFINED "
								+ "(-1.0) will be returned!", "Regression: "
								+ this.getName() + " Method: double "
								+ "residualStdDev()",
						"You can not calculate the residual standard deviation as long "
								+ "as no sufficient data is collected.",
						"Make sure to ask for the residual standard deviation only "
								+ "after enough data has been collected.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double dx = Math.Abs(n * _sumSquareX - _sumX * _sumX);
			double dy = Math.Abs(n * _sumSquareY - _sumY * _sumY);

			if (dx < C_EPSILON || dy < C_EPSILON) // not changed considerably
			{
				sendWarning(
						"The x or y values have not changed considerably. The "
								+ "data seems to be degenerated. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName() + " Method: double "
								+ "residualStdDev()",
						"The x or y values have not changed considerably. Some failure "
								+ "might have occured.",
						"One or both values are almost constant. It seems that nothing "
								+ "really happens. Check that!");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the residual std. deviation
			double result = Math.Sqrt(Math.Abs(_sumSquareY - intercept() * _sumY
					- regCoeff() * _sumXtimesY) / (n - 2));
			// calculate the rounded result
			double rndResult = round(result);
			return rndResult;
		}

		/// <summary>Returns the standard deviation of the regression coefficients.</summary>
		/// <returns>The standard deviation of the regression coefficients.</returns>
		public double stdDevRegCoeff() {
			long n = getObservations(); // get the number of observations so far

			if (n <= 5) // not enough data yet
			{
				sendWarning(
						"Attempt to get the standard deviation of the "
								+ "regression coefficients, but there is insufficient data yet "
								+ "to calculate it. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double stdDevRegCoeff()",
						"The standard deviation of the regression coefficients can not be "
								+ "calculated, because there is insufficient data collected so far.",
						"Make sure to ask for the standard deviation of the regression "
								+ "coefficients only after enough data has been collected.");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			double dx = Math.Abs(n * _sumSquareX - _sumX * _sumX);
			double dy = Math.Abs(n * _sumSquareY - _sumY * _sumY);

			if (dx < C_EPSILON || dy < C_EPSILON) // not changed considerably
			{
				sendWarning(
						"The x or y values have not changed considerably. The "
								+ "data seems to be degenerated. UNDEFINED (-1.0) will be returned!",
						"Regression: " + this.getName()
								+ " Method: double stdDevRegCoeff()",
						"The x or y values have not changed considerably. Some failure "
								+ "might have occured.",
						"One or both values are almost constant. It seems that nothing "
								+ "really happens. Check that!");

				return UNDEFINED; // return UNDEFINED = -1.0
			}

			// calculate the standard deviation Reg. Coeff.
			double result = n * residualStdDev() / Math.Sqrt((n - 2) * dx);

			// calculate the rounded result
			double rndResult = round(result);
			return rndResult;
		}

		/// <summary>
		/// Updates this <c>Regression</c> object by fetching the actual values of the <c>ValueSupplier</c> s and
		/// processing them. The <c>ValueSupplier</c> s are passed in the constructor of this <c>Regression</c>
		/// object. This <c>update()</c> method complies with the one described in DESMO, see [Page91].
		/// </summary>
		public void update() {
			// not connected to a x- OR y-ValueSupplier
			if (_xSupplier == null || _ySupplier == null) {
				sendWarning(
						"Attempt to update a Regression analysis without "
								+ "providing any x- or y-value. Which value(s) should be used to "
								+ "update the Regression statistic? The command will be ignored!",
						"Regression: " + this.getName() + " Method: update () ",
						"The given ValueSupplier: xSupplier or ySupplier is only a null "
								+ "pointer.",
						"Make sure to pass a valid x-ValueSupplier and y-ValueSupplier "
								+ "when constructing a new Regression object. Or use the update "
								+ "(double xVal, double yVal) method.");

				return; // just return
			}

			this._x = _xSupplier.value(); // get the x-value

			this._y = _ySupplier.value(); // get the y-value

			_sumX += _x; // update the variables...
			_sumY += _y;
			_sumSquareX += _x * _x;
			_sumSquareY += _y * _y;
			_sumXtimesY += _x * _y;

			incrementObservations(); // increment the observations (see
									 // Reportable)

			traceUpdate(); // leave a message in the trace
		}

		/// <summary>
		/// Updates this <c>Regression</c> object with the x and y double values given as parameters. In some cases
		/// it might be more convenient to pass the values this <c>Regression</c> will be updated with directly
		/// within the <c>update(double xVal, double yVal)</c> method instead of going via the
		/// <c>ValueSupplier</c>.
		/// </summary>
		/// <param name="xVal">The x value with which this <c>Regression</c> will be updated.</param>
		/// <param name="yVal">The y value with which this <c>Regression</c> will be updated.</param>
		public void update(double xVal, double yVal) {

			this._x = xVal; // get the x-value

			this._y = yVal; // get the y-value

			_sumX += _x; // update the variables...
			_sumY += _y;
			_sumSquareX += _x * _x;
			_sumSquareY += _y * _y;
			_sumXtimesY += _x * _y;

			incrementObservations(); // increment the observations (see
									 // Reportable)

			traceUpdate(); // leave a message in the trace
		}

		/// <summary>
		/// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
		/// interface. This method will be called automatically from an <c>Observable</c> object within its
		/// <c>notifyObservers()</c> method. Whenever either of the two <c>ValueSupplier</c> objects providing the
		/// x or y value is updated and calling <c>notifyStatistics()</c> the x and the y value will be fetched and
		/// a new regression will be calculated.
		/// </summary>
		/// <param name="o">
		/// The Observable calling this method within its own <c>notifyObservers()</c> method.
		/// </param>
		/// <param name="arg">The Object with which this <c>Regression</c> is updated.</param>
		public void update(Observable o, Object arg) {
			if (o == null) // null was passed instead of an Observable
			{
				sendWarning(
						"Attempt to update a Regression with no reference to "
								+ "an Observable. The x-value of '"
								+ _xSupplier.getName() + "' and "
								+ "the y-value of '" + _ySupplier.getName()
								+ "'will be fetched and " + "processed anyway!",
						"Regression: " + this.getName()
								+ " Method: update (Observable o," + " Object arg)",
						"The passed Observable: o in this method is only a null pointer.",
						"The update()-method was not called via notifyObservers() from an "
								+ "Observable. Who was calling it? Why don't you let the Observable "
								+ "do the work?");
			}

			// update was called from xSupplier
			if (o == _xSupplier) {
				if (arg == null) // notifyStatistics() was called with no arg
				{
					this._x = _xSupplier.value(); // get the x-value
				} else {
					if (arg is double) // actual x value passed in arg?
				{
						this._x = convertToDouble(arg); // get the x-value out of
														// the
														// arg
					} else {
						sendWarning(
								"Attempt to update a Regression with an argument arg,"
										+ " that could not be recognized. The attempted action is ignored!",
								"Regression: " + this.getName()
										+ " Method: update (Observable "
										+ "o, Object arg)",
								"The passed Object in the argument arg could not be recognized.",
								"Make sure to pass null or a Number object as the arg argument");

						return; // do nothing, just return
					}
				}

				this._y = _ySupplier.value(); // get the y-value
			} // end update was called from xSupplier

			// update was called from ySupplier
			if (o == _ySupplier) {
				if (arg == null) // notifyStatistics() was called with no arg
				{
					this._y = _ySupplier.value(); // get the y-value
				} else {
					if (arg is double) // actual y value passed in arg?
				{
						this._y = convertToDouble(arg); // get the y-value out of
														// the
														// arg
					} else {
						sendWarning(
								"Attempt to update a Regression with an argument arg,"
										+ " that could not be recognized. The attempted action is ignored!",
								"Regression: " + this.getName()
										+ " Method: update (Observable "
										+ "o, Object arg)",
								"The passed Object in the argument arg could not be recognized.",
								"Make sure to pass null or a Number object as the arg argument");

						return; // do nothing, just return
					}
				}

				this._x = _xSupplier.value(); // get the x-value
			} // end update was called from ySupplier

			// update was called from the SimClock
			if (o is SimClock) {
				this._x = _xSupplier.value(); // get the x-value

				this._y = _ySupplier.value(); // get the y-value

			} // end update was called from SimClock

			_sumX += _x; // update the variables...
			_sumY += _y;
			_sumSquareX += _x * _x;
			_sumSquareY += _y * _y;
			_sumXtimesY += _x * _y;

			incrementObservations(); // increment the observations (see
									 // Reportable)

			traceUpdate(); // leave a message in the trace
		}

		/// <summary>
		/// Returns <c>true</c> when the x-values are constant. That means no considerable change can be
		/// recognized.
		/// </summary>
		/// <returns>
		/// <c>true</c> is returned if no considerable change of the x-values can be recognized. <c>false</c>
		/// otherwise.
		/// </returns>
		public bool xIsConstant() {
			double dx = Math.Abs(getObservations() * _sumSquareX - _sumX * _sumX);

			return (dx < C_EPSILON);
		}

		/// <summary>
		/// Returns <c>true</c> when the y-values are constant. That means no considerable change can be
		/// recognized.
		/// </summary>
		/// <returns>
		/// <c>true</c> is returned if no considerable change of the y-values can be recognized. <c>false</c>
		/// otherwise.
		/// </returns>
		public bool yIsConstant() {
			double dy = Math.Abs(getObservations() * _sumSquareY - _sumY * _sumY);

			return (dy < C_EPSILON);
		}
	} // end class Regression
}
