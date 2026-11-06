// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler (HTMLFileOutput), Nicolas Knaak
// Modified for ORTools4AI.Simulator.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ORTools4AI.Simulator.core.report
{
    /**
  * An interface representing basic facilites for writing data into tables. The
  * specified operations are adapted from the deprecated class
  * demoj.report.HTMLFileOutput.
  */
    public interface TableFormatter
    {

        /// <summary>Should open a new document containing multiple tables with the given name</summary>
        /// <param name="name">table name</param>
        public void open(string name);

        /** Should close the document */
        public void close();

        /** Should close a table row */
        public void closeRow();

        /** Should close a table */
        public void closeTable();

        /** Should close a table without writing a top tag (HTML specific) */
        public void closeTableNoTopTag();

        /** Should open a row */
        public void openRow();

        /// <summary>Should open a table with the given heading</summary>
        /// <param name="heading">table heading</param>
        public void openTable(string heading);

        /// <returns><c>true</c> if a row is currently open, <c>false</c> otherwise</returns>
        public bool rowIsOpen();

        /// <returns><c>true</c> if a table is currently open, <c>false</c> otherwise</returns>
        public bool tableIsOpen();

        /// <summary>Should write the given string into a new table cell</summary>
        /// <param name="s">string to write</param>
        /// <param name="spanning">number of cells to span</param>
        public void writeCell(string s, int spanning);

        /// <summary>Should write the given heading of size i into a new table cell</summary>
        /// <param name="i">size (must be interpreted in a sensible way).</param>
        /// <param name="s">string to write</param>
        public void writeHeading(int i, string s);

        /// <summary>Should write the given heading of default size into a new table cell</summary>
        /// <param name="s">string to write</param>
        public void writeHeadingCell(string s);

        /** Writes a horizontal ruler */
        public void writeHorizontalRuler();

        /// <summary>Should format the given time String and write it into a cell</summary>
        /// <param name="s">a string containing simulation time in float format.</param>
        public string writeTime(string s);

        /// <summary>Should return the precision used for time values.</summary>
        /// <returns>precision</returns>
        public int timePrecision();

        /// <summary>Should set an output file to write the table to</summary>
        /// <param name="outFile">a FileOutput to write the table to</param>
        public void setOutput(FileOutput outFile);

        /// <summary>Should set the required precision of time values.</summary>
        /// <param name="tp">precision</param>
        public void setTimePrecision(int tp);

        /// <returns>appendix of the file format the table is stored in (e.g. "html" or "txt")</returns>
        public string getFileFormat();
    }
}
