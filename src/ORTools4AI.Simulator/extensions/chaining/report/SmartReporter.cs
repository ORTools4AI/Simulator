// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Malte Unkrig
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.extensions.chaining.report
{
    /**
  * The SmartReporter is a helper class for generating reporters based
  * on user-defined header-value pairs, supporting the modification
  * of entries.  
  */
    public class SmartReporter : Reporter
    {




        public SmartReporter(string groupHeading, int groupId, List<HeaderValuePair> headerValuePairs,
                Reportable reportable) : base(reportable)
        {
            //super(reportable);
            this.groupHeading = groupHeading;
            groupID = groupId;

            numColumns = headerValuePairs.Count;
            columns = new string[numColumns];
            entries = new string[numColumns];

            for (int i = 0; i < numColumns; i++)
            {
                HeaderValuePair pair = headerValuePairs[i];
                columns[i] = pair.header;
                entries[i] = pair.value;
            }
        }

        public void overrideValueAt(string newValue, int columnPos)
        {
            entries[columnPos] = newValue;
        }


        public override string[] getEntries()
        {
            return entries;
        }
    }


    public class HeaderValuePair
    {
        public readonly string header;
        public readonly string value;

        public HeaderValuePair(string header, object value)
        {
            //super();
            this.header = header;

            if (value is double || value is float)
            {
                //NumberFormat numberFormat;

                //numberFormat = DecimalFormat.getInstance(Locale.ENGLISH);
                //numberFormat.setMaximumFractionDigits(4);
                //numberFormat.setMinimumFractionDigits(4);
                //numberFormat.setGroupingUsed(false);

                //this.value =  numberFormat.format(value);
                this.value = value.ToString();
            }
            else
            {
                this.value = value.ToString();
            }
        }


        public override string ToString()
        {
            return header + ": " + value;
        }

    }
}
