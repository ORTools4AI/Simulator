// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * An interface for objects with parameters provided via access points.
     */
    public interface Parameterizable
    {
        /** Creates a map of parameters (name → access point). */
        Dictionary<string, AccessPoint> createParameters();
    }
}
