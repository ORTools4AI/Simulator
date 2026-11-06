// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.observer
{

	/**
     * Helper class for the subject interface to make the administration code reusable
     * 
     * 
     * @param <T>
     *            Class of an possible Subject
     * @param <X>
     *            Type of the Event which is passed in the update Method
     */
	public class SubjectAdministration<T, X> {

		/**
		 * a list of all registered observers of this subject
		 */
		private readonly List<Observer<T, X>> observers = new List<Observer<T, X>>();

		/// <summary>Adds an observer to the observer list</summary>
		/// <param name="observer">the observer to be added</param>
		public void addObserver(Observer<T, X> observer) {
			if (!observers.Contains(observer)) {
				observers.Add(observer);
			}
		}

		/// <summary>Deletes an observer from the observer list</summary>
		/// <param name="observer">the observer to be deleted</param>
		public void deleteObserver(Observer<T, X> observer) {
			observers.Remove(observer);

		}

		/// <summary>
		/// Notifies all registered observers and passes a reference of the subject and a eventtype to the observer
		/// </summary>
		/// <param name="subject">the reference of the actual subject</param>
		/// <param name="eventObject">
		/// the event type of the notify call. This is optional to distinguish the events and can be null
		/// </param>
		public void notifyObservers(T subject, X eventObject) {
			foreach (var observer in observers) {
				observer.update(subject, eventObject);
			}
		}
	}
}