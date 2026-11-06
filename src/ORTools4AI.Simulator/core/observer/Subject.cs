// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.observer
{

	/**
     * Interface which an Subject can implement to notify possible Observers
     * 
     * 
     * @param <T>
     *            Class of an possible Subject
     * @param <X>
     *            Type of the Event which is passed in the update Method
     */
	public interface Subject<T, X>
	{
		/// <summary>This method adds an observer to the subject</summary>
		/// <param name="observer">the observer which should be added to the subject</param>
		public void addObserver(Observer<T, X> observer);

		/// <summary>This method deletes an observer from the subject</summary>
		/// <param name="observer">the observer which should be deleted</param>
		public void deleteObserver(Observer<T, X> observer);

		/// <summary>This method is used to notify the registerd observers</summary>
		/// <param name="subject">reference to the actual subject</param>
		/// <param name="eventObject">
		/// an eventtype to distinguish different notification types like itemAdded or itemRemoved
		/// </param>
		public void notifyObservers(T subject, X eventObject);
	}
}