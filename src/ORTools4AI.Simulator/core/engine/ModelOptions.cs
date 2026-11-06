// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Defines an interface to design the individual model options. This interface
     * does not define any special methods. It is used to clarify the API by
     * publishing a name for this type of objects.
     */
    public interface ModelOptions
    {
        /// <summary>
        /// Sets the options for the specific model. The model has to call this method once the ModelOptions
        /// object has been passed to the model's <c>init(Modeloptions)</c> method. This is done
        /// automatically within the <c>init(Modeloptions)</c> method.
        /// </summary>
        /// <param name="m">The specific model for this options setting</param>
        void setOptions(Model m);
    }
}