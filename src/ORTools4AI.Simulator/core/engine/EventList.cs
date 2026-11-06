// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * This interface describes the set of methods an event-list carrying the
     * scheduled events and entities in a temporal order has to implement. This is a
     * part of the framework that does not have to be implemented by the user in
     * order to get a simulation running, since the scheduler already uses the class
     * EventVector as default EventList construction. Since each step in the
     * discrete simulation requires searching and manipulating the event-list, this
     * is probably one of the best places in the framework to optimize execution
     * performance. Especially if special models show specific behaviour i.e.
     * primarily inserting new events at the very end of the event-list, other
     * implementations of the event-list might support faster access times.
     * 
     * @see EventVectorList
     */
    public abstract class EventList
    {

        /// <summary>
        /// Returns the first event-note in the event-list. That EventNote contains information about the
        /// next event to happen and will be the next event processed by the scheduler.
        /// </summary>
        /// <returns>The first event-note in the event-list</returns>
        public abstract EventNote firstNote();

        /// <summary>
        /// Inserts a new event-note into the event-list. Event notes are sorted by the point if time they
        /// are scheduled to happen. This method ensures that the new event-note is inserted in the right
        /// place in the event-list, scanning through the list whenever a new event-note has to be inserted.
        /// </summary>
        /// <param name="newNote">The event-note to be sorted into the event-list</param>
        public abstract void insert(EventNote newNote);

        /// <summary>
        /// Inserts the new event-note straight after the other specified EventNote. This other EventNote
        /// must already be scheduled. Otherwise a warning will be issued and the method returns without
        /// changing the event-list.
        /// </summary>
        /// <param name="afterNote">
        /// The event-note already scheduled, that the new EventNote is supposed to be inserted after.
        /// </param>
        /// <param name="newNote">The new event-note to be inserted before the specified EventNote</param>
        public abstract void insertAfter(EventNote afterNote, EventNote newNote);

        /// <summary>
        /// Inserts the given EventNote at the first position in the event-list. The Event encapsulated in
        /// that EventNote will probably be the next event to be processed by the scheduler (unless some
        /// other calls to this method are made before). Note that this operation changes the scheduled time
        /// of the given EventNote to the actual simulation time to keep the temporal order of the
        /// event-list.
        /// </summary>
        /// <param name="newNote">
        /// The event-note to be inserted at the first position in the event-list
        /// </param>
        public abstract void insertAsFirst(EventNote newNote);

        /// <summary>
        /// Inserts the new event-note straight before the other specified EventNote. This other EventNote
        /// must already be scheduled. Otherwise the position to insert the given EventNote can not be
        /// determined, resulting in a ItemNotScheduledException being thrown.
        /// </summary>
        /// <param name="beforeNote">
        /// The event-note already scheduled, that the new EventNote is supposed to be inserted before.
        /// </param>
        /// <param name="newNote">The new event-note to be inserted before the specified EventNote</param>
        public abstract void insertBefore(EventNote beforeNote, EventNote newNote);

        /// <summary>
        /// Tests if there are any scheduled events contained in the event-list. If the event-list happens to
        /// be empty during the run of a simulation, this is a criterium to stop the simulation, since no
        /// further action is scheduled.
        /// </summary>
        /// <returns>
        /// Is <c>true</c> if there are no EventNote contained in the event-list, <c>false</c> otherwise.
        /// </returns>
        public abstract bool isEmpty();

        /// <summary>Returns the last EventNote in the event-list.</summary>
        /// <returns>The last EventNote in the event-list</returns>
        public abstract EventNote lastNote();

        /// <summary>
        /// Returns the next event-note in the event-list relative to the given EventNote. If the given
        /// EventNote is not contained in the event-list or happens to be the last EventNote in the
        /// event-list, <c>null</c> will be returned.
        /// </summary>
        /// <param name="origin">The event-note whose successor is wanted</param>
        /// <returns>
        /// The event-note following the given EventNote or &lt;ocde&gt;null if the given EventNote was last
        /// or not found
        /// </returns>
        public abstract EventNote nextNote(EventNote origin);

        /// <summary>
        /// Returns the previous EventNote in the event-list relative to the given EventNote. If the given
        /// EventNote is not contained in the event-list or happens to be the first event-note in the
        /// evnet-list, <c>null</c> will be returned.
        /// </summary>
        /// <param name="origin">The event-note whose predecessor is wanted</param>
        /// <returns>
        /// The event-note following the given EventNote or &lt;ocde&gt;null if the given EventNote was first
        /// or not found
        /// </returns>
        public abstract EventNote prevNote(EventNote origin);

        /// <summary>Removes the given EventNote from the event-list.</summary>
        /// <param name="note">EventNote . The event-note to be removed from the event-list</param>
        public abstract void remove(EventNote note);

        /// <summary>Removes the first event-note from the event-list.</summary>
        /// <returns>The removed EventNote, or null if the list was empty.</returns>
        public abstract EventNote removeFirst();

        public abstract List<EventNote> iterators();

        public abstract bool isRandomizingConcurrentEvents();
    }
}