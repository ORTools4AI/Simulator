// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author of ReportFileOut class : Tim Lechler, modified by Soenke Claassen and
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{
   

    /**
     * ReportMultRowsFileOut is used to create a file to let the reporters write
     * their reports to. It receives a reporter and divides its individual text
     * information up to be displayed in HTML format on disc in the user's local
     * directory. The messages are displayed in a tabular design with columns for
     * each item displayed by the reporter. For each Reporter giving a new group-ID,
     * a new table is opened. This leads to a number of distinct tables displayed in
     * one HTML-page with all reporters grouped by their group-id in one table. This
     * special ReportFileOut is designed to write reports with more than one row for
     * one reporter. This is needed for the <code>WaitQueueReporter</code> where
     * there will be one row for the data of the waiting slaves and one row for the
     * data of the waiting masters and for the <code>HistogramReporter</code>
     * where there will be multiple rows, one for each segment (cell) of ther
     * interval. Note that if the framework is used as an Applet in webpages there
     * is no access to the disc and an alternative output on screen or into a
     * graphics window must be registered at the Experiment's MessageManager for
     * this type of messages. Errors affecting the java runtime are always displayed
     * on the system's standard output PrintStream.
     * 
     *         Nicolas Knaak, modified by Chr. M&uuml;ller (TH Wildau) 28.11.2012
     */
    public class ReportMultRowsFileOut : ReportFileOut
    {

    /**
     * Flag to shift nested reporter descriptions one column if set to 
     * <code>true</code>.
     */
    private static bool offsetDescriptionsOneColumn = false;

    /// <summary>
    /// Creates a file to print reports into a HTML page. By opening the file, the necessary HTML tags to
    /// define a webpage are already inserted into the file. The parameter given should reflect the
    /// experiment that produces this file.
    /// </summary>
    /// <param name="simTimeFloatingDigits">
    /// The number of floating point digits of the simulation time values to be displayed
    /// </param>
    public ReportMultRowsFileOut(int simTimeFloatingDigits, string formatter):base(simTimeFloatingDigits, formatter)
    {
       // super(simTimeFloatingDigits, formatter); // make a ReportFileOut
    }

    /// <summary>
    /// Receives a reporter and writes its contents formatted to a HTML table into a file in the user's
    /// default directory. In case of a ModelReporter it writes the model's information into the file. After
    /// writing the model data, all reportables registered with the ModelReporter are asked for a reporter
    /// and those are also written to the report file. After processing the Reportables, the registered
    /// subModels are asked to produce their reporters which in turn are recursively sent to this method
    /// until no other submodels are available.
    /// </summary>
    /// <param name="r">The Reporter to be processed</param>
    public void receive(Reporter r)
    {
        // check parameters
        if (r == null)
            return; // invalid parameter

        // copy values in buffer variables for faster access
        string[] titleBuf = r.getColumnTitles();
        string[] entryBuf = r.getEntries();

        // first Reporter to be printed here always writes heading and starts
        // table
        // (most likely a ModelReporter)
        if (lastReporter == null)
        {
            // open a table with the heading needed for this type of reporter
            formatter.openTable(r.getHeading());
            formatter.openRow();

            for (int i = 0; i < r.numColumns; i++)
            {
                formatter.writeHeadingCell(titleBuf[i]);
            }

            formatter.closeRow();
            lastReporter = r;
            // trick the next if-clause to be avoided first time
        }
        else
        {
            // if two HistogramReporter or two StockReporter or two
            // WorkStationReporter
            // or two TransportReporter will be written one after the other
            // @modified by Nick Knaak 6.2.06: Condition 1: isContinuingReporter
            /*	
			if (r.isContinuingReporter() && lastReporter.isContinuingReporter()
					&& (r.getClass().isAssignableFrom(lastReporter.getClass())
							|| lastReporter.getClass().isAssignableFrom(r.getClass()))) { 
			*/
            // modified if condition by Chr. M&uuml;ller (TH Wildau) 28.11.2012
            if (r.isContinuingReporter() && lastReporter.isContinuingReporter()
                    && Reporter.isSameGroup(r, lastReporter))
            {
                formatter.closeTable();
                // open new table for new XYReporter with no special header
                formatter.openTable(" ");
                formatter.openRow();

                for (int i = 0; i < r.numColumns; i++)
                {
                    formatter.writeHeadingCell(titleBuf[i]);
                }

                formatter.closeRow();
            } // end if
        } // end else

        // if new Reporter group type is received, close last table and open new
        // one
        // --- Modification by N. Knaak (27.11.01): Every table reporter has
        // it's own
        // headings.
        if (Reporter.isOtherGroup(r, lastReporter)
                && !(r is TableReporter)) {

            formatter.closeTable();
            formatter.writeHorizontalRuler();
            formatter.openTable(r.getHeading());
            formatter.openRow();

            for (int i = 0; i < r.numColumns; i++)
            {
                formatter.writeHeadingCell(titleBuf[i]);
            }

            formatter.closeRow();
        }

        // the WaitQueueReporter, the TransportReporter and the
        // WorkStationReporter
        // produce two rows
        // @modified: Condition 2 - isTwoRowReporter
        // by Nick Knaak 6.2.06
        if (r.isTwoRowReporter())
        {
            // open the row for the master queue data
            if (formatter is AbstractTableFormatter) {
                ((AbstractTableFormatter)formatter).openRow(r);
            } else
            {
                formatter.openRow();
            }

            // write all data of the master queue in this row
            for (int i = 0; i < r.numColumns; i++)
            {
                formatter.writeCell(entryBuf[i], 1);
            }

            formatter.closeRow(); // close the row for the master queue data

            // open the row for the slave queue data
            if (formatter is AbstractTableFormatter) {
                ((AbstractTableFormatter)formatter).openRow(r);
            } else
            {
                formatter.openRow();
            }

            // write all data of the slave queue in this row
            for (int i = r.numColumns; i < (r.numColumns * 2); i++)
            {
                formatter.writeCell(entryBuf[i], 1);
            }

            formatter.closeRow(); // close the row for the slave queue data

            // if a WorkStation has more than one slave queue
            // @modified by Nick Knaak, 6.2.06
            if (r.getNumOfSlaveQueues() > 0)
            {
                int numberSlaveQueues = r.getNumOfSlaveQueues();

                // write all the slave queue rows
                for (int j = 1; j < numberSlaveQueues; j++)
                {
                    if (formatter is AbstractTableFormatter) {
                    ((AbstractTableFormatter)formatter).openRow(r);
                } else
                {
                    formatter.openRow();
                }
                // open the row for the slave queue data

                // write all data of the slave queue in this row
                for (int i = (r.numColumns * (j + 1)); i < (r.numColumns * (j + 2)); i++)
                {
                    formatter.writeCell(entryBuf[i], 1);
                }

                formatter.closeRow();
                // close the row for the slave queue data
            }

        }
        else
        {

            //other overlong Reporters containing more than two rows
            int rows = entryBuf.Length / r.numColumns;
            for (int row = 2; row < rows; row++)
            {

                // skip if all cells are empty
                bool empty = true;
                for (int i = row * r.numColumns; i < r.numColumns * (row + 1); i++)
                {
                    if (entryBuf[i] != null && entryBuf[i].Length > 0)
                    {
                        empty = false;
                        break;
                    }
                }
                if (empty) continue;  // row empty 

                // at least one cell non-empty --> continue with output: open new row
                if (formatter is AbstractTableFormatter) {
                ((AbstractTableFormatter)formatter).openRow(r);
                } else
                {
                    formatter.openRow();
                }

            // write row data 
            for (int i = row * r.numColumns; i < r.numColumns * (row + 1); i++)
            {
                formatter.writeCell(entryBuf[i], 1);
            }

            formatter.closeRow(); // close the row 
        }

    }
} // end of two row reporter
        // --- Modification by N. Knaak (27.11.01): TableReporters are
        // completely handled below.
        else if (!(r is TableReporter))
        // normal handling of all other Reporters
        {
    // always write the reporter's content into a table row
    if (formatter is AbstractTableFormatter) {
        ((AbstractTableFormatter)formatter).openRow(r);
    } else
    {
        formatter.openRow();
    }

    for (int i = 0; i < r.numColumns; i++)
    {
        formatter.writeCell(entryBuf[i], 1);
    }

    // the row's finished now
    formatter.closeRow();
} // end of single row and no table reporter


// ---------- description row, if set -------------------------------

if (r.getDescription() != null && r.getDescription().Length > 0)
{

    if (formatter is AbstractTableFormatter) {
        ((AbstractTableFormatter)formatter).openRow(r);
    } else
    {
        formatter.openRow();
    }
    if (offsetDescriptionsOneColumn)
    {
        formatter.writeCell("", 1);
        formatter.writeCell(r.getDescription(), r.numColumns - 1);
    }
    else
    {
        formatter.writeCell(r.getDescription(), r.numColumns);
    }
    formatter.closeRow(); // close the row
}

// ---------- inner reporter ----------------------------------------

// the HistogramReporter produces a special histogram table
if (r is HistogramReporter) {
    // "Type-Cast"
    var hr = (HistogramReporter)r;
    this.innerReport(hr);
}

// extended by Chr. M&uuml;ller (TH Wildau) 28.11.12
// the HistogramAccumulateReporter produces a special histogram table
if (r  is HistogramAccumulateReporter) {
    // "Type-Cast"
    var har = (HistogramAccumulateReporter)r;
    this.innerReport(har);
}

// the TextHistogramReporter produces a special histogram table
if (r is TextHistogramReporter) {
    // "Type-Cast"
    var thr = (TextHistogramReporter)r;
    this.innerReport(thr);
}

// the StockReporter produces a special stock report about the two
// queues for the producers and consumers
// @TODO: Cond 3: isStockReporter
//if (r  is StockReporter) {
//    // "Type-Cast"
//    desmoj.core.advancedModellingFeatures.report.StockReporter sr = (desmoj.core.advancedModellingFeatures.report.StockReporter)r;
//    this.innerReport(sr);
//}

// --- The table reporter produces a table of arbitrary length.
// (Modification: Nicolas Knaak 27.11.2001)
if (r is TableReporter)
            writeTableReporter((TableReporter)r, titleBuf);

// remember the last reporter
        lastReporter = r;
	}
	
	private void innerReport(HistogramReporter hr)
{
    // copy values in buffer variables for faster access
    String[] histTitleBuf = hr.getHistColumnTitles();
    String[][] histEntryBuf = hr.getHistEntries();

    formatter.closeTableNoTopTag();
    // close the normal (Tally-like) Histogram table
    formatter.openTable(" ");
    // open table for histogram part with no special header
    formatter.openRow(); // open header row

    // in case there is not enough data collected
    if (hr.getObservations() < 3)
    {
        formatter.writeCell("Insufficient data for displaying histogram statistics", 1);
        formatter.closeRow();
    }
    else // enough data collected to display histogram part
    {
        for (int i = 0; i < hr.getHistNumColumns(); i++)
        {
            formatter.writeHeadingCell(histTitleBuf[i]);
        }

        formatter.closeRow();

        // write the HistrogramReporter's content into a table

        // loop through all cells
        for (int j = 0; j < hr.getNoOfCells() + 2; j++)
        {
            if (formatter is AbstractTableFormatter) {
            ((AbstractTableFormatter)formatter).openRow(hr);
        } else
        {
            formatter.openRow();
        }

        for (int i = 0; i < hr.getHistNumColumns(); i++)
        {
            formatter.writeCell(histEntryBuf[j][i], 1);
        }

        // the row is finished now
        formatter.closeRow();
    }
}

	}

	private void innerReport(HistogramAccumulateReporter hr)
{
    // copy values in buffer variables for faster access
    String[] histTitleBuf = hr.getHistColumnTitles();
    String[][] histEntryBuf = hr.getHistEntries();

    formatter.closeTableNoTopTag();
    // close the normal (Tally-like) Histogram table
    formatter.openTable(" ");
    // open table for histogram part with no special header
    formatter.openRow(); // open header row

    // in case there is not enough data collected
    if (hr.getObservations() < 3)
    {
        formatter.writeCell("Insufficient data for displaying histogram statistics", 1);
        formatter.closeRow();
    }
    else // enough data collected to display histogram part
    {
        for (int i = 0; i < hr.getHistNumColumns(); i++)
        {
            formatter.writeHeadingCell(histTitleBuf[i]);
        }

        formatter.closeRow();

        // write the HistrogramReporter's content into a table

        // loop through all cells
        for (int j = 0; j < hr.getNoOfCells() + 2; j++)
        {
            if (formatter is AbstractTableFormatter) {
            ((AbstractTableFormatter)formatter).openRow(hr);
        } else
        {
            formatter.openRow();
        }

        for (int i = 0; i < hr.getHistNumColumns(); i++)
        {
            formatter.writeCell(histEntryBuf[j][i], 1);
        }

        // the row is finished now
        formatter.closeRow();
    }
}
	}

	private void innerReport(TextHistogramReporter thr)
{
    // copy values in buffer variables for faster access
    String[] textHistTitleBuf = thr.getTextHistColumnTitles();
    String[][] histEntryBuf = thr.getTextHistEntries();

    formatter.closeTableNoTopTag();
    // close the normal TextHistogram table
    formatter.openTable(" ");
    // open table for histogram part with no special header
    formatter.openRow(); // open header row

    // in case there is not enough data collected
    if (thr.getObservations() < 3)
    {
        formatter
                .writeCell("Insufficient data for displaying histogram statistics", 1);

        formatter.closeRow();
    }
    else // enough data collected to display histogram part
    {
        for (int i = 0; i < thr.getTextHistNumColumns(); i++)
        {
            formatter.writeHeadingCell(textHistTitleBuf[i]);
        }

        formatter.closeRow();

        // write the TextHistrogramReporter's content into a table

        // loop through all observed Strings
        for (int j = 0; j < thr.getNoOfStrings(); j++)
        {
            if (formatter is AbstractTableFormatter) {
            ((AbstractTableFormatter)formatter).openRow(thr);
        } else
        {
            formatter.openRow();
        }

        for (int i = 0; i < thr.getTextHistNumColumns(); i++)
        {
            formatter.writeCell(histEntryBuf[j][i], 1);
        }

        // the row is finished now
        formatter.closeRow();
    }
}

	}
	
//	private void innerReport(StockReporter sr)
//{
//    // copy values in buffer variables for faster access
//    String[] stockTitleBuf = sr.getStockColumnTitles();
//    String[] stockEntryBuf = sr.getStockEntries();

//    formatter.closeTableNoTopTag();
//    // close the normal Stock table (stock part)
//    formatter.openTable(" ");
//    // open table for queues part with no special header
//    formatter.openRow(); // write header row

//    // write queues heading
//    for (int i = 0; i < sr.getStockNumColumns(); i++)
//    {
//        formatter.writeHeadingCell(stockTitleBuf[i]);
//    }

//    formatter.closeRow();

//    // write the producer queue's content into a table
//    formatter.openRow(); // open the row for the producer queue data

//    // write all data of the producer queue in this row
//    for (int i = 0; i < sr.getStockNumColumns(); i++)
//    {
//        formatter.writeCell(stockEntryBuf[i], 1);
//    }

//    formatter.closeRow(); // close the row for the producer queue data

//    formatter.openRow(); // open the row for the consumer queue data

//    // write all data of the consumer queue in this row
//    for (int i = sr.getStockNumColumns(); i < (sr.getStockNumColumns() * 2); i++)
//    {
//        formatter.writeCell(stockEntryBuf[i], 1);
//    }

//    formatter.closeRow(); // close the row for the consumer queue data

//}

/** Writes a table reporter */
private void writeTableReporter(TableReporter tr, String[] titleBuf)
{
    int cols = tr.numColumns;
    int rows = tr.numRows;
    String[][] entryTable = tr.getEntryTable();
    if (formatter.tableIsOpen())
        formatter.closeTable();

    formatter.writeHorizontalRuler();
    formatter.openTable(tr.getTitle());

    // Write additional info in table header

    string[][] header = tr.getHeader();
    if (header != null)
    {
        for (int i = 0; i < header.Length; i++)
        {
            formatter.openRow();
            for (int j = 0; j < header[i].Length; j++)
            {
                formatter.writeCell(header[i][j], 1);
            }
            formatter.closeRow();
        }
        formatter.openRow();
        formatter.writeCell("___", 1);
        formatter.closeRow();
    }

    // Write Headings
    formatter.openRow();
    for (int i = 0; i < cols; i++)
    {
        formatter.writeHeadingCell(titleBuf[i]);
    }
    formatter.closeRow();

    formatter.openRow();
    formatter.closeRow();

    // Write Data Entries
    for (int i = 0; i < rows; i++)
    {
        formatter.openRow();
        for (int j = 0; j < cols; j++)
        {
            formatter.writeCell(entryTable[i][j], 1);
        }
        formatter.closeRow();
    }
    formatter.closeTable();
}


/**
 * Set report output such that nested Reporter descriptions 
 * are offset by one column, thus improving readability at the expense
 * of space.
 */
//public static void offsetDescriptionsOneColumn()
//{
//    offsetDescriptionsOneColumn = true;
//}
} // end class ReportMultRowsFileOut

}
