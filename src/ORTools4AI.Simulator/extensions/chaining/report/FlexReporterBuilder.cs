// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.extensions.chaining.report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ORTools4AI.Simulator.extensions.chaining.report.FlexReporterBuilder;

namespace ORTools4AI.Simulator.extensions.chaining.report
{
    /**
 * The FlexReporterBuilder is a helper class for constructing
 * {@link FlexReporter}s which are special Reporters whose content 
 * (i.e. the cells that may displayed in multiple rows) is defined
 * dynamically.  
 */
    public class FlexReporterBuilder : FlexReporterBuilder.RowCloseCallback
    {

        /**
         * Represents a cell that will be drawn in a report
         * 
         */
        public class Cell
        {

            /**
             * The value of the cell
             */
            private readonly String value;

            /// <summary>Constructs a cell with the given value</summary>
            /// <param name="cellValue">The value of the cell</param>
            public Cell(object cellValue)
            {
                value = cellValue.ToString();
            }

            /// <returns>the value of the cell</returns>
            public String getValue()
            {
                return value;
            }

        }

        /**
         * A flexible reporter that allows multiple rows with individual formatted
         * cells.
         * 
         * @author Malte Unkrig
         * 
         */
        public class FlexReporter : Reporter
        {

            /**
             * The rows that will be drawn for this reporter
             */
            private readonly LinkedList<Row> rows;

          
            /// <summary>
            /// Constructs a FlexReporter with the given group heaing, group id and the given list of rows.
            /// </summary>
            /// <param name="groupHeading">The heading for the group this reporter contributes to</param>
            /// <param name="groupId">The id of the group this reporter contributes to</param>
            /// <param name="rows">
            /// The list of rows which will be drawn for this reporter in the report
            /// </param>
            public FlexReporter(string groupHeading, int groupId,
                    LinkedList<Row> rows) : base(null)
            {

                this.groupHeading = groupHeading;
                this.rows = rows;
            }

            public string[] getColumnTitles()
            {
                return new string[0];
            }

            /// <inheritdoc/>

            public override string[] getEntries()
            {
                return new string[0];
            }


            public int getGroupID()
            {
                return this.groupID;
            }

            /// <returns>the rows that will be drawn for this reporter in the report</returns>
            public LinkedList<Row> getRows()
            {
                return rows;
            }

        }

        /**
         * A cell that should be visually displayed as a heading cell in the report.
         * For example by drawing it bold.
         * 
         * 
         */
        public class HeadingCell : Cell
        {

            /// <summary>Constructs a heading cell with the given value</summary>
            public HeadingCell(object cellValue) : base(cellValue)
            {

            }

        }

        /**
         * Represents a row that will be drawn in the report. A row is made up of
         * individual cells
         * 
         */
        public class Row
        {

            /**
             * Callback for when a row is closed
             */
            private readonly RowCloseCallback closeCallback;

            /**
             * The cells belonging to this row
             */
            private readonly LinkedList<Cell> cells;

            /// <summary>Constructs the Row with the given close callback</summary>
            /// <param name="closeCallback">A callback object that is notified when a row is closed</param>
            public Row(RowCloseCallback closeCallback)
            {
                this.closeCallback = closeCallback;
                cells = new LinkedList<Cell>();
            }

            /// <summary>Adds a cell with the given value to this row</summary>
            /// <param name="cellValue">The value of the new cell</param>
            /// <returns>the row for cahining</returns>
            public Row addCell(object cellValue)
            {
                cells.AddLast(new Cell(cellValue));
                return this;
            }

            /// <summary>Adds a heading cell with the given value to this row</summary>
            /// <param name="cellValue">Row</param>
            /// <returns>the row for cahining</returns>
            public Row addHeadingCell(object cellValue)
            {
                cells.AddLast(new HeadingCell(cellValue));
                return this;
            }

            /// <summary>Closes the row so a new one can be opened</summary>
            /// <returns>The FlexReporterBuilder for chaining</returns>
            public FlexReporterBuilder closeRow()
            {
                return closeCallback.onRowClosed();
            }

            /// <returns>the list of cells belonging to this row</returns>
            public LinkedList<Cell> getCells()
            {
                return cells;
            }
        }

        /**
         * A callback class that should be notified when a row is closed
         * 
         * 
         */
        public interface RowCloseCallback
        {
            abstract FlexReporterBuilder onRowClosed();
        }

        /**
         * The row that is currently under construction in the FlexReporterBuilder
         */
        private Row currentRow;

        /**
         * The rows that are build in the FlexReporterBuilder
         */
        private  LinkedList<Row> rows;

        /**
         * The group heading for the FlexReporter
         */
        private readonly string groupHeading;

        /**
         * The group id for the FlexReporter
         */
        private readonly int groupId;

        /// <summary>Constructs a new FlexReporterBuilder with the given group heading and group id.</summary>
        /// <param name="groupHeading">
        /// The group heading that will be used when the FlexReporter is built with this builder
        /// </param>
        /// <param name="groupId">
        /// The group id that will be used when the FlexReporter is built with this builder
        /// </param>
        public FlexReporterBuilder(string groupHeading, int groupId)
        {
            this.groupHeading = groupHeading;
            this.groupId = groupId;
            rows = new LinkedList<Row>();
        }

        /// <summary>
        /// Builds a FlexReporter from all the rows and cells that have been added to this builder
        /// </summary>
        /// <returns>The newly build FlexReporter</returns>
        public FlexReporter build()
        {
            if (currentRow != null)
            {
                throw new Exception(
                        "Please close the currently open row before building the report.");
            }

            return new FlexReporter(groupHeading, groupId, rows);
        }

        /// <summary>Closes the row currently under construction</summary>
        /// <returns>The FlexReporterBuilder for chaining</returns>
        public FlexReporterBuilder closeRow()
        {
            rows.AddLast(currentRow);
            currentRow = null;
            return this;
        }

        /// <summary>Opens a new row for construction</summary>
        /// <returns>the new row for chaining</returns>
        public Row openRow()
        {
            if (currentRow != null)
            {
                throw new Exception(
                    "There is already an open row. Please close it before opening a new one.");
            }
            currentRow = new Row(this);
            return currentRow;
        }

        /**
         * RowCloseCallback implementation: commits the current row and clears it.
         */
        public FlexReporterBuilder onRowClosed()
        {
            if (currentRow == null)
            {
                throw new Exception("There is no open row that can be closed.");
            }
            rows.AddLast(currentRow);
            currentRow = null;
            return this;
        }
    }
}


