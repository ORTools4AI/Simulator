// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * An object that formats TimeInstant and TimeString objects.
     */
    public interface TimeFormatter
    {

        /// <summary>Returns the String representation of the given instant of time.</summary>
        /// <param name="instant">the instant of time to be formatted</param>
        /// <returns>the String representation of the given timeInstant</returns>
        public string buildTimeString(TimeInstant instant);

        /// <summary>Returns the String representation of the given span of time.</summary>
        /// <param name="span">the instant of time to be formatted</param>
        /// <returns>the String representation of the given timeSpan.</returns>
        public string buildTimeString(TimeSpan span);

        /// <summary>The units used to format time span and time instants.</summary>
        /// <returns>The description of the smallest time unit or all time units.</returns>
        public string getUnit();
    }
}
