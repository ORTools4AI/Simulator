// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.core.engine
{
    /**
  * @deprecated Replaced by PatternBasedTimeFormatter.
  * 
  * Use this class to format TimeInstant objects using a java.text.DateFormat object.
  * The UTCTimeFormatter uses a MultiUnitTimeFormatter to format TimeSpans.
  */
  //  @Deprecated
public class UTCTimeFormatter 
    {

    /**
	 * Constructor for a UTCTimeFormatter. This shortcut constructor uses the
	 * pattern "dd.MM.yyyy HH:mm:ss:SSS". Default Locale is Germany.
	 */
    public UTCTimeFormatter()
    {
        //super(new SimpleDateFormat("dd.MM.yyyy HH:mm:ss:SSS", Locale.GERMANY), TimeUnit.DAYS, TimeUnit.MILLISECONDS);
    }

    /**
	 * Constructor for a UTCTimeFormatter.
	 * 
	 * @param dateFormat
	 *            DateFormat : the dateFormat used by this UTCTimeFormatter.
	 * @param coarsestUnit
	 *          		TimeUnit: The coarsest Unit for the TimeSpan
	 * @param finestUnit
	 *   				TimeUnit: The finest Unit for the TimeSpan
	 */
    //public UTCTimeFormatter(DateFormat dateFormat, TimeUnit coarsestUnit, TimeUnit finestUnit)
    //{
    //    super(dateFormat, coarsestUnit, finestUnit);
    //}

    /**
	 * Constructor for a UTCTimeFormatter. Default Locale is Germany.
	 * 
	 * @param pattern
	 *            String : the pattern used by this UTCTimeFormatter to format
	 *            TimeSpan and TimeInstant objects. See 
	 *            http://download.oracle.com/javase/1.5.0/docs/api/java/text/SimpleDateFormat.html
	 *            for a description of the syntax of such patterns. 
	 *             @param coarsestUnit
	 *          		TimeUnit: The coarsest Unit for the TimeSpan
	 * @param finestUnit
	 *   				TimeUnit: The finest Unit for the TimeSpan
	 */
    //public UTCTimeFormatter(String pattern, TimeUnit coarsestUnit, TimeUnit finestUnit)
    //{
    //    this(new SimpleDateFormat(pattern, Locale.GERMANY), coarsestUnit, finestUnit);
    //}
	
	}

}
