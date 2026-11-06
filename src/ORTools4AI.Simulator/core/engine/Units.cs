// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Ruth Meyer
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * @deprecated Prefer usage of Java standard classes, e.g. <code>java.util.concurrent.TimeUnit</code>
     * for time units. 
     * 
     * The interface Units provides constants for representing units for time,
     * length and speed. Additionally, textual representations for these units and
     * conversion factors are given.
     * <p>
     * Any class implementing this interface gains direct access to all defined
     * constants. It is recommended to use this interface in every class that has to
     * represent values with units.
     */
    //@Deprecated
public abstract class Units
    {

        // Konstanten deklarieren f�r Zeit-Einheiten

        /** The time unit milliseconds. */
        public static readonly int MS = 0;

        /** The time unit seconds. */
        public static readonly int S = 1;

        /** The time unit minutes. */
        public static readonly int MIN = 2;

        /** The time unit hours. */
        public static readonly int H = 3;

        // Konstanten deklarieren f�r L�ngen-Einheiten

        /** The length unit millimetres. */
        public static readonly int MM = 4;

        /** The length unit centimetres. */
        public static readonly int CM = 5;

        /** The length unit metres. */
        public static readonly int M = 6;

        /** The length unit kilometres. */
        public static readonly int KM = 7;

        // Konstanten deklarieren f�r Geschwindigkeits-Einheiten

        /** The speed unit metres/second. */
        public static readonly int M_S = 8;

        /** The speed unit kilometres/hour. */
        public static readonly int KM_H = 9;

        // Konstanten f�r textuelle Repr�sentation der Einheiten

        /**
         * Textual representations for the defined units. To get the appropriate
         * unit String use the unit constant as an index. For example:
         * <code>Units.unitStrings[Units.MIN]</code> gives "min".
         */
        public static readonly string[] unitStrings = { "ms", "s", "min", "h", "mm",
			"cm", "m", "km", "m/s", "km/h" };

	// Konstanten f�r Umrechungsfaktoren

	/**
	 * Conversion factors to convert between units. To get the appropriate unit
	 * factor use the unit constant as an index. For example:
	 * <code>Units.unitFactors[Units.MIN]</code> gives 60000.
	 * <p>
	 * To convert a value from an old unit to a new unit multiply the value with
	 * the factor for the old unit and divide by the factor for the new unit.
	 * Example: <code>value * unitFactors[oldUnit] / unitFactors[newUnit]</code>
	 * <p>
	 * Note that these factors of course only make sense when used for units of
	 * the same phenomenon, so you should never attempt to convert a time unit
	 * into a length unit, for example.
	 */
	public static readonly double[] unitFactors = { 1, 1000, 60000, 3600000, 1,
            100, 1000, 1000000, 1, 1000 / 3600.0 };
    }
}
