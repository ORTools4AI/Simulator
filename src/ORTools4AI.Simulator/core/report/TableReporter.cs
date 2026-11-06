// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * A reporter with an arbitrary number
     * of rows and columns. 
     */

	public abstract class TableReporter : Reporter
	{

		/** The reporter's number of rows */
		public int numRows { get; set; }

		/** 2-dimensional table of entries */
		protected string[][] entryTable;

		/** The reporter's title */
		private string _title;

		/**
		 * Header table printed before the data table stored in <tt>entryTable</tt>.
		 * This should contain additional information like e.g. the number of
		 * observations.
		 */
		protected string[][] header = null;

		/// <summary>Creates a new TableReporter for the given Reportable with the given title</summary>
		/// <param name="isReportable">the source of information to be displayed in the table</param>
		/// <param name="title">title of the table reporter</param>
		public TableReporter(Reportable isReportable, string title) : base(isReportable)
		{

			this._title = title;
			this.groupHeading = "Data Tables";
		}

		/// <summary>Returns the header table (or null if no header should be printed in report).</summary>
		/// <returns>header field of the table</returns>

		public string[][] getHeader()
		{
			if (header == null)
				return null;
			else
				return header.Clone() as string[][];
		}

		/// <summary>
		/// Returns a one-dimensional array of entries (Computed from getEntryTable() by entries[numColumns * i +
		/// j] = entryTable[i][j]).
		/// </summary>
		/// <returns>field of table entries</returns>
		public override string[] getEntries()
		{
			getEntryTable();
			entries = new string[numRows * numColumns];
			for (int i = 0; i < numRows; i++)
			{
				for (int j = 0; j < numColumns; j++)
				{
					entries[numColumns * i + j] = entryTable[i][j];
				}
			}
			return entries;
		}

		/**
		 * Returns the number of rows of this table reporter.
		 * 
		 * @return integer number of rows
		 */
		//public int numRows() {
		//	return numRows;
		//}

		/**
		 * Returns the table reporter's title.
		 * 
		 * @return the titel as a string
		 */
		public string getTitle()
		{
			return _title;
		}

		/// <summary>Should initialize and return the 2-dimensional entry table.</summary>
		/// <returns>field of entries</returns>
		public abstract string[][] getEntryTable();

	}
}