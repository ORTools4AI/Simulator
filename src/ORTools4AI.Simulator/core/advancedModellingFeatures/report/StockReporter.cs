// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures.report
{

	/**
     * Captures all relevant information about the <code>Stock</code>. That means
     * from the producer and the consumer queue.
     */

	public class StockReporter : Reporter {
		// ****** attributes ******

		/**
		 * The column headings of the wait queues (producer and consumer) part of
		 * this StockReporter. Entries should contain the elements in the same order
		 * as the <code>stockEntries[]</code>.
		 */
		private string[] _stockColumns;

		/**
		 * The data entries of the wait queues (producer and consumer) part of this
		 * StockReporter. The entries should contain the data elements in the same
		 * order as defined in the <code>stockColumns[]</code> array.
		 */
		private string[] _stockEntries;

		/**
		 * The number of columns of the wait queues (producer and consumer) part of
		 * this StockReporter.
		 */
		private int _stockNumColumns;

		// ****** methods ******

		/// <summary>
		/// Constructor for a new StockReporter. Note that although any Reportable is accepted you should make sure
		/// that only subtypes of Stock are passed to this constructor. Otherwise the number of column titles and
		/// their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Stock to report about</param>
		public StockReporter(Reportable informationSource):base(informationSource) {
			//super(informationSource); // make a Reporter

			groupHeading = "Stocks";
			groupID = 911; // see Reporter for more information about groupID

			// the stock part of the Stock
			numColumns = 10;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Prod";
			columns[3] = "Cons";
			columns[4] = "Init";
			columns[5] = "Limit";
			columns[6] = "Max";
			columns[7] = "Min";
			columns[8] = "Now";
			columns[9] = "Average";

			entries = new string[numColumns];

			// *** wait queues for producers and consumers ***

			_stockNumColumns = 9;
			_stockColumns = new string[_stockNumColumns];
			_stockColumns[0] = "Queues";
			_stockColumns[1] = "Order";
			_stockColumns[2] = "pass";
			_stockColumns[3] = "(Re)set";
			_stockColumns[4] = "Users";
			_stockColumns[5] = "avg.Wait";
			_stockColumns[6] = "QLimit";
			_stockColumns[7] = "QMaxL";
			_stockColumns[8] = "refused";

			// entries of producer and consumer queue
			_stockEntries = new string[_stockNumColumns * 2];

		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. This method is fetching the data from the master Stock. Implement this method in a
		/// way, that an array of the same length as the column titles is produced containing the data at the point
		/// of time this method is called by someone else to produce up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting.</returns>
		public override string[] getEntries() {

			if (source is Stock) {
				// the Stock we report about (source = informationsource)
				var st = (Stock)source;

				// *** entries of stock
				// Title
				entries[0] = st.getName();
				// (Re)set
				entries[1] = st.resetAt().toString();
				// Prod
				entries[2] = st.getProducers().ToString();
				// Cons
				entries[3] = st.getConsumers().ToString();
				// Init
				entries[4] = st.getInitial().ToString();
				// Limit
				entries[5] = st.getCapacity().ToString();
				if (st.getCapacity() == int.MaxValue) {
					entries[5] = "unlim.";
				}
				// Max
				entries[6] = st.getMaximum().ToString();
				// Min
				entries[7] = st.getMinimum().ToString();
				// Now
				entries[8] = st.getAvail().ToString();
				// Average
				entries[9] = st.avgAvail().ToString();

			} else {
				for (int i = 0; i < numColumns; i++) {
					entries[i] = "Invalid source!";
				} // end for
			} // end else

			return entries;
		}

		/// <summary>
		/// Returns the titles of the wait queues part of this StockReporter as an array of Strings.
		/// </summary>
		/// <returns>Array containing the titles of the wait queues part of this StockReporter.</returns>
		public string[] getStockColumnTitles() {

			return _stockColumns;
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>stockColumns[]</c>. This method is fetching the data from the master Stock. Implement this method in
		/// a way, that an array of the same length as the stockColumns titles is produced containing the data at
		/// the point of time this method is called by someone else to produce up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting.</returns>
		public String[] getStockEntries() {

			if (source is Stock) {
				// the Stock (consumer queue) we report about (source =
				// informationsource)
				var st = (Stock)source;

				// the producer queue inside the Stock (is a ProcessQueue)
				ProcessQueue <SimProcess> pq = st.getProducerQueue();

				// *** stockEntries of the producers queue
				// Title
				_stockEntries[0] = pq.getName();
				// pOrder
				_stockEntries[1] = st.getProdQueueStrategy();
				// pass
				String passProds = "no";
				if (st.getPassByProducers()) {
					passProds = "yes";
				}
				_stockEntries[2] = passProds;
				// (Re)set
				_stockEntries[3] = pq.resetAt().toString();
				// Users
				_stockEntries[4] =st.getProducers().ToString();
				// avg.Wait
				_stockEntries[5] = pq.averageWaitTime().toString();
				// Qlimit
				_stockEntries[6] = pq.getQueueLimit().ToString();
				if (pq.getQueueLimit() == int.MaxValue) {
					_stockEntries[6] = "unlimit.";
				}
				// QMaxL
				_stockEntries[7] = pq.maxLength().ToString();
				// refused
				_stockEntries[8] = pq.getRefused().ToString();

				// *** stockEntries of the consumer queue
				// Title
				_stockEntries[9] = (st.getName() + "_C");
				// cOrder
				_stockEntries[10] = st.getConsQueueStrategy();
				// pass
				String passCons = "no";
				if (st.getPassByConsumers()) {
					passCons = "yes";
				}
				_stockEntries[11] = passCons;
				// (Re)set
				_stockEntries[12] = st.resetAt().toString();
				// Users
				_stockEntries[13] = st.getConsumers().ToString();
				// avg.Wait
				_stockEntries[14] = st.averageWaitTime().toString();
				// QLimit
				_stockEntries[15] = st.getQueueLimit().ToString();
				if (st.getQueueLimit() == int.MaxValue) {
					_stockEntries[15] = "unlimit.";
				}
				// QMaxL
				_stockEntries[16] = st.maxLength().ToString();
				// refused
				_stockEntries[17] = st.getRefused().ToString();

			} else {
				for (int i = 0; i < _stockNumColumns * 2; i++) {
					_stockEntries[i] = "Invalid source!";
				} // end for
			} // end else

			return _stockEntries;
		}

		/// <summary>Returns the number of columns of the wait queues part of this StockReporter.</summary>
		/// <returns>The number of columns of the wait queues part of this StockReporter</returns>
		public int getStockNumColumns() {

			return _stockNumColumns;
		}

		public override bool isContinuingReporter() {
			return true;
		}
	} // end class StockReporter
}
