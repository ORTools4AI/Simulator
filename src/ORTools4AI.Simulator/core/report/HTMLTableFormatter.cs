// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Nicolas Knaak
// @author based on HTMLFileOutput by Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using System.Text;

namespace ORTools4AI.Simulator.core.report
{
    

    /**
     * A table formatter class for writing simulation output to HTML tables. This
     * class implements the HTML formatting functionality of the deprecated class
     * desmoj.report.HTMLFileOuptut
     */
    public class HTMLTableFormatter:AbstractTableFormatter
    {

    /**
	 * Close the HTML file by writing the footing tags, flushing the buffer and
	 * closing the file.
	 */
    public override void close()
    {

        if (rowOpen)
            closeRow();
        if (tableOpen)
            closeTable();
		output.writeln("<FONT SIZE=-2>created using <A HREF=http://www.desmoj.de>"
                + "DESMO-J</A> Version " + Experiment.getDesmoJVersion() + " at "
                + DateTime.Now  + " - DESMO-J is licensed under "
                + Experiment.getDesmoJLicense(true)
                + "</FONT>");
		output.write("</BODY></HTML>");
    }

    /**
	 * Writes the tag to close a row in a table to the file.
	 */
    public override void closeRow()
    {

        if ((rowOpen) && (tableOpen))
        {
			output.writeln("</TR>");
            rowOpen = false;
            this._currentReporter = null;
        }
    }

    /**
	 * Inserts the tags needed to close a HTML 3.2 table into the file. Also
	 * inserts a paragraph tag to add some space below the table.
	 */
    public override void closeTable()
    {

        if (!tableOpen)
            return; // do nothing if table not open

        if (rowOpen)
        {
            closeRow(); // correct an open row if necessary
            rowOpen = false;
        };

		output.writeln("</TABLE><P>"); // The table end tag
		output.writeln("<FONT SIZE=-1><A HREF=#top>top</A></FONT><P>");

        tableOpen = false;

    }

    /// <summary>
    /// Inserts the tags needed to close a HTML 3.2 table into the file. Also inserts a paragraph tag to add
    /// some space below the table. But omits the top tag as used in method <c>closeTable()</c>. This is
    /// needed if one reportable is generating a report that consists of more than one table (see
    /// <c>StockReporter</c> or <c>HistogramReporter</c>)
    /// </summary>
    public override void closeTableNoTopTag()
    {

        if (!tableOpen)
            return; // do nothing if table not open, just return

        if (rowOpen)
        {
            closeRow(); // correct an open row if necessary
            rowOpen = false;
        }

		output.writeln("</TABLE><P>"); // The table end tag
                                    // writeln("<FONT SIZE=-1><A HREF=#top>top</A></FONT><P>");

        tableOpen = false;

    }

    /// <summary>
    /// Opens a new file with the given fileName for writing a HTML table to. If no String is given, the
    /// default filename "unnamed_DESMOJ_file" is used.
    /// </summary>
    /// <param name="name">The name of the file to be created</param>
    public override void open(string name)
    {

            var sb = new StringBuilder();
        sb.Append("<HTML><HEAD>" + FileOutput.getEndOfLine());
        sb.Append("<META HTTP-EQUIV=\"Content-Type\" CONTENT=");
        sb.Append("\"text/html; charset=iso-8859-1\">" + FileOutput.getEndOfLine());
        sb.Append("<META NAME=\"Author\" CONTENT=\"Tim Lechler\">"
                + FileOutput.getEndOfLine());
        sb.Append("<META NAME=\"GENERATOR\" CONTENT=\"DESMO-J "
                + Experiment.getDesmoJVersion() + "\">" + FileOutput.getEndOfLine());
        sb.Append("<TITLE>" + name + "</TITLE></HEAD>" + FileOutput.getEndOfLine());
        sb.Append("<BODY TEXT=\"#000000\" BGCOLOR=\"#FFFFFF\" LINK=\"#0000EE\"");
        sb.Append(" VLINK=\"#551A8B\" ALINK=\"#FF0000\">" + FileOutput.getEndOfLine());
        sb.Append("<A NAME=\"top\"></A><br>" + FileOutput.getEndOfLine());

		output.write(sb.ToString());

    }

    /**
	 * Writes the HTML 3.2 tags to open a new row in a table to the file. A new
	 * row can only be started, if the table has alerady been opened and the
	 * previous row has been closed.
	 */
    public override void openRow()
    {

        if (tableOpen)
        {

            if (!rowOpen)
            {
				output.write("<TR VALIGN=TOP>");
                rowOpen = true; // keep a note to shut the row on closes
            }

        }

    }

    /// <summary>
    /// Inserts the tags needed to build a HTML 3.2 table heading into the file. The table's heading text is
    /// given with the parameter.
    /// </summary>
    /// <param name="s">The heading for the table</param>
    public override void openTable(string s)
    {

        if (tableOpen)
            return; // table already opened

            var sb = new StringBuilder();

        sb.Append("<DIV align=center><H3>" + s + "</H3></DIV>"
                         + FileOutput.getEndOfLine());
        sb.Append("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=3 ");
        sb.Append("WIDTH=\"100%\" >" + FileOutput.getEndOfLine());

		output.write(sb.ToString());
        tableOpen = true;
        rowOpen = false;

    }

    /// <summary>Returns the status of the current table row that is written to.</summary>
    /// <returns>
    /// Is <c>true</c> if the method <c>openRow()</c> has been called last, <c>false</c> if the method
    /// <c>closeRow()</c> has been called last
    /// </returns>
    public bool rowIsOpen()
    {

        return rowOpen;

    }

    /// <summary>Returns the status of the current table that is written to.</summary>
    /// <returns>
    /// Is <c>true</c> if the method <c>openTable()</c> has been called last, <c>false</c> if the method
    /// <c>closeTable()</c> has been called last
    /// </returns>
    public  bool tableIsOpen()
    {

        return tableOpen;

    }

    /// <summary>
    /// Creates a new table cell and writes the given String into that cell. Note that there this is raw HTML
    /// code so there must not be any special language specific characters that might confuse any browser. A
    /// new cell can not be written, if neither a table nor a row have been opened yet. The method will
    /// simply return without action in that case.
    /// </summary>
    /// <param name="s">The text to be printed into a cell</param>
    /// <param name="spanning">number of cells to span</param>
    public override void writeCell(string s, int spanning)
    {

        if (s == null)
            return;

        if (!((rowOpen) && (tableOpen))) return;

        if (spanning == 1)
		    output.write("<TD>" + s + "</TD>");

        else
		    output.write("<TD colspan=\"" + spanning + "\"><i>&ensp;&ensp;&#151; " + s + "<i></TD>");
    }

    /// <summary>
    /// Creates a newcentered heading row to print a title in. Note that there this is raw HTML code so the
    /// string given must not contain any special language specific characters that might confuse any
    /// browser. been opened yet. The method will simply return without action in that case. The number for
    /// the HTML heading style must be inside the range [1,6]. If not, it will be trimmed to the nearest
    /// legal heading style number.
    /// </summary>
    /// <param name="style">The heading style format number for the text to be printed in</param>
    /// <param name="s">The text to be printed as heading</param>
    public override void writeHeading(int style, string s)
    {

        // check parameter
        if (s == null)
            return;

        // check style number
        if (style > 6)
            style = 6;
        if (style < 1)
            style = 1;

        // check if no table is open, otherwise I can't write centered heading
        if (tableOpen)
            return;

		// now write heading
		output.write("<P><H" + style + "><DIV align=center>" + s + "</DIV></H"
                + style + "><P>");

    }

    /// <summary>
    /// Creates a new table cell and writes the given String into that cell as heading cells in bold letters
    /// and with centered text. Note that there this is raw HTML code so there must not be any special
    /// language specific characters that might confuse any browser. A new cell can not be written, if
    /// netiher a tbale nor a row have been opened yet. The method will simply return without action in that
    /// case.
    /// </summary>
    /// <param name="s">The text to be printed into a cell</param>
    public override void writeHeadingCell(string s)
    {

        if (s == null)
            return;

        if ((rowOpen) && (tableOpen))
			output.write("<TD><B><DIV align=left>" + s + "</DIV></B></TD>");

    }

    /**
	 * Writes the HTML tag for inserting a horizontal ruler into the file. Note
	 * that horizontal rulers are not written into table cells, thus this method
	 * simply returns, if a table is still open.
	 */
    public override void writeHorizontalRuler()
    {

        if (!tableOpen)
			output.write("<HR>");

    }

    /// <returns>The string <c>"html"</c></returns>
    public override string getFileFormat()
    {
        return "html";
    }

}
}
