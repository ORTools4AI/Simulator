// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Nicolas Knaak
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * An access point provides a generic interface to get attribute values from
     * objects. It can e.g. be used to provide access to a model's or experiment's
     * parameters.
     */
    public interface AccessPoint
    {
        /** Returns the current value of the attribute referenced by this access point. */
        object getValue();

        /** Returns the name of this AccessPoint. */
        string getName();
    }
}
