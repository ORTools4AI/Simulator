// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Nicolas Knaak
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * An interface for access points that allow to set the accessed parameter's
     * value (and not only read it as in AccessPoint).
     */
    public interface MutableAccessPoint : AccessPoint
    {
        /** Sets the value of the attribute referenced by this access point. */
        void setValue(object value);
    }
}
