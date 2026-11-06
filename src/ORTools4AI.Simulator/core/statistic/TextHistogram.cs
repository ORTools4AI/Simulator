// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Lorna Slawski
// @author based on the classes <code>Histogram</code> and <code>ValueStatistics</code> from Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>TextHistogram</code> class is providing a statistic analysis about
     * text values. Given Strings are counted and the report will show the amount of
     * every given text.
     * If the given parameter is a null pointer it will not be counted.
     */
	public class TextHistogram : Reportable, Observer {

		// ****** attributes ******

		/**
		 * The alphabetically sorted map. The keys are given by the Strings and their values are their amount.
		 */
		private Dictionary<string, long> _map;

		/**
		 * The ValueSupplier providing the Strings which will be processed.
		 */
		private TextSupplier _textSuppl;

		/**
		 * The last String received from the TextSupplier
		 */
		private string _lastString;

		/// <summary>
		/// Constructor for a TextHistogram object that will NOT be connected to a <c>TextSupplier</c>.
		/// </summary>
		/// <param name="ownerModel">The model this TextHistogram is associated to.</param>
		/// <param name="name">The name of this TextHistogram object.</param>
		/// <param name="showInReport">Flag for showing the report about this Histogram.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Histogram.</param>
		public TextHistogram(Model ownerModel, String name, bool showInReport,
				bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			// call the constructor of StatisticObject
			//super(ownerModel, name, showInReport, showInTrace);

			// no ValueSupplier will be observed
			this._textSuppl = null;

			// make a new table
			this._map = new Dictionary<string, long>();
		}

		/// <summary>
		/// Constructor for a TextHistogram object that will be connected to a <c>TextSupplier</c>.
		/// </summary>
		/// <param name="ownerModel">The model this TextHistogram is associated to.</param>
		/// <param name="name">The name of this TextHistogram object.</param>
		/// <param name="textSup">
		/// The TextSupplier providing the String for this TextHistogram. The given TextValueSupplier will be
		/// observed by this TextHistogram object.
		/// </param>
		/// <param name="showInReport">Flag for showing the report about this TextHistogram.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this TextHistogram.</param>
		public TextHistogram(Model ownerModel, string name, TextSupplier textSup,
				bool showInReport, bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			// call the constructor of StatisticObject


			// textSup is no valid TextSupplier
			if (textSup == null) {
				sendWarning(
						"Attempt to produce a TextHistogram about a non existing "
								+ "TextValueSupplier. The command will be ignored!",
								"TextHistogram: "
										+ this.getName()
										+ " Constructor: TextHistogram"
										+ " (Model ownerModel, String name, TextSupplier textSup, "
										+ "boolean showInReport, boolean showInTrace)",
										"The given TextSupplier: textSup is only a null pointer.",
										"Make sure to pass a valid TextSupplier when constructing a new "
												+ "TextHistogram object.");
				return; // just return
			}
			this._textSuppl = textSup;

			// this TextHistogram will observe the given TextValueSupplier
			this._textSuppl.addObserver(this);

			// make a new table
			this._map = new Dictionary<string, long>();
		}

		/// <summary>Returns a Reporter to produce a report about this TextHistogram.</summary>
		/// <returns>The Reporter for this TextHistogram.</returns>
		protected override Reporter createDefaultReporter() {
			return new TextHistogramReporter(this);
		}

		/// <summary>Returns the last observed String of this TextHistogram object.</summary>
		/// <returns>The last String observed so far.</returns>
		public String getLastString() {
			return this._lastString;
		}

		/// <summary>
		/// Returns the String with the least number of entries. Several Strings are separated by a comma. If there
		/// are no entries yet an empty String ("") is being returned.
		/// </summary>
		/// <returns>The String with the least number of entries.</returns>
		public String getLeastFrequentedString() {
			long leastValue = long.MaxValue; // largest possible value
			String leastStrings = "";
			long value;
			foreach (string str in this._map.Keys) {
				value = this._map[str];
				if (value < leastValue) {
					leastStrings = str;
					leastValue = value;
				}
				else if (value == leastValue) {
					leastStrings = leastStrings + ", " + str;
				}
			}
			return leastStrings;
		}

		/// <summary>
		/// Returns the String with the largest number of entries. Several Strings are separated by a comma. If
		/// there are no entries yet an empty String ("") is being returned.
		/// </summary>
		/// <returns>The String with the highest number of entries.</returns>
		public String getMostFrequentedString() {
			long mostValue = 0;
			String mostStrings = "";
			long value;
			foreach (string str in this._map.Keys) {
				value = this._map[str];
				if (value > mostValue) {
					mostStrings = str;
					mostValue = value;
				}
				else if (value == mostValue) {
					mostStrings = mostStrings + ", " + str;
				}
			}
			return mostStrings;
		}

		/// <summary>Returns the number of Strings observed.</summary>
		/// <returns>The number of Strings which have been observed.</returns>
		public int getNumberOfStringsObserved() {
			return this._map.Count;
		}

		/// <summary>Returns the number of observations for the given String.</summary>
		/// <param name="obsStr">The String of which want to get the number of observations made for.</param>
		/// <returns>The number of observations for the given String.</returns>
		public long getObservationsOfString(string obsStr) {
			if (this._map.ContainsKey(obsStr)) {
				return this._map[obsStr];
			}
			else {
				return 0;
			}

		}

		/// <summary>Returns an array of Strings observed.</summary>
		/// <returns>The array of Strings observed.</returns>
		public String[] getStringsObserved() {
			String[] observed = new String[getNumberOfStringsObserved()];
			int i = 0;
			foreach (string str in this._map.Keys) {
				observed[i] = str;
				i++;
			}
			return observed;
		}

		/// <summary>Returns the TextSupplier object providing all the Strings.</summary>
		/// <returns>The TextSupplier object providing the Strings for this TextHistogram.</returns>
		protected TextSupplier getTextSupplier() {
			return this._textSuppl;
		}

		/**
		 * Resets this TextHistogram object by deleting all observed Strings.
		 */
		public void reset() {
			base.reset();
			this._map.Clear();
		}

		/// <summary>
		/// Updates this <c>TextHistogram</c> object by fetching the actual value of the <c>TextSupplier</c> and
		/// processing it. If the actual value is a <c>null</c> pointer it will not be counted and an error is
		/// given out in the error report. The <c>TextSupplier</c> is passed in the constructor of this
		/// <c>TextHistogram</c> object. This <c>update()</c> method complies with the one described in DESMO, see
		/// [Page91].
		/// </summary>
		public void update() {
			if (this._textSuppl == null) {
				sendWarning(
						"Attempt to update a TextHistogram that is not "
								+ "connected to a TextSupplier. No String is provided with which "
								+ "the histogram could be updated. The command will be ignored!",
								"TextHistogram: " + this.getName() + " Method: update()",
								"The given TextSupplier is only a null pointer.",
								"Make sure to update a TextHistogram only when it is connected "
										+ "to a valid TextSupplier. Or use the update(String updateStr) method.");

				return; // that's it
			}
			this._lastString = this._textSuppl.text();
			if (this._lastString == null) {
				sendWarning(
						"Attempt to insert a null pointer to the TextHistogram. "
								+ "The null pointer will not be counted! ",
								"update(): ",
								"Invalid reference. ",
								"Make sure to pass a valid String when calling the "
										+ "update method.");
				return; // that's it
			}
			else {
				incrementObservations();
				if (this._map.ContainsKey(this._lastString)) {
					this._map.Add(this._lastString, this._map[this._lastString] + 1);
				}
				else {
					this._map.Add(this._lastString.ToString(), 1L);
				}
				traceUpdate(); // leave a message in the trace
			}
		}

		/// <summary>
		/// Updates this <c>TextHistogram</c> object with the String value given as parameter. If the parameter is
		/// a <c>null</c> pointer it will not be counted and an error is given out in the error report. In some
		/// cases it might be more convenient to pass the String value this <c>TextHistogram</c> will be updated
		/// with directly within the <c>update(String updateStr)</c> method instead of going via the
		/// <c>TextSupplier</c>.
		/// </summary>
		/// <param name="updateStr">The String with which this <c>TextHistogram</c> will be updated.</param>
		public void update(String updateStr) {
			if (updateStr == null) {
				sendWarning(
						"Attempt to insert a null pointer to the TextHistogram. "
								+ "The null pointer will not be counted! ",
								"update(): ",
								"Invalid reference. ",
								"Make sure to pass a valid String when calling the "
										+ "update method.");
				return; // that's it
			}
			else {
				this._lastString = updateStr;
				base.incrementObservations();
				if (this._map.ContainsKey(updateStr)) {
					this._map.Add(updateStr, this._map[updateStr] + 1);
				}
				else {
					this._map.Add(updateStr, 1L);
				}
				traceUpdate(); // leave a message in the trace
			}
		}

		/// <summary>
		/// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
		/// interface. This method will be called automatically from an <c>Observable</c> object within its
		/// <c>notifyObservers()</c> method. If no Object (a<c>null</c> value) is passed as arg, the actual value
		/// of the TextSupplier will be fetched with the <c>text()</c> method of the TextSupplier. Otherwise it is
		/// expected that the actual text is passed in the Object arg. If the the actual value is a <c>null</c>
		/// pointer it will not be counted and an error is given out in the error report.
		/// </summary>
		/// <param name="o">
		/// The Observable calling this method within its own <c>notifyObservers()</c> method.
		/// </param>
		/// <param name="arg">
		/// The Object with which this <c>Statistic Object</c> is updated. Normally a String which is added to the
		/// statistics or <c>null</c> when there is an actual text to be given with the <c>text()</c> method.
		/// </param>
		public void update(Observable o, object arg) {
			// update was called with no arg Object OR from the SimClock
			if (arg == null || o is SimClock) {
				if (_textSuppl == null) {
					sendWarning(
							"Attempt to update a TextHistogram that is not "
									+ "connected to a TextValueSupplier. No value is provided with which "
									+ "the statistic could be updated. The command will be ignored!",
									"TextHistogram: " + this.getName()
									+ " Method: update "
									+ "(Observable o, Object arg)",
									"The given TextValueSupplier: TextValSuppl is only a null pointer.",
									"Make sure to update a TextHistogram only when it is connected "
											+ "to a valid TextValueSupplier. Or use the update(String) method.");

					return; // that's it
				}

				// get the actual value from the TextSupplier
				this._lastString = this._textSuppl.text();

				if (this._lastString == null) {
					sendWarning(
							"Attempt to insert a null pointer to the TextHistogram. "
									+ "The null pointer will not be counted! ",
									"update(): ",
									"Invalid reference. ",
									"Make sure to pass a valid String when calling the "
											+ "update method.");
					return; // that's it
				}
			}

		else {
				if (arg is string) {
					// get the value out of the Object arg
					this._lastString = arg.ToString();
				}
			else {
					sendWarning(
							"Attempt to update a TextHistogram with an argument "
									+ "arg, that can not be recognized. The attempted action is ignored!",
									"TextHistogram: " + this.getName()
									+ " Method: update (Observable "
									+ "o, Object arg)",
									"The passed Object in the argument arg could not be recognized.",
							"Make sure to pass null or a String object as the arg argument.");
					return; // do nothing, just return
				}
			}
			incrementObservations(); // use the method from the Reportable class
			if (this._map.ContainsKey(this._lastString)) {
				this._map.Add(this._lastString, this._map[this._lastString] + 1);
			}
			else {
				this._map.Add(this._lastString, 1L);
			}
			traceUpdate(); // leave a message in the trace
		}

		/**
		 * Leaves a message in the trace that this StatisticObject has been updated.
		 */
		protected void traceUpdate() {
			if (currentlySendTraceNotes()) {
				sendTraceNote("updates " + this.getQuotedName());
			} // tell in the trace which StatisticObject is updated
		}

        public void update(object value1, object value2)
        {
            throw new NotImplementedException();
        }
    }
	//\ Kein Zeilenumbruch am Dateiende.
}