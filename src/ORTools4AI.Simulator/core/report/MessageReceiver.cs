// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{

    /**
     * Provides the basic interface implemented by all classes receiving messages of
     * any type in the DESMO-J framework. This allows for flexible organisation of
     * the messaging subsystem, enabling different kinds of output paths for
     * different kinds of messages, including multiple outputs of the same type of
     * message.
     */
    public interface MessageReceiver
    {
        /// <summary>
        /// Implement this method to define the behaviour of the messagereceiver when messages are sent to
        /// it.
        /// </summary>
        /// <param name="m">The message sent to the messagereceiver</param>
        void receive(Message m);

        /// <summary>
        /// Implement this method to define the behaviour of the messagereceiver when reporters are sent to
        /// it.
        /// </summary>
        /// <param name="r">The reporter sent to the messagereceiver</param>
        void receive(Reporter r);
    }
}