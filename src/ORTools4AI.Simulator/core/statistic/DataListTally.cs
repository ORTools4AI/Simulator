// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.statistic
{

	/**
     * The <code>DataListTally</code> class is providing a statistic analysis about
     * one value. The mean value and the standard deviation is calculated on basis
     * of the total number of observations. All observed values are stored in a list
     * to be able to calculate quartiles or histograms.
     * 
     * @see Tally
     */

	public class DataListTally : Tally {

        /**
		 * A list to store values passed to this Tally.
		 */
        private DataList _list;

		/**
		 * Should a histogram-reporter be used for the DataList?
		 */
		private bool _reportDataListHistogram = true;

		/**
		 * Is the DataList already sorted?
		 */
		private bool _sorted = false;



		/// <summary>Constructor for a DataListTally object.</summary>
		/// <param name="ownerModel">The model this Tally is associated to</param>
		/// <param name="name">The name of this Tally object</param>
		/// <param name="showInReport">Flag for showing the report about this Tally.</param>
		/// <param name="showInTrace">Flag for showing the trace output of this Tally.</param>
		public DataListTally(Model ownerModel, string name, bool showInReport,
				bool showInTrace) : base(ownerModel, name, showInReport, showInTrace) {
			//super(ownerModel, name, showInReport, showInTrace);
            _list = new DataList(getModel(), getName()
                + " Data-List", false, false);
        }

		/// <summary>Should a histogram-reporter be used for the DataList?</summary>
		/// <param name="value">
		/// true, if the DataList should be reported using a histogram-reporter, false if not.
		/// </param>
		public void setReportDataListHistogram(bool value) {
			_reportDataListHistogram = value;
		}

		/// <summary>Will a histogram-reporter be used to create a report for the DataList?</summary>
		/// <returns>true if the reporting is enabled, false if not.</returns>
		public bool getReportDataListHistogram() {
			return _reportDataListHistogram;
		}

		/// <summary>Creates a Reporter for this DataListTally.</summary>
		/// <returns>The reporter for this DataListTally.</returns>
		//@Override
		protected override Reporter createDefaultReporter() {
			DataListTallyReporter result = new DataListTallyReporter(this);
			return result;
		}

		/// <summary>
		/// Updates this <c>DataListTally</c> object by fetching the actual value of the <c>ValueSupplier</c> and
		/// processing it. The <c>ValueSupplier</c> is passed in the constructor of this <c>DataListTally</c>
		/// object. This <c>update()</c> method complies with the one described in DESMO, see [Page91].
		/// </summary>
		//@Override
		public void update() {
			base.update();
			internalUpdate(getLastValue());
		}

		/// <summary>
		/// Updates this <c>DataListTally</c> object with the double value given as parameter. In some cases it
		/// might be more convenient to pass the value this <c>DataListTally</c> will be updated with directly
		/// within the <c>update(double val)</c> method instead of going via the <c>ValueSupplier</c>.
		/// </summary>
		/// <param name="val">The value with which this <c>Tally</c> will be updated.</param>
		//@Override
		public  void update(double val) {
			base.update(val);
			internalUpdate(val);
		}

		/// <summary>
		/// Implementation of the virtual <c>update(Observable, Object)</c> method of the <c>Observer</c>
		/// interface. This method will be called automatically from an <c>Observable</c> object within its
		/// <c>notifyObservers()</c> method. If no Object (a<c>null</c> value) is passed as arg, the actual value
		/// of the ValueSupplier will be fetched with the <c>value()</c> method of the ValueSupplier. Otherwise it
		/// is expected that the actual value is passed in the Object arg.
		/// </summary>
		/// <param name="o">
		/// The Observable calling this method within its own <c>notifyObservers()</c> method.
		/// </param>
		/// <param name="arg">
		/// The Object with which this <c>Tally</c> is updated. Normally a double number which is added to the
		/// statistics or <c>null</c>.
		/// </param>
		//@Override
		public void update(Observable o, object arg) {
			base.update(o, arg);
			internalUpdate(getLastValue());
		}

		/**
		 * Resets the DataListTally and its DataList.
		 */
		//	@Override
		public void reset() {
			base.reset();

			if (_list != null)
				_list.clear();

			_sorted = false;
		}

		/// <summary>Returns the sorted DataList.</summary>
		/// <returns>the DataList.</returns>
		public DataList getDataListSorted() {
			if (!_sorted) {
				_list = _list.sort(_list);
				_sorted = true;
			}
			return _list;
		}

		/// <summary>Internal method to update the DataList with a new sample.</summary>
		/// <param name="value">The new sample.</param>
		private void internalUpdate(double value) {
			if (_sorted)
				_sorted = false;
			_list.add(value);
		}
	}


    /**
		 * A simple List for storing values passed to a Tally. This list is created
		 * to produce low overhead while simulating.
		 */
    public class DataList : Reportable
    {

        /**
         * A List's Element.
         */
        public class Element
        {
            /**
             * The value of this Element.
             */
            private double _value;

            /**
             * The next Element in the List.
             */
            private Element _next;

            /// <summary>Constructs a new Element with the given value.</summary>
            /// <param name="value">The value of this Element.</param>
            public Element(double value)
            {
                _value = value;
            }

            /// <summary>Returns the next Element in the List.</summary>
            /// <returns>the next Element in the list of <c>null</c> if this is the last Element.</returns>
            public Element getNext()
            {
                return _next;
            }

            /// <summary>Sets the next Element in the list.</summary>
            /// <param name="next">The Element to be the successor of this Element.</param>
            public void setNext(Element next)
            {
                _next = next;
            }

            /**
             * Returns the Element's value as String.
             */
            //@Override
            public string toString()
            {
                return "" + _value;
            }

            /// <summary>Returns the Element's value.</summary>
            /// <returns>the value of this Element.</returns>
            public double getValue()
            {
                return _value;
            }
        }

        /**
         * 
         */
        protected DataList nextInStack = null;

        /**
         * The first Element in the list.
         */
        private Element _first = null;

        /**
         * The last Element in the list.
         */
        private Element _last = null;

        /**
         * The list's length.
         */
        private int _length = 0;

        /// <summary>Constructor for a new DataList</summary>
        /// <param name="owner">The model this DataList is associated to</param>
        /// <param name="name">The name of this DataList object</param>
        /// <param name="showInReport">Flag for showing the report about this DataList.</param>
        /// <param name="showInTrace">Flag for showing the trace output of this DataList.</param>
        public DataList(Model owner, string name, bool showInReport,
                bool showInTrace) : base(owner, name, showInReport, showInTrace)
        {
            //super(owner, name, showInReport, showInTrace);
        }

        /// <summary>Returns a Reporter to produce a report about this DataList.</summary>
        /// <returns>The Reporter for this DataList.</returns>
        //@Override
        protected override Reporter createDefaultReporter()
        {
            //return new DataListHistogramReporter(getDataListSorted(),
            //        getMinimum(), getMaximum());

            throw new NotImplementedException();
        }

        /// <summary>Returns the list's length.</summary>
        /// <returns>the length of this DataList.</returns>
        public int getLength()
        {
            return _length;
        }

        /// <summary>Adds an Element to the end of the DataList.</summary>
        /// <param name="value">The value of the new Element in the list.</param>
        public void add(double value)
        {
            if (_first == null)
                _first = _last = new Element(value);
            else
            {
                _last.setNext(new Element(value));
                _last = _last.getNext();
            }

            _length++;
        }

        /**
         * Removes all Elements from this List.
         */
        public void clear()
        {
            _first = _last = null;
            _length = 0;
        }

        /// <summary>
        /// This Method is part of the sorting-algorithm. Cuts the first two Elements from the list and sort
        /// them.
        /// </summary>
        /// <returns>A DataList containing the first two elements in a sorted order.</returns>
        private DataList popTwoSorted()
        {
            DataList result = new DataList(getModel(), null, false, false);

            if (_first.getNext() == null)
            {
                result._first = result._last = _first;
                result._length = 1;
                _first = _last = null;
                _length = 0;

                return result;
            }

            if (_first.getValue() > _first.getNext().getValue())
            {
                result._first = _first.getNext();
                result._last = _first;
            }
            else
            {
                result._first = _first;
                result._last = _first.getNext();
            }

            _first = _first.getNext().getNext();
            result._first.setNext(result._last);
            result._last.setNext(null);

            result._length = 2;
            _length = _length - 2;

            return result;
        }

        /// <summary>
        /// This Method is part of the sorting-algorithm. Merges two list on the list-stack to produce one
        /// sorted List for them.
        /// </summary>
        /// <param name="stack">A list to be merged with its predecessor on its list-stack.</param>
        /// <returns>
        /// A merged DataList created from given DataList and its predecessor on the list-stack.
        /// </returns>
        private DataList merge(DataList stack)
        {
            DataList l1 = stack;
            DataList l2 = stack.nextInStack;

            DataList result = new DataList(getModel(), null, false, false);
            int l = l1.getLength() + l2.getLength();

            while (result.getLength() < l)
            {
                if (l1._first != null
                        && (l2._first == null || l1._first.getValue() <= l2._first
                                .getValue()))
                {
                    result.add(l1._first.getValue());
                    l1._first = l1._first.getNext();
                    l1._length--;
                }
                else
                {
                    result.add(l2._first.getValue());
                    l2._first = l2._first.getNext();
                    l2._length--;
                }
            }

            stack = result;
            stack.nextInStack = l2.nextInStack;

            return stack;
        }

        private bool _reportDataListHistogram = true;

        /// <summary>Sorts the given list using a merge-sort algorithm</summary>
        /// <param name="list">the list to be sorted.</param>
        /// <returns>The sorted list.</returns>
        public DataList sort(DataList list)
        {

            String name = this.getName() + " (sorted)";

            if (list._length <= 1)
            {
                list.rename(name);

                if (_reportDataListHistogram)
                {
                    list.reportOn();
                    list.traceOn();
                }

                return list;
            }

            DataList stack = null;

            int pass = 0;
            int l = 0;

            while (getLength() > 0)
            {
                DataList tmp = stack;
                stack = popTwoSorted();
                stack.nextInStack = tmp;
                l++;

                int p = ++pass;

                while ((p & 1) == 0)
                {
                    stack = merge(stack);
                    l--;
                    p >>= 1;
                }
            }

            while (l > 1)
            {
                stack = merge(stack);
                l--;
            }

            stack.rename(name);

            if (_reportDataListHistogram)
            {
                stack.reportOn();
                stack.traceOn();
            }

            return stack;
        }

        /// <summary>Creates a String containing all Elements of the list and its length.</summary>
        /// <returns>the list as String.</returns>
        //@Override
        public string toString()
        {
            String result = "[";
            Element e = _first;

            while (e != null)
            {
                result += e;

                if (e.getNext() != null)
                    result += ", ";

                e = e.getNext();
            }

            return result + "] (" + _length + ")";
        }

        /// <summary>Returns the list's first Element.</summary>
        /// <returns>the first Element or <c>null</c> if the list is empty.</returns>
        public Element getFirst()
        {
            return _first;
        }

        /// <summary>Returns the list's last Element.</summary>
        /// <returns>the last Element of <c>null</c> if the list is empty.</returns>
        public Element getLast()
        {
            return _last;
        }

        /// <summary>Returns the name of the Tally, this list belongs to.</summary>
        /// <returns>the Tally's name.</returns>
        public string getTallyName()
        {
            return  typeof(DataListTally).Name;
        }
    }


}